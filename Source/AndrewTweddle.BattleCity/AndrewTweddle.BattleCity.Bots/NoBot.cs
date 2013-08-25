using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.Solvers;
using AndrewTweddle.BattleCity.Core.Actions;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Bots
{
    public class NoBot<TGameState>: BaseSolver<TGameState>
        where TGameState: GameState<TGameState>, new()
    {
        public override string Name
        {
            get
            {
                return "NoBot";
            }
        }

        protected override void ChooseMoves()
        {
        }
    }
}
