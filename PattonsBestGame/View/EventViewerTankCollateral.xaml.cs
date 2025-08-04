using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;
using static Pattons_Best.EventViewerBattleSetup;
using static Pattons_Best.EventViewerRatingImprove;

namespace Pattons_Best
{
   public partial class EventViewerTankCollateral : UserControl
   {
      public delegate bool EndResolveTankCollateralCallback();
      private const int MAX_GRID_LEN = 5;
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int KIA_CREWMAN = 100;
      private const int BUTTONED_UP = 101;
      private const int NO_WOUNDS = 102;
      public enum E0481Enum
      {
         COLLATERAL_DAMAGE_ROLL,
         COLLATERAL_DAMAGE_ROLL_SHOW,
         COLLATERAL_DAMAGE_WOUND_ROLL,
         COLLATERAL_DAMAGE_WOUND_ROLL_SHOW,
         END
      };
      public bool CtorError { get; } = false;
      private EndResolveTankCollateralCallback? myCallback = null;
      private E0481Enum myState = E0481Enum.COLLATERAL_DAMAGE_ROLL;
      private int myMaxRowCount = 4;
      private int myRollResultRowNum = 0;
      private bool myIsRollInProgress = false;
      //============================================================
      public struct GridRow
      {
         public ICrewMember myCrewMember;
         public int myWoundsModifier = 0;
         public int myDieRollWound = Utilities.NO_RESULT;
         //---------------------------------------------------
         public GridRow(ICrewMember cm)
         {
            myCrewMember = cm;
         }
      };
      private GridRow[] myGridRows = new GridRow[5];
      //============================================================
      private IGameEngine? myGameEngine;
      private IGameInstance? myGameInstance;
      private readonly Canvas? myCanvas;
      private readonly ScrollViewer? myScrollViewer;
      private RuleDialogViewer? myRulesMgr;
      private IDieRoller? myDieRoller;
      private int myDieRollCollateral = Utilities.NO_RESULT;
      private string myCollateralDamage = "Uninit";
      private string myWoundsResults = "Uninit";
      //---------------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-------------------------------------------------------------------------------------
      public EventViewerTankCollateral(IGameEngine? ge, IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
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
      public bool ResolveCollateralDamage(EndResolveTankCollateralCallback callback)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveCollateralDamage(): myGameInstance=null");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "ResolveCollateralDamage(): ++++++++++++++++++++++++++++++ battlestacks=" + myGameInstance.BattleStacks.ToString());
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveCollateralDamage(): myCanvas=null");
            return false;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveCollateralDamage(): myScrollViewer=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveCollateralDamage(): myRulesMgr=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveCollateralDamage(): myDieRoller=null");
            return false;
         }
         //--------------------------------------------------
         if( 0 == myGameInstance.NumCollateralDamage )
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveCollateralDamage(): improper state NumCollateralDamage=0");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[MAX_GRID_LEN];
         myState = E0481Enum.COLLATERAL_DAMAGE_ROLL;
         myIsRollInProgress = false;
         myRollResultRowNum = 0;
         myCallback = callback;
         myDieRollCollateral = Utilities.NO_RESULT;
         myCollateralDamage = "Uninit";
         int i = 0;
         string[] crewmembers = new string[4] { "Driver", "Assistant", "Loader", "Commander" }; // must be in this order
         foreach (string crewmember in crewmembers)
         {
            ICrewMember? cm = myGameInstance.GetCrewMember(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "ImproveCrewRatings(): cm=null for name=" + crewmember);
               return false;
            }
            myGridRows[i] = new GridRow(cm);
            if (true == cm.IsButtonedUp)
               myGridRows[i].myDieRollWound = BUTTONED_UP;
            if (true == cm.IsKilled)
               myGridRows[i].myDieRollWound = KIA_CREWMAN;
            ++i;
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveCollateralDamage(): UpdateGrid() return false");
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
         if (E0481Enum.END == myState)
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
         if (E0481Enum.END == myState)
         {
            if (null == myGameInstance)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myGameInstance=null");
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "EventViewerTankCollateral.UpdateEndState(): ------------------------------ battlestacks=" + myGameInstance.BattleStacks.ToString());
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
            case E0481Enum.COLLATERAL_DAMAGE_ROLL:
               myTextBlockInstructions.Inlines.Add(new Run("Roll on "));
               Button bCollateral = new Button() { Content = "Collateral", FontFamily = myFontFam1, FontSize = 8 };
               bCollateral.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bCollateral));
               myTextBlockInstructions.Inlines.Add(new Run(" Damage Table."));
               break;
            case E0481Enum.COLLATERAL_DAMAGE_ROLL_SHOW:
            case E0481Enum.COLLATERAL_DAMAGE_WOUND_ROLL_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            case E0481Enum.COLLATERAL_DAMAGE_WOUND_ROLL:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die to determine wounds."));
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
            case E0481Enum.COLLATERAL_DAMAGE_ROLL:
               Rectangle r11 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r11);
               Rectangle r12 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r12);
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
               bmi.EndInit();
               System.Windows.Controls.Image img1 = new System.Windows.Controls.Image { Name = "DiceRoll", Source = bmi, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img1, bmi);
               myStackPanelAssignable.Children.Add(img1);
               break;
            case E0481Enum.COLLATERAL_DAMAGE_ROLL_SHOW:
            case E0481Enum.COLLATERAL_DAMAGE_WOUND_ROLL:
               Image? img = GetImageFromDamage();
               if( null == img )
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): GetImageFromDamage() returned null");
                  return false;
               }
               myStackPanelAssignable.Children.Add(img);
               Rectangle r13 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r13);
               StringBuilder sb = new StringBuilder();
               sb.Append(myDieRollCollateral.ToString());
               sb.Append(" = ");
               sb.Append(myCollateralDamage);
               Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sb.ToString() };
               myStackPanelAssignable.Children.Add(label1);
               break;
            case E0481Enum.COLLATERAL_DAMAGE_WOUND_ROLL_SHOW:
               Image img4 = new System.Windows.Controls.Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img4);
               Rectangle r14 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r14);
               StringBuilder sb4 = new StringBuilder();
               sb4.Append(myWoundsResults);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = sb4.ToString() };
               myStackPanelAssignable.Children.Add(label4);
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
         for(int i=0; i < myMaxRowCount; ++i )
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            GridRow row = myGridRows[i];
            ICrewMember crewMember = row.myCrewMember;
            Button b = CreateButton(crewMember);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            Label ratingLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = crewMember.Rating.ToString() };
            myGrid.Children.Add(ratingLabel);
            Grid.SetRow(ratingLabel, rowNum);
            Grid.SetColumn(ratingLabel, 1);
            if (false == crewMember.IsButtonedUp)
            {
               ITerritory? t = Territories.theTerritories.Find("OffBottomRight");
               if (null == t)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): reached default OffBottomRight=null");
                  return false;
               }
               IMapItem buttonUp = new MapItem(crewMember.Role + "OpenHatch", 1.0, "c15OpenHatch", t);
               Button b2 = CreateButton(buttonUp);
               myGrid.Children.Add(b2);
               Grid.SetRow(b2, rowNum);
               Grid.SetColumn(b2, 2);
            }
            else
            {
               Label buLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "No" };
               myGrid.Children.Add(buLabel);
               Grid.SetRow(buLabel, rowNum);
               Grid.SetColumn(buLabel, 2);
            }
            //-------------------------------
            string content = "NA";
            if( null != myGameInstance.Death )
               content = myGameInstance.Death.myEnemyFireDirection;
            Label labelDirection = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = content};
            myGrid.Children.Add(labelDirection);
            Grid.SetRow(labelDirection, rowNum);
            Grid.SetColumn(labelDirection, 3);
            //-------------------------------
            switch (myState)
            {
               case E0481Enum.COLLATERAL_DAMAGE_ROLL:
               case E0481Enum.COLLATERAL_DAMAGE_ROLL_SHOW:
                  break;
               case E0481Enum.COLLATERAL_DAMAGE_WOUND_ROLL:
                  if (NO_WOUNDS == myGridRows[i].myDieRollWound)
                  {
                     Label modifier = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
                     myGrid.Children.Add(modifier);
                     Grid.SetRow(modifier, rowNum);
                     Grid.SetColumn(modifier, 4);
                     Label dieRollLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
                     myGrid.Children.Add(dieRollLabel);
                     Grid.SetRow(dieRollLabel, rowNum);
                     Grid.SetColumn(dieRollLabel, 5);
                     Label resultLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
                     myGrid.Children.Add(resultLabel);
                     Grid.SetRow(resultLabel, rowNum);
                     Grid.SetColumn(resultLabel, 6);
                  }
                  else 
                  {
                     Label modifier = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myWoundsModifier.ToString() };
                     myGrid.Children.Add(modifier);
                     Grid.SetRow(modifier, rowNum);
                     Grid.SetColumn(modifier, 4);
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
                  break;
               case E0481Enum.COLLATERAL_DAMAGE_WOUND_ROLL_SHOW:
                  if (NO_WOUNDS == myGridRows[i].myDieRollWound)
                  {
                     Label modifier = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
                     myGrid.Children.Add(modifier);
                     Grid.SetRow(modifier, rowNum);
                     Grid.SetColumn(modifier, 4);
                     Label dieRollLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
                     myGrid.Children.Add(dieRollLabel);
                     Grid.SetRow(dieRollLabel, rowNum);
                     Grid.SetColumn(dieRollLabel, 5);
                     Label resultLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
                     myGrid.Children.Add(resultLabel);
                     Grid.SetRow(resultLabel, rowNum);
                     Grid.SetColumn(resultLabel, 6);
                  }
                  else
                  {
                     Label modifier = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myWoundsModifier.ToString() };
                     myGrid.Children.Add(modifier);
                     Grid.SetRow(modifier, rowNum);
                     Grid.SetColumn(modifier, 4);
                     Label dieRollLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRollWound.ToString() };
                     myGrid.Children.Add(dieRollLabel);
                     Grid.SetRow(dieRollLabel, rowNum);
                     Grid.SetColumn(dieRollLabel, 5);
                     int diePlusMod = myGridRows[i].myDieRollWound + myGridRows[i].myWoundsModifier;
                     Label resultLabel = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = diePlusMod.ToString() };
                     myGrid.Children.Add(resultLabel);
                     Grid.SetRow(resultLabel, rowNum);
                     Grid.SetColumn(resultLabel, 6);
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
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = mi.Name;
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(0);
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         MapItem.SetButtonContent(b, mi, false, true, true); // This sets the image as the button's content
         return b;
      }
      private Image? GetImageFromDamage()
      {
         if( null == myGameInstance)
         { 
            Logger.Log(LogEnum.LE_ERROR, "GetImageFromDamage(): myGameInstance=null");
            return null;
         }
         Image? img = null;
         string name = "Continue";
         if (0 < myGameInstance.NumCollateralDamage)
            name = "CollateralCheck";
         if (true == myCollateralDamage.Contains("Wound"))
            img = new System.Windows.Controls.Image { Name = "Wounded", Source = MapItem.theMapImages.GetBitmapImage("OBlood1"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         else if (true == myCollateralDamage.Contains("Periscope"))
            img = new System.Windows.Controls.Image { Name = name, Source = MapItem.theMapImages.GetBitmapImage("BrokenPeriscope"), Width = Utilities.ZOOM * Utilities.theMapItemSize * 2, Height = Utilities.ZOOM * Utilities.theMapItemSize * 2 };
         else if (true == myCollateralDamage.Contains("Gunsight"))
            img = new System.Windows.Controls.Image { Name = name, Source = MapItem.theMapImages.GetBitmapImage("BrokenGunsight"), Width = Utilities.ZOOM * Utilities.theMapItemSize * 2, Height = Utilities.ZOOM * Utilities.theMapItemSize * 2 };
         else if (true == myCollateralDamage.Contains("AA MG"))
            img = new System.Windows.Controls.Image { Name = name, Source = MapItem.theMapImages.GetBitmapImage("c115BrokenAaMg"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         else
            img = new System.Windows.Controls.Image { Name = name, Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         return img;
      }
      //------------------------------------------------------------------------------------
      public void ShowDieResults(int dieRoll)
      {
         Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "EventViewerTankCollateral.ShowDieResults(): ++++++++++++++myState=" + myState.ToString());
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
         //-------------------------------
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "ShowDieResults(): myGameInstance.MapItemMoves.Clear()");
         myGameInstance.MapItemMoves.Clear();
         //-------------------------------
         switch (myState)
         {
            case E0481Enum.COLLATERAL_DAMAGE_ROLL:
               myGameInstance.NumCollateralDamage--;
               //dieRoll = 91; // <cgs> TEST - Commander Collateral Damage
               myDieRollCollateral = dieRoll;
               myCollateralDamage = TableMgr.GetCollateralDamage(myGameInstance, dieRoll);
               if ("ERROR" == myCollateralDamage)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): GetCollateralDamage() returned ERROR");
                  return;
               }
               if( "No Effect" == myCollateralDamage)
               {
                  for (int k = 0; k < myMaxRowCount; ++k)
                     myGridRows[k].myDieRollWound = NO_WOUNDS;
               }
               else
               {
                  StringBuilder sb = new StringBuilder("At ");
                  sb.Append(TableMgr.GetTime(lastReport));
                  sb.Append(", Tank suffered ");
                  sb.Append(myCollateralDamage);
                  lastReport.Notes.Add(sb.ToString());
                  for (int k = 0; k < myMaxRowCount; ++k)
                  {
                     ICrewMember cm1 = myGridRows[k].myCrewMember;
                     if (null == cm1)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): GetCollateralDamage() returned ERROR");
                        return;
                     }
                     if (false == myCollateralDamage.Contains(cm1.Role))
                     {
                        myGridRows[k].myDieRollWound = NO_WOUNDS;
                     }
                     else
                     {
                        myGridRows[k].myWoundsModifier = TableMgr.GetWoundsModifier(myGameInstance, cm1, false, false, true);
                        if (TableMgr.FN_ERROR == myGridRows[k].myWoundsModifier)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): TableMgr.GetWoundsModifier() returned error for k=" + k.ToString());
                           return;
                        }
                     }
                  }
               }
               myState = E0481Enum.COLLATERAL_DAMAGE_ROLL_SHOW;
               break;
            case E0481Enum.COLLATERAL_DAMAGE_WOUND_ROLL:
               int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
               if (i < 0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString());
                  return;
               }
               myGridRows[i].myDieRollWound = dieRoll;
               ICrewMember cm = myGridRows[i].myCrewMember;
               myWoundsResults = TableMgr.SetWounds(myGameInstance, cm, dieRoll, myGridRows[i].myWoundsModifier);
               if ("ERROR" == myWoundsResults)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): TableMgr.GetWounds() returned ERROR");
                  return;
               }
               StringBuilder sb1 = new StringBuilder("At ");
               sb1.Append(TableMgr.GetTime(lastReport));
               sb1.Append(", ");
               sb1.Append(cm.Name);
               sb1.Append(" (");
               sb1.Append(cm.Role);
               sb1.Append(" ) suffered ");
               sb1.Append(myWoundsResults);
               lastReport.Notes.Add(sb1.ToString());
               myState = E0481Enum.COLLATERAL_DAMAGE_WOUND_ROLL_SHOW;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default myState=" + myState.ToString());
               return;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
         //-------------------------------
         Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "ShowDieResults(): ---------------myState=" + myState.ToString());
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
               Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): ShowRule() returned false");
               return;
            }
         }
         else  // table based click
         {
            if (false == myRulesMgr.ShowTable(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): ShowTable() returned false");
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
                     if ("DiceRoll" == img.Name)
                     {
                        if (false == myIsRollInProgress)
                        {
                           myIsRollInProgress = true;
                           RollEndCallback callback = ShowDieResults;
                           myDieRoller.RollMovingDice(myCanvas, callback);
                           img.Visibility = Visibility.Hidden;
                        }
                        return;
                     }
                     if ("CollateralCheck" == img.Name)
                     {
                        myState = E0481Enum.COLLATERAL_DAMAGE_ROLL;
                        myIsRollInProgress = false;
                        myRollResultRowNum = 0;
                        myDieRollCollateral = Utilities.NO_RESULT;
                        myCollateralDamage = "Uninit";
                        for( int i=0; i < myMaxRowCount; ++i )
                        {
                           ICrewMember cm = myGridRows[i].myCrewMember;
                           if (true == cm.IsButtonedUp)
                              myGridRows[i].myDieRollWound = BUTTONED_UP;
                           else if (true == cm.IsKilled)
                              myGridRows[i].myDieRollWound = KIA_CREWMAN;
                           else
                              myGridRows[i].myDieRollWound = Utilities.NO_RESULT;
                        }
                     }
                     if ("Wounded" == img.Name)
                        myState = E0481Enum.COLLATERAL_DAMAGE_WOUND_ROLL;
                     if ("Continue" == img.Name)
                        myState = E0481Enum.END;
                  }
               }
            }
            else if (ui is Image img1) // next check all images within the Grid Rows
            {
               if (result.VisualHit == img1)
               {
                  if (false == myIsRollInProgress)
                  {
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
                     return;
                  }
               }
            }
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
      }
   }
}
