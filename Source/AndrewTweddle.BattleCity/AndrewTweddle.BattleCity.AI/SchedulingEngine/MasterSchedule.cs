using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine
{
    public class MasterSchedule
    {
        public Schedule[] ElementSchedules { get; private set; }

        public MasterSchedule()
        {
            ElementSchedules = new Schedule[Constants.MOBILE_ELEMENT_COUNT];
        }
    }
}
