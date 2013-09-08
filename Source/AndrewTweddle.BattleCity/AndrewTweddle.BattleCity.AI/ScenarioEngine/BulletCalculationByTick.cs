using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public class BulletCalculationByTick
    {
        #region Common Properties

        public int Tick { get; set; }
        public int TickOffset { get; set; }
        public bool AreTanksAtRisk { get; set; }
        public bool IsDestroyed { get; set; }
        public Point[] BulletPoints { get; set; }
        public Point BulletPointAtEndOfTick 
        {
            get
            {
                return BulletPoints[BulletPoints.Length - 1];
            }
        }

        #endregion

        #region Properties that only apply if tanks are at risk:

        public Rectangle TankCentrePointsThatDie { get; set; }
        public Point[] ClosestTankCentrePointsThatSurviveAntiClockwise { get; set; }
        public Point[] ClosestTankCentrePointsThatSurviveClockwise { get; set; }
        public MobileState[] ClosestTankStatesThatCanShootBullet { get; set; }
        public MobileState ClosestTankStateThatCanShootBulletHeadOn { get; set; }
        // This will also be in ClosestTankStatesThatCanShootBullet
        public MobileState ClosestTankStateMovingInBehindBulletFacingFiringTank { get; set; }

        #endregion
    }
}
