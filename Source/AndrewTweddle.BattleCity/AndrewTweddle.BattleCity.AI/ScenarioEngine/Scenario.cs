using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.AI.ScenarioEngine.MoveGenerators;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;
using AndrewTweddle.BattleCity.Core.Calculations;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public abstract class Scenario
    {
        public const int MAX_POINTS_TO_TRY_FOR_DEFENCE_POS = 6;

        public static TankAction[] NoTankActions = new TankAction[0];

        public GameState GameState { get; set; }
        public GameSituation GameSituation { get; set; }

        public Scenario(GameState gameState, GameSituation gameSituation)
        {
            GameState = gameState;
            GameSituation = gameSituation;
        }

        public virtual string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        #region Abstract Methods

        public abstract MoveGenerator[] GetMoveGeneratorsByMoveTreeLevel();
        public abstract MoveResult EvaluateLeafNodeMove(Move move);
        public abstract void ChooseMovesAsP(MoveResult moveResult);
        public abstract void ChooseMovesAsPBar(MoveResult moveResult);

        #endregion

        #region Utility methods

        public MobileState GetTankState_i(Move move)
        {
            return GetTankState(move.p, move.i);
        }

        public MobileState GetTankState_iBar(Move move)
        {
            return GetTankState(move.p, move.iBar);
        }

        public MobileState GetTankState_j(Move move)
        {
            return GetTankState(move.pBar, move.j);
        }

        public MobileState GetTankState_jBar(Move move)
        {
            return GetTankState(move.pBar, move.jBar);
        }

        public TankSituation GetTankSituation(int playerIndex, int tankNumber)
        {
            return GameSituation.GetTankSituationByPlayerAndTankNumber(playerIndex, tankNumber);
        }

        public MobileState GetTankState(int playerIndex, int tankNumber)
        {
            Tank tank = Game.Current.Players[playerIndex].Tanks[tankNumber];
            return GameState.GetMobileState(tank.Index);
        }

        public Base GetFriendlyBase(int playerIndex)
        {
            return Game.Current.Players[playerIndex].Base;
        }

        public Base GetEnemyBase(int playerIndex)
        {
            return Game.Current.Players[1 - playerIndex].Base;
        }

        public bool AreAllTanksAlive()
        {
            return AreAllTanksForPlayerStillAlive(0) && AreAllTanksForPlayerStillAlive(1);
        }

        public bool AreAllTanksForPlayerStillAlive(int playerIndex)
        {
            return GetTankState(playerIndex, 0).IsActive && GetTankState(playerIndex, 1).IsActive;
        }

        public static Direction GetCentralLineOfFireAttackDirection(int playerIndex)
        {
            Direction centralAttackDir = Direction.NONE;
            foreach (Direction incomingAttackDir in Game.Current.Players[playerIndex].Base.GetPossibleIncomingAttackDirections())
            {
                if (incomingAttackDir == Direction.DOWN || incomingAttackDir == Direction.UP)
                {
                    centralAttackDir = incomingAttackDir;
                    break;
                }
            }
            return centralAttackDir;
        }

        public int GetAttackDistanceOfTankToEnemyBase(int playerIndex, int tankNumber)
        {
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)
            {
                DirectionalMatrix<DistanceCalculation> attackDistanceMatrix
                    = GameState.CalculationCache.GetIncomingDistanceMatrixForBase(1 - playerIndex);
                return attackDistanceMatrix[tankState].Distance;
            }
            return Constants.UNREACHABLE_DISTANCE;
        }

        /* TODO: Allow easy comparison across actions...
        public int[] GetAttackDistanceOfTankToEnemyBasePerTankAction(int playerIndex, int tankNumber)
        {
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            int[] attackDistancesByTankAction = new int[Constants.TANK_ACTION_COUNT];
            foreach (TankAction tankAction in TankHelper.TankActions)
            {
                if (tankState.IsActive)
                {


                    DirectionalMatrix<DistanceCalculation> attackDistanceMatrix
                        = GameState.CalculationCache.GetIncomingDistanceMatrixForBase(1 - playerIndex);
                    return attackDistanceMatrix[tankState].Distance;
                }
                return Constants.UNREACHABLE_DISTANCE;
            }
            return attackDistancesByTankAction;
        }
         */

        public TankAction[] GetActionsToAttackEnemyBase(int playerIndex, int tankNumber)
        {
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)
            {
                Base enemyBase = GetEnemyBase(playerIndex);
                FiringLineMatrix firingLineMatrix = GameState.CalculationCache.FiringLinesForPointsMatrix;
                DirectionalMatrix<DistanceCalculation> attackDistanceMatrix
                    = GameState.CalculationCache.GetIncomingDistanceMatrixForBase(1 - playerIndex);
                return PathCalculator.GetTankActionsOnIncomingShortestPath(attackDistanceMatrix, tankState,
                    enemyBase.Pos, firingLineMatrix, keepMovingCloserOnFiringLastBullet: true);
            }
            return NoTankActions;
        }

        public int GetAttackDistanceOfTankToEnemyBaseFromDirection(int playerIndex, int tankNumber, 
            Direction finalDirectionOfMovement)
        {
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)
            {
                DirectionalMatrix<DistanceCalculation> attackDistanceMatrix
                    = GameState.CalculationCache.GetIncomingDistanceMatrixForBaseWithFinalDirectionOfMovement(
                        1 - playerIndex, finalDirectionOfMovement);
                return attackDistanceMatrix[tankState].Distance;
            }
            return Constants.UNREACHABLE_DISTANCE;
        }

        public TankAction[] GetActionsToAttackEnemyBaseFromDirection(
            int playerIndex, int tankNumber, Direction finalDirectionOfMovement)
        {
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)
            {
                Base enemyBase = GetEnemyBase(playerIndex);
                FiringLineMatrix firingLineMatrix = GameState.CalculationCache.FiringLinesForPointsMatrix;
                DirectionalMatrix<DistanceCalculation> attackDistanceMatrix
                    = GameState.CalculationCache.GetIncomingDistanceMatrixForBaseWithFinalDirectionOfMovement(
                        1 - playerIndex, finalDirectionOfMovement);
                return PathCalculator.GetTankActionsOnIncomingShortestPath(attackDistanceMatrix,
                    tankState, enemyBase.Pos, firingLineMatrix, keepMovingCloserOnFiringLastBullet: true);
            }
            return NoTankActions;
        }

        public int GetLineOfFireDefenceDistanceToHomeBaseByIncomingAttackDirection(int playerIndex, int tankNumber, 
            Direction finalIncomingDirectionOfAttack)
        {
            Player player = Game.Current.Players[playerIndex];
            Tank tank = player.Tanks[tankNumber];
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)  // TODO: Check if locked in a firefight also
            {
                DirectionalMatrix<DistanceCalculation> distanceMatrix
                    = GameState.CalculationCache.GetDistanceMatrixFromTankByTankIndex(tank.Index);
                Direction defenceDir = finalIncomingDirectionOfAttack.GetOpposite();
                if (defenceDir == Direction.NONE)
                {
                    defenceDir = Direction.RIGHT;
                }
                int startPosOffsetFromBase = Constants.TANK_OUTER_EDGE_OFFSET;
                int endPosOffsetFromBase = startPosOffsetFromBase + MAX_POINTS_TO_TRY_FOR_DEFENCE_POS;
                for (int offsetSize = startPosOffsetFromBase; offsetSize < endPosOffsetFromBase; offsetSize++)
                {
                    Point defencePos = player.Base.Pos + defenceDir.GetOffset(offsetSize);
                    DistanceCalculation tankDefenceCalc = distanceMatrix[defenceDir, defencePos];
                    if (tankDefenceCalc.CodedDistance == 0)
                    {
                        // This defence point can't be reached, try further away
                        continue;
                    }
                    return tankDefenceCalc.Distance;
                }
            }
            return Constants.UNREACHABLE_DISTANCE;
        }

        public int GetCentralLineOfFireDefenceDistanceToHomeBase(int playerIndex, int tankNumber)
        {
            Direction centralAttackDir = GetCentralLineOfFireAttackDirection(playerIndex);
            return GetLineOfFireDefenceDistanceToHomeBaseByIncomingAttackDirection(playerIndex, tankNumber, centralAttackDir);
        }

        public TankAction[] GetActionsToReachLineOfFireDefencePointByIncomingAttackDirection(
            int playerIndex, int tankNumber, Direction finalIncomingDirectionOfAttack)
        {
            Player player = Game.Current.Players[playerIndex];
            Tank tank = player.Tanks[tankNumber];
            MobileState tankState = GetTankState(playerIndex, tankNumber);
            if (tankState.IsActive)  // TODO: Check if locked in a firefight also
            {
                DirectionalMatrix<DistanceCalculation> distanceMatrix
                    = GameState.CalculationCache.GetDistanceMatrixFromTankByTankIndex(tank.Index);
                Direction defenceDir = finalIncomingDirectionOfAttack.GetOpposite();
                if (defenceDir == Direction.NONE)
                {
                    defenceDir = Direction.RIGHT;
                }
                int startPosOffsetFromBase = Constants.TANK_OUTER_EDGE_OFFSET;
                int endPosOffsetFromBase = startPosOffsetFromBase + MAX_POINTS_TO_TRY_FOR_DEFENCE_POS;
                for (int offsetSize = startPosOffsetFromBase; offsetSize < endPosOffsetFromBase; offsetSize++)
                {
                    Point defencePos = player.Base.Pos + defenceDir.GetOffset(offsetSize);
                    DistanceCalculation tankDefenceCalc = distanceMatrix[defenceDir, defencePos];
                    if (tankDefenceCalc.CodedDistance == 0)
                    {
                        // This defence point can't be reached, try further away
                        continue;
                    }
                    return PathCalculator.GetTankActionsOnOutgoingShortestPath(distanceMatrix, defenceDir, defencePos);
                }
            }
            return new TankAction[0];
        }

        public int GetDistanceFromTankToTargetTank(
            int attackPlayerIndex, int attackTankNumber, int targetPlayerIndex, int targetTankNumber)
        {
            Tank attackTank = Game.Current.Players[attackPlayerIndex].Tanks[attackTankNumber];
            DirectionalMatrix<DistanceCalculation> distanceMatrix 
                = GameState.CalculationCache.GetDistanceMatrixFromTankByTankIndex(attackTank.Index);
            
            Tank defenceTank = Game.Current.Players[targetPlayerIndex].Tanks[targetTankNumber];
            MobileState targetTankState = GetTankState(targetPlayerIndex, targetTankNumber);
            DistanceCalculation distanceCalc = distanceMatrix[targetTankState];
            return distanceCalc.Distance;
        }

        public int GetDistanceFromTankToPoint(int playerIndex, int tankNumber, MobileState targetState)
        {
            return GetDistanceFromTankToPoint(playerIndex, tankNumber, targetState.Dir, targetState.Pos);
        }

        public int GetDistanceFromTankToPoint(int playerIndex, int tankNumber,
            Direction directionAtDestination, Point destination)
        {
            Tank tank = Game.Current.Players[playerIndex].Tanks[tankNumber];
            DirectionalMatrix<DistanceCalculation> distanceMatrix
                = GameState.CalculationCache.GetDistanceMatrixFromTankByTankIndex(tank.Index);
            DistanceCalculation distanceCalc = distanceMatrix[directionAtDestination, destination];
            return distanceCalc.Distance;
        }

        public TankAction[] GetTankActionsToMoveToPoint(int playerIndex, int tankNumber,
            Direction directionAtDestination, Point destination)
        {
            Tank tank = Game.Current.Players[playerIndex].Tanks[tankNumber];
            DirectionalMatrix<DistanceCalculation> distanceMatrix
                = GameState.CalculationCache.GetDistanceMatrixFromTankByTankIndex(tank.Index);
            return PathCalculator.GetTankActionsOnOutgoingShortestPath(distanceMatrix, directionAtDestination, destination);
        }

        public DirectionalMatrix<DistanceCalculation> GetCustomDistanceMatrixFromTank(
            int playerIndex, int tankNumber, int ticksWithoutFiring, Rectangle restrictedBoardArea)
        {
            Tank tank = Game.Current.Players[playerIndex].Tanks[tankNumber];
            TurnCalculationCache turnCalcCache = Game.Current.Turns[GameState.Tick].CalculationCache;

            // Don't ride over your own base!
            Base @base = tank.Player.Base;
            TankLocation tankLoc = turnCalcCache.TankLocationMatrix[@base.Pos];
            Rectangle[] tabooAreas = new Rectangle[] { tankLoc.TankBody };

            DistanceCalculator distanceCalculator = new DistanceCalculator();
            distanceCalculator.Walls = GameState.Walls;
            distanceCalculator.TankOuterEdgeMatrix = GameState.CalculationCache.TankOuterEdgeMatrix;
            distanceCalculator.CellMatrix = turnCalcCache.CellMatrix;
            distanceCalculator.TabooAreas = tabooAreas;
            distanceCalculator.TicksWithoutFiring = ticksWithoutFiring;
            distanceCalculator.RestrictedMovementArea = restrictedBoardArea;
            MobileState tankState = GameState.GetMobileState(tank.Index);
            DirectionalMatrix<DistanceCalculation> distanceMatrix 
                = distanceCalculator.CalculateShortestDistancesFromTank(ref tankState);
            return distanceMatrix;
        }

        public int GetDistanceFromTankToPointUsingDistanceMatrix(
            DirectionalMatrix<DistanceCalculation> distanceMatrix,
            Direction directionAtDestination, Point destination)
        {
            DistanceCalculation distanceCalc = distanceMatrix[directionAtDestination, destination];
            return distanceCalc.Distance;
        }

        public TankAction[] GetTankActionsToMoveToPointUsingCustomDistanceMatrix(
            DirectionalMatrix<DistanceCalculation> distanceMatrix,
            Direction directionAtDestination, Point destination)
        {
            return PathCalculator.GetTankActionsOnOutgoingShortestPath(distanceMatrix, directionAtDestination, destination);
        }

        public int GetAttackDistanceFromTankToTankAtPointAlongDirectionOfMovement(
            int playerIndex, int tankNumber, Point targetPoint, Direction finalMovementDir,
            EdgeOffset[] edgeOffsets)
        {
            Tank tank = Game.Current.Players[playerIndex].Tanks[tankNumber];
            MobileState tankState = GameState.GetMobileState(tank.Index);
            TurnCalculationCache turnCalcCache = Game.Current.Turns[GameState.Tick].CalculationCache;
            Cell targetCell = turnCalcCache.CellMatrix[targetPoint];
            FiringLineMatrix firingLinesForTanksMatrix = GameState.CalculationCache.FiringLinesForTanksMatrix;
            AttackTargetDistanceCalculator attackCalculator = new AttackTargetDistanceCalculator(
                ElementType.TANK, firingLinesForTanksMatrix, GameState.CalculationCache, turnCalcCache);
            attackCalculator.MovementDirections = new Direction[] { finalMovementDir };
            attackCalculator.EdgeOffsets = edgeOffsets;
            CombinedMovementAndFiringDistanceCalculation combinedDistCalc
                = attackCalculator.GetShortestAttackDistanceFromCurrentTankPosition(tank.Index,
                    targetCell);
            return combinedDistCalc.TicksTillTargetShot;
            /* was:
            DirectionalMatrix<DistanceCalculation> incomingDistanceMatrix
                = attackCalculator.CalculateMatrixOfShortestDistancesToTargetCell(targetCell);
            DistanceCalculation distanceCalc = incomingDistanceMatrix[tankState];
            return distanceCalc.Distance;
             */
        }

        public TankAction[] GetTankActionsFromTankToAttackTankAtPointAlongDirectionOfMovement(
            int playerIndex, int tankNumber, Point targetPoint, Direction finalMovementDir,
            EdgeOffset[] edgeOffsets, bool keepMovingCloserOnFiringLastBullet)
        {
            Tank tank = Game.Current.Players[playerIndex].Tanks[tankNumber];
            MobileState tankState = GameState.GetMobileState(tank.Index);
            TurnCalculationCache turnCalcCache = Game.Current.Turns[GameState.Tick].CalculationCache;
            Cell targetCell = turnCalcCache.CellMatrix[targetPoint];
            FiringLineMatrix firingLinesForTanksMatrix = GameState.CalculationCache.FiringLinesForTanksMatrix;
            AttackTargetDistanceCalculator attackCalculator = new AttackTargetDistanceCalculator(
                ElementType.TANK, firingLinesForTanksMatrix, GameState.CalculationCache, turnCalcCache);
            attackCalculator.MovementDirections = new Direction[] { finalMovementDir };
            attackCalculator.EdgeOffsets = edgeOffsets;
            CombinedMovementAndFiringDistanceCalculation combinedDistCalc
                = attackCalculator.GetShortestAttackDistanceFromCurrentTankPosition(tank.Index,
                    targetCell);
            return PathCalculator.GetTanksActionsOnOutgoingShortestAttackPathFromCurrentTankPosition(
                tank.Index, combinedDistCalc, GameState.CalculationCache, keepMovingCloserOnFiringLastBullet);
            /* was:
            DirectionalMatrix<DistanceCalculation> incomingDistanceMatrix
                = attackCalculator.CalculateMatrixOfShortestDistancesToTargetCell(targetCell);
            DistanceCalculation distanceCalc = incomingDistanceMatrix[tankState];
            return PathCalculator.GetTankActionsOnIncomingShortestPath(incomingDistanceMatrix, tankState.Dir, tankState.Pos.X, tankState.Pos.Y,
                targetPoint.X, targetPoint.Y, firingLinesForTanksMatrix, keepMovingCloserOnFiringLastBullet);
             */
        }

        #endregion

        #region Debugging

        [System.Diagnostics.Conditional("DEBUG")]
        public void LogDebugMessage(string format, params object[] args)
        {
            DebugHelper.LogDebugMessage(Name, format, args);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public void LogDebugError(Exception exc, string message = "")
        {
            DebugHelper.LogDebugError(Name, exc, message);
        }

        #endregion
    }
}
