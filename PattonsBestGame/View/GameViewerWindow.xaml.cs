﻿using Pattons_Best.Properties;
using System.ComponentModel;
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
      private readonly SolidColorBrush mySolidColorBrushStart = new SolidColorBrush() { Color = Colors.Gold };
      private readonly SolidColorBrush mySolidColorBrushRed = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushPurple = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushRosyBrown = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushOrange = new SolidColorBrush();
      private readonly SolidColorBrush mySolidColorBrushRest = new SolidColorBrush { Color = Colors.Yellow };
      private readonly SolidColorBrush mySolidColorBrushSkyBlue = new SolidColorBrush { Color = Colors.LightBlue };
      private readonly SolidColorBrush mySolidColorBrushWaterBlue = new SolidColorBrush { Color = Colors.DeepSkyBlue };
      private readonly SolidColorBrush mySolidColorBrushWaterDark = new SolidColorBrush { Color = Colors.SteelBlue };
      private readonly SolidColorBrush mySolidColorBrushFollow = new SolidColorBrush { Color = Colors.HotPink };
      private readonly SolidColorBrush mySolidColorBrushPath = new SolidColorBrush { Color = Colors.White };
      //---------------------------------------------------------------------
      private readonly List<Button> myButtonMapItems = new List<Button>();
      private readonly SplashDialog mySplashScreen;
      private Button[] myButtonTimeTrackDays = new Button[7];
      private Button[] myButtonTimeTrackWeeks = new Button[15];
      private Button[] myButtonFoodSupply1s = new Button[10];
      private Button[] myButtonFoodSupply10s = new Button[10];
      private Button[] myButtonFoodSupply100s = new Button[5];
      private Button[] myButtonEndurances = new Button[12];
      private readonly List<Button> myButtonDailyAcions = new List<Button>();
      private readonly string[] myButtonDailyContents = new string[MAX_DAILY_ACTIONS] { "Travel", "Rest", "News", "Hire", "Audience", "Offering", "Search Ruins", "Search Cache", "Search Clue", "Arch Travel", "Follow", "Rafting", "Air Travel", "Steal Gems", "Rescue", "Attack" };
      //---------------------------------------------------------------------
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
         Image imageMap = new Image() { Name = "Map", Width = 810, Height = 985, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("Map") };
         myCanvas.Children.Add(imageMap);
         Canvas.SetLeft(imageMap, 0);
         Canvas.SetTop(imageMap, 0);
         //---------------------------------------------------------------
         myGameEngine = ge;
         myGameInstance = gi;
         gi.GamePhase = GamePhase.GameSetup;
         myMainMenuViewer = new MainMenuViewer(myMainMenu, ge, gi);
         if (false == AddHotKeys(myMainMenuViewer))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): AddHotKeys() returned false");
            CtorError = true;
            return;
         }
         Options options = Deserialize(Settings.Default.GameOptions);
         myMainMenuViewer.NewGameOptions = options;
         gi.Options = options; // use the new game options for setting up the first game
         //---------------------------------------------------------------
         if (false == String.IsNullOrEmpty(Settings.Default.GameDirectoryName))
            GameLoadMgr.theGamesDirectory = Settings.Default.GameDirectoryName; // remember the game directory name
         //---------------------------------------------------------------
         Utilities.ZoomCanvas = Settings.Default.ZoomCanvas;
         myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas); // Constructor - revert to save zoom
         StatusBarViewer sbv = new StatusBarViewer(myStatusBar, ge, gi, myCanvas);
         //---------------------------------------------------------------
         Utilities.theBrushBlood.Color = Color.FromArgb(0xFF, 0xA4, 0x07, 0x07);
         Utilities.theBrushRegion.Color = Color.FromArgb(0x7F, 0x11, 0x09, 0xBB); // nearly transparent but slightly colored
         Utilities.theBrushRegionClear.Color = Color.FromArgb(0, 0, 0x01, 0x0); // nearly transparent but slightly colored
         Utilities.theBrushControlButton.Color = Color.FromArgb(0xFF, 0x43, 0x33, 0xFF); // menu blue
         Utilities.theBrushScrollViewerActive.Color = Color.FromArgb(0xFF, 0xB9, 0xEA, 0x9E); // light green 
         Utilities.theBrushScrollViewerInActive.Color = Color.FromArgb(0x17, 0x00, 0x00, 0x00); // gray
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
         myDieRoller = new DieRoller(myCanvas, CloseSplashScreen); // Close the splash screen when die resources are loaded
         if (true == myDieRoller.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerWindow(): myDieRoller.CtorError=true");
            CtorError = true;
            return;
         }
         //---------------------------------------------------------------
         myEventViewer = new EventViewer(myGameEngine, myGameInstance, myCanvas, myScrollViewerTextBlock, myStackPanelEndurance, Territory.theTerritories, myDieRoller);
         //---------------------------------------------------------------
         // Implement the Model View Controller (MVC) pattern by registering views with
         // the game engine such that when the model data is changed, the views are updated.
         ge.RegisterForUpdates(myMainMenuViewer);
         ge.RegisterForUpdates(myEventViewer);
         ge.RegisterForUpdates(sbv);
         ge.RegisterForUpdates(this); // needs to be last so that canvas updates after all actions taken
         Logger.Log(LogEnum.LE_GAME_INIT, "GameViewerWindow(): \nzoomCanvas=" + Settings.Default.ZoomCanvas.ToString() + "\nwp=" + Settings.Default.WindowPlacement + "\noptions=" + Settings.Default.GameOptions);
#if UT1
            if (false == ge.CreateUnitTests(gi, myDockPanelTop, myEventViewer, myDieRoller))
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
            this.myCanvas.Cursor = myTargetCursor;
         }
         //-------------------------------------------------------
         else if ((GameAction.UpdateLoadingGame == action) || (GameAction.UpdateNewGame == action) )
         {
            myGameInstance = gi;
            myButtonMapItems.Clear();
            foreach (UIElement ui in myCanvas.Children) // remove all buttons on map
            {
               if (ui is Button b)
               {
                  if (true == b.Name.Contains("Prince"))
                  {
                     myCanvas.Children.Remove(ui);
                     break;
                  }
               }
            }
            myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas); // UploadNewGame - Return to previous saved zoom level
            this.Title = UpdateTitle(gi.Options);
         }
         switch (action)
         {
            case GameAction.ShowInventory:
            case GameAction.ShowRuleListing:
            case GameAction.ShowEventListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
               break;
            case GameAction.EndGameWin:
            case GameAction.EndGameLost:
               SaveDefaultsToSettings();
               break;
            case GameAction.RemoveSplashScreen:
               this.Title = UpdateTitle(gi.Options);
               if (false == UpdateCanvas(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvas() returned error ");
               mySplashScreen.Close();
               myScollViewerInside.UpdateLayout();
               //UpdateScrollbarThumbnails(gi.Prince.Territory);
               break;
            case GameAction.UpdateGameOptions:
               this.Title = UpdateTitle(gi.Options);
               SaveDefaultsToSettings();
               break;
            case GameAction.EndGameFinal:
               myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);  // EndGameFinal - show map for last time
               if (false == UpdateCanvas(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvas() returned error ");
               break;
            case GameAction.SetupChooseFunOptions:
            case GameAction.SetupFinalize:
               this.Title = UpdateTitle(gi.Options);
               if ( 1.0 == Utilities.ZoomCanvas)
                  Utilities.ZoomCanvas = 2.0;
               myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
               if (false == UpdateCanvas(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvas() returned error ");
               break;
            default:
               if (false == UpdateCanvas(gi, action))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): UpdateCanvas() returned error ");
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
         string territoryName = Utilities.RemoveSpaces(mi.TerritoryCurrent.ToString());
         ITerritory territory = Territory.theTerritories.Find(territoryName);
         if (null == territory)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItem(): TerritoryExtensions.Find() returned null");
            return false;
         }
         System.Windows.Controls.Button b = new Button { ContextMenu = myContextMenuButton, Name = Utilities.RemoveSpaces(mi.Name), Width = mi.Zoom * Utilities.theMapItemSize, Height = mi.Zoom * Utilities.theMapItemSize, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
         Canvas.SetLeft(b, territory.CenterPoint.X - mi.Zoom * Utilities.theMapItemOffset + (counterCount * Utilities.STACK));
         Canvas.SetTop(b, territory.CenterPoint.Y - mi.Zoom * Utilities.theMapItemOffset + (counterCount * Utilities.STACK));
         MapItem.SetButtonContent(b, mi, false, false, false, false); // This sets the image as the button's content
         myButtonMapItems.Add(b);
         myCanvas.Children.Add(b);
         Canvas.SetZIndex(b, counterCount);
         b.Click += ClickButtonMapItem;
         b.MouseEnter += MouseEnterMapItem;
         b.MouseLeave += MouseLeaveMapItem;
         return true;
      }
      //---------------------------------------
      private Options Deserialize(String s_xml)
      {
         Options options = new Options();
         if (false == String.IsNullOrEmpty(s_xml))
         {
            try // XML serializer does not work for Interfaces
            {
               StringReader stringreader = new StringReader(s_xml);
               XmlReader xmlReader = XmlReader.Create(stringreader);
               XmlSerializer serializer = new XmlSerializer(typeof(Options)); // Sustem.IO.FileNotFoundException thrown but normal behavior - handled in XmlSerializer constructor
               options = (Options)serializer.Deserialize(xmlReader);
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
         Option option = options.Find(name);
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
      private bool UpdateCanvas(IGameInstance gi, GameAction action, bool isOnlyLastLegRemoved = false)
      {
         //-------------------------------------------------------
         // Clean the Canvas of all marks
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvas.Children)
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
               if ("Map" == img.Name)
                  continue;
               elements.Add(ui);
            }
            if (ui is TextBlock tb)
               elements.Add(ui);
         }
         foreach (UIElement ui1 in elements)
            myCanvas.Children.Remove(ui1);
         //-------------------------------------------------------
         if (GamePhase.UnitTest == gi.GamePhase)
            return true;
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
            Console.WriteLine("UpdateCanvas() - EXCEPTION THROWN a=" + action.ToString() + "\ne={0}", e.ToString());
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
               IMapItemMove mim2 = gi.MapItemMoves[0];
               IMapItem mi = mim2.MapItem;
               if (false == MovePathAnimate(mim2))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCanvasMovement(): MovePathAnimate() returned false t=" + mim2.OldTerritory.ToString());
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "UpdateCanvasMovement(): MovePathAnimate() returned false gi.MapItemMoves.Clear()");
                  gi.MapItemMoves.Clear();
                  return false;
               }
               mi.TerritoryCurrent = mim2.NewTerritory;
               mi.TerritoryStarting = mim2.NewTerritory;
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
         Button b = myButtonMapItems.Find(Utilities.RemoveSpaces(mim.MapItem.Name));
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
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovePathDisplay(): mim.NewTerritory=null");
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
         myCanvas.Children.Add(aPolyline);
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
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
         Polygon? clickedPolygon = (Polygon)sender;
         if (null == clickedPolygon)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygonTravel(): clickedPolygon=null for " + clickedPolygon.Tag.ToString());
            return;
         }
         myTerritorySelected = Territory.theTerritories.Find(Utilities.RemoveSpaces(clickedPolygon.Tag.ToString()));
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
         if (0 < mapPanelHeight) // Need to resize to take up panel content not taken by menu and status bar
         {
            myDockPanelInside.Height = mapPanelHeight;
            myScollViewerInside.Height = mapPanelHeight;
         }
         double mapPanelWidth = myDockPanelTop.ActualWidth - myDockPanelControls.ActualWidth - System.Windows.SystemParameters.VerticalScrollBarWidth;
         if (0 < mapPanelWidth) // need to resize so that scrollbar takes up panel not allocated to Control's DockPanel, i.e. where app controls are shown
            myScollViewerInside.Width = mapPanelWidth;
      }
      private void SizeChangedGameViewerWindow(object sender, SizeChangedEventArgs e)
      {
         double mapPanelHeight = myDockPanelTop.ActualHeight - myMainMenu.ActualHeight - myStatusBar.ActualHeight;
         if (0 < mapPanelHeight) // Need to resize to take up panel content not taken by menu and status bar
         {
            myDockPanelInside.Height = mapPanelHeight;
            myScollViewerInside.Height = mapPanelHeight;
         }
         double mapPanelWidth = myDockPanelTop.ActualWidth - myDockPanelControls.ActualWidth - System.Windows.SystemParameters.VerticalScrollBarWidth;
         if (0 < mapPanelWidth) // need to resize so that scrollbar takes up panel not allocated to Control's DockPanel, i.e. where app controls are shown
            myScollViewerInside.Width = mapPanelWidth;
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
               myScollViewerInside.Height = Settings.Default.ScrollViewerHeight;
            if (0.0 != Settings.Default.ScrollViewerWidth)
               myScollViewerInside.Width = Settings.Default.ScrollViewerWidth;
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
         if (false == GameLoadMgr.SaveGameToFile(myGameInstance))
            Logger.Log(LogEnum.LE_ERROR, "OnClosing(): SaveGameToFile() returned false");
      }
      //-------------CONTROLLER HELPER FUNCTIONS---------------------------------
      private void SaveDefaultsToSettings()
      {
         WindowPlacement wp; // Persist window placement details to application settings
         var hwnd = new WindowInteropHelper(this).Handle;
         if (false == GetWindowPlacement(hwnd, out wp))
            Logger.Log(LogEnum.LE_ERROR, "OnClosing(): GetWindowPlacement() returned false");
         string sWinPlace = Utilities.Serialize<WindowPlacement>(wp);
         Settings.Default.WindowPlacement = sWinPlace;
         //-------------------------------------------
         Settings.Default.ZoomCanvas = Utilities.ZoomCanvas;
         //-------------------------------------------
         Settings.Default.ScrollViewerHeight = myScollViewerInside.Height;
         Settings.Default.ScrollViewerWidth = myScollViewerInside.Width;
         //-------------------------------------------
         Settings.Default.GameDirectoryName = GameLoadMgr.theGamesDirectory;
         //-------------------------------------------
         string sOptions = Utilities.Serialize<Options>(myGameInstance.Options);
         Settings.Default.GameOptions = sOptions;
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
            InputBindings.Add(new KeyBinding(command, keyGesture));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.I, ModifierKeys.Control);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            CommandBindings.Add(new CommandBinding(command, mmv.MenuItemViewInventory_Click));
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
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.F4, ModifierKeys.None);
            InputBindings.Add(new KeyBinding(command, keyGesture));
            //------------------------------------------------
            command = new RoutedCommand();
            keyGesture = new KeyGesture(Key.F5, ModifierKeys.None);
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
      public static Button Find(this IList<Button> list, string name)
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