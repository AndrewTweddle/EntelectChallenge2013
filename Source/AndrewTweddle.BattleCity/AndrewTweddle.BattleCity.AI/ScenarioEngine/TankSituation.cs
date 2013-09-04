using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Calculations.Bullets;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public class TankSituation
    {
        private const int TANK_ACTION_COUNT = 6;

        public Tank Tank { get; set; }
        public MobileState TankState { get; set; }

        public bool IsTankActionDetermined { get; set; }
        public TankAction ChosenTankAction { get; set; }

        public double[] TankActionValues { get; set; }

        public bool IsShotAt { get; set; }
        public BulletThreat[] BulletThreats { get; set; }

        public bool IsInLineOfFire { get; set; }
        public bool IsLockedDown { get; set; }
        public bool IsShutIntoQuadrant { get; set; }
        public Rectangle Quadrant { get; set; }

        public TankSituation()
        {
            TankActionValues = new double[TANK_ACTION_COUNT];
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
            TankActionValues[(int)tankAction] += valueModification;
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
                    double actionValue = TankActionValues[(int)tankAction];
                    if (actionValue > bestValue)
                    {
                        bestValue = actionValue;
                        bestAction = tankAction;
                    }
                }
            }
            return bestAction;
        }
    }
}
