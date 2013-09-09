using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.Scenarios
{
    public class ScenarioToApplyLockDownActions: Scenario
    {
        private const int LOCKDOWN_FIRING_THRESHOLD = 8;  // At this distance a tank that moves will die. It must either shoot or wait to shoot.

        public ScenarioToApplyLockDownActions(GameState gameState, GameSituation gameSituation)
            : base(gameState, gameSituation)
        {
        }

        public override MoveGenerator[] GetMoveGeneratorsByMoveTreeLevel()
        {
            return new MoveGenerator[]
            {
                new MoveGeneratorOfPlayers(),
                new MoveGeneratorOfTankCombinationsForPlayerP(ScenarioDecisionMaker.None),
                new MoveGeneratorOfTankCombinationsForPlayerPBar(ScenarioDecisionMaker.None)
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

            Direction horizDir = tankState_i.Pos.GetHorizontalDirectionToPoint(tankState_j.Pos);
            Direction vertDir = tankState_i.Pos.GetVerticalDirectionToPoint(tankState_j.Pos);
            int xDiff = tankState_j.Pos.X - tankState_i.Pos.X;
            int yDiff = tankState_j.Pos.Y - tankState_i.Pos.Y;
            int absXDiff = Math.Abs(xDiff);
            int absYDiff = Math.Abs(yDiff);

            if (absXDiff == 0)
            {
                return CheckForLockDown(move, ref tankState_i, ref tankState_j, vertDir, absYDiff);
            }

            if (absYDiff == 0)
            {
                return CheckForLockDown(move, ref tankState_i, ref tankState_j, horizDir, absXDiff);
            }

            return new MoveResult(move)
            {
                EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
            };
        }

        private MoveResult CheckForLockDown(Move move, ref MobileState tankState_i, ref MobileState tankState_j, 
            Direction attackDir, int distanceBetweenCentrePoints)
        {
            if (distanceBetweenCentrePoints <= LOCKDOWN_FIRING_THRESHOLD)
            {
                if (tankState_i.Dir == attackDir)
                {
                    // Check if there are walls between the tanks:
                    bool wallFound = false;

                    for (int offset = Constants.TANK_OUTER_EDGE_OFFSET;
                        offset <= distanceBetweenCentrePoints - Constants.TANK_OUTER_EDGE_OFFSET;
                        offset++)
                    {
                        Point wallPoint = tankState_i.Pos + attackDir.GetOffset(offset);
                        if (GameState.Walls[wallPoint])
                        {
                            wallFound = true;
                            break;
                        }
                    }

                    if (!wallFound)
                    {
                        TankSituation tankSit_i = GetTankSituation(move.p, move.i);
                        if (tankSit_i.ExpectedNextTickWhenTankCanFireAgain > GameState.Tick)
                        {
                            return new MoveResult(move)
                            {
                                EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
                            };
                        }

                        tankSit_i.IsLockedDown = true;
                        tankSit_i.DirectionOfAttackForLockDown = attackDir;
                        TankSituation tankSit_j = GetTankSituation(move.pBar, move.j);
                        tankSit_j.IsLockedDown = true;
                        tankSit_j.DirectionOfAttackForLockDown = attackDir.GetOpposite();
                        tankSit_i.TankActionSituationsPerTankAction[(int)TankAction.FIRE].Value = ScenarioValueFunctions.VALUE_OF_A_TANK;

                        if (tankState_j.Dir == attackDir.GetOpposite())
                        {
                            tankSit_j.TankActionSituationsPerTankAction[(int)TankAction.FIRE].Value = ScenarioValueFunctions.VALUE_OF_A_TANK;
                        }
                        else
                        {
                            TankAction tankAction_j = attackDir.GetOpposite().ToTankAction();
                            tankSit_j.TankActionSituationsPerTankAction[(int)tankAction_j].Value = ScenarioValueFunctions.VALUE_OF_A_TANK;
                        }

                        return new MoveResult(move)
                        {
                            EvaluationOutcome = ScenarioEvaluationOutcome.Current
                        };
                    }
                }
            }
            return new MoveResult(move)
            {
                EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
            };
        }

        public override void ChooseMovesAsP(MoveResult moveResult)
        {
            // Not used
        }

        public override void ChooseMovesAsPBar(MoveResult moveResult)
        {
            // Not used
        }
    }
}
