using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
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
using Windows.ApplicationModel.Activation;
using WpfAnimatedGif;
using static Pattons_Best.EventViewerBattleSetup;
using static System.Windows.Forms.AxHost;

namespace Pattons_Best
{
   public partial class EventViewerChangeFacing : UserControl
   {
      public delegate bool EndResolveChangeFacingCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int THROWN_TRACK = 106;
      public enum E04741Enum
      {
         ENEMY_FACING_ROLL,
         ENEMY_FACING_ROLL_SHOW,
         END
      };
      public bool CtorError { get; } = false;
      private EndResolveChangeFacingCallback? myCallback = null;
      private E04741Enum myState = E04741Enum.ENEMY_FACING_ROLL;
      private int myMaxRowCount = 0;
      private int myRollResultRowNum = 0;
      private int myRollResultColNum = 0;
      private bool myIsRollInProgress = false;
      //============================================================
      public struct GridRow
      {
         public IMapItem myMapItem;
         //---------------------------------------------------
         public char mySector = 'E';
         public char myRange = 'E';
         public string mySectorRangeDisplay = "UNINT";
         public string myFacingOld = "NA";
         public int myDieRollFacing = Utilities.NO_RESULT;
         public string myFacingNew = "NA";
         //---------------------------------------------------
         public GridRow(IMapItem enemyUnit)
         {
            myMapItem = enemyUnit;
         }
      };
      private GridRow[] myGridRows = new GridRow[10];
      //============================================================
      private IGameEngine? myGameEngine;
      private IGameInstance? myGameInstance;
      private readonly Canvas? myCanvas;
      private readonly ScrollViewer? myScrollViewer;
      private RuleDialogViewer? myRulesMgr;
      private IDieRoller? myDieRoller;
      //---------------------------------------------------
      private EnumScenario myScenario = EnumScenario.None;
      private string myAreaType = "UNINT";
      //---------------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-------------------------------------------------------------------------------------
      public EventViewerChangeFacing(IGameEngine? ge, IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
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
      public bool PerformFacingChange(EndResolveChangeFacingCallback callback)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): myGameInstance=null");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "PerformEnemyAction(): ++++++++++++++++++++++++++++++ battlestacks=" + myGameInstance.BattleStacks.ToString());
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
         //--------------------------------------------------
         if (null == myGameInstance.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): myGameInstance.EnteredArea=null");
            return false;
         }
         myAreaType = myGameInstance.EnteredArea.Type;
         //--------------------------------------------------
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "PerformEnemyAction(): myGameInstance.MapItemMoves.Clear()");
         myGameInstance.MapItemMoves.Clear();
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
            Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): lastReport=null");
            return false;
         }
         myScenario = lastReport.Scenario;
         //--------------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "PerformEnemyAction(): BattleStacks=" + myGameInstance.BattleStacks.ToString());
         int i = 0;
         foreach(IStack stack3 in myGameInstance.BattleStacks)
         {
            foreach (IMapItem mi in stack3.MapItems)
            {
               if (true == mi.IsEnemyUnit())
               {
                  if (false == mi.IsVehicle())
                     continue;
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
                  myGridRows[i].mySectorRangeDisplay = GetSectorRangeDisplay(i);
                  if( "ERROR" == myGridRows[i].mySectorRangeDisplay )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "PerformEnemyAction(): count<3 for mi.TerritoryCurrent.Name=" + mi.TerritoryCurrent.Name);
                     return false;
                  }
                  myGridRows[i].myFacingOld = TableMgr.GetEnemyFacingFromRotationAndSector(mi);
                  if (true == mi.IsThrownTrack)
                  {
                     myGridRows[i].myDieRollFacing = THROWN_TRACK;
                     myGridRows[i].myFacingNew = myGridRows[i].myFacingOld;
                  }
                  ++i;
               }
            }
         }
         myMaxRowCount = i;
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
         if (E04741Enum.END == myState)
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
         if (E04741Enum.END == myState)
         {
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
            case E04741Enum.ENEMY_FACING_ROLL:
               myTextBlockInstructions.Inlines.Add(new Run("Roll all dice for all rows."));
               break;
            case E04741Enum.ENEMY_FACING_ROLL_SHOW:
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
            case E04741Enum.ENEMY_FACING_ROLL:
               Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r1);
               break;
            case E04741Enum.ENEMY_FACING_ROLL_SHOW:
               System.Windows.Controls.Image img23 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img23);
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
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            GridRow row = myGridRows[i];
            IMapItem mi = row.myMapItem;
            Button b = CreateButton(mi);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //--------------------------
            Label locationLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].mySectorRangeDisplay };
            myGrid.Children.Add(locationLabel);
            Grid.SetRow(locationLabel, rowNum);
            Grid.SetColumn(locationLabel, 1);
            //--------------------------
            Label oldFacingLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myFacingOld };
            myGrid.Children.Add(oldFacingLabel);
            Grid.SetRow(oldFacingLabel, rowNum);
            Grid.SetColumn(oldFacingLabel, 2);
            //-------------------------------
            if( THROWN_TRACK == myGridRows[i].myDieRollFacing)
            {
               Label dieRollLabel = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "Thrown Track" };
               myGrid.Children.Add(dieRollLabel);
               Grid.SetRow(dieRollLabel, rowNum);
               Grid.SetColumn(dieRollLabel, 3);
               Label newFacingLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myFacingNew };
               myGrid.Children.Add(newFacingLabel);
               Grid.SetRow(newFacingLabel, rowNum);
               Grid.SetColumn(newFacingLabel, 4);

            }
            else if (Utilities.NO_RESULT < myGridRows[i].myDieRollFacing)
            {
               Label dieRollLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollFacing.ToString() };
               myGrid.Children.Add(dieRollLabel);
               Grid.SetRow(dieRollLabel, rowNum);
               Grid.SetColumn(dieRollLabel, 3);
               Label newFacingLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myFacingNew };
               myGrid.Children.Add(newFacingLabel);
               Grid.SetRow(newFacingLabel, rowNum);
               Grid.SetColumn(newFacingLabel, 4);
            }
            else
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollWhite.gif", UriKind.Absolute);
               bmi.EndInit();
               System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "FacingChange", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
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
         b.Name = Utilities.RemoveSpaces(mi.Name);
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(0);
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         MapItem.SetButtonContent(b, mi, false, false, false); // This sets the image as the button's content
         return b;
      }
      private string GetSectorRangeDisplay(int i)
      {
         StringBuilder sb = new StringBuilder();
         switch (myGridRows[i].mySector)
         {
            case '1':
               sb.Append("1 ");
               sb.Append(myGridRows[i].myRange);
               break;
            case '2':
               sb.Append("2 ");
               sb.Append(myGridRows[i].myRange);
               break;
            case '3':
               sb.Append("3 ");
               sb.Append(myGridRows[i].myRange);
               break;
            case '4':
               sb.Append("4-5 ");
               sb.Append(myGridRows[i].myRange);
               break;
            case '6':
               sb.Append("6-8 ");
               sb.Append(myGridRows[i].myRange);
               break;
            case '9':
               sb.Append("9-10 ");
               sb.Append(myGridRows[i].myRange);
               break;
            case 'O':
               sb.Append("Off");
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "EventViewerChangeFacing.GetSectorRangeDisplay(): Reached default sector=" + myGridRows[i].mySector + " for mi=" + myGridRows[i].myMapItem.Name + " t=" + myGridRows[i].myMapItem.TerritoryCurrent.Name);
               return "ERROR";
         }
         Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "GetSectorRangeDisplay(): loc=" + sb.ToString());
         return sb.ToString();
      }
      //------------------------------------------------------------------------------------
      public void ShowDieResults(int dieRoll)
      {
         Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "EventViewerChangeFacing.ShowDieResults(): ++++++++++++++myState=" + myState.ToString());
         if( null == myGameInstance )
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerChangeFacing.ShowDieResults(): myGameInstance=null");
            return;
         }
         //-------------------------------
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString());
            return;
         }
         //-------------------------------
         myGridRows[i].myDieRollFacing = dieRoll;
         IMapItem mi = myGridRows[i].myMapItem;
         if (true == mi.IsThrownTrack)
         {
            myGridRows[i].myFacingNew = myGridRows[i].myFacingOld;  // do not change facing if thrown track
         }
         else
         {
            myGridRows[i].myFacingNew = TableMgr.GetEnemyNewFacing(myGameInstance, mi, dieRoll);
            if ("ERROR" == myGridRows[i].myFacingNew)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): TableMgr.Get_EnemyNewFacing() returned ERROR");
               return;
            }
         }
         if (false == ShowDieResultUpdateFacing(i))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): ShowDieResult_UpdateFacing() returned false");
            return;
         }
         //-------------------------------
         myState = E04741Enum.ENEMY_FACING_ROLL_SHOW;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            if (Utilities.NO_RESULT == myGridRows[j].myDieRollFacing)
               myState = E04741Enum.ENEMY_FACING_ROLL;
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
         Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "EventViewerChangeFacing.ShowDieResults(): ---------------myState=" + myState.ToString());
      }
      private bool ShowDieResultUpdateFacing(Index i)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myGameInstance=null");
            return false;
         }
         //----------------------------
         IMapItem? mi = myGridRows[i].myMapItem;
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResult_UpdateFacing(): mi=null for i=" + i.ToString());
            return false;
         }
         if( false == mi.SetMapItemRotation(myGameInstance.Sherman))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResult_UpdateFacing(): Set_MapItemRotation() returned false");
            return false;
         }
         if (false == mi.UpdateMapRotation(myGridRows[i].myFacingNew))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResult_UpdateFacing(): Update_MapRotation() returned false for i=" + i.ToString());
            return false;
         }
         return true;
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
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myCanvas=null");
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
                           myState = E04741Enum.END;
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
                     myRollResultColNum = Grid.GetColumn(img1);
                     myRollResultRowNum = Grid.GetRow(img1);
                     myIsRollInProgress = true;
                     myDieRoller.RollMovingDie(myCanvas, ShowDieResults);
                     img1.Visibility = Visibility.Hidden;
                  }
                  return;
               }
            }
         }
      }
   }
}
