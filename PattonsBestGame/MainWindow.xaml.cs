using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pattons_Best
{
   public partial class MainWindow : Window
   {
      private IGameEngine? myGameEngine = null;
      private GameViewerWindow? myGameViewerWindow = null;
      //-----------------------------------------------------------------------
      public MainWindow()
      {
         InitializeComponent();
         try
         {
            //--------------------------------------------
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().Location;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            string? assemblyDir = System.IO.Path.GetDirectoryName(path);
            if (null == assemblyDir)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): GameInstance() ctor error");
               Application.Current.Shutdown();
               return;
            }
            MapImage.theImageDirectory = assemblyDir + @"\Images\";
            ConfigFileReader.theConfigDirectory = assemblyDir + @"\config\";
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Logger.theLogDirectory = appDataDir + @"\PattonsBest\Logs\";
            GameLoadMgr.theGamesDirectory = appDataDir + @"\PattonsBest\Games\";
            //--------------------------------------------
            Utilities.InitializeRandomNumGenerators();
            //--------------------------------------------
            string iconFilename = MapImage.theImageDirectory + "PattonsBest.ico";
            Uri iconUri = new Uri(iconFilename, UriKind.Absolute);
            this.Icon = BitmapFrame.Create(iconUri);
            //--------------------------------------------
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
               //string docs1Src = assemblyDir + @"\Docs\BP2-eventsbook_singleA4.pdf";
               //string docs2Src = assemblyDir + @"\Docs\BP2-rulesbook_singleA4.pdf";
               //string docsDir = appDataDir + @"\Pattons_Best\Docs\";
               //if (false == Directory.Exists(docsDir))
               //   Directory.CreateDirectory(docsDir);
               //string docs1Dest = assemblyDir + @"\Docs\BP2-eventsbook_singleA4.pdf";
               //if (false == File.Exists(docs1Dest))
               //   File.Copy(docs1Src, docs1Dest);
               //string docs2Dest = assemblyDir + @"\Docs\BP2-rulesbook_singleA4.pdf";
               //if (false == File.Exists(docs2Dest))
               //   File.Copy(docs1Src, docs2Dest);
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