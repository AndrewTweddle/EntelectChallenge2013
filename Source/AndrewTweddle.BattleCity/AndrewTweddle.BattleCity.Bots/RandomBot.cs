using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.Solvers;
using AndrewTweddle.BattleCity.Core.Actions;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.Bots
{
    public class RandomBot<TGameState>: BaseSolver<TGameState>
        where TGameState: GameState<TGameState>, new()
    {
        protected override void DoSolve()
        {
            GameState currGameState = Coordinator.CurrentGameState;
            TankActionSet actionSet = new TankActionSet
            {
                Tick = currGameState.Tick
            };

            Random rnd = new Random();
            for (int t = 0; t < Constants.TANKS_PER_PLAYER; t++)
            {
                int tankAction = rnd.Next(6);
                actionSet.Actions[0] = (TankAction) tankAction;
            }

            Coordinator.SetBestMoveSoFar(actionSet);
        }
    }
}
