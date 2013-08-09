using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.States
{
    public struct MobileState
    {
        public Point Pos { get; private set; }
        public Direction Dir { get; private set; }
        public bool IsActive { get; private set; }

        public MobileState(Point pos, Direction dir, bool isActive): this()
        {
            this.Pos = pos;
            this.Dir = dir;
            this.IsActive = isActive;
        }

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
