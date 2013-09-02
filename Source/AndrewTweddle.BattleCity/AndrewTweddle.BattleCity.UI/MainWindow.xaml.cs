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
using Microsoft.Win32;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.UI.ViewModels;
using AndrewTweddle.BattleCity.UI.Views;
using System.IO;

namespace AndrewTweddle.BattleCity.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;

        MainViewModel ViewModel
        {
            get
            {
                return viewModel;
            }
            set
            {
                viewModel = value;
                DataContext = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
        }

        private void LoadGameFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\Competitions\EntelectChallenge2013\temp\GameLogs";
            openFileDialog.Filter = "Game State files *.xml|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                ViewModel.GameFilePath = openFileDialog.FileName;    
            }
        }

        private void ViewGameFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Check that file exists...
            GameViewModel gameViewModel = ViewModel.LoadGameViewModel();
            GameView gameView = new GameView(gameViewModel);
            gameView.Show();
        }
    }
}
