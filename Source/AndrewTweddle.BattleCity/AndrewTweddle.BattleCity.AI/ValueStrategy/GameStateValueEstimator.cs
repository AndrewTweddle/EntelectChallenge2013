using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.AI.ValueStrategy
{
    public class GameStateValueEstimator
    {
        public GameState GameState { get; set; }
        public Player Player { get; set; }

        public double Evaluate()
        {
            double evaluation = EvaluateSmallestAttackDistanceToEnemyBase();
            return evaluation;
        }

        public double EvaluateSmallestAttackDistanceToEnemyBase()
        {
            int[] minDistanceToAttackEnemyBaseByPlayerIndex = new int[Constants.PLAYERS_PER_GAME];
            int[] minDistanceToDefendOwnBaseByPlayerIndex = new int[Constants.PLAYERS_PER_GAME];
            bool[] isUnmarked = new bool[Constants.PLAYERS_PER_GAME];
            Direction[] attackDirOnEnemyBaseByPlayerIndex = new Direction[Constants.PLAYERS_PER_GAME];
            double[] score = new double[Constants.PLAYERS_PER_GAME];

            foreach (Player player in Game.Current.Players)
            {
                int minDistanceToEnemyBase = Constants.UNREACHABLE_DISTANCE;

                foreach (Tank tank in player.Tanks)
                {
                    MobileState tankState = GameState.GetMobileState(tank.Index);
                    if (tankState.IsActive)  // TODO: Check if locked in a firefight also
                    {
                        DistanceCalculation baseAttackCalc 
                            = GameState.CalculationCache.GetIncomingDistanceMatrixForBase(1 - player.Index)[tankState];

                        int distanceToEnemyBase = baseAttackCalc.Distance;
                        if (distanceToEnemyBase < minDistanceToEnemyBase)
                        {
                            minDistanceToEnemyBase = distanceToEnemyBase;
                            attackDirOnEnemyBaseByPlayerIndex[player.Index] = baseAttackCalc.AdjacentNode.Dir;
                        }
                    }
                }
                minDistanceToAttackEnemyBaseByPlayerIndex[player.Index] = minDistanceToEnemyBase;
            }

            foreach (Player player in Game.Current.Players)
            {
                int minDistanceToDefendOwnBase = Constants.UNREACHABLE_DISTANCE;

                foreach (Tank tank in player.Tanks)
                {
                    MobileState tankState = GameState.GetMobileState(tank.Index);
                    if (tankState.IsActive)  // TODO: Check if locked in a firefight also
                    {
                        DirectionalMatrix<DistanceCalculation> distanceMatrix
                            = GameState.CalculationCache.GetDistanceMatrixFromTankByTankIndex(tank.Index);
                        Direction attackDir = attackDirOnEnemyBaseByPlayerIndex[ 1- player.Index];
                        Direction defenceDir = attackDir.GetOpposite();
                        DistanceCalculation tankDefenceCalc = distanceMatrix[
                            defenceDir, player.Base.Pos + (Constants.SEGMENT_SIZE + 1) * defenceDir.GetOffset()];

                        int distance = tankDefenceCalc.Distance;
                        if (distance < minDistanceToDefendOwnBase)
                        {
                            minDistanceToDefendOwnBase = distance;
                        }
                    }
                }
                minDistanceToDefendOwnBaseByPlayerIndex[player.Index] = minDistanceToDefendOwnBase;
                if (minDistanceToAttackEnemyBaseByPlayerIndex[1 - player.Index] < minDistanceToDefendOwnBase)
                {
                    isUnmarked[1 - player.Index] = true;
                }
            }

            foreach (Player player in Game.Current.Players)
            {
                int minAttackDistance = minDistanceToAttackEnemyBaseByPlayerIndex[player.Index];
                int playerScore = 0;
                if (!(Game.Current.CurrentTurn.Tick + minAttackDistance > Game.Current.FinalTickInGame))
                {
                    if (isUnmarked[Player.Index])
                    {
                        playerScore = 100000 * (250 - minAttackDistance);
                    }
                    else
                    {
                        playerScore = 100 * (minDistanceToDefendOwnBaseByPlayerIndex[1 - player.Index] - minAttackDistance);
                    }
                }
                score[player.Index] = playerScore;
            }

            return score[Player.Index] - score[1 - Player.Index];
        }
    }
}
