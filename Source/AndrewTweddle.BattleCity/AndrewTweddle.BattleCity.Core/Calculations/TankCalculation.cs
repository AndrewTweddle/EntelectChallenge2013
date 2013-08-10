using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class TankCalculation
    {
        public Point Centre { get; private set; }
        public short CentrePointIndex { get; private set; }
        public bool IsValidPosition { get; private set; }
        public Rectangle Area { get; private set; }
        public Point[] Boundary { get; private set; }

    }
}
