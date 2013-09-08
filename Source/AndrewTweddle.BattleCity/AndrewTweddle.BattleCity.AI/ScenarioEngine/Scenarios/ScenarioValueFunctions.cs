using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.MathFunctions;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public static class ScenarioValueFunctions
    {
        private const double valueOfATank = 50000; // fifty thousand

        public static MathematicalFunction ClearRunAtBaseScenarioValueFunction { get; private set; }
        public static MathematicalFunction LockDownEnemyTankForOtherTankToDestroyValueFunction { get; private set; }
        public static MathematicalFunction AvoidBlockingFriendlyTankFunction { get; private set; }
        public static MathematicalFunction AvoidWalkingIntoABulletFunction { get; private set; }
        public static MathematicalFunction ShootBulletHeadOnFunction { get; private set; }
        public static MathematicalFunction DodgeBulletFunction { get; private set; }
        public static MathematicalFunction ProlongEnemyDisarmamentFunction { get; private set; }

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
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 6, minAsymptoticY: 1, maxAsymptoticY: -valueOfATank);  // fifty thousand
                    // slack is number of ticks until bullet collides, 6 ticks is enough time to cross the path of the bullet
            ShootBulletHeadOnFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 1, minAsymptoticY: 0, maxAsymptoticY: valueOfATank);  // fifty thousand
                    // slack is number of ticks until bullet collides
            DodgeBulletFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 1, minAsymptoticY: 0, maxAsymptoticY: valueOfATank);  // fifty thousand
                    // slack is number of ticks till reaching a survival point less number of ticks until bullet collides
            ProlongEnemyDisarmamentFunction
                = new TriangularFunction(startX: -500, modeX: -81, endX: 0, maxY: 30000); // thirty thousand
                    // use this to fake a linear decline, rather than the logistic function decline
                    // was: ReverseLogisticFunction(leftAsymptoticX: -80, rightAsymptoticX: 0, minAsymptoticY: 0, maxAsymptoticY: 30000);
        }
    }
}
