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

namespace Pattons_Best
{
   public partial class EventViewerEnemyAction : UserControl
   {
      public delegate bool EndResolveEnemyActionCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      public enum E0475Enum
      {
         ENEMY_ACTION_SELECT,
         ENEMY_ACTION_SELECT_SHOW,
         ENEMY_ACTION_MOVE,
         ENEMY_ACTION_FIRE,
         ENEMY_ACTION_FIRE_SHOW,
         ENEMY_ACTION_FIRE_YOUR_TANK,
         END
      };
      public bool CtorError { get; } = false;
      private EndResolveEnemyActionCallback? myCallback = null;
      private E0475Enum myState = E0475Enum.ENEMY_ACTION_SELECT;
      private int myMaxRowCount = 0;
      private int myRollResultRowNum = 0;
      private bool myIsRollInProgress = false;
      //---------------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItemEnemy;
         public int myDieRoll = Utilities.NO_RESULT;
         public char mySector = '0';
         public char myRange = 'E';
         public int myModifier = 0;
         public string myResult = "";
         public GridRow(IMapItem enemyUnit)
         {
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
      private EnumScenario myScenario = EnumScenario.Advance;
      //---------------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-------------------------------------------------------------------------------------
      public EventViewerEnemyAction(IGameEngine? ge, IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == ge) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCrewMgr(): ge=null");
            CtorError = true;
            return;
         }
         myGameEngine = ge;
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerResolveArtillery(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerResolveArtillery(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerResolveArtillery(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerResolveArtillery(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerResolveArtillery(): dr=null");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool PerformEnemyAction(EndResolveEnemyActionCallback callback)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): myGameInstance=null");
            return false;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): myCanvas=null");
            return false;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): myScrollViewer=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): myRulesMgr=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): myDieRoller=null");
            return false;
         }
         //--------------------------------------------------
         myCallback = callback;
         if (true == myGameInstance.IsAmbush)
            myButtonR465.Visibility = Visibility.Visible;
         //--------------------------------------------------
         if (null == myGameInstance.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): myGameInstance.EnteredArea=null");
            return false;
         }
         IStack? stack1 = myGameInstance.MoveStacks.Find(myGameInstance.EnteredArea);
         if (null == stack1)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): stack=null");
            return false;
         }
         //--------------------------------------------------
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): lastReport=null");
            return false;
         }
         myScenario = lastReport.Scenario;
         //--------------------------------------------------
         int i = 0;
         foreach(IStack stack3 in myGameInstance.BattleStacks)
         {
            foreach (IMapItem mi in stack3.MapItems)
            {
               if (true == Utilities.IsEnemyUnit(mi))
               {
                  int count = mi.TerritoryCurrent.Name.Length;
                  if (count < 3)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): count<3 for mi.TerritoryCurrent.Name=" + mi.TerritoryCurrent.Name);
                     return false;
                  }
                  char range = mi.TerritoryCurrent.Name[--count];
                  char sector = mi.TerritoryCurrent.Name[--count];
                  myGridRows[i] = new GridRow(mi);
                  myGridRows[i].myRange = range;
                  myGridRows[i].mySector = sector;
                  ++i;
               }
            }
         }
         myMaxRowCount = i;
         //--------------------------------------------------
         for(int k=0; k<myMaxRowCount; ++k )
            myGridRows[k].myModifier = TableMgr.GetEnemyActionModifier(myGameInstance, myGridRows[k].myMapItemEnemy);
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): UpdateGrid() return false");
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
         if (E0475Enum.END == myState)
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
         if (E0475Enum.END == myState)
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
                  if (true == mapItem.IsKilled)
                     removals.Add(mapItem);
               }
            }
            foreach (IMapItem mi in removals)
               myGameInstance.BattleStacks.Remove(mi);
            //-----------------------------------------
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
            case E0475Enum.ENEMY_ACTION_SELECT:
            case E0475Enum.ENEMY_ACTION_FIRE:
               myTextBlockInstructions.Inlines.Add(new Run("Roll on "));
               {
                  Button bSelect = new Button() { FontFamily = myFontFam1, FontSize = 8 };
                  bSelect.Click += ButtonRule_Click;
                  switch (myScenario)
                  {
                     case EnumScenario.Advance:
                        bSelect.Content = "Enemy Advance";
                        break;
                     case EnumScenario.Battle:
                        bSelect.Content = "Enemy Battle";
                        break;
                     case EnumScenario.Counterattack:
                        bSelect.Content = "Enemy Counterattack";
                        break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default = " + myScenario.ToString());
                        return false;
                  }
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bSelect));
               }
               myTextBlockInstructions.Inlines.Add(new Run(" Table "));
               break;
            case E0475Enum.ENEMY_ACTION_SELECT_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            case E0475Enum.ENEMY_ACTION_MOVE:
               myTextBlockInstructions.Inlines.Add(new Run("Enemy Units move per the "));
               Button bMove = new Button() { Content="Movement", FontFamily = myFontFam1, FontSize = 8 };
               bMove.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bMove));
               myTextBlockInstructions.Inlines.Add(new Run(" Diagram. Click image to continue."));
               break;
            case E0475Enum.ENEMY_ACTION_FIRE_SHOW:
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
            case E0475Enum.ENEMY_ACTION_SELECT:
            case E0475Enum.ENEMY_ACTION_FIRE:
               Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r1);
               break;
            case E0475Enum.ENEMY_ACTION_SELECT_SHOW:
               //-------------------------------
               bool isEnemyMoving = false;
               bool isEnemyFiring = false;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (true == myGridRows[j].myResult.Contains("Move"))
                  {
                     isEnemyMoving = true;
                     break;
                  }
                  if (true == myGridRows[j].myResult.Contains("Fire"))
                     isEnemyFiring = true;
               }
               if (true == isEnemyMoving)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "TigerMoving.gif", UriKind.Absolute);
                  bmi.EndInit();
                  System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "Move", Source = bmi, Width = Utilities.ZOOM * Utilities.theMapItemSize * 1.75, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myStackPanelAssignable.Children.Add(img);
                  break;
               }
               else if (true == isEnemyFiring)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "TigerFiring.gif", UriKind.Absolute);
                  bmi.EndInit();
                  System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "Fire", Source = bmi, Width = Utilities.ZOOM * Utilities.theMapItemSize * 1.75, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myStackPanelAssignable.Children.Add(img);
               }
               else
               {
                  System.Windows.Controls.Image img23 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img23);
               }
               break;
            case E0475Enum.ENEMY_ACTION_MOVE:
               bool isEnemyFiring1 = false;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (true == myGridRows[j].myResult.Contains("Fire"))
                  {
                     isEnemyFiring1 = true;
                     break;
                  }
               }
               if (true == isEnemyFiring1)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "TigerFiring.gif", UriKind.Absolute);
                  bmi.EndInit();
                  System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "Fire", Source = bmi, Width = Utilities.ZOOM * Utilities.theMapItemSize * 1.75, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myStackPanelAssignable.Children.Add(img);
               }
               else
               {
                  System.Windows.Controls.Image img23 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img23);
               }
               break;
            case E0475Enum.ENEMY_ACTION_FIRE_SHOW:
               System.Windows.Controls.Image img4 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img4);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateGridRows()
      {
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): myGameEngine=null");
            return false;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): myGameInstance=null");
            return false;
         }
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
         GameAction outAction = GameAction.UpdateBattleBoard;
         switch (myState)
         {
            case E0475Enum.ENEMY_ACTION_SELECT:
            case E0475Enum.ENEMY_ACTION_SELECT_SHOW:
               if (false == UpdateGridRowsEnemyActionSelect())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsEnemyActionSelect() returned false");
                  return false;
               }
               break;
            case E0475Enum.ENEMY_ACTION_MOVE:
               if (false == UpdateGridRowsEnemyActionMove())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsEnemyActionMove() returned false");
                  return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               break;
            case E0475Enum.ENEMY_ACTION_FIRE:
               if (false == UpdateGridRowsEnemyActionFire())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsEnemyActionFire() returned false");
                  return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateGridRowsEnemyActionSelect()
      {
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItemEnemy;
            Button b1 = CreateButton(mi);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            StringBuilder sb = new StringBuilder();
            switch (myGridRows[i].mySector)
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
            if (Utilities.NO_RESULT < myGridRows[i].myDieRoll)
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
      private bool UpdateGridRowsEnemyActionMove()
      {
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItemEnemy;
            Button b1 = CreateButton(mi);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            //---------------------------------------
            ITerritory? newT = null;
            switch (myGridRows[i].myResult)
            {
               case "Do Nothing":
               case "Fire-Infantry":
               case "Fire-Any Tank":
               case "Fire-Lead Tank":
               case "Fire-Your Tank":
               case "Collateral":
                  return true;
               case "Move-F":
               case "Move-L":
               case "Move-R":
               case "Move-B":
                  newT = TableMgr.SetNewTerritory(mi, myGridRows[i].myResult);
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsEnemyActionMove(): reached default r=" + myGridRows[i].myResult);
                  return false;
            }
            if (null == newT)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsEnemyActionMove(): SetNewTerritory() returned null");
               return false;
            }
            //--------------------------------------------
            MapItemMove mim = new MapItemMove(Territories.theTerritories, mi, newT);
            if (true == mim.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsEnemyActionMove(): mim.CtorError=true for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
               return false;
            }
            if (null == mim.NewTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsEnemyActionMove(): Invalid Parameter mim.NewTerritory=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
               return false;
            }
            if (null == mim.BestPath)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsEnemyActionMove(): Invalid Parameter mim.BestPath=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
               return false;
            }
            if (0 == mim.BestPath.Territories.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsEnemyActionMove(): Invalid State Territories.Count=" + mim.BestPath.Territories.Count.ToString() + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
               return false;
            }
            if (null == myGameInstance)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsEnemyActionMove(): myGameInstance=null");
               return false;
            }
            myGameInstance.MapItemMoves.Insert(0, mim); // add at front
         }
         return true;
      }
      private bool UpdateGridRowsEnemyActionFire()
      {
         return true;
      }
      //------------------------------------------------------------------------------------
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = mi.Name;
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(0);
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         MapItem.SetButtonContent(b, mi, false, false); // This sets the image as the button's content
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
         myGridRows[i].myDieRoll = dieRoll + myGridRows[i].myModifier;
         myGridRows[i].myResult = TableMgr.SetEnemyActionResult(myGameInstance, mi, dieRoll);
         if ( "ERROR" == myGridRows[i].myResult )
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): SetFriendlyActionResult() returned ERROR");
            return;
         }
         //-------------------------------
         switch(myState)
         {
            case E0475Enum.ENEMY_ACTION_SELECT:
               myState = E0475Enum.ENEMY_ACTION_SELECT_SHOW;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
                     myState = E0475Enum.ENEMY_ACTION_SELECT;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default myState=" + myState.ToString());
               return;
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
                        if ("Move" == img.Name)
                           myState = E0475Enum.ENEMY_ACTION_MOVE;
                        if ("Fire" == img.Name)
                           myState = E0475Enum.ENEMY_ACTION_FIRE;
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
