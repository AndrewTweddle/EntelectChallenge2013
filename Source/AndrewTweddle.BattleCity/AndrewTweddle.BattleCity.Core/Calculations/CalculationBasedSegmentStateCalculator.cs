using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class CalculationBasedSegmentStateCalculator: ISegmentStateCalculator
    {
        private BitMatrix Board { get; set; }

        public CalculationBasedSegmentStateCalculator(BitMatrix board)
        {
            Board = board;
        }

        public SegmentState GetSegmentState(Axis axisOfMovement, Point point)
        {
            Point[] offsets;
            SegmentState segmentState = SegmentState.Clear;
            switch (axisOfMovement)
            {
                case Axis.Horizontal:
                    offsets = new Point[] { new Point(0, -2), new Point(0, -1), new Point(0, 0), new Point(0, 1), new Point(0, 2) };
                    break;

                case Axis.Vertical:
                    offsets = new Point[] { new Point(-2, 0), new Point(-1,0), new Point(0, 0), new Point(1, 0), new Point(2, 0) };
                    break;

                default:
                    // case Axis.None:
                    throw new ArgumentException(
                        "Segment state can't be determined due to axis of movement being null",
                        "axisOfMovement"
                        );
                    offsets = new Point[0];  // To stop compiler complaining
            }
            Point[] segmentPoints = point.GetRelativePoints(offsets);
            foreach (Point relativePoint in segmentPoints)
            {
                if (!Board.IsOnBoard(relativePoint))
                {
                    return SegmentState.OutOfBounds;
                }
                else
                {
                    if (Board[relativePoint])
                    {
                        segmentState = SegmentState.UnshootablePartialWall;
                    }
                }
            }
            if (segmentState == SegmentState.UnshootablePartialWall)
            {
                if (Board[point])
                {
                    return SegmentState.ShootableWall;
                }
            }
            return segmentState;
        }

        public SegmentState GetSegmentState(Axis axisOfMovement, int x, int y)
        {
            Point point = new Point((short) x, (short) y);
            return GetSegmentState(axisOfMovement, point);
        }
    }
}
