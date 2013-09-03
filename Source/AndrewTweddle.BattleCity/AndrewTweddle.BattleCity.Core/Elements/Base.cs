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

        public Direction[] GetPossibleIncomingAttackDirections()
        {
            if (Pos.Y < Game.Current.BoardHeight / 2)
            {
                return new Direction[] { Direction.UP, Direction.LEFT, Direction.RIGHT };
            }
            else
            {
                return new Direction[] { Direction.DOWN, Direction.LEFT, Direction.RIGHT };
            }
        }
    }
}
