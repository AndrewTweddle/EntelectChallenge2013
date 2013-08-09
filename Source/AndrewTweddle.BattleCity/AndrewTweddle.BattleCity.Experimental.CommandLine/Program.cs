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

namespace AndrewTweddle.BattleCity.Experimental.CommandLine
{
    class Program
    {
        private const short BIT_MATRIX_DIMENSION = 200;

        static void Main(string[] args)
        {
            // Test reading of JSon file:
            string jsonFilePath = @"C:\Competitions\EntelectChallenge2013\Harnesses\TestHarnessBeta\harness\boards\board.txt";
            BitMatrix board = JsonBoardReader.LoadBoardFromJsonFile(jsonFilePath);
            string boardFilePath = @"c:\Competitions\EntelectChallenge2013\temp\board.bmp";
            ImageGenerator imageGen = new ImageGenerator { Magnification = 2 };
            Bitmap boardBitmap = imageGen.GenerateBoardImage(board);
            boardBitmap.Save(boardFilePath);

            // ======================
            // Run performance tests:

            // BitMatrix tests:
            string title;
            int repetitions = 1000;
            string bitMatrixType = "BitMatrix using an int array";

            // Test segment type calculations:
            title = String.Format("Test calculation of vertical segment state matrix using a {0}", bitMatrixType);
            TimeActionWithArgument(title, repetitions, board, GetVerticalSegmentStateMatrix);

            Matrix<SegmentState> segStateMatrix = board.GetBoardSegmentMatrixForAxisOfMovement(Axis.Vertical);
            Bitmap segStateBitmap = imageGen.GenerateBoardImage(board);
            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, segStateMatrix, Axis.Vertical);
            string segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\VertSegmentMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            title = String.Format("Test calculation of horizontal segment state matrix using a {0}", bitMatrixType);
            TimeActionWithArgument(title, repetitions, board, GetHorizontalSegmentStateMatrix);

            segStateMatrix = board.GetBoardSegmentMatrixForAxisOfMovement(Axis.Horizontal);
            segStateBitmap = imageGen.GenerateBoardImage(board);
            imageGen.DrawSegmentMatrixOverlay(segStateBitmap, board, segStateMatrix, Axis.Horizontal);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\HorizSegmentMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            // Test construction time for a BitMatrix:
            title = String.Format("Construct and populate a {0}", bitMatrixType);
            TimeAction(title, repetitions, ConstructBitMatrix);

            BitMatrix bm = new BitMatrix(BIT_MATRIX_DIMENSION, BIT_MATRIX_DIMENSION);
            for (short x = 0; x < BIT_MATRIX_DIMENSION; x++)
            {
                for (short y = 0; y < BIT_MATRIX_DIMENSION; y++)
                {
                    bm[x, y] = x + y % 2 == 1;
                }
            }
            title = String.Format("Read a {0}", bitMatrixType);
            TimeActionWithArgument(title, repetitions, bm, ReadBitMatrix);

            title = String.Format("Clone a {0}", bitMatrixType);
            TimeActionWithArgument(title, repetitions, bm, CloneBitMatrix);
        }

        private static void GetVerticalSegmentStateMatrix(BitMatrix board)
        {
            Matrix<SegmentState> segStateMatrix = board.GetBoardSegmentMatrixForAxisOfMovement(Axis.Vertical);
        }

        private static void GetHorizontalSegmentStateMatrix(BitMatrix board)
        {
            Matrix<SegmentState> segStateMatrix = board.GetBoardSegmentMatrixForAxisOfMovement(Axis.Horizontal);
        }

        private static void TimeAction(string title, int repetitions, Action action)
        {
            Console.WriteLine("Timing action: {0}", title);
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < repetitions; i++)
            {
                action();
            }
            sw.Stop();
            Console.WriteLine("Duration for {0} repetitions: {1}", repetitions, sw.Elapsed);
            Console.WriteLine("Average duration: {0} microseconds", sw.ElapsedMilliseconds * 1000.0 / repetitions);
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        private static void TimeActionWithArgument<T>(string title, int repetitions, T arg, Action<T> action)
        {
            Console.WriteLine("Timing action: {0}", title);
            Console.WriteLine("Starting at: {0}", DateTime.Now);
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < repetitions; i++)
            {
                action(arg);
            }
            sw.Stop();
            Console.WriteLine("Duration for {0} repetitions: {1}", repetitions, sw.Elapsed);
            Console.WriteLine("Average duration: {0} microseconds", sw.ElapsedMilliseconds * 1000.0 / repetitions);
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        private static void ConstructBitMatrix()
        {
            BitMatrix bm = new BitMatrix(BIT_MATRIX_DIMENSION, BIT_MATRIX_DIMENSION);
            for (short x = 0; x < BIT_MATRIX_DIMENSION; x++)
            {
                for (short y = 0; y < BIT_MATRIX_DIMENSION; y++)
                {
                    bm[x, y] = x + y % 2 == 1;
                }
            }
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

        /* Timings with optimization off:

        Timing action: Construct and populate a BitMatrix using BitArrays
        Duration for 1000 repetitions: 00:00:00.9579366
        Average duration: 957 microseconds
        -------------------------------------------------------------------------------

        Timing action: Read a BitMatrix using BitArrays
        Starting at: 2013/08/09 02:00:55 PM
        Duration for 1000 repetitions: 00:00:00.9155386
        Average duration: 915 microseconds
        -------------------------------------------------------------------------------

        Timing action: Construct and populate a BitMatrix using an int array
        Duration for 1000 repetitions: 00:00:01.6278661
        Average duration: 1627 microseconds
        -------------------------------------------------------------------------------

        Timing action: Read a BitMatrix using an int array
        Starting at: 2013/08/09 02:52:07 PM
        Duration for 1000 repetitions: 00:00:01.6403336
        Average duration: 1640 microseconds
        -------------------------------------------------------------------------------

        *******************************************************************************
                Timings with Optimization on:

        Timing action: Test calculation of vertical segment state matrix using a BitMatr
        ix using an int array
        Starting at: 2013/08/09 10:08:08 PM
        Duration for 1000 repetitions: 00:00:00.0663844
        Average duration: 66 microseconds
        -------------------------------------------------------------------------------

        Timing action: Test calculation of horizontal segment state matrix using a BitMa
        trix using an int array
        Starting at: 2013/08/09 10:08:09 PM
        Duration for 1000 repetitions: 00:00:00.0521115
        Average duration: 52 microseconds
        -------------------------------------------------------------------------------

        Timing action: Construct and populate a BitMatrix using an int array
        Duration for 1000 repetitions: 00:00:00.4540543
        Average duration: 454 microseconds
        -------------------------------------------------------------------------------

        Timing action: Read a BitMatrix using an int array
        Starting at: 2013/08/09 10:08:09 PM
        Duration for 1000 repetitions: 00:00:00.3435888
        Average duration: 343 microseconds
        -------------------------------------------------------------------------------

        Timing action: Clone a BitMatrix using an int array
        Starting at: 2013/08/09 10:08:10 PM
        Duration for 1000 repetitions: 00:00:00.0012824
        Average duration: 1 microseconds
        -------------------------------------------------------------------------------          
        
        */

    }
}
