using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public enum AdjacencyType
    {
        /// <summary>
        /// Each adjacent node occurs earlier in time, so the arcs are directed towards the target
        /// </summary>
        IncomingToTarget,

        /// <summary>
        /// Each adjacent node occurs later in time, so the arcs are directed away from the source
        /// </summary>
        OutgoingFromSource
    }
}
