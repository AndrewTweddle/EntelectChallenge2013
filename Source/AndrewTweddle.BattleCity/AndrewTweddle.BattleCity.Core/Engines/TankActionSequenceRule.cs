using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Engines
{
    public enum TankActionSequenceRule
    {
        Unknown,
        Simultaneous,
        InPlayerThenIdOrder,
        Player1FirstAndLastByIdOrder
    }
}
