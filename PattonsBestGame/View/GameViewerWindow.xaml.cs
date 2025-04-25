using Pattons_Best.Properties;
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
using Pattons_Best.Model;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections;

namespace Pattons_Best
{
   public partial class GameViewerWindow : Window, IView
   {
      private const int MAX_DAILY_ACTIONS = 16;
      private const Double MARQUEE_SCROLL_ANMINATION_TIME = 30.0;
      private const Double ELLIPSE_DIAMETER = 40.0;
      private const Double ELLIPSE_RADIUS = ELLIPSE_DIAMETER / 2.0;
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
      private ContextMenu myContextMenuButton = new ContextMenu();
      private readonly ContextMenu myContextMenuCanvas = new ContextMenu();
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private int myBrushIndex = 0;
      private readonly List<Brush> myBrushes = new List<Brush>();
      private readonly List<Rectangle> myRectangles = new List<Rectangle>();
      private readonly List<Polygon> myPolygons = new List<Polygon>();
      private readonly List<Ellipse> myEllipses = new List<Ellipse>();
      private Rectangle? myRectangleMoving = null;               // Not used - Rectangle that is moving with button
      private Rectangle myRectangleSelected = new Rectangle(); // Player has manually selected this button
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
            case GameAction.TestingStartMovement:
            case GameAction.MorningBriefingAmmoReadyRackLoad:
            case GameAction.PreparationsHatches:
            case GameAction.PreparationsGunLoad:
            case GameAction.PreparationsGunLoadSelect:
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               break;
            case GameAction.PreparationsTurret:
               if (false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.PreparationsTurretRotateLeft:
            case GameAction.PreparationsTurretRotateRight:
               Button? b100 = this.myBattleButtons.Find(gi.Sherman.Name);
               if (null == b100)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): b100=null");
               else
                  MapItem.SetButtonContent(b100, gi.Sherman);
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
         System.Windows.Controls.Button b = new Button { ContextMenu = myContextMenuButton, Name = mi.Name, Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
         MapItem.SetButtonContent(b, mi); // This sets the image as the button's content
         RotateTransform rotateTransform = new RotateTransform();
         b.RenderTransformOrigin = new Point(0.5, 0.5);
         rotateTransform.Angle = mi.RotationBase;
         b.RenderTransform = rotateTransform;
         buttons.Add(b);
         Canvas.SetLeft(b, mi.Location.X);
         Canvas.SetTop(b, mi.Location.Y);
         b.Click += ClickButtonMapItem;
         b.MouseEnter += MouseEnterMapItem;
         b.MouseLeave += MouseLeaveMapItem;

         return b;
      }
      private bool SetTerritory(IMapItem mi, ITerritory newT)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTerritory(): myGameInstance=null");
            return false;
         }
         myGameInstance.MoveStacks.Remove(mi);
         //-------------------------------------
         IStacks? stacks = null;
         if( "Tank" == newT.CanvasName )
         {
            stacks = myGameInstance.TankStacks;
         }
         else if ("Main" == newT.CanvasName)
         {
            if( "Battle" == newT.Type )
               stacks = myGameInstance.BattleStacks;
            else if (("A" == newT.Type) || ("B" == newT.Type) || ("D" == newT.Type) || ("D" == newT.Type) || ("E" == newT.Type))
               stacks = myGameInstance.MoveStacks;
         }
         if( null == stacks )
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTerritory(): unknown Parent=" + newT.CanvasName + " or unknown Type=" + newT.Type);
            return false;
         }
         IStack? stack = stacks.Find(mi.TerritoryCurrent);
         if (null == stack)
         {
            stack = new Stack(newT, mi);
            stacks.Add(stack);
         }
         else // add to top of stack
         {
            mi.TerritoryCurrent = newT;
            mi.SetLocation(newT.CenterPoint);
            stack.MapItems.Add(mi);
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
         }
         foreach (UIElement ui1 in elements)
            myCanvasTank.Children.Remove(ui1);
         //-------------------------------------------------------
         if (GamePhase.UnitTest == gi.GamePhase)
            return true;
         //-------------------------------------------------------
         if (false == UpdateCanvasTankMapItems(gi.ReadyRacks))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasTankMapItems(ReadyRacks) returned false");
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
                  if (false == UpdateCanvasTankGunLoad(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): UpdateCanvasTankGunLoad() returned false");
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): EXCEPTION THROWN a=" + action.ToString() + "\ne=" + e.ToString());
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
               b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
               b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
               Canvas.SetLeft(b, mi.Location.X);
               Canvas.SetTop(b, mi.Location.Y);
            }
            else
            {
               Button newButton = CreateButtonMapItem(myTankButtons, mi);
               myCanvasTank.Children.Add(newButton);
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankHatches(): report=null");
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
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankHatches(): EXCEPTION THROWN a=" + action.ToString() + "\n" + e.ToString());
            return false;
         }
         Button? b = myTankButtons.Find("GunLoad");
         if (null != b)
            Canvas.SetZIndex(b, 99999);
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
                  IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce"); // center thumbnails around exit area
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
               case GameAction.MovementAdvanceFireCheck:
               case GameAction.MovementEnterAreaUsControl:
                  if (false == UpdateCanvasMovement(gi, action))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): UpdateCanvasMovement() returned false");
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
               case GameAction.UpdateBattleBoard:
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
                  MapItem.SetButtonContent(button, mi);
               }
            }
            if (ui is Label label)  // A Game Feat Label
               elements.Add(ui);
            if (ui is Image img)
            {
               if (true == img.Name.Contains("Canvas"))
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
                  rotateTransform.Angle = mi.RotationBase;
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
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankHatches(): cannot find tName=" + s);
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
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTankHatches(): cannot find tName=" + s);
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
      private bool UpdateCanvasMovement(IGameInstance gi, GameAction action)
      {
         try
         {
            if (0 < gi.MapItemMoves.Count)
            {
               IMapItemMove? mim2 = gi.MapItemMoves[0];
               if (null == mim2)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim2=null");
                  return false;
               }
               if (null == mim2.OldTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim2.OldTerritory=null");
                  return false;
               }
               IMapItem mi = mim2.MapItem;
               if (false == MovePathAnimate(mim2))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): MovePathAnimate() returned false t=" + mim2.OldTerritory.ToString());
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "UpdateCanvasMovement(): MovePathAnimate() returned false gi.MapItemMoves.Clear()");
                  gi.MapItemMoves.Clear();
                  return false;
               }
               if (null == mim2)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim2=null");
                  return false;
               }
               if (null == mim2.NewTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): mim2.NewTerritory=null");
                  return false;
               }
               //------------------------------------------
               gi.MoveStacks.Remove(mi);
               Logger.Log(LogEnum.LE_SHOW_STACK_DEL, "UpdateCanvasMovement(): Removed mi=" + mi.Name + " t=" + mim2.OldTerritory.Name + " to " + gi.MoveStacks.ToString());
               IStack? stack = gi.MoveStacks.Find(mim2.NewTerritory);
               if (null == stack)
               {
                  gi.MoveStacks.Add(new Stack(mim2.NewTerritory, mi));
                  Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "UpdateCanvasMovement(): 1-Adding mi=" + mi.Name + " t=" + mim2.NewTerritory.Name + " to " + gi.MoveStacks.ToString());
               }
               else
               {
                  stack.MapItems.Add(mi);
                  Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "UpdateCanvasMovement(): 2-Adding mi=" + mi.Name + " t=" + mim2.NewTerritory.Name + " to " + gi.MoveStacks.ToString());
               }
               Logger.Log(LogEnum.LE_VIEW_MIM, "UpdateCanvasMovement(): mi=" + mi.Name + " t=" + mi.TerritoryCurrent.Name + " moving to t=" + mim2.NewTerritory.Name + " " + gi.MoveStacks.ToString());
               mi.TerritoryCurrent = mi.TerritoryStarting = mim2.NewTerritory;
            }
         }
         catch (Exception e)
         {
            Console.WriteLine("UpdateCanvasMovement() - EXCEPTION THROWN e={0}", e.ToString());
            return false;
         }
         return true;
      }
      private bool MovePathAnimate(IMapItemMove mim)
      {
         const int ANIMATE_TIME_SEC = 3;
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): b=null for n=" + mim.MapItem.Name);
            return false;
         }
         Button? b = myMoveButtons.Find(Utilities.RemoveSpaces(mim.MapItem.Name));
         if (null == b)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): b=null for n=" + mim.MapItem.Name);
            return false;
         }
         try
         {
            Canvas.SetZIndex(b, 10000); // Move the button to the top of the Canvas
            double xStart = mim.MapItem.Location.X;
            double yStart = mim.MapItem.Location.Y;
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
            int multiplerX = Utilities.RandomGenerator.Next(0, 3) - 1;
            int multiplerY = Utilities.RandomGenerator.Next(0, 3) - 1;
            double xEnd = mim.NewTerritory.CenterPoint.X - Utilities.theMapItemOffset + multiplerX * Utilities.theMapItemOffset;
            double yEnd = mim.NewTerritory.CenterPoint.Y - Utilities.theMapItemOffset + multiplerY * Utilities.theMapItemOffset;
            if ((Math.Abs(xEnd - xStart) < 2) && (Math.Abs(yEnd - yStart) < 2)) // if already at final location, skip animation or get runtime exception
               return true;
            System.Windows.Point newPoint2 = new System.Windows.Point(xEnd, yEnd);
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
            b.BeginAnimation(Canvas.LeftProperty, xAnimiation);
            b.BeginAnimation(Canvas.TopProperty, yAnimiation);
            mim.MapItem.Location.X = xEnd;
            mim.MapItem.Location.Y = yEnd;
            if (null == myRectangleSelected)
            {
               Console.WriteLine("MovePathAnimate() myRectangleSelection=null");
               return false;
            }
            myRectangleSelected.RenderTransform = new TranslateTransform();
            myRectangleSelected.BeginAnimation(Canvas.LeftProperty, xAnimiation);
            myRectangleSelected.BeginAnimation(Canvas.TopProperty, yAnimiation);
            return true;
         }
         catch (Exception e)
         {
            b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
            b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
            Console.WriteLine("MovePathAnimate() - EXCEPTION THROWN e={0}", e.ToString());
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
         GameAction outAction = GameAction.PreparationsHatches;
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
         ITerritory? t = Territories.theTerritories.Find(clickedPolygon.Name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonGunLoad(): t=null for " + clickedPolygon.Name.ToString());
            return;
         }
         IMapItem? gunLoad = myGameInstance.GunLoads[0];
         if (null == gunLoad)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonGunLoad(): t=null for " + clickedPolygon.Name.ToString());
            return;
         }
         if (false == SetTerritory(gunLoad, t))
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonGunLoad(): SetTerritory() returned false");
         GameAction outAction = GameAction.PreparationsGunLoadSelect;
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
         GameAction outAction = GameAction.MovementAdvanceFireCheck;
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
         GameAction outAction = GameAction.BattlePlaceAdvanceFire;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private void ClickButtonMapItem(object sender, RoutedEventArgs e)
      {
         Button? button = sender as Button;
         if (null == button)
         {
            Logger.Log(LogEnum.LE_ERROR, "ClickButtonMapItem(): button = null");
            return;
         }
         if (true == button.Name.Contains("OpenHatch"))
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
               if (true == button.Name.Contains(s))
               {
                  crewMember.IsButtonedUp = true;
                  myGameInstance.Hatches.Remove(button.Name);
                  this.myTankButtons.Remove(button);
                  GameAction outAction = GameAction.PreparationsHatches;
                  myGameEngine.PerformAction(ref myGameInstance, ref outAction);
                  myCanvasTank.Children.Remove(button);
                  return;
               }
            }
         }
         //-----------------------------------------------
         else if( true == button.Name.Contains("Sherman")) // This is for the turret
         {
            myGameInstance.Sherman.Count++;
            if (5 < myGameInstance.Sherman.Count)
               myGameInstance.Sherman.Count = 0;
            MapItem.SetButtonContent(button, myGameInstance.Sherman);
         }
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
