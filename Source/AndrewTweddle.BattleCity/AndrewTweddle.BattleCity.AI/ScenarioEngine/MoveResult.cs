using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.AI.Scenarios;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public class MoveResult
    {
        public int Slack { get; set; }
        public Move[] MovesByLevel { get; set; }
        public ScenarioEvaluationOutcome EvaluationOutcome { get; set; }
        public TankActionRecommendation[] RecommendedTankActionsByTankIndex { get; set; }

        protected MoveResult()
        {
            RecommendedTankActionsByTankIndex = new TankActionRecommendation[Constants.TANK_COUNT];
        }

        public MoveResult(Move[] movesByLevel): this()
        {
            MovesByLevel = movesByLevel;
        }

        public MoveResult(Move move)
            : this()
        {
            MovesByLevel = new Move[] { move };
        }

        public void SetTankActionRecommendation(int playerIndex, int tankNumber, TankActionRecommendation recommendation)
        {
            int tankIndex = Game.Current.Players[playerIndex].Tanks[tankNumber].Index;
            RecommendedTankActionsByTankIndex[tankIndex] = recommendation;
        }
    }
}
