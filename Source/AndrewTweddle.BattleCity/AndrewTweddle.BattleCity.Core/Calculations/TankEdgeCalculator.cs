using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public static class TankEdgeCalculator
    {
        public static Matrix<SegmentState[]> CalculateTankEdges(
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
            for (int x = board.TopLeft.X; x <= board.BottomRight.X; x++)
            {
                for (int y = board.TopLeft.Y; y <= board.BottomRight.Y; y++)
                {
                    SegmentState[] segStatesByDir = new SegmentState[Constants.EDGE_COUNT];
                    tankEdgeMatrix[x, y] = segStatesByDir;

                    // 1. Vertical edges:
                    axisOfMovement = Axis.Vertical;
                    segX = x;

                    // 1.1 Top edge of tank:
                    dir = Direction.UP;
                    segY = y - Constants.TANK_OUTER_EDGE_OFFSET;
                    segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                    segStatesByDir[(byte) dir] = segState;

                    // 1.2 Bottom edge of tank:
                    dir = Direction.DOWN;
                    segY = y + Constants.TANK_OUTER_EDGE_OFFSET;
                    segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                    segStatesByDir[(byte)dir] = segState;

                    // 2. Horizontal edges:
                    axisOfMovement = Axis.Horizontal;
                    segY = y;

                    // 2.1 Left edge of tank:
                    dir = Direction.LEFT;
                    segX = segX - Constants.TANK_OUTER_EDGE_OFFSET;
                    segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                    segStatesByDir[(byte)dir] = segState;

                    // 2.2 Right edge of tank:
                    dir = Direction.RIGHT;
                    segX = segX + Constants.TANK_OUTER_EDGE_OFFSET;
                    segState = segStateCalculator.GetSegmentState(axisOfMovement, segX, segY);
                    segStatesByDir[(byte)dir] = segState;
                }
            }
            return tankEdgeMatrix;
        }
    }
}
