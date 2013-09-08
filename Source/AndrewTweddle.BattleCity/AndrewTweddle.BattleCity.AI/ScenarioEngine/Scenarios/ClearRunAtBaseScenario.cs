using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;
using AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.Scenarios
{
    /// <summary>
    /// This scenario identifies situations where a tank can reach the enemy base 
    /// quicker than the enemy can either defend the base or attack his base.
    /// The other friendly tank is out of action 
    /// (e.g. dead, locked down, shunted into a quadrant or too far away from either base).
    /// One or both enemy tanks may be out of action as well.
    /// 
    /// This situation could arise when a friendly tank has sacrificed itself
    /// by luring two enemy tanks away from the other friendly tank, 
    /// giving it a clear run at the enemy base.
    /// </summary>
    public class ClearRunAtBaseScenario: Scenario
    {
        private const int EVALUATION_OUTCOME_CLOSE_THRESHOLD = 5;

        public ClearRunAtBaseScenario(GameState gameState, GameSituation gameSituation)
            : base(gameState, gameSituation)
        {
        }

        public override MoveGenerator[] GetMoveGeneratorsByMoveTreeLevel()
        {
            return new MoveGenerator[]
            {
                new MoveGeneratorOfPlayers(),
                new MoveGeneratorOfTankCombinationsForPlayerP(),
                new MoveGeneratorOfDirectionsForDir1(ScenarioDecisionMaker.p)
                // NOT NEEDED, SYMMETRICAL: new MoveGeneratorOfTankCombinationsForPlayerPBar()
            };
        }

        public bool IsValid(Move move)
        {
            // Exclude directions of attack which aren't possible:
            if (!Game.Current.Players[move.pBar].Base.GetPossibleIncomingAttackDirections().Contains(move.dir1))
            {
                return false;
            }

            if (!GetTankState_i(move).IsActive)
            {
                return false;
            }

            TankSituation tankSit_i = GetTankSituation(move.p, move.i);
            if (tankSit_i.IsInLineOfFire || tankSit_i.IsLockedDown || tankSit_i.IsShutIntoQuadrant)
            {
                return false;
            }
            return true;
        }

        public override MoveResult EvaluateLeafNodeMove(Move move)
        {
            TankAction[] actions_p_i;
            TankAction[] actions_p_iBar = new TankAction[0];
            TankAction[] actions_pBar_j;
            TankAction[] actions_pBar_jBar;

            MobileState tankState_j = GetTankState(move.pBar, move.j);
            MobileState tankState_jBar = GetTankState(move.pBar, move.jBar);

            MoveResult moveResult = new MoveResult(move);

            if (!IsValid(move))
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Invalid;
                return moveResult;
            }

            // Get the attack distance of player p's tank i to the enemy base:
            int A_p_i = GetAttackDistanceOfTankToEnemyBaseFromDirection(move.p, move.i, move.dir1);

            if (A_p_i >= Constants.UNREACHABLE_DISTANCE)
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Invalid;
                return moveResult;
            }

            // In this scenario the other tank (iBar) is either dead or too far from the action to be of any use:
            int A_p_iBar = GetAttackDistanceOfTankToEnemyBaseFromDirection(move.p, move.iBar, move.dir1);
            if (A_p_iBar < A_p_i)
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Invalid;
                return moveResult;
            }

            // Set the recommended action for the attacking tank:
            actions_p_i = GetActionsToAttackEnemyBaseFromDirection(move.p, move.i, move.dir1);
            TankActionRecommendation tankActionRec = new TankActionRecommendation
            {
                IsAMoveRecommended = true,
                RecommendedTankAction = actions_p_i[0]
            };
            moveResult.SetTankActionRecommendation(move.p, move.i, tankActionRec);

            // *** The role for tank p_i is to attack the enemy base along direction dir1.

            // Get the minimum attack distance of player pBar's tanks:
            int A_pBar_j = GetAttackDistanceOfTankToEnemyBase(move.pBar, move.j);
            int A_pBar_jBar = GetAttackDistanceOfTankToEnemyBase(move.pBar, move.jBar);
            int A_pBar_MIN = Math.Min(A_pBar_j, A_pBar_jBar);

            // iBar is not part of this scenario. Ensure they can't interfere with the defence:
            int D_p_iBar = GetCentralLineOfFireDefenceDistanceToHomeBase(move.p, move.iBar);
            if (D_p_iBar <= A_pBar_MIN)
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Invalid;
                return moveResult;
            }

            // Calculate slack A as p's attack distance less pBar's attack distance
            int slackA = A_p_i - A_pBar_MIN;

            // *** slackA is the attack slack (p's attack distance less pBar's best attack distance)
            
            // Get the minimum defence distances of player pBar's tank j to the base:
            int D_pBar_j = GetLineOfFireDefenceDistanceToHomeBaseByIncomingAttackDirection(move.pBar, move.j, move.dir1);
            int D_pBar_jBar = GetLineOfFireDefenceDistanceToHomeBaseByIncomingAttackDirection(move.pBar, move.jBar, move.dir1);
            int D_pBar_MIN = Math.Min(D_pBar_j, D_pBar_jBar);

            // Calculate slack D as p's attack distance less pBar's defence distance 
            // (to the same base and on the same direction of attack):
            int slackD = A_p_i - D_pBar_MIN;

            // *** slackD is the defence slack (defender distance to defence less attacker distance to attack with direction dir1

            // Get the overall slack (distance to activating this scenario):
            int slack = Math.Max(slackA, slackD);
            moveResult.Slack = slack;
            if (slack < 0)
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Current;
            }
            else
                if (slack <= EVALUATION_OUTCOME_CLOSE_THRESHOLD)
                {
                    moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Close;
                }
                else
                {
                    moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Possible;
                }

            // Calculate best defensive actions for the defender:
            bool pBar_j_defends;
            bool pBar_jBar_defends;
            if (slackD < slackA)
            {
                // Defending is the smallest slack, so put best defensive effort here:
                if (D_pBar_j < D_pBar_jBar)
                {
                    pBar_j_defends = true;
                    pBar_jBar_defends = false;

                    // *** Tank goal: pBar_j defends base from incoming attack with direction dir1
                    // *** Tank goal: pBar_jBar does nothing
                }
                else
                {
                    pBar_j_defends = false;
                    pBar_jBar_defends = true;

                    // *** Tank goal: pBar_j does nothing
                    // *** Tank goal: pBar_jBar defends base from incoming attack with direction dir1
                }
            }
            else
            {
                // Attacking is the smallest slack, so put most effort here:
                if (A_pBar_j < A_pBar_jBar)
                {
                    pBar_j_defends = false;
                    pBar_jBar_defends = true;

                    // *** Tank goal: pBar_j does nothing
                    // *** Tank goal: pBar_jBar defends base from incoming attack with direction dir1
                }
                else
                {
                    pBar_j_defends = true;
                    pBar_jBar_defends = false;

                    // *** Tank goal: pBar_j defends base from incoming attack with direction dir1
                    // *** Tank goal: pBar_jBar does nothing
                }
            }

            // Determine best action for pBar.j:
            // TODO: Check if is alive, is locked down, is locked in a quadrant, already has a move assigned, etc.
            if (pBar_j_defends)
            {
                actions_pBar_j 
                    = base.GetActionsToReachLineOfFireDefencePointByIncomingAttackDirection(move.pBar, move.j, move.dir1);
            }
            else
            {
                actions_pBar_j = GetActionsToAttackEnemyBase(move.pBar, move.j);
            }
            if (actions_pBar_j != null && actions_pBar_j.Length > 0)
            {
                tankActionRec = new TankActionRecommendation
                {
                    IsAMoveRecommended = true,
                    RecommendedTankAction = actions_pBar_j[0]
                };
                moveResult.SetTankActionRecommendation(move.pBar, move.j, tankActionRec);
            }

            // Determine best action for pBar.jBar:
            // TODO: Check if is alive, is locked down, is locked in a quadrant, already has a move assigned, etc.
            if (pBar_jBar_defends)
            {
                actions_pBar_jBar
                    = base.GetActionsToReachLineOfFireDefencePointByIncomingAttackDirection(move.pBar, move.jBar, move.dir1);
            }
            else
            {
                actions_pBar_jBar = GetActionsToAttackEnemyBase(move.pBar, move.jBar);
            }

            if (actions_pBar_jBar != null && actions_pBar_jBar.Length > 0)
            {
                tankActionRec = new TankActionRecommendation
                {
                    IsAMoveRecommended = true,
                    RecommendedTankAction = actions_pBar_jBar[0]
                };
                moveResult.SetTankActionRecommendation(move.pBar, move.jBar, tankActionRec);
            }

            return moveResult;
        }

        public override void ChooseMovesAsP(MoveResult moveResult)
        {
            Move move = moveResult.Move;
            LogDebugMessage("*** PROTAGONIST (P = {0}) ***", move.p);
            LogDebugMessage("Slack: {0}", moveResult.Slack);

            double valueOfMove = ScenarioValueFunctions.ClearRunAtBaseScenarioValueFunction.Evaluate(moveResult.Slack);
            LogDebugMessage("Value of move: {0}", valueOfMove);

            // Attack the enemy base:
            LogDebugMessage("Tank number: {0}", move.i);
            TankActionRecommendation recommendation
                = moveResult.GetRecommendedTankActionsByPlayerAndTankNumber(move.p, move.i);

            if (recommendation.IsAMoveRecommended)
            {
                TankSituation tankSituation = GameSituation.GetTankSituationByPlayerAndTankNumber(move.p, move.i);
                TankAction recommendedTankAction = recommendation.RecommendedTankAction;
                LogDebugMessage("Recommended tank action: {0}", recommendedTankAction);

                tankSituation.AdjustTankActionValue(recommendedTankAction, valueOfMove);
            }
            else
            {
                LogDebugMessage("No moves recommended");
            }
        }

        public override void ChooseMovesAsPBar(MoveResult moveResult)
        {
            Move move = moveResult.Move;
            LogDebugMessage("*** ANTAGONIST (PBar = {0}) ***", move.pBar);
            LogDebugMessage("Slack: {0}", moveResult.Slack);

            double valueOfMove = ScenarioValueFunctions.ClearRunAtBaseScenarioValueFunction.Evaluate(moveResult.Slack);
            LogDebugMessage("Value of move: {0}", valueOfMove);

            // Defend against an enemy attack on your base:
            for (int tankNumber = 0; tankNumber < Constants.TANKS_PER_PLAYER; tankNumber++)
            {
                LogDebugMessage("Tank number: {0}", tankNumber);
                TankActionRecommendation recommendation = moveResult.GetRecommendedTankActionsByPlayerAndTankNumber(move.pBar, tankNumber);
                if (recommendation.IsAMoveRecommended)
                {
                    TankSituation tankSituation
                        = GameSituation.GetTankSituationByPlayerAndTankNumber(move.pBar, tankNumber);
                    TankAction recommendedTankAction = recommendation.RecommendedTankAction;
                    LogDebugMessage("Recommended tank action: {0}", recommendedTankAction);

                    tankSituation.AdjustTankActionValue(recommendedTankAction, valueOfMove);
                }
                else
                {
                    LogDebugMessage("No moves recommended");
                }
            }
        }

        /* *** Pseudocode for considering effect of all tank actions on the scenario slack:
        public void AdjustValueForTankAction(MoveResult moveResult,
            GameSituation gameSituation, TankSituation tankSituation,
            TankActionSituation tankActionSituation, ScenarioTankRole tankRole)
        {
            Direction attackDir = moveResult.Move.dir1; ;
            double valueAdjustment = 0.0;
            int slackA = 0;  // TODO
            int slackD = 0;  // TODO
            int A_p_i = 0; // TODO

            switch (tankRole)
            {
                case ScenarioTankRole.p_i:
                    int A_p_i_adjustment = 1;  // 1 action taken
                    MobileState newState = tankActionSituation.NewGameState.GetMobileState(...);
                    if (tankActionSituation.IsAdjacentWallRemoved)
                    {
                        newState = new MobileState(newState.Pos + newState.Dir.GetOffset(), newState.Dir, newState.IsActive);
                        A_p_i_adjustment++;
                    }
                    int A_p_i_fromNewSpot = 0; // TODO: Calculate
                    A_p_i_fromNewSpot += A_p_i_adjustment;
                    int A_p_iBar_diff = A_p_i_fromNewSpot - A_p_i;
                    // TODO: Set valueAdjustment
                    break;
                case ScenarioTankRole.p_iBar:
                    break;
                case ScenarioTankRole.pBar_j:
                    break;
            }
        }
         */
    }
}
