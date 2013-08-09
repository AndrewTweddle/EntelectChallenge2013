using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.TankMovement.Core
{
    public class GameRuleConfiguration
    {
        public bool CanATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet { get; set; }

        // The following edge case is so rare, it's not worth handling:
        // public bool AllowUnresolvedMovesToTakePlaceIfTheOnlyDependenciesAreOnTrailingEdges { get; set; }

        public GameRuleConfiguration()
        {
            // TODO: Are these reasonable defaults???
            CanATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet = true;
        }
    }
}
