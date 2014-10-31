using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CardGames.Data;

namespace CardGames.FreecellGame
{

    //记录移动的数据
    public struct MoveInfo
    {
        //可以用于撤销
        public int src;
        public int srcIndex;
        public int des;

    };

    public class FreecellCore
    {
        #region 属性
        List<MoveInfo> _moveList = new List<MoveInfo>();
        public List<MoveInfo> RevokeList
        {
            get
            {
                return _moveList;
            }
        }

        Card[] _freecell = new Card[4];
        public Card[] Freecell
        {
            get
            {
                return _freecell;
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
        List<Card>[] _cardStack = new List<Card>[8];
        public List<Card>[] Stack
        {
            get
            {
                return _cardStack;
            }
        }

        //焦点域
        //0-7 为卡栈 8Freecell
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
                return IsHomeFull() || IsAllOrdered();
            }
        }

        #endregion

        #region Init
        public FreecellCore(int gameIndex)
        {
            if (gameIndex < 0)
                return;

            List<Card> cardLibrary = Card_DealData.getDealCardList(gameIndex);
            for (int i = 0; i < 8; i++)
                _cardStack[i] = new List<Card>();

            for (var i = 0; i < 52; i++)
            {
                _cardStack[i % 8].Add(cardLibrary[i]);
            }


        }
        //从字符串中获取卡组数据 - wasted 保留参考解析部分代码
        FreecellCore(string data)
        {
            //从c++代码改写过来的算法，竟然能用
            List<string> dataList = new List<string>();
            int currPos = 0, prevPos = 0;
            while ((currPos = data.IndexOf('\n', prevPos)) != -1)
            {
                string lineData = data.Substring(prevPos, currPos - prevPos);
                dataList.Add(lineData);
                prevPos = currPos + 1;
            }

            int index = 0;
            foreach (string str in dataList)
            {
                _cardStack[index] = new List<Card>();
                ParseStack(index, dataList[index]);
                index++;
            }
        }
        //每个卡栈分别转换该栈的卡 - wasted
        void ParseStack(int index, string data)
        {
            List<string> dataVec = new List<string>();

            int currPos = 0, prevPos = 0;
            while ((currPos = data.IndexOf(' ', prevPos)) != -1)
            {
                string cardData = data.Substring(prevPos, currPos - prevPos);
                dataVec.Add(cardData);
                prevPos = currPos + 1;
            }

            foreach (string card in dataVec)
            {
                _cardStack[index].Add(new Card(card));
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
                AddRevokeList(des);
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
                if (_focusedStack >= 0 && _focusedStack <= 7)
                {
                    //TODO：可拖动的长度是有限的 取消掉，拖动无限，但是移动有限
                    //超过可拖动的长度，不能拖动
                    //if (_cardStack[_focusedStack].Count - _focusedCard > FreeNum())
                    //    return false;
                    //非顺序不能拖动
                    if (!IsOrdered(_focusedStack, _focusedCard))
                        return false;
                }
                return true;
            }
        }
        #endregion

        #region 撤销
        public void Revoke()
        {
            if (_moveList.Count > 0)
            {
                //int last = moveList.Count - 1;
                MoveInfo mi = _moveList.Last();
                Move(mi.src, mi.des, mi.srcIndex);
                _moveList.Remove(mi);
            }
        }
        //插入移动记录，该方法应当在移动前调用，否则数据会异常
        void AddRevokeList(int des)
        {
            int nSrc, nDes, nSrcIndex;
            if (des <= 7)
            {
                nSrc = des;
                nSrcIndex = _cardStack[des].Count;
            }
            else if (des <= 11)
            {
                nSrc = 8;
                nSrcIndex = des - 8;
            }
            else
            {
                nSrc = 9;
                nSrcIndex = des - 12;
            }
            if (_focusedStack <= 7)
                nDes = _focusedStack;
            else
                nDes = _focusedStack + _focusedCard;

            _moveList.Add(new MoveInfo() { src = nSrc, des = nDes, srcIndex = nSrcIndex });
        }
        #endregion

        #region Focus
        //获取焦点牌 
        //TODO:控制为唯一的焦点来源
        //算法：通过焦点控件的数据来查找
        public void GetFocus(int rank, int suit)
        {
            //最大O(8*？=52)
            for (int i = 0; i < 8; i++)
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
            for (int i = 0; i < 4; i++)
            {
                if (_freecell[i] != null && _freecell[i].Rank == rank && _freecell[i].Suit == suit)
                {
                    this._isFocused = true;
                    this._focusedStack = 8;
                    this._focusedCard = i;
                    return;
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

        #region 移动判断
        /// <summary>
        /// 该次移动是否有效
        /// </summary>
        /// <param name="src">[0,8] 0-7 为卡栈 8Freecell</param>
        /// <param name="des">[0,15] stack 0-7 Freecell 8-11 Homecell 12-15</param>
        /// <param name="n">卡源内联值</param>
        /// <returns></returns>
        bool CanMove(int src, int des, int n)
        {
            bool result = false;
            if (src <= 7)
            {
                //源和目的不能相同
                if (src == des)
                    return false;

                if (des <= 7)
                {

                    result = CanSSMove(src, des, n);
                }
                else if (des <= 11)
                    result = CanSFMove(src, des, n);
                else if (des <= 15)
                    result = CanSHMove(src, des, n);
            }
            else if (src == 8)
            {
                //源和目的不能相同
                if (src + n == des)
                    return false;

                if (des <= 7)
                    result = CanFSMove(src, des, n);
                else if (des <= 11)
                    result = CanFFMove(src, des, n);
                else if (des <= 15)
                    result = CanFHMove(src, des, n);
            }
            return result;
        }
        //stack to stack:要求是顺序的
        bool CanSSMove(int src, int des, int n)
        {
            if (_cardStack[des].Count == 0)
            {
                //TODO:验证张数
               // int num = _cardStack[src].Count - n;
                if (_cardStack[src].Count - n > FreeNum(0))
                    return false;
                else
                    return true;
            }
            else
            {
                if (_cardStack[src].Count - n > FreeNum(1))
                    return false;
                else
                {
                    if (IsOrdered(_cardStack[des].Last(), _cardStack[src][n]))
                        return true;
                    else
                        return false;
                }
            }
        }
        //stack to freecell:要求是1张 且 该格为空
        bool CanSFMove(int iSrc, int iDest, int n)
        {
            if (n + 1 == _cardStack[iSrc].Count && _freecell[iDest - 8] == null)
                return true;
            else
                return false;
        }
        //stack to homecell:1要求花色一致() 2值是顺序的 3 1张
        bool CanSHMove(int iSrc, int iDest, int n)
        {
            //验证是否是1张
            if (n + 1 != _cardStack[iSrc].Count)
                return false;
            //验证花色
            if (_cardStack[iSrc][n].Suit != iDest - 11)
                return false;
            //check rank
            if (_homecell[iDest - 12] == null && _cardStack[iSrc][n].Rank != 1)
                return false;
            if (_homecell[iDest - 12] != null && _cardStack[iSrc][n].Rank - _homecell[iDest - 12].Rank != 1)
                return false;
            return true;
        }
        //重合问题已验过，所以只验证目的地是否为空
        bool CanFFMove(int iSrc, int iDest, int n)
        {
            if (_freecell[iDest - 8] == null)
                return true;
            else
                return false;
        }
        //similar to SS ,but source can only be one card
        bool CanFSMove(int iSrc, int iDest, int n)
        {
            if (_cardStack[iDest].Count == 0 || IsOrdered(_cardStack[iDest].Last(), _freecell[n]))
                return true;
            else
                return false;
        }
        //similar to SH ,but source can only be one card
        bool CanFHMove(int iSrc, int iDest, int n)
        {
            //验证花色
            if (_freecell[n].Suit != iDest - 11)
                return false;
            //check rank
            if (_homecell[iDest - 12] == null && _freecell[n].Rank != 1)
                return false;
            if (_homecell[iDest - 12] != null && _freecell[n].Rank - _homecell[iDest - 12].Rank != 1)
                return false;
            return true;
        }
        #endregion

        #region 移动
        void Move(int src, int des, int n)
        {
            if (src <= 7)
            {
                if (des <= 7)
                {
                    //SSMove
                    int moveCount = _cardStack[src].Count - n;
                    _cardStack[des].AddRange(_cardStack[src].GetRange(n, moveCount));
                    _cardStack[src].RemoveRange(n, moveCount);
                }
                else if (des <= 11)
                {
                    //SFMove
                    _freecell[des - 8] = _cardStack[src][n];
                    _cardStack[src].RemoveAt(n);
                }
                else if (des <= 15)
                {
                    //SHMove
                    _homecell[des - 12] = _cardStack[src][n];
                    _cardStack[src].RemoveAt(n);
                }
            }
            else if (src == 8)
            {
                if (des <= 7)
                {
                    //FSMove
                    _cardStack[des].Add(_freecell[n]);
                }
                else if (des <= 11)
                {
                    //FFMove
                    _freecell[des - 8] = _freecell[n];
                }
                else if (des <= 15)
                {
                    //FHMove
                    _homecell[des - 12] = _freecell[n];
                }
                _freecell[n] = null;
            }
            else if (src == 9)
            {
                //撤销时才会src ==9
                if (des <= 7)
                {
                    //FSMove
                    _cardStack[des].Add(_homecell[n]);
                }
                else if (des <= 11)
                {
                    //FFMove
                    _freecell[des - 8] = _homecell[n];
                }
                else if (des <= 15)
                {
                    //FHMove
                    _homecell[des - 12] = _homecell[n];
                }
                //homecell因为是重复覆盖型容器，因此需要判断是改变还是移除
                if (_homecell[n].Rank == 1)
                    _homecell[n] = null;
                else
                {
                    //homecell[n].Rank--;//不能直接改，因为会导致移出的卡发生改变
                    _homecell[n] = new Card(_homecell[n].Suit, _homecell[n].Rank - 1);
                }
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

        //
        /// <summary>
        /// 返回可移动的数目
        /// </summary>
        /// <param name="targetType">0 移至空stack 1 移至非空stack</param>
        /// <returns></returns>
        int FreeNum(int targetType = 0)
        {
            int fNum = 0, sNum = 0;
            for (int i = 0; i < 4; i++)
            {
                if (_freecell[i] == null)
                    fNum++;
            }
            for (int i = 0; i < 8; i++)
            {
                if (_cardStack[i].Count == 0)
                    sNum++;
            }
            if (sNum > 0)
            {
                if (targetType == 0)
                    return (fNum + 1) * sNum;
                else
                    return (fNum + 1) * (sNum+1);
            }
            else
                return fNum + 1;
        }
    }


    //public enum MoveType
    //{
    //    SS_MOVE, //CardStack to CardStack
    //    FF_MOVE, // FreeCell to FreeCell
    //    SF_MOVE, //CardStack to FreeCell
    //    FS_MOVE, // FreeCell to CardStack
    //    SH_MOVE, //CardStack to HomeCell
    //    FH_MOVE  // FreeCell to HomeCell
    //};

}
