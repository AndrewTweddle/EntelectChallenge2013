using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Helpers
{
    public static class BoardHelper
    {
        public static Axis GetPerpendicular(this Axis axis)
        {
            return (Axis)(1 - axis);
        }
    }
}
