using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public class Path
    {
        public Point FinalPosition { get; set; }
        public Direction FinalDirection { get; set; }
        public DistanceCalculation DistanceCalculation { get; set; }
    }
}
