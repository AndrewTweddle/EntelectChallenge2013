using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Engines;

namespace AndrewTweddle.BattleCity.Core.States
{
    public class MutableGameState: GameState<MutableGameState>
    {
        #region Public Properties

        public MobileState[] MobileStates { get; private set; }

        #endregion

        #region Constructors

        public MutableGameState()
        {
            MobileStates = new MobileState[Constants.MOBILE_ELEMENT_COUNT];
        }
        
        #endregion

        #region Public methods

        public MobileState[] CloneMobileStates()
        {
            MobileState[] newMobileStates = new MobileState[MobileStates.Length];
            for (int i = 0; i < MobileStates.Length; i++)
            {
                newMobileStates[i] = MobileStates[i].Clone();
            }
            return newMobileStates;
        }

        public override MutableGameState CloneDerived()
        {
            MutableGameState clone = base.CloneDerived();
            clone.MobileStates = this.CloneMobileStates();
            return clone;
        }

        #endregion

        #region Private and Protected Methods

        protected override void  InitializeGameState()
        {
 	        base.InitializeGameState();

            // Set up tanks and bullets:
            foreach (Player player in Game.Current.Players)
            {
                foreach (Tank tank in player.Tanks)
                {
                    MobileStates[tank.Index] = new MobileState(tank.InitialCentrePosition, tank.InitialDirection, isActive:true);
                    MobileStates[tank.Bullet.Index] = new MobileState(tank.InitialCentrePosition, tank.InitialDirection, isActive: false);
                }
            }
        }

        public override MobileState GetMobileState(int index)
        {
            return MobileStates[index];
        }

        public override void SetMobileState(int index, ref MobileState newMobileState)
        {
            MobileStates[index] = newMobileState;
        }

        public override void ApplyActions(TankAction[] tankActions)
        {
            MutableGameStateEngine.ApplyAllActions(this, tankActions);
        }

        #endregion
    }
}
