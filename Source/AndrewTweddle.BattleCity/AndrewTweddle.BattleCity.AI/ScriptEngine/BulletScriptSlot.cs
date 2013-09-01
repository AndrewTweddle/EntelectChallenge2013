using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScriptEngine
{
    public class BulletScriptSlot: ScriptSlot
    {
        public BulletStatus ExpectedStatus { get; set; }
    }
}
