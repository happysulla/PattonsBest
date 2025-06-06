using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace Pattons_Best
{
   public partial class EventViewerTankDestroyed : UserControl
   {
      public delegate bool EndResolveTankDestroyedCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int KIA_CREWMAN = 0;
      private const int NO_BAILOUT = 100;
      public enum E0481Enum
      {
         TANK_EXPLOSION_ROLL,
         TANK_EXPLOSION_ROLL_SHOW,
         WOUNDS_ROLL,
         WOUNDS_ROLL_SHOW,
         BAILOUT_ROLL,
         BAILOUT_ROLL_SHOW,
         BAILOUT_WOUNDS_ROLL,
         BAILOUT_WOUNDS_ROLL_SHOW,
         BAILOUT_RESCUE_SELECT,
         BAILOUT_RESCUE_ROLL,
         BAILOUT_RESCUE_ROLL_SHOW,
         END
      };
      public bool CtorError { get; } = false;
      private EndResolveTankDestroyedCallback? myCallback = null;
      private E0481Enum myState = E0481Enum.TANK_EXPLOSION_ROLL;
      private int myMaxRowCountWound = 0;
      private int myMaxRowCountRescue = 0;
      private int myRollResultRowNum = 0;
      private bool myIsRollInProgress = false;
      //============================================================
      public struct GridRowExplode
      {
         public IMapItem myMapItem;
         //---------------------------------------------------
         public int myExplosionModifier = 0;
         public int myDieRollExplosion = Utilities.NO_RESULT;
         public string myExplosionResult = "Uninit";
         //---------------------------------------------------
         public GridRowExplode(IMapItem mi)
         {
            myMapItem = mi;
         }
      };
      private GridRowExplode[] myGridRowExplodes = new GridRowExplode[7];
      public struct GridRowWound
      {
         public ICrewMember myCrewMember;
         //---------------------------------------------------
         public int myWoundModifier = 0;
         public int myDieRollWound = Utilities.NO_RESULT;
         public string myWoundResult = "Uninit";
         public string myWoundEffect = "Uninit";
         //------------------------------------
         public string myBailoutEffect = "Uninit";
         public int myBailOutModifier =  0;
         public int myDieRollBailout = Utilities.NO_RESULT;
         public string myBailOutResult = "Uninit";
         //------------------------------------
         public int myBailoutWoundModifier = 0;
         public int myDieRollBailoutWound = Utilities.NO_RESULT;
         public string myBailoutWoundResult = "Uninit";
         public string myBailoutWoundEffect = "Uninit";
         //---------------------------------------------------
         public GridRowWound(ICrewMember cm)
         {
            myCrewMember = cm;
         }
      };
      private GridRowWound[] myGridRowWounds = new GridRowWound[11];
      public struct GridRowRescue
      {
         public ICrewMember myCrewMember;
         //------------------------------------
         public ICrewMember? myCrewMemberRescuing = null;
         public int myDieRollRescue = Utilities.NO_RESULT;
         public string myRescueResult = "Uninit";
         public string myRescueEffect = "Uninit";
         //---------------------------------------------------
         public GridRowRescue(ICrewMember cm)
         {
            myCrewMember = cm;
         }
      };
      private GridRowRescue[] myGridRowRescues = new GridRowRescue[11];
      //============================================================
      private IGameEngine? myGameEngine;
      private IGameInstance? myGameInstance;
      private readonly Canvas? myCanvas;
      private readonly ScrollViewer? myScrollViewer;
      private RuleDialogViewer? myRulesMgr;
      private IDieRoller? myDieRoller;
      //---------------------------------------------------
      private IMapItems myAssignables = new MapItems();    // listing of new crewmen who can rescue others
      private ICrewMember? mySelectedCrewman = null;       // crewmember selected to be resucer
      //---------------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-------------------------------------------------------------------------------------
      public EventViewerTankDestroyed(IGameEngine? ge, IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
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
      public bool ResolveTankDestroyed(EndResolveTankDestroyedCallback callback)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveTankDestroyed(): myGameInstance=null");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "ResolveTankDestroyed(): ++++++++++++++++++++++++++++++ battlestacks=" + myGameInstance.BattleStacks.ToString());
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveTankDestroyed(): myCanvas=null");
            return false;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveTankDestroyed(): myScrollViewer=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveTankDestroyed(): myRulesMgr=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveTankDestroyed(): myDieRoller=null");
            return false;
         }
         if (null == callback)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveTankDestroyed(): callback=null");
            return false;
         }
         //--------------------------------------------------
         myCallback = callback;
         myState = E0481Enum.TANK_EXPLOSION_ROLL;
         myMaxRowCountWound = 0;
         myMaxRowCountRescue = 0;
         myRollResultRowNum = 0;
         myIsRollInProgress = false;
         //--------------------------------------------------
         int i = 0;
         string[] crewmembers = new string[5] { "Commander", "Gunner", "Loader", "Driver", "Assistant" };
         foreach (string crewmember in crewmembers)
         {
            ICrewMember? cm = myGameInstance.GetCrewMember(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResolveTankDestroyed(): cm=null for name=" + crewmember);
               return false;
            }
            myGridRowWounds[i] = new GridRowWound(cm);
            myGridRowWounds[i].myWoundModifier = TableMgr.GetWoundsModifier(myGameInstance, cm, true, false, false);
            myGridRowWounds[i].myBailoutWoundModifier = TableMgr.GetWoundsModifier(myGameInstance, cm, true, true, false);
            if (true == cm.IsKilled)
               myGridRowWounds[i].myDieRollWound = KIA_CREWMAN;
            ++i;
         }
         myMaxRowCountWound = i;
         //--------------------------------------------------
         if ( null == myGameInstance.Death )
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveTankDestroyed(): myGameInstance.Death=null");
            return false;
         }
         ShermanDeath death = myGameInstance.Death;
         myGridRowExplodes[0] = new GridRowExplode(myGameInstance.Sherman);
         myGridRowExplodes[0].myExplosionModifier = TableMgr.GetExplosionModifier(myGameInstance, death.myEnemyUnit, death.myHitLocation);
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveTankDestroyed(): UpdateGrid() return false");
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRowExplodes() returned false");
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
            Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "EventViewerTankDestroyed.UpdateEndState(): ------------------------------ battlestacks=" + myGameInstance.BattleStacks.ToString());
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
            case E0481Enum.TANK_EXPLOSION_ROLL:
               myTextBlockInstructions.Inlines.Add(new Run("Roll on Tank "));
               Button bExplosion = new Button() { Content = "Explosion", FontFamily = myFontFam1, FontSize = 8 };
               bExplosion.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bExplosion));
               myTextBlockInstructions.Inlines.Add(new Run(" Table."));
               break;
            case E0481Enum.TANK_EXPLOSION_ROLL_SHOW:
               if("Explodes" == myGridRowExplodes[0].myExplosionResult)
                  myTextBlockInstructions.Inlines.Add(new Run("All crew members die. Click image to continue."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            case E0481Enum.WOUNDS_ROLL:
               myTextBlockInstructions.Inlines.Add(new Run("Roll on "));
               Button bWounds = new Button() { Content = "Wounds", FontFamily = myFontFam1, FontSize = 8 };
               bWounds.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bWounds));
               myTextBlockInstructions.Inlines.Add(new Run(" Table for each crew member."));
               break;
            case E0481Enum.WOUNDS_ROLL_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            case E0481Enum.BAILOUT_ROLL:
               myTextBlockInstructions.Inlines.Add(new Run("Roll on "));
               Button bBailout = new Button() { Content = "Bail Out", FontFamily = myFontFam1, FontSize = 8 };
               bBailout.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(bBailout));
               myTextBlockInstructions.Inlines.Add(new Run(" Table for each surviving crew member."));
               break;
            case E0481Enum.BAILOUT_ROLL_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            case E0481Enum.BAILOUT_WOUNDS_ROLL:
               myTextBlockInstructions.Inlines.Add(new Run("Roll on "));
               Button b1Wounds = new Button() { Content = "Wounds", FontFamily = myFontFam1, FontSize = 8 };
               b1Wounds.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(b1Wounds));
               myTextBlockInstructions.Inlines.Add(new Run(" Table for bailing crew members."));
               break;
            case E0481Enum.BAILOUT_WOUNDS_ROLL_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            case E0481Enum.BAILOUT_RESCUE_SELECT:
               myTextBlockInstructions.Inlines.Add(new Run("Select a rescuer by clicking and dragging to rescuer column."));
               break;
            case E0481Enum.BAILOUT_RESCUE_ROLL:
               myTextBlockInstructions.Inlines.Add(new Run("Roll on "));
               Button b1Rescue = new Button() { Content = "Wounds", FontFamily = myFontFam1, FontSize = 8 };
               b1Rescue.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(b1Rescue));
               myTextBlockInstructions.Inlines.Add(new Run(" Table for rescuing crew member."));
               break;
            case E0481Enum.BAILOUT_RESCUE_ROLL_SHOW:
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowExplodes(): myGameInstance=null");
            return false;
         }
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case E0481Enum.TANK_EXPLOSION_ROLL:
               Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r1);
               break;
            case E0481Enum.TANK_EXPLOSION_ROLL_SHOW:
               if (true == myGridRowExplodes[0].myExplosionResult.Contains("Explodes"))
               {
                  Image img0 = new Image { Name = "Explodes", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img0);
               }
               else
               {
                  Image img111 = new Image { Name = "Wounds", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img111);
               }
               Rectangle r11 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r11);
               break;
            case E0481Enum.WOUNDS_ROLL:
               Rectangle r12 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r12);
               break;
            case E0481Enum.WOUNDS_ROLL_SHOW:
               Image img1 = new Image { Name = "BailOut", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img1);
               Rectangle r13 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r13);
               break;
            case E0481Enum.BAILOUT_ROLL:
               Rectangle r14 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r14);
               break;
            case E0481Enum.BAILOUT_ROLL_SHOW:
               Image img11 = new Image { Name = "BailoutWound", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img11);
               break;
            case E0481Enum.BAILOUT_WOUNDS_ROLL:
               Rectangle r15 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r15);
               break;
            case E0481Enum.BAILOUT_WOUNDS_ROLL_SHOW:
               Image img15 = new Image { Name = "Rescue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img15);
               break;
            case E0481Enum.BAILOUT_RESCUE_SELECT:
               if (0 < myAssignables.Count)
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
                  Image img112 = new Image { Name = "Continue", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img112);
               }
               break;
            case E0481Enum.BAILOUT_RESCUE_ROLL:
            case E0481Enum.BAILOUT_RESCUE_ROLL_SHOW:
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowExplodes(): myGameEngine=null");
            return false;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowExplodes(): myGameInstance=null");
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
         switch (myState)
         {
            case E0481Enum.TANK_EXPLOSION_ROLL:
            case E0481Enum.TANK_EXPLOSION_ROLL_SHOW:
               if (false == UpdateGridRowExplodes())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowExplodes(): UpdateGridRowExplodes() returned false");
                  return false;
               }
               break;
            case E0481Enum.WOUNDS_ROLL:
            case E0481Enum.WOUNDS_ROLL_SHOW:
               if (false == UpdateGridRowWounds())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowExplodes(): UpdateGridRowWounds() returned false");
                  return false;
               }
               break;
            case E0481Enum.BAILOUT_ROLL:
            case E0481Enum.BAILOUT_ROLL_SHOW:
               if (false == UpdateGridRowBailout())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowExplodes(): UpdateGridRowBailout() returned false");
                  return false;
               }
               break;
            case E0481Enum.BAILOUT_WOUNDS_ROLL:
            case E0481Enum.BAILOUT_WOUNDS_ROLL_SHOW:
               if (false == UpdateGridRowBailoutWound())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowExplodes(): UpdateGridRowBailoutWound() returned false");
                  return false;
               }
               break;
            case E0481Enum.BAILOUT_RESCUE_SELECT:
            case E0481Enum.BAILOUT_RESCUE_ROLL:
            case E0481Enum.BAILOUT_RESCUE_ROLL_SHOW:
               if (false == UpdateGridRowBailoutRescue())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowExplodes(): UpdateGridRowBailoutRescue() returned false");
                  return false;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowExplodes(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      //------------------------------------------------------------------------------------
      private bool UpdateGridRowExplodes()
      {
         Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "UpdateGridRowExplodes(): myState=" + myState.ToString());
         //-----------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum1 = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum1)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //-----------------------------
         int rowNum = STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRowExplodes[0].myMapItem;
         Button b1 = CreateButton(mi);
         myGrid.Children.Add(b1);
         Grid.SetRow(b1, rowNum);
         Grid.SetColumn(b1, 0);
         //----------------------------
         Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowExplodes[0].myExplosionModifier };
         myGrid.Children.Add(label1);
         Grid.SetRow(label1, rowNum);
         Grid.SetColumn(label1, 1);
         //----------------------------
         if (Utilities.NO_RESULT == myGridRowExplodes[0].myDieRollExplosion)
         {
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
            bmi.EndInit();
            System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "DiceRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
            ImageBehavior.SetAnimatedSource(img, bmi);
            myGrid.Children.Add(img);
            Grid.SetRow(img, rowNum);
            Grid.SetColumn(img, 2);
         }
         else 
         {
            Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowExplodes[0].myDieRollExplosion.ToString() };
            myGrid.Children.Add(label2);
            Grid.SetRow(label2, rowNum);
            Grid.SetColumn(label2, 2);
            int combo = myGridRowExplodes[0].myExplosionModifier + myGridRowExplodes[0].myDieRollExplosion;
            Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = combo.ToString() };
            myGrid.Children.Add(label3);
            Grid.SetRow(label3, rowNum);
            Grid.SetColumn(label3, 3);
            Label label4 = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowExplodes[0].myExplosionResult };
            myGrid.Children.Add(label4);
            Grid.SetRow(label4, rowNum);
            Grid.SetColumn(label4, 4);
         }
         return true;
      }
      private bool UpdateGridRowWounds()
      {
         Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "UpdateGridRowWounds(): myState=" + myState.ToString());
         //-----------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum1 = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum1)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //-----------------------------
         for ( int i = 0; i<myMaxRowCountWound; ++i)
         {
            int rowNum = STARTING_ASSIGNED_ROW + i;
            ICrewMember cm = myGridRowWounds[i].myCrewMember;
            Button b1 = CreateButton(cm);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myWoundModifier.ToString() };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 1);
            //----------------------------
            if (Utilities.NO_RESULT == myGridRowWounds[i].myDieRollWound)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
               bmi.EndInit();
               System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "DiceRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img, bmi);
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 2);
            }
            else
            {
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myDieRollWound.ToString() };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               int combo = myGridRowWounds[i].myWoundModifier + myGridRowWounds[i].myDieRollWound;
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = combo.ToString() };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myWoundResult };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
            }
         }
         return true;
      }
      private bool UpdateGridRowBailout()
      {
         Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "UpdateGridRowBailout(): myState=" + myState.ToString());
         //-----------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum1 = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum1)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //-----------------------------
         for (int i = 0; i < myMaxRowCountWound; ++i)
         {
            int rowNum = STARTING_ASSIGNED_ROW + i;
            ICrewMember cm = myGridRowWounds[i].myCrewMember;
            Button b1 = CreateButton(cm);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myBailoutEffect };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 1);
            //----------------------------
            string content = myGridRowWounds[i].myBailOutModifier.ToString();
            if (NO_BAILOUT == myGridRowWounds[i].myDieRollBailout)
               content = "NA";
            Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = content };
            myGrid.Children.Add(label2);
            Grid.SetRow(label2, rowNum);
            Grid.SetColumn(label2, 2);
            //----------------------------
            if (NO_BAILOUT == myGridRowWounds[i].myDieRollBailout)
            {
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
            }
            else if (Utilities.NO_RESULT == myGridRowWounds[i].myDieRollBailout)
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
            else
            {
               int combo = myGridRowWounds[i].myBailOutModifier + myGridRowWounds[i].myDieRollBailout;
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = combo.ToString() };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myBailOutResult };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
            }
         }
         return true;
      }
      private bool UpdateGridRowBailoutWound()
      {
         Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "UpdateGridRowBailout(): myState=" + myState.ToString());
         //-----------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum1 = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum1)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //-----------------------------
         for (int i = 0; i < myMaxRowCountWound; ++i)
         {
            int rowNum = STARTING_ASSIGNED_ROW + i;
            ICrewMember cm = myGridRowWounds[i].myCrewMember;
            Button b1 = CreateButton(cm);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myBailoutWoundModifier.ToString() };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 1);
            //----------------------------
            if (NO_BAILOUT == myGridRowWounds[i].myDieRollBailout)
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
            }
            else if (Utilities.NO_RESULT == myGridRowWounds[i].myDieRollBailoutWound)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
               bmi.EndInit();
               System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "DiceRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img, bmi);
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 2);
            }
            else
            {
               int combo = myGridRowWounds[i].myBailoutWoundModifier + myGridRowWounds[i].myDieRollBailoutWound;
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = combo.ToString() };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myBailoutWoundResult };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myBailoutWoundEffect };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
            }
         }
         return true;
      }
      private bool UpdateGridRowBailoutRescue()
      {
         Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "UpdateGridRowBailoutRescue(): myState=" + myState.ToString());
         //-----------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum1 = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum1)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //-----------------------------
         for (int i = 0; i < myMaxRowCountWound; ++i)
         {
            int rowNum = STARTING_ASSIGNED_ROW + i;
            ICrewMember cm = myGridRowWounds[i].myCrewMember;
            Button b1 = CreateButton(cm);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 0);
            Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myBailoutWoundModifier.ToString() };
            myGrid.Children.Add(label1);
            Grid.SetRow(label1, rowNum);
            Grid.SetColumn(label1, 1);
            //----------------------------
            if (NO_BAILOUT == myGridRowWounds[i].myDieRollBailoutWound)
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
            }
            else if (Utilities.NO_RESULT == myGridRowWounds[i].myDieRollBailoutWound)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
               bmi.EndInit();
               System.Windows.Controls.Image img = new System.Windows.Controls.Image { Name = "DiceRoll", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img, bmi);
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 2);
            }
            else
            {
               int combo = myGridRowWounds[i].myBailoutWoundModifier + myGridRowWounds[i].myDieRollBailoutWound;
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = combo.ToString() };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myBailoutWoundResult };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               Label label4 = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myBailoutWoundEffect };
               myGrid.Children.Add(label4);
               Grid.SetRow(label4, rowNum);
               Grid.SetColumn(label4, 4);
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
      private Button CreateButton(ICrewMember cm)
      {
         System.Windows.Controls.Button b = new System.Windows.Controls.Button { };
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(1);
         b.BorderBrush = Brushes.Black;
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         CrewMember.SetButtonContent(b, cm); // This sets the image as the button's content
         return b;
      }
      //------------------------------------------------------------------------------------
      public void ShowDieResults(int dieRoll)
      {
         Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "EventViewerTankDestroyed.ShowDieResults(): ++++++++++++++myState=" + myState.ToString() + " dr=" + dieRoll.ToString());
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
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): lastReport=null");
            return;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //-------------------------------
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "ShowDieResults(): myGameInstance.MapItemMoves.Clear()");
         myGameInstance.MapItemMoves.Clear();
         //-------------------------------
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString());
            return;
         }
         //-------------------------------
         switch (myState)
         {
            case E0481Enum.TANK_EXPLOSION_ROLL:
               dieRoll = 110; // <cgs> TEST - tank explodes
               myGridRowExplodes[0].myDieRollExplosion = dieRoll;
               int rollPlusModifier = dieRoll + myGridRowExplodes[0].myDieRollExplosion;
               if( 99 < rollPlusModifier )
               {
                  myGridRowExplodes[0].myExplosionResult = "Explodes";
                  string[] crewmembers = new string[5] { "Driver", "Assistant", "Commander", "Loader", "Gunner" };
                  foreach (string crewmember in crewmembers)
                  {
                     ICrewMember? cm0 = myGameInstance.GetCrewMember(crewmember);
                     if (null == cm0)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myGameInstance.GetCrewMember() returned null for " + crewmember);
                        return;
                     }
                     cm0.IsKilled = true;
                     cm0.SetBloodSpots();
                  }
                  GameAction outAction = GameAction.UpdateTankExplosion;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
               else
               {
                  myGridRowExplodes[0].myExplosionResult = "Penetration";
               }
               myState = E0481Enum.TANK_EXPLOSION_ROLL_SHOW;
               break;
            case E0481Enum.WOUNDS_ROLL:
               myGridRowWounds[i].myDieRollWound = dieRoll;
               ICrewMember cm = myGridRowWounds[i].myCrewMember;
               myGridRowWounds[i].myWoundResult = TableMgr.SetWounds(myGameInstance, cm, dieRoll, myGridRowWounds[i].myWoundModifier);
               if ("ERROR" == myGridRowWounds[i].myWoundResult)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): TableMgr.SetWounds() returned ERROR");
                  return;
               }
               //--------------------------------------
               myGridRowWounds[i].myWoundEffect = TableMgr.GetWoundEffect(myGameInstance, cm, dieRoll, myGridRowWounds[i].myWoundModifier);
               myGridRowWounds[i].myBailoutEffect = TableMgr.GetBailoutEffectResult(myGameInstance, cm, dieRoll, myGridRowWounds[i].myWoundModifier);
               if (("Loader" == cm.Role) && (false == card.myIsLoaderHatch))
                  myGridRowWounds[i].myBailOutModifier += 1;
               switch (myGridRowWounds[i].myBailoutEffect)
               {
                  case "Cannot Bail":
                     myGridRowRescues[myMaxRowCountRescue] = new GridRowRescue(cm);
                     myMaxRowCountRescue++;
                     myGridRowWounds[i].myDieRollBailout      = NO_BAILOUT;
                     myGridRowWounds[i].myDieRollBailoutWound = NO_BAILOUT;
                     break;
                  case "None":
                     break;
                  case "Bail out +2":
                     myGridRowWounds[i].myBailOutModifier += 2;
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default bailouteffect=" + myGridRowWounds[i].myBailoutEffect);
                     return;
               }
               //--------------------------------------
               if (false == myGridRowWounds[i].myWoundResult.Contains("Near Miss"))
               {
                  StringBuilder sb1 = new StringBuilder("At ");
                  sb1.Append(TableMgr.GetTime(lastReport));
                  sb1.Append(" when bailing out, ");
                  sb1.Append(cm.Name);
                  sb1.Append(" suffered ");
                  sb1.Append(myGridRowWounds[i].myWoundResult);
                  if( false == myGridRowWounds[i].myWoundEffect.Contains("None"))
                  {
                     sb1.Append(" - ");
                     sb1.Append(myGridRowWounds[i].myWoundEffect);
                  }
                  sb1.Append(".");
                  lastReport.Notes.Add(sb1.ToString());
               }
               //--------------------------------------
               myState = E0481Enum.WOUNDS_ROLL_SHOW;
               for (int k = 0; k < myMaxRowCountWound; k++)
               {
                  if (Utilities.NO_RESULT == myGridRowWounds[k].myDieRollWound)
                     myState = E0481Enum.WOUNDS_ROLL;
               }
               break;
            case E0481Enum.BAILOUT_ROLL:
               myGridRowWounds[i].myDieRollBailout = dieRoll;
               dieRoll += myGridRowWounds[i].myBailOutModifier;
               if (dieRoll < 11)
               {
                  myGridRowWounds[i].myBailOutResult = "Crewman out";
               }
               else
               {
                  myGridRowWounds[i].myBailOutResult = "Unable to bail";
                  myGridRowRescues[myMaxRowCountRescue] = new GridRowRescue(myGridRowWounds[i].myCrewMember);
                  myMaxRowCountRescue++;
               }
               //--------------------------------------
               myState = E0481Enum.BAILOUT_ROLL_SHOW;
               for (int k = 0; k < myMaxRowCountWound; k++)
               {
                  if (Utilities.NO_RESULT == myGridRowWounds[k].myDieRollBailout)
                     myState = E0481Enum.BAILOUT_ROLL;
               }
               break;
            case E0481Enum.BAILOUT_WOUNDS_ROLL:
               ICrewMember cm1 = myGridRowWounds[i].myCrewMember;
               myGridRowWounds[i].myDieRollBailoutWound = dieRoll;
               myGridRowWounds[i].myBailoutWoundResult = TableMgr.SetWounds(myGameInstance, cm1, dieRoll, myGridRowWounds[i].myBailoutWoundModifier);
               myGridRowWounds[i].myBailoutWoundEffect = TableMgr.GetWoundEffect(myGameInstance, cm1, dieRoll, myGridRowWounds[i].myBailoutWoundModifier);
               if (false == myGridRowWounds[i].myBailoutWoundResult.Contains("Near Miss"))
               {
                  StringBuilder sb1 = new StringBuilder("At ");
                  sb1.Append(TableMgr.GetTime(lastReport));
                  sb1.Append(", ");
                  sb1.Append(cm1.Name);
                  sb1.Append(" suffered ");
                  sb1.Append(myGridRowWounds[i].myBailoutWoundResult);
                  if (false == myGridRowWounds[i].myBailoutWoundEffect.Contains("None"))
                  {
                     sb1.Append(" - ");
                     sb1.Append(myGridRowWounds[i].myBailoutWoundEffect);
                  }
                  sb1.Append(".");
                  lastReport.Notes.Add(sb1.ToString());
               }
               //--------------------------------------
               myState = E0481Enum.BAILOUT_WOUNDS_ROLL_SHOW;
               for (int k = 0; k < myMaxRowCountWound; k++)
               {
                  if (Utilities.NO_RESULT == myGridRowWounds[k].myDieRollBailoutWound)
                     myState = E0481Enum.BAILOUT_WOUNDS_ROLL;
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
         Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "EventViewerTankDestroyed.ShowDieResults(): ---------------myState=" + myState.ToString());
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
         if (null == mySelectedCrewman)
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): myGameInstance.GetCrewMember() returned null for cm=" + b.Name);
            return;
         }
         myAssignables.Remove(b.Name);
         //---------------------------------------
         myState = E0481Enum.BAILOUT_RESCUE_ROLL;
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): UpdateGrid() return false");
            return;
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
                        if (("Explodes" == img.Name) || ("End" == img.Name) )
                        {
                           Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "Grid_MouseDown(): myState=" + myState.ToString() + "-->END" );
                           myState = E0481Enum.END;
                        }
                        if ("Wounds" == img.Name)
                        {
                           Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "Grid_MouseDown(): myState=" + myState.ToString() + "-->WOUNDS_ROLL");
                           myState = E0481Enum.WOUNDS_ROLL;
                           myTextBlockHeader.Text = "r4.81.4b Crew Casualties";
                           myButtonRule.Content = "r19.12";
                           myTextBlock0.Text = "Crewman";
                           myTextBlock1.Text = "Modifier";
                           myTextBlock2.Text = "Roll";
                           myTextBlock3.Text = "Roll + Modifier";
                           myTextBlock4.Text = "Wound Result";
                        }
                        else if ("BailOut" == img.Name)
                        {
                           Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "Grid_MouseDown(): myState=" + myState.ToString() + "-->BAILOUT_ROLL");
                           myState = E0481Enum.BAILOUT_ROLL;
                           myTextBlockHeader.Text = "r4.81.4c Crew Escape";
                           myButtonRule.Content = "r19.13";
                           myTextBlock1.Text = "Bailout Effect";
                           myTextBlock2.Text = "Modifier";
                           myTextBlock3.Text = "Roll + Modifier";
                           myTextBlock4.Text = "Bailout Result";
                        }
                        else if ("BailoutWound" == img.Name)
                        {
                           Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "Grid_MouseDown(): myState=" + myState.ToString() + "-->BAILOUT_ROLL");
                           myState = E0481Enum.BAILOUT_WOUNDS_ROLL;
                           myTextBlockHeader.Text = "r4.81.4d Bailout Wound";
                           myButtonRule.Content = "r19.13";
                           myTextBlock1.Text = "Modifier";
                           myTextBlock2.Text = "Roll + Modifier";
                           myTextBlock3.Text = "Would Result";
                           myTextBlock4.Text = "Wound Effect";
                        }
                        else if ("Rescue" == img.Name)
                        {
                           if (0 == myMaxRowCountRescue)
                           {
                              Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "Grid_MouseDown(): myState=" + myState.ToString() + "-->BAILOUT_ROLL");
                              myState = E0481Enum.BAILOUT_RESCUE_SELECT;
                              string[] crewmembers = new string[5] { "Commander", "Gunner", "Loader", "Driver", "Assistant" };
                              foreach (string crewmember in crewmembers)
                              {
                                 ICrewMember? cm = myGameInstance.GetCrewMember(crewmember);
                                 if (null == cm)
                                 {
                                    Logger.Log(LogEnum.LE_ERROR, "ResolveTankDestroyed(): cm=null for name=" + crewmember);
                                    return;
                                 }
                                 if ((false == cm.IsIncapacitated) && (false == cm.IsUnconscious) && (false == cm.IsKilled))
                                 {
                                    bool isNeedRescuing = false;
                                    for (int i = 0; i < myMaxRowCountRescue; ++i)
                                    {
                                       if (crewmember == myGridRowRescues[i].myCrewMember.Role)
                                       {
                                          isNeedRescuing = true;
                                          break;
                                       }
                                    }
                                    if (true == isNeedRescuing)
                                       break;
                                    else
                                       myAssignables.Add(cm);
                                 }
                                 if( 0 == myAssignables.Count )
                                 {
                                    Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "Grid_MouseDown(): myState=" + myState.ToString() + "-->END");
                                    myState = E0481Enum.END;
                                 }
                              }
                           }
                           else
                           {
                              Logger.Log(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED, "Grid_MouseDown(): myState=" + myState.ToString() + "-->END");
                              myState = E0481Enum.END;
                           }
                           myTextBlockHeader.Text = "r4.81.4d Rescue";
                           myButtonRule.Content = "r19.14";
                           myTextBlock1.Text = "Rescuer";
                           myTextBlock2.Text = "Modifier";
                           myTextBlock3.Text = "Roll + Modifier";
                           myTextBlock4.Text = "Wound Result";
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
