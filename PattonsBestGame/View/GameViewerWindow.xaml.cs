using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging; // for BitmapImage
using System.Windows.Shapes;
using System.Xml;
using WpfAnimatedGif;
using Button = System.Windows.Controls.Button;
using MenuItem = System.Windows.Controls.MenuItem;
using Point = System.Windows.Point;
using Label = System.Windows.Controls.Label;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Pattons_Best
{
   public partial class GameViewerWindow : System.Windows.Window, IView
   {
      private static Mutex theSaveSettingsMutex = new Mutex();
      #region Win32 API declarations to set and get window placement
         [DllImport("user32.dll")]
         private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacement lpwndpl);
         [DllImport("user32.dll")]
         private static extern bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement lpwndpl);
         private const int SwShownormal = 1;
         private const int SwShowminimized = 2;
      #endregion
      private const Double MARQUEE_SCROLL_ANMINATION_TIME = 30.0;
      private const Double ELLIPSE_DIAMETER = 40.0;
      private const Double ELLIPSE_RADIUS = ELLIPSE_DIAMETER / 2.0;
      private Double theOldXAfterAnimation = 0.0;
      private Double theOldYAfterAnimation = 0.0;
      public bool CtorError { get; } = false;
      //-------------------------------------------
      private static Double theEllipseDiameter = 14.0;
      private static Double theEllipseOffset = theEllipseDiameter / 2.0;
      //---------------------------------------------------------------------
      private readonly IGameEngine myGameEngine;
      private IGameInstance myGameInstance;
      //---------------------------------------------------------------------
      private IDieRoller? myDieRoller = null;
      private EventViewer? myEventViewer = null;
      private MainMenuViewer? myMainMenuViewer = null;
      //---------------------------------------------------------------------
      private readonly List<Button> myMoveButtons = new List<Button>();
      private readonly List<Button> myBattleButtons = new List<Button>();
      public readonly List<Button> myTankButtons = new List<Button>();
      private bool myIsInitialAmmoReloadButtonCreated = false;
      private int myTankCardNum = 1;
      private Dictionary<string, Polyline> myRoads = new Dictionary<string, Polyline>();
      //---------------------------------------------------------------------
      private int myBrushIndex = 0;
      private readonly List<Brush> myBrushes = new List<Brush>();
      private readonly SolidColorBrush mySolidColorBrushClear = new SolidColorBrush() { Color = Color.FromArgb(0, 0, 1, 0) };
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly SolidColorBrush mySolidColorBrushGreen = new SolidColorBrush() { Color = Colors.Green };
      private readonly SolidColorBrush mySolidColorBrushRed = new SolidColorBrush() { Color = Colors.Red };
      private readonly SolidColorBrush mySolidColorBrushGold = new SolidColorBrush() { Color = Colors.Gold };
      private readonly SolidColorBrush mySolidColorBrushSteelBlue = new SolidColorBrush { Color = Colors.SteelBlue };
      private readonly SolidColorBrush mySolidColorBrushWhite = new SolidColorBrush { Color = Colors.White };
      private readonly SolidColorBrush mySolidColorBrushLawnGreen = new SolidColorBrush { Color = Colors.LawnGreen };
      private readonly FontFamily myFontFam = new FontFamily("Tahofma");
      //---------------------------------------------------------------------
      private Button? myDraggedButton = null;
      private System.Windows.Input.Cursor? myTargetCursor = null;
      private EllipseDisplayDialog? myEllipseDisplayDialog = null;
      private double myPreviousScrollHeight = 0.0;
      private double myPreviousScrollWidth = 0.0;
      //---------------------------------------------------------------------
      private readonly SplashDialog mySplashScreen;
      private Dictionary<string, ContextMenu> myContextMenuCrewActions = new Dictionary<string, ContextMenu>();
      private Dictionary<string, ContextMenu> myContextMenuGunLoadActions = new Dictionary<string, ContextMenu>();
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private readonly List<Rectangle> myRectangles = new List<Rectangle>();
      private readonly List<Polygon> myPolygons = new List<Polygon>();
      private readonly List<Ellipse> myEllipses = new List<Ellipse>();
      private Rectangle? myRectangleMoving = null;               // Not used - Rectangle that is moving with button
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
         string appendText = "";
         if (9 < myTankCardNum)
            appendText = myTankCardNum.ToString();
         else
            appendText = "0" + myTankCardNum.ToString();
         Image imageMat = new Image() { Name = "TankMat", Width = 600, Height = 500, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("m" + appendText) };
         myCanvasTank.Children.Add(imageMat); // TankMat changes as get new tanks
         Canvas.SetLeft(imageMat, 0);
         Canvas.SetTop(imageMat, 0);
         //---------------------------------------------------------------
         myGameEngine = ge;
         myGameInstance = gi;
         gi.GamePhase = GamePhase.GameSetup;
         myMainMenuViewer = new MainMenuViewer(myMainMenu, ge, gi);
         //---------------------------------------------------------------
         if (false == AddHotKeys(myMainMenuViewer))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): AddHotKeys() returned false");
            CtorError = true;
            return;
         }
         //---------------------------------------------------------------
         if (false == String.IsNullOrEmpty(Properties.Settings.Default.GameDirectoryName))
            GameLoadMgr.theGamesDirectory = Properties.Settings.Default.GameDirectoryName; // remember the game directory name
         //---------------------------------------------------------------
         if ( false == DeserializeOptions(Properties.Settings.Default.GameOptions, gi.Options))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): DeserializeOptions() returned false");
            CtorError = true;
            return;
         }
         myMainMenuViewer.NewGameOptions = gi.Options;
         Logger.Log(LogEnum.LE_VIEW_SHOW_OPTIONS, "GameViewerWindow(): Options=" + gi.Options.ToString());
         //---------------------------------------------------------------
         if (false == DeserializeGameFeats(GameEngine.theInGameFeats))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): DeserializeGameFeats() returned false");
            CtorError = true;
            return;        
         }
         GameEngine.theStartingFeats = GameEngine.theInGameFeats.Clone(); // need to know difference between starting feats and feats that happen in this game
         GameEngine.theStartingFeats.SetGameFeatThreshold();
         Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "GameViewerWindow():\n  feats=" + GameEngine.theInGameFeats.ToString() + "\n Sfeats=" + GameEngine.theStartingFeats.ToString());
         //---------------------------------------------------------------
         if (false == DeserializeGameStatistics(GameEngine.theSingleDayStatistics, "stat0"))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): Deserialize_GameStatistics(theSingleDayStatistics) returned false");
            CtorError = true;
            return;
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "GameViewerWindow():\n  single day stats=" + GameEngine.theSingleDayStatistics.ToString());
         if (false == DeserializeGameStatistics(GameEngine.theCampaignStatistics, "stat1"))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): Deserialize_GameStatistics(theCampaignStatistics) returned false");
            CtorError = true;
            return;
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "GameViewerWindow():\n  campaign stats=" + GameEngine.theCampaignStatistics.ToString());
         if (false == DeserializeGameStatistics(GameEngine.theTotalStatistics, "stat2"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Update_CanvasShowStatsAdds(): Deserialize_GameStatistics(theTotalStatistics) returned false");
            CtorError = true;
            return;
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "GameViewerWindow():\n  total stats=" + GameEngine.theTotalStatistics.ToString());
         //---------------------------------------------------------------
         if (false == DeserializeRoadsFromXml())
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): DeserializeRoadsFromXml() returned false");
            CtorError = true;
            return;
         }
         //---------------------------------------------------------------
         Utilities.ZoomCanvas = Properties.Settings.Default.ZoomCanvas;
         myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas); // Constructor - revert to save zoom
         StatusBarViewer sbv = new StatusBarViewer(myStatusBar, ge, gi, myCanvasMain);
         //---------------------------------------------------------------
         SetDisplayIconForUninstall(); // This is specialized code to add to Windows Registry the icon for uninstall
         //---------------------------------------------------------------
         Utilities.theBrushBlood.Color = Color.FromArgb(0xFF, 0xA4, 0x07, 0x07);
         Utilities.theBrushRegion.Color = Color.FromArgb(0x7F, 0x11, 0x09, 0xBB); // nearly transparent but slightly colored
         Utilities.theBrushRegionClear.Color = Color.FromArgb(0, 0, 0x01, 0x0); // nearly transparent but slightly colored
         Utilities.theBrushControlButton.Color = Color.FromArgb(0xFF, 0x43, 0x33, 0xFF); // menu blue
         Utilities.theBrushScrollViewerActive.Color = Color.FromArgb(0xFF, 0xB9, 0xEA, 0x9E); // light green 
         //Utilities.theBrushScrollViewerInActive.Color = Color.FromArgb(0x17, 0x00, 0x00, 0x00); // gray
         Utilities.theBrushScrollViewerInActive.Color = Colors.LightGray;
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
         myDieRoller = new DieRoller(myCanvasMain, CloseSplashScreen); // Close the splash screen when die resources are loaded
         if (true == myDieRoller.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): myDieRoller.CtorError=true");
            CtorError = true;
            return;
         }
         //----------------------------------------------------------------
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
         ge.RegisterForUpdates(sbv);
         ge.RegisterForUpdates(myEventViewer); // needs to be last so UploadGameView
         ge.RegisterForUpdates(this); // needs to be last so that canvas updates after all actions taken
         Logger.Log(LogEnum.LE_GAME_INIT, "GameViewerWindow(): \nzoomCanvas=" + Properties.Settings.Default.ZoomCanvas.ToString() + "\nwp=" + Properties.Settings.Default.WindowPlacement + "\noptions=" + Properties.Settings.Default.GameOptions);
#if UT1
            if (false == ge.CreateUnitTests(gi, myDockPanelTop, this, myEventViewer, myDieRoller, civ))
            {
               Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): Create_UnitTests() returned false");
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
         else if ((GameAction.UpdateLoadingGame == action) || (GameAction.UpdateNewGame == action) || (GameAction.RemoveSplashScreen == action) )
         {
            if( false == UpdateViewForNewGame(ref gi, action)) // This calls PerformAction() to get to proper event
               Logger.Log(LogEnum.LE_ERROR, "Update_View(): UpdateViewForNewGame() returned false");
            return;
         }
         else if( GameAction.UpdateUndo == action)
         {
            if (false == UpdateViewUndo(ref gi)) // This calls PerformAction() to get to proper event
               Logger.Log(LogEnum.LE_ERROR, "Update_View(): UpdateViewUndo() returned false");
            return;
         }
         //-------------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Update_View(): lastReport=null");
            return;
         }
         this.Title = UpdateViewTitle(gi, lastReport);
         //-------------------------------------------------------
         if( myTankCardNum != lastReport.TankCardNum) // switch out tank mat if do not match
         {
            myTankCardNum = lastReport.TankCardNum;
            List<UIElement> elements = new List<UIElement>();
            foreach (UIElement ui in myCanvasTank.Children) // Clean the Canvas of all marks
            {
               if (ui is Image img)
               {
                  if (true == img.Name.Contains("TankMat"))
                     elements.Add(img);
               }
            }
            foreach( UIElement ui in elements )
               myCanvasTank.Children.Remove(ui);
            //-------------------------------------------------------
            elements.Clear();
            foreach (UIElement ui in myCanvasMain.Children) // Clean the Canvas of Sherman
            {
               if (ui is Button b)
               {
                  if (true == b.Name.Contains("Sherman"))
                     elements.Add(b); // Remove the old buttons
               }
            }
            foreach (UIElement ui in elements)
               myCanvasMain.Children.Remove(ui);
            //-------------------------------------------------------
            string appendText = "";
            if (9 < myTankCardNum)
               appendText = myTankCardNum.ToString();
            else
               appendText = "0" + myTankCardNum.ToString();
            Image imageMat = new Image() { Name = "TankMat", Width = 600, Height = 500, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("m" + appendText) };
            myCanvasTank.Children.Add(imageMat); // TankMat changes as get new tanks
            Canvas.SetLeft(imageMat, 0);
            Canvas.SetTop(imageMat, 0);
         }
         //-------------------------------------------------------
         switch (action)
         {
            case GameAction.ShowCombatCalendarDialog:
            case GameAction.ShowAfterActionReportDialog:
            case GameAction.ShowGameFeatsDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowMovementDiagramDialog:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.SetupShowMapHistorical:
            case GameAction.SetupShowMovementBoard:
            case GameAction.SetupShowBattleBoard:
            case GameAction.SetupShowTankCard:
            case GameAction.SetupShowAfterActionReport:
            case GameAction.SetupShowCombatCalendarCheck:
            case GameAction.SetupCombatCalendarRoll:
               return; // do not update Tank Card Canvas
            case GameAction.UpdateGameOptions:
               Logger.Log(LogEnum.LE_VIEW_SHOW_SETTINGS, "GameViewerWindow.UpdateView(): Save_DefaultsToSettings() due to a=" + action.ToString());
               SaveDefaultsToSettings();
               return; // do not update Tank Card Canvas
            case GameAction.ShowTankForcePath:
               if (null == myMainMenuViewer)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Update_View(): myMainMenuViewer=null");
                  return;
               }
               if ((true == myMainMenuViewer.IsPathShown) && (EnumMainImage.MI_Move == CanvasImageViewer.theMainImage))
               {
                  if (false == UpdateCanvasPath(gi))
                     Logger.Log(LogEnum.LE_ERROR, "Update_View(): UpdateCanvasPath() returned false");
               }
               else
               {
                  myMainMenuViewer.IsPathShown = false;
                  List<UIElement> elements = new List<UIElement>();
                  foreach (UIElement ui in myCanvasMain.Children)
                  {
                     if (ui is Polyline polyline) // remove all polylines 
                     {
                        if (false == polyline.Name.Contains("Road"))
                           elements.Add(ui);
                     }
                     if (ui is Ellipse ellipse) // remove all ellipse 
                        elements.Add(ui);
                  }
                  foreach (UIElement ui1 in elements)
                     myCanvasMain.Children.Remove(ui1);
               }
               return; // do not update Tank Card Canvas
            case GameAction.ShowRoads:
               if (null == myMainMenuViewer)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myMainMenuViewer=null");
                  return;
               }
               if ((true == myMainMenuViewer.IsRoadsShown) && (EnumMainImage.MI_Move == CanvasImageViewer.theMainImage))
               {
                  foreach (KeyValuePair<string, Polyline> kvp in myRoads)
                     myCanvasMain.Children.Add(kvp.Value);
               }
               else
               {
                  myMainMenuViewer.IsRoadsShown = false;
                  List<UIElement> roadElements = new List<UIElement>();
                  foreach (UIElement ui in myCanvasMain.Children)
                  {
                     if (ui is Polyline polyline) // remove all polylines 
                     {
                        if (true == polyline.Name.Contains("Road"))
                           roadElements.Add(ui);
                     }
                  }
                  foreach (UIElement ui1 in roadElements)
                     myCanvasMain.Children.Remove(ui1);
               }
               return; // do not update Tank Card Canvas
            case GameAction.UpdateTankCard:
            case GameAction.TestingStartMorningBriefing:
            case GameAction.TestingStartPreparations:
            case GameAction.TestingStartMovement:
            case GameAction.TestingStartBattle:
            case GameAction.TestingStartAmbush:
            case GameAction.EveningDebriefingResetDay:
            case GameAction.EveningDebriefingReplaceCrew:
            case GameAction.MorningBriefingBegin:
            case GameAction.MorningBriefingCrewmanHealing:
            case GameAction.MorningBriefingExistingCrewman:
            case GameAction.MorningBriefingReturningCrewman:
            case GameAction.MorningBriefingCalendarRoll:
            case GameAction.MorningBriefingDayOfRest:
            case GameAction.MorningBriefingTankReplacementHvssRoll:
            case GameAction.MorningBriefingTankReplacementRoll:
            case GameAction.MorningBriefingDecreaseTankNum:
            case GameAction.MorningBriefingIncreaseTankNum:
            case GameAction.MorningBriefingTankReplacementEnd:
            case GameAction.MorningBriefingAmmoReadyRackLoad:
            case GameAction.BattleRandomEventRoll:
            case GameAction.PreparationsHatches:
            case GameAction.PreparationsGunLoad:
            case GameAction.PreparationsGunLoadSelect:
            case GameAction.BattleRoundSequenceRoundStart:
            case GameAction.BattleRoundSequenceShermanToHitRollNothing:
            case GameAction.BattleRoundSequenceReadyRackHeMinus:
            case GameAction.BattleRoundSequenceReadyRackApMinus:
            case GameAction.BattleRoundSequenceReadyRackWpMinus:
            case GameAction.BattleRoundSequenceReadyRackHbciMinus:
            case GameAction.BattleRoundSequenceReadyRackHvapMinus:
            case GameAction.BattleRoundSequenceReadyRackHePlus:
            case GameAction.BattleRoundSequenceReadyRackApPlus:
            case GameAction.BattleRoundSequenceReadyRackWpPlus:
            case GameAction.BattleRoundSequenceReadyRackHbciPlus:
            case GameAction.BattleRoundSequenceReadyRackHvapPlus:
            case GameAction.EveningDebriefingStart:
               break;
            case GameAction.PreparationsShowFeat:
            case GameAction.BattleRoundSequenceShowFeat:
            case GameAction.EveningDebriefingShowFeat:
            case GameAction.EndGameShowFeats:
               if (false == UpdateCanvasShowFeats())
                  Logger.Log(LogEnum.LE_ERROR, "Update_View(): UpdateCanvas_ShowFeats(" + action.ToString() + ") returned error ");
               break;
            case GameAction.MorningBriefingDeployment:
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.BattleRoundSequenceSpotting:
               if (false == UpdateCanvasMain(gi, action)) // update smoke depletion
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.BattleRoundSequenceCrewOrders:
               if (false == CreateContextMenuCrewAction(myGameInstance))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): CreateContextMenu_CrewAction() returned false");
               if (false == UpdateCanvasMain(gi, action)) // update smoke depletion
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.PreparationsTurret:
            case GameAction.BattleRoundSequenceTurretEnd:
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.BattleEmptyResolve:
               if (false == UpdateCanvasAnimateBattlePhase(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasAnimateBattlePhase() returned error ");
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
               if (false == UpdateCanvasMain(gi, action))  // show updated canvas if loading a game
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMain() returned error ");
               foreach (Button b in myTankButtons)
                  b.ContextMenu = null;
               if ( false == UpdateCanvasShermanSelectTarget(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasShermanSelectTarget() returned error ");
               break;
            case GameAction.BattleRoundSequenceShermanFiringSelectTargetMg:
               if (false == UpdateCanvasMain(gi, action))  // show updated canvas if loading a game
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMain() returned error ");
               foreach (Button b in myTankButtons)
                  b.ContextMenu = null;
               if (false == UpdateCanvasShermanSelectTargetMg(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasShermanSelectTargetMg() returned error ");
               break;
            case GameAction.BattleRoundSequenceEnemyAction:
            case GameAction.BattleRoundSequenceShermanToHitRoll:
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.BattleRoundSequenceLoadMainGun:
            case GameAction.BattleRoundSequenceLoadMainGunEnd:
            case GameAction.BattleRoundSequenceBackToSpotting:
               if (false == UpdateCanvasAnimateBattlePhase(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasAnimateBattlePhase() returned error ");
               break;
            case GameAction.EveningDebriefingRatingImprovement:
               UpdateCanvasMainClear(myBattleButtons, gi.BattleStacks, action);
               foreach (Button b in myBattleButtons)
                  myCanvasMain.Children.Remove(b);
               myBattleButtons.Clear();
               foreach (IStack stack in gi.BattleStacks)
                  stack.MapItems.Clear();
               break;
            case GameAction.RemoveSplashScreen:
            case GameAction.UpdateNewGameEnd:
            case GameAction.SetupAssignCrewRating:
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               mySplashScreen.Close();
               myScrollViewerMap.UpdateLayout();
               break;
            case GameAction.SetupFinalize:
               myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.EndGameWin:
            case GameAction.EndGameLost:
               if (false == UpdateCanvasAnimateBattlePhase(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasAnimateBattlePhase() returned error ");
               Logger.Log(LogEnum.LE_VIEW_SHOW_SETTINGS, "GameViewerWindow.UpdateView(): Save_DefaultsToSettings() due to a=" + action.ToString());
               SaveDefaultsToSettings(); // End_GameWin or End_GameLost
               break;
            case GameAction.EndGameShowStats:
               if (false == UpdateCanvasShowStatistics(gi))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasShowStatistics() returned error ");
               break;
            case GameAction.UpdateTankExplosion:
            case GameAction.UpdateTankBrewUp:
            case GameAction.BattleShermanKilled:
            default:
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasMain() returned error ");
               break;
         }
         //-------------------------------------------------------
         if (false == UpdateCanvasTank(gi, action))
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasTank() returned error for action=" + action.ToString());
      }
      private bool UpdateViewForNewGame(ref IGameInstance gi, GameAction action) // GameAction.UpdateLoadingGame  GameAction.UpdateNewGame
      {
         myGameInstance = gi;
         myMoveButtons.Clear();
         myBattleButtons.Clear();
         myTankButtons.Clear();
         List<UIElement> elementRemovals = new List<UIElement>();
         foreach (UIElement ui in myCanvasTank.Children)
         {
            if (ui is Button b)
               elementRemovals.Add(ui);
         }
         foreach (UIElement ui1 in elementRemovals)
            myCanvasTank.Children.Remove(ui1);
         Logger.Log(LogEnum.LE_SHOW_MAIN_CLEAR, "UpdateViewForNewGame(): Clearing action=" + action.ToString());
         UpdateCanvasMainClear(myMoveButtons, gi.MoveStacks, action);
         UpdateCanvasMainClear(myBattleButtons, gi.BattleStacks, action);
         myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas); // UploadNewGame - Return to previous saved zoom level
         //----------------------------------
         GameAction nextAction = GameAction.Error;
         if ( GameAction.UpdateLoadingGame == action )
         {
            IGameCommand? cmd = gi.GameCommands.GetLast();
            if (null == cmd)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateView_ForNewGame(): cmd=null");
               return false;
            }
            nextAction = cmd.Action;
            gi.GamePhase = cmd.Phase;
            gi.DieRollAction = cmd.ActionDieRoll;
            gi.EventDisplayed = gi.EventActive = cmd.EventActive;
            if ("e034" == gi.EventActive)
            {
               if (0 < gi.AdvancingFireMarkerCount)
                  nextAction = GameAction.BattleAdvanceFireStart;
               else
                  nextAction = GameAction.BattleActivation; // UpdateView_ForNewGame() - GameAction.UpdateLoadingGame
            }
            else  if( "e038" == gi.EventActive )
            {
               if (false == UpdateCanvasTank(gi, action))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Update_CanvasTank() returned error for action=" + action.ToString());
                  return false;
               }
               if ( false == UpdateViewCrewOrderButtons(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView_ForNewGame(): cmd=null");
                  return false;
               }
            }
            else if ("e044" == gi.EventActive)
            {
               gi.DieRollAction = GameAction.BattleRoundSequencePanzerfaustSectorRoll;
            }
         }
         else if (GameAction.UpdateNewGame == action)
         {
            nextAction = GameAction.UpdateNewGameEnd;
         }
         else if( GameAction.RemoveSplashScreen == action )
         {
            mySplashScreen.Close();
            nextAction = GameAction.UpdateNewGameEnd;
         }
         //----------------------------------
         if (false == UpdateCanvasMain(gi, action)) 
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView_ForNewGame(): UpdateCanvasMain() returned error ");
            return false;
         }
         myGameEngine.PerformAction(ref gi, ref nextAction, Utilities.NO_RESULT);
         return true;
      }
      private bool UpdateViewUndo(ref IGameInstance gi)
      {
         if (null == gi.UndoCmd)
         {
            Logger.Log(LogEnum.LE_ERROR, "Update_ViewUndo(): gi.UndoCmd=null");
            return false;
         }
         Logger.Log(LogEnum.LE_UNDO_COMMAND, "Update_ViewUndo(): undo=" + gi.UndoCmd.ToString());
         if (false == gi.UndoCmd.Undo(gi, myGameEngine, this))
         {
            Logger.Log(LogEnum.LE_ERROR, "Update_ViewUndo():  gi.UndoCmd.Undo() return false");
            return false;
         }
         gi.UndoCmd = null;
         return true;
      }
      public bool UpdateViewCrewOrderButtons(IGameInstance gi)
      {
         if (false == CreateContextMenuCrewAction(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView_CrewOrderButtons(): CreateContextMenu_CrewAction() returned false");
            return false;
         }
         foreach (Button b in myTankButtons)
         {
            switch(b.Name)
            {
               case "Loader_Load":
               case "Loader_RepairMainGun":
               case "Loader_RepairCoaxialMg":
               case "Loader_FireMortar":
               case "Loader_ChangeGunLoad":
               case "Loader_RestockReadyRack":
               case "Loader_RepairScope":
               case "Loader_FireAaMg":
               case "Loader_RepairAaMg":
               case "Loader_FireSubMg":
               case "Loader_SwitchLdr":
               case "Loader_SwitchDvr":
               case "Loader_SwitchGunr":
               case "Loader_SwitchAsst":
               case "Loader_SwitchCmdr":
                  b.ContextMenu = myContextMenuCrewActions["Loader"];
                  break;
               case "Driver_Stop":
               case "Driver_Forward":
               case "Driver_ForwardToHullDown":
               case "Driver_Reverse":
               case "Driver_ReverseToHullDown":
               case "Driver_PivotTank":
               case "Driver_RepairScope":
               case "Driver_SwitchLdr":
               case "Driver_SwitchDvr":
               case "Driver_SwitchGunr":
               case "Driver_SwitchAsst":
               case "Driver_SwitchCmdr":
                  b.ContextMenu = myContextMenuCrewActions["Driver"];
                  break;
               case "Gunner_FireMainGun":
               case "Gunner_FireCoaxialMg":
               case "Gunner_RotateTurret":
               case "Gunner_RotateFireMainGun":
               case "Gunner_RepairMainGun":
               case "Gunner_RepairScope":
               case "Gunner_ThrowGrenade":
               case "Gunner_SwitchLdr":
               case "Gunner_SwitchDvr":
               case "Gunner_SwitchGunr":
               case "Gunner_SwitchAsst":
               case "Gunner_SwitchCmdr":
                  b.ContextMenu = myContextMenuCrewActions["Gunner"];
                  break;
               case "Assistant_FireBowMg":
               case "Assistant_RepairBowMg":
               case "Assistant_PassAmmo":
               case "Assistant_RepairScope":
               case "Assistant_SwitchLdr":
               case "Assistant_SwitchDvr":
               case "Assistant_SwitchGunr":
               case "Assistant_SwitchAsst":
               case "Assistant_SwitchCmdr":
                  b.ContextMenu = myContextMenuCrewActions["Assistant"];
                  break;
               case "Commander_Move":
               case "Commander_MainGunFire":
               case "Commander_MGFire":
               case "Commander_RepairScope":
               case "Commander_FireAaMg":
               case "Commander_RepairAaMg":
               case "Commander_FireSubMg":
               case "Commander_ThrowGrenade":
               case "Commander_Bail":
               case "Commander_SwitchLdr":
               case "Commander_SwitchDvr":
               case "Commander_SwitchGunr":
               case "Commander_SwitchAsst":
               case "Commander_SwitchCmdr":
                  b.ContextMenu = myContextMenuCrewActions["Commander"];
                  break;
               default:
                  break; // do nothing
            }
         }
         return true;
      }
      public bool UpdateViewAmmoOrderButtons(IGameInstance gi)
      {
         if (false == CreateContextMenuGunLoadAction(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView_AmmoOrderButtons(): CreateContextMenu_GunLoadAction() returned false");
            return false;
         }
         foreach (Button b in myTankButtons)
         {
            switch (b.Name)
            {
               case "GunLoadHe_AmmoReload":
               case "GunLoadHe_ReadyRackAmmoReload":
                  b.ContextMenu = myContextMenuGunLoadActions["GunLoadHe"];
                  break;
               case "GunLoadAp_AmmoReload":
               case "GunLoadAp_ReadyRackAmmoReload":
                  b.ContextMenu = myContextMenuGunLoadActions["GunLoadAp"];
                  break;
               case "GunLoadHbci_AmmoReload":
               case "GunLoadHbci_ReadyRackAmmoReload":
                  b.ContextMenu = myContextMenuGunLoadActions["GunLoadHbci"];
                  break;
               case "GunLoadWp_AmmoReload":
               case "GunLoadWp_ReadyRackAmmoReload":
                  b.ContextMenu = myContextMenuGunLoadActions["GunLoadWp"];
                  break;
               case "GunLoadHvap_AmmoReload":
               case "GunLoadHvap_ReadyRackAmmoReload":
                  b.ContextMenu = myContextMenuGunLoadActions["GunLoadHvap"];
                  break;
               default:
                  break; // do nothing
            }
         }
         return true;
      }
      private string UpdateViewTitle(IGameInstance gi, IAfterActionReport report)
      {
         Option optionSingleDayScenario = gi.Options.Find("SingleDayScenario");
         StringBuilder sb = new StringBuilder();
         sb.Append("Patton's Best  -  ");
         if( true == optionSingleDayScenario.IsEnabled )
            sb.Append("Single Day - ");
         else
            sb.Append("Campaign Game - ");
         int index= gi.Options.GetGameIndex();
         if( 0 == index )
            sb.Append("Original Game - ");
         else if ( 1 == index )
            sb.Append("Generalv24#3 Game - ");
         else if (2 == index)
            sb.Append("Tactics Game - ");
         else if (3 == index)
            sb.Append("Generalv24#3 + Tactics Game - ");
         else 
            sb.Append("Custom Game - ");
         sb.Append(TableMgr.GetDate(gi.Day));
         sb.Append(" ");
         sb.Append(TableMgr.GetTime(report));
         sb.Append("  -  ");
         sb.Append(report.Scenario.ToString());
         sb.Append(" Scenario -  ");
         sb.Append(report.Resistance.ToString());
         sb.Append(" Resistance Expected");
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
         System.Windows.Controls.Button b = new Button { Name = Utilities.RemoveSpaces(mi.Name), Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
         MapItem.SetButtonContent(b, mi, true, true); // This sets the image as the button's content
         RotateTransform rotateTransform = new RotateTransform();
         b.RenderTransformOrigin = new Point(0.5, 0.5);
         rotateTransform.Angle = mi.RotationHull + mi.RotationOffsetHull;
         b.RenderTransform = rotateTransform;
         buttons.Add(b);
         Canvas.SetLeft(b, mi.Location.X);
         Canvas.SetTop(b, mi.Location.Y);
         if( true == b.Name.Contains("Smoke"))
            Canvas.SetZIndex(b, 100);
         else if (true == b.Name.Contains("Sherman"))
            Canvas.SetZIndex(b, 9999);
         else
            Canvas.SetZIndex(b, 1000);
         b.Click += ClickButtonMapItem;
         this.PreviewMouseMove += MouseMoveGameViewerWindow;
         b.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDownMapItem;
         b.PreviewMouseLeftButtonUp += PreviewMouseLeftButtonUpMapItem;
         return b;
      }
      private bool CreateContextMenuCrewAction(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateContextMenu_CrewAction(): lastReport=null");
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
         bool isLoaderRepairingGun = false;
         bool isLoaderChangingLoad = false;
         bool isTankMoving = false;
         int periscopeRepairCount = 0;
         //-------------------------------------------------------
         foreach (IMapItem mi in gi.CrewActions) // This menu is created on each crew action
         {
            if (true == mi.Name.Contains("Loader_ChangeGunLoad"))
               isLoaderChangingLoad = true;
            if (true == mi.Name.Contains("Commander_FireAaMg"))
               isCommanderFireAaMg = true;
            if (true == mi.Name.Contains("Loader_FireAaMg"))
               isLoaderFireAaMg = true;
            if (true == mi.Name.Contains("Commander_RepairAaMg"))
               isCommanderRepairAaMg = true;
            if (true == mi.Name.Contains("Loader_RepairAaMg"))
               isLoaderRepairAaMg = true;
            if (true == mi.Name.Contains("Loader_RepairMainGun"))
               isLoaderRepairingGun = true;
            if (true == mi.Name.Contains("Commander_FireSubMg"))
               isCommanderFireSubMg = true;
            if (true == mi.Name.Contains("Loader_FireSubMg"))
               isLoaderFireSubMg = true;
            if (true == mi.Name.Contains("Commander_ThrowGrenade"))
               isCommanderThrowGrenade = true;
            if (true == mi.Name.Contains("Gunner_ThrowGrenade"))
               isGunnerThrowGrenade = true;
            if (true == mi.Name.Contains("Driver_Forward"))
               isTankMoving = true;
            if (true == mi.Name.Contains("Driver_ForwardToHullDown"))
               isTankMoving = true;
            if (true == mi.Name.Contains("Driver_Reverse"))
               isTankMoving = true;
            if (true == mi.Name.Contains("Driver_ReverseToHullDown"))
               isTankMoving = true;
            if (true == mi.Name.Contains("Driver_PivotTank"))
               isTankMoving = true;
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
         //---------------------------------
         string sector = Territory.GetMainGunSector(gi);
         if( "ERROR" == sector )
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateContextMenu_CrewAction(): GetMainGunSector() returned ERROR");
            return false;
         }
         bool isGunnerTrainedInHvss = ( (true == gi.TrainedGunners.Contains(lastReport.Gunner.Name)) && (false == lastReport.Gunner.IsIncapacitated) );
         bool isTargetInCurrentMainGunSector = Territory.IsEnemyUnitInSector(gi, sector);
         bool iMainGunAbleAbleToFireDueToMoving = ( (false == isTankMoving) || (true == isGunnerTrainedInHvss) );
         bool isMainGunFiringAvailable = ((true == iMainGunAbleAbleToFireDueToMoving) && (false == gi.IsMalfunctionedMainGun) && (false == gi.IsBrokenMainGun) && (false == gi.IsBrokenGunSight) && (0 < totalAmmo) && ("None" != gi.GetGunLoadType()) && (false == isLoaderChangingLoad) );
         bool isShermanMoveAvailable = ( (false == gi.Sherman.IsThrownTrack) && (false == gi.Sherman.IsAssistanceNeeded) && ((false == gi.IsBrokenPeriscopeDriver) || (true == isDriverOpenHatch)) );
         //---------------------------------
         myContextMenuCrewActions["Driver"] = new ContextMenu();
         myContextMenuCrewActions["Loader"] = new ContextMenu();
         myContextMenuCrewActions["Assistant"] = new ContextMenu();
         myContextMenuCrewActions["Gunner"] = new ContextMenu();
         myContextMenuCrewActions["Commander"] = new ContextMenu();
         //---------------------------------
         MenuItem menuItem1 = new MenuItem();
         ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
         if( null == loader )
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateContextMenu_CrewAction(): Loader=null");
            return false;
         }
         if (false == loader.IsIncapacitated) // CreateContextMenu_CrewAction() 
         {
            menuItem1 = new MenuItem();
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
            if ((0 < lastReport.AmmoSmokeBomb) && (true == card.myIsSmokeMortar))
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Loader_FireMortar";
               menuItem1.Header = "Fire Mortar";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
            }
            if ((0 < totalAmmo) && (false == gi.IsBrokenMainGun) && (false == gi.IsBrokenGunSight))
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
            if ((true == isLoaderOpenHatch) && (0 < lastReport.Ammo50CalibreMG) && (false == isCommanderFireAaMg) && (false == gi.IsMalfunctionedMgAntiAircraft) && (false == gi.IsBrokenMgAntiAircraft) && (true == card.myIsLoaderAaMgMount))
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Loader_FireAaMg";
               menuItem1.Header = "Fire AA MG";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
            }
            if ((true == gi.IsMalfunctionedMgAntiAircraft) && (false == isCommanderRepairAaMg) && (false == gi.IsBrokenMgAntiAircraft) && (true == card.myIsLoaderAaMgMount))
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Loader_RepairAaMg";
               menuItem1.Header = "Repair AA MG";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
            }
            if ((true == isLoaderOpenHatch) && (false == isCommanderFireSubMg) && ("A" == card.myChasis)) // Sub MG uses own ammo
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Loader_FireSubMg";
               menuItem1.Header = "Fire Sub MG";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Loader"].Items.Add(menuItem1);
            }
         }
         //===========================================================================================================
         ICrewMember? driver = gi.GetCrewMemberByRole("Driver");
         if (null == driver)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateContextMenu_CrewAction(): Driver=null");
            return false;
         }
         if(false == driver.IsIncapacitated)  // CreateContextMenu_CrewAction()
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Driver_Stop";
            menuItem1.Header = "Stop";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Driver"].Items.Add(menuItem1);
            if ((true == isDriverOpenHatch) || (false == gi.IsBrokenPeriscopeDriver)) // If broken scope and button up, cannot drive
            {
               if (false == gi.Sherman.IsThrownTrack)
               {
                  if ((false == gi.Sherman.IsBoggedDown) && (0 < gi.Fuel) ) // bogged tanks can only attempt to free themselves by ordering reverse.
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
                     menuItem1 = new MenuItem();
                     menuItem1.Name = "Driver_Reverse";
                     menuItem1.Header = "Reverse";
                     menuItem1.Click += MenuItemCrewActionClick;
                     myContextMenuCrewActions["Driver"].Items.Add(menuItem1);
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
                  else // if bogged down, allow attempt to reverse
                  {
                     if ( (false == gi.Sherman.IsAssistanceNeeded) && (0 < gi.Fuel) ) // if assistenance is needed, the tank is stuck and cannot free itself
                     {
                        menuItem1 = new MenuItem();
                        menuItem1.Name = "Driver_Reverse";
                        menuItem1.Header = "Reverse out of Bog";
                        menuItem1.Click += MenuItemCrewActionClick;
                        myContextMenuCrewActions["Driver"].Items.Add(menuItem1);
                     }
                     if (0 == gi.Fuel)
                        gi.Sherman.IsAssistanceNeeded = true;
                  }
               }
            }
            if ((true == gi.IsBrokenPeriscopeDriver) && (0 < diffPeriscopes))
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Driver_RepairScope";
               menuItem1.Header = "Replace Periscope";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Driver"].Items.Add(menuItem1);
            }
         }
         //===========================================================================================================--
         string gunload = myGameInstance.GetGunLoadType();
         ICrewMember? gunner = gi.GetCrewMemberByRole("Gunner");
         if (null == gunner)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateContextMenu_CrewAction(): Gunner=null");
            return false;
         }
         if (false == gunner.IsIncapacitated) // CreateContextMenu_CrewAction()
         {
            if (true == isMainGunFiringAvailable)
            {
               if (true == isTargetInCurrentMainGunSector)
               {
                  menuItem1 = new MenuItem();
                  menuItem1.Name = "Gunner_FireMainGun";
                  menuItem1.Header = "Fire Main Gun";
                  menuItem1.Click += MenuItemCrewActionClick;
                  myContextMenuCrewActions["Gunner"].Items.Add(menuItem1);
               }
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
            if ((true == gi.IsMalfunctionedMainGun) && (true == isLoaderRepairingGun)) // can only help loader repair main gun
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Gunner_RepairMainGun";
               menuItem1.Header = "Repair Main Gun";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Gunner"].Items.Add(menuItem1);
            }
            if ((true == isCommanderOpenHatch) && (0 < lastReport.AmmoSmokeGrenade) && (false == isCommanderThrowGrenade)) // Gunner must throw grenade out commander hatch
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
         }
         //===========================================================================================================
         bool is30CalibreMGFirePossible = (0 < lastReport.Ammo30CalibreMG) && (((false == gi.IsBrokenMgBow) && (false == gi.IsMalfunctionedMgBow)) || ((false == gi.IsBrokenMgCoaxial) && (false == gi.IsMalfunctionedMgCoaxial))); // bow and coaxial MGs
         bool is50CalibreMGFirePossible = (0 < lastReport.Ammo50CalibreMG) && ((false == gi.IsBrokenMgAntiAircraft) && (false == gi.IsMalfunctionedMgAntiAircraft) && (false == isLoaderFireAaMg)); // subMG can always be fired
         ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
         if (null == commander)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateContextMenu_CrewAction(): Commander=null");
            return false;
         }
         if (false == commander.IsIncapacitated)  // CreateContextMenu_CrewAction()
         {
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
            if ((true == isCommanderOpenHatch) && (false == isLoaderFireSubMg))
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Commander_FireSubMg";
               menuItem1.Header = "Fire Sub MG";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Commander"].Items.Add(menuItem1);
            }
         }
         if( (true == gi.Sherman.IsThrownTrack) || (true == gi.Sherman.IsAssistanceNeeded) )
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Commander_Bail";
            menuItem1.Header = "Crew Bail";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Commander"].Items.Add(menuItem1);
         }
         //===========================================================================================================
         ICrewMember? assistant = gi.GetCrewMemberByRole("Assistant");
         if (null == assistant)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateContextMenu_CrewAction(): Assistant=null");
            return false;
         }
         if (false == assistant.IsIncapacitated) // CreateContextMenu_CrewAction()
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = "Assistant_PassAmmo";
            menuItem1.Header = "Pass Ammo";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions["Assistant"].Items.Add(menuItem1);
            if ((true == isAssistantOpenHatch) || (false == gi.IsBrokenPeriscopeAssistant)) // If broken scope and button up, cannot drive
            {
               if ((0 < lastReport.Ammo30CalibreMG) && (false == gi.IsMalfunctionedMgBow) && (false == gi.IsBrokenMgBow) && (false == gi.Sherman.IsHullDown))
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
         }
         //===========================================================================================================
         if (true == string.IsNullOrEmpty(gi.SwitchedCrewMemberRole)) // assistant has not yet switched with anybody if null
         {
            if (true == driver.IsIncapacitated)
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Assistant_SwitchDvr";
               menuItem1.Header = "Switch w/ Driver";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Assistant"].Items.Add(menuItem1);
            }
            if (true == loader.IsIncapacitated)
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Assistant_SwitchLdr";
               menuItem1.Header = "Switch w/ Loader";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Assistant"].Items.Add(menuItem1);
            }
            if (true == gunner.IsIncapacitated)
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Assistant_SwitchGunr";
               menuItem1.Header = "Switch w/ Gunner";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Assistant"].Items.Add(menuItem1);
            }
            if (true == commander.IsIncapacitated)
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = "Assistant_SwitchCmdr";
               menuItem1.Header = "Switch w/ Commander";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions["Assistant"].Items.Add(menuItem1);
            }
         }
         else // assistant already switched with somebody - gi.SwitchedCrewMemberRole is where the Assistant current is now
         {
            menuItem1 = new MenuItem();
            menuItem1.Name = gi.SwitchedCrewMemberRole + "_SwitchAsst";
            menuItem1.Header = "Return to Assistant";
            menuItem1.Click += MenuItemCrewActionClick;
            myContextMenuCrewActions[gi.SwitchedCrewMemberRole].Items.Add(menuItem1);
            if (true == driver.IsIncapacitated)
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = gi.SwitchedCrewMemberRole + "_SwitchDvr";
               menuItem1.Header = "Switch w/ Driver";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions[gi.SwitchedCrewMemberRole].Items.Add(menuItem1);
            }
            if (true == loader.IsIncapacitated)
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = gi.SwitchedCrewMemberRole + "_SwitchLdr";
               menuItem1.Header = "Switch w/ Loader";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions[gi.SwitchedCrewMemberRole].Items.Add(menuItem1);
            }
            if (true == gunner.IsIncapacitated)
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = gi.SwitchedCrewMemberRole + "_SwitchGunr";
               menuItem1.Header = "Switch w/ Gunner";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions[gi.SwitchedCrewMemberRole].Items.Add(menuItem1);
            }
            if (true == commander.IsIncapacitated)
            {
               menuItem1 = new MenuItem();
               menuItem1.Name = gi.SwitchedCrewMemberRole + "_SwitchCmdr";
               menuItem1.Header = "Switch w/ Commander";
               menuItem1.Click += MenuItemCrewActionClick;
               myContextMenuCrewActions[gi.SwitchedCrewMemberRole].Items.Add(menuItem1);
            }
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
         myContextMenuGunLoadActions["GunLoadHe"] = new ContextMenu();
         int minCount = 0;
         if ("He" == gunLoadType)
            minCount = 1;
         if (minCount < lastReport.MainGunHE )
         {
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
         myContextMenuGunLoadActions["GunLoadAp"] = new ContextMenu();
         minCount = 0;
         if ("Ap" == gunLoadType) // if the one ammo is loaded in gun, cannot perform ammo reload
            minCount = 1;
         if (minCount < lastReport.MainGunAP)
         {
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
         myContextMenuGunLoadActions["GunLoadHbci"] = new ContextMenu();
         minCount = 0;
         if ("Hbci" == gunLoadType) // if the one ammo is loaded in gun, cannot perform ammo reload
            minCount = 1;
         myContextMenuGunLoadActions["GunLoadHbci"].Items.Clear();
         if (0 < lastReport.MainGunHBCI)
         {
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
         myContextMenuGunLoadActions["GunLoadWp"] = new ContextMenu();
         minCount = 0;
         if ("Wp" == gunLoadType) // if the one ammo is loaded in gun, cannot perform ammo reload
            minCount = 1;
         if (0 < lastReport.MainGunWP)
         {
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
         myContextMenuGunLoadActions["GunLoadHvap"] = new ContextMenu();
         if ("Hvap" == gunLoadType) // if the one ammo is loaded in gun, cannot perform ammo reload
            minCount = 1;
         if (0 < lastReport.MainGunHVAP)
         {
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
      private void SaveDefaultsToSettings(bool isWindowPlacementSaved = true)
      {
         theSaveSettingsMutex.WaitOne();
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
            //-------------------------------------------
            if (true == isWindowPlacementSaved)
            {
               WindowPlacement wp; // Persist window placement details to application settings
               var hwnd = new WindowInteropHelper(this).Handle;
               if (false == GetWindowPlacement(hwnd, out wp))
                  Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): GetWindowPlacement() returned false");
               string sWinPlace = Utilities.Serialize<WindowPlacement>(wp);
               Properties.Settings.Default.WindowPlacement = sWinPlace;
            }
            //-------------------------------------------
            Properties.Settings.Default.ZoomCanvas = Utilities.ZoomCanvas;
            //-------------------------------------------
            Properties.Settings.Default.ScrollViewerHeight = myScrollViewerMap.Height;
            Properties.Settings.Default.ScrollViewerWidth = myScrollViewerMap.Width;
            //-------------------------------------------
            Logger.Log(LogEnum.LE_VIEW_SHOW_OPTIONS, "Save_DefaultsToSettings(): Options=" + myGameInstance.Options.ToString());
            string? sOptions = SerializeOptions(myGameInstance.Options);
            if (null == sOptions)
               Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): SerializeOptions() returned false");
            else
               Properties.Settings.Default.GameOptions = sOptions;
            //-------------------------------------------
            Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Save_DefaultsToSettings():\n  SAVING feats=" + GameEngine.theInGameFeats.ToString() );
            if (false == SerializeGameFeats(GameEngine.theInGameFeats))
               Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): SerializeGameFeats() returned false");
            //-------------------------------------------
            if (false == SerializeGameStatistics(GameEngine.theSingleDayStatistics, "stat0"))
               Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): SerializeGameStatistics() returned false");
            if (false == SerializeGameStatistics(GameEngine.theCampaignStatistics, "stat1"))
               Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): SerializeGameStatistics(theCampaignStatistics) returned false");
            if (false == SerializeGameStatistics(GameEngine.theTotalStatistics, "stat2"))
               Logger.Log(LogEnum.LE_ERROR, "Save_DefaultsToSettings(): SerializeGameStatistics(theTotalStatistics) returned false");
            //-------------------------------------------
            Properties.Settings.Default.Save();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         theSaveSettingsMutex.ReleaseMutex();
      }
      private string? SerializeOptions(Options options)
      {
         //--------------------------------                                                                                              //--------------------------------                                                                                  //--------------------------------
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<Options></Options>");
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_Options(): aXmlDocument.DocumentElement=null");
            return null;
         }
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_Options(): root is null");
            return null;
         }
         aXmlDocument.DocumentElement.SetAttribute("count", options.Count.ToString());
         //--------------------------------
         foreach (Option option in options)
         {
            XmlElement? optionElem = aXmlDocument.CreateElement("Option");
            if (null == optionElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_Options(): CreateElement(Option) returned null");
               return null;
            }
            optionElem.SetAttribute("Name", option.Name);
            optionElem.SetAttribute("IsEnabled", option.IsEnabled.ToString());
            XmlNode? optionNode = root.AppendChild(optionElem);
            if (null == optionNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_Options(): AppendChild(optionNode) returned null");
               return null;
            }
         }
         //--------------------------------
         return aXmlDocument.OuterXml;
      }
      private bool SerializeGameFeats(GameFeats feats)
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<GameFeats></GameFeats>");
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): aXmlDocument.DocumentElement=null");
            return false;
         }
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): root is null");
            return false;
         }
         aXmlDocument.DocumentElement.SetAttribute("count", feats.Count.ToString());
         //--------------------------------
         foreach (GameFeat feat in feats)
         {
            XmlElement? featElem = aXmlDocument.CreateElement("Feat");
            if (null == featElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): CreateElement(Feat) returned null");
               return false;
            }
            featElem.SetAttribute("Key", feat.Key);
            featElem.SetAttribute("Value", feat.Value.ToString());
            XmlNode? featNode = root.AppendChild(featElem);
            if (null == featNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): AppendChild(featNode) returned null");
               return false;
            }
         }
         //-----------------------------------------
         if (null == aXmlDocument)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameTo_File(): aXmlDocument=null");
            return false;
         }
         //-----------------------------------------
         try
         {
            if (false == Directory.Exists(GameFeats.theGameFeatDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameFeats.theGameFeatDirectory);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): path=" + GameFeats.theGameFeatDirectory + "\n e=" + e.ToString());
            return false;
         }
         string filename = GameFeats.theGameFeatDirectory + "feats.xml";
         if (File.Exists(filename))
            File.Delete(filename);
         FileStream? writer = null;
         //-----------------------------------------
         try
         {
            writer = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
            XmlWriter xmlWriter = XmlWriter.Create(writer, settings);// For XmlWriter, it uses the stream that was created: writer.
            aXmlDocument.Save(xmlWriter);
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameFeats(): path=" + GameFeats.theGameFeatDirectory + "\n e =" + ex.ToString());
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return false;
         }
         finally
         {
            if (writer != null)
               writer.Close();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         }
         return true;
      }
      private bool SerializeGameStatistics(GameStatistics statistics, string filename)
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<GameStatistics> </GameStatistics>");
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameStatistics(): aXmlDocument.DocumentElement=null");
            return false;
         }
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameStatistics(): root is null");
            return false;
         }
         aXmlDocument.DocumentElement.SetAttribute("count", statistics.Count.ToString());
         //-----------------------------------------
         foreach (GameStatistic statistic in statistics)
         {
            XmlElement? statisticElem = aXmlDocument.CreateElement("GameStatistic");
            if (null == statisticElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_GameStatistics(): CreateElement(GameStatistic) returned null");
               return false;
            }
            statisticElem.SetAttribute("Key", statistic.Key);
            statisticElem.SetAttribute("Value", statistic.Value.ToString());
            XmlNode? statisticNode = root.AppendChild(statisticElem);
            if (null == statisticNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "Serialize_GameStatistics(): AppendChild(statisticNode) returned null");
               return false;
            }
         }
         //-----------------------------------------
         string filenameFull = GameStatistics.theGameStatisticsDirectory + filename + ".xml";
         if (File.Exists(filenameFull))
            File.Delete(filenameFull);
         FileStream? writer = null;
         //-----------------------------------------
         try
         {
            writer = new FileStream(filenameFull, FileMode.OpenOrCreate, FileAccess.Write);
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
            XmlWriter xmlWriter = XmlWriter.Create(writer, settings);// For XmlWriter, it uses the stream that was created: writer.
            aXmlDocument.Save(xmlWriter);
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Serialize_GameStatistics(): path=" + GameFeats.theGameFeatDirectory + "\n e =" + ex.ToString());
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return false;
         }
         finally
         {
            if (writer != null)
               writer.Close();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         }
         //--------------------------------
         return true;
      }
      private void SetDisplayIconForUninstall()
      {
#if !DEBUG // Only do this for release version
         if (true == Properties.Settings.Default.theIsFirstRun) // only do once - must set it in registry
         {
            try
            {
               string iconSourcePath = System.IO.Path.Combine(MapImage.theImageDirectory, "PattonsBest.ico");
               Microsoft.Win32.RegistryKey? sUnInstallKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
               if (null == sUnInstallKey)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SetDisplayIconForUninstall(): sUnInstallKey=null");
                  return;
               }
               string[] sSubKeyNames = sUnInstallKey.GetSubKeyNames();
               for (int i = 0; i < sSubKeyNames.Length; i++)
               {
                  string? sSubKeyName = sSubKeyNames[i];
                  if (null == sSubKeyName)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "SetDisplayIconForUninstall(): sSubKeyName=null");
                     return;
                  }
                  Microsoft.Win32.RegistryKey? aKey = sUnInstallKey.OpenSubKey(sSubKeyName, true);
                  if (null == aKey)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "SetDisplayIconForUninstall(): aKey=null");
                     return;
                  }
                  // ClickOnce(Publish)
                  // Publish -> Settings -> Options 
                  // Publish Options -> Description -> Product Name (is your DisplayName)
                  Object? key = aKey.GetValue("DisplayName");
                  if( null == key )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "SetDisplayIconForUninstall(): aKey=null");
                     return;
                  }
                  string? sDisplayName = (string)key;
                  if( null == sDisplayName )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "SetDisplayIconForUninstall(): sDisplayName=null");
                     return;
                  }
                  if (true == sDisplayName.Contains("Patton's Best"))
                  {
                     Logger.Log(LogEnum.LE_GAME_INIT, "SetDisplayIconForUninstall(): iconSourcePath=" + iconSourcePath);
                     aKey.SetValue("DisplayIcon", iconSourcePath);
                     break;
                  }
               }
               Properties.Settings.Default.theIsFirstRun = false;
               Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetDisplayIconForUninstall(): e=" + ex.ToString());
            }
         }
#endif
      }
      //---------------------------------------
      private bool DeserializeOptions(String sXml, Options options)
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         //-----------------------------------------------
         options.Clear();
         if (true == String.IsNullOrEmpty(sXml))
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): String.IsNullOrEmpty() returned true");
            if (0 == options.Count)
               options.SetOriginalGameOptions();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            return true;
         }
         //-----------------------------------------------
         try // XML serializer does not work for Interfaces
         {
            StringReader stringreader = new StringReader(sXml);
            XmlReader reader = XmlReader.Create(stringreader);
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): reader.IsStartElement(Options) = false");
               return false;
            }
            if (reader.Name != "Options")
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): Options != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): Count=null");
               return false;
            }
            //-------------------------------------
            int count = int.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): IsStartElement(Option) returned false");
                  return false;
               }
               if (reader.Name != "Option")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): Option != " + reader.Name);
                  return false;
               }
               string? name = reader.GetAttribute("Name");
               if (name == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): Name=null");
                  return false;
               }
               string? sEnabled = reader.GetAttribute("IsEnabled");
               if (sEnabled == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): IsEnabled=null");
                  return false;
               }
               bool isEnabled = bool.Parse(sEnabled);
               Option option = new Option(name, isEnabled);
               options.Add(option);
            }
            if (0 < count)
               reader.Read(); // get past </Options>
         }
         catch (DirectoryNotFoundException dirException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): s=" + sXml + "\ndirException=" + dirException.ToString());
         }
         catch (FileNotFoundException fileException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): s=" + sXml + "\nfileException=" + fileException.ToString());
         }
         catch (IOException ioException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): s=" + sXml + "\nioException=" + ioException.ToString());
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_Options(): s=" + sXml + "\nex=" + ex.ToString());
         }
         finally
         {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            if (0 == options.Count)
               options.SetOriginalGameOptions();
         }
         return true;
      }
      private bool DeserializeGameFeats(GameFeats feats)
      {
         feats.Clear();
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         XmlTextReader? reader = null;
         try
         {
            string filename = GameFeats.theGameFeatDirectory + "feats.xml";
            reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
            if( null == reader )
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): reader=null");
               return false;
            }
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): reader.IsStartElement(Options) = false");
               return false;
            }
            if (reader.Name != "GameFeats")
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): Options != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): Count=null");
               return false;
            }
            //-------------------------------------
            int count = int.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): IsStartElement(Feat) returned false");
                  return false;
               }
               if (reader.Name != "Feat")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): Feat != " + reader.Name);
                  return false;
               }
               string? key = reader.GetAttribute("Key");
               if (key == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): Key=null");
                  return false;
               }
               string? sValue = reader.GetAttribute("Value");
               if (sValue == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): sValue=null");
                  return false;
               }
               int value = Convert.ToInt32(sValue);
               GameFeat feat = new GameFeat(key, value);
               feats.Add(feat);
            }
            if (0 < count)
               reader.Read(); // get past </GameFeats>
         }
         //==========================================
         catch (DirectoryNotFoundException dirException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): dirException=" + dirException.ToString());
            return false;
         }
         catch (FileNotFoundException )
         {
            // expected on first run
         }
         catch (IOException ioException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): ioException=" + ioException.ToString());
            return false;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): ex=" + ex.ToString());
            return false;
         }
         finally
         {
            if (reader != null)
               reader.Close();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            if (0 == feats.Count)
               feats.SetOriginalGameFeats();
            feats.SetGameFeatThreshold(); // always set game feat thresholds to a known value on startup
         }
         return true;
      }
      private bool DeserializeGameStatistics(GameStatistics statistics, string filename)
      {
         statistics.Clear();
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         XmlTextReader? reader = null;
         try
         {
            string qualifiedFilename = GameStatistics.theGameStatisticsDirectory + filename + ".xml";
            reader = new XmlTextReader(qualifiedFilename) { WhitespaceHandling = WhitespaceHandling.None };
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): reader.IsStartElement(Options) = false");
               return false;
            }
            if (reader.Name != "GameStatistics")
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): GameStatistics != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): Count=null");
               return false;
            }
            //-------------------------------------
            int count = int.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): IsStartElement(Feat) returned false");
                  return false;
               }
               if (reader.Name != "GameStatistic")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): GameStatistic != " + reader.Name);
                  return false;
               }
               string? key = reader.GetAttribute("Key");
               if (key == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): Key=null");
                  return false;
               }
               string? sValue = reader.GetAttribute("Value");
               if (sValue == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameStatistics(): sValue=null");
                  return false;
               }
               int value = Convert.ToInt32(sValue);
               GameStatistic stat = new GameStatistic(key, value);
               statistics.Add(stat);
            }
            if (0 < count)
               reader.Read(); // get past </GameFeats>
         }
         //==========================================
         catch (DirectoryNotFoundException dirException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): dirException=" + dirException.ToString());
            return false;
         }
         catch (FileNotFoundException)
         {
            // expected on first run
         }
         catch (IOException ioException)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): ioException=" + ioException.ToString());
            return false;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Deserialize_GameFeats(): ex=" + ex.ToString());
            return false;
         }
         finally
         {
            if (reader != null)
               reader.Close();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            if (0 == statistics.Count)
               statistics.SetOriginalGameStatistics();
         }
         return true;
      }
      private bool DeserializeRoadsFromXml()
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for saving doubles with decimal instead of comma for German users
         XmlTextReader? reader = null;
         PointCollection? points = null;
         string? name = null;
         try
         {
            string filename = ConfigFileReader.theConfigDirectory + "Roads.xml";
            reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None }; // Load the reader with the data file and ignore all white space nodes.    
            if (null == reader)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateRoadsFromXml(): reader=null");
               return false;
            }
            while (reader.Read())
            {
               if (reader.Name == "Road")
               {
                  points = new PointCollection();
                  if (reader.IsStartElement())
                  {
                     name = reader.GetAttribute("value");
                     if (null == name)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "CreateRoadsFromXml(): value=null for name");
                        return false;
                     }
                     string[] aStringArray1 = name.Split('_');
                     if (2 != aStringArray1.Length)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "CreateRoadsFromXml(): aStringArray1.Length=" + aStringArray1.Length);
                        return false;
                     }
                     int indexOfRoad = Int32.Parse(aStringArray1[1]);
                     while (reader.Read())
                     {
                        if ((reader.Name == "point" && (reader.IsStartElement())))
                        {
                           string? value = reader.GetAttribute("X");
                           if (null == value)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "CreateRoadsFromXml(): X=null");
                              return false;
                           }
                           Double X1 = Double.Parse(value);
                           value = reader.GetAttribute("Y");
                           if (null == value)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "CreateRoadsFromXml(): Y=null");
                              return false;
                           }
                           Double Y1 = Double.Parse(value);
                           points.Add(new System.Windows.Point(X1, Y1));
                        }
                        else
                        {
                           break;
                        }
                     }  // end while
                     //-----------------------------------------
                     System.Windows.Media.Brush? brush = null;
                     double roadThickness = 8.0;
                     if (indexOfRoad < 4)
                     {
                        brush = mySolidColorBrushSteelBlue;
                     }
                     else
                     {
                        brush = mySolidColorBrushLawnGreen;
                        roadThickness = 5.0;
                     }
                     Polyline polyline = new Polyline { Name = name, Points = points, Stroke = brush, StrokeThickness = roadThickness, StrokeDashArray = myDashArray, Visibility = Visibility.Visible };
                     myRoads[name] = polyline;
                  } // end if
               } // end if
            } // end while
         } // try
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateRoadsFromXml(): Exception=\n" + e.Message);
            return false;
         }
         finally
         {
            if (reader != null)
               reader.Close();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         }
         return true;
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
               if ((true == gi.Sherman.IsKilled) && (true == img.Name.Contains("TankKilled"))) // UpdateCanvasTank()
                  continue;
               elements.Add(ui);
            }
            if (ui is Button b)
            {
               IMapItem? mi = null;
               if ((null != gi.ShermanHvss) && (true == b.Name.Contains("Hvss")))
                  mi = gi.ShermanHvss;
               if (null == mi)
                  mi = gi.Hatches.Find(b.Name);
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
         if (true == gi.Sherman.IsKilled)
         {
            double offsetHeight = myCanvasTank.ActualHeight / 2.0 - 100;
            double offsetWidth = 30;
            Image imgDeny = new Image { Name = "TankKilled", Source = MapItem.theMapImages.GetBitmapImage("DestroyedTank"), Height = 250, Width = 250 };
            myCanvasTank.Children.Add(imgDeny);
            Canvas.SetLeft(imgDeny, offsetWidth);
            Canvas.SetTop(imgDeny, offsetHeight);
            Canvas.SetZIndex(imgDeny, 99999);
         }
         //-------------------------------------------------------
         if ( true == Logger.theLogLevel[(int)LogEnum.LE_SHOW_TANK_BUTTONS])
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
         if( null != gi.ShermanHvss )
         {
            Button? b = myTankButtons.Find(gi.ShermanHvss.Name);
            if (null != b)
            {
               Logger.Log(LogEnum.LE_SHOW_MAPITEM_TANK, "UpdateCanvasTank(): 1-mi=" + gi.ShermanHvss.Name + " loc=" + gi.ShermanHvss.Location.ToString() + " t=" + gi.ShermanHvss.TerritoryCurrent.Name + " tLoc=" + gi.ShermanHvss.TerritoryCurrent.CenterPoint.ToString());
               Canvas.SetLeft(b, gi.ShermanHvss.Location.X);
               Canvas.SetTop(b, gi.ShermanHvss.Location.Y);
               Canvas.SetZIndex(b, 900);
            }
            else
            {
               Button newButton = CreateButtonMapItem(myTankButtons, gi.ShermanHvss);
               myCanvasTank.Children.Add(newButton);
               Logger.Log(LogEnum.LE_SHOW_MAPITEM_TANK, "UpdateCanvasTank(): 2-mi=" + gi.ShermanHvss.Name + " loc=" + gi.ShermanHvss.Location.ToString() + " t=" + gi.ShermanHvss.TerritoryCurrent.Name + " tLoc=" + gi.ShermanHvss.TerritoryCurrent.CenterPoint.ToString());
            }
         }
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
                  foreach (Button b in myTankButtons)
                  {
                     if( null != b.ContextMenu )
                        b.ContextMenu.IsEnabled = false;  // BattleRoundSequence_AmmoOrders
                  }
                  if (false == CreateContextMenuGunLoadAction(myGameInstance))
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): CreateContextMenuGunLoadAction() returned false");
                  if (false == UpdateCanvasAnimateBattlePhase(gi))
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasAnimateBattlePhase() returned error ");
                  if (false == UpdateCanvasTankAmmoOrders(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvas_TankAmmoOrders() returned false");
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
         Logger.Log(LogEnum.LE_SHOW_MAPITEM_TANK, "UpdateCanvasTankMapItems(): mapItems=" + mapItems.ToString());
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
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankHatches(): lastReport=null");
            return false;
         }
         string tType = lastReport.TankCardNum.ToString();
         //--------------------------------------------
         myPolygons.Clear();
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankHatches(): report=null");
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
               ICrewMember? cm = myGameInstance.GetCrewMemberByRole(crewmember);
               if (null == cm)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankHatches(): cm=null for " + crewmember);
                  return false;
               }
               Logger.Log(LogEnum.LE_SHOW_CREW_BU, "UpdateCanvas_TankHatches(): role=" + crewmember + " isBU=" + cm.IsButtonedUp.ToString() + " isInc=" + cm.IsIncapacitated.ToString());
               if ( (true == cm.IsButtonedUp) && (false == cm.IsIncapacitated) )
               {

                  string tName = crewmember + "_Hatch";
                  ITerritory? t = Territories.theTerritories.Find(tName, tType);
                  if (null == t)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankHatches(): cannot find tName=" + tName + " tType=" + tType);
                     return false;
                  }
                  PointCollection points = new PointCollection();
                  foreach (IMapPoint mp1 in t.Points)
                     points.Add(new System.Windows.Point(mp1.X, mp1.Y));
                  Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = tName };
                  myPolygons.Add(aPolygon);
                  myCanvasTank.Children.Add(aPolygon);
                  aPolygon.MouseDown += MouseDownPolygonHatches;
                  Canvas.SetZIndex(aPolygon, 101);
               }
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankHatches(): EXCEPTION THROWN a=" + action.ToString() + "\ne=" + e.ToString());
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankGunLoad(): report=null");
            return false;
         }
         string tType = report.TankCardNum.ToString();
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
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankGunLoad(): reached default gunload=" + gunload);
                     return false;
               }
               string tName = "GunLoad" + gunload;
               ITerritory? t = Territories.theTerritories.Find(tName, tType);
               if (null == t)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankGunLoad(): cannot find tName=" + tName + " tType=" + tType);
                  return false;
               }
               PointCollection points = new PointCollection();
               foreach (IMapPoint mp1 in t.Points)
                  points.Add(new System.Windows.Point(mp1.X, mp1.Y));
               Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = tName };
               myPolygons.Add(aPolygon);
               myCanvasTank.Children.Add(aPolygon);
               Canvas.SetZIndex(aPolygon, 101);
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankGunLoad(): EXCEPTION THROWN a=" + action.ToString() + "\n" + e.ToString());
            return false;
         }
         return true;
      }
      private bool UpdateCanvasTankOrders(IGameInstance gi, GameAction action)
      {
         foreach (Button b in myTankButtons)
         {
            foreach (IMapItem ca in gi.CrewActions)
            {
               if ( ca.Name == b.Name )
               {
                  if (null != b.ContextMenu)
                     b.ContextMenu.IsEnabled = true;
               }
            }
         }
         //--------------------------------
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankOrders(): report=null");
            return false;
         }
         TankCard tankCard = new TankCard(report.TankCardNum);
         string tType = report.TankCardNum.ToString();
         //--------------------------------
         string[] crewmembers = new string[5] { "Driver", "Assistant", "Commander", "Loader", "Gunner" };
         foreach (string crewmember in crewmembers)
         {
            if (crewmember == "Gunner") // Gunners have no hatches
               continue;
            if ((crewmember == "Loader") && (false == tankCard.myIsLoaderHatch)) // some loaders have no hatches
               continue;
            ICrewMember? cm = myGameInstance.GetCrewMemberByRole(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankOrders(): cm=null for " + crewmember);
               return false;
            }
            if ((true == cm.IsButtonedUp) && (false == cm.IsIncapacitated) )
            {
               string tName = crewmember + "_Hatch";
               ITerritory? t = Territories.theTerritories.Find(tName, tType);
               if (null == t)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankOrders(): cannot find tName=" + tName + " tType=" + tType);
                  return false;
               }
               PointCollection points = new PointCollection();
               foreach (IMapPoint mp1 in t.Points)
                  points.Add(new System.Windows.Point(mp1.X, mp1.Y));
               Polygon aPolygon = new Polygon {Fill = Utilities.theBrushRegion, Points = points, Name = tName };
               myPolygons.Add(aPolygon);
               myCanvasTank.Children.Add(aPolygon);
               Canvas.SetZIndex(aPolygon, 101);
               aPolygon.MouseDown += MouseDownPolygonHatches;
            }
         }
         foreach (string crewmember in crewmembers)
         {
            string tName = crewmember + "Action";
            ITerritory? t = Territories.theTerritories.Find(tName, tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankOrders(): cannot find tName=" + tName + " tType=" + tType);
               return false;
            }
            //--------------------------------------
            ICrewMember? cm = myGameInstance.GetCrewMemberByRole(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankOrders(): cm=null for " + crewmember);
               return false;
            }
            if( false == cm.IsIncapacitated) // UpdateCanvasTankOrders() - show incapacitated people
            {
               PointCollection points = new PointCollection();
               foreach (IMapPoint mp1 in t.Points)
                  points.Add(new System.Windows.Point(mp1.X, mp1.Y));
               Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = tName };
               aPolygon.ContextMenu = myContextMenuCrewActions[crewmember];
               myPolygons.Add(aPolygon);
               myCanvasTank.Children.Add(aPolygon);
               Canvas.SetZIndex(aPolygon, 101);
               aPolygon.MouseDown += MouseDownPolygonCrewActions;
            }
         }
         return true;
      }
      private bool UpdateCanvasTankAmmoOrders(IGameInstance gi, GameAction action)
      {
         foreach (Button b in myTankButtons)
         {
            foreach (IMapItem gl in gi.GunLoads)
            {
               if (gl.Name == b.Name)
               {
                  if (null != b.ContextMenu)
                     b.ContextMenu.IsEnabled = true;
               }
            }
         }
         //--------------------------------
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankAmmoOrders(): report=null");
            return false;
         }
         TankCard tankCard = new TankCard(report.TankCardNum);
         string tType = report.TankCardNum.ToString();
         //--------------------------------
         IMapItem? gunLoadMapItem = null;
         foreach (IMapItem mi in gi.GunLoads) // The context menu is assigned to the GunLoad Button
         {
            if (true == mi.Name.Contains("GunLoadInGun"))
               gunLoadMapItem = mi;
         }
         //--------------------------------
         string[] gunLoads = new string[5] { "He", "Ap", "Wp", "Hbci", "Hvap" }; // used to draw blue rectangles
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
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankAmmoOrders(): reached default gunload=" + gunload);
                  return false;
            }
            string tName = "GunLoad" + gunload;
            ITerritory? t = Territories.theTerritories.Find(tName, tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankAmmoOrders(): cannot find tName=" + tName + " tType=" + tType);
               return false;
            }
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = tName };
            myPolygons.Add(aPolygon);
            myCanvasTank.Children.Add(aPolygon);
            Canvas.SetZIndex(aPolygon, 101);
            //------------------------------------------------
            if ( BattlePhase.MarkAmmoReload == gi.BattlePhase ) 
            {
               aPolygon.MouseDown += MouseDownPolygonAmmoActions;
               aPolygon.ContextMenu = myContextMenuGunLoadActions[tName];
            }
            else
            {
               aPolygon.MouseDown += MouseDownPolygonGunLoad;
            }
         }
         //------------------------------------------------
         if ( (BattlePhase.MarkAmmoReload == gi.BattlePhase) && (null != gunLoadMapItem) )
         {
            StringBuilder sb0 = new StringBuilder("UpdateCanvas_TankAmmoOrders(): ");
            IMapItem? ammoReLoadMapItem = null;
            foreach (IMapItem mi in gi.GunLoads) // The context menu is assigned to the GunLoad Button
            {
               if (true == mi.Name.Contains("AmmoReload"))
                  ammoReLoadMapItem = mi;
            }
            //------------------------------------------------------------
            string gunLoadType = gi.GetGunLoadType(); // This is the ammo loaded into gun after firing
            int ammoCount = 0;
            switch (gunLoadType) // get count of what is still available
            {
               case "He": ammoCount = report.MainGunHE; break;
               case "Ap": ammoCount = report.MainGunAP; break;
               case "Hvap": ammoCount = report.MainGunHVAP; break;
               case "Hbci": ammoCount = report.MainGunHBCI; break;
               case "Wp": ammoCount = report.MainGunWP; break;
               case "None": ammoCount = 0; break;
               default: Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankAmmoOrders(): 2-Reached default ammoReloadType=" + gunLoadType); return false;
            }
            //------------------------------------------------------------
            Option optionAutoSetAmmoLoad = gi.Options.Find("AutoSetAmmoLoad");
            if ((true == optionAutoSetAmmoLoad.IsEnabled) && (false == myIsInitialAmmoReloadButtonCreated) && (1 < ammoCount)) // Automatically place ammo reload in same spot as gun load if ammo exists 
            {
               myIsInitialAmmoReloadButtonCreated = true;
               if (null == ammoReLoadMapItem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankAmmoOrders(): ammoReLoadMapItem=null");
                  return false;
               }
               ContextMenu menu = myContextMenuGunLoadActions[gunLoadMapItem.TerritoryCurrent.Name];
               Thickness thickness = new Thickness(0);
               double size = Utilities.theMapItemSize;
               SolidColorBrush brush = new SolidColorBrush(Colors.Transparent);
               System.Windows.Controls.Button? newButton = null;
               Logger.Log(LogEnum.LE_SHOW_GUN_LOAD, "UpdateCanvas_TankAmmoOrders(): adding new button=" + ammoReLoadMapItem.Name);
               newButton = new Button { ContextMenu = menu, Name = ammoReLoadMapItem.Name, Width = size, Height = size, BorderThickness = thickness, Background = brush, Foreground = brush };
               newButton.Click += ClickButtonMapItem;
               MapItem.SetButtonContent(newButton, ammoReLoadMapItem, true, false); // This sets the image as the button's content
               myTankButtons.Insert(0,newButton);
               myCanvasTank.Children.Insert(0,newButton);
               Canvas.SetLeft(newButton, ammoReLoadMapItem.Location.X);
               Canvas.SetTop(newButton, ammoReLoadMapItem.Location.Y);
               Canvas.SetZIndex(newButton, 899);
            }
            foreach (Button b in myTankButtons)
            {
               if (b.Name == gunLoadMapItem.Name) // find corresponding button - if it exists, set the context menu
               {
                  if (true == gunLoadMapItem.TerritoryCurrent.Name.Contains("OffBoard"))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_TankAmmoOrders(): gunLoadMapItem.Name=" + gunLoadMapItem.Name + " is offboard");
                     return false;
                  }
                  double offset = gunLoadMapItem.Zoom * Utilities.theMapItemOffset;
                  if (null != ammoReLoadMapItem)
                  {
                     if(ammoReLoadMapItem.TerritoryCurrent.Name == gunLoadMapItem.TerritoryCurrent.Name)
                        offset -= 3;
                  }
                  b.ContextMenu = myContextMenuGunLoadActions[gunLoadMapItem.TerritoryCurrent.Name];
                  gunLoadMapItem.Location.X = gunLoadMapItem.TerritoryCurrent.CenterPoint.X - offset;
                  gunLoadMapItem.Location.Y = gunLoadMapItem.TerritoryCurrent.CenterPoint.Y - offset;
                  Canvas.SetLeft(b, gunLoadMapItem.Location.X);
                  Canvas.SetTop(b, gunLoadMapItem.Location.Y);
                  Canvas.SetZIndex(b, 900);
               }
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
            Logger.Log(LogEnum.LE_SHOW_MAIN_CLEAR, "UpdateCanvasMain(): UUUUUUUUUUUUUUUUUUUUUU Clearing MoveButtons action=" + action.ToString());
         }
         else if (EnumMainImage.MI_Battle == CanvasImageViewer.theMainImage )
         {
            foreach (IMapItem mi in gi.Targets) // Remove all boarders around targets
            {
               foreach (Button b in myBattleButtons)
               {
                  if (mi.Name == b.Name)
                     b.BorderThickness = new Thickness(0);  // UpdateCanvasMain()
               }
            }
            stacks = gi.BattleStacks;
            buttons = myBattleButtons;
            myMoveButtons.Clear();
            Logger.Log(LogEnum.LE_SHOW_MAIN_CLEAR, "UpdateCanvasMain(): UUUUUUUUUUUUUUUUUUUUUU Clearing BattleButtons action=" + action.ToString());
         }
         else
         {
            myBattleButtons.Clear();
            myMoveButtons.Clear();
            List<UIElement> buttonRemovals = new List<UIElement>();
            foreach (UIElement ui in myCanvasMain.Children) // Clean the Canvas of all marks
            {
               if (ui is Button button)
               {
                  if (false == button.Name.Contains("Die"))  // die buttons never disappear - only one copy of them
                     buttonRemovals.Add(button);
               }
            }
            foreach (UIElement ui1 in buttonRemovals)
               myCanvasMain.Children.Remove(ui1);
            return true;
         }
         //-------------------------------------------------------
         if( GameAction.UpdateLoadingGame != action)
            UpdateCanvasMainClear(buttons, stacks, action);
         //-------------------------------------------------------
         //if (GamePhase.UnitTest == gi.GamePhase)
         //   return true;
         //-------------------------------------------------------
         if (false == UpdateCanvasMainMapItems(buttons, stacks))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMainMapItems() returned false");
            return false;
         }
         //-------------------------------------------------------
         if( null == myMainMenuViewer )
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): myMainMenuViewer=null");
            return false;
         }  
         if ((true == myMainMenuViewer.IsPathShown) && (EnumMainImage.MI_Move == CanvasImageViewer.theMainImage) )
         {
            if (false == UpdateCanvasPath(gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas(): UpdateCanvasPath() returned false");
               return false;
            }
         }
         if ((true == myMainMenuViewer.IsRoadsShown) && (EnumMainImage.MI_Move == CanvasImageViewer.theMainImage))
         {
            foreach (KeyValuePair<string, Polyline> kvp in myRoads)
               myCanvasMain.Children.Add(kvp.Value);
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
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMain_EnterArea() returned false");
                     return false;
                  }
                  break;
               case GameAction.BattleRoundSequenceShermanRetreatChoice:
                  if (false == UpdateCanvasMainCounterattackChoice(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMain_CounterattackChoice() returned false");
                     return false;
                  }
                  break;
               case GameAction.MovementAdvanceFireChoice:
               case GameAction.UpdateBattleBoard:
               case GameAction.BattleRoundSequenceMovementRoll:
               case GameAction.BattleRoundSequenceShermanAdvanceOrRetreat:
                  if (false == UpdateCanvasMovement(gi, action, stacks, buttons))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMovement() returned false for a=" + action.ToString());
                     return false;
                  }
                  break;
               case GameAction.MovementEnemyStrengthChoice:
               case GameAction.MovementEnterAreaUsControl:
                  IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce"); // center thumbnails around task force
                  if (null != taskForce)
                     UpdateScrollbarThumbnails(taskForce.TerritoryCurrent);
                  if (false == UpdateCanvasMovement(gi, action, stacks, buttons))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMovement() returned false  a=" + action.ToString());
                     return false;
                  }
                  if (false == UpdateCanvasMainEnemyStrengthCheckTerritory(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMainEnemyStrengthCheckTerritory() returned false");
                     return false;
                  }
                  break;
               case GameAction.BattleAdvanceFireStart:
                  if( true == gi.IsAdvancingFireChosen)
                  {
                     if (false == UpdateCanvasMainAdvancingMarkerFirePlace(gi))
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
      private void UpdateCanvasMainClear(List<Button> buttons, IStacks stacks, GameAction action)
      {
         Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "UpdateCanvasMainClear(): " + stacks.ToString());
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children) // Clean the Canvas of all marks
         {
            if (ui is Polygon polygon) 
            {
               elements.Add(ui);
            }
            if (ui is Polyline polyline)
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
               if (true == button.Name.Contains("Die"))  // die buttons never disappear - only one copy of them
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
                  Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "UpdateCanvasMainMapItems(): Updating mi=" + mi.Name + " X=" + mi.Location.X.ToString() + " Y=" + mi.Location.Y.ToString());
                  Canvas.SetLeft(b, mi.Location.X);
                  Canvas.SetTop(b, mi.Location.Y);
                  if (true == b.Name.Contains("Smoke"))
                     Canvas.SetZIndex(b, 100);
                  else if (true == b.Name.Contains("Sherman"))
                     Canvas.SetZIndex(b, 9999);
                  else
                     Canvas.SetZIndex(b, 1000);
                  RotateTransform rotateTransform = new RotateTransform();
                  b.RenderTransformOrigin = new Point(0.5, 0.5);
                  rotateTransform.Angle = mi.RotationHull + mi.RotationOffsetHull;
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
      private bool UpdateCanvasMovement(IGameInstance gi, GameAction action, IStacks stacks, List<Button> buttons)
      {
         try
         {
            foreach (IMapItemMove mim in gi.MapItemMoves)
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
               if (false == MovePathAnimate(gi, mim, buttons))
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
               Logger.Log(LogEnum.LE_VIEW_ROTATION, "UpdateCanvasMovement(): mi=" + mim.MapItem.Name + " r=" + mim.MapItem.RotationOffsetHull + " rb=" + mim.MapItem.RotationHull);
               //------------------------------------------
               stacks.Remove(mi); // remove from existing stack
               Logger.Log(LogEnum.LE_VIEW_MIM, "UpdateCanvasMovement(): a=" + action.ToString() + " mi=" + mi.Name + " t=" + mi.TerritoryCurrent.Name + "==>" + mim.NewTerritory.Name + " X=" + mi.Location.X.ToString() + " Y=" + mi.Location.Y.ToString() + " " + stacks.ToString() );
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
      private bool MovePathAnimate(IGameInstance gi, IMapItemMove mim, List<Button> buttons)
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
            Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): b=null for mi=" + mim.MapItem.Name + " mim=" + mim.ToString());
            return false;
         }
         try
         {
            Canvas.SetZIndex(b, 10000); // Move the button to the top of the Canvas
            double diffXOld = theOldXAfterAnimation - mim.MapItem.Location.X;
            double diffYOld = theOldYAfterAnimation - mim.MapItem.Location.Y;
            double xStart = mim.MapItem.Location.X; // get top left point of MapItem
            double yStart = mim.MapItem.Location.Y;
            Logger.Log(LogEnum.LE_VIEW_ROTATION, "+++++++++++++++++MovePathAnimate(): 1 - mi.X=" + xStart.ToString("F0") + "  mi.Y=" + yStart.ToString("F0") + " r=" + mim.MapItem.RotationOffsetHull.ToString("F0") + " dX=" + diffXOld.ToString("F0") + " dY=" + diffYOld.ToString("F0") + " rb=" + mim.MapItem.RotationHull.ToString("F0"));
            PathFigure aPathFigure = new PathFigure() { StartPoint = new System.Windows.Point(xStart, yStart) };
            if (null == mim.BestPath)
            {
               Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): mim.BestPath=null for mi=" + mim.MapItem.Name);
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
            if ( ((true == mim.MapItem.IsVehicle()) || (true == mim.MapItem.IsAntiTankGun())) && (BattlePhase.ConductCrewAction != myGameInstance.BattlePhase))
            {
               double xDiff = xStart - mp.X;
               double yDiff = yStart - mp.Y;
               double angleRotation = Math.Atan2(yDiff, xDiff) * 180 / Math.PI - 90;
               rotateTransform.Angle = angleRotation;
               b.RenderTransform = rotateTransform;
            }
            else
            {
               rotateTransform.Angle = mim.MapItem.RotationOffsetHull + mim.MapItem.RotationHull;
               b.RenderTransform = rotateTransform;
            }
            //----------------------------------------------------
            b.BeginAnimation(Canvas.LeftProperty, xAnimiation);
            b.BeginAnimation(Canvas.TopProperty, yAnimiation);
            mim.MapItem.Location.X = mp.X;
            mim.MapItem.Location.Y = mp.Y;
            if (true == mim.MapItem.Name.Contains("TaskForce"))
            {
               double offset = mim.MapItem.Zoom * Utilities.theMapItemOffset;
               IMapPoint mpTaskForce = new MapPoint(mim.MapItem.Location.X + offset, mim.MapItem.Location.Y + offset);
               EnteredHex newHex = new EnteredHex(gi, mim.NewTerritory, ColorActionEnum.CAE_ENTER, mpTaskForce);
               if (true == newHex.CtorError)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): newHex.Ctor=true");
                  return false;
               }
               gi.EnteredHexes.Add(newHex);  // Move_PathAnimate()
            }
            Logger.Log(LogEnum.LE_VIEW_ROTATION, "-----------------MovePathAnimate(): 2 - mi.X=" + mim.MapItem.Location.X.ToString("F0") + " mi.Y=" + mim.MapItem.Location.Y.ToString("F0") + " r=" + mim.MapItem.RotationOffsetHull.ToString("F0") + " rb=" + mim.MapItem.RotationHull.ToString("F0"));
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
      //---------------------------------------
      private bool UpdateCanvasShowFeats()
      {
         myMoveButtons.Clear();
         myBattleButtons.Clear();
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Image img)
            {
               if (true == img.Name.Contains("Canvas"))
                  continue;
               elements.Add(ui);
            }
            if (ui is Polygon polygon)
               elements.Add(ui);
            if (ui is TextBlock tb)
               elements.Add(ui);
            if (ui is Label label)
               elements.Add(ui);
            if (ui is Button b)
            {
               if (false == b.Name.Contains("Die"))
                  elements.Add(ui);
            }
         }
         foreach (UIElement ui1 in elements)
            myCanvasMain.Children.Remove(ui1);
         //------------------------------------
         myCanvasMain.LayoutTransform = new ScaleTransform(1.0, 1.0);
         double centerX = myCanvasMain.ActualWidth * 0.5;
         double centerY = myCanvasMain.ActualHeight * 0.5;
         //------------------------------------
         GameFeat featChange;
         Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "UpdateCanvas_ShowFeats(): \n  Feats=" + GameEngine.theInGameFeats.ToString() + " \n SFeats=" + GameEngine.theStartingFeats.ToString());
         if ( false == GameEngine.theInGameFeats.GetFeatChange(GameEngine.theStartingFeats, out featChange)) // Update_CanvasShowFeats()
         {
            Logger.Log(LogEnum.LE_ERROR, "Update_CanvasShowFeats(): Get_FeatChange() returned false");
            return false;
         }
         if (true == String.IsNullOrEmpty(featChange.Key))
         {
            Logger.Log(LogEnum.LE_ERROR, "Update_CanvasShowFeats(): featChange=empty");
            return false;
         }
         Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "UpdateCanvas_ShowFeats(): Change=" + featChange.ToString());
         //------------------------------------
         double sizeOfImage = Math.Min(myCanvasMain.ActualHeight, myCanvasMain.ActualWidth);
         BitmapImage bmi1 = new BitmapImage();
         bmi1.BeginInit();
         bmi1.UriSource = new Uri(MapImage.theImageDirectory + "StarReward.gif", UriKind.Absolute);
         bmi1.EndInit();
         Image imgFeat = new Image { Source = bmi1, Height = sizeOfImage, Width = sizeOfImage, Name = "Feat" };
         ImageBehavior.SetAnimatedSource(imgFeat, bmi1);
         myCanvasMain.Children.Add(imgFeat);
         double X = centerX - (sizeOfImage * 0.5);
         double Y = centerY - (sizeOfImage * 0.5);
         Canvas.SetLeft(imgFeat, X);
         Canvas.SetTop(imgFeat, Y);
         Canvas.SetZIndex(imgFeat, 99998);
         myCanvasMain.MouseDown += MouseDownGameFeat;
         //-------------------------------------
         System.Windows.Controls.Label labelTitle = new System.Windows.Controls.Label() { Content = "Game Feat Completed!", FontStyle = FontStyles.Italic, FontSize = 24, FontWeight = FontWeights.Bold, FontFamily = myFontFam, VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center };
         myCanvasMain.Children.Add(labelTitle);
         System.Windows.Controls.Label labelForFeat = new System.Windows.Controls.Label() { Content = GameFeats.GetFeatMessage(featChange), FontSize = 24, FontWeight = FontWeights.Bold, FontFamily = myFontFam, VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center };
         myCanvasMain.Children.Add(labelForFeat);
         System.Windows.Controls.Label labelClick = new System.Windows.Controls.Label() { Content = "Click to continue", FontStyle = FontStyles.Italic, FontSize = 24, FontWeight = FontWeights.Bold, FontFamily = myFontFam, VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center };
         myCanvasMain.Children.Add(labelClick);
         labelTitle.UpdateLayout();
         labelForFeat.UpdateLayout();
         labelClick.UpdateLayout();
         //-------------------------------------
         double X1 = centerX - labelTitle.ActualWidth * 0.5;
         double Y1 = centerY - labelTitle.ActualHeight * 0.5;
         double X2 = centerX - labelForFeat.ActualWidth * 0.5;
         double Y2 = centerY + labelTitle.ActualHeight * 0.5;
         double X3 = centerX - labelClick.ActualWidth * 0.5;
         double Y3 = centerY + labelTitle.ActualHeight * 0.5 + labelForFeat.ActualHeight;
         //-------------------------------------
         Canvas.SetLeft(labelTitle, X1);
         Canvas.SetTop(labelTitle, Y1);
         Canvas.SetZIndex(labelTitle, 99999);
         Canvas.SetLeft(labelForFeat, X2);
         Canvas.SetTop(labelForFeat, Y2);
         Canvas.SetZIndex(labelForFeat, 99999);
         Canvas.SetLeft(labelClick, X3);
         Canvas.SetTop(labelClick, Y3);
         Canvas.SetZIndex(labelClick, 99999);
         //-------------------------------------
         GameFeat? startingFeat = GameEngine.theStartingFeats.Find(featChange.Key);
         if ( null == startingFeat )
         {
            Logger.Log(LogEnum.LE_ERROR, "Update_CanvasShowFeats(): startingFeat=null");
            return false;
         }
         startingFeat.Value = featChange.Value;
         return true;
      }
      private bool UpdateCanvasShowStatistics(IGameInstance gi)
      {
         if( null == myDieRoller )
         {
            Logger.Log(LogEnum.LE_ERROR, "Update_CanvasShowStatistics(): myDieRoller=null");
            return false;
         }
         myMoveButtons.Clear();
         myBattleButtons.Clear();
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Image img)
            {
               if ("Map" == img.Name)
                  continue;
               elements.Add(ui);
            }
            if (ui is TextBlock tb)
               elements.Add(ui);
            if (ui is Label label)
               elements.Add(ui);
            if (ui is Button b)
            {
               if (false == b.Name.Contains("Die"))
                  elements.Add(ui);
            }
         }
         foreach (UIElement ui1 in elements)
            myCanvasMain.Children.Remove(ui1);
         myDieRoller.HideDie();
         //-------------------------------
         GameStatistic statNumGames = gi.Statistics.Find("NumGames"); // current game always set to one
         statNumGames.Value = 1;
         //-------------------------------
         myTextBoxMarquee.Inlines.Clear();
         myTextBoxMarquee.Inlines.Add(new Run("Current Game Statistics:") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic, TextDecorations = TextDecorations.Underline, Foreground = Brushes.Red });
         if (false == UpdateCanvasShowStatsText(myTextBoxMarquee, gi.Statistics, Brushes.Red))
         {
            Logger.Log(LogEnum.LE_VIEW_SHOW_SETTINGS, "GameViewerWindow.Update_CanvasShowStatistics(): UpdateCanvasShowStatsText() returned false");
            return false;
         }
         //-------------------------------
         Option optionSingleDayGame = gi.Options.Find("SingleDayScenario");
         if( true == optionSingleDayGame.IsEnabled)
         {
            Logger.Log(LogEnum.LE_VIEW_SHOW_STATS, "Update_CanvasShowStatsAdds(): Before==>GameEngine.theSingleDayStatistics=" + GameEngine.theSingleDayStatistics.ToString());
            UpdateCanvasShowStatsAdds(gi.Statistics, GameEngine.theSingleDayStatistics);
            Logger.Log(LogEnum.LE_VIEW_SHOW_STATS, "Update_CanvasShowStatsAdds(): After==>GameEngine.theSingleDayStatistics=" + GameEngine.theSingleDayStatistics.ToString());
            GameStatistic stat0NumGames = GameEngine.theSingleDayStatistics.Find("NumGames");
            if (1 < stat0NumGames.Value)
            {
               myTextBoxMarquee.Inlines.Add(new LineBreak());
               myTextBoxMarquee.Inlines.Add(new LineBreak());
               string title2 = "Single Games Statistics:";
               myTextBoxMarquee.Inlines.Add(new Run(title2) { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic, TextDecorations = TextDecorations.Underline, Foreground = Brushes.Blue });
               UpdateCanvasShowStatsText(myTextBoxMarquee, GameEngine.theSingleDayStatistics, Brushes.Blue);
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_VIEW_SHOW_STATS, "Update_CanvasShowStatsAdds(): Before==>GameEngine.theCampaignStatistics=" + GameEngine.theCampaignStatistics.ToString());
            UpdateCanvasShowStatsAdds(gi.Statistics, GameEngine.theCampaignStatistics);
            Logger.Log(LogEnum.LE_VIEW_SHOW_STATS, "Update_CanvasShowStatsAdds(): After==>GameEngine.theCampaignStatistics=" + GameEngine.theCampaignStatistics.ToString());
            GameStatistic stat1NumGames = GameEngine.theCampaignStatistics.Find("NumGames");
            if (1 < stat1NumGames.Value)
            {
               myTextBoxMarquee.Inlines.Add(new LineBreak());
               myTextBoxMarquee.Inlines.Add(new LineBreak());
               string title2 = "Campaign Games Statistics:";
               myTextBoxMarquee.Inlines.Add(new Run(title2) { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic, TextDecorations = TextDecorations.Underline, Foreground = Brushes.Blue });
               UpdateCanvasShowStatsText(myTextBoxMarquee, GameEngine.theCampaignStatistics, Brushes.Blue);
            }
         }
         //-------------------------------
         Logger.Log(LogEnum.LE_VIEW_SHOW_STATS, "Update_CanvasShowStatsAdds(): Before====>GameEngine.theTotalStatistics=" + GameEngine.theTotalStatistics.ToString());
         UpdateCanvasShowStatsAdds(gi.Statistics, GameEngine.theTotalStatistics);
         Logger.Log(LogEnum.LE_VIEW_SHOW_STATS, "Update_CanvasShowStatsAdds(): After====>GameEngine.theTotalStatistics=" + GameEngine.theTotalStatistics.ToString());
         GameStatistic stat0NumGamesAfter = GameEngine.theSingleDayStatistics.Find("NumGames");
         GameStatistic stat1NumGamesAfter = GameEngine.theCampaignStatistics.Find("NumGames"); 
         GameStatistic stat2NumGames = GameEngine.theTotalStatistics.Find("NumGames");
         if ((stat0NumGamesAfter.Value != stat2NumGames.Value) && (stat1NumGamesAfter.Value != stat2NumGames.Value)) 
         {
            myTextBoxMarquee.Inlines.Add(new LineBreak());
            myTextBoxMarquee.Inlines.Add(new LineBreak());
            string title2 = "All Games Statistics:";
            myTextBoxMarquee.Inlines.Add(new Run(title2) { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic, TextDecorations = TextDecorations.Underline, Foreground=Brushes.Goldenrod });
            UpdateCanvasShowStatsText(myTextBoxMarquee, GameEngine.theTotalStatistics, Brushes.Goldenrod);
         }
         //-------------------------------
         myCanvasMain.ClipToBounds = true;
         myCanvasMain.Children.Add(myTextBoxMarquee);
         myTextBoxMarquee.UpdateLayout();
         //-------------------------------
         DoubleAnimation doubleAnimation = new DoubleAnimation();
         doubleAnimation.From = -myTextBoxMarquee.ActualHeight;
         doubleAnimation.To = myCanvasMain.ActualHeight;
         doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
         doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(MARQUEE_SCROLL_ANMINATION_TIME));
         Storyboard.SetTargetName(doubleAnimation, "tbMarquee");
         Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Canvas.BottomProperty));
         myStoryboard.Children.Add(doubleAnimation);
         myStoryboard.Begin(this, true);
         //-------------------------------
         Logger.Log(LogEnum.LE_VIEW_SHOW_SETTINGS, "GameViewerWindow.Update_CanvasShowStatistics(): Called Save_DefaultsToSettings()");
         SaveDefaultsToSettings();
         return true;
      }
      private void UpdateCanvasShowStatsAdds(GameStatistics statistics, GameStatistics totalStatistics)
      {
         //-------------------------------------
         foreach (GameStatistic stat in statistics)
         {
            if (true == stat.Key.Contains("Num"))
            {
               GameStatistic statAllNum = totalStatistics.Find(stat.Key);
               statAllNum.Value += stat.Value;
            }
            else if (true == stat.Key.Contains("Max"))
            {
               GameStatistic statMax = totalStatistics.Find(stat.Key);
               if (statMax.Value < stat.Value)
                  statMax.Value = stat.Value;
            }
            else if (true == stat.Key.Contains("Min"))
            {
               GameStatistic statMin = totalStatistics.Find(stat.Key);
               Logger.Log(LogEnum.LE_VIEW_SHOW_STATS_MIN, "Perform_EndCheck(): key=" + stat.Key + " statMin.Value=" + statMin.Value.ToString() + " stat.Value=" + stat.Value.ToString());
               if ( (stat.Value < statMin.Value) || (0 == statMin.Value)  )
               {
                  if( 0 < stat.Value )
                  {
                     Logger.Log(LogEnum.LE_VIEW_SHOW_STATS_MIN, "Perform_EndCheck(): (stat.Value=" + stat.Value.ToString() + ") < (statMin.Value=" + statMin.Value.ToString() + ")");
                     statMin.Value = stat.Value;
                  }
               }
            }
         }
      }
      private bool UpdateCanvasShowStatsText(TextBlock tb, GameStatistics statistics, Brush brushFont)
      {
         GameStatistic numGames = statistics.Find("NumGames"); // check that at least one
         if( 0 == numGames.Value )
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasShowStatsText(): numGames=0");
            return false;
         }
         GameStatistic numDays = statistics.Find("NumDays");
         if (0 == numDays.Value)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasShowStatsText(): numDays=0");
            return false;
         }
         Option optionSingleDayGame = myGameInstance.Options.Find("SingleDayScenario");
         GameStatistic numWins = statistics.Find("NumWins");
         GameStatistic numOfBattles = statistics.Find("NumOfBattles");
         GameStatistic numOfKilledCrewman = statistics.Find("NumOfKilledCrewman");
         GameStatistic numShermanExplodes = statistics.Find("NumShermanExplodes");
         GameStatistic numShermanBurns = statistics.Find("NumShermanBurns");
         GameStatistic numShermanPenetrations = statistics.Find("NumShermanPenetration");
         GameStatistic numPanzerfaultAttacks = statistics.Find("NumPanzerfaustAttack");
         GameStatistic numPanzerfaultDeaths = statistics.Find("NumPanzerfaustDeath");
         GameStatistic numMineAttack = statistics.Find("NumMineAttack");
         GameStatistic numMineImmobilization = statistics.Find("NumMineImmobilization");
         int numLostTanks = numShermanExplodes.Value + numShermanPenetrations.Value;
         int numKilledEnemyFriendlyFire = 0;
         int numKilledEnemyYourFire = 0;
         foreach(GameStatistic stat in statistics)
         {
            if (true == stat.Key.Contains("FriendlyFire")) numKilledEnemyFriendlyFire += stat.Value;
            if (true == stat.Key.Contains("YourFire")) numKilledEnemyYourFire += stat.Value;
         }
         int numKilledEnemyTotal = numKilledEnemyFriendlyFire + numKilledEnemyYourFire;
         int average = 0;
         if (1 < numGames.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Games = " + numGames.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground=brushFont });
            int winRatio = (int)Math.Round(100.0 * ((double)numWins.Value) / (double)numGames.Value);
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("% Wins = " + winRatio.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            if (true == optionSingleDayGame.IsEnabled)
            {
               GameStatistic victoryPointsGame = statistics.Find("MaxPointsSingleDayGame");
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Max Victory Points = " + victoryPointsGame.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            else
            {
               GameStatistic victoryPointsGame = statistics.Find("MaxPointsCampaignGame");
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Max Victory Points = " + victoryPointsGame.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            GameStatistic maxCrewRatingWin = statistics.Find("MaxCrewRatingWin");
            if (0 < maxCrewRatingWin.Value)
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Max Crew Rating with Win = " + maxCrewRatingWin.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            GameStatistic minCrewRatingWin = statistics.Find("MinCrewRatingWin");
            if (0 < minCrewRatingWin.Value)
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Min Crew Rating with Win = " + minCrewRatingWin.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            tb.Inlines.Add(new LineBreak());
            average = numDays.Value / numGames.Value;
            tb.Inlines.Add(new Run("Average Days per Game = " + average.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            //-------------------------------------
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("------------------------------") { FontWeight = FontWeights.Bold, Foreground = brushFont });
            if (0 < numLostTanks)
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Lost Tanks = " + numLostTanks.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
               tb.Inlines.Add(new LineBreak());
               average = numDays.Value / numLostTanks;
               tb.Inlines.Add(new Run("Average Days per Lost Tanks = " + average.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            if( 0 < numOfKilledCrewman.Value )
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Killed Crewmen = " + numOfKilledCrewman.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
               tb.Inlines.Add(new LineBreak());
               average = numDays.Value / numOfKilledCrewman.Value;
               tb.Inlines.Add(new Run("Average Days/Killed Crewman = " + average.ToString()) { FontWeight = FontWeights.Bold });
               average = numOfKilledCrewman.Value / numGames.Value;
               tb.Inlines.Add(new Run("Average Crewmen Killed per Game = " + average.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            if( 0 < numKilledEnemyTotal )
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Killed Enemy = " + numKilledEnemyTotal.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
               tb.Inlines.Add(new LineBreak());
               average = numKilledEnemyTotal / numGames.Value;
               tb.Inlines.Add(new Run("Average Killed Enemy per Day = " + average.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            if (0 < numKilledEnemyFriendlyFire)
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Killed by Friendly Fire = " + numKilledEnemyFriendlyFire.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
               tb.Inlines.Add(new LineBreak());
               average = numKilledEnemyFriendlyFire / numGames.Value;
               tb.Inlines.Add(new Run("Average FF Killed per Game = " + average.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            if (0 < numKilledEnemyYourFire)
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Killed by Your Tank = " + numKilledEnemyYourFire.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
               tb.Inlines.Add(new LineBreak());
               average = numKilledEnemyYourFire / numGames.Value;
               tb.Inlines.Add(new Run("Average Your Kills per Game = " + average.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
         }
         else
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("End Date = " + TableMgr.GetDate(myGameInstance.Day-1)) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            //-------------------------------------
            if (0 < numWins.Value)
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Game Won!") { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            else
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Game Lost!") { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            if( true == optionSingleDayGame.IsEnabled)
            {
               GameStatistic victoryPointsGame = statistics.Find("MaxPointsSingleDayGame");
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Victory Points = " + victoryPointsGame.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });

            }
            else
            {
               GameStatistic victoryPointsGame = statistics.Find("MaxPointsCampaignGame");
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Victory Points = " + victoryPointsGame.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
            if (null == lastReport)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_ShowStatsText(): lastReport=null");
               return false;
            }
            int crewRating = lastReport.Commander.Rating + lastReport.Gunner.Rating + lastReport.Loader.Rating + lastReport.Driver.Rating + lastReport.Assistant.Rating;
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Crew Rating = " + crewRating.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            //-------------------------------------
            if (1 < numDays.Value)
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Days = " + numDays.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("------------------------------") { FontWeight = FontWeights.Bold, Foreground = brushFont });
            if ( 0 < numLostTanks )
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Lost Tanks = " + numLostTanks.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            if (0 < numOfKilledCrewman.Value)
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Killed Crewmen = " + numOfKilledCrewman.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
               tb.Inlines.Add(new LineBreak());
               average = numDays.Value / numOfKilledCrewman.Value;
               tb.Inlines.Add(new Run("Average Days/Killed Crewman = " + average.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            if (0 < numKilledEnemyTotal)
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Killed Enemy = " + numKilledEnemyTotal.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            if (0 < numKilledEnemyFriendlyFire)
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Killed by Friendly Fire = " + numKilledEnemyFriendlyFire.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
            //-------------------------------------
            if (0 < numKilledEnemyYourFire)
            {
               tb.Inlines.Add(new LineBreak());
               tb.Inlines.Add(new Run("Killed by Your Tank = " + numKilledEnemyYourFire.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            }
         }
         //-------------------------------------
         tb.Inlines.Add(new LineBreak());
         tb.Inlines.Add(new Run("------------------------------") { FontWeight = FontWeights.Bold, Foreground = brushFont });
         GameStatistic numPurpleHearts = statistics.Find("NumPurpleHearts");
         if (0 < numPurpleHearts.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Num Purple Hearts = " + numPurpleHearts.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         GameStatistic numBronzeStars = statistics.Find("NumBronzeStars");
         if (0 < numBronzeStars.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Num Bronze Stars = " + numBronzeStars.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         GameStatistic numSilverStars = statistics.Find("NumSilverStars");
         if (0 < numSilverStars.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Num Silver Stars = " + numSilverStars.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         GameStatistic numDistinguishedCrosses = statistics.Find("NumDistinguishedCrosses");
         if (0 < numDistinguishedCrosses.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Num Distinguished Crosses = " + numDistinguishedCrosses.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         GameStatistic numMedalOfHonors = statistics.Find("NumMedalOfHonors");
         if (0 < numMedalOfHonors.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Num Medal of Honor = " + numMedalOfHonors.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         //-------------------------------------
         if (0 < numPanzerfaultAttacks.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Panzerfaust Attacks = " + numPanzerfaultAttacks.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            tb.Inlines.Add(new LineBreak());
            average = numPanzerfaultDeaths.Value / numPanzerfaultAttacks.Value;
            tb.Inlines.Add(new Run("Average Deaths per PzFaust Attack = " + average.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         if (0 < numMineAttack.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Mine Attacks = " + numMineAttack.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
            tb.Inlines.Add(new LineBreak());
            average = numMineImmobilization.Value / numMineAttack.Value;
            tb.Inlines.Add(new Run("Average Mine Immobilizations = " + average.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         //-------------------------------------
         tb.Inlines.Add(new LineBreak());
         tb.Inlines.Add(new Run("------------------------------") { FontWeight = FontWeights.Bold, Foreground = brushFont });
         GameStatistic maxDayBetweenCombat = statistics.Find("MaxDayBetweenCombat");
         if (1 < maxDayBetweenCombat.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Max Days Between Combat = " + maxDayBetweenCombat.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         GameStatistic maxRollsForAirSupport = statistics.Find("MaxRollsForAirSupport");
         if (1 < maxRollsForAirSupport.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Max Rolls For Air Strikes = " + maxRollsForAirSupport.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         GameStatistic maxRollsForArtillerySupport = statistics.Find("MaxRollsForArtillerySupport");
         if (1 < maxRollsForArtillerySupport.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Max Rolls For Artillery Support = " + maxRollsForArtillerySupport.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         GameStatistic maxEnemiesInOneBattle = statistics.Find("MaxEnemiesInOneBattle");
         if (0 < maxEnemiesInOneBattle.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Max Enemies In One Battle = " + maxEnemiesInOneBattle.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         GameStatistic maxRoundsOfCombat = statistics.Find("MaxRoundsOfCombat");
         if (0 < maxRoundsOfCombat.Value)
         {
            tb.Inlines.Add(new LineBreak());
            tb.Inlines.Add(new Run("Max Rounds of Combat = " + maxRoundsOfCombat.Value.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
         }
         //-------------------------------------
         tb.Inlines.Add(new LineBreak());
         tb.Inlines.Add(new Run("------------------------------") { FontWeight = FontWeights.Bold, Foreground = brushFont });
         foreach (GameStatistic kill in statistics)
         {
            StringBuilder sb = new StringBuilder();
            if (true == kill.Key.Contains("FriendlyFire"))
            {
               if (0 < kill.Value)
               {
                  int index = kill.Key.IndexOf("Friendly"); //NumKillLwFriendlyFire
                  sb.Append("Killed ");
                  sb.Append(kill.Key.Substring(7, index - 7));
                  sb.Append(" by Friendly Fire = ");
                  sb.Append(kill.Value.ToString());
                  tb.Inlines.Add(new LineBreak());
                  tb.Inlines.Add(new Run(sb.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
               }
            }
         }
         tb.Inlines.Add(new LineBreak());
         tb.Inlines.Add(new Run("------------------------------") { FontWeight = FontWeights.Bold, Foreground = brushFont });
         foreach (GameStatistic kill in statistics)
         {
            StringBuilder sb = new StringBuilder();
            if (true == kill.Key.Contains("YourFire"))
            {
               if (0 < kill.Value)
               {
                  int index = kill.Key.IndexOf("Your");
                  sb.Append("Killed ");
                  sb.Append(kill.Key.Substring(7, index-7));
                  sb.Append(" by Your Fire = ");
                  sb.Append(kill.Value.ToString());
                  tb.Inlines.Add(new LineBreak());
                  tb.Inlines.Add(new Run(sb.ToString()) { FontWeight = FontWeights.Bold, Foreground = brushFont });
               }
            }
         }
         return true;
      }
      //---------------------------------------
      private bool UpdateCanvasMainSpottingLoader(IGameInstance gi, GameAction action)
      {
         Logger.Log(LogEnum.LE_SHOW_MAIN_CLEAR, "UpdateCanvasMainSpottingLoader(): SSSSSSS setting ellipse action=" + action.ToString());
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
      private bool UpdateCanvasMainAdvancingMarkerFirePlace(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): lastReport=null");
            return false;
         }
         //-----------------------------------------------------------------
         string[] sectors = new string[8] { "B4L", "B4M", "B4C", "B6M", "B6C", "B9L", "B9M", "B9C" };
         foreach (string s in sectors)
         {
            ITerritory? t = Territories.theTerritories.Find(s);
            if( null == t )
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMainAdvancingMarkerFirePlace(): t=null for tName=" + s);
               return false;
            }
            if (((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling"))) ) 
            {
               if( ('6' == s[1]) && ('C' != s[2])) // only close range allowed in Fog or Falling Snow for sector 6-8
                  continue;
            }
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.Name };
            aPolygon.MouseDown += MouseDownPolygonPlaceAdvanceFire;
            myPolygons.Add(aPolygon);
            myCanvasMain.Children.Add(aPolygon);
            Canvas.SetZIndex(aPolygon, 101);
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
         if ( (BattlePhase.None == gi.BattlePhase) || (EnumMainImage.MI_Battle != CanvasImageViewer.theMainImage) )
            return true;
         string tName = gi.BattlePhase.ToString();
         if (true == gi.IsCounterattackAmbush)
            tName = "Ambush";
         ITerritory? t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasAnimateBattlePhase(): t=null for name=" + tName);
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
                  if( (null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)) )
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B2M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)))
                     highlightedTerritoryNames.Add(adjName);
                  break;
               case "2":
                  adjName = "B1M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)))
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B3M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)))
                     highlightedTerritoryNames.Add(adjName);
                  break;
               case "3":
                  adjName = "B2M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)))
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B4M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)))
                     highlightedTerritoryNames.Add(adjName);
                  break;
               case "4":
                  adjName = "B3M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)))
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B6M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)))
                     highlightedTerritoryNames.Add(adjName);
                  break;
               case "6":
                  adjName = "B4M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)))
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B9M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)))
                     highlightedTerritoryNames.Add(adjName);
                  break;
               case "9":
                  adjName = "B6M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)))
                     highlightedTerritoryNames.Add(adjName);
                  adjName = "B1M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector, true)))
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
               Canvas.SetZIndex(aPolygon, 101);
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
         Canvas.SetZIndex(aPolygon, 101);
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
            Canvas.SetZIndex(aPolygon, 101);
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
            Canvas.SetZIndex(aPolygon, 101);
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
            Canvas.SetZIndex(aPolygon, 101);
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
            Canvas.SetZIndex(aPolygon, 101);
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_MainEnterArea(): taskForce=null");
            return false;
         }
         //--------------------------------
         List<String> sTerritories = taskForce.TerritoryCurrent.Adjacents;
         foreach (string s in sTerritories)
         {
            ITerritory? t = Territories.theTerritories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvas_MainEnterArea(): 1 t=null for " + s);
               return false;
            }
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.Name };
            aPolygon.MouseDown += MouseDownPolygonEnterArea;
            myPolygons.Add(aPolygon);
            myCanvasMain.Children.Add(aPolygon);
            Canvas.SetZIndex(aPolygon, 101);
         }
         return true;
      }
      private bool UpdateCanvasMainCounterattackChoice(IGameInstance gi, GameAction action)
      {
         myPolygons.Clear();

         foreach (ITerritory t in gi.CounterattachRetreats)
         {
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in t.Points)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.Name };
            aPolygon.MouseDown += MouseDownPolygonCounterattackChoice;
            myPolygons.Add(aPolygon);
            myCanvasMain.Children.Add(aPolygon);
            Canvas.SetZIndex(aPolygon, 101);
         }
         return true;
      }
      //---------------------------------------
      private bool UpdateCanvasPath(IGameInstance gi)
      {
         try
         {
            PointCollection aPointCollection = new PointCollection();
            foreach (EnteredHex hex in gi.EnteredHexes)
            {
               Ellipse aEllipse = new Ellipse
               {
                  Name = hex.Identifer,
                  Fill = mySolidColorBrushClear,
                  StrokeThickness = 2,
                  Stroke = mySolidColorBrushBlack,
                  Width = theEllipseDiameter,
                  Height = theEllipseDiameter
               };
               aEllipse.MouseEnter += this.MouseEnterEllipse;
               aEllipse.MouseLeave += this.MouseLeaveEllipse;
               SolidColorBrush brush = mySolidColorBrushBlack;
               switch (hex.ColorAction)
               {
                  case ColorActionEnum.CAE_START:
                     brush = mySolidColorBrushGold;
                     break;
                  case ColorActionEnum.CAE_ENTER:
                     brush = mySolidColorBrushGreen;
                     break;
                  case ColorActionEnum.CAE_RETREAT:
                     brush = mySolidColorBrushRed;
                     break;
                  case ColorActionEnum.CAE_STOP:
                     brush = mySolidColorBrushWhite;
                     break;
                  default:
                     break;
               }
               aEllipse.Stroke = brush;
               aEllipse.Fill = brush;
               //-----------------------------------------
               System.Windows.Point p = new System.Windows.Point(hex.MapPoint.X, hex.MapPoint.Y);
               aPointCollection.Add(p);
               Canvas.SetLeft(aEllipse, hex.MapPoint.X - theEllipseOffset);
               Canvas.SetTop(aEllipse, hex.MapPoint.Y - theEllipseOffset);
               Canvas.SetZIndex(aEllipse, 9999);
               myCanvasMain.Children.Add(aEllipse);
            }
            Polyline aPolyline = new Polyline();
            aPolyline.Stroke = mySolidColorBrushWhite;
            aPolyline.StrokeThickness = 2;
            aPolyline.Points = aPointCollection;
            aPolyline.StrokeDashArray = myDashArray;
            Canvas.SetZIndex(aPolyline, 9999);
            myCanvasMain.Children.Add(aPolyline);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasPath(): EXCEPTION THROWN e=" + e.ToString());
            return false;
         }
         return true;
      }
      //-------------CONTROLLER FUNCTIONS---------------------------------
      private void MouseDownPolygonHatches(object sender, MouseButtonEventArgs e)
      {
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonHatches(): lastReport=null");
            return;
         }
         string tType = lastReport.TankCardNum.ToString();
         //--------------------------------------------
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonHatches(): clickedPolygon=null");
            return;
         }
         ITerritory? t = Territories.theTerritories.Find(clickedPolygon.Name, tType);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonHatches(): t=null for " + clickedPolygon.Name.ToString() + " tType=" + tType);
            return;
         }
         string[] crewmembers = new string[4] { "Driver", "Assistant", "Commander", "Loader" };
         foreach (string crewmember in crewmembers)
         {
            if (true == t.Name.Contains(crewmember))
            {
               ICrewMember? cm = myGameInstance.GetCrewMemberByRole(crewmember);
               if (null == cm)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonHatches(): myGameInstance.Driver=null for " + clickedPolygon.Name.ToString());
                  return;
               }
               cm.IsButtonedUp = false;
               Logger.Log(LogEnum.LE_SHOW_CREW_BU, "MouseDownPolygonHatches(): cm.Name= " + cm.Name + " role=" + cm.Role);
               string name = crewmember + Utilities.MapItemNum.ToString() + "_OpenHatch";
                Utilities.MapItemNum++;
               IMapItem mi = new MapItem(name, 1.0, "c15OpenHatch", t);
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
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDown_PolygonHatches(): lastReport=null");
            return;
         }
         string tType = lastReport.TankCardNum.ToString();
         //--------------------------------------------
         myIsInitialAmmoReloadButtonCreated = false;
         //--------------------------------------------
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDown_PolygonHatches(): clickedPolygon=null");
            return;
         }
         ITerritory? newT = Territories.theTerritories.Find(clickedPolygon.Name, tType);
         if (null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDown_PolygonHatches(): t=null for " + clickedPolygon.Name.ToString() + " tType=" + tType);
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
            string miName = "GunLoadInGun" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            gunLoad = new MapItem(miName, 1.0, "c17GunLoad", newT);
            myGameInstance.GunLoads.Add(gunLoad);
         }
         gunLoad.TerritoryCurrent = newT;
         double delta = gunLoad.Zoom * Utilities.theMapItemOffset;
         gunLoad.Location.X = newT.CenterPoint.X - delta;
         gunLoad.Location.Y = newT.CenterPoint.Y - delta;
         Logger.Log(LogEnum.LE_SHOW_MAPITEM_TANK, "MouseDown_PolygonHatches(): gunLoad=" + gunLoad.Name + " loc=" + gunLoad.Location.ToString() + " t=" + newT.Name + " tLoc=" + newT.CenterPoint.ToString());
         GameAction outAction = GameAction.BattleRoundSequenceAmmoOrders; //  MouseDown_PolygonHatches()
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
         GameAction outAction = GameAction.BattleRoundSequenceAmmoOrders; // MouseDown_PolygonAmmoActions()
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
         IMapItem? spotter = null;
         foreach (IStack stack in myGameInstance.BattleStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.Name.Contains("LoaderSpot"))
                  spotter = mi;
            }
         }
         if (null == spotter)
         {
            string name = "LoaderSpot" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            myGameInstance.BattleStacks.Add(new MapItem(name, 1.0, "c18LoaderSpot", t));
         }
         else
         {
            spotter.TerritoryCurrent = t;
            double offset = spotter.Zoom * Utilities.theMapItemOffset;
            spotter.Location.X = t.CenterPoint.X - offset;
            spotter.Location.Y = t.CenterPoint.Y - offset;
         }
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
         IMapItem? spotter = null;
         foreach (IStack stack in myGameInstance.BattleStacks) 
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.Name.Contains("CommanderSpot"))
               {
                  spotter = mi;
                  break;
               }
            }
         }
         if (null == spotter)
         {
            string name = "CommanderSpot" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            myGameInstance.BattleStacks.Add(new MapItem(name, 1.0, "c19CommanderSpot", t));
         }
         else
         {
            spotter.TerritoryCurrent = t;
            double offset = spotter.Zoom * Utilities.theMapItemOffset;
            spotter.Location.X = t.CenterPoint.X - offset;
            spotter.Location.Y = t.CenterPoint.Y - offset;
         }
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
      private void MouseDownPolygonCounterattackChoice(object sender, MouseButtonEventArgs e)
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
         GameAction outAction = GameAction.BattleRoundSequenceShermanRetreatChoiceEnd;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void MouseDownPolygonPlaceAdvanceFire(object sender, MouseButtonEventArgs e)
      {
         Polygon? clickedPolygon = sender as Polygon;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDown_PolygonPlaceAdvanceFire(): clickedPolygon=null");
            return;
         }
         myGameInstance.AdvanceFire = Territories.theTerritories.Find(clickedPolygon.Name);
         if (null == myGameInstance.AdvanceFire)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDown_PolygonPlaceAdvanceFire(): t=null for " + clickedPolygon.Name.ToString());
            return;
         }
         GameAction outAction = GameAction.Error;
         if(GamePhase.BattleRoundSequence == myGameInstance.GamePhase)
         {
            outAction = GameAction.BattleRoundSequenceMgPlaceAdvanceFire;
         }
         else if( (GamePhase.Preparations == myGameInstance.GamePhase) || (GamePhase.Battle == myGameInstance.GamePhase) )
         {
            outAction = GameAction.BattlePlaceAdvanceFire;
         }
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
         Logger.Log(LogEnum.LE_SHOW_ORDERS_MENU, "ClickButtonMapItem(): clicking button=" + button.Name );
         //====================================================
         if ((true == button.Name.Contains("OpenHatch")) && ((true == myGameInstance.IsHatchesActive) || (BattlePhase.MarkCrewAction == myGameInstance.BattlePhase)))
         {
            string[] crewmembers = new string[4] { "Driver", "Assistant", "Commander", "Loader" };
            foreach (string role in crewmembers)
            {
               ICrewMember? crewMember = myGameInstance.GetCrewMemberByRole(role);
               if (null == crewMember)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ClickButtonMapItem(): role=" + role);
                  return;
               }
               //----------------------------------
               if (true == button.Name.Contains(role)) // Remove the open hatch
               {
                  crewMember.IsButtonedUp = true;
                  Logger.Log(LogEnum.LE_SHOW_CREW_BU, "ClickButtonMapItem(): cm.Name= " + crewMember.Name + " role=" + role);
                  myGameInstance.Hatches.Remove(button.Name);
                  this.myTankButtons.Remove(button);
                  myCanvasTank.Children.Remove(button);
                  //----------------------------------
                  IMapItems removals = new MapItems();  // Remove Loader, Gunner, and Commander actions that require open hatch - remove Gunner_RepairGun if Loader not repairing
                  foreach (IMapItem crewAction in myGameInstance.CrewActions)
                  {
                     if( false == myGameInstance.IsCrewActionPossibleButtonUp(role, crewAction.Name))
                        removals.Add(crewAction);
                  }
                  foreach (IMapItem crewAction in removals)
                  {
                     myGameInstance.CrewActions.Remove(crewAction);
                     Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "ClickButtonMapItem(): -----------------removing ca=" + crewAction.Name);
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
         //====================================================
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
         //====================================================
         else if ( BattlePhase.ConductCrewAction == myGameInstance.BattlePhase )
         {
            IMapItem? selectedMapItem = myGameInstance.BattleStacks.FindMapItem(button.Name); // selectedMapItem is the new target
            if (null == selectedMapItem)
            {
               Logger.Log(LogEnum.LE_ERROR, "ClickButtonMapItem(): selectedMapItem=null for button.Name=" + button.Name);
               return;
            }
            if( (CrewActionPhase.TankMainGunFire == myGameInstance.CrewActionPhase) || (CrewActionPhase.TankMgFire == myGameInstance.CrewActionPhase) )
            {
               if (true == myGameInstance.Targets.Contains(selectedMapItem))
               {
                  foreach (IMapItem mi in myGameInstance.Targets) // All buttons in the target view are selected
                  {
                     foreach (Button b in myBattleButtons)
                     {
                        if (mi.Name == b.Name)
                           b.BorderThickness = new Thickness(0);
                     }
                  }
                  GameAction outAction = GameAction.Error;
                  if (CrewActionPhase.TankMainGunFire == myGameInstance.CrewActionPhase)
                  {
                     myGameInstance.UndoCmd = new UndoTargetSelectedMainGun(myGameInstance.TargetMainGun);
                     if (null == myGameInstance.TargetMainGun) // no previous target
                     {
                        Logger.Log(LogEnum.LE_SHOW_NUM_SHERMAN_SHOTS, "ClickButtonMapItem(): no previous target zeroize selectedMapItem.EnemyAcquiredShots.Clear()");
                        selectedMapItem.EnemyAcquiredShots.Clear();  // the only thing in this list is the Sherman attacks
                     }
                     else if( selectedMapItem.Name != myGameInstance.TargetMainGun.Name ) // previous target is no longer the target. selectedMapItem is the target. Changed targets.
                     {
                        Logger.Log(LogEnum.LE_SHOW_NUM_SHERMAN_SHOTS, "ClickButtonMapItem(): differt target zeroize selectedMapItem.EnemyAcquiredShots.Clear() and myGameInstance.TargetMainGun.EnemyAcquiredShots.Clear()");
                        selectedMapItem.EnemyAcquiredShots.Clear();
                        myGameInstance.TargetMainGun.EnemyAcquiredShots.Clear();
                     }
                     myGameInstance.TargetMainGun = selectedMapItem;
                     outAction = GameAction.BattleRoundSequenceShermanFiringMainGun;
                  }
                  else
                  {
                     myGameInstance.UndoCmd = new UndoTargetSelectedMachineGun(null);
                     myGameInstance.TargetMg = selectedMapItem;
                     outAction = GameAction.BattleRoundSequenceShermanFiringMachineGun;
                  }
                  selectedMapItem = null;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
               }
            }
         }
         e.Handled = true;
      }
      private void MouseDownGameFeat(object send, MouseEventArgs e)
      {
         System.Windows.Point p = e.GetPosition((UIElement)send);
         HitTestResult result = VisualTreeHelper.HitTest(myCanvasMain, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Image img1)
            {
               if (result.VisualHit == img1)
               {
                  if ("Feat" == img1.Name)
                  {
                     GameAction action = GameAction.Error;
                     GameFeat featChange;
                     Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Mouse_DownGameFeat(): \n Feats=" + GameEngine.theInGameFeats.ToString() + " \n SFeats=" + GameEngine.theStartingFeats.ToString());
                     if (false == GameEngine.theInGameFeats.GetFeatChange(GameEngine.theStartingFeats, out featChange)) // MouseDownGameFeat - EventingDebriefing
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Mouse_DownGameFeat(): Get_FeatChange() returned false");
                        return;
                     }
                     //-------------------------------------
                     if (GamePhase.EndGame == myGameInstance.GamePhase)
                     {
                        if (false == String.IsNullOrEmpty(featChange.Key))
                        {
                           action = GameAction.EndGameShowFeats;
                           Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Mouse_DownGameFeat(): 1-Change=" + featChange.Key);
                        }
                        else
                        {
                           action = GameAction.EndGameShowStats;
                           myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
                        }
                     }
                     else if (GamePhase.EveningDebriefing == myGameInstance.GamePhase)
                     {
                        if (false == String.IsNullOrEmpty(featChange.Key))
                        {
                           action = GameAction.EveningDebriefingShowFeat;
                           Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Mouse_DownGameFeat(): 2-Change=" + featChange.Key);
                        }
                        else
                        {
                           action = GameAction.EveningDebriefingShowFeatEnd;
                           myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
                        }
                     }
                     else if (GamePhase.BattleRoundSequence == myGameInstance.GamePhase)
                     {
                        if (false == String.IsNullOrEmpty(featChange.Key)) // no changed feat
                        {
                           action = GameAction.BattleRoundSequenceShowFeat;
                           Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Mouse_DownGameFeat(): 3-Change=" + featChange.Key);

                        }
                        else
                        {
                           action = GameAction.BattleRoundSequenceShowFeatEnd;
                           myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
                        }
                     }
                     else if (GamePhase.Preparations == myGameInstance.GamePhase)
                     {
                        if (false == String.IsNullOrEmpty(featChange.Key)) // no changed feat
                        {
                           action = GameAction.PreparationsShowFeat;
                           Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Mouse_DownGameFeat(): 4-Change=" + featChange.Key);
                        }
                        else
                        {
                           action = GameAction.PreparationsShowFeatEnd;
                           myCanvasMain.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
                        }
                     }
                     myCanvasMain.MouseDown -= MouseDownGameFeat;
                     e.Handled = true;
                     myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                     return;
                  }
               }
            }
         }
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
      private void MouseEnterEllipse(object sender, MouseEventArgs e)
      {
         Ellipse mousedEllipse = (Ellipse)sender;
         if (null == mousedEllipse)
            return;
         foreach (EnteredHex hex in myGameInstance.EnteredHexes)
         {
            string name = (string)mousedEllipse.Name;
            if (hex.Identifer == name)
            {
               myEllipseDisplayDialog = new EllipseDisplayDialog(hex);
               myEllipseDisplayDialog.Show();
               break;
            }
         }
         //e.Handled = true;  
      }
      private void MouseLeaveEllipse(object sender, MouseEventArgs e)
      {
         Ellipse mousedEllipse = (Ellipse)sender;
         if (null == mousedEllipse)
            return;
         if (null != myEllipseDisplayDialog)
            myEllipseDisplayDialog.Close();
         myEllipseDisplayDialog = null;
         e.Handled = true;
      }
      private void MenuItemCrewActionClick(object sender, RoutedEventArgs e)
      {
         //--------------------------------------
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItem_CrewActionClick(): myGameInstance=null");
            return;
         }
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItem_CrewActionClick(): lastReport=null");
            return;
         }
         string tType = lastReport.TankCardNum.ToString();
         bool isGunnerTrainedInHvss = ( (true == myGameInstance.TrainedGunners.Contains(lastReport.Gunner.Name)) && (false == lastReport.Gunner.IsIncapacitated) );
         //--------------------------------------
         MenuItem? menuitem = sender as MenuItem;
         if( null == menuitem)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItem_CrewActionClick(): menuitem=null");
            return;
         }
         //--------------------------------------
         IMapItem? mi = null;
         string[] aStringArray1 = menuitem.Name.Split(new char[] { '_' });
         if(aStringArray1.Length < 2)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItem_CrewActionClick(): underscore not found in " + menuitem.Name + " len=" + aStringArray1.Length);
            return;
         }
         string sCrewMemberRole = aStringArray1[0];
         //--------------------------------------
         IMapItems crewActionremovals = new MapItems(); 
         foreach (IMapItem crewAction in myGameInstance.CrewActions) // get rid of existing crew action for this crew member
         {
            if ((true == crewAction.Name.Contains(sCrewMemberRole + "_")) || (true == crewAction.Name.Contains("Bail")))
            {
               crewActionremovals.Add(crewAction);
               Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "MenuItem_CrewActionClick(): -----------------removing ca=" + crewAction.Name);
               foreach (Button oldButton in myTankButtons)     // Remove existing Button
               {
                  if( oldButton.Name == crewAction.Name )
                  {
                     myTankButtons.Remove(oldButton);
                     myCanvasTank.Children.Remove(oldButton);
                     break;
                  }
               }
            }
         }
         foreach (IMapItem ca in crewActionremovals) 
            myGameInstance.CrewActions.Remove(ca); // Remove existing Crew Action
         //--------------------------------------
         string action = aStringArray1[1];
         if ( true == action.Contains("Switch")) // need to find where the Assistant is located
         {
            if (false == string.IsNullOrEmpty(myGameInstance.SwitchedCrewMemberRole))
               sCrewMemberRole = myGameInstance.SwitchedCrewMemberRole;
         }
         //--------------------------------------
         string tName = sCrewMemberRole + "Action";
         ITerritory? t = Territories.theTerritories.Find(tName, tType);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItem_CrewActionClick(): t=null for " + tName + " tType=" + tType);
            return;
         }
         //--------------------------------------
         switch (menuitem.Name)
         {
            case "Loader_Load":
               mi = new MapItem("Loader_Load", 1.0, "c54LLoad", t);
               MenuItemCrewActionClickRemoveGunnerRepair();
               break;
            case "Loader_RepairMainGun":
               mi = new MapItem(menuitem.Name, 1.0, "c55LRepairMainGun", t);
               break;
            case "Loader_RepairCoaxialMg":
               mi = new MapItem(menuitem.Name, 1.0, "c56LRepairCoaxialMg", t);
               MenuItemCrewActionClickRemoveGunnerRepair();
               break;
            case "Loader_FireMortar":
               mi = new MapItem(menuitem.Name, 1.0, "c58LFireMortar", t);
               MenuItemCrewActionClickRemoveGunnerRepair();
               break;
            case "Loader_ChangeGunLoad":
               mi = new MapItem(menuitem.Name, 1.0, "c59LChangeGunLoad", t);
               MenuItemCrewActionClickRemoveGunnerRepair();
               MenuItemCrewActionClickRemoveGunnerFire();
               MenuItemCrewActionClickRemoveCommanderDirectFire();
               break;
            case "Loader_RestockReadyRack":
               mi = new MapItem(menuitem.Name, 1.0, "c60LRestockReadyRack", t);
               MenuItemCrewActionClickRemoveGunnerRepair();
               break;
            case "Loader_RepairScope":
               mi = new MapItem(menuitem.Name, 1.0, "c73ReplacePeriscope", t);
               MenuItemCrewActionClickRemoveGunnerRepair();
               break;
            case "Loader_FireAaMg":
               mi = new MapItem(menuitem.Name, 1.0, "c71FireAaMg", t);
               MenuItemCrewActionClickRemoveGunnerRepair();
               break;
            case "Loader_RepairAaMg":
               mi = new MapItem(menuitem.Name, 1.0, "c72RepairAaMg", t);
               MenuItemCrewActionClickRemoveGunnerRepair();
               break;
            case "Loader_FireSubMg":
               mi = new MapItem(menuitem.Name, 1.0, "c74FireSubMg", t);
               MenuItemCrewActionClickRemoveGunnerRepair();
               break;
            case "Driver_Stop":
               mi = new MapItem(menuitem.Name, 1.0, "c61DStop", t);
               break;
            case "Driver_Forward":
               mi = new MapItem(menuitem.Name, 1.0, "c62DForward", t);
               if ((false == isGunnerTrainedInHvss) || (null == myGameInstance.ShermanHvss))
               {
                  MenuItemCrewActionClickRemoveGunnerFire();  // Cannot fire if moving and do not have HVSS
                  MenuItemCrewActionClickRemoveCommanderDirectFire();
               }
               break;
            case "Driver_ForwardToHullDown":
               mi = new MapItem(menuitem.Name, 1.0, "c63DForwardToHullDown", t);
               if ((false == isGunnerTrainedInHvss) || (null == myGameInstance.ShermanHvss))
               {
                  MenuItemCrewActionClickRemoveGunnerFire();  // Cannot fire if moving and do not have HVSS
                  MenuItemCrewActionClickRemoveCommanderDirectFire();
               }
               break;
            case "Driver_Reverse":
               mi = new MapItem(menuitem.Name, 1.0, "c64DReverse", t);
               if ((false == isGunnerTrainedInHvss) || (null == myGameInstance.ShermanHvss))
               {
                  MenuItemCrewActionClickRemoveGunnerFire();  // Cannot fire if moving and do not have HVSS
                  MenuItemCrewActionClickRemoveCommanderDirectFire();
               }
               break;
            case "Driver_ReverseToHullDown":
               mi = new MapItem(menuitem.Name, 1.0, "c65DReverseToHullDown", t);
               if ((false == isGunnerTrainedInHvss) || (null == myGameInstance.ShermanHvss) )
               {
                  MenuItemCrewActionClickRemoveGunnerFire();  // Cannot fire if moving and do not have HVSS
                  MenuItemCrewActionClickRemoveCommanderDirectFire();
               }
               break;
            case "Driver_PivotTank":
               mi = new MapItem(menuitem.Name, 1.0, "c66DPivotTank", t);
               if ((false == isGunnerTrainedInHvss) || (null == myGameInstance.ShermanHvss))
               {
                  MenuItemCrewActionClickRemoveGunnerFire();  // Cannot fire if moving and do not have HVSS
                  MenuItemCrewActionClickRemoveCommanderDirectFire();
               }
               break;
            case "Driver_RepairScope":
               mi = new MapItem(menuitem.Name, 1.0, "c73ReplacePeriscope", t);
               break;
            case "Gunner_FireMainGun":
               mi = new MapItem(menuitem.Name, 1.0, "c50GFireMainGun", t);
               break;
            case "Gunner_FireCoaxialMg":
               mi = new MapItem(menuitem.Name, 1.0, "c51GFireCoaxialMg", t);
               break;
            case "Gunner_RotateTurret":
               mi = new MapItem(menuitem.Name, 1.0, "c52GRotateTurret", t);
               break;
            case "Gunner_RotateFireMainGun":
               mi = new MapItem(menuitem.Name, 1.0, "c53GRotateTurretFireMainGun", t);
               break;
            case "Gunner_RepairMainGun":
               mi = new MapItem(menuitem.Name, 1.0, "c57GRepairMainGun", t);
               break;
            case "Gunner_RepairScope":
               mi = new MapItem(menuitem.Name, 1.0, "c73ReplacePeriscope", t);
               break;
            case "Gunner_ThrowGrenade":
               mi = new MapItem(menuitem.Name, 1.0, "c70ThrowSmokeGrenade", t);
               break;
            case "Assistant_FireBowMg":
               mi = new MapItem(menuitem.Name, 1.0, "c67AFireBowMg", t);
               break;
            case "Assistant_RepairBowMg":
               mi = new MapItem(menuitem.Name, 1.0, "c68ARepairBowMg", t);
               break;
            case "Assistant_PassAmmo":
               mi = new MapItem(menuitem.Name, 1.0, "c69APassAmmo", t);
               break;
            case "Assistant_RepairScope":
               mi = new MapItem(menuitem.Name, 1.0, "c73ReplacePeriscope", t);
               break;
            case "Assistant_SwitchLdr":
            case "Gunner_SwitchLdr":
            case "Commander_SwitchLdr":
            case "Driver_SwitchLdr":
            case "Loader_SwitchLdr":
               mi = new MapItem(menuitem.Name, 1.0, "c200LoaderSwitch", t);
               break;
            case "Assistant_SwitchDvr":
            case "Gunner_SwitchDvr":
            case "Commander_SwitchDvr":
            case "Driver_SwitchDvr":
            case "Loader_SwitchDvr":
               mi = new MapItem(menuitem.Name, 1.0, "c199DriverSwitch", t);
               break;
            case "Assistant_SwitchGunr":
            case "Gunner_SwitchGunr":
            case "Commander_SwitchGunr":
            case "Driver_SwitchGunr":
            case "Loader_SwitchGunr":
               mi = new MapItem(menuitem.Name, 1.0, "c201GunnerSwitch", t);
               break;
            case "Assistant_SwitchCmdr":
            case "Gunner_SwitchCmdr":
            case "Commander_SwitchCmdr":
            case "Driver_SwitchCmdr":
            case "Loader_SwitchCmdr":
               mi = new MapItem(menuitem.Name, 1.0, "c202CommanderSwitch", t);
               break;
            case "Assistant_SwitchAsst":
            case "Gunner_SwitchAsst":
            case "Commander_SwitchAsst":
            case "Driver_SwitchAsst":
            case "Loader_SwitchAsst":
               mi = new MapItem(menuitem.Name, 1.0, "c203AssistantReturn", t);
               break;
            case "Commander_Move":
               mi = new MapItem(menuitem.Name, 1.0, "c48CDirectMove", t);
               break;
            case "Commander_MainGunFire":
               mi = new MapItem(menuitem.Name, 1.0, "c49CDirectMainGunFire", t);
               break;
            case "Commander_MGFire":
               mi = new MapItem(menuitem.Name, 1.0, "c49CDirectMGFire", t);
               break;
            case "Commander_RepairScope":
               mi = new MapItem(menuitem.Name, 1.0, "c73ReplacePeriscope", t);
               break;
            case "Commander_FireAaMg":
               mi = new MapItem(menuitem.Name, 1.0, "c71FireAaMg", t);
               break;
            case "Commander_RepairAaMg":
               mi = new MapItem(menuitem.Name, 1.0, "c72RepairAaMg", t);
               break;
            case "Commander_FireSubMg":
               mi = new MapItem(menuitem.Name, 1.0, "c74FireSubMg", t);
               break;
            case "Commander_ThrowGrenade":
               mi = new MapItem(menuitem.Name, 1.0, "c70ThrowSmokeGrenade", t);
               break;
            case "Commander_Bail":
               mi = new MapItem(menuitem.Name, 1.0, "c204Bail", t);
               MenuItemCrewActionClickBail();  
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "MenuItem_CrewActionClick(): reached default name=" + menuitem.Name);
               return;
         }
         if( null == mi )
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItem_CrewActionClick(): mi=null");
            return;
         }
         myGameInstance.CrewActions.Add(mi);
         Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "MenuItem_CrewActionClick(): adding ca=" + mi.Name);
         //--------------------------------------
         Logger.Log(LogEnum.LE_SHOW_ORDERS_MENU, "MenuItem_CrewActionClick(): adding new button=" + menuitem.Name + " for sCrewMemberRole=" + sCrewMemberRole);
         ContextMenu menu = myContextMenuCrewActions[sCrewMemberRole];
         System.Windows.Controls.Button newButton = new Button { ContextMenu = menu, Name = menuitem.Name, Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
         MapItem.SetButtonContent(newButton, mi, true, false); // This sets the image as the button's content
         myTankButtons.Add(newButton);
         myCanvasTank.Children.Add(newButton);
         Canvas.SetLeft(newButton, mi.Location.X);
         Canvas.SetTop(newButton, mi.Location.Y);
         Canvas.SetZIndex(newButton, 900);
         //--------------------------------------
         MenuItemCrewActionSetLoad();
         if( false ==  UpdateViewCrewOrderButtons(myGameInstance))
            Logger.Log(LogEnum.LE_ERROR, "MenuItem_CrewActionClick(): UpdateView_CrewOrderButtons() returned false");
         GameAction outaction = GameAction.BattleRoundSequenceCrewOrders;
         myGameEngine.PerformAction(ref myGameInstance, ref outaction, 0);
      }
      private void MenuItemCrewActionClickRemoveGunnerRepair()
      {
         foreach (IMapItem crewaction in myGameInstance.CrewActions) // This menu is created on each crew action - Remove Gunner_RepairGun if load does anything other than Repair
         {
            if ("Gunner_RepairMainGun" == crewaction.Name)
            {
               IMapItem? mi = myGameInstance.CrewActions.Find("Gunner_RepairMainGun");
               if (null != mi)
               {
                  myGameInstance.CrewActions.Remove(mi);
                  Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "MenuItemCrewActionClickRemoveGunnerRepair(): -----------------removing ca=" + crewaction.Name);
                  foreach (Button oldButton in myTankButtons)     // Remove existing Button
                  {
                     if (oldButton.Name == mi.Name)
                     {
                        myTankButtons.Remove(oldButton);
                        myCanvasTank.Children.Remove(oldButton);
                        return;
                     }
                  }
                  Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClickRemoveGunnerRepair(): unable to find button with name=" + mi.Name);
                  return;
               }
            }
         }
      }
      private void MenuItemCrewActionClickRemoveGunnerFire()
      {
         foreach (IMapItem crewaction in myGameInstance.CrewActions) // This menu is created on each crew action - Remove Gunner_RepairGun if load does anything other than Repair
         {
            if ("Gunner_FireMainGun" == crewaction.Name )
            {
               IMapItem? mi = myGameInstance.CrewActions.Find(crewaction.Name);
               if (null != mi)
               {
                  myGameInstance.CrewActions.Remove(mi);
                  Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "MenuItemCrewActionClickRemoveGunnerFire(): -----------------removing ca=" + crewaction.Name);
                  foreach (Button oldButton in myTankButtons)     // Remove existing Button
                  {
                     if (oldButton.Name == mi.Name)
                     {
                        myTankButtons.Remove(oldButton);
                        myCanvasTank.Children.Remove(oldButton);
                        break;
                     }
                  }
                  break;
               }
            }
         }
         foreach (IMapItem crewaction in myGameInstance.CrewActions) // This menu is created on each crew action - Remove Gunner_RepairGun if load does anything other than Repair
         {
            if ("Gunner_RotateFireMainGun" == crewaction.Name)
            {
               IMapItem? mi = myGameInstance.CrewActions.Find(crewaction.Name);
               if (null != mi)
               {
                  myGameInstance.CrewActions.Remove(mi);
                  Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "MenuItemCrewActionClickRemoveGunnerFire(): -----------------removing ca=" + crewaction.Name);
                  foreach (Button oldButton in myTankButtons)     // Remove existing Button
                  {
                     if (oldButton.Name == mi.Name)
                     {
                        myTankButtons.Remove(oldButton);
                        myCanvasTank.Children.Remove(oldButton);
                        break;
                     }
                  }
                  break;
               }
            }
         }
      }
      private void MenuItemCrewActionClickRemoveCommanderDirectFire()
      {
         foreach (IMapItem crewaction in myGameInstance.CrewActions) // This menu is created on each crew action - Remove Gunner_RepairGun if load does anything other than Repair
         {
            if ("Commander_MainGunFire" == crewaction.Name)
            {
               IMapItem? mi = myGameInstance.CrewActions.Find(crewaction.Name);
               if (null != mi)
               {
                  myGameInstance.CrewActions.Remove(mi);
                  Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "MenuItemCrewActionClickRemoveCommanderDirectFire(): -----------------removing ca=" + crewaction.Name);
                  foreach (Button oldButton in myTankButtons)     // Remove existing Button
                  {
                     if (oldButton.Name == mi.Name)
                     {
                        myTankButtons.Remove(oldButton);
                        myCanvasTank.Children.Remove(oldButton);
                        return;
                     }
                  }
                  Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClickRemoveCommanderDirectFire(): unable to find button with name=" + mi.Name);
                  return;
               }
            }
         }
      }
      private void MenuItemCrewActionSetLoad()
      {
         //----------------------------------------------
         bool isGunnerFiring = false;
         bool isLoaderLoading = false;
         foreach (IMapItem crewaction in myGameInstance.CrewActions) // This menu is created on each crew action - Remove Gunner_RepairGun if load does anything other than Repair
         {
            if (("Gunner_FireMainGun" == crewaction.Name) || ("Gunner_RotateFireMainGun" == crewaction.Name))
               isGunnerFiring = true;
            if ("Loader_Load" == crewaction.Name)
               isLoaderLoading = true;
         }
         //----------------------------------------------
         if (true == isLoaderLoading)
         {
            foreach (IMapItem crewaction in myGameInstance.CrewActions) // This menu is created on each crew action - Remove Gunner_RepairGun if load does anything other than Repair
            {
               if ("Loader_Load" == crewaction.Name)
               {
                  IMapItem? ca = myGameInstance.CrewActions.Find(crewaction.Name);
                  if (null != ca)
                  {
                     myGameInstance.CrewActions.Remove(ca);
                     Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "MenuItemCrewActionSetLoad(): -----------------removing ca=" + crewaction.Name);
                     foreach (Button oldButton in myTankButtons)     // Remove existing Button
                     {
                        if (oldButton.Name == ca.Name)
                        {
                           myTankButtons.Remove(oldButton);
                           myCanvasTank.Children.Remove(oldButton);
                           break;
                        }
                     }
                     break;
                  }
               }
            }
            //----------------------------------------------
            IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
            if (null == lastReport)
            {
               Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionSetLoad(): lastReport=null");
               return;
            }
            string tType = lastReport.TankCardNum.ToString();
            //----------------------------------------------
            string tName = "LoaderAction";
            ITerritory? t = Territories.theTerritories.Find(tName, tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClickLoad(): t=null for " + tName + " tType=" + tType);
               return;
            }
            IMapItem mi;
            if (true == isGunnerFiring)
               mi = new MapItem("Loader_Load", 1.0, "c54LLoadNoSpot", t);
            else
               mi = new MapItem("Loader_Load", 1.0, "c54LLoad", t);
            myGameInstance.CrewActions.Add(mi);
            ContextMenu menu = myContextMenuCrewActions["Loader"];
            System.Windows.Controls.Button newButton = new Button { ContextMenu = menu, Name = "Loader_Load", Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
            MapItem.SetButtonContent(newButton, mi, true, false); // This sets the image as the button's content
            myTankButtons.Add(newButton);
            myCanvasTank.Children.Add(newButton);
            Canvas.SetLeft(newButton, mi.Location.X);
            Canvas.SetTop(newButton, mi.Location.Y);
            Canvas.SetZIndex(newButton, 900);
            Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "MenuItemCrewActionClickLoad(): adding new button=" + mi.Name); // LE_SHOW_ORDERS_MENU
         }
      }
      private void MenuItemCrewActionClickBail()
      {
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClickBail(): lastReport=null");
            return;
         }
         string tType = lastReport.TankCardNum.ToString();
         //-----------------------------------------
         string[] crewmembers = new string[4] { "Gunner", "Loader", "Driver", "Assistant" }; // switch incapacitated members with new crew members
         foreach (string crewmember in crewmembers)
         {
            string tName = crewmember + "Action";
            ITerritory? t = Territories.theTerritories.Find(tName, tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClickBail(): t=null for " + tName + " tType=" + tType);
               return;
            }
            IMapItem mi = new MapItem(crewmember + "_Bail", 1.0, "c204Bail", t);
            myGameInstance.CrewActions.Add(mi);
            System.Windows.Controls.Button newButton = new Button { Name = mi.Name, Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
            MapItem.SetButtonContent(newButton, mi, true, false); // This sets the image as the button's content
            myTankButtons.Add(newButton);
            myCanvasTank.Children.Add(newButton);
            Canvas.SetLeft(newButton, mi.Location.X);
            Canvas.SetTop(newButton, mi.Location.Y);
            Canvas.SetZIndex(newButton, 900);
            Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "MenuItemCrewActionClickBail(): adding new button=" + mi.Name + " for crewmember=" + crewmember); // LE_SHOW_ORDERS_MENU
         }
      }
      private void MenuItemCrewActionClickLoad()
      {
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClickLoad(): lastReport=null");
            return;
         }
         string tType = lastReport.TankCardNum.ToString();
         //----------------------------------------------
         bool isGunnerFiring = false;
         bool isLoaderLoading = false;
         foreach (IMapItem crewaction in myGameInstance.CrewActions) // This menu is created on each crew action - Remove Gunner_RepairGun if load does anything other than Repair
         {
            if (("Gunner_FireMainGun" == crewaction.Name) || ("Gunner_RotateFireMainGun" == crewaction.Name))
               isGunnerFiring = true;
            if ("Loader_Load" == crewaction.Name)
               isLoaderLoading = true;
         }
         //----------------------------------------------
         if (true == isLoaderLoading)
         {
            string tName = "LoaderAction";
            ITerritory? t = Territories.theTerritories.Find(tName, tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "MenuItemCrewActionClickLoad(): t=null for " + tName + " tType=" + tType);
               return;
            }
            IMapItem mi;
            if (true == isGunnerFiring)
               mi = new MapItem("Loader_Load", 1.0, "c54LLoadNoSpot", t);
            else
               mi = new MapItem("Loader_Load", 1.0, "c54LLoad", t);
            myGameInstance.CrewActions.Add(mi);
            System.Windows.Controls.Button newButton = new Button { Name = mi.Name, Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
            MapItem.SetButtonContent(newButton, mi, true, false); // This sets the image as the button's content
            myTankButtons.Add(newButton);
            myCanvasTank.Children.Add(newButton);
            Canvas.SetLeft(newButton, mi.Location.X);
            Canvas.SetTop(newButton, mi.Location.Y);
            Canvas.SetZIndex(newButton, 900);
            Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "MenuItemCrewActionClickLoad(): adding new button=" + mi.Name ); // LE_SHOW_ORDERS_MENU
         }
      }
      private void MenuItemAmmoReloadClick(object sender, RoutedEventArgs e)
      {
         MenuItem? menuitem = sender as MenuItem;
         if (null == menuitem)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItem_AmmoReloadClick(): menuitem=null");
            return;
         }
         //--------------------------------------
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItem_AmmoReloadClick(): lastReport=null");
            return;
         }
         string tType = lastReport.TankCardNum.ToString();
         //--------------------------------------
         string[] aStringArray = menuitem.Name.Split(new char[] { '_' });
         if (aStringArray.Length < 2)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItem_AmmoReloadClick(): underscore not found in " + menuitem.Name + " len=" + aStringArray.Length);
            return;
         }
         string tName = aStringArray[0];
         //--------------------------------------
         ITerritory? t = Territories.theTerritories.Find(tName, tType);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItem_AmmoReloadClick(): t=null for " + tName + " tType=" + tType);
            return;
         }
         //--------------------------------------
         IMapItem? oldGunLoadMapItem = null;
         foreach (IMapItem mi in myGameInstance.GunLoads)
         {
            if (true == mi.Name.Contains("GunLoadInGun"))
               oldGunLoadMapItem = mi;
         }
         //--------------------------------------
         List<Button> removalButtons = new List<Button>();
         foreach (IMapItem gunload in myGameInstance.GunLoads) // get rid of existing gunloads for this crew member
         {
            foreach (Button oldButton in myTankButtons)    // Remove existing Button
            {
               if (oldButton.Name == gunload.Name)
               {
                  removalButtons.Add(oldButton);  // adding button to remove
                  break;
               }
            }
         }
         //--------------------------------------
         StringBuilder sb0 = new StringBuilder("MenuItem_AmmoReloadClick(): myTankButtonsRemoved=[");
         int count = myTankButtons.Count;
         foreach (Button b in removalButtons)
         {
            int i = 1;
            sb0.Append(b.Name);
            if (myGameInstance.GunLoads.Count != i++)
               sb0.Append(",");
            myTankButtons.Remove(b);
            myCanvasTank.Children.Remove(b);
         }
         sb0.Append("] myTankButtonsRemaining=[");
         foreach (Button b in myTankButtons)
         {
            int i = 1;
            sb0.Append(b.Name);
            if (myTankButtons.Count != i++)
               sb0.Append(",");
         }
         sb0.Append("]");
         Logger.Log(LogEnum.LE_SHOW_GUN_LOAD, sb0.ToString());
         //--------------------------------------
         myGameInstance.GunLoads.Clear();
         //--------------------------------------
         ContextMenu menu = myContextMenuGunLoadActions[tName];
         SolidColorBrush brush = new SolidColorBrush(Colors.Transparent);
         Thickness thickness = new Thickness(0);
         double size = Utilities.theMapItemSize;
         System.Windows.Controls.Button? newButton = null;
         string miName = menuitem.Name + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem ammoLoad;
         //--------------------------------------
         if (true == menuitem.Name.Contains("ReadyRackAmmoReload")) // either this button or AmmoReload button
            ammoLoad = new MapItem(miName, 1.0, "c30ReadyRackAmmoReload", t);
         else
            ammoLoad = new MapItem(miName, 1.0, "c29AmmoReload", t);
         Logger.Log(LogEnum.LE_SHOW_GUN_LOAD, "MenuItem_AmmoReloadClick(): adding new button=" + miName);
         newButton = new Button { ContextMenu = menu, Name = menuitem.Name, Width = size, Height = size, BorderThickness = thickness, Background = brush, Foreground = brush };
         newButton.Click += ClickButtonMapItem;
         MapItem.SetButtonContent(newButton, ammoLoad, true, false); // This sets the image as the button's content
         myGameInstance.GunLoads.Insert(0, ammoLoad);
         myTankButtons.Insert(0, newButton);
         myCanvasTank.Children.Insert(0, newButton);
         Canvas.SetLeft(newButton, ammoLoad.Location.X);
         Canvas.SetTop(newButton, ammoLoad.Location.Y);
         Canvas.SetZIndex(newButton, 899);
         //--------------------------------------
         if( null != oldGunLoadMapItem) 
         {
            string newGunLoadName = "GunLoadInGun" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            IMapItem newGunLoadMapItem = new MapItem(newGunLoadName, 1.0, "c17GunLoad", oldGunLoadMapItem.TerritoryCurrent); // replace with new GunLoad mapitem
            myGameInstance.GunLoads.Add(newGunLoadMapItem);
            //--------------------------------------
            int countCounter = 0;
            if (tName == oldGunLoadMapItem.TerritoryCurrent.Name)
               countCounter++;
            double gunLoadLocationOffset = (countCounter * 3) - (newGunLoadMapItem.Zoom * Utilities.theMapItemOffset);
            newGunLoadMapItem.Location.X = oldGunLoadMapItem.TerritoryCurrent.CenterPoint.X + gunLoadLocationOffset;
            newGunLoadMapItem.Location.Y = oldGunLoadMapItem.TerritoryCurrent.CenterPoint.Y + gunLoadLocationOffset;
            Logger.Log(LogEnum.LE_SHOW_GUN_LOAD, "MenuItem_AmmoReloadClick(): adding new button=" + newGunLoadMapItem.Name);
            ContextMenu menuGunLoad = myContextMenuGunLoadActions[newGunLoadMapItem.TerritoryCurrent.Name];
            newButton = new Button { ContextMenu = menuGunLoad, Name = newGunLoadMapItem.Name, Width = size, Height = size, BorderThickness = thickness, Background = brush, Foreground = brush };
            newButton.Click += ClickButtonMapItem;
            MapItem.SetButtonContent(newButton, newGunLoadMapItem, true, false); // This sets the image as the button's content
            myTankButtons.Add(newButton);
            myCanvasTank.Children.Add(newButton);
            Canvas.SetLeft(newButton, newGunLoadMapItem.Location.X);
            Canvas.SetTop(newButton, newGunLoadMapItem.Location.Y);
            Canvas.SetZIndex(newButton, 900);
         }
         //--------------------------------------
         GameAction outaction = GameAction.BattleRoundSequenceAmmoOrders;  // MenuItem_AmmoReloadClick() - when clicking menu item
         myGameEngine.PerformAction(ref myGameInstance, ref outaction, 0);
      }
      private void PreviewMouseLeftButtonDownMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (e.LeftButton == MouseButtonState.Pressed)
         {
            Button? button = sender as Button;
            if (null == button)
            {
               Logger.Log(LogEnum.LE_ERROR, "MouseDownMapItem(): button = null");
               return;
            }
            if ((BattlePhase.ConductCrewAction != myGameInstance.BattlePhase) && (false == button.Name.Contains("Sherman")) && (false == button.Name.Contains("TaskForce")) && (false == button.Name.Contains("Weather")))
            {
               Logger.Log(LogEnum.LE_SHOW_BUTTON_MOVE, "MouseDownMapItem(): selected button.Name=" + button.Name);
               myDraggedButton = button;
            }
         }
      }
      private void PreviewMouseLeftButtonUpMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (null == myDraggedButton)
            Logger.Log(LogEnum.LE_SHOW_BUTTON_MOVE, "MouseUpMapItem(): myDraggedButton=null");
         else
            Logger.Log(LogEnum.LE_SHOW_BUTTON_MOVE, "MouseUpMapItem(): unselecting button.Name=" + myDraggedButton.Name);
         myDraggedButton = null;
      }
      //-------------GameViewerWindow---------------------------------
      private void MouseMoveGameViewerWindow(object sender, MouseEventArgs e)
      {
         if (null == myDraggedButton)
         {
            base.OnMouseMove(e);
            return;
         }
         if( true == myTankButtons.Contains(myDraggedButton))
         {
            base.OnMouseMove(e);
            return;
         }
         //-----------------------------------
         IStacks? stacks = null;
         if (EnumMainImage.MI_Move == CanvasImageViewer.theMainImage)
            stacks = myGameInstance.MoveStacks;
         else if (EnumMainImage.MI_Battle == CanvasImageViewer.theMainImage)
            stacks = myGameInstance.BattleStacks;
         else
            return;
         IMapItem? selectedMapItem = stacks.FindMapItem(myDraggedButton.Name); // selectedMapItem is the new target
         if (null == selectedMapItem)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseMoveGameViewerWindow(): selectedMapItem=null for button.Name=" + myDraggedButton.Name);
            return;
         }
         //-----------------------------------
         System.Windows.Point newPoint = e.GetPosition(myCanvasMain);
         if (true == Territory.IsPointInPolygon(selectedMapItem.TerritoryCurrent, newPoint))
         {
            Logger.Log(LogEnum.LE_SHOW_BUTTON_MOVE, "MouseMoveGameViewerWindow(): button.Name=" + myDraggedButton.Name + " moving to p=(" + newPoint.X.ToString("###") + "," + newPoint.Y.ToString("###") + ")");
            double offset = selectedMapItem.Zoom * Utilities.theMapItemOffset;
            selectedMapItem.Location.X = newPoint.X - offset;
            selectedMapItem.Location.Y = newPoint.Y - offset;
            Canvas.SetLeft(myDraggedButton, newPoint.X - offset);
            Canvas.SetTop(myDraggedButton, newPoint.Y - offset);
         }

         e.Handled = true;
      }
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
         System.Windows.Application app = System.Windows.Application.Current;
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
            if (false == String.IsNullOrEmpty(Properties.Settings.Default.WindowPlacement))
            {
               WindowPlacement wp = Utilities.Deserialize<WindowPlacement>(Properties.Settings.Default.WindowPlacement);
               wp.length = Marshal.SizeOf(typeof(WindowPlacement));
               wp.flags = 0;
               wp.showCmd = (wp.showCmd == SwShowminimized ? SwShownormal : wp.showCmd);
               var hwnd = new WindowInteropHelper(this).Handle;
               if (false == SetWindowPlacement(hwnd, ref wp))
                  Logger.Log(LogEnum.LE_ERROR, "SetWindowPlacement() returned false");
            }
            if (0.0 != Properties.Settings.Default.ScrollViewerHeight)
               myScrollViewerMap.Height = Properties.Settings.Default.ScrollViewerHeight;
            if (0.0 != Properties.Settings.Default.ScrollViewerWidth)
               myScrollViewerMap.Width = Properties.Settings.Default.ScrollViewerWidth;
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
         System.Diagnostics.Debug.WriteLine("GameViewerWindow.ClosedGameViewerWindow(): Called Save_DefaultsToSettings()");
         SaveDefaultsToSettings();
      }
      //-------------CONTROLLER HELPER FUNCTIONS---------------------------------
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
            CommandBinding undoCmdBinding = new CommandBinding(command, mmv.MenuItemEditUndo_Click, mmv.MenuItemEditUndo_ClickCanExecute);
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemEditUndo_Click));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.D, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBinding recoverCmdBinding = new CommandBinding(command, mmv.MenuItemEditRecoverCheckpoint_Click, mmv.MenuItemEditRecoverCheckpoint_ClickCanExecute);
            CommandBindings.Add(recoverCmdBinding);
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.R, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBinding recoverRoundCmdBinding = new CommandBinding(command, mmv.MenuItemEditRecoverRound_Click, mmv.MenuItemEditRecoverRound_ClickCanExecute);
            CommandBindings.Add(recoverRoundCmdBinding);
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
            keyGesture = new KeyGesture(Key.R, ModifierKeys.Control | ModifierKeys.Shift);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemViewRoads_Click));
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
