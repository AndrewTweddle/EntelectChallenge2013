using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public interface IGameEngine
    {
        Game GetStatus();
        void SetAction(int unitId, Action action);  // assume arg0 = Id
        IList<State> Login();  // Looks wrong - we're given the state, but how do we know the positions of each block, or the size of the board?
    }
}
