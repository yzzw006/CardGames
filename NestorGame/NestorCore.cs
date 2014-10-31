using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CardGames.Data;

namespace CardGames.NestorGame
{
    class NestorCore
    {
        #region Property
        List<Card> _cardList = new List<Card>();

        List<Card>[] _cardStack = new List<Card>[12];
        public List<Card>[] Stack
        {
            get
            {
                return _cardStack;
            }
        }

        int _removedCards = 0;
        public int RemovedCards
        {
            get
            {
                return _removedCards;
            }
        }
        public bool IsFinished
        {
            get
            {
                if (_removedCards != 52)
                    return false;
                else
                    return true;
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

        
        #endregion

        #region Init
        public NestorCore()
        {
            //初始化牌
            //for (int j = 1; j <= 13; j++)
            //{
            //    for (int i = 1; i <= 4; i++)
            //    {
            //        _cardList.Add(new Card(i, j));
            //    }
            //}
            ////洗牌
            //_cardList = Card.GetRandomList<Card>(_cardList);

            Random rd = new Random(DateTime.Now.Millisecond);
            _cardList = Card_DealData.getDealCardList(rd.Next(0, 100000));

            for (int i = 0; i < 12; i++)
                _cardStack[i] = new List<Card>();
            //将牌均匀分为12列 暂时保留
            //for (var i = 0; i < 52; i++)
            //{
            //    _cardStack[i % 12].Add(_cardList[i]);
            //}
            for (int i = 0; i < 12; i++)
            {
                if (i < 8)
                    _cardStack[i].AddRange(_cardList.GetRange(i * 6, 6));
                else
                    _cardStack[i].Add(_cardList[40 + i]);
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

                //该游戏只能拖一个
                if (_cardStack[_focusedStack].Count != _focusedCard + 1)
                    return false;

                return true;
            }
        }
        #endregion

        #region 移动(该游戏中为消除)
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

            //同数即可消除
            if (_cardStack[des].Last().Rank == _cardStack[src].Last().Rank)
                return true;
            else
                return false;
        }
        void Move(int src, int des)
        {
            _cardStack[des].Remove(_cardStack[des].Last());
            _cardStack[src].Remove(_cardStack[src].Last());
            _removedCards += 2;
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
    }
}
