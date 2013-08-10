using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Helpers
{
    public static class BoardHelper
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
    }
}
