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

namespace Pattons_Best
{
   public partial class GameViewerWindow : Window, IView
   {
      private const int MAX_DAILY_ACTIONS = 16;
      private const Double MARQUEE_SCROLL_ANMINATION_TIME = 30;
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
      private readonly List<Button> myButtonMains = new List<Button>();
      private readonly List<Button> myButtonTanks = new List<Button>();
      private readonly SplashDialog mySplashScreen;
      private ContextMenu myContextMenuButton = new ContextMenu();
      private readonly ContextMenu myContextMenuCanvas = new ContextMenu();
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private int myBrushIndex = 0;
      private readonly List<Brush> myBrushes = new List<Brush>();
      private readonly List<Rectangle> myRectangles = new List<Rectangle>();
      private readonly List<Polygon> myPolygons = new List<Polygon>();
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
         myCanvasMap.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas); // Constructor - revert to save zoom
         StatusBarViewer sbv = new StatusBarViewer(myStatusBar, ge, gi, myCanvasMap);
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
         myDieRoller = new DieRoller(myCanvasMap, CloseSplashScreen); // Close the splash screen when die resources are loaded
         if (true == myDieRoller.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): myDieRoller.CtorError=true");
            CtorError = true;
            return;
         }
         //---------------------------------------------------------------
         myEventViewer = new EventViewer(myGameEngine, myGameInstance, myCanvasMap, myScrollViewerTextBlock, Territories.theTerritories, myDieRoller);
         if( true == myEventViewer.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): myEventViewer.CtorError=true");
            CtorError = true;
            return;
         }
         CanvasImageViewer civ = new CanvasImageViewer(myCanvasMap, myDieRoller);
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
            this.myCanvasMap.Cursor = myTargetCursor;
         }
         //-------------------------------------------------------
         else if ((GameAction.UpdateLoadingGame == action) || (GameAction.UpdateNewGame == action) )
         {
            myGameInstance = gi;
            myButtonMains.Clear();
            foreach (UIElement ui in myCanvasMap.Children) // remove all buttons on map
            {
               if (ui is Button b)
               {
                  if (true == b.Name.Contains("Prince"))
                  {
                     myCanvasMap.Children.Remove(ui);
                     break;
                  }
               }
            }
            myCanvasMap.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas); // UploadNewGame - Return to previous saved zoom level
            this.Title = UpdateTitle(gi.Options);
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
            case GameAction.MorningBriefingAmmoReadyRackLoad:
               if( false == UpdateCanvasTank(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasTank() returned error ");
               break;
            case GameAction.EndGameWin:
            case GameAction.EndGameLost:
               SaveDefaultsToSettings();
               break;
            case GameAction.RemoveSplashScreen:
               this.Title = UpdateTitle(gi.Options);
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               mySplashScreen.Close();
               myScrollViewerMap.UpdateLayout();
               //UpdateScrollbarThumbnails(gi.Prince.Territory);
               break;
            case GameAction.UpdateGameOptions:
               this.Title = UpdateTitle(gi.Options);
               SaveDefaultsToSettings();
               break;
            case GameAction.EndGameFinal:
               myCanvasMap.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);  // EndGameFinal - show map for last time
               if (false == UpdateCanvasMain(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvasMain() returned error ");
               break;
            case GameAction.SetupChooseFunOptions:
            case GameAction.SetupFinalize:
               this.Title = UpdateTitle(gi.Options);
               myCanvasMap.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
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
      //-----------------------SUPPORTING FUNCTIONS--------------------
      private void CloseSplashScreen() // callback function that removes splash screen when dice are loaded
      {
         GameAction outAction = GameAction.RemoveSplashScreen;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
      }
      private bool CreateButtonMapItem(IMapItem mi, int counterCount)
      {
         string? tName = mi.TerritoryCurrent.ToString();
         if (null == tName)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateButtonMapItem(): tName=null");
            return false;
         }
         string territoryName = Utilities.RemoveSpaces(tName);
         ITerritory? territory = Territories.theTerritories.Find(territoryName);
         if (null == territory)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateButtonMapItem(): TerritoryExtensions.Find() returned null");
            return false;
         }
         System.Windows.Controls.Button b = new Button { ContextMenu = myContextMenuButton, Name = Utilities.RemoveSpaces(mi.Name), Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
         Canvas.SetLeft(b, territory.CenterPoint.X - mi.Zoom * Utilities.theMapItemOffset + (counterCount * Utilities.STACK));
         Canvas.SetTop(b, territory.CenterPoint.Y - mi.Zoom * Utilities.theMapItemOffset + (counterCount * Utilities.STACK));
         MapItem.SetButtonContent(b, mi, false, false, false, false); // This sets the image as the button's content
         if( "Tank" == territory.CanvasName)
         {
            myButtonTanks.Add(b);
            myCanvasTank.Children.Add(b);
         }
         else if ("Main" == territory.CanvasName)
         {
            myButtonMains.Add(b);
            myCanvasMap.Children.Add(b);
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateButtonMapItem(): reached default  for territory.CanvasName=" + territory.CanvasName);
            return false;
         }
         Canvas.SetZIndex(b, counterCount);
         b.Click += ClickButtonMapItem;
         b.MouseEnter += MouseEnterMapItem;
         b.MouseLeave += MouseLeaveMapItem;
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
      private string UpdateTitle(Options options)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Barbarian Prince - ");
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
      private bool UpdateCanvasTank(IGameInstance gi, GameAction action)
      {
         //-------------------------------------------------------
         // Clean the Canvas of all marks
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMap.Children)
         {
            if (ui is Image img)
            {
               if ("CanvasTank" == img.Name)
                  continue;
               elements.Add(ui);
            }
         }
         foreach (UIElement ui1 in elements)
            myCanvasMap.Children.Remove(ui1);
         //-------------------------------------------------------
         if (GamePhase.UnitTest == gi.GamePhase)
            return true;
         //-------------------------------------------------------
         foreach(IMapItem mi in gi.ReadyRacks)
         {
            if( null == mi )
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): mi=null");
               return false;
            }
            Button? b = myButtonTanks.Find(mi.Name);
            if (null != b)
            {
               b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
               b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
               ITerritory t = mi.TerritoryCurrent;
               Double x = t.CenterPoint.X - (mi.Zoom * Utilities.theMapItemOffset);
               Double y = t.CenterPoint.Y - (mi.Zoom * Utilities.theMapItemOffset);
               Canvas.SetLeft(b, x);
               Canvas.SetTop(b, y);
               Canvas.SetZIndex(b, 9999);
            }
            else
            {
               if (false == CreateButtonMapItem(mi, 0))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasTank(): CreateButtonMapItem() returned false");
                  return false;
               }
            }
         }
         //-------------------------------------------------------
         try
         {
            switch (action)
            {
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
            Console.WriteLine("UpdateCanvasMain() - EXCEPTION THROWN a=" + action.ToString() + "\ne={0}", e.ToString());
            return false;
         }
         return true;
      }
      private bool UpdateCanvasMain(IGameInstance gi, GameAction action, bool isOnlyLastLegRemoved = false)
      {
         //-------------------------------------------------------
         // Clean the Canvas of all marks
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMap.Children)
         {
            if (ui is Polygon polygon)
               elements.Add(ui);
            if (ui is Polyline polyline)
               elements.Add(ui);
            if (ui is Ellipse ellipse)
            {
               if("CenterPoint" != ellipse.Name) // CenterPoint is a unit test ellipse
                  elements.Add(ui);
            }
            if (ui is Label label)  // A Game Feat Label
               elements.Add(ui);
            if (ui is Image img)
            {
               if ( true == img.Name.Contains("Canvas"))
                  continue;
               elements.Add(ui);
            }
            if (ui is TextBlock tb)
               elements.Add(ui);
         }
         foreach (UIElement ui1 in elements)
            myCanvasMap.Children.Remove(ui1);
         //-------------------------------------------------------
         if (GamePhase.UnitTest == gi.GamePhase)
            return true;
         //-------------------------------------------------------
         foreach (IMapItem mi in gi.MainMapItems)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): mi=null");
               return false;
            }
            Button? b = myButtonMains.Find(mi.Name);
            if (null != b)
            {
               b.BeginAnimation(Canvas.LeftProperty, null); // end animation offset
               b.BeginAnimation(Canvas.TopProperty, null);  // end animation offset
               ITerritory t = mi.TerritoryCurrent;
               Double x = t.CenterPoint.X - (mi.Zoom * Utilities.theMapItemOffset);
               Double y = t.CenterPoint.Y - (mi.Zoom * Utilities.theMapItemOffset);
               Canvas.SetLeft(b, x);
               Canvas.SetTop(b, y);
               Canvas.SetZIndex(b, 9999);
            }
            else
            {
               if (false == CreateButtonMapItem(mi, 0))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMain(): CreateButtonMapItem() returned false");
                  return false;
               }
            }
         }
         //-------------------------------------------------------
         try
         {
            switch (action)
            {
               case GameAction.PreparationsStart:
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
            Console.WriteLine("UpdateCanvasMain() - EXCEPTION THROWN a=" + action.ToString() + "\ne={0}", e.ToString());
            return false;
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
               if( null == mim2 )
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
               mi.TerritoryStarting = mim2.NewTerritory;
               mi.TerritoryCurrent = mi.TerritoryStarting; 
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
         if ("Prince" != mim.MapItem.Name) // only prince is moved on map
            return true;
         const int ANIMATE_TIME_SEC = 2;
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): b=null for n=" + mim.MapItem.Name);
            return false;
         }
         Button? b = myButtonMains.Find(Utilities.RemoveSpaces(mim.MapItem.Name));
         if (null == b)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): b=null for n=" + mim.MapItem.Name);
            return false;
         }
         try
         {
            Canvas.SetZIndex(b, 100); // Move the button to the top of the Canvas
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
            double xEnd = mim.NewTerritory.CenterPoint.X - Utilities.theMapItemOffset;
            double yEnd = mim.NewTerritory.CenterPoint.Y - Utilities.theMapItemOffset;
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
         myCanvasMap.Children.Add(aPolyline);
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
      private void ClickButtonMapItem(object sender, RoutedEventArgs e)
      {
      }
      private void MouseEnterMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
      }
      private void MouseLeaveMapItem(object sender, System.Windows.Input.MouseEventArgs e)
      {
      }
      private void MouseDownPolygonTravel(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point canvasPoint = e.GetPosition(myCanvasMap);
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
