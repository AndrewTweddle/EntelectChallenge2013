using System;
using System.IO;

namespace AndrewTweddle.BattleCity.Tournaments
{
    public class Contestant
    {
        public int Port { get; set; }
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public string OutputFolderPath { get; set; }
        public string BatchFilePath
        {
            get
            {
                return Path.Combine(FolderPath, "start.bat");
            }
        }
    }
}
