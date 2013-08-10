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
        // public static Matrix<CellCalculation> CellCalculations { get; private set; }

        public static Matrix<CellCalculation> Calculate(BitMatrix board)
        {
            // Widen the matrix slightly, so that points just off the board are also considered:
            Point topLeft = new Point(-Constants.TANK_EXTENT_OFFSET, -Constants.TANK_EXTENT_OFFSET);
            short extendedWidth = (short) (board.Width + 2 * Constants.TANK_EXTENT_OFFSET);
            short extendedHeight = (short) (board.Height + 2 * Constants.TANK_EXTENT_OFFSET);
            Matrix<CellCalculation> matrix = new Matrix<CellCalculation>(topLeft, extendedWidth, extendedHeight);

            // Do the cell calculations:
            bool isFirstColumn = true;
            for (int x = topLeft.X; x < extendedWidth; x++)
            {
                CellCalculation upCalculation = null;
                for (int y = topLeft.Y; y < extendedHeight; y++)
                {
                    CellCalculation calculation = new CellCalculation();
                    calculation.IsValid = (x >= 0) && (y >= 0) && (x < board.Width) && (y < board.Height);
                    calculation.Position = new Point((short) x, (short) y);
                    calculation.PointIndex = calculation.Position.BoardIndex;
                    if (calculation.IsValid)
                    {
                        calculation.BitMatrixIndex = board.GetBitMatrixIndex(x, y);
                    }
                    if (upCalculation != null)
                    {
                        calculation.SetAdjacentCellCalculation(Direction.UP, upCalculation);
                        upCalculation.SetAdjacentCellCalculation(Direction.DOWN, calculation);
                    }
                    if (!isFirstColumn)
                    {
                        CellCalculation leftCalculation = matrix[x-1, y];
                        calculation.SetAdjacentCellCalculation(Direction.LEFT, leftCalculation);
                        leftCalculation.SetAdjacentCellCalculation(Direction.RIGHT, calculation);
                    }
                    matrix[x, y] = calculation;
                    upCalculation = calculation;
                }
                isFirstColumn = false;
            }

            return matrix;
        }

    }
}
