using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Aux.IO;
using AndrewTweddle.BattleCity.VisualUtils;
using System.Drawing;
using System.Drawing.Imaging;
using AndrewTweddle.BattleCity.Core.Calculations;
using System.IO;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Experimental.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int repetitions = 1000;
                int smallRepetitions = 10;

                if (args.Length > 0)
                {
                    int.TryParse(args[0], out repetitions);
                    if (args.Length > 1)
                    {
                        int.TryParse(args[1], out smallRepetitions);
                    }
                }

                string logFilePath = String.Format(
                    @"C:\Competitions\EntelectChallenge2013\temp\PerformanceStats\PerfStats_{0}.txt", DateTime.Now.ToString("yyyy-MM-dd_HHmmss"));

                // Test reading of JSon file:
                ImageGenerator imageGen = new ImageGenerator { Magnification = 2 };
                BitMatrix board = TestReadingJsonBoardFile(imageGen);

                // ======================
                // Run performance tests:
                Console.WriteLine("Add any comments about this test run:");
                string comments = Console.ReadLine();
                PerformanceTestHelper.WriteCommentsToLog(logFilePath, comments);

                SegmentStateMatrixTester segmentStateMatrixTester = new SegmentStateMatrixTester
                {
                    Board = board,
                    ImageGenerator = imageGen
                };
                segmentStateMatrixTester.Test(repetitions, smallRepetitions, logFilePath);

                Matrix<SegmentState> vertSegStateMatrix = segmentStateMatrixTester.VerticalSegmentStateMatrix;
                Matrix<SegmentState> horizSegStateMatrix = segmentStateMatrixTester.HorizontalSegStateMatrix;
                Matrix<Cell> cellMatrix = segmentStateMatrixTester.CellMatrix;

                DistanceCalculationTester distCalcTester = new DistanceCalculationTester
                {
                    Board = board,
                    HorizSegmentStateMatrix = horizSegStateMatrix,
                    VertSegmentStateMatrix = vertSegStateMatrix
                };
                distCalcTester.TestDistanceCalculator(logFilePath, repetitions, smallRepetitions);

                // Test time to generate a boolean matrix from a bit matrix:
                Matrix<bool> boolMatrix = PerformanceTestHelper.TimeFunctionWithArgument(
                    logFilePath, "Convert a bit matrix to a bool matrix",
                    repetitions, board, b => b.ConvertToBoolMatrix());

                /* Compare segment state calculation times over the whole board for cached and calculated segment state calculators: */
                SegmentStateCalculationTester.ComparePerformanceOfCachedAndOnTheFlySegmentStateCalculators(
                    logFilePath, repetitions, board, cellMatrix,
                    vertSegStateMatrix, horizSegStateMatrix, boolMatrix);

                // Test construction time for a BitMatrix:
                BitMatrixTester.TestBitMatrix(logFilePath, repetitions);
            }
            catch (Exception exc)
            {
                Console.WriteLine("An unexpected error occurred: {0}", exc);
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(exc.StackTrace);
                Console.WriteLine();
            }
        }

        private static BitMatrix TestReadingJsonBoardFile(ImageGenerator imageGen)
        {
            BitMatrix board;
            string jsonFilePath = @"C:\Competitions\EntelectChallenge2013\Harnesses\TestHarnessBeta\harness\boards\board.txt";
            board = JsonBoardReader.LoadBoardFromJsonFile(jsonFilePath);
            string boardFilePath = @"c:\Competitions\EntelectChallenge2013\temp\board.bmp";
            Bitmap boardBitmap = imageGen.GenerateBoardImage(board);
            boardBitmap.Save(boardFilePath);
            return board;
        }

    }
}
