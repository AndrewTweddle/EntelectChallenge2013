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
        public static readonly DependencyProperty ViewModelProperty;

        public GameViewModel ViewModel
        {
            get
            {
                return (GameViewModel)GetValue(ViewModelProperty);
            }
            set
            {
                SetValue(ViewModelProperty, value);
                DataContext = value;
            }
        }

        static GameView()
        {
            ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(GameViewModel), typeof(GameView));
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
