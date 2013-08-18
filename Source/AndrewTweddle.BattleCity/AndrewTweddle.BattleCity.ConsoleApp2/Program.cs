using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Comms.Client;
using AndrewTweddle.BattleCity.AI;
using AndrewTweddle.BattleCity.AI.Solvers;
using AndrewTweddle.BattleCity.Bots;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.ConsoleApp2
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
                string serverUrl = args[0];
                WebServiceAdapter wsAdapter = new WebServiceAdapter
                {
                    Url = serverUrl,
                    EndPointConfigurationName = "ChallengePort"
                };
                ISolver<MutableGameState> solver = new RandomBot<MutableGameState>();
                Coordinator<MutableGameState> coordinator = new Coordinator<MutableGameState>(solver, wsAdapter);
                coordinator.Run();
            }
        }
    }
}
