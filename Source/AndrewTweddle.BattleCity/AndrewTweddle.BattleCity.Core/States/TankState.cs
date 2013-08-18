using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations;

namespace AndrewTweddle.BattleCity.Core.States
{
    public class TankState: MobileElementState
    {
        public Direction Dir { get; set; }
        public TankLocation Location { get; set; }
        public Segment OutsideLeadingEdge { get; set; }
        public Segment InsideTrailingEdge { get; set; }
        
        public Cell BulletFiringCell
        {
            get
            {
                return OutsideLeadingEdge.CentreCell;
            }
        }

        public Cell NextCentreCell
        {
            get
            {
                return Location.CentreCell.AdjacentCellsByDirection[(byte)Dir];
            }
        }

        public override MobileState GetMobileState()
        {
            return new MobileState(pos: Location.CentreCell.Position, dir: Dir, isActive: true);
        }
    }
}
