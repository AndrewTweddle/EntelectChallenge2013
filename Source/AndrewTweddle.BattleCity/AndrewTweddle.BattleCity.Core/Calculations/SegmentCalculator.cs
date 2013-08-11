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


        /* Do segment state calculations: */

        public static Matrix<SegmentState> GetBoardSegmentMatrixForAxisOfMovement(Matrix<Cell> cellMatrix, BitMatrix board, Axis axis)
        {
            Matrix<SegmentState> segmentMatrix = new Matrix<SegmentState>(board.Width, board.Height);
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Cell cell = cellMatrix[x, y];
                    Segment segment = cell.GetSegmentByAxisOfMovement(axis);
                    SegmentState segmentState;
                    if (segment.IsOutOfBounds)
                    {
                        segmentState = SegmentState.OutOfBounds;
                    }
                    else
                    {
                        bool isSegmentClear = board.AreAllMaskedElementsClear(segment.BitMasksOfPoints);
                        if (isSegmentClear)
                        {
                            segmentState = SegmentState.Clear;
                        }
                        else
                        {
                            if (board[cell.BitIndexAndMask])
                            {
                                segmentState = SegmentState.ShootableWall;
                            }
                            else
                            {
                                segmentState = SegmentState.UnshootablePartialWall;
                            }
                        }
                    }
                    segmentMatrix[x, y] = segmentState;
                }
            }
            return segmentMatrix;
        }
    }
}
