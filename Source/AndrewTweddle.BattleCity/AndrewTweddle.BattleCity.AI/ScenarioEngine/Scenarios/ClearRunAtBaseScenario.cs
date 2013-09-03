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

        public bool IsValid(Move[] movesByLevel)
        {
            Move move = movesByLevel[0];

            // Exclude directions of attack which aren't possible:
            if (!Game.Current.Players[move.pBar].Base.GetPossibleIncomingAttackDirections().Contains(move.dir1))
            {
                return false;
            }

            // Tank p_i must be alive:
            return GetTankState_i(move).IsActive;
        }

        public void EvaluateLeafNodeMove(Move[] movesByLevel)
        {
            // There is only one level:
            Move move = movesByLevel[0];
            MobileState tankState_j = GetTankState(move.pBar, move.j);
            MobileState tankState_jBar = GetTankState(move.pBar, move.jBar);

            // Get the attack distance of player p's tank i to the enemy base:
            int A_p_i = base.GetAttackDistanceOfTankToEnemyBaseFromDirection(move.p, move.i, move.dir1);

            // TODO: TankAction[] attackActions = 

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
        }
    }
}
