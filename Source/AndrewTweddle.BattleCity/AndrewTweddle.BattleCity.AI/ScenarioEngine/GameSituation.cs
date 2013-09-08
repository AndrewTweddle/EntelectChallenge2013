using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Calculations.Bullets;
using AndrewTweddle.BattleCity.Core.Actions;
using AndrewTweddle.BattleCity.Core.Helpers;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public class GameSituation
    {
        public TankSituation[] TankSituationsByTankIndex { get; private set; }

        public GameSituation()
        {
            TankSituationsByTankIndex = new TankSituation[Constants.TANK_COUNT];
        }

        public void UpdateSituation(GameState gameState)
        {
            for (int t = 0; t < Constants.TANK_COUNT; t++)
            {
                Tank tank = Game.Current.Elements[t] as Tank;
                TankSituation tankSituation = new TankSituation(this);
                tankSituation.Tank = tank;
                tankSituation.TankState = gameState.GetMobileState(t);

                // tankSituation.IsShotAt
                // tankSituation.ThreateningBullets
                // tankSituation.IsInLineOfFire
                // tankSituation.IsLockedDown
                // tankSituation.IsShutIntoQuadrant
                // tankSituation.Quadrant

                TankSituationsByTankIndex[tank.Index] = tankSituation;
                tankSituation.UpdateTankActionSituations(gameState);
            }
        }

        public TankSituation GetTankSituationByPlayerAndTankNumber(int playerIndex, int tankNumber)
        {
            int tankIndex = Game.Current.Players[playerIndex].Tanks[tankNumber].Index;
            return TankSituationsByTankIndex[tankIndex];
        }

        public TankActionSet GenerateTankActions(int yourPlayerIndex, int tick)
        {
            TankActionSet tankActionSet = new TankActionSet(yourPlayerIndex, tick);

            foreach (Tank tank in Game.Current.Players[yourPlayerIndex].Tanks)
            {
                TankSituation tankSituation = TankSituationsByTankIndex[tank.Index];
                TankAction bestAction = tankSituation.GetBestTankAction();
                tankActionSet.Actions[tank.Number] = bestAction;
            }

            return tankActionSet;
        }

        public bool AreAllTankActionsGenerated(int yourPlayerIndex)
        {
            foreach (Tank tank in Game.Current.Players[yourPlayerIndex].Tanks)
            {
                TankSituation tankSituation = TankSituationsByTankIndex[tank.Index];
                if (!tankSituation.IsTankActionDetermined)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
