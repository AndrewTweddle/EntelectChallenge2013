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
            SegmentState segmentState = SegmentState.Clear;
            Point topLeft = Board.TopLeft;
            Point bottomRight = Board.BottomRight;
            int first;
            int last;
            int x;
            int y;

            switch (axisOfMovement)
            {
                case Axis.Horizontal:
                    x = point.X;
                    if (x < topLeft.X || x > bottomRight.X)
                    {
                        return SegmentState.OutOfBounds;
                    }
                    first = point.Y - 2;
                    last = point.Y + 2;
                    if (first < topLeft.Y || last > bottomRight.Y)
                    {
                        return SegmentState.OutOfBounds;
                    }
                    for (y = first; y <= last; y++)
                    {
                        if (Board[x, y])
                        {
                            segmentState = SegmentState.UnshootablePartialWall;
                            break;
                        }
                    }
                    break;

                case Axis.Vertical:
                    y = point.Y;
                    if (y < topLeft.Y || y > bottomRight.Y)
                    {
                        return SegmentState.OutOfBounds;
                    }
                    first = point.X - 2;
                    last = point.X + 2;
                    if (first < topLeft.X || last > bottomRight.X)
                    {
                        return SegmentState.OutOfBounds;
                    }
                    for (x = first; x <= last; x++)
                    {
                        if (Board[x, y])
                        {
                            segmentState = SegmentState.UnshootablePartialWall;
                            break;
                        }
                    }
                    break;

                default:
                    // case Axis.None:
                    throw new ArgumentException(
                        "Segment state can't be determined due to axis of movement being null",
                        "axisOfMovement"
                        );
            }
            if (segmentState == SegmentState.UnshootablePartialWall && Board[point])
            {
                return SegmentState.ShootableWall;
            }
            return segmentState;
        }

        /// <summary>
        /// Very slow compared to caching.
        /// </summary>
        /// <param name="axisOfMovement"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public SegmentState GetSegmentStateUsingOffsets(Axis axisOfMovement, Point point)
        {
            Point[] offsets;
            SegmentState segmentState = SegmentState.Clear;
            switch (axisOfMovement)
            {
                case Axis.Horizontal:
                    offsets = new Point[] { new Point(0, -2), new Point(0, -1), new Point(0, 0), new Point(0, 1), new Point(0, 2) };
                    break;

                case Axis.Vertical:
                    offsets = new Point[] { new Point(-2, 0), new Point(-1, 0), new Point(0, 0), new Point(1, 0), new Point(2, 0) };
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
