using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Comms.Client;
using AndrewTweddle.BattleCity.AI;
using AndrewTweddle.BattleCity.AI.Solvers;
using AndrewTweddle.BattleCity.Bots;
using AndrewTweddle.BattleCity.Core.States;
using System.Diagnostics;
using System.Reflection;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "/?" || args[0] == "-?")
            {
                Console.WriteLine("USAGE: serverUrl [logFolderPath [gameStateFileToReplay [TickToReplayTo|yourPlayerIndex]]] ");
            }
            else
            {
                Console.WindowWidth = 120;  // columns

                string appName = "ConsoleApp";

                // Extract parameters from args:
                string serverUrl = args[0];
                string loggingRootFolder = (args.Length >= 2) ? args[1] : null;
                string gameFilePath = (args.Length >= 3) ? args[2] : null;
                int tickToReplayTo = -1;
                bool errorReadingTickToReplayTo = false;
                if (args.Length >= 4)
                {
                    errorReadingTickToReplayTo = !int.TryParse(args[3], out tickToReplayTo);
                }

                RunSolverInConsole<MutableGameState, ScenarioDrivenBot<MutableGameState>>(
                    appName, serverUrl, loggingRootFolder, gameFilePath, 
                    tickToReplayTo, errorReadingTickToReplayTo);
            }
        }

        // TODO: Move into a common ConsoleHelper class.
        // TODO: Move parameters into a GameRunnerConfiguration class.
        private static void RunSolverInConsole<TGameState, TSolver>(
            string appName, string serverUrl, string loggingRootFolder, string gameFilePath, int tickToReplayTo, bool errorReadingTickToReplayTo)
            where TGameState: GameState<TGameState>, new()
            where TSolver: ISolver<TGameState>, new()
        {
            // Set up debugging:
            DebugHelper.InitializeLogFolder(DateTime.Now, loggingRootFolder, appName);
            DebugHelper.WireUpDebugListeners(includeConsoleListener: true);
            try
            {
                // Set up web service adapter:
                WebServiceAdapter wsAdapter = new WebServiceAdapter
                {
                    Url = serverUrl,
                    EndPointConfigurationName = "ChallengePort"
                };

                // Set up solvers and coordinators:
                ISolver<TGameState> solver = new TSolver();
                ICommunicatorCallback communicatorCallback = new RemoteCommunicatorCallback();
                Coordinator<TGameState> coordinator = new Coordinator<TGameState>(solver, wsAdapter, communicatorCallback);

                bool stepIntoGame = false;
                if (!string.IsNullOrEmpty(gameFilePath))
                {
                    if (tickToReplayTo == 0 || tickToReplayTo == 1)
                    {
                        stepIntoGame = true;
                    }
                    else
                    {
                        DebugHelper.LogDebugMessage(appName, "Replaying game: {0}", gameFilePath);
                        if (errorReadingTickToReplayTo)
                        {
                            DebugHelper.LogDebugMessage(appName,
                                "WARNING: The fourth parameter can't be parsed as a 'Tick To Replay To'. Using default.");
                        }

                        Game gameToReplay = Game.Load(gameFilePath);
                        if (tickToReplayTo == -1)
                        {
                            tickToReplayTo = gameToReplay.CurrentTurn.Tick;
                        }
                        DebugHelper.LogDebugMessage(appName, "Replaying to turn {0}.", tickToReplayTo);
                        if (gameToReplay.Turns[tickToReplayTo].TankActionsTakenAfterPreviousTurn == null)
                        {
                            throw new ApplicationException(
                                String.Format(
                                    "No tank actions were recorded for the game's last tick ({0})",
                                    tickToReplayTo
                                )
                            );
                        }

                        solver.GameToReplay = gameToReplay;
                        solver.TickToReplayTo = tickToReplayTo;
                    }
                }

                // Write log file headers and set the coordinator running:
                DebugHelper.LogDebugMessage(appName, "Running solver: {0}", solver.Name);
                DebugHelper.WriteLine('=');
                DebugHelper.WriteLine();
                try
                {
                    if (stepIntoGame)
                    {
                        // The tickToReplayTo is actually the playerIndex
                        // Load the game state as the initial game state of the game:
                        DebugHelper.LogDebugMessage(appName,
                            "Stepping into solver for player {0} with initial game state loaded from game file {1}", 
                            tickToReplayTo, gameFilePath);
                        coordinator.StepInto(yourPlayerIndex: tickToReplayTo, initialGameStateFilePath: gameFilePath);
                        DebugHelper.LogDebugMessage(appName, "coordinator.BestMoveSoFar =\r\n{0}", coordinator.BestMoveSoFar);
                    }
                    else
                    {
                        coordinator.Run();
                    }
                }
                catch (Exception exc)
                {
                    DebugHelper.LogDebugError(appName, exc);
                }
            }
            finally
            {
                DebugHelper.LogDebugMessage(appName, "EXITING");
                DebugHelper.WriteLine('-');
            }
        }
    }
}
