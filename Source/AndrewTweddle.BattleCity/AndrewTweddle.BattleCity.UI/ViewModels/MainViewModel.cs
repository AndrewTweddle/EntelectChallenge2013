using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.UI.ViewModels
{
    public class MainViewModel: BaseViewModel
    {
        private string gameFilePath;

        public string GameFilePath 
        {
            get
            {
                return gameFilePath;
            }
            set
            {
                gameFilePath = value;
                OnPropertyChanged("GameFilePath");
            }
        }

        public GameViewModel LoadGameViewModel()
        {
            Game game = Game.Load(GameFilePath);
            Game.Current = game;
            GameViewModel gameViewModel = new GameViewModel
            {
                Game = game
            };
            return gameViewModel;
        }
    }
}
