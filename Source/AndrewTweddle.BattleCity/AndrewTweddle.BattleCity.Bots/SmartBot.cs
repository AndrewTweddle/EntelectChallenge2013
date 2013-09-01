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
    public class SmartBot<TGameState> : BaseSolver<TGameState>
        where TGameState : GameState<TGameState>, new()
    {
        public override string Name
        {
            get
            {
                return "SmartBot";
            }
        }

        protected override void ChooseMoves()
        {
            GameState currGameState = Game.Current.CurrentTurn.GameState;

            int[] attackDistancesToBasesByTank = new int[Constants.TANK_COUNT];
            Node[][] attackPathsToBasesByTank = new Node[Constants.TANK_COUNT][];

            for (int p = 0; p < Constants.PLAYERS_PER_GAME; p++)
            {
                Base @base = Game.Current.Players[p].Base;


                for (int tNum = 0; tNum < Constants.TANKS_PER_PLAYER; tNum++)
                {
                    MobileState tankState = currGameState.GetMobileState(tNum);
                    if (!tankState.IsActive)
                    {
                        attackDistancesToBasesByTank[tNum] = Constants.UNREACHABLE_DISTANCE;
                        continue;
                    }

                    Base enemyBase = Opponent.Base;
                    DirectionalMatrix<DistanceCalculation> distancesToEnemyBase
                        = currGameState.CalculationCache.GetIncomingDistanceMatrixForBase(Opponent.Index);
                    DistanceCalculation distanceCalc = distancesToEnemyBase[tankState.Dir, tankState.Pos];

                    FiringLineMatrix firingLineMatrix = currGameState.CalculationCache.FiringLinesForPointsMatrix;
                    TankAction[] tankActions = PathCalculator.GetTankActionsOnIncomingShortestPath(distancesToEnemyBase,
                        tankState.Dir, tankState.Pos.X, tankState.Pos.Y, enemyBase.Pos.X, enemyBase.Pos.Y,
                        firingLineMatrix, keepMovingCloserOnFiringLastBullet: false);

                    /*
                    if (tankActions.Length == 0)
                    {
                        tankNumberDistanceAndActionArray[tNum]
                            = new Tuple<int, int, TankAction>(tNum, Constants.UNREACHABLE_DISTANCE, TankAction.NONE);
                    }
                    else
                    {
                        tankNumberDistanceAndActionArray[tNum]
                            = new Tuple<int, int, TankAction>(tNum, distanceCalc.Distance, tankActions[0]);
                    }
                     */
                }
            }
        }
    }
}
