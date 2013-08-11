using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class Segment
    {
        public Point Centre { get; set; }
        public Axis Axis { get; set; }
        public Cell CentreCell { get; set; }
        public Cell[] Cells { get; private set; }
        public Point[] Points { get; set; }
        public Point[] ValidPoints { get; set; }
        public bool IsOutOfBounds { get; set; }
        public BitMatrixIndex[] BitMasksOfPoints { get; set; }

        public Segment()
        {
            Cells = new Cell[Constants.SEGMENT_SIZE];
        }
    }
}
