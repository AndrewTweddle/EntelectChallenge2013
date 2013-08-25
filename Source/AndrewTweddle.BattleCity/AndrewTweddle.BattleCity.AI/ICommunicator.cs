using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Actions;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI
{
    public interface ICommunicator
    {
        int LoginAndGetYourPlayerIndex(ICommunicatorCallback callback);
        bool TryGetNewGameState(int playerIndex, ICommunicatorCallback callback);
        void WaitForNextTick(int playerIndex, ICommunicatorCallback callback);
        bool TrySetAction(int playerIndex, int tankId, TankAction tankAction, ICommunicatorCallback callback, int timeoutInMilliseconds);
        bool TrySetActions(int playerIndex, TankAction tankAction1, TankAction tankAction2, ICommunicatorCallback callback, int timeoutInMilliseconds);
    }
}
