using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public class UnitEvent
    {
        public int TickTime { get; set; }
        public Unit Unit { get; set; }
        public Bullet Bullet { get; set; }
    }
}
