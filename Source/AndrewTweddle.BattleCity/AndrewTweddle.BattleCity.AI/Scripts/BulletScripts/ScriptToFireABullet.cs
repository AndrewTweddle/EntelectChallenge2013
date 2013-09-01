using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.SchedulingEngine;
using AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Calculations;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.AI.Scripts.BulletScripts
{
    public class ScriptToFireABullet
    {
        /*
        TimeBlock<BulletEvent> GenerateScriptFromGameState(GameState gameState, Bullet bullet)
        {
            MobileState bulletState = gameState.GetMobileState(bullet.Index);

            if (!bulletState.IsActive)
            {
                throw new ArgumentException(
                    "The bullet is not active, so a fire bullet script can't be generated for it", "bullet");
            }

            int tick = gameState.Tick;
            Matrix<Cell> cellMatrix = Game.Current.Turns[tick].CalculationCache.CellMatrix;
            Cell currCell = cellMatrix[bulletState.Pos];
            Line<Point> pointsTravelled = currCell.LineFromCellToEdgeOfBoardByDirection[(int) bulletState.Dir];
            
            int maxPhasesNeeded = pointsTravelled.Length * 4;
            Phase startPhase = new Phase(tick - 1, PhaseType.FireBullets);
            Phase endPhase = startPhase.PhaseId + maxPhasesNeeded;

            int maxTicksNeeded = Game.Current.FinalTickInGame - tick + 2;
            
            TimeBlock<BulletEvent> bulletEvents = new TimeBlock<BulletEvent>(startPhase, endPhase);

                BulletEvent bulletEvent = new BulletFiredEvent(bullet, bulletState.Pos, bulletState.Dir);
                bulletEvents[new Phase(tick - 1, PhaseType.FireBullets)] = bulletEvent;
                bulletEvents[new Phase(tick - 1, PhaseType.ResolveFiredBulletCollisions)]
                    = new BulletNoCollisionEvent();  // Correct?
                Phase collisionPhase = endPhase;  // Just to stop compiler complaining
                Point pos = bulletState.Pos;
                Point offset = bulletState.Dir.GetOffset();
                Direction dir = bulletState.Dir;
                while (tick <= Game.Current.FinalTickInGame)
                {
                    if ((Game.Current.Turns[tick].LeftBoundary > bulletState.Pos.X)
                        || (Game.Current.Turns[tick].RightBoundary < bulletState.Pos.X))
                    {
                        collisionPhase = new Phase(tick, PhaseType.Curtains);
                        bulletEvents[collisionPhase] = new BulletCollisionWithCurtains();
                        break;
                    }

                    PhaseType[] moveBulletPhases 
                        = new PhaseType[] { PhaseType.MoveBullets1, PhaseType.MoveBullets2 };
                    PhaseType[] resolveBulletCollisionPhases 
                        = new PhaseType[] { PhaseType.ResolveBulletCollisions1, PhaseType.ResolveBulletCollisions2 };
                    for (int bmi = 0; bmi < 2; bmi++)
                    {
                        pos = pos + offset;
                        bulletEvent = new BulletMovementEvent(bullet, pos, dir);
                        bulletEvents[new Phase(tick, moveBulletPhases[bmi])] = bulletEvent;

                        // Out-of-bounds collisions:
                        Cell cell = Game.Current.Turns[tick].CalculationCache.CellMatrix[pos];
                        if (!cell.IsValid)
                        {
                            cell.
                        }

                        // Wall collisions:
                        if (gameState.Walls[pos])
                        {

                        }
                    }
                }
            }
            return bulletEvents;
        }
         */
    }
}
