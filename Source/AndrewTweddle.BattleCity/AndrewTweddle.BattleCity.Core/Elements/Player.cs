using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    [DataContract]
    public class Player
    {
        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Tank[] Tanks { get; set; }

        [DataMember]
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
