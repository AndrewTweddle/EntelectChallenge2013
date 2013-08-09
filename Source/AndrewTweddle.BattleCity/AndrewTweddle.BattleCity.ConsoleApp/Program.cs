using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Comms.Client;
using AndrewTweddle.BattleCity.AI;
using AndrewTweddle.BattleCity.AI.Solvers;
using AndrewTweddle.BattleCity.Bots;

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
                string serverUrl = args[0];
                WebServiceAdapter wsAdapter = new WebServiceAdapter
                {
                    Url = serverUrl,
                    EndPointConfigurationName = "ChallengePort"
                };
                ISolver solver = new RandomBot();
                Coordinator coordinator = new Coordinator(solver, wsAdapter);
                coordinator.Run();
            }
        }
    }
}
