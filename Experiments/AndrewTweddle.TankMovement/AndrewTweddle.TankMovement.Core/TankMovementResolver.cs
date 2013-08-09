using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AndrewTweddle.TankMovement.Core
{
    public class TankMovementResolver
    {
        #region Input Properties

        public GameRuleConfiguration RuleConfiguration { get; private set; }

        /// <summary>
        /// Each element in the 2-D array is true if there is a wall in that position
        /// </summary>
        public bool[,] IsAWall { get; private set; }

        /// <summary>
        /// The 4 tanks in the game.
        /// Leave a slot null for a destroyed tank.
        /// </summary>
        public Unit[] Tanks { get; private set; }

        /// <summary>
        /// The state of the 4 bullets in the game after the two phases of bullet movement.
        /// These correspond to the 4 tanks.
        /// Leave a slot null when the corresponding tank doesn't have a bullet in play.
        /// </summary>
        public Bullet[] Bullets { get; private set; }

        #endregion
        
        
        #region Output Properties

        public MoveRequest[] Requests { get; private set; }

        /// <summary>
        /// Each row represents a depending tank.
        /// Each column represents the tank depended on.
        /// </summary>
        public DependencyType[,] DependencyMatrix { get; private set; }

        #endregion


        #region Convenience Properties

        public int BoardWidth  { get { return IsAWall.GetLength(0); } }
        public int BoardHeight { get { return IsAWall.GetLength(1); } }

        #endregion

        #region Diagnostic properties

        public TimeSpan ResolutionDuration { get; private set; }

        #endregion

        #region Constructors

        protected TankMovementResolver()
        {
            Bullets = new Bullet[Constants.TANK_COUNT];
            DependencyMatrix = new DependencyType[Constants.TANK_COUNT, Constants.TANK_COUNT];
            Requests = new MoveRequest[Constants.TANK_COUNT];
        }

        public TankMovementResolver(bool[,] isAWall, Unit[] tanks, Bullet[] bullets, GameRuleConfiguration ruleConfiguration)
            : this()
        {
            RuleConfiguration = ruleConfiguration;
            if (tanks.Length != Constants.TANK_COUNT)
            {
                throw new ApplicationException(
                    String.Format("Exactly {0} tanks are expected!", Constants.TANK_COUNT));
            }

            // For convenience (mainly for testing), allow no bullets to be provided:
            if (bullets == null || bullets.Length == 0)
            {
                bullets = new Bullet[Constants.TANK_COUNT];
            }
            else
                if (bullets.Length != Constants.TANK_COUNT)
                {
                    throw new ApplicationException(
                        String.Format("Exactly {0} bullets are expected (or none)!", Constants.TANK_COUNT));
                }
                else
                {
                    Bullets = bullets;
                }
            Tanks = tanks;
            IsAWall = isAWall;
        }

        #endregion

        #region Methods

        public void ResolveMoves()
        {
            Stopwatch sw = Stopwatch.StartNew(); 
            GenerateMoveRequests();
            GenerateDependencyMatrix();
            CancelTankMovesBlockedByWallsOrOutOfBoundsAreas();

            ResolveUnresolvedMoves();  
            // This continues resolving moves until all are resolved or unresolvable
            // To step through (and visualize the status of the algorithm) rather call the following code repeatedly until wasAMoveResolved is false:
            // ResolveNextMoves(out wasAMoveResolved, nextMoveOnly: true);

            // All remaining unresolved tanks are dependent on one another in some way. So cancel all of their moves:
            CancelAllUnresolvedMoveRequests();
            sw.Stop();
            ResolutionDuration = sw.Elapsed;
        }

        public void GenerateMoveRequests()
        {
            for (int i = 0; i < Tanks.Length; i++)
            {
                Unit tank = Tanks[i];
                MoveRequest request = new MoveRequest(tank);
                /* Note: The constructor automatically calculates properties such as: 
                 *       Status, NewLeadingEdge, OldTrailingEdge, UnchangedBody, OldPosition, NewPosition
                 */
                Requests[i] = request;
            }
        }

        public void GenerateDependencyMatrix()
        {
            for (int i = 0; i < Requests.Length; i++)
            {
                MoveRequest request = Requests[i];
                if (request.Status == MoveStatus.Unresolved)
                {
                    for (int j = 0; j < Requests.Length; j++)
                    {
                        if (j != i)
                        {
                            MoveRequest otherRequest = Requests[j];
                            switch (otherRequest.Status)
                            {
                                case MoveStatus.TankDoesNotExist:
                                    break;

                                case MoveStatus.NotMoving:
                                case MoveStatus.Cancelled:
                                    if (request.NewLeadingEdge.IntersectsWith(otherRequest.UnchangedBody))
                                    {
                                        DependencyMatrix[i, j] = DependencyType.OnUnchangedBody;
                                    }
                                    break;

                                case MoveStatus.Approved:
                                case MoveStatus.Unresolved:
                                    if (request.NewLeadingEdge.IntersectsWith(otherRequest.UnchangedBody))
                                    {
                                        DependencyMatrix[i, j] = DependencyType.OnUnchangedBody;
                                    }
                                    else
                                        if (request.NewLeadingEdge.IntersectsWith(otherRequest.OldTrailingEdge))
                                        {
                                            DependencyMatrix[i, j] = DependencyType.OnOldTrailingEdge;
                                        }
                                        else
                                            if (request.NewLeadingEdge.IntersectsWith(otherRequest.NewLeadingEdge))
                                            {
                                                if (otherRequest.NewLeadingEdge.IntersectsWith(request.UnchangedBody))
                                                {
                                                    /* The other tank is trying to move into space already occupied by this tank.
                                                     * So there is no dependency on the other tank.
                                                     */
                                                    DependencyMatrix[i, j] = DependencyType.None;
                                                }
                                                else
                                                {
                                                    // Neither tank is already in the space the other is trying to move to, so neither has preference:
                                                    DependencyMatrix[i, j] = DependencyType.OnNewLeadingEdge;
                                                }
                                            }
                                            else
                                            {
                                                DependencyMatrix[i, j] = DependencyType.None;
                                            }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public void CancelTankMovesBlockedByWallsOrOutOfBoundsAreas()
        {
            for (int i = 0; i < Requests.Length; i++)
            {
                MoveRequest request = Requests[i];
                if (request.Status == MoveStatus.Unresolved)
                {
                    Rectangle nle = request.NewLeadingEdge;

                    // Check if out-of-bounds:
                    if (nle.TopLeft.X < 0 || nle.TopLeft.Y < 0 || nle.BottomRight.X >= BoardWidth || nle.BottomRight.Y >= BoardHeight)
                    {
                        CancelMove(i);
                        continue;
                    }

                    // Check if any of the cells in the new leading edge is a wall:
                    if (nle.GetPoints().Any(pt => IsAWall[pt.X, pt.Y]))
                    {
                        CancelMove(i);
                    }
                }
            }
        }

        // The following method is left public, so that the operation of the algorithm could be visualized in steps:
        public void ResolveUnresolvedMoves()
        {
            bool wasAMoveResolved = false;

            // Repeatedly look for a tank which has not dependencies on any other tanks and move it.
            // This may lead to changes in the dependencies of other tanks which were depending on this tank.
            do
            {
                ResolveNextMoves(out wasAMoveResolved);
            }
            while (wasAMoveResolved);
        }

        /// <summary>
        /// Resolve the next move 
        /// </summary>
        /// <param name="wasAMoveResolved">
        /// Returns true once all moves are resolved, or a set of circular dependencies is found which prevents further resolution
        /// </param>
        /// <param name="nextMoveOnly">
        /// Set to true to only run the next move (useful for stepping through).
        /// Set to false (the default) to complete a single pass through all the tanks, resolving all that can be resolved in that pass. This is more efficient.
        /// </param>
        public void ResolveNextMoves(out bool wasAMoveResolved, bool nextMoveOnly = false)
        {
            wasAMoveResolved = false;
            for (int i = 0; i < Requests.Length; i++)
            {
                MoveRequest request = Requests[i];
                if (request.Status == MoveStatus.Unresolved)
                {
                    // A move can be resolved if there are no dependencies on another tank:
                    bool canResolve = true;
                    for (int j = 0; j < Constants.TANK_COUNT; j++)
                    {
                        if (i != j && DependencyMatrix[i, j] != DependencyType.None)
                        {
                            canResolve = false;
                            break;
                        }
                    }

                    if (canResolve)
                    {
                        ApproveMove(i);  // Note: This might still lead to a destroyed tank if it moves into a bullet
                        wasAMoveResolved = true;
                        if (nextMoveOnly)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void CancelAllUnresolvedMoveRequests()
        {
            for (int i = 0; i < Requests.Length; i++)
            {
                MoveRequest request = Requests[i];
                if (request.Status == MoveStatus.Unresolved)
                {
                    request.Status = MoveStatus.Cancelled;
                    // This is safer than calling CancelMove() which might have unexpected side-effects
                }
            }
        }

        private void CancelMove(int moveRequestIndex)
        {
            MoveRequest request = Requests[moveRequestIndex];
            request.Status = MoveStatus.Cancelled;

            // Update dependencies on this tank, based on its move being cancelled:
            for (int i = 0; i < Requests.Length; i++)
            {
                if (i != moveRequestIndex && Requests[i].Status == MoveStatus.Unresolved)
                {
                    // Determine how tank i depends on this tank:
                    DependencyType depType = DependencyMatrix[i, moveRequestIndex];
                    switch (depType)
                    {
                        case DependencyType.OnNewLeadingEdge:
                            // Since the move is cancelled, the new leading edge will never occur, so is no longer a dependency:
                            DependencyMatrix[i, moveRequestIndex] = DependencyType.None;
                            break;

                        case DependencyType.OnUnchangedBody:
                        case DependencyType.OnOldTrailingEdge:
                            // Since the tank move has been cancelled, its old trailing edge is now blocking tank i's move:
                            CancelMove(i);
                            break;
                    }
                }
            }
        }

        private void ApproveMove(int moveRequestIndex)
        {
            MoveRequest request = Requests[moveRequestIndex];
            if (IsTankMovingIntoABullet(request))
            {
                request.Status = MoveStatus.Destroyed;
            }
            else
            {
                request.Status = MoveStatus.Approved;
            }

            // Update dependencies on this tank, based on its move being approved:
            for (int i = 0; i < Requests.Length; i++)
            {
                if (i != moveRequestIndex && Requests[i].Status == MoveStatus.Unresolved)
                {
                    // Determine how tank i depends on this tank:
                    DependencyType depType = DependencyMatrix[i, moveRequestIndex];
                    switch (depType)
                    {
                        case DependencyType.OnNewLeadingEdge:
                            // Since the move is approved, the new leading edge becomes a barrier (in theory this should never happen):
                            CancelMove(i);
                            break;

                        case DependencyType.OnUnchangedBody:
                            if (request.Status == MoveStatus.Destroyed && RuleConfiguration.CanATankMoveIntoTheSpaceLeftByATankThatJustMovedIntoABullet)
                            {
                                // Since the tank was destroyed, and the rules allow moving into its space, remove the dependency that was blocking tank i:
                                DependencyMatrix[i, moveRequestIndex] = DependencyType.None;
                            }
                            else
                            {
                                // Otherwise the tank is blocked by the other tank (which was there first, 
                                // since there is a dependency on the unchanged cells in its body), so cancel its move:
                                CancelMove(i);
                            }
                            break;

                        case DependencyType.OnOldTrailingEdge:
                            // Since the tank move has been approved, its old trailing edge is no longer blocking tank i's move:
                            DependencyMatrix[i, moveRequestIndex] = DependencyType.None;
                            break;
                    }
                }
            }
        }

        public bool IsTankMovingIntoABullet(MoveRequest request)
        {
            for (int b = 0; b < Bullets.Length; b++)
            {
                Bullet bullet = Bullets[b];
                if (bullet != null && request.NewLeadingEdge.ContainsPoint(new Point(bullet.X, bullet.Y)))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Extensibility support

        /// <summary>
        /// This method can be called for applying all the resolved moves.
        /// To make this truly useful, the protected virtual template methods 
        /// lower down will probably need to be overridden in a derived class.
        /// </summary>
        public void ApplyMoves()
        {
            for (int i = 0; i < Requests.Length; i++)
            {
                MoveRequest request = Requests[i];
                switch (request.Status)
                {
                    case MoveStatus.Approved:
                        MoveUnit(request);
                        break;

                    case MoveStatus.Destroyed:
                        DestroyUnit(i);
                        break;

                    case MoveStatus.Cancelled:
                        if (request.NewDirection != request.Tank.Direction)
                        {
                            // The tank is still changing direction
                            ChangeDirectionOfUnit(request);
                        }
                        else
                        {
                            UnitWillNotMove(request);
                        }
                        break;

                    case MoveStatus.NotMoving:
                        UnitWillNotMove(request);
                        break;
                }
            }
        }

        #region Template methods to be overridden to customize behaviour

        /* These template methods are called if ApplyMoves() is called.
         * 
         * The default implementations here are to update the Direction, X and Y properties of the Unit and Bullet objecdts.
         * When a tank or bullet is destroyed, the corresponding slot in the Tanks and Bullets arrays is set to null.
         * 
         * Override these template methods in derived classes to implement the appropriate update logic.
         */

        protected virtual void MoveUnit(MoveRequest request)
        {
            request.Tank.X = request.NewPosition.X;
            request.Tank.Y = request.NewPosition.Y;
            request.Tank.Direction = request.NewDirection;
            // NOTE: If on the server, override to do whatever is required to generate unit events to move the tank
        }

        protected virtual void ChangeDirectionOfUnit(MoveRequest request)
        {
            request.Tank.Direction = request.NewDirection;
            // NOTE: If on the server, override to do whatever is required to generate unit events to move the tank
        }

        /// <summary>
        /// Derived classes can override DestroyUnit to update their status to indicate that the tank was destroyed.
        /// They should also remove any bullets which were in the cells of request.NewLeadingEdge.
        /// They could do this by calling RemoveBulletThatATankMovedInto(MoveRequest).
        /// </summary>
        /// <param name="request">The movement request for a particular tank</param>
        protected virtual void DestroyUnit(int moveRequestIndex)
        {
            // Remove bullets:
            MoveRequest request = Requests[moveRequestIndex];
            RemoveBulletsThatATankMovedInto(request);

            Tanks[moveRequestIndex] = null;
            // NOTE: If on the server, override to do whatever is required to destroy the tank
        }

        protected virtual void UnitWillNotMove(MoveRequest request)
        {
            // NOTE: If on the server, override to do whatever is required for a tank that is not moving
        }

        protected virtual void RemoveBulletsThatATankMovedInto(MoveRequest request)
        {
            for (int b = 0; b < Bullets.Length; b++)
            {
                Bullet bullet = Bullets[b];
                if (bullet != null && request.NewLeadingEdge.ContainsPoint(new Point(bullet.X, bullet.Y)))
                {
                    Bullets[b] = null;
                }
            }
            // NOTE: If on the server, override to do whatever is required to destroy any bullets which the tank moved into
        }

        #endregion

        #endregion
    }
}
