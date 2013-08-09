using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    // TODO: Use conditional defines to be able to choose struct or class:
    public class MobileState
    {
        public Position Pos;
        public Direction Dir;
        public bool IsActive;

        public MobileState Clone()
        {
            MobileState clonedState = new MobileState
            {
                Pos = this.Pos,
                Dir = this.Dir,
                IsActive = this.IsActive
            };
            return clonedState;
        }
    }
}
