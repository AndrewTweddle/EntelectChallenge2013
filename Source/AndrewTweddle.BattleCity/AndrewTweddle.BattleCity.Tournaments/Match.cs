using System;
using System.IO;

namespace AndrewTweddle.BattleCity.Tournaments
{
    public class Match
    {
        public string ResultsFolderPath { get; set; }
        public string ResultsFilePath
        {
            get
            {
                return Path.Combine(ResultsFolderPath, "results.csv");
            }
        }
    }
}
