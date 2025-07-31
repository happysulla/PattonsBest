using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using WpfAnimatedGif;

namespace Pattons_Best
{
   public partial class EventViewerBattleSetup : UserControl
   {
      public delegate bool EndBattleSetupCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int NO_FACING = -1;
      public enum E046Enum
      {
         ACTIVATION,
         SPW_OR_PSW_ROLL,
         PLACE_SECTOR,
         PLACE_RANGE,
         PLACE_FACING,
         PLACE_TERRAIN,
         SHOW_RESULTS,
         END,
         ERROR
      };
      public bool CtorError { get; } = false;
      private EndBattleSetupCallback? myCallback = null;
      private E046Enum myState = E046Enum.ERROR;
      private int myMaxRowCount = 0;
      private int myRollResultRowNum = 0;
      private bool myIsRollInProgress = false;
      private EnumScenario myScenario = EnumScenario.None;
      private int myDay = 0;
      private bool myIsVehicleActivated = false;
      private bool[] myIsSectorUsControlled = new bool[6] { false, false, false, false, false, false };
      private string myAreaType = "ERROR";
      //---------------------------------------------------
      public struct GridRow
      {
         public IMapItem? myMapItem;
         public string myActivation;
         public string mySector;
         public string myRange;
         public string myFacing;
         public string myTerrain;
         public int myDieRollActivation;
         public int myDieRollSector;
         public int myDieRollRange;
         public int myDieRollFacing;
         public int myDieRollTerrain;
         public GridRow()
         {
            myMapItem = null;
            myActivation = "Unknown";
            mySector = "Unknown";
            myRange = "Unknown";
            myFacing = "Unknown";
            myTerrain = "Unknown";
            myDieRollActivation = Utilities.NO_RESULT;
            myDieRollSector = Utilities.NO_RESULT;
            myDieRollRange = Utilities.NO_RESULT;
            myDieRollFacing = Utilities.NO_RESULT;
            myDieRollTerrain = Utilities.NO_RESULT;
         }
      };
      private GridRow[] myGridRows = new GridRow[4]; // five possible crew members
      //---------------------------------------------------
      private IGameEngine? myGameEngine;
      private IGameInstance? myGameInstance;
      private readonly Canvas? myCanvas;
      private readonly ScrollViewer? myScrollViewer;
      private RuleDialogViewer? myRulesMgr;
      private IDieRoller? myDieRoller;
      //---------------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      //-------------------------------------------------------------------------------------
      public EventViewerBattleSetup(IGameEngine? ge, IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
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
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCrewMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCrewMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCrewMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCrewMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCrewMgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool SetupBattle(EndBattleSetupCallback callback)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetupBattle(): myGameInstance=null");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "SetupBattle(): ++++++++++++++++++++++++++++++ battlestacks=" + myGameInstance.BattleStacks.ToString());
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetupBattle(): myCanvas=null");
            return false;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetupBattle(): myScrollViewer=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetupBattle(): myRulesMgr=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetupBattle(): myDieRoller=null");
            return false;
         }
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): lastReport=null");
            return false;
         }

         //--------------------------------------------------
         myState = E046Enum.ACTIVATION;
         myMaxRowCount = 0;
         myRollResultRowNum = 0;
         myIsRollInProgress = false;
         myScenario = lastReport.Scenario;
         myDay = myGameInstance.Day;
         myCallback = callback;
         //--------------------------------------------------
         if (null == myGameInstance.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetupBattle(): myGameInstance.EnteredArea=null");
            return false;
         }
         myAreaType = myGameInstance.EnteredArea.Type;
         //--------------------------------------------------
         if( GamePhase.Battle == myGameInstance.GamePhase )
         {
            IStack? stack = myGameInstance.MoveStacks.Find(myGameInstance.EnteredArea);
            if (null == stack)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetupBattle(): stack=null");
               return false;
            }
            foreach (IMapItem mi1 in stack.MapItems)  // determine how many to activiate based on enemy strength in area
            {
               if (true == mi1.Name.Contains("Strength"))
               {
                  if (1 == mi1.Count)
                     myMaxRowCount = 2;
                  else if (2 == mi1.Count)
                     myMaxRowCount = 3;
                  else if (3 == mi1.Count)
                     myMaxRowCount = 4;
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "SetupBattle(): reached default mi1.Count =" + mi1.Count.ToString());
                     return false;
                  }
                  break;
               }
            }
         }
         else
         {
            if (EnumScenario.Advance == lastReport.Scenario) // activate one additional for Advance and two additional for Battle | Counterattack
               myMaxRowCount = 1;
            else
               myMaxRowCount = 2;
         }
         //--------------------------------------------------
         string[] sectors = new string[6] {"B1M", "B2M", "B3M", "B4M", "B6M", "B9M" };
         int i = 0;
         foreach (string sector in sectors )
         {
            myIsSectorUsControlled[i] = false;
            ITerritory? t = Territories.theTerritories.Find(sector);
            if( null == t )
            {
               Logger.Log(LogEnum.LE_ERROR, "SetupBattle(): t=null for s=" + sector);
               return false;
            }
            IStack? stack1 = myGameInstance.BattleStacks.Find(t);
            if (null == stack1)
            {
               ++i;
               continue;
            }
            foreach(IMapItem mi in stack1.MapItems)
            {
               if( true == mi.Name.Contains("UsControl"))
               {
                  myIsSectorUsControlled[i] = true;
                  break;
               }
            }
            ++i;
         }
         //--------------------------------------------------
         for (int i1 = 0; i1 < myMaxRowCount; ++i1)
            myGridRows[i1] = new GridRow();
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "SetupBattle(): UpdateGrid() return false");
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
         if (E046Enum.END == myState)
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
         if (E046Enum.END == myState)
         {
            if( null == myGameInstance)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myGameInstance=null");
               return false;
            }
            //-----------------------------------------
            IMapItems removals = new MapItems();
            foreach (IStack stack in myGameInstance.BattleStacks)
            {
               foreach (IMapItem mapItem in stack.MapItems)
               {
                  if (true == mapItem.TerritoryCurrent.Name.Contains("Off")) // remove all units that left the board
                     removals.Add(mapItem);
               }
            }
            foreach (IMapItem mi in removals)
               myGameInstance.BattleStacks.Remove(mi);
            Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "EventViewerBattleSetup.UpdateEndState(): ------------------------------ battlestacks=" + myGameInstance.BattleStacks.ToString());
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
            case E046Enum.ACTIVATION:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for each enemy unit on the "));
               Button b1 = new Button() { Content = "Activation", FontFamily = myFontFam1, FontSize = 8 };
               b1.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(b1));
               myTextBlockInstructions.Inlines.Add(new Run(" Table for appearance of enemy units."));
               break;
            case E046Enum.SPW_OR_PSW_ROLL:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for SPW or PSW on the "));
               Button b2 = new Button() { Content = "Enemy Appearance", FontFamily = myFontFam1, FontSize = 8 };
               b2.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(b2));
               myTextBlockInstructions.Inlines.Add(new Run(" Table."));
               break;
            case E046Enum.PLACE_SECTOR:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for each enemy unit to determine sector."));
               myButtonR462.Visibility = Visibility.Hidden;
               myButtonR174.Visibility = Visibility.Hidden;
               myButtonR512.Visibility = Visibility.Visible;
               break;
            case E046Enum.PLACE_RANGE:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for range on the Battle "));
               Button b3 = new Button() { Content = "Placement", FontFamily = myFontFam1, FontSize = 8 };
               b3.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(b3));
               myTextBlockInstructions.Inlines.Add(new Run(" Table."));
               myButtonR512.Visibility = Visibility.Hidden;
               myButtonR1232.Visibility = Visibility.Visible;
               break;
            case E046Enum.PLACE_FACING:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for facing on the Battle "));
               Button b4 = new Button() { Content = "Placement", FontFamily = myFontFam1, FontSize = 8 };
               b4.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(b4));
               myTextBlockInstructions.Inlines.Add(new Run(" Table."));
               myButtonR1232.Visibility = Visibility.Hidden;
               myButtonR1233.Visibility = Visibility.Visible;
               break;
            case E046Enum.PLACE_TERRAIN:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for terrain on the Battle "));
               Button b5 = new Button() { Content = "Placement", FontFamily = myFontFam1, FontSize = 8 };
               b5.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(b5));
               myTextBlockInstructions.Inlines.Add(new Run(" Table."));
               myButtonR1233.Visibility = Visibility.Hidden;
               myButtonR1234.Visibility = Visibility.Visible;
               break;
            case E046Enum.SHOW_RESULTS:
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
         Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM*Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case E046Enum.ACTIVATION:
            case E046Enum.PLACE_SECTOR:
            case E046Enum.PLACE_RANGE:
            case E046Enum.PLACE_FACING:
            case E046Enum.PLACE_TERRAIN:
               myStackPanelAssignable.Children.Add(r1);
               break;
            case E046Enum.SPW_OR_PSW_ROLL:
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollWhite.gif", UriKind.Absolute);
               bmi.EndInit();
               System.Windows.Controls.Image img1 = new System.Windows.Controls.Image { Name = "DieRoll", Source = bmi, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img1, bmi);
               myStackPanelAssignable.Children.Add(img1);
               break;
            case E046Enum.SHOW_RESULTS:
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
            switch (myState)
            {
               case E046Enum.ACTIVATION:
                  if( Utilities.NO_RESULT < myGridRows[i].myDieRollActivation)
                  {
                     Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollActivation.ToString() };
                     myGrid.Children.Add(label1);
                     Grid.SetRow(label1, rowNum);
                     Grid.SetColumn(label1, 0);
                     //--------------------------
                     IMapItem? mi1 = myGridRows[i].myMapItem;
                     if( null == mi1)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): myGridRows[i].myMapItem=null for i=" + i);
                        return false;
                     }
                     Button b1 = CreateButton(mi1);
                     myGrid.Children.Add(b1);
                     Grid.SetRow(b1, rowNum);
                     Grid.SetColumn(b1, 1);
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
                     Grid.SetColumn(img, 0);
                     //--------------------------
                     Rectangle r = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
                     myGrid.Children.Add(r);
                     Grid.SetRow(r, rowNum);
                     Grid.SetColumn(r, 1);
                  }
                  break;
               case E046Enum.SPW_OR_PSW_ROLL:
                  Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollActivation.ToString() };
                  myGrid.Children.Add(label2);
                  Grid.SetRow(label2, rowNum);
                  Grid.SetColumn(label2, 0);
                  break;
               case E046Enum.PLACE_SECTOR:
                  Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollActivation.ToString() };
                  myGrid.Children.Add(label4);
                  Grid.SetRow(label4, rowNum);
                  Grid.SetColumn(label4, 0);
                  //-----------------------
                  IMapItem? mi4 = myGridRows[i].myMapItem;
                  if (null == mi4)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): mi4=null for i=" + i);
                     return false;
                  }
                  Button b4 = CreateButton(mi4);
                  myGrid.Children.Add(b4);
                  Grid.SetRow(b4, rowNum);
                  Grid.SetColumn(b4, 1);
                  //-----------------------
                  if (Utilities.NO_RESULT < myGridRows[i].myDieRollSector)
                  {
                     Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].mySector };
                     myGrid.Children.Add(label1);
                     Grid.SetRow(label1, rowNum);
                     Grid.SetColumn(label1, 2);
                  }
                  else
                  {
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollWhite.gif", UriKind.Absolute);
                     bmi.EndInit();
                     System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name="DieRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 2);
                  }
                  break;
               case E046Enum.PLACE_RANGE:
                  Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollActivation.ToString() };
                  myGrid.Children.Add(label5);
                  Grid.SetRow(label5, rowNum);
                  Grid.SetColumn(label5, 0);
                  //-----------------------
                  IMapItem? mi5 = myGridRows[i].myMapItem;
                  if (null == mi5)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): myGridRows[i].myMapItem=null for i=" + i);
                     return false;
                  }
                  Button b5 = CreateButton(mi5);
                  myGrid.Children.Add(b5);
                  Grid.SetRow(b5, rowNum);
                  Grid.SetColumn(b5, 1);
                  //-----------------------
                  Label label6 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].mySector };
                  myGrid.Children.Add(label6);
                  Grid.SetRow(label6, rowNum);
                  Grid.SetColumn(label6, 2);
                  //-----------------------
                  if (Utilities.NO_RESULT < myGridRows[i].myDieRollRange)
                  {
                     Label label51 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myRange };
                     myGrid.Children.Add(label51);
                     Grid.SetRow(label51, rowNum);
                     Grid.SetColumn(label51, 3);
                  }
                  else
                  {
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollWhite.gif", UriKind.Absolute);
                     bmi.EndInit();
                     Image img = new Image { Name = "DieRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 3);
                  }
                  break;
               case E046Enum.PLACE_FACING:
                  label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollActivation.ToString() };
                  myGrid.Children.Add(label5);
                  Grid.SetRow(label5, rowNum);
                  Grid.SetColumn(label5, 0);
                  //-----------------------
                  mi5 = myGridRows[i].myMapItem;
                  if (null == mi5)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): myGridRows[i].myMapItem=null for i=" + i);
                     return false;
                  }
                  b5 = CreateButton(mi5);
                  myGrid.Children.Add(b5);
                  Grid.SetRow(b5, rowNum);
                  Grid.SetColumn(b5, 1);
                  //-----------------------
                  label6 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].mySector };
                  myGrid.Children.Add(label6);
                  Grid.SetRow(label6, rowNum);
                  Grid.SetColumn(label6, 2);
                  //-----------------------
                  Label label52 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myRange };
                  myGrid.Children.Add(label52);
                  Grid.SetRow(label52, rowNum);
                  Grid.SetColumn(label52, 3);
                  //-----------------------
                  if (Utilities.NO_RESULT < myGridRows[i].myDieRollFacing)
                  {
                     Label label531 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myFacing };
                     myGrid.Children.Add(label531);
                     Grid.SetRow(label531, rowNum);
                     Grid.SetColumn(label531, 4);
                  }
                  else
                  {
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollWhite.gif", UriKind.Absolute);
                     bmi.EndInit();
                     Image img = new Image { Name = "DieRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 4);
                  }
                  break;
               case E046Enum.PLACE_TERRAIN:
               case E046Enum.SHOW_RESULTS:
                  label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollActivation.ToString() };
                  myGrid.Children.Add(label5);
                  Grid.SetRow(label5, rowNum);
                  Grid.SetColumn(label5, 0);
                  //-----------------------
                  mi5 = myGridRows[i].myMapItem;
                  if (null == mi5)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): myGridRows[i].myMapItem=null for i=" + i);
                     return false;
                  }
                  b5 = CreateButton(mi5);
                  myGrid.Children.Add(b5);
                  Grid.SetRow(b5, rowNum);
                  Grid.SetColumn(b5, 1);
                  //-----------------------
                  label6 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].mySector };
                  myGrid.Children.Add(label6);
                  Grid.SetRow(label6, rowNum);
                  Grid.SetColumn(label6, 2);
                  //-----------------------
                  label52 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myRange };
                  myGrid.Children.Add(label52);
                  Grid.SetRow(label52, rowNum);
                  Grid.SetColumn(label52, 3);
                  //-----------------------
                  Label label53 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myFacing };
                  myGrid.Children.Add(label53);
                  Grid.SetRow(label53, rowNum);
                  Grid.SetColumn(label53, 4);
                  //-----------------------
                  IMapItem? terrain = null;
                  if (Utilities.NO_RESULT < myGridRows[i].myDieRollTerrain)
                  {
                     switch(myGridRows[i].myTerrain)
                     {
                        case "Hull Down":
                           terrain = new MapItem("Terrain", 1.0, "c14HullDownFull", mi5.TerritoryCurrent);
                           break;
                        case "Woods":
                           terrain = new MapItem("Terrain", 1.0, "C97TerrainWoods", mi5.TerritoryCurrent);
                           break;
                        case "Fortification":
                           terrain = new MapItem("Terrain", 1.0, "C98TerrainFort", mi5.TerritoryCurrent);
                           break;
                        case "Building":
                           terrain = new MapItem("Terrain", 1.0, "C96TerrainBuilding", mi5.TerritoryCurrent);
                           break;
                        case "Open":
                           terrain = new MapItem("Terrain", 1.0, "c114Open", mi5.TerritoryCurrent);
                           break;
                        case "Moving in Open":
                           terrain = new MapItem("Terrain", 1.0, "c13Moving", mi5.TerritoryCurrent);
                           break;
                        default:
                           Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): reached default terrain=" + myGridRows[i].myDieRollTerrain);
                           return false;
                     }
                     if( null == terrain)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): terrain=null");
                        return false;
                     }
                     Button bTerrain = CreateButton(terrain);
                     myGrid.Children.Add(bTerrain);
                     Grid.SetRow(bTerrain, rowNum);
                     Grid.SetColumn(bTerrain, 5);
                  }
                  else
                  {
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollWhite.gif", UriKind.Absolute);
                     bmi.EndInit();
                     Image img = new Image { Name = "DieRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 5);
                  }
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): reached default s=" + myState.ToString());
                  return false;
            }
         }
         return true;
      }
      //------------------------------------------------------------------------------------
      private bool CreateMapItem(Index i)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItem(): myGameInstance=null");
            return false;
         }

         ITerritory? tLeft = Territories.theTerritories.Find("OffBottomLeft");
         if (null == tLeft)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItem(): tLeft=null for OffBottomLeft");
            return false;
         }
         ITerritory? tRight = Territories.theTerritories.Find("OffBottomRight");
         if (null == tRight)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItem(): tRight=null for OffBottomRight");
            return false;
         }
         ITerritory? t = null;
         IMapItem? mi = null;
         string name = myGridRows[i].myActivation + Utilities.MapItemNum;
         Utilities.MapItemNum++;
         switch (myGridRows[i].myActivation)
         {
            case "ATG":
               t = tLeft;
               mi = new MapItem(name, Utilities.ZOOM + 0.1, "c76UnidentifiedAtg", t);
               myGridRows[i].myDieRollFacing = NO_FACING;
               myGridRows[i].myFacing = "NA";
               break;
            case "LW":
               t = tRight;
               mi = new MapItem(name, Utilities.ZOOM, "c91Lw", t);
               mi.IsSpotted = true; 
               myGridRows[i].myDieRollFacing = NO_FACING;
               myGridRows[i].myFacing = "NA";
               break;
            case "MG":
               t = tRight;
               mi = new MapItem(name, Utilities.ZOOM, "c92MgTeam", t);
               mi.IsSpotted = true;
               myGridRows[i].myDieRollFacing = NO_FACING;
               myGridRows[i].myFacing = "NA";
               break;
            case "PSW/SPW":
               t = tLeft;
               mi = new MapItem(name, Utilities.ZOOM, "SpwOrPsw", t);
               mi.IsSpotted = true;
               mi.IsVehicle = true;
               return true;
            case "PSW":
               t = tLeft;
               mi = new MapItem(name, Utilities.ZOOM + 0.2, "c89Psw232", t);
               mi.IsSpotted = true;
               mi.IsVehicle = true;
               myIsVehicleActivated = true;
               break;
            case "SPW":
               t = tLeft;
               mi = new MapItem(name, Utilities.ZOOM + 0.2, "c90Spw251", t);
               mi.IsSpotted = true;
               mi.IsVehicle = true;
               myIsVehicleActivated = true;
               break;
            case "SPG":
               t = tLeft;
               mi = new MapItem(name, Utilities.ZOOM + 0.5, "c77UnidentifiedSpg", t);
               mi.IsVehicle = true;
               myIsVehicleActivated = true;
               break;
            case "TANK":
               t = tLeft;
               mi = new MapItem(name, Utilities.ZOOM + 0.5, "c78UnidentifiedTank", t);
               mi.IsVehicle = true;
               mi.IsTurret = true;
               myIsVehicleActivated = true;
               break;
            case "TRUCK":
               t = tRight;
               mi = new MapItem(name, Utilities.ZOOM + 0.3, "c88Truck", t);
               mi.IsSpotted = true;
               mi.IsVehicle = true;
               myIsVehicleActivated = true;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "CreateMapItem(): reached default with enemyUnit=" + myGridRows[i].myActivation);
               return false;
         }
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItem(): mi=null");
            return false;
         }
         IMapPoint mp = Territory.GetRandomPoint(t, mi.Zoom * Utilities.theMapItemOffset);
         mi.Location = mp;
         Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "CreateMapItem(): Adding mi=" + mi.Name + " t=" + mi.TerritoryCurrent.Name + " to " + myGameInstance.BattleStacks.ToString() + " at i=" + i.ToString());
         myGameInstance.BattleStacks.Add(mi);
         myGridRows[i].myMapItem = mi;
         return true;
      }
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
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
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString());
            return;
         }
         switch(myState)
         {
            case E046Enum.ACTIVATION:
               myGridRows[i].myDieRollActivation = dieRoll;
               myGridRows[i].myActivation = TableMgr.SetEnemyUnit(myScenario, myDay, dieRoll);
               if (false == CreateMapItem(i))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): CreateMapItem() returned false");
                  return;
               }
               if ("PSW/SPW" == myGridRows[i].myActivation)
               {
                  myState = E046Enum.SPW_OR_PSW_ROLL;
                  if (false == UpdateGrid())
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false for i=" + i.ToString());
                  myIsRollInProgress = false;
                  return;
               }
               myState = E046Enum.PLACE_SECTOR;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (Utilities.NO_RESULT == myGridRows[j].myDieRollActivation)
                     myState = E046Enum.ACTIVATION;
               }
               break;
            case E046Enum.SPW_OR_PSW_ROLL:
               if (dieRoll < 9)
                  myGridRows[i].myActivation = "SPW";
               else
                  myGridRows[i].myActivation = "PSW";
               if (false == CreateMapItem(i))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): CreateMapItem() returned false");
                  return;
               }
               myState = E046Enum.PLACE_SECTOR;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (Utilities.NO_RESULT == myGridRows[j].myDieRollActivation)
                     myState = E046Enum.ACTIVATION;
               }
               break;
            case E046Enum.PLACE_SECTOR:
               myGridRows[i].myDieRollSector = dieRoll;
               if (false == ShowDieResultUpdateSector(i))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): ShowDieResultUpdateSector() returned false");
                  return;
               }
               myState = E046Enum.PLACE_RANGE;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (Utilities.NO_RESULT == myGridRows[j].myDieRollSector)
                     myState = E046Enum.PLACE_SECTOR;
               }
               break;
            case E046Enum.PLACE_RANGE:
               myGridRows[i].myDieRollRange = dieRoll;
               myGridRows[i].myRange = TableMgr.GetEnemyRange(myAreaType, myGridRows[i].myActivation, dieRoll);
               if ( "ERROR" == myGridRows[i].myRange )
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): TableMgr.GetEnemyRange() returned ERROR");
                  return;
               }
               if (false == ShowDieResultUpdateRange(i))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): ShowDieResultUpdateRange() returned false");
                  return;
               }
               if (true == myIsVehicleActivated)
                  myState = E046Enum.PLACE_FACING;
               else
                  myState = E046Enum.PLACE_TERRAIN;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (Utilities.NO_RESULT == myGridRows[j].myDieRollRange)
                     myState = E046Enum.PLACE_RANGE;
               }
               break;
            case E046Enum.PLACE_FACING:
               myGridRows[i].myDieRollFacing= dieRoll;
               myGridRows[i].myFacing = TableMgr.GetEnemyNewFacing(myGridRows[i].myActivation, dieRoll);
               if ("ERROR" == myGridRows[i].myFacing)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): TableMgr.GetEnemyNewFacing() returned ERROR");
                  return;
               }
               IMapItem? mi = myGridRows[i].myMapItem;
               if (null == mi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateFacing(): mi=null for i=" + i.ToString());
                  return;
               }
               if (false == mi.UpdateMapRotation(myGridRows[i].myFacing))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateMapRotation() returned false");
                  return;
               }
               myState = E046Enum.PLACE_TERRAIN;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (Utilities.NO_RESULT == myGridRows[j].myDieRollFacing)
                     myState = E046Enum.PLACE_FACING;
               }
               break;
            case E046Enum.PLACE_TERRAIN:
               myGridRows[i].myDieRollTerrain = dieRoll;
               myGridRows[i].myTerrain = TableMgr.GetEnemyTerrain(myScenario, myDay, myAreaType, myGridRows[i].myActivation, dieRoll);
               if ("ERROR" == myGridRows[i].myTerrain)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): TableMgr.GetEnemyTerrain() returned ERROR");
                  return;
               }
               if (false == ShowDieResultUpdateTerrain(i))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): ShowDieResultUpdateTerrain() returned ERROR");
                  return;
               }
               myState = E046Enum.SHOW_RESULTS;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (Utilities.NO_RESULT == myGridRows[j].myDieRollTerrain)
                     myState = E046Enum.PLACE_TERRAIN;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default with myState=" + myState.ToString());
               return;
         }
         //-------------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
         //-------------------------------
         if( null == myGameEngine )
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
      private bool ShowDieResultUpdateSector(Index i)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateSector(): myGameInstance=null");
            return false;
         }
         string? tName = null;
         switch (myGridRows[i].myDieRollSector)
         {
            case 1:
               if (false == myIsSectorUsControlled[0])
               {
                  tName = "B1M";
                  myGridRows[i].mySector = "1";
               }
               else
               {
                  tName = "B9M";
                  if (true == myIsSectorUsControlled[5])
                     myGridRows[i].mySector = "X";
                  else
                     myGridRows[i].mySector = "(1)";
               }
               break;
            case 2:
               if (false == myIsSectorUsControlled[1])
               {
                  tName = "B2M";
                  myGridRows[i].mySector = "2";
               }
               else
               {
                  tName = "B6M";
                  if (true == myIsSectorUsControlled[4])
                     myGridRows[i].mySector = "X";
                  else
                     myGridRows[i].mySector = "(2)";
               }
               break;
            case 3:
               if (false == myIsSectorUsControlled[2])
               {
                  tName = "B3M";
                  myGridRows[i].mySector = "3";
               }
               else
               {
                  tName = "B4M";
                  if (true == myIsSectorUsControlled[3])
                     myGridRows[i].mySector = "X";
                  else
                     myGridRows[i].mySector = "(3)";
               }
               break;
            case 4:
            case 5:
               if (false == myIsSectorUsControlled[3])
               {
                  tName = "B4M";
                  myGridRows[i].mySector = "4-5";
               }
               else
               {
                  tName = "B3M";
                  if (true == myIsSectorUsControlled[2])
                     myGridRows[i].mySector = "X";
                  else
                     myGridRows[i].mySector = "(3)";
               }
               break;
            case 6:
            case 7:
            case 8:
               if (false == myIsSectorUsControlled[4])
               {
                  tName = "B6M";
                  myGridRows[i].mySector = "6-8";
               }
               else
               {
                  tName = "B2M";
                  if (true == myIsSectorUsControlled[1])
                     myGridRows[i].mySector = "X";
                  else
                     myGridRows[i].mySector = "(2)";
               }
               break;
            case 9:
            case 10:
               if (false == myIsSectorUsControlled[5])
               {
                  tName = "B9M";
                  myGridRows[i].mySector = "9-10";
               }
               else
               {
                  tName = "B1M";
                  if (true == myIsSectorUsControlled[0])
                     myGridRows[i].mySector = "X";
                  else
                     myGridRows[i].mySector = "(1)";
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateSector(): reached default dr=" + myGridRows[i].myDieRollSector.ToString());
               return false;
         }
         if (null == tName)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateSector(): tName=null for dr=" + myGridRows[i].myDieRollSector.ToString());
            return false;
         }
         if ("X" != myGridRows[i].mySector)
         {
            ITerritory? t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateSector(): t=null for " + tName);
               return false;
            }
            IMapItem? mi = myGridRows[i].myMapItem;
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateSector(): mi=null for i=" + i.ToString());
               return false;
            }
            IMapPoint mp = Territory.GetRandomPoint(t, mi.Zoom * Utilities.theMapItemOffset);
            mi.Location = mp;
            mi.TerritoryCurrent = mi.TerritoryStarting = t;
            myGameInstance.BattleStacks.Remove(mi.Name);
            myGameInstance.BattleStacks.Add(mi);
            if (false == mi.SetMapItemRotation(myGameInstance.Sherman))
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateSector(): SetMapItemRotation() returned false");
               return false;
            }
         }
         return true;
      }
      private bool ShowDieResultUpdateRange(Index i)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateRange(): myGameInstance=null");
            return false;
         }
         if ("M" == myGridRows[i].myRange)
            return true;
         //----------------------------
         IMapItem? mi = myGridRows[i].myMapItem;
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateRange(): mi=null for i=" + i.ToString());
            return false;
         }
         string tName = mi.TerritoryCurrent.Name;
         if (true == tName.Contains("Off"))
            return true;
         //----------------------------
         string modified = tName.Remove(tName.Length - 1) + myGridRows[i].myRange; // change last character
         ITerritory? t = Territories.theTerritories.Find(modified);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateRange(): t=null for i=" + i.ToString());
            return false;
         }
         mi.TerritoryCurrent = mi.TerritoryStarting = t;
         myGameInstance.BattleStacks.Remove(mi.Name);
         myGameInstance.BattleStacks.Add(mi);
         IMapPoint mp = Territory.GetRandomPoint(t, mi.Zoom * Utilities.theMapItemOffset);
         mi.Location = mp;
         if (false == mi.SetMapItemRotation(myGameInstance.Sherman))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateRange(): SetMapItemRotation() returned false");
            return false;
         }
         return true;
      }
      private bool ShowDieResultUpdateTerrain(Index i)
      {
         IMapItem? mi = myGridRows[i].myMapItem;
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateTerrain(): mi=null for i=" + i.ToString());
            return false;
         }
         mi.IsHullDown = false;
         mi.IsWoods = false;
         mi.IsFortification = false;
         mi.IsBuilding = false;
         mi.IsMovingInOpen = false;
         mi.IsMoving = false;
         switch (myGridRows[i].myTerrain)
         {
            case "Hull Down":
               mi.IsHullDown = true;
               break;
            case "Woods":
               mi.IsWoods = true;
               break;
            case "Fortification":
               mi.IsFortification = true;
               break;
            case "Building":
               mi.IsBuilding = true;
               break;
            case "Open":
               break;
            case "Moving in Open":
               mi.IsMoving = true;
               mi.IsMovingInOpen = true;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateTerrain(): reached default terrain=" + myGridRows[i].myDieRollTerrain);
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
                        if ("Continue" == img.Name)
                           myState = E046Enum.END;
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
                     if ("DieRoll" == img1.Name)
                        myDieRoller.RollMovingDie(myCanvas, callback);
                     else
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
