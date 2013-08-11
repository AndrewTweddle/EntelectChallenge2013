using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Aux.IO;
using AndrewTweddle.BattleCity.VisualUtils;
using System.Drawing;
using AndrewTweddle.BattleCity.Core;
using System.Drawing.Imaging;
using AndrewTweddle.BattleCity.Core.Calculations;
using System.IO;

namespace AndrewTweddle.BattleCity.Experimental.CommandLine
{
    class Program
    {
        private const short BIT_MATRIX_DIMENSION = 200;

        static void Main(string[] args)
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
            string jsonFilePath = @"C:\Competitions\EntelectChallenge2013\Harnesses\TestHarnessBeta\harness\boards\board.txt";
            BitMatrix board = JsonBoardReader.LoadBoardFromJsonFile(jsonFilePath);
            string boardFilePath = @"c:\Competitions\EntelectChallenge2013\temp\board.bmp";
            ImageGenerator imageGen = new ImageGenerator { Magnification = 2 };
            Bitmap boardBitmap = imageGen.GenerateBoardImage(board);
            boardBitmap.Save(boardFilePath);

            // ======================
            // Run performance tests:
            Console.WriteLine("Add any comments about this test run:");
            string comments = Console.ReadLine();
            WriteCommentsToLog(logFilePath, comments);

            // BitMatrix tests:
            string title;
            string bitMatrixType = "BitMatrix using an int array";

            // Test segment type calculations:
            title = String.Format("Test calculation of vertical segment state matrix using a {0}", bitMatrixType);
            Matrix<SegmentState> vertSegStateMatrix 
                = TimeFunctionWithArgument(logFilePath, title, repetitions, board, GetVerticalSegmentStateMatrix);

            // Save image for vertical segment state matrix:
            Bitmap segStateBitmap = imageGen.GenerateBoardImage(board);
            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, vertSegStateMatrix, Axis.Vertical);
            string segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\VertSegmentMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            title = String.Format("Test calculation of horizontal segment state matrix using a {0}", bitMatrixType);
            Matrix<SegmentState> horizSegStateMatrix 
                = TimeFunctionWithArgument(logFilePath, title, repetitions, board, GetHorizontalSegmentStateMatrix);

            // Save image for horizontal segment state matrix:
            segStateBitmap = imageGen.GenerateBoardImage(board);
            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, horizSegStateMatrix, Axis.Horizontal);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\HorizSegmentMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, vertSegStateMatrix, Axis.Vertical);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\BiDiSegmentMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            // Test calculation caches:
            TimeActionWithArgument(logFilePath, "Time cell calculator on challenge board 1", smallRepetitions, board, PerformCellCalculation);
            Matrix<Cell> cellMatrix = TimeFunctionWithArgument(logFilePath, "Time cell and segment calculator on challenge board 1", 
                smallRepetitions, board, PerformCellAndSegmentCalculation);

            // Repeat segment stage calculations using implementation based on pre-calculated cell and segment calculations:
            title = String.Format("Test calculation of vertical segment state matrix using a cell matrix", bitMatrixType);
            vertSegStateMatrix = TimeFunctionWithArgument(logFilePath, title, repetitions, 
                Tuple.Create(cellMatrix, board, Axis.Vertical),
                GetSegmentStateMatrixUsingCellMatrix);

            // Save image for vertical segment state matrix:
            segStateBitmap = imageGen.GenerateBoardImage(board);
            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, vertSegStateMatrix, Axis.Vertical);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\VertSegmentMatrixUsingCellMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            title = String.Format("Test calculation of horizontal segment state matrix using a cell matrix", bitMatrixType);
            horizSegStateMatrix = TimeFunctionWithArgument(logFilePath, title, repetitions,
                Tuple.Create(cellMatrix, board, Axis.Horizontal),
                GetSegmentStateMatrixUsingCellMatrix);

            // Save image for horizontal segment state matrix:
            segStateBitmap = imageGen.GenerateBoardImage(board);
            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, horizSegStateMatrix, Axis.Horizontal);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\HorizSegmentMatrixUsingCellMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, vertSegStateMatrix, Axis.Vertical);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\BiDiSegmentMatrixUsingCellMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            // Test construction time for a BitMatrix:
            title = String.Format("Construct and populate a {0}", bitMatrixType);
            BitMatrix bm = TimeFunction(logFilePath, title, repetitions, ConstructBitMatrix);

            title = String.Format("Read a {0}", bitMatrixType);
            TimeActionWithArgument(logFilePath, title, repetitions, bm, ReadBitMatrix);

            title = String.Format("Clone a {0}", bitMatrixType);
            TimeActionWithArgument(logFilePath, title, repetitions, bm, CloneBitMatrix);
        }

        private static void PerformCellCalculation(BitMatrix board)
        {
            Matrix<Cell> matrix = CellCalculator.Calculate(board);
        }

        private static Matrix<Cell> PerformCellAndSegmentCalculation(BitMatrix board)
        {
            Matrix<Cell> matrix = CellCalculator.Calculate(board);
            SegmentCalculator.Calculate(matrix);
            return matrix;
        }

        private static Matrix<SegmentState> GetVerticalSegmentStateMatrix(BitMatrix board)
        {
            Matrix<SegmentState> segStateMatrix = board.GetBoardSegmentMatrixForAxisOfMovement(Axis.Vertical);
            return segStateMatrix;
        }

        private static Matrix<SegmentState> GetHorizontalSegmentStateMatrix(BitMatrix board)
        {
            Matrix<SegmentState> segStateMatrix = board.GetBoardSegmentMatrixForAxisOfMovement(Axis.Horizontal);
            return segStateMatrix;
        }

        private static Matrix<SegmentState> GetSegmentStateMatrixUsingCellMatrix(
            Tuple<Matrix<Cell>, BitMatrix, Axis> cellMatrixAndBoardAndAxis)
        {
            Matrix<SegmentState> segStateMatrix 
                = SegmentCalculator.GetBoardSegmentMatrixForAxisOfMovement(
                    cellMatrixAndBoardAndAxis.Item1, cellMatrixAndBoardAndAxis.Item2, cellMatrixAndBoardAndAxis.Item3);
            return segStateMatrix;
        }

        private static BitMatrix ConstructBitMatrix()
        {
            BitMatrix bm = new BitMatrix(BIT_MATRIX_DIMENSION, BIT_MATRIX_DIMENSION);
            for (int x = 0; x < BIT_MATRIX_DIMENSION; x++)
            {
                for (int y = 0; y < BIT_MATRIX_DIMENSION; y++)
                {
                    bm[x, y] = x + y % 2 == 1;
                }
            }
            return bm;
        }

        private static void ReadBitMatrix(BitMatrix matrixToRead)
        {
            bool b;
            for (short x = 0; x < matrixToRead.Width; x++)
            {
                for (short y = 0; y < matrixToRead.Height; y++)
                {
                    b = matrixToRead[x, y];
                }
            }
        }

        private static void CloneBitMatrix(BitMatrix matrixToRead)
        {
            BitMatrix clone = matrixToRead.Clone();
        }

        #region Utility methods for timing and reporting on actions

        private static void TimeAction(string logFilePath, string title, int repetitions, Action action)
        {
            WriteTestTitle(logFilePath, title);
            Stopwatch swatch = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < repetitions; i++)
                {
                    action();
                }
                swatch.Stop();

                WriteDurationStats(logFilePath, repetitions, swatch);
            }
            catch (Exception exc)
            {
                WriteExceptionToLog(logFilePath, exc);
                throw;
            }
        }

        private static void TimeActionWithArgument<T>(string logFilePath, string title, int repetitions, T arg, Action<T> action)
        {
            WriteTestTitle(logFilePath, title);
            Stopwatch swatch = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < repetitions; i++)
                {
                    action(arg);
                }
                swatch.Stop();
                WriteDurationStats(logFilePath, repetitions, swatch);
            }
            catch (Exception exc)
            {
                WriteExceptionToLog(logFilePath, exc);
                throw;
            }
        }

        private static TResult TimeFunction<TResult>(string logFilePath, string title, int repetitions,
            Func<TResult> function)
        {
            TResult result = default(TResult);
            WriteTestTitle(logFilePath, title);
            Stopwatch swatch = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < repetitions; i++)
                {
                    result = function();
                }
                swatch.Stop();
                WriteDurationStats(logFilePath, repetitions, swatch);
                return result;
            }
            catch (Exception exc)
            {
                WriteExceptionToLog(logFilePath, exc);
                throw;
            }
        }

        private static TResult TimeFunctionWithArgument<T, TResult>(string logFilePath, string title, int repetitions,
            T arg, Func<T, TResult> function)
        {
            TResult result = default(TResult);
            WriteTestTitle(logFilePath, title);
            Stopwatch swatch = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < repetitions; i++)
                {
                    result = function(arg);
                }
                swatch.Stop();
                WriteDurationStats(logFilePath, repetitions, swatch);
            }
            catch (Exception exc)
            {
                WriteExceptionToLog(logFilePath, exc);
                throw;
            }
            return result;
        }

        private static void WriteCommentsToLog(string logFilePath, string comments)
        {
            if (!string.IsNullOrWhiteSpace(comments))
            {
                string message = String.Format(
                    "COMMENTS:\r\n{0}\r\n===============================================================================\r\n\r\n", 
                    comments);
                File.AppendAllText(logFilePath, message);
            }
        }

        private static void WriteExceptionToLog(string logFilePath, Exception exc)
        {
            string excMessage = String.Format("An error occurred: {0}\r\n", exc);
            File.AppendAllText(logFilePath, excMessage);
        }

        private static void WriteTestTitle(string logFilePath, string title)
        {
            string titleText = String.Format("Timing action: {0}\r\n\r\nStarting at: {1}\r\n", title, DateTime.Now);
            File.AppendAllText(logFilePath, titleText);
            Console.WriteLine(titleText);
        }

        private static void WriteDurationStats(string logFilePath, int repetitions, Stopwatch swatch)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            sw.WriteLine("Duration for {0} repetitions: {1}", repetitions, swatch.Elapsed);
            sw.WriteLine("Average duration: {0} microseconds", swatch.ElapsedMilliseconds * 1000.0 / repetitions);
            sw.WriteLine("-------------------------------------------------------------------------------");
            sw.WriteLine();
            sw.Flush();
            string durationText = sb.ToString();
            Console.Write(durationText);
            File.AppendAllText(logFilePath, durationText);
        }
        
        #endregion
    }
}
