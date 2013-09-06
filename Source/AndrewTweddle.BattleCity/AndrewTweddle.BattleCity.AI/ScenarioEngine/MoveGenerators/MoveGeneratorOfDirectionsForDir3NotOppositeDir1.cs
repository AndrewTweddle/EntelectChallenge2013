using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators
{
    public class MoveGeneratorOfDirectionsForDir3NotOppositeDir1: MoveGenerator
    {
        public MoveGeneratorOfDirectionsForDir3NotOppositeDir1(ScenarioDecisionMaker decisionMaker)
            : base(decisionMaker)
        {
        }

        public override Move[] Generate(Scenario scenario, Move parentMove)
        {
            Move[] childMoves = new Move[Constants.RELEVANT_DIRECTION_COUNT];
            Direction dir1Opposite = parentMove.dir1.GetOpposite();

            foreach (Direction dir in BoardHelper.AllRealDirections)
            {
                if (dir != dir1Opposite)
                {
                    Move childMove = parentMove.CloneAsChild();
                    childMove.dir3 = dir;
                    childMoves[(int)dir] = childMove;
                }
            }
            return childMoves;
        }
    }
}
