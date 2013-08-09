using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    public class Bullet: Element
    {
        public int Index { get; private set; }
        public Tank Tank { get; private set; }

        public Bullet(Tank tank)
        {
            Tank = tank;
        }
    }
}
