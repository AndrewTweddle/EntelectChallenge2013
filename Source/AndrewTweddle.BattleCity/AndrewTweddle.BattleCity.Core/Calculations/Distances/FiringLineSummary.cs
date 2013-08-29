using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public class FiringLineSummary
    {
        public Line<FiringDistance> FiringLine { get; set; }
        public int IndexOfNextFiringLinePoint { get; set; }

        /// <summary>
        /// This is the weighting in the graph of the next firing line edge.
        /// It is 1 less than the number of ticks for that firing distance calculation.
        /// This is because it has to compensate for the edge of weight 1 between
        /// the tank's normal movement node (position + correct direction) 
        /// at that point on the firing line
        /// </summary>
        public int NextEdgeWeighting { get; set; }
    }
}
