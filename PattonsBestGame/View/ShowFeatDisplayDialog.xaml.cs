using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;


namespace Pattons_Best
{
   public partial class ShowFeatDisplayDialog : Window
   {
      public bool CtorError = false;
      private RuleDialogViewer myRulesManager = null;
      private readonly FontFamily myFontFam1 = new FontFamily("Georgia");
      private bool myIsAllFeatsShown = false;
      private GameFeat myGameFeatToShow = new GameFeat();
      private bool[] myDisplayedFeats = new bool[300];
      //-----------------------------------------------
      public ShowFeatDisplayDialog(RuleDialogViewer rm)
      {
         Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "ShowFeatDisplayDialog(): \n feats=" + GameEngine.theInGameFeats.ToString() );
         InitializeComponent();
         if (null == rm)
         {
            Logger.Log(LogEnum.LE_ERROR, "InventoryDisplayDialog(): rv=null");
            CtorError = true;
            return;
         }
         if( 299 < GameEngine.theInGameFeats.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowFeatDisplayDialog(): Too many feats=" + GameEngine.theInGameFeats.Count.ToString() );
            CtorError = true;
            return;
         }
         myRulesManager = rm;
         if( false == UpdateGridRows())
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowFeatDisplayDialog(): UpdateGridRows() returne false");
            CtorError = true;
            return;
         }
      }
      //-----------------------------------------------
      private bool UpdateGridRows()
      {
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children) // Clear out existing Grid Row data
         {
            int row = Grid.GetRow(ui);
            if (1 < row)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         if( true == myIsAllFeatsShown )
            myButtonShowAll.Visibility = Visibility.Hidden;
         //------------------------------------------------------------
         for(int i=0; i< GameEngine.theInGameFeats.Count; ++i)
         {
            int rowNum = i + 2;
            GameFeat? feat = GameEngine.theInGameFeats[i];
            if( null == feat)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowFeatDisplayDialog(): feat=null for i=" + i.ToString() );
               return false;
            }
            bool isFeatChecked = false;
            if (0 == feat.Threshold)
            {
               if (0 < feat.Value)
                  isFeatChecked = true;
            }
            else if (feat.Threshold <= feat.Value)
            {
               isFeatChecked = true;
            }
            CheckBox cb = new CheckBox() { IsEnabled = false, IsChecked = isFeatChecked, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            myGrid.Children.Add(cb);
            Grid.SetColumn(cb, 0);
            Grid.SetRow(cb, rowNum);
            if ((false == myIsAllFeatsShown) && (false == myDisplayedFeats[i]) && (false == isFeatChecked))
            {
               System.Windows.Controls.Button b = new Button { Name = feat.Key, FontFamily = myFontFam1, FontSize = 10, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Show", Margin = new Thickness(5) };
               b.Click += ButtonShowFeat_Click;
               myGrid.Children.Add(b);
               Grid.SetColumn(b, 1);
               Grid.SetRow(b, rowNum);
            }
            else
            {
               TextBlock tb = new TextBlock() { FontFamily = myFontFam1, FontSize = 14, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
               tb.Inlines.Add(new Run(GameFeats.GetFeatMessage(feat, true)));
               myGrid.Children.Add(tb);
               Grid.SetColumn(tb, 1);
               Grid.SetRow(tb, rowNum);
            }
         }
         return true;
      }
      //-----------------------------------------------
      private void ButtonShowEventDialog_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (null == myRulesManager)
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr=null");
         else if (false == myRulesManager.ShowEventDialog(key))
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr.ShowRule() returned false for c=" + key);
      }
      private void ButtonShowFeat_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         for(int i=0; i< GameEngine.theInGameFeats.Count; ++i)
         {
            GameFeat? feat = GameEngine.theInGameFeats[i];
            if( null == feat)
            {
               Logger.Log(LogEnum.LE_ERROR, "ButtonShowFeat_Click(): feat=null for i=" + i.ToString() );
               return;
            }
            if( feat.Key == (string)b.Name )
            {
               myDisplayedFeats[i] = true;
               break;
            }
         }  
         if ( false == UpdateGridRows() )
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowFeat_Click(): UpdateGridRows() returned false");
      }
      private void ButtonShowAll_Click(object sender, RoutedEventArgs e)
      {
         myIsAllFeatsShown = true;
         if (false == UpdateGridRows())
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowAll_Click(): UpdateGridRows() returned false");
      }
   }
}
