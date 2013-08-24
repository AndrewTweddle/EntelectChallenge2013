using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations.SegmentStates;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class GameStateCalculationCache
    {
        #region Private Member Variables

        private Matrix<SegmentState> horizontalSegmentStateMatrix;
        private Matrix<SegmentState> verticalSegmentStateMatrix;
        private Matrix<SegmentState[]> tankEdgeMatrix;
        private DirectionalMatrix<DistanceCalculation>[] distanceMatricesByTank;

        #endregion

        #region Public Properties

        public GameState GameState { get; private set; }

        public Matrix<SegmentState> HorizontalSegmentStateMatrix 
        { 
            get
            {
                if (horizontalSegmentStateMatrix == null)
                {
                    horizontalSegmentStateMatrix = GameState.Walls.GetBoardSegmentStateMatrixForAxisOfMovement(Axis.Horizontal);
                }
                return horizontalSegmentStateMatrix;
            }
        }

        public Matrix<SegmentState> VerticalSegmentStateMatrix
        { 
            get
            {
                if (verticalSegmentStateMatrix == null)
                {
                    verticalSegmentStateMatrix = GameState.Walls.GetBoardSegmentStateMatrixForAxisOfMovement(Axis.Vertical);
                }
                return verticalSegmentStateMatrix;
            }
        }

        public Matrix<SegmentState[]> TankEdgeMatrix
        {
            get
            {
                if (tankEdgeMatrix == null)
                {
                    CacheBasedSegmentStateCalculator segStateCalculator 
                        = new CacheBasedSegmentStateCalculator(HorizontalSegmentStateMatrix, VerticalSegmentStateMatrix);
                    tankEdgeMatrix = TankEdgeCalculator.CalculateTankEdges(segStateCalculator, GameState.Walls);
                }
                return tankEdgeMatrix;
            }
        }

        #endregion

        #region Constructors

        private GameStateCalculationCache()
        {

        }

        public GameStateCalculationCache(GameState gameState)
        {
            GameState = gameState;
        }

        #endregion

        #region Public Methods

        public DirectionalMatrix<DistanceCalculation> GetDistanceMatrixForTank(int tankIndex)
        {
            if (distanceMatricesByTank == null)
            {
                distanceMatricesByTank = new DirectionalMatrix<DistanceCalculation>[Constants.TANK_COUNT];
            }
            if (distanceMatricesByTank[tankIndex] == null)
            {
                MobileState tankState = GameState.GetMobileState(tankIndex);
                distanceMatricesByTank[tankIndex]
                    = DistanceCalculator.CalculateShortestDistancesFromTank(ref tankState, GameState.Walls, TankEdgeMatrix);
            }
            return distanceMatricesByTank[tankIndex];
        }

        #endregion
    }
}
