using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class SegmentCalculation
    {
        public Point Centre { get; set; }
        public CellCalculation CentreCalculation { get; set; }
        public Axis Axis { get; set; }
        public Point[] Points { get; set; }
        public Point[] ValidPoints { get; set; }
        public CellCalculation[] PointCalculations { get; set; }
        public SegmentCalculation[] AdjacentSegmentCalculationsByDirection { get; set; }
    }
}
