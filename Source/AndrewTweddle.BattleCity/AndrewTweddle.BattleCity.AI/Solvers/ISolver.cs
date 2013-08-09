using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.AI.Solvers
{
    public interface ISolver
    {
        Coordinator Coordinator { get; set; }
        SolverState SolverState { get; set; }
        string Name { get; }

        void Solve();
        void Stop();
    }
}
