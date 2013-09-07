using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.MathFunctions;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public static class ScenarioValueFunctions
    {
        public static MathematicalFunction ClearRunAtBaseScenarioValueFunction { get; private set; }
        public static MathematicalFunction LockDownEnemyTankForOtherTankToDestroyValueFunction { get; private set; }

        static ScenarioValueFunctions()
        {
            ClearRunAtBaseScenarioValueFunction 
                = new ReverseLogisticFunction(leftAsymptoticX: -50, rightAsymptoticX: 30, minAsymptoticY: 0, maxAsymptoticY: 100000); // hundred thousand
            LockDownEnemyTankForOtherTankToDestroyValueFunction
                = new ReverseLogisticFunction(leftAsymptoticX: -120, rightAsymptoticX: 120, minAsymptoticY: 0, maxAsymptoticY: 10000); // ten thousand

        }
    }
}
