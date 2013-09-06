using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;
using AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Calculations;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine.Scenarios
{
    public class LockDownEnemyTankForOtherTankToDestroyScenario: Scenario
    {
        private const int THRESHOLD_DISTANCE_CONSIDERED_CLOSE_TO_LOCKDOWN = 23;

        public LockDownEnemyTankForOtherTankToDestroyScenario(GameState gameState, GameSituation gameSituation)
            : base(gameState, gameSituation)
        {
        }

        public override MoveGenerator[] GetMoveGeneratorsByMoveTreeLevel()
        {
            return new MoveGenerator[]
            {
                new MoveGeneratorOfPlayers(),
                new MoveGeneratorOfTankCombinationsForPlayerP(),
                new MoveGeneratorOfDirectionsForDir1(ScenarioDecisionMaker.p),
                new MoveGeneratorOfDirectionsForDir2NotEqualToDir1(ScenarioDecisionMaker.p),
                new MoveGeneratorOfDirectionsForDir3NotOppositeDir1(ScenarioDecisionMaker.pBar),
                // NOT NEEDED, SYMMETRICAL: new MoveGeneratorOfTankCombinationsForPlayerPBar()
            };
        }

        // Tank t_p_i is the tank that locks down the enemy tank
        // Tank t_pBar_j is the enemy tank that gets locked down
        // Tank t_p_j is the one that finishes off the locked down tank
        // Tank t_pBar_jBar is the other enemy tank. It must not be able to attack t_p_i sooner than t_p_iBar can attack t_pBar_j
        // Note: Rely on scenario 1 to prevent a situation where the locked down enemy tank is destroyed, but the other enemy tank reaches the base first
        // dir1 is the direction that t_p_i attacks t_pBar_j from (and their attack direction is the opposite)
        // dir2 is the direction that t_p_iBar attack t_pBar_j from - and must be different from dir1
        // dir3 is the direction that t_pBar_jBar attacks t_p_i from - and must be different from dir1.GetOpposite()
        public override MoveResult EvaluateLeafNodeMove(Move move)
        {
            MoveResult moveResult = new MoveResult(move);
            if (!IsValid(move))
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Invalid;
                return moveResult;
            }

            MobileState tankState_i = GetTankState_i(move);
            MobileState tankState_j = GetTankState_j(move);

            // Note that the following could be Direction.None if in line...
            Direction horizDir = tankState_i.Pos.GetHorizontalDirectionToPoint(tankState_j.Pos);
            Direction vertDir = tankState_i.Pos.GetVerticalDirectionToPoint(tankState_j.Pos);

            // Don't try and attack from a different direction than the two obvious ones:
            if ((move.dir1 == horizDir.GetOpposite()) || (move.dir1 == vertDir.GetOpposite()))
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Invalid;
                return moveResult;
            }

            int tank_i_moveDistance_to_j = base.GetDistanceFromTankToTargetTank(move.p, move.i, move.pBar, move.j);

            if (tank_i_moveDistance_to_j > THRESHOLD_DISTANCE_CONSIDERED_CLOSE_TO_LOCKDOWN)
            {
                moveResult = EvaluateLeafNodeMoveWhenTanksAreNotCloseToLockDown(
                    move, tankState_i, tankState_j, horizDir, vertDir);
                return moveResult;
            }

            // ***************************************************
            // We are in close-range lock-down mode.
            
            Direction movementDir;
            if (horizDir != move.dir1)
            {
                movementDir = horizDir;
            }
            else
            {
                movementDir = vertDir;
            }

            int tank_j_attackDistance_to_i = base.GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
                move.pBar, move.j, tankState_i.Pos, move.dir1.GetOpposite(), TankHelper.EdgeOffsets);
            int tank_i_attackDistance_to_j = base.GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
                move.p, move.i,  tankState_i.Pos, move.dir1.GetOpposite(), TankHelper.EdgeOffsets);
            Point newPos = tankState_i.Pos;

            if (movementDir == Direction.NONE)
            {
                // In lock-down position. Hope we don't get shot!
                movementDir = move.dir1;
                bool moveCloser = tank_j_attackDistance_to_i > 3;
                TankAction[] tankActions = GetTankActionsFromTankToAttackTankAtPointAlongDirectionOfMovement(
                    move.p, move.i, tankState_j.Pos, move.dir1, new EdgeOffset[] { EdgeOffset.Centre },
                    keepMovingCloserOnFiringLastBullet: moveCloser);
                if (tankActions.Length > 0)
                {
                    TankActionRecommendation tankActRec = new TankActionRecommendation
                    {
                        IsAMoveRecommended = true,
                        RecommendedTankAction = tankActions[0]
                    };
                    moveResult.SetTankActionRecommendation(move.p, move.i, tankActRec);
                }
            }
            else
            {
                // Move towards the enemy tank, but be careful not to become a target:

                // TODO: Check not walking into a bullet

                // Time has run out for the competition. Just move, don't calculate if it's a good idea...
                Point movementOffset = movementDir.GetOffset();
                newPos = tankState_i.Pos + movementOffset;
                TankAction[] tankActions = GetTankActionsToMoveToPoint(move.p, move.i, movementDir, newPos);
                if (tankActions.Length > 0)
                {
                    TankActionRecommendation tankActRec = new TankActionRecommendation
                    {
                        IsAMoveRecommended = true,
                        RecommendedTankAction = tankActions[0]
                    };
                    moveResult.SetTankActionRecommendation(move.p, move.i, tankActRec);
                }

                // TODO: For future improvement...
                // We want to move close to the enemy, but not so close that he can attack us.
                // - unless we are on the same line as him, as then the lock-down can commence.
                // So his attack distance to us must be kept above a certain number, based on how far from the centre line we are:

                // tank_j_attackDistance_to_i = base.GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
                //    move.pBar, move.j, newPos, move.dir1.GetOpposite(), TankHelper.EdgeOffsets);
            }

            int A_p_iBar = GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
                move.p, move.iBar, tankState_j.Pos, move.dir2, TankHelper.EdgeOffsets);
            int A_pBar_jBar
                = GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
                    move.pBar, move.jBar, newPos, move.dir3, TankHelper.EdgeOffsets);
            
            int slack = A_p_iBar - A_pBar_jBar;
            moveResult.Slack = slack;

            // p_iBar can now commence the attack (actually it's premature, but I'm running out of time!):
            TankAction[] tankActions_p_iBar = GetTankActionsFromTankToAttackTankAtPointAlongDirectionOfMovement(
                move.p, move.iBar, tankState_j.Pos, move.dir2, TankHelper.EdgeOffsets, keepMovingCloserOnFiringLastBullet:false);
            if (tankActions_p_iBar.Length > 0)
            {
                TankActionRecommendation tankActRec = new TankActionRecommendation
                {
                    IsAMoveRecommended = true,
                    RecommendedTankAction = tankActions_p_iBar[0],
                };
                moveResult.SetTankActionRecommendation(move.p, move.iBar, tankActRec);
            }

            // Tank pBar_j can commence attack on target p_i:
            TankAction[] tankActions_pBar_j = GetTankActionsFromTankToAttackTankAtPointAlongDirectionOfMovement(
                move.pBar, move.j, newPos, move.dir3, TankHelper.EdgeOffsets, keepMovingCloserOnFiringLastBullet: false);
            if (tankActions_pBar_j.Length > 0)
            {
                TankActionRecommendation tankActRec = new TankActionRecommendation
                {
                    IsAMoveRecommended = true,
                    RecommendedTankAction = tankActions_pBar_j[0],
                };
                moveResult.SetTankActionRecommendation(move.pBar, move.j, tankActRec);
            }

            if (slack < 0)
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Current;
            }
            else
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Close;
            }
            return moveResult;
        }

        public MoveResult EvaluateLeafNodeMoveWhenTanksAreNotCloseToLockDown(Move move, 
            MobileState tankState_i, MobileState tankState_j, Direction horizDir, Direction vertDir)
        {
            MoveResult moveResult = new MoveResult(move);

            // Find the Voronoi point on the straight line between the two tanks as an estimate of where the lock-down could take place:
            Point[] zigZagPoints = tankState_i.Pos.GetPointsOnZigZagLineToTargetPoint(tankState_j.Pos);
            int minDiff = Constants.UNREACHABLE_DISTANCE;
            Point minDiffPoint = new Point();

            foreach (Point pointOnLine in zigZagPoints)
            {
                int dist1 = GetDistanceFromTankToPoint(move.p, move.i, move.dir1, pointOnLine);
                int dist2 = GetDistanceFromTankToPoint(move.pBar, move.j, move.dir1.GetOpposite(), pointOnLine);
                int distDiff = Math.Abs(dist1 - dist2);
                if (distDiff < minDiff)
                {
                    minDiff = distDiff;
                    minDiffPoint = pointOnLine;
                }
            }

            if (minDiff == Constants.UNREACHABLE_DISTANCE)
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Invalid;
                return moveResult;
            }

            Rectangle boardBoundary = new Rectangle(0, 0, (short)(GameState.Walls.Width - 1), (short)(GameState.Walls.Height - 1));

            Point estimated_pos_pBar_j = minDiffPoint + move.dir1.GetOffset(4);  // 4 points away on one side
            estimated_pos_pBar_j = estimated_pos_pBar_j.BringIntoBounds(boardBoundary);

            Point estimated_pos_p_i = minDiffPoint + move.dir1.GetOpposite().GetOffset(4);  // 4 points away on the other side
            estimated_pos_p_i = estimated_pos_p_i.BringIntoBounds(boardBoundary);

            int slack_lock_down = minDiff;

            int A_p_iBar
                = GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
                    move.p, move.iBar, estimated_pos_pBar_j, move.dir2, TankHelper.EdgeOffsets);
            int A_pBar_jBar 
                = GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
                    move.pBar, move.jBar, estimated_pos_p_i, move.dir3, TankHelper.EdgeOffsets);
            int slack_Bar = A_p_iBar - A_pBar_jBar;
            int slack = Math.Max(slack_Bar, slack_lock_down);

            // slack_Bar is symmetrical. If it's above zero, then we are in trouble.
            // If below, then he's in trouble. So choose actions based on this slack:
            if (slack_Bar < 0)
            {
                // Move tank p_i closer to the estimated interception point with pBar_j:
                TankAction[] tankActions_move_p_i = GetTankActionsToMoveToPoint(move.p, move.i, move.dir1, estimated_pos_pBar_j);
                if (tankActions_move_p_i.Length > 0)
                {
                    moveResult.SetTankActionRecommendation(move.p, move.i, 
                        new TankActionRecommendation
                        { 
                            IsAMoveRecommended = true, 
                            RecommendedTankAction = tankActions_move_p_i[0] 
                        });
                }

                // Move tank pBar_j further away from attackers p_i and p_iBar:
                // (Also ideally towards the edge of the board, if that's not where the attack is coming from, to act as a decoy)
                int maxDistanceFromAttackers = int.MinValue;
                TankAction escapeAction = TankAction.NONE;

                foreach (Direction escapeDir in BoardHelper.AllRealDirections)
                {
                    Point adjacentPoint = tankState_j.Pos + escapeDir.GetOffset();
                    TurnCalculationCache turnCalcCache = Game.Current.Turns[GameState.Tick].CalculationCache;
                    TankLocation tankLoc = turnCalcCache.TankLocationMatrix[adjacentPoint];
                    if (tankLoc.IsValid)
                    {
                        int adj_moveDistance_pBar_j = GetDistanceFromTankToPoint(move.pBar, move.j, escapeDir, adjacentPoint);
                        if (adj_moveDistance_pBar_j < Constants.UNREACHABLE_DISTANCE)
                        {
                            int adj_attackDistance_p_i = GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
                                move.p, move.i, adjacentPoint, move.dir1, TankHelper.EdgeOffsets);
                            int adj_attackDistance_p_iBar = GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
                                move.p, move.iBar, adjacentPoint, move.dir2, TankHelper.EdgeOffsets);
                            int adj_attackDistance_SUM = adj_attackDistance_p_i + adj_attackDistance_p_iBar - adj_moveDistance_pBar_j;

                            if (adj_attackDistance_SUM < maxDistanceFromAttackers)
                            {
                                TankAction[] escapeActions = GetTankActionsToMoveToPoint(move.pBar, move.j, escapeDir, adjacentPoint);
                                if (escapeActions.Length > 0)
                                {
                                    maxDistanceFromAttackers = adj_attackDistance_SUM;
                                    escapeAction = escapeActions[0];
                                }
                            }
                        }
                    }
                }

                if (escapeAction != TankAction.NONE)
                {
                    TankActionRecommendation tankActRec = new TankActionRecommendation
                    {
                        IsAMoveRecommended = true,
                        RecommendedTankAction = escapeAction
                    };
                    moveResult.SetTankActionRecommendation(move.pBar, move.j, tankActRec);
                }
            }

            // Move tank p_iBar closer to lock down target pBar_j (don't attack yet, because lock-down hasn't occurred):
            TankAction[] tankActions_p_iBar = GetTankActionsToMoveToPoint(move.p, move.iBar, move.dir2, estimated_pos_pBar_j);
            if (tankActions_p_iBar.Length > 0)
            {
                TankActionRecommendation tankActRec = new TankActionRecommendation
                { 
                    IsAMoveRecommended = true,
                    RecommendedTankAction = tankActions_p_iBar[0],
                };
                moveResult.SetTankActionRecommendation(move.p, move.iBar, tankActRec);
            }
            
            // Move tank pBar_j closer to lock down target p_i:
            TankAction[] tankActions_pBar_j = GetTankActionsToMoveToPoint(move.pBar, move.j, move.dir3, estimated_pos_p_i);
            if (tankActions_pBar_j.Length > 0)
            {
                TankActionRecommendation tankActRec = new TankActionRecommendation
                { 
                    IsAMoveRecommended = true,
                    RecommendedTankAction = tankActions_pBar_j[0],
                };
                moveResult.SetTankActionRecommendation(move.pBar, move.j, tankActRec);
            }

            if (slack < 0)
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Close;
            }
            else
            {
                moveResult.EvaluationOutcome = ScenarioEvaluationOutcome.Possible;
            }
            return moveResult;
        }

        public bool IsValid(Move move)
        {
            // All tanks except t_pBar_j must be alive still:
            MobileState tankState_i = GetTankState_i(move);
            MobileState tankState_iBar = GetTankState_iBar(move);
            MobileState tankState_j = GetTankState_j(move);

            if (!(tankState_i.IsActive && tankState_iBar.IsActive && tankState_j.IsActive))
            {
                return false;
            }

            TankSituation tankSit_i = GetTankSituation(move.p, move.i);
            TankSituation tankSit_iBar = GetTankSituation(move.p, move.iBar);
            TankSituation tankSit_j = GetTankSituation(move.pBar, move.j);

            if (tankSit_iBar.IsLockedDown || tankSit_iBar.IsShutIntoQuadrant
                || tankSit_iBar.IsTankActionDetermined  // unless this is just due to being locked down already?
                || tankSit_i.IsTankActionDetermined  // unless this is just due to being locked down already?
                )
            {
                return false;
            }

            if (move.dir2 == move.dir1)
            {
                return false;
            }

            if (move.dir3 == move.dir1.GetOpposite())
            {
                return false;
            }

            // Check that directions are compatible with tank positions on the board:
            BoundaryProximity boundaryProximity_j = tankState_j.Pos.GetClosestBoundary(GameState.Walls.BoardBoundary);
            if (boundaryProximity_j.DistanceToBoundary <= Constants.SEGMENT_SIZE + Constants.TANK_OUTER_EDGE_OFFSET)
            {
                Direction invalidMovementDir = boundaryProximity_j.BoundaryDir.GetOpposite();
                if (move.dir1 == invalidMovementDir)
                {
                    return false;
                }
                if (move.dir2 == invalidMovementDir)
                {
                    return false;
                }
            }

            BoundaryProximity boundaryProximity_i = tankState_i.Pos.GetClosestBoundary(GameState.Walls.BoardBoundary);
            if (boundaryProximity_i.DistanceToBoundary <= Constants.SEGMENT_SIZE + Constants.TANK_OUTER_EDGE_OFFSET)
            {
                Direction invalidMovementDir = boundaryProximity_i.BoundaryDir.GetOpposite();
                if (move.dir3 == invalidMovementDir)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
