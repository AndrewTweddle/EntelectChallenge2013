using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.Strategies
{
    public class TankProfile
    {
        public Tank Tank { get; set; }
        public PlayerType PlayerType { get; set; }
        public PathPackage AttackPathToEnemyBase { get; set; }
        public PathPackage[] DefencePathsToOwnBaseByOutwardDirection { get; set; }
        public PathPackage[,] DefencePathsToShootEnemyBulletAwayByOutwardDirectionAndParity { get; set; }
    }
}
