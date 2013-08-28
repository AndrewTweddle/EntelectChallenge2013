using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations.Firing
{
    public struct FiringActionSet
    {
        public byte IndexOfTankFiringPoint { get; private set; }
        public byte TicksToShootNextWall { get; private set; }
        public byte NumberOfMovesMade { get; private set; }
        public bool CanMoveOnceBeforeFiring { get; private set; }

        public FiringActionSet(byte indexOfTankFiringPoint,
            byte ticksToShootNextWall, byte numberOfMovesMade,
            bool canMoveOnceBeforeFiring)
            : this()
        {
            IndexOfTankFiringPoint = indexOfTankFiringPoint;
            TicksToShootNextWall = ticksToShootNextWall;
            NumberOfMovesMade = numberOfMovesMade;
            CanMoveOnceBeforeFiring = canMoveOnceBeforeFiring;
        }
    }
}
