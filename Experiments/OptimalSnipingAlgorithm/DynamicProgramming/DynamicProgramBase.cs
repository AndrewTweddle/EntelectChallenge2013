using System;
using System.Collections.Generic;
using System.Text;

namespace AndrewTweddle.ORToolkit.DynamicProgramming
{
    public abstract class DynamicProgramBase<TState, TDecision>
        : CommonDynamicProgramBase
    {
        private SolutionNodeStorageType solutionStorageType
            = SolutionNodeStorageType.DecisionAndState;

        /* The following pairs of methods can be used to add 
         * a SolutionStorageType property to a derived class, 
         * for customisation by the consumer of the class.
         * 
         * Alternatively, derived classes can call SetSolutionStorageType() 
         * to customize the storage type for the particular algorithm:
         */
        protected SolutionNodeStorageType GetSolutionStorageType()
        {
            return solutionStorageType;
        }

        protected void SetSolutionStorageType(SolutionNodeStorageType value)
        {
            solutionStorageType = value;
        }

        protected abstract IEnumerable<TDecision> GenerateDecisions(
            TState state);

        protected abstract BranchStatus GetBranchStatus(TState state, int stage);

        protected abstract double GetDecisionValue(TState priorState,
            TDecision decision, int stage);

        /* GenerateNewState() should create a new state
         * from the existing state.
         * NB: The new state can't just be a modified copy
         *     of the state passed to the method.
         *     i.e. The TState class should be treated as immutable.
         */
        protected abstract TState GenerateNewState(
            TState state, TDecision decision, int stage);

        protected SolutionNode<TState, TDecision> CreateSolutionNode(
            TState newState, TDecision decision, double contribution,
            List<SolutionNode<TState, TDecision>> nextSolutionNodes)
        {
            SolutionNode<TState, TDecision> newSolutionNode;

            switch (solutionStorageType)
            {
                case SolutionNodeStorageType.DecisionOnly:
                    newSolutionNode
                        = new DecisionSolutionNode<TState, TDecision>(
                            decision, contribution, nextSolutionNodes);
                    break;

                case SolutionNodeStorageType.PostDecisionStateOnly:
                    newSolutionNode
                        = new StateSolutionNode<TState, TDecision>(
                            newState, contribution, nextSolutionNodes);
                    break;

                default:
                    newSolutionNode 
                        = new DecisionAndStateSolutionNode<TState, TDecision>(
                            newState, decision, contribution, nextSolutionNodes);
                    break;
            }

            return newSolutionNode;
        }

        public virtual SolutionSet<TState, TDecision> Solve(
            TState initialState)
        {
            double bestValue;
            List<SolutionNode<TState, TDecision>> bestInitialSolutionNodes
                = new List<SolutionNode<TState, TDecision>>();
            BranchStatus status;

            GenerateSolutions(initialState, 1 /*first stage*/, 
                out status, out bestValue, bestInitialSolutionNodes);

            return new SolutionSet<TState, TDecision>(bestInitialSolutionNodes, 
                bestValue);
        }

        protected void GenerateSolutions(TState state, int stage, 
            out BranchStatus status, out double bestValue, 
            List<SolutionNode<TState, TDecision>> bestSolutionNodes)
        {
            bestValue = 0.0;

            /* Start by deciding whether it is worth continuing or not.
             * 
             * There are various reasons why it might be time to stop:
             *   1. There is no feasible solution in this branch.
             *   2. There is no more Value to be extracted from this branch.
             *      It is now a complete feasible solution.
             *   3. Some arbitrary (i.e. problem-specific) 
             *      stopping condition has been encountered.
             *   4. There are no more initialSolutionNodes which can be taken
             *      at this point. This is treated as a special case
             *      of reason 2, unless in the very first stage
             *      (to prevent completely decisionless solutions)!
             * 
             * The GetBranchStatus() function allows one of the first
             * three reasons to be tested for.
             */
            status = GetBranchStatus(state, stage);

            if ((status == BranchStatus.Infeasible)
                || (status == BranchStatus.Complete))
            {
                return;
            }

            /* Now search for an optimimal solution from this stage forward: 
             * 
             * NB: The contents of the block of the foreach loop 
             *     should ideally be refactored into a new method.
             *     I haven't done this because the recursive call is
             *     inside this block, and I don't want to double the
             *     depth of the call stack.
             */
            foreach (TDecision dec in GenerateDecisions(state))
            {
                TState newState = GenerateNewState(state, dec, stage);
                BranchStatus subBranchStatus;
                double value; 
                    /* Value of the sub-problem from this stage forwards */
                List<SolutionNode<TState, TDecision>> nextSolutionNodes
                    = new List<SolutionNode<TState,TDecision>>();

                GenerateSolutions(newState, stage + 1, out subBranchStatus,
                    out value, nextSolutionNodes);

                /* Skip initialSolutionNodes that lead to infeasible branches: */
                if (subBranchStatus == BranchStatus.Infeasible)
                    continue;

                /* Calculate the contribution of the current decision
                 * to the value of itself and its best sub-branch/es:
                 */
                double contribution = GetDecisionValue(state, dec, stage);
                value += contribution;

                /* Only consider this decision if it is the 
                 * first solution found, or if it is at least 
                 * as good as the best solution found:
                 */
                if ((bestSolutionNodes.Count == 0) || (value >= bestValue))
                {
                    SolutionNode<TState, TDecision> newSolutionNode 
                        = CreateSolutionNode(newState, dec, contribution,
                            nextSolutionNodes);

                    if (bestSolutionNodes.Count == 0)
                    {
                        bestValue = value;
                    }
                    else
                    {
                        if (value > bestValue)
                        {
                            bestSolutionNodes.Clear();
                            bestValue = value;
                        }
                    }

                    bestSolutionNodes.Add(newSolutionNode);
                }
            }

            if (bestSolutionNodes.Count == 0)
            {
                status = BranchStatus.Infeasible;
            }
            else
            {
                status = BranchStatus.Complete;
            }
        }
    }
}
