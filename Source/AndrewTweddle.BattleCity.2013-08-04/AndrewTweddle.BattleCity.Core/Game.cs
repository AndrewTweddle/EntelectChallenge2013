using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public class Game
    {
        public Player[] Players { get; private set; }
        public int BoardHeight { get; set; }
        public int BoardWidth { get; set; }

        public Game(CellStatus[][] initialCellStatuses, Player[] players, Tank[] tanks)
        {
            Players = new Player[2];
        }

        public static Game LoadFromGameSetupString(string[] gameSetupStrings)
        {
            int rowCount = gameSetupStrings.Length;
            int columnCount = gameSetupStrings[0].Length;

            CellStatus[][] initialCellStatuses = gameSetupStrings.Select(
                rowString => rowString.Select(
                    statusChar => statusChar == '#' ? CellStatus.Wall : CellStatus.Empty).ToArray()
            ).ToArray();

            Player xPlayer = new Player { Name = "X" };
            Player yPlayer = new Player { Name = "Y" };

            return new Game(initialCellStatuses, 
        }
    }
}
