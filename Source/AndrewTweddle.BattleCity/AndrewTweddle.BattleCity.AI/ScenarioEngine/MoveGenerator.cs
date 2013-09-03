using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.Scenarios;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public abstract class MoveGenerator
    {
        public abstract Move[] Generate(Scenario scenario, Move parentMove);
    }
}
