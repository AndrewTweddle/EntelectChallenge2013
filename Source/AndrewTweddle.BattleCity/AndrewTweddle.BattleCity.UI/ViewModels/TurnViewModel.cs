using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.UI.ViewModels
{
    public class TurnViewModel: BaseViewModel
    {
        private Turn turn;
        private GameViewModel parentViewModel;
        private GameStateViewModel gameStateViewModel;


        public Turn Turn 
        {
            get
            {
                return turn;
            }
            set
            {
                turn = value;
                if (gameStateViewModel == null)
                {
                    gameStateViewModel = new GameStateViewModel(this);
                }
                gameStateViewModel.GameState = turn.GameState;
            }
        }

        public GameViewModel ParentViewModel 
        {
            get
            {
                return parentViewModel;
            }
            set
            {
                parentViewModel = value;
                OnPropertyChanged("ParentViewModel");
            }
        }

        public GameStateViewModel GameStateViewModel 
        {
            get
            {
                return gameStateViewModel;
            }
            set
            {
                gameStateViewModel = value;
                OnPropertyChanged("GameStateViewModel");
            }
        }

        public TurnViewModel(GameViewModel gameViewModel)
        {
            ParentViewModel = gameViewModel;
        }
    }
}
