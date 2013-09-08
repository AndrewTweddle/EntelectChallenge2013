using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public class DistanceCalculator
    {
        #region Required Input Properties

        public BitMatrix Walls { get; set; }
        public Matrix<SegmentState[]> TankOuterEdgeMatrix { get; set; }
        public Matrix<Cell> CellMatrix { get; set; }

        #endregion

        
        #region Optional Input Properties

        public int TicksWithoutFiring { get; set; }
        public int CircularBufferCapacityRequired { get; set; }
        public Rectangle[] TabooAreas { get; set; }
        public Rectangle RestrictedMovementArea { get; set; }

        #endregion


        #region Constructors

        public DistanceCalculator()
        {
            RestrictedMovementArea = Rectangle.Unrestricted;
            CircularBufferCapacityRequired = DistanceCalculationConstants.SUGGESTED_CIRCULAR_BUFFER_CAPACITY_REQUIRED;
        }

        #endregion

        public DirectionalMatrix<DistanceCalculation> CalculateShortestDistancesFromTank(ref MobileState tankState)
        {
            DirectionalMatrix<DistanceCalculation> distanceMatrix
                = new DirectionalMatrix<DistanceCalculation>(Walls.Width, Walls.Height);

            distanceMatrix[tankState.Dir, tankState.Pos] = new DistanceCalculation(0, new Node());

            // Mark taboo areas:
            if (TabooAreas != null)
            {
                for (int r = 0; r < TabooAreas.Length; r++)
                {
                    Rectangle tabooRect = TabooAreas[r];
                    foreach (Point tabooPoint in tabooRect.GetPoints())
                    {
                        foreach (Direction dir in BoardHelper.AllRealDirections)
                        {
                            if (CellMatrix[tabooPoint].IsValid)
                            {
                                distanceMatrix[dir, tabooPoint] = new DistanceCalculation(
                                    DistanceCalculationConstants.TABOO_DISTANCE, new Node());
                            }
                        }
                    }
                }
            }

            // Restrict the area of the board within which to do shortest path calculations (i.e. for efficiency reasons):
            if (RestrictedMovementArea != Rectangle.Unrestricted)
            {
                foreach (Direction restrictedAreaDir in BoardHelper.AllRealDirections)
                {
                    Rectangle outerEdge = RestrictedMovementArea.GetOuterEdgeInDirection(restrictedAreaDir);
                    if (outerEdge.IntersectsWith(Walls.BoardBoundary))
                    {
                        foreach (Point restrictedPoint in outerEdge.GetPoints())
                        {
                            if (Walls.BoardBoundary.ContainsPoint(restrictedPoint) && CellMatrix[restrictedPoint].IsValid)
                            {
                                foreach (Direction dir in BoardHelper.AllRealDirections)
                                {
                                    distanceMatrix[dir, restrictedPoint] = new DistanceCalculation(
                                        DistanceCalculationConstants.TABOO_DISTANCE, new Node());
                                }
                            }
                        }
                    }
                }
            }

            if (TicksWithoutFiring > 0)
            {
                FindShortestPathsWithRestrictionsOnFiring(tankState, distanceMatrix);
            }
            else
            {
                FindShortestPaths(tankState, distanceMatrix);
            }
            return distanceMatrix;
        }

        private void FindShortestPaths(MobileState tankState, DirectionalMatrix<DistanceCalculation> distanceMatrix)
        {
            TwoValuedCircularBuffer<Node> bfsQueue = new TwoValuedCircularBuffer<Node>(CircularBufferCapacityRequired);
            Node currNode = new Node(ActionType.Moving, tankState.Dir, tankState.Pos);
            int adjDistance = 1;
            bool nodesToProcess = true;

            while (nodesToProcess)
            {
                // Get each node adjacent to the current node:
                Node[] adjacentNodes = currNode.GetAdjacentOutgoingNodes(TankOuterEdgeMatrix[currNode.X, currNode.Y]);
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
        }

        private void FindShortestPathsWithRestrictionsOnFiring(MobileState tankState, DirectionalMatrix<DistanceCalculation> distanceMatrix)
        {
            TwoValuedCircularBuffer<Node> bfsQueue = new TwoValuedCircularBuffer<Node>(CircularBufferCapacityRequired);
            Node currNode = new Node(ActionType.Moving, tankState.Dir, tankState.Pos);
            int adjDistance = 1;
            bool nodesToProcess = true;
            bool canFire = TicksWithoutFiring < adjDistance;

            while (nodesToProcess)
            {
                // Get each node adjacent to the current node:
                Node[] adjacentNodes 
                    = canFire 
                    ? currNode.GetAdjacentOutgoingNodes(TankOuterEdgeMatrix[currNode.X, currNode.Y])
                    : currNode.GetAdjacentOutgoingNodesWithoutFiring(TankOuterEdgeMatrix[currNode.X, currNode.Y]);
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
                    canFire = adjDistance > TicksWithoutFiring;
                }
            }
        }

        public static Node[] GetNodesOnShortestPath(DirectionalMatrix<DistanceCalculation> distances, Direction dir, Point pos)
        {
            return GetNodesOnShortestPath(distances, dir, pos.X, pos.Y);
        }

        public static Node[] GetNodesOnShortestPath(DirectionalMatrix<DistanceCalculation> distances, Direction dir, int x, int y)
        {
            DistanceCalculation distanceCalc = distances[dir, x, y];
            if (distanceCalc.Distance == Constants.UNREACHABLE_DISTANCE)
            {
                return new Node[0];
            }

            Node[] nodes = new Node[distanceCalc.Distance];
            int index = distanceCalc.Distance;
            Node node = new Node(ActionType.Moving, dir, x, y);

            while (index != 0)
            {
                index--;
                nodes[index] = node;
                node = distanceCalc.AdjacentNode;
                if (node.ActionType == ActionType.Firing)
                {
                    index--;
                    nodes[index] = node;
                    node = new Node(ActionType.Moving, node.Dir, node.X, node.Y);
                }
                distanceCalc = distances[node.Dir, node.X, node.Y];
            }
            return nodes;
        }

        public static TankAction[] GetTankActionsOnShortestPath(
            DirectionalMatrix<DistanceCalculation> distances, Direction dir, Point position)
        {
            return GetTankActionsOnShortestPath(distances, dir, position.X, position.Y);
        }

        public static TankAction[] GetTankActionsOnShortestPath(
            DirectionalMatrix<DistanceCalculation> distances, Direction dir, int x, int y)
        {
            DistanceCalculation distanceCalc = distances[dir, x, y];
            if (distanceCalc.Distance == Constants.UNREACHABLE_DISTANCE)
            {
                return new TankAction[0];
            }

            TankAction[] tankActions = new TankAction[distanceCalc.Distance];
            int index = distanceCalc.Distance;
            Node node = new Node(ActionType.Moving, dir, x, y);

            while (index != 0)
            {
                index--;
                tankActions[index] = node.Dir.ToTankAction();
                node = distanceCalc.AdjacentNode;
                if (node.ActionType == ActionType.Firing)
                {
                    index--;
                    tankActions[index] = TankAction.FIRE;
                    node = new Node(ActionType.Moving, node.Dir, node.X, node.Y);
                }
                distanceCalc = distances[node.Dir, node.X, node.Y];
            }
            return tankActions;
        }
    }
}
