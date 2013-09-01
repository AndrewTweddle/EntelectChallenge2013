using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents
{
    public class BulletMovementEvent: ActiveBulletEvent
    {
        protected BulletMovementEvent()
        {
        }

        public BulletMovementEvent(Bullet bullet, Point pos, Direction dir): base(bullet, pos, dir)
        {
        }

        public override PhaseType[] ApplicablePhaseTypes
        {
            get 
            {
                return new PhaseType[]
                {
                    PhaseType.MoveBullets1,
                    PhaseType.MoveBullets2,
                    PhaseType.FireBullets
                };
            }
        }
    }
}
