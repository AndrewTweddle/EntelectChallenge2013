using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.Solvers
{
    public interface ISolver<TGameState>
        where TGameState: GameState<TGameState>, new()
    {
        Coordinator<TGameState> Coordinator { get; set; }
        SolverState SolverState { get; set; }
        string Name { get; }
        int YourPlayerIndex { get; set; }
        Game GameToReplay { get; set; }
        int TickToReplayTo { get; set; }

        void Start();
        void StartChoosingMoves();
        void StopChoosingMoves();
        void Stop();
    }
}
