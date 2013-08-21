using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Calculations.SegmentStates
{
    public class CacheBasedSegmentStateCalculator: ISegmentStateCalculator
    {
        private Matrix<SegmentState> HorizontalSegmentStateMatrix { get; set; }
        private Matrix<SegmentState> VerticalSegmentStateMatrix { get; set; }

        public CacheBasedSegmentStateCalculator(
            Matrix<SegmentState> horizontalSegmentStateMatrix,
            Matrix<SegmentState> verticalSegmentStateMatrix)
        {
            HorizontalSegmentStateMatrix = horizontalSegmentStateMatrix;
            VerticalSegmentStateMatrix = verticalSegmentStateMatrix;
        }

        public SegmentState GetSegmentState(Axis axisOfMovement, int x, int y)
        {
            switch (axisOfMovement)
            {
                case Axis.Horizontal:
                    return HorizontalSegmentStateMatrix[x, y];
                case Axis.Vertical:
                    return VerticalSegmentStateMatrix[x, y];
                default:
                    throw new ArgumentException(
                        "Segment state can't be determined due to axis of movement being null", 
                        "axisOfMovement"
                        );
            }
        }

        public SegmentState GetSegmentState(Axis axisOfMovement, Point randomPoint)
        {
            return GetSegmentState(axisOfMovement, randomPoint.X, randomPoint.Y);
        }
    }
}
