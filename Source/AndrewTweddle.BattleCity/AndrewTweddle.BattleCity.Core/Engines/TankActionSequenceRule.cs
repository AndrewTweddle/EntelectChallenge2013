using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Engines
{
    public enum TankActionSequenceRule
    {
        Simultaneous,
        InPlayerThenIdOrder,
        Player1FirstAndLastByIdOrder,
        InTankIdOrder
    }
}
