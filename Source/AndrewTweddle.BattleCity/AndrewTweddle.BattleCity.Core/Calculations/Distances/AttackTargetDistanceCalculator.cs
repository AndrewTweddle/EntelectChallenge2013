using System;
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

        private const int SUGGESTED_CIRCULAR_BUFFER_CAPACITY_REQUIRED = 1024;

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
            CircularBufferCapacityRequired = SUGGESTED_CIRCULAR_BUFFER_CAPACITY_REQUIRED;
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
                    // TODO: Make points taboo if they would lead to both tank bodies overlapping
                }
                else
                {
                    // Bullet that can't be moved into...
                    
                    // TODO: Make points taboo within the "outline" of a tank body centred on the bullet
                }

            // Use the firing distance calculations for the target point, and add the initial points to the BFS queue:
            FiringLineSummary[,] firingLineSummariesByMovementDirAndEdgeOffset
                = new FiringLineSummary[MovementDirections.Length, EdgeOffsets.Length];
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
                SegmentState[] outerEdgeStates = GameStateCalculationCache.TankInnerEdgeMatrix[currNode.X, currNode.Y];
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
            DistanceCalculation uncalculatedDistCalc = new DistanceCalculation();
            foreach (Direction movementDir in MovementDirections)
            {
                Direction oppositeDir = movementDir.GetOpposite();

                // Get the inside edge (segment) of the tank centred at the base in the opposite direction:
                Segment tankPositionsInDirection = tankLocationAtTargetPoint.InsideEdgesByDirection[(int)oppositeDir];

                // For each point on the segment add it to the bfs queue with a distance of zero:
                foreach (Cell tankCellInDir in tankPositionsInDirection.Cells)
                {
                    attackMatrix[movementDir, tankCellInDir.Position] = uncalculatedDistCalc;
                    if (tankCellInDir.IsValid)
                    {
                        Node node = new Node(ActionType.Moving, oppositeDir, tankCellInDir.Position);
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
                    if (firingLineSummary.NextEdgeWeighting == distanceToAdd)
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
                Node firingLineNode = new Node(ActionType.FiringLine, movementDir, firingDist.StartingTankPosition, edgeOffset);
                bfsQueue.Add(firingLineNode, nextEdgeWeighting);
            }
            if (nextFiringLineIndex == firingLine.Length)
            {
                firingLineSummary.NextEdgeWeighting = Constants.UNREACHABLE_DISTANCE;
                firingLineSummary.IndexOfNextFiringLinePoint = -1;
            }
        }

        #endregion
    }
}