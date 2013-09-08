using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public class BulletSurvivalTactic
    {
        public int TicksToEscape { get; set; }
        public int Slack { get; set; }
        public MobileState TargetState { get; set; }
        public TankAction InitialTankAction { get; set; }
    }
}
