using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;
using AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.Scenarios
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
        public ClearRunAtBaseScenario(GameState gameState): base(gameState)
        {
        }

        public override MoveGenerator[] GetMoveGeneratorsByMoveTreeLevel()
        {
            return new MoveGenerator[]
            {
                new MoveGeneratorOfPlayers(),
                new MoveGeneratorOfTankCombinationsForPlayerP(),
                new MoveGeneratorOfDirectionsForDir1()
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

            // TODO: Tank p_i must be alive, not locked down, not locked in a quadrant, 
            return GetTankState_i(move).IsActive;
        }

        public MoveResult EvaluateLeafNodeMove(Move[] movesByLevel)
        {
            TankAction[] actions_p_i;
            TankAction[] actions_p_iBar = new TankAction[0];
            TankAction[] actions_pBar_j;
            TankAction[] actions_pBar_jBar;

            // There is only one level:
            Move move = movesByLevel[0];
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

            if (A_p_i == Constants.UNREACHABLE_DISTANCE)
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

            // Get the minimum attack distance of player pBar's tanks:
            int A_pBar_j = GetAttackDistanceOfTankToEnemyBase(move.pBar, move.j);
            int A_pBar_jBar = GetAttackDistanceOfTankToEnemyBase(move.pBar, move.jBar);
            int A_pBar_MIN = Math.Min(A_pBar_j, A_pBar_jBar);

            // Calculate slack A as p's attack distance less pBar's attack distance
            int slackA = A_p_i - A_pBar_MIN;
            
            // Get the minimum defence distances of player pBar's tank j to the base:
            int D_pBar_j = GetLineOfFireDefenceDistanceToHomeBaseByIncomingAttackDirection(move.pBar, move.j, move.dir1);
            int D_pBar_jBar = GetLineOfFireDefenceDistanceToHomeBaseByIncomingAttackDirection(move.pBar, move.jBar, move.dir1);
            int D_pBar_MIN = Math.Min(D_pBar_j, D_pBar_jBar);

            // Calculate slack D as p's attack distance less pBar's defence distance 
            // (to the same base and on the same direction of attack):
            int slackD = A_p_i - D_pBar_MIN;

            // Get the overall slack (distance to activating this scenario):
            int slack = Math.Max(slackA, slackD);

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
                }
                else
                {
                    pBar_j_defends = false;
                    pBar_jBar_defends = true;
                }
            }
            else
            {
                // Attacking is the smallest slack, so put most effort here:
                if (A_pBar_j < A_pBar_jBar)
                {
                    pBar_j_defends = false;
                    pBar_jBar_defends = true;
                }
                else
                {
                    pBar_j_defends = true;
                    pBar_jBar_defends = false;
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
            tankActionRec = new TankActionRecommendation
            {
                IsAMoveRecommended = true,
                RecommendedTankAction = actions_pBar_j[0]
            };
            moveResult.SetTankActionRecommendation(move.pBar, move.j, tankActionRec);

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
            tankActionRec = new TankActionRecommendation
            {
                IsAMoveRecommended = true,
                RecommendedTankAction = actions_pBar_jBar[0]
            };
            moveResult.SetTankActionRecommendation(move.pBar, move.jBar, tankActionRec);

            return moveResult;
        }
    }
}
