using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AndrewTweddle.ORToolkit.DynamicProgramming
{
    public abstract class SolutionNode<TState, TDecision> 
    {
        int solutionCount;
        double contribution;
        ReadOnlyCollection<SolutionNode<TState, TDecision>> 
            nextSolutionNodes;

        protected abstract TState GetPostDecisionState();
        protected abstract TDecision GetDecisionChosen();

        /* The contribution of this decision to the total value: */
        public double Contribution
        {
            get { return contribution; }
        }

        public int SolutionCount
        {
            get { return solutionCount; }
        }

        public TState PostDecisionState
        {
            get { return GetPostDecisionState(); }
        }

        public TDecision DecisionChosen
        {
            get { return GetDecisionChosen(); }
        }

        public ReadOnlyCollection<SolutionNode<TState, TDecision>> 
            NextSolutionNodes
        {
          get { return nextSolutionNodes; }
        }

        public SolutionNode(double contribution, 
            List<SolutionNode<TState, TDecision>> nextSolutionNodes)
        {
            this.contribution = contribution;

            if (nextSolutionNodes == null)
            {
                nextSolutionNodes = new List<SolutionNode<TState, TDecision>>();
            }

            this.nextSolutionNodes 
                = new ReadOnlyCollection<SolutionNode<TState, TDecision>>(
                    nextSolutionNodes.ToArray());

            /* If there are no subsequent initialSolutionNodes,
             * then just count the current node as a solution.
             */
            if (nextSolutionNodes.Count == 0)
            {
                solutionCount = 1;
            }
            else
            {
                solutionCount = 0;

                foreach (SolutionNode<TState, TDecision> solNode
                    in nextSolutionNodes)
                {
                    solutionCount += solNode.solutionCount;
                }
            }
        }

        public SolutionNode(double contribution)
            : this(contribution, null)
        { 
        }
    }
}
