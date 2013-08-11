using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public static class SegmentCalculator
    {
        public static void Calculate(Matrix<Cell> matrix)
        {
            for (int x = matrix.TopLeft.X; x <= matrix.BottomRight.X; x++)
            {
                for (int y = matrix.TopLeft.Y; y <= matrix.BottomRight.Y; y++)
                {
                    Cell cell = matrix[x, y];
                    foreach (Axis axis in BoardHelper.AllRealAxes)
                    {
                        CreateSegment(cell, axis);
                    }
                }
            }
        }

        private static void CreateSegment(Cell cell, Axis axis)
        {
            Segment newSegment = new Segment();
            newSegment.Centre = cell.Position;
            newSegment.CentreCell = cell;
            newSegment.Axis = axis;

            // Get the cells on the segment, noting that they are perpendicular to the direction of movement:
            Axis segmentAxis = axis.GetPerpendicular();
            Direction[] segmentAxisDirections = segmentAxis.ToDirections();
            Cell cellLeftOrUp = cell.GetAdjacentCell(segmentAxisDirections[0]);
            if (cellLeftOrUp != null)
            {
                newSegment.Cells[0] = cellLeftOrUp.GetAdjacentCell(segmentAxisDirections[0]);
                newSegment.Cells[1] = cellLeftOrUp;
            }
            newSegment.Cells[2] = cell;
            Cell cellRightOrDown = cell.GetAdjacentCell(segmentAxisDirections[1]);
            if (cellRightOrDown != null)
            {
                newSegment.Cells[3] = cellRightOrDown;
                newSegment.Cells[4] = cellRightOrDown.GetAdjacentCell(segmentAxisDirections[1]);
            }

            newSegment.Points
                = newSegment.Cells.Where(cc => cc != null).Select(cc => cc.Position).ToArray();
            newSegment.ValidPoints
                = newSegment.Cells.Where(cc => cc != null && cc.IsValid).Select(cc => cc.Position).ToArray();
            cell.SetSegmentByAxis(axis, newSegment);
            newSegment.IsOutOfBounds = newSegment.Cells.Where(cc => cc == null || !cc.IsValid).Any();

            // Calculate one or more BitMaskIndex'es to potentially check the walls of all of the segments in a single operation:
            newSegment.BitMasksOfPoints = newSegment.Cells
                .Where(c => c != null && c.BitIndexAndMask != null)
                .GroupBy(c => c.BitIndexAndMask.ArrayIndex).Select(
                grouping => new BitMatrixMask(
                    grouping.Key,
                    grouping.Aggregate(0, (bitMask, c) => bitMask |= c.BitIndexAndMask.BitMask))
            ).ToArray();
        }

        /* was...
        public Matrix<Segment> CalculateForAxisOfMovement(BitMatrix board, Axis axisOfMovement, 
            Matrix<Cell> cellMatrix)
        {
            Matrix<Segment> matrix = new Matrix<Segment>(board.Width, board.Height);
            switch (axisOfMovement)
            {
                case Axis.Horizontal:
                    CalculateForHorizontalMovement(matrix, board, cellMatrix);
                    break;
                case Axis.Vertical:
                    CalculateForHorizontalMovement(matrix, board, cellMatrix);
                    break;
            }
            return matrix;
        }

        private void CalculateForHorizontalMovement(Matrix<Segment> matrix, BitMatrix board, 
            Matrix<Cell> cellMatrix)
        {
            Axis axis = Axis.Horizontal;
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Segment calculation = new Segment();
                    calculation.Centre = new Point((short) x, (short) y);
                    calculation.CentreCell = cellMatrix[x, y];
                    calculation.Axis = axis;
                    calculation.Points = 
                    calculation.ValidPoints
                    calculation.Cells
                    calculation.AdjacentSegmentsByDirection
                }
            
            
            
            int y;

            for (int x = 0; x < board.Width; x++)
            {
                int segment = 0;
                int centreY = 2;

                for (y = 0; y < Constants.SEGMENT_SIZE - 1; y++)
                {
                    if (board[x, y])
                    {
                        segment = (segment << 1) | 1;
                    }
                    else
                    {
                        segment <<= 1;
                    }
                }

                for (y = Constants.SEGMENT_SIZE - 1; y < board.Height; y++, centreY++)
                {
                    if (board[x, y])
                    {
                        segment = ((segment << 1) & BitMatrix.MASK_LEAST_SIGNIFICANT_SEGMENT_BITS) | 1;
                    }
                    else
                    {
                        segment = (segment << 1) & BitMatrix.MASK_LEAST_SIGNIFICANT_SEGMENT_BITS;
                    }

                    if (segment != 0)
                    {
                        if ((segment & BitMatrix.MASK_CENTRE_OF_SEGMENT) != 0)
                        {
                            matrix[x, centreY] = SegmentState.ShootableWall;
                        }
                        else
                        {
                            matrix[x, centreY] = SegmentState.UnshootablePartialWall;
                        }
                    }
                    else
                    {
                        matrix[x, centreY] = SegmentState.Clear;
                    }
                }
            }
        }

        private void SetSegmentMatrixForVerticalMovement(Matrix<Segment> matrix, BitMatrix board, 
            Matrix<Cell> cellMatrix)
        {
            int leftMask;
            int rightMask;
            int leftPointIndex;
            int offset;
            bool isSplit;
            int combinedMask;

            for (int y = 0; y < board.Height; y++)
            {
                int startOfRow = y * board.Width;

                for (int leftX = 0; leftX < board.Width - Constants.SEGMENT_SIZE; leftX++)
                {
                    leftPointIndex = (startOfRow + leftX) / BitMatrix.BITS_PER_INT;
                    offset = (startOfRow + leftX) % BitMatrix.BITS_PER_INT;
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
                        if (board[leftX + 2, y])
                        {
                            matrix[leftX + 2, y] = SegmentState.ShootableWall;
                        }
                        else
                        {
                            matrix[leftX + 2, y] = SegmentState.UnshootablePartialWall;
                        }
                    }
                    else
                    {
                        matrix[leftX + 2, y] = SegmentState.Clear;
                    }
                }
            }
        }
        */
    }
}
