using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace OptimalSnipingAlgorithm
{
    class Program
    {
        /* Some sample wall strings and the answer:
         * Checked                 34: ----x--x----xxxx--------x-------
         * Checked                  9: xxx----
         * Not checked (too slow)  47: --x-x-x-----xxx-x---xxxx---x-xxx
         * Checked                 20: xxxx---x-xxx
         * Checked                 19: xxxxxxxxx
         * Checked                  6: ---------
         * 
         * Walls              : ---------xxxxx-----x-x-x-----xxxxxxxxxx-----xxxxx----xxxxx-x-x-x-xxxxx--x-x-x-x----xxxx-x---xxx----
         * Calculated ticks   : 145
         * Distance to target : 100
         * Time to calculate  : 00:00:00.0002559
         */
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: a single parameter of the form -#--#--- ");
                Console.WriteLine("use - for empty space, anything else for a while. Tank is left of first character. Base is right of last character");
            }
            else
            {
                int repetitions = 10000000;
                string wallString = args[0];
                bool[] walls = wallString.Select(ch => ch != '-').ToArray();
                int ticksTaken = 0;

                // Calculate using the recursive calculations:
                Stopwatch sw = Stopwatch.StartNew();
                try
                {
                    for (int i = 0; i < repetitions; i++)
                    {
                        ticksTaken = SnipingDistanceCalculator.Calculate(walls);
                    }
                }
                finally
                {
                    sw.Stop();
                }
                Console.WriteLine("Calculated ticks   : {0}", ticksTaken);
                Console.WriteLine("Distance to target : {0}", walls.Length + 1);
                Console.WriteLine("Repetitions        : {0}", ticksTaken);
                Console.WriteLine("Time to calculate  : {0}", sw.Elapsed);
                Console.WriteLine();

                if (walls.Length > 20)
                {
                    Console.WriteLine("Wall too long for the dynamic programming algorithm!");
                }
                else
                {
                    // Calculate using dynamic programming:
                    SniperState initialState = new SniperState(walls);
                    SnipingAlgorithm alg = new SnipingAlgorithm();

                    var solutionSet = alg.Solve(initialState);
                    Console.WriteLine("# solutions: {0}", solutionSet.SolutionCount);
                    Console.WriteLine("Turns taken: {0}", -solutionSet.Value);
                    Console.WriteLine();

                    for (int i = 0; i < solutionSet.SolutionCount; i++)
                    {
                        Console.WriteLine("------------");
                        Console.WriteLine("Solution {0}:", i + 1);
                        Console.WriteLine("  0: T{0}B", wallString);
                        SniperState[] states = solutionSet.GetStatesForSolution(i);
                        SniperState finalState = states[states.Length - 1];

                        foreach (var s in finalState.StateByTurn)
                        {
                            Console.WriteLine(s);
                        }
                        Console.WriteLine();
                        Console.WriteLine("Press <ENTER> to continue, or 'N <ENTER>' to exit");
                        string response = Console.ReadLine();
                        if (!String.IsNullOrEmpty(response))
                        {
                            return;
                        }
                    }
                }
            }

            // Wait 5 seconds so as not to accidentally exit prematurely...
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Press <ENTER> to exit");
            Console.ReadLine();
        }
    }
}
