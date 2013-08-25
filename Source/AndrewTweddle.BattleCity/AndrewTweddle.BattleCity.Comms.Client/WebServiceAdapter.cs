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
                DoBeforeLoggingIn();
                state?[][] states = client.login();

                InitializeGameBoard(ref states);

                int tickAtWhichGameEndSequenceBegins = 200; // TODO: Get from login() when this is added to it
                InitializeEndGameSequence(tickAtWhichGameEndSequenceBegins);

                DateTime localTimeBeforeGetStatusCall = DateTime.Now;
                game wsGame = client.getStatus();
                DateTime localTimeAfterGetStatusCall = DateTime.Now;

                UpdateCurrentTurn(
                    wsGame.currentTick,
                    wsGame.nextTickTime,
                    TimeSpan.FromMilliseconds(wsGame.millisecondsToNextTick),
                    localTimeBeforeGetStatusCall,
                    localTimeAfterGetStatusCall);

                int yourPlayerIndex = InitializePlayersAndUnitsAndGetYourPlayerIndex(wsGame);
                DoAfterLoggingInAndInitializingElements();
                return yourPlayerIndex;
            }
            finally
            {
                client.Close();
            }
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

                    GameState prevGameState = Game.Current.CurrentTurn.GameState;
                    UpdateCurrentTurn(wsGame.currentTick, wsGame.nextTickTime, 
                        TimeSpan.FromMilliseconds(wsGame.millisecondsToNextTick), 
                        localTimeBeforeGetStatusCall, localTimeAfterGetStatusCall);
                    GameState newGameState = prevGameState.Clone();
                    newGameState.Tick = currentTick;
                    Game.Current.CurrentTurn.GameState = newGameState;

                    //  Remove any walls that have been shot:
                    List<Point> wallsRemovedAfterPreviousTick = new List<Point>();
                    List<Point> outOfBoundsBlocksAfterPreviousTick = new List<Point>();

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
                                        wallsRemovedAfterPreviousTick.Add(wallPoint);
                                        break;

#if DEBUG
                                    case state.OUT_OF_BOUNDS:
                                        outOfBoundsBlocksAfterPreviousTick.Add(blockEv.point.Convert());
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

                    UpdateWalls(newGameState, wallsRemovedAfterPreviousTick, outOfBoundsBlocksAfterPreviousTick);
                    TankAction[] tankActionsTaken = new TankAction[Constants.TANK_COUNT];

                    // Update states of tanks and bullets which were destroyed:
                    if (evts != null && evts.unitEvents != null)
                    {
                        foreach (unitEvent unitEv in evts.unitEvents)
                        {
                            unit u = unitEv.unit;
                            if (u != null)
                            {
                                Point newPos = new Point((short)u.x, (short)u.y);
                                TankAction tankAction = u.actionSpecified ? u.action.Convert() : TankAction.NONE;
                                UpdateTankState(newGameState, tankActionsTaken, u.id, tankAction, newPos, u.direction.Convert(), isActive: false);
                            }

                            bullet blt = unitEv.bullet;
                            if (blt != null)
                            {
                                Point bulletPos = new Point((short)blt.x, (short)blt.y);
                                UpdateBulletState(newGameState, blt.id, bulletPos, blt.direction.Convert(), isActive: false);
                            }
                        }
                    }

                    Turn prevTurn = Game.Current.PreviousTurn; 
                    for (int i = 0; i < tankActionsTaken.Length; i++)
                    {
                        tankActionsTaken[i] = TankAction.NONE;
                    }

                    foreach (player plyr in wsGame.players)
                    {
                        if (plyr.units != null)
                        {
                            foreach (unit u in plyr.units)
                            {
                                if (u != null)
                                {
                                    Point newPos = new Point((short)u.x, (short)u.y);
                                    TankAction tankAction = u.actionSpecified ? u.action.Convert() : TankAction.NONE;
                                    UpdateTankState(newGameState, tankActionsTaken, u.id, tankAction, newPos, u.direction.Convert(), isActive: true);
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
                                UpdateBulletState(newGameState, blt.id, bulletPos, blt.direction.Convert(), isActive: true);
                            }
                        }
                    }

                    CheckGameState(prevGameState, newGameState, tankActionsTaken);
                    return true;
                }
                catch (FaultException<EndOfGameException> endOfGameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", endOfGameFault, endOfGameFault.Message);
                    UpdateGameOutcomeToLoseDueToError(playerIndex);
                    return false;
                }
                catch (FaultException<NoBlameException> noBlameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", noBlameFault, noBlameFault.Message);
                    UpdateGameOutcomeDueToNoBlameCrash();
                    throw;
                }
            }
            finally
            {
                client.Close();
            }
        }

        private bool TrySetAction(int playerIndex, int tankId, TankAction tankAction)
        {
            ChallengeClient client = new ChallengeClient(EndPointConfigurationName, Url);
            client.Open();
            try
            {
                try
                {
                    client.setAction(tankId, tankAction.Convert());
                    return true;
                }
                catch (FaultException<EndOfGameException> endOfGameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", endOfGameFault, endOfGameFault.Message);
                    UpdateGameOutcomeToLoseDueToError(playerIndex);
                    return false;
                }
                catch (FaultException<NoBlameException> noBlameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", noBlameFault, noBlameFault.Message);
                    UpdateGameOutcomeDueToNoBlameCrash();
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
                catch (EndpointNotFoundException endpointException)
                {
                    DebugHelper.LogDebugError("Web service adapter", endpointException, endpointException.Message);
                    UpdateGameOutcomeDueToServerUnavailable();
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
                catch (CommunicationException commsException)
                {
                    DebugHelper.LogDebugError("Web service adapter", commsException, commsException.Message);
                    UpdateGameOutcomeDueToServerUnavailable();
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
            }
            finally
            {
                client.Close();
            }
        }

        private bool TrySetActions(int playerIndex, TankAction tankAction1, TankAction tankAction2)
        {
            ChallengeClient client = new ChallengeClient(EndPointConfigurationName, Url);
            client.Open();
            try
            {
                try
                {
                    client.setActions(tankAction1.Convert(), tankAction2.Convert());
                    return true;
                }
                catch (FaultException<EndOfGameException> endOfGameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", endOfGameFault, endOfGameFault.Message);
                    UpdateGameOutcomeToLoseDueToError(playerIndex);
                    return false;
                }
                catch (FaultException<NoBlameException> noBlameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", noBlameFault, noBlameFault.Message);
                    UpdateGameOutcomeDueToNoBlameCrash();
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
                catch (EndpointNotFoundException endpointException)
                {
                    DebugHelper.LogDebugError("Web service adapter", endpointException, endpointException.Message);
                    UpdateGameOutcomeDueToServerUnavailable();
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
                catch (CommunicationException commsException)
                {
                    DebugHelper.LogDebugError("Web service adapter", commsException, commsException.Message);
                    UpdateGameOutcomeDueToServerUnavailable();
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
            }
            finally
            {
                client.Close();
            }
        }

        #endregion

        #region Private Game Initialization Methods

        private static int InitializePlayersAndUnitsAndGetYourPlayerIndex(game wsGame)
        {
            int yourPlayerIndex = -1;  // To keep the compiler happy

            for (int playerIndex = 0; playerIndex < wsGame.players.Length; playerIndex++)
            {
                bool isYou = false;
                player wsPlayer = wsGame.players[playerIndex];
                string playerName = wsPlayer.name;
                if (wsPlayer.name == playerName)
                {
                    yourPlayerIndex = playerIndex;
                    isYou = true;
                }
                Point basePos = new Point((short)wsPlayer.@base.x, (short)wsPlayer.@base.y);

                Player player = Game.Current.Players[playerIndex];
                player.Index = playerIndex;
                player.Name = playerName;

                // Set up the base:
                player.Base.Pos = basePos;

                // Set up the tanks:
                for (int tankNumber = 0; tankNumber < wsPlayer.units.Length; tankNumber++)
                {
                    unit tankUnit = wsPlayer.units[tankNumber];
                    int unitId = tankUnit.id;
                    Point initialTankPosition = new Point((short)tankUnit.x, (short)tankUnit.y);
                    Direction initialTankDirection 
                        = tankUnit.directionSpecified 
                        ? tankUnit.direction.Convert() 
                        : Direction.NONE;
                    TankAction initialTankAction 
                        = tankUnit.actionSpecified
                        ? tankUnit.action.Convert()
                        : TankAction.NONE;

                    InitializeTank(playerIndex, tankNumber, unitId, ref initialTankPosition, initialTankDirection, initialTankAction);
                }
            }
            return yourPlayerIndex;
        }

        #endregion

        #region Methods that are independent of the web service objects

        private static void DoBeforeLoggingIn()
        {
            Game.Current.LocalGameStartTime = DateTime.Now;
        }

        private static void DoAfterLoggingInAndInitializingElements()
        {
            foreach (Player player in Game.Current.Players)
            {
                OrderTanksById(player);
            }
            Game.Current.InitializeElements();
        }

        private static void InitializeEndGameSequence(int tickAtWhichGameEndSequenceBegins)
        {
            Game.Current.TickAtWhichGameEndSequenceBegins = tickAtWhichGameEndSequenceBegins;
            Game.Current.FinalTickInGame = tickAtWhichGameEndSequenceBegins - 1 + Game.Current.BoardWidth / 2;
            Game.Current.InitializeTurns();
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

        private static void UpdateGameOutcomeDueToNoBlameCrash()
        {
            Game.Current.CurrentTurn.GameState.Outcome = Outcome.Crashed;
        }

        private static void UpdateGameOutcomeToLoseDueToError(int playerIndex)
        {
            Outcome outcome = playerIndex == 0 ? Outcome.Player2Win : Outcome.Player1Win;
            Game.Current.CurrentTurn.GameState.Outcome = Outcome.Crashed | outcome;
        }

        private static void UpdateGameOutcomeDueToServerUnavailable()
        {
            // TODO: Infer a winner, since this exception usually occurs because the game ended and the harness shut down...
            Game.Current.CurrentTurn.GameState.Outcome = Outcome.Crashed;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void CheckGameState(GameState prevGameState, GameState newGameState, TankAction[] tankActionsTaken)
        {
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
        }

        private static void UpdateTankState(GameState newGameState, TankAction[] tankActionsTaken,
            int unitId, TankAction tankAction, Point newPos, Direction newDir,
            bool isActive)
        {
            bool tankFound = false;
            for (int t = 0; t < Constants.TANK_COUNT; t++)
            {
                Tank tank = Game.Current.Elements[t] as Tank;
                if (tank.Id == unitId)
                {
                    MobileState newMobileState = new MobileState(newPos, newDir, isActive);
                    newGameState.SetMobileState(t, ref newMobileState);
                    tankActionsTaken[tank.Index] = tankAction;
                    tankFound = true;
                    break;
                }
            }

            /* TODO: Else find a likely matching tank in the previous game state based on position and direction: */

#if DEBUG
            if (!tankFound)
            {
                throw new InvalidOperationException(
                    String.Format(
                        "No existing tank could be found with id {0} at position ({1}, {2})",
                        unitId, newPos.X, newPos.Y)
                );
            }
#endif
        }

        private static void UpdateBulletState(GameState newGameState, int bulletId, Point bulletPos, Direction bulletDir, bool isActive)
        {
            MobileState newMobileState = new MobileState(bulletPos, bulletDir, isActive);

            // Look for the bullet:
            int i = 0;
            bool bulletFound = false;
            for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
            {
                if (Game.Current.CurrentTurn.BulletIds[i] == bulletId)
                {
                    newGameState.SetMobileState(b, ref newMobileState);
                    bulletFound = true;
                    break;
                }
                i++;
            }

            if (!bulletFound)
            {
                /* Find a tank which has the same direction as the bullet, 
                 * and where the bullet is in the correct position for a newly fired bullet:
                 */
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
                            Game.Current.CurrentTurn.BulletIds[t] = bulletId;
                            bulletFound = true;
                            break;
                        }
                    }
                }

                /* TODO: Find a likely matching bullet in the previous game state based on position and direction:
                if (!bulletFound)
                {
                    // Find a bullet which has the same direction and is two spaces before this bullet:

                }
                 */

#if DEBUG
                if (!bulletFound)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "The {0}bullet with id {1} at {2} could not be matched to a tank",
                            isActive ? String.Empty : "dead ", bulletId, bulletPos
                        )
                    );
                }
#endif
            }
        }

        private static void UpdateWalls(GameState newGameState, List<Point> wallsRemovedAfterPreviousTick, List<Point> outOfBoundsBlocksAfterPreviousTick)
        {
            foreach (Point wallPoint in wallsRemovedAfterPreviousTick)
            {
                newGameState.Walls[wallPoint] = false;
            }
            newGameState.WallsRemovedAfterPreviousTick = wallsRemovedAfterPreviousTick.ToArray();
            foreach (Point outOfBoundsPoint in outOfBoundsBlocksAfterPreviousTick)
            {
                if (outOfBoundsPoint.X >= Game.Current.CurrentTurn.LeftBoundary
                    && outOfBoundsPoint.X <= Game.Current.CurrentTurn.RightBoundary)
                {
                    throw new InvalidOperationException(
                        String.Format(
                            "An out-of-bounds block was found at {0} inside the boundaries of the board",
                            outOfBoundsPoint)
                    );
                }
            }
        }

        private static void OrderTanksById(Player player)
        {
            // Ensure that the player's tanks are in order of Id:
            if (player.Tanks[0] == null || player.Tanks[1] == null)
            {
                throw new ApplicationException("A player's tanks were not all initialized during initial setup");
            }

            if (player.Tanks[0].Id > player.Tanks[1].Id)
            {
                Tank newTank1 = player.Tanks[0];
                player.Tanks[0] = player.Tanks[1];
                player.Tanks[1] = newTank1;
            }
        }

        private static void InitializeTank(int playerIndex, int tankNumber, int unitId,
            ref Point initialTankPosition, Direction initialTankDirection, TankAction initialTankAction)
        {
            Tank tank = new Tank
            {
                Id = unitId
            };
            Player player = Game.Current.Players[playerIndex];
            player.Tanks[tankNumber] = tank;
            tank.InitialCentrePosition = initialTankPosition;
            tank.InitialDirection = initialTankDirection;
            tank.InitialAction = initialTankAction;
        }

        private static void InitializeGameBoard(ref state?[][] states)
        {
            int boardHeight = states[0].Length;
            int boardWidth = states.Length;
            CellState[,] initialCellStates = new CellState[boardWidth, boardHeight];

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
                                initialCellStates[x, y] = CellState.Wall;
                                break;
                            case global::state.OUT_OF_BOUNDS:
                                initialCellStates[x, y] = CellState.OutOfBounds;
                                break;
                            default:
                                // case global::state.EMPTY:
                                // case global::state.NONE:
                                initialCellStates[x, y] = CellState.Empty;
                                break;
                        }
                    }
                }
            }

            InitializeGameBoard(boardHeight, boardWidth, initialCellStates);
        }

        private static void InitializeGameBoard(int boardHeight, int boardWidth, CellState[,] initialCellStates)
        {
            Game.Current.BoardHeight = (short)boardHeight;
            Game.Current.BoardWidth = (short)boardWidth;
            Game.Current.InitializeCellStates(initialCellStates);
        }

        public bool TrySetTankActions(TankActionSet actionSet, int timeoutInMilliseconds)
        {
            if (actionSet == null)
            {
                return true;
            }

            if (actionSet.Tick != Game.Current.CurrentTurn.Tick)
            {
                return false;
            }

            int playerIndex = actionSet.PlayerIndex;
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

            TankAction tankAction1 = actionSet.Actions[0];
            TankAction tankAction2 = actionSet.Actions[1];

            if (numberAlive == 1)
            {
                return TrySetAction(playerIndex, tankId, tankAction);
            }
            else
                if (numberAlive == 2)
                {
                    return TrySetActions(playerIndex, tankAction1, tankAction2);
                }
                else
                {
                    return true;
                }
        }

        #endregion
    }
}
