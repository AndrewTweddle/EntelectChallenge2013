using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Tournaments;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace AndrewTweddle.BattleCity.Harnesses.ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 9)
            {
                System.Console.WriteLine(
                    "Usage: Style[MANUAL/AUTO] EntelectHarnessFolderPath Player1Name Player1FolderPath Player1OutputPath Player2Name Player2FolderPath Player2OutputPath ResultsFolderPath [StartIndex [EndIndex]]");
            }

            string styleString = args[0];
            bool isLaunchedManually = styleString[0] == 'M';

            /* Initialize harness: */
            string entelectHarnessFolderPath = args[1];

            /* Initialize contestants: */
            IList<Contestant> contestants = new List<Contestant>();
            Contestant player1
                = new Contestant
                {
                    Name = args[2],
                    FolderPath = args[3],
                    OutputFolderPath = args[4],
                    Port = 7070
                };
            Contestant player2
                = new Contestant
                {
                    Name = args[5],
                    FolderPath = args[6],
                    OutputFolderPath = args[7],
                    Port = 7071
                };
            contestants.Add(player1);
            contestants.Add(player2);

            Match match = new Match
            {
                ResultsFolderPath = args[8]
            };

            int startIndex = 1;
            int endIndex;

            if (args.Length >= 10)
            {
                if (!int.TryParse(args[9], out startIndex))
                {
                    throw new ArgumentException("The repetitions (or start index) parameter is not an integer");
                }
            }
            if (args.Length < 11)
            {
                endIndex = startIndex;
                startIndex = 1;
            }
            else
            {
                if (!int.TryParse(args[10], out endIndex))
                {
                    throw new ArgumentException("The end index argument is not an integer");
                }
            }

            // Write header line to results file (but only if it doesn't already exist):
            if (!File.Exists(match.ResultsFilePath))
            {
                File.WriteAllText(match.ResultsFilePath, "Match,StartTime,EndTime,Duration,StartPlayer,SecondPlayer,Winner\r\n");
            }

            for (int matchIndex = startIndex; matchIndex <= endIndex; matchIndex++)
            {
                DateTime startTimeOfMatch = DateTime.Now;
                string matchNumberAsString = matchIndex.ToString("D2");

                // Alternate start player:
                Contestant startPlayer = contestants[1 - matchIndex % 2];
                Contestant secondPlayer = contestants[matchIndex % 2];

                // Write header:
                Console.WriteLine("=======================================================================");
                Console.WriteLine("MATCH {0} between {1} and {2}", matchNumberAsString, player1.Name, player2.Name);
                Console.WriteLine("Started at {0}", startTimeOfMatch);

                Contestant winner;
                if (isLaunchedManually)
                {
                    winner = LaunchEntelectHarnessManuallyAndGetTheWinner(
                        entelectHarnessFolderPath, startPlayer, secondPlayer, matchIndex);
                }
                else
                {
                    winner = LaunchEntelectHarnessAutomaticallyAndGetTheWinner(
                        entelectHarnessFolderPath, startPlayer, secondPlayer, matchIndex);
                }
                string winnerName = winner != null ? winner.Name : String.Empty;
                DateTime endTimeOfMatch = DateTime.Now;
                TimeSpan duration = endTimeOfMatch - startTimeOfMatch;

                // Write result to csv:
                string result = String.Format("{0},{1},{2},{3},{4},{5},{6}\r\n",
                    matchNumberAsString, startTimeOfMatch, endTimeOfMatch, duration, startPlayer.Name, secondPlayer.Name, winnerName);
                File.AppendAllText(match.ResultsFilePath, result);

                // Write duration:
                Console.WriteLine();
                Console.WriteLine("Match {0} ended at {1}", matchNumberAsString, DateTime.Now);
                Console.WriteLine("It lasted {0}", duration);
                if (!string.IsNullOrWhiteSpace(winnerName))
                {
                    Console.WriteLine("The winner was {0}", winnerName);
                }
                else
                {
                    Console.WriteLine("The game was a draw");
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            System.Console.WriteLine("Press any key to continue");
            System.Console.ReadKey();
        }

        private static Contestant LaunchEntelectHarnessAutomaticallyAndGetTheWinner(
            string entelectHarnessFolderPath, Contestant player1, Contestant player2, int matchIndex)
        {
            string entelectLaunchPath = Path.Combine(entelectHarnessFolderPath, "launch.bat");
            string arguments = String.Format(
                "-playerOne \"{0}\" -playerOnePort {1} -playerTwo \"{2}\" -playerTwoPort {3} \"{4}\" \"{5}\"",
                player1.Name, player1.Port, player2.Name, player2.Port, player1.FolderPath, player2.FolderPath);

            Console.WriteLine("LAUNCHING {0}", entelectLaunchPath);
            Console.WriteLine("ARGUMENTS: {0}", arguments);
            Console.WriteLine("...");

            ProcessStartInfo startInfo = new ProcessStartInfo(entelectLaunchPath, arguments);
            startInfo.WorkingDirectory = entelectHarnessFolderPath;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            Process process = Process.Start(startInfo);
            process.WaitForExit();

            // TODO: Load Json file with the game history

            // TODO: Save game outcome

            // TODO: Determine the winner

            // TODO: Return the winner, or null if a draw
            return null;
        }

        private static Contestant LaunchEntelectHarnessManuallyAndGetTheWinner(
            string entelectHarnessFolderPath, Contestant player1, Contestant player2, int matchIndex)
        {
            string entelectLaunchPath = Path.Combine(entelectHarnessFolderPath, "launch.bat");
            Console.WriteLine("LAUNCHING {0}", entelectLaunchPath);
            Console.WriteLine("...");

            ProcessStartInfo startInfo = new ProcessStartInfo(entelectLaunchPath);
            startInfo.WorkingDirectory = entelectHarnessFolderPath;
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = false;
            startInfo.RedirectStandardInput = false;
            startInfo.RedirectStandardError = false;
            startInfo.RedirectStandardOutput = false;
            Process process = Process.Start(startInfo);
            Thread.Sleep(15000);

            Process player1Process = LaunchPlayerProcess(player1);
            Thread.Sleep(2000);
            Process player2Process = LaunchPlayerProcess(player2);

            process.WaitForExit();

            EndPlayerProcess(player1Process);
            EndPlayerProcess(player2Process);

            // TODO: Load Json file with the game history

            // TODO: Save game outcome

            // TODO: Determine the winner

            // TODO: Return the winner, or null if a draw
            return null;
        }

        private static Process LaunchPlayerProcess(Contestant player)
        {
            Process playerProcess;
            ProcessStartInfo botStartInfo = new ProcessStartInfo(player.BatchFilePath);
            botStartInfo.WorkingDirectory = player.FolderPath;
            botStartInfo.UseShellExecute = true;
            botStartInfo.CreateNoWindow = false;
            botStartInfo.Arguments
                = string.Format("http://localhost:{0}/Challenge/ChallengeService \"{1}\"",
                    player.Port, player.OutputFolderPath);
            playerProcess = Process.Start(botStartInfo);
            return playerProcess;
        }

        private static void EndPlayerProcess(Process playerProcess)
        {
            Thread.Sleep(5000);
            if (!playerProcess.HasExited)
            {
                playerProcess.Kill();
            }
        }

    }
}
