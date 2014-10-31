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

namespace CardGames.TrustyTwelveGame
{
    /// <summary>
    /// Interaction logic for TrustyTwelve.xaml
    /// </summary>
    public partial class TrustyTwelve : UserControl, IGameBase
    {
        const int _stackWidth = 536;
        const int _cellWidth = 91;


        TrustyTwelveCore gameCore;
        CardStack[] stackList = new CardStack[12];

        public TrustyTwelve()
        {
            InitializeComponent();

            Init();            
        }
        void Init()
        {
            this.MouseLeftButtonDown += TrustyTwelveClick;
            DragPannel.MouseMove += new MouseEventHandler(Element_MouseMove);
            DragPannel.MouseLeftButtonUp += new MouseButtonEventHandler(Element_MouseLeftButtonUp);
            this.MouseRightButtonDown += Card_RightMouseDown;
            this.MouseRightButtonUp += Card_RightMouseUp;
            

            for (int i = 1; i <= 12; i++)
            {
                stackList[i - 1] = FindName("cardStack" + i) as CardStack;
            }

            gameCore = new TrustyTwelveCore();
            int index = 0;
            foreach (CardStack cs in stackList)
            {
                cs.RefreshStack(gameCore.Stack[index]);
                cs.TopMargin = 10;
                cs.Index = index;
                cs.GameBase = this;
                index++;
            }
            DealList.TopMargin = 8;
            DealList.RefreshStack(gameCore.DealLibrary);
            DealList.Index = 12;
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
            if (gameCore.FocusedStack <= 5)
            {
                top = 70 + 5 * gameCore.FocusedCard;
                left = (this.ActualWidth - _stackWidth) / 2 + _cellWidth * gameCore.FocusedStack;
            }
            else
            {
                top = 270 + 5 * gameCore.FocusedCard;
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
            stackList[des].AIRefresh(gameCore.Stack[des]);

            RemoveFocus();

            
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
                if (p.Y > 70 && p.Y < 220)
                {
                    result = (int)(p.X - (this.ActualWidth - _stackWidth) / 2) / _cellWidth;
                }
                else if (p.Y > 270 && p.Y < 420)
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
        void TrustyTwelveClick(object sender, RoutedEventArgs e)
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
            gameCore = new TrustyTwelveCore();
            for (int i = 0; i < 12; i++)
            {
                stackList[i].RefreshStack(gameCore.Stack[i]);
            }
            RefreshInfo();
        }

        
        public void Menu_Return_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Grid).Children.Remove(this);
            //MainWindow main = App.Current.MainWindow as MainWindow;
            //main.EndGame(2);
        }

        void Menu_Help(object sender, RoutedEventArgs e)
        {
            HelpWindow hw = new HelpWindow();
            hw.Info = "目的：把所有的牌都移至桌面上的12列。\n";
            hw.Info += "排列方式：桌面上有12列。游戏开始时，每一列只有一张牌。发牌列在上方。游戏开始时发牌列有40张随机牌。\n";
            hw.Info += "规则：将牌不计花色地由大向小排列。一次只能移动一张牌。空列可以得到发的新牌。其他列的牌不能移至空列。K可以移至A上。";
            GamePannel.Children.Add(hw);
        }

        private void Menu_Deal(object sender, RoutedEventArgs e)
        {
            if (gameCore.Deal())
            {
                RefreshInfo();
                int index = 0;
                foreach (CardStack cs in stackList)
                {
                    cs.AIRefresh(gameCore.Stack[index]);
                    index++;
                }
                if (gameCore.IsFinished)
                {
                    GameResult gr = new GameResult();
                    gr.GameBase = this;
                    GamePannel.Children.Add(gr);
                }
            }
            DealList.AIRefresh(gameCore.DealLibrary);
        }

        void RefreshInfo()
        {
           // CountLabel.Text = "Remained Cards : " + gameCore.DealLibrary.Count;
        }
        #endregion

        #region TopCard
        void Card_RightMouseDown(object sender, MouseButtonEventArgs e)
        {
            int des = ParseDestination(e.GetPosition(null));
            if (des <= 11 && des>=0)
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
