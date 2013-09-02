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
using AndrewTweddle.BattleCity.Core.Calculations.Bullets;

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

            TankActionSet actionSet = new TankActionSet(YourPlayerIndex, currGameState.Tick);
            bool[] moveChosen = new bool[Constants.TANKS_PER_PLAYER];

            RespondToBullets(currGameState, actionSet, moveChosen);




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

        private void RespondToBullets(GameState currGameState, TankActionSet actionSet, bool[] moveChosen)
        {
            BulletCalculation bulletCalc = BulletCalculator.GetBulletCalculation(currGameState, You);
            foreach (BulletPathCalculation bulletPathCalc in bulletCalc.BulletPaths)
            {
                BulletThreat[] bulletThreats = bulletPathCalc.BulletThreats;
                for (int i = 0; i < bulletThreats.Length; i++)
                {
                    BulletThreat bulletThreat = bulletThreats[i];
                    if (bulletThreat.TankThreatened.Player == You && !moveChosen[bulletThreat.TankThreatened.Number])
                    {
                        if ((bulletPathCalc.BaseThreatened == You.Base)
                            && (bulletThreat.NodePathToTakeOnBullet != null)
                            && (bulletThreat.NodePathToTakeOnBullet.Length > 0))
                        {
                            actionSet.Actions[bulletThreat.TankThreatened.Number]
                                = bulletThreat.TankActionsToTakeOnBullet[0];
                            moveChosen[bulletThreat.TankThreatened.Number] = true;
                            continue;
                        }

                        if (bulletThreat.LateralMoveInOneDirection != null
                            && bulletThreat.LateralMoveInOneDirection.Length > 0)
                        {
                            actionSet.Actions[bulletThreat.TankThreatened.Number]
                                = bulletThreat.TankActionsForLateralMoveInOneDirection[0];
                            moveChosen[bulletThreat.TankThreatened.Number] = true;
                            continue;
                        }

                        if (bulletThreat.LateralMoveInOtherDirection != null
                            && bulletThreat.LateralMoveInOtherDirection.Length > 0)
                        {
                            actionSet.Actions[bulletThreat.TankThreatened.Number]
                                = bulletThreat.TankActionsForLateralMoveInOtherDirection[0];
                            moveChosen[bulletThreat.TankThreatened.Number] = true;
                            continue;
                        }

                        if ((bulletThreat.NodePathToTakeOnBullet != null)
                            && (bulletThreat.NodePathToTakeOnBullet.Length > 0))
                        {
                            actionSet.Actions[bulletThreat.TankThreatened.Number]
                                = bulletThreat.TankActionsToTakeOnBullet[0];
                            moveChosen[bulletThreat.TankThreatened.Number] = true;
                            continue;
                        }
                    }
                }
            }
        }
    }
}
