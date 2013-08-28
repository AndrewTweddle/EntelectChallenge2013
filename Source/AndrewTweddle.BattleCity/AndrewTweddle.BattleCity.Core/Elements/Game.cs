using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Engines;
using System.Runtime.Serialization;
using System.IO;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    [DataContract]
    public class Game
    {
        #region Constants

        public const int MAX_ITEMS_IN_OBJECT_GRAPH = 1000000;
        
        #endregion

        #region Properties

        [DataMember]
        public DateTime LocalGameStartTime { get; set; }

        [DataMember]
        public Player[] Players { get; private set; }

        [DataMember]
        public int TickAtWhichGameEndSequenceBegins { get; set; }

        [DataMember]
        public int FinalTickInGame { get; set; }

        [DataMember]
        public short BoardHeight { get; set; }

        [DataMember]
        public short BoardWidth { get; set; }

        [DataMember]
        public Element[] Elements { get; private set; }

        /// <summary>
        /// Turn-specific information. This can be for past, current and future turns.
        /// Storing future turn information is useful, because the game end conditions can be pre-calculated, 
        /// which the solver algorithm may need to take into account.
        /// Note that the game ticks start at 1, so the first element of Turns is ignored.
        /// </summary>
        [DataMember]
        public List<Turn> Turns { get; private set; }

        [DataMember]
        public Turn CurrentTurn { get; set; }

        public Turn PreviousTurn
        {
            get
            {
                if (CurrentTurn == null)
                {
                    return null;
                }
                if (CurrentTurn.Tick == 1)
                {
                    return null;
                }
                return Turns[CurrentTurn.Tick - 1];
            }
        }

        public Turn VeryFirstTurn
        {
            get
            {
                return Turns[1];  // The ticks appear to be 1-based
            }
        }

        // Initial setup:
        public States.CellState[,] InitialCellStates { get; private set; }
        // Not serialized, since multi-dimensional arrays are not supported by the DataContractSerializer

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
        }

        #endregion

        #region Methods

        public void InitializeCellStates(CellState[,] initialCellStates)
        {
            InitialCellStates = initialCellStates;
        }

        public void InitializeElements()
        {
            for (byte playerMaskValue = 0; playerMaskValue < Players.Length; playerMaskValue++)
            {
                Player player = Players[playerMaskValue];
                int baseMaskValue = Constants.BASE_MASK_VALUE | playerMaskValue;
                player.Base.Index = baseMaskValue;
                Elements[baseMaskValue] = player.Base;

                for (int t = 0; t < player.Tanks.Length; t++)
                {
                    // Add the tank:
                    Tank tank = player.Tanks[t];
                    int tankMaskValue = Constants.TANK_MASK_VALUE | (t * Constants.UNIT_INDEX_MASK) | playerMaskValue;
                    tank.Index = tankMaskValue;
                    Elements[tankMaskValue] = tank;

                    // Add the bullet:
                    int bulletMaskValue = Constants.BULLET_MASK_VALUE | (t * Constants.UNIT_INDEX_MASK) | playerMaskValue;
                    tank.Bullet.Index = bulletMaskValue;
                    Elements[bulletMaskValue] = tank.Bullet;
                }
            }
        }

        public void InitializeTurns()
        {
            Turns = new List<Turn>(Game.Current.FinalTickInGame + 1);
            for (int i = 0; i <= Game.Current.FinalTickInGame; i++)
            {
                Turn turn = new Turn(i);
                Turns.Add(turn);
            }
        }

        public void UpdateCurrentTurn(int turnTick)
        {
            Turn prevTurn = CurrentTurn;
            if (prevTurn != null && (prevTurn.Tick < turnTick - 1))
            {
                for (int i = prevTurn.Tick + 1; i < turnTick; i++)
                {
                    Turns[i].IsSkipped = true;
                }
#if DEBUG
                throw new ApplicationException(
                    string.Format(
                        "Turns between {0} and {1} where skipped", 
                        prevTurn.Tick + 1, turnTick));
#endif
            }
            CurrentTurn = Turns[turnTick];
            if (prevTurn != null)
            {
                Array.Copy(prevTurn.BulletIds, CurrentTurn.BulletIds, Constants.TANK_COUNT);
            }
        }

        public void Save(string filePath)
        {
            DataContractSerializer dcs = new DataContractSerializer(
                typeof(Game), knownTypes: null, 
                maxItemsInObjectGraph: MAX_ITEMS_IN_OBJECT_GRAPH, ignoreExtensionDataObject: true, 
                preserveObjectReferences: true, dataContractSurrogate:null);
            using (Stream fs = File.Create(filePath))
            {
                dcs.WriteObject(fs, this);
            }
        }

        public static Game Load(string filePath)
        {
            DataContractSerializer dcs = new DataContractSerializer(
                typeof(Game), knownTypes: null,
                maxItemsInObjectGraph: MAX_ITEMS_IN_OBJECT_GRAPH, ignoreExtensionDataObject: true,
                preserveObjectReferences: true, dataContractSurrogate: null);
            using (Stream fs = File.OpenRead(filePath))
            {
                Game game = (Game) dcs.ReadObject(fs);
                return game;
            }
        }

        #endregion
    }
}
