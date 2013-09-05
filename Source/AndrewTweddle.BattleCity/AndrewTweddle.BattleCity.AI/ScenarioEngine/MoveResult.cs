using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public class MoveResult
    {
        public int Slack { get; set; }
        public Move Move { get; set; }
        public ScenarioEvaluationOutcome EvaluationOutcome { get; set; }
        public TankActionRecommendation[] RecommendedTankActionsByTankIndex { get; set; }

        protected MoveResult()
        {
            RecommendedTankActionsByTankIndex = new TankActionRecommendation[Constants.TANK_COUNT];
        }

        public MoveResult(Move move)
            : this()
        {
            Move = move;
        }

        public void SetTankActionRecommendation(int playerIndex, int tankNumber, TankActionRecommendation recommendation)
        {
            int tankIndex = Game.Current.Players[playerIndex].Tanks[tankNumber].Index;
            RecommendedTankActionsByTankIndex[tankIndex] = recommendation;
        }

        public TankActionRecommendation GetRecommendedTankActionsByPlayerAndTankNumber(int playerIndex, int tankNumber)
        {
            int tankIndex = Game.Current.Players[playerIndex].Tanks[tankNumber].Index;
            return RecommendedTankActionsByTankIndex[tankIndex];
        }
    }
}
