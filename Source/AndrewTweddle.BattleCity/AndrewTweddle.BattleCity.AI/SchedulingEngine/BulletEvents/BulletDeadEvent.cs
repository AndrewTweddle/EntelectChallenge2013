using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents
{
    public class BulletDeadEvent: InactiveBulletEvent
    {
        public override Core.PhaseType[] ApplicablePhaseTypes
        {
            get 
            {
                return new PhaseType[] 
                { 
                    PhaseType.Curtains, 
                    PhaseType.MoveBullets1, 
                    PhaseType.ResolveBulletCollisions1,
                    PhaseType.MoveBullets2,
                    PhaseType.ResolveBulletCollisions2,
                    PhaseType.MoveTanksAndResolveCollisions,
                    PhaseType.FireBullets, 
                    PhaseType.ResolveFiredBulletCollisions
                };
            }
        }
    }
}
