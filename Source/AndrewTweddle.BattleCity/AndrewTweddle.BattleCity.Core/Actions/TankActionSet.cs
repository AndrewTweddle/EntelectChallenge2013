using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Actions
{
    public class TankActionSet
    {
        #region Properties

        public int PlayerIndex { get; set; }
        public int Tick { get; set; }
        public TankAction[] Actions { get; private set; }
        public TimeSpan TimeTakenToSubmit { get; set; }

        #endregion


        #region Constructors

        protected TankActionSet()
        {
            Actions = new TankAction[Constants.TANKS_PER_PLAYER];
        }

        public TankActionSet(int playerIndex, int tick)
            : this()
        {
            PlayerIndex = playerIndex;
            Tick = tick;
        }

        #endregion
    }
}
