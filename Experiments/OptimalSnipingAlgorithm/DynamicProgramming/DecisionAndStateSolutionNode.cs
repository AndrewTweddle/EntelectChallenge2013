using System;
using System.Collections.Generic;
using System.Text;

namespace AndrewTweddle.ORToolkit.DynamicProgramming
{
    public class DecisionAndStateSolutionNode<TState, TDecision>
        : SolutionNode<TState, TDecision>
    {
        TState postDecisionState;
        TDecision decisionChosen;

        protected override TState GetPostDecisionState()
        {
            return postDecisionState;
        }

        protected override TDecision GetDecisionChosen()
        {
            return decisionChosen;
        }

        public DecisionAndStateSolutionNode(TState state,
            TDecision decision, double contribution,
            List<SolutionNode<TState, TDecision>> nextSolutionNodes)
            : base(contribution, nextSolutionNodes)
        {
            this.postDecisionState = state;
            this.decisionChosen = decision;
        }

    }
}
