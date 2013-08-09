using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public enum Outcome : byte
    {
        NotStarted,
        InProgress,
        Draw,
        WinForYou,
        WinForOpponent,
        Crashed
    }
}
