using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AndrewTweddle.BattleCity.Core
{
    // TODO: Add support for out-of-bounds cells
    public class GameState
    {
        public int Tick { get; set; }
        public Outcome Outcome { get; set; }
        public Game Game { get; private set; }
        public BitArray[] Walls { get; private set; }
        public MobileState[] MobileStates { get; private set; }

        public GameState(Game game)
        {
            Game = game;
            Tick = 0;
            Outcome = Outcome.InProgress;

            Walls = new BitArray[game.BoardHeight];
            for (int row = 0; row < game.BoardHeight; row++)
            {
                Walls[row] = new BitArray(game.BoardWidth);
            }
            MobileStates = new MobileState[Constants.MOBILE_ELEMENT_COUNT];
        }

        public MobileState[] CloneMobileStates()
        {
            MobileState[] newMobileStates = new MobileState[Constants.MOBILE_ELEMENT_COUNT];
            for (int i = 0; i < Constants.MOBILE_ELEMENT_COUNT; i++)
            {
                newMobileStates[i] = MobileStates[i].Clone();
            }
            return newMobileStates;
        }

        public bool IsWall(Position position)
        {
            return Walls[position.Y][position.X];
        }
    }
}
