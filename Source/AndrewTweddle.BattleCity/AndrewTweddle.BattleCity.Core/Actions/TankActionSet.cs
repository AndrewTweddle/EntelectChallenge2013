using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
            for (int i = 0; i < Constants.TANKS_PER_PLAYER; i++)
            {
                Actions[i] = TankAction.NONE;
            }
        }

        public TankActionSet(int playerIndex, int tick)
            : this()
        {
            PlayerIndex = playerIndex;
            Tick = tick;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            sw.WriteLine("Tank actions for player {0} @ tick {1}:", PlayerIndex, Tick);
            for (int i = 0; i < Constants.TANKS_PER_PLAYER; i++)
            {
                sw.WriteLine("Tank {0}: {1}", i, Actions[i]);
                Actions[i] = TankAction.NONE;
            }
            sw.Flush();
            return sb.ToString();
        }

        #endregion
    }
}
