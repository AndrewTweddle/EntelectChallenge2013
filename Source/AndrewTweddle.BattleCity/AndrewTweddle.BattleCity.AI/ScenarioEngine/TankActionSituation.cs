using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Engines;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public class TankActionSituation
    {
        public TankActionSituation(TankSituation tankSituation, TankAction tankAction)
        {
            TankSituation = tankSituation;
            TankAction = tankAction;
        }

        public TankSituation TankSituation { get; set; }
        public TankAction TankAction { get; set; }
        public GameState NewGameState { get; set; }
        public MobileState NewTankState { get; set; }
        public Point[] WallsRemoved { get; set; }
        public bool IsAdjacentWallRemoved { get; set; }
        public bool IsValid { get; set; }
        public double Value { get; set; }

        public void UpdateTankActionSituation(TankSituation tankSituation, TankAction tankAction, GameState newGameState)
        {
            TankAction[] tankActions = new TankAction[] { TankAction.NONE, TankAction.NONE, TankAction.NONE, TankAction.NONE };
            tankActions[tankSituation.Tank.Index] = tankAction;
            List<Point> wallsRemoved = new List<Point>();
            MutableGameStateEngine.ApplyAllActions(newGameState as MutableGameState, tankActions, wallsRemoved);
            bool isAdjacentWallRemoved = false;

            if (!tankSituation.TankState.IsActive)
            {
                IsValid = (TankAction == TankAction.NONE);
                return;
            }
            else
            {
                IsValid = true;
            }

            if (tankAction == TankAction.FIRE)
            {
                Direction movementDir = tankSituation.TankState.Dir;
                TankLocation tankLoc
                    = Game.Current.Turns[newGameState.Tick].CalculationCache.TankLocationMatrix[tankSituation.TankState.Pos];
                Point adjacentWallPoint = tankLoc.OutsideEdgesByDirection[(int)movementDir].CentreCell.Position;
                isAdjacentWallRemoved = wallsRemoved.Any(
                    wallPoint => wallPoint == adjacentWallPoint
                );
            }

            NewGameState = newGameState;
            WallsRemoved = wallsRemoved.ToArray();
            IsAdjacentWallRemoved = isAdjacentWallRemoved;
            NewTankState = NewGameState.GetMobileState(TankSituation.Tank.Index);
        }


    }
}
