using System;
using System.Linq;
using System.Collections.Generic;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Calculations;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Engines
{
    public static class MutableGameStateEngine
    {
        const byte MAX_WALLS_SHOT = 20;

        public static void ApplyAllActions(MutableGameState gameState, TankAction[] tankActions, IList<Point> wallsRemoved = null)
        {
            bool[] bulletsThatMovedThisTurn = new bool[Constants.TANK_COUNT];
            for (int b = 0; b < Constants.TANK_COUNT; b++)
            {
                bulletsThatMovedThisTurn[b] = gameState.MobileStates[Constants.MIN_BULLET_INDEX + b].IsActive;
            }
            DestroyOutOfBoundsBulletsAndTanks(gameState);
            MoveBulletsTwice(gameState, wallsRemoved);
            ApplyTankActions(gameState, tankActions, bulletsThatMovedThisTurn, wallsRemoved);
            CheckForNoMoreTanksAndBullets(gameState);

            if (wallsRemoved != null)
            {
                gameState.WallsRemovedAfterPreviousTick = wallsRemoved.ToArray();
            }
        }

        private static void DestroyOutOfBoundsBulletsAndTanks(MutableGameState gameState)
        {
            Turn turn = Game.Current.Turns[gameState.Tick];
            int leftBoundary = turn.LeftBoundary;
            if (leftBoundary == 0)
            {
                return;
            }
            int rightBoundary = turn.RightBoundary;

            for (int i = 0; i < gameState.MobileStates.Length; i++)
            {
                MobileState mobileState = gameState.MobileStates[i];
                if (!mobileState.IsActive)
                {
                    continue;
                }
                if (i < Constants.TANK_COUNT)
                {
                    // Tank:
                    int leftEdgeX = mobileState.Pos.X - Constants.TANK_EXTENT_OFFSET;
                    int rightEdgeX = mobileState.Pos.X + Constants.TANK_EXTENT_OFFSET;
                    if (leftEdgeX < leftBoundary || rightEdgeX > rightBoundary)
                    {
                        gameState.MobileStates[i] = mobileState.Kill();
                    }
                }
                else
                {
                    // Bullet:
                    if ((mobileState.Pos.X < leftBoundary) || (mobileState.Pos.X > rightBoundary))
                    {
                        gameState.MobileStates[i] = mobileState.Kill();
                    }
                }
            }
        }

        public static void ApplyTankActions(MutableGameState gameState, TankAction[] tankActions, 
            bool[] bulletsThatMovedThisTurn, IList<Point> wallsRemoved)
        {
            MoveTanks(gameState, tankActions);
            FireBullets(gameState, tankActions, bulletsThatMovedThisTurn, wallsRemoved);
        }

        /// <summary>
        /// Separate out the moving of bullets from moving of tanks.
        /// This is so that distance calculations can take place AFTER the bullets are moved.
        /// ISSUE: What about when the board shrinks after time is up? 
        /// That could cause bullets to be removed during the next game tick, before they move.
        /// </summary>
        /// <param name="gameState"></param>
        public static void MoveBulletsTwice(MutableGameState gameState, IList<Point> wallsRemoved)
        {
            for (byte phase = 0; phase < 2; phase++)
            {
#if DEBUG
                CollisionStatus[] collisionStatuses = new CollisionStatus[Constants.ALL_ELEMENT_COUNT];
#endif
                Point[] wallsShot = new Point[MAX_WALLS_SHOT];
                byte wallsShotCount = 0;

 	            for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
                {
                    MobileState bulletState = gameState.MobileStates[b];
                    if (!bulletState.IsActive)
                    {
                        continue;
                    }
                    bulletState = bulletState.Move();
                    gameState.MobileStates[b] = bulletState;
                    if (bulletState.Pos.X < 0 || bulletState.Pos.Y < 0 
                        || bulletState.Pos.X >= Game.Current.BoardWidth
                        || bulletState.Pos.Y >= Game.Current.BoardHeight)
                    {
#if DEBUG
                        collisionStatuses[b] |= CollisionStatus.WithOutOfBoundsArea;
#endif
                        bulletState = bulletState.Kill();
                        gameState.MobileStates[b] = bulletState;
                    }
                    else
                    {
                        if (gameState.Walls[bulletState.Pos])
                        {
                            bulletState = bulletState.Kill();
                            gameState.MobileStates[b] = bulletState;

                            /* Shoot out the walls on the segment.
                             * Assume that they are all in bounds since the tank could not have shot the bullet otherwise
                             * TODO: What to do when the board shrinks after the time limit is up, 
                             * as then part of the segment could be out of bounds?
                             */
                            int first;
                            int last;
                            int x;
                            int y;

                            switch (bulletState.Dir)
                            {
                                case Direction.LEFT:
                                case Direction.RIGHT:
                                    x = bulletState.Pos.X;
                                    first = bulletState.Pos.Y - 2;
                                    last = bulletState.Pos.Y + 2;
                                    for (y = first; y <= last; y++)
                                    {
                                        if (gameState.Walls[x, y])
                                        {
                                            wallsShot[wallsShotCount] = new Point((short) x, (short) y);
                                            wallsShotCount++;
                                        }
                                    }
                                    break;

                                case Direction.DOWN:
                                case Direction.UP:
                                    y = bulletState.Pos.Y;
                                    first = bulletState.Pos.X - 2;
                                    last = bulletState.Pos.X + 2;
                                    for (x = first; x <= last; x++)
                                    {
                                        if (gameState.Walls[x, y])
                                        {
                                            wallsShot[wallsShotCount] = new Point((short) x, (short) y);
                                            wallsShotCount++;
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            // Check for swapping places with another bullet:
                            Direction oppositeDir = bulletState.Dir.GetOpposite();

                            // Check if colliding with a bullet that hasn't moved yet, and which is moving in the opposite direction:
                            for (int otherBulletIndex = b + 1; otherBulletIndex <= Constants.MAX_BULLET_INDEX; otherBulletIndex++)
                            {
                                MobileState otherBulletState = gameState.MobileStates[otherBulletIndex];
                                if (otherBulletState.IsActive 
                                    && (otherBulletState.Dir == oppositeDir)
                                    && (otherBulletState.Pos == bulletState.Pos))
                                {
                                    // Collision with another bullet
#if DEBUG
                                    collisionStatuses[b] |= CollisionStatus.WithBullet;
                                    collisionStatuses[otherBulletIndex] |= CollisionStatus.WithBullet;
#endif
                                    bulletState = bulletState.Kill();
                                    gameState.MobileStates[b] = bulletState;

                                    gameState.MobileStates[otherBulletIndex] = otherBulletState.Kill();
                                    break;
                                }

                                // TODO: What happens if two bullets collide, but then a tank moves into the two bullets?
                                // Check with Entelect and ensure logic works for this case
                            }
                        }
                    }
                }

                // Check for collisions between bullets, tanks and bases:
                for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
                {
                    MobileState bulletState = gameState.MobileStates[b];

                    // Only if bullet is active:
                    if (!bulletState.IsActive)
                    {
                        continue;
                    }

                    for (int i = 0; i < Constants.MOBILE_ELEMENT_COUNT; i++)
                    {
                        // Only if not self:
                        if (i == b)
                        {
                            continue;
                        }

                        // Only check active elements:
                        MobileState otherState = gameState.MobileStates[i];
                        if (!otherState.IsActive)
                        {
                            continue;
                        }

                        if (i < Constants.TANK_COUNT)
                        {
                            // Check for collision with a tank:
                            if (otherState.GetTankExtent().ContainsPoint(bulletState.Pos))
                            {
#if DEBUG
                                collisionStatuses[i] |= CollisionStatus.WithBullet;
                                collisionStatuses[b] |= CollisionStatus.WithTank;
#endif
                                bulletState = bulletState.Kill();
                                gameState.MobileStates[b] = bulletState;
                                gameState.MobileStates[i] = otherState.Kill();
                            }
                        }
                        else
                        {
                            // Check for collision with another bullet:
                            if (otherState.Pos == bulletState.Pos)
                            {
#if DEBUG
                                collisionStatuses[b] |= CollisionStatus.WithBullet;
                                collisionStatuses[i] |= CollisionStatus.WithBullet;
#endif
                                bulletState = bulletState.Kill();
                                gameState.MobileStates[b] = bulletState;
                                gameState.MobileStates[i] = otherState.Kill();
                            }
                        }
                    }

                    // Check for collisions with bases:
                    for (int p = 0; p < Constants.PLAYERS_PER_GAME; p++)
                    {
                        Base @base = Game.Current.Players[p].Base;
                        if (bulletState.Pos == @base.Pos)
                        {
#if DEBUG
                            collisionStatuses[b] |= CollisionStatus.WithBase;
#endif
                            bulletState = bulletState.Kill();
                            gameState.MobileStates[b] = bulletState;
                            gameState.Outcome |= (Outcome) (((byte) Outcome.Player1BaseKilled) << p);
                        }
                    }
                }

                // Remove walls that were shot:
                for (int w = 0; w < wallsShotCount; w++)
                {
                    if (wallsRemoved == null)
                    {
                        gameState.Walls[wallsShot[w]] = false;
                    }
                    else
                    {
                        Point wall = wallsShot[w];
                        if (gameState.Walls[wall])
                        {
                            gameState.Walls[wall] = false;
                            wallsRemoved.Add(wall);
                        }
                    }
                }
            }
        }

        private static void MoveTanks(MutableGameState gameState,TankAction[] tankActions)
        {
#if DEBUG
            CollisionStatus[] collisionStatuses = new CollisionStatus[Constants.ALL_ELEMENT_COUNT];
#endif
            foreach (int t in GameRuleConfiguration.RuleConfiguration.TankMovementIndexes)
            {
                MobileState tankState = gameState.MobileStates[t];
                if (!tankState.IsActive)
                {
                    continue;
                }
                bool canMove = true;
                bool willTurn = true;
                bool willDie = false;
                TankAction tankAction = tankActions[t];
                Direction movementDir = Direction.NONE;
                int firstSeg;
                int lastSeg;
                int segX = 0;
                int segY = 0;
                int newCentreX = tankState.Pos.X;
                int newCentreY = tankState.Pos.Y;

                switch (tankAction)
                {
                    case TankAction.UP:
                        movementDir = Direction.UP;
                        willTurn = (movementDir != tankState.Dir);
                        if (tankState.Pos.Y <= Constants.TANK_EXTENT_OFFSET)
                        {
                            // Trying to move off the edge of the board:
#if DEBUG
                            collisionStatuses[t] |= CollisionStatus.WithOutOfBoundsArea;
#endif
                            canMove = false;
                            willDie = GameRuleConfiguration.RuleConfiguration.DoesATankDieIfTryingToMoveOffTheBoard;
                            willTurn = willTurn && GameRuleConfiguration.RuleConfiguration.DoesATankTurnIfTryingToMoveOffTheBoard;
                        }
                        else
                        {
                            newCentreY--;
                            segY = newCentreY - Constants.TANK_EXTENT_OFFSET;
                        }
                        break;

                    case TankAction.DOWN:
                        movementDir = Direction.DOWN;
                        willTurn = (movementDir != tankState.Dir);
                        if (tankState.Pos.Y + Constants.TANK_OUTER_EDGE_OFFSET >= Game.Current.BoardHeight)
                        {
                            // Trying to move off the edge of the board:
#if DEBUG
                            collisionStatuses[t] |= CollisionStatus.WithOutOfBoundsArea;
#endif
                            canMove = false;
                            willDie = GameRuleConfiguration.RuleConfiguration.DoesATankDieIfTryingToMoveOffTheBoard;
                            willTurn = willTurn && GameRuleConfiguration.RuleConfiguration.DoesATankTurnIfTryingToMoveOffTheBoard;
                        }
                        else
                        {
                            newCentreY++;
                            segY = newCentreY + Constants.TANK_EXTENT_OFFSET;
                        }
                        break;

                    case TankAction.LEFT:
                        movementDir = Direction.LEFT;
                        willTurn = (movementDir != tankState.Dir);
                        if (tankState.Pos.X <= Constants.TANK_EXTENT_OFFSET)
                        {
                            // Trying to move off the edge of the board:
#if DEBUG
                            collisionStatuses[t] |= CollisionStatus.WithOutOfBoundsArea;
#endif
                            canMove = false;
                            willDie = GameRuleConfiguration.RuleConfiguration.DoesATankDieIfTryingToMoveOffTheBoard;
                            willTurn = willTurn && GameRuleConfiguration.RuleConfiguration.DoesATankTurnIfTryingToMoveOffTheBoard;
                        }
                        else
                        {
                            newCentreX--;
                            segX = newCentreX - Constants.TANK_EXTENT_OFFSET;
                        }
                        break;

                    case TankAction.RIGHT:
                        movementDir = Direction.RIGHT;
                        willTurn = (movementDir != tankState.Dir);
                        if (tankState.Pos.X + Constants.TANK_OUTER_EDGE_OFFSET >= Game.Current.BoardWidth)
                        {
                            // Trying to move off the edge of the board:
#if DEBUG
                            collisionStatuses[t] |= CollisionStatus.WithOutOfBoundsArea;
#endif
                            canMove = false;
                            willDie = GameRuleConfiguration.RuleConfiguration.DoesATankDieIfTryingToMoveOffTheBoard;
                            willTurn = willTurn && GameRuleConfiguration.RuleConfiguration.DoesATankTurnIfTryingToMoveOffTheBoard;
                        }
                        else
                        {
                            newCentreX++;
                            segX = newCentreX + Constants.TANK_EXTENT_OFFSET;
                        }
                        break;

                    default:
                        // case TankAction.FIRE:
                        // case TankAction.NONE:
                        canMove = false;
                        willTurn = false;
                        break;
                }

                if (canMove)
                {
                    switch (movementDir)
                    {
                        case Direction.DOWN:
                        case Direction.UP:
                            firstSeg = newCentreX - Constants.TANK_EXTENT_OFFSET;
                            lastSeg = newCentreX + Constants.TANK_EXTENT_OFFSET;
                            for (segX = firstSeg; segX <= lastSeg; segX++)
                            {
                                if (gameState.Walls[segX, segY])
                                {
                                    canMove = false;
                                    break;
                                }
                            }

                            if (canMove)
                            {
                                Rectangle newRect = new Rectangle(
                                    (short) (newCentreX - Constants.TANK_EXTENT_OFFSET),
                                    (short) (newCentreY - Constants.TANK_EXTENT_OFFSET),
                                    (short) (newCentreX + Constants.TANK_EXTENT_OFFSET),
                                    (short) (newCentreY + Constants.TANK_EXTENT_OFFSET));

                                // Check if moving into another tank:
                                for (int tOther = 0; tOther < Constants.TANK_COUNT; tOther++)
                                {
                                    if (tOther != t)
                                    {
                                        MobileState otherTankState = gameState.MobileStates[tOther];
                                        if (otherTankState.IsActive && otherTankState.GetTankExtent().IntersectsWith(newRect))
                                        {
                                            canMove = false;
                                            break;
                                        }
                                    }
                                }

                                if (canMove)
                                {
                                    // Check if moving into a bullet:
                                    for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
                                    {
                                        MobileState bulletState = gameState.MobileStates[b];
                                        if (bulletState.IsActive && (bulletState.Pos.Y == segY))
                                        {
                                            for (segX = firstSeg; segX <= lastSeg; segX++)
                                            {
                                                if (bulletState.Pos.X == segX)
                                                {
                                                    willDie = GameRuleConfiguration.RuleConfiguration.DoesATankDieIfMovingIntoABullet;
#if DEBUG
                                                    collisionStatuses[t] |= CollisionStatus.WithBullet;
#endif
                                                    if (willDie)
                                                    {
#if DEBUG
                                                        collisionStatuses[b] |= CollisionStatus.MovedIntoByATank;
#endif
                                                        bulletState = bulletState.Kill();
                                                        gameState.MobileStates[b] = bulletState;
                                                    }
                                                    canMove = !willDie;
                                                    break;
                                                }
                                            }
                                            if (willDie || !canMove)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            break;

                        case Direction.LEFT:
                        case Direction.RIGHT:
                            firstSeg = newCentreY - Constants.TANK_EXTENT_OFFSET;
                            lastSeg = newCentreY + Constants.TANK_EXTENT_OFFSET;
                            for (segY = firstSeg; segY <= lastSeg; segY++)
                            {
                                if (gameState.Walls[segX, segY])
                                {
                                    canMove = false;
                                    break;
                                }
                            }

                            if (canMove)
                            {
                                Rectangle newRect = new Rectangle(
                                    (short) (newCentreX - Constants.TANK_EXTENT_OFFSET),
                                    (short) (newCentreY - Constants.TANK_EXTENT_OFFSET),
                                    (short) (newCentreX + Constants.TANK_EXTENT_OFFSET),
                                    (short) (newCentreY + Constants.TANK_EXTENT_OFFSET));

                                // Check if moving into another tank:
                                for (int tOther = 0; tOther < Constants.TANK_COUNT; tOther++)
                                {
                                    if (tOther != t)
                                    {
                                        MobileState otherTankState = gameState.MobileStates[tOther];
                                        if (otherTankState.IsActive && otherTankState.GetTankExtent().IntersectsWith(newRect))
                                        {
                                            canMove = false;
                                            break;
                                        }
                                    }
                                }

                                if (canMove)
                                {
                                    // Check if moving into a bullet:
                                    for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
                                    {
                                        MobileState bulletState = gameState.MobileStates[b];
                                        if (bulletState.IsActive && (bulletState.Pos.X == segX))
                                        {
                                            for (segY = firstSeg; segY <= lastSeg; segY++)
                                            {
                                                if (bulletState.Pos.Y == segY)
                                                {
                                                    willDie = GameRuleConfiguration.RuleConfiguration.DoesATankDieIfMovingIntoABullet;
#if DEBUG
                                                    collisionStatuses[t] |= CollisionStatus.WithBullet;
#endif
                                                    if (willDie)
                                                    {
#if DEBUG
                                                        collisionStatuses[b] |= CollisionStatus.MovedIntoByATank;
#endif
                                                        bulletState = bulletState.Kill();
                                                        gameState.MobileStates[b] = bulletState;
                                                    }
                                                    canMove = !willDie;
                                                    break;
                                                }
                                            }
                                            if (willDie || !canMove)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }

                if (canMove || willTurn || willDie)
                {
                    if (canMove)
                    {
                        tankState = tankState.MoveTo(newCentreX, newCentreY);
                    }
                    if (willDie)
                    {
                        tankState = tankState.Kill();
                    }
                    if (willTurn)
                    {
                        tankState = tankState.ChangeDirection(movementDir);
                    }
                    gameState.MobileStates[t] = tankState;
                }
            }

            // Check for collisions with bases:
            for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
            {
                MobileState bulletState = gameState.MobileStates[b];
                if (bulletState.IsActive)
                {
                    for (int i = 0; i < Constants.MOBILE_ELEMENT_COUNT; i++)
                    {
                        if (i == b)
                        {
                            continue;
                        }

                        MobileState otherState = gameState.MobileStates[i];
                        if (!otherState.IsActive)
                        {
                            continue;
                        }

                        if (i < Constants.TANK_COUNT)
                        {
                            // Check for collision with a tank:
                            if (otherState.GetTankExtent().ContainsPoint(bulletState.Pos))
                            {
#if DEBUG
                                collisionStatuses[i] |= CollisionStatus.WithBullet;
                                collisionStatuses[b] |= CollisionStatus.WithTank;
#endif
                                bulletState = bulletState.Kill();
                                gameState.MobileStates[b] = bulletState;
                                gameState.MobileStates[i] = otherState.Kill();
                            }
                        }
                        else
                        {
                            // Check for collision with another bullet:
                            if (otherState.Pos == bulletState.Pos)
                            {
#if DEBUG
                                collisionStatuses[b] |= CollisionStatus.WithBullet;
                                collisionStatuses[i] |= CollisionStatus.WithBullet;
#endif
                                bulletState = bulletState.Kill();
                                gameState.MobileStates[b] = bulletState;
                                gameState.MobileStates[i] = otherState.Kill();
                            }
                        }
                    }

                    // Check for collisions with bases:
                    for (int p = 0; p < Constants.PLAYERS_PER_GAME; p++)
                    {
                        Base @base = Game.Current.Players[p].Base;
                        if (bulletState.Pos == @base.Pos)
                        {
#if DEBUG
                            collisionStatuses[b] |= CollisionStatus.WithBase;
#endif
                            bulletState = bulletState.Kill();
                            gameState.MobileStates[b] = bulletState;
                            gameState.Outcome |= (Outcome) (((byte) Outcome.Player1BaseKilled) << p);
                        }
                    }
                }
            }
        }

        private static void FireBullets(MutableGameState gameState, TankAction[] tankActions, bool[] bulletsThatMovedThisTurn, IList<Point> wallsRemoved)
        {
            bool[] bulletWasFired = new bool[Constants.TANK_COUNT];
            Point[] wallsShot = new Point[MAX_WALLS_SHOT];
            byte wallsShotCount = 0;
#if DEBUG
            CollisionStatus[] collisionStatuses = new CollisionStatus[Constants.ALL_ELEMENT_COUNT];
#endif
            for (int t = 0; t < tankActions.Length; t++)
            {
                TankAction tankAction = tankActions[t];
                if (tankAction != TankAction.FIRE)
                {
                    continue;
                }

                MobileState tankState = gameState.MobileStates[t];
                if (!tankState.IsActive)
                {
                    continue;
                }

                // A bullet can't be fired if there is already an active bullet from the same tank:
                int bulletIndex = t + Constants.TANK_COUNT;
                MobileState bulletState = gameState.MobileStates[bulletIndex];
                if (bulletState.IsActive)
                {
                    continue;
                }

                // If a bullet was active this turn but was destroyed, 
                // then the tank probably still can't fire another bullet yet:
                if (bulletsThatMovedThisTurn[t]
                    && !GameRuleConfiguration.RuleConfiguration.CanATankFireAgainOnTheSameTurnThatItsBulletWasDestroyed)
                {
                    continue;
                }

                bulletState = tankState.FireABulletAndGetItsState();

                // A bullet can't be shot outside the board:
                if (bulletState.Pos.X < 0 
                    || bulletState.Pos.Y < 0
                    || bulletState.Pos.X >= Game.Current.BoardWidth
                    || bulletState.Pos.Y >= Game.Current.BoardHeight)
                {
#if DEBUG
                    collisionStatuses[bulletIndex] |= CollisionStatus.WithOutOfBoundsArea;
#endif
                    bulletState = bulletState.Kill();
                    gameState.MobileStates[bulletIndex] = bulletState;
                    continue;
                }

                // Check if there is a wall to shoot:
                if (!gameState.Walls[bulletState.Pos])
                {
                    gameState.MobileStates[bulletIndex] = bulletState;
                    bulletWasFired[t] = true;
                }
                else
                {
                    bulletState = bulletState.Kill();
                    gameState.MobileStates[bulletIndex] = bulletState;

                    /* Shoot out the walls on the segment.
                        * Assume that they are all in bounds since the tank could not have shot the bullet otherwise
                        * TODO: What to do when the board shrinks after the time limit is up, 
                        * as then part of the segment could be out of bounds?
                        */
                    int first;
                    int last;
                    int x;
                    int y;

                    switch (bulletState.Dir)
                    {
                        case Direction.LEFT:
                        case Direction.RIGHT:
                            x = bulletState.Pos.X;
                            first = bulletState.Pos.Y - 2;
                            last = bulletState.Pos.Y + 2;
                            for (y = first; y <= last; y++)
                            {
                                if (gameState.Walls[x, y])
                                {
                                    wallsShot[wallsShotCount] = new Point((short)x, (short)y);
                                    wallsShotCount++;
                                }
                            }
                            break;

                        case Direction.DOWN:
                        case Direction.UP:
                            y = bulletState.Pos.Y;
                            first = bulletState.Pos.X - 2;
                            last = bulletState.Pos.X + 2;
                            for (x = first; x <= last; x++)
                            {
                                if (gameState.Walls[x, y])
                                {
                                    wallsShot[wallsShotCount] = new Point((short)x, (short)y);
                                    wallsShotCount++;
                                }
                            }
                            break;
                    }
                }
            }

            // Check for collisions between bullets, tanks and bases:
            for (int t = 0; t < Constants.TANK_COUNT; t++)
            {
                if (!bulletWasFired[t])
                {
                    continue;
                }

                int b = t + Constants.TANK_COUNT;
                MobileState bulletState = gameState.MobileStates[b];
                if (!bulletState.IsActive)
                {
                    continue;
                }

                // Check for collisions with other bullets, tanks and bases:
                for (int i = 0; i < Constants.MOBILE_ELEMENT_COUNT; i++)
                {
                    if (i == b)
                    {
                        continue;
                    }

                    MobileState otherState = gameState.MobileStates[i];
                    if (otherState.IsActive)
                    {
                        if (i < Constants.TANK_COUNT)
                        {
                            // Check for collision with a tank:
                            if (otherState.GetTankExtent().ContainsPoint(bulletState.Pos))
                            {
#if DEBUG
                                collisionStatuses[i] |= CollisionStatus.WithBullet;
                                collisionStatuses[b] |= CollisionStatus.WithTank;
#endif
                                bulletState = bulletState.Kill();
                                gameState.MobileStates[b] = bulletState;
                                gameState.MobileStates[i] = otherState.Kill();
                            }
                        }
                        else
                        {
                            // Check for collision with another bullet:
                            if (otherState.Pos == bulletState.Pos)
                            {
#if DEBUG
                                collisionStatuses[b] |= CollisionStatus.WithBullet;
                                collisionStatuses[i] |= CollisionStatus.WithBullet;
#endif
                                bulletState = bulletState.Kill();
                                gameState.MobileStates[b] = bulletState;
                                gameState.MobileStates[i] = otherState.Kill();
                            }
                        }
                    }
                }

                // Check for collisions with bases:
                for (int p = 0; p < Constants.PLAYERS_PER_GAME; p++)
                {
                    Base @base = Game.Current.Players[p].Base;
                    if (bulletState.Pos == @base.Pos)
                    {
#if DEBUG
                        collisionStatuses[b] |= CollisionStatus.WithBase;
#endif
                        bulletState = bulletState.Kill();
                        gameState.MobileStates[b] = bulletState;
                        gameState.Outcome |= (Outcome)(((byte)Outcome.Player1BaseKilled) << p);
                    }
                }
            }

            // Remove walls that were shot:
            for (int w = 0; w < wallsShotCount; w++)
            {
                if (wallsRemoved == null)
                {
                    gameState.Walls[wallsShot[w]] = false;
                }
                else
                {
                    Point wall = wallsShot[w];
                    if (gameState.Walls[wall])
                    {
                        gameState.Walls[wall] = false;
                        wallsRemoved.Add(wall);
                    }
                }
            }
        }

        private static void CheckForNoMoreTanksAndBullets(MutableGameState gameState)
        {
            if (gameState.IsGameOver)
            {
                return;
            }

            int elementCountToCheck
                = GameRuleConfiguration.RuleConfiguration.DoesTheGameEndInADrawWhenTheLastBulletIsGone
                ? Constants.MOBILE_ELEMENT_COUNT
                : Constants.TANK_COUNT;

 	        for (int i = 0; i < elementCountToCheck; i++)
            {
                if (gameState.MobileStates[i].IsActive)
                {
                    return;
                }
            }

            // There are no more tanks (and possibly bullets) on the board, so the game is a draw:
            gameState.Outcome |= Outcome.NoElementsLeftDraw;
        }

    }
}
