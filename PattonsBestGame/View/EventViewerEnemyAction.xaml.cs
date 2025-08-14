using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
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
using System.Xml.Linq;
using Windows.ApplicationModel.Activation;
using WpfAnimatedGif;
using static Pattons_Best.EventViewerBattleSetup;
using static System.Windows.Forms.AxHost;

namespace Pattons_Best
{
   public partial class EventViewerEnemyAction : UserControl
   {
      public delegate bool EndResolveEnemyActionCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int NO_MOVE = 100;
      private const int NO_FIRE = 101;
      private const int NO_FACING = 102;
      private const int KEEP_TERRAIN = 103;
      private const int NO_FIRE_YOUR_TANK = 105;
      private const int NO_FIRE_THROWN_TRACK = 106;
      private const int NO_FIRE_MISSED_TURRET = 107;
      public enum E0475Enum
      {
         ENEMY_ACTION_SELECT,
         ENEMY_ACTION_SELECT_SHOW,
         ENEMY_ACTION_MOVE,
         ENEMY_ACTION_MOVE_SHOW,
         ENEMY_ACTION_ADVANCE_FIRE,
         ENEMY_ACTION_ADVANCE_FIRE_SHOW,
         ENEMY_ACTION_FIRE,
         ENEMY_ACTION_FIRE_SHOW,
         ENEMY_ACTION_TO_HIT_YOUR_TANK,
         ENEMY_ACTION_TO_HIT_YOUR_TANK_SHOW,
         ENEMY_ACTION_TO_KILL_YOUR_TANK,
         ENEMY_ACTION_TO_KILL_YOUR_TANK_SHOW,
         END
      };
      public bool CtorError { get; } = false;
      private EndResolveEnemyActionCallback? myCallback = null;
      private E0475Enum myState = E0475Enum.ENEMY_ACTION_SELECT;
      private int myMaxRowCount = 0;
      private int myMaxRowCountAdvanceFire = 0;
      private int myRollResultRowNum = 0;
      private int myRollResultColNum = 0;
      private bool myIsRollInProgress = false;
      //============================================================
      public struct GridRowAdvanceFire
      {
         public IMapItem myEnemyUnit = new MapItem("Dummy", 1.0, "c44AdvanceFire", new Territory());
         public IMapItem myAdvanceFire = new MapItem("AdvanceFire", 1.0, "c44AdvanceFire", new Territory());
         public string mySectorRangeDisplay = "UNINT";
         public int myDieRollAdvanceFire = Utilities.NO_RESULT;
         public int myAdvanceFireBaseNum = 0;
         public int myAdvanceFireModifier = 0;
         public string myAdvanceFireResult = "UNINT";
         public GridRowAdvanceFire(){ }
      }
      private GridRowAdvanceFire[] myAdvanceFireGridRows = new GridRowAdvanceFire[15];
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myModifierEnemyAction = 0;
         public string myEnemyAction = "";
         public int myDieRollEnemyAction = Utilities.NO_RESULT;
         //---------------------------------------------------
         public char mySector = 'E';
         public char myRange = 'E';
         public string mySectorRangeDisplay = "UNINT";
         public string myFacing = "NA";
         public string myTerrain = "NA";
         public int myDieRollFacing = Utilities.NO_RESULT;
         public int myDieRollTerrain = Utilities.NO_RESULT;
         //---------------------------------------------------
         public int myDieRollFire = Utilities.NO_RESULT;
         public string myToKillResult = "NA";
         public int myToKillNumber = 0;
         //---------------------------------------------------
         public int myModifierToHitYourTank = 0;
         public int myToHitNumberYourTank = 0;
         public string myToHitResultYourTank = "UNINT";
         public int myDieRollToHitYourTank = Utilities.NO_RESULT;
         //---------------------------------------------------
         public string myHitLocationYourTank = "UNINT";
         public int myDieRollHitLocationYourTank = Utilities.NO_RESULT;
         //---------------------------------------------------
         public string myToKillResultYourTank = "UNINT";
         public int myToKillNumberYourTank = 0;
         public int myDieRollToKillYourTank = Utilities.NO_RESULT;
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
         myMaxRowCount = 0;
         myMaxRowCountAdvanceFire = 0;
         myAreaType = "UNINIT";
         myState = E0475Enum.ENEMY_ACTION_SELECT;
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
         if (BattlePhase.Ambush == myGameInstance.BattlePhase)
            myButtonR465.Visibility = Visibility.Visible;
         //--------------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "PerformEnemyAction(): BattleStacks=" + myGameInstance.BattleStacks.ToString());
         int i = 0;
         foreach(IStack stack3 in myGameInstance.BattleStacks)
         {
            foreach (IMapItem mi in stack3.MapItems)
            {
               if (true == mi.IsEnemyUnit())
               {
                  mi.IsFired = false;
                  mi.IsMoved = false;
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
                  if (false == mi.IsVehicle)
                  {
                     Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "PerformEnemyAction(): mi=" + mi.Name + " isVehicle=false for i=" + i.ToString());
                     myGridRows[i].myDieRollFacing = NO_FACING;
                  }
                  if (true == mi.IsHullDown)
                     myGridRows[i].myTerrain = "Hull Down";
                  else if (true == mi.IsWoods)
                     myGridRows[i].myTerrain = "Woods";
                  else if (true == mi.IsFortification)
                     myGridRows[i].myTerrain = "Fortification";
                  else if (true == mi.IsBuilding)
                     myGridRows[i].myTerrain = "Building";
                  else if (true == mi.IsMoving)
                     myGridRows[i].myTerrain = "Moving";
                  else
                     myGridRows[i].myTerrain = "Open";
                  ++i;
               }
            }
         }
         myMaxRowCount = i;
         //--------------------------------------------------
         for(int k=0; k<myMaxRowCount; ++k )
            myGridRows[k].myModifierEnemyAction = TableMgr.GetEnemyActionModifier(myGameInstance, myGridRows[k].myMapItem);
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
            //-----------------------------------------
            IMapItems removals = new MapItems();
            foreach (IStack stack in myGameInstance.BattleStacks)
            {
               foreach (IMapItem mapItem in stack.MapItems)
               {
                  if ( (true == mapItem.IsKilled) && (false == mapItem.Name.Contains("Sherman")) ) // remove enemy KIA units
                     removals.Add(mapItem);
                  if( true == mapItem.TerritoryCurrent.Name.Contains("Off")) // EventViewerEnemyAction.UpdateEndState() - remove all units that left the board
                     removals.Add(mapItem);
               }
            }
            foreach (IMapItem mi in removals)
               myGameInstance.BattleStacks.Remove(mi);
            Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "EventViewerEnemyAction.UpdateEndState(): ------------------------------ battlestacks=" + myGameInstance.BattleStacks.ToString());
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
               myTextBlockInstructions.Inlines.Add(new Run(" Table to determine enemy action."));
               break;
            case E0475Enum.ENEMY_ACTION_SELECT_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            case E0475Enum.ENEMY_ACTION_MOVE:
               myTextBlockInstructions.Inlines.Add(new Run("Enemy Units move per the "));
               Button bMove = new Button() { Content="Movement", FontFamily = myFontFam1, FontSize = 8 };
               bMove.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bMove));
               myTextBlockInstructions.Inlines.Add(new Run(" Diagram. Roll die for vehicle facing and/or terrain."));
               break;
            case E0475Enum.ENEMY_ACTION_ADVANCE_FIRE:
               myTextBlockInstructions.Inlines.Add(new Run("Roll on the "));
               Button bAdvanceFire = new Button() { Content = "Sherman MG", FontFamily = myFontFam1, FontSize = 8 };
               bAdvanceFire.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bAdvanceFire));
               myTextBlockInstructions.Inlines.Add(new Run(" Table for effects of advance fire."));
               break;
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
               myTextBlockInstructions.Inlines.Add(new Run(" Table to determine if infantry/tank KO'ed."));
               break;
            case E0475Enum.ENEMY_ACTION_TO_HIT_YOUR_TANK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll on these tables in this order: "));
               Button bToHit = new Button() { Content = "Enemy AP To Hit", FontFamily = myFontFam1, FontSize = 8 };
               bToHit.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bToHit));
               myTextBlockInstructions.Inlines.Add(new Run(" "));
               Button bHitLocation = new Button() { Content = "Hit Location Tank", FontFamily = myFontFam1, FontSize = 8 };
               bHitLocation.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bHitLocation));
               myTextBlockInstructions.Inlines.Add(new Run(" "));
               Button bToKill = new Button() { Content = "Enemy AP To Kill", FontFamily = myFontFam1, FontSize = 8 };
               bToKill.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bToKill));
               break;
            case E0475Enum.ENEMY_ACTION_TO_KILL_YOUR_TANK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll on these tables in this order: "));
               Button bHitLocation1 = new Button() { Content = "Hit Location Tank", FontFamily = myFontFam1, FontSize = 8 };
               bHitLocation1.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bHitLocation1));
               myTextBlockInstructions.Inlines.Add(new Run(" "));
               Button bToKill1 = new Button() { Content = "Enemy AP To Kill", FontFamily = myFontFam1, FontSize = 8 };
               bToKill1.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bToKill1));
               break;
            case E0475Enum.ENEMY_ACTION_MOVE_SHOW:
            case E0475Enum.ENEMY_ACTION_ADVANCE_FIRE_SHOW:
            case E0475Enum.ENEMY_ACTION_FIRE_SHOW:
            case E0475Enum.ENEMY_ACTION_TO_HIT_YOUR_TANK_SHOW:
            case E0475Enum.ENEMY_ACTION_TO_KILL_YOUR_TANK_SHOW:
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
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): myGameInstance=null");
            return false;
         }
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case E0475Enum.ENEMY_ACTION_SELECT:
            case E0475Enum.ENEMY_ACTION_MOVE:
            case E0475Enum.ENEMY_ACTION_ADVANCE_FIRE:
            case E0475Enum.ENEMY_ACTION_FIRE:
            case E0475Enum.ENEMY_ACTION_TO_HIT_YOUR_TANK:
            case E0475Enum.ENEMY_ACTION_TO_KILL_YOUR_TANK:
               Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r1);
               break;
            case E0475Enum.ENEMY_ACTION_SELECT_SHOW:
               //-------------------------------
               bool isEnemyMoving = false;
               bool isEnemyFiring = false;
               bool isYourTank = false;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (true == myGridRows[j].myEnemyAction.Contains("Move"))
                     isEnemyMoving = true;
                  else if ( (true == myGridRows[j].myEnemyAction.Contains("Your")) || ( (true == myGridRows[j].myEnemyAction.Contains("Lead")) && (true == myGameInstance.IsLeadTank) ) )
                     isYourTank = true;
                  else if (true == myGridRows[j].myEnemyAction.Contains("Fire"))
                     isEnemyFiring = true;
               }
               if (true == isEnemyMoving)
               {
                  BitmapImage bmi1 = new BitmapImage();
                  bmi1.BeginInit();
                  bmi1.UriSource = new Uri(MapImage.theImageDirectory + "TigerMoving.gif", UriKind.Absolute);
                  bmi1.EndInit();
                  System.Windows.Controls.Image img1 = new System.Windows.Controls.Image { Name = "Move", Source = bmi1, Width = Utilities.ZOOM * Utilities.theMapItemSize * 2.5, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img1, bmi1);
                  myStackPanelAssignable.Children.Add(img1);
                  break;
               }
               else if (true == isEnemyFiring)
               {
                  BitmapImage bmi2 = new BitmapImage();
                  bmi2.BeginInit();
                  bmi2.UriSource = new Uri(MapImage.theImageDirectory + "TigerFiring.gif", UriKind.Absolute);
                  bmi2.EndInit();
                  System.Windows.Controls.Image img2 = new System.Windows.Controls.Image { Name = "Fire", Source = bmi2, Width = Utilities.ZOOM * Utilities.theMapItemSize * 1.75, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img2, bmi2);
                  myStackPanelAssignable.Children.Add(img2);
               }
               else if (true == isYourTank)
               {
                  BitmapImage bmi3 = new BitmapImage();
                  bmi3.BeginInit();
                  bmi3.UriSource = new Uri(MapImage.theImageDirectory + "TigerFiring.gif", UriKind.Absolute);
                  bmi3.EndInit();
                  System.Windows.Controls.Image img3 = new System.Windows.Controls.Image { Name = "ToHitYourTank", Source = bmi3, Width = Utilities.ZOOM * Utilities.theMapItemSize * 1.75, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img3, bmi3);
                  myStackPanelAssignable.Children.Add(img3);
               }
               else
               {
                  System.Windows.Controls.Image img23 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img23);
               }
               break;
            case E0475Enum.ENEMY_ACTION_MOVE_SHOW:
               bool isAdvanceFire = false;
               bool isEnemyFiring1 = false;
               bool isYourTank1 = false;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (true == myGridRows[j].myEnemyAction.Contains("Move"))
                  {
                     ITerritory t = myGridRows[j].myMapItem.TerritoryCurrent;
                     IStack? stack = myGameInstance.BattleStacks.Find(t);
                     if (null == stack)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): stack=null for t=" + t.Name);
                        return false;
                     }
                     foreach(IMapItem mi1 in stack.MapItems)
                     {
                        if( true == mi1.Name.Contains("Advance") )
                        {
                           isAdvanceFire = true;
                           break;
                        }
                     }
                  }
                  if ((true == myGridRows[j].myEnemyAction.Contains("Your")) || ((true == myGridRows[j].myEnemyAction.Contains("Lead")) && (true == myGameInstance.IsLeadTank)))
                     isYourTank1 = true;
                  else if (true == myGridRows[j].myEnemyAction.Contains("Fire"))
                     isEnemyFiring1 = true;
               }
               if (true == isAdvanceFire)
               {
                  System.Windows.Controls.Image imgAdv = new System.Windows.Controls.Image { Name = "AdvanceFire", Source = MapItem.theMapImages.GetBitmapImage("c44AdvanceFIre"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(imgAdv);
               }
               else if (true == isEnemyFiring1)
               {
                  BitmapImage bmi4 = new BitmapImage();
                  bmi4.BeginInit();
                  bmi4.UriSource = new Uri(MapImage.theImageDirectory + "InfantryFiring.gif", UriKind.Absolute);
                  bmi4.EndInit();
                  System.Windows.Controls.Image img4 = new System.Windows.Controls.Image { Name = "Fire", Source = bmi4, Width = Utilities.ZOOM * Utilities.theMapItemSize * 1.75, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img4, bmi4);
                  myStackPanelAssignable.Children.Add(img4);
               }
               else if (true == isYourTank1)
               {
                  BitmapImage bmi51 = new BitmapImage();
                  bmi51.BeginInit();
                  bmi51.UriSource = new Uri(MapImage.theImageDirectory + "TigerFiring4.gif", UriKind.Absolute);
                  bmi51.EndInit();
                  System.Windows.Controls.Image img51 = new System.Windows.Controls.Image { Name = "ToHitYourTank", Source = bmi51, Width = Utilities.ZOOM * Utilities.theMapItemSize * 1.75, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img51, bmi51);
                  myStackPanelAssignable.Children.Add(img51);
               }
               else
               {
                  System.Windows.Controls.Image img23 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img23);
               }
               break;
             case E0475Enum.ENEMY_ACTION_ADVANCE_FIRE_SHOW:
               bool isEnemyFiring11 = false;
               bool isYourTank11 = false;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if ((true == myGridRows[j].myEnemyAction.Contains("Your")) || ((true == myGridRows[j].myEnemyAction.Contains("Lead")) && (true == myGameInstance.IsLeadTank)))
                     isYourTank1 = true;
                  else if (true == myGridRows[j].myEnemyAction.Contains("Fire"))
                     isEnemyFiring1 = true;
               }
               if (true == isEnemyFiring11)
               {
                  BitmapImage bmi4 = new BitmapImage();
                  bmi4.BeginInit();
                  bmi4.UriSource = new Uri(MapImage.theImageDirectory + "InfantryFiring.gif", UriKind.Absolute);
                  bmi4.EndInit();
                  System.Windows.Controls.Image img4 = new System.Windows.Controls.Image { Name = "Fire", Source = bmi4, Width = Utilities.ZOOM * Utilities.theMapItemSize * 1.75, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img4, bmi4);
                  myStackPanelAssignable.Children.Add(img4);
               }
               else if (true == isYourTank11)
               {
                  BitmapImage bmi51 = new BitmapImage();
                  bmi51.BeginInit();
                  bmi51.UriSource = new Uri(MapImage.theImageDirectory + "TigerFiring4.gif", UriKind.Absolute);
                  bmi51.EndInit();
                  System.Windows.Controls.Image img51 = new System.Windows.Controls.Image { Name = "ToHitYourTank", Source = bmi51, Width = Utilities.ZOOM * Utilities.theMapItemSize * 1.75, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img51, bmi51);
                  myStackPanelAssignable.Children.Add(img51);
               }
               else
               {
                  System.Windows.Controls.Image img23 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img23);
               }
               break;
            case E0475Enum.ENEMY_ACTION_FIRE_SHOW:
               bool isYourTank2 = false;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if ((true == myGridRows[j].myEnemyAction.Contains("Your")) || ((true == myGridRows[j].myEnemyAction.Contains("Lead")) && (true == myGameInstance.IsLeadTank)))
                     isYourTank2 = true;
               }
               if (true == isYourTank2)
               {
                  BitmapImage bmi6 = new BitmapImage();
                  bmi6.BeginInit();
                  bmi6.UriSource = new Uri(MapImage.theImageDirectory + "TigerFiring.gif", UriKind.Absolute);
                  bmi6.EndInit();
                  System.Windows.Controls.Image img6 = new System.Windows.Controls.Image { Name = "ToHitYourTank", Source = bmi6, Width = Utilities.ZOOM * Utilities.theMapItemSize * 1.75, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img6, bmi6);
                  myStackPanelAssignable.Children.Add(img6);
               }
               else
               {
                  System.Windows.Controls.Image img52 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img52);
               }
               break;
            case E0475Enum.ENEMY_ACTION_TO_HIT_YOUR_TANK_SHOW:
               bool isYourTankHit = false;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if ("Hit" == myGridRows[j].myToHitResultYourTank)
                     isYourTankHit = true;
               }
               if (true == isYourTankHit)
               {
                  BitmapImage bmi8 = new BitmapImage();
                  bmi8.BeginInit();
                  bmi8.UriSource = new Uri(MapImage.theImageDirectory + "TigerFiring2.gif", UriKind.Absolute);
                  bmi8.EndInit();
                  System.Windows.Controls.Image img8 = new System.Windows.Controls.Image { Name = "ToKillYourTank", Source = bmi8, Width = Utilities.ZOOM * Utilities.theMapItemSize * 1.7, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img8, bmi8);
                  myStackPanelAssignable.Children.Add(img8);
               }
               else
               {
                  System.Windows.Controls.Image img54 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img54);
               }
               break;
            case E0475Enum.ENEMY_ACTION_TO_KILL_YOUR_TANK_SHOW:
               System.Windows.Controls.Image img56 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img56);
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
            case E0475Enum.ENEMY_ACTION_MOVE_SHOW:
               if (false == UpdateGridRowsMove())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsMove() returned false");
                  return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               break;
            case E0475Enum.ENEMY_ACTION_ADVANCE_FIRE:
            case E0475Enum.ENEMY_ACTION_ADVANCE_FIRE_SHOW:
               if (false == UpdateGridRowsAdvanceFire())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsAdvanceFire() returned false");
                  return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               break;
            case E0475Enum.ENEMY_ACTION_FIRE:
            case E0475Enum.ENEMY_ACTION_FIRE_SHOW:
               if (false == UpdateGridRowsFire())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsFire() returned false");
                  return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               break;
            case E0475Enum.ENEMY_ACTION_TO_HIT_YOUR_TANK:
            case E0475Enum.ENEMY_ACTION_TO_HIT_YOUR_TANK_SHOW:
               if (false == UpdateGridRowsToHitYourTank())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsToHitYourTank() returned false");
                  return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               break;
            case E0475Enum.ENEMY_ACTION_TO_KILL_YOUR_TANK:
            case E0475Enum.ENEMY_ACTION_TO_KILL_YOUR_TANK_SHOW:
               if (false == UpdateGridRowsToKillYourTank())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsToKillYourTank() returned false");
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
      //------------------------------------------------------------------------------------
      private bool UpdateGridRowsEnemyActionSelect()
      {
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            Button b1 = CreateButton(mi);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);

            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = GetSectorRangeDisplay(i) };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 1);
            //----------------------------------
            Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myModifierEnemyAction.ToString() };
            myGrid.Children.Add(label2);
            Grid.SetRow(label2, rowNum);
            Grid.SetColumn(label2, 2);
            //----------------------------------
            if (Utilities.NO_RESULT < myGridRows[i].myDieRollEnemyAction)
            {
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollEnemyAction.ToString() };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               int dieRollPlusModifier = myGridRows[i].myDieRollEnemyAction + myGridRows[i].myModifierEnemyAction;
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dieRollPlusModifier.ToString() };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
               Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myEnemyAction };
               myGrid.Children.Add(label5);
               Grid.SetRow(label5, rowNum);
               Grid.SetColumn(label5, 5);
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
      private bool UpdateGridRowsMove()
      {
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            Button b1 = CreateButton(mi);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            //----------------------------
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].mySectorRangeDisplay };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 1);
            //----------------------------
            if( (NO_MOVE == myGridRows[i].myDieRollFacing) || (NO_FACING == myGridRows[i].myDieRollFacing) )
            {
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 2);
            }
            else if (Utilities.NO_RESULT < myGridRows[i].myDieRollFacing)
            {
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myFacing };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 2);
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
               Grid.SetColumn(img, 2);
            }
            //----------------------------
            if (Utilities.NO_RESULT < myGridRows[i].myDieRollTerrain)
            {
               if (false == SetTerrainCounter(i))
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): SetTerrainCounter() returned false");
                  return false;
               }
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
            //----------------------------
            Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myEnemyAction };
            myGrid.Children.Add(label5);
            Grid.SetRow(label5, rowNum);
            Grid.SetColumn(label5, 5);
         }
         return true;
      }
      private bool UpdateGridRowsAdvanceFire()
      {
         for (int i = 0; i < myMaxRowCountAdvanceFire; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myAdvanceFireGridRows[i].myEnemyUnit;
            Button b1 = CreateButton(mi);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            //----------------------------
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myAdvanceFireGridRows[i].mySectorRangeDisplay };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 1);
            //----------------------------
            Button bAdvance = CreateButton(myAdvanceFireGridRows[i].myAdvanceFire);
            myGrid.Children.Add(bAdvance);
            Grid.SetRow(bAdvance, rowNum);
            Grid.SetColumn(bAdvance, 2);
            Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myAdvanceFireGridRows[i].myAdvanceFireBaseNum.ToString() };
            myGrid.Children.Add(label3);
            Grid.SetRow(label3, rowNum);
            Grid.SetColumn(label3, 3);
            Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myAdvanceFireGridRows[i].myAdvanceFireModifier.ToString() };
            myGrid.Children.Add(label4);
            Grid.SetRow(label4, rowNum);
            Grid.SetColumn(label4, 4);
            if (Utilities.NO_RESULT < myAdvanceFireGridRows[i].myDieRollAdvanceFire)
            {
               Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myAdvanceFireGridRows[i].myAdvanceFireResult };
               myGrid.Children.Add(label5);
               Grid.SetRow(label5, rowNum);
               Grid.SetColumn(label5, 5);
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
               Grid.SetColumn(img, 5);
            }
         }
         return true;
      }
      private bool UpdateGridRowsFire()
      {
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            Button b1 = CreateButton(mi);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            //----------------------------
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].mySectorRangeDisplay };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 1);
            //----------------------------
            if (NO_FIRE == myGridRows[i].myDieRollFire)
            {
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myEnemyAction };
               myGrid.Children.Add(label5);
               Grid.SetRow(label5, rowNum);
               Grid.SetColumn(label5, 5);
            }
            else if (Utilities.NO_RESULT < myGridRows[i].myDieRollFire)
            {
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myToKillNumber };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollFire };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myToKillResult };
               myGrid.Children.Add(label5);
               Grid.SetRow(label5, rowNum);
               Grid.SetColumn(label5, 5);
            }
            else
            {
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myToKillNumber };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
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
      private bool UpdateGridRowsToHitYourTank()
      {
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            Button b1 = CreateButton(mi);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            //----------------------------
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].mySectorRangeDisplay };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 1);
            //----------------------------
            if (NO_FIRE_YOUR_TANK == myGridRows[i].myDieRollToHitYourTank)
            {
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
               Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myEnemyAction };
               myGrid.Children.Add(label5);
               Grid.SetRow(label5, rowNum);
               Grid.SetColumn(label5, 5);
            }
            else if (Utilities.NO_RESULT < myGridRows[i].myDieRollToHitYourTank)
            {
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myToHitNumberYourTank.ToString() };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myModifierToHitYourTank.ToString() }; 
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               int dieRollPlusMod = myGridRows[i].myModifierToHitYourTank + myGridRows[i].myDieRollToHitYourTank;
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = dieRollPlusMod.ToString() };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
               Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myToHitResultYourTank };
               myGrid.Children.Add(label5);
               Grid.SetRow(label5, rowNum);
               Grid.SetColumn(label5, 5);
            }
            else
            {
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myToHitNumberYourTank.ToString() }; 
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myModifierToHitYourTank.ToString() };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
               bmi.EndInit();
               System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "DiceRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img, bmi);
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 4);
            }
         }
         return true;
      }
      private bool UpdateGridRowsToKillYourTank()
      {
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            Button b1 = CreateButton(mi);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            //----------------------------
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].mySectorRangeDisplay };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 1);
            //----------------------------
            if (NO_FIRE_YOUR_TANK == myGridRows[i].myDieRollToKillYourTank)
            {
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
               Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myEnemyAction };
               myGrid.Children.Add(label5);
               Grid.SetRow(label5, rowNum);
               Grid.SetColumn(label5, 5);
            }
            else if (NO_FIRE_THROWN_TRACK == myGridRows[i].myDieRollToKillYourTank)
            {
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollHitLocationYourTank.ToString() };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
               Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "Thrown Track" };
               myGrid.Children.Add(label5);
               Grid.SetRow(label5, rowNum);
               Grid.SetColumn(label5, 5);
            }
            else if (NO_FIRE_MISSED_TURRET == myGridRows[i].myDieRollToKillYourTank)
            {
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollHitLocationYourTank.ToString()}; //myGridRows[i].myHitLocationYourTank 
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
               Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "Miss Turret" };
               myGrid.Children.Add(label5);
               Grid.SetRow(label5, rowNum);
               Grid.SetColumn(label5, 5);
            }
            else if (Utilities.NO_RESULT == myGridRows[i].myDieRollHitLocationYourTank)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollWhite.gif", UriKind.Absolute);
               bmi.EndInit();
               System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "DieRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img, bmi);
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 2);
            }
            else if (Utilities.NO_RESULT < myGridRows[i].myDieRollHitLocationYourTank)
            {
               Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "UpdateGridRowsToKillYourTank(): myState=" + myState.ToString() + " dr=" + myGridRows[i].myDieRollHitLocationYourTank + " hitLoc=" + myGridRows[i].myHitLocationYourTank);
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myHitLocationYourTank };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myToKillNumberYourTank.ToString() };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               if (Utilities.NO_RESULT == myGridRows[i].myDieRollToKillYourTank)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
                  bmi.EndInit();
                  System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "DiceRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, rowNum);
                  Grid.SetColumn(img, 4);
               }
               else
               {
                  Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollToKillYourTank.ToString() };
                  myGrid.Children.Add(label4);
                  Grid.SetRow(label4, rowNum);
                  Grid.SetColumn(label4, 4);
                  Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myToKillResultYourTank };
                  myGrid.Children.Add(label5);
                  Grid.SetRow(label5, rowNum);
                  Grid.SetColumn(label5, 5);
               }
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
         MapItem.SetButtonContent(b, mi, false, false, false); // This sets the image as the button's content
         return b;
      }
      private bool CreateMapItemMove(int i)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): myGameInstance=null");
            return false;
         }
         int rowNum = i + STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         //---------------------------------------
         ITerritory? newT = null;
         switch (myGridRows[i].myEnemyAction)
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
               newT = TableMgr.SetNewTerritory(mi, myGridRows[i].myEnemyAction);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): reached default r=" + myGridRows[i].myEnemyAction);
               return false;
         }
         if (null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): SetNewTerritory() returned null");
            return false;
         }
         //--------------------------------------------
         if (3 == newT.Name.Length) 
         {
            char sector = newT.Name[newT.Name.Length - 2];
            string tName = "B" + sector + "M";
            IStack? stack = myGameInstance.BattleStacks.Find(tName);
            if (null != stack)
            {
               IMapItems removals = new MapItems();
               foreach (MapItem removal in stack.MapItems)
               {
                  if (true == removal.Name.Contains("UsControl"))
                     removals.Add(removal);
               }
               foreach (IMapItem removal in removals)
                  myGameInstance.BattleStacks.Remove(removal);
            }
         }
         //--------------------------------------------
         MapItemMove mim = new MapItemMove(Territories.theTerritories, mi, newT);
         if (true == mim.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): mim.CtorError=true for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): Invalid Parameter mim.NewTerritory=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): Invalid Parameter mim.BestPath=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (0 == mim.BestPath.Territories.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): Invalid State Territories.Count=" + mim.BestPath.Territories.Count.ToString() + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         myGameInstance.MapItemMoves.Insert(0, mim); // add at front
         Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "UpdateGridRowsMove(): mi=" + mi.Name + " moving to t=" + newT.Name);
         //-----------------------------------------------------
         myGridRows[i].myRange = 'O';  // assume off board if not equal to three
         myGridRows[i].mySector = 'O'; // assume off board if not equal to three
         if (3 == newT.Name.Length)
         {
            myGridRows[i].myRange = newT.Name[newT.Name.Length - 1];
            myGridRows[i].mySector = newT.Name[newT.Name.Length - 2];
         }
         myGridRows[i].mySectorRangeDisplay = GetSectorRangeDisplay(i);
         if ("ERROR" == myGridRows[i].mySectorRangeDisplay)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): GetSectorRangeDisplay() returned ERROR");
            return false;
         }
         if ("Off" == myGridRows[i].mySectorRangeDisplay) // EventViewerEnemyAction.CreateMapItemMove() - do not set range/facing if off board
         {
            myGridRows[i].myDieRollTerrain = KEEP_TERRAIN;
            myGridRows[i].myDieRollFacing = NO_FACING;
            if (true == newT.Name.Contains("OffBottom")) // BattleRoundSequenceMovementRoll -  Enemy Movement advanced past sector 1,2,3
               myGameInstance.AdvancingEnemies.Add(mi);
         }
         return true;
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
               Logger.Log(LogEnum.LE_ERROR, "EventViewerEnemyAction.GetSectorRangeDisplay(): Reached default sector=" + myGridRows[i].mySector + " for mi=" + myGridRows[i].myMapItem.Name + " t=" + myGridRows[i].myMapItem.TerritoryCurrent.Name);
               return "ERROR";
         }
         Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "GetSectorRangeDisplay(): loc=" + sb.ToString());
         return sb.ToString();
      }
      private bool SetTerrainCounter(int i)
      {
         int rowNum = i + STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[i].myMapItem;
         if( "Off" == myGridRows[i].mySectorRangeDisplay) // EventViewerEnemyAction.SetTerrainCounter() - do not set terrain if off board
         {
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 3);
            return true;
         }
         //-------------------------------------------
         IMapItem? terrain = null;
         mi.IsMoving = false;
         switch (myGridRows[i].myTerrain)
         {
            case "Hull Down":
               mi.IsHullDown = true;
               terrain = new MapItem("Terrain", 1.0, "c14HullDownFull", mi.TerritoryCurrent);
               break;
            case "Woods":
               mi.IsWoods = true;
               terrain = new MapItem("Terrain", 1.0, "C97TerrainWoods", mi.TerritoryCurrent);
               break;
            case "Fortification":
               mi.IsFortification = true;
               terrain = new MapItem("Terrain", 1.0, "C98TerrainFort", mi.TerritoryCurrent);
               break;
            case "Building":
               mi.IsBuilding = true;
               terrain = new MapItem("Terrain", 1.0, "C96TerrainBuilding", mi.TerritoryCurrent);
               break;
            case "Open":
               terrain = new MapItem("Terrain", 1.0, "c114Open", mi.TerritoryCurrent);
               break;
            case "Moving in Open":
               mi.IsMovingInOpen = true;
               mi.IsMoving = true;
               terrain = new MapItem("Terrain", 1.0, "c13Moving", mi.TerritoryCurrent);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): reached default terrain=" + myGridRows[i].myDieRollTerrain);
               return false;
         }
         if (null == terrain)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): terrain=null");
            return false;
         }
         Button bTerrain = CreateButton(terrain);
         myGrid.Children.Add(bTerrain);
         Grid.SetRow(bTerrain, rowNum);
         Grid.SetColumn(bTerrain, 3);
         return true;
      }
      //------------------------------------------------------------------------------------
      public void ShowDieResults(int dieRoll)
      {
         Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "EventViewerEnemyAction.ShowDieResults(): ++++++++++++++myState=" + myState.ToString());
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myGameInstance=null");
            return;
         }
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): lastReport=null");
            return;
         }
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "ShowDieResults(): myGameInstance.MapItemMoves.Clear()");
         myGameInstance.MapItemMoves.Clear();
         //-------------------------------
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString());
            return;
         }
         IMapItem mi;
         string enemyUnit;
         //-------------------------------
         switch (myState)
         {
            case E0475Enum.ENEMY_ACTION_SELECT:
               mi = myGridRows[i].myMapItem;
               if (null == mi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(ENEMY_ACTION_SELECT): mi = null for i=" + i.ToString());
                  return;
               }
               enemyUnit = mi.GetEnemyUnit();
               if ("ERROR" == enemyUnit)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(ENEMY_ACTION_SELECT): mi.GetEnemyUnit() returned error");
                  return;
               }
               mi.IsFired = false; // set to true if unit fires

               //if( ('1' == myGridRows[i].mySector) || ('2' == myGridRows[i].mySector) || ('3' == myGridRows[i].mySector) )
               //dieRoll = 51; // <cgs> TEST - Move-B for Infantry in Battle Scenario
               //else
               //dieRoll = 5; // <cgs> TEST - Do Nothing for Infantry in Battle Scenario
               //dieRoll = 15; // <cgs> TEST - Move-F for Infantry in Battle Scenario
               //dieRoll = 87; // <cgs> TEST - Fire At Your Tank
               myGridRows[i].myDieRollEnemyAction = dieRoll;
               string enemyAction = TableMgr.SetEnemyActionResult(myGameInstance, mi, dieRoll);
               if ("ERROR" == enemyAction)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): SetEnemyActionResult() returned ERROR");
                  return;
               }
               Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "ShowDieResults(): myState=" + myState.ToString() + " enemyAction=" + enemyAction);
               //----------------------------------------
               if (true == enemyAction.Contains("Infantry"))
               {
                  mi.IsFired = true;
                  myGridRows[i].myToKillNumber = (int) TableMgr.GetEnemyToKillNumberInfantry(myGameInstance, mi, myGridRows[i].mySector, myGridRows[i].myRange);
                  if(TableMgr.FN_ERROR == myGridRows[i].myToKillNumber )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): GetEnemyToKillNumberInfantry() returned " + myGridRows[i].myToKillNumber.ToString() + " for action=" + enemyAction);
                     return;
                  }
                  myGridRows[i].myDieRollToHitYourTank = NO_FIRE_YOUR_TANK;
                  myGridRows[i].myDieRollToKillYourTank = NO_FIRE_YOUR_TANK;
               }
               else if (true == enemyAction.Contains("Tank"))
               {                  
                  mi.IsFired = true;
                  if ( ( (true == enemyAction.Contains("Lead") ) && (true == myGameInstance.IsLeadTank)) || (true == enemyAction.Contains("Your") ) )
                  {
                     Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "ShowDieResults(): Firing at Your Tank myState=" + myState.ToString() + " enemyAction=" + enemyAction);
                     myGridRows[i].myModifierToHitYourTank = (int)TableMgr.GetEnemyToHitNumberModifierForYourTank(myGameInstance, mi, myGridRows[i].myRange);
                     if (TableMgr.FN_ERROR == myGridRows[i].myModifierToHitYourTank)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): GetEnemyToKillNumberInfantry() returned " + myGridRows[i].myDieRollToHitYourTank.ToString() + " for action=" + enemyAction);
                        return;
                     }
                     myGridRows[i].myToHitNumberYourTank = (int)TableMgr.GetEnemyToHitNumberYourTank(myGameInstance, mi, myGridRows[i].mySector, myGridRows[i].myRange);
                     if (TableMgr.FN_ERROR == myGridRows[i].myToHitNumberYourTank)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): GetEnemyToKillNumberInfantry() returned " + myGridRows[i].myDieRollToHitYourTank.ToString() + " for action=" + enemyAction);
                        return;
                     }
                     myGridRows[i].myDieRollFire = NO_FIRE; // not firing at other tanks... only firing at your tank
                     if( true == mi.IsTurret )
                     {
                        if (false == mi.SetMapItemRotationTurret(myGameInstance.Sherman))
                        {
                           Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): SetMapItemRotationTurret() returned false for mi=" + mi.Name + " i=" + i.ToString());
                           return;
                        }
                     }
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "ShowDieResults(): Firing at Any Tank myState=" + myState.ToString() + " enemyAction=" + enemyAction);
                     myGridRows[i].myToKillNumber = (int)TableMgr.GetEnemyToKillNumberTank(myGameInstance, mi, myGridRows[i].mySector, myGridRows[i].myRange);
                     if (TableMgr.FN_ERROR == myGridRows[i].myToKillNumber)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): GetEnemyToKillNumberInfantry() returned " + myGridRows[i].myToKillNumber.ToString() + " for action=" + enemyAction);
                        return;
                     }
                     myGridRows[i].myDieRollToHitYourTank = NO_FIRE_YOUR_TANK;
                     myGridRows[i].myDieRollToKillYourTank = NO_FIRE_YOUR_TANK;
                  }
               }
               else
               {
                  myGridRows[i].myDieRollFire = NO_FIRE;
                  myGridRows[i].myDieRollToHitYourTank = NO_FIRE_YOUR_TANK;
                  myGridRows[i].myDieRollToKillYourTank = NO_FIRE_YOUR_TANK;
               }
               //----------------------------------------
               if ( (true == enemyAction.Contains("Move") ) && (false == mi.IsThrownTrack) )
               {
                  if (EnumSpottingResult.HIDDEN == mi.Spotting) // Hidden units that move become unspotted
                     mi.Spotting = EnumSpottingResult.UNSPOTTED;
                  mi.IsMoved = true;
                  mi.IsHullDown = false;
                  mi.IsBuilding = false;
                  mi.IsFortification = false;
                  mi.IsWoods = false;
                  mi.IsMoving = true;
               }
               else
               {
                  myGridRows[i].myDieRollFacing = NO_MOVE;
                  myGridRows[i].myDieRollTerrain = KEEP_TERRAIN;
               }
               //----------------------------------------
               if (true == enemyAction.Contains("Collateral"))
               {
                  myGameInstance.NumCollateralDamage++;
                  mi.IsFired = true;
               }
               //----------------------------------------
               myGridRows[i].myEnemyAction = enemyAction;
               myState = E0475Enum.ENEMY_ACTION_SELECT_SHOW;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (Utilities.NO_RESULT == myGridRows[j].myDieRollEnemyAction)
                     myState = E0475Enum.ENEMY_ACTION_SELECT;
               }
               break;
            //------------------------------------------------------------------------------------------------
            case E0475Enum.ENEMY_ACTION_MOVE:
               mi = myGridRows[i].myMapItem;
               if (null == mi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(ENEMY_ACTION_MOVE): mi = null for i=" + i.ToString());
                  return;
               }
               enemyUnit = mi.GetEnemyUnit();
               if ("ERROR" == enemyUnit)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(ENEMY_ACTION_MOVE): mi.GetEnemyUnit() returned error");
                  return;
               }
               if ( 2 == myRollResultColNum )
               {
                  myGridRows[i].myDieRollFacing = dieRoll;
                  myGridRows[i].myFacing = TableMgr.GetEnemyNewFacing(enemyUnit, dieRoll);
                  if ("ERROR" == myGridRows[i].myFacing)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): TableMgr.GetEnemyNewFacing() returned ERROR");
                     return;
                  }
                  if (false == ShowDieResultUpdateFacing(i))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): ShowDieResultUpdateFacing() returned false");
                     return;
                  }
               }
               else if (3 == myRollResultColNum)
               {
                  myGridRows[i].myDieRollTerrain = dieRoll;
                  myGridRows[i].myTerrain = TableMgr.GetEnemyTerrain(myScenario, myGameInstance.Day, myAreaType, enemyUnit, dieRoll);
                  if ("ERROR" == myGridRows[i].myTerrain)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): TableMgr.GetEnemyTerrain() returned ERROR");
                     return;
                  }
                  if( false == ShowDieResultUpdateTerrain(i))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): ShowDieResultUpdateTerrain() returned ERROR");
                     return;
                  }
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default myRollResultColNum=" + myRollResultColNum.ToString());
                  return;
               }
               myState = E0475Enum.ENEMY_ACTION_MOVE_SHOW;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if ((Utilities.NO_RESULT == myGridRows[j].myDieRollTerrain) || (Utilities.NO_RESULT == myGridRows[j].myDieRollFacing) )
                     myState = E0475Enum.ENEMY_ACTION_MOVE;
               }
               break;
            //------------------------------------------------------------------------------------------------
            case E0475Enum.ENEMY_ACTION_ADVANCE_FIRE:
               mi = myAdvanceFireGridRows[i].myEnemyUnit;
               if (null == mi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(ENEMY_ACTION_MOVE): mi = null for i=" + i.ToString());
                  return;
               }
               enemyUnit = mi.GetEnemyUnit();
               if ("ERROR" == enemyUnit)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(ENEMY_ACTION_MOVE): mi.GetEnemyUnit() returned error");
                  return;
               }
               myAdvanceFireGridRows[i].myDieRollAdvanceFire = dieRoll;
               int combo = myAdvanceFireGridRows[i].myAdvanceFireBaseNum - myAdvanceFireGridRows[i].myAdvanceFireModifier;
               if (combo < dieRoll )
               {
                  myAdvanceFireGridRows[i].myAdvanceFireResult = "MISS";
               }
               else
               {
                  myAdvanceFireGridRows[i].myAdvanceFireResult = "KO";
                  myAdvanceFireGridRows[i].myEnemyUnit.IsKilled = true;
                  myAdvanceFireGridRows[i].myEnemyUnit.SetBloodSpots();
               }
               myState = E0475Enum.ENEMY_ACTION_ADVANCE_FIRE_SHOW;
               for (int j = 0; j < myMaxRowCountAdvanceFire; ++j)
               {
                  if (Utilities.NO_RESULT == myAdvanceFireGridRows[j].myDieRollAdvanceFire)
                     myState = E0475Enum.ENEMY_ACTION_ADVANCE_FIRE;
               }
               break;
            //------------------------------------------------------------------------------------------------
            case E0475Enum.ENEMY_ACTION_FIRE:
               myGridRows[i].myDieRollFire = dieRoll;
               if( dieRoll <= myGridRows[i].myToKillNumber )
               {
                  if( true == myGridRows[i].myEnemyAction.Contains("Infantry"))
                  {
                     myGridRows[i].myToKillResult = "Infantry KO";
                     lastReport.VictoryPtsFriendlySquad++;
                  }
                  else
                  {
                     if (true == myGridRows[i].myEnemyAction.Contains("Lead")) // if kia lead tank, all other fire on lead tank is ignored
                     {
                        for (int j = 0; j < myMaxRowCount; ++j)
                        {
                           if ((Utilities.NO_RESULT == myGridRows[j].myDieRollFire) && (true == myGridRows[j].myEnemyAction.Contains("Lead")))
                              myGridRows[j].myDieRollFire = NO_FIRE;
                        }
                     }
                     myGridRows[i].myToKillResult = "Tank KO";
                     lastReport.VictoryPtsFriendlyTank++;
                  }
               }
               else
               {
                  myGridRows[i].myToKillResult = "No Effect";
               }
               myState = E0475Enum.ENEMY_ACTION_FIRE_SHOW;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (Utilities.NO_RESULT == myGridRows[j].myDieRollFire) 
                     myState = E0475Enum.ENEMY_ACTION_FIRE;
               }
               break;
            //------------------------------------------------------------------------------------------------
            case E0475Enum.ENEMY_ACTION_TO_HIT_YOUR_TANK:
               dieRoll = 1; // <cgs> TEST - To Hit your Tank
               myGridRows[i].myDieRollToHitYourTank = dieRoll;
               int modifiedDieRoll = dieRoll + myGridRows[i].myModifierToHitYourTank;
               Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "ShowDieResults(): Firing at Your Tank myState=" + myState.ToString() + " dr=" + modifiedDieRoll.ToString());
               if (modifiedDieRoll <= myGridRows[i].myToHitNumberYourTank)
               {
                  myGridRows[i].myToHitResultYourTank = "Hit";
               }
               else
               {
                  myGridRows[i].myToHitResultYourTank = "Miss";
                  myGridRows[i].myDieRollHitLocationYourTank = NO_FIRE_YOUR_TANK;
                  myGridRows[i].myDieRollToKillYourTank = NO_FIRE_YOUR_TANK;
               }
               //---------------------------------
               myState = E0475Enum.ENEMY_ACTION_TO_HIT_YOUR_TANK_SHOW;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  if (Utilities.NO_RESULT == myGridRows[j].myDieRollToHitYourTank)
                     myState = E0475Enum.ENEMY_ACTION_TO_HIT_YOUR_TANK;
               }
               break;
            //------------------------------------------------------------------------------------------------
            case E0475Enum.ENEMY_ACTION_TO_KILL_YOUR_TANK:
               dieRoll = 1; // <cgs> TEST - To Kill your Tank
               myGridRows[i].myDieRollToHitYourTank = dieRoll;
               IMapItem enemyMapItem = myGridRows[i].myMapItem;
               if (2 == myRollResultColNum)
               {
                  Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "ShowDieResults(): Hit Location for myState=" + myState.ToString() + " dr=" + dieRoll);
                  myGridRows[i].myDieRollHitLocationYourTank = dieRoll;
                  myGridRows[i].myHitLocationYourTank = TableMgr.GetEnemyHitLocationYourTank(myGameInstance, dieRoll); // Turret or Hull
                  if ("ERROR" == myGridRows[i].myHitLocationYourTank)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): TableMgr.myHitLocationYourTank() returned ERROR");
                     return;
                  }
                  myGridRows[i].myToKillNumberYourTank = (int)TableMgr.GetEnemyToKillNumberYourTank(myGameInstance, myGridRows[i].myMapItem, myGridRows[i].myFacing, myGridRows[i].myRange, myGridRows[i].myHitLocationYourTank);
                  if (TableMgr.FN_ERROR == myGridRows[i].myToKillNumberYourTank)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): TableMgr.GetEnemyToKillNumberYourTank() returned error=" + myGridRows[i].myToKillNumberYourTank.ToString());
                     return;
                  }
                  if ( "Miss" == myGridRows[i].myHitLocationYourTank )
                     myGridRows[i].myDieRollToKillYourTank = NO_FIRE_MISSED_TURRET;
                  if ("Track" == myGridRows[i].myHitLocationYourTank)
                  {
                     myGridRows[i].myDieRollToKillYourTank = NO_FIRE_THROWN_TRACK;
                     myGameInstance.Sherman.IsThrownTrack = true;
                     lastReport.Breakdown = "Thrown Track";
                     myGameInstance.Sherman.IsMoving = false;
                  }
               }
               else if (4 == myRollResultColNum)
               {
                  Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "ShowDieResults(): Killing Your Tank for myState=" + myState.ToString() + " dr=" + dieRoll);
                  myGridRows[i].myDieRollToKillYourTank = dieRoll;
                  if( dieRoll <= myGridRows[i].myToKillNumberYourTank )
                  {
                     myGameInstance.Sherman.SetBloodSpots();
                     myGameInstance.KillSherman(lastReport, "Enemy Action");
                     myGridRows[i].myToKillResultYourTank = "KO";
                     myGameInstance.Death = new ShermanDeath(myGameInstance, enemyMapItem, myGridRows[i].myHitLocationYourTank, "Enemy Action");
                     if( true == myGameInstance.Death.myCtorError)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): ShermanDeath.myCtorError = true");
                        return;
                     }
                     for (int j = 0; j < myMaxRowCount; ++j)
                     {
                        if (Utilities.NO_RESULT == myGridRows[j].myDieRollHitLocationYourTank)
                           myGridRows[j].myDieRollHitLocationYourTank = NO_FIRE_YOUR_TANK;
                        if (Utilities.NO_RESULT == myGridRows[j].myDieRollToKillYourTank)
                           myGridRows[j].myDieRollToKillYourTank = NO_FIRE_YOUR_TANK;
                     }
                  }
                  else
                  {
                     myGridRows[i].myToKillResultYourTank = "No Effect";
                  }
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default myRollResultColNum=" + myRollResultColNum.ToString());
                  return;
               }
               //-----------------------------
               myState = E0475Enum.ENEMY_ACTION_TO_KILL_YOUR_TANK_SHOW;
               if( false == myGameInstance.Sherman.IsKilled )
               {
                  for (int j = 0; j < myMaxRowCount; ++j)
                  {
                     Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "ShowDieResults(): i=" + i.ToString() + " j=" + j.ToString() + " mi=" + myGridRows[j].myMapItem.Name + " dr2" + myGridRows[j].myDieRollToKillYourTank.ToString());
                     if (Utilities.NO_RESULT == myGridRows[j].myDieRollToKillYourTank)
                     {
                        Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "ShowDieResults(): j=" + j.ToString() + " myState=E0475Enum.ENEMY_ACTION_TO_KILL_YOUR_TANK");
                        myState = E0475Enum.ENEMY_ACTION_TO_KILL_YOUR_TANK;
                     }
                  }
               }
               break;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
         //-------------------------------
         Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "ShowDieResults(): ---------------myState=" + myState.ToString());
      }
      private bool ShowDieResultUpdateFacing(Index i)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateFacing(): myGameInstance=null");
            return false;
         }
         //----------------------------
         IMapItem? mi = myGridRows[i].myMapItem;
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateFacing(): mi=null for i=" + i.ToString());
            return false;
         }
         if (false == mi.SetMapItemRotation(myGameInstance.Sherman))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateFacing(): SetMapItemRotation() returned false");
            return false;
         }
         if (false == mi.UpdateMapRotation(myGridRows[i].myFacing))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateFacing(): UpdateMapRotation() returned false");
            return false;
         }
         return true;
      }
      private bool ShowDieResultUpdateTerrain(Index i)
      {
         IMapItem? mi = myGridRows[i].myMapItem;
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResultUpdateFacing(): mi=null for i=" + i.ToString());
            return false;
         }
         mi.IsHullDown = false;
         mi.IsWoods = false;
         mi.IsFortification = false;
         mi.IsBuilding = false;
         mi.IsMovingInOpen = false;
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
                        if ("Move" == img.Name)
                        {
                           for (int j = 0; j < myMaxRowCount; ++j)
                           {
                              if (false == CreateMapItemMove(j))
                              {
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): CreateMapItemMove(i) returned false");
                                 return;
                              }                                
                           }
                           E0475Enum state = myState;
                           myState = E0475Enum.ENEMY_ACTION_MOVE_SHOW;
                           for (int j = 0; j < myMaxRowCount; ++j)
                           {
                              if ((Utilities.NO_RESULT == myGridRows[j].myDieRollTerrain) || (Utilities.NO_RESULT == myGridRows[j].myDieRollFacing))
                                 myState = E0475Enum.ENEMY_ACTION_MOVE;
                           }
                           Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "Grid_MouseDown(): p=" + state.ToString() + "-->" + myState.ToString());
                           myTextBlockHeader.Text = "r4.75 Enemy Action - Move";
                           myTextBlock2.Text = "Vehicle Facing";
                           myTextBlock3.Text = "Terrain";
                           myTextBlock4.Visibility = Visibility.Hidden;
                        }
                        if ("AdvanceFire" == img.Name)
                        {
                           int k = 0;
                           for (int j = 0; j < myMaxRowCount; ++j)
                           {
                              IMapItem enemyUnit = myGridRows[j].myMapItem;
                              if (true == myGridRows[j].myEnemyAction.Contains("Move"))
                              {
                                 if ( (true == enemyUnit.IsVehicle) && ("TRUCK" != enemyUnit.GetEnemyUnit()) ) // Trucks are attacked by MG fire - other vehicles are not
                                    continue;
                                 ITerritory t = enemyUnit.TerritoryCurrent;
                                 IStack? stack = myGameInstance.BattleStacks.Find(t);
                                 if (null == stack)
                                 {
                                    Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): stack=null for t=" + t.Name);
                                    return;
                                 }
                                 foreach (IMapItem advanceFireMarker in stack.MapItems)
                                 {
                                    if (true == advanceFireMarker.Name.Contains("Advance"))
                                    {
                                       myAdvanceFireGridRows[k].myEnemyUnit = enemyUnit;
                                       myAdvanceFireGridRows[k].mySectorRangeDisplay = myGridRows[j].mySectorRangeDisplay;
                                       myAdvanceFireGridRows[k].myAdvanceFire = advanceFireMarker;
                                       string[] aStringArray1 = advanceFireMarker.Name.Split(new char[] { '_' });
                                       if (aStringArray1.Length < 2)
                                       {
                                          Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): underscore not found in " + advanceFireMarker.Name + " len=" + aStringArray1.Length);
                                          return;
                                       }
                                       string mgType = aStringArray1[0];
                                       myAdvanceFireGridRows[k].myAdvanceFireBaseNum = TableMgr.GetShermanMgToKillNumber(myGameInstance, enemyUnit, mgType);
                                       if( TableMgr.FN_ERROR == myAdvanceFireGridRows[k].myAdvanceFireBaseNum)
                                       {
                                          Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): GetShermanMgToKillNumber() returned false");
                                          return;
                                       }
                                       myAdvanceFireGridRows[k].myAdvanceFireModifier = TableMgr.GetShermanMgToKillModifier(myGameInstance, enemyUnit, mgType, true);
                                       if (TableMgr.FN_ERROR == myAdvanceFireGridRows[k].myAdvanceFireModifier)
                                       {
                                          Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): GetShermanMgToKillModifier() returned false");
                                          return;
                                       }
                                       myAdvanceFireGridRows[k].myDieRollAdvanceFire = Utilities.NO_RESULT; 
                                       k++;
                                    }
                                 }
                              }
                              myMaxRowCountAdvanceFire = k;
                           }
                           Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "Grid_MouseDown(): p=" + myState.ToString() + "-->ENEMY_ACTION_ADVANCE_FIRE");
                           myState = E0475Enum.ENEMY_ACTION_ADVANCE_FIRE;
                           myTextBlockHeader.Text = "r22.2 Advance Fire";
                           myTextBlock2.Text = "MG Attack";
                           myTextBlock3.Text = "To Kill #";
                           myTextBlock4.Text = "Modifier";
                           myTextBlock4.Visibility = Visibility.Visible;
                           myTextBlock5.Text = "Result";
                        }
                        if ("Fire" == img.Name)
                        {
                           Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "Grid_MouseDown(): p=" + myState.ToString() + "-->ENEMY_ACTION_FIRE");
                           myState = E0475Enum.ENEMY_ACTION_FIRE;
                           myTextBlockHeader.Text = "r4.75 Enemy Action - Fire At Friends";
                           myTextBlock2.Text = "To Kill #";
                           myTextBlock3.Text = "Roll";
                           myTextBlock4.Visibility = Visibility.Hidden;
                           myTextBlock5.Text = "Result";
                        }
                        if ("ToHitYourTank" == img.Name)
                        {
                           Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "Grid_MouseDown(): p=" + myState.ToString() + "-->ENEMY_ACTION_TO_HIT_YOUR_TANK");
                           myState = E0475Enum.ENEMY_ACTION_TO_HIT_YOUR_TANK;
                           myTextBlockHeader.Text = "r4.75 Enemy Action - To Hit Your Tank";
                           myTextBlock2.Visibility = Visibility.Visible;
                           myTextBlock2.Text = "Hit Number";
                           myTextBlock3.Text = "Hit Modifer";
                           myTextBlock4.Visibility = Visibility.Visible;
                           myTextBlock4.Text = "Die + Modifier";
                           myTextBlock5.Text = "Results";
                        }
                        if ("ToKillYourTank" == img.Name)
                        {
                           Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "Grid_MouseDown(): p=" + myState.ToString() + "-->ENEMY_ACTION_TO_KILL_YOUR_TANK");
                           myState = E0475Enum.ENEMY_ACTION_TO_KILL_YOUR_TANK;
                           myTextBlockHeader.Text = "r4.75 Enemy Action - To Kill Your Tank";
                           myTextBlock2.Text = "Hit Location";
                           myTextBlock3.Text = "To Kill Number";
                           myTextBlock4.Visibility = Visibility.Visible;
                           myTextBlock4.Text = "Die Roll";
                           myTextBlock5.Text = "Results";
                        }
                        if ("Continue" == img.Name)
                           myState = E0475Enum.END;
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
                     RollEndCallback callback = ShowDieResults;
                     if( "DieRoll" == img1.Name )
                        myDieRoller.RollMovingDie(myCanvas, callback);
                     else if ("DiceRoll" == img1.Name)
                        myDieRoller.RollMovingDice(myCanvas, callback);
                     else
                        Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): unknown image name img1.Name=" + img1.Name);
                     img1.Visibility = Visibility.Hidden;
                  }
                  return;
               }
            }
         }
      }
   }
}
