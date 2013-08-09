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
    /// A segment represents 5 cells in a line centred on a Centre point.
    /// This can be used to represent an inside our outside edge of a tank 
    /// or a segment of a wall (for calculating the effects of a hit on the centre of the wall).
    /// </summary>
    public class Segment
    {
        public Point Centre { get; set; }
        public Axis Axis { get; set; }

        public Cell[] GetCellsOnSegment(BitMatrix walls)
        {
            Cell[] cells = new Cell[5];
            CellState cellState;
            bool checkLowerBound = true;
            bool isUpperBoundExceeded = false;
            byte i = 0;

            switch (Axis)
            {
                case Core.Axis.Horizontal:
                    for (short x = (short)(Centre.X - Constants.TANK_EXTENT_OFFSET); x <= (short)(Centre.X + Constants.TANK_EXTENT_OFFSET); x++, i++)
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
                            cells[i] = new Cell(pos:new Point(x, Centre.Y), state:cellState);
                        }
                    }
                    break;

                case Core.Axis.Vertical:
                    for (short y = (short)(Centre.Y - Constants.TANK_EXTENT_OFFSET); y <= (short)(Centre.Y + Constants.TANK_EXTENT_OFFSET); y++)
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
                            cells[i] = new Cell(pos: new Point(Centre.X, y), state: cellState);
                        }
                    }
                    break;
            }
            return cells;
        }
    }
}
