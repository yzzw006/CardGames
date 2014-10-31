using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CardGames.Data;

namespace CardGames.KlondikeGame
{
    class KlondikeCore
    {
        #region Property
        //牌池
        List<Card> _cardList = new List<Card>();
        int _dealIndex = 0;
        public double DealPercent
        {
            get { return (double)_dealIndex/_cardList.Count; }
        }
        public Card DealCard
        {
            get 
            {
                if (_cardList.Count > 0)
                    return _cardList[_dealIndex];
                else
                    return null;
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
        List<Card>[] _cardStack = new List<Card>[7];
        public List<Card>[] Stack
        {
            get
            {
                return _cardStack;
            }
        }

        //焦点域
        //0-6 为卡栈 7 Deal 8 Home
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

        //只读属性
        public bool IsFinished
        {
            get
            {
                return IsHomeFull();
            }
        }
        #endregion

        #region Init
        public KlondikeCore()
        {
            Random rd = new Random(DateTime.Now.Millisecond);
            _cardList = Card_DealData.getDealCardList(rd.Next(0, 100000));
            //翻牌
            foreach (Card c in _cardList)
            {
                c.IsContrary = true;
            }
            for (int i = 0; i < 7; i++)
            {
                _cardStack[i] = new List<Card>();
                _cardStack[i].AddRange(_cardList.GetRange(0, i + 1));
                //最后一张牌正过来
                _cardStack[i].Last().IsContrary = false;
                _cardList.RemoveRange(0, i + 1);
            }
            //牌池是正的
            foreach (Card c in _cardList)
            {
                c.IsContrary = false;
            }
        }

        public void Deal()
        {
            _dealIndex++;
            if (_dealIndex >= _cardList.Count)
            {
                _dealIndex = 0;
            }
        }
        #endregion

        #region 接口：尝试移动，判断是否可拖动
        //判断目的地址是否是可移动的
        public bool TryMove(int des)
        {
            if (-1 == des)
                return false;

            if (CanMove(_focusedStack, des, _focusedCard))
            {
                Move(_focusedStack, des, _focusedCard);

                return true;
            }
            else
                return false;
            //return false;
        }
        //焦点是否可拖动
        public bool IsDragable
        {
            get
            {
                //无效焦点不可拖动
                if (-1 == _focusedStack || -1 == _focusedCard)
                    return false;

                //TODO: 只有stack需要判断
                if (_focusedStack >= 0 && _focusedStack <= 6)
                {
                    //反面无法拖动
                    if (_cardStack[_focusedStack][_focusedCard].IsContrary)
                        return false;
                    //非顺序不能拖动
                    if (!IsOrdered(_focusedStack, _focusedCard))
                        return false;
                }
                return true;
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
            for (int i = 0; i < 7; i++)
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

            if (this.DealCard.Rank == rank && this.DealCard.Suit == suit)
                {
                    this._isFocused = true;
                    this._focusedStack = 7;
                    this._focusedCard = 0;
                    return;
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

        #region 移动判断
        /// <summary>
        /// 该次移动是否有效
        /// </summary>
        /// <param name="src">[0,7] 0-6 为卡栈 7 Deal</param>
        /// <param name="des">[0,11] stack 0-6 Deal 7(无效) Homecell 8-11</param>
        /// <param name="n">卡源内联值</param>
        /// <returns></returns>
        bool CanMove(int src, int des, int n)
        {
            //源和目的不能相同
            if (src == des)
                return false;

            bool result = false;
            
            if (src <= 6)
            {
                if (des <= 6)
                {
                    result = CanSSMove(src, des, n);
                }
                else if (des >= 8)
                    result = CanSHMove(src, des, n);
            }
            else if (src == 7)
            {
                if (des <= 6)
                    result = CanDSMove(src, des, n);
                else if (des >= 8)
                    result = CanDHMove(src, des, n);
            }
            return result;
        }
        //stack to stack:要求是顺序的
        bool CanSSMove(int src, int des, int n)
        {
            if (_cardStack[des].Count == 0 )
            {
                if (_cardStack[src][n].Rank == 13)
                    return true;
                else
                    return false;
            }
            else
            {
                if (IsOrdered(_cardStack[des].Last(), _cardStack[src][n]))
                    return true;
                else
                    return false;
            }
        }
        //stack to homecell:1要求花色一致() 2值是顺序的 3 1张
        bool CanSHMove(int iSrc, int iDest, int n)
        {
            //验证是否是1张
            if (n + 1 != _cardStack[iSrc].Count)
                return false;
            //验证花色
            if (_cardStack[iSrc][n].Suit != iDest - 7)
                return false;
            //check rank
            if (_homecell[iDest - 8] == null && _cardStack[iSrc][n].Rank != 1)
                return false;
            if (_homecell[iDest - 8] != null && _cardStack[iSrc][n].Rank - _homecell[iDest - 8].Rank != 1)
                return false;
            return true;
        }
        //similar to SS ,but source can only be one card
        bool CanDSMove(int iSrc, int iDest, int n)
        {
            if (_cardStack[iDest].Count == 0 )
            {
                if(DealCard.Rank == 13)
                    return true;
                else
                    return false;
            }
            else{
                if(IsOrdered(_cardStack[iDest].Last(), this.DealCard))
                    return true;
                else
                    return false;
            }
        }
        //similar to SH ,but source can only be one card
        bool CanDHMove(int src, int des, int n)
        {
            //验证花色
            if (DealCard.Suit != des - 7)
                return false;
            //check rank
            if (_homecell[des - 8] == null && DealCard.Rank != 1)
                return false;
            if (_homecell[des - 8] != null && DealCard.Rank - _homecell[des - 8].Rank != 1)
                return false;
            return true;
        }
        #endregion

        #region 移动
        void Move(int src, int des, int n)
        {
            if (src <= 6)
            {
                if (des <= 6)
                {
                    //SSMove
                    int moveCount = _cardStack[src].Count - n;
                    _cardStack[des].AddRange(_cardStack[src].GetRange(n, moveCount));
                    _cardStack[src].RemoveRange(n, moveCount);                    
                }
                else if (des >= 8)
                {
                    //SHMove
                    _homecell[des - 8] = _cardStack[src][n];
                    _cardStack[src].RemoveAt(n);
                }
                if (_cardStack[src].Count > 0 && _cardStack[src].Last().IsContrary)
                    _cardStack[src].Last().IsContrary = false;
            }
            else if (src == 7)
            {
                if (des <= 6)
                {
                    //DSMove
                    _cardStack[des].Add(DealCard);
                }
                else if (des >= 8)
                {
                    //FHMove
                    _homecell[des - 8] = DealCard;
                }
                _cardList.RemoveAt(_dealIndex);
            }
        }
        #endregion

        #region 胜利条件判断
        //homecell 是否已满（全为K）
        bool IsHomeFull()
        {
            foreach (Card c in _homecell)
            {
                if (c == null || c.Rank != 13)
                    return false;
            }
            return true;
        }
        //判断所有stack是否已消除逆序
        bool IsAllOrdered()
        {
            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < _cardStack[j].Count - 1; i++)
                {
                    if (_cardStack[j][i].Rank < _cardStack[j][i + 1].Rank)
                        return false;
                }
            }
            return true;
        }
        #endregion

        #region 顺序判断
        //判断目标牌及后面的牌是否是顺序的
        bool IsOrdered(int sIndex, int cIndex)
        {
            for (int i = cIndex; i < _cardStack[sIndex].Count - 1; i++)
            {
                if (!IsOrdered(_cardStack[sIndex][i], _cardStack[sIndex][i + 1]))
                    return false;
            }
            return true;
        }
        //判断前后两张牌是否是顺序的 c1在上方
        bool IsOrdered(Card c1, Card c2)
        {
            //判断大小(必须顺序)
            if (c1.Rank - c2.Rank != 1)
                return false;
            //判断颜色(同色为非)
            if (!((c1.Suit <= 2 && c2.Suit >= 3) || (c1.Suit >= 3 && c2.Suit <= 2)))
                return false;
            return true;
        }
        #endregion
    }
}
