using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Actions
{
    public class TankActionSet
    {
        public int Tick { get; set; }
        public TankAction[] Actions { get; private set; }
    }
}
