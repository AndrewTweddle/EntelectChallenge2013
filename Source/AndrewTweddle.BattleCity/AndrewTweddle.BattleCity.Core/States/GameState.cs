﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations;

namespace AndrewTweddle.BattleCity.Core.States
{
    public abstract class GameState
    {
        #region Private Member Variables

        private GameStateCalculationCache calculationCache;

        #endregion

        #region Public Properties

        public int Tick { get; set; }
        public Outcome Outcome { get; set; }
        public BitMatrix Walls { get; protected set; }
        public Point[] WallsRemovedAfterPreviousTick { get; set; }
        public GameStateCalculationCache CalculationCache 
        {
            get
            {
                if (calculationCache == null)
                {
                    calculationCache = new GameStateCalculationCache(this);
                }
                return calculationCache;
            }
        }

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

        #region Virtual and Abstract Methods

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

        #region Public Methods

        public static bool AreGameStatesEquivalent(GameState gameState1, GameState gameState2, out string reasonDifferent)
        {
            if (gameState1.Tick != gameState2.Tick)
            {
                reasonDifferent = "Different ticks";
                return false;
            }

            if (gameState1.Outcome != gameState2.Outcome)
            {
                reasonDifferent = "Outcomes different";
                return false;
            }

            if (!object.Equals(gameState1.Walls, gameState2.Walls))
            {
                reasonDifferent = "Walls are different";
                return false;
            }

            for (int i = 0; i < Constants.MOBILE_ELEMENT_COUNT; i++)
            {
                MobileState mobileState1 = gameState1.GetMobileState(i);
                MobileState mobileState2 = gameState2.GetMobileState(i);
                if (mobileState1 != mobileState2)
                {
                    Element element = Game.Current.Elements[i];
                    ElementType elementType = element.ElementType;
                    reasonDifferent = String.Format("{0} Element {1} different (player {2}, index {3})", 
                        elementType, i, element.PlayerNumber, element.Number);
                }
            }

            reasonDifferent = String.Empty;
            return true;
        }

        #endregion
    }
}
