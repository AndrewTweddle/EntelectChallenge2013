using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.States
{
    public class GameState
    {
        #region Public Properties

        public int Tick { get; set; }
        public Outcome Outcome { get; set; }
        public BitMatrix Walls { get; private set; }
        public MobileState[] MobileStates { get; private set; }

        #endregion

        #region Calculated Properties

        public bool IsGameOver
        {
            get
            {
                return Outcome == Outcome.WinForYou
                    || Outcome == Core.Outcome.WinForOpponent
                    || Outcome == Core.Outcome.Draw
                    || Outcome == Core.Outcome.Crashed;
            }
        }

        #endregion

        #region Constructors

        public GameState()
        {
            MobileStates = new MobileState[Constants.MOBILE_ELEMENT_COUNT];
        }
        
        #endregion

        #region Methods

        public static GameState GetInitialGameState()
        {
            GameState gameState = new GameState();
            gameState.InitializeGameState();
            return gameState;
        }

        private void InitializeGameState()
        {
            Tick = Game.Current.CurrentTick;

            // Set up walls:
            Walls = new BitMatrix();
            for (short y = 0; y < Walls.RowCount; y++)
            {
                for (short x = 0; x < Game.Current.BoardWidth; x++)
                {
                    Walls[x, y] = Game.Current.InitialCellStates[x, y] == CellState.Wall;
                }
            }

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

        public MobileState[] CloneMobileStates()
        {
            MobileState[] newMobileStates = new MobileState[MobileStates.Length];
            for (int i = 0; i < MobileStates.Length; i++)
            {
                newMobileStates[i] = MobileStates[i].Clone();
            }
            return newMobileStates;
        }

        public GameState Clone()
        {
            GameState clone = new GameState
            {
                MobileStates = CloneMobileStates(),
                Outcome = this.Outcome,
                Tick = this.Tick,
                Walls = this.Walls.Clone()
            };
            return clone;
        }

        #endregion
    }
}
