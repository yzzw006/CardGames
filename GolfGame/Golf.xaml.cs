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

namespace CardGames.GolfGame
{
    /// <summary>
    /// Interaction logic for Golf.xaml
    /// </summary>
    public partial class Golf : UserControl, IGameBase
    {
        const int _stackWidth = 729;
        const int _cellWidth = 81;
        const int _stackCount = 7;

        GolfCore gameCore;

        CardStack[] stackList = new CardStack[_stackCount];
        //HomeList

        public Golf()
        {
            InitializeComponent();
            Init();

        }
        void Init()
        {
            this.MouseLeftButtonDown += FreecellClick;
            this.MouseRightButtonDown += Card_RightMouseDown;
            this.MouseRightButtonUp += Card_RightMouseUp;
            DragPannel.MouseMove += new MouseEventHandler(Element_MouseMove);
            DragPannel.MouseLeftButtonUp += new MouseButtonEventHandler(Element_MouseLeftButtonUp);

            for (int i = 1; i <= _stackCount; i++)
            {
                stackList[i - 1] = FindName("cardStack" + i) as CardStack;
            }

            gameCore = new GolfCore();
            int index = 0;
            foreach (CardStack cs in stackList)
            {
                cs.TopMargin = 20;
                cs.RefreshStack(gameCore.Stack[index]);
                cs.Index = index;
                cs.GameBase = this;
                index++;
            }
            HomeList.TopMargin = 15;
            HomeList.RefreshStack(gameCore.HomeList);
            HomeList.Index = 7;
            HomeList.GameBase = this;

            DealList.TopMargin = 15;
            DealList.RefreshStack(gameCore.DealLibrary);
            DealList.Index = 8;
            DealList.GameBase = this;

            RefreshInfo();
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
            if (gameCore.FocusedStack < _stackCount)
            {
                top = 50 + 20 * gameCore.FocusedCard;
                left = (this.ActualWidth - _stackWidth) / 2 + _cellWidth * gameCore.FocusedStack;
            }
            //else
            //{
            //    top = 50 + 8 * gameCore.FocusedCard;
            //    left = 32;
            //}


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
                    if (sIndex < _stackCount)
                        stackList[sIndex].FocusLastCard();
                    else
                        HomeList.FocusLastCard();
                    //stackList[sIndex].FocusLastCard();
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

                    stackList[src].RefreshPannel();
                }
                else
                {
                    DragPannel.Children.Clear();

                    UIMove(src, des);
                }
            }
        }
        #endregion

        #region 移牌方法
        //移牌
        //
        public void UIMove(int src, int des)
        {
            stackList[src].AIRefresh(gameCore.Stack[src]);
            HomeList.AIRefresh(gameCore.HomeList);

            RemoveFocus();
            RefreshInfo();

            if (gameCore.IsFinished)
            {
                GameResult gr = new GameResult();
                gr.GameBase = this;
                GamePannel.Children.Add(gr);
            }
        }

        // 0-7 stack
        int ParseDestination(Point p)
        {
            int result = -1;
            //判断是否是stack 0-7
            double x = (p.X - (this.ActualWidth - _stackWidth) / 2) % _cellWidth;
            if (x>0 && x < 71 && p.Y > 50 && p.Y < 350)
            {
                //p.X < (this.ActualWidth + _stackWidth) / 2 && x > 0 && 
                result = (int)(p.X - (this.ActualWidth - _stackWidth) / 2) / _cellWidth;
                if (result < 0 || result > _stackCount) result = -1;
                //return result;
            }
            //else
            //{
            //    //判断是否是HomeList
            //    if (p.X>=32 && p.X<=103 && p.Y>=50)
            //    {
            //        result = 7;
            //    }
            //}

            return result;
        }
        #endregion

        #region 焦点相关
        //作用：可以进行卡的控制
        //作用：X玩家可以主动消除卡牌上的焦点
        //空白处点击会消除焦点 
        void FreecellClick(object sender, RoutedEventArgs e)
        {
            foreach (CardStack cs in stackList)
            {
                if (cs.IsMouseOver)
                    return;
            }

            if (HomeList.IsMouseOver)
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
            CountLabel.Focus();
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
            HomeList.RemoveAll();
        }
        public void NewGame()
        {
            EndGame();

            gameCore = new GolfCore();
            for (int i = 0; i < _stackCount; i++)
            {
                stackList[i].RefreshStack(gameCore.Stack[i]);
            }
            HomeList.RefreshStack(gameCore.HomeList);
            RefreshInfo();
        }

        private void Menu_Deal(object sender = null, RoutedEventArgs e = null)
        {
            gameCore.Deal();
            HomeList.AIRefresh(gameCore.HomeList);
            DealList.AIRefresh(gameCore.DealLibrary);
            //RefreshInfo();
        }
        void Menu_Help(object sender, RoutedEventArgs e)
        {
            HelpWindow hw = new HelpWindow();
            hw.Info = "目的：移动所有的牌至得分列。\n";
            hw.Info += "排列方式：七列牌上下两行排列在桌面。游戏是只有最下面的牌才能使用。游戏开始时，每列随机发五张牌。发牌列在右下方，牌面朝下。游戏开始时，发牌列有17张随机放置的牌。得分列在右上方。游戏开始时只有一张牌。\n";
            hw.Info += "规则：不分花色按照从大到小和从小到大顺序（必须按顺序来）排列。比如2可以放在A之上，Q也可以放在K之上。也可以向得分列发新牌。";
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

        void RefreshInfo()
        {
            CountLabel.Text = "Score : " + gameCore.HomeList.Count * 2;
        }
        #endregion

        #region TopCard
        void Card_RightMouseDown(object sender, MouseButtonEventArgs e)
        {
            int des = ParseDestination(e.GetPosition(null));
            if (des < _stackCount && des >= 0)
                stackList[des].CheckTop();
            else if (des == _stackCount)
                HomeList.CheckTop();
        }
        void Card_RightMouseUp(object sender, MouseButtonEventArgs e)
        {
            foreach (CardStack cs in stackList)
            {
                if (cs.isToped)
                    cs.RecoverTop();
            }
            if (HomeList.isToped)
                HomeList.RecoverTop();

        }
        #endregion
    }
}
