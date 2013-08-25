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
using AndrewTweddle.BattleCity.Core.Engines;
using challenge.entelect.co.za;

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

                InitializeGameBoard(ref states);

                int tickAtWhichGameEndSequenceBegins = 200; // TODO: Get from login() when this is added to it
                Game.Current.TickAtWhichGameEndSequenceBegins = tickAtWhichGameEndSequenceBegins;
                Game.Current.FinalTickInGame = tickAtWhichGameEndSequenceBegins -1 + Game.Current.BoardWidth / 2;

                Game.Current.InitializeTurns();

                DateTime localTimeBeforeGetStatusCall = DateTime.Now;
                game wsGame = client.getStatus();
                DateTime localTimeAfterGetStatusCall = DateTime.Now;

                UpdateCurrentTurn(
                    wsGame.currentTick,
                    wsGame.nextTickTime,
                    TimeSpan.FromMilliseconds(wsGame.millisecondsToNextTick),
                    localTimeBeforeGetStatusCall,
                    localTimeAfterGetStatusCall);

                int yourPlayerIndex;
                InitializePlayersAndUnits(wsGame, out yourPlayerIndex);
                return yourPlayerIndex;
            }
            finally
            {
                client.Close();
            }
        }

        private static void UpdateCurrentTurn(
            int currentTick, 
            DateTime nextTickTimeOnServer, 
            TimeSpan timeUntilNextTick, 
            DateTime localTimeBeforeGetStatusCall, 
            DateTime localTimeAfterGetStatusCall)
        {
            Game.Current.UpdateCurrentTurn(currentTick);
            Game.Current.CurrentTurn.NextServerTickTime = nextTickTimeOnServer;
            Game.Current.CurrentTurn.EstimatedLocalStartTime = localTimeBeforeGetStatusCall;
            Game.Current.CurrentTurn.EarliestLocalNextTickTime
                = localTimeBeforeGetStatusCall
                + timeUntilNextTick;
            Game.Current.CurrentTurn.LatestLocalNextTickTime
                = localTimeAfterGetStatusCall
                + timeUntilNextTick;
        }

        public void WaitForNextTick(int playerIndex)
        {
            while (!TryGetNewGameState(playerIndex))
            {
                Thread.Sleep(StatePollInterval);
            }
        }


        public bool TryGetNewGameState(int playerIndex)
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
                        // We don't want to lose important events. 
                        // Check that the harness only sends them when the current tick changes:
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
                    
                    UpdateCurrentTurn(wsGame.currentTick, wsGame.nextTickTime, 
                        TimeSpan.FromMilliseconds(wsGame.millisecondsToNextTick), 
                        localTimeBeforeGetStatusCall, localTimeAfterGetStatusCall);

                    GameState newGameState = prevGameState.Clone();
                    Game.Current.CurrentTurn.GameState = newGameState;
                    newGameState.Tick = currentTick;

                    //  Remove any walls that have been shot:
                    List<Point> wallsRemoveAfterPreviousTick = new List<Point>();
                    events evts = wsGame.events;
                    if (evts != null && evts.blockEvents != null)
                    {
                        foreach (blockEvent blockEv in evts.blockEvents)
                        {
                            if (blockEv.newStateSpecified)
                            {
                                switch (blockEv.newState)
                                {
                                    case state.EMPTY:
                                    case state.NONE:
                                        Point wallPoint = blockEv.point.Convert();
                                        wallsRemoveAfterPreviousTick.Add(wallPoint);
                                        newGameState.Walls[wallPoint] = false;
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
                    }
                    newGameState.WallsRemovedAfterPreviousTick = wallsRemoveAfterPreviousTick.ToArray();

                    // Update states of tanks and bullets which were destroyed:
                    if (evts != null && evts.unitEvents != null)
                    {
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
                                Point bulletPos = new Point((short)blt.x, (short)blt.y);
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
                    }

                    Turn prevTurn = Game.Current.PreviousTurn; 
                    TankAction[] tankActionsTaken = new TankAction[Constants.TANK_COUNT];
                    for (int i = 0; i < tankActionsTaken.Length; i++)
                    {
                        tankActionsTaken[i] = TankAction.NONE;
                    }

                    foreach (player plyr in wsGame.players)
                    {
                        int plyrIndex = -1;
                        for (int p = 0; p < Constants.PLAYERS_PER_GAME; p++)
                        {
                            if (Game.Current.Players[p].Name == plyr.name)
                            {
                                plyrIndex = p;
                                break;
                            }
                        }

                        if (plyr.units != null)
                        {
                            foreach (unit u in plyr.units)
                            {
                                if (u != null)
                                {
                                    Point newPos = new Point((short)u.x, (short)u.y);
                                    Direction newDir = u.direction.Convert();
                                    TankAction tankAction = u.actionSpecified ? u.action.Convert() : TankAction.NONE;

                                    bool tankFound = false;
                                    for (int t = 0; t < Constants.TANK_COUNT; t++)
                                    {
                                        Tank tank = Game.Current.Elements[t] as Tank;
                                        if (tank.Id == u.id)
                                        {
                                            MobileState newMobileState = new MobileState(newPos, newDir, isActive: true);
                                            newGameState.SetMobileState(t, ref newMobileState);
                                            tankActionsTaken[tank.Index] = tankAction;
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
                        }

                        // Record the actual actions taken by the player:
                        if (prevTurn != null)
                        {
                            prevTurn.TankActionsTakenAfterPreviousTurn = tankActionsTaken;
                        }

                        if (plyr.bullets != null)
                        {
                            foreach (bullet blt in plyr.bullets)
                            {
                                Point bulletPos = new Point((short)blt.x, (short)blt.y);
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
                    }

#if DEBUG
                    GameState calculatedGameState = prevGameState.Clone();
                    calculatedGameState.Tick = Game.Current.CurrentTurn.Tick;
                    if (calculatedGameState is MutableGameState)
                    {
                        MutableGameStateEngine.ApplyAllActions((MutableGameState)calculatedGameState, tankActionsTaken);
                    }
                    else
                        if (calculatedGameState is ImmutableGameState)
                        {
                            ImmutableGameStateEngine.ApplyActions((ImmutableGameState)calculatedGameState, tankActionsTaken);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                String.Format(
                                    "The calculated game state on turn {0} can't be cast to a known game state type", 
                                    Game.Current.CurrentTurn.Tick
                                )
                            );
                        }
                    Game.Current.CurrentTurn.GameStateCalculatedByGameStateEngine = calculatedGameState;

                    string reasonDifferent;
                    if (!GameState.AreGameStatesEquivalent(newGameState, calculatedGameState, out reasonDifferent))
                    {
                        throw new ApplicationException(
                            String.Format(
                                "The calculated game state does not match the new harness game state. \r\nReason: {0}", 
                                reasonDifferent
                            )
                        );
                    }
#endif
                    return true;
                }
                catch (FaultException<EndOfGameException> endOfGameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", endOfGameFault, endOfGameFault.Message);
                    Outcome outcome = playerIndex == 0 ? Outcome.Player2Win : Outcome.Player1Win;
                    Game.Current.CurrentTurn.GameState.Outcome = Outcome.Crashed | outcome;
                    return false;
                }
                catch (FaultException<NoBlameException> noBlameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", noBlameFault, noBlameFault.Message);
                    Game.Current.CurrentTurn.GameState.Outcome = Outcome.Crashed;
                    throw;
                }
            }
            finally
            {
                client.Close();
            }
        }

        public bool TrySetTankActions(TankActionSet actionSet, int timeoutInMilliseconds)
        {
            if (actionSet == null)
            {
                return true;
            }

            ChallengeClient client = new ChallengeClient(EndPointConfigurationName, Url);
            client.Open();
            try
            {
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
                catch (FaultException<EndOfGameException> endOfGameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", endOfGameFault, endOfGameFault.Message);
                    Outcome outcome = actionSet.PlayerIndex == 0 ? Outcome.Player2Win : Outcome.Player1Win;
                    Game.Current.CurrentTurn.GameState.Outcome = Outcome.Crashed | outcome;
                    return false;
                }
                catch (FaultException<NoBlameException> noBlameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", noBlameFault, noBlameFault.Message);
                    Game.Current.CurrentTurn.GameState.Outcome = Outcome.Crashed;
                    throw;
                }
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
                    Tank tank = new Tank
                    {
                        Id = tankUnit.id
                    };
                    player.Tanks[t] = tank;
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
            int boardHeight = states[0].Length;
            int boardWidth = states.Length;

            Game.Current.BoardHeight = (short) boardHeight;
            Game.Current.BoardWidth = (short) boardWidth;
            Game.Current.InitializeCellStates();

            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
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
