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
    public class ShortestPathBot<TGameState> : BaseSolver<TGameState>
        where TGameState : GameState<TGameState>, new()
    {
        public override string Name
        {
            get
            {
                return "ShortestPathBot";
            }
        }

        protected override void ChooseMoves()
        {
            GameState currGameState = Game.Current.CurrentTurn.GameState;

            Tuple<int, int, TankAction>[] tankNumberDistanceAndActionArray = new Tuple<int, int, TankAction>[Constants.TANKS_PER_PLAYER];

            for (int tankNumber = 0; tankNumber < Constants.TANKS_PER_PLAYER; tankNumber++)
            {
                Tank tank = You.Tanks[tankNumber];
                MobileState tankState = currGameState.GetMobileState(tank.Index);
                if (!tankState.IsActive)
                {
                    tankNumberDistanceAndActionArray[tankNumber] 
                        = new Tuple<int, int, TankAction>(tankNumber, Constants.UNREACHABLE_DISTANCE, TankAction.NONE);
                    continue;
                }

                Base enemyBase = Opponent.Base;
                DirectionalMatrix<DistanceCalculation> distancesToEnemyBase
                    = currGameState.CalculationCache.GetIncomingDistanceMatrixForBase(Opponent.Index);
                DistanceCalculation distanceCalc = distancesToEnemyBase[tankState.Dir, tankState.Pos];

                FiringLineMatrix firingLineMatrix = currGameState.CalculationCache.FiringLinesForPointsMatrix;
                TankAction[] tankActions = PathCalculator.GetTankActionsOnIncomingShortestPath(distancesToEnemyBase,
                    tankState.Dir, tankState.Pos.X, tankState.Pos.Y, enemyBase.Pos.X, enemyBase.Pos.Y, 
                    keepMovingCloserOnFiringLastBullet: false);

                if (tankActions.Length == 0)
                {
                    tankNumberDistanceAndActionArray[tankNumber]
                        = new Tuple<int, int, TankAction>(tankNumber, Constants.UNREACHABLE_DISTANCE, TankAction.NONE);
                }
                else
                {
                    tankNumberDistanceAndActionArray[tankNumber]
                        = new Tuple<int, int, TankAction>(tankNumber, distanceCalc.Distance, tankActions[0]);
                }
            }

            int chosenTank;
            if (tankNumberDistanceAndActionArray[1].Item2 < tankNumberDistanceAndActionArray[0].Item2)
            {
                chosenTank = 1;
            }
            else
            {
                chosenTank = 0;
            }
            TankActionSet actionSet = new TankActionSet(YourPlayerIndex, currGameState.Tick);
            actionSet.Actions[chosenTank] = tankNumberDistanceAndActionArray[chosenTank].Item3;
            actionSet.Actions[1 - chosenTank] = TankAction.NONE;

            Coordinator.SetBestMoveSoFar(actionSet);
        }
    }
}
