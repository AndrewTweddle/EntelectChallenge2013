using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core.Calculations.Bullets
{
    public class BulletCalculation
    {
        public int CurrentTick { get; set; }
        public BulletPathCalculation[] BulletPaths { get; set; }
    }
}
