using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;

namespace AndrewTweddle.BattleCity.AI.Scenarios
{
    public abstract class Scenario
    {
        public const int MAX_POINTS_TO_TRY_FOR_DEFENCE_POS = 6;

        public static TankAction[] NoTankActions = new TankAction[0];

        public GameState GameState { get; set; }

        public Scenario(GameState gameState)
        {
            GameState = gameState;
        }

        public abstract MoveGenerator[] GetMoveGeneratorsByMoveTreeLevel();

        public MobileState GetTankState_i(Move move)
        {
            return GetTankState(move.p, move.i);
        }

        public MobileState GetTankState_iBar(Move move)
        {
            return GetTankState(move.p, move.iBar);
        }

        public MobileState GetTankState_j(Move move)
        {
            return GetTankState(move.pBar, move.j);
        }

        public MobileState GetTankState_jBar(Move move)
        {
            return GetTankState(move.pBar, move.jBar);
        }

        public MobileState GetTankState(int playerIndex, int tankNumber)
        {
            Tank tank = Game.Current.Players[playerIndex].Tanks[tankNumber];
            return GameState.GetMobileState(tank.Index);
        }

        public Base GetFriendlyBase(int playerIndex)
        {
            return Game.Current.Players[playerIndex].Base;
        }

        public Base GetEnemyBase(int playerIndex)
        {
            return Game.Current.Players[1 - playerIndex].Base;
        }

        public bool AreAllTanksAlive()
        {
            return AreAllTanksForPlayerStillAlive(0) && AreAllTanksForPlayerStillAlive(1);
        }

        public bool AreAllTanksForPlayerStillAlive(int playerIndex)
        {
            return GetTankState(playerIndex, 0).IsActive && GetTankState(playerIndex, 1).IsActive;
        }

        public int GetAttackDistanceOfTankToEnemyBase(int playerIndex, int tankNumber)
        {
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)
            {
                DirectionalMatrix<DistanceCalculation> attackDistanceMatrix
                    = GameState.CalculationCache.GetIncomingDistanceMatrixForBase(1 - playerIndex);
                return attackDistanceMatrix[tankState].Distance;
            }
            return Constants.UNREACHABLE_DISTANCE;
        }

        public TankAction[] GetActionsToAttackEnemyBase(int playerIndex, int tankNumber)
        {
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)
            {
                Base enemyBase = GetEnemyBase(playerIndex);
                FiringLineMatrix firingLineMatrix = GameState.CalculationCache.FiringLinesForPointsMatrix;
                DirectionalMatrix<DistanceCalculation> attackDistanceMatrix
                    = GameState.CalculationCache.GetIncomingDistanceMatrixForBase(1 - playerIndex);
                return PathCalculator.GetTankActionsOnIncomingShortestPath(attackDistanceMatrix, tankState,
                    enemyBase.Pos, firingLineMatrix, keepMovingCloserOnFiringLastBullet: true);

            }
            return NoTankActions;
        }

        public int GetAttackDistanceOfTankToEnemyBaseFromDirection(int playerIndex, int tankNumber, 
            Direction finalDirectionOfMovement)
        {
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)
            {
                DirectionalMatrix<DistanceCalculation> attackDistanceMatrix
                    = GameState.CalculationCache.GetIncomingDistanceMatrixForBaseWithFinalDirectionOfMovement(
                        1 - playerIndex, finalDirectionOfMovement);
                return attackDistanceMatrix[tankState].Distance;
            }
            return Constants.UNREACHABLE_DISTANCE;
        }

        public TankAction[] GetActionsToAttackEnemyBaseFromDirection(
            int playerIndex, int tankNumber, Direction finalDirectionOfMovement)
        {
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)
            {
                Base enemyBase = GetEnemyBase(playerIndex);
                FiringLineMatrix firingLineMatrix = GameState.CalculationCache.FiringLinesForPointsMatrix;
                DirectionalMatrix<DistanceCalculation> attackDistanceMatrix
                    = GameState.CalculationCache.GetIncomingDistanceMatrixForBaseWithFinalDirectionOfMovement(
                        1 - playerIndex, finalDirectionOfMovement);
                return PathCalculator.GetTankActionsOnIncomingShortestPath(attackDistanceMatrix,
                    tankState, enemyBase.Pos, firingLineMatrix, keepMovingCloserOnFiringLastBullet: true);
            }
            return NoTankActions;
        }

        public int GetLineOfFireDefenceDistanceToHomeBaseByIncomingAttackDirection(int playerIndex, int tankNumber, 
            Direction finalIncomingDirectionOfAttack)
        {
            Player player = Game.Current.Players[playerIndex];
            Tank tank = player.Tanks[tankNumber];
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)  // TODO: Check if locked in a firefight also
            {
                DirectionalMatrix<DistanceCalculation> distanceMatrix
                    = GameState.CalculationCache.GetDistanceMatrixFromTankByTankIndex(tank.Index);
                Direction defenceDir = finalIncomingDirectionOfAttack.GetOpposite();
                if (defenceDir == Direction.NONE)
                {
                    defenceDir = Direction.RIGHT;
                }
                int startPosOffsetFromBase = Constants.TANK_OUTER_EDGE_OFFSET;
                int endPosOffsetFromBase = startPosOffsetFromBase + MAX_POINTS_TO_TRY_FOR_DEFENCE_POS;
                for (int offsetSize = startPosOffsetFromBase; offsetSize < endPosOffsetFromBase; offsetSize++)
                {
                    Point defencePos = player.Base.Pos + defenceDir.GetOffset(offsetSize);
                    DistanceCalculation tankDefenceCalc = distanceMatrix[defenceDir, defencePos];
                    if (tankDefenceCalc.CodedDistance == 0)
                    {
                        // This defence point can't be reached, try further away
                        continue;
                    }
                    return tankDefenceCalc.Distance;
                }
            }
            return Constants.UNREACHABLE_DISTANCE;
        }

        public TankAction[] GetActionsToReachLineOfFireDefencePointByIncomingAttackDirection(
            int playerIndex, int tankNumber, Direction finalIncomingDirectionOfAttack)
        {
            Player player = Game.Current.Players[playerIndex];
            Tank tank = player.Tanks[tankNumber];
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)  // TODO: Check if locked in a firefight also
            {
                DirectionalMatrix<DistanceCalculation> distanceMatrix
                    = GameState.CalculationCache.GetDistanceMatrixFromTankByTankIndex(tank.Index);
                Direction defenceDir = finalIncomingDirectionOfAttack.GetOpposite();
                if (defenceDir == Direction.NONE)
                {
                    defenceDir = Direction.RIGHT;
                }
                int startPosOffsetFromBase = Constants.TANK_OUTER_EDGE_OFFSET;
                int endPosOffsetFromBase = startPosOffsetFromBase + MAX_POINTS_TO_TRY_FOR_DEFENCE_POS;
                for (int offsetSize = startPosOffsetFromBase; offsetSize < endPosOffsetFromBase; offsetSize++)
                {
                    Point defencePos = player.Base.Pos + defenceDir.GetOffset(offsetSize);
                    DistanceCalculation tankDefenceCalc = distanceMatrix[defenceDir, defencePos];
                    if (tankDefenceCalc.CodedDistance == 0)
                    {
                        // This defence point can't be reached, try further away
                        continue;
                    }
                    return PathCalculator.GetTankActionsOnOutgoingShortestPath(distanceMatrix, defenceDir, defencePos);
                }
            }
            return new TankAction[0];
        }
    }
}
