using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Engines
{
    public class ActionCalculation
    {
        #region Input properties

        /// <summary>
        /// This calculated Id has:
        /// a) the MovingOrFiringDirection in its first 3 bits (bits 0 - 2)
        /// b) The CurrentDirectionOnSourceCell in its next 3 bits (bits 3 - 5)
        /// c) The SegmentStateInMovementDirection in the next 2 bits (bits 6 - 7)
        /// </summary>
        public byte Id { get; private set; }
        public TankAction TankActionOnSourceCell { get; private set; }
        public Direction CurrentDirectionOnSourceCell { get; private set; }
        public SegmentState SegmentStateInMovementDirection { get; private set; }

        #endregion

        #region Output properties

        #region Properties about moving in the direction indicated

        public bool IsAdjacentCellReachable { get; set; }
        public short TicksTakenToReachAdjacentCell { get; set; }
        public TankAction[] MovementActions { get; set; }

        #endregion

        #region Properties about getting set up to snipe (i.e. fire but not move) in the direction indicated

        public bool IsSnipingPossibleWithThisAction { get; set; }

        /// <summary>
        /// This is the number of extra actions required before sniping can take place from the source cell in the MovementOrFiringDirection
        /// </summary>
        public short TicksTakenToMoveIntoSnipingPosition { get; set; }

        /// <summary>
        /// This will have 0 or 1 actions, depending on whether sniping is possible, 
        /// and whether a change in direction is required first
        /// </summary>
        public TankAction[] SnipingActions { get; set; } 

        #endregion

        #endregion

        #region Constructors

        protected ActionCalculation()
        {
        }

        public ActionCalculation(byte id, TankAction movingOrFiringDirection,
            Direction currentDirectionOnSourceCell, SegmentState segmentStateInMovementDirection)
        {
            Id = id;
            TankActionOnSourceCell = movingOrFiringDirection;
            CurrentDirectionOnSourceCell = currentDirectionOnSourceCell;
            SegmentStateInMovementDirection = segmentStateInMovementDirection;
        }

        #endregion

    }
}
