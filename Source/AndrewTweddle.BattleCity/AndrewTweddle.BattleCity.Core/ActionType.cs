using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public enum ActionType: byte
    {
        Moving = 0,
        Firing = 1,
        FiringLine = 2  // Firing line distances have a node which connects directly to the target
    }
}
