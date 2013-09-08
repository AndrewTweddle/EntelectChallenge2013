using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public enum TankGoal
    {
        None,
        AttackEnemyBase,
        AttackEnemyBaseWithDirectionDir1,
        RetreatToDefendBase,
        RetreatToDefendBaseAgainstAttackWithDirectionDir1
    }
}
