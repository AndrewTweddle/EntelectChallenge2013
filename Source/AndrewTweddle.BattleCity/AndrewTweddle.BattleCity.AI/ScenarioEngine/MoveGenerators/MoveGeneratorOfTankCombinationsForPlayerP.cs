using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.AI.Scenarios;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators
{
    public class MoveGeneratorOfTankCombinationsForPlayerP: MoveGenerator
    {
        public override Move[] Generate(Scenario scenario, Move parentMove)
        {
            Move[] moves = new Move[Constants.TANKS_PER_PLAYER];
            for (int t = 0; t < moves.Length; t++)
            {
                Move move = parentMove.CloneAsChild();
                move.i = t;
                move.iBar = 1 - t;
                moves[t] = move;
            }
            return moves;
        }
    }
}
