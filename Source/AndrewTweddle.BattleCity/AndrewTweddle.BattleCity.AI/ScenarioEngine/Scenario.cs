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

namespace AndrewTweddle.BattleCity.AI.Scenarios
{
    public abstract class Scenario
    {
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
                Point defencePos = player.Base.Pos + (Constants.SEGMENT_SIZE + 1) * defenceDir.GetOffset();
                DistanceCalculation tankDefenceCalc = distanceMatrix[defenceDir, defencePos];
                return tankDefenceCalc.Distance;
            }
            return Constants.UNREACHABLE_DISTANCE;
        }
    }
}
