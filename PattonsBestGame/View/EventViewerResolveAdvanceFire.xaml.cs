using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;
using static Pattons_Best.EventViewerBattleSetup;

namespace Pattons_Best
{
   public partial class EventViewerResolveAdvanceFire : UserControl
   {
      public delegate bool EndResolveAdvanceFireCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int PREVIOUSLY_KIA = 100;
      public enum E0464Enum
      {
         ROLL_ADVANCE_FIRE,
         SHOW_RESULTS,
         END
      };
      public bool CtorError { get; } = false;
      private EndResolveAdvanceFireCallback? myCallback = null;
      private E0464Enum myState = E0464Enum.ROLL_ADVANCE_FIRE;
      private int myMaxRowCount = 0;
      private int myRollResultRowNum = 0;
      private bool myIsRollInProgress = false;
      //---------------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItemAdvance;
         public IMapItem myMapItemEnemy;
         public int myDieRoll = Utilities.NO_RESULT;
         public char mySector = '0';
         public char myRange = 'E';
         public int myModifier = 0;
         public string myResult = "";
         public GridRow(IMapItem enemyUnit, IMapItem advanceFire)
         {
            myMapItemAdvance = advanceFire;
            myMapItemEnemy = enemyUnit;
         }
      };
      private GridRow[] myGridRows = new GridRow[10];
      //---------------------------------------------------
      private IGameEngine? myGameEngine;
      private IGameInstance? myGameInstance;
      private readonly Canvas? myCanvas;
      private readonly ScrollViewer? myScrollViewer;
      private RuleDialogViewer? myRulesMgr;
      private IDieRoller? myDieRoller;
      //---------------------------------------------------
      private int myNumUseControlled = 0;
      //---------------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-------------------------------------------------------------------------------------
      public EventViewerResolveAdvanceFire(IGameEngine? ge, IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == ge) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventVieweResolveAdvance(): ge=null");
            CtorError = true;
            return;
         }
         myGameEngine = ge;
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventVieweResolveAdvance(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventVieweResolveAdvance(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventVieweResolveAdvance(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventVieweResolveAdvance(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventVieweResolveAdvance(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool ResolveAdvanceFire(EndResolveAdvanceFireCallback callback)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveAdvanceFire(): myGameInstance=null");
            return false;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveAdvanceFire(): myCanvas=null");
            return false;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveAdvanceFire(): myScrollViewer=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveAdvanceFire(): myRulesMgr=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveAdvanceFire(): myDieRoller=null");
            return false;
         }
         //--------------------------------------------------
         myCallback = callback;
         myNumUseControlled = 0;
         //--------------------------------------------------
         string[] sectors = new string[6] { "B1M", "B2M", "B3M", "B4M", "B6M", "B9M" };
         foreach (string sector in sectors)
         {
            ITerritory? t = Territories.theTerritories.Find(sector);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResolveAdvanceFire(): t=null for s=" + sector);
               return false;
            }
            IStack? stack1 = myGameInstance.BattleStacks.Find(t);
            if (null == stack1)
               continue;
            foreach (IMapItem mi in stack1.MapItems)
            {
               if (true == mi.Name.Contains("UsControl"))
               {
                  myNumUseControlled++;
                  break;
               }
            }
         }
         Logger.Log(LogEnum.LE_VIEW_ADV_FIRE_RESOLVE, "ResolveAdvanceFire(): myNumUseControlled=" + myNumUseControlled.ToString());
         //--------------------------------------------------
         int i = 0;
         string[] advanceFireTerritories = new string[8] { "B4L", "B4M", "B4C", "B6M", "B6C", "B9L", "B9M", "B9C" };
         foreach (string s in advanceFireTerritories)
         {
            ITerritory? t = Territories.theTerritories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResolveAdvanceFire(): t=null for s=" + s);
               return false;
            }
            IStack? stack = myGameInstance.BattleStacks.Find(t);
            if (null == stack)
               continue;
            IMapItems advanceFires = new MapItems();
            IMapItems enemyUnits = new MapItems();
            foreach ( IMapItem mi in stack.MapItems)
            {
               if (true == mi.Name.Contains("AdvanceFire"))
                  advanceFires.Add(mi);
               if (true == Utilities.IsEnemyUnit(mi))
                  enemyUnits.Add(mi);
            }
            if (0 == enemyUnits.Count)
            {
               foreach (IMapItem advanceFire in advanceFires)
                  myGameInstance.BattleStacks.Remove(advanceFire);
               continue;
            }
            Logger.Log(LogEnum.LE_VIEW_ADV_FIRE_RESOLVE, "ResolveAdvanceFire(): AdvFire=true for s=" + stack.ToString());
            foreach(IMapItem enemyUnit in enemyUnits )
            {
               foreach (IMapItem advanceFire in advanceFires)
               {
                  Logger.Log(LogEnum.LE_VIEW_ADV_FIRE_RESOLVE, "ResolveAdvanceFire(): IsEnemyUnit()=true for mi.Name=" + enemyUnit.Name);
                  myGridRows[i] = new GridRow(enemyUnit, advanceFire);
                  int count = enemyUnit.TerritoryCurrent.Name.Length;
                  if (count < 3)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ResolveAdvanceFire():  count<3 for mi.TerritoryCurrent.Name=" + enemyUnit.TerritoryCurrent.Name);
                     return false;
                  }
                  myGridRows[i].myRange = enemyUnit.TerritoryCurrent.Name[--count];
                  myGridRows[i].mySector = enemyUnit.TerritoryCurrent.Name[--count];
                  ++i;
               }
            }
         }
         myMaxRowCount = i;
         //--------------------------------------------------
         for(int k=0; k<myMaxRowCount; ++k )
            myGridRows[k].myModifier = TableMgr.GetFriendlyActionModifier(myGameInstance, myGridRows[k].myMapItemEnemy, myNumUseControlled, true, false, false, false);
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveAdvanceFire(): UpdateGrid() return false");
            return false;
         }
         myScrollViewer.Content = myGrid;
         return true;
      }
      private bool UpdateGrid()
      {
         if (false == UpdateEndState())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateEndState() returned false");
            return false;
         }
         if (E0464Enum.END == myState)
            return true;
         if (false == UpdateUserInstructions())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
            return false;
         }
         if (false == UpdateAssignablePanel())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
            return false;
         }
         if (false == UpdateGridRows())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
            return false;
         }
         return true;
      }
      private bool UpdateEndState()
      {
         if (E0464Enum.END == myState)
         {
            if (null == myGameInstance)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myGameInstance=null");
               return false;
            }
            IMapItems removals = new MapItems();
            foreach (IStack stack in myGameInstance.BattleStacks)
            {
               foreach (IMapItem mapItem in stack.MapItems)
               {
                  if( (true == mapItem.Name.Contains("AdvanceFire")) || (true == mapItem.IsKilled) )
                     removals.Add(mapItem);
               }
            }
            foreach (IMapItem mi in removals)
               myGameInstance.BattleStacks.Remove(mi);
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            if (false == myCallback())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateUserInstructions()
      {
         myTextBlockInstructions.Inlines.Clear();
         switch (myState)
         {
            case E0464Enum.ROLL_ADVANCE_FIRE:
               myTextBlockInstructions.Inlines.Add(new Run("Roll on "));
               Button b4 = new Button() { Content = "Friendly Action", FontFamily = myFontFam1, FontSize = 8 };
               b4.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(b4));
               myTextBlockInstructions.Inlines.Add(new Run(" Table for possible smoke or Knock-Out (KO)"));
               break;
            case E0464Enum.SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default state=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case E0464Enum.ROLL_ADVANCE_FIRE:
               Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r1);
               break;
            case E0464Enum.SHOW_RESULTS:
               System.Windows.Controls.Image img2 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img2);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateGridRows()
      {
         //------------------------------------------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //------------------------------------------------------------
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItemEnemy;
            Button b1 = CreateButton(mi);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            //----------------------------------
            StringBuilder sb = new StringBuilder();
            switch(myGridRows[i].mySector)
            {
               case '1':
               case '2':
               case '3':
                  sb.Append(myGridRows[i].mySector);
                  break;
               case '4':
                  sb.Append("4-5 ");
                  break;
               case '6':
                  sb.Append("6-8 ");
                  break;
               case '9':
                  sb.Append("9-10");
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): Reached default sector=" + myGridRows[i].mySector);
                  return false;
            }
            sb.Append(myGridRows[i].myRange);
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sb.ToString() };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 1);
            //----------------------------------
            Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myModifier.ToString() };
            myGrid.Children.Add(label2);
            Grid.SetRow(label2, rowNum);
            Grid.SetColumn(label2, 2);
            //----------------------------------
            if (PREVIOUSLY_KIA == myGridRows[i].myDieRoll)
            {
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
            }
            else if (Utilities.NO_RESULT < myGridRows[i].myDieRoll)
            {
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRoll.ToString() };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
            }
            else
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
               bmi.EndInit();
               System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "DiceRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img, bmi);
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 3);
            }
         }
         return true;
      }
      //------------------------------------------------------------------------------------
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = mi.Name;
         b.Width = mi.Zoom * Utilities.theMapItemSize;
         b.Height = mi.Zoom * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(0);
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         MapItem.SetButtonContent(b, mi, true, false); // This sets the image as the button's content
         return b;
      }
      public void ShowDieResults(int dieRoll)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myGameInstance=null");
            return;
         }
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString());
            return;
         }
         IMapItem mi = myGridRows[i].myMapItemEnemy;
         myGridRows[i].myDieRoll = dieRoll;
         myGridRows[i].myResult = TableMgr.SetFriendlyActionResult(myGameInstance, mi, dieRoll, myNumUseControlled, true, false, false, false);
         if ( "ERROR" == myGridRows[i].myResult )
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): SetFriendlyActionResult() returned ERROR");
            return;
         }
         myGameInstance.BattleStacks.Remove(myGridRows[i].myMapItemAdvance);
         //-------------------------------
         if( true == mi.IsKilled )
         {
            for (int j = 0; j < myMaxRowCount; ++j)
            {
               if ((Utilities.NO_RESULT == myGridRows[j].myDieRoll) && (myGridRows[j].myMapItemEnemy.Name == mi.Name))
               {
                  myGridRows[j].myDieRoll = PREVIOUSLY_KIA;
                  myGridRows[j].myResult = myGridRows[i].myResult;
               }
            }
         }
         //-------------------------------
         if ("Smoke" == myGridRows[i].myResult)
         {
            for (int j = 0; j < myMaxRowCount; ++j)
            {
               if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
                  myGridRows[j].myModifier = TableMgr.GetFriendlyActionModifier(myGameInstance, myGridRows[j].myMapItemEnemy, myNumUseControlled, true, false, false, false);
            }
         }
         //-------------------------------
         myState = E0464Enum.SHOW_RESULTS;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
               myState = E0464Enum.ROLL_ADVANCE_FIRE;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
         //-------------------------------
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myGameEngine=null");
            return;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myGameInstance=null");
            return;
         }
         GameAction outAction = GameAction.UpdateBattleBoard;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      //---------------------Controller Function--------------------------------------------
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
            return;
         }
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (true == key.StartsWith("r")) // rules based click
         {
            if (false == myRulesMgr.ShowRule(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowRule() returned false");
               return;
            }
         }
         else  // table based click
         {
            if (false == myRulesMgr.ShowTable(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowTable() returned false");
               return;
            }
         }
      }
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myGameInstance=null");
            return;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myCanvas=null");
            return;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myScrollViewer=null");
            return;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myRulesMgr=null");
            return;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myDieRoller=null");
            return;
         }
         //--------------------------------------------------
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            if (ui is StackPanel panel)
            {
               foreach (UIElement ui1 in panel.Children)
               {
                  if (ui1 is Image img) // Check all images within the myStackPanelAssignable
                  {
                     if (result.VisualHit == img)
                     {
                        if ("Continue" == img.Name)
                           myState = E0464Enum.END;
                        if (false == UpdateGrid())
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                        return;
                     }
                  }
               }
            }
            if (ui is Image img1) // next check all images within the Grid Rows
            {
               if (result.VisualHit == img1)
               {
                  if (false == myIsRollInProgress)
                  {
                     myRollResultRowNum = Grid.GetRow(img1);
                     myIsRollInProgress = true;
                     RollEndCallback callback = ShowDieResults;
                     myDieRoller.RollMovingDice(myCanvas, callback);
                     img1.Visibility = Visibility.Hidden;
                  }
                  return;
               }
            }
         }
      }
   }
}
