using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    /// <summary>
    /// This enumeration generally represents the axis of movement, 
    /// indicating whether a tank or bullet is moving in a vertical or horizontal direction
    /// </summary>
    public enum Axis: byte
    {
        Horizontal = 0,
        Vertical = 1,
        None = 2,
        X = Horizontal,
        Y = Vertical
    }
}
