using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.Core.States
{
    public class CellDirectionalState
    {
        #region Private Member Variables

        private DistanceCalculation[] attackingDistancesToBaseByPlayerId;

        #endregion

        #region Public Properties

        public CellState CellState { get; private set; }
        public Direction Direction { get; private set; }

        public MobileState MobileState
        {
            get
            {
                return new MobileState(CellState.Position, Direction, isActive: true);
            }
        }

        public DistanceCalculation[] AttackingDistancesToBaseByPlayerId 
        {
            get
            {
                if (attackingDistancesToBaseByPlayerId == null)
                {
                    attackingDistancesToBaseByPlayerId = new DistanceCalculation[Constants.PLAYERS_PER_GAME];
                    for (int p = 0; p < Constants.PLAYERS_PER_GAME; p++)
                    {
                        attackingDistancesToBaseByPlayerId[(int) Direction] 
                            = CellState.GameState.CalculationCache.GetIncomingDistanceMatrixForBase(p)[Direction, CellState.Position];
                    }
                }
                return AttackingDistancesToBaseByPlayerId;
            }
        }

        public int AttackingDistanceToYourBase
        {
            get
            {
                return AttackingDistancesToBaseByPlayerId[CellState.GameState.YourPlayerIndex].Distance;
            }
        }

        public int AttackingDistanceToOpponentsBase
        {
            get
            {
                return AttackingDistancesToBaseByPlayerId[CellState.GameState.OpponentsPlayerIndex].Distance;
            }
        }

        public int AttackingDistanceToBase0
        {
            get
            {
                return AttackingDistancesToBaseByPlayerId[0].Distance;
            }
        }

        public int AttackingDistanceToBase1
        {
            get
            {
                return AttackingDistancesToBaseByPlayerId[1].Distance;
            }
        }

        public int AttackingDistanceToEnemyTank0
        {
            get
            {
                Tank tank = CellState.GameState.Opponent.Tanks[0];
                return GetAttackingDistanceToTank(tank);
            }
        }

        public int AttackingDistanceToEnemyTank1
        {
            get
            {
                Tank tank = CellState.GameState.Opponent.Tanks[1];
                return GetAttackingDistanceToTank(tank);
            }
        }

        public int AttackingDistanceToFriendlyTank0
        {
            get
            {
                Tank tank = CellState.GameState.You.Tanks[0];
                return GetAttackingDistanceToTank(tank);
            }
        }

        public int AttackingDistanceToFriendlyTank1
        {
            get
            {
                Tank tank = CellState.GameState.You.Tanks[1];
                return GetAttackingDistanceToTank(tank);
            }
        }

        #endregion


        #region Constructors

        protected CellDirectionalState()
        {
        }

        public CellDirectionalState(CellState cellState, Direction direction)
        {
            CellState = cellState;
            Direction = direction;

            // attackingDistancesToBaseByPlayerId = new DistanceCalculation[Constants.PLAYERS_PER_GAME];
        }

        #endregion

        #region public methods

        public int GetAttackingDistanceToTank(Tank tank)
        {
            DirectionalMatrix<DistanceCalculation> distanceCalcs
                = CellState.GameState.CalculationCache.GetIncomingAttackMatrixForTankByTankIndex(tank.Index);
            return distanceCalcs[MobileState].Distance;
        }

        #endregion
    }
}
