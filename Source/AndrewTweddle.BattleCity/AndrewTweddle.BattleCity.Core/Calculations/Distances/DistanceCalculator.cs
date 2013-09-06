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
    public static class DistanceCalculator
    {
        private const int SUGGESTED_CIRCULAR_BUFFER_CAPACITY_REQUIRED = 1024;
        private const int TABOO_DISTANCE = -2;

        public static DirectionalMatrix<DistanceCalculation> CalculateShortestDistancesFromTank(
            ref MobileState tankState, BitMatrix walls,
            Matrix<SegmentState[]> tankEdgeMatrix, Rectangle[] tabooAreas = null,
            int circularBufferCapacityRequired = SUGGESTED_CIRCULAR_BUFFER_CAPACITY_REQUIRED)
        {
            DirectionalMatrix<DistanceCalculation> distanceMatrix
                = new DirectionalMatrix<DistanceCalculation>(walls.Width, walls.Height);

            distanceMatrix[tankState.Dir, tankState.Pos] = new DistanceCalculation(0, new Node());

            TwoValuedCircularBuffer<Node> bfsQueue = new TwoValuedCircularBuffer<Node>(circularBufferCapacityRequired);

            Node currNode = new Node(ActionType.Moving, tankState.Dir, tankState.Pos);
            int adjDistance = 1;
            bool nodesToProcess = true;

            // Mark taboo areas:
            if (tabooAreas != null)
            {
                for (int r = 0; r < tabooAreas.Length; r++)
                {
                    Rectangle tabooRect = tabooAreas[r];
                    foreach (Point tabooPoint in tabooRect.GetPoints())
                    {
                        foreach (Direction dir in BoardHelper.AllRealDirections)
                        {
                            distanceMatrix[dir, tabooPoint] = new DistanceCalculation(TABOO_DISTANCE, new Node());
                        }
                    }
                }
            }
            
            while (nodesToProcess)
            {
                // Get each node adjacent to the current node:
                Node[] adjacentNodes = currNode.GetAdjacentOutgoingNodes(tankEdgeMatrix[currNode.X, currNode.Y]);
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
