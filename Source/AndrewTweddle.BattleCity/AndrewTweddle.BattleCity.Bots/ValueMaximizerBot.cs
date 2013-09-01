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
using AndrewTweddle.BattleCity.Core.Engines;
using AndrewTweddle.BattleCity.AI.ValueStrategy;

namespace AndrewTweddle.BattleCity.Bots
{
    public class ValueMaximizerBot<TGameState> : BaseSolver<TGameState>
        where TGameState : GameState<TGameState>, new()
    {
        public override string Name
        {
            get
            {
                return "Value maximizer";
            }
        }

        protected override void ChooseMoves()
        {
            GameState currGameState = Game.Current.CurrentTurn.GameState;
            TankAction[] tankActionsForLiveTank = new TankAction[]
            {
                TankAction.UP,
                TankAction.DOWN,
                TankAction.FIRE,
                TankAction.LEFT,
                TankAction.RIGHT,
                TankAction.NONE
            };
            TankAction[] tankActionsForDeadTank = new TankAction[]
            {
                TankAction.NONE
            };

            TankAction[][] tankActionsPerTank = new TankAction[Constants.TANK_COUNT][];

            foreach (Tank tank in You.Tanks)
            {
                MobileState mobileState = currGameState.GetMobileState(tank.Index);
                if (mobileState.IsActive)
                {
                    // Check if needing to avoid a bullet or shoot a bullet on the path to the base or the tank itself
                    // If so, limit the actions to only those that allow avoiding the bullet

                    tankActionsPerTank[tank.Number] = tankActionsForLiveTank;
                }
                else
                {
                    tankActionsPerTank[tank.Number] = tankActionsForDeadTank;
                }
            }

            double[,] valueEstimates = new double[tankActionsPerTank[0].Length, tankActionsPerTank[1].Length];
            GameStateValueEstimator valueEstimator = new GameStateValueEstimator();
            valueEstimator.Player = You;

            Tank tank0 = You.Tanks[0];
            Tank tank1 = You.Tanks[1];
            double bestValue = double.NegativeInfinity;
            TankAction[] bestTankActions = null;

            for (int ta0 = 0; ta0 < tankActionsPerTank[0].Length; ta0++)
            {
                TankAction tankAction0 = tankActionsPerTank[0][ta0];

                for (int ta1 = 0; ta1 < Constants.TANK_COUNT; ta1++)
                {
                    TankAction tankAction1 = tankActionsPerTank[1][ta1];

                    GameState gameStateClone = currGameState.Clone();
                    gameStateClone.Tick++;

                    TankAction[] tankActions = new TankAction[Constants.TANK_COUNT];
                    tankActions[tank0.Index] = tankAction0;
                    tankActions[tank1.Index] = tankAction1;
                    MutableGameStateEngine.ApplyAllActions(gameStateClone as MutableGameState, tankActions);
                    valueEstimator.GameState = gameStateClone;
                    double value = valueEstimator.Evaluate();
                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestTankActions = tankActions;
                    }
                }
            }
            TankActionSet actionSet = new TankActionSet(YourPlayerIndex, currGameState.Tick);
            if (bestTankActions != null)
            {
                actionSet.Actions[tank0.Number] = bestTankActions[tank0.Number];
                actionSet.Actions[tank1.Number] = bestTankActions[tank1.Number];
            }

            Coordinator.SetBestMoveSoFar(actionSet);
        }
    }
}
