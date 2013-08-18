using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.States
{
    public abstract class MobileElementState
    {
        public abstract MobileState GetMobileState();
    }
}
