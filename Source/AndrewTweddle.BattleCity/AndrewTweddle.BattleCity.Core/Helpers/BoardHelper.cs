using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;

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

        #region Methods to generate a text-based representation of the board state:

        public static string[] GenerateTextImageOfBoard(GameState gameState)
        {
            int boardHeight = Game.Current.BoardHeight;
            int boardWidth = Game.Current.BoardWidth;
            char[,] cells = new char[boardWidth, boardHeight];

            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    cells[x, y] = '-';
                }
            }

            for (int i = 0; i < Constants.TANK_COUNT; i++)
            {
                MobileState tankState = gameState.GetMobileState(i);
                Tank tank = Game.Current.Elements[i] as Tank;
                if (tank != null)
                {
                    char tankChar = (char) ( (int) 'A' + i);
                    char dirChar = Enum.GetName(typeof(Direction), tankState.Dir)[0];
                    int xBarrel, yBarrel;
                    switch (tankState.Dir)
                    {
                        case Direction.DOWN:
                            xBarrel = tankState.Pos.X;
                            yBarrel = tankState.Pos.Y + 1;
                            break;
                        case Direction.LEFT:
                            xBarrel = tankState.Pos.X - 1;
                            yBarrel = tankState.Pos.Y;
                            break;
                        case Direction.RIGHT:
                            xBarrel = tankState.Pos.X + 1;
                            yBarrel = tankState.Pos.Y;
                            break;
                        case Direction.UP:
                            xBarrel = tankState.Pos.X;
                            yBarrel = tankState.Pos.Y - 1;
                            break;
                        default: // case Direction.NONE:
                            // Choose values which will be ignored:
                            xBarrel = tankState.Pos.X;
                            yBarrel = tankState.Pos.Y;
                            break;
                    }

                    for (int x = tankState.Pos.X - 2; x <= tankState.Pos.X + 2; x++)
                    {
                        for (int y = tankState.Pos.Y - 2; y <= tankState.Pos.Y + 2; y++)
                        {
                            if (x == tankState.Pos.X && y == tankState.Pos.Y)
                            {
                                cells[x, y] = dirChar;
                            }
                            else if ((x == tankState.Pos.X - 2) || (x == tankState.Pos.X + 2) || (y == tankState.Pos.Y - 2) || (y == tankState.Pos.Y + 2))
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
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    if (gameState.Walls[x, y])
                    {
                        cells[x, y] = '#';
                    }
                }
            }

            // Mark bases:
            foreach (Player player in Game.Current.Players)
            {
                cells[player.Base.Pos.X, player.Base.Pos.Y] = player.Index.ToString()[0];
            }

            // Mark bullets:
            for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
            {
                MobileState bulletState = gameState.GetMobileState(b);
                if (bulletState.IsActive && bulletState.Dir != Direction.NONE)
                {
                    char bulletChar;
                    switch (bulletState.Dir)
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
                    cells[bulletState.Pos.X, bulletState.Pos.Y] = bulletChar;
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
