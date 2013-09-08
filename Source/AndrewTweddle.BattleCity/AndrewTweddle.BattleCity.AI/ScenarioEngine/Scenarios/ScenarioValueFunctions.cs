using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.MathFunctions;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public static class ScenarioValueFunctions
    {
        private const double valueOfATank = 500000; // five hundred thousand

        public static MathematicalFunction ClearRunAtBaseScenarioValueFunction { get; private set; }
        public static MathematicalFunction LockDownEnemyTankForOtherTankToDestroyValueFunction { get; private set; }
        public static MathematicalFunction AvoidBlockingFriendlyTankFunction { get; private set; }
        public static MathematicalFunction AvoidWalkingIntoABulletFunction { get; private set; }
        public static MathematicalFunction ShootBulletHeadOnFunction { get; private set; }
        public static MathematicalFunction DodgeBulletFunction { get; private set; }

        static ScenarioValueFunctions()
        {
            ClearRunAtBaseScenarioValueFunction
                = new ReverseLogisticFunction(leftAsymptoticX: -50, rightAsymptoticX: 30, minAsymptoticY: 0, maxAsymptoticY: 100000); // one hundred thousand
            LockDownEnemyTankForOtherTankToDestroyValueFunction
                = new ReverseLogisticFunction(leftAsymptoticX: -120, rightAsymptoticX: 120, minAsymptoticY: 0, maxAsymptoticY: 10000); // ten thousand
            AvoidBlockingFriendlyTankFunction
                = new TriangularFunction(startX: -1, modeX: 5, endX: 300, maxY: 1000, minY: 10);  // Ten to one thousand
                    // Note that the function falls off very slowly, meaning that there is a slight incentive to stay closer together
            AvoidWalkingIntoABulletFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 6, minAsymptoticY: 1, maxAsymptoticY: -valueOfATank);  // five hundred thousand
                    // slack is number of ticks until bullet collides
            ShootBulletHeadOnFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 3, minAsymptoticY: 0, maxAsymptoticY: valueOfATank);  // five hundred thousand
                    // slack is number of ticks until bullet collides
            DodgeBulletFunction
                = new ReverseLogisticFunction(leftAsymptoticX: -2, rightAsymptoticX: 2, minAsymptoticY: 0, maxAsymptoticY: valueOfATank);  // five hundred thousand
                    // slack is number of ticks till reaching a survival point less number of ticks until bullet collides
        }
    }
}
