using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Calculations.SegmentStates
{
    public class OnTheFlyBoolMatrixBasedSegmentStateCalculator: ISegmentStateCalculator
    {
        Matrix<bool> BoardMatrix { get; set; }

        public OnTheFlyBoolMatrixBasedSegmentStateCalculator(Matrix<bool> boardMatrix)
        {
            BoardMatrix = boardMatrix;
        }

        public SegmentState GetSegmentState(Axis axisOfMovement, Point point)
        {
            return GetSegmentState(axisOfMovement, point.X, point.Y);
        }

        public SegmentState GetSegmentState(Axis axisOfMovement, int x, int y)
        {
            SegmentState segmentState = SegmentState.Clear;
            Point topLeft = BoardMatrix.TopLeft;
            Point bottomRight = BoardMatrix.BottomRight;
            int first;
            int last;
            int segX;
            int segY;

            switch (axisOfMovement)
            {
                case Axis.Horizontal:
                    segX = x;
                    if (segX < topLeft.X || segX > bottomRight.X)
                    {
                        return SegmentState.OutOfBounds;
                    }
                    first = y - 2;
                    last = y + 2;
                    if (first < topLeft.Y || last > bottomRight.Y)
                    {
                        return SegmentState.OutOfBounds;
                    }
                    for (segY = first; segY <= last; segY++)
                    {
                        if (BoardMatrix[segX, segY])
                        {
                            segmentState = SegmentState.UnshootablePartialWall;
                            break;
                        }
                    }
                    break;

                case Axis.Vertical:
                    segY = y;
                    if (segY < topLeft.Y || segY > bottomRight.Y)
                    {
                        return SegmentState.OutOfBounds;
                    }
                    first = x - 2;
                    last = x + 2;
                    if (first < topLeft.X || last > bottomRight.X)
                    {
                        return SegmentState.OutOfBounds;
                    }
                    for (segX = first; segX <= last; segX++)
                    {
                        if (BoardMatrix[segX, segY])
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
            if (segmentState == SegmentState.UnshootablePartialWall && BoardMatrix[x, y])
            {
                return SegmentState.ShootableWall;
            }
            return segmentState;
        }
    }
}
