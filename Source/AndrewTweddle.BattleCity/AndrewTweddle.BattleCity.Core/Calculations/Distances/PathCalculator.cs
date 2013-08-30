using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public static class PathCalculator
    {
        public static Node[] GetIncomingNodesOnShortestPath(
            DirectionalMatrix<DistanceCalculation> distances, 
            Direction dir, int fromX, int fromY, int toX, int toY, 
            FiringLineMatrix firingLineMatrix = null,
            bool keepMovingCloserOnFiringLastBullet = false)
        {
            DistanceCalculation distanceCalc = distances[dir, fromX, fromY];
            if (distanceCalc.Distance == Constants.UNREACHABLE_DISTANCE)
            {
                return new Node[0];
            }

            int length = distanceCalc.Distance;
            Node[] nodes = new Node[length];
            int nodeIndex;
            Node node;

            nodeIndex = 0;
            node = distanceCalc.AdjacentNode;
            while (nodeIndex != length)
            {
                if (node.ActionType == ActionType.FiringLine)
                {
                    Line<FiringDistance> firingLine = firingLineMatrix[toX, toY, node.Dir, node.EdgeOffset];
                    FiringDistance firingDistance = firingLine[node.FiringLineIndex];
                    FiringDistanceCalculator.AddFiringLineNodesToRoute(firingDistance,
                        firingLine.DirectionOfLine.GetOpposite(), nodes, ref nodeIndex, keepMovingCloserOnFiringLastBullet);
                    break;
                }

                // Add the node:
                nodes[nodeIndex] = node;
                nodeIndex++;
                if (node.ActionType == ActionType.Firing)
                {
                    // Add a moving node - the only reason to fire is to move...
                    node = new Node(ActionType.Moving, node.Dir, node.X + node.Dir.GetXOffset(), node.Y + node.Dir.GetYOffset());
                    nodes[nodeIndex] = node;
                    nodeIndex++;
                }
                distanceCalc = distances[node.Dir, node.X, node.Y];
            }
            if (nodeIndex < length)
            {
                Array.Resize(ref nodes, nodeIndex);
            }
            return nodes;
        }

        public static TankAction[] GetTankActionsOnIncomingShortestPath(
            DirectionalMatrix<DistanceCalculation> distances, 
            Direction dir, int fromX, int fromY, int toX, int toY,
            FiringLineMatrix firingLineMatrix,
            bool keepMovingCloserOnFiringLastBullet = false)
        {
            Node[] nodes = GetIncomingNodesOnShortestPath(
                distances, dir, fromX, fromY, toX, toY, firingLineMatrix, keepMovingCloserOnFiringLastBullet);
            TankAction[] tankActions = new TankAction[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                Node node = nodes[i];
                TankAction tankAction = node.ActionType == ActionType.Firing ? TankAction.FIRE : node.Dir.ToTankAction();
                tankActions[i] = tankAction;
            }
            return tankActions;
        }

        public static Node[] GetOutgoingNodesOnShortestPath(
            DirectionalMatrix<DistanceCalculation> distances, Direction dir, int x, int y)
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

        public static TankAction[] GetTankActionsOnOutgoingShortestPath(
            DirectionalMatrix<DistanceCalculation> distances, Direction dir, Point position)
        {
            return GetTankActionsOnOutgoingShortestPath(distances, dir, position.X, position.Y);
        }

        public static TankAction[] GetTankActionsOnOutgoingShortestPath(
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
