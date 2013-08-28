using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class TurnCalculationCache
    {
        #region Private Member Variables

        private Matrix<Cell> cellMatrix;
        private Matrix<Segment> horizontalMovementSegmentMatrix;
        private Matrix<Segment> verticalMovementSegmentMatrix;
        private Matrix<TankLocation> tankLocationMatrix;
        private DirectionalMatrix<TankState> tankStateMatrix;

        #endregion


        #region Public Properties

        /// <summary>
        /// Many turns may share a TurnCalculationCache. 
        /// This is the first turn which created this cache object.
        /// </summary>
        public Turn FirstTurn { get; private set; }

        public Matrix<Cell> CellMatrix
        {
            get
            {
                if (cellMatrix == null)
                {
                    /* Get a bitMatrix for the board. 
                     * Note: Only the dimensions and masks are used, not the status of any of the walls, 
                     *       so it doesn't matter which turn it is from, as long as the GameState is initialized.
                     */
                    BitMatrix bitMatrix = Game.Current.VeryFirstTurn.GameState.Walls;
                    cellMatrix = CellCalculator.Calculate(bitMatrix, FirstTurn.LeftBoundary, FirstTurn.RightBoundary);
                    SegmentCalculator.Calculate(cellMatrix);
                }
                return cellMatrix;
            }
        }

        public Matrix<Segment> HorizontalMovementSegmentMatrix
        {
            get
            {
                if (horizontalMovementSegmentMatrix == null)
                {
                    horizontalMovementSegmentMatrix 
                        = SegmentCalculator.GetSegmentMatrix(CellMatrix, FirstTurn.GameState.Walls, Axis.Horizontal);
                }
                return horizontalMovementSegmentMatrix;
            }
        }

        public Matrix<Segment> VerticalMovementSegmentMatrix
        {
            get
            {
                if (verticalMovementSegmentMatrix == null)
                {
                    verticalMovementSegmentMatrix
                        = SegmentCalculator.GetSegmentMatrix(CellMatrix, FirstTurn.GameState.Walls, Axis.Vertical);
                }
                return verticalMovementSegmentMatrix;
            }
        }

        public Matrix<TankLocation> TankLocationMatrix
        {
            get
            {
                if (tankLocationMatrix == null)
                {
                    tankLocationMatrix = TankLocationAndStateCalculator.Calculate(FirstTurn, 
                        Game.Current.VeryFirstTurn.GameState.Walls, CellMatrix, 
                        VerticalMovementSegmentMatrix, HorizontalMovementSegmentMatrix);
                }
                return tankLocationMatrix;
            }
        }

        public DirectionalMatrix<TankState> TankStateMatrix
        {
            get
            {
                if (tankStateMatrix == null)
                {
                    tankStateMatrix = TankLocationAndStateCalculator.CalculateTankStateMatrix(TankLocationMatrix);
                }
                return tankStateMatrix;
            }
        }

        #endregion


        #region Constructors

        protected TurnCalculationCache()
        {
        }

        public TurnCalculationCache(Turn firstTurn): this()
        {
            FirstTurn = firstTurn;
        }

        #endregion


        #region Public Methods

        public Matrix<Segment> GetSegmentMatrix(Axis axisOfMovement)
        {
            switch (axisOfMovement)
            {
                case Axis.Vertical:
                    return VerticalMovementSegmentMatrix;
                case Axis.Horizontal:
                    return HorizontalMovementSegmentMatrix;
                default:
                    return null;
            }
        }

        #endregion
    }
}
