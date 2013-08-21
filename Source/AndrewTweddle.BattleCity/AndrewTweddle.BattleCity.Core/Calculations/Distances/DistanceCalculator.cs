using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public static class DistanceCalculator
    {
        private const int MAX_CIRCULAR_BUFFER_CAPACITY_REQUIRED = 1024;

        public static DirectionalMatrix<DistanceCalculation> CalculateShortestDistancesFromTank(
            MobileState tankState, BitMatrix walls,
            Matrix<SegmentState[]> tankEdgeMatrix)
        {
            DirectionalMatrix<DistanceCalculation> distanceMatrix 
                = new DirectionalMatrix<DistanceCalculation>(walls.Width, walls.Height);

            distanceMatrix[tankState.Dir, tankState.Pos] = new DistanceCalculation(0, new Node());

            TwoValuedCircularBuffer<Node> bfsQueue = new TwoValuedCircularBuffer<Node>(MAX_CIRCULAR_BUFFER_CAPACITY_REQUIRED);

            Node currNode = new Node(ActionType.Moving, tankState.Dir, tankState.Pos);
            int adjDistance = 1;
            bool nodesToProcess = true;
            
            while (nodesToProcess)
            {
                // Get each node adjacent to the current node:
                Node[] adjacentNodes = currNode.GetAdjacentNodes(tankEdgeMatrix[currNode.X, currNode.Y]);
                foreach (Node adj in adjacentNodes)
                {
                    if (adj.ActionType == ActionType.Moving) 
                    {
                        // Check if the node already has a distance i.e. has it already been expanded?
                        if (distanceMatrix[adj.Dir, adj.X, adj.Y].CodedDistance == 0)
                        {
                            // Set the new shortest distance:
                            distanceMatrix[adj.Dir, adj.X, adj.Y] = new DistanceCalculation(adjDistance, currNode);

                            // Add to the queue to be expanded:
                            bfsQueue.Add(adj, adjDistance);
                        }
                    }
                    else
                    {
                        // A firing node can only be reached in one way (from the moving node on the same cell).
                        // So we don't need to check if it is the shortest path to the node (it must be).
                        // And we aren't interested in it, so no need to store it.
                        // Hence just add it to the queue (it is a "convenience" node to allow a BFS):
                        bfsQueue.Add(adj, adjDistance);
                    }
                }

                if (bfsQueue.Size == 0)
                {
                    nodesToProcess = false;
                }
                else
                {
                    CircularBufferItem<Node> nextItem = bfsQueue.Remove();
                    currNode = nextItem.Item;
                    adjDistance = nextItem.Value + 1;
                }
            }
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
