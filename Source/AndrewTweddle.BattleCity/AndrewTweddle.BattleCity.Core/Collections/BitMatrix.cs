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
        private const int BITS_PER_INT = 32;

        public short Height { get; private set; }
        public short Width { get; private set; }

        private int[] bits;

        public BitMatrix(short height, short width)
        {
            Height = height;
            Width = width;

            int length = (height * width + BITS_PER_INT - 1) / BITS_PER_INT;
            bits = new int[length];
        }

        public BitMatrix() : this(Game.Current.BoardWidth, Game.Current.BoardHeight)
        {
        }

        public bool this[short x, short y]
        {
            get
            {
                /* Removed - too slow...
                if (x < 0 || x >= Width)
                {
                    throw new ArgumentOutOfRangeException("x", "The x value for the BitMatrix indexer get is out of range");
                }
                if (y < 0 || y >= Height)
                {
                    throw new ArgumentOutOfRangeException("y", "The y value for the BitMatrix indexer get is out of range");
                }
                 */

                int arrayIndex = (y * Width + x) / BITS_PER_INT;
                int bitOffset = 1 << (y * Width + x) % BITS_PER_INT;
                return (bits[arrayIndex] & bitOffset) != 0;
            }
            set
            {
                /* Removed - too slow...
                if (x < 0 || x >= Width)
                {
                    throw new ArgumentOutOfRangeException("x", "The x value for the BitMatrix indexer get is out of range");
                }
                if (y < 0 || y >= Height)
                {
                    throw new ArgumentOutOfRangeException("y", "The y value for the BitMatrix indexer get is out of range");
                }
                 */

                int arrayIndex = (y * Width + x) / BITS_PER_INT;
                int bitOffset = 1 << ((y * Width + x) % BITS_PER_INT);
                if (value)
                {
                    bits[arrayIndex] |= bitOffset;
                }
                else
                {
                    bits[arrayIndex] &= ~bitOffset;
                }
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
            BitMatrix clonedMatrix = new BitMatrix(Height, Width);
            clonedMatrix.bits = bits;
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
