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
using CardGames.UI;
using CardGames.FreecellGame;
using CardGames.TrustyTwelveGame;
using CardGames.NestorGame;
using CardGames.GolfGame;
using System.IO;
using System.Runtime.InteropServices;
using CardGames.ShiftingGame;
using CardGames.Data;
using CardGames.KlondikeGame;
using CardGames.AcesUpGame;

namespace CardGames
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        UserControl _startedGame;

        public MainWindow()
        {
            InitializeComponent();
            
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }



        #region Start Game
        void StartGame(object sender, RoutedEventArgs e)
        {
            Button source = sender as Button;
            switch (source.Content as string)
            {
                case "Freecell":
                    _startedGame = new Freecell();
                    break;
                case "TrustyTwelve":
                    _startedGame = new TrustyTwelve();
                    break;
                case "Shifting":
                    _startedGame = new Shifting();
                    break;
                case "Nestor":
                    _startedGame = new Nestor();
                    break;
                //Klondike
                case "Klondike":
                    _startedGame = new Klondike();
                    break;
                case "Golf":
                    _startedGame = new Golf();
                    break;
                case "AcesUp":
                    _startedGame = new AcesUp();
                    break;
            }
            _startedGame.Margin = new Thickness(0, 0, 0, 0);
            //_startedGame.Width = 800;
            //_startedGame.Height = 600;
            //GameBox.Child = _startedGame;
            //GamePanel.SizeChanged += new SizeChangedEventHandler(GameBox_SizeChanged);
            GamePanel.Children.Add(_startedGame);
        }

        //void GameBox_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    GameBox.Width = GamePanel.ActualWidth;
        //    GameBox.Height = GamePanel.ActualHeight;
        //}
        //void StartGame(int index = 1)
        //{
        //    switch (index)
        //    {
        //        case 1:
        //            _startedGame = new Freecell();
        //            break;
        //        case 2:
        //            _startedGame = new TrustyTwelve();
        //            break;
        //        case 3:
        //            _startedGame = new Shifting();
        //            break;
        //        case 4:
        //            _startedGame = new Nestor();
        //            break;
        //    }
        //    _startedGame.Margin = new Thickness(0, 0, 0, 0);
        //    GameBox.Child = _startedGame;
        //   // GamePanel.Children.Add(_startedGame);
        //}
        #endregion

        //保存成图片
        private void SaveFrameworkElementToImage(FrameworkElement ui, string filename)
        {
            FileStream ms = new FileStream(filename, FileMode.Create);
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)ui.ActualWidth+1, (int)ui.ActualHeight+1, 96d, 96d, PixelFormats.Pbgra32);
            bmp.Render(ui);
            //bmp.
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(ms);
            ms.Close();
        }
    }
}
