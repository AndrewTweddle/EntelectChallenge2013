using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.Scenarios
{
    public class ScenarioToAttackEnemyBase: Scenario
    {
        public int YourPlayerIndex { get; private set; }

        public ScenarioToAttackEnemyBase(
            GameState gameState, GameSituation gameSituation, int yourPlayerIndex)
            : base(gameState, gameSituation)
        {
            YourPlayerIndex = yourPlayerIndex;
        }

        public override MoveGenerator[] GetMoveGeneratorsByMoveTreeLevel()
        {
            return new MoveGenerator[] 
            { 
                new MoveGeneratorForPlayerPAsYourPlayerIndex(YourPlayerIndex),
                new MoveGeneratorOfTankCombinationsForPlayerP()
            };
        }

        public override MoveResult EvaluateLeafNodeMove(Move move)
        {
            MobileState tankState_i = GetTankState_i(move);
            TankSituation tankSit_i = GetTankSituation(move.p, move.i);
            if (tankSit_i.IsLockedDown || !tankState_i.IsActive)
            {
                return new MoveResult(move)
                {
                    EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
                };
            }

            foreach (TankActionSituation tankActSit in tankSit_i.TankActionSituationsPerTankAction)
            {
                MobileState newTankState = tankActSit.NewTankState;
                int attackDistToEnemyBase = GetAttackDistanceToEnemyBaseFromTankState(YourPlayerIndex, ref newTankState);
                if (tankActSit.TankAction == TankAction.FIRE && tankActSit.IsAdjacentWallRemoved)
                {
                    Point adjacentPos = tankActSit.NewTankState.Pos + tankActSit.NewTankState.Dir.GetOffset();
                    MobileState adjacentState = new MobileState(adjacentPos, tankActSit.NewTankState.Dir, isActive: true);
                    int attackDistToEnemyBase2 = GetAttackDistanceToEnemyBaseFromTankState(YourPlayerIndex, ref adjacentState) + 1;
                    if (attackDistToEnemyBase2 < attackDistToEnemyBase)
                    {
                        attackDistToEnemyBase = attackDistToEnemyBase2;
                    }
                }
                double value = ScenarioValueFunctions.AttackEnemyBaseFunction.Evaluate(attackDistToEnemyBase);
                tankActSit.Value += value;
            }

            // Convert a good position into killing the enemy base, by weighting the attacking action more highly:
            int attackDistance = GetAttackDistanceToEnemyBaseFromTankState(move.i, ref tankState_i);
            TankAction[] attackActions = GetActionsToAttackEnemyBase(move.p, move.i);
            if (attackActions.Length > 0)
            {
                TankAction attackAction = attackActions[0];
                double attackActionValue = ScenarioValueFunctions.AttackEnemyBaseAttackActionFunction.Evaluate(attackDistance);
                tankSit_i.TankActionSituationsPerTankAction[(int)attackAction].Value += attackActionValue;
            }

            return new MoveResult(move)
            {
                EvaluationOutcome = ScenarioEvaluationOutcome.Possible
            };
        }

        public override void ChooseMovesAsP(MoveResult moveResult)
        {
            // not used
        }

        public override void ChooseMovesAsPBar(MoveResult moveResult)
        {
            // not used
        }
    }
}
