using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.AI.Solvers
{
    public enum SolverState
    {
        NotRunning = 0,
        ChoosingMoves = 1,
        StoppingChoosingMoves = 2,
        Thinking = 3,
        WaitingToChooseMoves = 4,
        CanChooseMoves = 5,
        Stopping = 6,
        Stopped = 7
    }
}
