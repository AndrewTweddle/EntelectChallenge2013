using System;
using System.Collections.Generic;
using System.Text;

namespace AndrewTweddle.ORToolkit.DynamicProgramming
{
    /* DynamicProgram extends DynamicProgramBase by providing
     * public delegates which can be set up to solve a problem.
     * This allows solutions to be created very simply and quickly.
     */
    public class DynamicProgram<TState, TDecision>
        : DynamicProgramBase<TState, TDecision>
    {
        /* The decision iterator is used to produce all possible initialSolutionNodes
         * which can be made when the system is in a given state:
         */
        public delegate IEnumerable<TDecision> DecisionIterator(TState state);

        /* The stopping function indicates when to 
         * stop recursion in a particular branch.
         * 
         * There are 2 reasons why a branch may be stopped:
         * 1. Because the branch no longer represents a feasible solution
         * 2. Because no more value can be extracted from the solution
         *    (i.e. the solution is complete and valid).
         * 
         */
        public delegate BranchStatus StoppingFunction(TState state, int stage);

        /* The Value function is the Value arising from 
         * a particular decision (but just the contribution 
         * of that decision, not all subsequent initialSolutionNodes).
         * The state is the state before the decision is made.
         */
        public delegate double ValueFunction(TState priorState, 
            TDecision decision, int stage);

        /* The TransitionFunction should generate a new state,
         * not just modify the existing one.
         * i.e. TState should be treated as an immutable class.
         */
        public delegate TState TransitionFunction(
            TState state, TDecision decision, int stage);

        private StoppingFunction stoppingTest;
        private DecisionIterator decisionGenerator;
        private ValueFunction valueCalculator;
        private TransitionFunction stateTransformation;

        public StoppingFunction StoppingTest
        {
            get { return stoppingTest; }
            set { stoppingTest = value; }
        }

        public DecisionIterator DecisionGenerator
        {
            get { return decisionGenerator; }
            set { decisionGenerator = value; }
        }

        public ValueFunction ValueCalculator
        {
            get { return valueCalculator; }
            set { valueCalculator = value; }
        }

        public TransitionFunction StateTransformation
        {
            get { return stateTransformation; }
            set { stateTransformation = value; }
        }

        public SolutionNodeStorageType SolutionStorageType
        {
            get { return GetSolutionStorageType(); }
            set { SetSolutionStorageType(value); }
        }

        #region Overridden protected members of the DynamicProgramBase class
        protected override IEnumerable<TDecision> GenerateDecisions(
            TState state)
        {
            return DecisionGenerator(state);
        }

        protected override BranchStatus GetBranchStatus(TState state, int stage)
        {
            if (StoppingTest == null)
            {
                return BranchStatus.Incomplete;
            }
            else
            {
                return StoppingTest(state, stage);
            }
        }

        protected override double GetDecisionValue(TState priorState,
            TDecision decision, int stage)
        {
            if (ValueCalculator == null)
            {
                return 0.0;
            }
            else
            {
                return ValueCalculator(priorState, decision, stage);
            }
        }

        protected override TState GenerateNewState(
            TState state, TDecision decision, int stage)
        {
            return StateTransformation(state, decision, stage);
        }

        #endregion

        public override SolutionSet<TState, TDecision> 
            Solve(TState initialState)
        {
            if (DecisionGenerator == null)
            {
                throw new UnimplementedDelegateException(
                    "No decision generator has been specified");
            }

            if (StateTransformation == null)
            {
                throw new UnimplementedDelegateException(
                    "No transition function has been specified");
            }

            return base.Solve(initialState);
        }
    }
}
