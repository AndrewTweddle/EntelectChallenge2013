using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.SchedulingEngine;
using AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.Scripts
{
    public class BulletMovementScript
    {
        /*
        TimeBlock<BulletEvent> GenerateEventsFromGameState(GameState gameState)
        {
            for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
            {
                Bullet bullet = (Bullet) Game.Current.Elements[b];
                MobileState bulletState = gameState.GetMobileState(b);
                int tick = gameState.Tick;
                Phase startPhase = new Phase(tick - 1, PhaseType.FireBullets);
                int maxTicksNeeded = Game.Current.FinalTickInGame - tick + 2;
                Phase endPhase = startPhase.AddTicks(maxTicksNeeded);
                TimeBlock<BulletEvent> bulletEvents = new TimeBlock<BulletEvent>(startPhase, endPhase);

                if (bulletState.IsActive)
                {
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
                            bulletEvents[new Phase(tick, PhaseType.MoveBullets1)] = bulletEvent;

                            // Collisions1:
                            if (gameState.Walls[pos])
                            {

                            }
                    }
                }
            }
        }
         */
    }
}
