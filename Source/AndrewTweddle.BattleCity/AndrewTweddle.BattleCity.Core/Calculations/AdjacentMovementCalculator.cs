using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public static class AdjacentMovementCalculator
    {
        public const int ADJACENT_MOVEMENT_CALCULATION_LOOKUP_COUNT = 1 << 8;
        // 8 bits are required: 3 bits for 5 movement directions, 3 bits for 5 current directions, 2 bits for 4 segment states

        public static AdjacentMovementCalculation[] AdjacentMovementCalculations { get; private set; }

        static AdjacentMovementCalculator()
        {
            CalculateActionLookups();
        }

        private static void CalculateActionLookups()
        {
            AdjacentMovementCalculations = new AdjacentMovementCalculation[ADJACENT_MOVEMENT_CALCULATION_LOOKUP_COUNT];

            int actionCalculationId = 0;
            Direction[] directions = (Direction[]) Enum.GetValues(typeof(Direction));

            foreach (Direction movingOrFiringDirection in directions)
            {
                actionCalculationId = (byte) movingOrFiringDirection;
                foreach (Direction currentDir in directions)
                {
                    actionCalculationId |= ((int)currentDir) << 3;
                    foreach (SegmentState segState in Enum.GetValues(typeof(SegmentState)))
                    {
                        actionCalculationId |= ((int)segState) << 6;

                        AdjacentMovementCalculation calculation = new AdjacentMovementCalculation(
                            (byte) actionCalculationId, movingOrFiringDirection, currentDir, segState);
                        PerformMovementCalculationsToAdjacentCell(calculation);
                        PerformSnipingCalculationsToAdjacentCell(calculation);

                        AdjacentMovementCalculations[actionCalculationId] = calculation;
                    }
                }
            }
        }

        public static void PerformMovementCalculationsToAdjacentCell(AdjacentMovementCalculation calculation)
        {
            switch (calculation.MovingOrFiringDirection)
            {
                case Direction.DOWN:
                case Direction.LEFT:
                case Direction.RIGHT:
                case Direction.UP:
                    switch (calculation.SegmentStateInMovementDirection)
                    {
                        case SegmentState.Clear:
                            // Move directly into the adjacent cell:
                            calculation.IsAdjacentCellReachable = true;
                            calculation.TicksTakenToReachAdjacentCell = 1;
                            calculation.MovementActions = new TankAction[]
                            {
                                calculation.MovingOrFiringDirection.ToTankAction()
                            };
                            break;
                        case SegmentState.ShootableWall:
                            calculation.IsAdjacentCellReachable = true;
                            if (calculation.CurrentDirectionOnSourceCell == calculation.MovingOrFiringDirection)
                            {
                                // Shoot the wall, then move into the adjacent cell:
                                calculation.TicksTakenToReachAdjacentCell = 2;
                                calculation.MovementActions = new TankAction[]
                                {
                                    TankAction.FIRE,
                                    calculation.MovingOrFiringDirection.ToTankAction()
                                };
                            }
                            else
                            {
                                // Move in direction to turn the tank, then shoot the wall, then move into the adjacent cell
                                calculation.TicksTakenToReachAdjacentCell = 3;
                                calculation.MovementActions = new TankAction[]
                                {
                                    calculation.MovingOrFiringDirection.ToTankAction(),
                                    TankAction.FIRE,
                                    calculation.MovingOrFiringDirection.ToTankAction()
                                };
                            }
                            break;
                        default:
                            // case SegmentState.OutOfBounds:
                            // case SegmentState.UnshootablePartialWall:
                            calculation.IsAdjacentCellReachable = false;
                            calculation.TicksTakenToReachAdjacentCell = Constants.UNREACHABLE_DISTANCE;
                            calculation.MovementActions = new TankAction[] { };
                            break;
                    }
                    break;

                default:
                    // case TankAction.NONE:
                    // case TankAction.FIRE:
                    calculation.IsAdjacentCellReachable = false;  // i.e. not with this action
                    calculation.TicksTakenToReachAdjacentCell = Constants.UNREACHABLE_DISTANCE;
                    calculation.MovementActions = new TankAction[] { calculation.MovingOrFiringDirection };
                        // i.e. Just the current action (even though it won't actually reach adjacent cell)

                    break;
            }
        }

        private static void PerformSnipingCalculationsToAdjacentCell(AdjacentMovementCalculation calculation)
        {
            switch (calculation.MovingOrFiringDirection)
            {
                case Direction.DOWN:
                case Direction.LEFT:
                case Direction.RIGHT:
                case Direction.UP:
                    switch (calculation.SegmentStateInMovementDirection)
                    {
                        case SegmentState.Clear:
                            if (calculation.CurrentDirectionOnSourceCell == calculation.MovingOrFiringDirection)
                            {
                                // Already in position for sniping from the current cell:
                                calculation.IsSnipingPossibleWithThisAction = true;
                                calculation.TicksTakenToMoveIntoSnipingPosition = 0;
                                calculation.SnipingActions = new TankAction[] { };
                            }
                            else
                            {
                                /* You can't turn to face in the right direction without moving off this cell:
                                 * 
                                 * Note: In theory one could move off and back to the cell.
                                 *       But this is an edge case, so only cater for it if a need arises.
                                 */
                                calculation.IsSnipingPossibleWithThisAction = false;
                                calculation.TicksTakenToMoveIntoSnipingPosition = Constants.UNREACHABLE_DISTANCE;
                                calculation.SnipingActions = new TankAction[] { };
                            }
                            break;
                        case SegmentState.ShootableWall:
                        case SegmentState.UnshootablePartialWall:
                            // The wall can be shot into or through, and it allows a change of direction:
                            calculation.IsSnipingPossibleWithThisAction = true;
                            if (calculation.CurrentDirectionOnSourceCell == calculation.MovingOrFiringDirection)
                            {
                                calculation.TicksTakenToMoveIntoSnipingPosition = 0;
                                calculation.SnipingActions = new TankAction[] { };
                            }
                            else
                            {
                                calculation.TicksTakenToMoveIntoSnipingPosition = 1;
                                calculation.SnipingActions = new TankAction[] 
                                { 
                                    calculation.MovingOrFiringDirection.ToTankAction()
                                };
                            }
                            break;
                        default:
                            // case SegmentState.OutOfBounds:
                            calculation.IsSnipingPossibleWithThisAction = false;
                            calculation.TicksTakenToMoveIntoSnipingPosition = Constants.UNREACHABLE_DISTANCE;
                            calculation.SnipingActions = new TankAction[] { };
                            break;
                    }
                    break;

                default:
                    // case Direction.NONE:
                    calculation.IsSnipingPossibleWithThisAction = false;  // i.e. not if you won't say what direction you want to snipe in!
                    calculation.TicksTakenToMoveIntoSnipingPosition = Constants.UNREACHABLE_DISTANCE;
                    calculation.SnipingActions = new TankAction[] { };
                    break;
            }
        }
    }
}
