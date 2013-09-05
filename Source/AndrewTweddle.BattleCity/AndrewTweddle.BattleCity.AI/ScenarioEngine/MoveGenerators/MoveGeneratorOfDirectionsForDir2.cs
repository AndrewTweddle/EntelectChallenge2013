using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators
{
    public class MoveGeneratorOfDirectionsForDir2: MoveGenerator
    {
        public MoveGeneratorOfDirectionsForDir2(ScenarioDecisionMaker decisionMaker)
            : base(decisionMaker)
        {
        }

        public override Move[] Generate(Scenario scenario, Move parentMove)
        {
            Move[] childMoves = new Move[Constants.RELEVANT_DIRECTION_COUNT];

            foreach (Direction dir in BoardHelper.AllRealDirections)
            {
                Move childMove = parentMove.CloneAsChild();
                childMove.dir2 = dir;
                childMoves[(int)dir] = childMove;
            }
            return childMoves;
        }
    }
}
