using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public abstract class MoveGenerator
    {
        protected MoveGenerator()
        {
        }

        public MoveGenerator(ScenarioDecisionMaker decisionMaker)
        {
            DecisionMaker = decisionMaker;
        }

        public ScenarioDecisionMaker DecisionMaker { get; set; }

        public abstract Move[] Generate(Scenario scenario, Move parentMove);
    }
}
