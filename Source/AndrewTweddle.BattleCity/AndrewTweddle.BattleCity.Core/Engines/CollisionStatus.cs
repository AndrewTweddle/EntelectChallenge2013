using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Engines
{
    [Flags]
    public enum CollisionStatus
    {
        None = 0,
        WithTank = 1,
        WithBullet = 2,
        WithBase = 4,
        WithWall = 8,
        WithOutOfBoundsArea = 16,
        MovedIntoByATank = 32
    }
}
