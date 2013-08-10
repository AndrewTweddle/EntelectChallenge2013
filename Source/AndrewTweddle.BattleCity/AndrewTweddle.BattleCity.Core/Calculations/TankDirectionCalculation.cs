using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class TankDirectionCalculation
    {
        public Direction Direction { get; private set; }
        public Rectangle UnchangedAreaDuringMovement { get; set; }
    }
}
