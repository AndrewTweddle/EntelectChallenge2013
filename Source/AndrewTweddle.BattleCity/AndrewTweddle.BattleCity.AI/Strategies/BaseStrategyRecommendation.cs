using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.AI.Strategies
{
    [Flags]
    public enum BaseStrategyRecommendation
    {
        NoRecommendation = 0,
        AttackTheEnemyBaseWithTheUnmarkedTank = 1,
        DefendYourOwnBaseWithTheClosestTank = 2  // TODO: Best unmarked tank later
    }
}
