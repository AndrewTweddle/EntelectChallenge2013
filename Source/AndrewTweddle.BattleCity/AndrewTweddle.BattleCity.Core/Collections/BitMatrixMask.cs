using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Collections
{
    /// <summary>
    /// This represents one or more bits at a particular position in the BitMatrix.
    /// It is typically used to cache the particular bit for a point on the board, 
    /// or some or all of the bits for a segment on the board.
    /// </summary>
    public struct BitMatrixMask
    {
        public int ArrayIndex { get; private set; }
        public int BitMask { get; private set; }

        public BitMatrixMask(int arrayIndex, int bitMask): this()
        {
            ArrayIndex = arrayIndex;
            BitMask = bitMask;
        }
    }
}
