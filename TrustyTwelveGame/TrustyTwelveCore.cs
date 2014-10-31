using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CardGames.Data;

namespace CardGames.TrustyTwelveGame
{
    class TrustyTwelveCore
    {
        #region Property
        //牌池
        List<Card> _cardList = new List<Card>();
        public List<Card> DealLibrary
        {
            get
            {
                return _cardList;
            }
        }

        List<Card>[] _cardStack = new List<Card>[12];
        public List<Card>[] Stack
        {
            get
            {
                return _cardStack;
            }
        }

        //焦点域
        //0-11 卡栈
        int _focusedStack = -1;
        public int FocusedStack
        {
            get
            {
                return _focusedStack;
            }
        }
        //焦点卡 0-n
        int _focusedCard = -1;
        public int FocusedCard
        {
            get
            {
                return _focusedCard;
            }
        }

        bool _isFocused = false;
        public bool IsFocused
        {
            get
            {
                return _isFocused;
            }
        }
        public bool IsFinished
        {
            get
            {
                if (this._cardList.Count > 0)
                    return false;
                else
                    return true;
            }
        }
        #endregion

        public TrustyTwelveCore() 
        {
            Random rd = new Random(DateTime.Now.Millisecond);
            _cardList = Card_DealData.getDealCardList(rd.Next(0, 100000));
            foreach (Card c in _cardList)
            {
                c.IsContrary = true;
            }

            for (int i = 0; i < 12; i++)
            {
                _cardStack[i] = new List<Card>();
            }
            Deal();
        }

        #region 接口：尝试移动，判断是否可拖动
        //判断目的地址是否是可移动的
        public bool TryMove(int des)
        {
            if (-1 == des)
                return false;

            if (CanMove(_focusedStack, des))
            {
                Move(_focusedStack, des);

                return true;
            }
            else
                return false;
        }
        //焦点是否可拖动
        public bool IsDragable
        {
            get
            {
                //无效焦点不可拖动
                if (-1 == _focusedStack || -1 == _focusedCard)
                    return false;

                //该游戏只能拖一个
                if (_cardStack[_focusedStack].Count != _focusedCard + 1)
                    return false;

                return true;
            }
        }
        #endregion

        #region 移动
        /// <summary>
        /// 该次移动是否有效
        /// </summary>
        /// <param name="src">[0,11] 源卡栈</param>
        /// <param name="des">[0,11] 目的卡栈</param>
        /// <returns>是否可移动</returns>
        bool CanMove(int src, int des)
        {
            if (src == des)
                return false;
            if (_cardStack[des].Count == 0)
                return false;
            else
            {
                int sub = _cardStack[des].Last().Rank - _cardStack[src].Last().Rank;
                if (sub == 1 || sub == -12)
                    return true;
                else
                    return false;
            }
        }
        void Move(int src, int des)
        {
            _cardStack[des].Add(_cardStack[src].Last());
            _cardStack[src].Remove(_cardStack[src].Last());
        }        
        #endregion

        #region Focus
        //获取焦点牌 
        //TODO:控制为唯一的焦点来源
        //算法：通过焦点控件的数据来查找
        public void GetFocus(int rank, int suit)
        {
            //最大O(12*？=52)
            for (int i = 0; i < 12; i++)
            {
                foreach (Card c in _cardStack[i])
                {
                    if (c.Rank == rank && c.Suit == suit)
                    {
                        this._isFocused = true;
                        this._focusedStack = i;
                        this._focusedCard = _cardStack[i].IndexOf(c);
                        return;
                    }
                }
            }

            RemoveFocus();
        }
        public void RemoveFocus()
        {
            this._isFocused = false;
            this._focusedStack = -1;
            this._focusedCard = -1;
        }
        #endregion

        #region Deal
        //发牌
        //如果未有效发牌，返回非
        public bool Deal()
        {
            bool result = false;
            foreach (List<Card> cList in _cardStack)
            {
                if (cList.Count == 0 && _cardList.Count>0)
                {
                    _cardList[0].IsContrary = false;
                    cList.Add(_cardList[0]);
                    _cardList.RemoveAt(0);
                    result = true;
                }
            }
            return result;
        }

        
        #endregion

        
    }
}
