using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents
{
    public class BulletNoCollisionEvent: ActiveBulletEvent
    {
        private BulletNoCollisionEvent()
        {
        }

        protected BulletNoCollisionEvent(Bullet bullet, Point pos, Direction dir)
            : base(bullet, pos, dir)
        {
        }

        public override Core.PhaseType[] ApplicablePhaseTypes
        {
            get 
            {
                return new PhaseType[] 
                {
                    PhaseType.Curtains,
                    PhaseType.ResolveBulletCollisions1,
                    PhaseType.ResolveBulletCollisions2,
                    PhaseType.ResolveFiredBulletCollisions,
                    PhaseType.MoveTanksAndResolveCollisions
                };
            }
        }
    }
}
