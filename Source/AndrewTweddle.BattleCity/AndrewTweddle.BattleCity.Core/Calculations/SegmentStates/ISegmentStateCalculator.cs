using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    /// <summary>
    /// This interface is used to define the contract for segment state calculators.
    /// For performance reasons it won't actually be used (rather use a type alias to the class to use), 
    /// so it's more for documentation.
    /// </summary>
    public interface ISegmentStateCalculator
    {
        SegmentState GetSegmentState(Axis axisOfMovement, Point point);
        SegmentState GetSegmentState(Axis axisOfMovement, int x, int y);
    }
}
