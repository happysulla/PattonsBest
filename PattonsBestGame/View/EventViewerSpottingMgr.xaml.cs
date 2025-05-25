
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
using Windows.System.RemoteSystems;
using WpfAnimatedGif;

namespace Pattons_Best
{
   public partial class EventViewerSpottingMgr : UserControl
   {
      public delegate bool EndSpottingMgrCallback();
      private const int MAX_GRID_LEN = 5;
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int SPOTTED = 100;
      public enum E0472Enum
      {
         SELECT_CREWMAN,
         SELECT_CREWMAN_SHOW,
         ROLL_SPOTTING,
         ROLL_SPOTTING_SHOW,
         END
      };
      public bool CtorError { get; } = false;
      private EndSpottingMgrCallback? myCallback = null;
      private E0472Enum myState = E0472Enum.ROLL_SPOTTING;
      private int myMaxRowCount = 5;
      private int myRollResultRowNum = 0;
      private bool myIsRollInProgress = false;
      //---------------------------------------------------
      private class GridRow
      {
         public IMapItem myMapItem;
         public char mySector = 'E';
         public char myRange = 'E';
         public string mySectorRangeDisplay = "ERROR";
         public int myModifier = 0;
         public int myDieRoll = Utilities.NO_RESULT;
         public string myResult = "UNINITIALIZED";
         public GridRow(IMapItem enemyUnit)
         {
            myMapItem = enemyUnit;
         }
      };
      private GridRow[] myGridRows = new GridRow[MAX_GRID_LEN]; // five possible crew members
      //---------------------------------------------------
      private IGameEngine? myGameEngine;
      private IGameInstance? myGameInstance;
      private readonly Canvas? myCanvas;
      private readonly ScrollViewer? myScrollViewer;
      private RuleDialogViewer? myRulesMgr;
      private IDieRoller? myDieRoller;
      //---------------------------------------------------
      private Dictionary<string, EnumSpottingResult>  mySpottings = new Dictionary<string, EnumSpottingResult>();
      private IMapItems myAssignables = new MapItems();    // listing of new crewmen 
      private ICrewMember? mySelectedCrewman = null;
      //---------------------------------------------------
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //-------------------------------------------------------------------------------------
      public EventViewerSpottingMgr(IGameEngine? ge, IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == ge) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerSpottingMgr(): ge=null");
            CtorError = true;
            return;
         }
         myGameEngine = ge;
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerSpottingMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerSpottingMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerSpottingMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerSpottingMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerSpottingMgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool PerformSpotting(EndSpottingMgrCallback callback)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformSpotting(): myGameInstance=null");
            return false;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformSpotting(): myCanvas=null");
            return false;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformSpotting(): myScrollViewer=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformSpotting(): myRulesMgr=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformSpotting(): myDieRoller=null");
            return false;
         }
         //--------------------------------------------------
         myCallback = callback;
         myState = E0472Enum.SELECT_CREWMAN;
         myIsRollInProgress = false;
         myRollResultRowNum = 0;
         mySelectedCrewman = null;
         myMaxRowCount = 0;
         //--------------------------------------------------
         string[] crewmembers = new string[5] { "Driver", "Assistant", "Commander", "Loader", "Gunner" };
         foreach (string crewmember in crewmembers)
         {
            ICrewMember? cm = myGameInstance.GetCrewMember(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformSpotting(): cm=null for name=" + crewmember);
               return false;
            }
            cm.Name = cm.Role;
            List<string>? spottedTerritories = Territory.GetSpottedTerritories(myGameInstance, cm);
            if (null == spottedTerritories)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformSpotting(): GetSpottedTerritories() returned null for cm=" + cm.Role);
               return false;
            }
            if( 0 < spottedTerritories.Count)
               myAssignables.Add(cm);
         }
         //--------------------------------------------------
         foreach(IMapItem enemyUnit in myGameInstance.BattleStacks) // get original spotting results
         {
            if( true == enemyUnit.IsEnemyUnit())
               mySpottings[enemyUnit.Name] = enemyUnit.Spotting;
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformSpotting(): UpdateGrid() return false");
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
         if (E0472Enum.END == myState)
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
         if (E0472Enum.END == myState)
         {
            if( null == myGameInstance)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myGameInstance=null");
               return false;
            }
            foreach( string s in mySpottings.Keys )
            {
               IMapItem? enemy = myGameInstance.BattleStacks.FindMapItem(s);
               if (null == enemy)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): enemy=null for s=" + s);
                  return false;
               }
               enemy.Spotting = mySpottings[s];
            }
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
            case E0472Enum.SELECT_CREWMAN:
               if( 0 == myAssignables.Count)
                  myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Select a crewman to perform spotting by clicking it."));
               break;
            case E0472Enum.SELECT_CREWMAN_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die to determine if enemy unit is spotted or hidden."));
               break;
            case E0472Enum.ROLL_SPOTTING:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for each enemy unit that can be spotted."));
               break;
            case E0472Enum.ROLL_SPOTTING_SHOW:
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
         Logger.Log(LogEnum.LE_EVENT_VIEWER_SPOTTING, "UpdateAssignablePanel(): myState=" + myState.ToString());
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case E0472Enum.SELECT_CREWMAN:
               if( 0 < myAssignables.Count )
               {
                  foreach (IMapItem mi in myAssignables)
                  {
                     ICrewMember? cm = mi as ICrewMember;
                     if (null == cm)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): cast of mi to cm failed");
                        return false;
                     }
                     Button b = CreateButton(cm);
                     b.Click += Button_Click;
                     myStackPanelAssignable.Children.Add(b);
                  }
               }
               else
               {
                  Image img1 = new Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img1);
               }
               break;
            case E0472Enum.SELECT_CREWMAN_SHOW:
            case E0472Enum.ROLL_SPOTTING:
               Rectangle r2 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r2);
               Rectangle r3 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r3);
               if ( null == mySelectedCrewman )
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): mySelectedCrewman=null");
                  return false;
               }
               Button b1 = CreateButton(mySelectedCrewman);
               myStackPanelAssignable.Children.Add(b1);
               Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = " rating = " + mySelectedCrewman.Rating.ToString() };
               myStackPanelAssignable.Children.Add(label1);
               break;
            case E0472Enum.ROLL_SPOTTING_SHOW:
               Image img2 = new Image { Name = "ContinueNext", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img2);
               Rectangle r4 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r4);
               if (null == mySelectedCrewman)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): mySelectedCrewman=null");
                  return false;
               }
               Button b2 = CreateButton(mySelectedCrewman);
               myStackPanelAssignable.Children.Add(b2);
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = " rating = " + mySelectedCrewman.Rating.ToString() };
               myStackPanelAssignable.Children.Add(label2);
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
         Logger.Log(LogEnum.LE_EVENT_VIEWER_SPOTTING, "UpdateGridRows(): myState=" + myState.ToString() + " myMaxRowCount=" + myMaxRowCount.ToString());
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            GridRow row = myGridRows[i];
            IMapItem enemyUnit = row.myMapItem;
            Button b = CreateButton(enemyUnit);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //----------------------------------
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = GetSectorRangeDisplay(i) };
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
               //----------------------------------
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
            }
            else
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollWhite.gif", UriKind.Absolute);
               bmi.EndInit();
               System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "DieRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
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
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(0);
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         MapItem.SetButtonContent(b, mi, false, false); // This sets the image as the button's content
         return b;
      }
      private Button CreateButton(ICrewMember cm)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = cm.Role;
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(0);
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         MapItem.SetButtonContent(b, cm, true, true); // This sets the image as the button's content
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
               Logger.Log(LogEnum.LE_ERROR, "GetSectorRangeDisplay(): Reached default sector=" + myGridRows[i].mySector);
               return "ERROR";
         }
         return sb.ToString();
      }
      //------------------------------------------------------------------------------------
      public void ShowDieResults(int dieRoll)
      {
         Logger.Log(LogEnum.LE_EVENT_VIEWER_SPOTTING, "EventViewerSpottingMgr.ShowDieResults(): +++++++++++++++++myState=" + myState.ToString() + " dr=" + dieRoll.ToString() );
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myGameInstance=null");
            return;
         }
         if (null == mySelectedCrewman)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): mySelectedCrewman=null");
            return;
         }
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowCombatResults(): 0 > i=" + i.ToString());
            return;
         }
         IMapItem mi = myGridRows[i].myMapItem;
         myGridRows[i].myDieRoll = dieRoll + myGridRows[i].myModifier;
         string spottingResult = TableMgr.GetSpottingResult(myGameInstance, mi, mySelectedCrewman, myGridRows[i].mySector, myGridRows[i].myRange, dieRoll);
         if ("ERROR" == myGridRows[i].myResult)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowCombatResults(): GetSpottingResult() returned error for i=" + i.ToString());
            return;
         }
         else if ( "Identified" == myGridRows[i].myResult )
         { 
            if (EnumSpottingResult.IDENTIFIED_HIDDEN == mySpottings[mi.Name])
            {
               spottingResult = "Already Hidden";
            }
            else if (EnumSpottingResult.SPOTTED_HIDDEN == mySpottings[mi.Name])
            {
               spottingResult = "Already Hidden";
            }
            else
            {
               mySpottings[mi.Name] = EnumSpottingResult.IDENTIFIED;
               spottingResult = "Identified";
            }
         }
         else if ("Spotted" == myGridRows[i].myResult)
         {
            if (EnumSpottingResult.IDENTIFIED_HIDDEN == mySpottings[mi.Name])
            {
               spottingResult = "Already Hidden";
            }
            else if (EnumSpottingResult.SPOTTED_HIDDEN == mySpottings[mi.Name])
            {
               spottingResult = "Already Hidden";
            }
            if (EnumSpottingResult.IDENTIFIED == mySpottings[mi.Name])
            {
               spottingResult = "Already Identified";
            }
            else
            {
               spottingResult = "Spotted";
               mySpottings[mi.Name] = EnumSpottingResult.SPOTTED;
            }
         }
         else if ("Unspotted" == myGridRows[i].myResult)
         {
            if (EnumSpottingResult.IDENTIFIED_HIDDEN == mySpottings[mi.Name])
               spottingResult = "Already Hidden";
            else if (EnumSpottingResult.SPOTTED_HIDDEN == mySpottings[mi.Name])
               spottingResult = "Already Hidden";
            else if (EnumSpottingResult.IDENTIFIED == mySpottings[mi.Name])
               spottingResult = "Already Identified";
            else if (EnumSpottingResult.SPOTTED == mySpottings[mi.Name])
               spottingResult = "Already Spotted";
            else
               spottingResult = "Unspotted";
         }
         else if ("Hidden" == myGridRows[i].myResult)
         {
            if (EnumSpottingResult.SPOTTED == mySpottings[mi.Name])
               mySpottings[mi.Name] = EnumSpottingResult.SPOTTED_HIDDEN;
            else if (EnumSpottingResult.IDENTIFIED == mySpottings[mi.Name])
               mySpottings[mi.Name] = EnumSpottingResult.IDENTIFIED_HIDDEN;
            else
               mySpottings[mi.Name] = EnumSpottingResult.HIDDEN;
            spottingResult = "Hidden";
         }
         myGridRows[i].myResult = spottingResult;
         //------------------------------------
         myState = E0472Enum.ROLL_SPOTTING_SHOW;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
               myState = E0472Enum.ROLL_SPOTTING;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
         Logger.Log(LogEnum.LE_EVENT_VIEWER_SPOTTING, "ShowDieResults(): -----------------myState=" + myState.ToString());
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
         if (false == myRulesMgr.ShowRule(key))
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false key=" + key);
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
                           myState = E0472Enum.END;
                        if ("ContinueNext" == img.Name)
                        {
                           mySelectedCrewman = null;
                           IMapItems removals = new MapItems();
                           foreach(ICrewMember cm in myAssignables)
                           {
                              List<string>? newSpottedTerritories = Territory.GetSpottedTerritories(myGameInstance, cm);
                              if (null == newSpottedTerritories)
                              {
                                 Logger.Log(LogEnum.LE_ERROR, "PerformSpotting(): GetSpottedTerritories() returned null for cm=" + cm.Role);
                                 return;
                              }
                              if (0 == newSpottedTerritories.Count)
                                 removals.Add(cm);
                           }
                           foreach (ICrewMember cm in removals) // Remove crew members that have no spotting opportunity
                              myAssignables.Remove(cm);
                           if (0 < myAssignables.Count)
                              myState = E0472Enum.SELECT_CREWMAN;
                           else
                              myState = E0472Enum.END;
                        }
                        if ("DieRoll" == img.Name)
                        {
                           if (false == myIsRollInProgress)
                           {
                              myIsRollInProgress = true;
                              RollEndCallback callback = ShowDieResults;
                              myDieRoller.RollMovingDie(myCanvas, callback);
                              img.Visibility = Visibility.Hidden;
                           }
                           return;
                        }
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
                     myDieRoller.RollMovingDie(myCanvas, callback);
                     img1.Visibility = Visibility.Hidden;
                  }
                  return;
               }
            }
         }
      }
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): myGameInstance=null");
            return;
         }
         Button b = (Button)sender;
         if (true == String.IsNullOrEmpty(b.Name))
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): b.Name=null");
            return;
         }
         if (null != mySelectedCrewman)
            return;
         mySelectedCrewman = myGameInstance.GetCrewMember(b.Name);
         if ( null == mySelectedCrewman )
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): myGameInstance.GetCrewMember() returned null for cm=" + b.Name);
            return;
         }
         myAssignables.Remove(b.Name);
         //---------------------------------------
         myState = E0472Enum.SELECT_CREWMAN_SHOW;
         List<string>? spottedTerritories = Territory.GetSpottedTerritories(myGameInstance, mySelectedCrewman);
         if( null == spottedTerritories )
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): GetSpottedTerritories() returned null for cm=" + b.Name);
            return;
         }
         StringBuilder sb = new StringBuilder("Button_Click(): spottedTerritories=[");
         int i = 0;
         foreach ( string tName in spottedTerritories )
         {
            sb.Append(tName);
            int count = tName.Length;
            if (3 != count)
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): length not 3 for tName=" + tName);
               return;
            }
            IStack? stack = myGameInstance.BattleStacks.Find(tName);
            if( null != stack )
            {
               foreach (IMapItem mi in stack.MapItems )
               {
                  sb.Append("=(");
                  sb.Append(mi.Name);
                  sb.Append(")");
                  if( (true == mi.Name.Contains("ATG")) || (true == mi.Name.Contains("TANK")) || (true == mi.Name.Contains("SPG")) )
                  {
                     sb.Append("a");
                     myGridRows[i] = new GridRow(mi);
                     myGridRows[i].myRange = tName[count-1];
                     myGridRows[i].mySector = tName[count-2];
                     myGridRows[i].mySectorRangeDisplay = GetSectorRangeDisplay(i);
                     myGridRows[i].myModifier = TableMgr.GetSpottingModifier(myGameInstance, mi, mySelectedCrewman, myGridRows[i].mySector, myGridRows[i].myRange);
                     if( myGridRows[i].myModifier < -100 )
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Button_Click(): invalid mod=" + myGridRows[i].myModifier);
                        return;
                     }
                     i++;
                  }
               }
            }
            if( i != (spottedTerritories.Count-1 ) )
             sb.Append(',');
         }
         myMaxRowCount = i;
         Logger.Log(LogEnum.LE_EVENT_VIEWER_SPOTTING, "Button_Click():myState=" + myState.ToString() + " myMaxRowCount=" + myMaxRowCount.ToString() + " " + sb.ToString() + "]");
         //---------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): UpdateGrid() return false");
            return;
         }
      }
   }
}
