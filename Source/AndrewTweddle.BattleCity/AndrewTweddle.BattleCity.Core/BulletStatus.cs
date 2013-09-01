using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public enum BulletStatus: byte
    {
        Unknown = 0,
        Loaded = 1,
        Fired = 2,
        /// <summary>
        /// i.e. it is not in play any longer and the tank that fired it is no longer alive
        /// </summary>
        Dead = 3
    }
}
