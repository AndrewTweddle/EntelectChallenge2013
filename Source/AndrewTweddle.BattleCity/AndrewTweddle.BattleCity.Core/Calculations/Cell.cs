using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class Cell
    {
        #region Positional properties

        public Point Position { get; set; }
        public int PointIndex { get; set; }
        public bool IsValid { get; set; }
        public BitMatrixIndex BitMatrixIndex { get; set; }
        public Cell[] AdjacentCellsByDirection { get; private set; }

        #endregion

        #region SegmentCalculation properties (for segment calculations centred on this cell)

        public Segment[] SegmentsByAxisOfMovement { get; private set; }

        #endregion

        #region Constructors

        public Cell()
        {
            InitializePositionalProperties();
            InitializeSegmentCalculationProperties();
        }

        #endregion

        #region Positional Methods

        private void InitializePositionalProperties()
        {
            AdjacentCellsByDirection = new Cell[Constants.ALL_DIRECTION_COUNT];
            SetAdjacentCell(Direction.NONE, this);
        }

        public Cell GetAdjacentCell(Direction direction)
        {
            return AdjacentCellsByDirection[(int)direction];
        }

        public void SetAdjacentCell(Direction direction, Cell adjacentCalculation)
        {
            AdjacentCellsByDirection[(int)direction] = adjacentCalculation;
        }

        #endregion

        #region SegmentCalculation methods

        private void InitializeSegmentCalculationProperties()
        {
            SegmentsByAxisOfMovement = new Segment[Constants.AXIS_COUNT];
        }

        public Segment GetSegmentCalculationByDirection(Direction direction)
        {
            if (direction == Direction.NONE)
            {
                throw new ArgumentException(
                    "A segment centred on the cell can't be found since an invalid direction has been provided", "direction");
            }
            return SegmentsByAxisOfMovement[(int) direction.ToAxis()];
        }

        public Segment GetSegmentCalculationByAxis(Axis axis)
        {
            return SegmentsByAxisOfMovement[(int)axis];
        }

        public void SetSegmentByAxis(Axis axis, Segment segment)
        {
            SegmentsByAxisOfMovement[(int)axis] = segment;
        }

        #endregion
    }
}
