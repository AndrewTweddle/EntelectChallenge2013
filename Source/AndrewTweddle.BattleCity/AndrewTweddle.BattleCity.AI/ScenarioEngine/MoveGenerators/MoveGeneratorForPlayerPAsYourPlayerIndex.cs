using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators
{
    public class MoveGeneratorForPlayerPAsYourPlayerIndex: MoveGenerator
    {
        public int YourPlayerIndex { get; private set; }

        public MoveGeneratorForPlayerPAsYourPlayerIndex(int yourPlayerIndex)
        {
            YourPlayerIndex = yourPlayerIndex;
        }

        public override Move[] Generate(Scenario scenario, Move parentMove)
        {
            Move[] moves = new Move[]
            {
                new Move(parentMove)
                {
                    p = YourPlayerIndex,
                    pBar = 1 - YourPlayerIndex
                }
            };
            return moves;
        }
    }
}
