using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using System.Diagnostics;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core.Calculations.Firing
{
    public class FiringLineMatrix
    {
        #region Private Member Variables

        private DirectionalMatrix<Line<FiringDistance>[]> directionalMatrixOfFiringLinesByEdgeOffset;

        #endregion

        #region Public Properties

        public ElementExtentType ExtentType { get; private set; }
        public TurnCalculationCache TurnCalcationCache { get; private set; }
        public GameStateCalculationCache GameStateCalculationCache { get; private set; }

        public EdgeOffsetType LastSupportedEdgeOffsetType { get; private set; }

        public int FiringLineArraySize
        {
            get
            {
                return 1 + 2 * (int)LastSupportedEdgeOffsetType;
            }
        }

        public Line<FiringDistance> this[int x, int y, Direction outwardDirection, EdgeOffset edgeOffset = EdgeOffset.Centre]
        {
            get
            {
                Debug.Assert((int) edgeOffset < FiringLineArraySize, "An unsupported edge offset has been requested of a firing line matrix");
                Line<FiringDistance>[] firingLinesByEdgeOffset
                    = directionalMatrixOfFiringLinesByEdgeOffset[outwardDirection, x, y];
                if (firingLinesByEdgeOffset == null)
                {
                    firingLinesByEdgeOffset = new Line<FiringDistance>[FiringLineArraySize];
                }

                Line<FiringDistance> firingLine = firingLinesByEdgeOffset[(int) edgeOffset];
                if (firingLine == null)
                {
                    Cell targetCell;
                    switch (ExtentType)
                    {
                        case ElementExtentType.TankBody:
                            targetCell
                                = TurnCalcationCache.TankLocationMatrix[x, y]
                                    .CellsOnEdgeByDirectionAndEdgeOffset[(int)outwardDirection, (int)edgeOffset];
                            break;
                        default:
                            // case ElementExtentType.Point:
                            targetCell = TurnCalcationCache.CellMatrix[x, y];
                            break;
                    }
                    firingLine = FiringDistanceCalculator.GetFiringDistancesToPoint(
                        targetCell, outwardDirection, TurnCalcationCache, GameStateCalculationCache);
                }
                return firingLine;
            }
        }

        #endregion

        #region Constructors

        public FiringLineMatrix(Point topLeft, int width, int height, ElementExtentType extentType,
            TurnCalculationCache turnCalcationCache, GameStateCalculationCache gameStateCalculationCache,
            EdgeOffsetType lastSupportedEdgeOffsetType = EdgeOffsetType.Centre)
        {
            directionalMatrixOfFiringLinesByEdgeOffset = new DirectionalMatrix<Line<FiringDistance>[]>(topLeft, width, height);
            LastSupportedEdgeOffsetType = lastSupportedEdgeOffsetType;
            ExtentType = extentType;
            TurnCalcationCache = turnCalcationCache;
            GameStateCalculationCache = gameStateCalculationCache;
        }

        #endregion


    }
}
