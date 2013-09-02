using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Calculations.Bullets
{
    public static class BulletCalculator
    {
        public static BulletCalculation GetBulletCalculation(GameState gameState, Player you)
        {
            TurnCalculationCache turnCalcCache = Game.Current.Turns[gameState.Tick].CalculationCache;
            BulletCalculation bulletCalc = new BulletCalculation();
            List<BulletPathCalculation> bulletPathCalculations = new List<BulletPathCalculation>();

            for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
            {
                MobileState bulletState = gameState.GetMobileState(b);
                if (bulletState.IsActive)
                {
                    BulletPathCalculation bulletPathCalc = new BulletPathCalculation
                    {
                        Bullet = Game.Current.Elements[b] as Bullet,
                        BulletState = bulletState
                    };
                    bulletPathCalculations.Add(bulletPathCalc);

                    BulletPathPoint[] bulletPathPoints = new BulletPathPoint[500];
                    int bulletPathPointCount = 0;

                    Cell cell = turnCalcCache.CellMatrix[bulletState.Pos];
                    Line<Point> pointsInBulletPath = cell.LineFromCellToEdgeOfBoardByDirection[(int) bulletState.Dir];
                    int tickOffset = 0;
                    for (int i = 1; i < pointsInBulletPath.Length; i++)
                    {
                        Point bulletPoint = pointsInBulletPath[i];
                        Cell bulletPointCell = turnCalcCache.CellMatrix[bulletPoint];
                        if (!bulletPointCell.IsValid)
                        {
                            break;
                        }

                        if (gameState.Walls[bulletPoint])
                        {
                            bulletPathCalc.TicksTillBulletDestroyed = tickOffset;
                            break;
                        }
                        TankLocation tankLoc = turnCalcCache.TankLocationMatrix[bulletPoint];
                        Rectangle dangerArea = tankLoc.TankBody;

                        BulletPathPoint bulletPathPoint = new BulletPathPoint
                        {
                            BulletPoint = pointsInBulletPath[i],
                            DangerArea = dangerArea,
                            Tick = gameState.Tick + tickOffset,
                            TicksToEscape = tickOffset,
                            MovementPhase = (i - 1) % 2
                        };
                        bulletPathPoints[bulletPathPointCount] = bulletPathPoint;
                        bulletPathPointCount++;
                        for (int p = 0; p < Constants.PLAYERS_PER_GAME; p++)
                        {
                            Base @base = Game.Current.Players[p].Base;
                            if (bulletPoint == @base.Pos)
                            {
                                bulletPathCalc.BaseThreatened = @base;
                                bulletPathCalc.TicksTillBulletDestroyed = tickOffset;
                            }
                        }
                        if (i % 2 == 0)
                        {
                            tickOffset++;
                        }
                    }

                    Array.Resize(ref bulletPathPoints, bulletPathPointCount);
                    bulletPathCalc.BulletPathPoints = bulletPathPoints;
                }
            }
            bulletCalc.BulletPaths = bulletPathCalculations.ToArray();

            CalculateBulletThreats(bulletCalc, gameState, you);

            return bulletCalc;
        }

        private static void CalculateBulletThreats(BulletCalculation bulletCalc, GameState gameState, Player you)
        {
            TurnCalculationCache turnCalcCache = Game.Current.Turns[gameState.Tick].CalculationCache;
            foreach (Tank tank in you.Tanks)
            {
                MobileState tankState = gameState.GetMobileState(tank.Index);
                foreach (BulletPathCalculation bulletPathCalc in bulletCalc.BulletPaths)
                {
                    List<BulletThreat> bulletThreats = new List<BulletThreat>();
                    foreach (BulletPathPoint bulletPathPoint in bulletPathCalc.BulletPathPoints)
                    {
                        if (bulletPathPoint.DangerArea.ContainsPoint(tankState.Pos))
                        {
                            BulletThreat bulletThreat = new BulletThreat
                            {
                                FiringTank = bulletPathCalc.Bullet.Tank,
                                TankThreatened = tank
                            };
                            bulletThreats.Add(bulletThreat);

                            // Take the bullet on:
                            FiringLineMatrix firingLineMatrix = gameState.CalculationCache.FiringLinesForPointsMatrix;
                            AttackTargetDistanceCalculator attackCalculator = new AttackTargetDistanceCalculator(
                                ElementType.BULLET, firingLineMatrix, gameState.CalculationCache, turnCalcCache);
                            attackCalculator.MovementDirections = new Direction[]
                            {
                                bulletPathCalc.BulletState.Dir.GetOpposite()
                            };
                            Cell bulletCell = turnCalcCache.CellMatrix[bulletPathCalc.BulletState.Pos];
                            DirectionalMatrix<DistanceCalculation> distanceCalcs 
                                = attackCalculator.CalculateMatrixOfShortestDistancesToTargetCell(bulletCell);
                            DistanceCalculation distanceCalc = distanceCalcs[tankState];
                            if (distanceCalc.Distance <= bulletPathPoint.TicksToEscape)
                            {
                                Node[] nodes = PathCalculator.GetIncomingNodesOnShortestPath(
                                    distanceCalcs, tankState.Dir, tankState.Pos.X, tankState.Pos.Y,
                                    bulletPathCalc.BulletState.Pos.X, bulletPathCalc.BulletState.Pos.Y,
                                    firingLineMatrix, keepMovingCloserOnFiringLastBullet:true);
                                bulletThreat.NodePathToTakeOnBullet = nodes;
                                TankAction[] tankActions
                                    = PathCalculator.GetTankActionsOnIncomingShortestPath(distanceCalcs, 
                                        tankState.Dir, tankState.Pos.X, tankState.Pos.Y,
                                        bulletPathCalc.BulletState.Pos.X, bulletPathCalc.BulletState.Pos.Y,
                                        firingLineMatrix, keepMovingCloserOnFiringLastBullet:true);
                                bulletThreat.TankActionsToTakeOnBullet = tankActions;
                            }

                            // Move off his path in one direction:
                            Point tankOffset = tankState.Pos - bulletPathCalc.BulletState.Pos;
                            Point bulletMovementOffset = bulletPathCalc.BulletState.Dir.GetOffset();

                            Point newTankPoint1;
                            Point newTankPoint2;
                            Direction newTankDir1;
                            Direction newTankDir2;

                            if (bulletPathCalc.BulletState.Dir.ToAxis() == Axis.Horizontal)
                            {
                                newTankPoint1 = new Point(
                                    (short)tankState.Pos.X,
                                    (short)(bulletPathCalc.BulletState.Pos.Y - Constants.TANK_OUTER_EDGE_OFFSET));
                                newTankDir1 = Direction.UP;
                                newTankPoint2 = new Point(
                                    (short)tankState.Pos.X,
                                    (short)(bulletPathCalc.BulletState.Pos.Y + Constants.TANK_OUTER_EDGE_OFFSET));
                                newTankDir2 = Direction.DOWN;
                            }
                            else
                            {
                                newTankPoint1 = new Point(
                                    (short)(bulletPathCalc.BulletState.Pos.X - Constants.TANK_OUTER_EDGE_OFFSET),
                                    (short)tankState.Pos.Y);
                                newTankDir1 = Direction.LEFT;
                                newTankPoint2 = new Point(
                                    (short)(bulletPathCalc.BulletState.Pos.X + Constants.TANK_OUTER_EDGE_OFFSET),
                                    (short)tankState.Pos.Y);
                                newTankDir2 = Direction.RIGHT;
                            }

                            DirectionalMatrix<DistanceCalculation> tankDistanceCalcs
                                = gameState.CalculationCache.GetDistanceMatrixFromTankByTankIndex(tank.Index);

                            distanceCalc = tankDistanceCalcs[newTankDir1, newTankPoint1.X, newTankPoint1.Y];
                            if (distanceCalc.Distance <= bulletPathPoint.TicksToEscape)
                            {
                                Node[] shortestPathsIn1Direction 
                                    = PathCalculator.GetOutgoingNodesOnShortestPath(tankDistanceCalcs,
                                        newTankDir1, newTankPoint1.X, newTankPoint1.Y);
                                bulletThreat.LateralMoveInOneDirection = shortestPathsIn1Direction;
                                TankAction[] tankActionsIn1Direction
                                    = PathCalculator.GetTankActionsOnOutgoingShortestPath(tankDistanceCalcs,
                                        newTankDir1, newTankPoint1.X, newTankPoint1.Y);
                                bulletThreat.TankActionsForLateralMoveInOneDirection = tankActionsIn1Direction;
                            }

                            distanceCalc = tankDistanceCalcs[newTankDir2, newTankPoint2.X, newTankPoint2.Y];
                            if (distanceCalc.Distance <= bulletPathPoint.TicksToEscape)
                            {
                                Node[] shortestPathsInOtherDirection = PathCalculator.GetOutgoingNodesOnShortestPath(tankDistanceCalcs,
                                    newTankDir2, newTankPoint2.X, newTankPoint2.Y);
                                bulletThreat.LateralMoveInOtherDirection = shortestPathsInOtherDirection;
                                TankAction[] tankActionsInOtherDirection
                                    = PathCalculator.GetTankActionsOnOutgoingShortestPath(tankDistanceCalcs,
                                        newTankDir2, newTankPoint2.X, newTankPoint2.Y);
                                bulletThreat.TankActionsForLateralMoveInOtherDirection = tankActionsInOtherDirection;
                            }
                        }
                    }
                    bulletPathCalc.BulletThreats = bulletThreats.ToArray();
                }
            }
            
        }
        
    }
}
