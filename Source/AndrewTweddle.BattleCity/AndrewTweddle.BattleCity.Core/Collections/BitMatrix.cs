using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Helpers;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core.Collections
{
    [DataContract]
    public class BitMatrix
    {
        public const int BITS_PER_INT = 32;
        public const int MASK_LEAST_SIGNIFICANT_SEGMENT_BITS = 31;
        public const int MASK_MOST_SIGNIFICANT_BIT = 1 << (BITS_PER_INT - 1);
        public const int MASK_CENTRE_OF_SEGMENT = 1 << Constants.TANK_EXTENT_OFFSET;

        private static bool[] doesSegmentCrossBitBoundary;
        private static int[,] segmentMasks;

        [DataMember]
        public Point TopLeft { get; set;  }

        public Point BottomRight
        {
            get
            {
                int bottomRightX = TopLeft.X + Width - 1;
                int bottomRightY = TopLeft.Y + Height - 1;
                return new Point((short)bottomRightX, (short)bottomRightY);
            }
        }

        public Rectangle BoardBoundary
        {
            get
            {
                return new Rectangle(TopLeft, BottomRight);
            }
        }

        [DataMember]
        public int Height { get; private set; }

        [DataMember]
        public int Width { get; private set; }

        [DataMember]
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

        public BitMatrix(int width, int height)
        {
            Height = height;
            Width = width;

            int length = (height * width + BITS_PER_INT - 1) / BITS_PER_INT;
            bits = new int[length];
        }

        public BitMatrix() : this(Game.Current.BoardWidth, Game.Current.BoardHeight)
        {
        }

        /// <summary>
        /// This gets the boolean value of the point represented by the BitMatrixMask.
        /// If multiple points are specified the value will be true for all of them.
        /// When setting this property the bit or all of the bits will be updated 
        /// to zero or one depending on the boolean property value being set.
        /// </summary>
        /// <param name="indexAndMask">
        /// The index of the bit (or bits) in the array of ints plus the bit mask within that int. 
        /// Alternatively the mask could refer to multiple bits within the same element of the array
        /// </param>
        /// <returns>a boolean indicating whether the bit is set (or whether ANY of the bits is set if multiple bits were specified in the mask)</returns>
        public bool this[BitMatrixMask indexAndMask]
        {
            get
            {
                return (bits[indexAndMask.ArrayIndex] & indexAndMask.BitMask) != 0;
                // NB: If multiple bits are set in the bit mask, then this returns true if ANY of them are set in the BitMatrix
            }
            set
            {
                if (value)
                {
                    bits[indexAndMask.ArrayIndex] |= indexAndMask.BitMask;
                    // NB: If multiple bits are set in the bit mask, then this sets ALL of them
                }
                else
                {
                    bits[indexAndMask.ArrayIndex] &= ~indexAndMask.BitMask;
                    // NB: If multiple bits are set in the bit mask, then this clears ALL of them
                }
            }
        }

        public bool this[int x, int y]
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
            clonedMatrix.bits = (int[]) bits.Clone();
            return clonedMatrix;
        }

        public Matrix<SegmentState> GetBoardSegmentStateMatrixForAxisOfMovement(Axis axisOfMovement)
        {
            Matrix<SegmentState> segmentMatrix = new Matrix<SegmentState>(Width, Height);
            switch (axisOfMovement)
            {
                case Axis.Horizontal:
                    SetSegmentStateMatrixForHorizontalMovement(segmentMatrix);
                    break;
                case Axis.Vertical:
                    SetSegmentStateMatrixForVerticalMovement(segmentMatrix);
                    break;
            }
            return segmentMatrix;
        }

        public Matrix<SegmentState[]> GetAdjacentSegmentStatesByTankPositionAndDirection(
            Matrix<SegmentState> horizontalSegmentStateMatrix, Matrix<SegmentState> verticalSegmentStateMatrix)
        {
            Matrix<SegmentState[]> adjacentSegmentStates = new Matrix<SegmentState[]>();

            for (int x = 0; x < Width; x++)
            {
                int leftEdgeX = x - Constants.TANK_EXTENT_OFFSET - 1;
                int rightEdgeX = x + Constants.TANK_EXTENT_OFFSET + 1;

                for (int y = 0; y < Height; y++)
                {
                    int topEdgeY = y - Constants.TANK_EXTENT_OFFSET - 1;
                    int bottomEdgeY = y + Constants.TANK_EXTENT_OFFSET + 1;

                    if (leftEdgeX < 0)
                    {
                        // TODO: Finish this...
                    }
                }
            }

            return adjacentSegmentStates;
        }

        private void SetSegmentStateMatrixForHorizontalMovement(Matrix<SegmentState> segmentMatrix)
        {
            int y;

            for (int x = 0; x < Width; x++)
            {
                int segment = 0;
                int centreY = 2;

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

        private void SetSegmentStateMatrixForVerticalMovement(Matrix<SegmentState> segmentMatrix)
        {
            int leftMask;
            int rightMask;
            int leftPointIndex;
            int offset;
            bool isSplit;
            int combinedMask;

            for (int y = 0; y < Height; y++)
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
                        if (this[leftX + 2, y])
                        {
                            segmentMatrix[leftX + 2, y] = SegmentState.ShootableWall;
                        }
                        else
                        {
                            segmentMatrix[leftX + 2, y] = SegmentState.UnshootablePartialWall;
                        }
                    }
                    else
                    {
                        segmentMatrix[leftX + 2, y] = SegmentState.Clear;
                    }
                }
            }
        }

        public BitMatrixMask GetBitMatrixMask(int x, int y)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException("x",
                    String.Format("The x value of ({0},{1}) is out of the Bit Matrix range", x, y));
            }

            if (y < 0 && y >= Height)
            {
            }

            int arrayIndex = (y * Width + x) / BITS_PER_INT;
            int bitOffset = 1 << ((y * Width + x) % BITS_PER_INT);
            return new BitMatrixMask(arrayIndex, bitOffset);
        }

        public BitMatrixMask GetBitMatrixMask(Point point)
        {
            return GetBitMatrixMask(point.X, point.Y);
        }

        /* TODO: Rewrite following for better performance by not using LINQ: */
        public bool AreAnyMaskedElementsSet(BitMatrixMask[] maskedElements)
        {
            return maskedElements.Where(indexAndMask => (bits[indexAndMask.ArrayIndex] & indexAndMask.BitMask) != 0).Any();
        }

        public bool AreAnyMaskedElementsClear(BitMatrixMask[] maskedElements)
        {
            return !AreAllMaskedElementsSet(maskedElements);
        }

        public bool AreAllMaskedElementsClear(BitMatrixMask[] maskedElements)
        {
            for (int i = 0; i < maskedElements.Length; i++)
            {
                if ((bits[maskedElements[i].ArrayIndex] & maskedElements[i].BitMask) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool AreAllMaskedElementsSet(BitMatrixMask[] maskedElements)
        {
            return maskedElements.All(indexAndMask => (bits[indexAndMask.ArrayIndex] & indexAndMask.BitMask) == indexAndMask.BitMask);
        }

        public bool IsOnBoard(Point relativePoint)
        {
            if (TopLeft.X == 0 && TopLeft.Y == 0)
            {
                return relativePoint.X >= 0 && relativePoint.Y >= 0
                    && relativePoint.X <= BottomRight.X && relativePoint.Y <= BottomRight.Y;
            }
            else
            {
                return relativePoint.X >= TopLeft.X && relativePoint.Y >= TopLeft.Y
                    && relativePoint.X <= BottomRight.X && relativePoint.Y <= BottomRight.Y;
            }
        }

        public Matrix<bool> ConvertToBoolMatrix()
        {
            Matrix<bool> boolMatrix = new Matrix<bool>(TopLeft, Width, Height);
            for (int x = TopLeft.X; x <= BottomRight.X; x++)
            {
                for (int y = TopLeft.Y; y <= BottomRight.Y; y++)
                {
                    boolMatrix[x, y] = this[x, y];
                }
            }
            return boolMatrix;
        }

        public static bool operator==(BitMatrix one, BitMatrix other)
        {
            return one.Equals(other);
        }

        public static bool operator!=(BitMatrix one, BitMatrix other)
        {
            return !one.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is BitMatrix))
            {
                return false;
            }

            BitMatrix other = (BitMatrix)obj;
            if (TopLeft != other.TopLeft)
            {
                return false;
            }
            if (BottomRight != other.BottomRight)
            {
                return false;
            }
            
            if (bits.Length != other.bits.Length)
            {
                return false;
            }

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] != other.bits[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int arrayHash = ((IStructuralEquatable)bits).GetHashCode(EqualityComparer<int>.Default);
            return arrayHash ^ TopLeft.GetHashCode() ^ BottomRight.GetHashCode();
        }

        public IEnumerable<Point> Diff(BitMatrix other)
        {
            for (int x = TopLeft.X; x <= BottomRight.X; x++)
            {
                for (int y = TopLeft.Y; y <= BottomRight.Y; y++)
                {
                    if (this[x, y] != other[x, y])
                    {
                        yield return new Point((short)x, (short)y);
                    }
                }
            }
        }
    }
}
