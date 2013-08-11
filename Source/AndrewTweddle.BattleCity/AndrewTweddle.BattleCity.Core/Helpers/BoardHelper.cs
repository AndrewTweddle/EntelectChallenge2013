using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.Helpers
{
    public static class BoardHelper
    {
        public static Direction[] AllDirections;
        public static Direction[] AllRealDirections;
        public static Axis[] AllRealAxes;

        static BoardHelper()
        {
            AllDirections = Enumerable.Range(0, Constants.ALL_DIRECTION_COUNT).Select(i => (Direction) i).ToArray();
            AllRealDirections = Enumerable.Range(0, Constants.RELEVANT_DIRECTION_COUNT).Select(i => (Direction) i).ToArray();
            AllRealAxes = new[] { (Axis)0, (Axis)1 };
        }

        
    }
}
