using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Actions;
using AndrewTweddle.BattleCity.AI;
using System.Threading;
using System.ServiceModel;
using AndrewTweddle.BattleCity.Core.Helpers;
using System.Threading.Tasks;

namespace AndrewTweddle.BattleCity.Comms.Client
{
    public class WebServiceAdapter: ICommunicator
    {
        #region Constants

        private const int DEFAULT_POLL_INTERVAL_IN_MILLISECONDS = 50;
        private const int DEFAULT_TIME_TO_WAIT_FOR_SET_ACTION_RESPONSE_IN_MS = 100;

        #endregion

        #region Public Properties

        public string EndPointConfigurationName { get; set; }
        public string Url { get; set; }
        public TimeSpan StatePollInterval { get; set; }

        #endregion

        #region Constructors

        public WebServiceAdapter()
        {
            StatePollInterval = TimeSpan.FromMilliseconds(DEFAULT_POLL_INTERVAL_IN_MILLISECONDS);
        }

        #endregion

        #region ICommunicator implementation

        public int LoginAndGetYourPlayerIndex()
        {
            ChallengeClient client = new ChallengeClient(EndPointConfigurationName, Url);
            client.Open();
            try
            {
                Game.Current.LocalGameStartTime = DateTime.Now;
                state?[][] states = client.login();

                Game.Current.TickAtWhichGameEndSequenceBegins = 200; // TODO: Get from login() when this is added to it
                InitializeGameBoard(ref states);

                DateTime localTimeBeforeGetStatusCall = DateTime.Now;
                game wsGame = client.getStatus();
                DateTime localTimeAfterGetStatusCall = DateTime.Now;
                
                UpdateCurrentTurn(wsGame, localTimeBeforeGetStatusCall, localTimeAfterGetStatusCall);

                int yourPlayerIndex;
                InitializePlayersAndUnits(wsGame, out yourPlayerIndex);
                return yourPlayerIndex;
            }
            finally
            {
                client.Close();
            }
        }

        private static void UpdateCurrentTurn(game wsGame, DateTime localTimeBeforeGetStatusCall, DateTime localTimeAfterGetStatusCall)
        {
            Game.Current.UpdateCurrentTurn(wsGame.currentTick);
            Game.Current.CurrentTurn.NextServerTickTime = wsGame.nextTickTime;
            Game.Current.CurrentTurn.EstimatedLocalStartTime = localTimeBeforeGetStatusCall;
            Game.Current.CurrentTurn.EarliestLocalNextTickTime
                = localTimeBeforeGetStatusCall
                + TimeSpan.FromMilliseconds(wsGame.millisecondsToNextTick);
            Game.Current.CurrentTurn.LatestLocalNextTickTime
                = localTimeAfterGetStatusCall
                + TimeSpan.FromMilliseconds(wsGame.millisecondsToNextTick);
        }

        public void WaitForNextTick()
        {
            while (!TryGetNewGameState())
            {
                Thread.Sleep(StatePollInterval);
            }
        }

        public bool TryGetNewGameState()
        {
            ChallengeClient client = new ChallengeClient(EndPointConfigurationName, Url);
            client.Open();
            try
            {
                try
                {
                    DateTime localTimeBeforeGetStatusCall = DateTime.Now;
                    game wsGame = client.getStatus();
                    DateTime localTimeAfterGetStatusCall = DateTime.Now;
                    int currentTick = wsGame.currentTick;
                    GameState prevGameState = Game.Current.CurrentTurn.GameState;

                    if (currentTick == Game.Current.CurrentTurn.Tick)
                    {
#if DEBUG
                        if (wsGame.events != null)
                        {
                            if (wsGame.events.blockEvents != null && wsGame.events.blockEvents.Length > 0)
                            {
                                throw new InvalidOperationException(
                                    "There are block events on a game object that is not the new game object");
                            }

                            if (wsGame.events.unitEvents != null && wsGame.events.unitEvents.Length > 0)
                            {
                                throw new InvalidOperationException(
                                    "There are unit events on a game object that is not the new game object");
                            }
                        }
#endif
                        return false;
                    }
                    
                    UpdateCurrentTurn(wsGame, localTimeBeforeGetStatusCall, localTimeAfterGetStatusCall);

                    GameState newGameState = prevGameState.Clone();
                    Game.Current.CurrentTurn.GameState = newGameState;
                    newGameState.Tick = currentTick;

                    //  Remove any walls that have been shot:
                    events evts = wsGame.events;
                    foreach (blockEvent blockEv in evts.blockEvents)
                    {
                        if (blockEv.newStateSpecified)
                        {
                            switch (blockEv.newState)
                            {
                                case state.EMPTY:
                                case state.NONE:
                                    newGameState.Walls[blockEv.point.Convert()] = false;
                                    break;

#if DEBUG
                                case state.OUT_OF_BOUNDS:
                                    if (blockEv.point.x >= Game.Current.CurrentTurn.LeftBoundary 
                                        && blockEv.point.x <= Game.Current.CurrentTurn.RightBoundary)
                                    {
                                        throw new InvalidOperationException(
                                            String.Format(
                                                "An out-of-bounds block event was found at ({0}, {1}) inside the boundaries of the board",
                                                blockEv.point.x, blockEv.point.y)
                                        );
                                    }
                                    break;

                                case state.FULL:
                                    throw new InvalidOperationException(
                                        String.Format(
                                            "A 'FULL' block event was found at ({0}, {1}) contrary to expectation",
                                            blockEv.point.x, blockEv.point.y)
                                    );
                                    break;
#endif
                            }
                        }
                    }

                    // Update states of tanks and bullets which were destroyed:
                    foreach (unitEvent unitEv in evts.unitEvents)
                    {
                        unit u = unitEv.unit;
                        if (u != null)
                        {
                            for (int t = 0; t < Constants.TANK_COUNT; t++)
                            {
                                Tank tank = Game.Current.Elements[t] as Tank;
                                if (tank.Id == u.id)
                                {
                                    Point newPos = new Point((short)u.x, (short)u.y);
                                    MobileState newMobileState = new MobileState(newPos, u.direction.Convert(), isActive: false);
                                    newGameState.SetMobileState(t, ref newMobileState);
                                }
                            }
                        }

                        bullet blt = unitEv.bullet;
                        if (blt != null)
                        {
                            Direction bulletDir = blt.direction.Convert();
                            Point bulletPos = new Point((short) blt.x, (short) blt.y);
                            MobileState newMobileState = new MobileState(bulletPos, bulletDir, isActive: false);

                            // Look for the bullet:
                            int i = 0;
                            bool bulletFound = false;
                            for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
                            {
                                if (Game.Current.CurrentTurn.BulletIds[i] == blt.id)
                                {
                                    newGameState.SetMobileState(b, ref newMobileState);
                                    bulletFound = true;
                                    break;
                                }
                                i++;
                            }
#if DEBUG
                            if (!bulletFound)
                            {
                                throw new InvalidOperationException(
                                    String.Format(
                                        "The destroyed bullet with id {0} was not found in the list of bullet id's on turn {1}",
                                        blt.id, currentTick)
                                );
                            }
#endif
                        }
                    }

                    foreach (player plyr in wsGame.players)
                    {
                        foreach (unit u in plyr.units)
                        {
                            if (u != null)
                            {
                                Point newPos = new Point((short)u.x, (short)u.y);
                                Direction newDir = u.direction.Convert();

                                bool tankFound = false;
                                for (int t = 0; t < Constants.TANK_COUNT; t++)
                                {
                                    Tank tank = Game.Current.Elements[t] as Tank;
                                    if (tank.Id == u.id)
                                    {
                                        MobileState newMobileState = new MobileState(newPos, newDir, isActive: true);
                                        newGameState.SetMobileState(t, ref newMobileState);
                                        tankFound = true;
                                        break;
                                    }
                                }

#if DEBUG
                                if (!tankFound)
                                {
                                    throw new InvalidOperationException(
                                        String.Format(
                                            "No existing tank could be found with id {0} at position ({1}, {2})",
                                            u.id, newPos.X, newPos.Y)
                                    );
                                }
#endif
                            }
                        }

                        foreach (bullet blt in plyr.bullets)
                        {
                            Point bulletPos = new Point((short) blt.x, (short) blt.y);
                            Direction bulletDir = blt.direction.Convert();
                            MobileState newMobileState = new MobileState(bulletPos, bulletDir, isActive: true);

                            // Look for the bullet:
                            int i = 0;
                            bool bulletFound = false;
                            for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
                            {
                                if (Game.Current.CurrentTurn.BulletIds[i] == blt.id)
                                {
                                    newGameState.SetMobileState(b, ref newMobileState);
                                    bulletFound = true;
                                    break;
                                }
                                i++;
                            }

                            if (!bulletFound)
                            {
                                // Find a tank which has the same direction as the bullet, 
                                // and where the bullet is in the correct position for a newly fired bullet:
                                for (int t = 0; t < Constants.TANK_COUNT; t++)
                                {
                                    MobileState tankState = newGameState.GetMobileState(t);
                                    if (tankState.Dir == bulletDir)
                                    {
                                        Point tankFiringPoint = tankState.GetTankFiringPoint();
                                        if (tankFiringPoint == bulletPos)
                                        {
                                            Tank tank = Game.Current.Elements[t] as Tank;
                                            int bulletIndex = tank.Bullet.Index;
                                            newGameState.SetMobileState(bulletIndex, ref newMobileState);
                                            Game.Current.CurrentTurn.BulletIds[t] = blt.id;
                                            bulletFound = true;
                                            break;
                                        }
                                    }
                                }

#if DEBUG
                                if (!bulletFound)
                                {
                                    throw new InvalidOperationException(
                                        string.Format(
                                            "The new bullet with id {0} at ({1}, {2}) could not be matched to a tank",
                                            blt.id, bulletPos.X, bulletPos.Y
                                        )
                                    );
                                }
#endif
                            }
                        }
                    }
                    return true;
                }
                catch (FaultException faultEx)
                {
                    throw;
                    // TODO: Work out how to access the challenge.entelect.co.za.EndOfGameException endOfGameExc)
                }
            }
            finally
            {
                client.Close();
            }
        }

        public bool TrySetTankActions(TankActionSet actionSet, int timeoutInMilliseconds)
        {
            ChallengeClient client = new ChallengeClient(EndPointConfigurationName, Url);
            client.Open();
            try
            {
                if (actionSet.Tick == Game.Current.CurrentTurn.Tick)
                {
                    GameState currentGameState = Game.Current.CurrentTurn.GameState;
                    int numberAlive = 0;
                    int tankId = -1;
                    TankAction tankAction = TankAction.NONE;

                    for (int t = 0; t < Constants.TANKS_PER_PLAYER; t++)
                    {
                        Tank tank = Game.Current.Players[actionSet.PlayerIndex].Tanks[t];
                        MobileState tankState = currentGameState.GetMobileState(tank.Index);
                        if (tankState.IsActive)
                        {
                            numberAlive++;
                            tankId = tank.Id;
                            tankAction = actionSet.Actions[t];
                        }
                    }

                    if (numberAlive == 2)
                    {
                        client.setActions(actionSet.Actions[0].Convert(), actionSet.Actions[1].Convert());
                    }
                    else
                        if (numberAlive == 1)
                        {
                            client.setAction(tankId, tankAction.Convert());
                        }
                    return true;
                }
                return false;
            }
            finally
            {
                client.Close();
            }
        }

        #endregion


        #region Private Game Initialization Methods

        private static void InitializePlayersAndUnits(game wsGame, out int yourPlayerIndex)
        {
            yourPlayerIndex = -1;  // To keep the compiler happy

            for (int p = 0; p < wsGame.players.Length; p++)
            {
                bool isYou = false;
                player wsPlayer = wsGame.players[p];
                Player player = Game.Current.Players[p];
                player.Index = p;
                player.Name = wsPlayer.name;
                if (wsPlayer.name == wsGame.playerName)
                {
                    yourPlayerIndex = p;
                    isYou = true;
                }

                // Set up the base:
                player.Base.Pos = new Point((short) wsPlayer.@base.x, (short) wsPlayer.@base.y);

                // Set up the tanks:
                for (int t = 0; t < wsPlayer.units.Length; t++)
                {
                    unit tankUnit = wsPlayer.units[t];
                    Tank tank = player.Tanks[t];
                    tank.Id = tankUnit.id;
                    tank.InitialCentrePosition = new Point((short) tankUnit.x, (short) tankUnit.y);
                    if (tankUnit.directionSpecified)
                    {
                        tank.InitialDirection = tankUnit.direction.Convert();
                    }
                    if (tankUnit.actionSpecified)
                    {
                        tank.InitialAction = tankUnit.action.Convert();
                    }
                }

                // Ensure that the player's tanks are in order of Id:
                if (player.Tanks[0] == null || player.Tanks[1] == null)
                {
                    if (isYou)
                    {
                        throw new ApplicationException("Your tanks were not all initialized during initial setup");
                    }
                    else
                    {
                        throw new ApplicationException("The opponent's tanks were not all initialized during initial setup");
                    }
                }

                if (player.Tanks[0].Id > player.Tanks[1].Id)
                {
                    Tank newTank1 = player.Tanks[0];
                    player.Tanks[0] = player.Tanks[1];
                    player.Tanks[1] = newTank1;
                }
            }
            Game.Current.InitializeElements();
        }

        private static void InitializeGameBoard(ref state?[][] states)
        {
            Game.Current.BoardHeight = (short) states.GetLength(0);
            Game.Current.BoardWidth = (short) states.GetLength(1);
            Game.Current.InitializeCellStates();

            for (int x = 0; x < states.GetLength(0); x++)
            {
                for (int y = 0; y < states.GetLength(1); y++)
                {
                    var state = states[x][y];
                    if (state.HasValue)
                    {
                        switch (state.Value)
                        {
                            case global::state.FULL:
                                Game.Current.InitialCellStates[x, y] = CellState.Wall;
                                break;
                            case global::state.OUT_OF_BOUNDS:
                                Game.Current.InitialCellStates[x, y] = CellState.OutOfBounds;
                                break;
                            default:
                                // case global::state.EMPTY:
                                // case global::state.NONE:
                                Game.Current.InitialCellStates[x, y] = CellState.Empty;
                                break;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
