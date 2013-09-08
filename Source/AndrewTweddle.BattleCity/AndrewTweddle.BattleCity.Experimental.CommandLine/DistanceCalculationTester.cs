using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Calculations;
using AndrewTweddle.BattleCity.Core.Calculations.SegmentStates;

namespace AndrewTweddle.BattleCity.Experimental.CommandLine
{
    public class DistanceCalculationTester
    {
        #region Input Properties

        public Matrix<SegmentState> HorizSegmentStateMatrix { get; set; }
        public Matrix<SegmentState> VertSegmentStateMatrix { get; set; }
        public BitMatrix Board { get; set; }
        public Matrix<Cell> CellMatrix { get; set; }
        
        #endregion

        #region Output Properties

        public CacheBasedSegmentStateCalculator SegStateCalculator { get; private set; }
        public Matrix<SegmentState[]> TankEdgeMatrix { get; private set; }
        public DirectionalMatrix<DistanceCalculation> DistancesFromTank1 { get; private set; }
        public DirectionalMatrix<DistanceCalculation> DistancesFromTank2 { get; private set; }

        #endregion

        public void TestDistanceCalculator(string logFilePath, int repetitions, int smallRepetitions)
        {
            SegStateCalculator = new CacheBasedSegmentStateCalculator(HorizSegmentStateMatrix, VertSegmentStateMatrix);
            TankEdgeMatrix = PerformanceTestHelper.TimeFunction(
                logFilePath, "Calculate tank edge matrix using TankEdgeCalculator", repetitions,
                CalculateTankOuterEdgeMatrix);

            MobileState tankState1 = new MobileState(new Point(20, 6), Direction.UP, isActive: true);
            DistancesFromTank1 = PerformanceTestHelper.TimeFunctionWithArgument(logFilePath, "Test Distance Calculator for tank 1",
                smallRepetitions, tankState1, CalculateDistancesForTank);

            MobileState tankState2 = new MobileState(new Point(56, 6), Direction.UP, isActive: true);
            DistancesFromTank2 = PerformanceTestHelper.TimeFunctionWithArgument(logFilePath, "Test Distance Calculator for tank 2",
                smallRepetitions, tankState2, CalculateDistancesForTank);
        }

        public Matrix<SegmentState[]> CalculateTankOuterEdgeMatrix()
        {
            return TankEdgeCalculator.CalculateTankOuterEdges(SegStateCalculator, Board);
        }

        public DirectionalMatrix<DistanceCalculation> CalculateDistancesForTank(MobileState tankState)
        {
            DistanceCalculator distanceCalculator = new DistanceCalculator();
            distanceCalculator.Walls = Board;
            distanceCalculator.TankOuterEdgeMatrix = TankEdgeMatrix;
            distanceCalculator.CellMatrix = CellMatrix;
            return distanceCalculator.CalculateShortestDistancesFromTank(ref tankState);
        }
    }
}
