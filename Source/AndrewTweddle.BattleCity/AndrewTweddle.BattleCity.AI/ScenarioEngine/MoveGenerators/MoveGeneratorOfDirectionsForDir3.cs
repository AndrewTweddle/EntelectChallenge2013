using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators
{
    public class MoveGeneratorOfDirectionsForDir3: MoveGenerator
    {
        public MoveGeneratorOfDirectionsForDir3(ScenarioDecisionMaker decisionMaker)
            : base(decisionMaker)
        {
        }

        public override Move[] Generate(Scenario scenario, Move parentMove)
        {
            Move[] childMoves = new Move[Constants.RELEVANT_DIRECTION_COUNT];

            foreach (Direction dir in BoardHelper.AllRealDirections)
            {
                Move childMove = parentMove.CloneAsChild();
                childMove.dir3 = dir;
                childMoves[(int)dir] = childMove;
            }
            return childMoves;
        }
    }
}
