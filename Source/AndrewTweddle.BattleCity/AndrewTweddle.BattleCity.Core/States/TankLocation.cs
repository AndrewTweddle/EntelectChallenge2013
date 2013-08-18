using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations;

namespace AndrewTweddle.BattleCity.Core.States
{
    public class TankLocation: Location
    {
        public TankState[] TankStatesByDirection { get; private set; }
        public Rectangle TankBody { get; set; }

        public TankLocation()
        {
            TankStatesByDirection = new TankState[Constants.RELEVANT_DIRECTION_COUNT];
        }
    }
}
