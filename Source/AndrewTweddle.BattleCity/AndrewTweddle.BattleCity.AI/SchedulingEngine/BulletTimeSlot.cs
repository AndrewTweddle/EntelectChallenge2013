using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine
{
    public class BulletTimeSlot: ElementTimeSlot<BulletTimeSlot>
    {
        public BulletStatus Status { get; set; }
    }
}
