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

namespace CardGames.AcesUpGame
{
    /// <summary>
    /// Interaction logic for AcesUp.xaml
    /// </summary>
    public partial class AcesUp : UserControl, IGameBase
    {
        const int _stackWidth = 525;
        const int _cellWidth = 111;
        const int _stackCount = 4;

        AcesUpCore gameCore;

        CardStack[] stackList = new CardStack[_stackCount];
        //HomeList

        public AcesUp()
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

            gameCore = new AcesUpCore();
            int index = 0;
            foreach (CardStack cs in stackList)
            {
                cs.TopMargin = 20;
                cs.RefreshStack(gameCore.Stack[index]);
                cs.Index = index;
                cs.GameBase = this;
                index++;
            }
            HomeList.TopMargin = 4;
            HomeList.RefreshStack(gameCore.HomeList);
            HomeList.Index = 4;
            HomeList.GameBase = this;

            DealList.TopMargin = 8;
            DealList.RefreshStack(gameCore.DealLibrary);
            DealList.Index = 5;
            DealList.GameBase = this;
            DealList.IsHitTestVisible = false;

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
            else
            {
                top = 50 + 8 * gameCore.FocusedCard;
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
                    if (sIndex < _stackCount)
                        stackList[sIndex].FocusLastCard();
                    else
                        HomeList.FocusLastCard();
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
            if (des == _stackCount)
            {
                HomeList.AIRefresh(gameCore.HomeList);
            }
            else
            {
                stackList[des].AIRefresh(gameCore.Stack[des]);
            }

            RemoveFocus();
            RefreshInfo();

            if (gameCore.IsFinished)
            {
                GameResult gr = new GameResult();
                gr.GameBase = this;
                GamePannel.Children.Add(gr);
            }
        }

        // 0-_ stack
        int ParseDestination(Point p)
        {
            int result = -1;
            //判断是否是stack 0-7
            double x = (p.X - (this.ActualWidth - _stackWidth) / 2) % _cellWidth;
            if (x>0 && x < 71 && p.Y > 50 && p.Y < 450)
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
            //        result = _stackCount;
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

            gameCore = new AcesUpCore();
            for (int i = 0; i < _stackCount; i++)
            {
                stackList[i].RefreshStack(gameCore.Stack[i]);
            }
            HomeList.RefreshStack(gameCore.HomeList);
            DealList.RefreshStack(gameCore.DealLibrary);
            RefreshInfo();
        }

        private void Menu_Deal(object sender = null, RoutedEventArgs e = null)
        {
            if (gameCore.DealLibrary.Count > 0)
            {
                gameCore.Deal();
                for (int i = 0; i < _stackCount; i++)
                {
                    stackList[i].AIRefresh(gameCore.Stack[i]);
                }
                DealList.AIRefresh(gameCore.DealLibrary);
                //RefreshInfo();
            }
        }
        void Menu_Help(object sender, RoutedEventArgs e)
        {
            HelpWindow hw = new HelpWindow();
            hw.Info = "目的：除4张A以外，将所有的牌移至得分列。\n";
            hw.Info += "排列方式：发牌列在桌面的左边，牌面朝下。游戏开始时，发牌列有48张牌。四列明牌在中间。游戏开始时，每张桌面上会有一张随机放置的明牌。游戏是只能使用最下面的一张牌。得分列在左下角。游戏开始是，得分列是空的。\n";
            hw.Info += "规则：任何最下面一张牌如果比其他行列的最下面的牌小而且有相同的花色，就可移至得分列（A最大）。";
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
