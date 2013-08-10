using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Helpers
{
    public static class BoardHelper
    {
        public static Direction[] AllDirections;
        public static Direction[] AllRealDirections;
        public static Axis[] AllRealAxes;

        static BoardHelper()
        {
            AllDirections = Enumerable.Range(0, Constants.ALL_DIRECTION_COUNT).Select(i => (Direction) i).ToArray();
            AllRealDirections = Enumerable.Range(0, Constants.RELEVANT_DIRECTION_COUNT).Select(i => (Direction) i).ToArray();
            AllRealAxes = new[] { (Axis)0, (Axis)1 };
        }

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
    }
}
