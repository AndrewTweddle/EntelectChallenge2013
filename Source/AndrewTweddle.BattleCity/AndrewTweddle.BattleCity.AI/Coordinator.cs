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
        public static readonly int MAX_GRACE_PERIOD_TO_STOP_IN = 500;
        public static readonly int LOCK_TIMEOUT = 100;

        public object BestMoveLock { get; private set; }
        public object CurrentGameStateLock { get; private set; }

        public TGameState CurrentGameState { get; private set; }
        public TankActionSet BestMoveSoFar { get; private set; }
        public ICommunicator Communicator { get; set; }
        public ISolver<TGameState> Solver { get; set; }
        public Thread SolverThread { get; private set; }
        public ManualResetEvent OutputTriggeringEvent { get; private set; }

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
                        /* Get a static copy of the CurrentGameState in case of concurrency issues 
                         * (i.e. the tick advances, and a new CurrentGameState is set):
                         */
                        GameState currGS = CurrentGameState;  
                        if (bestMove.Tick >= currGS.Tick)
                        {
                            BestMoveSoFar = bm;
                            Communicator.SetTankActions(currGS, BestMoveSoFar);
                        }
                    });
        }

        private void SetCurrentGameState(TGameState newGameState)
        {
            PerformActionInALock<TGameState>(newGameState, CurrentGameStateLock, "Lock timeout updating CurrentGameState",
                delegate(TGameState newGS)
                {
                    CurrentGameState = newGS;
                }
            );
        }

        /// <summary>
        /// This method is used to run the solver in a separate thread
        /// </summary>
        private void RunTheSolver()
        {
            /* Save the property value in a local variable, so that if the property is later changed, 
             * this method won't affect the newly created ManualResetEvent:
             */
            ManualResetEvent triggerOutputEvent = OutputTriggeringEvent;
            try
            {
                if (Solver != null)
                {
                    Solver.Solve();
                }
            }
            catch (Exception exc)
            {
                throw;  // Just to have somewhere to put a breakpoint
            }
            finally
            {
                /* Signal that the solver has run: */
                if (triggerOutputEvent != null)
                {
                    triggerOutputEvent.Set();
                }
            }
        }

        /// <summary>
        /// Runs the game until it ends.
        /// This will also generate the initial game state.
        /// </summary>
        public void Run()
        {
            Communicator.Login();
            TGameState initialGameState = GameState<TGameState>.GetInitialGameState();
            Run(initialGameState);
        }

        /// <summary>
        /// Sets the initial game state and runs the game until it ends.
        /// </summary>
        /// <param name="initialGameState"></param>
        public void Run(TGameState initialGameState)
        {
            SetCurrentGameState(initialGameState);

            // TODO: Give the solver a chance to initialize here

            while (!CurrentGameState.IsGameOver)
            {
                /* Run solver in a separate thread: */
                SolverThread = new Thread(RunTheSolver);
                SolverThread.Start();
                try
                {
                    /* Wait for communicator to signal that the server engine has moved to the next tick: */
                    TGameState newGameState = CurrentGameState.CloneDerived();
                    Communicator.WaitForNextTick(newGameState);

                    /* Stop the solver algorithm: */
                    Solver.Stop();

                    /* Wait for solver to stop: */
                    int milliSecondsUntilSolverStopped = 0;
                    while ((Solver.SolverState != SolverState.NotRunning) && (milliSecondsUntilSolverStopped <= MAX_GRACE_PERIOD_TO_STOP_IN))
                    {
                        Thread.Sleep(10);
                        milliSecondsUntilSolverStopped += 10;
                    }
                    System.Diagnostics.Debug.WriteLine("Time for solver to stop: {0}", TimeSpan.FromMilliseconds(milliSecondsUntilSolverStopped));

                    /* Update the current game state: */
                    SetCurrentGameState(newGameState);
                }
                finally
                {
                    SolverThread = null;
                }
            }
        }
    }
}
