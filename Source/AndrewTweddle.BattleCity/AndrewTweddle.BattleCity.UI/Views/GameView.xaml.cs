using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AndrewTweddle.BattleCity.UI.ViewModels;

namespace AndrewTweddle.BattleCity.UI.Views
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : Window
    {
        private GameViewModel viewModel;

        public GameViewModel ViewModel
        {
            get
            {
                return viewModel;
            }
            set
            {
                viewModel = value;
                DataContext = viewModel;
            }
        }

        public GameView()
        {
            InitializeComponent();
        }

        public GameView(GameViewModel viewModel): this()
        {
            ViewModel = viewModel;
        }
    }
}
