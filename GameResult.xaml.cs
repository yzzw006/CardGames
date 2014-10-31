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
using CardGames.Data;

namespace CardGames
{
    /// <summary>
    /// Interaction logic for GameResult.xaml
    /// </summary>
    public partial class GameResult : UserControl
    {
        //用于存储parent
        IGameBase _gameBase;
        public IGameBase GameBase
        {
            set
            {
                _gameBase = value;
            }
        }

        public GameResult()
        {
            InitializeComponent();
        }

        private void Btn_NewGame_Click(object sender, RoutedEventArgs e)
        {
            _gameBase.Menu_Restart(sender, e);

            (this.Parent as Grid).Children.Remove(this);
        }

        private void Btn_Back_Click(object sender, RoutedEventArgs e)
        {
            _gameBase.Menu_Return_Click(sender, e);

            (this.Parent as Grid).Children.Remove(this);
        }
    }
}
