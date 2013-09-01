using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Calculations.Bullets
{
    public class BulletPathPoint
    {
        public int TicksToEscape { get; set; }
        public int Tick { get; set; }
        public int MovementPhase { get; set; }
        public Point BulletPoint { get; set; }
        public Rectangle DangerArea { get; set; }
    }
}
