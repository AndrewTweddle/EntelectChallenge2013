using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Helpers;

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

        /* TODO: Complete following after converting BitMatrix to use custom array of uint
         * 
        public Matrix<SegmentState> GetBoardSegmentMatrixForSegmentAxis(Axis segmentAxis)
        {
            return GetBoardSegmentMatrixForAxisOfMovement(segmentAxis.GetPerpendicular());
        }

        public Matrix<SegmentState> GetBoardSegmentMatrixForAxisOfMovement(Axis axisOfMovement)
        {
            Matrix<SegmentState> segmentMatrix = new Matrix<SegmentState>();
            switch (axisOfMovement)
            {
                case Axis.Horizontal:
                    SetSegmentMatrixForHorizontalMovement(segmentMatrix);
                    break;
                case Axis.Vertical:
                    // TODO: add vertical segment
                    break;
            }
            return segmentMatrix;
        }

        private static void SetSegmentMatrixForHorizontalMovement(Matrix<SegmentState> segmentMatrix)
        {
            for (short x = Constants.TANK_EXTENT_OFFSET; x < segmentMatrix.Width - Constants.TANK_EXTENT_OFFSET; x++)
            {
                for (short y = 0; y <= Constants.TANK_EXTENT_OFFSET; y++)
                {
                    segmentMatrix[x, y] = SegmentState.OutOfBounds;
                }
            }
        }
        */
    }
}
