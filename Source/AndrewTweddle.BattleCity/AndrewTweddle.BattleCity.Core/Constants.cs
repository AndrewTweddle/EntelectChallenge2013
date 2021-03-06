﻿using System;
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
        public const byte MAX_BULLET_INDEX = 7;

        // bit 1 holds the player index (you/opponent):
        public const byte PLAYER_MASK = 1;
        public const byte START_PLAYER_MASK_VALUE = 0;
        public const byte SECOND_PLAYER_MASK_VALUE = 1;

        // bit 2 holds the unit number (1 or 2):
        public const byte UNIT_INDEX_MASK = 2;
        public const byte UNIT_INDEX_BIT_SHIFT = 1;
        public const byte UNIT1_MASK_VALUE = 0 << UNIT_INDEX_BIT_SHIFT; // = 0
        public const byte UNIT2_MASK_VALUE = 1 << UNIT_INDEX_BIT_SHIFT; // = 2

        // bits 3 and 4 hold the element type:
        public const byte ELEMENT_TYPE_MASK = 12;
        public const byte ELEMENT_TYPE_BIT_SHIFT = 2;
        public const byte TANK_MASK_VALUE = 0 << ELEMENT_TYPE_BIT_SHIFT;   // = 0;   // 0 << 2
        public const byte BULLET_MASK_VALUE = 1 << ELEMENT_TYPE_BIT_SHIFT; // = 4; // 1 << 2
        public const byte BASE_MASK_VALUE = 2 << ELEMENT_TYPE_BIT_SHIFT;   // = 8;   // 2 << 2

        /* -------------------------------------------------------------------------------------
         * 
         * Constants to assist with determining the extent of a tank:
         * 
         */
        public const int EDGE_COUNT = 4;
        public const byte TANK_EXTENT_OFFSET = 2;
        public const byte TANK_OUTER_EDGE_OFFSET = 3;
        public const byte SEGMENT_SIZE = 5;

        /* -------------------------------------------------------------------------------------
         * 
         * Direction matrix constants:
         * 
         */
        public const short UNREACHABLE_DISTANCE = 10000;

        /* -------------------------------------------------------------------------------------
         * 
         * Enumeration constants:
         * 
         */
        public const int RELEVANT_DIRECTION_COUNT = 4;
        public const int ALL_DIRECTION_COUNT = 5;
        public const int AXIS_COUNT = 2;
        public const int EDGE_OFFSET_COUNT = 5;
        public const int ROTATION_TYPE_COUNT = 4;
        public const int TANK_ACTION_COUNT = 6;
        public const int BULLET_MOVEMENTS_PER_TICK = 2;
    }
}
