using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.Engines
{
    /// <summary>
    /// This class is responsible for updating the game state as a result of tank actions
    /// </summary>
    public class GameStateEngine
    {
        public const int ACTION_CALCULATION_LOOKUP_COUNT = 1 << 8;
        // 8 bits are required: 3 bits for 5 movement directions, 3 bits for 5 current directions, 2 bits for 4 segment states

        public static ActionCalculation[] ActionCalculationLookups { get; private set; }

        static GameStateEngine()
        {
            CalculateActionLookups();
        }

        private static void CalculateActionLookups()
        {
            ActionCalculationLookups = new ActionCalculation[ACTION_CALCULATION_LOOKUP_COUNT];

            int actionCalculationId = 0;

            foreach (TankAction action in Enum.GetValues(typeof(TankAction)))
            {
                actionCalculationId = (byte) action;
                foreach (Direction currentDir in Enum.GetValues(typeof(Direction)))
                {
                    actionCalculationId |= ((int)currentDir) << 3;
                    foreach (SegmentState segState in Enum.GetValues(typeof(SegmentState)))
                    {
                        actionCalculationId |= ((int)segState) << 6;

                        ActionCalculation calculation = new ActionCalculation(
                            (byte) actionCalculationId, action, currentDir, segState);
                        PerformMovementCalculationsToAdjacentCell(calculation);
                        PerformSnipingCalculationsToAdjacentCell(calculation);

                        ActionCalculationLookups[actionCalculationId] = calculation;
                    }
                }
            }
        }

        private static void PerformSnipingCalculationsToAdjacentCell(ActionCalculation calculation)
        {
            // TODO: Switch back to being based on desired movement direction, not an action
            switch (calculation.TankActionOnSourceCell)
            {
                case TankAction.DOWN:
                case TankAction.LEFT:
                case TankAction.RIGHT:
                case TankAction.UP:
                    switch (calculation.SegmentStateInMovementDirection)
                    {
                        case SegmentState.Clear:
                            calculation.IsSnipingPossibleWithThisAction = true;
                            calculation.TicksTakenToMoveIntoSnipingPosition = 0;
                            calculation.SnipingActions = new TankAction[] { };
                            break;
                        case SegmentState.OutOfBounds:
                            return Constants.UNREACHABLE_DISTANCE;
                        default:
                            if (currentDir == shootingDir)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1;  // Tank must change direction by moving in that direction first
                            }
                    }
                    break;

                default:
                    // case TankAction.NONE:
                    // case TankAction.FIRE:
                    calculation.IsSnipingPossibleWithThisAction = false;  // i.e. not with this action
                    calculation.TicksTakenToMoveIntoSnipingPosition = Constants.UNREACHABLE_DISTANCE;
                    calculation.SnipingActions = new TankAction[] { };
                    break;
            }
        }

        public static void PerformMovementCalculationsToAdjacentCell(ActionCalculation calculation)
        {
            switch (calculation.TankActionOnSourceCell)
            {
                case TankAction.DOWN:
                case TankAction.LEFT:
                case TankAction.RIGHT:
                case TankAction.UP:
                    switch (calculation.SegmentStateInMovementDirection)
                    {
                        case SegmentState.Clear:
                            // Move directly into the space
                            calculation.IsAdjacentCellReachable = true;
                            calculation.TicksTakenToReachAdjacentCell = 1;
                            calculation.MovementActions = new TankAction[] { calculation.TankActionOnSourceCell };
                            break;
                        case SegmentState.ShootableWall:
                            calculation.IsAdjacentCellReachable = true;
                            if (calculation.CurrentDirectionOnSourceCell == calculation.TankActionOnSourceCell.ToDirection())
                            {
                                // Shoot the wall, then move into the adjacent cell
                                calculation.TicksTakenToReachAdjacentCell = 2;
                                calculation.MovementActions = new TankAction[]
                                {
                                    TankAction.FIRE,
                                    calculation.TankActionOnSourceCell
                                };
                            }
                            else
                            {
                                // Move in direction to turn the tank, then shoot the wall, then move into the adjacent cell
                                calculation.TicksTakenToReachAdjacentCell = 3;
                                calculation.MovementActions = new TankAction[]
                                {
                                    calculation.TankActionOnSourceCell,
                                    TankAction.FIRE,
                                    calculation.TankActionOnSourceCell
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
                    calculation.MovementActions = new TankAction[] { calculation.TankActionOnSourceCell };
                        // i.e. Just the current action (even though it won't actually reach adjacent cell)

                    break;
            }
        }

        public static short GetSnipingDistanceAdjustment(
            Direction currentDir, Direction shootingDir, SegmentState outsideLeadingEdgeSegmentState)
        {
            switch (outsideLeadingEdgeSegmentState)
            {
                case SegmentState.Clear:
                    return 0;
                case SegmentState.OutOfBounds:
                    return Constants.UNREACHABLE_DISTANCE;
                default:
                    if (currentDir == shootingDir)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;  // Tank must change direction by moving in that direction first
                    }
            }
        }

    }
}
