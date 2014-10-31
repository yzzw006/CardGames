using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardGames.Data
{
    public class Card
    {
        int _rank;
        public int Rank
        {
            get
            {
                return _rank;
            }
            set
            {
                _rank = value;
            }
        }
        int _suit;
        public int Suit
        {
            get
            {
                return _suit;
            }
        }

        bool _isContrary;
        public bool IsContrary
        {
            get { return _isContrary; }
            set
            {
                _isContrary = value;
            }
        }

        //char _data
        public Card(int suit=1,int rank = 1,bool isContrary = false)
        {
            _rank = rank;
            _suit = suit;
            _isContrary = isContrary;
        }
        public Card(string card)
        {
            _rank = ConverterRank(card[0]);
            _suit = ConverterSuit(card[1]);
        }

        int ConverterRank(char c)
        {
            return "A23456789TJQK".IndexOf(c) + 1;
        }
        int ConverterSuit(char c)
        {
            // 红桃 方块 黑桃 梅花
            return "HDSC".IndexOf(c) + 1;
        }

        /// <summary>
        /// the function of shuffle the library洗牌 随机排序的泛型函数
        /// </summary>
        /// <typeparam name="T">the type ,normally it's Card</typeparam>
        /// <param name="inputList">the library to shuffle</param>
        /// <returns>the shuffled library</returns>
        public static List<T> GetRandomList<T>(List<T> inputList)
        {
            //Copy to a array
            //T[] copyArray = new T[inputList.Count];
            //inputList.CopyTo(copyArray);

            //Add range
            List<T> copyList = new List<T>();
            copyList.AddRange(inputList);

            //Set outputList and random
            List<T> outputList = new List<T>();
            Random rd = new Random(DateTime.Now.Millisecond);

            while (copyList.Count > 0)
            {
                //Select an index and item
                int rdIndex = rd.Next(0, copyList.Count);

                //remove it from copyList and add it to output
                outputList.Add(copyList[rdIndex]);
                copyList.RemoveAt(rdIndex);
            }
            return outputList;
        }
    }
}
