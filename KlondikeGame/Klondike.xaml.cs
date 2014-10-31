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
using CardGames.Data;

namespace CardGames.KlondikeGame
{
    /// <summary>
    /// Interaction logic for Klondike.xaml
    /// </summary>
    public partial class Klondike : UserControl, IGameBase
    {
        const int _stackWidth = 687;
        const int _cellWidth = 101;
        const int _stackCount = 7;

        KlondikeCore gameCore;

        CardStack[] stackList = new CardStack[_stackCount];
        Label[] homecellList = new Label[4];
        //DealCell

        public Klondike()
        {
            InitializeComponent();
            InitUI();
            InitData();

        }
        void InitUI()
        {
            this.MouseLeftButtonDown += FreecellClick;
            this.MouseRightButtonDown += Card_RightMouseDown;
            this.MouseRightButtonUp += Card_RightMouseUp;
            DragPannel.MouseMove += new MouseEventHandler(Element_MouseMove);
            DragPannel.MouseLeftButtonUp += new MouseButtonEventHandler(Element_MouseLeftButtonUp);

            for (int i = 1; i <= 7; i++)
            {
                stackList[i - 1] = FindName("CardStack" + i) as CardStack;
                stackList[i - 1].Index = i - 1;
                stackList[i - 1].GameBase = this;
            }
            for (int i = 1; i <= 4; i++)
            {
                homecellList[i - 1] = FindName("Homecell" + i) as Label;

                homecellList[i - 1].MouseLeftButtonDown += Homecell_MouseLeftButtonDown;
            }
            DealCell.MouseLeftButtonDown += Element_MouseLeftButtonDown;
        }

        public void InitData()
        {
            //string boardBuffer = MainWindow.InitializeBoard(gameIndex);
            gameCore = new KlondikeCore();

            int sindex = 0;
            foreach (CardStack cs in stackList)
            {
                cs.RefreshStack(gameCore.Stack[sindex]);
                //cs.Index = sindex;
                sindex++;
            }

            DealCell.Content = CreateCardBorder(gameCore.DealCard);
        }


        #region 拖动部分
        bool isDragDropInEffect = false;
        Point pos = new Point();
        //开始拖动，需要子控件上传被拖动的牌
        List<CardBorder> sCardBorderList;
        public void StartDrag(List<CardBorder> cbList)
        {

            double top = .0, left = .0;
            //重新生成时有些问题，所以保持牌的数量
            sCardBorderList = cbList;

            int index = 0;
            foreach (CardBorder cb in cbList)
            {
                if (gameCore.FocusedStack != 7)
                {
                    cb.Margin = new Thickness(0, 25 * index, 0, 0);
                    if (cb.Parent != null)
                        (cb.Parent as Grid).Children.Remove(cb);
                    DragPannel.Children.Add(cb);

                    index++;
                }
                else
                {
                    DragPannel.Children.Add(cb);
                }
            }
            //根据src确定拖动的控件的位置，尽量做到无缝
            if (gameCore.FocusedStack <= _stackCount-1)
            {
                top = 150 + 25 * gameCore.FocusedCard;
                left = (this.ActualWidth - _stackWidth) / 2 + _cellWidth * gameCore.FocusedStack;
            }
            else
            {
                top = 13;
                left = 97;
            }

            isDragDropInEffect = true;
            DragPannel.CaptureMouse();
            DragPannel.SetValue(Canvas.LeftProperty, left);
            DragPannel.SetValue(Canvas.TopProperty, top);
        }
        //Freecell(Label)的单击事件
        void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isDragDropInEffect)
            {
                //TODO_______________________________________
                if (gameCore.IsFocused)
                {
                    //TODO 移动焦点到此位置
                    //des实际上不可能是stack
                    int des = ParseDestination(e.GetPosition(null));
                    int src = gameCore.FocusedStack;
                    //如果移动尝试失败消除焦点
                    if (gameCore.TryMove(des))
                    {
                        //开始移动
                        //DragPannel.Children.Clear();
                        UIMove(src, des, gameCore.FocusedCard);
                    }
                    else
                    {
                        RemoveFocus();
                    }
                }
                else if (sender.GetType().Name == "CardBorder")
                {//如果没有焦点，则此次点击为设置焦点并可能准备拖动

                    FindFocus();

                    if (gameCore.FocusedCard != -1)
                    {
                        CardBorder cb = sender as CardBorder;
                        List<CardBorder> dragList = new List<CardBorder>() { cb };
                        //移除缓存内的事件，可能存在缓存已不再的问题

                        cb.RemoveHandler(Button.MouseLeftButtonDownEvent,
                                new MouseButtonEventHandler(Element_MouseLeftButtonDown));
                        DealCell.Content = null;

                        StartDrag(dragList);
                    }
                }
            }
        }
        //用于传递stackd的鼠标左键按下事件
        public void Stack_MouseLeftButtonDown(List<CardBorder> cbList, int sIndex)
        {
            //如果游戏已经有焦点并且该焦点可以移动到此位置，则直接Move
            if (!gameCore.IsFocused || !gameCore.TryMove(sIndex))
            {
                FindFocus();

                if (gameCore.IsDragable)
                {
                    StartDrag(cbList);
                }
                else
                {
                    //如果不能拖动，则焦点下浮到最后一个
                    stackList[sIndex].FocusLastCard();
                    FindFocus();
                }
            }
            else
            {

                UIMove(gameCore.FocusedStack, sIndex, gameCore.FocusedCard);
            }
        }
        public void EmptyStack_MouseLeftButtonDown(int sIndex)
        {
            if (gameCore.IsFocused && gameCore.TryMove(sIndex))
            {
                UIMove(gameCore.FocusedStack, sIndex, gameCore.FocusedCard);
            }
        }

        //用于单击移动牌到Homecell
        void Homecell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (gameCore.IsFocused)
            {
                //TODO 移动焦点到此位置
                //des 应当是Homecell中的一个
                int des = ParseDestination(e.GetPosition(null));
                int src = gameCore.FocusedStack;
                //如果移动尝试失败消除焦点
                if (gameCore.TryMove(des))
                {
                    //开始移动
                    //DragPannel.Children.Clear();
                    UIMove(src, des, gameCore.FocusedCard);
                }
                else
                {
                    RemoveFocus();
                }
            }
        }

        void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragDropInEffect)
            {
                FrameworkElement currEle = sender as FrameworkElement;
                double xPos = e.GetPosition(null).X - pos.X + (double)currEle.GetValue(Canvas.LeftProperty);
                double yPos = e.GetPosition(null).Y - pos.Y + (double)currEle.GetValue(Canvas.TopProperty);
                currEle.SetValue(Canvas.LeftProperty, xPos);
                currEle.SetValue(Canvas.TopProperty, yPos);
                pos = e.GetPosition(null);
            }
        }
        //结束拖动
        void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragDropInEffect)
            {
                FrameworkElement ele = sender as FrameworkElement;
                isDragDropInEffect = false;
                ele.ReleaseMouseCapture();

                int des = ParseDestination(e.GetPosition(null));
                int src = gameCore.FocusedStack;
                if (!gameCore.TryMove(des))
                {
                    DragPannel.Children.Clear();
                    if (src <= _stackCount-1)
                        stackList[src].RefreshPannel();
                    else
                    {
                       DealCell.Content = sCardBorderList[0];
                        sCardBorderList[0].AddHandler(Button.MouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(Element_MouseLeftButtonDown), true);
                    }
                }
                else
                {
                    DragPannel.Children.Clear();

                    UIMove(src, des, gameCore.FocusedCard, true);
                }
            }
        }
        #endregion

        #region 移牌方法
        //移牌
        //
        public void UIMove(int src, int des, int n, bool isOnDrag = false)
        {
            if (src <= 6)
            {
                stackList[src].AIRefresh(gameCore.Stack[src]);
                stackList[src].Flop();
            }
            else if (src == 7)
            {
                if (!isOnDrag)
                {
                    if (DealCell.Content != null)
                        (DealCell.Content as CardBorder).RemoveHandler(Button.MouseLeftButtonDownEvent,
                        new MouseButtonEventHandler(Element_MouseLeftButtonDown));
                    DealCell.Content = null;                    
                }
                Menu_Deal();
            }
           

            if (des <= 6)
            {
                stackList[des].AIRefresh(gameCore.Stack[des]);
            }
            else if (des == 7)
            {
                DealCell.Content = CreateCardBorder(gameCore.DealCard);
            }
            else if (des >= 8)
            {
                homecellList[des - 8].Content = CreateCardBorder(gameCore.Homecell[des - 8], 1);
            }
            RemoveFocus();

            if (gameCore.IsFinished)
            {
                GameResult gr = new GameResult();
                gr.GameBase = this;
                GamePannel.Children.Add(gr);
            }
        }
        //type 0 用于 freecell 1 homecell
        CardBorder CreateCardBorder(Card c, int type = 0)
        {
            if (c != null)
            {
                CardBorder cb = new CardBorder(c);
                cb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                cb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                if (type == 0)
                {
                    cb.AddHandler(Button.MouseLeftButtonDownEvent,
                        new MouseButtonEventHandler(Element_MouseLeftButtonDown), true);
                }
                else
                {
                    cb.AddHandler(Button.MouseLeftButtonDownEvent,
                        new MouseButtonEventHandler(Homecell_MouseLeftButtonDown), true);
                }
                return cb;
            }
            else
                return null;
        }

        // 0-7 stack
        int ParseDestination(Point p)
        {
            int result = -1;
            //判断是否是stack 0-7
            double x = (p.X - (this.ActualWidth - _stackWidth) / 2) % _cellWidth;
            if (x < 71 && p.Y > 150 && p.Y < 550)
            {
                //p.X < (this.ActualWidth + _stackWidth) / 2 && x > 0 && 
                result = (int)(p.X - (this.ActualWidth - _stackWidth) / 2) / _cellWidth;
                if (result < 0 || result > 7) result = -1;
                //return result;
            }
            else
            {
                //判断是否是Homecell 12-15
                if (this.ActualWidth - p.X > 0 && this.ActualWidth - p.X < 314)
                {
                    x = (314 - this.ActualWidth + p.X) % 81;
                    if (x < 71 && p.Y > 12 && p.Y < 120)
                    {
                        result = 8 + (int)(314 - this.ActualWidth + p.X) / 81;
                    }
                }
                else if (x>= 97 && x < 168 && p.Y > 12 && p.Y < 120)
                {
                    result = 7;
                }
            }

            return result;
        }
        #endregion

        #region 焦点相关
        //作用：可以进行卡的控制
        //作用：X玩家可以主动消除卡牌上的焦点
        //空白处点击会消除焦点 
        void FreecellClick(object sender , RoutedEventArgs e)
        {
            foreach (CardStack cs in stackList)
            {
                if (cs.IsMouseOver)
                    return;
            }
            if (DealCell.IsMouseOver)
                return;
            RemoveFocus();
        }

        public void FindFocus()
        {
            if (Keyboard.FocusedElement.GetType().Name == "CardBorder")
            {
                CardBorder cb = Keyboard.FocusedElement as CardBorder;
                gameCore.GetFocus(cb.Rank, cb.Suit);
            }
        }

        void RemoveFocus()
        {
            FocusControl.Focus();
            gameCore.RemoveFocus();
        }
        #endregion

        #region 菜单事件
        void EndGame()
        {
            DragPannel.Children.Clear();
            foreach (CardStack cs in stackList)
            {
                cs.RemoveAll();
            }
            for (int i = 0; i < 4; i++)
            {
                homecellList[i].Content = null;
            }
            DealCell.Content = null;
        }
        public void NewGame()
        {

            EndGame();
            this.InitData();
        }
        //void Deal()
        //{

        //}

        private void Menu_Deal(object sender = null, RoutedEventArgs e= null)
        {
            gameCore.Deal();
            if (gameCore.DealCard != null)
            {
                if (DealCell.Content == null)
                    DealCell.Content = CreateCardBorder(gameCore.DealCard);
                else
                    (DealCell.Content as CardBorder).Redraw(gameCore.DealCard.Rank, gameCore.DealCard.Suit);

                DealLine.Margin = new Thickness(12, 12 + gameCore.DealPercent * 107, 0, 0);
            }
        }
        void Menu_Help(object sender, RoutedEventArgs e)
        {
            HelpWindow hw = new HelpWindow();
            hw.Info = "目的：在四个回收单元中各创建一叠牌，每叠 13 张，且花色相同。每叠牌必须按从小 (A) 到大 (K) 的顺序排列。\n";
            hw.Info += "七列牌上下两行排列在桌面。游戏是只有下面的牌才能使用。\n";
            hw.Info += "规则：从每列底部拖牌，并按以下方式移动：";
            hw.Info += "从列和发牌区到列（或从可用单元到列）,在列中必须按降序依次放牌，而且红黑花色交替。";
            hw.Info += "从列和发牌区到回收单元,每叠牌必须由同一花色组成，并从 A 开始。";
            GamePannel.Children.Add(hw);
        }

        public void Menu_Restart(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        public void Menu_Return_Click(object sender, RoutedEventArgs e)
        {
            EndGame();
            (this.Parent as Grid).Children.Remove(this);

            //MainWindow main = App.Current.MainWindow as MainWindow;
            //main.EndGame();
        }
        #endregion

        #region TopCard
        void Card_RightMouseDown(object sender, MouseButtonEventArgs e)
        {
            int des = ParseDestination(e.GetPosition(null));
            if (des <= 7 && des >= 0)
                stackList[des].CheckTop();
        }
        void Card_RightMouseUp(object sender, MouseButtonEventArgs e)
        {
            foreach (CardStack cs in stackList)
            {
                if (cs.isToped)
                    cs.RecoverTop();
            }

        }
        #endregion

        
    }
}
