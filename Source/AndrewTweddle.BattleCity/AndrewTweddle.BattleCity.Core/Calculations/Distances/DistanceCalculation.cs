﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public struct DistanceCalculation
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
                if (CodedDistance <= 0)
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

        public Node AdjacentNode { get; private set; }

        public DistanceCalculation(int distance, Node adjacentNode)
            : this()
        {
            Distance = distance;
            AdjacentNode = adjacentNode;
        }

        public override string ToString()
        {
            return String.Format("Distance {0} with adjacent node {1}", Distance, AdjacentNode);
        }
    }
}
