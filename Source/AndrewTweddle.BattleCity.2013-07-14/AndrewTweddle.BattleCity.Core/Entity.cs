using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public class Entity
    {
        int Id { get; set; }
        int X { get; set; }
        int Y { get; set; }
        public Direction Direction { get; set; }
    }
}
