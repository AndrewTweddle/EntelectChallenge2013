using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.Core.States
{
    public class CellState
    {
        public Point Position { get; set; }
        public GameState GameState { get; set; }
        public CellDirectionalState[] DirectionalStates { get; set; }

        protected CellState()
        {
        }

        public CellState(GameState gameState, Point position)
        {
            DirectionalStates = new CellDirectionalState[Constants.RELEVANT_DIRECTION_COUNT];
            foreach (Direction dir in BoardHelper.AllRealDirections)
            {
                CellDirectionalState directionalState = new CellDirectionalState(this, dir);
                DirectionalStates[(int)dir] = directionalState;
            }
        }

    }
}
