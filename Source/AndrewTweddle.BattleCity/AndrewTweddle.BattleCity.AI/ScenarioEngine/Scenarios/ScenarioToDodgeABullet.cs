using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Calculations.Bullets;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Calculations;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.Scenarios
{
    public class ScenarioToDodgeABullet: Scenario
    {
        #region Constants

        public const int DEFAULT_EXPANSION = 15;

        #endregion

        public int YourPlayerIndex { get; private set; }

        public ScenarioToDodgeABullet(GameState gameState, GameSituation gameSituation, int yourPlayerIndex)
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
            TankSituation tankSituation = GetTankSituation(move.p, move.i);
            int ticksUntil_p_i_CanFireAgain = tankSituation.ExpectedNextTickWhenTankCanFireAgain - GameState.Tick;
            int worstSlack = -Constants.UNREACHABLE_DISTANCE;
            TankAction tankActionToAddressWorstSlack = TankAction.NONE;
            bool isScenarioApplicable = false;

            foreach (BulletSituation bulletSituation in GameSituation.BulletSituationsByTankIndex)
            {
                if (bulletSituation == null)
                {
                    continue;
                }

                EvaluateBulletSituation(move, tankSituation, bulletSituation,
                    ticksUntil_p_i_CanFireAgain, ref worstSlack, ref tankActionToAddressWorstSlack, ref isScenarioApplicable);
            }

            if (isScenarioApplicable)
            {
                MoveResult moveResult = new MoveResult(move)
                {
                    Slack = worstSlack,
                    EvaluationOutcome
                        = worstSlack < -10
                        ? ScenarioEvaluationOutcome.Possible
                        : (worstSlack <= 0
                          ? ScenarioEvaluationOutcome.Close
                          : ScenarioEvaluationOutcome.Current
                          )
                };
                moveResult.RecommendedTankActionsByTankIndex[tankSituation.Tank.Index] 
                    = new TankActionRecommendation { IsAMoveRecommended = true, RecommendedTankAction = tankActionToAddressWorstSlack };
                return moveResult;
            }
            else
            {
                return new MoveResult(move)
                {
                    EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
                };
            }
        }

        private void EvaluateBulletSituation(Move move, TankSituation tankSituation, BulletSituation bulletSituation,
            int ticksUntil_p_i_CanFireAgain, ref int worstSlack, ref TankAction tankActionToAddressWorstSlack,
            ref bool isScenarioApplicable)
        {
            bool isScenarioApplicableForThisBulletSituation = false;
            int bestSlackForThisBulletSituation = Constants.UNREACHABLE_DISTANCE;
            TankAction bestTankActionForThisBulletSituation = TankAction.NONE;

            foreach (BulletCalculationByTick bulletCalc in bulletSituation.BulletCalculationsByTick)
            {
                if (!bulletCalc.AreTanksAtRisk)
                {
                    continue;
                }
                int ticksToEscape = bulletCalc.Tick - GameState.Tick;
                if (ticksToEscape < 0)
                {
                    continue;
                }

                // Are you in the firing line:
                if (bulletCalc.TankCentrePointsThatDie.ContainsPoint(tankSituation.TankState.Pos))
                {
                    List<BulletSurvivalTactic> bulletTactics = new List<BulletSurvivalTactic>();
                    isScenarioApplicableForThisBulletSituation = true;
                    
                    // Suppose the bullet just misses the tank on turn n, with t ticks to escape...
                    int closestEscapeTick = ticksToEscape * 2 / 3;   // 2t - n + 1 = 2n + 1 so 3n = 2t
                    int furthestEscapeTick = 2 * ticksToEscape + 1;  // 2t + n + 1 = 2n so n = 2t + 1

                    for (int tk = closestEscapeTick; tk <= furthestEscapeTick; tk++)
                    {
                        if (tk >= bulletSituation.BulletCalculationsByTick.Length)
                        {
                            break;
                        }
                        BulletCalculationByTick bulletCalc_tk = bulletSituation.BulletCalculationsByTick[tk];
                        if (!bulletCalc_tk.AreTanksAtRisk)
                        {
                            break;
                        }
                        ticksToEscape = bulletCalc_tk.Tick - GameState.Tick;

                        // Fire at the bullet:
                        foreach (MobileState headOnState in bulletCalc_tk.ClosestTankStatesThatCanShootBulletHeadOn)
                        {
                            // Move into position to confront the bullet:
                            AddBulletSurvivalTactic(move, ticksUntil_p_i_CanFireAgain, ticksToEscape,
                                bulletTactics, headOnState, bulletSituation.TickOffsetWhenTankCanFireAgain,
                                isConfrontingBullet: true);
                        }

                        // Other moves to dodge the bullet:
                        foreach (Point survivalPoint in bulletCalc_tk.ClosestTankCentrePointsThatSurviveClockwise)
                        {
                            Direction dir = bulletSituation.BulletMovementDir.Clockwise();
                            AddBulletSurvivalTactic(move, ticksUntil_p_i_CanFireAgain, ticksToEscape,
                                bulletTactics, survivalPoint, dir, bulletSituation.TickOffsetWhenTankCanFireAgain, 
                                isConfrontingBullet: false);
                        }

                        foreach (Point survivalPoint in bulletCalc_tk.ClosestTankCentrePointsThatSurviveAntiClockwise)
                        {
                            Direction dir = bulletSituation.BulletMovementDir.AntiClockwise();
                            AddBulletSurvivalTactic(move, ticksUntil_p_i_CanFireAgain, ticksToEscape,
                                bulletTactics, survivalPoint, dir, bulletSituation.TickOffsetWhenTankCanFireAgain, 
                                isConfrontingBullet: false);
                        }
                    }

                    var bulletTacticsByTankAction = bulletTactics.GroupBy(tactic => tactic.InitialTankAction);
                    foreach (var grouping in bulletTacticsByTankAction)
                    {
                        TankAction tankAction = grouping.Key;
                        BulletSurvivalTactic bestTacticForTankAction = grouping.OrderBy(tactic => tactic.Value).First();
                        tankSituation.TankActionSituationsPerTankAction[(int)tankAction].Value 
                            += bestTacticForTankAction.Value;
                            // Note that this is a combination of the slack value and the disarmament value
                        if (bestTacticForTankAction.Slack < bestSlackForThisBulletSituation)
                        {
                            bestSlackForThisBulletSituation = bestTacticForTankAction.Slack;
                            bestTankActionForThisBulletSituation = tankAction;
                        }
                    }

                    // We've made our escape plans. Don't continue iterating through bullet calculations...
                    break;
                }

                // If any of the danger points are adjacent to your current point, 
                // prevent moving into the bullet by add a large negative value to the move, 
                // with the size of the value depending on the slack...
                foreach (TankActionSituation tankActionSituation in tankSituation.TankActionSituationsPerTankAction)
                {
                    if (bulletCalc.TankCentrePointsThatDie.ContainsPoint(tankActionSituation.NewTankState.Pos))
                    {
                        // Calculate the slack values and tank action value adjustment...
                        double value = ScenarioValueFunctions.AvoidWalkingIntoABulletFunction.Evaluate(ticksToEscape);
                        tankActionSituation.Value += value;
                    }
                }
            }

            if (isScenarioApplicableForThisBulletSituation)
            {
                if (bestSlackForThisBulletSituation > worstSlack)
                {
                    worstSlack = bestSlackForThisBulletSituation;
                    tankActionToAddressWorstSlack = bestTankActionForThisBulletSituation;
                }
                isScenarioApplicable = true;
            }
        }

        private void AddBulletSurvivalTactic(Move move, int ticksUntil_p_i_CanFireAgain, int ticksToEscape,
            List<BulletSurvivalTactic> bulletTactics, Point survivalPoint, Direction dir, 
            int tickOffsetWhenTankCanFireAgain, bool isConfrontingBullet)
        {
            MobileState survivalState = new MobileState(survivalPoint, dir, isActive: true);
            AddBulletSurvivalTactic(move, ticksUntil_p_i_CanFireAgain, ticksToEscape, 
                bulletTactics, survivalState, tickOffsetWhenTankCanFireAgain, isConfrontingBullet);
        }

        private void AddBulletSurvivalTactic(Move move, int ticksUntil_p_i_CanFireAgain, int ticksToEscape, 
            List<BulletSurvivalTactic> bulletTactics, MobileState survivalState,
            int tickOffsetWhenTankCanFireAgain, bool isConfrontingBullet)
        {
            Cell survivalCell = TurnCalculationCache.CellMatrix[survivalState.Pos];
            if (!survivalCell.IsValid)
            {
                return;
            }

            TankLocation tankLocAtSurvivalState = TurnCalculationCache.TankLocationMatrix[survivalState.Pos];
            if (!tankLocAtSurvivalState.IsValid)
            {
                return;
            }

            int distanceToSurvivalState;
            TankAction[] tankActions;
            if (ticksUntil_p_i_CanFireAgain > 0)
            {
                Rectangle tankBody = TurnCalculationCache.TankLocationMatrix[survivalState.Pos].TankBody;
                int expansionFactor = Math.Min(DEFAULT_EXPANSION, ticksToEscape);
                Rectangle restrictedCalculationArea = tankBody.Merge(survivalState.Pos.ToPointRectangle()).Expand(expansionFactor);
                DirectionalMatrix<DistanceCalculation> customDistanceMatrix = base.GetCustomDistanceMatrixFromTank(
                    move.p, move.i, ticksUntil_p_i_CanFireAgain + 1, restrictedCalculationArea);
                distanceToSurvivalState = customDistanceMatrix[survivalState].Distance;
                tankActions = GetTankActionsToMoveToPointUsingCustomDistanceMatrix(
                    customDistanceMatrix, survivalState.Dir, survivalState.Pos);
            }
            else
            {
                distanceToSurvivalState = GetDistanceFromTankToPoint(move.p, move.i, survivalState);
                tankActions = GetTankActionsToMoveToPoint(move.p, move.i, survivalState.Dir, survivalState.Pos);
            }
            if (distanceToSurvivalState != Constants.UNREACHABLE_DISTANCE)
            {
                TankAction initialTankAction
                    = tankActions != null && tankActions.Length > 0
                    ? tankActions[0]
                    : (isConfrontingBullet && ticksToEscape <= 1
                        ? TankAction.FIRE // Assume we are already in position to shoot the bullet
                        : TankAction.NONE
                      );
                int escapeSlack
                    = distanceToSurvivalState - ticksToEscape
                    + (isConfrontingBullet ? 1 : 0);  // if confronting, then an extra turn is required to fire
                double escapeValue = ScenarioValueFunctions.DodgeBulletFunction.Evaluate(escapeSlack);
                int disarmamentSlack
                    = isConfrontingBullet || escapeSlack > 0
                    ? -ticksToEscape
                    : 1 - tickOffsetWhenTankCanFireAgain;
                double disarmamentValue = ScenarioValueFunctions.ProlongEnemyDisarmamentFunction.Evaluate(disarmamentSlack);
                double combinedValue = escapeValue + disarmamentValue;

                bulletTactics.Add(
                    new BulletSurvivalTactic
                    {
                        TargetState = survivalState,
                        TicksToEscape = ticksToEscape,
                        Slack = escapeSlack,
                        InitialTankAction = initialTankAction,
                        Value = combinedValue
                    });
            }
        }

        public override void ChooseMovesAsP(MoveResult moveResult)
        {
            // Do nothing - we've already set the values in the main method
        }

        public override void ChooseMovesAsPBar(MoveResult moveResult)
        {
            // Do nothing - this won't be called anyway
        }
    }
}
