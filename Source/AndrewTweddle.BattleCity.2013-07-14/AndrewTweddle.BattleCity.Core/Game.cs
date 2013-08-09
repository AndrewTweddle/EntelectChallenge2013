using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public class Game
    {
        public int CurrentTick { get; set; }
        public BlockEvent[] BlockEvents { get; set; }
        public UnitEvent[] UnitEvents { get; set; }
        public DateTime NextTickTime { get; set; }
        public string PlayerName { get; set; }
        public List<Player> Players { get; set; }
    }
}
