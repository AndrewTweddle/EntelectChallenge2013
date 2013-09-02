using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations;
using System.IO;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core.States
{
    [DataContract, KnownType(typeof(MutableGameState))]
    public abstract class GameState
    {
        #region Private Member Variables

        private GameStateCalculationCache calculationCache;

        #endregion

        #region Public Properties

        [DataMember]
        public int Tick { get; set; }

        [DataMember]
        public Outcome Outcome { get; set; }

        [DataMember]
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
                    || ((Outcome & Core.Outcome.CompletedButUnknown) == Core.Outcome.CompletedButUnknown);
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
                    Walls[x, y] = Game.Current.InitialCellTypes[x, y] == CellType.Wall;
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
                    return false;
                }
            }

            reasonDifferent = String.Empty;
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            sw.WriteLine("Tick: {0}. Outcome: {1}", Tick, Outcome);
            for (int i = 0; i < Constants.TANK_COUNT; i++)
            {
                int b = Constants.MIN_BULLET_INDEX + i;
                Tank tank = (Tank) Game.Current.Elements[i];
                Bullet bullet = (Bullet) Game.Current.Elements[b];

                MobileState tankState = GetMobileState(i);
                MobileState bulletState = GetMobileState(b);
                sw.Write("t{0}.{1} [id:{2}]: ", tank.PlayerNumber, tank.Number, tank.Id);
                if (tankState.IsActive)
                {
                    sw.Write("{0} @ {1}. ", tankState.Dir, tankState.Pos);
                }
                else
                {
                    sw.Write("DEAD. ");
                }
                if (bulletState.IsActive)
                {
                    sw.WriteLine("BULLET [id:{0}]: {1} @ {2}.",
                        Game.Current.Turns[Tick].BulletIds[i],
                        bulletState.Dir, bulletState.Pos);
                }
                else
                {
                    sw.WriteLine("NO BULLET IN PLAY.");
                }
            }
            sw.Flush();
            return sb.ToString();
        }

        #endregion
    }
}
