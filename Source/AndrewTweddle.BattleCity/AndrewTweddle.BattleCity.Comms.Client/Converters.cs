using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.Comms.Client
{
    public static class Converters
    {
        public static Direction Convert(this direction dir)
        {
            switch (dir)
            {
                case direction.DOWN:
                    return Direction.DOWN;
                case direction.LEFT:
                    return Direction.LEFT;
                case direction.RIGHT:
                    return Direction.RIGHT;
                case direction.UP:
                    return Direction.UP;
                default:
                    // case direction.NONE:
                    return Direction.NONE;
            }
        }

        public static TankAction Convert(this action action)
        {
            switch (action)
            {
                case global::action.DOWN:
                    return TankAction.DOWN;
                case global::action.FIRE:
                    return TankAction.FIRE;
                case global::action.LEFT:
                    return TankAction.LEFT;
                case global::action.RIGHT:
                    return TankAction.RIGHT;
                case global::action.UP:
                    return TankAction.UP;
                default:
                    // case action.NONE
                    return TankAction.NONE;
            }
        }

        public static action Convert(this TankAction tankAction)
        {
            switch (tankAction)
            {
                case TankAction.DOWN:
                    return global::action.DOWN;
                case TankAction.FIRE:
                    return global::action.FIRE;
                case TankAction.LEFT:
                    return global::action.LEFT;
                case TankAction.RIGHT:
                    return global::action.RIGHT;
                case TankAction.UP:
                    return global::action.UP;
                default:
                    // case TankAction.NONE
                    return global::action.NONE;
            }
        }

    }
}
