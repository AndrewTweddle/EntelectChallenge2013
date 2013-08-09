using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Helpers
{
    public static class DirectionHelper
    {
        public static int GetXOffsetForDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.DOWN:
                    return 1;
                case Direction.UP:
                    return -1;
                default:
                    return 0;
            }
        }

        public static int GetYOffsetForDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.RIGHT:
                    return 1;
                case Direction.LEFT:
                    return -1;
                default:
                    return 0;
            }
        }

        public static Direction GetOppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.UP:
                    return Direction.DOWN;
                case Direction.DOWN:
                    return Direction.UP;
                case Direction.LEFT:
                    return Direction.RIGHT;
                case Direction.RIGHT:
                    return Direction.LEFT;
                default:
                    return Direction.NONE;
            }
        }

        /// <summary>
        /// The end of the barrel is a point on the edge of the tank where the bullet will emerge
        /// </summary>
        /// <param name="tankPos">The centre of the tank</param>
        /// <param name="tankDir">The direction in which the tank is pointing</param>
        /// <returns></returns>
        public static Position GetEndOfBarrel(Position tankPos, Direction tankDir)
        {
            Position endOfBarrel = tankPos;
            endOfBarrel.X += GetXOffsetForDirection(tankDir) * Constants.TANK_EXTENT_OFFSET;
            endOfBarrel.Y += GetYOffsetForDirection(tankDir) * Constants.TANK_EXTENT_OFFSET;
            return endOfBarrel;
        }

        public static Position[] GetAdjacentWallPositions(Game game, Position centrePoint, Direction dir)
        {
            int xOffset;
            int yOffset;
            int xStart;
            int xEnd;
            int yStart;
            int yEnd;
            int size;

            switch (dir)
            {
                case Direction.DOWN:
                case Direction.UP:
                    xOffset = 1;
                    yOffset = 0;
                    yStart = yEnd = centrePoint.Y;

                    // Left-most point:
                    xStart = centrePoint.X - Constants.TANK_EXTENT_OFFSET;
                    if (xStart < 0)
                    {
                        xStart = 0;
                    }

                    // Right-most point:
                    xEnd = centrePoint.X + Constants.TANK_EXTENT_OFFSET;
                    if (xEnd >= game.BoardWidth)
                    {
                        xEnd = game.BoardWidth - 1;
                    }

                    size = xStart - xEnd + 1;
                    break;

                case Direction.LEFT:
                case Direction.RIGHT:
                    xOffset = 0;
                    yOffset = 1;
                    xStart = xEnd = centrePoint.X;

                    // Top-most point:
                    yStart = centrePoint.Y - Constants.TANK_EXTENT_OFFSET;
                    if (yStart < 0)
                    {
                        yStart = 0;
                    }

                    // Bottom array:
                    yEnd = centrePoint.Y + Constants.TANK_EXTENT_OFFSET;
                    if (yEnd >= game.BoardHeight)
                    {
                        yEnd = game.BoardHeight - 1;
                    }

                    size = yStart - yEnd + 1;
                    break;

                default:
                    return null; // or: new Position[][] {new Position[0]};
            }

            int x;
            int y;
            Position[] positions = new Position[size];
            for (int i = 0; i < size; i++)
            {
                x = xStart + xOffset * i;
                y = yStart + yOffset * i;
                positions[i].X = x;
                positions[i].Y = y;
            }
            return positions;
        }

        public static Position[][] GetAdjacentWallPositionsInTwoSegments(Game game, Position impactPoint, Direction bulletDir)
        {
            int xOffset1;
            int yOffset1;
            int xStart1;
            int xEnd1;
            int yStart1;
            int yEnd1;
            int size1;

            int xOffset2;
            int yOffset2;
            int xStart2;
            int xEnd2;
            int yStart2;
            int yEnd2;
            int size2;

            switch (bulletDir)
            {
                case Direction.DOWN:
                case Direction.UP:
                    yOffset1 = 0;
                    yOffset2 = 0;
                    yStart2 = yEnd2 = yStart1 = yEnd1 = impactPoint.Y;

                    // Left array:
                    xOffset1 = -1;

                    xStart1 = impactPoint.X;
                    xEnd1 = impactPoint.X - Constants.TANK_EXTENT_OFFSET;
                    if (xStart1 < 0)
                    {
                        xStart1 = 0;
                    }

                    // Right array:
                    xOffset2 = 1;
                    xStart2 = impactPoint.X + 1;
                    if (xStart2 == game.BoardWidth)
                    {
                        xEnd2 = xStart2 = xStart1;
                        xStart1--;
                    }
                    else
                    {
                        xEnd2 = impactPoint.X + Constants.TANK_EXTENT_OFFSET;
                        if (xEnd2 >= game.BoardWidth)
                        {
                            xEnd2 = game.BoardWidth - 1;
                        }
                    }

                    size1 = xStart1 - xEnd1 + 1;
                    size2 = xEnd2 - xStart2 + 1;

                    break;

                case Direction.LEFT:
                case Direction.RIGHT:
                    xOffset1 = 0;
                    xOffset2 = 0;
                    xStart2 = xEnd2 = xStart1 = xEnd1 = impactPoint.X;

                    // Top array:
                    yOffset1 = -1;

                    yStart1 = impactPoint.Y;
                    yEnd1 = impactPoint.Y - Constants.TANK_EXTENT_OFFSET;
                    yStart1 = yEnd1 = impactPoint.Y;
                    if (yStart1 < 0)
                    {
                        yStart1 = 0;
                    }

                    // Bottom array:
                    yOffset2 = 1;
                    yStart2 = impactPoint.Y + 1;
                    if (yStart2 == game.BoardHeight)
                    {
                        yEnd2 = yStart2 = yStart1;
                        yStart1--;
                    }
                    else
                    {
                        yEnd2 = impactPoint.Y + Constants.TANK_EXTENT_OFFSET;
                        if (yEnd2 >= game.BoardHeight)
                        {
                            yEnd2 = game.BoardHeight - 1;
                        }
                    }

                    size1 = yStart1 -yEnd1 + 1;
                    size2 = yEnd2 - yStart2 + 1;

                    break;

                default:
                    return null; // or: new Position[][] {new Position[0]};
            }

            int x;
            int y;
            Position[] positions1 = new Position[size1];
            Position[] positions2 = new Position[size2];
            Position[][] positions = new Position[][] {positions1, positions2};
            for (int i1 = 0; i1 < size1; i1++)
            {
                x = xStart1 + xOffset1 * i1;
                y = yStart1 + yOffset1 * i1;
                positions1[i1].X = x;
                positions1[i1].Y = y;
            }
            for (int i2 = 0; i2 < size2; i2++)
            {
                x = xStart2 + xOffset2 * i2;
                y = yStart2 + yOffset2 * i2;
                positions1[i2].X = x;
                positions1[i2].Y = y;
            }
            return positions;
        }
    }
}
