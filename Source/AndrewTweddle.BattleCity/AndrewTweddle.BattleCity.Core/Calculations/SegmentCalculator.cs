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
        public static void Calculate(Matrix<CellCalculation> matrix, BitMatrix board)
        {
            for (int x = matrix.TopLeft.X; x <= matrix.BottomRight.X; x++)
            {
                for (int y = matrix.TopLeft.Y; y <= matrix.BottomRight.Y; y++)
                {
                    CellCalculation cellCalculation = matrix[x, y];
                    foreach (Axis axis in BoardHelper.AllRealAxes)
                    {
                        CreateSegmentCalculation(cellCalculation, axis);
                    }
                }
            }
        }

        private static void CreateSegmentCalculation(CellCalculation cellCalculation, Axis axis)
        {
            SegmentCalculation segmentCalc = new SegmentCalculation();
            segmentCalc.Centre = cellCalculation.Position;
            segmentCalc.CentreCalculation = cellCalculation;
            segmentCalc.Axis = axis;

            // Get the cells on the segment, noting that they are perpendicular to the direction of movement:
            Axis segmentAxis = axis.GetPerpendicular();
            Direction[] segmentAxisDirections = segmentAxis.ToDirections();
            CellCalculation cellCalcLeftOrUp = cellCalculation.GetAdjacentCellCalculation(segmentAxisDirections[0]);
            if (cellCalcLeftOrUp != null)
            {
                segmentCalc.CellCalculations[0] = cellCalcLeftOrUp.GetAdjacentCellCalculation(segmentAxisDirections[0]);
                segmentCalc.CellCalculations[1] = cellCalcLeftOrUp;
            }
            segmentCalc.CellCalculations[2] = cellCalculation;
            CellCalculation cellCalcRightOrDown = cellCalculation.GetAdjacentCellCalculation(segmentAxisDirections[1]);
            if (cellCalcRightOrDown != null)
            {
                segmentCalc.CellCalculations[3] = cellCalcRightOrDown;
                segmentCalc.CellCalculations[4] = cellCalcRightOrDown.GetAdjacentCellCalculation(segmentAxisDirections[1]);
            }

            segmentCalc.Points
                = segmentCalc.CellCalculations.Where(cc => cc != null).Select(cc => cc.Position).ToArray();
            segmentCalc.ValidPoints
                = segmentCalc.CellCalculations.Where(cc => cc != null && cc.IsValid).Select(cc => cc.Position).ToArray();
            cellCalculation.SetSegmentCalculationByAxis(axis, segmentCalc);
            segmentCalc.IsOutOfBounds = segmentCalc.CellCalculations.Where(cc => cc == null || !cc.IsValid).Any();
        }

        /* was...
        public Matrix<SegmentCalculation> CalculateForAxisOfMovement(BitMatrix board, Axis axisOfMovement, 
            Matrix<CellCalculation> cellMatrix)
        {
            Matrix<SegmentCalculation> matrix = new Matrix<SegmentCalculation>(board.Width, board.Height);
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

        private void CalculateForHorizontalMovement(Matrix<SegmentCalculation> matrix, BitMatrix board, 
            Matrix<CellCalculation> cellMatrix)
        {
            Axis axis = Axis.Horizontal;
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    SegmentCalculation calculation = new SegmentCalculation();
                    calculation.Centre = new Point((short) x, (short) y);
                    calculation.CentreCalculation = cellMatrix[x, y];
                    calculation.Axis = axis;
                    calculation.Points = 
                    calculation.ValidPoints
                    calculation.CellCalculations
                    calculation.AdjacentSegmentCalculationsByDirection
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

        private void SetSegmentMatrixForVerticalMovement(Matrix<SegmentCalculation> matrix, BitMatrix board, 
            Matrix<CellCalculation> cellMatrix)
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
