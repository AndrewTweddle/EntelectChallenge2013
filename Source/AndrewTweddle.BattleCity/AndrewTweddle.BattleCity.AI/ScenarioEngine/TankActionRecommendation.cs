using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public class TankActionRecommendation
    {
        public bool IsAMoveRecommended { get; set; }
        public TankAction RecommendedTankAction { get; set; }
        public double[] TankActionValueAdjustments { get; private set; }

        public TankActionRecommendation()
        {
            TankActionValueAdjustments = new double[Constants.TANK_ACTION_COUNT];
        }

        public static TankActionRecommendation NoRecommendation { get; private set; }

        static TankActionRecommendation()
        {
            NoRecommendation = new TankActionRecommendation
            {
                IsAMoveRecommended = false
            };
        }
    }
}
