using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine
{
    public class ElementTimeSlot<TDerivedTimeSlot>: TimeSlot
        where TDerivedTimeSlot: ElementTimeSlot<TDerivedTimeSlot>
    {
        public MobileState ElementState { get; set; }
    }
}
