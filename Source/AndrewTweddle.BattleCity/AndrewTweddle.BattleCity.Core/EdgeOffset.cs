using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public enum EdgeOffset: byte
    {
        Centre = 0,
        OffCentreAntiClockwise = 1,
        OffCentreClockwise = 2,
        CornerAntiClockwise = 3,
        CornerClockwise = 4
    }
}
