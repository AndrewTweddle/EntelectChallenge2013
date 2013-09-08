using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Calculations.Bullets;
using AndrewTweddle.BattleCity.Core.Helpers;
using AndrewTweddle.BattleCity.Core.Engines;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public class TankSituation
    {
        public GameSituation GameSituation { get; private set; }
        public Tank Tank { get; set; }
        public MobileState TankState { get; set; }

        public bool IsTankActionDetermined { get; set; }
        public TankAction ChosenTankAction { get; set; }

        public bool IsShotAt { get; set; }
        public BulletThreat[] BulletThreats { get; set; }

        public BulletSituation TanksBulletSituation { get; set; }
        public int ExpectedNextTickWhenTankCanFireAgain 
        {
            get
            {
                if (TanksBulletSituation == null)
                {
                    return GameSituation.GameState.Tick;
                }
                return TanksBulletSituation.TickWhenTankCanFireAgain;
            }
        }

        public bool IsInLineOfFire { get; set; }
        public bool IsLockedDown { get; set; }
        public bool IsShutIntoQuadrant { get; set; }
        public Rectangle Quadrant { get; set; }

        /// <summary>
        /// This is the new game state if only this tank moves.
        /// It is useful for getting the relative values of various tank moves.
        /// </summary>
        public TankActionSituation[] TankActionSituationsPerTankAction { get; private set; }

        public TankSituation(GameSituation gameSituation)
        {
            GameSituation = gameSituation;
            TankActionSituationsPerTankAction = new TankActionSituation[Constants.TANK_ACTION_COUNT];
        }

        public void UpdateTankActionSituations(GameState currentGameState)
        {
            foreach (TankAction tankAction in TankHelper.TankActions)
            {
                GameState newGameState = currentGameState.Clone();
                newGameState.Tick++;

                TankActionSituation tankActSit = new TankActionSituation(this, tankAction);
                tankActSit.UpdateTankActionSituation(this, tankAction, newGameState);
                TankActionSituationsPerTankAction[(int)tankAction] = tankActSit;
            }
        }

        public void UpdateBulletSituation(GameState gameState)
        {
            BulletCalculation bulletCalc = BulletCalculator.GetBulletCalculation(gameState);
            List<BulletThreat> bulletThreats = new List<BulletThreat>();
            foreach (BulletPathCalculation bulletPathCalc in bulletCalc.BulletPaths)
            {
                foreach (BulletThreat bulletThreat in bulletPathCalc.BulletThreats)
                {
                    if (bulletThreat.TankThreatened == Tank)
                    {
                        bulletThreats.Add(bulletThreat);

                        // If base is attacked, tank will automatically defend it:
                        // 
                        // TODO: Will this kill some scenarios, because it applies to the scenario, 
                        // but doesn't seem to through its predetermined move?
                        // If so, do this in a separate scenario.
                        if ((bulletPathCalc.BaseThreatened == Tank.Player.Base)
                            && (bulletThreat.NodePathToTakeOnBullet != null)
                            && (bulletThreat.NodePathToTakeOnBullet.Length > 0))
                        {
                            ChosenTankAction = bulletThreat.TankActionsToTakeOnBullet[0];
                            IsTankActionDetermined = true;
                            continue;
                        }
                    }
                }
            }

            BulletThreats = bulletThreats.ToArray();
            IsShotAt = BulletThreats.Length > 0;
        }

        public void ChooseTankAction(TankAction tankAction)
        {
            if (IsTankActionDetermined)
            {
                if (tankAction != ChosenTankAction)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "The tank with index {0}) already has a chosen action of {1}. This can't be changed to {2}.",
                            Tank.Index, ChosenTankAction, tankAction));
                }
                return;
            }
            ChosenTankAction = tankAction;
            IsTankActionDetermined = true;
        }

        public void AdjustTankActionValue(TankAction tankAction, double valueModification)
        {
            TankActionSituation tankActSit = TankActionSituationsPerTankAction[(int)tankAction];
            tankActSit.Value += valueModification;
        }

        public TankAction GetBestTankAction()
        {
            TankAction bestAction = TankAction.NONE;
            if (IsTankActionDetermined)
            {
                bestAction = ChosenTankAction;
            }
            else
            {
                double bestValue = double.NegativeInfinity;

                foreach (TankAction tankAction in TankHelper.TankActions)
                {
                    TankActionSituation tankActSit = TankActionSituationsPerTankAction[(int)tankAction];
                    if (tankActSit.IsValid)
                    {
                        double actionValue = tankActSit.Value;
                        if (actionValue > bestValue)
                        {
                            bestValue = actionValue;
                            bestAction = tankAction;
                        }
                    }
                }
            }
            return bestAction;
        }
    }
}
