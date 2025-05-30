using System;
using System.Collections.Generic;
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
      public enum E0481Enum
      {
         TANK_EXPLOSION_ROLL,
         TANK_EXPLOSION_ROLL_SHOW,
         WOUNDS_ROLL,
         WOUNDS_ROLL_SHOW,
         END
      };
      public bool CtorError { get; } = false;
      private EndResolveTankDestroyedCallback? myCallback = null;
      private E0481Enum myState = E0481Enum.TANK_EXPLOSION_ROLL;
      private int myMaxRowCount = 0;
      private int myRollResultRowNum = 0;
      private int myRollResultColNum = 0;
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
         public string myBailOutEffect = "Uninit";
         public int myDieRollBailout = Utilities.NO_RESULT;
         public string myBailOutResult = "Uninit";
         //---------------------------------------------------
         public GridRowWound(ICrewMember cm)
         {
            myCrewMember = cm;
         }
      };
      private GridRowWound[] myGridRowWounds = new GridRowWound[11];
      //============================================================
      private IGameEngine? myGameEngine;
      private IGameInstance? myGameInstance;
      private readonly Canvas? myCanvas;
      private readonly ScrollViewer? myScrollViewer;
      private RuleDialogViewer? myRulesMgr;
      private IDieRoller? myDieRoller;
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
         //--------------------------------------------------
         myMaxRowCount = 5;
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
            if (true == cm.IsKilled)
               myGridRowWounds[i].myDieRollWound = KIA_CREWMAN;
            ++i;
         }
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
               if ("Explodes" == myGridRowExplodes[0].myExplosionResult)
               {
                  Image img0 = new Image { Name = "Explodes", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img0);
               }
               else
               {
                  Image img1 = new Image { Name = "Wounds", Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img1);
               }
               Rectangle r11 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r11);
               break;
            case E0481Enum.WOUNDS_ROLL:
               Rectangle r12 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r12);
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
               if (false == UpdateGridRowWounds())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowExplodes(): UpdateGridRowWounds() returned false");
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
            int combo = myGridRowExplodes[0].myDieRollExplosion + myGridRowExplodes[0].myDieRollExplosion;
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
         int rowNum = STARTING_ASSIGNED_ROW;
         //----------------------------
         for( int i = 0; i<myMaxRowCount; ++i)
         {
            IMapItem cm = myGridRowWounds[i].myCrewMember;
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
               int combo = myGridRowWounds[i].myWoundModifier + myGridRowWounds[i].myDieRollWound;
               Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = combo.ToString() };
               myGrid.Children.Add(label2);
               Grid.SetRow(label2, rowNum);
               Grid.SetColumn(label2, 2);
               Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myWoundResult };
               myGrid.Children.Add(label3);
               Grid.SetRow(label3, rowNum);
               Grid.SetColumn(label3, 3);
               if (Utilities.NO_RESULT == myGridRowWounds[i].myDieRollBailout)
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
                  Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myDieRollBailout.ToString() };
                  myGrid.Children.Add(label4);
                  Grid.SetRow(label4, rowNum);
                  Grid.SetColumn(label4, 4);
                  Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRowWounds[i].myDieRollBailout.ToString() };
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
         Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "EventViewerTankDestroyed.ShowDieResults(): ++++++++++++++myState=" + myState.ToString());
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
               myGridRowExplodes[0].myDieRollExplosion = dieRoll;
               int rollPlusModifier = dieRoll + myGridRowExplodes[0].myDieRollExplosion;
               if( 99 < rollPlusModifier )
               {
                  myGridRowExplodes[0].myExplosionResult = "Explodes";
                  string[] crewmembers = new string[5] { "Driver", "Assistant", "Commander", "Loader", "Gunner" };
                  foreach (string crewmember in crewmembers)
                  {
                     ICrewMember? cm = myGameInstance.GetCrewMember(crewmember);
                     if (null == cm)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myGameInstance.GetCrewMember() returned null for " + crewmember);
                        return;
                     }
                     cm.IsKilled = true;
                     cm.SetBloodSpots();
                  }
                  GameAction outAction = GameAction.UpdateTankExplosion;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
               else
               {
                  myGameInstance.IsBailOut = true;
                  myGridRowExplodes[0].myExplosionResult = "Penetration";
               }
               myState = E0481Enum.TANK_EXPLOSION_ROLL_SHOW;
               break;
            case E0481Enum.WOUNDS_ROLL:
               if(2 == myRollResultColNum)
               {
                  myGridRowWounds[i].myDieRollWound = dieRoll;
                  ICrewMember cm = myGridRowWounds[i].myCrewMember;
                  myGridRowWounds[i].myWoundResult = TableMgr.SetWounds(myGameInstance, cm, dieRoll);
                  if ("ERROR" == myGridRowWounds[i].myWoundResult)
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
                  sb1.Append(myGridRowWounds[i].myWoundResult);
                  lastReport.Notes.Add(sb1.ToString());
               }
               else if (5 == myRollResultColNum)
               {
                  myGridRowWounds[i].myDieRollBailout = dieRoll;
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myRollResultColNum=" + myRollResultColNum.ToString());
                  return;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default myState=" + myState.ToString());
               return;
         }
         myState = E0481Enum.WOUNDS_ROLL_SHOW;
         for ( int k=0; k<myMaxRowCount; k++)
         {
            if (Utilities.NO_RESULT == myGridRowWounds[k].myDieRollWound)
               myState = E0481Enum.WOUNDS_ROLL;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
         //-------------------------------
         Logger.Log(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION, "EventViewerTankDestroyed.ShowDieResults(): ---------------myState=" + myState.ToString());
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
                        if ("Wounds" == img.Name)
                        {
                           myState = E0481Enum.WOUNDS_ROLL;
                           myTextBlock2.Text = "Roll";
                           myTextBlock3.Text = "Roll + Modifier";
                           myTextBlock4.Text = "Wound Result";
                           myTextBlock5.Text = "Bail Out";
                           myTextBlock5.Visibility = Visibility.Visible;
                        }
                        else if ("Explodes" == img.Name)
                        {
                           myState = E0481Enum.TANK_EXPLOSION_ROLL_SHOW;
                        }
                        else if ("Continue" == img.Name)
                           myState = E0481Enum.END;
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
