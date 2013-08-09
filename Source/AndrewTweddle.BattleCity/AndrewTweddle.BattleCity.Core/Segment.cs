using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core
{
    /// <summary>
    /// A segment represents 5 cells in a line.
    /// This can be used to represent an inside our outside edge of a tank 
    /// or a segment of a wall (for calculating the effects of a hit on the centre of the wall).
    /// </summary>
    public class Segment
    {
        public Point Centre;
        public Axis Axis;

        public IEnumerable<Cell> GetCellsOnSegment(BitMatrix walls, bool includeOutOfBounds = true)
        {
            CellState cellState;
            bool checkLowerBound = true;
            bool isUpperBoundExceeded = false;

            switch (Axis)
            {
                case Core.Axis.Horizontal:
                    for (short x = (short) (Centre.X - Constants.TANK_EXTENT_OFFSET); x < (short) (Centre.X + Constants.TANK_EXTENT_OFFSET); x++)
                    {
                        if (checkLowerBound)
                        {
                            if (isUpperBoundExceeded)
                            {
                                cellState = CellState.OutOfBounds;
                            }
                            else
                            {
                                if (x < 0)
                                {
                                    cellState = CellState.OutOfBounds;
                                }
                                else
                                {
                                    checkLowerBound = false;
                                    if (x >= Game.Current.BoardWidth)
                                    {
                                        isUpperBoundExceeded = true;
                                        cellState = CellState.OutOfBounds;
                                    }
                                    else
                                    {
                                        cellState = walls[x, Centre.Y] ? CellState.Wall : CellState.Empty;
                                    }
                                }
                            }
                            if (includeOutOfBounds || cellState != CellState.OutOfBounds)
                            {
                                yield return new Cell { Pos = new Point( x, Centre.Y), State = cellState };
                            }
                        }
                    }
                    break;

                case Core.Axis.Vertical:
                    for (short y = (short) (Centre.Y - Constants.TANK_EXTENT_OFFSET); y < (short) (Centre.Y + Constants.TANK_EXTENT_OFFSET); y++)
                    {
                        if (checkLowerBound)
                        {
                            if (isUpperBoundExceeded)
                            {
                                cellState = CellState.OutOfBounds;
                            }
                            else
                            {
                                if (y < 0)
                                {
                                    cellState = CellState.OutOfBounds;
                                }
                                else
                                {
                                    checkLowerBound = false;
                                    if (y >= Game.Current.BoardWidth)
                                    {
                                        isUpperBoundExceeded = true;
                                        cellState = CellState.OutOfBounds;
                                    }
                                    else
                                    {
                                        cellState = walls[Centre.X, y] ? CellState.Wall : CellState.Empty;
                                    }
                                }
                            }
                            if (includeOutOfBounds || cellState != CellState.OutOfBounds)
                            {
                                yield return new Cell { Pos = new Point( Centre.X, y), State = cellState };
                            }
                        }
                    }
                    break;
            }
        }
    }
}
