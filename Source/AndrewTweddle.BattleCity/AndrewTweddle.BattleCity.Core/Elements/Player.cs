using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    public class Player
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public Tank[] Tanks { get; set; }
        public Base Base { get; private set; }

        public Player()
        {
            Base = new Base();
            Tanks = new Tank[Constants.TANKS_PER_PLAYER];
            for (int t = 0; t < Tanks.Length - 1; t++)
            {
                Tanks[t] = new Tank();
            }
        }
    }
}
