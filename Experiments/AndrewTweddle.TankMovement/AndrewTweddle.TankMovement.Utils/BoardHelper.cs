using AndrewTweddle.TankMovement.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace AndrewTweddle.TankMovement.Utils
{
    public static class BoardHelper
    {
        #region Methods to help set up the board:

        public static void AddWallsToBoard(ref bool[,] isWall, Core.Rectangle walls)
        {
            IEnumerable<Core.Point> wallCells = walls.GetPoints();
            foreach (Core.Point wallCell in wallCells)
            {
                if (wallCell.X >= 0 && wallCell.X < isWall.GetLength(0) && wallCell.Y >= 0 && wallCell.Y < isWall.GetLength(1))
                {
                    isWall[wallCell.X, wallCell.Y] = true;
                }
            }
        }

        #endregion

        #region Methods to generate a text-based representation of the board state:

        public static string[] GenerateViewOfBoard(ref bool[,] isWall, Unit[] units, Bullet[] bullets)
        {
            char[,] cells = new char[isWall.GetLength(0), isWall.GetLength(1)];

            for (int x = 0; x < isWall.GetLength(0); x++)
            {
                for (int y = 0; y < isWall.GetLength(1); y++)
                {
                    cells[x, y] = '-';
                }
            }

            for (int i = 0; i < 4; i++)
            {
                Unit tank = units[i];
                if (tank != null)
                {
                    char tankChar = (char) ( (int) 'A' + i);
                    char actionChar = Enum.GetName(typeof(Core.Action), tank.Action)[0];
                    int xBarrel, yBarrel;
                    switch (tank.Direction)
                    {
                        case Direction.DOWN:
                            xBarrel = tank.X;
                            yBarrel = tank.Y + 1;
                            break;
                        case Direction.LEFT:
                            xBarrel = tank.X - 1;
                            yBarrel = tank.Y;
                            break;
                        case Direction.RIGHT:
                            xBarrel = tank.X + 1;
                            yBarrel = tank.Y;
                            break;
                        case Direction.UP:
                            xBarrel = tank.X;
                            yBarrel = tank.Y - 1;
                            break;
                        default: // case Direction.NONE:
                            // Choose values which will be ignored:
                            xBarrel = tank.X;
                            yBarrel = tank.Y;
                            break;
                    }

                    for (int x = tank.X - 2; x <= tank.X + 2; x++)
                    {
                        for (int y = tank.Y - 2; y <= tank.Y + 2; y++)
                        {
                            if (x == tank.X && y == tank.Y)
                            {
                                cells[x, y] = actionChar;
                            }
                            else if ((x == tank.X - 2) || (x == tank.X + 2) || (y == tank.Y - 2) || (y == tank.Y + 2))
                            {
                                cells[x, y] = tankChar;
                            }
                            else if (x == xBarrel && y == yBarrel)
                            {
                                cells[x, y] = tankChar;
                            }
                            else
                            {
                            cells[x, y] = ' ';
                            }
                        }
                    }
                }
            }

            // Mark wall spaces. Do after tanks, to detect errors with walls overlapping with tanks:
            for (int x = 0; x < isWall.GetLength(0); x++)
            {
                for (int y = 0; y < isWall.GetLength(1); y++)
                {
                    if (isWall[x, y])
                    {
                        cells[x, y] = '#';
                    }
                }
            }

            // Mark bullets:
            if (bullets != null)
            {
                foreach (Bullet bullet in bullets)
                {
                    if (bullet != null && bullet.Direction != Direction.NONE)
                    {
                        char bulletChar;
                        switch (bullet.Direction)
                        {
                            case Direction.DOWN:
                                bulletChar = 'v';
                                break;
                            case Direction.LEFT:
                                bulletChar = '<';
                                break;
                            case Direction.RIGHT:
                                bulletChar = '>';
                                break;
                            case Direction.UP:
                            default:
                                bulletChar = '^';
                                break;
                        }
                        cells[bullet.X, bullet.Y] = bulletChar;
                    }
                }
            }

            // Turn into an array of strings:
            string[] rows = new string[3 + cells.GetLength(1)];
            for (int y = 0; y < rows.Length - 3; y++)
            {
                string prefix = "    ";
                if (y % 5 == 0)
                {
                    prefix = String.Format("{0,3} ", y);
                }
                StringBuilder sb = new StringBuilder(prefix);
                for (int x = 0; x < cells.GetLength(0); x++)
                {
                    sb.Append(cells[x, y]);
                }
                rows[y] = sb.ToString();
            }

            for (int digitPosition = 0; digitPosition < 3; digitPosition++)
            {
                StringBuilder sb = new StringBuilder("    ");
                for (int x = 0; x < cells.GetLength(1); x++)
                {
                    char digit = ' ';
                    if (x % 5 == 0)
                    {
                        string xAsString = String.Format("{0,-3}", x);
                        digit = xAsString[digitPosition];
                    }
                    sb.Append(digit);
                }
                rows[rows.Length - 3 + digitPosition] = sb.ToString();
            }
            return rows;
        }

        #endregion
    }
}
