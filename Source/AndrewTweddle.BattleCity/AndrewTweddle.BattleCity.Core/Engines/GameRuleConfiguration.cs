using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core.Engines
{
    public class GameRuleConfiguration
    {
        #region Private member variables

        private TankActionSequenceRule tankMovementSequence;
        private TankActionSequenceRule tankFiringSequence;

        #endregion

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
            DoesATankDieIfTryingToMoveOffTheBoard = false;  // If false, then it is prevented from moving
            DoesATankDieIfMovingIntoABullet = true;         // If false, then it is prevented from moving
            DoesATankDestroyABaseIfAlsoMovingIntoABullet = true;  // If false, then it dies and the base it would have moved into is still safe
            DoesItsBulletContinueMovingWhenATankDies = true;
            DoesTheGameEndInADrawWhenTheLastBulletIsGone = false;  
                // If false, then it ends in a draw as soon as the last tank is destroyed.
                // Otherwise it will only end when the last bullet is gone.

            // At this stage, the rule is that player 1's tanks will always move before player 2's:
            TankMovementSequence = TankActionSequenceRule.InPlayerThenIdOrder;
            TankFiringSequence = TankActionSequenceRule.InPlayerThenIdOrder;
        }

        #endregion

        public TankMovementLookup[] TankMovementOrders { get; private set; }
        public TankMovementLookup[] TankFiringOrders { get; private set; }
        
        public int[] TankMovementIndexes { get; private set; }
        public int[] TankFiringIndexes { get; private set; }

        public bool CanATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet { get; set; }
        public bool CanATankFireAgainOnTheSameTurnThatItsBulletWasDestroyed { get; set; }
        public bool DoesATankDieIfTryingToMoveOffTheBoard { get; set; }
        public bool DoesATankDieIfMovingIntoABullet { get; set; }  // If not, then assume that it isn't allowed to move
        public bool DoesATankDestroyABaseIfAlsoMovingIntoABullet { get; set; }
        public bool DoesTheGameEndInADrawWhenTheLastBulletIsGone { get; set; }
        public bool DoesItsBulletContinueMovingWhenATankDies { get; set; }
            // TODO: the movement engines don't honor this setting yet (if false)

        public TankActionSequenceRule TankMovementSequence 
        {
            get
            {
                return tankMovementSequence;
            }
            set
            {
                TankMovementOrders = GetTankMovementLookupsBySequenceRule(value);
                TankMovementIndexes = GetTankSequenceIndexes(TankMovementOrders);
                tankMovementSequence = value;
            }
        }

        private int[] GetTankSequenceIndexes(TankMovementLookup[] TankMovementOrders)
        {
            int[] tankSequenceIndexes = new int[Constants.TANK_COUNT];
            for (int t = 0; t < tankSequenceIndexes.Length; t++)
            {
                TankMovementLookup lookup = TankMovementOrders[t];
                tankSequenceIndexes[t] 
                    = Game.Current.Players[lookup.PlayerNumber].Tanks[lookup.TankNumber].Index;
            }
            return tankSequenceIndexes;
        }

        public TankActionSequenceRule TankFiringSequence 
        {
            get
            {
                return tankFiringSequence;
            }
            set
            {
                TankFiringOrders = GetTankMovementLookupsBySequenceRule(value);
                TankFiringIndexes = GetTankSequenceIndexes(TankFiringOrders);
                tankFiringSequence = value;
            }
        }

        // The following edge case is so rare, it's not worth handling:
        // public bool AllowUnresolvedMovesToTakePlaceIfTheOnlyDependenciesAreOnTrailingEdges { get; set; }

        private static TankMovementLookup[] GetTankMovementLookupsBySequenceRule(TankActionSequenceRule sequenceRule)
        {
            TankMovementLookup[] tankMovementLookups;
            switch (sequenceRule)
            {
                case TankActionSequenceRule.InPlayerThenIdOrder:
                    tankMovementLookups = new TankMovementLookup[] 
                    {
                        new TankMovementLookup { PlayerNumber = 0, TankNumber = 0 },
                        new TankMovementLookup { PlayerNumber = 0, TankNumber = 1 },
                        new TankMovementLookup { PlayerNumber = 1, TankNumber = 0 },
                        new TankMovementLookup { PlayerNumber = 1, TankNumber = 1 }
                    };
                    break;

                case TankActionSequenceRule.Player1FirstAndLastByIdOrder:
                    tankMovementLookups = new TankMovementLookup[] 
                    {
                        new TankMovementLookup { PlayerNumber = 0, TankNumber = 0 },
                        new TankMovementLookup { PlayerNumber = 1, TankNumber = 0 },
                        new TankMovementLookup { PlayerNumber = 1, TankNumber = 1 },
                        new TankMovementLookup { PlayerNumber = 0, TankNumber = 1 }
                    };
                    break;

                default:
                    tankMovementLookups = new TankMovementLookup[] { };
                    break;
            }
            return tankMovementLookups;
        }
    }
}
