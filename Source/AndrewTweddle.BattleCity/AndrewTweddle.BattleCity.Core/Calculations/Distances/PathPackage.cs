using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public class PathPackage
    {
        public Point StartPoint { get; set; }
        public Point EndingTankPosition { get; set; }
        public Point Target { get; set; }
        public int Distance { get; set; }
        public Node[] NodesOnShortestPath { get; set; }

        /// <summary>
        /// The reverse path is useful for racing back to defend the base along your own path 
        /// or for chasing down an enemy tank on their path by connecting to their path. 
        /// The time saved depends on the number of firing actions performed.
        /// </summary>
        public Node[] NodesOnReversePath { get; set; }
        public Direction MovementDirectionOnFinalAttack { get; set; }
        public EdgeOffset EdgeOffsetOfFinalAttack { get; set; }
    }
}
