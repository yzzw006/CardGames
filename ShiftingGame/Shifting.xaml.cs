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

namespace CardGames.ShiftingGame
{
    /// <summary>
    /// Interaction logic for Shifting.xaml
    /// </summary>
    public partial class Shifting : UserControl, IGameBase
    {
        const int _stackWidth = 414;
        const int _cellWidth = 111;

        ShiftingCore gameCore;
        CardStack[] stackList = new CardStack[4];
        Label[] homecellList = new Label[4];

        public Shifting()
        {
            InitializeComponent();

            Init();
        }
        void Init()
        {
            this.MouseLeftButtonDown += ShiftingClick;
            DragPannel.MouseMove += new MouseEventHandler(Element_MouseMove);
            DragPannel.MouseLeftButtonUp += new MouseButtonEventHandler(Element_MouseLeftButtonUp);
            this.MouseRightButtonDown += Card_RightMouseDown;
            this.MouseRightButtonUp += Card_RightMouseUp;


            for (int i = 1; i <= 4; i++)
            {
                stackList[i - 1] = FindName("cardStack" + i) as CardStack;
                stackList[i - 1].TopMargin = 10;
                stackList[i - 1].Index = i - 1;
                stackList[i - 1].GameBase = this;

                homecellList[i - 1] = FindName("Homecell" + i) as Label;

                homecellList[i - 1].MouseLeftButtonDown += Homecell_MouseLeftButtonDown;
            }

            gameCore = new ShiftingCore();

            CardLibrary.GameBase = this;
            CardLibrary.Index = 8;
            CardLibrary.TopMargin = 5;
            CardLibrary.RefreshStack(gameCore.CardLibrary);
        }

        #region Move
        bool isDragDropInEffect = false;
        Point pos = new Point();
        //开始拖动，需要子控件上传被拖动的牌
        List<CardBorder> sCardBorderList;
        public void StartDrag(List<CardBorder> cbList)
        {

            double top = .0, left = .0;
            //重新生成时有些问题，所以保持牌的数量
            sCardBorderList = cbList;

            //int index = 0;
            foreach (CardBorder cb in cbList)
            {
                cb.Margin = new Thickness(0, 0, 0, 0);
                if (cb.Parent != null)
                    (cb.Parent as Grid).Children.Remove(cb);
                DragPannel.Children.Add(cb);

                //index++;
            }
            //根据src确定拖动的控件的位置，尽量做到无缝
            if (gameCore.FocusedStack <= 3)
            {
                top = 160 + 10 * gameCore.FocusedCard;
                left = (this.ActualWidth - _stackWidth) / 2 + _cellWidth * gameCore.FocusedStack;
            }
            else
            {
                top = 12 + 5 * gameCore.FocusedCard;
                left = 32;
            }


            isDragDropInEffect = true;
            DragPannel.CaptureMouse();
            DragPannel.SetValue(Canvas.LeftProperty, left);
            DragPannel.SetValue(Canvas.TopProperty, top);
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
                    if (sIndex <= 3)
                        stackList[sIndex].FocusLastCard();
                    else
                        CardLibrary.FocusLastCard();
                    FindFocus();
                }
            }
            else
            {

                UIMove(gameCore.FocusedStack, sIndex);
            }
        }
        public void EmptyStack_MouseLeftButtonDown(int sIndex)
        {
            if (gameCore.IsFocused && gameCore.TryMove(sIndex))
            {
                UIMove(gameCore.FocusedStack, sIndex);
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

                    if (src <= 3)
                        stackList[src].RefreshPannel();
                    else
                        CardLibrary.RefreshPannel();
                }
                else
                {
                    DragPannel.Children.Clear();

                    UIMove(src, des);
                }
            }
        }
        //Homecell(Label)的单击事件(空的时候起作用)
        void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isDragDropInEffect)
            {
                //TODO for homecell
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
                        UIMove(src, des);
                    }
                    else
                    {
                        RemoveFocus();
                    }
                }                
            }
        }
        //用于单击移动牌到Homecell（事件源为内容卡）
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
                    UIMove(src, des);
                }
                else
                {
                    RemoveFocus();
                }
            }
        }
        #endregion

        #region 移牌方法
        //移牌
        //
        public void UIMove(int src, int des)
        {
            if(src==8)
                CardLibrary.AIRefresh(gameCore.CardLibrary);
            else
                stackList[src].AIRefresh(gameCore.Stack[src]);

            if(des<=3)
                stackList[des].AIRefresh(gameCore.Stack[des]);
            else
                homecellList[des - 4].Content = CreateCardBorder(gameCore.Homecell[des - 4]);

            RemoveFocus();

            if (gameCore.IsFinished)
            {
                GameResult gr = new GameResult();
                gr.GameBase = this;
                GamePannel.Children.Add(gr);
            }
        }
        //type 0 用于 freecell 1 homecell
        CardBorder CreateCardBorder(Card c)
        {
            CardBorder cb = new CardBorder(c);
            cb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            cb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            cb.AddHandler(Button.MouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(Homecell_MouseLeftButtonDown), true);
            return cb;
        }
        // 0-7 stack
        int ParseDestination(Point p)
        {
            int result = -1;

            //only consider valid destinaton
            //is homecell
            if (p.Y <= 120 && p.Y >= 12 && p.X>=(this.ActualWidth - 314) / 2)
            {
                double x = (p.X - (this.ActualWidth - 314) / 2) % 81;
                if (x <= 71)
                {
                    result = 4+(int)(p.X - (this.ActualWidth - 314) / 2) / 81;
                }
            }
            if (p.Y >= 160 && p.Y <= 560 && p.X >= (this.ActualWidth - _stackWidth) / 2)
            {
                double x = (p.X - (this.ActualWidth - _stackWidth) / 2) % _cellWidth;
                if (x <= 71)
                {
                    result = (int)(p.X - (this.ActualWidth - _stackWidth) / 2) / _cellWidth;
                }
            }
            return result;
        }
        #endregion

        #region 焦点相关
        //作用：玩家可以主动消除卡牌上的焦点
        //空白处点击会消除焦点 
        void ShiftingClick(object sender, RoutedEventArgs e)
        {
            //Keyboard.f
            //if (Keyboard.FocusedElement.GetType().Name == "CardBorder")
            //    return;
            foreach (CardStack cs in stackList)
            {
                if (cs.IsMouseOver)
                    return;
            }
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
            CardLibrary.RemoveAll();
            foreach (Label l in homecellList)
            {
                l.Content = null;
            }
        }

        public void Menu_Restart(object sender, RoutedEventArgs e)
        {
            EndGame();
            gameCore = new ShiftingCore();

            CardLibrary.RefreshStack(gameCore.CardLibrary);
            //for (int i = 0; i < 12; i++)
            //{
            //    stackList[i].RefreshStack(gameCore.Stack[i]);
            //}
            //CountLabel.Text = "Remained Cards : " + gameCore.RemainedCount;
        }


        public void Menu_Return_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Grid).Children.Remove(this);
            //MainWindow main = App.Current.MainWindow as MainWindow;
            //main.EndGame(3);
        }

        void Menu_Help(object sender, RoutedEventArgs e)
        {
            HelpWindow hw = new HelpWindow();
            hw.Info = "目的：将所有牌移至得分列。\n";
            hw.Info += "排列方式：桌面上有4个得分列,此外还有另外4列。游戏开始时，每列都是空的。发牌列只有最上面的一张牌是可用的。游戏开始时，发牌列有52张牌随机顺序排列。\n";
            hw.Info += "规则：把牌从A到K不管花色顺序摆放在得分列。只要每列的最上面一张牌符合得分列的排列顺序，就可以移动到得分列。桌面上的牌也可以从大到小,不计花色进行排列。每次只能移动一张牌。空列只能放入一张发的牌或K。发的牌可以放到任一非得分列，不计花色大小。";
            GamePannel.Children.Add(hw);
        }
        #endregion

        #region TopCard
        void Card_RightMouseDown(object sender, MouseButtonEventArgs e)
        {
            int des = ParseDestination(e.GetPosition(null));
            if (des <= 11 && des >= 0)
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
