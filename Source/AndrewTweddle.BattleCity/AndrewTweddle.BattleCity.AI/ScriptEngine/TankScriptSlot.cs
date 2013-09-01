using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScriptEngine
{
    public class TankScriptSlot: ScriptSlot
    {
        public TankAction TankActionToTake { get; set; }
        public bool CanFire { get; set; }
        public Instant EstimatedInstantWhenBulletIsAvailableAgain { get; set; }

        public int EstimatedTicksUntilBulletIsAvailableAgain
        {
            get
            {
                return EstimatedInstantWhenBulletIsAvailableAgain.Tick - Instant.Tick;
            }
        }
    }
}
