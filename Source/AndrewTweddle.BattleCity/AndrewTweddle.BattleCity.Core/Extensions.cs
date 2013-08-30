using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core
{
    public static class Extensions
    {
        public static Axis GetPerpendicular(this Axis axis)
        {
            return (Axis)(1 - axis);
        }

        public static Direction GetOpposite(this Direction direction)
        {
            switch (direction)
            {
                case Direction.DOWN:
                    return Direction.UP;
                case Direction.LEFT:
                    return Direction.RIGHT;
                case Direction.RIGHT:
                    return Direction.LEFT;
                case Direction.UP:
                    return Direction.DOWN;
                default:
                    // case Direction.NONE:
                    return Direction.NONE;
            }
        }

        public static Axis ToAxis(this Direction direction)
        {
            switch (direction)
            {
                case Direction.DOWN:
                case Direction.UP:
                    return Axis.Vertical;
                case Direction.LEFT:
                case Direction.RIGHT:
                    return Axis.Horizontal;
                default:
                    // case Direction.NONE:
                    return Axis.None;
            }
        }

        public static Direction[] ToDirections(this Axis axis)
        {
            switch (axis)
            {
                case Axis.Horizontal:
                    return new[] { Direction.LEFT, Direction.RIGHT };
                case Axis.Vertical:
                    return new[] { Direction.UP, Direction.DOWN };
                default:
                    return new Direction[0];
            }
        }

        public static short GetXOffset(this Direction dir)
        {
            switch (dir)
            {
                case Direction.DOWN:
                case Direction.UP:
                    return 0;
                case Direction.LEFT:
                    return -1;
                case Direction.RIGHT:
                    return 1;
                default:
                    // case Direction.NONE:
                    return 0;
            }
        }

        public static short GetYOffset(this Direction dir)
        {
            switch (dir)
            {
                case Direction.DOWN:
                    return 1;
                case Direction.UP:
                    return -1;
                case Direction.LEFT:
                case Direction.RIGHT:
                    return 0;
                default:
                    // case Direction.NONE:
                    return 0;
            }
        }

        public static Point GetOffset(this Direction dir)
        {
            switch (dir)
            {
                case Direction.DOWN:
                    return new Point(0, 1);
                case Direction.UP:
                    return new Point(0, -1);
                case Direction.LEFT:
                    return new Point(-1, 0);
                case Direction.RIGHT:
                    return new Point(1, 0);
                default:
                    // case Direction.NONE:
                    return new Point();
            }
        }

        public static Direction Clockwise(this Direction dir)
        {
            switch (dir)
            {
                case Direction.DOWN:
                    return Direction.LEFT;
                case Direction.UP:
                    return Direction.RIGHT;
                case Direction.LEFT:
                    return Direction.UP;
                case Direction.RIGHT:
                    return Direction.DOWN;
                default:
                    return Direction.NONE;
            }
        }

        public static Direction AntiClockwise(this Direction dir)
        {
            switch (dir)
            {
                case Direction.DOWN:
                    return Direction.RIGHT;
                case Direction.UP:
                    return Direction.LEFT;
                case Direction.LEFT:
                    return Direction.DOWN;
                case Direction.RIGHT:
                    return Direction.UP;
                default:
                    return Direction.NONE;
            }
        }

        public static TankAction ToTankAction(this Direction dir)
        {
            return (TankAction)dir;
        }

        public static Direction ToDirection(this TankAction action, Direction currentDirection = Direction.NONE)
        {
            switch (action)
            {
                case TankAction.DOWN:
                case TankAction.LEFT:
                case TankAction.RIGHT:
                case TankAction.UP:
                    return (Direction)action;
                default:
                    // case TankAction.NONE:
                    // case TankAction.FIRE:
                    return currentDirection;
            }
        }

        public static ElementExtentType ToExtentType(ElementType elementType)
        {
            if (elementType == ElementType.TANK)
            {
                return ElementExtentType.TankBody;
            }
            return ElementExtentType.Point;
        }
    }
}
