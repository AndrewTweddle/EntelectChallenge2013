using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.Solvers;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Actions;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Calculations;
using AndrewTweddle.BattleCity.Core.Calculations.Firing;
using AndrewTweddle.BattleCity.Core.Calculations.Bullets;

namespace AndrewTweddle.BattleCity.Bots
{
    public class SmartBot<TGameState> : BaseSolver<TGameState>
        where TGameState : GameState<TGameState>, new()
    {
        public override string Name
        {
            get
            {
                return "SmartBot";
            }
        }

        protected override void ChooseMoves()
        {
            try
            {
                GameState currGameState = Game.Current.CurrentTurn.GameState;

                TankActionSet actionSet = new TankActionSet(YourPlayerIndex, currGameState.Tick);
                bool[] moveChosenByTankNumber = new bool[Constants.TANKS_PER_PLAYER];

                RespondToBullets(currGameState, actionSet, moveChosenByTankNumber);
                ChooseActions(currGameState, actionSet, moveChosenByTankNumber);
                Coordinator.SetBestMoveSoFar(actionSet);
                return;
            }
            catch (Exception exc)
            {
                // Swallow errors
            }

            // Shortest Path Bot code:
            ChooseShortestPathBotMoves();
        }

        private void RespondToBullets(GameState currGameState, TankActionSet actionSet, bool[] moveChosenByTankNumber)
        {
            BulletCalculation bulletCalc = BulletCalculator.GetBulletCalculation(currGameState, You);
            foreach (BulletPathCalculation bulletPathCalc in bulletCalc.BulletPaths)
            {
                BulletThreat[] bulletThreats = bulletPathCalc.BulletThreats;
                for (int i = 0; i < bulletThreats.Length; i++)
                {
                    BulletThreat bulletThreat = bulletThreats[i];
                    if (bulletThreat.TankThreatened.Player == You && !moveChosenByTankNumber[bulletThreat.TankThreatened.Number])
                    {
                        if ((bulletPathCalc.BaseThreatened == You.Base)
                            && (bulletThreat.NodePathToTakeOnBullet != null)
                            && (bulletThreat.NodePathToTakeOnBullet.Length > 0))
                        {
                            actionSet.Actions[bulletThreat.TankThreatened.Number]
                                = bulletThreat.TankActionsToTakeOnBullet[0];
                            moveChosenByTankNumber[bulletThreat.TankThreatened.Number] = true;
                            continue;
                        }

                        if (bulletThreat.LateralMoveInOneDirection != null
                            && bulletThreat.LateralMoveInOneDirection.Length > 0)
                        {
                            actionSet.Actions[bulletThreat.TankThreatened.Number]
                                = bulletThreat.TankActionsForLateralMoveInOneDirection[0];
                            moveChosenByTankNumber[bulletThreat.TankThreatened.Number] = true;
                            continue;
                        }

                        if (bulletThreat.LateralMoveInOtherDirection != null
                            && bulletThreat.LateralMoveInOtherDirection.Length > 0)
                        {
                            actionSet.Actions[bulletThreat.TankThreatened.Number]
                                = bulletThreat.TankActionsForLateralMoveInOtherDirection[0];
                            moveChosenByTankNumber[bulletThreat.TankThreatened.Number] = true;
                            continue;
                        }

                        if ((bulletThreat.NodePathToTakeOnBullet != null)
                            && (bulletThreat.NodePathToTakeOnBullet.Length > 0))
                        {
                            actionSet.Actions[bulletThreat.TankThreatened.Number]
                                = bulletThreat.TankActionsToTakeOnBullet[0];
                            moveChosenByTankNumber[bulletThreat.TankThreatened.Number] = true;
                            continue;
                        }
                    }
                }
            }
        }

        public void ChooseActions(GameState currGameState, TankActionSet actionSet, bool[] moveChosenByTankNumber)
        {
            int[] minDistanceToAttackEnemyBaseByPlayerIndex = new int[Constants.PLAYERS_PER_GAME];
            int[] minDistanceToDefendOwnBaseByPlayerIndex = new int[Constants.PLAYERS_PER_GAME];
            bool[] isUnmarked = new bool[Constants.PLAYERS_PER_GAME];
            Direction[] attackDirOnEnemyBaseByPlayerIndex = new Direction[Constants.PLAYERS_PER_GAME];
            for (int p = 0; p < 2; p++)
            {
                attackDirOnEnemyBaseByPlayerIndex[p] = Direction.NONE;
            }
            Tank closestFriendlyTankToAttackEnemyBase = null;
            Tank closestFriendlyTankToDefendOwnBase = null;
            Tank closestEnemyTankToAttackYourBase = null;
            TankAction[] tankActionsForClosestFriendlyTankToAttackEnemyBase = null;
            TankAction[] tankActionsForClosestFriendlyTankToDefendOwnBase = null;

            foreach (Player player in Game.Current.Players)
            {
                int minDistanceToEnemyBase = Constants.UNREACHABLE_DISTANCE;

                foreach (Tank tank in player.Tanks)
                {
                    MobileState tankState = currGameState.GetMobileState(tank.Index);
                    if (!tankState.IsActive)  // TODO: Check if locked in a firefight also
                    {
                        continue;
                    }

                    DirectionalMatrix<DistanceCalculation> distanceCalcMatrix
                        = currGameState.CalculationCache.GetIncomingDistanceMatrixForBase(1 - player.Index);
                    DistanceCalculation baseAttackCalc = distanceCalcMatrix[tankState];

                    int distanceToEnemyBase = baseAttackCalc.Distance;
                    if (distanceToEnemyBase < minDistanceToEnemyBase)
                    {
                        minDistanceToEnemyBase = distanceToEnemyBase;
                        attackDirOnEnemyBaseByPlayerIndex[player.Index] = baseAttackCalc.AdjacentNode.Dir;
                        if (player == You)
                        {
                            closestFriendlyTankToAttackEnemyBase = tank;
                            Base enemyBase = Game.Current.Players[1 - You.Index].Base;
                            FiringLineMatrix firingLineMatrix = currGameState.CalculationCache.FiringLinesForPointsMatrix;
                            tankActionsForClosestFriendlyTankToAttackEnemyBase = PathCalculator.GetTankActionsOnIncomingShortestPath(
                                distanceCalcMatrix, tankState, enemyBase.Pos, firingLineMatrix, true);
                        }
                        else
                        {
                            closestEnemyTankToAttackYourBase = tank;
                        }
                    }
                }
                minDistanceToAttackEnemyBaseByPlayerIndex[player.Index] = minDistanceToEnemyBase;
            }

            foreach (Player player in Game.Current.Players)
            {
                int minDistanceToDefendOwnBase = Constants.UNREACHABLE_DISTANCE;

                foreach (Tank tank in player.Tanks)
                {
                    MobileState tankState = currGameState.GetMobileState(tank.Index);
                    if (tankState.IsActive)  // TODO: Check if locked in a firefight also
                    {
                        DirectionalMatrix<DistanceCalculation> distanceMatrix
                            = currGameState.CalculationCache.GetDistanceMatrixFromTankByTankIndex(tank.Index);
                        Direction attackDir = attackDirOnEnemyBaseByPlayerIndex[1 - player.Index];
                        Direction defenceDir = attackDir.GetOpposite();
                        if (defenceDir == Direction.NONE)
                        {
                            defenceDir = Direction.RIGHT;
                        }
                        Point defencePos = player.Base.Pos + (Constants.SEGMENT_SIZE + 1) * defenceDir.GetOffset();
                        DistanceCalculation tankDefenceCalc = distanceMatrix[
                            defenceDir, defencePos];

                        int distance = tankDefenceCalc.Distance;
                        if (distance < minDistanceToDefendOwnBase)
                        {
                            minDistanceToDefendOwnBase = distance;
                            if (player == You)
                            {
                                closestFriendlyTankToDefendOwnBase = tank;
                                tankActionsForClosestFriendlyTankToDefendOwnBase = PathCalculator.GetTankActionsOnOutgoingShortestPath(
                                    distanceMatrix, defenceDir, defencePos.X, defencePos.Y);
                            }
                        }
                    }
                }
                minDistanceToDefendOwnBaseByPlayerIndex[player.Index] = minDistanceToDefendOwnBase;
                if (minDistanceToAttackEnemyBaseByPlayerIndex[1 - player.Index] < minDistanceToDefendOwnBase)
                {
                    isUnmarked[1 - player.Index] = true;
                }
            }

            /*
            int minAttackDistance = minDistanceToAttackEnemyBaseByPlayerIndex[You.Index];
            if (Game.Current.CurrentTurn.Tick + minAttackDistance > Game.Current.FinalTickInGame)
            {
                // Can't attack in time, rather defend...
                return;
            }
            */

            // You're unmarked...
            Tank attackTank = closestFriendlyTankToAttackEnemyBase;
            if (isUnmarked[You.Index] && attackTank != null)
            {
                bool shouldAttack = true;

                // Enemy is marked...
                if (!isUnmarked[1 - You.Index])
                {
                    int minAttackDistance = minDistanceToAttackEnemyBaseByPlayerIndex[You.Index];
                    int minEnemyAttackDistance = minDistanceToAttackEnemyBaseByPlayerIndex[1 - You.Index];

                    // But if you stop marking them, by going for their base, they can get there first...
                    if ((minAttackDistance > minEnemyAttackDistance) 
                        && (closestFriendlyTankToDefendOwnBase == attackTank))
                    {
                        shouldAttack = false;
                        // TODO: See if your other tank can do defence duty instead...
                    }
                }

                if (shouldAttack && !moveChosenByTankNumber[attackTank.Number])
                {
                    actionSet.Actions[attackTank.Number] = tankActionsForClosestFriendlyTankToAttackEnemyBase[0];
                    moveChosenByTankNumber[attackTank.Number] = true;
                }
            }

            // If there is a slack of less than 5 ticks to defend your base, get back to defend it:
            Tank defenceTank = closestFriendlyTankToDefendOwnBase;
            if ((defenceTank != null)
                && (minDistanceToAttackEnemyBaseByPlayerIndex[1 - You.Index] < minDistanceToDefendOwnBaseByPlayerIndex[You.Index] + 5))
            {
                if (!moveChosenByTankNumber[defenceTank.Number])
                {
                    actionSet.Actions[defenceTank.Number] = tankActionsForClosestFriendlyTankToAttackEnemyBase[0];
                    moveChosenByTankNumber[defenceTank.Number] = true;
                }
            }

            // Otherwise choose an enemy tank to attack:
            for (int t = 0; t < Constants.TANKS_PER_PLAYER; t++)
            {
                if ((closestEnemyTankToAttackYourBase != null) && (!moveChosenByTankNumber[t]))
                {
                    // Rather attack an enemy tank in a firefight with own tank..
                    Tank killerTank = You.Tanks[t];
                    AttackClosestEnemyTankToOwnBase(currGameState, killerTank,
                        closestEnemyTankToAttackYourBase, actionSet, 
                        moveChosenByTankNumber);
                }
            }
        }

        public void AttackClosestEnemyTankToOwnBase(GameState currGameState, Tank killerTank, Tank targetTank,
            TankActionSet actionSet, bool[] moveChosenByTankNumber)
        {
            MobileState killerTankState = currGameState.GetMobileState(killerTank.Index);
            if (killerTankState.IsActive)
            {
                MobileState enemyTankState = currGameState.GetMobileState(targetTank.Index);
                if (enemyTankState.IsActive)
                {
                    DirectionalMatrix<DistanceCalculation> attackMatrix
                        = currGameState.CalculationCache.GetIncomingAttackMatrixForTankByTankIndex(targetTank.Index);
                    if (attackMatrix != null)
                    {
                        DistanceCalculation attackCalculation = attackMatrix[killerTankState.Dir, killerTankState.Pos];
                        TankAction[] tankActions 
                            = PathCalculator.GetTankActionsOnIncomingShortestPath(attackMatrix, killerTankState, enemyTankState.Pos, 
                                currGameState.CalculationCache.FiringLinesForTanksMatrix, keepMovingCloserOnFiringLastBullet: true);
                        if (tankActions.Length > 0)
                        {
                            actionSet.Actions[killerTank.Number] = tankActions[0];
                            moveChosenByTankNumber[killerTank.Number] = true;
                        }
                    }
                }
            }
        }

        private void ChooseShortestPathBotMoves()
        {
            GameState currGameState = Game.Current.CurrentTurn.GameState;
            TankActionSet actionSet = new TankActionSet(YourPlayerIndex, currGameState.Tick);
            bool[] moveChosen = new bool[Constants.TANKS_PER_PLAYER];

            RespondToBullets(currGameState, actionSet, moveChosen);

            Tuple<int, int, TankAction>[] tankNumberDistanceAndActionArray = new Tuple<int, int, TankAction>[Constants.TANKS_PER_PLAYER];

            // The closest bot can attack the base:
            for (int tankNumber = 0; tankNumber < Constants.TANKS_PER_PLAYER; tankNumber++)
            {
                Tank tank = You.Tanks[tankNumber];
                MobileState tankState = currGameState.GetMobileState(tank.Index);
                if (!tankState.IsActive)
                {
                    tankNumberDistanceAndActionArray[tankNumber]
                        = new Tuple<int, int, TankAction>(tankNumber, Constants.UNREACHABLE_DISTANCE, TankAction.NONE);
                    continue;
                }

                Base enemyBase = Opponent.Base;
                DirectionalMatrix<DistanceCalculation> distancesToEnemyBase
                    = currGameState.CalculationCache.GetIncomingDistanceMatrixForBase(Opponent.Index);
                DistanceCalculation distanceCalc = distancesToEnemyBase[tankState.Dir, tankState.Pos];

                FiringLineMatrix firingLineMatrix = currGameState.CalculationCache.FiringLinesForPointsMatrix;
                TankAction[] tankActions = PathCalculator.GetTankActionsOnIncomingShortestPath(distancesToEnemyBase,
                    tankState.Dir, tankState.Pos.X, tankState.Pos.Y, enemyBase.Pos.X, enemyBase.Pos.Y,
                    firingLineMatrix, keepMovingCloserOnFiringLastBullet: false);

                if (tankActions.Length == 0)
                {
                    tankNumberDistanceAndActionArray[tankNumber]
                        = new Tuple<int, int, TankAction>(tankNumber, Constants.UNREACHABLE_DISTANCE, TankAction.NONE);
                }
                else
                {
                    tankNumberDistanceAndActionArray[tankNumber]
                        = new Tuple<int, int, TankAction>(tankNumber, distanceCalc.Distance, tankActions[0]);
                }
            }

            int chosenTank;
            if (tankNumberDistanceAndActionArray[1].Item2 < tankNumberDistanceAndActionArray[0].Item2)
            {
                chosenTank = 1;
            }
            else
            {
                chosenTank = 0;
            }
            actionSet.Actions[chosenTank] = tankNumberDistanceAndActionArray[chosenTank].Item3;

            // The other bot can attack the closest enemy tank:
            Tank killerTank = You.Tanks[1 - chosenTank];
            MobileState killerTankState = currGameState.GetMobileState(killerTank.Index);
            if (killerTankState.IsActive)
            {
                Tank enemyTank1 = Opponent.Tanks[0];
                MobileState enemyTankState1 = currGameState.GetMobileState(enemyTank1.Index);

                int killerDistance1 = Constants.UNREACHABLE_DISTANCE;
                TankAction killerAction1 = TankAction.NONE;

                if (enemyTankState1.IsActive)
                {
                    DirectionalMatrix<DistanceCalculation> attackMatrix1
                        = currGameState.CalculationCache.GetIncomingAttackMatrixForTankByTankIndex(enemyTank1.Index);
                    if (attackMatrix1 != null)
                    {
                        DistanceCalculation attackCalculation1 = attackMatrix1[killerTankState.Dir, killerTankState.Pos];
                        killerDistance1 = attackCalculation1.Distance;
                        TankAction[] tankActions
                            = PathCalculator.GetTankActionsOnIncomingShortestPath(attackMatrix1, killerTankState, enemyTankState1.Pos,
                                currGameState.CalculationCache.FiringLinesForTanksMatrix, keepMovingCloserOnFiringLastBullet: true);
                        if (tankActions.Length > 0)
                        {
                            killerAction1 = tankActions[0];
                        }
                    }
                }

                Tank enemyTank2 = Opponent.Tanks[1];
                MobileState enemyTankState2 = currGameState.GetMobileState(enemyTank2.Index);

                int killerDistance2 = Constants.UNREACHABLE_DISTANCE;
                TankAction killerAction2 = TankAction.NONE;

                if (enemyTankState2.IsActive)
                {
                    DirectionalMatrix<DistanceCalculation> attackMatrix2
                        = currGameState.CalculationCache.GetIncomingAttackMatrixForTankByTankIndex(enemyTank2.Index);
                    if (attackMatrix2 != null)
                    {
                        DistanceCalculation attackCalculation2 = attackMatrix2[killerTankState.Dir, killerTankState.Pos];
                        killerDistance2 = attackCalculation2.Distance;
                        TankAction[] tankActions
                            = PathCalculator.GetTankActionsOnIncomingShortestPath(attackMatrix2, killerTankState, enemyTankState2.Pos,
                                currGameState.CalculationCache.FiringLinesForTanksMatrix, keepMovingCloserOnFiringLastBullet: true);
                        if (tankActions.Length > 0)
                        {
                            killerAction2 = tankActions[0];
                        }
                    }
                }

                actionSet.Actions[1 - chosenTank]
                    = (killerDistance1 <= killerDistance2)
                    ? killerAction1
                    : killerAction2;
            }

            Coordinator.SetBestMoveSoFar(actionSet);
        }
    }
}
