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

namespace AndrewTweddle.BattleCity.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("USAGE: serverUrl");
            }
            else
            {
                Console.WindowWidth = 120;  // columns

                // Set up debugging, using the second argument as the root folder:
                string appName = "ConsoleApp";
                string rootFolder = null;
                if (args.Length >= 2)
                {
                    rootFolder = args[1];
                }
                DebugHelper.InitializeLogFolder(DateTime.Now, rootFolder, appName);
                DebugHelper.WireUpDebugListeners(includeConsoleListener: true);

                // Set up web service adapter:
                string serverUrl = args[0];
                WebServiceAdapter wsAdapter = new WebServiceAdapter
                {
                    Url = serverUrl,
                    EndPointConfigurationName = "ChallengePort"
                };

                // Set up solvers and coordinators:
                ISolver<MutableGameState> solver = new ShortestPathBot<MutableGameState>();  // new RandomBot<MutableGameState>();
                ICommunicatorCallback communicatorCallback = new RemoteCommunicatorCallback();
                Coordinator<MutableGameState> coordinator = new Coordinator<MutableGameState>(solver, wsAdapter, communicatorCallback);

                // Write log file headers and set the coordinator running:
                DebugHelper.LogDebugMessage(appName, "Running solver: {0}", solver.Name);
                DebugHelper.WriteLine('=');
                DebugHelper.WriteLine();
                try
                {
                    try
                    {
                        coordinator.Run();
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
}
