using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents
{
    public class BulletCollisionWithWallEvent: BulletCollisionEvent
    {
        public Segment Segment { get; set; }
        public Axis AxisOfMovement { get; set; }
    }
}
