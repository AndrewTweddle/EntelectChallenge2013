using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public enum PhaseType: byte
    {
        StartOfTurn = 0,
        Curtains = 0,
        MoveBullets1 = 1,
        ResolveBulletCollisions1 = 2,
        MoveBullets2 = 3,
        ResolveBulletCollisions2 = 4,
        MoveTanksAndResolveCollisions = 5,
        FireBullets = 6,
        ResolveFiredBulletCollisions = 7,
        EndOfTurn = 7
    }
}
