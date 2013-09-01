using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;

namespace AndrewTweddle.BattleCity.AI.Strategies
{
    public class TankHeadToHeadProfile
    {
        public Tank Subject { get; set; }
        public Tank Target { get; set; }

        public PathPackage ClosestPathToInterceptTarget { get; set; }
        public PathPackage AttackPathToStationaryTarget { get; set; }

        public int SlackTimeToGetBackToBaseBeforeAttackFromTarget { get; set; }
        public int[,] SlackTimeToGetBackAndShootTargetsBulletAwayByOutwardDirectionAndParity { get; set; }

        public BaseDefenceStatus SubjectDefenceStatusAgainstTarget
        {
            get
            {
                if (SlackTimeToGetBackToBaseBeforeAttackFromTarget < 0)
                {
                    return BaseDefenceStatus.Undefended;
                }
                if (SlackTimeToGetBackToBaseBeforeAttackFromTarget < 10)
                {
                    return BaseDefenceStatus.Warning;
                }
                return BaseDefenceStatus.Safe;
            }
        }
    }
}
