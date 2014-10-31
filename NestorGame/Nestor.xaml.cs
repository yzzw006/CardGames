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

namespace CardGames.NestorGame
{
    /// <summary>
    /// Interaction logic for Nestor.xaml
    /// </summary>
    public partial class Nestor : UserControl, IGameBase
    {
        const int _stackWidth = 636;
        const int _stackTopMargin = 20;
        const int _cellWidth = 111;

        NestorCore gameCore;
        CardStack[] stackList = new CardStack[12];

        

        public Nestor()
        {
            InitializeComponent();

            Init();
        }
        void Init()
        {
            this.MouseLeftButtonDown += NestorClick;
            DragPannel.MouseMove += new MouseEventHandler(Element_MouseMove);
            DragPannel.MouseLeftButtonUp += new MouseButtonEventHandler(Element_MouseLeftButtonUp);
            this.MouseRightButtonDown += Card_RightMouseDown;
            this.MouseRightButtonUp += Card_RightMouseUp;


            for (int i = 1; i <= 12; i++)
            {
                stackList[i - 1] = FindName("cardStack" + i) as CardStack;
            }

            gameCore = new NestorCore();
            int index = 0;
            foreach (CardStack cs in stackList)
            {
                cs.TopMargin = _stackTopMargin;
                cs.RefreshStack(gameCore.Stack[index]);                
                cs.Index = index;
                cs.GameBase = this;
                index++;
            }
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

            //因为只有一个，所以直接添加
            foreach (CardBorder cb in cbList)
            {
                cb.Margin = new Thickness(0, 0, 0, 0);
                if (cb.Parent != null)
                    (cb.Parent as Grid).Children.Remove(cb);
                DragPannel.Children.Add(cb);
            }
            //根据src确定拖动的控件的位置，尽量做到无缝
            if (gameCore.FocusedStack <= 5)
            {
                top = 50 + _stackTopMargin * gameCore.FocusedCard;
                left = (this.ActualWidth - _stackWidth) / 2 + _cellWidth * gameCore.FocusedStack;
            }
            else
            {
                top = 270 + _stackTopMargin * gameCore.FocusedCard;
                left = (this.ActualWidth - _stackWidth) / 2 + _cellWidth * (gameCore.FocusedStack - 6);
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
                    stackList[sIndex].FocusLastCard();
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
            //该游戏空栈不可能进行有效移动
            //if (gameCore.IsFocused && gameCore.TryMove(sIndex))
            //{
            //    UIMove(gameCore.FocusedStack, sIndex);
            //}
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
            stackList[des].AIRefresh(gameCore.Stack[des]);

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

            double x = (p.X - (this.ActualWidth - _stackWidth) / 2) % _cellWidth;
            //切掉边距剩下的宽度
            if (x < 71)
            {
                //判断是上层stack还是下层
                if (p.Y > 50 && p.Y < 259)
                {
                    result = (int)(p.X - (this.ActualWidth - _stackWidth) / 2) / _cellWidth;
                }
                else if (p.Y > 270 && p.Y < 479)
                {
                    result = 6 + (int)(p.X - (this.ActualWidth - _stackWidth) / 2) / _cellWidth;
                }
            }
            return result;
        }
        #endregion

        #region 焦点相关
        //作用：玩家可以主动消除卡牌上的焦点
        //空白处点击会消除焦点 
        void NestorClick(object sender, RoutedEventArgs e)
        {
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
        }

        public void Menu_Restart(object sender, RoutedEventArgs e)
        {
            EndGame();
            gameCore = new NestorCore();
            for (int i = 0; i < 12; i++)
            {
                stackList[i].RefreshStack(gameCore.Stack[i]);
            }
            RefreshInfo();
        }


        public void Menu_Return_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Grid).Children.Remove(this);
        }

        void Menu_Help(object sender, RoutedEventArgs e)
        {
            HelpWindow hw = new HelpWindow();
            hw.Info = "目的：将所有扑克牌配对\n";
            hw.Info += "排列方式：桌面上有12列。游戏时只能使用最下面一张牌。游戏开始时，上方的8列牌每列随机发6张牌。下放的4列每列只有一张牌。得分列在左上角。游戏开始时，得分列是空的。\n";
            hw.Info += "规则：配对相同点数的两张牌，然后选择移动至得分列。";
            GamePannel.Children.Add(hw);
        }

        //private void Menu_Deal(object sender, RoutedEventArgs e)
        //{
        //    if (gameCore.Deal())
        //    {
        //        RefreshInfo();
        //        int index = 0;
        //        foreach (CardStack cs in stackList)
        //        {
        //            cs.AIRefresh(gameCore.Stack[index]);
        //            index++;
        //        }
        //        if (gameCore.IsFinished)
        //        {
        //            GameResult gr = new GameResult();
        //            gr.GameBase = this;
        //            GamePannel.Children.Add(gr);
        //        }
        //    }

        //}
        void RefreshInfo()
        {
            CountLabel.Text = "Score : " + gameCore.RemovedCards * 2;
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
