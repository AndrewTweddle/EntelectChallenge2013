using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents
{
    public class BulletCollisionWithTankEvent: BulletCollisionEvent
    {
        public Tank Tank { get; set; }
    }
}
