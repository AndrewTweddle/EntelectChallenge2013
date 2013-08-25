using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    [DataContract]
    public class Bullet: Element
    {
        [DataMember]
        public Tank Tank { get; private set; }

        public Bullet(Tank tank)
        {
            Tank = tank;
        }
    }
}
