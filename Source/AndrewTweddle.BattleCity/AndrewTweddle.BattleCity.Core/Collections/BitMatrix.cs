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
        private const int MASK_LEAST_SIGNIFICANT_SEGMENT_BITS = 31;
        private const int MASK_MOST_SIGNIFICANT_BIT = 1 << (BITS_PER_INT - 1);
        private const int MASK_CENTRE_OF_SEGMENT = 1 << Constants.TANK_EXTENT_OFFSET;

        private static bool[] doesSegmentCrossBitBoundary;
        private static int[,] segmentMasks;

        public short Height { get; private set; }
        public short Width { get; private set; }

        private int[] bits;

        static BitMatrix()
        {
            doesSegmentCrossBitBoundary = new bool[BITS_PER_INT];
            segmentMasks = new int[2, BITS_PER_INT];

            int leftMask = MASK_LEAST_SIGNIFICANT_SEGMENT_BITS;
            int rightMask = 1;
            bool boundaryThresholdCrossed = false;

            for (int i = 0; i < BITS_PER_INT; i++)
            {
                segmentMasks[0, i] = leftMask;
                if (boundaryThresholdCrossed)
                {
                    doesSegmentCrossBitBoundary[i] = true;
                    segmentMasks[1, i] = rightMask;
                    rightMask = (rightMask << 1) | 1;
                }
                else
                {
                    if ((leftMask & MASK_MOST_SIGNIFICANT_BIT) != 0)
                    {
                        boundaryThresholdCrossed = true;
                    }
                }
                leftMask <<= 1;
            }
        }

        public BitMatrix(short width, short height)
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
            BitMatrix clonedMatrix = new BitMatrix(Width, Height);
            clonedMatrix.bits = bits;
            return clonedMatrix;
        }

        public Matrix<SegmentState> GetBoardSegmentMatrixForSegmentAxis(Axis segmentAxis)
        {
            return GetBoardSegmentMatrixForAxisOfMovement(segmentAxis.GetPerpendicular());
        }

        public Matrix<SegmentState> GetBoardSegmentMatrixForAxisOfMovement(Axis axisOfMovement)
        {
            Matrix<SegmentState> segmentMatrix = new Matrix<SegmentState>(Width, Height);
            switch (axisOfMovement)
            {
                case Axis.Horizontal:
                    SetSegmentMatrixForHorizontalMovement(segmentMatrix);
                    break;
                case Axis.Vertical:
                    SetSegmentMatrixForVerticalMovement(segmentMatrix);
                    break;
            }
            return segmentMatrix;
        }

        private void SetSegmentMatrixForHorizontalMovement(Matrix<SegmentState> segmentMatrix)
        {
            short y;

            for (short x = 0; x < Width; x++)
            {
                int segment = 0;
                short centreY = 2;

                for (y = 0; y < Constants.SEGMENT_SIZE - 1; y++)
                {
                    if (this[x, y])
                    {
                        segment = (segment << 1) | 1;
                    }
                    else
                    {
                        segment <<= 1;
                    }
                }

                for (y = Constants.SEGMENT_SIZE - 1; y < Height; y++, centreY++)
                {
                    if (this[x, y])
                    {
                        segment = ((segment << 1) & MASK_LEAST_SIGNIFICANT_SEGMENT_BITS ) | 1;
                    }
                    else
                    {
                        segment = (segment << 1) & MASK_LEAST_SIGNIFICANT_SEGMENT_BITS;
                    }

                    if (segment != 0)
                    {
                        if ((segment & MASK_CENTRE_OF_SEGMENT) != 0)
                        {
                            segmentMatrix[x, centreY] = SegmentState.ShootableWall;
                        }
                        else
                        {
                            segmentMatrix[x, centreY] = SegmentState.UnshootablePartialWall;
                        }
                    }
                    else
                    {
                        segmentMatrix[x, centreY] = SegmentState.Clear;
                    }
                }
            }
        }

        private void SetSegmentMatrixForVerticalMovement(Matrix<SegmentState> segmentMatrix)
        {
            int leftMask;
            int rightMask;
            int leftPointIndex;
            int offset;
            bool isSplit;
            int combinedMask;

            for (short y = 0; y < Height; y++)
            {
                int startOfRow = y * Width;

                for (int leftX = 0; leftX < Width - Constants.SEGMENT_SIZE; leftX++)
                {
                    leftPointIndex = (startOfRow + leftX) / BITS_PER_INT;
                    offset = (startOfRow + leftX) % BITS_PER_INT;
                    leftMask = segmentMasks[0, offset];
                    isSplit = doesSegmentCrossBitBoundary[offset];
                    if (isSplit)
                    {
                        rightMask = segmentMasks[1, offset];
                        combinedMask = (bits[leftPointIndex] & leftMask) | (bits[leftPointIndex + 1] & rightMask);
                    }
                    else
                    {
                        combinedMask = bits[leftPointIndex] & leftMask;
                    }
                    if (combinedMask != 0)
                    {
                        if (this[(short) (leftX + 2), y])
                        {
                            segmentMatrix[(short) (leftX + 2), y] = SegmentState.ShootableWall;
                        }
                        else
                        {
                            segmentMatrix[(short)(leftX + 2), y] = SegmentState.UnshootablePartialWall;
                        }
                    }
                    else
                    {
                        segmentMatrix[(short)(leftX + 2), y] = SegmentState.Clear;
                    }
                }
            }
        }
    }
}
