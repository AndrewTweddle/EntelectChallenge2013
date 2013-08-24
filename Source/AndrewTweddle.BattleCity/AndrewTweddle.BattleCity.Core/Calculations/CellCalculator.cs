using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public static class CellCalculator
    {
        public static Matrix<Cell> Calculate(BitMatrix board, int leftBoundary, int rightBoundary)
        {
            // Widen the matrix slightly, so that points just off the board are also considered:
            Point topLeft = new Point(-Constants.TANK_OUTER_EDGE_OFFSET, -Constants.TANK_OUTER_EDGE_OFFSET);
            Point bottomRight = new Point(
                (short)(board.Width + Constants.TANK_OUTER_EDGE_OFFSET - 1),
                (short)(board.Height + Constants.TANK_OUTER_EDGE_OFFSET - 1));
            int extendedWidth = board.Width + 2 * Constants.TANK_OUTER_EDGE_OFFSET;
            int extendedHeight = board.Height + 2 * Constants.TANK_OUTER_EDGE_OFFSET;
            Matrix<Cell> matrix = new Matrix<Cell>(topLeft, extendedWidth, extendedHeight);

            // Do the cell calculations:
            bool isFirstColumn = true;
            for (int x = topLeft.X; x <= bottomRight.X; x++)
            {
                Cell upCell = null;
                for (int y = topLeft.Y; y <= bottomRight.Y; y++)
                {
                    Cell newCell = new Cell();
                    newCell.IsValid = (x >= leftBoundary) && (y >= 0) && (x <= rightBoundary) && (y < board.Height);
                    newCell.Position = new Point((short) x, (short) y);
                    if (newCell.IsValid)
                    {
                        newCell.BitIndexAndMask = board.GetBitMatrixMask(x, y);
                    }
                    if (upCell != null)
                    {
                        newCell.SetAdjacentCell(Direction.UP, upCell);
                        upCell.SetAdjacentCell(Direction.DOWN, newCell);
                    }
                    if (!isFirstColumn)
                    {
                        Cell leftCell = matrix[x-1, y];
                        newCell.SetAdjacentCell(Direction.LEFT, leftCell);
                        leftCell.SetAdjacentCell(Direction.RIGHT, newCell);
                    }
                    matrix[x, y] = newCell;
                    upCell = newCell;
                }
                isFirstColumn = false;
            }

            return matrix;
        }
    }
}
