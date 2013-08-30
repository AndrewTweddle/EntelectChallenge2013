using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Engines;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    /// <summary>
    /// CacheNode is like Node except it stores all the fields instead of calculating them.
    /// It is a class not a struct (since Node is really just an embellished int!)
    /// </summary>
    public struct CacheNode
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Direction Dir { get; set; }
        public ActionType ActionType { get; set; }

        public CacheNode(ActionType actionType, Direction dir, int x, int y): this()
        {
            X = x;
            Y = y;
            Dir = dir;
            ActionType = actionType;
        }

        public CacheNode(ActionType actionType, Direction dir, Point pos) : this(actionType, dir, pos.X, pos.Y) 
        {
        }

        public CacheNode(ActionType actionType, MobileState mobileState)
            : this(actionType, mobileState.Dir, mobileState.Pos)
        {
        }

        public CacheNode[] GetAdjacentNodes(SegmentState[] segStatesByDir)
        {
            CacheNode adjacentNode;
            CacheNode[] adjacentNodes = new CacheNode[Constants.RELEVANT_DIRECTION_COUNT];
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
                    adjacentNode = new CacheNode(ActionType.Moving, edgeDir, newX, newY);
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
                            adjacentNode = new CacheNode(ActionType.Firing, edgeDir, X, Y);
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
                                adjacentNode = new CacheNode(ActionType.Moving, edgeDir, X, Y);
                                adjacentNodes[adjacentNodeCount] = adjacentNode;
                                adjacentNodeCount++;
                                break;
                            case SegmentState.OutOfBounds:
                                if (GameRuleConfiguration.RuleConfiguration.DoesATankTurnIfTryingToMoveOffTheBoard)
                                {
                                    adjacentNode = new CacheNode(ActionType.Moving, edgeDir, X, Y);
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
    }
}
