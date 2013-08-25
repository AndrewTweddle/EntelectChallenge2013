using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI
{
    public interface ICommunicatorCallback
    {
        #region Common callbacks

        void UpdateGameOutcomeDueToNoBlameCrash(string reasonForCrash);
        void UpdateGameOutcomeToLoseDueToError(int losingPlayerIndex, string reasonForLoss);
        void UpdateGameOutcomeDueToServerUnavailable(string errorMessage);

        #endregion

        #region Login callbacks 
        
        // These should be called in the following sequence:
        void DoBeforeLoggingIn();
        void InitializeGameBoard(CellState[,] initialCellStates);
        void InitializeEndGameSequence(int tickAtWhichGameEndSequenceBegins);
        void DoBeforeInitializingPlayersAndUnits(
            int currentTick,
            DateTime nextTickTimeOnServer,
            TimeSpan timeUntilNextTick,
            DateTime localTimeBeforeGetStatusCall,
            DateTime localTimeAfterGetStatusCall);
        void InitializePlayer(int playerIndex, string playerName, Point basePos, bool isYou);
        void InitializeTank(int playerIndex, int tankNumber, int unitId,
            ref Point initialTankPosition, Direction initialTankDirection, TankAction initialTankAction);
        void DoAfterInitializingPlayersAndUnits();
        void DoBeforeReturningFromLogin();

        #endregion

        void DoBeforeCheckingForANewState();
        void DoBeforeUpdatingTheState(int currentTick, DateTime nextTickTimeOnServer, 
            TimeSpan timeUntilNextTick, DateTime localTimeBeforeGetStatusCall, DateTime localTimeAfterGetStatusCall);
        void UpdateWalls(IEnumerable<Point> wallsRemovedAfterPreviousTick, IEnumerable<Point> outOfBoundsBlocksAfterPreviousTick);
        void UpdateTankState(int unitId, TankAction tankAction, Point newPos, Direction newDir, bool isActive);
        void UpdateBulletState(int bulletId, Point bulletPos, Direction bulletDir, bool isActive);
        void DoAfterUpdatingTheState(bool stateUpdateCompletedSuccessfully);

        /// <summary>
        /// This always occurs at the very end, regardless of whether a new state was found or not.
        /// </summary>
        void DoAfterCheckingForANewState();
    }
}
