   M S B A M L   `   `   `  ����D  @CardGames, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:CardGames.UICardGames.UI  5   7   %    CardGames.ShiftingGame.Shifting   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      )myclr-namespace:CardGames.UI     5   7      TableBrushGreen#��[  5      .��5      � -   5        $��
GamePannel��5      .+�5	   
      CardGames.UI.CardStack  -   5           Name$  CardLibrary��5	   7   $	��364��6   $��	32,12,0,0q�6$   $	��Top=�6L   $
��Left=�5
      �� 5   
   $��Center=�6   $��	0,12,12,0q�62   $	��Top=�6E    �� Orientation$ 
Horizontal=�6]   .+�5      �� -   5        $��	Homecell1��5        	CellLabel#��[ 6%   �� 5      -   5        $��	Homecell2��5      #��[ 6%   $��10,0,0,0q�6H   �� 5      -   5        $��	Homecell3��5      #��[ 6%   $��10,0,0,0q�6H   �� 5      -   5        $��	Homecell4��5      #��[ 6%   $��10,0,0,0q�6H   5      �� 5   
   $	��Top=�6   $��Center=�6-   $��	0,160,0,0q�6J   $ 
Horizontal=�5      .+�5        -   5        $  
cardStack1��5   ,   $��0,0,0,0q�6   6,     5      -   5        $  
cardStack2��5   -   $��30,0,0,0q�6   6-     5      -	   5        $  
cardStack3��5   -   $��30,0,0,0q�6   6-     5      -
   5        $  
cardStack4��5   -   $��30,0,0,0q�6   6-   5      �� 5   
   -   5        $��	DragField��5      .+�5      � -   5        $��
DragPannel��5      $��71��6%   $
��Auto��60   5      �� 5   
   $��Bottom=�6   $��Right=�61   $	��130��6M   .+�5      �� -   5        ���#7658F8$��Restart��5   *   $��0,0,0,0q�6Q     
FlatButton#��[ 5      �� 5       -   5        ���#7658F8$
��Help��5    *   $��0,5,0,0q�6K   #��[ 5!      �� 5"      -   5        ���#7658F8$
��Back��5"   *   $��0,5,0,0q�6S   #��[ 5#      5$      �� 5%   
   -   5        $��FocusControl��5%       $��23��6   $�� ��64   $��71��6<   |�. $��Bottom=�5&      $
��Left=�6/   5'      5(      