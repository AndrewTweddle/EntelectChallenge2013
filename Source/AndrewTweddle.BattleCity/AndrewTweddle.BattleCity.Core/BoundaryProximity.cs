using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public struct BoundaryProximity
    {
        public Direction BoundaryDir { get; set; }
        public short DistanceToBoundary { get; set; }

        public BoundaryProximity(Direction boundaryDir, short distanceToBoundary)
            : this()
        {
            BoundaryDir = boundaryDir;
            DistanceToBoundary = distanceToBoundary;
        }
    }
}
