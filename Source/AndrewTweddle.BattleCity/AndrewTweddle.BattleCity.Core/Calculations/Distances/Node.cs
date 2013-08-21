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
                return (Direction) (MobilityId & 0x30000 >> 16);
            }
            set
            {
                MobilityId = (MobilityId & (~0x30000)) | ((int) value << 16);
            }
        }

        public ActionType ActionType 
        {
            get
            {
                return (ActionType)(MobilityId & 0xC0000 >> 18); ;
            }
            set
            {
                MobilityId = (MobilityId & (~0xC0000)) | ((int) value << 18);
            }
        }

        public Node(int mobilityId): this()
        {
            MobilityId = mobilityId;
        }

        public Node(ActionType actionType, Direction dir, int x, int y): this()
        {
            MobilityId = ((byte)actionType << 18) | (byte)dir << 16 | (byte) y << 8 | (byte) x;
        }

        public Node(ActionType actionType, Direction dir, Point pos): this(actionType, dir, pos.X, pos.Y)
        {
        }

        public Node(ActionType actionType, MobileState mobileState)
            : this(actionType, mobileState.Dir, mobileState.Pos)
        {
        }

        public Node[] GetAdjacentNodes(SegmentState[] segStatesByDir)
        {
            Node adjacentNode;
            Node[] adjacentNodes = new Node[Constants.RELEVANT_DIRECTION_COUNT];
            byte adjacentNodeCount = 0;
            int newX = X;
            int newY = Y;

            foreach (Direction edgeDir in BoardHelper.AllRealDirections)
            {
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
                Node[] adjNodesCopy = new Node[adjacentNodeCount];
                Array.Copy(adjacentNodes, adjNodesCopy, adjacentNodeCount);
                return adjNodesCopy;
            }
            else
            {
                return adjacentNodes;
            }
        }

    }
}
