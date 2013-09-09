using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.Scenarios
{
    public class ScenarioOfAttackingAnEnemyTank: Scenario
    {
        public int YourPlayerIndex { get; private set; }

        public ScenarioOfAttackingAnEnemyTank(
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
                new MoveGeneratorOfTankCombinationsForPlayerPBar(ScenarioDecisionMaker.p)
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

            // Weight for distance to attack enemy over the long haul versus having the upper hand in the encounter in the short term:
            foreach (TankActionSituation tankActSit in tankSit_i.TankActionSituationsPerTankAction)
            {
                int A_p_i = GetAttackDistanceFromHypotheticalTankStateToTank(
                    tankActSit.NewTankState, move.pBar, move.j, TankHelper.EdgeOffsets);
                int A_pBar_j = GetAttackDistanceFromTankToHypotheticalTankAtPoint(
                    move.pBar, move.j, tankActSit.NewTankState.Pos, TankHelper.EdgeOffsets);
                int attackDiffSlack = A_p_i - A_pBar_j;
                double attackValue = ScenarioValueFunctions.GrappleWithEnemyTankAttackFunction.Evaluate(A_p_i); 
                double grappleValue = ScenarioValueFunctions.GrappleWithEnemyTankAttackDiffFunction.Evaluate(attackDiffSlack);
                tankActSit.Value += attackValue + grappleValue;
            }

            // Convert a good attacking position into killing the enemy tank, by weighting the attacking action more highly:
            int attackDistance = GetAttackDistanceFromTankToTank(move.p, move.i, move.j);
            TankAction[] attackActions = GetTankActionsFromTankToAttackTank(move.p, move.i, move.j);
            if (attackActions.Length > 0)
            {
                TankAction attackAction = attackActions[0];
                double attackActionValue = ScenarioValueFunctions.GrappleWithEnemyTankAttackActionFunction.Evaluate(attackDistance);
                tankSit_i.TankActionSituationsPerTankAction[(int)attackAction].Value += attackActionValue;
            }

            return new MoveResult(move)
            {
                EvaluationOutcome = ScenarioEvaluationOutcome.Possible
            };
        }

        public override void ChooseMovesAsP(MoveResult moveResult)
        {
        }

        public override void ChooseMovesAsPBar(MoveResult moveResult)
        {
        }
    }
}
