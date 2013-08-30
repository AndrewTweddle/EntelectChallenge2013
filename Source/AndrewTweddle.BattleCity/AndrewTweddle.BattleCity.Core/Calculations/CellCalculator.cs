using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public static class CellCalculator
    {
        public static Matrix<Cell> Calculate(BitMatrix board, int leftBoundary, int rightBoundary)
        {
            // Widen the matrix slightly, so that points just off the board are also considered:
            Point topLeft = new Point(-Constants.SEGMENT_SIZE, -Constants.SEGMENT_SIZE);
            Point bottomRight = new Point(
                (short)(board.Width + Constants.SEGMENT_SIZE - 1),
                (short)(board.Height + Constants.SEGMENT_SIZE - 1));
            int extendedWidth = board.Width + 2 * Constants.SEGMENT_SIZE;
            int extendedHeight = board.Height + 2 * Constants.SEGMENT_SIZE;
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

                    // Calculate lines from cell in each direction:
                    CalculateLinesFromCellInEachDirection(newCell, board, leftBoundary, rightBoundary);
                }
                isFirstColumn = false;
            }

            return matrix;
        }

        private static void CalculateLinesFromCellInEachDirection(Cell newCell, BitMatrix board, int leftBoundary, int rightBoundary)
        {
            int[] lineLengths = new int[Constants.RELEVANT_DIRECTION_COUNT];
            lineLengths[(int)Direction.LEFT] = newCell.IsValid ? (newCell.Position.X - leftBoundary + 1) : 0;
            lineLengths[(int)Direction.RIGHT] = newCell.IsValid ? (rightBoundary - newCell.Position.X + 1) : 0;
            lineLengths[(int)Direction.UP] = newCell.IsValid ? (newCell.Position.Y + 1) : 0;
            lineLengths[(int)Direction.DOWN] = newCell.IsValid ? (board.Height - newCell.Position.Y + 1) : 0;

            foreach (Direction dir in BoardHelper.AllRealDirections)
            {
                int lineLength = lineLengths[(int)dir];
                Line<Point> line = new Line<Point>(newCell.Position, dir, lineLength);
                newCell.LineFromCellToEdgeOfBoardByDirection[(int)dir] = line;
                Point currPoint = newCell.Position;
                Point offset = dir.GetOffset();
                for (int i = 0; i < lineLength; i++)
                {
                    line[i] = currPoint;
                    currPoint = currPoint + offset;
                }
            }
        }
    }
}
