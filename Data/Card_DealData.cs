using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardGames.Data
{
    class Card_DealData
    {
        #region Get CardList From seed:int
        public static List<Card> getDealCardList(int seed)
        {
            List<Card> cList = new List<Card>();
            List<int> deck = new List<int>();
            for (int i = 0; i < 52; i++)
            {
                deck.Add(i);
            }

            MSRand ms = new MSRand(seed);
            ms.shuffle<int>(deck);

            deck.Reverse();
            foreach (int i in deck)
            {
                cList.Add(RenderCard(i));
            }

            return cList;
        }
        static Card RenderCard(int cardId)
        {
            //将花色转换为目前已使用的顺序
            int[] suitArray = new int[4] { 4, 2, 1, 3 };
            return new Card(suitArray[cardId % 4], cardId / 4 + 1);
        }
        #endregion

        #region Get CardString From seed:int (Wasted)
        static string getDealCards(int seed)
        {
            int num_cols = 8;
            List<int>[] columns = new List<int>[8];
            for (int i = 0; i < 8; i++)
            {
                columns[i] = new List<int>();
            }
            List<int> deck = new List<int>();
            for (int i = 0; i < 52; i++)
            {
                deck.Add(i);
            }

            MSRand ms = new MSRand(seed);
            //ms._seed = seed;
            ms.shuffle<int>(deck);

            deck.Reverse();

            for (var i = 0; i < 52; i++)
            {
                columns[i % num_cols].Add(deck[i]);
            }

            string result = "";
            foreach (List<int> iList in columns)
            {
                //result += ": ";
                foreach (int i in iList)
                {
                    result += render_card(i) + " ";
                }
                result += "\n";
            }
            return result;
        }
        static string render_card(int card)
        {
            int suit = (card % 4);
            int rank = card / 4;
            string r = "A23456789TJQK";
            string s = "CDHS";
            return "" + r[rank] + s[suit];
        }
        #endregion
    }

    public class MSRand
    {
        int _seed;
        public MSRand(int seed = 1)
        {
            _seed = seed;
        }
        int rand()
        {
            
            this._seed = (this._seed * 214013 + 2531011) & 0x7FFFFFFF;
            return ((this._seed >> 16) & 0x7fff);
        }
        int max_rand(int m)
        {
            return this.rand() % m;
        }
        public List<T> shuffle<T>(List<T> deck)
        {
            if (deck.Count > 0)
            {
                int i = deck.Count;
                while (--i > 0)
                {
                    var j = this.max_rand(i + 1);
                    var tmp = deck[i];
                    deck[i] = deck[j];
                    deck[j] = tmp;
                }
            }
            return deck;
        }
    }
}
