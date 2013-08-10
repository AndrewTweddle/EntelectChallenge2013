﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class DistanceCalculator
    {
        public static short GetMovingDistanceToAdjacentCell(
            Direction currentDir, Direction movementDir, SegmentState outsideLeadingEdgeSegmentState)
        {
            switch (outsideLeadingEdgeSegmentState)
            {
                case SegmentState.Clear:
                    return 1;  // Move directly into the space
                case SegmentState.ShootableWall:
                    if (currentDir == movementDir)
                    {
                        return 2;  // Shoot wall, then move into space
                    }
                    else
                    {
                        return 3;  // Move in direction to turn, then shoot wall, then move into space
                    }
                default:
                    return Constants.UNREACHABLE_DISTANCE;
            }
        }

        public static short GetSnipingDistanceAdjustment(
            Direction currentDir, Direction shootingDir, SegmentState outsideLeadingEdgeSegmentState)
        {
            switch (outsideLeadingEdgeSegmentState)
            {
                case SegmentState.Clear:
                    return 0;
                case SegmentState.OutOfBounds:
                    return Constants.UNREACHABLE_DISTANCE;
                default:
                    if (currentDir == shootingDir)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;  // Tank must change direction by moving in that direction first
                    }
            }
        }
    }
}
