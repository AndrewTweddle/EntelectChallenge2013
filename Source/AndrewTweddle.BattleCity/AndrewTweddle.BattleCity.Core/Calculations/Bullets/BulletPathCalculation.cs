using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Calculations.Bullets
{
    public class BulletPathCalculation
    {
        public Bullet Bullet { get; set; }
        public MobileState BulletState { get; set; }
        public BulletPathPoint[] BulletPathPoints { get; set; }
        public Base BaseThreatened { get; set; }
        public int TicksTillBulletDestroyed { get; set; }  
            // This is also the offset to the next tick at which the firer can fire a bullet again

        public BulletThreat[] BulletThreats { get; set; }
    }
}
