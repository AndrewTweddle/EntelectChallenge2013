﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class DistanceCalculation
    {
        public short MinDistance { get; set; }
        public Direction DirectionTakenToGetHere { get; set; }

        public short[] DistanceViaDirectionTaken { get; private set; }
        /* Sometimes the presence of a wall can add an extra step for turning (as well as the extra step for shooting at the wall).
         * So store the distances via each direction, as the incoming and outgoing direction can be significant.
         */

        public DistanceCalculation()
        {
            DirectionTakenToGetHere = Direction.NONE;  // Signals no calculation yet (although so does a distance of zero)
            MinDistance = Constants.UNREACHABLE_DISTANCE;
            DistanceViaDirectionTaken = new short[Constants.RELEVANT_DIRECTION_COUNT];
            for (int i = 0; i < DistanceViaDirectionTaken.Length; i++)
            {
                DistanceViaDirectionTaken[i] = Constants.UNREACHABLE_DISTANCE;
            }
        }

        /* TODO: Optional storage of actions taken to get from previous space...
         * 
         * Calculate short CellAdjacentStatusId as tank's SegmentStatus[Up] << 8 | SegmentStatus[Down] << 6 | ... | Direction
         * 
         */
    }
}
