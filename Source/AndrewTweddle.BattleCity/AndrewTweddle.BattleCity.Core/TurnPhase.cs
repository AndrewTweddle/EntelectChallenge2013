using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public enum TurnPhase: byte
    {
        None = 0,
        BoardClosesIn = 1,
        MoveBullets1 = 2,
        MoveBullets2 = 3,
        MoveTanks = 4,
        FireBullets = 5
    }
}
