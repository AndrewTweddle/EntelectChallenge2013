using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.Intelligence;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.AI.Strategies
{
    public class BaseAttackAndDefenceStrategy
    {
        #region "Constants"

        private Parity[] parities = new Parity[] { Parity.Even, Parity.Odd };

        #endregion

        
        #region Inputs

        public int HardDefenceSlackThreshold { get; set; }
        public IntelligencePortfolio Intelligence { get; set; }
        public GameState GameState { get; set; }

        #endregion

        
        #region Outputs

        public BaseDefenceStatus EnemyBaseDefenceStatus { get; set; }
        public BaseDefenceStatus FriendlyBaseDefenceStatus { get; set; }
        public int EnemyHardDefenceSlack { get; set; }
        public int FriendlyHardDefenceSlack { get; set; }
        public Tank BestPlacedFriendlyTankToAttackEnemyBase { get; set; }
        public Tank BestPlacedEnemyTankToAttackYourBase { get; set; }
        public Tank BestPlacedFriendlyTankToDefendYourBase { get; set; }
        public Tank BestPlacedEnemyTankToDefendHisBase { get; set; }

        #endregion

        #region Constructors

        public BaseAttackAndDefenceStrategy()
        {
            HardDefenceSlackThreshold = 0;  // 
        }

        #endregion


        #region Methods
        
        public void Evaluate(Player You, Player Opponent, GameState gameState)
        {
            BestPlacedFriendlyTankToAttackEnemyBase = null;
            BestPlacedEnemyTankToAttackYourBase = null;

            EnemyHardDefenceSlack = - Constants.UNREACHABLE_DISTANCE;
            FriendlyHardDefenceSlack = - Constants.UNREACHABLE_DISTANCE;
            
            // A soft defence is shooting bullets away from the side, at a distance, rather than straight on. Ignore it for now...
            // int enemySoftDefenceSlack = - Constants.UNREACHABLE_DISTANCE;
            // int friendlySoftDefenceSlack = Constants.UNREACHABLE_DISTANCE;

            for (int f = 0; f < Constants.TANKS_PER_PLAYER; f++)
            {
                Tank friendlyTank = You.Tanks[f];
                MobileState friendlyTankState = gameState.GetMobileState(friendlyTank.Index);
                if (!friendlyTankState.IsActive)
                {
                    continue;
                }
                // TODO: We should also see if the enemy tank is locked down and ignore it if it is

                // TankProfile friendlyProfile = Intelligence.TankProfilesByIndex[friendlyTank.Index];

                for (int e = 0; e < Constants.TANKS_PER_PLAYER; e++)
                {
                    Tank enemyTank = Opponent.Tanks[e];
                    MobileState enemyTankState = gameState.GetMobileState(enemyTank.Index);
                    if (!enemyTankState.IsActive)
                    {
                        continue;
                    }
                    // TODO: We should also see if the enemy tank is locked down and ignore it if it is

                    TankHeadToHeadProfile friendlyVsEnemy = Intelligence.FriendlyVersusEnemyHeadToHeadProfiles[f, e];
                    TankHeadToHeadProfile enemyVsFriendly = Intelligence.EnemyVersusFriendlyHeadToHeadProfiles[e, f];

                    if (enemyVsFriendly.SlackTimeToGetBackToBaseBeforeAttackFromTarget > EnemyHardDefenceSlack)
                    {
                        EnemyHardDefenceSlack = enemyVsFriendly.SlackTimeToGetBackToBaseBeforeAttackFromTarget;
                        BestPlacedEnemyTankToAttackYourBase = enemyTank;
                    }

                    if (friendlyVsEnemy.SlackTimeToGetBackToBaseBeforeAttackFromTarget > FriendlyHardDefenceSlack)
                    {
                        FriendlyHardDefenceSlack = friendlyVsEnemy.SlackTimeToGetBackToBaseBeforeAttackFromTarget;
                        BestPlacedFriendlyTankToAttackEnemyBase = friendlyTank;
                    }
                }
            }

            EnemyBaseDefenceStatus
                = EnemyHardDefenceSlack < 0
                ? BaseDefenceStatus.Undefended
                : ( EnemyHardDefenceSlack < 10
                  ? BaseDefenceStatus.Warning
                  : BaseDefenceStatus.Safe );
            FriendlyBaseDefenceStatus
                = FriendlyHardDefenceSlack < 0
                ? BaseDefenceStatus.Undefended
                : ( FriendlyHardDefenceSlack < 10
                  ? BaseDefenceStatus.Warning
                  : BaseDefenceStatus.Safe );

            if (EnemyBaseDefenceStatus == BaseDefenceStatus.Undefended)
            {

            }
        }

        #endregion
    }
}
