using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    /*
    public static class PathCalculator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adjacencyType"></param>
        /// <param name="distances"></param>
        /// <param name="dir"></param>
        /// <param name="toPos"></param>
        /// <param name="firingLineMatrix">Only required for an adjacency type of incoming</param>
        /// <returns></returns>
        public static Node[] GetNodesOnShortestPath(AdjacencyType adjacencyType, 
            DirectionalMatrix<DistanceCalculation> distances, Direction dir, Point toPos,
            DirectionalMatrix<Line<FiringDistance>> firingLineMatrix = null)
        {
            return GetNodesOnShortestPath(adjacencyType, distances, dir, toPos.X, toPos.Y, firingLineMatrix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adjacencyType"></param>
        /// <param name="distances"></param>
        /// <param name="dir"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        /// <param name="firingLineMatrix">Only required for an adjacency type of incoming</param>
        /// <returns></returns>
        public static Node[] GetNodesOnShortestPath(AdjacencyType adjacencyType,
            DirectionalMatrix<DistanceCalculation> distances, Direction dir, int toX, int toY, 
            DirectionalMatrix<Line<FiringDistance>> firingLineMatrix = null)
        {
            DistanceCalculation distanceCalc = distances[dir, toX, toY];
            if (distanceCalc.Distance == Constants.UNREACHABLE_DISTANCE)
            {
                return new Node[0];
            }

            Node[] nodes = new Node[distanceCalc.Distance];
            int nodeIndex;
            Node node;

            if (adjacencyType == AdjacencyType.IncomingToTarget)
            {
                nodeIndex = 0;
                int length = distanceCalc.Distance;
                node = distanceCalc.AdjacentNode;
                while (nodeIndex != length)
                {
                    switch (node.ActionType)
                    {
                        case ActionType.FiringLine:
                            Line<FiringDistance> firingLine = firingLineMatrix[node.Dir, node.X, node.Y];
                            FiringDistance firingDistance = firingLine[node.X, node.Y];
                            FiringDistanceCalculator.AddFiringLineNodesToRoute(firingDistance, 
                    }

                    nodes[nodeIndex] = node;
                    node = distanceCalc.AdjacentNode;
                    if (node.ActionType == ActionType.Firing)
                    {
                        nodeIndex--;
                        nodes[nodeIndex] = node;
                        node = new Node(ActionType.Moving, node.Dir, node.X, node.Y);
                    }
                    distanceCalc = distances[node.Dir, node.X, node.Y];
                    nodeIndex++;
                }
            }
            else
            {
                nodeIndex = distanceCalc.Distance;
                node = new Node(ActionType.Moving, dir, toX, toY);
                while (nodeIndex != 0)
                {
                    nodeIndex--;
                    nodes[nodeIndex] = node;
                    node = distanceCalc.AdjacentNode;
                    if (node.ActionType == ActionType.Firing)
                    {
                        nodeIndex--;
                        nodes[nodeIndex] = node;
                        node = new Node(ActionType.Moving, node.Dir, node.X, node.Y);
                    }
                    distanceCalc = distances[node.Dir, node.X, node.Y];
                }
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
    */
}
