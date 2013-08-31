using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Calculations.SegmentStates;
using System.Diagnostics;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public static class TankEdgeCalculator
    {
        public static Matrix<SegmentState[]> CalculateTankOuterEdges(
            CacheBasedSegmentStateCalculator segStateCalculator, 
            BitMatrix board)
        {
            int segX;
            int segY;
            Axis axisOfMovement;
            SegmentState segState;
            Direction dir;

            Matrix<SegmentState[]> tankEdgeMatrix 
                = new Matrix<SegmentState[]>(board.TopLeft, board.Width, board.Height);
            int topLeftX = board.TopLeft.X;
            int topLeftY = board.TopLeft.Y;
            int bottomRightX = board.BottomRight.X;
            int bottomRightY = board.BottomRight.Y;

            for (int x = topLeftX; x <= bottomRightX; x++)
            {
                for (int y = topLeftY; y <= bottomRightY; y++)
                {
                    SegmentState[] segStatesByDir = new SegmentState[Constants.EDGE_COUNT];
                    tankEdgeMatrix[x, y] = segStatesByDir;

                    // 1. Vertical edges:
                    axisOfMovement = Axis.Vertical;
                    segX = x;

                    // 1.1 Top edge of tank:
                    dir = Direction.UP;
                    segY = y - Constants.TANK_OUTER_EDGE_OFFSET;
                    if (segY >= topLeftY)
                    {
                        segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                        segStatesByDir[(byte)dir] = segState;
                    }

                    // 1.2 Bottom edge of tank:
                    dir = Direction.DOWN;
                    segY = y + Constants.TANK_OUTER_EDGE_OFFSET;
                    if (segY <= bottomRightY)
                    {
                        segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                        segStatesByDir[(byte)dir] = segState;
                    }

                    // 2. Horizontal edges:
                    axisOfMovement = Axis.Horizontal;
                    segY = y;

                    // 2.1 Left edge of tank:
                    dir = Direction.LEFT;
                    segX = segX - Constants.TANK_OUTER_EDGE_OFFSET;
                    if (segX >= topLeftX)
                    {
                        segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                        segStatesByDir[(byte)dir] = segState;
                    }

                    // 2.2 Right edge of tank:
                    dir = Direction.RIGHT;
                    segX = segX + Constants.TANK_OUTER_EDGE_OFFSET;
                    if (segX <= bottomRightX)
                    {
                        segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                        segStatesByDir[(byte)dir] = segState;
                    }
                }
            }
            return tankEdgeMatrix;
        }

        public static Matrix<SegmentState[]> CalculateTankInnerEdges(
            CacheBasedSegmentStateCalculator segStateCalculator,
            BitMatrix board)
        {
            int segX;
            int segY;
            Axis axisOfMovement;
            SegmentState segState;
            Direction dir;

            Matrix<SegmentState[]> tankEdgeMatrix
                = new Matrix<SegmentState[]>(board.TopLeft, board.Width, board.Height);
            int topLeftX = board.TopLeft.X;
            int topLeftY = board.TopLeft.Y;
            int bottomRightX = board.BottomRight.X;
            int bottomRightY = board.BottomRight.Y;

            for (int x = topLeftX; x <= bottomRightX; x++)
            {
                for (int y = topLeftY; y <= bottomRightY; y++)
                {
#if CONDITIONAL_BREAKPOINT_TankEdgeCalculator_CalculateTankInnerEdges
                    Debug.Assert(x != 28 || y != 72, "Conditional breakpoint");
#endif
                    SegmentState[] segStatesByDir = new SegmentState[Constants.EDGE_COUNT];
                    tankEdgeMatrix[x, y] = segStatesByDir;

                    // 1. Vertical edges:
                    axisOfMovement = Axis.Vertical;
                    segX = x;

                    // 1.1 Top edge of tank:
                    dir = Direction.UP;
                    segY = y - Constants.TANK_EXTENT_OFFSET;
                    if (segY >= topLeftY)
                    {
                        segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                        segStatesByDir[(byte)dir] = segState;
                    }

                    // 1.2 Bottom edge of tank:
                    dir = Direction.DOWN;
                    segY = y + Constants.TANK_EXTENT_OFFSET;
                    if (segY <= bottomRightY)
                    {
                        segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                        segStatesByDir[(byte)dir] = segState;
                    }

                    // 2. Horizontal edges:
                    axisOfMovement = Axis.Horizontal;
                    segY = y;

                    // 2.1 Left edge of tank:
                    dir = Direction.LEFT;
                    segX = x - Constants.TANK_EXTENT_OFFSET;
                    if (segX >= topLeftX)
                    {
                        segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                        segStatesByDir[(byte)dir] = segState;
                    }

                    // 2.2 Right edge of tank:
                    dir = Direction.RIGHT;
                    segX = x + Constants.TANK_EXTENT_OFFSET;
                    if (segX <= bottomRightX)
                    {
                        segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                        segStatesByDir[(byte)dir] = segState;
                    }
                }
            }
            return tankEdgeMatrix;
        }
    }
}
