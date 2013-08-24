using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Actions;
using AndrewTweddle.BattleCity.Core.Calculations;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    public class Turn
    {
        #region Private Member Variables

        private TurnCalculationCache calculationCache;

        #endregion


        #region Public Properties

        public DateTime NextServerTickTime { get; set; }
        public DateTime EarliestLocalNextTickTime { get; set; }
        public DateTime LatestLocalNextTickTime { get; set; }

        public int Tick { get; set; }
        public DateTime ServerStartTime { get; set; }
        public DateTime EstimatedLocalStartTime { get; set; }
        public int[] BulletIds { get; private set; }
        public GameState GameState { get; set; }
        public TankActionSet[] TankActionSetsSent { get; private set; }
        public TankAction[] TankActionsTakenAfterPreviousTurn { get; set; }
        public GameState GameStateCalculatedByGameStateEngine { get; set; }

        // An out-of-bounds area will encroach on the board from the sides after the game end phase is reached.
        // The following properties store the predicted min and max valid x values.
        // So values of x < LeftBoundary or x > RightBoundary will automatically be out of bounds:
        public int LeftBoundary { get; set; }
        public int RightBoundary { get; set; }

        public Turn PreviousTurn
        {
            get
            {
                if (Tick <= 1)
                {
                    return null;
                }
                else
                {
                    return Game.Current.Turns[Tick - 1];
                }
            }
        }

        public TurnCalculationCache CalculationCache
        {
            get
            {
                if (calculationCache == null)
                {
                    // Share calculation cache with previous turn if possible:
                    if ((PreviousTurn != null) && (PreviousTurn.LeftBoundary == LeftBoundary))
                    {
                        calculationCache = PreviousTurn.CalculationCache;
                    }
                    else
                    {
                        calculationCache = new TurnCalculationCache(this);
                    }
                }
                return calculationCache;
            }
        }

        #endregion


        #region Constructors

        protected Turn()
        {
            BulletIds = new int[Constants.TANK_COUNT];
            TankActionsTakenAfterPreviousTurn = new TankAction[Constants.PLAYERS_PER_GAME];
            TankActionSetsSent = new TankActionSet[Constants.PLAYERS_PER_GAME];
        }

        public Turn(int tick): this()
        {
            Tick = tick;
            if (Game.Current.TickAtWhichGameEndSequenceBegins > Tick)
            {
                LeftBoundary = 0;
                RightBoundary = Game.Current.BoardWidth - 1;
            }
            else
            {
                LeftBoundary = Tick - Game.Current.TickAtWhichGameEndSequenceBegins + 1;
                RightBoundary = Game.Current.BoardWidth - 2 - Tick + Game.Current.TickAtWhichGameEndSequenceBegins;
            }
        }

        #endregion
    }
}
