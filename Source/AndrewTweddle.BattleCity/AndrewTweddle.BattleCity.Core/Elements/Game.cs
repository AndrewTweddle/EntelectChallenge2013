using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Engines;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    public class Game
    {
        #region Properties

        public DateTime LocalGameStartTime { get; set; }
        public Player[] Players { get; private set; }
        public int YourPlayerIndex { get; set; }
        public short BoardHeight { get; set; }
        public short BoardWidth { get; set; }
        public Element[] Elements { get; private set; }

        // Turn-specific information. This can be for past, current and future turns
        // Storing future turn information is useful, because the game end conditions can be pre-calculated, 
        // which the solver algorithm may need to take into account:
        public List<Turn> Turns { get; private set; }
        public Turn CurrentTurn { get; set; }

        // Initial setup:
        public States.CellState[,] InitialCellStates { get; private set; }

        // Convenience properties:
        public Player You
        {
            get
            {
                return Players[YourPlayerIndex];
            }
        }

        public Player Opponent
        {
            get
            {
                return Players[1 - YourPlayerIndex];
            }
        }

        #endregion

        #region Singleton pattern

        public static Game Current { get; private set; }

        static Game()
        {
            Current = new Game();
        }

        protected Game()
        {
            Elements = new Element[Constants.ALL_ELEMENT_COUNT];
            Players = new Player[Constants.PLAYERS_PER_GAME];
            Turns = new List<Turn>();
            for (int i = 0; i < Constants.PLAYERS_PER_GAME; i++)
            {
                Players[i] = new Player();
            }
        }

        #endregion

        #region Methods

        public void InitializeCellStates()
        {
            InitialCellStates = new States.CellState[BoardWidth, BoardHeight];
        }

        public void InitializeElements()
        {
            for (byte playerMaskValue = 0; playerMaskValue < Players.Length; playerMaskValue++)
            {
                Player player = Players[playerMaskValue];
                int baseMaskValue = Constants.BASE_MASK_VALUE | playerMaskValue;
                Elements[baseMaskValue] = player.Base;

                for (int t = 0; t < player.Tanks.Length; t++)
                {
                    Tank tank = player.Tanks[t];
                    int tankMaskValue = (t * Constants.TANK_MASK_VALUE) | playerMaskValue;
                    int bulletMaskValue = (t * Constants.BULLET_MASK_VALUE) | playerMaskValue;
                    Elements[tankMaskValue] = tank;
                    Elements[bulletMaskValue] = tank.Bullet;
                }
            }
        }

        public void UpdateCurrentTurn(int turnTickTime)
        {
            // TODO: Synchronize access to CurrentTurn
            if (Turns.Count <= turnTickTime)
            {
                for (int i = Turns.Count; i < turnTickTime; i++)
                {
                    Turn turn = new Turn
                    {
                        Tick = i
                    };
                    Turns[i] = turn;
                }
            }
            CurrentTurn = Turns[turnTickTime];
        }

        #endregion
    }
}
