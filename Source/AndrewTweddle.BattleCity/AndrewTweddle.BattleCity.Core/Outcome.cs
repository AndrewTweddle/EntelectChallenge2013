using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    [Flags]
    public enum Outcome : byte
    {
        NotStarted         = 0,
        InProgress         = 1,
        Player1BaseKilled  = 2,
        Player2Win         = 2,
        Player2BaseKilled  = 4,
        Player1Win         = 4,
        Draw               = 6,
        Crashed            = 8,
        Timeout            = 16,
        TimeoutDraw        = 22,
        NoElementsLeft     = 32,
        NoElementsLeftDraw = 38,
        CompletedButUnknown = 64
    }
}
