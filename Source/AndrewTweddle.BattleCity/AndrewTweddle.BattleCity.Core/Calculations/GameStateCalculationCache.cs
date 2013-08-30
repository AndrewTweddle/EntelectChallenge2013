using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations.SegmentStates;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class GameStateCalculationCache
    {
        #region Private Member Variables

        private Matrix<SegmentState> horizontalSegmentStateMatrix;
        private Matrix<SegmentState> verticalSegmentStateMatrix;
        private Matrix<SegmentState[]> tankOuterEdgeMatrix;
        private Matrix<SegmentState[]> tankInnerEdgeMatrix;
        private DirectionalMatrix<DistanceCalculation>[] distanceMatricesByTank;
        private FiringLineMatrix firingLinesForPointsMatrix;
        private DirectionalMatrix<DistanceCalculation>[] incomingDistanceMatricesByBase;

        #endregion

        #region Private Properties

        private FiringLineMatrix FiringLinesForPointsMatrix
        {
            get
            {
                if (firingLinesForPointsMatrix == null)
                {
                    TurnCalculationCache turnCalcCache = Game.Current.Turns[GameState.Tick].CalculationCache;
                    firingLinesForPointsMatrix = new FiringLineMatrix(
                        GameState.Walls.TopLeft, GameState.Walls.Width, GameState.Walls.Height,
                        ElementExtentType.Point, turnCalcCache, gameStateCalculationCache: this);
                }
                return firingLinesForPointsMatrix;
            }
        }

        #endregion

        #region Public Properties

        public GameState GameState { get; private set; }

        public bool IsDistanceMatrixCalculatedForTank(int tankIndex)
        {
            return (distanceMatricesByTank != null) && (distanceMatricesByTank[tankIndex] != null);
        }

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

        public Matrix<SegmentState[]> TankOuterEdgeMatrix
        {
            get
            {
                if (tankOuterEdgeMatrix == null)
                {
                    CacheBasedSegmentStateCalculator segStateCalculator 
                        = new CacheBasedSegmentStateCalculator(HorizontalSegmentStateMatrix, VerticalSegmentStateMatrix);
                    tankOuterEdgeMatrix = TankEdgeCalculator.CalculateTankOuterEdges(segStateCalculator, GameState.Walls);
                }
                return tankOuterEdgeMatrix;
            }
        }

        public Matrix<SegmentState[]> TankInnerEdgeMatrix
        {
            get
            {
                if (tankInnerEdgeMatrix == null)
                {
                    CacheBasedSegmentStateCalculator segStateCalculator
                        = new CacheBasedSegmentStateCalculator(HorizontalSegmentStateMatrix, VerticalSegmentStateMatrix);
                    tankInnerEdgeMatrix = TankEdgeCalculator.CalculateTankInnerEdges(segStateCalculator, GameState.Walls);
                }
                return tankInnerEdgeMatrix;
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
                    = DistanceCalculator.CalculateShortestDistancesFromTank(ref tankState, GameState.Walls, TankOuterEdgeMatrix);
            }
            return distanceMatricesByTank[tankIndex];
        }

        public DirectionalMatrix<DistanceCalculation> GetIncomingDistanceMatrixForBase(int playerIndex)
        {
            if (incomingDistanceMatricesByBase == null)
            {
                incomingDistanceMatricesByBase = new DirectionalMatrix<DistanceCalculation>[Constants.PLAYERS_PER_GAME];
            }
            DirectionalMatrix<DistanceCalculation> incomingDistanceMatrix = incomingDistanceMatricesByBase[playerIndex];
            if (incomingDistanceMatrix == null)
            {
                Base @base = Game.Current.Players[playerIndex].Base;
                TurnCalculationCache turnCalcCache = Game.Current.Turns[GameState.Tick].CalculationCache;
                Cell baseCell = turnCalcCache.CellMatrix[@base.Pos];
                incomingDistanceMatrix 
                    = AttackTargetDistanceCalculator.CalculateShortestDistancesToTargetPoint(
                        baseCell, turnCalcCache, this, FiringLinesForPointsMatrix, ElementType.BASE);
                incomingDistanceMatricesByBase[playerIndex] = incomingDistanceMatrix;
            }
            return incomingDistanceMatrix;
        }

        public Line<FiringDistance> GetFiringDistancesToPointViaDirectionOfMovement(
            Point targetPoint, Direction directionOfMovement)
        {
            return FiringLinesForPointsMatrix[targetPoint.X, targetPoint.Y, directionOfMovement.GetOpposite()];
        }

        #endregion
    }
}
