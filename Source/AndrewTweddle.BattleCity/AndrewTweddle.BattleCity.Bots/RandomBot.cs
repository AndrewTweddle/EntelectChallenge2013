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
    public class RandomBot<TGameState>: BaseSolver<TGameState>
        where TGameState: GameState<TGameState>, new()
    {
        private Random rnd;

        protected override void Initialize()
        {
            base.Initialize();
            rnd = new Random();
        }

        protected override void ChooseMoves()
        {
            GameState currGameState = Game.Current.CurrentTurn.GameState;
            TankActionSet actionSet = new TankActionSet(YourPlayerIndex, currGameState.Tick);

            for (int t = 0; t < Constants.TANKS_PER_PLAYER; t++)
            {
                int tankAction = rnd.Next(6);
                actionSet.Actions[t] = (TankAction) tankAction;
            }

            Coordinator.SetBestMoveSoFar(actionSet);
        }
    }
}
