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
    public class ScenarioOfAttackingAnUnarmedTank: Scenario
    {
        public int YourPlayerIndex { get; private set; }

        public ScenarioOfAttackingAnUnarmedTank(
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
            TankSituation tankSit_j = GetTankSituation(move.pBar, move.j);
            int ticksUntilFriendlyTankCanFireAgain = tankSit_i.ExpectedNextTickWhenTankCanFireAgain - GameState.Tick;
            int ticksUntilEnemyTankCanFireAgain = tankSit_j.ExpectedNextTickWhenTankCanFireAgain - GameState.Tick;

            if (ticksUntilEnemyTankCanFireAgain <= 0 || ticksUntilFriendlyTankCanFireAgain > 0)
            {
                return new MoveResult(move)
                {
                    EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
                };
            }

            int attackDistance = GetAttackDistanceFromTankToTank(move.p, move.i, move.j);

            if (attackDistance - ticksUntilEnemyTankCanFireAgain > 5)
            {
                return new MoveResult(move)
                {
                    EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
                };
            }

            foreach (TankActionSituation tankActSit in tankSit_i.TankActionSituationsPerTankAction)
            {
                int A_p_i = GetAttackDistanceFromHypotheticalTankStateToTank(
                    tankActSit.NewTankState, move.pBar, move.j, TankHelper.EdgeOffsets);
                int slack = A_p_i - ticksUntilEnemyTankCanFireAgain;
                double attackValue = ScenarioValueFunctions.AttackDisarmedEnemyTankSlackUntilRearmedFunction.Evaluate(slack);
                tankActSit.Value += attackValue;
            }

            // Convert a good attacking position into killing the enemy tank, by weighting the attacking action more highly:
            TankAction[] attackActions = GetTankActionsFromTankToAttackTank(move.p, move.i, move.j);
            if (attackActions.Length > 0)
            {
                TankAction attackAction = attackActions[0];
                double attackActionValue = ScenarioValueFunctions.AttackDisarmedEnemyTankAttackActionFunction.Evaluate(attackDistance);
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
