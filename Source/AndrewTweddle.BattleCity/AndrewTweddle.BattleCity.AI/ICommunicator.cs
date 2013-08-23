﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Actions;

namespace AndrewTweddle.BattleCity.AI
{
    public interface ICommunicator
    {
        int LoginAndGetYourPlayerIndex();
        bool TryGetNewGameState();
        void WaitForNextTick();
        bool TrySetTankActions(TankActionSet actionSet, int timeoutInMilliseconds);
    }
}
