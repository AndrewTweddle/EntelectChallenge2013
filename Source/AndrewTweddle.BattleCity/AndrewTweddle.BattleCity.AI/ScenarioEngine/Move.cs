using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
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
        public Direction dir2 { get; set; }
        public Direction dir3 { get; set; }

        public Move()
        {
            p = 0;
            pBar = 1;
            i = 0;
            iBar = 1;
            j = 0;
            jBar = 1;
            dir1 = Direction.NONE;
            dir2 = Direction.NONE;
            dir3 = Direction.NONE;
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
                dir1 = this.dir1,
                dir2 = this.dir2,
                dir3 = this.dir3
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
            if (childMoveLevel == moveGenerators.Length)
            {
                // At the leaf level, so evaluate the scenario:
                return scenario.EvaluateLeafNodeMove(this);
            }

            MoveGenerator moveGen = moveGenerators[childMoveLevel];
            Move[] childMoves = moveGen.Generate(scenario, parentMove: this);

            int bestSlackForDecisionMaker;
            MoveResult bestResultForDecisionMaker = null;
            bestSlackForDecisionMaker = int.MaxValue;
            foreach (Move childMove in childMoves)
            {
                if (childMove == null)
                {
                    continue;
                }
                MoveResult childMoveResult = childMove.ExpandAndEvaluate(scenario, childMoveLevel + 1);
                
                if (childMoveResult == null)
                {
                    continue;
                }

                if (childMoveResult.EvaluationOutcome != ScenarioEvaluationOutcome.Invalid)
                {
                    if ( // Player p wants to minimize slack:
                        ((moveGen.DecisionMaker == ScenarioDecisionMaker.p) && (childMoveResult.Slack < bestSlackForDecisionMaker))
                        // Player pBar wants to maximize slack:
                        || (moveGen.DecisionMaker == ScenarioDecisionMaker.pBar) && (childMoveResult.Slack > bestSlackForDecisionMaker))
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

            return bestResultForDecisionMaker;
        }
    }
}
