using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators
{
    public class MoveGeneratorOfTankCombinationsForPlayerPBar: MoveGenerator
    {
        public MoveGeneratorOfTankCombinationsForPlayerPBar(ScenarioDecisionMaker decisionMaker)
            : base(decisionMaker)
        {
        }

        public MoveGeneratorOfTankCombinationsForPlayerPBar(): this(ScenarioDecisionMaker.pBar)
        {
        }

        public override Move[] Generate(Scenario scenario, Move parentMove)
        {
            Move[] moves = new Move[Constants.PLAYERS_PER_GAME];
            for (int t = 0; t < moves.Length; t++)
            {
                Move move = parentMove.CloneAsChild();
                move.j = t;
                move.jBar = 1 - t;
                moves[t] = move;
            }
            return moves;
        }
    }
}
