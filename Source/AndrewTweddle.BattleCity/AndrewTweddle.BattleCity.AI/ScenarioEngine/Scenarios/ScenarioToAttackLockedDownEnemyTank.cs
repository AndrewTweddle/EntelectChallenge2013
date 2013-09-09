using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.Scenarios
{
    public class ScenarioToAttackLockedDownEnemyTank: Scenario
    {
        public int YourPlayerIndex { get; private set; }

        public ScenarioToAttackLockedDownEnemyTank(
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
                new MoveGeneratorOfTankCombinationsForPlayerP(),
                new MoveGeneratorOfTankCombinationsForPlayerPBar(ScenarioDecisionMaker.p),
                new MoveGeneratorOfDirectionsForDir1(ScenarioDecisionMaker.p)
            };
        }

        public override MoveResult EvaluateLeafNodeMove(Move move)
        {
            MobileState tankState_i = GetTankState_i(move);
            MobileState tankState_j = GetTankState_j(move);

            // Both tanks must be alive:
            if (!(tankState_i.IsActive && tankState_j.IsActive))
            {
                return new MoveResult(move)
                {
                    EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
                };
            }

            TankSituation tankSit_i = GetTankSituation(move.p, move.i);
            TankSituation tankSit_j = GetTankSituation(move.pBar, move.j);

            if (tankSit_i.IsLockedDown || !tankSit_j.IsLockedDown)
            {
                return new MoveResult(move)
                {
                    EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
                };
            }

            if (move.dir1 == tankSit_j.DirectionOfAttackForLockDown.GetOpposite())
            {
                return new MoveResult(move)
                {
                    EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
                };
            }

            int attackDistance = GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
                move.p, move.i, tankState_j.Pos, move.dir1, TankHelper.EdgeOffsets);

            return new MoveResult(move)
            {
                EvaluationOutcome = ScenarioEvaluationOutcome.Current,
                Slack = attackDistance
            };
        }

        public override void ChooseMovesAsP(MoveResult moveResult)
        {
            Move move = moveResult.Move;
            MobileState tankState_j = GetTankState_j(move);
            TankAction[] attackActions = GetTankActionsFromTankToAttackTankAtPointAlongDirectionOfMovement(
                move.p, move.i, tankState_j.Pos, move.dir1, TankHelper.EdgeOffsets, 
                keepMovingCloserOnFiringLastBullet: false);
            if (attackActions.Length > 0)
            {
                int attackDistance = GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
                    move.p, move.i, tankState_j.Pos, move.dir1, TankHelper.EdgeOffsets);
                TankAction attackAction = attackActions[0];
                double attackActionValue = ScenarioValueFunctions.AttackLockedDownEnemyTankFunction.Evaluate(attackDistance);
                TankSituation tankSit_i = GetTankSituation(move.p, move.i);
                tankSit_i.TankActionSituationsPerTankAction[(int)attackAction].Value += attackActionValue;
            }
        }

        public override void ChooseMovesAsPBar(MoveResult moveResult)
        {
        }
    }
}
