using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core.Collections
{
    public class BitMatrix
    {
        public short RowCount { get; private set; }
        public short ColumnCount { get; private set; }

        private BitArray[] Rows { get; set; }

        public BitMatrix(short rowCount, short columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;

            Rows = new BitArray[rowCount];
            for (int i = 0; i < Rows.Length; i++)
            {
                Rows[i] = new BitArray(columnCount);
            }
        }

        public BitMatrix() : this(Game.Current.BoardWidth, Game.Current.BoardHeight)
        {
        }

        public bool this[short x, short y]
        {
            get
            {
                return Rows[y][x];
            }
            set
            {
                Rows[y][x] = value;
            }
        }

        public bool this[Point point]
        {
            get
            {
                return this[point.X, point.Y];
            }
            set
            {
                this[point.X, point.Y] = value;
            }
        }

        public BitMatrix Clone()
        {
            BitMatrix clonedMatrix = new BitMatrix(RowCount, ColumnCount);
            for (int y = 0; y < Rows.Length; y++)
            {
                BitArray newRow = (BitArray) Rows[y].Clone();
                clonedMatrix.Rows[y] = newRow;
            }
            return clonedMatrix;
        }
    }
}
