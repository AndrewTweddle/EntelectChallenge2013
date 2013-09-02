using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public static class CellStateCalculator
    {
        public static Matrix<CellState> Calculate(GameState gameState)
        {
            Point topLeft = gameState.Walls.TopLeft;
            Point bottomRight = gameState.Walls.BottomRight;

            Matrix<CellState> cellStateMatrix = new Matrix<CellState>(topLeft, gameState.Walls.Width, gameState.Walls.Height);

            // gameState.CalculationCache.
            DirectionalMatrix<DistanceCalculation> incomingDistanceMatrixForBase0
                = gameState.CalculationCache.GetIncomingDistanceMatrixForBase(0);
            DirectionalMatrix<DistanceCalculation> incomingDistanceMatrixForBase1
                = gameState.CalculationCache.GetIncomingDistanceMatrixForBase(1);

            for (int x = topLeft.X; x <= bottomRight.X; x++)
            {
                for (int y = topLeft.Y; y <= bottomRight.Y; y++)
                {
                    CellState cellState = new CellState(gameState, new Point((short) x, (short) y));
                    cellStateMatrix[x, y] = cellState;

                    /* TODO: Calculate or fetch on the fly?
                    foreach (Direction dir in BoardHelper.AllRealDirections)
                    {
                        for (int p = 0; p < Constants.PLAYERS_PER_GAME; p++)
                        {
                            DirectionalMatrix<DistanceCalculation> incomingDistanceMatrixForBase
                                = gameState.CalculationCache.GetIncomingDistanceMatrixForBase(p);
                            cellState.DirectionalStates[(int) dir].AttackingDistancesToBaseByPlayerId[p] 
                                = incomingDistanceMatrixForBase[dir, x, y];
                        }
                    }
                     */
                }
            }

            return cellStateMatrix;
        }
    }
}
