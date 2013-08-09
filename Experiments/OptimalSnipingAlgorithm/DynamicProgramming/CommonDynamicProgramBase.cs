using System;
using System.Collections.Generic;
using System.Text;

namespace AndrewTweddle.ORToolkit.DynamicProgramming
{
    public class CommonDynamicProgramBase
    {
        public enum BranchStatus
        {
            Infeasible, /* The solution branch is infeasible */
            Incomplete, /* Continue recursing through solutions */
            Complete    /* The branch represents a feasible solution, 
                         * but should not be expanded further.
                         */
        }

        public enum SolutionNodeStorageType
        {
            DecisionAndState,
            DecisionOnly,
            PostDecisionStateOnly
        }
    }
}
