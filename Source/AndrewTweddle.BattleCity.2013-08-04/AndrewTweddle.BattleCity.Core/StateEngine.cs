using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core
{
    public static class StateEngine
    {
        const byte MAX_WALLS_SHOT = 20;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="actions">An array of 4 actions - 1 per tank</param>
        public void AdvanceState(GameState gameState, Action[] actions)
        {
            MoveElements(gameState, actions, moveBulletsOnly:true);
            MoveElements(gameState, actions, moveBulletsOnly:false);
            FireBullets(gameState, actions);
        }

        private void MoveElements(GameState gameState, Action[] actions, bool moveBulletsOnly = false)
        {
            // Clone the array of states, so we can compare before and after:
            MobileState[] newMobileStates = gameState.CloneMobileStates();
            CollisionFlag[] collisionFlags = new CollisionFlag[Constants.ALL_ELEMENT_COUNT];
            Position[] wallsShot = new Position[MAX_WALLS_SHOT];
            byte wallsShotCount = 0;

            byte startIndex, endIndex;
            if (moveBulletsOnly)
            {
                startIndex = Constants.MIN_BULLET_INDEX;
                endIndex = Constants.MAX_BULLET_INDEX;
            }
            else
            {
                startIndex = 0;
                endIndex = Constants.MOBILE_ELEMENT_COUNT - 1;
            }

            bool isBullet;

            for (byte elementIndex = startIndex; elementIndex < endIndex; elementIndex++)
            {
                MobileState newState = newMobileStates[elementIndex];
                if (newState.IsActive)
                {
                    Action action = Action.NONE;
                    bool isMoving = true;

                    if (moveBulletsOnly)
                    {
                        isBullet = true;
                    }
                    else
                    {
                        isBullet = (elementIndex & Constants.ELEMENT_TYPE_MASK) == Constants.BULLET_MASK_VALUE;
                        if (!isBullet)
                        {
                            // Tanks will always change their direction to be the direction of the move:
                            action = actions[elementIndex];
                            switch (action)
                            {
                                case Action.DOWN:
                                    newState.Dir = Direction.DOWN;
                                    break;
                                case Action.LEFT:
                                    newState.Dir = Direction.LEFT;
                                    break;
                                case Action.RIGHT:
                                    newState.Dir = Direction.RIGHT;
                                    break;
                                case Action.UP:
                                    newState.Dir = Direction.UP;
                                    break;
                                default:
                                    isMoving = false;
                                    break;
                            }
                        }
                    }

                    if (!isMoving)
                    {
                        continue;
                    }

                    int newX = newState.Pos.X + DirectionHelper.GetXOffsetForDirection(newState.Dir);
                    int newY = newState.Pos.Y + DirectionHelper.GetYOffsetForDirection(newState.Dir);
                    int newLeftX, newRightX, newTopY, newBottomY;

                    if (isBullet)
                    {
                        newLeftX = newRightX = newX;
                        newTopY = newBottomY = newY;
                    }
                    else
                    {
                        newLeftX = newX - Constants.TANK_EXTENT_OFFSET;
                        newRightX = newX + Constants.TANK_EXTENT_OFFSET;
                        newTopY = newY - Constants.TANK_EXTENT_OFFSET;
                        newBottomY = newY + Constants.TANK_EXTENT_OFFSET;
                    }
                    
                    // Check if out-of-bounds:
                    // TODO: Use the direction to decide which of these conditions to check
                    if (newLeftX < 0 || newRightX >= gameState.Game.BoardWidth || newTopY < 0 || newBottomY >= gameState.Game.BoardHeight)
                    {
                        collisionFlags[elementIndex] |= CollisionFlag.WithOutOfBoundsArea;
                        if (isBullet)
                        {
                            // The bullet is out of bounds:
                            newState.IsActive = false;
                        }
                        // A tank's move will be ignored if it tries to move off the board
                    }
                    else
                    {
                        // Update the new position:
                        newState.Pos.X = newX;
                        newState.Pos.Y = newY;

                        // Track status:
                        bool isStillActive = true;
                        bool canMove = true;

                        // Check if there is another bullet with the same original location and opposite direction:
                        if (isBullet)
                        {
                            Direction oppositeDirection = DirectionHelper.GetOppositeDirection(newState.Dir);

                            // Only include bullets which have already been moved, to avoid checking each pair twice:
                            for (int otherBulletIndex = Constants.MIN_BULLET_INDEX; otherBulletIndex < elementIndex; otherBulletIndex++)
                            {
                                MobileState otherOriginalBulletState = gameState.MobileStates[otherBulletIndex];
                                if (otherOriginalBulletState.IsActive && otherOriginalBulletState.Dir == oppositeDirection 
                                    && otherOriginalBulletState.Pos.X == newX && otherOriginalBulletState.Pos.Y == newY)
                                {
                                    // Collision with another bullet
                                    collisionFlags[elementIndex] |= CollisionFlag.WithBullet;
                                    collisionFlags[otherBulletIndex] |= CollisionFlag.WithBullet;
                                    newState.IsActive = false;
                                    newMobileStates[otherBulletIndex].IsActive = false;
                                    isStillActive = false;
                                    break;
                                }

                                // TODO: What happens if two bullets collide, but a tank moves into the two bullets?
                                // Check with Entelect and ensure logic works for this case
                            }
                        }

                        // Check if the element has collided with a wall:
                        if (isStillActive)
                        {
                            if (isBullet)
                            {
                                bool hasCollidedWithWall = gameState.IsWall(newState.Pos);
                                if (hasCollidedWithWall)
                                {
                                    newState.IsActive = false;
                                    isStillActive = false;
                                    collisionFlags[elementIndex] |= CollisionFlag.WithWall;

                                    Position[][] wallPositions = DirectionHelper.GetAdjacentWallPositionsInTwoSegments(gameState.Game, newState.Pos, newState.Dir);
                                    foreach (Position[] wallPosSegment in wallPositions)
                                    {
                                        foreach (Position wallPos in wallPosSegment)
                                        {
                                            if (gameState.IsWall(wallPos))
                                            {
                                                wallsShot[wallsShotCount] = wallPos;
                                                wallsShotCount++;
                                            }
                                            else
                                            {
                                                // TODO: Check with Entelect whether a gap in the wall of 5 bricks causes shrapnel to propagate further or not
                                            
                                                // If there is a gap in the wall, then don't shoot the shrapnel any further:
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Position endOfBarrel = Helpers.DirectionHelper.GetEndOfBarrel(newState.Pos, newState.Dir);
                                Position[] leadingEdgeOfTank = Helpers.DirectionHelper.GetAdjacentWallPositions(gameState.Game, endOfBarrel, newState.Dir);
                                foreach (Position leadingEdgePos in leadingEdgeOfTank)
                                {
                                    if (gameState.IsWall(leadingEdgePos))
                                    {
                                        collisionFlags[elementIndex] |= CollisionFlag.WithWall;
                                        newState.Pos = gameState.MobileStates[elementIndex].Pos;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Check for collisions between any pair of elements:
            for (byte i = startIndex; i < endIndex; i++)
            {
                MobileState newState = newMobileStates[i];
                if (newState.IsActive)
                {
                    if (moveBulletsOnly)
                    {
                        isBullet = true;
                    }
                    else
                    {
                        isBullet = (i & Constants.ELEMENT_TYPE_MASK) == Constants.BULLET_MASK_VALUE;
                    }

                    // Determine extent of the state:
                    int startX1, endX1, startY1, endY1;
                    if (isBullet)
                    {
                        startX1 = endX1 = newState.Pos.X;
                        startY1 = endY1 = newState.Pos.Y;
                    }
                    else
                    {

                    }

                    for (int j = i + 1; j < Constants.MOBILE_ELEMENT_COUNT; j++)
                    {
                        MobileState otherNewState = newMobileStates[j];
                    }
                


                if (newMobileState.IsActive)
                {
                    for (int j = 0; j < i; j++)
                    {
                        MobileState otherNewMobileState = newMobileStates[j];

                    }
                }
            }

            // Update the game state:

        }

        private void FireBullets(GameState gameState, Action[] actions)
        {
            throw new NotImplementedException();
        }
    }
}
