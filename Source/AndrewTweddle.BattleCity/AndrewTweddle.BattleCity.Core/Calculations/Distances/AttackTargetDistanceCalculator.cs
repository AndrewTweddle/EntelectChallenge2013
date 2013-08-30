using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public static class AttackTargetDistanceCalculator
    {
        private const int SUGGESTED_CIRCULAR_BUFFER_CAPACITY_REQUIRED = 1024;

        public static DirectionalMatrix<DistanceCalculation> CalculateShortestDistancesToTargetPoint(
            Cell target, TurnCalculationCache turnCalcCache, GameStateCalculationCache gameStateCache,
            bool isBase, int circularBufferCapacityRequired = SUGGESTED_CIRCULAR_BUFFER_CAPACITY_REQUIRED)
        {
            BitMatrix walls = gameStateCache.GameState.Walls;
            DirectionalMatrix<DistanceCalculation> attackMatrix 
                = new DirectionalMatrix<DistanceCalculation>(walls.Width, walls.Height);

            TwoValuedCircularBuffer<Node> bfsQueue = new TwoValuedCircularBuffer<Node>(circularBufferCapacityRequired);

            // Note: the target point will not have a distance, since it will be shot, not moved to:
            TankLocation tankLoc = turnCalcCache.TankLocationMatrix[target.Position];
            if (isBase)
            {
                // If the target point is a base, then calculate the tank positions that will destroy it via movement:
                // Get the distances to move onto the base from various directions:
                foreach (Direction dir in BoardHelper.AllRealDirections)
                {
                    Direction oppositeDir = dir.GetOpposite();

                    // Get the inside edge (segment) of the tank centred at the base in the opposite direction:
                    Segment tankPositionsInDirection = tankLoc.InsideEdgesByDirection[(int)oppositeDir];

                    // For each point on the segment add it to the bfs queue with a distance of zero:
                    foreach (Cell tankCellInDir in tankPositionsInDirection.Cells)
                    {
                        if (tankCellInDir.IsValid)
                        {
                            Node node = new Node(ActionType.Moving, oppositeDir, tankCellInDir.Position);
                            bfsQueue.Add(node, 0);
                        }
                    }
                }

                // Give a distance of zero for internal points, but don't add them to the queue:
                foreach (Point interiorPoint in tankLoc.InteriorOfTankBody.GetPoints())
                {
                    foreach (Direction dir in BoardHelper.AllRealDirections)
                    {
                        DistanceCalculation distCalc = new DistanceCalculation(0, new Node());
                        attackMatrix[dir, interiorPoint] = distCalc;
                    }
                }
            }
            else
            {
                // If it's not a base, then all points within a tank centred at that point will be unreachable:

                // TODO: Pass in an array of the types of points that firing distances will be used for:
                // a. Centre of a tank edge
                // b. Offset points on a tank edge (1 away from centre)
                // c. Corner points on a tank edge
                throw new NotImplementedException("Firing distances with tank targets are not yet supported");
            }

            // Use the firing distance calculations for the target point, and add the initial points to the BFS queue:
            bool areFiringLinesStillActive = false;
            FiringLineSummary[] firingLineSummariesByMovementDir = new FiringLineSummary[Constants.RELEVANT_DIRECTION_COUNT];
            foreach (Direction movementDir in BoardHelper.AllRealDirections)
            {
                Line<FiringDistance> firingLine 
                    = gameStateCache.GetFiringDistancesToPointViaDirectionOfMovement(target.Position, movementDir);
                FiringLineSummary firingLineSummary = new FiringLineSummary
                {
                    FiringLine = firingLine
                };
                firingLineSummariesByMovementDir[(int) movementDir] = firingLineSummary;

                if (firingLine.Length == 0)
                {
                    firingLineSummary.IndexOfNextFiringLinePoint = -1;
                    firingLineSummary.NextEdgeWeighting = Constants.UNREACHABLE_DISTANCE;
                }
                else
                {
                    areFiringLinesStillActive = true;
                    firingLineSummary.IndexOfNextFiringLinePoint = 0;
                    firingLineSummary.NextEdgeWeighting = firingLine[0].TicksTillTargetShot - 1;
                    // Note that we add these initial firing line nodes with a distance of zero.
                    // This is because we need an edge of length 1 from the movement node at the tank's position to the firing line node.
                    // So we have to subtract 1 from the firing line node's own distance to compensate for this.
                    
                    // Add the firing line points to the queue:
                    if (firingLineSummary.NextEdgeWeighting == 0)
                    {
                        AddNextFiringLineNodesToQueueForMovementDir(bfsQueue, movementDir, firingLineSummary, distanceToAdd: 0);
                    }
                }
            }

            int currDistance = 0;
            while (true)
            {
                if (areFiringLinesStillActive && bfsQueue.Size == 0)
                {
                    // Get nodes from the firing line/s with the next shortest distance:
                    areFiringLinesStillActive = TryAddFiringLineNodesWithNextShortestDistance(
                        bfsQueue, firingLineSummariesByMovementDir, out currDistance);
                }
                if (bfsQueue.Size == 0)
                {
                    break;
                }

                CircularBufferItem<Node> bufferItem = bfsQueue.Remove();
                Node currNode = bufferItem.Item;
                if (bufferItem.Value > currDistance)
                {
                    currDistance = bufferItem.Value;

                    // Add firing line nodes with the new distance to the queue:
                    if (areFiringLinesStillActive)
                    {
                        areFiringLinesStillActive = TryAddNextFiringLineNodesToQueue(bfsQueue, firingLineSummariesByMovementDir, currDistance);
                    }
                }

                int adjDistance = currDistance + 1;
                
                // Get each node adjacent to the current node:
                SegmentState innerEdgeStateInNodeDir = gameStateCache.TankInnerEdgeMatrix[currNode.X, currNode.Y][(int) currNode.Dir];
                SegmentState[] outerEdgeStates = gameStateCache.TankInnerEdgeMatrix[currNode.X, currNode.Y];
                SegmentState outerEdgeStateInNodeDir = outerEdgeStates[(int) currNode.Dir];
                SegmentState outerEdgeStateInOppositeDir = outerEdgeStates[(int)(currNode.Dir.GetOpposite())];

                Node[] adjacentNodes = currNode.GetAdjacentIncomingNodes(
                    innerEdgeStateInNodeDir, outerEdgeStateInNodeDir, outerEdgeStateInOppositeDir);
                foreach (Node adj in adjacentNodes)
                {
                    // Note: adj will never be a firing line node, as they are not incoming nodes for any other node type.

                    if (adj.ActionType == ActionType.Moving) 
                    {
                        // Check if the node already has a distance i.e. has it already been expanded?
                        if (attackMatrix[adj.Dir, adj.X, adj.Y].CodedDistance == 0)
                        {
                            // Set the new shortest distance:
                            attackMatrix[adj.Dir, adj.X, adj.Y] = new DistanceCalculation(adjDistance, currNode);

                            // Add to the queue to be expanded:
                            bfsQueue.Add(adj, adjDistance);
                        }
                    }
                    else
                    {
                        // It would be useful to have the firing nodes as well, as a tank could currently be firing.
                        // However this will double the storage requirements.
                        // Rather just do quick calculations to estimate the distance.
                        // For example:
                        // 1. Get the shortest distance from the current node.
                        // 2. If firing is on the shortest distance from the current node, then subtract 1 from the distance (since it's already firing).
                        // 3. If it's not on the shortest distance (but it still might be a shortest path, due to multiple equal paths), 
                        //    then get one plus the shortest distance from the adjacent node in the firing direction.
                        //    a. If this is less than the shortest distance, use it as the shortest distance.
                        //    b. Otherwise use the shortest distance path instead (don't make use of the space fired at).
                        bfsQueue.Add(adj, adjDistance);
                    }
                }
            }
            return attackMatrix;
        }

        private static bool TryAddFiringLineNodesWithNextShortestDistance(
            TwoValuedCircularBuffer<Node> bfsQueue, FiringLineSummary[] firingLineSummariesByMovementDir, 
            out int newDistance)
        {
            bool canAddMoreNodes = false;
            int nextShortestDistance = Constants.UNREACHABLE_DISTANCE;
            foreach (Direction dir in BoardHelper.AllRealDirections)
            {
                FiringLineSummary firingLineSummary = firingLineSummariesByMovementDir[(int)dir];
                if (firingLineSummary.NextEdgeWeighting < nextShortestDistance)
                {
                    nextShortestDistance = firingLineSummary.NextEdgeWeighting;
                    canAddMoreNodes = true;
                }
            }
            if (!canAddMoreNodes)
            {
                newDistance = Constants.UNREACHABLE_DISTANCE;
                return false;
            }
            newDistance = nextShortestDistance;
            return TryAddNextFiringLineNodesToQueue(bfsQueue, firingLineSummariesByMovementDir, newDistance);
        }

        private static bool TryAddNextFiringLineNodesToQueue(
            TwoValuedCircularBuffer<Node> bfsQueue, FiringLineSummary[] firingLineSummarys, int distanceToAdd)
        {
            bool anyAdded = false;
            foreach (Direction dir in BoardHelper.AllRealDirections)
            {
                FiringLineSummary firingLineSummary = firingLineSummarys[(int) dir];
                if (firingLineSummary.NextEdgeWeighting == distanceToAdd)
                {
                    AddNextFiringLineNodesToQueueForMovementDir(bfsQueue, dir, firingLineSummary, distanceToAdd);
                    anyAdded = true;
                }
            }
            return anyAdded;
        }

        private static void AddNextFiringLineNodesToQueueForMovementDir(
            TwoValuedCircularBuffer<Node> bfsQueue, Direction movementDir, FiringLineSummary firingLineSummary, int distanceToAdd)
        {
            Line<FiringDistance> firingLine = firingLineSummary.FiringLine;
            int nextFiringLineIndex;
            for (nextFiringLineIndex = firingLineSummary.IndexOfNextFiringLinePoint;
                nextFiringLineIndex < firingLine.Length;
                nextFiringLineIndex++)
            {
                FiringDistance firingDist = firingLine[nextFiringLineIndex];
                int nextEdgeWeighting = firingDist.TicksTillTargetShot - 1;  
                    // This distance is the edge weight in the graph. 
                    // The -1 is because we need to compensate for the weight 1 edge added from 
                    // the tank's position + direction on the firing line, to the firing line node.

                if (nextEdgeWeighting > distanceToAdd)
                {
                    firingLineSummary.NextEdgeWeighting = nextEdgeWeighting;
                    firingLineSummary.IndexOfNextFiringLinePoint = nextFiringLineIndex;
                    break;
                }
                Node firingLineNode = new Node(ActionType.FiringLine, movementDir, firingDist.StartingTankPosition);
                bfsQueue.Add(firingLineNode, nextEdgeWeighting);
            }
            if (nextFiringLineIndex == firingLine.Length)
            {
                firingLineSummary.NextEdgeWeighting = Constants.UNREACHABLE_DISTANCE;
                firingLineSummary.IndexOfNextFiringLinePoint = -1;
            }
        }

    }
}