using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Experimental.CommandLine
{
    public static class BitMatrixTester
    {
        private const short BIT_MATRIX_DIMENSION = 200;

        public static void TestBitMatrix(string logFilePath, int repetitions)
        {
            string bitMatrixType = "BitMatrix using an int array";
            string title;

            title = String.Format("Construct and populate a {0}", bitMatrixType);
            BitMatrix bm = PerformanceTestHelper.TimeFunction(logFilePath, title, repetitions, ConstructBitMatrix);

            title = String.Format("Read a {0}", bitMatrixType);
            PerformanceTestHelper.TimeActionWithArgument(logFilePath, title, repetitions, bm, ReadBitMatrix);

            title = String.Format("Clone a {0}", bitMatrixType);
            PerformanceTestHelper.TimeActionWithArgument(logFilePath, title, repetitions, bm, CloneBitMatrix);
        }

        public static BitMatrix ConstructBitMatrix()
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

        public static void ReadBitMatrix(BitMatrix matrixToRead)
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

        public static void CloneBitMatrix(BitMatrix matrixToRead)
        {
            BitMatrix clone = matrixToRead.Clone();
        }
    }
}
