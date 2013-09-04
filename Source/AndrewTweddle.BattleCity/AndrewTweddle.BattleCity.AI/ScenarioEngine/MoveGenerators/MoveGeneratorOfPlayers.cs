using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.AI.Scenarios;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators
{
    public class MoveGeneratorOfPlayers: MoveGenerator
    {
        public MoveGeneratorOfPlayers()
            : base(ScenarioDecisionMaker.None)
        {
        }

        public override Move[] Generate(Scenario scenario, Move parentMove)
        {
            Move[] moves = new Move[Constants.PLAYERS_PER_GAME];
            for (int p = 0; p < Constants.PLAYERS_PER_GAME; p++)
            {
                Move move = new Move(parentMove)
                {
                    p = p,
                    pBar = 1 - p
                };
                moves[p] = move;
            }
            return moves;
        }
    }
}
