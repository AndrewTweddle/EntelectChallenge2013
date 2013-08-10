using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class CellCalculation
    {
        public Point Position { get; set; }
        public int PointIndex { get; set; }
        public bool IsValid { get; set; }
        public BitMatrixIndex BitMatrixIndex { get; set; }
        public CellCalculation[] AdjacentCellCalculationsByDirection { get; private set; }

        public CellCalculation()
        {
            AdjacentCellCalculationsByDirection = new CellCalculation[Enum.GetValues(typeof(Direction)).Length];
            SetAdjacentCellCalculation(Direction.NONE, this);
        }

        public CellCalculation GetAdjacentCellCalculation(Direction direction)
        {
            return AdjacentCellCalculationsByDirection[(int)direction];
        }

        public void SetAdjacentCellCalculation(Direction direction, CellCalculation adjacentCalculation)
        {
            AdjacentCellCalculationsByDirection[(int)direction] = adjacentCalculation;
        }
    }
}
