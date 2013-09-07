using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public class CombinedMovementAndFiringDistanceCalculation
    {
        public int TicksTillTargetShot { get; private set; }
        public int TicksTillLastShotFired { get; private set; }
        public Direction FinalMovementDirectionTowardsTarget { get; private set; }
        public DistanceCalculation MovementDistanceToFiringLine { get; private set; }
        public FiringDistance FiringDistance { get; private set; }

        public CombinedMovementAndFiringDistanceCalculation(
            DistanceCalculation movementDistanceToFiringLine,
            FiringDistance firingDistance, Direction finalMovementDirectionTowardsTarget)
        {
            MovementDistanceToFiringLine = movementDistanceToFiringLine;
            FiringDistance = firingDistance;
            FinalMovementDirectionTowardsTarget = finalMovementDirectionTowardsTarget;

            // Calculate ticks until the target is shot:
            int ticksTillTargetShot = MovementDistanceToFiringLine.Distance + FiringDistance.TicksTillTargetShot;
            if (ticksTillTargetShot > Constants.UNREACHABLE_DISTANCE)
            {
                ticksTillTargetShot = Constants.UNREACHABLE_DISTANCE;
            }
            TicksTillTargetShot = ticksTillTargetShot;

            // Calculate ticks until the last shot is fired:
            int ticksTillLastShotFired = MovementDistanceToFiringLine.Distance + FiringDistance.TicksTillLastShotFired;
            if (ticksTillLastShotFired > Constants.UNREACHABLE_DISTANCE)
            {
                ticksTillLastShotFired = Constants.UNREACHABLE_DISTANCE;
            }
            TicksTillLastShotFired = ticksTillLastShotFired;
        }
    }
}
