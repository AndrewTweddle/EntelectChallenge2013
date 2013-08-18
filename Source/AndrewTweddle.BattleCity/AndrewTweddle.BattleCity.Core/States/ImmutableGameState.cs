using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core.States
{
    public class ImmutableGameState: GameState<ImmutableGameState>
    {
        public TankState[] TankStates { get; private set; }
        public BulletState[] BulletStates { get; private set; }

        protected override void InitializeGameState()
        {
            base.InitializeGameState();

            // Set up tanks and bullets:
            foreach (Player player in Game.Current.Players)
            {
                foreach (Tank tank in player.Tanks)
                {
                    TankStates[tank.Index] = null;  // TODO: Get this from the pre-calculated tank states based on their position
                    BulletStates[tank.Index] = null;
                }
            }
        }

        public override ImmutableGameState CloneDerived()
        {
            ImmutableGameState clone = base.CloneDerived();
            clone.TankStates = (TankState[]) TankStates.Clone();
            clone.BulletStates = (BulletState[]) BulletStates.Clone();
            return clone;
        }

        public override MobileState GetMobileState(int index)
        {
            if (index < Constants.TANK_COUNT)
            {
                TankState tankState = TankStates[index];
                if (tankState == null)
                {
                    return new MobileState();  // IsActive will be false
                }
                return tankState.GetMobileState();
            }
            int bulletIndex = index - Constants.TANK_COUNT;
            BulletState bulletState = BulletStates[bulletIndex];
            if (bulletState == null)
            {
                return new MobileState();  // IsActive will be false
            }
            return bulletState.GetMobileState();
        }

        public override void SetMobileState(int index, ref MobileState newMobileState)
        {
            if (index < Constants.TANK_COUNT)
            {
                if (newMobileState.IsActive)
                {
                    // TODO: Set the new TankState

                    return;
                }
                else
                {
                    TankStates[index] = null;
                }
            }
            else
            {
                int bulletIndex = index - Constants.TANK_COUNT;
                if (newMobileState.IsActive)
                {
                    // TODO: Set the new BulletState
                }
                else
                {
                    BulletStates[bulletIndex] = null;
                }
            }
        }

        public override void ApplyActions(TankAction[] tankActions)
        {
            throw new NotImplementedException();
        }
    }
}
