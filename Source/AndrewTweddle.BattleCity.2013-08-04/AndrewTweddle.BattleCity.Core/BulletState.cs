using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public class BulletState
    {
        public int ID { get; set; }
        public Tank FiringTank { get; set; }
        public bool IsFired { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Direction Direction { get; set; }
    }
}
