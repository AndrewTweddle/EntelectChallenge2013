using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core.States
{
    public class GameState
    {
        #region Public Properties

        public int Tick { get; set; }
        public Outcome Outcome { get; set; }
        public BitArray[] Walls { get; private set; }
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
            Walls = new BitArray[Game.Current.BoardHeight];
            for (int y = 0; y < Walls.Length; y++)
            {
                Walls[y] = new BitArray(Game.Current.BoardWidth);
                for (int x = 0; x < Game.Current.BoardWidth; x++)
                {
                    Walls[y][x] = Game.Current.InitialCellStates[x, y] == CellState.Wall;
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

        public BitArray[] CloneWalls()
        {
            BitArray[] newWalls = new BitArray[Walls.Length];
            for (int y = 0; y < Walls.Length; y++)
            {
                BitArray newWallRow = (BitArray) Walls[y].Clone();
                newWalls[y] = newWallRow;
            }
            return newWalls;
        }

        public GameState Clone()
        {
            GameState clone = new GameState
            {
                MobileStates = CloneMobileStates(),
                Outcome = this.Outcome,
                Tick = this.Tick,
                Walls = CloneWalls()
            };
            return clone;
        }

        public bool IsAWall(Point position)
        {
            return Walls[position.Y][position.X];
        }

        #endregion
    }
}
