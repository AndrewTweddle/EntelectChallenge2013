using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.Scenarios
{
    public class Move
    {
        public Move ParentMove { get; set; }

        /// <summary>
        /// The index of the player
        /// </summary>
        public int p { get; set; }

        /// <summary>
        /// The index of the other player
        /// </summary>
        public int pBar { get; set; }

        /// <summary>
        /// The index of player p's tank with the primary role
        /// </summary>
        public int i { get; set; }

        /// <summary>
        /// The index of player p's other tank
        /// </summary>
        public int iBar { get; set; }

        /// <summary>
        /// The index of player pBar's tank with the primary role
        /// </summary>
        public int j { get; set; }

        /// <summary>
        /// The index of player pBar's other tank
        /// </summary>
        public int jBar { get; set; }

        public Direction dir1 { get; set; }

        public Move()
        {
            p = 0;
            pBar = 1;
            i = 0;
            iBar = 1;
            j = 0;
            jBar = 1;
            dir1 = Direction.NONE;
        }

        public Move(Move parentMove): this()
        {
            ParentMove = parentMove;
        }

        public Move Clone()
        {
            Move move = new Move(ParentMove)
            {
                p = this.p,
                pBar = this.pBar,
                i = this.i,
                iBar = this.iBar,
                j = this.j,
                jBar = this.jBar,
                dir1 = this.dir1
            };
            return move;
        }

        public Move CloneAsChild()
        {
            Move childMove = Clone();
            childMove.ParentMove = this;
            return childMove;
        }
    }
}
