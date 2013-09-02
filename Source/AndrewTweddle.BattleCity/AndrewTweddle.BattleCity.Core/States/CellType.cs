using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.States
{
    public enum CellState: byte
    {
        Empty,
        Wall,
        OutOfBounds
    }
}
