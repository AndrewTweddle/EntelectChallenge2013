using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.Solvers;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Actions;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Calculations;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;

namespace AndrewTweddle.BattleCity.Bots
{
    public class ScaredyBot<TGameState> : BaseSolver<TGameState>
        where TGameState : GameState<TGameState>, new()
    {
        public override string Name
        {
            get
            {
                return "ScaredyBot";
            }
        }

        protected override void ChooseMoves()
        {
            GameState currGameState = Game.Current.CurrentTurn.GameState;
            TankActionSet actionSet = new TankActionSet(YourPlayerIndex, currGameState.Tick);

            for (int tankNumber = 0; tankNumber < Constants.TANKS_PER_PLAYER; tankNumber++)
            {
                Tank tank = You.Tanks[tankNumber];
                MobileState tankState = currGameState.GetMobileState(tank.Index);
                if (!tankState.IsActive)
                {
                    continue;
                }

                int midX = currGameState.Walls.Width / 2;
                int midY = currGameState.Walls.Height / 2;

                int targetX;
                int targetY;
                Direction direction;

                if (tankState.Pos.X > midX)
                {
                    targetX = currGameState.Walls.Width - Constants.SEGMENT_SIZE;
                }
                else
                {
                    targetX = Constants.SEGMENT_SIZE;
                }

                if (tankState.Pos.Y > midY)
                {
                    targetY = currGameState.Walls.Height - Constants.SEGMENT_SIZE;
                    direction = Direction.UP;
                }
                else
                {
                    targetY = Constants.SEGMENT_SIZE;
                    direction = Direction.DOWN;
                }

                DirectionalMatrix<DistanceCalculation> distancesFromTank
                    = currGameState.CalculationCache.GetDistanceMatrixFromTankByTankIndex(tank.Index);
                TankAction[] tankActions = PathCalculator.GetTankActionsOnOutgoingShortestPath(
                    distancesFromTank, direction, targetX, targetY);
                if (tankActions.Length > 0)
                {
                    actionSet.Actions[tank.Number] = tankActions[0];
                }
            }

            Coordinator.SetBestMoveSoFar(actionSet);
        }
    }
}
