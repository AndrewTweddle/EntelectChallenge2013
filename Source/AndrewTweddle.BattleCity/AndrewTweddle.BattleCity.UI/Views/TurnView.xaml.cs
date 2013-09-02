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
    /// Interaction logic for TurnView.xaml
    /// </summary>
    public partial class TurnView : UserControl
    {
        private TurnViewModel viewModel;

        public TurnViewModel ViewModel
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

        public TurnView()
        {
            InitializeComponent();
        }

        public TurnView(TurnViewModel viewModel)
            : this()
        {
            ViewModel = viewModel;
        }

    }
}
