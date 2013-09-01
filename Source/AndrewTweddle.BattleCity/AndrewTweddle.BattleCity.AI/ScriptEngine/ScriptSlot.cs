using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScriptEngine
{
    public class ScriptSlot
    {
        public Instant Instant { get; set; }
        MobileState ExpectedElementState { get; set; }
    }
}
