using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents
{
    public class BulletFiredEvent: BulletMovementEvent
    {
        protected BulletFiredEvent()
        {
        }

        public BulletFiredEvent(Bullet bullet, Point pos, Direction dir): base(bullet, pos, dir)
        {
        }

        public override Core.PhaseType[] ApplicablePhaseTypes
        {
            get
            {
                return new PhaseType[] { PhaseType.FireBullets };
            }
        }
    }
}
