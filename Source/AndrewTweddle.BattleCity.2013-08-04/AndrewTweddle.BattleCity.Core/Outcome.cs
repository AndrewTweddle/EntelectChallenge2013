using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public enum Outcome: byte
    {
        NotStarted = 0,
        InProgress = 1,
        Draw = 2,
        WinForYou = 4,
        WinForOpponent = 8,
        Crashed = 16
    }
}
