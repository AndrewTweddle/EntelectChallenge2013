using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Calculations;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Experimental.CommandLine
{
    public static class SegmentStateCalculationTester
    {
        public static void ComparePerformanceOfCachedAndOnTheFlySegmentStateCalculators(
                    string logFilePath, int repetitions, BitMatrix board, Matrix<Cell> cellMatrix,
                    Matrix<SegmentState> vertSegStateMatrix, Matrix<SegmentState> horizSegStateMatrix, Matrix<bool> boolMatrix)
        {
            // Cache based segment state calculator:
            CacheBasedSegmentStateCalculator cacheSegStateCalculator = new CacheBasedSegmentStateCalculator(horizSegStateMatrix, vertSegStateMatrix);
            PerformanceTestHelper.TimeActionWithArgument(logFilePath, "Calculate all segments using cache calculator", repetitions,
                Tuple.Create(board.BottomRight, cacheSegStateCalculator),
                CalculateSegmentStatesUsingCache);

            // Calculation based segment state calculator:
            CalculationBasedSegmentStateCalculator calcSegStateCalculator = new CalculationBasedSegmentStateCalculator(board);
            PerformanceTestHelper.TimeActionWithArgument(logFilePath, "Calculate all segments using direct BitMatrix-based calculator", repetitions,
                Tuple.Create(board.BottomRight, calcSegStateCalculator),
                CalculateSegmentStatesUsingCalculationBasedCalculator);

            // Bool Matrix based on the fly segment state calculator:
            OnTheFlyBoolMatrixBasedSegmentStateCalculator boolMatrixBasedSegStateCalculator
                = new OnTheFlyBoolMatrixBasedSegmentStateCalculator(boolMatrix);
            PerformanceTestHelper.TimeActionWithArgument(logFilePath, "Calculate all segments using direct bool matrix based calculator", repetitions,
                Tuple.Create(boolMatrix.BottomRight, boolMatrixBasedSegStateCalculator),
                CalculateSegmentStatesUsingOnTheFlyBoolBoolBasedCalculator);

            CellMatrixBasedSegmentStateCalculator cellCacheSegStateCalculator
                = new CellMatrixBasedSegmentStateCalculator(board, cellMatrix);
            PerformanceTestHelper.TimeActionWithArgument(logFilePath, "Calculate all segments using cell matrix-based cache calculator",
                repetitions, Tuple.Create(board.BottomRight, cellCacheSegStateCalculator),
                CalculateSegmentStatesUsingCellCacheBasedCalculator);

            /* Compare segment state calculation times over random points for cached and calculated segment state calculators, 
             * to make sure the cache calculator is not just benefiting from memory cache effects due to the sequence of points: 
             */
            Core.Point[] randomPoints = DataGenerator.GenerateRandomPoints(board.TopLeft, board.BottomRight, 10000);

            // Cache based segment state calculator:
            PerformanceTestHelper.TimeActionWithArgument(logFilePath, "Calculate segments on random points using cache calculator", repetitions,
                Tuple.Create(randomPoints, cacheSegStateCalculator),
                CalculateSegmentStatesOnRandomPointsUsingCache);

            PerformanceTestHelper.TimeActionWithArgument(logFilePath, "Calculate segments on random points using cell matrix-based cache calculator", repetitions,
                Tuple.Create(randomPoints, cellCacheSegStateCalculator),
                CalculateSegmentStatesOnRandomPointsUsingCache);

            // Calculation based segment state calculator:
            PerformanceTestHelper.TimeActionWithArgument(logFilePath, 
                "Calculate segments on random points using on the fly calculator", repetitions,
                Tuple.Create(randomPoints, calcSegStateCalculator),
                CalculateSegmentStatesOnRandomPointsUsingCalculationBasedCalculator);

            // Bool Matrix based on the fly segment state calculator:
            PerformanceTestHelper.TimeActionWithArgument(logFilePath, 
                "Calculate segments on random points using on the fly bool matrix based calculator",
                repetitions, Tuple.Create(randomPoints, boolMatrixBasedSegStateCalculator),
                CalculateSegmentStatesOnRandomPointsUsingOnTheFlyBoolBoolBasedCalculator);
        }

        private static void CalculateSegmentStatesUsingCache(Tuple<Core.Point, CacheBasedSegmentStateCalculator> tuple)
        {
            AndrewTweddle.BattleCity.Core.Point bottomRight = tuple.Item1;
            CacheBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                for (int x = 0; x < bottomRight.X; x++)
                {
                    for (int y = 0; y < bottomRight.Y; y++)
                    {
                        SegmentState segState = calculator.GetSegmentState(axis, x, y);
                    }
                }
            }
        }

        private static void CalculateSegmentStatesUsingCellCacheBasedCalculator(Tuple<Core.Point, CellMatrixBasedSegmentStateCalculator> tuple)
        {
            AndrewTweddle.BattleCity.Core.Point bottomRight = tuple.Item1;
            CellMatrixBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                for (int x = 0; x < bottomRight.X; x++)
                {
                    for (int y = 0; y < bottomRight.Y; y++)
                    {
                        SegmentState segState = calculator.GetSegmentState(axis, x, y);
                    }
                }
            }
        }

        public static void CalculateSegmentStatesUsingCalculationBasedCalculator(
            Tuple<Core.Point, CalculationBasedSegmentStateCalculator> tuple)
        {
            Core.Point bottomRight = tuple.Item1;
            CalculationBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                for (int x = 0; x < bottomRight.X; x++)
                {
                    for (int y = 0; y < bottomRight.Y; y++)
                    {
                        SegmentState segState = calculator.GetSegmentState(axis, x, y);
                    }
                }
            }
        }

        public static void CalculateSegmentStatesUsingOnTheFlyBoolBoolBasedCalculator(
            Tuple<Core.Point, OnTheFlyBoolMatrixBasedSegmentStateCalculator> tuple)
        {
            Core.Point bottomRight = tuple.Item1;
            OnTheFlyBoolMatrixBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                for (int x = 0; x < bottomRight.X; x++)
                {
                    for (int y = 0; y < bottomRight.Y; y++)
                    {
                        SegmentState segState = calculator.GetSegmentState(axis, x, y);
                    }
                }
            }
        }

        public static void CalculateSegmentStatesOnRandomPointsUsingCache(Tuple<Core.Point[], CacheBasedSegmentStateCalculator> tuple)
        {
            Core.Point[] randomPoints = tuple.Item1;
            CacheBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                foreach (Core.Point randomPoint in randomPoints)
                {
                    SegmentState segState = calculator.GetSegmentState(axis, randomPoint);
                }
            }
        }

        public static void CalculateSegmentStatesOnRandomPointsUsingCache(Tuple<Core.Point[], CellMatrixBasedSegmentStateCalculator> tuple)
        {
            Core.Point[] randomPoints = tuple.Item1;
            CellMatrixBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                foreach (Core.Point randomPoint in randomPoints)
                {
                    SegmentState segState = calculator.GetSegmentState(axis, randomPoint);
                }
            }
        }

        public static void CalculateSegmentStatesOnRandomPointsUsingCalculationBasedCalculator(
            Tuple<Core.Point[], CalculationBasedSegmentStateCalculator> tuple)
        {
            Core.Point[] randomPoints = tuple.Item1;
            CalculationBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                foreach (Core.Point randomPoint in randomPoints)
                {
                    SegmentState segState = calculator.GetSegmentState(axis, randomPoint);
                }
            }
        }

        public static void CalculateSegmentStatesOnRandomPointsUsingOnTheFlyBoolBoolBasedCalculator(
            Tuple<Core.Point[], OnTheFlyBoolMatrixBasedSegmentStateCalculator> tuple)
        {
            Core.Point[] randomPoints = tuple.Item1;
            OnTheFlyBoolMatrixBasedSegmentStateCalculator calculator = tuple.Item2;
            foreach (Axis axis in BoardHelper.AllRealAxes)
            {
                foreach (Core.Point randomPoint in randomPoints)
                {
                    SegmentState segState = calculator.GetSegmentState(axis, randomPoint);
                }
            }
        }
    }
}
