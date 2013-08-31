using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Engines;
using System.Diagnostics;

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

        private const int XBitMask = 0xFF;
        private const int XBitShift = 0;

        private const int YBitMask = 0xFF00;
        private const int YBitShift = 8;

        private const int FiringLineIndexMask = 0xFF0000;
        private const int FiringLineIndexBitShift = 16;

        private const int DirMask = 0x3000000;
        private const int DirBitShift = 24;

        private const int ActionTypeMask = 0xC000000;
        private const int ActionTypeBitShift = 26;

        private const int EdgeOffsetMask = 0x70000000;
        private const int EdgeOffsetBitShift = 28;

        public int MobilityId { get; set; }

        public byte X
        {
            get
            {
                return (byte)(MobilityId & XBitMask);
            }
            set
            {
                MobilityId = (MobilityId & ~XBitMask) | value;
            }
        }

        public byte Y
        {
            get
            {
                return (byte)((MobilityId & YBitMask) >> YBitShift);
            }
            set
            {
                MobilityId = (MobilityId & ~YBitMask) | (value << YBitShift);
            }
        }

        public Direction Dir 
        {
            get
            {
                return (Direction)((MobilityId & DirMask) >> DirBitShift);
            }
            set
            {
                MobilityId = (MobilityId & ~DirMask) | ((int)value << DirBitShift);
            }
        }

        public ActionType ActionType 
        {
            get
            {
                return (ActionType)((MobilityId & ActionTypeMask) >> ActionTypeBitShift); ;
            }
            set
            {
                MobilityId = (MobilityId & ~ActionTypeMask) | ((int)value << ActionTypeBitShift);
            }
        }

        public EdgeOffset EdgeOffset
        {
            get
            {
                return (EdgeOffset)((MobilityId & EdgeOffsetMask) >> EdgeOffsetBitShift); ;
            }
            set
            {
                MobilityId = (MobilityId & ~EdgeOffsetMask) | ((int)value << EdgeOffsetBitShift);
            }
        }

        public byte FiringLineIndex
        {
            get
            {
                return (byte)((MobilityId & FiringLineIndexMask) >> FiringLineIndexBitShift); ;
            }
            set
            {
                MobilityId = (MobilityId & ~FiringLineIndexMask) | ((int)value << FiringLineIndexBitShift);
            }
        }

        public Node(int mobilityId): this()
        {
            MobilityId = mobilityId;
        }

        public Node(ActionType actionType, Direction dir, int x, int y, 
            byte firingLineIndex = 0, EdgeOffset edgeOffset = EdgeOffset.Centre): this()
        {
            MobilityId
                = (byte)x
                | ((byte)y << YBitShift)
                | ((byte)dir << DirBitShift)
                | ((byte)actionType << ActionTypeBitShift)
                | ((byte)edgeOffset << EdgeOffsetBitShift)
                | (firingLineIndex << FiringLineIndexBitShift);

            Debug.Assert(ActionType == actionType, "ActionType wrong");
            Debug.Assert(Dir == dir, "Dir wrong");
            Debug.Assert(X == x, "X wrong");
            Debug.Assert(Y == y, "Y wrong");
            Debug.Assert(FiringLineIndex == firingLineIndex, "FiringLineIndex wrong");
            Debug.Assert(EdgeOffset == edgeOffset, "EdgeOffset wrong");
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
                Direction movementDir 
                    = ActionType == Core.ActionType.FiringLine 
                    ? Dir.GetOpposite() 
                    : Dir;
                Node positioningNode = new Node(ActionType.Moving, movementDir, X, Y);
                return new Node[] { positioningNode };
            }

            // So now the destination node (this) must be a moving node in the given direction...
            Node[] adjacentNodes = new Node[MAX_POSSIBLE_PRECEDING_NODE_COUNT];
            byte adjacentNodeCount = 0;
            Node adjacentNode;

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
                switch (Dir)
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
                        adjacentNode = new Node(ActionType.Firing, Dir, newX, newY);
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
