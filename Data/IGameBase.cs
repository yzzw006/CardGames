using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CardGames.UI;
using System.Windows;

namespace CardGames.Data
{
    public interface IGameBase
    {
        void Stack_MouseLeftButtonDown(List<CardBorder> cbList, int sIndex);
        void EmptyStack_MouseLeftButtonDown(int sIndex);

        void Menu_Restart(object sender, RoutedEventArgs e);
        void Menu_Return_Click(object sender, RoutedEventArgs e);
    }
}
