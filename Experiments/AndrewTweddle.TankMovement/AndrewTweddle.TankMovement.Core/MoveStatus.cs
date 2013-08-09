using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.TankMovement.Core
{
    public enum MoveStatus: byte
    {
        Unresolved,
        TankDoesNotExist,
        NotMoving,
        Cancelled,
        Approved,
        Destroyed  // If the tank move is approved, and the tank then moves into a bullet
    }
}
