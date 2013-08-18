using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Engines
{
    /// <summary>
    /// This game state engine is not immutable itself.
    /// Instead it uses the ImmutableGameState class, 
    /// which uses immutable TankState and BulletState classes.
    /// </summary>
    public static class ImmutableGameStateEngine
    {
        public static void ApplyActions(ImmutableGameState gameState, TankAction[] tankActions)
        {
            throw new NotImplementedException();
        }
    }
}
