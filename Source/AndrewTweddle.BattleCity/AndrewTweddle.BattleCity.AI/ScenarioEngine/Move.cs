using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;

namespace AndrewTweddle.BattleCity.AI.Scenarios
{
    public class Move
    {
        public Move ParentMove { get; set; }

        /// <summary>
        /// The index of the player
        /// </summary>
        public int p { get; set; }

        /// <summary>
        /// The index of the other player
        /// </summary>
        public int pBar { get; set; }

        /// <summary>
        /// The index of player p's tank with the primary role
        /// </summary>
        public int i { get; set; }

        /// <summary>
        /// The index of player p's other tank
        /// </summary>
        public int iBar { get; set; }

        /// <summary>
        /// The index of player pBar's tank with the primary role
        /// </summary>
        public int j { get; set; }

        /// <summary>
        /// The index of player pBar's other tank
        /// </summary>
        public int jBar { get; set; }

        public Direction dir1 { get; set; }

        public Move()
        {
            p = 0;
            pBar = 1;
            i = 0;
            iBar = 1;
            j = 0;
            jBar = 1;
            dir1 = Direction.NONE;
        }

        public Move(Move parentMove): this()
        {
            ParentMove = parentMove;
        }

        public Move Clone()
        {
            Move move = new Move(ParentMove)
            {
                p = this.p,
                pBar = this.pBar,
                i = this.i,
                iBar = this.iBar,
                j = this.j,
                jBar = this.jBar,
                dir1 = this.dir1
            };
            return move;
        }

        public Move CloneAsChild()
        {
            Move childMove = Clone();
            childMove.ParentMove = this;
            return childMove;
        }


        /// <summary>
        /// Recursively expand the current move, or evaluate it if it is at the leaf level
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="childMoveLevel"></param>
        /// <returns>Either null if no suitable decision could be found, or the best result for the decision maker at this level</returns>
        public MoveResult ExpandAndEvaluate(Scenario scenario, int childMoveLevel)
        {
            MoveGenerator[] moveGenerators = scenario.GetMoveGeneratorsByMoveTreeLevel();
            MoveGenerator moveGen = moveGenerators[childMoveLevel];
            int slackMultiplier
                = moveGen.DecisionMaker == ScenarioDecisionMaker.p
                ? 1
                : -1;

            if (childMoveLevel == moveGenerators.Length)
            {
                // At the leaf level, so evaluate the scenario:
                return scenario.EvaluateLeafNodeMove(this);
            }

            Move[] childMoves = moveGen.Generate(scenario, parentMove: this);

            int bestSlackForDecisionMaker;
            MoveResult bestResultForDecisionMaker = null;

            switch (moveGen.DecisionMaker)
            {
                case ScenarioDecisionMaker.p:
                    // Player p wants to minimize slack:
                    bestSlackForDecisionMaker = int.MaxValue;
                    foreach (Move childMove in childMoves)
                    {
                        MoveResult childMoveResult = childMove.ExpandAndEvaluate(scenario, childMoveLevel + 1);

                        if (childMoveResult != null)
                        {
                            if (childMoveResult.EvaluationOutcome != ScenarioEvaluationOutcome.Invalid)
                            {
                                if (childMoveResult.Slack < bestSlackForDecisionMaker)
                                {
                                    bestResultForDecisionMaker = childMoveResult;
                                    bestSlackForDecisionMaker = childMoveResult.Slack;
                                }
                            }
                            else
                            {
                                // Return an invalid result rather than null if possible:
                                if (bestResultForDecisionMaker == null)
                                {
                                    bestResultForDecisionMaker = childMoveResult;
                                }
                            }
                        }
                    }
                    break;

                case ScenarioDecisionMaker.pBar:
                    // Player pBar wants to maximize slack:
                    bestSlackForDecisionMaker = int.MinValue;
                    foreach (Move childMove in childMoves)
                    {
                        MoveResult childMoveResult = childMove.ExpandAndEvaluate(scenario, childMoveLevel + 1);

                        if (childMoveResult != null)
                        {
                            if (childMoveResult.EvaluationOutcome != ScenarioEvaluationOutcome.Invalid)
                            {
                                if (childMoveResult.Slack > bestSlackForDecisionMaker)
                                {
                                    bestResultForDecisionMaker = childMoveResult;
                                    bestSlackForDecisionMaker = childMoveResult.Slack;
                                }
                            }
                            else
                            {
                                // Return an invalid result rather than null if possible:
                                if (bestResultForDecisionMaker == null)
                                {
                                    bestResultForDecisionMaker = childMoveResult;
                                }
                            }
                        }
                    }
                    break;
            }

            return bestResultForDecisionMaker;
        }
    }
}
