using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CardGames.Data;

namespace CardGames.AcesUpGame
{
    class AcesUpCore
    {
        #region Property
        const int _stackCount = 4;

        //牌池
        List<Card> _cardList = new List<Card>();
        public List<Card> DealLibrary
        {
            get
            {
                return _cardList;
            }
        }

        List<Card>[] _cardStack = new List<Card>[_stackCount];
        public List<Card>[] Stack
        {
            get
            {
                return _cardStack;
            }
        }
        //得分池
        List<Card> _homeList = new List<Card>();
        public List<Card> HomeList
        {
            get
            {
                return _homeList;
            }
        }

        //焦点域
        //0-6 为卡栈 7 _homeList
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
                if (_homeList.Count >= 48)
                    return true;
                else
                    return false;
            }
        }
        #endregion

        #region Init
        public AcesUpCore()
        {
            Random rd = new Random(DateTime.Now.Millisecond);
            _cardList = Card_DealData.getDealCardList(rd.Next(0, 100000));
            foreach (Card c in _cardList)
            {
                c.IsContrary = true;
            }

            for (int i = 0; i < _stackCount; i++)
            {
                _cardStack[i] = new List<Card>();
            }
            Deal();
        }
        public void Deal()
        {
            for (int i = 0; i < _stackCount; i++)
            {
                if (_cardList.Count > 0)
                {
                    _cardList[0].IsContrary = false;
                    _cardStack[i].Add(_cardList[0]);
                    _cardList.RemoveAt(0);
                }
            }

        }
        #endregion


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
        //数据已上传，所以不需要参数
        public bool IsDragable
        {
            get
            {
                //无效焦点不可拖动
                if (-1 == _focusedStack || -1 == _focusedCard)
                    return false;
                //only last card of stack is dragable.
                if (_focusedStack < _stackCount && _cardStack[_focusedStack].Count == _focusedCard + 1)
                    return true;
                else
                    return false;
            }
        }
        #endregion

        #region 移动
        /// <summary>
        /// 该次移动是否有效
        /// </summary>
        /// <param name="src">[0,6] 源卡栈 7 home</param>
        /// <param name="des">[0,6] 源卡栈 7 home</param>
        /// <returns>是否可移动</returns>
        bool CanMove(int src, int des)
        {
            //src can only be stack and des can only be home
            if (src > _stackCount-1)
                return false;
            if (des == _stackCount)
            {
                //有同花且比该牌大才能移动
                foreach (List<Card> cList in _cardStack)
                {
                    if(cList.Count>0){
                        if (IsLarger( cList.Last().Rank , _cardStack[src].Last().Rank) && cList.Last().Suit == _cardStack[src].Last().Suit)
                            return true;
                    }
                }
            }
            else
            {
                if (_cardStack[des].Count == 0)
                    return true;                
            }
            return false;
        }
        bool IsLarger(int max, int min)
        {
            if (max == 1)
                max += 13;
            if (min == 1)
                min += 13;
            if (max > min)
                return true;
            else
                return false;
        }

        void Move(int src, int des)
        {
            if (des == _stackCount)
            {
                _homeList.Add(_cardStack[src].Last());
                _cardStack[src].Remove(_cardStack[src].Last());
            }
            else
            {
                _cardStack[des].Add(_cardStack[src].Last());
                _cardStack[src].Remove(_cardStack[src].Last());
            }
        }
        #endregion

        #region Focus
        //获取焦点牌 
        //TODO:控制为唯一的焦点来源
        //算法：通过焦点控件的数据来查找
        public void GetFocus(int rank, int suit)
        {
            //最大O(52)
            foreach (Card c in _homeList)
            {
                if (c.Rank == rank && c.Suit == suit)
                {
                    this._isFocused = true;
                    this._focusedStack = 4;
                    this._focusedCard = _cardList.IndexOf(c);
                    return;
                }
            }
            for (int i = 0; i < _stackCount; i++)
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
    }
}
