using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    [DataContract]
    public class Tank: Element
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public Point InitialCentrePosition { get; set; }

        [DataMember]
        public Direction InitialDirection { get; set; }

        [DataMember]
        public TankAction InitialAction { get; set; }

        [DataMember]
        public Bullet Bullet { get; set; }

        public Tank()
        {
            Bullet = new Bullet(this);
        }
    }
}
