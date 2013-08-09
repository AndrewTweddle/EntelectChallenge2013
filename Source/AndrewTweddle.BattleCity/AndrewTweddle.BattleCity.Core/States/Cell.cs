using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.States
{
    public struct Cell
    {
        public Point Pos { get; private set; }
        public CellState State { get; private set; }

        public Cell(Point pos, CellState state):this()
        {
            Pos = pos;
            State = state;
        }
    }
}
