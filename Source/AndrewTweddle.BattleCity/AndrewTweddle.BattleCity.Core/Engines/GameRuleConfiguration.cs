using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Engines
{
    public class GameRuleConfiguration
    {
        #region Singleton pattern

        public static GameRuleConfiguration RuleConfiguration { get; private set; }

        static GameRuleConfiguration()
        {
            RuleConfiguration = new GameRuleConfiguration();
        }

        protected GameRuleConfiguration()
        {
            // TODO: Are these reasonable defaults???
            CanATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet = true;
            CanATankFireAgainOnTheSameTurnThatItsBulletWasDestroyed = false;

            // At this stage, the rule is that player 1's tanks will alway move before player 2's:
            TankMovementOrders = new TankMovementLookup[] 
            {
                new TankMovementLookup { PlayerNumber = 0, TankNumber = 0 },
                new TankMovementLookup { PlayerNumber = 0, TankNumber = 1 },
                new TankMovementLookup { PlayerNumber = 1, TankNumber = 0 },
                new TankMovementLookup { PlayerNumber = 1, TankNumber = 1 }
            };
        }

        #endregion

        public TankMovementLookup[] TankMovementOrders { get; private set; }
        public bool CanATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet { get; set; }
        public bool CanATankFireAgainOnTheSameTurnThatItsBulletWasDestroyed { get; set; }

        // The following edge case is so rare, it's not worth handling:
        // public bool AllowUnresolvedMovesToTakePlaceIfTheOnlyDependenciesAreOnTrailingEdges { get; set; }
    }
}
