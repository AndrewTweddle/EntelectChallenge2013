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

namespace AndrewTweddle.BattleCity.Comms.Client
{
    public class WebServiceAdapter: ICommunicator
    {
        #region Constants

        private const int DEFAULT_POLL_INTERVAL_IN_MILLISECONDS = 50;

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

        public void Login()
        {
            ChallengeClient client = new ChallengeClient(EndPointConfigurationName, Url);
            client.Open();
            try
            {
                state?[][] states = client.login();
                InitializeGameBoard(ref states);
                game wsGame = client.getStatus();
                InitializePlayersAndUnits(wsGame);
                Game.Current.CurrentTick = wsGame.currentTick;
                if (wsGame.nextTickTimeSpecified)
                {
                    Game.Current.NextServerTickTime = wsGame.nextTickTime;
                }
            }
            finally
            {
                client.Close();
            }
        }

        public void WaitForNextTick(GameState gameStateToUpdate)
        {
            while (!TryGetNewGameState(gameStateToUpdate))
            {
                Thread.Sleep(StatePollInterval);
            }
        }

        public bool TryGetNewGameState(GameState gameStateToUpdate)
        {
            ChallengeClient client = new ChallengeClient(EndPointConfigurationName, Url);
            client.Open();
            try
            {
                try
                {
                    game wsGame = client.getStatus();
                    if (wsGame.currentTick > gameStateToUpdate.Tick)
                    {
                        // TODO: Resolve overlap in responsibilities between Game, Coordinator and GameState classes
                        Game.Current.CurrentTick = wsGame.currentTick;
                        if (wsGame.nextTickTimeSpecified)
                        {
                            Game.Current.NextServerTickTime = wsGame.nextTickTime;
                        }

                        //  Remove any walls that have been shot:
                        events evts = wsGame.events;
                        foreach (blockEvent blockEv in evts.blockEvents)
                        {
                            if (blockEv.newStateSpecified && (blockEv.newState == state.EMPTY || blockEv.newState == state.NONE))
                            {
                                gameStateToUpdate.Walls[blockEv.point.Convert()] = false;
                            }
                        }

                        // Update states of tanks and bullets:
                        foreach (unitEvent unitEv in evts.unitEvents)
                        {
                            if (unitEv.unit != null)
                            {
                                unit u = unitEv.unit;
                                for (int t = 0; t < Constants.TANK_COUNT; t++)
                                {
                                    Tank tank = Game.Current.Elements[t] as Tank;
                                    if (tank.Id == u.id)
                                    {
                                        Point newPos = new Point((short)u.x, (short)u.y);
                                        MobileState newMobileState = new MobileState(newPos, u.direction.Convert(), isActive: true);
                                        gameStateToUpdate.SetMobileState(t, ref newMobileState);
                                    }
                                }
                            }

                            if (unitEv.bullet != null)
                            {
                                bullet blt = unitEv.bullet;
                                Direction bulletDir = blt.direction.Convert();
                                Point bulletPos = new Point((short) blt.x, (short) blt.y);
                                MobileState newMobileState = new MobileState(bulletPos, bulletDir, isActive:true);

                                // Look for the bullet:
                                int i = 0;
                                bool bulletFound = false;
                                for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
                                {
                                    if (Game.Current.BulletIds[i] == blt.id)
                                    {
                                        gameStateToUpdate.SetMobileState(b, ref newMobileState);
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
                                        MobileState tankState = gameStateToUpdate.GetMobileState(t);
                                        if (tankState.Dir == bulletDir)
                                        {
                                            Point tankFiringPoint = tankState.GetTankFiringPoint();
                                            if (tankFiringPoint == bulletPos)
                                            {
                                                Tank tank = Game.Current.Elements[t] as Tank;
                                                int bulletIndex = tank.Bullet.Index;
                                                gameStateToUpdate.SetMobileState(bulletIndex, ref newMobileState);
                                                Game.Current.BulletIds[t] = blt.id;
                                                bulletFound = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (!bulletFound)
                                {
                                    throw new ApplicationException("A new bullet could not be found");
                                }
                            }
                        }

                        // TODO: How do we determine when a tank or bullet has been destroyed?

                        return true;
                    }
                    return false;
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

        public void SetTankActions(GameState currentGameState, TankActionSet actionSet)
        {
            ChallengeClient client = new ChallengeClient(EndPointConfigurationName, Url);
            client.Open();
            try
            {
                if (actionSet.Tick == Game.Current.CurrentTick)
                {
                    for (int t = 0; t < Constants.TANKS_PER_PLAYER; t++)
                    {
                        Tank tank = Game.Current.You.Tanks[t];
                        MobileState tankState = currentGameState.GetMobileState(tank.Index);
                        if (tankState.IsActive)
                        {
                            TankAction tankAction = actionSet.Actions[t];
                            global::action wsAction = tankAction.Convert();
                            client.setAction(tank.Id, wsAction);
                        }
                    }
                }
            }
            finally
            {
                client.Close();
            }
        }

        #endregion


        #region Private Game Initialization Methods

        private static void InitializePlayersAndUnits(game wsGame)
        {
            for (int p = 0; p < wsGame.players.Length; p++)
            {
                bool isYou = false;
                player wsPlayer = wsGame.players[p];
                Player player = Game.Current.Players[p];
                player.Index = p;
                player.Name = wsPlayer.name;
                if (wsPlayer.name == wsGame.playerName)
                {
                    Game.Current.YourPlayerIndex = p;
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
                    var state = states[y][x];
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
