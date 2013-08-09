using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public static class Constants
    {
        /* ---------------------------------------------------------------------------
         * 
         * Constants to assist with initializing game states:
         * 
         */
        public const byte PLAYERS_PER_GAME = 2;
        public const byte TANKS_PER_PLAYER = 2;

        /* ---------------------------------------------------------------------------
         * 
         * Constants to assist with mapping specific elements to specific locations in
         * the game's array of elements, and the game state's array of element states:
         * 
         */

        // The index in the game & game state's array of elements gives the type of the element:
        public const byte ALL_ELEMENT_COUNT = 10;
        public const byte MOBILE_ELEMENT_COUNT = 8;
        public const byte TANK_COUNT = 4;
        public const byte MIN_BULLET_INDEX = 4;
        public const byte MAX_BULLET_INDEX = 8;

        // bit 1 holds the player index (you/opponent):
        public const byte PLAYER_MASK = 1;
        public const byte YOU_MASK_VALUE = 0;
        public const byte OPPONENT_MASK_VALUE = 1;

        // bit 2 holds the unit number (1 or 2):
        public const byte UNIT_INDEX_MASK = 2;
        public const byte UNIT1_MASK_VALUE = 0; // 0 << 1
        public const byte UNIT2_MASK_VALUE = 2; // 1 << 1

        // bits 3 and 4 hold the element type:
        public const byte ELEMENT_TYPE_MASK = 12;
        public const byte TANK_MASK_VALUE = 0;   // 0 << 2
        public const byte BULLET_MASK_VALUE = 4; // 1 << 2
        public const byte BASE_MASK_VALUE = 8;   // 2 << 2

        /* -------------------------------------------------------------------------------------
         * 
         * Constants to assist with determining the extent of a tank:
         * 
         */
        public const byte TANK_EXTENT_OFFSET = 2;
    }
}
