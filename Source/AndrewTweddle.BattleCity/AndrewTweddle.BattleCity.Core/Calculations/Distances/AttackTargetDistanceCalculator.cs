﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public class AttackTargetDistanceCalculator
    {
        #region Constants

        private const int MAX_POSSIBLE_PRECEDING_NODE_COUNT = 7;
        private const int TABOO_DISTANCE = -2;

        #endregion

        #region Private Member Variables

        #endregion

        #region Public Properties

        public FiringLineMatrix FiringLineMatrix { get; set; }
        public GameStateCalculationCache GameStateCalculationCache { get; set; }
        public TurnCalculationCache TurnCalculationCache { get; set; }
        public ElementType TargetElementType { get; set; }
        public bool AllowDestroyingABulletByMovingIntoIt { get; set; }
        public int CircularBufferCapacityRequired { get; set; }
        public Rectangle[] TabooAreas { get; set; }

        /// <summary>
        /// This filter restricts the approach directions for shooting the target.
        /// It defaults to all directions.
        /// </summary>
        public Direction[] MovementDirections { get; set; }

        /// <summary>
        /// This filter restricts the edges of a tank which may be shot.
        /// It defaults to the edge offsets contained in the firing line matrix.
        /// </summary>
        public EdgeOffset[] EdgeOffsets { get; set; }

        #endregion

        #region Constructors

        protected AttackTargetDistanceCalculator()
        {
            AllowDestroyingABulletByMovingIntoIt = true;
            MovementDirections = BoardHelper.AllRealDirections;
            CircularBufferCapacityRequired = DistanceCalculationConstants.SUGGESTED_CIRCULAR_BUFFER_CAPACITY_REQUIRED;
        }

        /// <summary>
        /// AttackTargetDistanceCalculator constructor
        /// </summary>
        /// <param name="targetElementType">The type of element (base, tank or bullet) which is being attacked</param>
        /// <param name="firingLineMatrix">The firing line matrix must be applicable to the target element type's extent</param>
        /// <param name="gameStateCalculationCache"></param>
        /// <param name="turnCalculationCache"></param>
        public AttackTargetDistanceCalculator(
            ElementType targetElementType,
            FiringLineMatrix firingLineMatrix,
            GameStateCalculationCache gameStateCalculationCache,
            TurnCalculationCache turnCalculationCache): this()
        {
            FiringLineMatrix = firingLineMatrix;
            GameStateCalculationCache = gameStateCalculationCache;
            TurnCalculationCache = turnCalculationCache;
            TargetElementType = targetElementType;
        }

        #endregion

        #region Methods

        public DirectionalMatrix<DistanceCalculation> CalculateMatrixOfShortestDistancesToTargetCell(Cell target)
        {
            if (MovementDirections == null)
            {
                MovementDirections = BoardHelper.AllRealDirections;
            }
            if (EdgeOffsets == null)
            {
                EdgeOffsets = FiringLineMatrix.EdgeOffsets;
            }

            BitMatrix walls = GameStateCalculationCache.GameState.Walls;
            DirectionalMatrix<DistanceCalculation> attackMatrix 
                = new DirectionalMatrix<DistanceCalculation>(walls.Width, walls.Height);
            TwoValuedCircularBuffer<Node> bfsQueue = new TwoValuedCircularBuffer<Node>(CircularBufferCapacityRequired);

            // Note: the target point will not have a distance, since it will be shot, not moved to:
            TankLocation tankLocationAtTargetPoint = TurnCalculationCache.TankLocationMatrix[target.Position];

            if ((TargetElementType == ElementType.BASE) || (TargetElementType == ElementType.BULLET && AllowDestroyingABulletByMovingIntoIt))
            {
                AddNodesToQueueForMovingOverTarget(attackMatrix, bfsQueue, tankLocationAtTargetPoint);
            }
            else
                if (TargetElementType == ElementType.TANK)
                {
                    // TODO: Make points taboo if they would lead to both tank bodies overlapping, or moving directly in front of an enemy tank
                    /* TODO: a. Is this needed? Won't it break some algorithms?
                     *       b. Check that points are in the board area
                    foreach (Point point in tankLocationAtTargetPoint.TankHalo.GetPoints())
                    {
                        foreach (Direction dir in BoardHelper.AllRealDirections)
                        {
                            attackMatrix[dir, point.X, point.Y] = new DistanceCalculation(-1, new Node());
                        }
                    }
                     */
                }
                else
                {
                    // Bullet that can't be moved into...
                    
                    // TODO: Make points taboo within the "outline" of a tank body centred on the bullet
                }

            if (TabooAreas != null)
            {
                foreach (Rectangle rect in TabooAreas)
                {
                    foreach (Point point in rect.GetPoints())
                    {
                        foreach (Direction dir in BoardHelper.AllRealDirections)
                        {
                            if (TurnCalculationCache.CellMatrix[point].IsValid)
                            {
                                attackMatrix[dir, point.X, point.Y] = new DistanceCalculation(TABOO_DISTANCE, new Node());
                            }
                        }
                    }
                }
            }

            // Use the firing distance calculations for the target point, and add the initial points to the BFS queue:
            FiringLineSummary[,] firingLineSummariesByMovementDirAndEdgeOffset
                = new FiringLineSummary[Constants.RELEVANT_DIRECTION_COUNT, Constants.EDGE_OFFSET_COUNT];
            bool areFiringLinesStillActive = TryInitializeFiringLinesAndAddInitialFiringLineNodesToQueue(
                target, bfsQueue, firingLineSummariesByMovementDirAndEdgeOffset);

            int currDistance = 0;
            while (true)
            {
                if (areFiringLinesStillActive && bfsQueue.Size == 0)
                {
                    // Get nodes from the firing line/s with the next shortest distance:
                    areFiringLinesStillActive = TryAddFiringLineNodesWithNextShortestDistance(
                        bfsQueue, firingLineSummariesByMovementDirAndEdgeOffset, out currDistance);
                }
                if (bfsQueue.Size == 0)
                {
                    break;
                }

                CircularBufferItem<Node> bufferItem = bfsQueue.Remove();
                Node currNode = bufferItem.Item;
                if (bufferItem.Value > currDistance)
                {
                    if (bufferItem.Value == Constants.UNREACHABLE_DISTANCE)
                    {
                        break;
                    }

                    currDistance = bufferItem.Value;

                    // Add firing line nodes with the new distance to the queue:
                    if (areFiringLinesStillActive)
                    {
                        areFiringLinesStillActive = TryAddNextFiringLineNodesToQueue(bfsQueue,
                            firingLineSummariesByMovementDirAndEdgeOffset, currDistance);
                    }
                }

                int adjDistance = currDistance + 1;
                
                // Get each node adjacent to the current node:
                SegmentState innerEdgeStateInNodeDir = GameStateCalculationCache.TankInnerEdgeMatrix[currNode.X, currNode.Y][(int) currNode.Dir];
                SegmentState[] outerEdgeStates = GameStateCalculationCache.TankOuterEdgeMatrix[currNode.X, currNode.Y];
                SegmentState outerEdgeStateInNodeDir = outerEdgeStates[(int) currNode.Dir];

#if CONDITIONAL_BREAKPOINT_AttackTargetDistanceCalculator_CalculateMatrixOfShortestDistancesToTargetCell
                System.Diagnostics.Debug.Assert(currNode.X != 39 || currNode.Y != 45 || currNode.Dir != Direction.UP, "Breakpoint");
#endif

                // Node[] adjacentNodes = currNode.GetAdjacentIncomingNodes(innerEdgeStateInNodeDir, outerEdgeStateInNodeDir, outerEdgeStateInOppositeDir);
                // Insert inline for better performance...
                // **********************

                Node[] adjacentNodes = new Node[MAX_POSSIBLE_PRECEDING_NODE_COUNT];
                byte adjacentNodeCount = 0;

                if (currNode.ActionType == ActionType.FiringLine || currNode.ActionType == ActionType.Firing)
                {
                    // Firing and/or the firing line can only be invoked if the tank is first facing in the correct direction:
                    Node positioningNode = new Node(ActionType.Moving, currNode.Dir /*movementDir*/, currNode.X, currNode.Y);
                    adjacentNodes[0] = positioningNode;
                    adjacentNodeCount = 1;
                }
                else
                {
                    // So now the destination node (this) must be a moving node in the given direction...
                    Node adjacentNode;

                    // If there is a blocking wall in its direction of movement, 
                    // then any of the other movement/positioning nodes on the same cell
                    // can be a preceding node on the path:
                    if (outerEdgeStateInNodeDir == SegmentState.ShootableWall || outerEdgeStateInNodeDir == SegmentState.UnshootablePartialWall)
                    {
                        foreach (Direction otherDir in BoardHelper.AllRealDirections)
                        {
                            if (otherDir != currNode.Dir)
                            {
                                adjacentNode = new Node(ActionType.Moving, otherDir, currNode.X, currNode.Y);
                                adjacentNodes[adjacentNodeCount] = adjacentNode;
                                adjacentNodeCount++;
                            }
                        }
                    }

                    // Ignore invalid prior cells:
                    if (outerEdgeStates[(int)(currNode.Dir.GetOpposite())] != SegmentState.OutOfBounds)
                    {
                        // Get the adjacent cell's position:
                        int newX = currNode.X;
                        int newY = currNode.Y;
                        switch (currNode.Dir)
                        {
                            case Direction.UP:
                                newY++;
                                break;
                            case Direction.DOWN:
                                newY--;
                                break;
                            case Direction.LEFT:
                                newX++;
                                break;
                            case Direction.RIGHT:
                                newX--;
                                break;
                        }

                        switch (innerEdgeStateInNodeDir)
                        {
                            case SegmentState.Clear:
                                // Add all 4 directions on the adjacent cell
                                foreach (Direction otherDir in BoardHelper.AllRealDirections)
                                {
                                    adjacentNode = new Node(ActionType.Moving, otherDir, newX, newY);
                                    adjacentNodes[adjacentNodeCount] = adjacentNode;
                                    adjacentNodeCount++;
                                }
                                break;

                            case SegmentState.ShootableWall:
                                // Add the firing node in the current direction on the adjacent cell:
                                adjacentNode = new Node(ActionType.Firing, currNode.Dir, newX, newY);
                                adjacentNodes[adjacentNodeCount] = adjacentNode;
                                adjacentNodeCount++;
                                break;
                        }
                    }
                }

                // **********************
                // End of inlined section

                for (int n = 0; n < adjacentNodeCount; n++)
                {
                    Node adj = adjacentNodes[n];

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

        private bool TryInitializeFiringLinesAndAddInitialFiringLineNodesToQueue(
            Cell target, TwoValuedCircularBuffer<Node> bfsQueue, 
            FiringLineSummary[,] firingLineSummariesByMovementDirAndEdgeOffset)
        {
            bool areFiringLinesStillActive = false;
            foreach (Direction movementDir in MovementDirections)
            {
                foreach (EdgeOffset edgeOffset in EdgeOffsets)
                {
                    Line<FiringDistance> firingLine = FiringLineMatrix[target.Position, movementDir.GetOpposite(), edgeOffset];
                    FiringLineSummary firingLineSummary = new FiringLineSummary
                    {
                        FiringLine = firingLine
                    };
                    firingLineSummariesByMovementDirAndEdgeOffset[(int)movementDir, (int)edgeOffset] = firingLineSummary;

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
                            AddNextFiringLineNodesToQueueForMovementDirAndEdgeOffset(bfsQueue,
                                movementDir, edgeOffset, firingLineSummary, distanceToAdd: 0);
                        }
                    }
                }
            }
            return areFiringLinesStillActive;
        }

        private void AddNodesToQueueForMovingOverTarget(
            DirectionalMatrix<DistanceCalculation> attackMatrix, 
            TwoValuedCircularBuffer<Node> bfsQueue, TankLocation tankLocationAtTargetPoint)
        {
            // Give a distance of zero for internal points, but don't add them to the queue:
            DistanceCalculation zeroDistCalc = new DistanceCalculation(0, new Node());
            foreach (Point interiorPoint in tankLocationAtTargetPoint.TankBody.GetPoints())
            {
                foreach (Direction dir in BoardHelper.AllRealDirections)
                {
                    attackMatrix[dir, interiorPoint] = zeroDistCalc;
                }
            }

            // Calculate the tank positions that will destroy the base or bullet via movement from various directions:
            foreach (Direction movementDir in MovementDirections)
            {
                Direction oppositeDir = movementDir.GetOpposite();

                // Get the inside edge (segment) of the tank centred at the base in the opposite direction:
                Segment tankPositionsInDirection = tankLocationAtTargetPoint.InsideEdgesByDirection[(int)oppositeDir];

                // For each point on the segment add it to the bfs queue with a distance of zero:
                foreach (Cell tankCellInDir in tankPositionsInDirection.Cells)
                {
                    if (tankCellInDir.IsValid)
                    {
                        Node node = new Node(ActionType.Moving, movementDir, tankCellInDir.Position);
                        bfsQueue.Add(node, 0);
                    }
                }
            }
        }

        private bool TryAddFiringLineNodesWithNextShortestDistance(
            TwoValuedCircularBuffer<Node> bfsQueue, FiringLineSummary[,] firingLineSummariesByMovementDirAndEdgeOffset, 
            out int newDistance)
        {
            bool canAddMoreNodes = false;
            int nextShortestDistance = Constants.UNREACHABLE_DISTANCE;
            foreach (Direction movementDir in MovementDirections)
            {
                foreach (EdgeOffset edgeOffset in EdgeOffsets)
                {
                    FiringLineSummary firingLineSummary = firingLineSummariesByMovementDirAndEdgeOffset[(int)movementDir, (int)edgeOffset];
                    if (firingLineSummary.NextEdgeWeighting < nextShortestDistance)
                    {
                        nextShortestDistance = firingLineSummary.NextEdgeWeighting;
                        canAddMoreNodes = true;
                    }
                }
            }
            if (!canAddMoreNodes)
            {
                newDistance = Constants.UNREACHABLE_DISTANCE;
                return false;
            }
            newDistance = nextShortestDistance;
            return TryAddNextFiringLineNodesToQueue(bfsQueue, firingLineSummariesByMovementDirAndEdgeOffset, newDistance);
        }

        private bool TryAddNextFiringLineNodesToQueue(
            TwoValuedCircularBuffer<Node> bfsQueue, FiringLineSummary[,] firingLineSummariesByMovementDirAndEdgeOffset,
            int distanceToAdd)
        {
            bool anyAdded = false;
            foreach (Direction movementDir in MovementDirections)
            {
                foreach (EdgeOffset edgeOffset in EdgeOffsets)
                {
                    FiringLineSummary firingLineSummary
                        = firingLineSummariesByMovementDirAndEdgeOffset[(int)movementDir, (int) edgeOffset];
                    if (firingLineSummary.NextEdgeWeighting == distanceToAdd && firingLineSummary.IndexOfNextFiringLinePoint >= 0)
                    {
                        AddNextFiringLineNodesToQueueForMovementDirAndEdgeOffset(
                            bfsQueue, movementDir, edgeOffset, firingLineSummary, distanceToAdd);
                        anyAdded = true;
                    }
                }
            }
            return anyAdded;
        }

        private void AddNextFiringLineNodesToQueueForMovementDirAndEdgeOffset(
            TwoValuedCircularBuffer<Node> bfsQueue, Direction movementDir, EdgeOffset edgeOffset, 
            FiringLineSummary firingLineSummary, int distanceToAdd)
        {
            Line<FiringDistance> firingLine = firingLineSummary.FiringLine;
            int nextFiringLineIndex;
            for (nextFiringLineIndex = firingLineSummary.IndexOfNextFiringLinePoint;
                nextFiringLineIndex < firingLine.Length;
                nextFiringLineIndex++)
            {
                if (nextFiringLineIndex == -1)
                {
                    return;
                }
                FiringDistance firingDist = firingLine[nextFiringLineIndex];
                int nextEdgeWeighting = firingDist.TicksTillTargetShot - 1;  
                    // This distance is the edge weight in the graph. 
                    // The -1 is because we need to compensate for the weight 1 edge added from 
                    // the tank's position + direction on the firing line, to the firing line node.

                if (nextEdgeWeighting > distanceToAdd)
                {
                    firingLineSummary.NextEdgeWeighting = nextEdgeWeighting;
                    firingLineSummary.IndexOfNextFiringLinePoint = nextFiringLineIndex;
                    return;
                }

                if (!TurnCalculationCache.CellMatrix[firingDist.StartingTankPosition].IsValid)
                {
                    firingLineSummary.NextEdgeWeighting = Constants.UNREACHABLE_DISTANCE;
                    firingLineSummary.IndexOfNextFiringLinePoint = -1;
                    return;
                }

                Node firingLineNode = new Node(ActionType.FiringLine, movementDir, firingDist.StartingTankPosition, 
                    (byte) nextFiringLineIndex, edgeOffset);
                bfsQueue.Add(firingLineNode, nextEdgeWeighting);
            }
            if (nextFiringLineIndex == firingLine.Length)
            {
                firingLineSummary.NextEdgeWeighting = Constants.UNREACHABLE_DISTANCE;
                firingLineSummary.IndexOfNextFiringLinePoint = -1;
            }
        }

        public CombinedMovementAndFiringDistanceCalculation GetShortestAttackDistanceFromCurrentTankPosition(
            int tankIndex, Cell target)
        {
            DirectionalMatrix<DistanceCalculation> movementDistanceMatrix 
                = GameStateCalculationCache.GetDistanceMatrixFromTankByTankIndex(tankIndex);
            return GetShortestAttackDistanceGivenTheSourcePointsMovementMatrix(movementDistanceMatrix, target);
        }

        public CombinedMovementAndFiringDistanceCalculation GetShortestAttackDistanceGivenTheSourcePointsMovementMatrix(
            DirectionalMatrix<DistanceCalculation> movementDistanceMatrix, Cell target)
        {
            int bestCombinedTicksUntilTargetShot = Constants.UNREACHABLE_DISTANCE;
            CombinedMovementAndFiringDistanceCalculation bestCombinedDistance = null;

            foreach (Direction attackDir in MovementDirections)
            {
                foreach (EdgeOffset edgeOffset in EdgeOffsets)
                {
                    Direction outwardDir = attackDir.GetOpposite();
                    Line<FiringDistance> firingLine = FiringLineMatrix[target.Position, outwardDir, edgeOffset];

                    for (int i = 0; i < firingLine.Length; i++)
                    {
                        FiringDistance firingDist = firingLine[i];

                        // Ignore firing line points that start off with a normal movement, 
                        // as that path can be found by moving to the closer point in a non-firing line way:
                        if (firingDist.CanMoveOrFire)
                        {
                            continue;
                        }

                        // Ignore invalid starting points on the firing line:
                        if (!(GameStateCalculationCache.GameState.Walls.BoardBoundary.ContainsPoint(firingDist.StartingTankPosition)
                            && TurnCalculationCache.TankLocationMatrix[firingDist.StartingTankPosition].IsValid))
                        {
                            break;
                        }

                        DistanceCalculation movementDist = movementDistanceMatrix[attackDir, firingDist.StartingTankPosition];

                        int combinedTicks = movementDist.Distance + firingDist.TicksTillTargetShot;
                        if (combinedTicks < bestCombinedTicksUntilTargetShot)
                        {
                            bestCombinedTicksUntilTargetShot = combinedTicks;
                            bestCombinedDistance
                                = new CombinedMovementAndFiringDistanceCalculation(movementDist, firingDist, attackDir);
                        }
                    }
                }
            }

            return bestCombinedDistance;
        }

        #endregion
    }
}