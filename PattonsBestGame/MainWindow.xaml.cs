using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Pattons_Best
{
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
   //===========================================================================
   public partial class MainWindow : Window
   {
      public static string theAssemblyDirectory = "";
      private IGameEngine? myGameEngine = null;
      private GameViewerWindow? myGameViewerWindow = null;
      //-----------------------------------------------------------------------
      public MainWindow()
      {
         InitializeComponent();
         try
         {
            //--------------------------------------------
            Assembly assem = Assembly.GetExecutingAssembly();
            string codeBase = assem.Location;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            String? assemDir = System.IO.Path.GetDirectoryName(path);
            if( null == assemDir)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): GameInstance() ctor error");
               Application.Current.Shutdown();
               return;
            }
            theAssemblyDirectory = assemDir;
            //--------------------------------------------
            MapImage.theImageDirectory = theAssemblyDirectory + @"\Images\";
            ConfigFileReader.theConfigDirectory = theAssemblyDirectory + @"\config\";
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Logger.theLogDirectory = appDataDir + @"\PattonsBest\Logs\";
            GameLoadMgr.theGamesDirectory = appDataDir + @"\PattonsBest\Games\";
            GameFeats.theGameFeatDirectory = appDataDir + @"\PattonsBest\GameData\";
            if (false == Directory.Exists(GameFeats.theGameFeatDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameFeats.theGameFeatDirectory);
            GameStatistics.theGameStatisticsDirectory = appDataDir + @"\PattonsBest\GameData\";
            if (false == Directory.Exists(GameStatistics.theGameStatisticsDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameStatistics.theGameStatisticsDirectory);
            //--------------------------------------------
            Utilities.InitializeRandomNumGenerators();
            //--------------------------------------------
            string iconFilename = MapImage.theImageDirectory + "PattonsBest.ico";
            Uri iconUri = new Uri(iconFilename, UriKind.Absolute);
            this.Icon = BitmapFrame.Create(iconUri);
            //--------------------------------------------
            if (false == Logger.SetInitial()) // tsetup logger
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): SetInitial() returned false");
               Application.Current.Shutdown();
               return;
            }
            if (false == SurnameMgr.SetInitial())
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): SurnameMgr.SetInitial() returned false");
               Application.Current.Shutdown();
               return;
            }
            IGameInstance gi = new GameInstance();
            if (true == gi.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): GameInstance() ctor error");
               Application.Current.Shutdown();
               return;
            }
            myGameEngine = new GameEngine(this);
            myGameViewerWindow = new GameViewerWindow(myGameEngine, gi); // Start the main view
            if (true == myGameViewerWindow.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): GameViewerWindow() ctor error");
               Application.Current.Shutdown();
               return;
            }
            myGameViewerWindow.Icon = this.Icon;
            myGameViewerWindow.Show(); // Finished initializing so show the window
            //--------------------------------------------
            try // copy user documentation to folder where user data is kept
            {
               string docs1Src = theAssemblyDirectory + @"\Docs\Pattons_Best-Summary.pdf";
               string docs2Src = theAssemblyDirectory + @"\Docs\PattonsBest-rules.pdf";
               string docsDir = appDataDir + @"\PattonsBest\Docs\";
               if (false == Directory.Exists(docsDir))
                  Directory.CreateDirectory(docsDir);
               string docs1Dest = docsDir + @"Pattons_Best-Summary.pdf";
               if (false == File.Exists(docs1Dest))
                  File.Copy(docs1Src, docs1Dest);
               string docs2Dest = docsDir + @"PattonsBest-rules.pdf";
               if (false == File.Exists(docs2Dest))
                  File.Copy(docs2Src, docs2Dest);
            }
            catch (Exception e)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): Copying docs to new folder caused exception e=" + e.ToString());
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "MainWindow() e=" + e.ToString());
            Application.Current.Shutdown();
            return;
         }
      }
      //-----------------------------------------------------------------------
      public void UpdateViews(IGameInstance gi, GameAction action)
      {
         if (null == myGameEngine)
            return;
         foreach (IView v in myGameEngine.Views)
         {
            if (null == v )
               Logger.Log(LogEnum.LE_ERROR, "UpdateView(): v=null");
            else
               v.UpdateView(ref gi, action);
         }

      }
   }
}