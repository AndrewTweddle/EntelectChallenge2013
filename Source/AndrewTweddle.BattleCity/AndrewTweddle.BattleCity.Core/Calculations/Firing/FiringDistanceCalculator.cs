using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;

namespace AndrewTweddle.BattleCity.Core.Calculations.Firing
{
    public static class FiringDistanceCalculator
    {
        public static Line<FiringDistance> GetFiringDistancesToPoint(Cell target, Direction directionOfMovement,
            TurnCalculationCache turnCalcCache, GameStateCalculationCache gameStateCache)
        {
            Direction outwardDirection = directionOfMovement.GetOpposite();
            
            Line<Point> line = target.LineFromCellToEdgeOfBoardByDirection[(int) outwardDirection];
            Line<FiringDistance> firingLine = new Line<FiringDistance>(line.StartOfLine, line.DirectionOfLine, line.Length);

            // The following distances are from the POINT IN FRONT OF THE TANK WHERE THE BULLET IS FIRED:
            int indexOfNextShootableWallSegment = 0;
            int indexOfNextUnshootableWallSegment = 0;
            int firingCount = 1;

            int prevIndexOfNextShootableWallSegment = 0;
            int prevIndexOfNextUnshootableWallSegment = 0;
            int prevTicksTillTargetShot = 1;
            bool isValid = true;

            Point startingPoint = target.Position;

            for (int i = 0; i < firingLine.Length; i++)
            {
                Point tankFiringPoint = line[i];
                Point tankCentrePoint = tankFiringPoint + Constants.TANK_OUTER_EDGE_OFFSET * outwardDirection.GetOffset();
                FiringDistance dist = new FiringDistance
                {
                    TankFiringPoint = tankFiringPoint,
                    StartingTankPosition = tankCentrePoint
                };
                firingLine[i] = dist;

                TankLocation tankLoc;
                SegmentState segStateOnLeadingOutsideEdge = SegmentState.Clear;
                    // NB: value assigned just to stop the compiler complaining

                Cell tankCentreCell = turnCalcCache.CellMatrix[tankCentrePoint];
                isValid = tankCentreCell.IsValid;
                if (isValid)
                {
                    tankLoc = turnCalcCache.TankLocationMatrix[tankCentrePoint];
                    segStateOnLeadingOutsideEdge = gameStateCache.TankOuterEdgeMatrix[tankCentrePoint][(int)directionOfMovement];
                    isValid = isValid && tankLoc.IsValid;
                }

                if (isValid)
                {
                    if (i == 0)
                    {
                        // This is the very first point, right next to the target:
                        dist.IndexOfNextShootableWallSegment = 0;
                        dist.IndexOfNextUnshootableWallSegment = 0;
                        dist.TicksTillTargetShot = 1;
                        dist.CanMoveOrFire = segStateOnLeadingOutsideEdge == SegmentState.Clear;
                    }
                    else
                    {
                        // This is not the closest point to the target:
                        if (segStateOnLeadingOutsideEdge == SegmentState.OutOfBounds)
                        {
                            isValid = false;
                        }
                        else
                        {
                            indexOfNextShootableWallSegment
                                = (segStateOnLeadingOutsideEdge == SegmentState.ShootableWall)
                                ? i
                                : prevIndexOfNextShootableWallSegment;
                            indexOfNextUnshootableWallSegment 
                                = (segStateOnLeadingOutsideEdge == SegmentState.UnshootablePartialWall)
                                ? i
                                : prevIndexOfNextUnshootableWallSegment;
                            if (segStateOnLeadingOutsideEdge == SegmentState.ShootableWall)
                            {
                                firingCount++;
                            }

                            dist.IndexOfNextShootableWallSegment = indexOfNextShootableWallSegment;
                            dist.IndexOfNextUnshootableWallSegment = indexOfNextUnshootableWallSegment;

                            // Save these variables for the next tick, as they are about to be overwritten:
                            prevIndexOfNextShootableWallSegment = indexOfNextShootableWallSegment;
                            prevIndexOfNextUnshootableWallSegment = indexOfNextShootableWallSegment;

                            // Calculate the number of ticks required to shoot the target:
                            int indexOfTankFiringPoint = i;
                            bool canTankMoveStill = indexOfTankFiringPoint > indexOfNextUnshootableWallSegment;
                            int ticksTillTargetShot = 0;
                            int ticksTillLastShotFired = 0;
                            FiringActionSet[] firingActionSets = new FiringActionSet[firingCount];
                            dist.FiringActionsSets = firingActionSets;
                            int firingActionSetIndex = 0;
                            while (true)
                            {
                                bool isFinalShot = indexOfNextShootableWallSegment == 0;

                                // Calculate the number of ticks to destroy the target:
                                int distanceToNewShootableWall = indexOfTankFiringPoint - indexOfNextShootableWallSegment + 1;
                                int ticksToShootNextWall = 1 + (distanceToNewShootableWall >> 1);
                                    // Ticks = 1 tick to fire + Ceiling((distanceToNextShootableWallSegment - 1)/2.0)
                                    //       = 1 + floor( distanceToNextShootableWallSegment / 2.0 )
                                    //       = 1 + distanceToNextShootableWallSegment / 2
                                    //       = 1 + (distanceToNextShootableWallSegment >> 1)
                                ticksTillTargetShot += ticksToShootNextWall;
                                ticksTillLastShotFired += (isFinalShot ? 1 : ticksToShootNextWall);

                                int newIndexOfTankFiringPoint = indexOfTankFiringPoint;
                                if (canTankMoveStill)
                                {
                                    newIndexOfTankFiringPoint = indexOfTankFiringPoint - ticksToShootNextWall + 1;
                                    // stationary for 1 tick (to fire), then moving closer on remaining ticks
                                    if (newIndexOfTankFiringPoint < indexOfNextUnshootableWallSegment)
                                    {
                                        // The tank can't get through an unshootable wall segment:
                                        newIndexOfTankFiringPoint = indexOfNextUnshootableWallSegment;
                                        canTankMoveStill = false;
                                    }
                                }
                                FiringActionSet firingActionSet = new FiringActionSet(
                                    (byte) indexOfTankFiringPoint,
                                    (byte) ticksToShootNextWall,
                                    numberOfMovesMade: (byte)(indexOfTankFiringPoint - newIndexOfTankFiringPoint),
                                    canMoveOnceBeforeFiring: canTankMoveStill && (distanceToNewShootableWall % 2 == 0),
                                    isFinalShot: (indexOfNextShootableWallSegment == 0)
                                );
                                firingActionSets[firingActionSetIndex] = firingActionSet;
                                firingActionSetIndex++;

                                if (indexOfNextShootableWallSegment == 0)
                                {
                                    break;
                                }
                                indexOfTankFiringPoint = newIndexOfTankFiringPoint;
                                indexOfNextShootableWallSegment = firingLine[indexOfNextShootableWallSegment - 1].IndexOfNextShootableWallSegment;
                            }
                            dist.TicksTillTargetShot = ticksTillTargetShot;
                            dist.TicksTillLastShotFired = ticksTillLastShotFired;
                            dist.EndingTankPosition = line[indexOfTankFiringPoint + Constants.TANK_OUTER_EDGE_OFFSET];
                            if ( prevTicksTillTargetShot == ticksTillTargetShot + 1)
                            {
                                dist.CanMoveOrFire = true;
                            }
                        }
                    }
                }

                if (isValid)
                {
                    dist.IsValid = true;
                }
                else
                {
                    dist.TicksTillTargetShot = Constants.UNREACHABLE_DISTANCE;;
                }
            }

            return firingLine;
        }

        public static Node[] GetNodesOnFiringLine(Line<FiringDistance> firingLine, int firingLineIndex, 
            bool keepMovingCloserOnFiringLastBullet)
        {
            FiringDistance firingDistance = firingLine.Items[firingLineIndex];
            int nodeCount 
                = keepMovingCloserOnFiringLastBullet
                ? firingDistance.TicksTillTargetShot
                : firingDistance.TicksTillLastShotFired;
            Node[] nodes = new Node[nodeCount];
            int nodeIndex = 0;

            Direction movementDirection = firingLine.DirectionOfLine.GetOpposite();
                // Since the lines go outwards from the target, but movement is inward
            
            AddFiringLineNodesToRoute(firingDistance, movementDirection, nodes, ref nodeIndex, keepMovingCloserOnFiringLastBullet);
            return nodes;
        }

        public static void AddFiringLineNodesToRoute(FiringDistance firingDistance, 
            Direction movementDirection, Node[] nodes, ref int nodeIndex,
            bool keepMovingCloserOnFiringLastBullet)
        {
            Point currPos = firingDistance.StartingTankPosition;
            Point movementOffset = movementDirection.GetOffset();
            Node node;

            foreach (FiringActionSet actionSet in firingDistance.FiringActionsSets)
            {
                int movementsRequired = actionSet.TicksToShootNextWall - 1;

                // After firing a killer shot, the tank can move onto something. 
                // So give the option of not moving closer after that point.
                if (actionSet.IsFinalShot && !keepMovingCloserOnFiringLastBullet)
                {
                    node = new Node(ActionType.Firing, movementDirection, currPos);
                    nodes[nodeIndex] = node;
                    nodeIndex++;
                    return;
                }
                
                // Move then fire on first action of current firing set, unless it's the final shot.
                // On the final shot, we don't want to accidentally move into a bullet when we could shoot it.
                // Or move right next to a tank, allowing it to shoot us, when we could shoot first.
                if (actionSet.CanMoveOnceBeforeFiring && !actionSet.IsFinalShot)
                {
                    currPos = currPos + movementOffset;
                    node = new Node(ActionType.Moving, movementDirection, currPos);
                    nodes[nodeIndex] = node;
                    nodeIndex++;
                    movementsRequired--;
                }

                node = new Node(ActionType.Firing, movementDirection, currPos);
                nodes[nodeIndex] = node;
                nodeIndex++;

                while (movementsRequired > 0)
                {
                    currPos = currPos + movementOffset;
                    node = new Node(ActionType.Moving, movementDirection, currPos);
                    nodes[nodeIndex] = node;
                    nodeIndex++;
                    movementsRequired--;
                }
            }
        }
    }
}
