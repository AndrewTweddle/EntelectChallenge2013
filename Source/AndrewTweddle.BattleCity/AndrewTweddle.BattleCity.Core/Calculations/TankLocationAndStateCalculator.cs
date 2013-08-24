using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public static class TankLocationAndStateCalculator
    {
        public static Matrix<TankLocation> Calculate(BitMatrix board, Matrix<Cell> cellMatrix, 
            Matrix<Segment> verticalMovementSegmentMatrix, Matrix<Segment> horizontalMovementSegmentMatrix)
        {
            Matrix<TankLocation> tankLocationMatrix = new Matrix<TankLocation>(board.Width, board.Height);

            for (int x = board.TopLeft.X; x <= board.BottomRight.X; x++)
            {
                for (int y = board.TopLeft.Y; y <= board.BottomRight.Y; y++)
                {
                    TankLocation tankLoc = new TankLocation();
                    tankLocationMatrix[x, y] = tankLoc;

                    tankLoc.CentreCell = cellMatrix[x, y];

                    int leftInnerEdgeX   = x - Constants.TANK_EXTENT_OFFSET;
                    int rightInnerEdgeX  = x + Constants.TANK_EXTENT_OFFSET;
                    int topInnerEdgeY    = y - Constants.TANK_EXTENT_OFFSET;
                    int bottomInnerEdgeY = y + Constants.TANK_EXTENT_OFFSET;

                    tankLoc.TankBody = new Rectangle(
                        (short) leftInnerEdgeX, 
                        (short) topInnerEdgeY,
                        (short) rightInnerEdgeX, 
                        (short) bottomInnerEdgeY);

                    // Calculate inside edges:
                    tankLoc.InsideEdgesByDirection[(int)Direction.UP]    = verticalMovementSegmentMatrix[x, topInnerEdgeY];
                    tankLoc.InsideEdgesByDirection[(int)Direction.DOWN]  = verticalMovementSegmentMatrix[x, bottomInnerEdgeY];
                    tankLoc.InsideEdgesByDirection[(int)Direction.LEFT]  = verticalMovementSegmentMatrix[leftInnerEdgeX, y];
                    tankLoc.InsideEdgesByDirection[(int)Direction.RIGHT] = verticalMovementSegmentMatrix[rightInnerEdgeX, y];

                    // Calculate outside edges:
                    int leftOuterEdgeX   = leftInnerEdgeX   - 1;
                    int rightOuterEdgeX  = rightInnerEdgeX  + 1;
                    int topOuterEdgeY    = topInnerEdgeY    - 1;
                    int bottomOuterEdgeY = bottomInnerEdgeY + 1;

                    tankLoc.OutsideEdgesByDirection[(int)Direction.UP] = verticalMovementSegmentMatrix[x, topOuterEdgeY];
                    tankLoc.OutsideEdgesByDirection[(int)Direction.DOWN] = verticalMovementSegmentMatrix[x, bottomOuterEdgeY];
                    tankLoc.OutsideEdgesByDirection[(int)Direction.LEFT] = verticalMovementSegmentMatrix[leftOuterEdgeX, y];
                    tankLoc.OutsideEdgesByDirection[(int)Direction.RIGHT] = verticalMovementSegmentMatrix[rightOuterEdgeX, y];

                    // TODO: Calculate TankStates in each direction:
                    foreach (Direction dir in BoardHelper.AllRealDirections)
                    {
                        TankState tankState = new TankState
                        {
                            Dir = dir,
                            Location = tankLoc
                        };
                        tankState.OutsideLeadingEdge = tankLoc.OutsideEdgesByDirection[(int)dir];
                        tankState.InsideTrailingEdge = tankLoc.InsideEdgesByDirection[(int)dir.GetOpposite()];

                        tankLoc.TankStatesByDirection[(int)dir] = tankState;
                    }
                }
            }

            return tankLocationMatrix;
        }

        public static DirectionalMatrix<TankState> CalculateTankStateMatrix(Matrix<TankLocation> locationMatrix)
        {
            DirectionalMatrix<TankState> tankStateMatrix 
                = new DirectionalMatrix<TankState>(
                    locationMatrix.TopLeft, locationMatrix.Width, locationMatrix.Height);

            for (int x = locationMatrix.TopLeft.X; x <= locationMatrix.BottomRight.X; x++)
            {
                for (int y = locationMatrix.TopLeft.Y; y <= locationMatrix.BottomRight.Y; y++)
                {
                    TankLocation tankLoc = locationMatrix[x, y];
                    foreach (Direction dir in BoardHelper.AllRealDirections)
                    {
                        tankStateMatrix[dir, x, y] = tankLoc.TankStatesByDirection[(int)dir];
                    }
                }
            }

            return tankStateMatrix;
        }
    }
}
