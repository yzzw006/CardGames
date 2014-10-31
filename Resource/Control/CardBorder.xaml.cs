using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CardGames.Data;

namespace CardGames.UI
{
    /// <summary>
    /// Interaction logic for CardBorder.xaml
    /// </summary>
    public partial class CardBorder : Button
    {
        bool _isContrary = false;
        public bool IsContrary
        {
            get { return _isContrary; }
            set 
            { 
                _isContrary = value;
                if (_isContrary)
                    Back.Opacity = 1;
                else
                    Back.Opacity = 0;
            }
        }

        int _rank;
        public int Rank
        {
            get
            {
                return _rank;
            }
        }
        int _suit;
        public int Suit
        {
            get
            {
                return _suit;
            }
        }
        String _tag;
        Path _path;

        #region 初始化
        public CardBorder()
        {
            InitializeComponent();
            Init();
        }
        public CardBorder(int rank, int suit,bool isContrary = false)
        {
            InitializeComponent();
            Init(rank, suit, isContrary);
        }
        public CardBorder(Card c)
        {
            InitializeComponent();
            Init(c.Rank, c.Suit,c.IsContrary);
        }
        void Init(int index = 1, int color = 1,bool isContrary = false)
        {
            this.SizeChanged += new SizeChangedEventHandler(CardBorder_SizeChanged);
            _rank = index;
            _suit = color;
            this.IsContrary = isContrary;
            this.Draw();
        }

        void Draw()
        {
            _tag = ConvertRank(_rank);
            _path = ConvertSuit(_suit);

            IndexB.Content = IndexA.Content = _tag;
            IndexB.Foreground = IndexA.Foreground = _path.Fill;

            ColorA.Content = ClonePath();
            ColorB.Content = ClonePath();

            DrawBody();
        }

        public void Redraw(int index, int color)
        {
            _rank = index;
            _suit = color;
            this.Draw();
        }

        Path ConvertSuit(int suit)
        {
            string[] colorArray = new string[2] { "HeartBrush", "SpadeBrush" };
            string[] suitArray = new string[4] { "Heart", "Diamond", "Spade", "Club" };

            Path result = new Path();
            result.Stretch = Stretch.Uniform;

            result.Fill = FindResource(colorArray[(suit - 1) / 2]) as LinearGradientBrush;
            result.Data = FindResource(suitArray[suit - 1]) as StreamGeometry;

            return result;
        }
        Path ClonePath()
        {
            return new Path() { Data = _path.Data, Fill = _path.Fill, Stretch = _path.Stretch };
        }

        String ConvertRank(int index)
        {
            if (index != 10)
                return "A23456789TJQK"[index - 1].ToString();
            else
                return "10";
        }

        #endregion

        ~CardBorder()
        {
            this.SizeChanged -= new SizeChangedEventHandler(CardBorder_SizeChanged);
        }

        #region 卡显示方法
        void DrawBody()
        {
            CardBody.Children.Clear();
            if (_rank >= 1 && _rank <= 10)
            {
                CardBody.Height = 75;

                ScaleTransform st = new ScaleTransform(-1, -1);
                Point sp = new Point(0.5, 0.5);
                List<Path> pathList = new List<Path>();
                double miniMargin = 13;
                for (int i = 0; i < _rank; i++)
                {
                    Path p = ClonePath();
                    p.Width = miniMargin;
                    p.RenderTransformOrigin = sp;
                    pathList.Add(p);
                    CardBody.Children.Add(p);
                }
                switch (_rank)
                {
                    case 1:
                        ArrangeControl(pathList[0]);
                        break;
                    case 2:
                        ArrangeControl(pathList[0], 2, 1);
                        ArrangeControl(pathList[1], 2, 3);
                        pathList[1].RenderTransform = st;
                        break;
                    case 3:
                        ArrangeControl(pathList[0], 2, 1);
                        ArrangeControl(pathList[1], 2, 2);
                        ArrangeControl(pathList[2], 2, 3);
                        pathList[2].RenderTransform = st;
                        break;
                    case 4:
                        ArrangeControl(pathList[0], 1, 1);
                        ArrangeControl(pathList[1], 1, 3);
                        ArrangeControl(pathList[2], 3, 1);
                        ArrangeControl(pathList[3], 3, 3);
                        pathList[1].RenderTransform = st;
                        pathList[3].RenderTransform = st;
                        break;
                    case 5:
                        ArrangeControl(pathList[0], 1, 1);
                        ArrangeControl(pathList[1], 1, 3);
                        ArrangeControl(pathList[2], 3, 1);
                        ArrangeControl(pathList[3], 3, 3);
                        ArrangeControl(pathList[4], 2, 2);
                        pathList[1].RenderTransform = st;
                        pathList[3].RenderTransform = st;
                        break;
                    case 6:
                        ArrangeControl(pathList[0], 1, 1);
                        ArrangeControl(pathList[1], 1, 3);
                        ArrangeControl(pathList[2], 3, 1);
                        ArrangeControl(pathList[3], 3, 3);
                        ArrangeControl(pathList[4], 1, 2);
                        ArrangeControl(pathList[5], 3, 2);
                        pathList[1].RenderTransform = st;
                        pathList[3].RenderTransform = st;
                        break;
                    case 7:
                        ArrangeControl(pathList[0], 1, 1);
                        ArrangeControl(pathList[1], 1, 3);
                        ArrangeControl(pathList[2], 3, 1);
                        ArrangeControl(pathList[3], 3, 3);
                        ArrangeControl(pathList[4], 1, 2);
                        ArrangeControl(pathList[5], 3, 2);
                        ArrangeControl(pathList[6], 2, 1);
                        pathList[6].Margin = new Thickness(0, miniMargin, 0, 0);
                        pathList[1].RenderTransform = st;
                        pathList[3].RenderTransform = st;
                        break;
                    case 8:
                        ArrangeControl(pathList[0], 1, 1);
                        ArrangeControl(pathList[1], 1, 3);
                        ArrangeControl(pathList[2], 3, 1);
                        ArrangeControl(pathList[3], 3, 3);
                        ArrangeControl(pathList[4], 1, 2);
                        ArrangeControl(pathList[5], 3, 2);
                        ArrangeControl(pathList[6], 2, 1);
                        ArrangeControl(pathList[7], 2, 3);
                        pathList[6].Margin = new Thickness(0, miniMargin, 0, 0);
                        pathList[7].Margin = new Thickness(0, 0, 0, miniMargin);
                        pathList[1].RenderTransform = st;
                        pathList[3].RenderTransform = st;
                        pathList[7].RenderTransform = st;
                        break;
                    case 9:
                        ArrangeControl(pathList[0], 1, 1);
                        ArrangeControl(pathList[1], 1, 1);
                        ArrangeControl(pathList[2], 1, 3);
                        ArrangeControl(pathList[3], 3, 1);
                        ArrangeControl(pathList[4], 3, 1);
                        ArrangeControl(pathList[5], 3, 3);
                        ArrangeControl(pathList[6], 1, 1);
                        ArrangeControl(pathList[7], 3, 1);
                        ArrangeControl(pathList[8], 2, 2);
                        pathList[1].Margin = new Thickness(0, 21, 0, 0);
                        pathList[4].Margin = new Thickness(0, 21, 0, 0);
                        pathList[6].Margin = new Thickness(0, 42, 0, 0);
                        pathList[7].Margin = new Thickness(0, 42, 0, 0);
                        pathList[2].RenderTransform = st;
                        pathList[5].RenderTransform = st;
                        pathList[6].RenderTransform = st;
                        pathList[7].RenderTransform = st;
                        break;
                    case 10:
                        ArrangeControl(pathList[0], 1, 1);
                        ArrangeControl(pathList[1], 1, 1);
                        ArrangeControl(pathList[2], 1, 3);
                        ArrangeControl(pathList[3], 3, 1);
                        ArrangeControl(pathList[4], 3, 1);
                        ArrangeControl(pathList[5], 3, 3);
                        ArrangeControl(pathList[6], 1, 1);
                        ArrangeControl(pathList[7], 3, 1);
                        pathList[1].Margin = new Thickness(0, 21, 0, 0);
                        pathList[4].Margin = new Thickness(0, 21, 0, 0);
                        pathList[6].Margin = new Thickness(0, 42, 0, 0);
                        pathList[7].Margin = new Thickness(0, 42, 0, 0);
                        ArrangeControl(pathList[8], 2, 1);
                        ArrangeControl(pathList[9], 2, 3);
                        pathList[8].Margin = new Thickness(0, miniMargin, 0, 0);
                        pathList[9].Margin = new Thickness(0, 0, 0, miniMargin);
                        pathList[2].RenderTransform = st;
                        pathList[5].RenderTransform = st;
                        pathList[6].RenderTransform = st;
                        pathList[7].RenderTransform = st;
                        pathList[9].RenderTransform = st;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Image cBody = FindResource(_tag + _suit) as Image;
                cBody.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                cBody.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                cBody.Stretch = Stretch.Uniform;
                if (cBody.Parent != null)
                {
                    (cBody.Parent as Grid).Children.Clear();
                }
                CardBody.Children.Add(cBody);
            }


        }
        void ArrangeControl(Path p, int hor = 2, int ver = 2)
        {
            p.HorizontalAlignment = System.Windows.HorizontalAlignment.Left + hor - 1;
            p.VerticalAlignment = System.Windows.VerticalAlignment.Top + ver - 1;
        }

        private void Adapt()
        {
            this.Width = this.Height * 0.65;
        }

        private void CardBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Adapt();
        }
        #endregion

    }
}
