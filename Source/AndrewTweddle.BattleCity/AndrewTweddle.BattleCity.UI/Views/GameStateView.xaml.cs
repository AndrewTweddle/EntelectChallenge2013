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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AndrewTweddle.BattleCity.UI.ViewModels;

namespace AndrewTweddle.BattleCity.UI.Views
{
    /// <summary>
    /// Interaction logic for GameStateView.xaml
    /// </summary>
    public partial class GameStateView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty;

        public GameStateViewModel ViewModel
        {
            get
            {
                return (GameStateViewModel)GetValue(ViewModelProperty);
            }
            set
            {
                SetValue(ViewModelProperty, value);
                DataContext = value;
            }
        }

        static GameStateView()
        {
            ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(GameStateViewModel), typeof(GameStateView));
        }

        public GameStateView()
        {
            InitializeComponent();
        }

        public GameStateView(GameStateViewModel viewModel)
            : this()
        {
            ViewModel = viewModel;
        }

        private void BoardListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void BoardListBox_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}
