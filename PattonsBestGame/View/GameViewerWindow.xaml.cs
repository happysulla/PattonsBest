using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using Button = System.Windows.Controls.Button;
using MenuItem = System.Windows.Controls.MenuItem;
using Point = System.Windows.Point;
using Pattons_Best.Properties;
using System.Diagnostics;
using System.Windows.Media.Media3D;

namespace Pattons_Best
{
   public partial class GameViewerWindow : Window, IView
   {
      private const int MAX_DAILY_ACTIONS = 16;
      private const Double MARQUEE_SCROLL_ANMINATION_TIME = 30.0;
      private const Double ELLIPSE_DIAMETER = 40.0;
      private const Double ELLIPSE_RADIUS = ELLIPSE_DIAMETER / 2.0;
      private Double theOldXAfterAnimation = 0.0;
      private Double theOldYAfterAnimation = 0.0;
      public bool CtorError { get; } = false;
      //---------------------------------------------------------------------
      [Serializable]
      [StructLayout(LayoutKind.Sequential)] 
      public struct POINT  // used in WindowPlacement structure
      {
         public int X;
         public int Y;
         public POINT(int x, int y)
         {
            X = x;
            Y = y;
         }
      }
      //-------------------------------------------
      [Serializable]
      [StructLayout(LayoutKind.Sequential)]
      public struct RECT // used in WindowPlacement structure
      {
         public int Left;
         public int Top;
         public int Right;
         public int Bottom;
         public RECT(int left, int top, int right, int bottom)
         {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
         }
      }
      //-------------------------------------------
      [Serializable]
      [StructLayout(LayoutKind.Sequential)]
      public struct WindowPlacement // used to save window position between sessions
      {
         public int length;
         public int flags;
         public int showCmd;
         public POINT minPosition;
         public POINT maxPosition;
         public RECT normalPosition;
         public bool IsZero()
         {
            if (0 != length)
               return false;
            if (0 != flags)
               return false;
            if (0 != minPosition.X)
               return false;
            if (0 != minPosition.Y)
               return false;
            if (0 != maxPosition.X)
               return false;
            if (0 != maxPosition.Y)
               return false;
            return true;
         }
      }
      //---------------------------------------------------------------------
      private readonly IGameEngine myGameEngine;
      private IGameInstance myGameInstance;
      //---------------------------------------------------------------------
      private IDieRoller? myDieRoller = null;
      private EventViewer? myEventViewer = null;
      private MainMenuViewer? myMainMenuViewer = null;
      private System.Windows.Input.Cursor? myTargetCursor = null;
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private double myPreviousScrollHeight = 0.0;
      private double myPreviousScrollWidth = 0.0;
      //---------------------------------------------------------------------
      private readonly SolidColorBrush mySolidColorBrushClear = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushGray = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushGreen = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushRed = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushPurple = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushRosyBrown = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushOrange = new SolidColorBrush();
      //---------------------------------------------------------------------
      private readonly List<Button> myMoveButtons = new List<Button>();
      private readonly List<Button> myBattleButtons = new List<Button>();
      private readonly List<Button> myTankButtons = new List<Button>();
      private readonly SplashDialog mySplashScreen;
      private Dictionary<string, ContextMenu> myContextMenuCrewActions = new Dictionary<string, ContextMenu>();
      private Dictionary<string, ContextMenu> myContextMenuGunLoadActions = new Dictionary<string, ContextMenu>();
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private int myBrushIndex = 0;
      private readonly List<Brush> myBrushes = new List<Brush>();
      private readonly List<Rectangle> myRectangles = new List<Rectangle>();
      private readonly List<Polygon> myPolygons = new List<Polygon>();
      private readonly List<Ellipse> myEllipses = new List<Ellipse>();
      private Rectangle? myRectangleMoving = null;               // Not used - Rectangle that is moving with button
      private ITerritory? myTerritorySelected = null;
      private Storyboard myStoryboard = new Storyboard();    // Show Statistics Marquee at end of game 
      private TextBlock myTextBoxMarquee; // Displayed at end to show Statistics of games
      private Double mySpeedRatioMarquee = 1.0;
      //-----------------------CONSTRUCTOR--------------------
      public GameViewerWindow(IGameEngine ge, IGameInstance gi)
      {
         NameScope.SetNameScope(this, new NameScope()); // TextBox Marquee is end game condtion - display Game Statistics
         myTextBoxMarquee = new TextBlock() { Foreground = Brushes.Red, FontFamily = myFontFam, FontSize = 24 };
         myTextBoxMarquee.MouseLeftButtonDown += MouseLeftButtonDownMarquee;
         myTextBoxMarquee.MouseLeftButtonUp += MouseLeftButtonUpMarquee;
         myTextBoxMarquee.MouseRightButtonDown += MouseRightButtonDownMarquee;
         this.RegisterName("tbMarquee", myTextBoxMarquee);
         //---------------------------------------------------------------
         mySplashScreen = new SplashDialog(); // show splash screen waiting for finish initializing
         mySplashScreen.Show();
         InitializeComponent();
         //---------------------------------------------------------------
         Image imageTank = new Image() { Name = "TankMat", Width = 600, Height = 500, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("m001M4") };
         myCanvasTank.Children.Add(imageTank);
         Canvas.SetLeft(imageTank, 0);
         Canvas.SetTop(imageTank, 0);
         //---------------------------------------------------------------
         myGameEngine = ge;
         myGameInstance = gi;
         gi.GamePhase = GamePhase.GameSetup;
         myMainMenuViewer = new MainMenuViewer(myMainMenu, ge, gi);
         //---------------------------------------------------------------
         ICombatCalendarEntry? entry = TableMgr.theCombatCalendarEntries[0];
         if (null == entry)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): entry=null");
            CtorError = true;
            return;
         }
         IAfterActionReport report1 = new AfterActionReport(entry);
         gi.Reports.Add(report1);
         //---------------------------------------------------------------
         if (false == AddHotKeys(myMainMenuViewer))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): AddHotKeys() returned false");
            CtorError = true;
            return;
         }
         //---------------------------------------------------------------
         Options options = Deserialize(Settings.Default.GameOptions);
         myMainMenuViewer.NewGameOptions = options;
         gi.Options = options; // use the new game options for setting up the first game
         //---------------------------------------------------------------
         if (false == String.IsNullOrEmpty(Settings.Default.GameDirectoryName))
            GameLoadMgr.theGamesDirectory = Settings.Default.GameDirectoryName; // remember the game directory name
         //---------------------------------------------------------------
         Utilities.ZoomCanvas = Settings.Default.ZoomCanvas;
         myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas); // Constructor - revert to save zoom
         StatusBarViewer sbv = new StatusBarViewer(myStatusBar, ge, gi, myCanvasMain);
         //---------------------------------------------------------------
         Utilities.theBrushBlood.Color = Color.FromArgb(0xFF, 0xA4, 0x07, 0x07);
         Utilities.theBrushRegion.Color = Color.FromArgb(0x7F, 0x11, 0x09, 0xBB); // nearly transparent but slightly colored
         Utilities.theBrushRegionClear.Color = Color.FromArgb(0, 0, 0x01, 0x0); // nearly transparent but slightly colored
         Utilities.theBrushControlButton.Color = Color.FromArgb(0xFF, 0x43, 0x33, 0xFF); // menu blue
         Utilities.theBrushScrollViewerActive.Color = Color.FromArgb(0xFF, 0xB9, 0xEA, 0x9E); // light green 
         //Utilities.theBrushScrollViewerInActive.Color = Color.FromArgb(0x17, 0x00, 0x00, 0x00); // gray
         Utilities.theBrushScrollViewerInActive.Color = Colors.LightGray;
         //---------------------------------------------------------------                                                                         
         mySolidColorBrushClear.Color = Color.FromArgb(0, 0, 1, 0); // Create standard color brushes
         mySolidColorBrushBlack.Color = Colors.Black;
         mySolidColorBrushGray.Color = Colors.Ivory;
         mySolidColorBrushGreen.Color = Colors.Green;
         mySolidColorBrushRed.Color = Colors.Red;
         mySolidColorBrushOrange.Color = Colors.Orange;
         mySolidColorBrushPurple.Color = Colors.Purple;
         mySolidColorBrushRosyBrown.Color = Colors.RosyBrown;
         //---------------------------------------------------------------
         // Create a container of brushes for painting paths.
         // The first brush is the alien color.
         // The second brush is the townspeople color.
         myBrushes.Add(Brushes.Green);
         myBrushes.Add(Brushes.Blue);
         myBrushes.Add(Brushes.Purple);
         myBrushes.Add(Brushes.Yellow);
         myBrushes.Add(Brushes.Red);
         myBrushes.Add(Brushes.Orange);
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines
         //---------------------------------------------------------------
         myContextMenuCrewActions["Commander"] = new ContextMenu();
         myContextMenuCrewActions["Gunner"] = new ContextMenu();
         myContextMenuCrewActions["Loader"] = new ContextMenu();
         myContextMenuCrewActions["Driver"] = new ContextMenu();
         myContextMenuCrewActions["Assistant"] = new ContextMenu();
         myContextMenuGunLoadActions["GunLoadHe"] = new ContextMenu();
         myContextMenuGunLoadActions["GunLoadAp"] = new ContextMenu();
         myContextMenuGunLoadActions["GunLoadHbci"] = new ContextMenu();
         myContextMenuGunLoadActions["GunLoadWp"] = new ContextMenu();
         myContextMenuGunLoadActions["GunLoadHvap"] = new ContextMenu();
         //---------------------------------------------------------------
         myDieRoller = new DieRoller(myCanvasMain, CloseSplashScreen); // Close the splash screen when die resources are loaded
         if (true == myDieRoller.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): myDieRoller.CtorError=true");
            CtorError = true;
            return;
         }
         //---------------------------------------------------------------
         myEventViewer = new EventViewer(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, Territories.theTerritories, myDieRoller);
         if (true == myEventViewer.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): myEventViewer.CtorError=true");
            CtorError = true;
            return;
         }
         CanvasImageViewer civ = new CanvasImageViewer(myCanvasMain, myDieRoller);
         if (true == civ.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): civ.CtorError=true");
            CtorError = true;
            return;
         }
         //---------------------------------------------------------------
         // Implement the Model View Controller (MVC) pattern by registering views with
         // the game engine such that when the model data is changed, the views are updated.
         ge.RegisterForUpdates(civ);
         ge.RegisterForUpdates(myMainMenuViewer);
         ge.RegisterForUpdates(myEventViewer);
         ge.RegisterForUpdates(sbv);
         ge.RegisterForUpdates(this); // needs to be last so that canvas updates after all actions taken
         Logger.Log(LogEnum.LE_GAME_INIT, "GameViewerWindow(): \nzoomCanvas=" + Settings.Default.ZoomCanvas.ToString() + "\nwp=" + Settings.Default.WindowPlacement + "\noptions=" + Settings.Default.GameOptions);
#if UT1
            if (false == ge.CreateUnitTests(gi, myDockPanelTop, myEventViewer, myDieRoller, civ))
            {
               Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): CreateUnitTests() returned false");
               CtorError = true;
               return;
            }
            gi.GamePhase = GamePhase.UnitTest;
#endif
      }
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         //-------------------------------------------------------
         if ((null != myTargetCursor) && (GameAction.UpdateStatusBar == action)) // increase/decrease size of cursor when zoom in or out
         {
            myTargetCursor.Dispose();
            double sizeCursor = Utilities.ZoomCanvas * Utilities.ZOOM * Utilities.theMapItemSize;
            System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, sizeCursor * 0.5); // set the center of the MapItem as the hot point for the cursor
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Target"), Width = sizeCursor, Height = sizeCursor };
            myTargetCursor = Utilities.ConvertToCursor(img1, hotPoint);
            this.myCanvasMain.Cursor = myTargetCursor;
         }
         //-------------------------------------------------------
         else if ((GameAction.UpdateLoadingGame == action) || (GameAction.UpdateNewGame == action))
         {
            myGameInstance = gi;
            myMoveButtons.Clear();
            myBattleButtons.Clear();
            myTankButtons.Clear();
            myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas); // UploadNewGame - Return to previous saved zoom level
            this.Title = UpdateViewTitle(gi.Options);
         }
         switch (action)
         {
            case GameAction.UnitTestStart:
            case GameAction.UnitTestCommand:
            case GameAction.UnitTestNext:
            case GameAction.UnitTestCleanup:
               break;
            case GameAction.ShowCombatCalendarDialog:
            case GameAction.ShowAfterActionReportDialog:
            case GameAction.ShowInventoryDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
               break;
            case GameAction.SetupShowMapHistorical:
            case GameAction.SetupShowMovementBoard:
            case GameAction.SetupShowBattleBoard:
            case GameAction.SetupShowTankCard:
            case GameAction.SetupShowAfterActionReport:
            case GameAction.SetupShowCombatCalendarCheck:
            case GameAction.SetupCombatCalendarRoll:
               break;
            case GameAction.MorningBriefingBegin:
            case GameAction.MorningBriefingCalendarRoll:
            case GameAction.MorningBriefingEnd:
               break;
            case GameAction.BattleRandomEventRoll:
               break;
            case GameAction.TestingStartMorningBriefing:
            case GameAction.TestingStartPreparations:
            case GameAction.TestingStartMovement:
            case GameAction.TestingStartBattle:
            case GameAction.TestingStartAmbush:
            case GameAction.MorningBriefingAmmoReadyRackLoad:
            case GameAction.PreparationsHatches:
            case GameAction.PreparationsGunLoad:
            case GameAction.PreparationsGunLoadSelect:
            case GameAction.BattleRoundSequenceStart:
            case GameAction.UpdateTankCard:
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               break;
            case GameAction.BattleRoundSequenceSpotting:
               if (false == UpdateCanvasMain(gi, action)) // update smoke depletion
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.BattleRoundSequenceCrewOrders:
               if (false == CreateContextMenuCrewAction(myGameInstance))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): CreateContextMenuCrewAction() returned false");
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               if (false == UpdateCanvasMain(gi, action)) // update smoke depletion
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.BattleRoundSequenceAmmoOrders:
               if (false == CreateContextMenuGunLoadAction(myGameInstance))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): CreateContextMenuGunLoadAction() returned false");
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               if (false == UpdateCanvasAnimateBattlePhase(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasAnimateBattlePhase() returned error ");
               break;
            case GameAction.PreparationsTurret:
            case GameAction.BattleRoundSequenceTurretEnd:
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.PreparationsTurretRotateLeft:
            case GameAction.PreparationsTurretRotateRight:
            case GameAction.BattleRoundSequenceTurretEndRotateLeft:
            case GameAction.BattleRoundSequenceTurretEndRotateRight:
            case GameAction.BattleRoundSequenceMinefieldRoll:
               Button? b100 = this.myBattleButtons.Find(gi.Sherman.Name);
               if (null == b100)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): b100=null");
               else
                  MapItem.SetButtonContent(b100, gi.Sherman, true, true);
               break;
            case GameAction.BattleRoundSequenceConductCrewAction:
            case GameAction.BattleRoundSequencePivotLeft:
            case GameAction.BattleRoundSequencePivotRight:
               foreach (Button b in myTankButtons)
                  b.ContextMenu = null;
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.BattleRoundSequenceFriendlyAdvance:
               if (false == UpdateCanvasFriendlyAdvance(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasFriendlyAdvance() returned error ");
               break;
            case GameAction.BattleRoundSequenceEnemyAdvance:
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               if (false == UpdateCanvasEnemyAdvance(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasEnemyAdvance() returned error ");
               break;
            case GameAction.BattleRoundSequenceShermanFiringSelectTarget:
               foreach (Button b in myTankButtons)
                  b.ContextMenu = null;
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               if ( false == UpdateCanvasShermanSelectTarget(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasShermanSelectTarget() returned error ");
               break;
            case GameAction.BattleRoundSequenceShermanFiringSelectTargetMg:
               foreach (Button b in myTankButtons)
                  b.ContextMenu = null;
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               if (false == UpdateCanvasShermanSelectTargetMg(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasShermanSelectTargetMg() returned error ");
               break;
            case GameAction.BattleRoundSequenceEnemyAction:
            case GameAction.BattleRoundSequenceShermanToHitRoll:
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.BattleRoundSequenceLoadMainGun:
            case GameAction.BattleRoundSequenceLoadMainGunEnd:
            case GameAction.BattleRoundSequenceBackToSpotting:
               if (false == UpdateCanvasAnimateBattlePhase(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasAnimateBattlePhase() returned error ");
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               break;
            case GameAction.EveningDebriefingStart:
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               break;
            case GameAction.EndGameWin:
            case GameAction.EndGameLost:
               SaveDefaultsToSettings();
               break;
            case GameAction.RemoveSplashScreen:
               this.Title = UpdateViewTitle(gi.Options);
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               mySplashScreen.Close();
               myScrollViewerMap.UpdateLayout();
               break;
            case GameAction.UpdateGameOptions:
               this.Title = UpdateViewTitle(gi.Options);
               SaveDefaultsToSettings();
               break;
            case GameAction.EndGameFinal:
               myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);  // EndGameFinal - show map for last time
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.SetupChooseFunOptions:
            case GameAction.SetupFinalize:
               this.Title = UpdateViewTitle(gi.Options);
               myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.EveningDebriefingRatingImprovement:
               UpdateCanvasMainClear(myBattleButtons, gi.BattleStacks);
               foreach (Button b in myBattleButtons)
                  myCanvasMain.Children.Remove(b);
               myBattleButtons.Clear();
               foreach (IStack stack in gi.BattleStacks)
                  stack.MapItems.Clear();
               break;
            default:
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
         }
         //UpdateScrollbarThumbnails(gi.Prince.Territory);
      }
      private string UpdateViewTitle(Options options)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Patton's Best - ");
         //--------------------------------
         string name = "CustomGame";
         Option? option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
         {
            sb.Append("Custom Game");
         }
         else
         {
            name = "MaxFunGame";
            option = options.Find(name);
            if (null == option)
               option = new Option(name, false);
            if (true == option.IsEnabled)
            {
               sb.Append("Fun Game");
            }
            else
            {
               name = "RandomGame";
               option = options.Find(name);
               if (null == option)
                  option = new Option(name, false);
               if (true == option.IsEnabled)
               {
                  sb.Append(" All Random Options Game - ");
               }
               else
               {
                  name = "RandomHexGame";
                  option = options.Find(name);
                  if (null == option)
                     option = new Option(name, false);
                  if (true == option.IsEnabled)
                  {
                     sb.Append("Random Starting Hex Game - ");
                  }
                  else
                  {
                     name = "RandomPartyGame";
                     option = options.Find(name);
                     if (null == option)
                        option = new Option(name, false);
                     if (true == option.IsEnabled)
                        sb.Append("Random Starting Party Game");
                     else
                        sb.Append("Orginal Game");
                  }
               }
            }
         }
         //----------------------------------
         name = "EasiestMonsters";
         option = options.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
         {
            sb.Append(" - Easiest");
         }
         else
         {
            name = "EasyMonsters";
            option = options.Find(name);
            if (null == option)
               option = new Option(name, false);
            if (true == option.IsEnabled)
            {
               sb.Append("");
            }
            else
            {
               name = "LessHardMonsters";
               option = options.Find(name);
               if (null == option)
                  option = new Option(name, false);
               if (true == option.IsEnabled)
                  sb.Append(" - Difficult Monsters");
               else
                  sb.Append(" - Brutally Difficult");
            }
         }
         return sb.ToString();
      }
      //-----------------------SUPPORTING FUNCTIONS--------------------
      private void CloseSplashScreen() // callback function that removes splash screen when dice are loaded
      {
         GameAction outAction = GameAction.RemoveSplashScreen;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private Button CreateButtonMapItem(List<Button> buttons, IMapItem mi)
      {
         Logger.Log(LogEnum.LE_SHOW_ORDERS_MENU, "CreateButtonMapItem(): creating new button=" + mi.Name);
         System.Windows.Controls.Button b = new Button { Name = mi.Name, Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
         MapItem.SetButtonContent(b, mi, true, true); // This sets the image as the button's content
         RotateTransform rotateTransform = new RotateTransform();
         b.RenderTransformOrigin = new Point(0.5, 0.5);
         rotateTransform.Angle = mi.RotationHull + mi.RotationOffset;
         b.RenderTransform = rotateTransform;
         buttons.Add(b);
         Canvas.SetLeft(b, mi.Location.X);
         Canvas.SetTop(b, mi.Location.Y);
         b.Click += ClickButtonMapItem;
         b.MouseEnter += MouseEnterMapItem;
         b.MouseLeave += MouseLeaveMapItem;
         return b;
      }
      private bool CreateContextMenuCrewAction(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateContextMenuCrewAction(): lastReport=null");
            return false;
         }
         int totalAmmo = lastReport.MainGunHE + lastReport.MainGunAP + lastReport.MainGunWP + lastReport.MainGunHBCI + lastReport.MainGunHVAP;
         //----------------------------------
         TankCard card = new TankCard(lastReport.TankCardNum);
         //----------------------------------
         bool isCommanderFireAaMg = false;
         bool isLoaderFireAaMg = false;
         bool isCommanderRepairAaMg = false;
         bool isLoaderRepairAaMg = false;
         bool isCommanderFireSubMg = false;
         bool isLoaderFireSubMg = false;
         bool isCommanderThrowGrenade = false;
         bool isGunnerThrowGrenade = false;
         int periscopeRepairCount = 0; 
         foreach (IMapItem mi in gi.CrewActions) // This menu is created on each crew action
         {
            if (true == mi.Name.Contains("Commander_FireAaMg"))
               isCommanderFireAaMg = true;
            if (true == mi.Name.Contains("Loader_FireAaMg"))
               isLoaderFireAaMg = true;
            if (true == mi.Name.Contains("Commander_RepairAaMg"))
               isCommanderRepairAaMg = true;
            if (true == mi.Name.Contains("Loader_RepairAaMg"))
               isLoaderRepairAaMg = true;
            if (true == mi.Name.Contains("Commander_FireSubMg"))
               isCommanderFireSubMg = true;
            if (true == mi.Name.Contains("Loader_FireSubMg"))
               isLoaderFireSubMg = true;
            if (true == mi.Name.Contains("Commander_ThrowGrenade"))
               isCommanderThrowGrenade = true;
            if (true == mi.Name.Contains("Gunner_ThrowGrenade"))
               isGunnerThrowGrenade = true;
            if (true == mi.Name.Contains("RepairScope"))
               periscopeRepairCount++;
         }
         int diffPeriscopes = lastReport.AmmoPeriscope - periscopeRepairCount; // How many periscopes can be repaired
         //---------------------------------
         bool isDriverOpenHatch = false;
         bool isLoaderOpenHatch = false;
         bool isAssistantOpenHatch = false;
         bool isGunnerOpenHatch = false;
         bool isCommanderOpenHatch = false;
         foreach (IMapItem mi in gi.Hatches) // Loader and Driver have default actions
         {
            if (true == mi.Name.Contains("Driver"))
               isDriverOpenHatch = true;
            if (true == mi.Name.Contains("Loader"))
               isLoaderOpenHatch = true;
            if (true == mi.Name.Contains("Assistant"))
               isAssistantOpenHatch = true;
            if (true == mi.Name.Contains("Gunner"))
               isGunnerOpenHatch = true;
            if (true == mi.Name.Contains("Commander"))
               isCommanderOpenHatch = true;
         }
         bool isMainGunFiringAvailable = ((false == gi.IsMalfunctionedMainGun) && (false == gi.IsBrokenMainGun) && (false == gi.IsBrokenGunsight) && (0 < totalAmmo) && ("None" != gi.GetGunLoadType()));
         bool isShermanMoveAvailable = ((false == gi.Sherman.IsThrownTrack) && (false == gi.Sherman.IsAssistanceNeeded) && (false == gi.IsBrokenPeriscopeDriver) || (true == isDriverOpenHatch));
         //---------------------------------
         myContextMenuCrewActions["Loader"].Items.Clear();
         myContextMenuCrewActions["Loader"].Visibility = Visibility.Visible;
         MenuItem menuItem1 = new MenuItem();
         menuItem1.Name = "Loader_Load";
         menuItem1.Header = "Load";
         menuItem1.Click += MenuItemCrewActionClick;
         myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
         if (true == gi.IsMalfunctionedMainGun) 
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Loader_RepairMainGun";
            menuItem1.Header = "Repair Main Gun";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
         }
         if (true == gi.IsMalfunctionedMgCoaxial)
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Loader_RepairCoaxialMg";
            menuItem1.Header = "Repair Coaxial MG";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
         }
         if ((0 < lastReport.AmmoSmokeBomb) && (true == card.myIsSmokeMortar) )
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Loader_FireMortar";
            menuItem1.Header = "Fire Mortar";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
         }
         if ((0 < totalAmmo) && ( false == gi.IsBrokenMainGun) && (false == gi.IsBrokenGunsight)) 
         { 
            menuItem1 = new MenuItem();
            menuItem1.Name = "Loader_ChangeGunLoad";
            menuItem1.Header = "Change Gun Load";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
            menuItem1 = new MenuItem();
            menuItem1.Name = "Loader_RestockReadyRack";
            menuItem1.Header = "Restock Ready Rack";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
         }
         if ((true == gi.IsBrokenPeriscopeLoader) && (0 < diffPeriscopes))
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Loader_RepairScope";
            menuItem1.Header = "Replace Periscope";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
         }
         if ((true == isLoaderOpenHatch) && (0 < lastReport.Ammo50CalibreMG) && (false == isCommanderFireAaMg) && (false == gi.IsMalfunctionedMgAntiAircraft) && (false == gi.IsBrokenMgAntiAircraft))
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Loader_FireAaMg";
            menuItem1.Header = "Fire AA MG";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
         }
         if ((true == gi.IsMalfunctionedMgAntiAircraft) && (false == isCommanderRepairAaMg) && (false == gi.IsBrokenMgAntiAircraft))
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Loader_RepairAaMg";
            menuItem1.Header = "Repair AA MG";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
         }
         if ((true == isLoaderOpenHatch) && (false == isCommanderFireSubMg) && ("A" == card.myChasis) ) // Sub MG uses own ammo
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Loader_FireSubMg";
            menuItem1.Header = "Fire Sub MG";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
         }
         //===========================================================================================================
         myContextMenuCrewActions["Driver"].Items.Clear();
         myContextMenuCrewActions["Driver"].Visibility = Visibility.Visible;
         menuItem1 = new MenuItem();
         menuItem1.Name = "Driver_Stop";
         menuItem1.Header = "Stop";
         menuItem1.Click += MenuItemCrewActionClick;
         myContextMenuCrewActions["Driver"].Items.Add(menuItem1);
         if((true == isDriverOpenHatch) || (false == gi.IsBrokenPeriscopeDriver)) // If broken scope and button up, cannot drive
         {
               if (false == gi.Sherman.IsThrownTrack)
               {
                  if (false == gi.Sherman.IsBoggedDown) // bogged tanks can only attempt to free themselves by ordering reverse.
                  {
                     menuItem1 = new MenuItem();
                     menuItem1.Name = "Driver_Forward";
                     menuItem1.Header = "Forward";
                     menuItem1.Click += MenuItemCrewActionClick;
                     myContextMenuCrewActions["Driver"].Items.Add(menuItem1);
                     menuItem1 = new MenuItem();
                     menuItem1.Name = "Driver_ForwardToHullDown";
                     menuItem1.Header = "Forward To Hull Down";
                     menuItem1.Click += MenuItemCrewActionClick;
                     myContextMenuCrewActions["Driver"].Items.Add(menuItem1);
                  }
                  if (false == gi.Sherman.IsAssistanceNeeded) // if assistenance is needed, the tank is stuck and cannot free itself
                  {
                     menuItem1 = new MenuItem();
                     menuItem1.Name = "Driver_Reverse";
                     menuItem1.Header = "Reverse";
                     menuItem1.Click += MenuItemCrewActionClick;
                     myContextMenuCrewActions["Driver"].Items.Add(menuItem1);
                  }
                  if (false == gi.Sherman.IsBoggedDown)
                  {
                     menuItem1 = new MenuItem();
                     menuItem1.Name = "Driver_ReverseToHullDown";
                     menuItem1.Header = "Reverse To Hull Down";
                     menuItem1.Click += MenuItemCrewActionClick;
                     myContextMenuCrewActions["Driver"].Items.Add(menuItem1);
                     menuItem1 = new MenuItem();
                     menuItem1.Name = "Driver_PivotTank";
                     menuItem1.Header = "Pivot Tank";
                     menuItem1.Click += MenuItemCrewActionClick;
                     myContextMenuCrewActions["Driver"].Items.Add(menuItem1);
                  }
               }
            }
         if ((true == gi.IsBrokenPeriscopeGunner) && (0 < diffPeriscopes) )
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Driver_RepairScope";
            menuItem1.Header = "Replace Periscope";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Driver"].Items.Add(menuItem1);
         }
         //===========================================================================================================--
         string gunload = myGameInstance.GetGunLoadType();
         myContextMenuCrewActions["Gunner"].Items.Clear();
         myContextMenuCrewActions["Gunner"].Visibility = Visibility.Visible;
         if ( (0 < totalAmmo) && (false == gi.IsMalfunctionedMainGun) && (false == gi.IsBrokenGunsight) && (false == gi.IsBrokenMainGun) && ("None" != gunload) )
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Gunner_FireMainGun";
            menuItem1.Header = "Fire Main Gun";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Gunner"].Items.Add(menuItem1);
            menuItem1 = new MenuItem();
            menuItem1.Name = "Gunner_RotateFireMainGun";
            menuItem1.Header = "Rotate & Fire Main Gun";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Gunner"].Items.Add(menuItem1);
         }
         if ((true == isGunnerOpenHatch) || (false == gi.IsBrokenPeriscopeGunner)) // If broken scope and button up, cannot shoot coaxial
         {
            if ((0 < lastReport.Ammo30CalibreMG) && (false == gi.IsMalfunctionedMgCoaxial) && (false == gi.IsBrokenMgCoaxial))
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Gunner_FireCoaxialMg";
               menuItem1.Header = "Fire Co-Axial MG";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Gunner"].Items.Add(menuItem1);
            }
            menuItem1 = new MenuItem();
            menuItem1.Name = "Gunner_RotateTurret";
            menuItem1.Header = "Rotate Turret";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Gunner"].Items.Add(menuItem1);
         }
         if (true == gi.IsMalfunctionedMainGun)
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Gunner_RepairMainGun";
            menuItem1.Header = "Repair Main Gun";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Gunner"].Items.Add(menuItem1);
         }
         if ((true == isCommanderOpenHatch) && (0 == lastReport.AmmoSmokeGrenade) && (false == isCommanderThrowGrenade) ) // Gunner must throw grenade out commander hatch
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Gunner_ThrowGrenade";
            menuItem1.Header = "Throw Smoke Grenade";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Gunner"].Items.Add(menuItem1);
         }
         if ((true == gi.IsBrokenPeriscopeGunner) && (0 < diffPeriscopes))
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Gunner_RepairScope";
            menuItem1.Header = "Replace Periscope";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Gunner"].Items.Add(menuItem1);
         }
         //===========================================================================================================
         myContextMenuCrewActions["Assistant"].Items.Clear();
         myContextMenuCrewActions["Assistant"].Visibility = Visibility.Visible;
         menuItem1 = new MenuItem();
         menuItem1.Name = "Assistant_PassAmmo";
         menuItem1.Header = "Pass Ammo";
         menuItem1.Click += MenuItemCrewActionClick;
         myContextMenuCrewActions["Assistant"].Items.Add(menuItem1);
         if ((true == isAssistantOpenHatch) || (false == gi.IsBrokenPeriscopeAssistant)) // If broken scope and button up, cannot drive
         {
            if ((0 < lastReport.Ammo30CalibreMG) && (false == gi.IsMalfunctionedMgBow) && (false == gi.IsBrokenMgBow) )
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Assistant_FireBowMg";
               menuItem1.Header = "Fire Bow MG";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Assistant"].Items.Add(menuItem1);
            }
         }
         if (true == gi.IsMalfunctionedMgBow)
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Assistant_RepairBowMg";
            menuItem1.Header = "Repair Bow MG";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Assistant"].Items.Add(menuItem1);
         }
         if ((true == gi.IsBrokenPeriscopeAssistant) && (0 < diffPeriscopes))
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Assistant_RepairScope";
            menuItem1.Header = "Replace Periscope";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Assistant"].Items.Add(menuItem1);
         }
         //===========================================================================================================
         myContextMenuCrewActions["Commander"].Items.Clear();
         myContextMenuCrewActions["Commander"].Visibility = Visibility.Visible;
         bool is30CalibreMGFirePossible = (0 < lastReport.Ammo30CalibreMG) && (((false == gi.IsBrokenMgBow) && (false == gi.IsMalfunctionedMgBow)) || ((false == gi.IsBrokenMgCoaxial) && (false == gi.IsMalfunctionedMgCoaxial))); // bow and coaxial MGs
         bool is50CalibreMGFirePossible = (0 < lastReport.Ammo50CalibreMG) && ((false == gi.IsBrokenMgAntiAircraft) && (false == gi.IsMalfunctionedMgAntiAircraft) && (false == isLoaderFireAaMg)); // subMG can always be fired
         if ((true == isCommanderOpenHatch) || (false == gi.IsBrokenPeriscopeCommander) || (true == card.myIsVisionCupola)) // If broken scope and button up, cannot direct
         {
            if (true == isShermanMoveAvailable) // If broken scope and button up, cannot drive
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Commander_Move";
               menuItem1.Header = "Direct Move";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Commander"].Items.Add(menuItem1);
            }
            if (true == isMainGunFiringAvailable)
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Commander_MainGunFire";
               menuItem1.Header = "Direct Main Gun Fire";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Commander"].Items.Add(menuItem1);
            }
            if ((true == is30CalibreMGFirePossible) || (true == is50CalibreMGFirePossible))
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Commander_MGFire";
               menuItem1.Header = "Direct MG Fire";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Commander"].Items.Add(menuItem1);
            }
         }
         if ((true == isCommanderOpenHatch) && (0 < lastReport.AmmoSmokeGrenade) && (false == isGunnerThrowGrenade))
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Commander_ThrowGrenade";
            menuItem1.Header = "Throw Smoke Grenade";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Commander"].Items.Add(menuItem1);
         }
         if ((true == gi.IsBrokenPeriscopeCommander) && (0 < lastReport.AmmoPeriscope))
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Commander_RepairScope";
            menuItem1.Header = "Replace Periscope";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Commander"].Items.Add(menuItem1);
         }
         if ((true == isCommanderOpenHatch) && (0 < lastReport.Ammo50CalibreMG) && (false == isLoaderFireAaMg) && (false == gi.IsBrokenMgAntiAircraft) && (false == gi.IsMalfunctionedMgAntiAircraft))
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Commander_FireAaMg";
            menuItem1.Header = "Fire AA MG";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Commander"].Items.Add(menuItem1);
         }
         if ((true == gi.IsMalfunctionedMgAntiAircraft) && (false == isLoaderRepairAaMg) && (false == gi.IsBrokenMgAntiAircraft))
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Commander_RepairAaMg";
            menuItem1.Header = "Repair AA MG";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Commander"].Items.Add(menuItem1);
         }
         if ((true == isCommanderOpenHatch) && (false == isLoaderFireSubMg) )
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Commander_FireSubMg";
            menuItem1.Header = "Fire Sub MG";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Commander"].Items.Add(menuItem1);
         }
         return true;
      }
      private bool CreateContextMenuGunLoadAction(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateContextMenuGunLoadAction(): lastReport=null");
            return false;
         }
         string gunLoadType = gi.GetGunLoadType();
         MenuItem menuitem = new MenuItem();
         //--------------------------------------------------
         myContextMenuGunLoadActions["GunLoadHe"].Items.Clear();
         int minCount = 0;
         if ("He" == gunLoadType)
            minCount = 1;
         if (minCount < lastReport.MainGunHE )
         {
            myContextMenuGunLoadActions["GunLoadHe"].IsEnabled = true;
            myContextMenuGunLoadActions["GunLoadHe"].Visibility = Visibility.Visible;
            menuitem = new MenuItem();
            menuitem.Name = "GunLoadHe_AmmoReload";
            menuitem.Header = "Place Ammo Reload";
            menuitem.Click += MenuItemAmmoReloadClick;
            myContextMenuGunLoadActions["GunLoadHe"].Items.Add(menuitem);
            if (0 < gi.GetReadyRackReload("He"))
            {
               menuitem = new MenuItem();
               menuitem.Name = "GunLoadHe_ReadyRackAmmoReload";
               menuitem.Header = "Place Ammo & Ready Rack Reload";
               menuitem.Click += MenuItemAmmoReloadClick;
               myContextMenuGunLoadActions["GunLoadHe"].Items.Add(menuitem);
            }
         }
         else
         {
            myContextMenuGunLoadActions["GunLoadHe"].IsEnabled = false;
            myContextMenuGunLoadActions["GunLoadHe"].Visibility = Visibility.Collapsed;
         }
         //--------------------------------------------------
         myContextMenuGunLoadActions["GunLoadAp"].Items.Clear();
         minCount = 0;
         if ("Ap" == gunLoadType) // if the one ammo is loaded in gun, cannot perform ammo reload
            minCount = 1;
         if (minCount < lastReport.MainGunAP)
         {
            myContextMenuGunLoadActions["GunLoadAp"].IsEnabled = true;
            myContextMenuGunLoadActions["GunLoadAp"].Visibility = Visibility.Visible;
            menuitem = new MenuItem();
            menuitem.Name = "GunLoadAp_AmmoReload";
            menuitem.Header = "Place Ammo Reload";
            menuitem.Click += MenuItemAmmoReloadClick;
            myContextMenuGunLoadActions["GunLoadAp"].Items.Add(menuitem);
            if (0 < gi.GetReadyRackReload("Ap"))
            {
               menuitem = new MenuItem();
               menuitem.Name = "GunLoadAp_ReadyRackAmmoReload";
               menuitem.Header = "Place Ammo & Ready Rack Reload";
               menuitem.Click += MenuItemAmmoReloadClick;
               myContextMenuGunLoadActions["GunLoadAp"].Items.Add(menuitem);
            }
         }
         else
         {
            myContextMenuGunLoadActions["GunLoadAp"].IsEnabled = false;
            myContextMenuGunLoadActions["GunLoadAp"].Visibility = Visibility.Collapsed;
         }
         //--------------------------------------------------
         minCount = 0;
         if ("Hbci" == gunLoadType) // if the one ammo is loaded in gun, cannot perform ammo reload
            minCount = 1;
         myContextMenuGunLoadActions["GunLoadHbci"].Items.Clear();
         if (0 < lastReport.MainGunHBCI)
         {
            myContextMenuGunLoadActions["GunLoadHbci"].IsEnabled = true;
            myContextMenuGunLoadActions["GunLoadHbci"].Visibility = Visibility.Visible;
            menuitem = new MenuItem();
            menuitem.Name = "GunLoadHbci_AmmoReload";
            menuitem.Header = "Place Ammo Reload";
            menuitem.Click += MenuItemAmmoReloadClick;
            myContextMenuGunLoadActions["GunLoadHbci"].Items.Add(menuitem);
            if (0 < gi.GetReadyRackReload("Hbci"))
            {
               menuitem = new MenuItem();
               menuitem.Name = "GunLoadHbci_ReadyRackAmmoReload";
               menuitem.Header = "Place Ammo & Ready Rack Reload";
               menuitem.Click += MenuItemAmmoReloadClick;
               myContextMenuGunLoadActions["GunLoadHbci"].Items.Add(menuitem);
            }
         }
         else
         {
            myContextMenuGunLoadActions["GunLoadHbci"].IsEnabled = false;
            myContextMenuGunLoadActions["GunLoadHbci"].Visibility = Visibility.Collapsed;
         }
         //--------------------------------------------------
         myContextMenuGunLoadActions["GunLoadWp"].Items.Clear();
         minCount = 0;
         if ("Wp" == gunLoadType) // if the one ammo is loaded in gun, cannot perform ammo reload
            minCount = 1;
         if (0 < lastReport.MainGunWP)
         {
            myContextMenuGunLoadActions["GunLoadWp"].IsEnabled = true;
            myContextMenuGunLoadActions["GunLoadWp"].Visibility = Visibility.Visible;
            menuitem = new MenuItem();
            menuitem.Name = "GunLoadWp_AmmoReload";
            menuitem.Header = "Place Ammo Reload";
            menuitem.Click += MenuItemAmmoReloadClick;
            myContextMenuGunLoadActions["GunLoadWp"].Items.Add(menuitem);
            if (0 < gi.GetReadyRackReload("Wp"))
            {
               menuitem = new MenuItem();
               menuitem.Name = "GunLoadWp_ReadyRackAmmoReload";
               menuitem.Header = "Place Ammo & Ready Rack Reload";
               menuitem.Click += MenuItemAmmoReloadClick;
               myContextMenuGunLoadActions["GunLoadWp"].Items.Add(menuitem);
            }
         }
         else
         {
            myContextMenuGunLoadActions["GunLoadWp"].IsEnabled = false;
            myContextMenuGunLoadActions["GunLoadWp"].Visibility = Visibility.Collapsed;
         }
         //--------------------------------------------------
         myContextMenuGunLoadActions["GunLoadHvap"].Items.Clear();
         if ("Hvap" == gunLoadType) // if the one ammo is loaded in gun, cannot perform ammo reload
            minCount = 1;
         if (0 < lastReport.MainGunHVAP)
         {
            myContextMenuGunLoadActions["GunLoadHvap"].IsEnabled = true;
            myContextMenuGunLoadActions["GunLoadHvap"].Visibility = Visibility.Visible;
            menuitem = new MenuItem();
            menuitem.Name = "GunLoadHvap_AmmoReload"; 
            menuitem.Header = "Place Ammo Reload";
            menuitem.Click += MenuItemAmmoReloadClick;
            myContextMenuGunLoadActions["GunLoadHvap"].Items.Add(menuitem);
            if (0 < gi.GetReadyRackReload("Hvap"))
            {
               menuitem = new MenuItem();
               menuitem.Name = "GunLoadHvap_ReadyRackAmmoReload";
               menuitem.Header = "Place Ammo & Ready Rack Reload";
               menuitem.Click += MenuItemAmmoReloadClick;
               myContextMenuGunLoadActions["GunLoadHvap"].Items.Add(menuitem);
            }
         }
         else
         {
            myContextMenuGunLoadActions["GunLoadHvap"].IsEnabled = false;
            myContextMenuGunLoadActions["GunLoadHvap"].Visibility = Visibility.Collapsed;
         }
         return true;
      }
      //---------------------------------------
      private Options Deserialize(String s_xml)
      {
         Options? options = new Options();
         if (false == String.IsNullOrEmpty(s_xml))
         {
            try // XML serializer does not work for Interfaces
            {
               StringReader stringreader = new StringReader(s_xml);
               XmlReader xmlReader = XmlReader.Create(stringreader);
               XmlSerializer serializer = new XmlSerializer(typeof(Options)); // Sustem.IO.FileNotFoundException thrown but normal behavior - handled in XmlSerializer constructor
               Object? obj = serializer.Deserialize(xmlReader);
               options = obj as Options;
            }
            catch (DirectoryNotFoundException dirException)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + "\ndirException=" + dirException.ToString());
            }
            catch (FileNotFoundException fileException)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + "\nfileException=" + fileException.ToString());
            }
            catch (IOException ioException)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + "\nioException=" + ioException.ToString());
            }
            catch (Exception ex)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize(): s=" + s_xml + "\nex=" + ex.ToString());
            }
         }
         if (null == options)
            options = new Options();
         if (0 == options.Count)
            options.SetOriginalGameOptions();
         return options;
      }
      //---------------------------------------
      private bool UpdateCanvasTank(IGameInstance gi, GameAction action)
      {
         // Clean the Canvas of all marks
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasTank.Children)
         {
            if (ui is Polygon polygon)
               elements.Add(ui);
            if (ui is Image img)
            {
               if (true == img.Name.Contains("TankMat"))
                  continue;
               elements.Add(ui);
            }
            if (ui is Button b)
            {
               IMapItem? mi = gi.Hatches.Find(b.Name);
               if (null == mi)
                  mi = gi.CrewActions.Find(b.Name);
               if ( null == mi )
                  mi = gi.ReadyRacks.Find(b.Name);
               if (null == mi)
                  mi = gi.GunLoads.Find(b.Name);
               if (null == mi) // If Button does not have corresponding MapItem, remove button.
               {
                  elements.Add(ui);
                  myTankButtons.Remove(b);
               }
            }
         }
         foreach (UIElement ui1 in elements)
            myCanvasTank.Children.Remove(ui1);
         //-------------------------------------------------------
         if( true == Logger.theLogLevel[(int)LogEnum.LE_SHOW_TANK_BUTTONS])
         {
            StringBuilder sbbuttons = new StringBuilder();
            int lastParen = myTankButtons.Count - 1;
            sbbuttons.Append("[");
            int i = 0;
            foreach (Button b in myTankButtons)
            {
               sbbuttons.Append(b.Name);
               if (i++ != lastParen)
                  sbbuttons.Append(",");
            }
            sbbuttons.Append("]");
            Logger.Log(LogEnum.LE_SHOW_TANK_BUTTONS, "UpdateCanvasTank(): gp=" + gi.GamePhase.ToString() + " bp=" + gi.BattlePhase.ToString() + " Buttons =" + sbbuttons.ToString());
         }
         //-------------------------------------------------------
         if (GamePhase.UnitTest == gi.GamePhase)
            return true;
         //-------------------------------------------------------
         if (false == UpdateCanvasTankMapItems(gi.Hatches))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasTankMapItems(Hatches) returned false");
            return false;
         }
         //-------------------------------------------------------
         if (false == UpdateCanvasTankMapItems(gi.ReadyRacks))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasTankMapItems(ReadyRacks) returned false");
            return false;
         }
         //-------------------------------------------------------
         if (false == UpdateCanvasTankMapItems(gi.CrewActions))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasTankMapItems(CrewActions) returned false");
            return false;
         }
         //-------------------------------------------------------
         try
         {
            switch (action)
            {
               case GameAction.PreparationsHatches:
                  if (false == UpdateCanvasTankHatches(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasTankHatches() returned false");
                     return false;
                  }
                  break;
               case GameAction.PreparationsGunLoad:
               case GameAction.PreparationsGunLoadSelect:
               case GameAction.BattleRoundSequenceLoadMainGun:
                  if (false == UpdateCanvasTankGunLoad(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasTankGunLoad() returned false");
                     return false;
                  }
                  break;
               case GameAction.BattleRoundSequenceCrewOrders:
                  if (false == UpdateCanvasTankOrders(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasTankOrders() returned false");
                     return false;
                  }
                  break;
               case GameAction.BattleRoundSequenceAmmoOrders:
                  if (false == UpdateCanvasTankAmmoOrders(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasTankAmmoOrders() returned false");
                     return false;
                  }
                  break;
               case GameAction.EndGameClose:
                  GameAction outActionClose = GameAction.EndGameExit;
                  myGameEngine.PerformAction(ref gi, ref outActionClose);
                  break;
               default:
                  break;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): EXCEPTION THROWN a=" + action.ToString() + "\ne=" + e.ToString());
            return false;
         }
         //-------------------------------------------------------
         if (false == UpdateCanvasTankMapItems(gi.Hatches))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasMapItems(Hatches) returned false");
            return false;
         }
         //-------------------------------------------------------
         if (false == UpdateCanvasTankMapItems(gi.GunLoads))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasMapItems(GunLoads) returned false");
            return false;
         }
         return true;
      }
      private bool UpdateCanvasTankMapItems(IMapItems mapItems)
      {
         foreach (IMapItem mi in mapItems)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMapItems(): mi=null");
               return false;
            }
            Button? b = myTankButtons.Find(mi.Name);
            if (null != b)
            {
               Logger.Log(LogEnum.LE_SHOW_MAPITEM_TANK, "UpdateCanvasTankMapItems(): 1-mi=" + mi.Name + " loc=" + mi.Location.ToString() + " t=" + mi.TerritoryCurrent.Name + " tLoc=" + mi.TerritoryCurrent.CenterPoint.ToString());
               b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
               b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
               Canvas.SetLeft(b, mi.Location.X);
               Canvas.SetTop(b, mi.Location.Y);
               Canvas.SetZIndex(b, 900);
            }
            else
            {
               Button newButton = CreateButtonMapItem(myTankButtons, mi);
               myCanvasTank.Children.Add(newButton);
               Logger.Log(LogEnum.LE_SHOW_MAPITEM_TANK, "UpdateCanvasTankMapItems(): 2-mi=" + mi.Name + " loc=" + mi.Location.ToString() + " t=" + mi.TerritoryCurrent.Name + " tLoc=" + mi.TerritoryCurrent.CenterPoint.ToString());
            }

         }
         return true;
      }
      private bool UpdateCanvasTankHatches(IGameInstance gi, GameAction action)
      {
         myPolygons.Clear();
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankHatches(): report=null");
            return false;
         }
         TankCard tankCard = new TankCard(report.TankCardNum);
         //--------------------------------
         string[] crewmembers = new string[4] { "Driver", "Assistant", "Commander", "Loader" };
         try
         {
            foreach (string crewmember in crewmembers)
            {
               if ((crewmember == "Loader") && (false == tankCard.myIsLoaderHatch))
                  continue;
               ICrewMember? cm = myGameInstance.GetCrewMember(crewmember);
               if (null == cm)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonHatches(): cm=null for " + crewmember);
                  return false;
               }
               if (true == cm.IsButtonedUp)
               {
                  string tName = crewmember + "Hatch";
                  ITerritory? t = Territories.theTerritories.Find(tName);
                  if (null == t)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankHatches(): cannot find tName=" + tName);
                     return false;
                  }
                  PointCollection points = new PointCollection();
                  foreach (IMapPoint mp1 in t.Points)
                     points.Add(new System.Windows.Point(mp1.X, mp1.Y));
                  Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.ToString() };
                  myPolygons.Add(aPolygon);
                  myCanvasTank.Children.Add(aPolygon);
                  aPolygon.MouseDown += MouseDownPolygonHatches;
               }
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankHatches(): EXCEPTION THROWN a=" + action.ToString() + "\ne=" + e.ToString());
            return false;
         }
         return true;
      }
      private bool UpdateCanvasTankGunLoad(IGameInstance gi, GameAction action)
      {
         myPolygons.Clear();
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankGunLoad(): report=null");
            return false;
         }
         //--------------------------------
         string[] gunLoads = new string[5] { "He", "Ap", "Wp", "Hbci", "Hvap" };
         try
         {
            foreach (string gunload in gunLoads)
            {
               switch (gunload)
               {
                  case "He":
                     if (0 == report.MainGunHE)
                        continue;
                     break;
                  case "Ap":
                     if (0 == report.MainGunAP)
                        continue;
                     break;
                  case "Wp":
                     if (0 == report.MainGunWP)
                        continue;
                     break;
                  case "Hbci":
                     if (0 == report.MainGunHBCI)
                        continue;
                     break;
                  case "Hvap":
                     if (0 == report.MainGunHVAP)
                        continue;
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankGunLoad(): reached default gunload=" + gunload);
                     return false;
               }
               string tName = "GunLoad" + gunload;
               ITerritory? t = Territories.theTerritories.Find(tName);
               if (null == t)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankHatches(): cannot find tName=" + tName);
                  return false;
               }
               PointCollection points = new PointCollection();
               foreach (IMapPoint mp1 in t.Points)
                  points.Add(new System.Windows.Point(mp1.X, mp1.Y));
               Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.ToString() };
               myPolygons.Add(aPolygon);
               myCanvasTank.Children.Add(aPolygon);
               aPolygon.MouseDown += MouseDownPolygonGunLoad;
               //-------------------------------------------
               if( BattlePhase.MarkAmmoReload == gi.BattlePhase )
               {
                  foreach(Button b in this.myTankButtons)
                  {
                     if (true == b.Name.Contains(tName))
                        b.ContextMenu = myContextMenuGunLoadActions[tName];
                  }
               }
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankHatches(): EXCEPTION THROWN a=" + action.ToString() + "\n" + e.ToString());
            return false;
         }
         return true;
      }
      private bool UpdateCanvasTankOrders(IGameInstance gi, GameAction action)
      {
         //--------------------------------
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankOrders(): report=null");
            return false;
         }
         TankCard tankCard = new TankCard(report.TankCardNum);
         //--------------------------------
         string[] crewmembers = new string[5] { "Driver", "Assistant", "Commander", "Loader", "Gunner" };
         foreach (string crewmember in crewmembers)
         {
            if (crewmember == "Gunner") // Gunners have no hatches
               continue;
            if ((crewmember == "Loader") && (false == tankCard.myIsLoaderHatch)) // some loaders have no hatches
               continue;
            ICrewMember? cm = myGameInstance.GetCrewMember(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankOrders(): cm=null for " + crewmember);
               return false;
            }
            if (true == cm.IsButtonedUp)
            {
               string tName = crewmember + "Hatch";
               ITerritory? t = Territories.theTerritories.Find(tName);
               if (null == t)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankOrders(): cannot find tName=" + tName);
                  return false;
               }
               PointCollection points = new PointCollection();
               foreach (IMapPoint mp1 in t.Points)
                  points.Add(new System.Windows.Point(mp1.X, mp1.Y));
               Polygon aPolygon = new Polygon {Fill = Utilities.theBrushRegion, Points = points, Name = t.ToString() };
               myPolygons.Add(aPolygon);
               myCanvasTank.Children.Add(aPolygon);
               aPolygon.MouseDown += MouseDownPolygonHatches;
            }
         }
         foreach (string crewmember in crewmembers)
         {
            string tName = crewmember + "Action";
            ITerritory? t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankOrders(): cannot find tName=" + tName);
               return false;
            }
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.ToString() };
            aPolygon.ContextMenu = myContextMenuCrewActions[crewmember];
            myPolygons.Add(aPolygon);
            myCanvasTank.Children.Add(aPolygon);
            aPolygon.MouseDown += MouseDownPolygonCrewActions;
         }
         return true;
      }
      private bool UpdateCanvasTankAmmoOrders(IGameInstance gi, GameAction action)
      {
         //--------------------------------
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankAmmoOrders(): report=null");
            return false;
         }
         TankCard tankCard = new TankCard(report.TankCardNum);
         //--------------------------------
         string[] gunLoads = new string[5] { "He", "Ap", "Wp", "Hbci", "Hvap" };
         foreach (string gunload in gunLoads)
         {
            switch (gunload)
            {
               case "He":
                  if (0 == report.MainGunHE)
                     continue;
                  break;
               case "Ap":
                  if (0 == report.MainGunAP)
                     continue;
                  break;
               case "Wp":
                  if (0 == report.MainGunWP)
                     continue;
                  break;
               case "Hbci":
                  if (0 == report.MainGunHBCI)
                     continue;
                  break;
               case "Hvap":
                  if (0 == report.MainGunHVAP)
                     continue;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankAmmoOrders(): reached default gunload=" + gunload);
                  return false;
            }
            string tName = "GunLoad" + gunload;
            ITerritory? t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankAmmoOrders(): cannot find tName=" + tName);
               return false;
            }
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.ToString() };
            myPolygons.Add(aPolygon);
            myCanvasTank.Children.Add(aPolygon);
            //------------------------------------------------
            if( BattlePhase.MarkAmmoReload == gi.BattlePhase )
            {
               aPolygon.MouseDown += MouseDownPolygonAmmoActions;
               aPolygon.ContextMenu = myContextMenuGunLoadActions[tName];
               IMapItem? gunLoad = null;
               foreach(IMapItem mi in gi.GunLoads) // The context menu is assigned to the GunLoad Button
               {
                  if (true == mi.Name.Contains("GunLoadInGun"))
                     gunLoad = mi;
               }
               if (null != gunLoad)
               {
                  foreach (Button b in myTankButtons)
                  {
                     if (b.Name == gunLoad.Name)
                        b.ContextMenu = myContextMenuGunLoadActions[gunLoad.TerritoryCurrent.Name];
                  }
               }
            }
            else
            {
               aPolygon.MouseDown += MouseDownPolygonGunLoad;
            }
         }
         return true;
      }
      //---------------------------------------
      private bool UpdateCanvasMain(IGameInstance gi, GameAction action)
      {
         IStacks? stacks = null;
         List<Button>? buttons = null;
         if(EnumMainImage.MI_Move == CanvasImageViewer.theMainImage )
         {
            stacks = gi.MoveStacks;
            buttons = myMoveButtons;
            myBattleButtons.Clear();
         }
         else if (EnumMainImage.MI_Battle == CanvasImageViewer.theMainImage )
         {
            stacks = gi.BattleStacks;
            buttons = myBattleButtons;
            myMoveButtons.Clear();
         }
         else
         {
            return true;
         }
         //-------------------------------------------------------
         UpdateCanvasMainClear(buttons, stacks);
         //-------------------------------------------------------
         if (GamePhase.UnitTest == gi.GamePhase)
            return true;
         //-------------------------------------------------------
         if (false == UpdateCanvasMainMapItems(buttons, stacks))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMainMapItems() returned false");
            return false;
         }
         //-------------------------------------------------------
         if (false == UpdateCanvasAnimateBattlePhase(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasAnimateBattlePhase() returned false");
            return false;
         }
         //-------------------------------------------------------
         try
         {
            switch (action)
            {
               case GameAction.PreparationsLoaderSpot:
                  if (false == UpdateCanvasMainSpottingLoader(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMainSpotting() returned false");
                     return false;
                  }
                  break;
               case GameAction.PreparationsCommanderSpot:
                  if (false == UpdateCanvasMainSpottingCommander(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMainSpotting() returned false");
                     return false;
                  }
                  break;
               case GameAction.MovementStartAreaSetRoll:
                  IMapItem? startArea = gi.MoveStacks.FindMapItem("StartArea"); // center thumbnails around taskforce
                  if (null != startArea)
                     UpdateScrollbarThumbnails(startArea.TerritoryCurrent);
                  break;
               case GameAction.MovementExitAreaSetRoll:
                  IMapItem? exitArea = gi.MoveStacks.FindMapItem("ExitArea"); // center thumbnails around exit area
                  if (null != exitArea)
                     UpdateScrollbarThumbnails(exitArea.TerritoryCurrent);
                  break;
               case GameAction.MovementEnemyStrengthChoice:
                  IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce"); // center thumbnails around task force
                  if (null != taskForce)
                     UpdateScrollbarThumbnails(taskForce.TerritoryCurrent);
                  if (false == UpdateCanvasMainEnemyStrengthCheckTerritory(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMainEnemyStrengthCheckTerritory() returned false");
                     return false;
                  }
                  break;
               case GameAction.MovementArtillerySupportChoice:
                  if (false == UpdateCanvasMainArtillerySupportCheck(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMainArtillerySupportCheck() returned false");
                     return false;
                  }
                  break;
               case GameAction.MovementAirStrikeChoice:
                  if (false == UpdateCanvasMainAirStrikeCheckTerritory(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMainAirStrikeCheckTerritory() returned false");
                     return false;
                  }
                  break;
               case GameAction.MovementEnterArea:
                  if (false == UpdateCanvasMainEnterArea(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMainEnterArea() returned false");
                     return false;
                  }
                  break;
               case GameAction.MovementAdvanceFireChoice:
               case GameAction.UpdateBattleBoard:
               case GameAction.BattleRoundSequenceMovementRoll:
                  if (false == UpdateCanvasMovement(gi, action, stacks, buttons))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMovement() returned false");
                     return false;
                  }
                  break;
               case GameAction.MovementEnterAreaUsControl:
                  if (false == UpdateCanvasMovement(gi, action, stacks, buttons))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMovement() returned false");
                     return false;
                  }
                  if (false == UpdateCanvasMainEnemyStrengthCheckTerritory(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMainEnemyStrengthCheckTerritory() returned false");
                     return false;
                  }
                  break;
               case GameAction.BattleStart:
                  if( true == gi.IsAdvancingFireChosen)
                  {
                     if (false == UpdateCanvasMainAdvancingMarkerFirePlace())
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMainAdvancingMarkerFirePlace() returned false");
                        return false;
                     }
                  }
                  break;
               case GameAction.EndGameClose:
                  GameAction outActionClose = GameAction.EndGameExit;
                  myGameEngine.PerformAction(ref gi, ref outActionClose);
                  break;
               default:
                  break;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): EXCEPTION THROWN a=" + action.ToString() + "\n" + e.ToString());
            return false;
         }
         return true;
      }
      private void UpdateCanvasMainClear(List<Button> buttons, IStacks stacks)
      {
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "UpdateCanvasMainClear(): " + stacks.ToString());
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children) // Clean the Canvas of all marks
         {
            if (ui is Polygon polygon)
            {
               elements.Add(ui);
            }
            if (ui is Ellipse ellipse)
            {
               if ("CenterPoint" != ellipse.Name) // CenterPoint is a unit test ellipse
                  elements.Add(ui);
            }
            if (ui is Button button)
            {
               if (true == button.Name.Contains("Die"))
                  continue;
               IMapItem? mi = stacks.FindMapItem(button.Name);
               if (null == mi) // If Button does not have corresponding MapItem, remove button.
               {
                  elements.Add(ui);
                  buttons.Remove(button);
                  IStack? stack = stacks.Find(button.Name);
                  if (null == stack)
                  {
                     Logger.Log(LogEnum.LE_SHOW_STACK_DEL, "UpdateCanvasMainClear(): mi=" + button.Name + " does not belong to " + stacks.ToString());
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_SHOW_STACK_DEL, "UpdateCanvasMainClear(): Remove mi=" + button.Name + " from stack=" + stack.ToString());
                     stack.MapItems.Remove(button.Name);
                  }
               }
               else
               {
                  MapItem.SetButtonContent(button, mi, true, true);
               }
            }
            if (ui is Label label)  // A Game Feat Label
               elements.Add(ui);
            if (ui is Rectangle rect)  
               elements.Add(ui);
            if (ui is Image img)
            {
               if (true == img.Name.Contains("Canvas"))
                  continue;
               if (true == img.Name.Contains("ShermanExploding"))
                  continue;
               if (true == img.Name.Contains("ShermanBrewUp"))
                  continue;
               elements.Add(ui);
            }
            if (ui is TextBlock tb)
               elements.Add(ui);
         }
         foreach (UIElement ui1 in elements)
            myCanvasMain.Children.Remove(ui1);
      }
      private void UpdateScrollbarThumbnails(ITerritory t)
      {
         double percentHeight = (t.CenterPoint.Y / myCanvasMain.ActualHeight);
         double percentToScroll = 0.0;
         if (percentHeight < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentHeight)
            percentToScroll = 1.0;
         else
            percentToScroll = percentHeight / 0.5 - 0.5;
         double scrollHeight = myScrollViewerMap.ScrollableHeight;
         if (0.0 == scrollHeight)
            scrollHeight = myPreviousScrollHeight;
         else
            myPreviousScrollHeight = myScrollViewerMap.ScrollableHeight;
         double amountToScrollV = percentToScroll * scrollHeight;
         myScrollViewerMap.ScrollToVerticalOffset(amountToScrollV);
         double percentWidth = (t.CenterPoint.X / myCanvasMain.ActualWidth);
         if (percentWidth < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentWidth)
            percentToScroll = 1.0;
         else
            percentToScroll = percentWidth / 0.5 - 0.5;
         double scrollWidth = myScrollViewerMap.ScrollableWidth;
         if (0.0 == scrollWidth)
            scrollWidth = myPreviousScrollWidth;
         else
            myPreviousScrollWidth = myScrollViewerMap.ScrollableWidth;
         double amountToScrollH = percentToScroll * scrollWidth;
         myScrollViewerMap.ScrollToHorizontalOffset(amountToScrollH);
      }
      private bool UpdateCanvasMainMapItems(List<Button> buttons, IStacks stacks)
      {
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "UpdateCanvasMainMapItems(): " + stacks.ToString());
         foreach (IStack stack in stacks)
         {
            ITerritory t = stack.Territory;
            int counterCount = 0;
            foreach (IMapItem mi in stack.MapItems)
            {
               if (null == mi)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainMapItems(): mi=null");
                  return false;
               }
               //---------------------------------------------
               Button? b = buttons.Find(mi.Name);
               if (null != b)
               {
                  b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
                  b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
                  Canvas.SetLeft(b, mi.Location.X);
                  Canvas.SetTop(b, mi.Location.Y);
                  Canvas.SetZIndex(b, 900 + counterCount);
                  ++counterCount;
                  RotateTransform rotateTransform = new RotateTransform();
                  b.RenderTransformOrigin = new Point(0.5, 0.5);
                  rotateTransform.Angle = mi.RotationHull + mi.RotationOffset;
                  b.RenderTransform = rotateTransform;
               }
               else
               {
                  Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "UpdateCanvasMainMapItems(): Adding mi=" + mi.Name + " to stack@" + stack.ToString());
                  Button newButton = CreateButtonMapItem(buttons, mi);
                  myCanvasMain.Children.Add(newButton);
               }
            }
         }
         return true;
      }
      private bool UpdateCanvasMainSpottingLoader(IGameInstance gi, GameAction action)
      {
         myEllipses.Clear();
         string[] sectors = new string[6] { "Spot1", "Spot2", "Spot3", "Spot4", "Spot6", "Spot9" };
         foreach (string s in sectors)
         {
            ITerritory? t = Territories.theTerritories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainSpottingLoader(): cannot find tName=" + s);
               return false;
            }
            Ellipse aEllipse = new Ellipse
            {
               Name = t.Name,
               Fill = Utilities.theBrushRegion,
               StrokeThickness = 3,
               Stroke = Utilities.theBrushRegion,
               Width = ELLIPSE_DIAMETER,
               Height = ELLIPSE_DIAMETER
            };
            System.Windows.Point p = new System.Windows.Point(t.CenterPoint.X, t.CenterPoint.Y);
            p.X -= ELLIPSE_RADIUS;
            p.Y -= ELLIPSE_RADIUS;
            Canvas.SetLeft(aEllipse, p.X);
            Canvas.SetTop(aEllipse, p.Y);
            myCanvasMain.Children.Add(aEllipse);
            myEllipses.Add(aEllipse);
            aEllipse.MouseDown += MouseDownEllipseSpottingLoader;
         }
         return true;
      }
      private bool UpdateCanvasMainSpottingCommander(IGameInstance gi, GameAction action)
      {
         myEllipses.Clear();
         string[] sectors = new string[6] { "CSpot1", "CSpot2", "CSpot3", "CSpot4", "CSpot6", "CSpot9" };
         foreach (string s in sectors)
         {
            ITerritory? t = Territories.theTerritories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainSpottingCommander(): cannot find tName=" + s);
               return false;
            }
            Ellipse aEllipse = new Ellipse
            {
               Name = t.Name,
               Fill = Utilities.theBrushRegion,
               StrokeThickness = 3,
               Stroke = Utilities.theBrushRegion,
               Width = ELLIPSE_DIAMETER,
               Height = ELLIPSE_DIAMETER
            };
            System.Windows.Point p = new System.Windows.Point(t.CenterPoint.X, t.CenterPoint.Y);
            p.X -= ELLIPSE_RADIUS;
            p.Y -= ELLIPSE_RADIUS;
            Canvas.SetLeft(aEllipse, p.X);
            Canvas.SetTop(aEllipse, p.Y);
            myCanvasMain.Children.Add(aEllipse);
            myEllipses.Add(aEllipse);
            aEllipse.MouseDown += MouseDownEllipseSpottingCommander;
         }
         return true;
      }
      private bool UpdateCanvasMainAdvancingMarkerFirePlace()
      {
         string[] sectors = new string[8] { "B4L", "B4M", "B4C", "B6M", "B6C", "B9L", "B9M", "B9C" };
         foreach (string s in sectors)
         {
            ITerritory? t = Territories.theTerritories.Find(s);
            if( null == t )
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainAdvancingMarkerFirePlace(): t=null for tName=" + s);
               return false;
            }
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.Name };
            aPolygon.MouseDown += MouseDownPolygonPlaceAdvanceFire;
            myPolygons.Add(aPolygon);
            myCanvasMain.Children.Add(aPolygon);
         }
         return true;
      }
      private bool UpdateCanvasAnimateBattlePhase(IGameInstance gi)
      {
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children) // Clean the Canvas of rectangles
         {
            if (ui is Rectangle)
               elements.Add(ui);
         }
         foreach (UIElement ui1 in elements)
            myCanvasMain.Children.Remove(ui1);
         //-------------------------------------
         if (BattlePhase.None == gi.BattlePhase)
            return true;
         ITerritory? t = Territories.theTerritories.Find( gi.BattlePhase.ToString() );
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasAnimateBattlePhase(): t=null for name=" + gi.BattlePhase.ToString());
            return false;
         }
         ColorAnimation colorAnimation = new ColorAnimation
         {
            From = Colors.Lime,
            To = Colors.Transparent,
            Duration = new Duration(TimeSpan.FromSeconds(1)),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
         };
         if( 4 != t.Points.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasAnimateBattlePhase(): t=" + t.Name + " has points=" + t.Points.Count.ToString());
            return false;
         }
         IMapPoint p1 = t.Points[0];
         IMapPoint p2 = t.Points[2];
         Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "UpdateCanvasAnimateBattlePhase(): phase=" + gi.BattlePhase.ToString() + " pt=" + p1.ToString());
         Rectangle r = new Rectangle();
         r.Fill = new SolidColorBrush(Colors.Blue);
         SolidColorBrush brush = (SolidColorBrush)r.Fill;
         brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
         r.Height = p2.Y - p1.Y;
         r.Width = p2.X - p1.X;
         myCanvasMain.Children.Add(r);
         Canvas.SetLeft(r, p1.X);
         Canvas.SetTop(r, p1.Y);
         return true;
      }
      private bool UpdateCanvasFriendlyAdvance(IGameInstance gi)
      {
         //--------------------------------------------
         List<ITerritory> usControlledTerritory = new List<ITerritory>();
         foreach (IStack stack in gi.BattleStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.Name.Contains("UsControl"))
                  usControlledTerritory.Add(stack.Territory);
            }
         }
         //--------------------------------------------
         foreach (ITerritory t in usControlledTerritory)
         {
            if (3 != t.Name.Length)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasFriendlyAdvance(): 3 != t.Name.Length for t=" + t.Name);
               return false;
            }
            string sector = t.Name[1].ToString();
            List<string> highlightedTerritoryNames = new List<string>();
            string adjName;
            string adjSector;
            ITerritory? adjTerritory = null;
            switch (sector)
            {
               case "1":
                  adjName = "B9M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if( (null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)) )
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B2M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     highlightedTerritoryNames.Add(adjName);
                  break;
               case "2":
                  adjName = "B1M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B3M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     highlightedTerritoryNames.Add(adjName);
                  break;
               case "3":
                  adjName = "B2M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B4M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     highlightedTerritoryNames.Add(adjName);
                  break;
               case "4":
                  adjName = "B3M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B6M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     highlightedTerritoryNames.Add(adjName);
                  break;
               case "6":
                  adjName = "B4M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B9M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     highlightedTerritoryNames.Add(adjName);
                  break;
               case "9":
                  adjName = "B6M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B1M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     highlightedTerritoryNames.Add(adjName);
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasFriendlyAdvance(): reached default sector=" + sector.ToString());
                  return false;
            }
            foreach( string s in highlightedTerritoryNames )
            {
               ITerritory? highlighted = Territories.theTerritories.Find(s);
               if( null == highlighted )
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasFriendlyAdvance(): highlighted=null for s=" + s);
                  return false;
               }
               PointCollection points = new PointCollection();
               foreach (IMapPoint mp1 in highlighted.Points)
                  points.Add(new System.Windows.Point(mp1.X, mp1.Y));
               Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = highlighted.Name };
               aPolygon.MouseDown += MouseDownPolygonFriendlyAdvance;
               myPolygons.Add(aPolygon);
               myCanvasMain.Children.Add(aPolygon);
            }
         }
         return true;
      }
      private bool UpdateCanvasEnemyAdvance(IGameInstance gi)
      {
         if (null == myGameInstance.EnemyAdvance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasEnemyAdvance(): myGameInstance.EnemyAdvance=null");
            return false;
         }
         PointCollection points = new PointCollection();
         foreach (IMapPoint mp1 in myGameInstance.EnemyAdvance.Points)
            points.Add(new System.Windows.Point(mp1.X, mp1.Y));
         Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = myGameInstance.EnemyAdvance.Name };
         aPolygon.MouseDown += MouseDownPolygonEnemyAdvance;
         myPolygons.Add(aPolygon);
         myCanvasMain.Children.Add(aPolygon);
         return true;
      }
      private bool UpdateCanvasShermanSelectTarget(IGameInstance gi)
      {
         foreach(IMapItem mi in gi.Targets)
         {
            foreach(Button b in myBattleButtons)
            {
               if( mi.Name == b.Name )
               {
                  b.BorderBrush = mySolidColorBrushRed;
                  b.BorderThickness = new Thickness(3);
               }
            }
         }
         return true;
      }
      private bool UpdateCanvasShermanSelectTargetMg(IGameInstance gi)
      {
         //------------------------------------------------
         foreach (IMapItem mi in gi.Targets) // All buttons in the target view are selected
         {
            foreach (Button b in myBattleButtons)
            {
               if (mi.Name == b.Name)
               {
                  b.BorderBrush = mySolidColorBrushRed;
                  b.BorderThickness = new Thickness(3);
               }
            }
         }
         //------------------------------------------------
         foreach (ITerritory t in gi.AreaTargets)
         {
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.Name };
            aPolygon.MouseDown += MouseDownPolygonPlaceAdvanceFire;
            myPolygons.Add(aPolygon);
            myCanvasMain.Children.Add(aPolygon);
         }
         return true;
      }
      //---------------------------------------
      private bool UpdateCanvasMainEnemyStrengthCheckTerritory(IGameInstance gi, GameAction action)
      {
         myPolygons.Clear();
         //--------------------------------
         IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
         if (null == taskForce)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainEnemyStrengthCheckTerritory(): taskForce=null");
            return false;
         }
         //--------------------------------
         List<String> sTerritories = taskForce.TerritoryCurrent.Adjacents;
         foreach (string s in sTerritories)
         {
            ITerritory? adj = Territories.theTerritories.Find(s);
            if (null == adj)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainEnemyStrengthCheckTerritory(): 1 adj=null for " + s);
               return false;
            }
            if ('E' == adj.Name.Last()) // ingore entry/exit territories
               continue;
            //-------------------------------
            ITerritory? t = Territories.theTerritories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainEnemyStrengthCheckTerritory(): 2 t=null for " + s);
               return false;
            }
            bool isSkip = false;
            IStack? stack = gi.MoveStacks.Find(t);
            if (null != stack)
            {
               foreach(IMapItem mi in stack.MapItems )
               {
                  if( (true == mi.Name.Contains("Strength")) || (true == mi.Name.Contains("UsControl")) )
                  {
                     isSkip = true;
                     break;
                  }
               }
               if (true == isSkip)
                  continue;
            }
            //-------------------------------
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in adj.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = adj.Name };
            aPolygon.MouseDown += MouseDownPolygonShowEnemyStrength;
            myPolygons.Add(aPolygon);
            myCanvasMain.Children.Add(aPolygon);
         }
         return true;
      }
      private bool UpdateCanvasMainArtillerySupportCheck(IGameInstance gi, GameAction action)
      {
         myPolygons.Clear();
         //--------------------------------
         IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
         if (null == taskForce)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainArtillerySupportCheck(): taskForce=null");
            return false;
         }
         //--------------------------------
         List<String> sTerritories = taskForce.TerritoryCurrent.Adjacents;
         foreach (string s in sTerritories)
         {
            ITerritory? t = Territories.theTerritories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainArtillerySupportCheck(): 1 t=null for " + s);
               return false;
            }
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.Name };
            aPolygon.MouseDown += MouseDownPolygonArtillerySupport;
            myPolygons.Add(aPolygon);
            myCanvasMain.Children.Add(aPolygon);
         }
         return true;
      }
      private bool UpdateCanvasMainAirStrikeCheckTerritory(IGameInstance gi, GameAction action)
      {
         myPolygons.Clear();
         //--------------------------------
         IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
         if (null == taskForce)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainAirStrikeCheckTerritory(): taskForce=null");
            return false;
         }
         //--------------------------------
         List<String> sTerritories = taskForce.TerritoryCurrent.Adjacents;
         foreach (string s in sTerritories)
         {
            ITerritory? t = Territories.theTerritories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainAirStrikeCheckTerritory(): 1 t=null for " + s);
               return false;
            }
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.Name };
            aPolygon.MouseDown += MouseDownPolygonAirStrike;
            myPolygons.Add(aPolygon);
            myCanvasMain.Children.Add(aPolygon);
         }
         return true;
      }
      private bool UpdateCanvasMainEnterArea(IGameInstance gi, GameAction action)
      {
         myPolygons.Clear();
         //--------------------------------
         IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
         if (null == taskForce)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainEnterArea(): taskForce=null");
            return false;
         }
         //--------------------------------
         List<String> sTerritories = taskForce.TerritoryCurrent.Adjacents;
         foreach (string s in sTerritories)
         {
            ITerritory? t = Territories.theTerritories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainEnterArea(): 1 t=null for " + s);
               return false;
            }
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.Name };
            aPolygon.MouseDown += MouseDownPolygonEnterArea;
            myPolygons.Add(aPolygon);
            myCanvasMain.Children.Add(aPolygon);
         }
         return true;
      }
      private bool UpdateCanvasMovement(IGameInstance gi, GameAction action, IStacks stacks, List<Button> buttons)
      {
         try
         {
            foreach ( IMapItemMove mim in gi.MapItemMoves)
            {
               if (null == mim)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim=null");
                  return false;
               }
               if (null == mim.OldTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim.OldTerritory=null");
                  return false;
               }
               IMapItem mi = mim.MapItem;
               if (false == MovePathAnimate(mim, buttons))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): MovePathAnimate() returned false t=" + mim.OldTerritory.ToString());
                  gi.MapItemMoves.Clear();
                  return false;
               }
               if (null == mim)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim2=null");
                  return false;
               }
               if (null == mim.NewTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim2.NewTerritory=null");
                  return false;
               }
               Logger.Log(LogEnum.LE_VIEW_ROTATION, "UpdateCanvasMovement(): mi=" + mim.MapItem.Name + " r=" + mim.MapItem.RotationOffset + " rb=" + mim.MapItem.RotationHull);
               //------------------------------------------
               stacks.Remove(mi); // remove from existing stack
               Logger.Log(LogEnum.LE_VIEW_MIM, "UpdateCanvasMovement(): mi=" + mi.Name + " t=" + mi.TerritoryCurrent.Name + " moving to t=" + mim.NewTerritory.Name + " " + gi.MoveStacks.ToString());
               mi.TerritoryCurrent = mi.TerritoryStarting = mim.NewTerritory;
               stacks.Add(mi); // add to new stack
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement():  EXCEPTION THROWN e=\n" + e.ToString());
            return false;
         }
         return true;
      }
      private bool MovePathAnimate(IMapItemMove mim, List<Button> buttons)
      {
         const int ANIMATE_TIME_SEC = 4;
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): myGameInstance=null for n=" + mim.MapItem.Name);
            return false;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): mim.NewTerritory=null n=" + mim.MapItem.Name);
            return false;
         }
         Button? b = buttons.Find(mim.MapItem.Name);
         if (null == b)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): b=null for n=" + mim.MapItem.Name);
            return false;
         }
         try
         {
            Canvas.SetZIndex(b, 10000); // Move the button to the top of the Canvas
            double diffXOld = theOldXAfterAnimation - mim.MapItem.Location.X;
            double diffYOld = theOldYAfterAnimation - mim.MapItem.Location.Y;
            double xStart = mim.MapItem.Location.X; // get top left point of MapItem
            double yStart = mim.MapItem.Location.Y;
            Logger.Log(LogEnum.LE_VIEW_ROTATION, "+++++++++++++++++MovePathAnimate(): 1 - mi.X=" + xStart.ToString("F0") + "  mi.Y=" + yStart.ToString("F0") + " r=" + mim.MapItem.RotationOffset.ToString("F0") + " dX=" + diffXOld.ToString("F0") + " dY=" + diffYOld.ToString("F0") + " rb=" + mim.MapItem.RotationHull.ToString("F0"));
            PathFigure aPathFigure = new PathFigure() { StartPoint = new System.Windows.Point(xStart, yStart) };
            if (null == mim.BestPath)
            {
               Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): mim.BestPath=null for n=" + mim.MapItem.Name);
               return false;
            }
            int lastItemIndex = mim.BestPath.Territories.Count - 1;
            for (int i = 0; i < lastItemIndex; i++) // add intermediate movement points - not really used in Barbarian Prince as only move one hex at a time
            {
               ITerritory t = mim.BestPath.Territories[i];
               double x = t.CenterPoint.X - Utilities.theMapItemOffset;
               double y = t.CenterPoint.Y - Utilities.theMapItemOffset;
               System.Windows.Point newPoint = new System.Windows.Point(x, y);
               LineSegment lineSegment = new LineSegment(newPoint, false);
               aPathFigure.Segments.Add(lineSegment);
            }
            // Add the last line segment
            IMapPoint mp;
            if (BattlePhase.EnemyAction == myGameInstance.BattlePhase)
            {
               mp = Territory.GetRandomPoint(mim.NewTerritory, mim.MapItem.Zoom * Utilities.theMapItemOffset);
            }
            else
            {
               mp = Territory.GetRandomPoint(mim.NewTerritory, mim.MapItem.Zoom * Utilities.theMapItemOffset);
               //double diffY = Math.Abs(myGameInstance.Home.CenterPoint.Y - yStart);
               //mp = new MapPoint(xStart, yStart + diffY);
            }
            if ((Math.Abs(mp.X - xStart) < 2) && (Math.Abs(mp.Y - yStart) < 2)) // if already at final location, skip animation or get runtime exception
               return true;
            theOldXAfterAnimation = mp.X;
            theOldYAfterAnimation = mp.Y;
            //----------------------------------------------------
            System.Windows.Point newPoint2 = new System.Windows.Point(mp.X, mp.Y);

            LineSegment lineSegment2 = new LineSegment(newPoint2, false);
            aPathFigure.Segments.Add(lineSegment2);
            // Animiate the map item along the line segment
            PathGeometry aPathGeo = new PathGeometry();
            aPathGeo.Figures.Add(aPathFigure);
            aPathGeo.Freeze();
            DoubleAnimationUsingPath xAnimiation = new DoubleAnimationUsingPath();
            xAnimiation.PathGeometry = aPathGeo;
            xAnimiation.Duration = TimeSpan.FromSeconds(ANIMATE_TIME_SEC);
            xAnimiation.Source = PathAnimationSource.X;
            DoubleAnimationUsingPath yAnimiation = new DoubleAnimationUsingPath();
            yAnimiation.PathGeometry = aPathGeo;
            yAnimiation.Duration = TimeSpan.FromSeconds(ANIMATE_TIME_SEC);
            yAnimiation.Source = PathAnimationSource.Y;
            b.RenderTransform = new TranslateTransform();
            b.RenderTransformOrigin = new Point(0.5, 0.5);
            RotateTransform rotateTransform = new RotateTransform();
            //----------------------------------------------------
            if ( (true == mim.MapItem.IsVehicle) && (BattlePhase.ConductCrewAction != myGameInstance.BattlePhase) )
            {
               double xDiff = xStart - mp.X;
               double yDiff = yStart - mp.Y;
               double angleRotation = Math.Atan2(yDiff, xDiff) * 180 / Math.PI - 90;
               rotateTransform.Angle = angleRotation;
               b.RenderTransform = rotateTransform;
            }
            else
            {
               rotateTransform.Angle = mim.MapItem.RotationOffset + mim.MapItem.RotationHull;
               b.RenderTransform = rotateTransform;
            }
            //----------------------------------------------------
            b.BeginAnimation(Canvas.LeftProperty, xAnimiation);
            b.BeginAnimation(Canvas.TopProperty, yAnimiation);
            mim.MapItem.Location = mp;
            Logger.Log(LogEnum.LE_VIEW_ROTATION, "-----------------MovePathAnimate(): 2 - mi.X=" + mim.MapItem.Location.X.ToString("F0") + " mi.Y=" + mim.MapItem.Location.Y.ToString("F0") + " r=" + mim.MapItem.RotationOffset.ToString("F0") + " rb=" + mim.MapItem.RotationHull.ToString("F0"));
            return true;
         }
         catch (Exception e)
         {
            b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
            b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
            Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate():  EXCEPTION THROWN e=\n" + e.ToString());
            return false;
         }
      }
      private bool MovePathDisplay(IMapItemMove mim, int mapItemCount)
      {
         if (null == mim.OldTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathDisplay(): mim.OldTerritory=null");
            return false;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathDisplay(): mim.NewTerritory=null");
            return false;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathDisplay(): mim.BestPath=null");
            return false;
         }
         //-----------------------------------------
         PointCollection aPointCollection = new PointCollection();
         double offset = 0.0;
         if (0 < mapItemCount)
         {
            if (0 == mapItemCount % 2)
               offset = mapItemCount - 1;
            else
               offset = -mapItemCount;
         }
         offset *= 3.0;
         double xPostion = mim.OldTerritory.CenterPoint.X + offset;
         double yPostion = mim.OldTerritory.CenterPoint.Y + offset;
         System.Windows.Point newPoint = new System.Windows.Point(xPostion, yPostion);
         aPointCollection.Add(newPoint);
         foreach (ITerritory t in mim.BestPath.Territories)
         {
            xPostion = t.CenterPoint.X + offset;
            yPostion = t.CenterPoint.Y + offset;
            newPoint = new System.Windows.Point(xPostion, yPostion);
            aPointCollection.Add(newPoint);
         }
         //-----------------------------------------
         Polyline aPolyline = new Polyline();
         aPolyline.Stroke = myBrushes[myBrushIndex];
         aPolyline.StrokeThickness = 3;
         aPolyline.StrokeEndLineCap = PenLineCap.Triangle;
         aPolyline.Points = aPointCollection;
         aPolyline.StrokeDashArray = myDashArray;
         myCanvasMain.Children.Add(aPolyline);
         //-----------------------------------------
         myRectangleMoving = myRectangles[myBrushIndex];
         if (myRectangles.Count <= ++myBrushIndex)
            myBrushIndex = 0;
         Canvas.SetLeft(myRectangleMoving, mim.MapItem.Location.X);
         Canvas.SetTop(myRectangleMoving, mim.MapItem.Location.Y);
         Canvas.SetZIndex(myRectangleMoving, 1000);
         myRectangleMoving.Visibility = Visibility.Visible;
         return true;
      }
      //-------------CONTROLLER FUNCTIONS---------------------------------
      private void MouseDownPolygonHatches(object sender, MouseButtonEventArgs e)
      {
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonHatches(): clickedPolygon=null");
            return;
         }
         ITerritory? t = Territories.theTerritories.Find(clickedPolygon.Name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonHatches(): t=null for " + clickedPolygon.Name.ToString());
            return;
         }
         string[] crewmembers = new string[4] { "Driver", "Assistant", "Commander", "Loader" };
         foreach (string crewmember in crewmembers)
         {
            if (true == t.Name.Contains(crewmember))
            {
               ICrewMember? cm = myGameInstance.GetCrewMember(crewmember);
               if (null == cm)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonHatches(): myGameInstance.Driver=null for " + clickedPolygon.Name.ToString());
                  return;
               }
               cm.IsButtonedUp = false;
               IMapItem mi = new MapItem(crewmember + "OpenHatch", 1.0, "c15OpenHatch", t);
               myGameInstance.Hatches.Add(mi);
               break;
            }
         }
         GameAction outAction = GameAction.BattleRoundSequenceCrewOrders;
         if (GamePhase.Preparations == myGameInstance.GamePhase)
            outAction = GameAction.PreparationsHatches;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonGunLoad(object sender, MouseButtonEventArgs e)
      {
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonGunLoad(): clickedPolygon=null");
            return;
         }
         ITerritory? newT = Territories.theTerritories.Find(clickedPolygon.Name);
         if (null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonGunLoad(): t=null for " + clickedPolygon.Name.ToString());
            return;
         }
         IMapItem? gunLoad = null;
         foreach (IMapItem mi in myGameInstance.GunLoads)
         {
            if (true == mi.Name.Contains("GunLoadInGun"))
               gunLoad = mi;
         }
         if (null == gunLoad)
         {
            gunLoad = new MapItem("GunLoadInGun", 1.0, "c17GunLoad", newT);
            myGameInstance.GunLoads.Add(gunLoad);
         }
         gunLoad.TerritoryCurrent = newT;
         double delta = gunLoad.Zoom * Utilities.theMapItemOffset;
         gunLoad.Location.X = newT.CenterPoint.X - delta;
         gunLoad.Location.Y = newT.CenterPoint.Y - delta;
         Logger.Log(LogEnum.LE_SHOW_MAPITEM_TANK, "MouseDownPolygonGunLoad(): gunLoad=" + gunLoad.Name + " loc=" + gunLoad.Location.ToString() + " t=" + newT.Name + " tLoc=" + newT.CenterPoint.ToString());
         GameAction outAction = GameAction.BattleRoundSequenceAmmoOrders;
         if (GamePhase.Preparations == myGameInstance.GamePhase)
            outAction = GameAction.PreparationsGunLoadSelect;
         else if (BattlePhase.BackToSpotting == myGameInstance.BattlePhase)
            outAction = GameAction.BattleRoundSequenceLoadMainGunEnd;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonCrewActions(object sender, MouseButtonEventArgs e)
      {
         GameAction outAction = GameAction.BattleRoundSequenceCrewOrders;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonAmmoActions(object sender, MouseButtonEventArgs e)
      {
         GameAction outAction = GameAction.BattleRoundSequenceAmmoOrders;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownEllipseSpottingLoader(object sender, MouseButtonEventArgs e)
      {
         Ellipse? ellipse = sender as Ellipse;
         if (null == ellipse)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseSpotting(): ellipse=null");
            return;
         }
         ITerritory? t = Territories.theTerritories.Find(ellipse.Name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseSpotting(): t=null for " + ellipse.Name.ToString());
            return;
         }
         IMapItem? spotter = myGameInstance.BattleStacks.FindMapItem("LoaderSpot");
         if (null == spotter)
            myGameInstance.BattleStacks.Add(new MapItem("LoaderSpot", 1.0, "c18LoaderSpot", t));
         else
            spotter.TerritoryCurrent = t;
         GameAction outAction = GameAction.PreparationsLoaderSpotSet;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownEllipseSpottingCommander(object sender, MouseButtonEventArgs e)
      {
         Ellipse? ellipse = sender as Ellipse;
         if (null == ellipse)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseSpotting(): ellipse=null");
            return;
         }
         ITerritory? t = Territories.theTerritories.Find(ellipse.Name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseSpotting(): t=null for " + ellipse.Name.ToString());
            return;
         }
         IMapItem? spotter = myGameInstance.BattleStacks.FindMapItem("CommanderSpot");
         if (null == spotter)
            myGameInstance.BattleStacks.Add(new MapItem("CommanderSpot", 1.0, "c19CommanderSpot", t));
         else
            spotter.TerritoryCurrent = t;
         GameAction outAction = GameAction.PreparationsCommanderSpotSet;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonShowEnemyStrength(object sender, MouseButtonEventArgs e)
      {
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonShowEnemyStrength(): clickedPolygon=null");
            return;
         }
         myGameInstance.EnemyStrengthCheckTerritory = Territories.theTerritories.Find(clickedPolygon.Name);
         if (null == myGameInstance.EnemyStrengthCheckTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonShowEnemyStrength(): t=null for " + clickedPolygon.Name.ToString());
            return;
         }
         GameAction outAction = GameAction.MovementEnemyStrengthCheckTerritory;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonArtillerySupport(object sender, MouseButtonEventArgs e)
      {
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonArtillerySupport(): clickedPolygon=null");
            return;
         }
         myGameInstance.ArtillerySupportCheck = Territories.theTerritories.Find(clickedPolygon.Name);
         if (null == myGameInstance.ArtillerySupportCheck)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonArtillerySupport(): t=null for " + clickedPolygon.Name.ToString());
            return;
         }
         GameAction outAction = GameAction.MovementArtillerySupportCheck;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonAirStrike(object sender, MouseButtonEventArgs e)
      {
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonAirStrike(): clickedPolygon=null");
            return;
         }
         myGameInstance.AirStrikeCheckTerritory = Territories.theTerritories.Find(clickedPolygon.Name);
         if (null == myGameInstance.AirStrikeCheckTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonAirStrike(): t=null for " + clickedPolygon.Name.ToString());
            return;
         }
         GameAction outAction = GameAction.MovementAirStrikeCheckTerritory;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonEnterArea(object sender, MouseButtonEventArgs e)
      {
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonEnterArea(): clickedPolygon=null");
            return;
         }
         string tName = clickedPolygon.Name;
         myGameInstance.EnteredArea = Territories.theTerritories.Find(tName);
         if (null == myGameInstance.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonEnterArea(): t=null for " + clickedPolygon.Name.ToString());
            return;
         }
         //-------------------------------------------------
         GameAction outAction = GameAction.MovementAdvanceFireChoice;
         IStack? stack = myGameInstance.MoveStacks.Find(myGameInstance.EnteredArea);
         if( null != stack )
         {
            foreach(IMapItem mi in stack.MapItems )
            {
               if( true == mi.Name.Contains("UsControl")) // If stack has a UsControl marker, resume with user choice
               {
                  outAction = GameAction.MovementEnterAreaUsControl;
                  break;
               }
            }
         }
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonPlaceAdvanceFire(object sender, MouseButtonEventArgs e)
      {
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonPlaceAdvanceFire(): clickedPolygon=null");
            return;
         }
         myGameInstance.AdvanceFire = Territories.theTerritories.Find(clickedPolygon.Name);
         if (null == myGameInstance.AdvanceFire)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonPlaceAdvanceFire(): t=null for " + clickedPolygon.Name.ToString());
            return;
         }
         foreach (IMapItem mi in myGameInstance.Targets)
         {
            foreach (Button b in myBattleButtons)
               b.BorderThickness = new Thickness(0);
         }
         GameAction outAction = GameAction.Error;
         if( GamePhase.BattleRoundSequence == myGameInstance.GamePhase )
            outAction = GameAction.BattleRoundSequencePlaceAdvanceFire;
         else if( GamePhase.Preparations == myGameInstance.GamePhase )
            outAction = GameAction.BattlePlaceAdvanceFire;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonFriendlyAdvance(object sender, MouseButtonEventArgs e)
      {
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonFriendlyAdvance(): clickedPolygon=null");
            return;
         }
         myGameInstance.FriendlyAdvance = Territories.theTerritories.Find(clickedPolygon.Name);
         if ( null == myGameInstance.FriendlyAdvance)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonFriendlyAdvance(): myGameInstance.FriendlyAdvance=null");
            return;
         }
         GameAction outAction = GameAction.BattleRoundSequenceFriendlyAdvanceSelected;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonEnemyAdvance(object sender, MouseButtonEventArgs e)
      {
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonEnemyAdvance(): clickedPolygon=null");
            return;
         }
         GameAction outAction = GameAction.BattleRoundSequenceEnemyAdvanceEnd;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void ClickButtonMapItem(object sender, RoutedEventArgs e)
      {
         if( null == myGameInstance )
         {
            Logger.Log(LogEnum.LE_ERROR, "ClickButtonMapItem(): myGameInstance=null");
            return;
         }
         Button? button = sender as Button;
         if (null == button)
         {
            Logger.Log(LogEnum.LE_ERROR, "ClickButtonMapItem(): button = null");
            return;
         }
         Logger.Log(LogEnum.LE_SHOW_ORDERS_MENU, "ClickButtonMapItem(): adding new button=" + button.Name );
         if ((true == button.Name.Contains("OpenHatch")) && ((true == myGameInstance.IsHatchesActive) || (BattlePhase.MarkCrewAction == myGameInstance.BattlePhase)))
         {
            string[] crewmembers = new string[4] { "Driver", "Assistant", "Commander", "Loader" };
            foreach (string s in crewmembers)
            {
               ICrewMember? crewMember = myGameInstance.GetCrewMember(s);
               if (null == crewMember)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ClickButtonMapItem(): s=" + s);
                  return;
               }
               //----------------------------------
               if (true == button.Name.Contains(s)) //Remove the open hatch
               {
                  crewMember.IsButtonedUp = true;
                  myGameInstance.Hatches.Remove(button.Name);
                  this.myTankButtons.Remove(button);
                  myCanvasTank.Children.Remove(button);
                  //----------------------------------
                  IMapItems removals = new MapItems();  // Remove Loader, Gunner, and Commander actions that require open hatch
                  foreach (IMapItem crewAction in myGameInstance.CrewActions)
                  {
                     if( false == myGameInstance.IsCrewActionPossibleButtonedUp(crewAction.Name))
                        removals.Add(crewAction);
                  }
                  foreach (IMapItem crewAction in removals)
                  {
                     myGameInstance.CrewActions.Remove(crewAction);
                     foreach (Button oldButton in myTankButtons)     // Remove corresponding Button
                     {
                        if (oldButton.Name == crewAction.Name)
                        {
                           myTankButtons.Remove(oldButton);
                           myCanvasTank.Children.Remove(oldButton);
                           break;
                        }
                     }
                  }
                  //----------------------------------
                  GameAction outAction = GameAction.BattleRoundSequenceCrewOrders;
                  if (GamePhase.Preparations == myGameInstance.GamePhase)
                     outAction = GameAction.PreparationsHatches;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
                  return;
               }
            }
         }
         //-----------------------------------------------
         else if ((true == button.Name.Contains("GunLoad")) && (BattlePhase.MarkAmmoReload == myGameInstance.BattlePhase) )
         {
            IMapItem? selectedMapItem = myGameInstance.GunLoads.Find(button.Name);
            if ( null == selectedMapItem)
            {
               Logger.Log(LogEnum.LE_ERROR, "ClickButtonMapItem(): GunLoad selectedMapItem=null for button.Name=" + button.Name);
               return;
            }
            myGameInstance.GunLoads.Rotate(1);
            int count = 0;
            foreach (IMapItem mi in myGameInstance.GunLoads) 
            {
               if( mi.TerritoryCurrent.Name == selectedMapItem.TerritoryCurrent.Name )
               {
                  mi.Location.X = selectedMapItem.TerritoryCurrent.CenterPoint.X + count * 3 - mi.Zoom * Utilities.theMapItemOffset;
                  mi.Location.Y = selectedMapItem.TerritoryCurrent.CenterPoint.Y + count * 3 - mi.Zoom * Utilities.theMapItemOffset;
                  foreach(Button b in myTankButtons )
                  {
                     if (b.Name == mi.Name)
                     {
                        Canvas.SetLeft(b, mi.Location.X);
                        Canvas.SetTop(b, mi.Location.Y);
                        Canvas.SetZIndex(b, count);
                     }
                  }
                  count++;
               }
            }
         }
         else if( BattlePhase.ConductCrewAction == myGameInstance.BattlePhase )
         {
            IMapItem? selectedMapItem = myGameInstance.BattleStacks.FindMapItem(button.Name);
            if (null == selectedMapItem)
            {
               Logger.Log(LogEnum.LE_ERROR, "ClickButtonMapItem(): selectedMapItem=null for button.Name=" + button.Name);
               return;
            }
            if( (CrewActionPhase.TankMainGunFire == myGameInstance.CrewActionPhase) || (CrewActionPhase.TankMgFire == myGameInstance.CrewActionPhase) )
            {
               if (true == myGameInstance.Targets.Contains(selectedMapItem))
               {
                  foreach (IMapItem mi in myGameInstance.Targets)
                  {
                     foreach (Button b in myBattleButtons)
                        b.BorderThickness = new Thickness(0);
                  }
                  GameAction outAction = GameAction.Error;
                  if (CrewActionPhase.TankMainGunFire == myGameInstance.CrewActionPhase)
                  {
                     myGameInstance.TargetMainGun = selectedMapItem;
                     outAction = GameAction.BattleRoundSequenceShermanFiringMainGun;
                  }
                  else
                  {
                     myGameInstance.TargetMg= selectedMapItem;
                     outAction = GameAction.BattleRoundSequenceShermanFiringMachineGun;
                  }
                  selectedMapItem = null;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
            }
         }
         e.Handled = true;
      }
      private void MouseEnterMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
      }
      private void MouseLeaveMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
      }
      private void MouseDownPolygonTravel(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point canvasPoint = e.GetPosition(myCanvasMain);
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonTravel(): clickedPolygon=null");
            return;
         }
         myTerritorySelected = Territories.theTerritories.Find(Utilities.RemoveSpaces(clickedPolygon.Name));
         if (null == myTerritorySelected)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonTravel(): selectedTerritory=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         GameAction outAction = GameAction.Error;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseLeftButtonDownMarquee(object sender, MouseEventArgs e)
      {
         myStoryboard.Pause(this);
      }
      private void MouseLeftButtonUpMarquee(object send, MouseEventArgs e)
      {
         myStoryboard.Resume(this);
      }
      private void MouseRightButtonDownMarquee(object send, MouseEventArgs e)
      {
         if (2.5 < mySpeedRatioMarquee)
            mySpeedRatioMarquee = 0.25;
         else if ((1.8 < mySpeedRatioMarquee) && (mySpeedRatioMarquee < 2.2))
            mySpeedRatioMarquee = 3.0;
         else if ((0.8 < mySpeedRatioMarquee) && (mySpeedRatioMarquee < 1.2))
            mySpeedRatioMarquee = 2.0;
         else if ((0.3 < mySpeedRatioMarquee) && (mySpeedRatioMarquee < 0.6))
            mySpeedRatioMarquee = 1.0;
         else
            mySpeedRatioMarquee = 0.5;
         myStoryboard.SetSpeedRatio(this, mySpeedRatioMarquee);
      }
      public void MenuItemCrewActionClick(object sender, RoutedEventArgs e)
      {
         MenuItem? menuitem = sender as MenuItem;
         if( null == menuitem)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClick(): menuitem=null");
            return;
         }
         //--------------------------------------
         IMapItem? mi = null;
         string[] aStringArray1 = menuitem.Name.Split(new char[] { '_' });
         if(aStringArray1.Length < 2)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClick(): underscore not found in " + menuitem.Name + " len=" + aStringArray1.Length);
            return;
         }
         string sCrewMemberRole = aStringArray1[0];
         //--------------------------------------
         foreach(IMapItem crewAction in myGameInstance.CrewActions) // get rid of existing crew action for this crew member
         {
            if( true == crewAction.Name.Contains(sCrewMemberRole)) 
            {
               myGameInstance.CrewActions.Remove(crewAction); // Remove existing Crew Action
               foreach(Button oldButton in myTankButtons)     // Remove existing Button
               {
                  if( oldButton.Name == crewAction.Name )
                  {
                     myTankButtons.Remove(oldButton);
                     myCanvasTank.Children.Remove(oldButton);
                     break;
                  }
               }
               break;
            }
         }
         //--------------------------------------
         string tName = aStringArray1[0] + "Action";
         ITerritory? t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClick(): t=null for " + tName);
            return;
         }
         //--------------------------------------
         ContextMenu? menu = null;  // Add in new button
         switch (menuitem.Name)
         {
            case "Loader_Load":
               mi = new MapItem(menuitem.Name, 1.0, "c54LLoad", t);
               menu = myContextMenuCrewActions["Loader"];
               break;
            case "Loader_RepairMainGun":
               mi = new MapItem(menuitem.Name, 1.0, "c55LRepairMainGun", t);
               menu = myContextMenuCrewActions["Loader"];
               break;
            case "Loader_RepairCoaxialMg":
               mi = new MapItem(menuitem.Name, 1.0, "c56LRepairCoaxialMg", t);
               menu = myContextMenuCrewActions["Loader"];
               break;
            case "Loader_FireMortar":
               mi = new MapItem(menuitem.Name, 1.0, "c58LFireMortar", t);
               menu = myContextMenuCrewActions["Loader"];
               break;
            case "Loader_ChangeGunLoad":
               mi = new MapItem(menuitem.Name, 1.0, "c59LChangeGunLoad", t);
               menu = myContextMenuCrewActions["Loader"];
               break;
            case "Loader_RestockReadyRack":
               mi = new MapItem(menuitem.Name, 1.0, "c60LRestockReadyRack", t);
               menu = myContextMenuCrewActions["Loader"];
               break;
            case "Loader_RepairScope":
               mi = new MapItem(menuitem.Name, 1.0, "c73ReplacePeriscope", t);
               menu = myContextMenuCrewActions["Loader"];
               break;
            case "Loader_FireAaMg":
               mi = new MapItem(menuitem.Name, 1.0, "c71FireAaMg", t);
               menu = myContextMenuCrewActions["Loader"];
               break;
            case "Loader_RepairAaMg":
               mi = new MapItem(menuitem.Name, 1.0, "c72RepairAaMg", t);
               menu = myContextMenuCrewActions["Loader"];
               break;
            case "Loader_FireSubMg":
               mi = new MapItem(menuitem.Name, 1.0, "c74FireSubMg", t);
               menu = myContextMenuCrewActions["Loader"];
               break;
            case "Driver_Stop":
               mi = new MapItem(menuitem.Name, 1.0, "c61DStop", t);
               menu = myContextMenuCrewActions["Driver"];
               break;
            case "Driver_Forward":
               mi = new MapItem(menuitem.Name, 1.0, "c62DForward", t);
               menu = myContextMenuCrewActions["Driver"];
               break;
            case "Driver_ForwardToHullDown":
               mi = new MapItem(menuitem.Name, 1.0, "c63DForwardToHullDown", t);
               menu = myContextMenuCrewActions["Driver"];
               break;
            case "Driver_Reverse":
               mi = new MapItem(menuitem.Name, 1.0, "c64DReverse", t);
               menu = myContextMenuCrewActions["Driver"];
               break;
            case "Driver_ReverseToHullDown":
               mi = new MapItem(menuitem.Name, 1.0, "c65DReverseToHullDown", t);
               menu = myContextMenuCrewActions["Driver"];
               break;
            case "Driver_PivotTank":
               mi = new MapItem(menuitem.Name, 1.0, "c66DPivotTank", t);
               menu = myContextMenuCrewActions["Driver"];
               break;
            case "Driver_RepairScope":
               mi = new MapItem(menuitem.Name, 1.0, "c73ReplacePeriscope", t);
               menu = myContextMenuCrewActions["Driver"];
               break;
            case "Gunner_FireMainGun":
               mi = new MapItem(menuitem.Name, 1.0, "c50GFireMainGun", t);
               menu = myContextMenuCrewActions["Gunner"];
               break;
            case "Gunner_FireCoaxialMg":
               mi = new MapItem(menuitem.Name, 1.0, "c51GFireCoaxialMg", t);
               menu = myContextMenuCrewActions["Gunner"];
               break;
            case "Gunner_RotateTurret":
               mi = new MapItem(menuitem.Name, 1.0, "c52GRotateTurret", t);
               menu = myContextMenuCrewActions["Gunner"];
               break;
            case "Gunner_RotateFireMainGun":
               mi = new MapItem(menuitem.Name, 1.0, "c53GRotateTurretFireMainGun", t);
               menu = myContextMenuCrewActions["Gunner"];
               break;
            case "Gunner_RepairMainGun":
               mi = new MapItem(menuitem.Name, 1.0, "c57GRepairMainGun", t);
               menu = myContextMenuCrewActions["Gunner"];
               break;
            case "Gunner_RepairScope":
               mi = new MapItem(menuitem.Name, 1.0, "c73ReplacePeriscope", t);
               menu = myContextMenuCrewActions["Gunner"];
               break;
            case "Gunner_ThrowGrenade":
               mi = new MapItem(menuitem.Name, 1.0, "c70ThrowSmokeGrenade", t);
               menu = myContextMenuCrewActions["Gunner"];
               break;
            case "Assistant_FireBowMg":
               mi = new MapItem(menuitem.Name, 1.0, "c67AFireBowMg", t);
               menu = myContextMenuCrewActions["Assistant"];
               break;
            case "Assistant_RepairBowMg":
               mi = new MapItem(menuitem.Name, 1.0, "c68ARepairBowMg", t);
               menu = myContextMenuCrewActions["Assistant"];
               break;
            case "Assistant_PassAmmo":
               mi = new MapItem(menuitem.Name, 1.0, "c69APassAmmo", t);
               menu = myContextMenuCrewActions["Assistant"];
               break;
            case "Assistant_RepairScope":
               mi = new MapItem(menuitem.Name, 1.0, "c73ReplacePeriscope", t);
               menu = myContextMenuCrewActions["Assistant"];
               break;
            case "Commander_Move":
               mi = new MapItem(menuitem.Name, 1.0, "c48CDirectMove", t);
               menu = myContextMenuCrewActions["Commander"];
               break;
            case "Commander_MainGunFire":
               mi = new MapItem(menuitem.Name, 1.0, "c49CDirectMainGunFire", t);
               menu = myContextMenuCrewActions["Commander"];
               break;
            case "Commander_MGFire":
               mi = new MapItem(menuitem.Name, 1.0, "c49CDirectMGFire", t);
               menu = myContextMenuCrewActions["Commander"];
               break;
            case "Commander_RepairScope":
               mi = new MapItem(menuitem.Name, 1.0, "c73ReplacePeriscope", t);
               menu = myContextMenuCrewActions["Commander"];
               break;
            case "Commander_FireAaMg":
               mi = new MapItem(menuitem.Name, 1.0, "c71FireAaMg", t);
               menu = myContextMenuCrewActions["Commander"];
               break;
            case "Commander_RepairAaMg":
               mi = new MapItem(menuitem.Name, 1.0, "c72RepairAaMg", t);
               menu = myContextMenuCrewActions["Commander"];
               break;
            case "Commander_FireSubMg":
               mi = new MapItem(menuitem.Name, 1.0, "c74FireSubMg", t);
               menu = myContextMenuCrewActions["Commander"];
               break;
            case "Commander_ThrowGrenade":
               mi = new MapItem(menuitem.Name, 1.0, "c70ThrowSmokeGrenade", t);
               menu = myContextMenuCrewActions["Commander"];
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClick(): reached default name=" + menuitem.Name);
               return;
         }
         if( null == mi )
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClick(): mi=null");
            return;
         }
         myGameInstance.CrewActions.Add(mi);
         //--------------------------------------
         if( null == menu )
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClick(): menu=null");
            return;
         }
         Logger.Log(LogEnum.LE_SHOW_ORDERS_MENU, "MenuItemCrewActionClick(): adding new button=" + menuitem.Name + " for sCrewMemberRole=" + sCrewMemberRole);
         System.Windows.Controls.Button newButton = new Button { ContextMenu = menu, Name = menuitem.Name, Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
         MapItem.SetButtonContent(newButton, mi, true, false); // This sets the image as the button's content
         myTankButtons.Add(newButton);
         myCanvasTank.Children.Add(newButton);
         Canvas.SetLeft(newButton, mi.Location.X);
         Canvas.SetTop(newButton, mi.Location.Y);
         Canvas.SetZIndex(newButton, 900);
         //--------------------------------------
         GameAction outaction = GameAction.BattleRoundSequenceCrewOrders;
         myGameEngine.PerformAction(ref myGameInstance, ref outaction, 0);
      }
      public void MenuItemAmmoReloadClick(object sender, RoutedEventArgs e)
      {
         MenuItem? menuitem = sender as MenuItem;
         if (null == menuitem)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemAmmoReloadClick(): menuitem=null");
            return;
         }
         //--------------------------------------
         string[] aStringArray = menuitem.Name.Split(new char[] { '_' });
         if (aStringArray.Length < 2)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemAmmoReloadClick(): underscore not found in " + menuitem.Name + " len=" + aStringArray.Length);
            return;
         }
         string tName = aStringArray[0];
         string order = aStringArray[1];
         //--------------------------------------
         ITerritory? t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemAmmoReloadClick(): t=null for " + tName);
            return;
         }
         ContextMenu menu = myContextMenuGunLoadActions[tName];
         //--------------------------------------
         IMapItem? gunLoad = null;
         foreach (IMapItem mi in myGameInstance.GunLoads)
         {
            if (true == mi.Name.Contains("GunLoadInGun"))
               gunLoad = mi;
         }
         //--------------------------------------
         List<Button> removalButtons = new List<Button>();
         foreach (IMapItem mapItem in myGameInstance.GunLoads) // get rid of existing gunloads for this crew member
         {
            int i = 1;
            StringBuilder sb0 = new StringBuilder("MenuItemAmmoReloadClick(): checking mi=");
            sb0.Append(mapItem.Name);
            sb0.Append(" myTankButtons=[");
            foreach (Button oldButton in myTankButtons)    // Remove existing Button
            {
               sb0.Append(oldButton.Name);
               if (oldButton.Name == mapItem.Name)
               {
                  sb0.Append("**REMOVE**");
                  removalButtons.Add(oldButton);  // adding button to remove
               }
               if (myTankButtons.Count != i++ )
                  sb0.Append(",");
            }
            sb0.Append("]");
            Logger.Log(LogEnum.LE_SHOW_AMMMO_MENU, sb0.ToString());
         }
         foreach(Button b in removalButtons)
         {
            myTankButtons.Remove(b);
            myCanvasTank.Children.Remove(b);
         }
         myGameInstance.GunLoads.Clear();
         //--------------------------------------
         int count = 0;
         SolidColorBrush brush = new SolidColorBrush(Colors.Transparent);
         Thickness thickness = new Thickness(0);
         double size = Utilities.theMapItemSize;
         System.Windows.Controls.Button? newButton = null;
         //--------------------------------------
         if ("ReadyRackAmmoReload" == order) // either this button or AmmoReload button
         {
            IMapItem readyRackReLoad = new MapItem(menuitem.Name, 1.0, "c30ReadyRackAmmoReload", t);
            myGameInstance.GunLoads.Add(readyRackReLoad);
            Logger.Log(LogEnum.LE_SHOW_AMMMO_MENU, "MenuItemAmmoReloadClick(): adding new button=" + menuitem.Name);
            newButton = new Button { ContextMenu = menu, Name = menuitem.Name, Width = size, Height = size, BorderThickness = thickness, Background = brush, Foreground = brush };
            newButton.Click += ClickButtonMapItem;
            MapItem.SetButtonContent(newButton, readyRackReLoad, true, false); // This sets the image as the button's content
            myTankButtons.Add(newButton);
            myCanvasTank.Children.Add(newButton);
            Canvas.SetLeft(newButton, readyRackReLoad.Location.X);
            Canvas.SetTop(newButton, readyRackReLoad.Location.Y);
            Canvas.SetZIndex(newButton, 900);
            if (null != gunLoad)
            {
               if (tName == gunLoad.TerritoryCurrent.Name)
                  count++;
            }
         }
         else
         {
            string name = tName + "AmmoReload";
            IMapItem ammoLoad = new MapItem(name, 1.0, "c29AmmoReload", t);
            double ammoLoadLocationOffset = count * 3 - (ammoLoad.Zoom * Utilities.theMapItemOffset);
            ammoLoad.Location.X = t.CenterPoint.X + ammoLoadLocationOffset;
            ammoLoad.Location.Y = t.CenterPoint.Y + ammoLoadLocationOffset;
            newButton = new Button { ContextMenu = menu, Name = ammoLoad.Name, Width = size, Height = size, BorderThickness = thickness, Background = brush, Foreground = brush };
            newButton.Click += ClickButtonMapItem;
            MapItem.SetButtonContent(newButton, ammoLoad, true, false); // This sets the image as the button's content
            myTankButtons.Add(newButton);
            myCanvasTank.Children.Add(newButton);
            Canvas.SetLeft(newButton, ammoLoad.Location.X);
            Canvas.SetTop(newButton, ammoLoad.Location.Y);
            Canvas.SetZIndex(newButton, 900);
            myGameInstance.GunLoads.Insert(0, ammoLoad);
            Logger.Log(LogEnum.LE_SHOW_AMMMO_MENU, "MenuItemAmmoReloadClick(): adding new button=" + menuitem.Name);
            if( null != gunLoad)
            {
               if (tName == gunLoad.TerritoryCurrent.Name)
                  count++;
            }
         }
         //--------------------------------------
         if( null != gunLoad )
         {
            double gunLoadLocationOffset = count * 3 - (gunLoad.Zoom * Utilities.theMapItemOffset);
            myGameInstance.GunLoads.Insert(0, gunLoad); // GunLoad must be first entry in MapItems
            gunLoad.Location.X = gunLoad.TerritoryCurrent.CenterPoint.X + gunLoadLocationOffset;
            gunLoad.Location.Y = gunLoad.TerritoryCurrent.CenterPoint.Y + gunLoadLocationOffset;
            Logger.Log(LogEnum.LE_SHOW_AMMMO_MENU, "MenuItemAmmoReloadClick(): adding new button=" + gunLoad.Name);
            newButton = new Button { ContextMenu = myContextMenuGunLoadActions[gunLoad.TerritoryCurrent.Name], Name = gunLoad.Name, Width = size, Height = size, BorderThickness = thickness, Background = brush, Foreground = brush };
            newButton.Click += ClickButtonMapItem;
            MapItem.SetButtonContent(newButton, gunLoad, true, false); // This sets the image as the button's content
            myTankButtons.Add(newButton);
            myCanvasTank.Children.Add(newButton);
            Canvas.SetLeft(newButton, gunLoad.Location.X);
            Canvas.SetTop(newButton, gunLoad.Location.Y);
         }
         //--------------------------------------
         GameAction outaction = GameAction.BattleRoundSequenceAmmoOrders;
         myGameEngine.PerformAction(ref myGameInstance, ref outaction, 0);
      }
      //-------------GameViewerWindow---------------------------------
      private void ContentRenderedGameViewerWindow(object sender, EventArgs e)
      {
         double mapPanelHeight = myDockPanelTop.ActualHeight - myMainMenu.ActualHeight - myStatusBar.ActualHeight;
         myDockPanelInside.Height = mapPanelHeight;
         myDockPanelControls.Height = mapPanelHeight;
         //-----------------------------------------------------
         myScrollViewerTextBlock.Height = mapPanelHeight - myCanvasTank.ActualHeight - 5;
         myTextBlockDisplay.Height = mapPanelHeight - myCanvasTank.ActualHeight;
         //-----------------------------------------------------
         double mapPanelWidth = myDockPanelTop.ActualWidth - myDockPanelControls.ActualWidth - System.Windows.SystemParameters.VerticalScrollBarWidth;
         myScrollViewerMap.Width = mapPanelWidth;
         myScrollViewerMap.Height = mapPanelHeight;
      }
      private void SizeChangedGameViewerWindow(object sender, SizeChangedEventArgs e)
      {
         double mapPanelHeight = myDockPanelTop.ActualHeight - myMainMenu.ActualHeight - myStatusBar.ActualHeight;
         myDockPanelInside.Height = mapPanelHeight;
         myDockPanelControls.Height = mapPanelHeight;
         //-----------------------------------------------------
         myScrollViewerTextBlock.Height = mapPanelHeight - myCanvasTank.ActualHeight - 5;
         myTextBlockDisplay.Height = mapPanelHeight - myCanvasTank.ActualHeight;
         //myTextBlockDisplay.Width = myScrollViewerTextBlock.ActualWidth;
         //Visibility v = myScrollViewerTextBlock.ComputedVerticalScrollBarVisibility;
         //if (v == Visibility.Visible)
         //   myTextBlockDisplay.Width -= System.Windows.SystemParameters.VerticalScrollBarWidth;
         //-----------------------------------------------------
         double mapPanelWidth = myDockPanelTop.ActualWidth - myDockPanelControls.ActualWidth - System.Windows.SystemParameters.VerticalScrollBarWidth;
         myScrollViewerMap.Width = mapPanelWidth;
         myScrollViewerMap.Height = mapPanelHeight;
      }
      private void ClosedGameViewerWindow(object sender, EventArgs e)
      {
         Application app = Application.Current;
         app.Shutdown();
      }
      protected override void OnSourceInitialized(EventArgs e)
      {
         base.OnSourceInitialized(e);
         try
         {
            // Load window placement details for previous application session from application settings
            // Note - if window was closed on a monitor that is now disconnected from the computer,
            //        SetWindowPlacement places the window onto a visible monitor.
            if (false == String.IsNullOrEmpty(Settings.Default.WindowPlacement))
            {
               WindowPlacement wp = Utilities.Deserialize<WindowPlacement>(Settings.Default.WindowPlacement);
               wp.length = Marshal.SizeOf(typeof(WindowPlacement));
               wp.flags = 0;
               wp.showCmd = (wp.showCmd == SwShowminimized ? SwShownormal : wp.showCmd);
               var hwnd = new WindowInteropHelper(this).Handle;
               if (false == SetWindowPlacement(hwnd, ref wp))
                  Logger.Log(LogEnum.LE_ERROR, "SetWindowPlacement() returned false");
            }
            if (0.0 != Settings.Default.ScrollViewerHeight)
               myScrollViewerMap.Height = Settings.Default.ScrollViewerHeight;
            if (0.0 != Settings.Default.ScrollViewerWidth)
               myScrollViewerMap.Width = Settings.Default.ScrollViewerWidth;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "OnSourceInitialized() e=" + ex.ToString());
         }
         return;
      }
      protected override void OnClosing(CancelEventArgs e) //  // WARNING - Not fired when Application.SessionEnding is fired
      {
         base.OnClosing(e);
         SaveDefaultsToSettings();
         GameLoadMgr loadMgr = new GameLoadMgr();
         if (false == loadMgr.SaveGameToFile(myGameInstance))
            Logger.Log(LogEnum.LE_ERROR, "OnClosing(): SaveGameToFile() returned false");
      }
      //-------------CONTROLLER HELPER FUNCTIONS---------------------------------
      private void SaveDefaultsToSettings()
      {
         //WindowPlacement wp; // Persist window placement details to application settings
         //var hwnd = new WindowInteropHelper(this).Handle;
         //if (false == GetWindowPlacement(hwnd, out wp))
         //   Logger.Log(LogEnum.LE_ERROR, "OnClosing(): GetWindowPlacement() returned false");
         //string sWinPlace = Utilities.Serialize<WindowPlacement>(wp);
         //Settings.Default.WindowPlacement = sWinPlace;
         //-------------------------------------------
         Settings.Default.ZoomCanvas = Utilities.ZoomCanvas;
         //-------------------------------------------
         Settings.Default.ScrollViewerHeight = myScrollViewerMap.Height;
         Settings.Default.ScrollViewerWidth = myScrollViewerMap.Width;
         //-------------------------------------------
         Settings.Default.GameDirectoryName = GameLoadMgr.theGamesDirectory;
         //-------------------------------------------
         //string sOptions = Utilities.Serialize<Options>(myGameInstance.Options);
         //Settings.Default.GameOptions = sOptions;
         //-------------------------------------------
         Settings.Default.Save();
      }
      private bool AddHotKeys(MainMenuViewer mmv)
      {
         try
         {
            RoutedCommand command = new RoutedCommand();
            KeyGesture keyGesture = new KeyGesture(Key.N, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemNew_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.O, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemFileOpen_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.C, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemClose_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.S, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemSaveAs_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.U, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBinding undoCmdBinding = new CommandBinding(command, mmv.MenuItemEditRecover_Click, mmv.MenuItemEditRecover_ClickCanExecute);
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemEditUndo_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.R, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBinding recoverCmdBinding = new CommandBinding(command, mmv.MenuItemEditRecover_Click, mmv.MenuItemEditRecover_ClickCanExecute);
            CommandBindings.Add(recoverCmdBinding);
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemFileOptions_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.P, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.R, ModifierKeys.Control | ModifierKeys.Shift);
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemViewCombatCalendar));
            InputBindings.Add(new KeyBinding(command, keyGesture));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.A, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemViewAfterActionReport));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.M, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemViewMoveDiagram));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.G, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemViewFeats_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.F1, ModifierKeys.None);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemHelpRules_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.F2, ModifierKeys.None);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemHelpEvents_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.F3, ModifierKeys.None);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemHelpTables_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.F4, ModifierKeys.None);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemHelpReportError_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.A, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemHelpAbout_Click));
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddHotKeys(): ex=" + ex.ToString());
            return false;
         }
         return true;
      }
      private IMapPoint GetCanvasCenter(ScrollViewer scrollViewer, Canvas canvas)
      {
         double x = 0.0;
         if (canvas.ActualWidth < scrollViewer.ActualWidth / Utilities.ZoomCanvas)
            x = canvas.ActualWidth / 2 + scrollViewer.HorizontalOffset;
         else
            x = scrollViewer.ActualWidth / (2 * Utilities.ZoomCanvas) + scrollViewer.HorizontalOffset / Utilities.ZoomCanvas;
         double y = 0.0;
         if (canvas.ActualHeight < scrollViewer.ActualHeight / Utilities.ZoomCanvas)
            y = canvas.ActualHeight / 2 + scrollViewer.VerticalOffset;
         else
            y = scrollViewer.ActualHeight / (2 * Utilities.ZoomCanvas) + scrollViewer.VerticalOffset / Utilities.ZoomCanvas;
         IMapPoint mp = (IMapPoint)new MapPoint(x, y);
         return mp;
      }
      //-----------------------------------------------------------------------
      #region Win32 API declarations to set and get window placement
      [DllImport("user32.dll")]
      private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacement lpwndpl);
      [DllImport("user32.dll")]
      private static extern bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement lpwndpl);
      private const int SwShownormal = 1;
      private const int SwShowminimized = 2;
      #endregion
   }
   public static class MyGameViewerWindowExtensions
   {
      public static Button? Find(this IList<Button> list, string name)
      {
         IEnumerable<Button> results = from button in list
                                       where button.Name == name
                                       select button;
         if (0 < results.Count())
            return results.First();
         else
            return null;
      }
   }
}
