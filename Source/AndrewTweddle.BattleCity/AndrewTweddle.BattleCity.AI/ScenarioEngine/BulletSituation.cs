using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Calculations.Bullets
{
    public class BulletSituation
    {
        #region Constructors

        public BulletSituation(int bulletIndex, int bulletId)
        {
            BulletIndex = bulletIndex;
            BulletId = bulletId;
        }

        #endregion

        #region Common Properties

        public int BulletIndex { get; set; }
        public int BulletId { get; set; }

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
