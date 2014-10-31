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

namespace CardGames.UI
{
    /// <summary>
    /// CardStack 用于处理扑克游戏中一个卡的队列
    /// 1 应该是可以用于程序内所有扑克游戏的
    /// 2 原则上不与Mainwindow下的任何子游戏发生耦合
    /// </summary>
    public partial class CardStack : UserControl
    {
        int _index = -1;
        public int Index
        {
            set
            {
                _index = value;
            }
        }
        int _margin = 25;
        public int TopMargin
        {
            set { _margin = value; }
        }

        //用于存储parent
        IGameBase _gameBase;
        public IGameBase GameBase
        {
            set
            {
                _gameBase = value;
            }
        }

        int changeNum;
        //缓存卡牌单位
        List<CardBorder> cardBorderList = new List<CardBorder>();

        #region Init
        public CardStack()
        {
            InitializeComponent();
            Stack.MouseLeftButtonDown += CardStack_LeftButtonDown;
            
        }
        ~CardStack()
        {
            this.MouseLeftButtonDown -= CardStack_LeftButtonDown;
            
        }
        //完全刷新cardstack
        public void RefreshStack(List<Card> cList)
        {
            CardPanel.Children.Clear();
            

            //int index = 0;
            foreach (Card c in cList)
            {
                CardBorder cb = new CardBorder(c);
                cb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                cb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                cb.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Card_StartGrag), true);
                CardPanel.Children.Add(cb);
                cardBorderList.Add(cb);
            }
            Arrange();
        }
        #endregion

        #region UI控制
        //对cardBorderList中的控件进行排列
        void Arrange()
        {
            for (int i = 0; i < cardBorderList.Count; i++)
            {
                cardBorderList[i].Margin = new Thickness(0, _margin * i, 0, 0);
            }
        }

        //智能刷新 为了效率，尽量避免对相同的牌进行刷新
        //暂时认为刷新间隔是一次移动，如果为多次移动，建议使用RefreshStack
        public void AIRefresh(List<Card> cList)
        {
            //移除 - cList 少于 实际
            if (cList.Count < cardBorderList.Count)
            {
                int change = cardBorderList.Count - cList.Count;
                foreach (CardBorder cb in cardBorderList.GetRange(cardBorderList.Count - change, change))
                {
                    cb.RemoveHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Card_StartGrag));
                }
                cardBorderList.RemoveRange(cardBorderList.Count - change, change);
            }
            else if (cList.Count > cardBorderList.Count)
            {
                int change = cList.Count - cardBorderList.Count;
                foreach (Card c in cList.GetRange(cardBorderList.Count,change))
                {
                    CardBorder cb = new CardBorder(c);
                    cb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    cb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    cb.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Card_StartGrag), true);
                    cardBorderList.Add(cb);
                }
            }
            RefreshPannel();
        }

        //根据cardBorderList刷新CardPannel
        //用于移动失败
        public void RefreshPannel()
        {
            //if (changed)
            //{
            //    RemoveHander();
            //    cardBorderList.RemoveRange(cardBorderList.Count - changeNum, changeNum);
            //}
            Arrange();
            CardPanel.Children.Clear();
            foreach (CardBorder cb in cardBorderList)
            {
                if (cb.Parent != null)
                {
                    (cb.Parent as Grid).Children.Remove(cb);
                }
                CardPanel.Children.Add(cb);
            }
        }
        void RemoveHander()
        {
            foreach (CardBorder cb in cardBorderList.GetRange(cardBorderList.Count - changeNum, changeNum))
            {
                cb.RemoveHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Card_StartGrag));
            }
        }
        //移除所有控件 为重开做准备
        public void RemoveAll()
        {
            //先移除所有卡的事件句柄
            foreach(CardBorder cb in cardBorderList)
            {
                cb.RemoveHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Card_StartGrag));
            }
            CardPanel.Children.Clear();
            cardBorderList.Clear();
        }
        #endregion

        #region 移牌事件
        void Card_StartGrag(object sender, RoutedEventArgs e)
        {
            CardBorder cb = (CardBorder)sender;
            int sIndex = cardBorderList.IndexOf(cb);

            List<CardBorder> dragList = cardBorderList.GetRange(sIndex, cardBorderList.Count - sIndex);
            changeNum = dragList.Count;

            _gameBase.Stack_MouseLeftButtonDown(dragList, _index);
            //MainWindow main = App.Current.MainWindow as MainWindow;
            //main.StackMoveControl(dragList, _index);
        }
        //此事件在stack为空时触发
        void CardStack_LeftButtonDown(object sender, RoutedEventArgs e)
        {
            _gameBase.EmptyStack_MouseLeftButtonDown(_index);
            //MainWindow main = App.Current.MainWindow as MainWindow;
            //main.EmptyStackMoveControl(_index);            
        }
        #endregion

        #region Top Card
        int TopedZIndex = 0;
        CardBorder topedCB;
        public bool isToped = false;
        public void CheckTop()
        {
            foreach (CardBorder b in cardBorderList)
            {
                //
                if (b.IsMouseOver && cardBorderList.IndexOf(b) + 1 != cardBorderList.Count)
                {
                    

                    TopedZIndex = Grid.GetZIndex(b);
                    topedCB = b;
                    isToped = true;
                    Grid.SetZIndex(b, 1);
                }
                //else
                //    Grid.SetZIndex(b, 0);
            }
        }
        public void RecoverTop()
        {
            Grid.SetZIndex(topedCB, TopedZIndex);
            isToped = false;

        }
        #endregion

        public void Flop()
        {
            if (cardBorderList.Count>0 && cardBorderList.Last().IsContrary)
                cardBorderList.Last().IsContrary = false;
        }

        public void FocusLastCard()
        {
            cardBorderList.Last().Focus();
        }
    }
}
