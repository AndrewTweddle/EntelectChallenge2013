using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Calculations.Bullets
{
    public class BulletCalculationByTick
    {
        public int Tick { get; set; }
        public int TickOffset { get; set; }

        public Rectangle TankCentrePointsThatDie { get; set; }
        public Rectangle ClosestTankCentrePointsThatSurviveOnOneSide { get; set; }
        public Rectangle ClosestTankCentrePointsThatSurviveOnOtherSide { get; set; }
        public MobileState[] ClosestTankStatesThatCanShootBullet { get; set; }
        public MobileState ClosestTankStateThatCanShootBulletHeadOn { get; set; }
            // This will also be in ClosestTankStatesThatCanShootBullet
    }
}
