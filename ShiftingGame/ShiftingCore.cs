using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CardGames.Data;

namespace CardGames.ShiftingGame
{
    class ShiftingCore
    {
        #region Property
        //牌池
        List<Card> _cardList = new List<Card>();
        public List<Card> CardLibrary
        {
            get
            {
                return _cardList;
            }
        }

        List<Card>[] _cardStack = new List<Card>[4];
        public List<Card>[] Stack
        {
            get
            {
                return _cardStack;
            }
        }

        Card[] _homecell = new Card[4];
        public Card[] Homecell
        {
            get
            {
                return _homecell;
            }
        }

        //焦点域
        //0-3 为卡栈 4-7Homecell 8Library
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
                foreach (Card c in _homecell)
                {
                    if (c == null || c.Rank != 13)
                        return false;
                }
                return true;
            }
        }
        #endregion

        #region Init
        public ShiftingCore()
        {    
            Random rd = new Random(DateTime.Now.Millisecond);
            _cardList = Card_DealData.getDealCardList(rd.Next(0, 100000));
            for (int i = 0; i < 4; i++)
            {
                _cardStack[i] = new List<Card>();
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
                //AddRevokeList(des);
                Move(_focusedStack, des);

                return true;
            }
            else
                return false;
            //return false;
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

                //该游戏只能拖一个
                if (_focusedStack == 8)
                {
                    if (_cardList.Count != _focusedCard + 1)
                        return false;
                }
                else
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
        /// <param name="src">[0,3] 源卡栈 [4,7] homecell 8 library</param>
        /// <param name="des">[0,3] 目的卡栈</param>
        /// <returns>是否可移动</returns>
        bool CanMove(int src, int des)
        {
            //homecell cannot be source and library cannot be destination
            if ((src >= 4 && src <= 7) || des == 8)
                return false;
            //source and destination cannot be same
            if (src == des)
                return false;
            //Library can move to stack directly
            if (src == 8)
            {
                if (des <= 3)
                    return true;
                else
                {
                    if (des >= 4)
                    {
                        //check rank
                        if (_homecell[des - 4] == null && _cardList.Last().Rank == 1)
                            return true;
                        if (_homecell[des - 4] != null && _cardList.Last().Rank - _homecell[des - 4].Rank == 1)
                            return true;
                    }
                }
            }
            else
            {
                //src == [0,3]
                if (des <= 3)
                {
                    if (_cardStack[des].Count == 0)
                    {
                        //only a K or Library card can be put into empty stack.                   
                        if (_cardStack[src].Last().Rank == 13)
                            return true;
                    }
                    else
                        if (_cardStack[des].Last().Rank - _cardStack[src].Last().Rank == 1)
                            return true;
                }
                else
                {
                    //check rank
                    if (_homecell[des - 4] == null && _cardStack[src].Last().Rank == 1)
                        return true;
                    if (_homecell[des - 4] != null && _cardStack[src].Last().Rank - _homecell[des - 4].Rank == 1)
                        return true;
                }
            }
            return false;
        }
        void Move(int src, int des)
        {
            if (src == 8)
            {
                if (des <= 3)
                {
                    _cardStack[des].Add(_cardList.Last());
                    _cardList.Remove(_cardList.Last());
                }
                else
                {
                    _homecell[des - 4] = _cardList.Last();
                    _cardList.Remove(_cardList.Last());
                }
            }
            else
            {
                if (des <= 3)
                    _cardStack[des].Add(_cardStack[src].Last());
                else
                    _homecell[des - 4] = _cardStack[src].Last();
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
            foreach (Card c in _cardList)
            {
                if (c.Rank == rank && c.Suit == suit)
                {
                    this._isFocused = true;
                    this._focusedStack = 8;
                    this._focusedCard = _cardList.IndexOf(c);
                    return;
                }
            }
            for (int i = 0; i < 4; i++)
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
