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

namespace CardGames.FreecellGame
{
    /// <summary>
    /// Interaction logic for FreecellSelect.xaml
    /// </summary>
    public partial class FreecellSelect : UserControl
    {
        Freecell _parent;
        public Freecell FParent
        {
            set
            {
                _parent = value;
            }
        }

        public FreecellSelect()
        {
            InitializeComponent();
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(Text_Index.Text + e.Text, @"^\d+$"))
            {
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }

        private void Btn_Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (Text_Index.Text != "")
            {
                int index = Convert.ToInt32(Text_Index.Text) > 1000000 ? 1000000 : Convert.ToInt32(Text_Index.Text);
                _parent.NewGame(index);

                (this.Parent as Grid).Children.Remove(this);
            }
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Grid).Children.Remove(this);
        }
    }
}
