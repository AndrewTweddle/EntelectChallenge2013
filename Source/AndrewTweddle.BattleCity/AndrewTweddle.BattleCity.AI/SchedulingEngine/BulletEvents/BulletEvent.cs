using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.AI.SchedulingEngine.Events;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents
{
    public abstract class BulletEvent: ElementEvent
    {
        public override Element Element
        {
            get
            {
                return Bullet;
            }
        }
        public Bullet Bullet { get; set; }
        public abstract bool IsActive { get; }
        public abstract PhaseType[] ApplicablePhaseTypes { get; }

        protected BulletEvent()
        {
        }

        protected BulletEvent(Bullet bullet)
        {
            Bullet = bullet;
        }
    }
}
