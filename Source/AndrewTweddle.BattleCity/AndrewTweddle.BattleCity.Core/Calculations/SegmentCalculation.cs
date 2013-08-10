using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class SegmentCalculation
    {
        public Point Centre { get; set; }
        public Axis Axis { get; set; }
        public CellCalculation CentreCalculation { get; set; }
        public CellCalculation[] CellCalculations { get; private set; }
        public Point[] Points { get; set; }
        public Point[] ValidPoints { get; set; }
        public bool IsOutOfBounds { get; set; }

        public SegmentCalculation()
        {
            CellCalculations = new CellCalculation[Constants.SEGMENT_SIZE];
        }
    }
}
