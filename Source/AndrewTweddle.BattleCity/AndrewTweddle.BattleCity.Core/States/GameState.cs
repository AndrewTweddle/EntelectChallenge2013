using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.States
{
    public abstract class GameState
    {
        #region Public Properties

        public int Tick { get; set; }
        public Outcome Outcome { get; set; }
        public BitMatrix Walls { get; protected set; }
        public Point[] WallsRemovedAfterPreviousTick { get; set; }

        #endregion

        #region Calculated Properties

        public bool IsGameOver
        {
            get
            {
                // A draw is treated as a pair of wins for both players, so the following covers all 3 possibilities:
                return ((Outcome & Core.Outcome.Player1Win) == Core.Outcome.Player1Win)
                    || ((Outcome & Core.Outcome.Player2Win) == Core.Outcome.Player2Win)
                    || (Outcome == Core.Outcome.Crashed);
            }
        }

        #endregion

        #region Constructors

        public GameState()
        {
            WallsRemovedAfterPreviousTick = new Point[0];  // Will be set elsewhere
        }
        
        #endregion

        #region Methods

        protected virtual void InitializeGameState()
        {
            Tick = Game.Current.CurrentTurn.Tick;

            // Set up walls:
            Walls = new BitMatrix();
            for (short y = 0; y < Walls.Height; y++)
            {
                for (short x = 0; x < Game.Current.BoardWidth; x++)
                {
                    Walls[x, y] = Game.Current.InitialCellStates[x, y] == CellState.Wall;
                }
            }
        }

        public abstract MobileState GetMobileState(int index);
        public abstract void SetMobileState(int index, ref MobileState newMobileState);
        public abstract GameState Clone();

        public abstract void ApplyActions(TankAction[] tankActions);

        #endregion
    }
}
