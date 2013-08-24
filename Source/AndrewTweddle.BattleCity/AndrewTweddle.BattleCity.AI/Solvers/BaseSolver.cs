using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.Solvers
{
    public abstract class BaseSolver<TGameState> : ISolver<TGameState>, INotifyPropertyChanged
        where TGameState: GameState<TGameState>, new()
    {
        #region Constants and lock objects

        // Lock timeouts in milliseconds - at all costs avoid a deadlock which could cause the solver to time out...
        private const int SOLVER_STATE_LOCK_TIMEOUT = 10;
        private const int SOLVER_STOP_CHOOSING_MOVES_LOCK_TIMEOUT = 100;

        private object solverStateLock = new object();
        private object solverStopChoosingMovesLock = new object();

        #endregion

        
        #region Private Member Variables

        private SolverState solverState;

        #endregion


        #region Virtual And Abstract Methods

        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// It is the responsibility of the ChooseMoves() implementation 
        /// to monitor SolverState for the states: StoppingChoosingMoves, Stopping or Stopped.
        /// The method should return as soon as one of these states is found.
        /// </summary>
        protected abstract void ChooseMoves();

        /// <summary>
        /// It is the responsibility of the Think() implementation
        /// to monitor SolverState for the states: CanChooseMoves, Stopping or Stopped.
        /// The method should return as soon as one of these states is found.
        /// It can return sooner.
        /// </summary>
        protected virtual void Think()
        {
        }

        #endregion


        #region Public Properties

        public Coordinator<TGameState> Coordinator { get; set; }
        public int YourPlayerIndex { get; set; }

        public Player You
        {
            get
            {
                return Game.Current.Players[YourPlayerIndex];
            }
        }

        public Player Opponent
        {
            get
            {
                return Game.Current.Players[1 - YourPlayerIndex];
            }
        }

        public virtual string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        public SolverState SolverState
        {
            get
            {
                return solverState;
            }
            set
            {
                bool isSolverStateLocked = Monitor.TryEnter(solverStateLock, SOLVER_STATE_LOCK_TIMEOUT);
                try
                {
#if DEBUG
                    if (!isSolverStateLocked)
                    {
                        System.Diagnostics.Debug.WriteLine("Lock timeout with solver state lock");
                    }
#endif
                    if (solverState == value)
                    {
                        return;
                    }

                    // Prevent a transition out of a stopping or stopped state:
                    if (solverState == SolverState.Stopping && value != SolverState.Stopped)
                    {
                        return;
                    }

                    if (solverState == SolverState.Stopped)
                    {
                        return;
                    }

                    /* Prevent a race condition which could cause a transition 
                     * to a WaitingToChooseMoves state if already in a CanChooseMoves state:
                     */
                    if (solverState == SolverState.CanChooseMoves && value == SolverState.WaitingToChooseMoves)
                    {
                        return;
                    }

                    // Once in solver shut-down, don't allow the state to be changed back:
                    if ((solverState != SolverState.Stopping)
                        && (value != SolverState.Stopped)
                        && (SolverState != SolverState.Stopped)
                        && (solverState != value))
                    {
                        solverState = value;
                        OnPropertyChanged("SolverState");
                    }
                }
                finally
                {
                    if (isSolverStateLocked)
                    {
                        Monitor.Exit(solverStateLock);
                    }
                }
            }
        }

        #endregion


        #region Public events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region Public Methods

        public virtual void Start()
        {
            if (Coordinator == null)
            {
                throw new ApplicationException("The solver has no coordinator");
            }

            Initialize();

            while (SolverState != SolverState.Stopping)
            {
                // Wait for the solver state to change:
                while ((SolverState != SolverState.CanChooseMoves)
                    && (SolverState != SolverState.Stopping))
                {
                    Thread.Sleep(10);
                }

                SolverState = SolverState.ChoosingMoves;
                try
                {
                    ChooseMoves();
                }
                catch (Exception exc)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error while choosing moves: {0}", exc);
                    System.Diagnostics.Debug.WriteLine("Stack trace:");
                    System.Diagnostics.Debug.WriteLine(exc.StackTrace);
                }

                if (SolverState == SolverState.Stopping)
                {
                    break;
                }

                SolverState = SolverState.Thinking;
                try
                {
                    Think();
                    // If this method returns due to finishing its work, then the next state can be set to WaitingToChooseMoves.
                    // Otherwise it must return when the state is changed to CanChooseMoves or Stopping.
                }
                catch(Exception exc)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "Error while thinking: {0}", exc);
                    System.Diagnostics.Debug.WriteLine("Stack trace:");
                    System.Diagnostics.Debug.WriteLine(exc.StackTrace);
                }

                if (SolverState == SolverState.Stopping)
                {
                    break;
                }

                if (SolverState != SolverState.CanChooseMoves)
                {
                    SolverState = SolverState.WaitingToChooseMoves;
                    // The SolverState setter will ignore this if in a CanChooseMoves state, 
                    // thus preventing a nasty race condition that would lead to an endless loop.
                }
            }
            SolverState = SolverState.Stopped;
        }

        public void StartChoosingMoves()
        {
            SolverState = SolverState.CanChooseMoves;
        }

        public virtual void StopChoosingMoves()
        {
            bool isSolverStopTurnLocked = Monitor.TryEnter(solverStopChoosingMovesLock, SOLVER_STOP_CHOOSING_MOVES_LOCK_TIMEOUT);
            try
            {
#if DEBUG
                if (!isSolverStopTurnLocked)
                {
                    System.Diagnostics.Debug.WriteLine("Lock timeout with 'solver stop choosing turns' locked");
                }
#endif
                if (SolverState == SolverState.ChoosingMoves)
                {
                    SolverState = SolverState.StoppingChoosingMoves;
                }
            }
            finally
            {
                if (isSolverStopTurnLocked)
                {
                    Monitor.Exit(solverStopChoosingMovesLock);
                }
            }
        }

        public void Stop()
        {
            SolverState = SolverState.Stopping;
        }

        #endregion

        
        #region Protected and Private Methods

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        #endregion

    }
}
