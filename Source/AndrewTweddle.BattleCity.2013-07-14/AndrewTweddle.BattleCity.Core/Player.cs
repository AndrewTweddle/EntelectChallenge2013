using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public class Player
    {
        public Base Base { get; set; }
        public string Name { get; set; }
        public List<Unit> Units { get; set; }
        public List<Bullet> Bullets { get; set; }
    }
}
