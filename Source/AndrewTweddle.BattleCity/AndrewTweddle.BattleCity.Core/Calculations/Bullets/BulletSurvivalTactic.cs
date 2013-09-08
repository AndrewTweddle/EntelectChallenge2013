using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Calculations.Bullets
{
    public class BulletSurvivalTactic
    {
        public int TicksToEscape { get; set; }
        public int Slack { get; set; }
        public MobileState TargetState { get; set; }
        public TankAction[] InitialTankActions { get; set; }
    }
}
