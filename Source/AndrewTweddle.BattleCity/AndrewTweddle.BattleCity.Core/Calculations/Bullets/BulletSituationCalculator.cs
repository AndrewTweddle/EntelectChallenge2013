using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Calculations.Bullets
{
    public class BulletSituationCalculator
    {
        public BulletSituation CreateBulletSituation(GameState gameState, int bulletIndex, int bulletId)
        {
            MobileState bulletState = gameState.GetMobileState(bulletIndex);
            MobileState tankState = gameState.GetMobileState(bulletIndex - Constants.MIN_BULLET_INDEX);
            BulletSituation bulletSituation;

            if (!bulletState.IsActive)
            {
                bulletSituation = new BulletSituation(bulletIndex, bulletId)
                {
                    IsActive = false,
                    TickOffsetWhenTankCanFireAgain = 0,
                    TankStateAtTimeOfFiring = tankState,
                    BulletStateAtTimeOfFiring = bulletState,
                    BulletCalculationsByTick = new BulletCalculationByTick[0]
                };
                return bulletSituation;
            }

            // Assume the bullet has just been fired (not correct, but it shouldn't matter):
            bulletSituation = new BulletSituation(bulletIndex, bulletId)
            {
                TickFired = gameState.Tick,
                TankStateAtTimeOfFiring = tankState,
                BulletStateAtTimeOfFiring = bulletState,
                IsActive = true
            };
            GenerateBulletTimeline(gameState, bulletSituation);
            return bulletSituation;
        }

        public BulletSituation CreateHypotheticalBulletSituationForNewlyFiredBullet(GameState gameState,
            int tickFired, int bulletIndex, int bulletId)
        {
            MobileState bulletState = gameState.GetMobileState(bulletIndex);
            MobileState tankState = gameState.GetMobileState(bulletIndex - Constants.MIN_BULLET_INDEX);
            BulletSituation bulletSituation = new BulletSituation(bulletIndex, bulletId)
            {
                TickFired = tickFired,
                TankStateAtTimeOfFiring = tankState,
                BulletStateAtTimeOfFiring = bulletState,
                IsActive = true
            };

            GenerateBulletTimeline(gameState, bulletSituation);

            return bulletSituation;
        }

        public void UpdateBulletSituation(GameState gameState, BulletSituation bulletSituation)
        {
            MobileState bulletState = gameState.GetMobileState(bulletSituation.BulletIndex);
            if (!bulletState.IsActive)
            {
                bulletSituation.IsActive = false;
                bulletSituation.BulletCalculationsByTick = new BulletCalculationByTick[0];
            }
            else
            {
                // Re-generate the bullet timeline every turn, since the board may have changed:
                GenerateBulletTimeline(gameState, bulletSituation);
            }
        }

        private void GenerateBulletTimeline(GameState gameState, BulletSituation bulletSituation)
        {
            MobileState bulletState = bulletSituation.BulletStateAtTimeOfFiring;
            TurnCalculationCache turnCalcCache = Game.Current.Turns[bulletSituation.TickFired].CalculationCache;
            Cell cell = turnCalcCache.CellMatrix[bulletState.Pos];
            Line<Point> pointsInBulletPath = cell.LineFromCellToEdgeOfBoardByDirection[(int)bulletState.Dir];

            int maxTickCount = (pointsInBulletPath.Length + 1) / 2;
            BulletCalculationByTick[] bulletCalcsByTick = new BulletCalculationByTick[maxTickCount];
            int tickOffset;

            for (tickOffset = 0; tickOffset < maxTickCount; tickOffset++)
            {
                BulletCalculationByTick bulletCalc = new BulletCalculationByTick
                {
                    Tick = bulletSituation.TickFired + tickOffset,
                    TickOffset = tickOffset
                };
                bool areTanksAtRisk = false;
                Rectangle dangerRect = new Rectangle();
                Point[] bulletPoints = new Point[2];
                Point[,] adjacentTankPointsByRotationTypeAndPhase = new Point[Constants.ROTATION_TYPE_COUNT, 2];
                int bulletPhase;
                int startPhase = (tickOffset == 0) ? 1 : 0;
                for (bulletPhase = startPhase; bulletPhase < 2; bulletPhase++)
                {
                    int pointIndex = 2 * tickOffset - 1 + bulletPhase;
                    Point bulletPoint = pointsInBulletPath[pointIndex];
                    bulletPoints[bulletPhase] = bulletPoint;

                    Cell bulletCell = turnCalcCache.CellMatrix[bulletPoint];
                    if ((!bulletCell.IsValid) || gameState.Walls[bulletPoint])
                    {
                        bulletCalc.IsDestroyed = true;
                        bulletSituation.TickOffsetWhenTankCanFireAgain = tickOffset;
                        break;
                    }

                    areTanksAtRisk = true;
                    TankLocation tankLoc = turnCalcCache.TankLocationMatrix[bulletPoint];
                    
                    if (bulletPhase == 0 || tickOffset == 0)
                    {
                        dangerRect = tankLoc.TankBody;
                    }
                    else
                    {
                        dangerRect = dangerRect.Merge(tankLoc.TankBody);
                    }

                    foreach (RotationType rotationType in BoardHelper.AllRotationTypes)
                    {
                        Direction rotatedDir = bulletSituation.BulletMovementDir.GetRotatedDirection(rotationType);
                        adjacentTankPointsByRotationTypeAndPhase[(int) rotationType, bulletPhase]
                            = bulletPoint + rotatedDir.GetOffset(Constants.TANK_OUTER_EDGE_OFFSET);
                    }
                }
                bulletCalc.AreTanksAtRisk =  areTanksAtRisk;
                if (areTanksAtRisk)
                {
                    bulletCalc.TankCentrePointsThatDie = dangerRect;
                    Direction oppositeDir = bulletSituation.BulletMovementDir.GetOpposite();
                    Point pointMovingInBehindBulletFacingFiringTank
                        = adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.OneEightyDegrees, 0]
                        + oppositeDir.GetOffset();  // Add the offset, so the tank can move in and turn to face the firing tank
                    bulletCalc.ClosestTankStateMovingInBehindBulletFacingFiringTank
                        = new MobileState(pointMovingInBehindBulletFacingFiringTank, oppositeDir, isActive: true);

                    if (bulletPhase == 2)
                    {
                        if (tickOffset == 0)
                        {
                            // Only include calculations relative to the second bullet point:
                            bulletCalc.BulletPoints = new Point[]
                            {
                                bulletPoints[1]
                            };
                            bulletCalc.ClosestTankCentrePointsThatSurviveAntiClockwise = new Point[]
                            {
                                adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.AntiClockwise, 1]
                            };
                            bulletCalc.ClosestTankCentrePointsThatSurviveClockwise = new Point[]
                            {
                                adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.Clockwise, 1]
                            };
                        }
                        else
                        {
                            // Include calculations relative to the first and second bullet point:
                            bulletCalc.BulletPoints = new Point[]
                            {
                                bulletPoints[0],
                                bulletPoints[1]
                            };
                            bulletCalc.ClosestTankCentrePointsThatSurviveAntiClockwise = new Point[]
                            {
                                adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.AntiClockwise, 0],
                                adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.AntiClockwise, 1]
                            };
                            bulletCalc.ClosestTankCentrePointsThatSurviveClockwise = new Point[]
                            {
                                adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.Clockwise, 0],
                                adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.Clockwise, 1]
                            };
                        }
                        // Include calculations relative to the second bullet point:
                        MobileState headOnConfrontationState = new MobileState(
                            adjacentTankPointsByRotationTypeAndPhase[(int)RotationType.None, 1],
                            oppositeDir,
                            isActive: true);
                        bulletCalc.ClosestTankStateThatCanShootBulletHeadOn = headOnConfrontationState;
                        bulletCalc.ClosestTankStatesThatCanShootBullet = new MobileState[]
                        {
                            new MobileState(
                                adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.AntiClockwise, 1],
                                bulletSituation.BulletMovementDir.GetRotatedDirection(RotationType.Clockwise),
                                isActive: true),
                            new MobileState(
                                adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.Clockwise, 1],
                                bulletSituation.BulletMovementDir.GetRotatedDirection(RotationType.AntiClockwise),
                                isActive: true),
                            headOnConfrontationState
                        };
                    }
                    else
                    {
                        // The second bullet point never occurred as the bullet was destroyed on the first one.
                        // So only include points and tank states relative to the first bullet point:
                        bulletCalc.BulletPoints = new Point[]
                        {
                            bulletPoints[0]
                        };
                        bulletCalc.ClosestTankCentrePointsThatSurviveAntiClockwise = new Point[]
                        {
                            adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.AntiClockwise, 0]
                        };
                        bulletCalc.ClosestTankCentrePointsThatSurviveClockwise = new Point[]
                        {
                            adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.Clockwise, 0]
                        };
                        MobileState headOnConfrontationState = new MobileState(
                            adjacentTankPointsByRotationTypeAndPhase[(int)RotationType.None, 0],
                            oppositeDir,
                            isActive: true);
                        bulletCalc.ClosestTankStateThatCanShootBulletHeadOn = headOnConfrontationState;
                        bulletCalc.ClosestTankStatesThatCanShootBullet = new MobileState[]
                        {
                            new MobileState(
                                adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.AntiClockwise, 0],
                                bulletSituation.BulletMovementDir.GetRotatedDirection(RotationType.Clockwise),
                                isActive: true),
                            new MobileState(
                                adjacentTankPointsByRotationTypeAndPhase[(int) RotationType.Clockwise, 0],
                                bulletSituation.BulletMovementDir.GetRotatedDirection(RotationType.Clockwise),
                                isActive: true),
                            headOnConfrontationState
                        };
                    }
                }
                if (bulletCalc.IsDestroyed)
                {
                    break;
                }
            }
            if (tickOffset + 1 < maxTickCount)
            {
                Array.Resize(ref bulletCalcsByTick, tickOffset + 1);
            }
        }

        /*
        public BulletSituation UpdateBulletSituation(
            BulletSituation existingBulletSituation,
            MobileState bulletState,
            int currentTick)
        {

        }
         */
    }
}
