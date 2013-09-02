using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using System.Collections.ObjectModel;

namespace AndrewTweddle.BattleCity.UI.ViewModels
{
    public class GameStateViewModel: BaseViewModel
    {
        private TurnViewModel parentViewModel;
        private GameState gameState;
        private CellStateViewModel selectedCellStateViewModel;
        private ObservableCollection<CellStateViewModel> cellStateViewModels;

        public GameStateViewModel(TurnViewModel parentViewModel)
        {
            ParentViewModel = parentViewModel;
        }

        public TurnViewModel ParentViewModel
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

        public GameState GameState
        {
            get
            {
                return gameState;
            }
            set
            {
                gameState = value;
                OnPropertyChanged("GameState");
                if (gameState == null)
                {
                    if (CellStateViewModels != null)
                    {
                        CellStateViewModels = null;
                    }
                }
                else
                {
                    List<CellState> newCellStates = gameState.CalculationCache.GetAllCellStates()
                        .OrderBy(cs => cs.Position.Y)
                        .ThenBy(cs => cs.Position.X)
                        .ToList();
                    if (CellStateViewModels == null)
                    {
                        ObservableCollection<CellStateViewModel> newCellStateViewModels = new ObservableCollection<CellStateViewModel>();
                        foreach (CellState cellState in newCellStates)
                        {
                            CellStateViewModel cellStateViewModel = new CellStateViewModel(this)
                            {
                                CellState = cellState
                            };
                            newCellStateViewModels.Add(cellStateViewModel);
                        }
                        CellStateViewModels = newCellStateViewModels;
                    }
                    else
                    {
                        int index = 0;
                        foreach (CellStateViewModel csvm in CellStateViewModels)
                        {
                            csvm.CellState = newCellStates[index];
                            index++;
                        }

                        // TODO: Assumes the same number of cell states in each game state... fix later!
                    }
                }
            }
        }

        public ObservableCollection<CellStateViewModel> CellStateViewModels
        {
            get
            {
                return cellStateViewModels;
            }
            private set
            {
                if (cellStateViewModels != value)
                {
                    cellStateViewModels = value;
                    OnPropertyChanged("CellStateViewModels");
                }
            }
        }

        public CellStateViewModel SelectedCellStateViewModel
        {
            get
            {
                return selectedCellStateViewModel;
            }
            set
            {
                selectedCellStateViewModel = value;
                OnPropertyChanged("SelectedCellStateViewModel");
            }
        }
    }
}
