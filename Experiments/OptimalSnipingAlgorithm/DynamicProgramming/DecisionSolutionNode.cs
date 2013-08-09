using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AndrewTweddle.ORToolkit.DynamicProgramming
{
    public class DecisionSolutionNode<TState, TDecision>
        : SolutionNode<TState, TDecision>
    {
        private TDecision decisionChosen;

        protected override TDecision GetDecisionChosen()
        {
            return decisionChosen;
        }

        protected override TState GetPostDecisionState()
        {
            throw new SolutionNodeStorageTypeException(
                "The state is not being stored in the solution node.");
        }

        public DecisionSolutionNode(TDecision decisionChosen, 
            double contribution, 
            List<SolutionNode<TState, TDecision>> nextSolutionNodes)
            : base(contribution, nextSolutionNodes)
        {
            this.decisionChosen = decisionChosen;
        }
    }
}
