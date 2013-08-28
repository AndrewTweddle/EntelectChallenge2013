using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations.Firing
{
    public class FiringDistance
    {
        public Point StartingTankPosition { get; set; }
        public Point EndingTankPosition { get; set; }
        public Point TankFiringPoint { get; set; }

        public int IndexOfNextShootableWallSegment { get; set; }
        public int IndexOfNextUnshootableWallSegment { get; set; }

        public bool IsValid { get; set; }
        public bool CanMoveOrFire { get; set; }

        public int TicksTillTargetShot { get; set; }

        public FiringActionSet[] FiringActionsSets { get; set; }
    }
}
