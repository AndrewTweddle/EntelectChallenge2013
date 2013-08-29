using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public struct MultiPathDistanceCalculation
    {
        /// <summary>
        /// CodedDistance is 0 for a node which has not yet been calculated or which is unreachable.
        /// This is the highest performance way of checking whether a node is assigned yet or not.
        /// Otherwise CodedDistance is 1 more than the actual distance.
        /// </summary>
        public short CodedDistance { get; private set; }

        /// <summary>
        /// Distance encodes and decodes the value in CodedDistance
        /// </summary>
        public int Distance 
        {
            get
            {
                if (CodedDistance == 0)
                {
                    return Constants.UNREACHABLE_DISTANCE;
                }
                else
                {
                    return CodedDistance - 1;
                }
            }
            private set
            {
                CodedDistance = (short) (value + 1);
            }
        }

        public Node[] PriorNodesByPriorDir { get; private set; }

        public MultiPathDistanceCalculation(int distance, Node priorNode)
            : this()
        {
            Distance = distance;
            PriorNodesByPriorDir = new Node[Constants.RELEVANT_DIRECTION_COUNT];
            PriorNodesByPriorDir[(int) priorNode.Dir] = priorNode;
        }
    }
}
