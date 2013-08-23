using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Actions
{
    public class TankActionSet
    {
        public int PlayerIndex { get; set; }
        public int Tick { get; set; }
        public TankAction[] Actions { get; private set; }
        public TimeSpan TimeToSubmit { get; set; }
    }
}
