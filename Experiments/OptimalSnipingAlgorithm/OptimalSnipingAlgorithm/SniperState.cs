using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimalSnipingAlgorithm
{
    public class SniperState
    {
        public bool TargetIsHit { get; set; }
        public bool[] Walls { get; set; }  // if true, then a wall, otherwise empty. These are the initial spaces between the tank and the target.
        public int TurnNumber { get; set; }
        public int TankPosition { get; set; }
        public IList<string> StateByTurn { get; set; }

        public SniperState()
        {
            TurnNumber = 0;
            TankPosition = -1;
            StateByTurn = new List<string>();
            Walls = null;
        }

        public SniperState(bool[] walls): this()
        {
            Walls = walls;
        }

        public SniperState Clone()
        {
            bool[] newWalls = new bool[Walls.Length];
            Array.Copy(Walls, newWalls, Walls.Length);
            SniperState newState = new SniperState
            {
                TargetIsHit = this.TargetIsHit,
                TurnNumber = this.TurnNumber,
                TankPosition = this.TankPosition,
                Walls = newWalls,
                StateByTurn = new List<string>(this.StateByTurn)
            };
            return newState;
        }
    }
}
