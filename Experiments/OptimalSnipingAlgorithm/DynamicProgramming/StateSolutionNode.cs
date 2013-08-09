using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AndrewTweddle.ORToolkit.DynamicProgramming
{
    public class StateSolutionNode<TState, TDecision>
        : SolutionNode<TState, TDecision>
    {
        private TState postDecisionState;

        protected override TDecision GetDecisionChosen()
        {
            throw new SolutionNodeStorageTypeException(
                "The decision is not being stored in the solution node.");
        }

        protected override TState GetPostDecisionState()
        {
            return postDecisionState;
        }

        public StateSolutionNode(TState postDecisionState,
            double contribution, 
            List<SolutionNode<TState, TDecision>> nextSolutionNodes)
            : base(contribution, nextSolutionNodes)
        {
            this.postDecisionState = postDecisionState;
        }
    }
}
