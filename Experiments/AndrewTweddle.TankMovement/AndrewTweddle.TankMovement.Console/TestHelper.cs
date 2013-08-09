using AndrewTweddle.BattleCity.VisualUtils;
using AndrewTweddle.TankMovement.Core;
using AndrewTweddle.TankMovement.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace AndrewTweddle.TankMovement.Console
{
    public static class TestHelper
    {
        public static void DisplayTestTitle(string testTitle)
        {
            System.Console.WriteLine(new String('=', testTitle.Length));
            System.Console.WriteLine(testTitle);
            System.Console.WriteLine();
        }

        public static void DisplayBoardResolveAndCheckExpectations(string imageFileName, ref bool[,] isAWall, MoveStatus[] expectedMoveStatuses, 
            Unit[] tanks, Bullet[] bullets = null, Base[] bases = null, GameRuleConfiguration ruleConfig = null, string imageFolderPath = null)
        {
            ImageGenerator imgGen = new ImageGenerator
            {
                Magnification = 5,
                IsBackgroundChequered = false
            };

            try
            {
                string indentation = new string(' ', isAWall.GetLength(1) / 2 - 2);
                System.Console.WriteLine("{0}BOARD BEFORE:", indentation);
                System.Console.WriteLine();

                DisplayBoard(ref isAWall, tanks, bullets);
                if (!String.IsNullOrEmpty(imageFolderPath))
                {
                    string imagePath = Path.Combine(imageFolderPath, String.Format("{0}_BEFORE.bmp", imageFileName));
                    Bitmap boardImage = imgGen.GenerateBoardImage(ref isAWall, ref tanks, ref bullets, ref bases);
                    boardImage.Save(imagePath, ImageFormat.Bmp);
                }

                if (PromptToRunTest())
                {
                    TankMovementResolver resolver = Resolve(ref isAWall, tanks, bullets, ruleConfig);
                    ReportDeviationsFromExpectedResults(resolver, expectedMoveStatuses);
                    ReportResolutionDuration(resolver);

                    System.Console.WriteLine();
                    System.Console.WriteLine("{0}BOARD AFTER:", indentation);
                    System.Console.WriteLine();

                    resolver.ApplyMoves();
                    DisplayBoard(ref isAWall, tanks, bullets, foregroundColor: ConsoleColor.Yellow);

                    if (!String.IsNullOrEmpty(imageFolderPath))
                    {
                        string imagePath = Path.Combine(imageFolderPath, String.Format("{0}_AFTER.bmp", imageFileName));
                        Bitmap boardImage = imgGen.GenerateBoardImage(ref isAWall, ref tanks, ref bullets, ref bases);
                        boardImage.Save(imagePath, ImageFormat.Bmp);
                    }

                    PromptToRunNextTest();
                }
            }
            catch (Exception exc)
            {
                ConsoleColor oldFGColor = System.Console.ForegroundColor;
                System.Console.ForegroundColor = ConsoleColor.Red;
                try
                {
                    // TODO: We should write to the error stream instead.
                    System.Console.WriteLine("An error occurred while running the tests: ");
                    System.Console.WriteLine(exc);
                }
                finally
                {
                    System.Console.ForegroundColor = oldFGColor;
                }
            }
        }

        public static void DisplayBoard(ref bool[,] isAWall, Unit[] tanks, Bullet[] bullets, ConsoleColor foregroundColor = ConsoleColor.White)
        {
            ConsoleColor oldFGColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = foregroundColor;
            try
            {
                string[] boardRows = BoardHelper.GenerateViewOfBoard(ref isAWall, tanks, bullets);
                foreach (string boardRow in boardRows)
                {
                    System.Console.WriteLine(boardRow);
                }
                System.Console.WriteLine();
            }
            finally
            {
                System.Console.ForegroundColor = oldFGColor;
            }
        }

        public static TankMovementResolver Resolve(ref bool[,] isAWall, Unit[] tanks, 
            Bullet[] bullets = null, GameRuleConfiguration ruleConfig = null)
        {
            if (ruleConfig == null)
            {
                ruleConfig = new GameRuleConfiguration();
            }
            TankMovementResolver resolver = new TankMovementResolver(isAWall, tanks, bullets, ruleConfig);
            resolver.ResolveMoves();
            return resolver;
        }

        public static void ReportDeviationsFromExpectedResults(TankMovementResolver resolver, MoveStatus[] expectedMoveStatuses)
        {
            for (int i = 0; i < 4; i++)
            {
                MoveRequest request = resolver.Requests[i];
                ConsoleColor oldFGColor = System.Console.ForegroundColor;
                try
                {
                    if (request.Status == expectedMoveStatuses[i])
                    {
                        System.Console.ForegroundColor = ConsoleColor.Green;
                        System.Console.WriteLine("    Tank {0} move = {1} (as expected).", i, request.Status);
                    }
                    else
                    {
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        System.Console.WriteLine("    Tank {0} move. Expected: {1}. Actual: {2}.", i, expectedMoveStatuses[i], request.Status);
                    }
                }
                finally
                {
                    System.Console.ForegroundColor = oldFGColor;
                }
            }
        }

        public static void ReportResolutionDuration(TankMovementResolver resolver)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Duration of resolver algorithm: {0}", resolver.ResolutionDuration);
        }

        public static bool PromptToRunTest()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Press ENTER to run the test or N to skip it");
            string response = System.Console.ReadLine();
            return String.IsNullOrEmpty(response) || response[0] == 'Y' || response[0] == 'y';
        }

        public static void PromptToRunNextTest()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Press ENTER to run the next test");
            System.Console.ReadLine();
        }
    }
}
