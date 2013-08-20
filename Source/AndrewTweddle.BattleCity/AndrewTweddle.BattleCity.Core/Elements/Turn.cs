using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    public class Turn
    {
        public DateTime NextServerTickTime { get; set; }
        public DateTime EarliestLocalNextTickTime { get; set; }
        public DateTime LatestLocalNextTickTime { get; set; }

        public int Tick { get; set; }
        public DateTime ServerStartTime { get; set; }
        public DateTime EstimatedLocalStartTime { get; set; }
        public int[] BulletIds { get; private set; }
        public GameState GameState { get; set; }

        // An out-of-bounds area will encroach on the board from the sides after the game end phase is reached.
        // The following properties store the predicted min and max valid x values.
        // So values of x < LeftBoundary or x > RightBoundary will automatically be out of bounds:
        public short LeftBoundary { get; set; }
        public short RightBoundary { get; set; }

        public Turn()
        {
            BulletIds = new int[Constants.TANK_COUNT];
        }
    }
}
