using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Engines;
using AndrewTweddle.BattleCity.Core.Actions;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.AI
{
    public class RemoteCommunicatorCallback: ICommunicatorCallback
    {
        #region Properties

        private GameState PrevGameState { get; set; }
        private GameState NewGameState { get; set; }
        private TankAction[] TankActionsTaken { get; set; }
        private bool[] AllMobileStatesAccountedFor { get; set; }

        #endregion

        #region ICommunicatorCallback interface

        #region Common callbacks

        public void UpdateGameOutcomeDueToNoBlameCrash(string reasonForCrash)
        {
            Game.Current.CurrentTurn.GameState.Outcome = Outcome.Crashed;
            Game.Current.CurrentTurn.ReasonForOutcome = reasonForCrash;
        }

        public void UpdateGameOutcomeToLoseDueToError(int losingPlayerIndex, string reasonForLoss)
        {
            Outcome outcome = losingPlayerIndex == 0 ? Outcome.Player2Win : Outcome.Player1Win;
            Game.Current.CurrentTurn.ReasonForOutcome = reasonForLoss;
            Game.Current.CurrentTurn.GameState.Outcome = Outcome.Crashed | outcome;
        }

        public void UpdateGameOutcomeDueToServerUnavailable(string errorMessage)
        {
            // TODO: Infer a winner, since this exception usually occurs because the game ended and the harness shut down...
            Game.Current.CurrentTurn.GameState.Outcome = Outcome.Crashed | Outcome.CompletedButUnknown;
            Game.Current.CurrentTurn.ReasonForOutcome = errorMessage;
        }

        #endregion

        
        #region Login callbacks

        public void DoBeforeLoggingIn()
        {
            Game.Current.LocalGameStartTime = DateTime.Now;
        }

        public void InitializeGameBoard(CellState[,] initialCellStates)
        {
            int boardWidth = initialCellStates.GetLength(0);
            int boardHeight = initialCellStates.GetLength(1);
            Game.Current.BoardHeight = (short)boardHeight;
            Game.Current.BoardWidth = (short)boardWidth;
            Game.Current.InitializeCellStates(initialCellStates);
        }

        public void InitializeEndGameSequence(int tickAtWhichGameEndSequenceBegins)
        {
            Game.Current.TickAtWhichGameEndSequenceBegins = tickAtWhichGameEndSequenceBegins;
            Game.Current.FinalTickInGame = tickAtWhichGameEndSequenceBegins - 1 + Game.Current.BoardWidth / 2;
            Game.Current.InitializeTurns();
        }

        public void DoBeforeInitializingPlayersAndUnits(
            int currentTick,
            DateTime nextTickTimeOnServer,
            TimeSpan timeUntilNextTick,
            DateTime localTimeBeforeGetStatusCall,
            DateTime localTimeAfterGetStatusCall)
        {
            UpdateCurrentTurn(currentTick, nextTickTimeOnServer, timeUntilNextTick, 
                localTimeBeforeGetStatusCall, localTimeAfterGetStatusCall);
        }

        public void InitializePlayer(int playerIndex, string playerName, Point basePos, bool isYou)
        {
            Player player = Game.Current.Players[playerIndex];
            player.Index = playerIndex;
            player.Name = playerName;

            // Set up the base:
            player.Base.Pos = basePos;
        }

        public void InitializeTank(int playerIndex, int tankNumber, int unitId,
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

        public void DoAfterInitializingPlayersAndUnits()
        {
            CheckTanks();
            Game.Current.InitializeElements();
        }

        public void DoBeforeReturningFromLogin()
        {
        }

        #endregion


        #region Callbacks when getting the latest state

        public void DoBeforeCheckingForANewState()
        {
        }

        public void DoBeforeReturningFromCheckingForANewState()
        {
        }

        public void DoBeforeUpdatingTheState(
            int currentTick,
            DateTime nextTickTimeOnServer,
            TimeSpan timeUntilNextTick,
            DateTime localTimeBeforeGetStatusCall,
            DateTime localTimeAfterGetStatusCall)
        {
            AllMobileStatesAccountedFor = new bool[Constants.MOBILE_ELEMENT_COUNT];

            TankActionsTaken = new TankAction[Constants.TANK_COUNT];
            for (int i = 0; i < TankActionsTaken.Length; i++)
            {
                TankActionsTaken[i] = TankAction.NONE;
            }

            PrevGameState = Game.Current.CurrentTurn.GameState;
            NewGameState = PrevGameState.Clone();
            NewGameState.Tick = currentTick;
            NewGameState.Outcome = Outcome.InProgress;

            // TODO: Ideally the current turn should be updated as the very last step, 
            // to prevent race conditions in scenarios where CurrentTurn is accessed before the game state is fully updated: 
            UpdateCurrentTurn(currentTick, nextTickTimeOnServer, timeUntilNextTick, localTimeBeforeGetStatusCall, localTimeAfterGetStatusCall);
        }

        public void UpdateWalls(
            IEnumerable<Point> wallsRemovedAfterPreviousTick, 
            IEnumerable<Point> outOfBoundsBlocksAfterPreviousTick)
        {
            foreach (Point wallPoint in wallsRemovedAfterPreviousTick)
            {
                NewGameState.Walls[wallPoint] = false;
            }
            NewGameState.WallsRemovedAfterPreviousTick = wallsRemovedAfterPreviousTick.ToArray();
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

        public void UpdateTankState(int unitId, TankAction tankAction, 
            Point newPos, Direction newDir, bool isActive)
        {
            bool tankFound = false;
            for (int t = 0; t < Constants.TANK_COUNT; t++)
            {
                Tank tank = Game.Current.Elements[t] as Tank;
                if (tank.Id == unitId)
                {
                    MobileState newMobileState = new MobileState(newPos, newDir, isActive);
                    NewGameState.SetMobileState(t, ref newMobileState);
                    TankActionsTaken[tank.Index] = tankAction;
                    tankFound = true;
                    AllMobileStatesAccountedFor[tank.Index] = true;
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

        public void UpdateBulletState(int bulletId, Point bulletPos, Direction bulletDir, bool isActive)
        {
            MobileState newMobileState = new MobileState(bulletPos, bulletDir, isActive);

            // Look for the bullet:
            int i = 0;
            bool bulletFound = false;
            for (int b = Constants.MIN_BULLET_INDEX; b <= Constants.MAX_BULLET_INDEX; b++)
            {
                if (Game.Current.CurrentTurn.BulletIds[i] == bulletId)
                {
                    NewGameState.SetMobileState(b, ref newMobileState);
                    bulletFound = true;
                    AllMobileStatesAccountedFor[b] = true;
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
                    MobileState tankState = NewGameState.GetMobileState(t);
                    if (tankState.Dir == bulletDir)
                    {
                        Point tankFiringPoint = tankState.GetTankFiringPoint();
                        if (tankFiringPoint == bulletPos)
                        {
                            Tank tank = Game.Current.Elements[t] as Tank;
                            int bulletIndex = tank.Bullet.Index;
                            NewGameState.SetMobileState(bulletIndex, ref newMobileState);
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

        public void DoAfterUpdatingTheState(bool stateUpdateCompletedSuccessfully)
        {
            // Find any mobile states unaccounted for, and remove them:
            for (int i = 0; i < AllMobileStatesAccountedFor.Length - 1; i++)
            {
                // Ignore if accounted for:
                if (AllMobileStatesAccountedFor[i])
                {
                    continue;
                }

                // Ignore if not active:
                MobileState mobiState = NewGameState.GetMobileState(i);
                if (!mobiState.IsActive)
                {
                    continue;
                }

                Element element = Game.Current.Elements[i];

                mobiState = mobiState.Kill();
                NewGameState.SetMobileState(i, ref mobiState);
                DebugHelper.LogDebugMessage("Remote Communicator Callback",
                    "Deactivating {0} {1} @{2}, which was not accounted for",
                    element.ElementType, i, mobiState.Pos);
            }

            // Record the actual actions taken by the players:
            Turn prevTurn = Game.Current.CurrentTurn.PreviousTurn;
            if (prevTurn != null)
            {
                prevTurn.TankActionsTakenAfterPreviousTurn = TankActionsTaken;
            }

            Game.Current.CurrentTurn.GameState = NewGameState;

            CheckGameState();

            // Clear the variables, so the callback can be used again without risk of debris from the previous time:
            TankActionsTaken = null;
            PrevGameState = null;
            NewGameState = null;
        }

        #endregion

        
        #region Callbacks when sending tank actions

        // None. The only ones used are common ones, such as checking for errors.

        #endregion

        #endregion

        
        #region Private Methods

        private void UpdateCurrentTurn(
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

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckTanks()
        {
            foreach (Player player in Game.Current.Players)
            {
                if (player.Tanks[0] == null || player.Tanks[1] == null)
                {
                    throw new ApplicationException("A player's tanks were not all initialized during initial setup");
                }
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckGameState()
        {
            GameState calculatedGameState = PrevGameState.Clone();
            calculatedGameState.Tick = Game.Current.CurrentTurn.Tick;
            if (calculatedGameState is MutableGameState)
            {
                MutableGameStateEngine.ApplyAllActions((MutableGameState)calculatedGameState, TankActionsTaken);
            }
            else
                if (calculatedGameState is ImmutableGameState)
                {
                    ImmutableGameStateEngine.ApplyActions((ImmutableGameState)calculatedGameState, TankActionsTaken);
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

            calculatedGameState.Outcome |= Outcome.InProgress;
            Game.Current.CurrentTurn.GameStateCalculatedByGameStateEngine = calculatedGameState;

            string reasonDifferent;
            if (!GameState.AreGameStatesEquivalent(NewGameState, calculatedGameState, out reasonDifferent))
            {
                throw new ApplicationException(
                    String.Format(
                        "The calculated game state does not match the new harness game state. \r\nReason: {0}",
                        reasonDifferent
                    )
                );
            }
        }

        #endregion
    }
}
