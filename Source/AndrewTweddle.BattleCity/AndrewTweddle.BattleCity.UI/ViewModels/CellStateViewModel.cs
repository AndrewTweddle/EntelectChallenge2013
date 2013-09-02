using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using System.ComponentModel;

namespace AndrewTweddle.BattleCity.UI.ViewModels
{
    public class CellStateViewModel: BaseViewModel
    {
        private CellState cellState;

        public GameStateViewModel ParentViewModel
        {
            get;
            private set;
        }

        public CellState CellState
        {
            get
            {
                return cellState;
            }
            set
            {
                cellState = value;
                OnPropertyChanged("CellState");
            }
        }

        public bool IsAWall
        {
            get
            {
                return ParentViewModel.GameState.Walls[cellState.Position];
            }
        }

        public CellStateViewModel(GameStateViewModel parentViewModel)
        {
            ParentViewModel = parentViewModel;
            ParentViewModel.PropertyChanged += UpdateCellStateViewModelWhenParentViewModelChanges;
        }

        private void UpdateCellStateViewModelWhenParentViewModelChanges(object sender, PropertyChangedEventArgs e)
        {
            /* e.g.
            if ((e.PropertyName.StartsWith("Your") || e.PropertyName.StartsWith("Opponents"))
                && e.PropertyName.EndsWith("Color"))
            {
                OnPropertyChanged("BackgroundColor");
            }
             */
        }
    }
}
