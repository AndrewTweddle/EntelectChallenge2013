using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents
{
    public abstract class ActiveBulletEvent: BulletEvent
    {
        protected ActiveBulletEvent()
        {
        }

        protected ActiveBulletEvent(Bullet bullet, Point pos, Direction dir): base(bullet)
        {
            Pos = pos;
            Dir = dir;
        }

        public Point Pos { get; set; }
        public Direction Dir { get; set; }

        public override bool IsActive
        {
            get { return true; }
        }

        public override MobileState MobileState
        {
            get 
            {
                return new MobileState(Pos, Dir, IsActive);
            }
        }
    }
}
