using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public static class CellCalculator
    {
        // TODO: No rather cache on the Game itself (or add a GameTick object) - reason is that after tick 200, board will start contracting
        // public static Matrix<Cell> Cells{ get; private set; }

        public static Matrix<Cell> Calculate(BitMatrix board)
        {
            // Widen the matrix slightly, so that points just off the board are also considered:
            Point topLeft = new Point(-Constants.TANK_EXTENT_OFFSET, -Constants.TANK_EXTENT_OFFSET);
            Point bottomRight = new Point(
                (short) (board.Width + Constants.TANK_EXTENT_OFFSET - 1),
                (short) (board.Height + Constants.TANK_EXTENT_OFFSET - 1));
            int extendedWidth = board.Width + 2 * Constants.TANK_EXTENT_OFFSET;
            int extendedHeight = board.Height + 2 * Constants.TANK_EXTENT_OFFSET;
            Matrix<Cell> matrix = new Matrix<Cell>(topLeft, extendedWidth, extendedHeight);

            // Do the cell calculations:
            bool isFirstColumn = true;
            for (int x = topLeft.X; x <= bottomRight.X; x++)
            {
                Cell upCell = null;
                for (int y = topLeft.Y; y <= bottomRight.Y; y++)
                {
                    Cell newCell = new Cell();
                    newCell.IsValid = (x >= 0) && (y >= 0) && (x < board.Width) && (y < board.Height);
                    newCell.Position = new Point((short) x, (short) y);
                    // Removed to improve performance:
                    // newCell.PointIndex = newCell.Position.BoardIndex;
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
