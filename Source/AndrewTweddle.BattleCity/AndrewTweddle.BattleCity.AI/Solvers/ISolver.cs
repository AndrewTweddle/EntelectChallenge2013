using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.AI.Solvers
{
    public interface ISolver<TGameState>
        where TGameState: GameState<TGameState>, new()
    {
        Coordinator<TGameState> Coordinator { get; set; }
        SolverState SolverState { get; set; }
        string Name { get; }

        void Solve();
        void Stop();
    }
}
