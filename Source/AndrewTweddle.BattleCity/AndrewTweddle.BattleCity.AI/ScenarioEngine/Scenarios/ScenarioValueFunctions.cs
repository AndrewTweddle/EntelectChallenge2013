using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.MathFunctions;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public static class ScenarioValueFunctions
    {
        public const double VALUE_OF_A_TANK = 50000; // fifty thousand  

        public static MathematicalFunction ClearRunAtBaseScenarioValueFunction { get; private set; }
        public static MathematicalFunction LockDownEnemyTankForOtherTankToDestroyValueFunction { get; private set; }
        public static MathematicalFunction AvoidBlockingFriendlyTankFunction { get; private set; }
        public static MathematicalFunction AvoidWalkingIntoABulletFunction { get; private set; }
        public static MathematicalFunction ShootBulletHeadOnFunction { get; private set; }
        public static MathematicalFunction DodgeBulletFunction { get; private set; }
        public static MathematicalFunction ProlongEnemyDisarmamentFunction { get; private set; }
        public static MathematicalFunction AttackEnemyBaseFunction { get; private set; }
        public static MathematicalFunction AttackEnemyBaseAttackActionFunction { get; private set; }
        public static MathematicalFunction GrappleWithEnemyTankAttackDiffFunction { get; private set; }
        public static MathematicalFunction GrappleWithEnemyTankAttackFunction { get; private set; }
        public static MathematicalFunction GrappleWithEnemyTankAttackActionFunction { get; private set; }
        public static MathematicalFunction AttackDisarmedEnemyTankSlackUntilRearmedFunction { get; private set; }
        public static MathematicalFunction AttackDisarmedEnemyTankAttackActionFunction { get; private set; }
        public static MathematicalFunction AttackLockedDownEnemyTankFunction { get; private set; }

        static ScenarioValueFunctions()
        {
            ClearRunAtBaseScenarioValueFunction
                = new ReverseLogisticFunction(leftAsymptoticX: -50, rightAsymptoticX: 30, minAsymptoticY: 0, maxAsymptoticY: 100000); // one hundred thousand
            LockDownEnemyTankForOtherTankToDestroyValueFunction
                = new ReverseLogisticFunction(leftAsymptoticX: -120, rightAsymptoticX: 120, minAsymptoticY: 0, maxAsymptoticY: 10000); // ten thousand
            AvoidBlockingFriendlyTankFunction
                = new RampFunction(leftX: 4, rightX: 8, minY: -100000, maxY: 0);
                    // Note that the function falls off very slowly, meaning that there is a slight incentive to stay closer together
            AvoidWalkingIntoABulletFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 6, minAsymptoticY: 1, maxAsymptoticY: -VALUE_OF_A_TANK);  // fifty thousand
                    // slack is number of ticks until bullet collides, 6 ticks is enough time to cross the path of the bullet
            ShootBulletHeadOnFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 1, minAsymptoticY: 0, maxAsymptoticY: VALUE_OF_A_TANK);  // fifty thousand
                    // slack is number of ticks until bullet collides
            DodgeBulletFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 1, minAsymptoticY: 0, maxAsymptoticY: VALUE_OF_A_TANK);  // fifty thousand
                    // slack is number of ticks till reaching a survival point less number of ticks until bullet collides
            ProlongEnemyDisarmamentFunction
                = new TriangularFunction(startX: -500, modeX: -81, endX: 0, maxY: 30000); // thirty thousand
                    // use this to fake a linear decline, rather than the logistic function decline
                    // was: ReverseLogisticFunction(leftAsymptoticX: -80, rightAsymptoticX: 0, minAsymptoticY: 0, maxAsymptoticY: 30000);
            AttackEnemyBaseFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 300, minAsymptoticY: 0, maxAsymptoticY: 100000);  // hundred thousand
                    // this is a low value, designed purely to get the tanks away from their base and into the game
            AttackEnemyBaseAttackActionFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 300, minAsymptoticY: 0, maxAsymptoticY: 30000);  // thirty thousand
                    // This function boosts the action along the calculated shortest path.
                    // This helps to break deadlocks between adjacent choices, probably caused by logistic curve not being granular enough
            GrappleWithEnemyTankAttackDiffFunction
                = new ReverseLogisticFunction(leftAsymptoticX: -5, rightAsymptoticX: -1, minAsymptoticY: 0, maxAsymptoticY: 20000);  // ten thousand
            GrappleWithEnemyTankAttackFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 40, minAsymptoticY: 0, maxAsymptoticY: 5000);
            GrappleWithEnemyTankAttackActionFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 4, minAsymptoticY: 0, maxAsymptoticY: 10000);
            AttackDisarmedEnemyTankSlackUntilRearmedFunction
                = new ReverseLogisticFunction(leftAsymptoticX: -25, rightAsymptoticX: 0, minAsymptoticY: 0, maxAsymptoticY: 30000);  // thirty thousand
            AttackDisarmedEnemyTankAttackActionFunction
                = new ReverseLogisticFunction(leftAsymptoticX: -25, rightAsymptoticX: 0, minAsymptoticY: 0, maxAsymptoticY: 5000);  // five thousand
            AttackLockedDownEnemyTankFunction
                = new ReverseLogisticFunction(leftAsymptoticX: 0, rightAsymptoticX: 200, minAsymptoticY: 10000, maxAsymptoticY: 40000);  // ten thousand to forty thousand
        }
    }
}
