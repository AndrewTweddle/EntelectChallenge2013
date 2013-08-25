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
using System.Threading.Tasks;
using AndrewTweddle.BattleCity.Core.Helpers;
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

        public int LoginAndGetYourPlayerIndex(ICommunicatorCallback callback)
        {
            ChallengeClient client = new ChallengeClient(EndPointConfigurationName, Url);
            client.Open();
            try
            {
                callback.DoBeforeLoggingIn();
                try
                {
                    // Login:
                    state?[][] states = client.login();

                    // Set up game board and end game sequence:
                    CellState[,] initialCellStates = ConvertGameBoard(ref states);
                    callback.InitializeGameBoard(initialCellStates);

                    int tickAtWhichGameEndSequenceBegins = 200; // TODO: Get from login() when this is added to it
                    callback.InitializeEndGameSequence(tickAtWhichGameEndSequenceBegins);

                    // Get status for the first time:
                    DateTime localTimeBeforeGetStatusCall = DateTime.Now;
                    game wsGame = client.getStatus();
                    DateTime localTimeAfterGetStatusCall = DateTime.Now;

                    callback.DoBeforeInitializingPlayersAndUnits(
                        wsGame.currentTick,
                        wsGame.nextTickTime,
                        TimeSpan.FromMilliseconds(wsGame.millisecondsToNextTick),
                        localTimeBeforeGetStatusCall,
                        localTimeAfterGetStatusCall);
                    try
                    {
                        int yourPlayerIndex = InitializePlayersAndUnitsAndGetYourPlayerIndex(wsGame, callback);
                        return yourPlayerIndex;
                    }
                    finally
                    {
                        callback.DoAfterInitializingPlayersAndUnits();
                    }
                }
                finally
                {
                    callback.DoBeforeReturningFromLogin();
                }
            }
            finally
            {
                client.Close();
            }
        }

        public void WaitForNextTick(int playerIndex, int currentTickOnClient, ICommunicatorCallback callback)
        {
            while (!TryGetNewGameState(playerIndex, currentTickOnClient, callback))
            {
                Thread.Sleep(StatePollInterval);
            }
        }

        public bool TryGetNewGameState(int playerIndex, int currentTickOnClient, ICommunicatorCallback callback)
        {
            ChallengeClient client = new ChallengeClient(EndPointConfigurationName, Url);
            client.Open();
            try
            {
                try
                {
                    callback.DoBeforeCheckingForANewState();
                    try
                    {
                        DateTime localTimeBeforeGetStatusCall = DateTime.Now;
                        game wsGame = client.getStatus();
                        DateTime localTimeAfterGetStatusCall = DateTime.Now;
                        int currentTick = wsGame.currentTick;

                        if (currentTick == currentTickOnClient)
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

                        callback.DoBeforeUpdatingTheState(wsGame.currentTick, wsGame.nextTickTime,
                            TimeSpan.FromMilliseconds(wsGame.millisecondsToNextTick),
                            localTimeBeforeGetStatusCall, localTimeAfterGetStatusCall);
                        bool stateUpdateCompletedSuccessfully = false;
                        try
                        {
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

                            callback.UpdateWalls(wallsRemovedAfterPreviousTick, outOfBoundsBlocksAfterPreviousTick);

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
                                        callback.UpdateTankState(u.id, tankAction, newPos, u.direction.Convert(), isActive: false);
                                    }

                                    bullet blt = unitEv.bullet;
                                    if (blt != null)
                                    {
                                        Point bulletPos = new Point((short)blt.x, (short)blt.y);
                                        callback.UpdateBulletState(blt.id, bulletPos, blt.direction.Convert(), isActive: false);
                                    }
                                }
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
                                            callback.UpdateTankState(u.id, tankAction, newPos, u.direction.Convert(), isActive: true);
                                        }
                                    }
                                }

                                if (plyr.bullets != null)
                                {
                                    foreach (bullet blt in plyr.bullets)
                                    {
                                        Point bulletPos = new Point((short)blt.x, (short)blt.y);
                                        callback.UpdateBulletState(blt.id, bulletPos, blt.direction.Convert(), isActive: true);
                                    }
                                }
                            }
                            stateUpdateCompletedSuccessfully = true;
                        }
                        finally
                        {
                            callback.DoAfterUpdatingTheState(stateUpdateCompletedSuccessfully);
                        }

                        return true;
                    }
                    finally
                    {
                        callback.DoAfterCheckingForANewState();
                    }
                }
                catch (FaultException<EndOfGameException> endOfGameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", endOfGameFault, endOfGameFault.Message);
                    callback.UpdateGameOutcomeToLoseDueToError(playerIndex, endOfGameFault.Message);
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
                catch (FaultException<NoBlameException> noBlameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", noBlameFault, noBlameFault.Message);
                    callback.UpdateGameOutcomeDueToNoBlameCrash(noBlameFault.Message);
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
                catch (EndpointNotFoundException endpointException)
                {
                    DebugHelper.LogDebugError("Web service adapter", endpointException, endpointException.Message);
                    callback.UpdateGameOutcomeDueToServerUnavailable(endpointException.Message);
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
                catch (CommunicationException commsException)
                {
                    DebugHelper.LogDebugError("Web service adapter", commsException, commsException.Message);
                    callback.UpdateGameOutcomeDueToServerUnavailable(commsException.Message);
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

        // TODO: honour the timeoutInMilliseconds setting
        public bool TrySetAction(int playerIndex, int tankId, TankAction tankAction, 
            ICommunicatorCallback callback, int timeoutInMilliseconds)
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
                    callback.UpdateGameOutcomeToLoseDueToError(playerIndex, endOfGameFault.Message);
                    return false;
                }
                catch (FaultException<NoBlameException> noBlameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", noBlameFault, noBlameFault.Message);
                    callback.UpdateGameOutcomeDueToNoBlameCrash(noBlameFault.Message);
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
                catch (EndpointNotFoundException endpointException)
                {
                    DebugHelper.LogDebugError("Web service adapter", endpointException, endpointException.Message);
                    callback.UpdateGameOutcomeDueToServerUnavailable(endpointException.Message);
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
                catch (CommunicationException commsException)
                {
                    DebugHelper.LogDebugError("Web service adapter", commsException, commsException.Message);
                    callback.UpdateGameOutcomeDueToServerUnavailable(commsException.Message);
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

        // TODO: honour the timeoutInMilliseconds setting
        public bool TrySetActions(int playerIndex, TankAction tankAction1, TankAction tankAction2, 
            ICommunicatorCallback callback, int timeoutInMilliseconds)
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
                    callback.UpdateGameOutcomeToLoseDueToError(playerIndex, endOfGameFault.Message);
                    return false;
                }
                catch (FaultException<NoBlameException> noBlameFault)
                {
                    DebugHelper.LogDebugError("Web service adapter", noBlameFault, noBlameFault.Message);
                    callback.UpdateGameOutcomeDueToNoBlameCrash(noBlameFault.Message);
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
                catch (EndpointNotFoundException endpointException)
                {
                    DebugHelper.LogDebugError("Web service adapter", endpointException, endpointException.Message);
                    callback.UpdateGameOutcomeDueToServerUnavailable(endpointException.Message);
#if DEBUG
                    throw;
#else
                    return false;
#endif
                }
                catch (CommunicationException commsException)
                {
                    DebugHelper.LogDebugError("Web service adapter", commsException, commsException.Message);
                    callback.UpdateGameOutcomeDueToServerUnavailable(commsException.Message);
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

        private static int InitializePlayersAndUnitsAndGetYourPlayerIndex(game wsGame, ICommunicatorCallback callback)
        {
            int yourPlayerIndex = -1;  // To keep the compiler happy

            for (int playerIndex = 0; playerIndex < wsGame.players.Length; playerIndex++)
            {
                player wsPlayer = wsGame.players[playerIndex];
                string playerName = wsPlayer.name;
                bool isYou = false;
                if (wsPlayer.name == playerName)
                {
                    yourPlayerIndex = playerIndex;
                    isYou = true;
                }
                Point basePos = new Point((short)wsPlayer.@base.x, (short)wsPlayer.@base.y);
                callback.InitializePlayer(playerIndex, playerName, basePos, isYou);

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

                    callback.InitializeTank(playerIndex, tankNumber, unitId, ref initialTankPosition, initialTankDirection, initialTankAction);
                }
            }
            return yourPlayerIndex;
        }

        private static CellState[,] ConvertGameBoard(ref state?[][] states)
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
            return initialCellStates;
        }

        #endregion
    }
}
