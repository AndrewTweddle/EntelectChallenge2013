﻿using System;
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
        private DirectionalMatrix<DistanceCalculation>[] distanceMatricesFromTankByTankIndex;
        private FiringLineMatrix firingLinesForPointsMatrix;
        private FiringLineMatrix firingLinesForTanksMatrix;
        private DirectionalMatrix<DistanceCalculation>[] incomingDistanceMatricesByBase;
        private DirectionalMatrix<DistanceCalculation>[,] incomingDistanceMatricesByBaseAndFinalDirectionOfMovement;
        private DirectionalMatrix<DistanceCalculation>[] incomingAttackMatrixByTankIndex;
        private Matrix<CellState> cellStateMatrix;

        #endregion

        #region Public Properties

        public GameState GameState { get; private set; }

        public TurnCalculationCache TurnCalculationCache
        {
            get
            {
                return Game.Current.Turns[GameState.Tick].CalculationCache;
            }
        }

        public bool IsDistanceMatrixCalculatedForTank(int tankIndex)
        {
            return (distanceMatricesFromTankByTankIndex != null) 
                && (distanceMatricesFromTankByTankIndex[tankIndex] != null);
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

        public Matrix<CellState> CellStateMatrix
        {
            get
            {
                if (cellStateMatrix == null)
                {
                    cellStateMatrix = CellStateCalculator.Calculate(GameState);
                }
                return cellStateMatrix;
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

        public FiringLineMatrix FiringLinesForPointsMatrix
        {
            get
            {
                if (firingLinesForPointsMatrix == null)
                {
                    firingLinesForPointsMatrix = new FiringLineMatrix(
                        GameState.Walls.TopLeft, GameState.Walls.Width, GameState.Walls.Height,
                        ElementExtentType.Point, TurnCalculationCache, gameStateCalculationCache: this);
                }
                return firingLinesForPointsMatrix;
            }
        }

        public FiringLineMatrix FiringLinesForTanksMatrix
        {
            get
            {
                if (firingLinesForTanksMatrix == null)
                {
                    firingLinesForTanksMatrix = new FiringLineMatrix(
                        GameState.Walls.TopLeft, GameState.Walls.Width, GameState.Walls.Height,
                        ElementExtentType.TankBody, TurnCalculationCache, gameStateCalculationCache: this, 
                        lastSupportedEdgeOffsetType: EdgeOffsetType.Corner);
                }
                return firingLinesForTanksMatrix;
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

        public DirectionalMatrix<DistanceCalculation> GetDistanceMatrixFromTankByTankIndex(int tankIndex)
        {
            if (distanceMatricesFromTankByTankIndex == null)
            {
                distanceMatricesFromTankByTankIndex = new DirectionalMatrix<DistanceCalculation>[Constants.TANK_COUNT];
            }
            if (distanceMatricesFromTankByTankIndex[tankIndex] == null)
            {
                TurnCalculationCache turnCalcCache = TurnCalculationCache;

                // Don't ride over your own base!
                Tank tank = Game.Current.Elements[tankIndex] as Tank;
                Base @base = tank.Player.Base;
                TankLocation tankLoc = turnCalcCache.TankLocationMatrix[@base.Pos];
                Rectangle[] tabooAreas = new Rectangle[] { tankLoc.TankBody };

                DistanceCalculator distanceCalculator = new DistanceCalculator();
                distanceCalculator.Walls = GameState.Walls;
                distanceCalculator.TankOuterEdgeMatrix = TankOuterEdgeMatrix;
                distanceCalculator.CellMatrix = turnCalcCache.CellMatrix;
                distanceCalculator.TabooAreas = tabooAreas;

                MobileState tankState = GameState.GetMobileState(tankIndex);

                distanceMatricesFromTankByTankIndex[tankIndex]
                    = distanceCalculator.CalculateShortestDistancesFromTank(ref tankState);
            }
            return distanceMatricesFromTankByTankIndex[tankIndex];
        }

        public DirectionalMatrix<DistanceCalculation> GetIncomingAttackMatrixForTankByTankIndex(int tankIndex)
        {
            if (incomingAttackMatrixByTankIndex == null)
            {
                incomingAttackMatrixByTankIndex = new DirectionalMatrix<DistanceCalculation>[Constants.TANK_COUNT];
            }
            DirectionalMatrix<DistanceCalculation> incomingAttackMatrix = incomingAttackMatrixByTankIndex[tankIndex];
            if (incomingAttackMatrix == null)
            {
                MobileState tankState = GameState.GetMobileState(tankIndex);
                if (!tankState.IsActive)
                {
                    return null;
                }
                TurnCalculationCache turnCalcCache = TurnCalculationCache;
                Cell tankCell = turnCalcCache.CellMatrix[tankState.Pos];
                AttackTargetDistanceCalculator attackCalculator = new AttackTargetDistanceCalculator(
                    ElementType.TANK, FiringLinesForTanksMatrix, this, turnCalcCache);
                incomingAttackMatrix
                    = attackCalculator.CalculateMatrixOfShortestDistancesToTargetCell(tankCell);
                incomingAttackMatrixByTankIndex[tankIndex] = incomingAttackMatrix;
            }
            return incomingAttackMatrix;
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
                Base @ownBase = Game.Current.Players[1 - playerIndex].Base;
                TurnCalculationCache turnCalcCache = TurnCalculationCache;
                Cell baseCell = turnCalcCache.CellMatrix[@base.Pos];
                AttackTargetDistanceCalculator attackCalculator = new AttackTargetDistanceCalculator(
                    ElementType.BASE, FiringLinesForPointsMatrix, this, turnCalcCache);
                // Don't move over your own base:
                TankLocation tankLoc = turnCalcCache.TankLocationMatrix[@ownBase.Pos];
                attackCalculator.TabooAreas = new Rectangle[] { tankLoc.TankHalo };
                incomingDistanceMatrix
                    = attackCalculator.CalculateMatrixOfShortestDistancesToTargetCell(baseCell);
                incomingDistanceMatricesByBase[playerIndex] = incomingDistanceMatrix;
            }
            return incomingDistanceMatrix;
        }

        public DirectionalMatrix<DistanceCalculation> GetIncomingDistanceMatrixForBaseWithFinalDirectionOfMovement(
            int playerIndex, Direction finalDirectionOfMovement)
        {
            if (incomingDistanceMatricesByBaseAndFinalDirectionOfMovement == null)
            {
                incomingDistanceMatricesByBaseAndFinalDirectionOfMovement 
                    = new DirectionalMatrix<DistanceCalculation>[
                        Constants.PLAYERS_PER_GAME, Constants.RELEVANT_DIRECTION_COUNT];
            }

            DirectionalMatrix<DistanceCalculation> incomingDistanceMatrix
                = incomingDistanceMatricesByBaseAndFinalDirectionOfMovement[playerIndex, (int) finalDirectionOfMovement];
            if (incomingDistanceMatrix == null)
            {
                Base @base = Game.Current.Players[playerIndex].Base;
                TurnCalculationCache turnCalcCache = TurnCalculationCache;
                Cell baseCell = turnCalcCache.CellMatrix[@base.Pos];
                AttackTargetDistanceCalculator attackCalculator = new AttackTargetDistanceCalculator(
                    ElementType.BASE, FiringLinesForPointsMatrix, this, turnCalcCache);
                attackCalculator.MovementDirections = new Direction[] { finalDirectionOfMovement };
                incomingDistanceMatrix
                    = attackCalculator.CalculateMatrixOfShortestDistancesToTargetCell(baseCell);
                incomingDistanceMatricesByBaseAndFinalDirectionOfMovement[playerIndex, (int) finalDirectionOfMovement]
                    = incomingDistanceMatrix;
            }
            return incomingDistanceMatrix;
        }

        public Line<FiringDistance> GetFiringDistancesToPointViaDirectionOfMovement(
            Point targetPoint, Direction directionOfMovement)
        {
            return FiringLinesForPointsMatrix[targetPoint.X, targetPoint.Y, directionOfMovement.GetOpposite()];
        }

        public IEnumerable<CellState> GetAllCellStates()
        {
            return CellStateMatrix.GetAllMatrixElements();
        }

        #endregion
    }
}
