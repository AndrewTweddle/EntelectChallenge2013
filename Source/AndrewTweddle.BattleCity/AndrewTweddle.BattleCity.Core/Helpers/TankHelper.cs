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
                    yOffset = 2;
                    break;
                case Direction.LEFT:
                    xOffset = -2;
                    break;
                case Direction.RIGHT:
                    xOffset = 2;
                    break;
                case Direction.UP:
                    yOffset = -2;
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
    }
}
