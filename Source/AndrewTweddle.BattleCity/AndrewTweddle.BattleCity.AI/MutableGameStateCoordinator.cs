using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.AI.Solvers;

namespace AndrewTweddle.BattleCity.AI
{
    public class MutableGameStateCoordinator: Coordinator<MutableGameState>
    {
        public MutableGameStateCoordinator(ISolver<MutableGameState> solver,
            ICommunicator communicator, ICommunicatorCallback communicatorCallback)
            : base(solver, communicator, communicatorCallback)
        {
        }
    }
}
