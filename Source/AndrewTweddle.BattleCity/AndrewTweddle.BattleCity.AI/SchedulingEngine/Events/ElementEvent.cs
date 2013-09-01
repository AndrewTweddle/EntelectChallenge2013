using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine.Events
{
    public abstract class ElementEvent
    {
        public abstract MobileState MobileState { get; }
        public abstract Element Element { get; }
    }
}
