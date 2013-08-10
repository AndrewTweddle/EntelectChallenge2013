using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    /* Numbers chosen to map to Direction values for easy conversion: */
    public enum TankAction: byte
    {
        UP    = 0,
        DOWN  = 1,
        LEFT  = 2,
        RIGHT = 3,
        NONE  = 4,
        FIRE  = 5
    }
}
