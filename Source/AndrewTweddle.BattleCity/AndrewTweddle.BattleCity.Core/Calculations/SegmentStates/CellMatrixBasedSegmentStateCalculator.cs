using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Calculations.SegmentStates
{
    public class CellMatrixBasedSegmentStateCalculator: ISegmentStateCalculator
    {
        BitMatrix Board { get; set; }
        Matrix<Cell> CellMatrix { get; set; }

        public CellMatrixBasedSegmentStateCalculator(BitMatrix board, Matrix<Cell> cellMatrix)
        {
            Board = board;
            CellMatrix = cellMatrix;
        }

        public SegmentState GetSegmentState(Axis axisOfMovement, Point point)
        {
            Segment segment = CellMatrix[point].GetSegmentByAxisOfMovement(axisOfMovement);
            if (segment.IsOutOfBounds)
            {
                return SegmentState.OutOfBounds;
            }
            else
            {
                foreach (BitMatrixMask mask in segment.BitMasksOfPoints)
                {
                    if (Board[mask])
                    {
                        if (Board[segment.CentreCell.BitIndexAndMask])
                        {
                            return SegmentState.ShootableWall;
                        }
                        else
                        {
                            return SegmentState.UnshootablePartialWall;
                        }
                    }
                }
                return SegmentState.Clear;
            }
        }

        public SegmentState GetSegmentState(Axis axisOfMovement, int x, int y)
        {
            Segment segment = CellMatrix[x, y].GetSegmentByAxisOfMovement(axisOfMovement);
            if (segment.IsOutOfBounds)
            {
                return SegmentState.OutOfBounds;
            }
            else
            {
                foreach (BitMatrixMask mask in segment.BitMasksOfPoints)
                {
                    if (Board[mask])
                    {
                        if (Board[segment.CentreCell.BitIndexAndMask])
                        {
                            return SegmentState.ShootableWall;
                        }
                        else
                        {
                            return SegmentState.UnshootablePartialWall;
                        }
                    }
                }
                return SegmentState.Clear;
            }
        }
    }
}
