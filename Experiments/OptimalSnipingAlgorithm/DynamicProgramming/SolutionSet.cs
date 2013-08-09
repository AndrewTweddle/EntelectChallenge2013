using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AndrewTweddle.ORToolkit.DynamicProgramming
{
    public class SolutionSet<TState, TDecision>
    {
        #region private variables

        double value = 0.0;
        int solutionCount = 0;
        ReadOnlyCollection<SolutionNode<TState, TDecision>> 
            initialSolutionNodes;

        #endregion

        public double Value
        {
            get { return this.value; }
        }

        public int SolutionCount
        {
            get { return solutionCount; }
        }

        public bool IsFeasible
        {
            get { return solutionCount > 0; }
        }

        public ReadOnlyCollection<SolutionNode<TState, TDecision>> 
            InitialSolutionNodes
        {
            get { return initialSolutionNodes; }
        }

        /* Each solution within the solution set 
         * can be retrieved as an array of SolutionNodes:
         */
        public SolutionNode<TState, TDecision>[] this[int index]
        {
            get
            {
                return GetSolutionNodesForSolution(index);
            }
        }

        public SolutionNode<TState, TDecision>[] 
            GetSolutionNodesForSolution(int solutionIndex)
        {
            if ((solutionIndex < 0) || (solutionIndex >= SolutionCount))
            {
                throw new ArgumentOutOfRangeException();
            }

            List<SolutionNode<TState, TDecision>> solutionNodeList
                = new List<SolutionNode<TState, TDecision>>();

            /* Recursively build up the sequence of decision nodes
             * for the solution with the given index:
             */
            PopulateListFromSolutionNodes<SolutionNode<TState, TDecision>>(
                solutionIndex, solutionNodeList, initialSolutionNodes,
                delegate(SolutionNode<TState, TDecision> solNode)
                {
                    return solNode;
                });

            return solutionNodeList.ToArray();
        }

        public TDecision[] GetDecisionsForSolution(int solutionIndex)
        {
            if ((solutionIndex < 0) || (solutionIndex >= SolutionCount))
            {
                throw new ArgumentOutOfRangeException();
            }

            List<TDecision> decisionList = new List<TDecision>();

            /* Recursively build up the sequence of initialSolutionNodes
             * for the solution with the given index:
             */
            PopulateListFromSolutionNodes<TDecision>(solutionIndex,
                decisionList, initialSolutionNodes,
                delegate(SolutionNode<TState, TDecision> solNode)
                {
                    return solNode.DecisionChosen;
                });

            return decisionList.ToArray();
        }


        public TState[] GetStatesForSolution(int solutionIndex)
        {
            if ((solutionIndex < 0) || (solutionIndex >= SolutionCount))
            {
                throw new ArgumentOutOfRangeException();
            }

            List<TState> stateList = new List<TState>();

            /* Recursively build up the sequence of states
             * for the solution with the given index:
             */
            PopulateListFromSolutionNodes<TState>(solutionIndex,
                stateList, initialSolutionNodes, 
                delegate(SolutionNode<TState, TDecision> solNode)
                {
                    return solNode.PostDecisionState;
                });

            return stateList.ToArray();
        }

        /* Each of the 3 methods above used to have its own
         * GenerateIndexXXXSequence() method which it would call.
         * These were identical except for populating the generic list
         * with instances obtained from the decision nodes.
         * It was possible to replace these 3 with a single generic method 
         * (PopulateListFromSolutionNodes<TReturnType>, using the 
         * SolutionNodeConverter delegate defined below
         * to encapsulate the process of obtaining an instance
         * of the desired type from the current solution node:
         */
        private delegate TReturnType SolutionNodeConverter<TReturnType>(
            SolutionNode<TState, TDecision> solNode);

        private void PopulateListFromSolutionNodes<TReturnType>(
            int solutionIndex, List<TReturnType> sequence,
            ReadOnlyCollection<SolutionNode<TState, TDecision>> 
                solutionNodesAtStage, 
            SolutionNodeConverter<TReturnType> convert)
        {
            int offset = 0;
            int index = 0;

            foreach (SolutionNode<TState, TDecision> solNode 
                in solutionNodesAtStage)
            {
                if (solNode.SolutionCount > solutionIndex - offset)
                {
                    SolutionNode<TState, TDecision> nextSolNode
                        = solutionNodesAtStage[index];
                    
                    if (convert != null)
                    {
                        sequence.Add(convert(nextSolNode));
                    }

                    PopulateListFromSolutionNodes<TReturnType>(
                        solutionIndex - offset, sequence, 
                        nextSolNode.NextSolutionNodes, convert);

                    break;
                }
                else
                {
                    offset += solNode.SolutionCount;
                    index++;
                }
            }
        }

        public SolutionSet(
            List<SolutionNode<TState, TDecision>> initialSolutionNodes,
            double value)
        {
            solutionCount = 0;
            this.value = value;

            if (initialSolutionNodes == null)
            {
                /* No feasible solution: */
                initialSolutionNodes 
                    = new List<SolutionNode<TState, TDecision>>();
            }

            this.initialSolutionNodes
                = new ReadOnlyCollection<SolutionNode<TState, TDecision>>(
                    initialSolutionNodes.ToArray());

            foreach (SolutionNode<TState, TDecision> solNode 
                in initialSolutionNodes)
            {
                solutionCount += solNode.SolutionCount;
            }
        }

        public SolutionSet(
            List<SolutionNode<TState, TDecision>> initialSolutionNodes)
            : this(initialSolutionNodes, 0.0)
        {
        }

        public SolutionSet(): this(null)
        {
        }
    }
}
