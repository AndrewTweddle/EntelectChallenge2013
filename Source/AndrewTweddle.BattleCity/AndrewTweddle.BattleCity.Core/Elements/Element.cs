using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    [DataContract, KnownType(typeof(Tank)), KnownType(typeof(Bullet)), KnownType(typeof(Base))]
    public class Element
    {
        [DataMember]
        public int Index { get; set; }

        /// <summary>
        /// This is the zero-based index of the owning player in the Game's array of players
        /// </summary>
        public int PlayerNumber
        {
            get
            {
                return Index & Constants.PLAYER_MASK;
            }
        }

        public Player Player
        {
            get
            {
                return Game.Current.Players[PlayerNumber];
            }
        }

        /// <summary>
        /// This is the zero-based index into the player's array of elements of this type (or zero for the base)
        /// </summary>
        public int Number
        {
            get
            {
                return (Index & Constants.UNIT_INDEX_MASK) >> Constants.UNIT_INDEX_BIT_SHIFT;
            }
        }

        public ElementType ElementType
        {
            get
            {
                return (ElementType) ((Index & Constants.ELEMENT_TYPE_MASK) >> Constants.ELEMENT_TYPE_BIT_SHIFT);
            }
        }
    }
}
