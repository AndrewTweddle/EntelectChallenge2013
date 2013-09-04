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
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.VisualUtils;
using System.Threading.Tasks;

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
        private const int NEXT_TURN_SAFETY_MARGIN_IN_MS = 500;  // i.e. 500 milliseconds after the last possible start of the next tick

        public object BestMoveLock { get; private set; }
        public object CurrentGameStateLock { get; private set; }

        private bool CanMovesBeChosen { get; set; }

        public TankActionSet BestMoveSoFar { get; private set; }
        public ICommunicator Communicator { get; set; }
        public ICommunicatorCallback CommunicatorCallback { get; set; }
        public ISolver<TGameState> Solver { get; set; }
        public Thread SolverThread { get; private set; }

        private Coordinator()
        {
        }

        public Coordinator(ISolver<TGameState> solver, ICommunicator communicator, ICommunicatorCallback communicatorCallback)
        {
            // Set lock objects:
            BestMoveLock = new object();
            CurrentGameStateLock = new object();

            solver.Coordinator = this;
            Solver = solver;
            Communicator = communicator;
            CommunicatorCallback = communicatorCallback;
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
            Solver.YourPlayerIndex = Communicator.LoginAndGetYourPlayerIndex(CommunicatorCallback);
            TGameState initialGameState = GameState<TGameState>.GetInitialGameState();
            Run(initialGameState);
        }

        /// <summary>
        /// Used to load a particular scenario file as the initial game state...
        /// </summary>
        /// <param name="yourPlayerIndex"></param>
        /// <param name="initialGameStateFilePath"></param>
        public void StepInto(int yourPlayerIndex, string initialGameStateFilePath)
        {
            Solver.YourPlayerIndex = yourPlayerIndex;
            Game loadedGame = Game.Load(initialGameStateFilePath);
            Game.Current = loadedGame;
            try
            {
                ChooseMovesForNextTick();
            }
            finally
            {
                LogGameStateAfterTick();
            }
        }

        /// <summary>
        /// Sets the initial game state and runs the game until it ends.
        /// </summary>
        /// <param name="initialGameState"></param>
        public void Run(TGameState initialGameState)
        {
            Game.Current.CurrentTurn.GameState = initialGameState;
            SaveGame("InitialGame.xml");

            /* Run solver in a separate thread: */
            StartTheSolver();
            try
            {
                while (!Game.Current.CurrentTurn.GameState.IsGameOver)
                {
                    try
                    {
                        ChooseMovesForNextTick();
                        SendBestMoveToTheCommunicator();
                        WaitForTheNextTick();
                    }
                    finally
                    {
                        LogGameStateAfterTick();
                    }
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

        public void StartTheSolver()
        {
            SolverThread = new Thread(RunTheSolver);
            SolverThread.Start();
        }

        public void ChooseMovesForNextTick()
        {
            int currentTickOnClient = Game.Current.CurrentTurn.Tick;
            LogDebugMessage("STARTING TICK {0}!", currentTickOnClient);

            // Let solver start choosing moves:
            CanMovesBeChosen = true;
            Solver.StartChoosingMoves();
            LogDebugMessage("Signalled solver to start choosing moves.");

            // Save game and images of board for debugging purposes:
            LogDebugMessage("Saving game");
            SaveGame();

            LogDebugMessage("Saving game state image");
            SaveGameStateImage(Game.Current.CurrentTurn.GameState,
                @"Images\GameStateImage_{3}.bmp", "GameState.bmp");

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
            LogDebugMessage("Signalled solver to stop choosing moves.");

            // Give the solver a bit of time to stop choosing moves:
            int milliSecondsUntilSolverStoppedChoosingMoves = 0;
            while ((Solver.SolverState == SolverState.StoppingChoosingMoves)
                && (milliSecondsUntilSolverStoppedChoosingMoves <= MAX_GRACE_PERIOD_TO_STOP_IN))
            {
                Thread.Sleep(10);
                milliSecondsUntilSolverStoppedChoosingMoves += 10;
            }
            LogDebugMessage(
                string.Format(
                    "Time for solver to stop choosing moves: {0}",
                    TimeSpan.FromMilliseconds(milliSecondsUntilSolverStoppedChoosingMoves)));
            CanMovesBeChosen = false;
        }

        public void SendBestMoveToTheCommunicator()
        {
            // Send the best move to the server:
            PerformActionInALock<TankActionSet>(BestMoveSoFar, BestMoveLock,
                "Lock timeout with best move lock to send actions to communicator",
                delegate(TankActionSet bm)
                {
                    if (bm == null)
                    {
                        LogDebugMessage("No tank actions to send.");
                        return;
                    }
                    LogDebugMessage("Sending tank actions.");
                    DateTime timeBeforeMovesSent = DateTime.Now;
                    bool wereMovesSent = TrySetTankActions(bm, DEFAULT_TIME_TO_WAIT_FOR_SET_ACTION_RESPONSE_IN_MS);
                    DateTime timeAfterMovesSent = DateTime.Now;
                    bm.TimeTakenToSubmit = timeAfterMovesSent - timeBeforeMovesSent;
                    if (wereMovesSent)
                    {
                        Game.Current.CurrentTurn.TankActionSetsSent[Solver.YourPlayerIndex] = bm;
                        LogDebugMessage(
                            "Sent actions successfully. Duration: {0}.",
                            timeAfterMovesSent - timeBeforeMovesSent);
                    }
                    else
                    {
                        LogDebugMessage("THE ACTIONS WERE NOT SENT SUCCESSFULLY!");

                        // TODO: If it fails, keep trying until it gets too close to the end of the turn
#if THROW_HARNESS_ERRORS
                                    throw new InvalidOperationException(
                                        String.Format(
                                            "The moves for turn {0} could not be submitted",
                                            Game.Current.CurrentTurn.Tick));
#endif
                    };
                    BestMoveSoFar = null;
                }
            );
        }

        public void WaitForTheNextTick()
        {
            // Wait for communicator to signal that the server engine has moved to the next tick:
            TimeSpan timeToWaitBeforeGettingNextState
                = Game.Current.CurrentTurn.LatestLocalNextTickTime
                - DateTime.Now + TimeSpan.FromMilliseconds(NEXT_TURN_SAFETY_MARGIN_IN_MS);

            if (timeToWaitBeforeGettingNextState.TotalMilliseconds > 0)
            {
                LogDebugMessage("Waiting {0} before checking for next tick.", timeToWaitBeforeGettingNextState);
                Thread.Sleep(timeToWaitBeforeGettingNextState);
            }
            LogDebugMessage("Waiting for next tick.");  
            Communicator.WaitForNextTick(Solver.YourPlayerIndex, Game.Current.CurrentTurn.Tick, CommunicatorCallback);

            DebugHelper.WriteLine();
        }

        public bool TrySetTankActions(TankActionSet actionSet, int timeoutInMilliseconds)
        {
            if (actionSet == null)
            {
                return true;
            }

            if (actionSet.Tick != Game.Current.CurrentTurn.Tick)
            {
                return false;
            }

            int playerIndex = actionSet.PlayerIndex;
            GameState currentGameState = Game.Current.CurrentTurn.GameState;
            int numberAlive = 0;
            int tankId = -1;
            TankAction tankAction = TankAction.NONE;

            for (int t = 0; t < Constants.TANKS_PER_PLAYER; t++)
            {
                Tank tank = Game.Current.Players[actionSet.PlayerIndex].Tanks[t];
                MobileState tankState = currentGameState.GetMobileState(tank.Index);
                if (tankState.IsActive)
                {
                    numberAlive++;
                    tankId = tank.Id;
                    tankAction = actionSet.Actions[t];
                }
            }

            TankAction tankAction1 = actionSet.Actions[0];
            TankAction tankAction2 = actionSet.Actions[1];

            if (numberAlive == 1)
            {
                return Communicator.TrySetAction(playerIndex, tankId, tankAction, CommunicatorCallback, timeoutInMilliseconds);
            }
            else
                if (numberAlive == 2)
                {
                    return Communicator.TrySetActions(playerIndex, tankAction1, tankAction2, CommunicatorCallback, timeoutInMilliseconds);
                }
                else
                {
                    return true;
                }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void LogGameStateAfterTick()
        {
            // Save the text versions of the game states if there is an error:
            LogDebugMessage("Saving game state as text");
            SaveTextImageOfGameState(Game.Current.CurrentTurn.GameState,
                @"Images\GameStateAsText_{3}.txt", "GameStateAsText.txt");

            if (Game.Current.CurrentTurn.GameStateCalculatedByGameStateEngine != null)
            {
                LogDebugMessage("Saving game state calculated by game state engine as text");
                SaveTextImageOfGameState(Game.Current.CurrentTurn.GameStateCalculatedByGameStateEngine,
                    @"Images\CalculatedGameStateAsText_{3}.txt", "CalculatedGameStateAsText.txt");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void LogDebugMessage(string format, params object[] args)
        {
            string caller = String.Format("Coordinator {0} - {1}", Solver.YourPlayerIndex, Solver.Name);
            DebugHelper.LogDebugMessage(caller, format, args);
        }

        /// <summary>
        /// This saves the game state image. This is a terrible layer violation.
        /// Ideally the dependency should be inverted by having a GameStateListeners collection,
        /// and plugging in the image generator as a listener.
        /// But, in the interests of time, don't fix this yet.
        /// </summary>
        /// <param name="gameState">the game state</param>
        /// <param name="perTickFileFormat">the file format for the file generated on every tick (now not used)</param>
        /// <param name="currentFileFormat">the file format for the latest image generated </param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void SaveGameStateImage(GameState gameState, 
            string perTickFileFormat, string currentFileFormat)
        {
            try
            {
                string filePath = DebugHelper.GenerateFilePath(perTickFileFormat);
                ImageGenerator imageGenerator = new ImageGenerator();
                imageGenerator.IsBackgroundChequered = true;
                // No need to have an image for every turn...
                // imageGenerator.SaveGameStateImage(filePath, gameState);

                filePath = DebugHelper.GenerateFilePath(currentFileFormat);
                imageGenerator.SaveGameStateImage(filePath, gameState);
            }
            catch
            {
                // swallow any exceptions...
            }
        }
        
        [System.Diagnostics.Conditional("DEBUG")]
        public static void SaveTextImageOfGameState(GameState gameState,
            string perTickFileFormat, string currentFileFormat)
        {
            try
            {
                string[] textViewOfBoard = BoardHelper.GenerateTextImageOfBoard(gameState);

                string filePath = DebugHelper.GenerateFilePath(perTickFileFormat);
                System.IO.File.WriteAllLines(filePath, textViewOfBoard);

                filePath = DebugHelper.GenerateFilePath(currentFileFormat);
                System.IO.File.WriteAllLines(filePath, textViewOfBoard);
            }
            catch
            {
                // swallow any exceptions...
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void SaveGame(string fileName="Game.xml")
        {
            try
            {
                string filePath = DebugHelper.GenerateFilePath(fileName);
                Game.Current.Save(filePath);
            }
            catch
            {
                // swallow any exceptions...
            }
        }
    }
}
