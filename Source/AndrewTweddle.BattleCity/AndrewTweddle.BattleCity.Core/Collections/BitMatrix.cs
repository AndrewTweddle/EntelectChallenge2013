using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AndrewTweddle.BattleCity.Core.Collections
{
    public class BitMatrix
    {
        public short Rows { get; private set; }
        public short Columns { get; private set; }

        private BitArray[] BitRows { get; set; }

        public BitMatrix(short rows, short columns)
        {
            Rows = rows;
            Columns = columns;

            BitRows = new BitArray[rows];
            for (int i = 0; i < BitRows.Length; i++)
            {
                BitRows[i] = new BitArray(columns);
            }
        }

        public bool this[int x, int y]
        {
            get
            {
                return BitRows[y][x];
            }
        }

        public BitMatrix Clone()
        {
            BitMatrix clonedMatrix = new BitMatrix(Rows, Columns);
            for (int y = 0; y < BitRows.Length; y++)
            {
                BitArray newRow = (BitArray) BitRows[y].Clone();
                clonedMatrix.BitRows[y] = newRow;
            }
            return clonedMatrix;
        }
    }
}
