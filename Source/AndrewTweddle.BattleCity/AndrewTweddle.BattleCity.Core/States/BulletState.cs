using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations;

namespace AndrewTweddle.BattleCity.Core.States
{
    public class BulletState: MobileElementState
    {
        public Direction Dir { get; set; }
        public Cell CentreCell { get; set; }

        public Cell NextCentreCell 
        {
            get
            {
                return CentreCell.AdjacentCellsByDirection[(byte) Dir];
            }
        }

        public override MobileState GetMobileState()
        {
            return new MobileState(pos: CentreCell.Position, dir: Dir, isActive: true);
        }
    }
}
