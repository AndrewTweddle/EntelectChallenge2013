using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.Strategies;

namespace AndrewTweddle.BattleCity.AI.Intelligence
{
    public class IntelligencePortfolio
    {
        public int Tick { get; set; }
        public TankProfile[] TankProfilesByIndex { get; set; }
        public TankHeadToHeadProfile[,] FriendlyVersusEnemyHeadToHeadProfiles { get; set; }
        public TankHeadToHeadProfile[,] EnemyVersusFriendlyHeadToHeadProfiles { get; set; }
    }
}
