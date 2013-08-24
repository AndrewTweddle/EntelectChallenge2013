using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.Solvers;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Actions;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Calculations;

namespace AndrewTweddle.BattleCity.Bots
{
    public class ShortestPathBot<TGameState> : BaseSolver<TGameState>
        where TGameState : GameState<TGameState>, new()
    {
        protected override void ChooseMoves()
        {
            GameState currGameState = Game.Current.CurrentTurn.GameState;
            TankActionSet actionSet = new TankActionSet(YourPlayerIndex, currGameState.Tick);

            Base @base = Opponent.Base;
            for (int tankNumber = 0; tankNumber <= Constants.TANKS_PER_PLAYER; tankNumber++)
            {
                Tank tank = You.Tanks[tankNumber];
                MobileState tankState = currGameState.GetMobileState(tank.Index);
                if (!tankState.IsActive)
                {
                    continue;
                }

                DirectionalMatrix<DistanceCalculation> distances = currGameState.CalculationCache.GetDistanceMatrixForTank(tank.Index);

                List<Path> shortestPaths = new List<Path>();
                int shortestDistance = Constants.UNREACHABLE_DISTANCE;

                TankLocation tankLoc = Game.Current.CurrentTurn.CalculationCache.TankLocationMatrix[@base.Pos];

                // Get the distances to move onto the base from various directions:
                foreach (Direction dir in BoardHelper.AllRealDirections)
                {
                    Direction oppositeDir = dir.GetOpposite();

                    // Get the inside edge (segment) of the tank centred at the base in the opposite direction:
                    Segment tankPositionsInDirection = tankLoc.InsideEdgesByDirection[(int)oppositeDir];

                    // For each point on the segment determine the moving distance to that point:
                    foreach (Cell tankCellInDir in tankPositionsInDirection.Cells)
                    {
                        if (tankCellInDir.IsValid)
                        {
                            DistanceCalculation distCalc = distances[dir, tankCellInDir.Position];
                            if (distCalc.CodedDistance != 0)
                            {
                                if (distCalc.Distance <= shortestDistance)
                                {
                                    if (distCalc.Distance != shortestDistance)
                                    {
                                        shortestPaths.Clear();
                                    }
                                    Path path = new Path
                                    {
                                        FinalDirection = dir,
                                        FinalPosition = tankCellInDir.Position,
                                        DistanceCalculation = distCalc
                                    };
                                    shortestPaths.Add(path);
                                }
                            }
                        }
                    }
                }

                // TODO: Get the distances to move into a firing position plus the distance to shoot at the base
                // TODO: Work out how to avoid multiple identical paths 
                // - perhaps only consider paths which don't approach along the axis of firing

                if (shortestPaths.Count > 0)
                {
                    Random rnd = new Random();
                    int shortestPathIndex = rnd.Next(shortestPaths.Count);
                    Path shortestPathChosen = shortestPaths[shortestPathIndex];
                    TankAction[] pointsOnPath = DistanceCalculator.GetTankActionsOnShortestPath(
                        distances, shortestPathChosen.FinalDirection, shortestPathChosen.FinalPosition);
                    if (pointsOnPath.Length > 0)
                    {
                        actionSet.Actions[tankNumber] = pointsOnPath[0];
                    }
                }
            }

            Coordinator.SetBestMoveSoFar(actionSet);
        }
    }
}
