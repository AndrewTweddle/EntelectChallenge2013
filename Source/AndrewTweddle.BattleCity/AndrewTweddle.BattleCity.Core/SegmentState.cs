using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public enum SegmentState: byte
    {
        OutOfBounds = 0,  // One or more cells are out of bounds
        UnshootablePartialWall = 1,  // The centre of the segment is empty, but some of the other cells contain a wall
        Clear = 2,
        ShootableWall
    }
}
