using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public class BulletSituation
    {
        #region Constructors

        public BulletSituation(TankSituation tankSituation, int bulletIndex, int bulletId)
        {
            TankSituation = tankSituation;
            tankSituation.TanksBulletSituation = this;
            BulletIndex = bulletIndex;
            BulletId = bulletId;
        }

        #endregion

        #region Common Properties

        public TankSituation TankSituation { get; private set; }
        public int BulletIndex { get; private set; }
        public int BulletId { get; private set; }

        public int TankIndex 
        {
            get
            {
                return BulletIndex - Constants.MIN_BULLET_INDEX;
            }
        }
        public bool IsActive { get; set; }

        public BulletCalculationByTick[] BulletCalculationsByTick { get; set; }

        #endregion

        #region Properties applicable if the bullet is active

        public int TickFired { get; set; }
        public MobileState TankStateAtTimeOfFiring { get; set; }
        public MobileState BulletStateAtTimeOfFiring { get; set; }
        public int TickOffsetWhenTankCanFireAgain { get; set; }
        public int TickWhenTankCanFireAgain
        {
            get
            {
                return TickFired + TickOffsetWhenTankCanFireAgain;
            }
        }

        public Direction BulletMovementDir 
        {
            get
            {
                if (IsActive)
                {
                    return BulletStateAtTimeOfFiring.Dir;
                }
                else
                {
                    return Direction.NONE;
                }
            }
        }

        public Point BulletFiringPoint 
        {
            get
            {
                return BulletStateAtTimeOfFiring.Pos;
            }
        }

        public Parity TickBasedParity
        {
            get
            {
                if (IsActive)
                {
                    if (TickFired % 2 == 0)
                    {
                        return BulletStateAtTimeOfFiring.Pos.Parity;
                    }
                    else
                    {
                        return (Parity)(1 - BulletStateAtTimeOfFiring.Pos.Parity);
                    }
                }
                else
                {
                    return Parity.Unknown;
                }
            }
        }

        #endregion
    }
}
