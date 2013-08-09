using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Experimental.CommandLine
{
    class Program
    {
        private const short BIT_MATRIX_DIMENSION = 200;

        static void Main(string[] args)
        {
            string title;
            int repetitions = 1000;
            string bitMatrixType = "BitMatrix using an int array";

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

        Timing action: Construct and populate a BitMatrix using an int array
        Duration for 1000 repetitions: 00:00:00.4847228
        Average duration: 484 microseconds
        -------------------------------------------------------------------------------

        Timing action: Read a BitMatrix using an int array
        Starting at: 2013/08/09 02:59:51 PM
        Duration for 1000 repetitions: 00:00:00.3531991
        Average duration: 353 microseconds
        -------------------------------------------------------------------------------

        Timing action: Clone a BitMatrix using an int array
        Starting at: 2013/08/09 02:59:51 PM
        Duration for 1000 repetitions: 00:00:00.0014168
        Average duration: 1 microseconds
        -------------------------------------------------------------------------------
          
        */

    }
}
