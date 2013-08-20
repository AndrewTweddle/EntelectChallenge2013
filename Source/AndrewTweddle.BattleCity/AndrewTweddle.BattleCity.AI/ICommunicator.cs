using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Actions;

namespace AndrewTweddle.BattleCity.AI
{
    public interface ICommunicator
    {
        void Login();
        bool TryGetNewGameState(GameState gameStateToUpdate);
        void WaitForNextTick(GameState gameStateToUpdate);
        bool TrySetTankActions(GameState currentGameState, TankActionSet actionSet, int timeoutInMilliseconds);
    }
}
