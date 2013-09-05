using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.AI.ScenarioEngine;

namespace AndrewTweddle.BattleCity.AI.ScenarioEngine
{
    public static class ScenarioEvaluator
    {
        public static MoveResult[] EvaluateScenario(Scenario scenario)
        {
            MoveGenerator[] moveGenerators = scenario.GetMoveGeneratorsByMoveTreeLevel();

            MoveGenerator initialMoveGenerator = moveGenerators[0];
            Move[] initialMoves = initialMoveGenerator.Generate(scenario, null);
            MoveResult[] moveResults = new MoveResult[initialMoves.Length];
            for (int i = 0; i < moveResults.Length; i++)
            {
                Move initialMove = initialMoves[i];

#if DEBUG
                System.Diagnostics.Stopwatch swatch = System.Diagnostics.Stopwatch.StartNew();
#endif
                MoveResult moveResult = initialMove.ExpandAndEvaluate(scenario, 1);
                moveResults[i] = moveResult;
#if DEBUG
                swatch.Stop();
                AndrewTweddle.BattleCity.Core.Helpers.DebugHelper.LogDebugMessage(
                    "ScenarioEvaluator", "Duration to expand moves for scenario {0}: {1}",
                    scenario.GetType().Name, swatch.Elapsed);
#endif
            }
            return moveResults;
        }
    }
}
