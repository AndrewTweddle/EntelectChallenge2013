using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.VisualUtils;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Calculations;
using System.Drawing.Imaging;
using System.Drawing;

namespace AndrewTweddle.BattleCity.Experimental.CommandLine
{
    public class SegmentStateMatrixTester
    {
        #region Input properties

        public BitMatrix Board { get; set; }
        public ImageGenerator ImageGenerator { get; set; }

        #endregion

        #region Output Properties

        public Matrix<SegmentState> VerticalSegmentStateMatrix { get; set; }
        public Matrix<SegmentState> HorizontalSegStateMatrix { get; set; }
        public Matrix<Cell> CellMatrix { get; set; }

        #endregion

        public void Test(int repetitions, int smallRepetitions, string logFilePath)
        {
            Board.ReadCount = 0;
            Board.WriteCount = 0;
            VerticalSegmentStateMatrix
                = PerformanceTestHelper.TimeFunction(logFilePath,
                    "Test calculation of vertical segment state matrix directly from the BitMatrix",
                    repetitions, GetVerticalSegmentStateMatrix);
            
            // TODO: Remove or re-enable if checking number of bit matrix reads and writes:
            // PerformanceTester.WriteToLog(logFilePath, string.Format("\r\n{0} reads, {1} writes\r\n", Board.ReadCount, Board.WriteCount));

            // Save image for vertical segment state matrix:
            Bitmap segStateBitmap = ImageGenerator.GenerateBoardImage(Board);
            ImageGenerator.DrawSegmentMatrixOverlay(segStateBitmap, Board, VerticalSegmentStateMatrix, Axis.Vertical);
            string segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\VertSegmentMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            Board.ReadCount = 0;
            Board.WriteCount = 0;
            HorizontalSegStateMatrix
                = PerformanceTestHelper.TimeFunction(logFilePath,
                    "Test calculation of horizontal segment state matrix directly from the BitMatrix",
                    repetitions, GetHorizontalSegmentStateMatrix);
            PerformanceTestHelper.WriteToLog(logFilePath, string.Format("\r\n{0} reads, {1} writes\r\n", Board.ReadCount, Board.WriteCount));

            // Save image for horizontal segment state matrix:
            segStateBitmap = ImageGenerator.GenerateBoardImage(Board);
            ImageGenerator.DrawSegmentMatrixOverlay(segStateBitmap, Board, HorizontalSegStateMatrix, Axis.Horizontal);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\HorizSegmentMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            ImageGenerator.DrawSegmentMatrixOverlay(segStateBitmap, Board, VerticalSegmentStateMatrix, Axis.Vertical);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\BiDiSegmentMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            // Test calculation caches:
            PerformanceTestHelper.TimeAction(logFilePath,
                "Time cell calculator on challenge board 1", smallRepetitions,
                PerformCellCalculation);
            CellMatrix = PerformanceTestHelper.TimeFunction(logFilePath,
                "Time cell and segment calculator on challenge board 1",
                smallRepetitions, PerformCellAndSegmentCalculation);

            // Repeat segment stage calculations using implementation based on pre-calculated cell and segment calculations:
            Board.ReadCount = 0;
            Board.WriteCount = 0;
            VerticalSegmentStateMatrix = PerformanceTestHelper.TimeFunctionWithArgument(logFilePath,
                "Test calculation of vertical segment state matrix using a cell matrix",
                repetitions, Axis.Vertical, GetSegmentStateMatrixUsingCellMatrix);
            PerformanceTestHelper.WriteToLog(logFilePath,
                string.Format("\r\n{0} reads, {1} writes\r\n", Board.ReadCount, Board.WriteCount));

            // Save image for vertical segment state matrix:
            segStateBitmap = ImageGenerator.GenerateBoardImage(Board);
            ImageGenerator.DrawSegmentMatrixOverlay(segStateBitmap, Board, VerticalSegmentStateMatrix, Axis.Vertical);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\VertSegmentMatrixUsingCellMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            Board.ReadCount = 0;
            Board.WriteCount = 0;
            HorizontalSegStateMatrix = PerformanceTestHelper.TimeFunctionWithArgument(logFilePath,
                "Test calculation of horizontal segment state matrix using a cell matrix",
                repetitions, Axis.Horizontal, GetSegmentStateMatrixUsingCellMatrix);
            PerformanceTestHelper.WriteToLog(logFilePath,
                string.Format("\r\n{0} reads, {1} writes\r\n", Board.ReadCount, Board.WriteCount));

            // Repeat segment state calculation using Segment matrix calculated from the Cell matrix:
            Matrix<Segment> vertSegmentMatrix = SegmentCalculator.GetSegmentMatrix(CellMatrix, Board, Axis.Vertical);
            VerticalSegmentStateMatrix = PerformanceTestHelper.TimeFunctionWithArgument(logFilePath,
                "Test calculation of vertical segment state matrix using a segment matrix",
                repetitions, Tuple.Create(vertSegmentMatrix, Board),
                (tuple) => SegmentCalculator.GetBoardSegmentStateMatrixFromSegmentMatrix(tuple.Item1, tuple.Item2));

            Matrix<Segment> horizSegmentMatrix = SegmentCalculator.GetSegmentMatrix(CellMatrix, Board, Axis.Horizontal);
            HorizontalSegStateMatrix = PerformanceTestHelper.TimeFunctionWithArgument(logFilePath,
                "Test calculation of vertical segment state matrix using a segment matrix",
                repetitions, Tuple.Create(horizSegmentMatrix, Board),
                (tuple) => SegmentCalculator.GetBoardSegmentStateMatrixFromSegmentMatrix(tuple.Item1, tuple.Item2));

            // Save image for horizontal segment state matrix:
            segStateBitmap = ImageGenerator.GenerateBoardImage(Board);
            ImageGenerator.DrawSegmentMatrixOverlay(segStateBitmap, Board, HorizontalSegStateMatrix, Axis.Horizontal);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\HorizSegmentMatrixUsingCellMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);

            ImageGenerator.DrawSegmentMatrixOverlay(segStateBitmap, Board, VerticalSegmentStateMatrix, Axis.Vertical);
            segmentMatrixFilePath = @"c:\Competitions\EntelectChallenge2013\temp\BiDiSegmentMatrixUsingCellMatrix.bmp";
            segStateBitmap.Save(segmentMatrixFilePath, ImageFormat.Bmp);
        }

        private Matrix<SegmentState> GetVerticalSegmentStateMatrix()
        {
            Matrix<SegmentState> segStateMatrix = Board.GetBoardSegmentStateMatrixForAxisOfMovement(Axis.Vertical);
            return segStateMatrix;
        }

        private Matrix<SegmentState> GetHorizontalSegmentStateMatrix()
        {
            Matrix<SegmentState> segStateMatrix = Board.GetBoardSegmentStateMatrixForAxisOfMovement(Axis.Horizontal);
            return segStateMatrix;
        }

        private Matrix<SegmentState> GetSegmentStateMatrixUsingCellMatrix(Axis axisOfMovement)
        {
            Matrix<SegmentState> segStateMatrix
                = SegmentCalculator.GetBoardSegmentStateMatrixForAxisOfMovement(
                    CellMatrix, Board, axisOfMovement);
            return segStateMatrix;
        }

        private void PerformCellCalculation()
        {
            Matrix<Cell> matrix = CellCalculator.Calculate(Board, Board.TopLeft.X, Board.BottomRight.X);
        }

        private Matrix<Cell> PerformCellAndSegmentCalculation()
        {
            Matrix<Cell> matrix = CellCalculator.Calculate(Board, Board.TopLeft.X, Board.BottomRight.X);
            SegmentCalculator.Calculate(matrix);
            return matrix;
        }
    }
}
