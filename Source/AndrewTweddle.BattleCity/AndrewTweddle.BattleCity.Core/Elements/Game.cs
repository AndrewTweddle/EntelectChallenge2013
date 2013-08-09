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

        public Player[] Players { get; private set; }
        public int YourPlayerIndex { get; set; }
        public short BoardHeight { get; set; }
        public short BoardWidth { get; set; }
        public Element[] Elements { get; private set; }
        public int[] TankMovementIndexes { get; private set; }

        // TODO: Add Id to Bullet class, and store bullet ids there instead of in the following array:
        public int[] BulletIds { get; private set; }

        // Initial setup:
        public int CurrentTick { get; set; }
        public DateTime NextServerTickTime { get; set; }
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
            for (int i = 0; i < Constants.PLAYERS_PER_GAME; i++)
            {
                Players[i] = new Player();
            }
            TankMovementIndexes = new int[Constants.TANK_COUNT];
            BulletIds = new int[Constants.TANK_COUNT];
        }

        #endregion

        #region Methods

        public void InitializeCellStates()
        {
            InitialCellStates = new States.CellState[BoardWidth, BoardHeight];
        }

        public void InitializeElements()
        {
            for (byte p = 0; p < Players.Length; p++)
            {
                Player player = Players[p];
                int playerMaskValue = (p == YourPlayerIndex) ? Constants.YOU_MASK_VALUE : Constants.OPPONENT_MASK_VALUE;
                int baseMaskValue = Constants.BASE_MASK_VALUE | playerMaskValue;
                Elements[baseMaskValue] = player.Base;

                for (int t = 0; t < player.Tanks.Length; t++)
                {
                    Tank tank = player.Tanks[t];
                    int tankMaskVale = (t * Constants.TANK_MASK_VALUE) | playerMaskValue;
                    int bulletMaskValue = (t * Constants.BULLET_MASK_VALUE) | playerMaskValue;
                    Elements[tankMaskVale] = tank;
                    Elements[bulletMaskValue] = tank.Bullet;
                }
            }
            
            for (int tmi = 0; tmi < GameRuleConfiguration.RuleConfiguration.TankMovementOrders.Length - 1; tmi++)
            {
                TankMovementLookup lookup = GameRuleConfiguration.RuleConfiguration.TankMovementOrders[tmi];
                TankMovementIndexes[tmi] = Players[lookup.PlayerNumber].Tanks[lookup.TankNumber].Index;
            }
        }

        #endregion
    }
}
