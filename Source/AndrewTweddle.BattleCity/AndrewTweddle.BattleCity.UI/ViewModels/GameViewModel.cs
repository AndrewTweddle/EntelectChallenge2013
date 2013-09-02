using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.UI.ViewModels
{
    public class GameViewModel: BaseViewModel
    {
        private Game game;
        private ObservableCollection<TurnViewModel> turnViewModels;
        private TurnViewModel selectedTurnViewModel;

        public Game Game
        {
            get
            {
                return game;
            }
            set
            {
                game = value;
                OnPropertyChanged("Game");
                if (game == null)
                {
                    if (TurnViewModels != null)
                    {
                        TurnViewModels = null;
                    }
                }
                else
                {
                    if (TurnViewModels == null)
                    {
                        ObservableCollection<TurnViewModel> newTurnViewModels = new ObservableCollection<TurnViewModel>();
                        foreach (Turn turn in Game.Turns)
                        {
                            TurnViewModel turnViewModel = new TurnViewModel(this)
                            {
                                Turn = turn
                            };
                            newTurnViewModels.Add(turnViewModel);
                        }
                        TurnViewModels = newTurnViewModels;
                    }
                    else
                    {
                        int index = 0;
                        foreach (TurnViewModel tvm in TurnViewModels)
                        {
                            if (index < Game.Turns.Count)
                            {
                                tvm.Turn = Game.Turns[index];
                                index++;
                            }
                            else
                            {
                                TurnViewModels.RemoveAt(index);
                            }
                        }

                        if (TurnViewModels.Count < Game.Turns.Count)
                        {
                            for (int i = TurnViewModels.Count; i < Game.Turns.Count; i++)
                            {
                                TurnViewModel turnViewModel = new TurnViewModel(this)
                                {
                                    Turn = Game.Turns[i]
                                };
                                TurnViewModels.Add(turnViewModel);
                            }
                        }
                    }
                }
            }
        }

        public ObservableCollection<TurnViewModel> TurnViewModels
        {
            get
            {
                return turnViewModels;
            }
            private set
            {
                if (turnViewModels != value)
                {
                    turnViewModels = value;
                    OnPropertyChanged("TurnViewModels");
                }
            }
        }

        public TurnViewModel SelectedTurnViewModel
        {
            get
            {
                return selectedTurnViewModel;
            }
            set
            {
                selectedTurnViewModel = value;
                
                OnPropertyChanged("SelectedTurnViewModel");
            }
        }
    }
}
