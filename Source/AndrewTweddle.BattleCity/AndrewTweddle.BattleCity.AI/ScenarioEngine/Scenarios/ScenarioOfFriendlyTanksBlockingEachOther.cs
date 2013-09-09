using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Calculations;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.Scenarios
{
    public class ScenarioOfFriendlyTanksBlockingEachOther: Scenario
    {
        public int YourPlayerIndex { get; private set; }

        public ScenarioOfFriendlyTanksBlockingEachOther(
            GameState gameState, GameSituation gameSituation, int yourPlayerIndex): base(gameState, gameSituation)
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
            MobileState t_p_i = GetTankState_i(move);
            MobileState t_p_iBar = GetTankState_iBar(move);

            // Not valid if either tank is dead:
            if (!(t_p_i.IsActive && t_p_iBar.IsActive))
            {
                return new MoveResult(move)
                {
                    EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
                };
            }

            int slackX = Math.Abs(t_p_i.Pos.X - t_p_iBar.Pos.X);
            int slackY = Math.Abs(t_p_i.Pos.Y - t_p_iBar.Pos.Y);
            int slack = Math.Max(slackX, slackY);

            MoveResult moveResult = new MoveResult(move)
            {
                Slack = slack
            };
            if (slack > 5)
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Possible;    
            }
            else
                if (slack <= 2)
                {
                    moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Current;
                }
                else
                {
                    moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Close;
                }

            Direction dirTowards_iBar 
                = slackX <= slackY
                ? t_p_i.Pos.GetVerticalDirectionToPoint(t_p_iBar.Pos)
                : t_p_i.Pos.GetHorizontalDirectionToPoint(t_p_iBar.Pos);
            Direction dirAwayFrom_iBar = dirTowards_iBar.GetOpposite();

            TankSituation tankSit_i = GetTankSituation(move.p, move.i);
            TankSituation tankSit_iBar = GetTankSituation(move.p, move.iBar);
            double bestMoveValue_i = tankSit_i.GetBestTankActionValue();
            double bestMoveValue_iBar = tankSit_iBar.GetBestTankActionValue();

            bool does_i_have_best_move
                = (bestMoveValue_i == bestMoveValue_iBar)
                ? (move.i < move.iBar)
                : (bestMoveValue_i > bestMoveValue_iBar);
            
            /*
            if (does_i_have_best_move)
            {
                return new MoveResult(move)
                {
                    EvaluationOutcome = ScenarioEvaluationOutcome.Invalid
                };
            }
             */

            foreach (Direction dir in BoardHelper.AllRealDirections)
            {
                int slackOffset = 0;
                if (dir == dirTowards_iBar)
                {
                    slackOffset = -1;
                }
                if (dir == dirAwayFrom_iBar)
                {
                    slackOffset = 1;
                }
                Point point = t_p_i.Pos + dir.GetOffset();
                Cell cell = TurnCalculationCache.CellMatrix[point];
                TankAction[] tankActions = GetTankActionsToMoveToPoint(move.p, move.i, dir, point);
                if (tankActions.Length > 0)
                {
                    int adjustedSlack = slack + slackOffset;
                    double value = ScenarioValueFunctions.AvoidBlockingFriendlyTankFunction.Evaluate(adjustedSlack);
                    if (slack <= 8 && (dirTowards_iBar == Direction.LEFT || dirTowards_iBar == Direction.DOWN))
                    {
                        if (dir == dirTowards_iBar)
                        {
                            value -= 100000;
                        }
                        if (dir == dirAwayFrom_iBar)
                        {
                            value += 100000;
                        }
                    }
                    TankAction tankAction = tankActions[0];
                    tankSituation.TankActionSituationsPerTankAction[(int) tankAction].Value += value;
                }
            }

            return moveResult;
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
