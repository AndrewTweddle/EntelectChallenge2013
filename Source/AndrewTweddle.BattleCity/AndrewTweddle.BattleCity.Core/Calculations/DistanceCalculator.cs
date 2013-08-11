using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class DistanceCalculator
    {
        public Matrix<DistanceCalculation> CalculateShortestDistancesFromTank(MobileState tankState, BitMatrix walls,
            Matrix<SegmentState> horizontalSegmentStateMatrix = null, Matrix<SegmentState> verticalSegmentStateMatrix = null)
        {
            Matrix<DistanceCalculation> distanceMatrix = new Matrix<DistanceCalculation>(walls.Width, walls.Height);

            if (horizontalSegmentStateMatrix == null)
            {
                horizontalSegmentStateMatrix = walls.GetBoardSegmentStateMatrixForAxisOfMovement(Axis.Horizontal);
            }

            if (verticalSegmentStateMatrix == null)
            {
                verticalSegmentStateMatrix = walls.GetBoardSegmentStateMatrixForAxisOfMovement(Axis.Vertical);
            }

            // TODO: Dijkstra calculations

            return distanceMatrix;
        }


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
                    if (currentDir == shootingDir)
                    {
                        return 0;
                    }
                    else
                    {
                        // In position, but an attempt to change direction will cause the tank to move out of position
                        return Constants.UNREACHABLE_DISTANCE;
                    }
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
