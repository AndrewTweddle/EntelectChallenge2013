using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Engines;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    /// <summary>
    /// The Node class represents a node in the distance graph.
    /// It is used to convert between an int representation (stored in the circular buffer) and its constituent parts.
    /// These are the cell it is part of. The direction the tank is facing on that cell. 
    /// And the action the tank must take to reach this node (moving or firing).
    /// </summary>
    public struct Node
    {
        private const int MAX_POSSIBLE_PRECEDING_NODE_COUNT = 7;

        public int MobilityId { get; set; }

        public byte X
        {
            get
            {
                return (byte) (MobilityId & 0xFF);
            }
            set
            {
                MobilityId = (MobilityId & (~0xFF)) | value;
            }
        }

        public byte Y
        {
            get
            {
                return (byte)((MobilityId & 0xFF00) >> 8);
            }
            set
            {
                MobilityId = (MobilityId & (~0xFF00)) | (value << 8);
            }
        }

        public Direction Dir 
        {
            get
            {
                return (Direction) ((MobilityId & 0x3000000) >> 24);
            }
            set
            {
                MobilityId = (MobilityId & (~0x3000000)) | ((int) value << 24);
            }
        }

        public ActionType ActionType 
        {
            get
            {
                return (ActionType)((MobilityId & 0xC000000) >> 26); ;
            }
            set
            {
                MobilityId = (MobilityId & (~0xC000000)) | ((int) value << 26);
            }
        }

        public EdgeOffset EdgeOffset
        {
            get
            {
                return (EdgeOffset)((MobilityId & 0x7F000000) >> 28); ;
            }
            set
            {
                MobilityId = (MobilityId & (~0x7F000000)) | ((int)value << 28);
            }
        }

        public byte FiringLineIndex
        {
            get
            {
                return (byte)((MobilityId & 0x00FF0000) >> 16); ;
            }
            set
            {
                MobilityId = (MobilityId & (~0x00FF0000)) | ((int)value << 16);
            }
        }

        public Node(int mobilityId): this()
        {
            MobilityId = mobilityId;
        }

        public Node(ActionType actionType, Direction dir, int x, int y, 
            byte firingLineIndex = 0, EdgeOffset edgeOffset = EdgeOffset.Centre): this()
        {
            MobilityId = ((byte)edgeOffset << 28) | ((byte)actionType << 26) | ((byte)dir << 24) | ((byte)y << 8) | (byte)x;
        }

        public Node(ActionType actionType, Direction dir, Point pos, 
            byte firingLineIndex = 0, EdgeOffset edgeOffset = EdgeOffset.Centre)
            : this(actionType, dir, pos.X, pos.Y, firingLineIndex, edgeOffset)
        {
        }

        public Node(ActionType actionType, MobileState mobileState, 
            byte firingLineIndex = 0, EdgeOffset edgeOffset = EdgeOffset.Centre)
            : this(actionType, mobileState.Dir, mobileState.Pos, firingLineIndex, edgeOffset)
        {
        }

        public Node[] GetAdjacentOutgoingNodes(SegmentState[] segStatesByDir)
        {
            Node adjacentNode;
            Node[] adjacentNodes = new Node[Constants.RELEVANT_DIRECTION_COUNT];
            byte adjacentNodeCount = 0;

            foreach (Direction edgeDir in BoardHelper.AllRealDirections)
            {
                int newX = X;
                int newY = Y;
                SegmentState edgeState = segStatesByDir[(byte)edgeDir];

                if ((ActionType == ActionType.Moving && edgeState == SegmentState.Clear)
                    || (ActionType == ActionType.Firing && Dir == edgeDir))
                {
                    // Move straight to the corresponding node in the adjacent cell:
                    switch (edgeDir)
                    {
                        case Direction.UP:
                            newY--;
                            break;
                        case Direction.DOWN:
                            newY++;
                            break;
                        case Direction.LEFT:
                            newX--;
                            break;
                        case Direction.RIGHT:
                            newX++;
                            break;
                    }
                    adjacentNode = new Node(ActionType.Moving, edgeDir, newX, newY);
                    adjacentNodes[adjacentNodeCount] = adjacentNode;
                    adjacentNodeCount++;
                    continue;
                }

                if (ActionType == ActionType.Moving)
                {
                    if (Dir == edgeDir)
                    {
                        // Fire in the current direction of movement to clear space to the adjacent cell:
                        if (edgeState == SegmentState.ShootableWall)
                        {
                            adjacentNode = new Node(ActionType.Firing, edgeDir, X, Y);
                            adjacentNodes[adjacentNodeCount] = adjacentNode;
                            adjacentNodeCount++;
                            break;
                        }
                    }
                    else
                    {
                        switch (edgeState)
                        {
                            case SegmentState.ShootableWall:
                            case SegmentState.UnshootablePartialWall:
                                adjacentNode = new Node(ActionType.Moving, edgeDir, X, Y);
                                adjacentNodes[adjacentNodeCount] = adjacentNode;
                                adjacentNodeCount++;
                                break;
                            case SegmentState.OutOfBounds:
                                if (GameRuleConfiguration.RuleConfiguration.DoesATankTurnIfTryingToMoveOffTheBoard)
                                {
                                    adjacentNode = new Node(ActionType.Moving, edgeDir, X, Y);
                                    adjacentNodes[adjacentNodeCount] = adjacentNode;
                                    adjacentNodeCount++;
                                }
                                break;
                        }
                    }
                }
            }

            if (adjacentNodeCount < 4)
            {
                Array.Resize(ref adjacentNodes, adjacentNodeCount);
            }
            return adjacentNodes;
        }

        public Node[] GetAdjacentIncomingNodes(SegmentState innerSegStateInDir, SegmentState outerSegStateInDir, 
            SegmentState outerSegStateInOppositeDir)
        {
            if (ActionType == ActionType.FiringLine || ActionType == ActionType.Firing)
            {
                // Firing and/or the firing line can only be invoked if the tank is first facing in the correct direction:
                Node positioningNode = new Node(ActionType.Moving, Dir, X, Y);
                return new Node[] { positioningNode };
            }

            // So now the destination node (this) must be a moving node in the given direction...
            Node[] adjacentNodes = new Node[MAX_POSSIBLE_PRECEDING_NODE_COUNT];
            byte adjacentNodeCount = 0;
            Node adjacentNode;
            Direction currentDir = Dir;

            // If there is a blocking wall in its direction of movement, 
            // then any of the other movement/positioning nodes on the same cell
            // can be a preceding node on the path:
            if (outerSegStateInDir == SegmentState.ShootableWall || outerSegStateInDir == SegmentState.UnshootablePartialWall)
            {
                foreach (Direction otherDir in BoardHelper.AllRealDirections)
                {
                    if (otherDir != Dir)
                    {
                        adjacentNode = new Node(ActionType.Moving, otherDir, X, Y);
                        adjacentNodes[adjacentNodeCount] = adjacentNode;
                        adjacentNodeCount++;
                    }
                }
            }

            // Ignore invalid prior cells:
            if (outerSegStateInOppositeDir != SegmentState.OutOfBounds)
            {
                // Get the adjacent cell's position:
                int newX = X;
                int newY = Y;
                switch (currentDir)
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

                switch (innerSegStateInDir)
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
                        adjacentNode = new Node(ActionType.Firing, currentDir, newX, newY);
                        adjacentNodes[adjacentNodeCount] = adjacentNode;
                        adjacentNodeCount++;
                        break;
                }
            }

            // Clean out any unused slots in the array of preceding nodes:
            if (adjacentNodeCount < MAX_POSSIBLE_PRECEDING_NODE_COUNT)
            {
                Array.Resize(ref adjacentNodes, adjacentNodeCount);
            }
            return adjacentNodes;
        }
    }
}
