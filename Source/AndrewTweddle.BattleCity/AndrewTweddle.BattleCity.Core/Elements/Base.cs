using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    [DataContract]
    public class Base: Element
    {
        [DataMember]
        public Point Pos { get; set; }
    }
}
