using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.Scenarios
{
    public class ScenarioEvaluator
    {
        public void EvaluateScenario(Scenario scenario)
        {
            /*
            // Player 0
            for (int p = 0; p < Constants.PLAYERS_PER_GAME; p++)
            {
                scenario.p = p;
                scenario.pBar = 1 - p;

                for (int i = 0; i < Constants.TANKS_PER_PLAYER; i++)
                {
                    scenario.i = i;
                    scenario.iBar = 1 - i;

                    for (int j = 0; j < Constants.TANKS_PER_PLAYER; j++)
                    {
                        scenario.j = j;
                        scenario.jBar = 1 - j;

                        // TODO: 

                        // Some scenarios may be independent of which way around the secondary player's tanks are tried...
                        if (scenario.IsSymmetricalWithRespectToPlayerPBarsTanks)
                        {
                            break;
                        }
                    }

                    // Some scenarios may be independent of which way around the primary player's tanks are tried...
                    if (scenario.IsSymmetricalWithRespectToPlayerPsTanks)
                    {
                        break;
                    }
                }

                if (scenario.IsSymmetricalWithRespectToPlayerPsTanks)
                {

                }
            }
             */
        }
    }
}
