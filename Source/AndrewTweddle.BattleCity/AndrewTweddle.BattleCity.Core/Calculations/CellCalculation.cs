using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class CellCalculation
    {
        #region Positional properties

        public Point Position { get; set; }
        public int PointIndex { get; set; }
        public bool IsValid { get; set; }
        public BitMatrixIndex BitMatrixIndex { get; set; }
        public CellCalculation[] AdjacentCellCalculationsByDirection { get; private set; }

        #endregion

        #region SegmentCalculation properties (for segment calculations centred on this cell)

        public SegmentCalculation[] SegmentCalculationsByAxis { get; private set; }

        #endregion

        #region Constructors

        public CellCalculation()
        {
            InitializePositionalProperties();
            InitializeSegmentCalculationProperties();
        }

        #endregion

        #region Positional Methods

        private void InitializePositionalProperties()
        {
            AdjacentCellCalculationsByDirection = new CellCalculation[Constants.ALL_DIRECTION_COUNT];
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

        #endregion

        #region SegmentCalculation methods

        private void InitializeSegmentCalculationProperties()
        {
            SegmentCalculationsByAxis = new SegmentCalculation[Constants.AXIS_COUNT];
        }

        public SegmentCalculation GetSegmentCalculationByDirection(Direction direction)
        {
            if (direction == Direction.NONE)
            {
                throw new ArgumentException(
                    "A segment centred on the cell can't be found since an invalid direction has been provided", "direction");
            }
            return SegmentCalculationsByAxis[(int) direction.ToAxis()];
        }

        public SegmentCalculation GetSegmentCalculationByAxis(Axis axis)
        {
            return SegmentCalculationsByAxis[(int)axis];
        }

        public void SetSegmentCalculationByAxis(Axis axis, SegmentCalculation calculation)
        {
            SegmentCalculationsByAxis[(int)axis] = calculation;
        }

        #endregion
    }
}
