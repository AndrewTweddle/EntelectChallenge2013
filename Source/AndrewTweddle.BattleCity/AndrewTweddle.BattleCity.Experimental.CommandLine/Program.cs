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

            // Test segment type calculations:
            board.ReadCount = 0;
            board.WriteCount = 0;
            Matrix<SegmentState> vertSegStateMatrix 
                = TimeFunctionWithArgument(logFilePath,
                    "Test calculation of vertical segment state matrix directly from the BitMatrix", 
                    repetitions, board, GetVerticalSegmentStateMatrix);
            WriteToLog(logFilePath, string.Format("\r\n{0} reads, {1} writes\r\n", board.ReadCount, board.WriteCount));

            // Save image for vertical segment state matrix:
            Bitmap segStateBitmap = imageGen.GenerateBoardImage(board);
            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, vertSegStateMatrix, Axis.Vertical);
            string segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\VertSegmentMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            board.ReadCount = 0;
            board.WriteCount = 0;
            Matrix<SegmentState> horizSegStateMatrix
                = TimeFunctionWithArgument(logFilePath, 
                    "Test calculation of horizontal segment state matrix directly from the BitMatrix", 
                    repetitions, board, GetHorizontalSegmentStateMatrix);
            WriteToLog(logFilePath, string.Format("\r\n{0} reads, {1} writes\r\n", board.ReadCount, board.WriteCount));

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
            board.ReadCount = 0;
            board.WriteCount = 0;
            vertSegStateMatrix = TimeFunctionWithArgument(logFilePath,
                "Test calculation of vertical segment state matrix using a cell matrix", 
                repetitions, Tuple.Create(cellMatrix, board, Axis.Vertical),
                GetSegmentStateMatrixUsingCellMatrix);
            WriteToLog(logFilePath, string.Format("\r\n{0} reads, {1} writes\r\n", board.ReadCount, board.WriteCount));

            // Save image for vertical segment state matrix:
            segStateBitmap = imageGen.GenerateBoardImage(board);
            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, vertSegStateMatrix, Axis.Vertical);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\VertSegmentMatrixUsingCellMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            board.ReadCount = 0;
            board.WriteCount = 0;
            horizSegStateMatrix = TimeFunctionWithArgument(logFilePath, 
                "Test calculation of horizontal segment state matrix using a cell matrix", 
                repetitions, Tuple.Create(cellMatrix, board, Axis.Horizontal),
                GetSegmentStateMatrixUsingCellMatrix);
            WriteToLog(logFilePath, string.Format("\r\n{0} reads, {1} writes\r\n", board.ReadCount, board.WriteCount));

            // Repeat segment state calculation using Segment matrix calculated from the Cell matrix:
            Matrix<Segment> vertSegmentMatrix = SegmentCalculator.GetSegmentMatrix(cellMatrix, board, Axis.Vertical);
            vertSegStateMatrix = TimeFunctionWithArgument(logFilePath,
                "Test calculation of vertical segment state matrix using a segment matrix", 
                repetitions, Tuple.Create(vertSegmentMatrix, board),
                (tuple) => SegmentCalculator.GetBoardSegmentStateMatrixFromSegmentMatrix(tuple.Item1, tuple.Item2));

            Matrix<Segment> horizSegmentMatrix = SegmentCalculator.GetSegmentMatrix(cellMatrix, board, Axis.Horizontal);
            horizSegStateMatrix = TimeFunctionWithArgument(logFilePath,
                "Test calculation of vertical segment state matrix using a segment matrix", 
                repetitions, Tuple.Create(horizSegmentMatrix, board),
                (tuple) => SegmentCalculator.GetBoardSegmentStateMatrixFromSegmentMatrix(tuple.Item1, tuple.Item2));

            // Save image for horizontal segment state matrix:
            segStateBitmap = imageGen.GenerateBoardImage(board);
            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, horizSegStateMatrix, Axis.Horizontal);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\HorizSegmentMatrixUsingCellMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, vertSegStateMatrix, Axis.Vertical);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\BiDiSegmentMatrixUsingCellMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            // Test time to generate a boolean matrix from a bit matrix:
            Matrix<bool> boolMatrix = TimeFunctionWithArgument(
                logFilePath, "Convert a bit matrix to a bool matrix",
                repetitions, board, b => b.ConvertToBoolMatrix());

            /* Compare segment state calculation times over the whole board for cached and calculated segment state calculators: */
            ComparePerformanceOfCachedAndOnTheFlySegmentStateCalculators(logFilePath, repetitions,
                board, cellMatrix, vertSegStateMatrix, horizSegStateMatrix, boolMatrix);

            // Test construction time for a BitMatrix:
            TestBitMatrix(logFilePath, repetitions);
        }

        #region Segment state calculations

        private static void ComparePerformanceOfCachedAndOnTheFlySegmentStateCalculators(
            string logFilePath, int repetitions, BitMatrix board, Matrix<Cell> cellMatrix,
            Matrix<SegmentState> vertSegStateMatrix, Matrix<SegmentState> horizSegStateMatrix, Matrix<bool> boolMatrix)
        {
            // Cache based segment state calculator:
            CacheBasedSegmentStateCalculator cacheSegStateCalculator = new CacheBasedSegmentStateCalculator(horizSegStateMatrix, vertSegStateMatrix);
            TimeActionWithArgument(logFilePath, "Calculate all segments using cache calculator", repetitions,
                Tuple.Create(board.BottomRight, cacheSegStateCalculator),
                CalculateSegmentStatesUsingCache);

            // Calculation based segment state calculator:
            CalculationBasedSegmentStateCalculator calcSegStateCalculator = new CalculationBasedSegmentStateCalculator(board);
            TimeActionWithArgument(logFilePath, "Calculate all segments using direct BitMatrix-based calculator", repetitions,
                Tuple.Create(board.BottomRight, calcSegStateCalculator),
                CalculateSegmentStatesUsingCalculationBasedCalculator);

            // Bool Matrix based on the fly segment state calculator:
            OnTheFlyBoolMatrixBasedSegmentStateCalculator boolMatrixBasedSegStateCalculator
                = new OnTheFlyBoolMatrixBasedSegmentStateCalculator(boolMatrix);
            TimeActionWithArgument(logFilePath, "Calculate all segments using direct bool matrix based calculator", repetitions,
                Tuple.Create(boolMatrix.BottomRight, boolMatrixBasedSegStateCalculator),
                CalculateSegmentStatesUsingOnTheFlyBoolBoolBasedCalculator);

            CellMatrixBasedSegmentStateCalculator cellCacheSegStateCalculator 
                = new CellMatrixBasedSegmentStateCalculator(board, cellMatrix);
            TimeActionWithArgument(logFilePath, "Calculate all segments using cell matrix-based cache calculator",
                repetitions, Tuple.Create(board.BottomRight, cellCacheSegStateCalculator),
                CalculateSegmentStatesUsingCellCacheBasedCalculator);

            /* Compare segment state calculation times over random points for cached and calculated segment state calculators, 
             * to make sure the cache calculator is not just benefiting from memory cache effects due to the sequence of points: 
             */
            Core.Point[] randomPoints = GenerateRandomPoints(board.TopLeft, board.BottomRight, 10000);

            // Cache based segment state calculator:
            TimeActionWithArgument(logFilePath, "Calculate segments on random points using cache calculator", repetitions,
                Tuple.Create(randomPoints, cacheSegStateCalculator),
                CalculateSegmentStatesOnRandomPointsUsingCache);

            TimeActionWithArgument(logFilePath, "Calculate segments on random points using cell matrix-based cache calculator", repetitions,
                Tuple.Create(randomPoints, cellCacheSegStateCalculator),
                CalculateSegmentStatesOnRandomPointsUsingCache);

            // Calculation based segment state calculator:
            TimeActionWithArgument(logFilePath, "Calculate segments on random points using on the fly calculator", repetitions,
                Tuple.Create(randomPoints, calcSegStateCalculator),
                CalculateSegmentStatesOnRandomPointsUsingCalculationBasedCalculator);

            // Bool Matrix based on the fly segment state calculator:
            TimeActionWithArgument(logFilePath, "Calculate segments on random points using on the fly bool matrix based calculator", 
                repetitions, Tuple.Create(randomPoints, boolMatrixBasedSegStateCalculator),
                CalculateSegmentStatesOnRandomPointsUsingOnTheFlyBoolBoolBasedCalculator);
        }

        private static void CalculateSegmentStatesUsingCache(Tuple<Core.Point, CacheBasedSegmentStateCalculator> tuple)
        {
            AndrewTweddle.BattleCity.Core.Point bottomRight = tuple.Item1;
            CacheBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                for (int x = 0; x < bottomRight.X; x++)
                {
                    for (int y = 0; y < bottomRight.Y; y++)
                    {
                        SegmentState segState = calculator.GetSegmentState(axis, x, y);
                    }
                }
            }
        }

        private static void CalculateSegmentStatesUsingCellCacheBasedCalculator(Tuple<Core.Point, CellMatrixBasedSegmentStateCalculator> tuple)
        {
            AndrewTweddle.BattleCity.Core.Point bottomRight = tuple.Item1;
            CellMatrixBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                for (int x = 0; x < bottomRight.X; x++)
                {
                    for (int y = 0; y < bottomRight.Y; y++)
                    {
                        SegmentState segState = calculator.GetSegmentState(axis, x, y);
                    }
                }
            }
        }

        public static void CalculateSegmentStatesUsingCalculationBasedCalculator(
            Tuple<Core.Point, CalculationBasedSegmentStateCalculator> tuple)
        {
            Core.Point bottomRight = tuple.Item1;
            CalculationBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                for (int x = 0; x < bottomRight.X; x++)
                {
                    for (int y = 0; y < bottomRight.Y; y++)
                    {
                        SegmentState segState = calculator.GetSegmentState(axis, x, y);
                    }
                }
            }
        }

        public static void CalculateSegmentStatesUsingOnTheFlyBoolBoolBasedCalculator(
            Tuple<Core.Point, OnTheFlyBoolMatrixBasedSegmentStateCalculator> tuple)
        {
            Core.Point bottomRight = tuple.Item1;
            OnTheFlyBoolMatrixBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                for (int x = 0; x < bottomRight.X; x++)
                {
                    for (int y = 0; y < bottomRight.Y; y++)
                    {
                        SegmentState segState = calculator.GetSegmentState(axis, x, y);
                    }
                }
            }
        }

        public static void CalculateSegmentStatesOnRandomPointsUsingCache(Tuple<Core.Point[], CacheBasedSegmentStateCalculator> tuple)
        {
            Core.Point[] randomPoints = tuple.Item1;
            CacheBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                foreach (Core.Point randomPoint in randomPoints)
                {
                    SegmentState segState = calculator.GetSegmentState(axis, randomPoint);
                }
            }
        }

        public static void CalculateSegmentStatesOnRandomPointsUsingCache(Tuple<Core.Point[], CellMatrixBasedSegmentStateCalculator> tuple)
        {
            Core.Point[] randomPoints = tuple.Item1;
            CellMatrixBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                foreach (Core.Point randomPoint in randomPoints)
                {
                    SegmentState segState = calculator.GetSegmentState(axis, randomPoint);
                }
            }
        }

        public static void CalculateSegmentStatesOnRandomPointsUsingCalculationBasedCalculator(
            Tuple<Core.Point[], CalculationBasedSegmentStateCalculator> tuple)
        {
            Core.Point[] randomPoints = tuple.Item1;
            CalculationBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                foreach (Core.Point randomPoint in randomPoints)
                {
                    SegmentState segState = calculator.GetSegmentState(axis, randomPoint);
                }
            }
        }

        public static void CalculateSegmentStatesOnRandomPointsUsingOnTheFlyBoolBoolBasedCalculator(
            Tuple<Core.Point[], OnTheFlyBoolMatrixBasedSegmentStateCalculator> tuple)
        {
            Core.Point[] randomPoints = tuple.Item1;
            OnTheFlyBoolMatrixBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                foreach (Core.Point randomPoint in randomPoints)
                {
                    SegmentState segState = calculator.GetSegmentState(axis, randomPoint);
                }
            }
        }

        #endregion

        private static void TestBitMatrix(string logFilePath, int repetitions)
        {
            string bitMatrixType = "BitMatrix using an int array";
            string title;

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
            Matrix<SegmentState> segStateMatrix = board.GetBoardSegmentStateMatrixForAxisOfMovement(Axis.Vertical);
            return segStateMatrix;
        }

        private static Matrix<SegmentState> GetHorizontalSegmentStateMatrix(BitMatrix board)
        {
            Matrix<SegmentState> segStateMatrix = board.GetBoardSegmentStateMatrixForAxisOfMovement(Axis.Horizontal);
            return segStateMatrix;
        }

        private static Matrix<SegmentState> GetSegmentStateMatrixUsingCellMatrix(
            Tuple<Matrix<Cell>, BitMatrix, Axis> cellMatrixAndBoardAndAxis)
        {
            Matrix<SegmentState> segStateMatrix 
                = SegmentCalculator.GetBoardSegmentStateMatrixForAxisOfMovement(
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

        private static void TimeActionWithArgument<T>(string logFilePath, 
            string title, int repetitions, T arg, Action<T> action)
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

        private static void WriteToLog(string logFilePath, string message)
        {
            File.AppendAllText(logFilePath, message);
            Console.Write(message);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="pointCount"></param>
        /// <param name="seed">The random seed to use, or a negative number to randomize the seed</param>
        /// <returns></returns>
        public static Core.Point[] GenerateRandomPoints(Core.Point topLeft, Core.Point bottomRight, int pointCount, int seed = -1)
        {
            int width = bottomRight.X - topLeft.X + 1;
            int height = bottomRight.Y - topLeft.Y + 1;

            Random rnd;
            if (seed < 0)
            {
                rnd = new Random();
            }
            else
            {
                rnd = new Random(seed);
            }
            Core.Point[] randomPoints = new Core.Point[pointCount];
            for (int i = 0; i < randomPoints.Length; i++)
            {
                int randomX = topLeft.X + rnd.Next(width);
                int randomY = topLeft.Y + rnd.Next(height);
                Core.Point randomPoint = new Core.Point((short) randomX, (short) randomY);
                randomPoints[i] = randomPoint;
            }
            return randomPoints;
        }
    }
}
