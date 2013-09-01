using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents
{
    public abstract class InactiveBulletEvent: BulletEvent
    {
        protected InactiveBulletEvent()
        {
        }

        public InactiveBulletEvent(Bullet bullet): base(bullet)
        {
        }

        public override bool IsActive
        {
            get { return false; }
        }

        public override MobileState MobileState
        {
            get 
            {
                return new MobileState(new Point(), Direction.NONE, IsActive);
            }
        }
    }
}
