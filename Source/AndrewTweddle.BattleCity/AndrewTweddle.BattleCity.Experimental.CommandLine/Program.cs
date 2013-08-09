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
        static void Main(string[] args)
        {
            string title;
            int repetitions = 1000;
            string bitMatrixType = "BitMatrix using BitArrays";

            title = String.Format("Construct and populate a {0}", bitMatrixType);
            TimeAction(title, repetitions, ConstructBitMatrix);

            BitMatrix bm = new BitMatrix(200, 200);
            for (short x = 0; x < 200; x++)
            {
                for (short y = 0; y < 200; y++)
                {
                    bm[x, y] = x + y % 2 == 1;
                }
            }
            title = String.Format("Read a {0}", bitMatrixType);
            TimeActionWithArgument(title, repetitions, bm, ReadBitMatrix);
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
            BitMatrix bm = new BitMatrix(200, 200);
            for (short x = 0; x < 200; x++)
            {
                for (short y = 0; y < 200; y++)
                {
                    bm[x, y] = x + y % 2 == 1;
                }
            }
        }

        private static void ReadBitMatrix(BitMatrix matrixToRead)
        {
            bool b;
            for (short x = 0; x < matrixToRead.ColumnCount; x++)
            {
                for (short y = 0; y < matrixToRead.RowCount; y++)
                {
                    b = matrixToRead[x, y];
                }
            }
        }

        /* Timings...

Timing action: Construct and populate a BitMatrix using BitArrays
Duration for 1000 repetitions: 00:00:00.9579366
Average duration: 957 microseconds
-------------------------------------------------------------------------------

Timing action: Read a BitMatrix using BitArrays
Starting at: 2013/08/09 02:00:55 PM
Duration for 1000 repetitions: 00:00:00.9155386
Average duration: 915 microseconds
-------------------------------------------------------------------------------

         */

    }
}
