using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AndrewTweddle.BattleCity.AI.Solvers;
using AndrewTweddle.BattleCity.Core.Actions;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI
{
    public class Coordinator<TGameState>
        where TGameState: GameState<TGameState>, new()
    {
        private const int END_OF_TURN_SAFETY_MARGIN_IN_MS = 1000;  // 1 second before the end of the tick
        private const int MAX_GRACE_PERIOD_TO_STOP_IN = 500;
        private const int LOCK_TIMEOUT = 100;
        private const int DEFAULT_TIME_TO_WAIT_FOR_SET_ACTION_RESPONSE_IN_MS = 500;
        private const int TIME_TO_WAIT_FOR_SOLVER_TO_STOP_IN_MS = 1000;

        public object BestMoveLock { get; private set; }
        public object CurrentGameStateLock { get; private set; }

        private bool CanMovesBeChosen { get; set; }

        public TankActionSet BestMoveSoFar { get; private set; }
        public ICommunicator Communicator { get; set; }
        public ISolver<TGameState> Solver { get; set; }
        public Thread SolverThread { get; private set; }

        private Coordinator()
        {
        }

        public Coordinator(ISolver<TGameState> solver, ICommunicator communicator)
        {
            // Set lock objects:
            BestMoveLock = new object();
            CurrentGameStateLock = new object();

            solver.Coordinator = this;
            Solver = solver;
            Communicator = communicator;
        }

        // TODO: Move this into a utilities project
        public void PerformActionInALock<T>(T target,
            object lockObject, string lockTimeoutErrorMessage, Action<T> action)
        {
            bool isLocked = Monitor.TryEnter(lockObject, LOCK_TIMEOUT);
            try
            {
#if DEBUG
                if (!isLocked)
                {
                    System.Diagnostics.Debug.WriteLine(lockTimeoutErrorMessage);
                }
#endif
                action(target);
            }
            finally
            {
                if (isLocked)
                {
                    Monitor.Exit(lockObject);
                }
            }
        }

        public void SetBestMoveSoFar(TankActionSet bestMove)
        {
            PerformActionInALock<TankActionSet>(bestMove, BestMoveLock, "Lock timeout with best move lock",
                delegate(TankActionSet bm)
                    {
                        if (CanMovesBeChosen)
                        {
                            /* Get a static copy of the CurrentGameState in case of concurrency issues 
                             * (i.e. the tick advances, and a new CurrentGameState is set):
                             */
                            if (bestMove.Tick >= Game.Current.CurrentTurn.Tick)
                            {
                                BestMoveSoFar = bm;
                            }
                        }
                    });
        }

        /// <summary>
        /// This method is used to run the solver in a separate thread
        /// </summary>
        private void RunTheSolver()
        {
            try
            {
                if (Solver != null)
                {
                    Solver.Start();
                }
            }
            catch (Exception exc)
            {
                throw;  // Just to have somewhere to put a breakpoint
            }
        }

        /// <summary>
        /// Runs the game until it ends.
        /// This will also generate the initial game state.
        /// </summary>
        public void Run()
        {
            Solver.YourPlayerIndex = Communicator.LoginAndGetYourPlayerIndex();
            TGameState initialGameState = GameState<TGameState>.GetInitialGameState();
            Run(initialGameState);
        }

        /// <summary>
        /// Sets the initial game state and runs the game until it ends.
        /// </summary>
        /// <param name="initialGameState"></param>
        public void Run(TGameState initialGameState)
        {
            Game.Current.CurrentTurn.GameState = initialGameState;

            /* Run solver in a separate thread: */
            SolverThread = new Thread(RunTheSolver);
            SolverThread.Start();
            try
            {
                while (!Game.Current.CurrentTurn.GameState.IsGameOver)
                {
                    CanMovesBeChosen = true;
                    Solver.StartChoosingMoves();

                    // Give the solver some time to choose its moves:
                    TimeSpan timeToWaitBeforeSendingBestMove
                        = Game.Current.CurrentTurn.EarliestLocalNextTickTime 
                        - DateTime.Now
                        - TimeSpan.FromMilliseconds(END_OF_TURN_SAFETY_MARGIN_IN_MS);
                    if (timeToWaitBeforeSendingBestMove.TotalMilliseconds > 0)
                    {
                        Thread.Sleep(timeToWaitBeforeSendingBestMove);
                    }

                    // Signal the solver algorithm to stop choosing moves:
                    Solver.StopChoosingMoves();

                    // Give the solver a bit of time to stop choosing moves:
                    int milliSecondsUntilSolverStoppedChoosingMoves = 0;
                    while ((Solver.SolverState == SolverState.StoppingChoosingMoves) 
                        && (milliSecondsUntilSolverStoppedChoosingMoves <= MAX_GRACE_PERIOD_TO_STOP_IN))
                    {
                        Thread.Sleep(10);
                        milliSecondsUntilSolverStoppedChoosingMoves += 10;
                    }
                    System.Diagnostics.Debug.WriteLine("Time for solver to stop choosing moves: {0}", 
                        TimeSpan.FromMilliseconds(milliSecondsUntilSolverStoppedChoosingMoves));
                    CanMovesBeChosen = false;

                    // Send the best move to the server:
                    PerformActionInALock<TankActionSet>(BestMoveSoFar, BestMoveLock, 
                        "Lock timeout with best move lock to send actions to communicator",
                        delegate(TankActionSet bm)
                        {
                            if (bm == null)
                            {
                                return;
                            }
                            DateTime timeBeforeMovesSent = DateTime.Now;
                            bool wereMovesSent = Communicator.TrySetTankActions(bm, DEFAULT_TIME_TO_WAIT_FOR_SET_ACTION_RESPONSE_IN_MS);
                            DateTime timeAfterMovesSent = DateTime.Now;
                            bm.TimeTakenToSubmit = timeAfterMovesSent - timeBeforeMovesSent;
                            if (wereMovesSent)
                            {
                                Game.Current.CurrentTurn.TankActionSetsSent[Solver.YourPlayerIndex] = bm;
                            }
#if DEBUG
                            else
                            {
                                // TODO: If it fails, keep trying until it gets too close to the end of the turn
                                throw new InvalidOperationException(
                                    String.Format(
                                        "The moves for turn {0} could not be submitted",
                                        Game.Current.CurrentTurn.Tick));
                            };
#endif
                            BestMoveSoFar = null;
                        }
                    );

                    // Wait for communicator to signal that the server engine has moved to the next tick:
                    TimeSpan timeToWaitBeforeGettingNextState
                        = Game.Current.CurrentTurn.EarliestLocalNextTickTime
                        - DateTime.Now;
                    if (timeToWaitBeforeGettingNextState.TotalMilliseconds > 0)
                    {
                        Thread.Sleep(timeToWaitBeforeGettingNextState);
                    }
                    Communicator.WaitForNextTick();
                }
                Solver.Stop();
                Thread.Sleep(TIME_TO_WAIT_FOR_SOLVER_TO_STOP_IN_MS);
            }
            finally
            {
                if (SolverThread.ThreadState != ThreadState.Stopped)
                {
                    SolverThread.Abort();
                }
                SolverThread = null;
            }
        }
    }
}
