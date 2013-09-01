using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents
{
    public class BulletCollisionEvent: InactiveBulletEvent
    {
        public override PhaseType[] ApplicablePhaseTypes
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
