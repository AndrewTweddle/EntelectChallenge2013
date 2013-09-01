using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.AI.SchedulingEngine;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.AI.SchedulingEngine.BulletEvents;

namespace AndrewTweddle.BattleCity.AI.Solvers
{
    public class ScriptSolver: BaseSolver<MutableGameState>
    {
        public MasterSchedule MasterSchedule { get; private set; }

        protected override void ChooseMoves()
        {
            GameState gameState = Game.Current.CurrentTurn.GameState;
            UpdateMasterSchedule(gameState);
        }

        private void UpdateMasterSchedule(GameState gameState)
        {
            if (MasterSchedule == null)
            {
                MasterSchedule = new MasterSchedule();

                

                // Generate options per element:


            }
            else
            {
                // Remove old events

                // Update events which occured in the last tick/s

                // This may lead to some schedules being invalidated, or requiring re-choosing...

            }
        }
    }
}
