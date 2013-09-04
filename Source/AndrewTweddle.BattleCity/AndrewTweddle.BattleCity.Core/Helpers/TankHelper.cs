using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Helpers
{
    public static class TankHelper
    {
        public static EdgeOffset[] EdgeOffsets { get; private set; }
        public static TankAction[] TankActions { get; private set; }

        static TankHelper()
        {
            EdgeOffsets = (EdgeOffset[]) Enum.GetValues(typeof(EdgeOffset));
            TankActions = (TankAction[])Enum.GetValues(typeof(TankAction));
        }

        public static Point GetTankFiringPoint(this MobileState tankState)
        {
            return GetTankFiringPoint(tankState.Pos, tankState.Dir);
        }

        public static Point GetTankFiringPoint(Point tankPos, Direction tankDir)
        {
            short x = tankPos.X;
            short y = tankPos.Y;
            short xOffset = 0;
            short yOffset = 0;

            switch (tankDir)
            {
                case Direction.DOWN:
                    yOffset = 3;
                    break;
                case Direction.LEFT:
                    xOffset = -3;
                    break;
                case Direction.RIGHT:
                    xOffset = 3;
                    break;
                case Direction.UP:
                    yOffset = -3;
                    break;
            }
            x += xOffset;
            y += yOffset;

            if (x < 0 || y < 0 || x >= Game.Current.BoardWidth || y >= Game.Current.BoardHeight)
            {
                // Move is invalid due to being off the edge of the board. Use tank's current position to signal this:
                return tankPos;
            }
            else
            {
                return new Point((short) x, (short) y);
            }
        }

        // TODO: Cache these offsets
        public static Point GetOffsetToPointOnTankEdge(Direction edge, EdgeOffset edgeOffset)
        {
            Point offsetToCentreOfEdge = Constants.TANK_EXTENT_OFFSET * edge.GetOffset();
            Direction perpendicular = Direction.NONE;
            int multiplier = 1;
            switch (edgeOffset)
            {
                case EdgeOffset.Centre:
                    return offsetToCentreOfEdge;
                case EdgeOffset.OffCentreAntiClockwise:
                    perpendicular = edge.AntiClockwise();
                    break;
                case EdgeOffset.OffCentreClockwise:
                    perpendicular = edge.Clockwise();
                    break;
                case EdgeOffset.CornerAntiClockwise:
                    perpendicular = edge.AntiClockwise();
                    multiplier = 2;
                    break;
                case EdgeOffset.CornerClockwise:
                    perpendicular = edge.Clockwise();
                    multiplier = 2;
                    break;
            }
            return offsetToCentreOfEdge + multiplier * perpendicular.GetOffset();
        }

        public static Point GetPointOnTankEdge(Point centreOfTank, Direction edge, EdgeOffset edgeOffset)
        {
            return centreOfTank + GetOffsetToPointOnTankEdge(edge, edgeOffset);
        }

        public static Point GetPointOnTankEdge(int centreX, int centreY, Direction edge, EdgeOffset edgeOffset)
        {
            return GetPointOnTankEdge(new Point((short) centreX, (short) centreY), edge, edgeOffset);
        }
    }
}
