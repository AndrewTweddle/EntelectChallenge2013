﻿using System;
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
            TankActionSet actionSet = new TankActionSet(YourPlayerIndex, currGameState.Tick);
            bool[] moveChosen = new bool[Constants.TANKS_PER_PLAYER];

            RespondToBullets(currGameState, actionSet, moveChosen);

            Tuple<int, int, TankAction>[] tankNumberDistanceAndActionArray = new Tuple<int, int, TankAction>[Constants.TANKS_PER_PLAYER];

            // The closest bot can attack the base:
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
                    firingLineMatrix, keepMovingCloserOnFiringLastBullet: false);

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
            actionSet.Actions[chosenTank] = tankNumberDistanceAndActionArray[chosenTank].Item3;

            // The other bot can attack the closest enemy tank:
            Tank killerTank = You.Tanks[1 - chosenTank];
            MobileState killerTankState = currGameState.GetMobileState(killerTank.Index);
            if (killerTankState.IsActive)
            {
                Tank enemyTank1 = Opponent.Tanks[0];
                MobileState enemyTankState1 = currGameState.GetMobileState(enemyTank1.Index);

                int killerDistance1 = Constants.UNREACHABLE_DISTANCE;
                TankAction killerAction1 = TankAction.NONE;
                
                if (enemyTankState1.IsActive)
                {
                    DirectionalMatrix<DistanceCalculation> attackMatrix1
                        = currGameState.CalculationCache.GetIncomingAttackMatrixForTankByTankIndex(enemyTank1.Index);
                    if (attackMatrix1 != null)
                    {
                        DistanceCalculation attackCalculation1 = attackMatrix1[killerTankState.Dir, killerTankState.Pos];
                        killerDistance1 = attackCalculation1.Distance;
                        TankAction[] tankActions 
                            = PathCalculator.GetTankActionsOnIncomingShortestPath(attackMatrix1, killerTankState, enemyTankState1.Pos, 
                                currGameState.CalculationCache.FiringLinesForTanksMatrix, keepMovingCloserOnFiringLastBullet: true);
                        if (tankActions.Length > 0)
                        {
                            killerAction1 = tankActions[0];
                        }
                    }
                }
            
                Tank enemyTank2 = Opponent.Tanks[1];
                MobileState enemyTankState2 = currGameState.GetMobileState(enemyTank2.Index);

                int killerDistance2 = Constants.UNREACHABLE_DISTANCE;
                TankAction killerAction2 = TankAction.NONE;

                if (enemyTankState2.IsActive)
                {
                    DirectionalMatrix<DistanceCalculation> attackMatrix2
                        = currGameState.CalculationCache.GetIncomingAttackMatrixForTankByTankIndex(enemyTank2.Index);
                    if (attackMatrix2 != null)
                    {
                        DistanceCalculation attackCalculation2 = attackMatrix2[killerTankState.Dir, killerTankState.Pos];
                        killerDistance2 = attackCalculation2.Distance;
                        TankAction[] tankActions
                            = PathCalculator.GetTankActionsOnIncomingShortestPath(attackMatrix2, killerTankState, enemyTankState2.Pos,
                                currGameState.CalculationCache.FiringLinesForTanksMatrix, keepMovingCloserOnFiringLastBullet: true);
                        if (tankActions.Length > 0)
                        {
                            killerAction2 = tankActions[0];
                        }
                    }
                }

                actionSet.Actions[1 - chosenTank]
                    = (killerDistance1 <= killerDistance2)
                    ? killerAction1
                    : killerAction2;
            }

            Coordinator.SetBestMoveSoFar(actionSet);
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
