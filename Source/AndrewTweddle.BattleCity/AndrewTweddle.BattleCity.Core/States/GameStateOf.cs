using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core.States
{
    [DataContract]
    public abstract class GameState<TGameState>: GameState
        where TGameState: GameState<TGameState>, new()
    {
        #region Constructors

        public GameState(): base()
        {
        }
        
        #endregion

        #region Methods

        public static TGameState GetInitialGameState()
        {
            TGameState gameState = new TGameState();
            gameState.InitializeGameState();
            return gameState;
        }

        public override GameState Clone()
        {
            return CloneDerived();
        }

        public virtual TGameState CloneDerived()
        {
            TGameState clone = new TGameState
            {
                Outcome = this.Outcome,
                Tick = this.Tick,
                Walls = this.Walls.Clone()
            };
            return clone;
        }

        #endregion
    }
}
