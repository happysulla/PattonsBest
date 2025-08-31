using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Point = System.Windows.Point;

namespace Pattons_Best
{
   public class TerritoryCreateUnitTest : IUnitTest
   {
      private static Double theEllipseDiameter = 20;
      private static Double theEllipseOffset = theEllipseDiameter / 2.0;
      //-----------------------------------------
      private string? myFileName = null;
      private DockPanel myDockPanelTop;
      private IGameInstance? myGameInstance = null;
      private CanvasImageViewer? myCanvasImageViewer = null;
      private Canvas? myCanvasTank = null;
      private Canvas? myCanvasMain = null;
      private int myTankNum = 1;
      private bool myIsDraggingMain = false;
      private bool myIsDraggingTank = false;
      private bool myIsBattleMapShown = false;
      private UIElement? myEllipseSelected = null;
      private Territory? myAnchorTerritory = null;
      private List<Ellipse> myEllipses = new List<Ellipse>();
      private readonly SolidColorBrush mySolidColorBrushWaterBlue = new SolidColorBrush { Color = Colors.DeepSkyBlue };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //-----------------------------------------
      private int myIndexName = 0;
      public bool CtorError { get; } = false;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      public TerritoryCreateUnitTest(DockPanel dp, IGameInstance gi, CanvasImageViewer civ)
      {
         myIndexName = 0;
         myHeaderNames.Add("02-Delete File");
         myHeaderNames.Add("02-Switch Main Canvas");
         myHeaderNames.Add("02-Switch Tank");
         myHeaderNames.Add("02-Delete Territory");
         myHeaderNames.Add("02-New Territories");
         myHeaderNames.Add("02-Set CenterPoints");
         myHeaderNames.Add("02-Verify Territories");
         myHeaderNames.Add("02-Set Adjacents");
         myHeaderNames.Add("02-Final");
         //------------------------------------
         myCommandNames.Add("00-Delete File");
         myCommandNames.Add("01-Switch Main Image");
         myCommandNames.Add("02-Change Tank Mat");
         myCommandNames.Add("03-Delete Territory");
         myCommandNames.Add("04-Click Canvas to Add");
         myCommandNames.Add("05-Click Elispse to Move");
         myCommandNames.Add("06-Click Ellispe to Verify");
         myCommandNames.Add("07-Verify Adjacents");
         myCommandNames.Add("08-Cleanup");
         //------------------------------------
         myDockPanelTop = dp;
         //------------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //------------------------------------
         if (null == civ)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): civ=null");
            CtorError = true;
            return;
         }
         myCanvasImageViewer = civ;
         //------------------------------------
         foreach (UIElement ui0 in myDockPanelTop.Children)
         {
            if (ui0 is DockPanel dockPanelInside)
            {
               foreach (UIElement ui1 in dockPanelInside.Children)
               {
                  if (ui1 is DockPanel dockpanelControl)
                  {
                     foreach (UIElement ui2 in dockpanelControl.Children)
                     {
                        if (ui2 is Canvas canvas)
                           myCanvasTank = canvas;  // Find the Canvas in the visual tree
                     }
                  }
                  if (ui1 is ScrollViewer sv)
                  {
                     if (sv.Content is Canvas canvas)
                        myCanvasMain = canvas;  // Find the Canvas in the visual tree
                  }
               }
            }
         }
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): myCanvasTank=null");
            CtorError = true;
            return;
         }
         if ( null == myCanvasMain )
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): myCanvasMain=null");
            CtorError = true;
            return;
         }
         //-------------------------------------
         foreach (UIElement ui in myCanvasMain.Children) // Clean the Canvas of all marks
         {
            if (ui is Image img)
            {
               if (true == img.Name.Contains("TankMat"))
               {
                  myCanvasTank.Children.Remove(img); // Remove the old image
                  break;
               }
            }
         }
         //-------------------------------------
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): gi.Reports.GetLast() returned null");
            CtorError = true;
            return;
         }
         //-------------------------------------
         myTankNum = report.TankCardNum;
         if (18 < myTankNum)
            myTankNum = 0;
         string tankMatName = "m";
         if (9 < myTankNum)
            tankMatName += myTankNum.ToString();
         else
            tankMatName += ("0" + myTankNum.ToString());
         Image image = new Image() { Name = "TankMat", Width = 600, Height = 500, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage(tankMatName) };
         myCanvasTank.Children.Add(image); // TankMat changes as get new tanks
         Canvas.SetLeft(image, 0);
         Canvas.SetTop(image, 0);
         //----------------------------------
         if ( false == SetFileName())
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): SetFileName() returned false");
            CtorError = true;
            return;
         }
      }
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasTank=null");
            return false;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasMain=null");
            return false;
         }
         if (null == myFileName)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myFileName=null");
            return false;
         }
         if (null == myCanvasImageViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasImageViewer=null");
            return false;
         }
         //-----------------------------------
         if (CommandName == myCommandNames[0])
         {
            System.IO.File.Delete(myFileName);  // delete old file
            Territories.theTerritories.Clear();
            if (false == NextTest(ref gi)) // automatically move next test
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): NextTest() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[1])  // Switch Main Image
         {
            if( false == DeleteEllipses() )
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): DeleteEllipses() returned false");
               return false;
            }
            if (true == myIsBattleMapShown)
            {
               myIsBattleMapShown = false;
               myCanvasImageViewer.ShowMovementMap(myCanvasMain);
            }
            else
            {
               myIsBattleMapShown = true;
               myCanvasImageViewer.ShowBattleMap(myCanvasMain);
            }
            if( false == CreateEllipses() )
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreateEllipses() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[2])  // Switch Tank Mat
         {
            if (false == DeleteEllipses())
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): DeleteEllipses() returned false");
               return false;
            }
            //-------------------------------------
            foreach (UIElement ui in myCanvasMain.Children) // Clean the Canvas of all marks
            {
               if (ui is Image img)
               {
                  if (true == img.Name.Contains("TankMat"))
                  {
                     myCanvasTank.Children.Remove(img); // Remove the old image
                     break;
                  }
               }
            }
            //-------------------------------------
            myTankNum++;
            if (18 < myTankNum)
               myTankNum = 0;
            string tankMatName = "m";
            if (9 < myTankNum)
               tankMatName += myTankNum.ToString();
            else
               tankMatName += ("0" + myTankNum.ToString());
            Image image = new Image() { Name = "TankMat", Width = 600, Height = 500, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage(tankMatName) };
            myCanvasTank.Children.Add(image); // TankMat changes as get new tanks
            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);
            //-------------------------------------
            if (false == CreateEllipses())
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreateEllipses() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[3])  // Delete Territory
         {

         }
         else if (CommandName == myCommandNames[4])  // New Territory
         {

         }
         else if (CommandName == myCommandNames[5]) // Move territories
         {

         }
         else if (CommandName == myCommandNames[6]) // verify territories
         {

         }
         else if (CommandName == myCommandNames[7]) // set adjacents
         {
            if (false == ShowAdjacents(Territories.theTerritories))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): ShowAdjacents() returned false");
               return false;
            }
         }
         else 
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): Cleanup() returned false");
               return false;
            }
         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi) // Move to the next test in this class's unit tests
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myGameInstance=null");
            return false;
         }
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myCanvasTank=null");
            return false;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myCanvasMain=null");
            return false;
         }
         //---------------------------------
         if (HeaderName == myHeaderNames[0])
         {
            ++myIndexName;
            if (false == CreateEllipses())
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): CreateEllipses() returned false");
               return false;
            }
         }
         else if (HeaderName == myHeaderNames[1]) // Switch Main Canvas Image
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[2]) // Switch Tank Mat
         {
            myCanvasTank.MouseLeftButtonDown += this.MouseLeftButtonDownDeleteTerritory;
            myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownDeleteTerritory;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3]) // Click to Add
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseLeftButtonDownDeleteTerritory;
            myCanvasTank.MouseLeftButtonDown += this.MouseLeftButtonDownCreateTerritory;
            //----------------------------
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownDeleteTerritory;
            myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownCreateTerritory;
            //----------------------------
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[4]) // Click Elispse to Move
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseLeftButtonDownCreateTerritory;
            myCanvasTank.MouseLeftButtonDown += this.MouseDownEllipseSetCenterPoint;
            myCanvasTank.MouseMove += MouseMove;
            myCanvasTank.MouseUp += MouseUp;
            //----------------------------
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownCreateTerritory;
            myCanvasMain.MouseLeftButtonDown += this.MouseDownEllipseSetCenterPoint;
            myCanvasMain.MouseMove += MouseMove;
            myCanvasMain.MouseUp += MouseUp;
            //----------------------------
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[5]) // Click Elispse to Verify
         {
            myCanvasTank.MouseMove -= MouseMove;
            myCanvasTank.MouseUp -= MouseUp;
            myCanvasTank.MouseLeftButtonDown -= this.MouseDownEllipseSetCenterPoint;
            myCanvasTank.MouseLeftButtonDown += this.MouseDownEllipseVerify;
            //----------------------------
            myCanvasMain.MouseMove -= MouseMove;
            myCanvasMain.MouseUp -= MouseUp;
            myCanvasMain.MouseLeftButtonDown -= this.MouseDownEllipseSetCenterPoint;
            myCanvasMain.MouseLeftButtonDown += this.MouseDownEllipseVerify;
            myAnchorTerritory = null;
            //----------------------------
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[6]) // Click Ellispe to Set Adjacents
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseDownEllipseVerify;
            myCanvasTank.MouseLeftButtonDown += this.MouseLeftButtonDownSetAdjacents;
            //----------------------------
            myCanvasMain.MouseLeftButtonDown -= this.MouseDownEllipseVerify;
            myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownSetAdjacents;
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else  // Verify Adjacents
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseLeftButtonDownSetAdjacents;
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownSetAdjacents;
            if (false == Cleanup(ref gi))
            {
               Console.WriteLine("TerritoryCreateUnitTest.Command(): Cleanup() returned false");
               return false;
            }
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi) // Remove an elipses from the canvas and save off Territories.xml file
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvasTank=null");
            return false;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvasMain=null");
            return false;
         }
         //---------------------------------
         if (HeaderName == myHeaderNames[1])
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseLeftButtonDownCreateTerritory;
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownCreateTerritory;
         }
         else if (HeaderName == myHeaderNames[2])
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseDownEllipseSetCenterPoint;
            myCanvasTank.MouseMove -= MouseMove;
            myCanvasTank.MouseUp -= MouseUp;
            myCanvasMain.MouseLeftButtonDown -= this.MouseDownEllipseSetCenterPoint;
            myCanvasMain.MouseMove -= MouseMove;
            myCanvasMain.MouseUp -= MouseUp;
         }
         else if (HeaderName == myHeaderNames[3])
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseDownEllipseVerify;
            myCanvasMain.MouseLeftButtonDown -= this.MouseDownEllipseVerify;
         }
         else if (HeaderName == myHeaderNames[4])
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseLeftButtonDownSetAdjacents;
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownSetAdjacents;
         }
         //--------------------------------------------------
         if( false == DeleteEllipses())
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): DeleteEllipses() returned false");
            return false;
         }
         //--------------------------------------------------
         if ( false == CreateXml(Territories.theTerritories))
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): CreateXml() returned false");
            return false;
         }
         //--------------------------------------------------
         ++gi.GameTurn;
         return true;
      }
      //--------------------------------------------------------------------
      private bool SetFileName()
      {
         string? path = ConfigFileReader.theConfigDirectory;
         if (null == path)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): path=null");
            return false;
         }
         System.IO.DirectoryInfo? dirInfo = Directory.GetParent(path);
         if (null == dirInfo)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): dirInfo=null");
            return false;
         }
         //----------------------------------
         string? path1 = dirInfo.FullName;
         if (null == path1)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): path1=null");
            return false;
         }
         dirInfo = Directory.GetParent(path1);
         if (null == dirInfo)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): dirInfo=null");
            return false;
         }
         //----------------------------------
         string? path2 = dirInfo.FullName;
         if (null == path2)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): path2=null");
            return false;
         }
         dirInfo = Directory.GetParent(path2);
         if (null == dirInfo)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): dirInfo=null");
            return false;
         }
         //----------------------------------
         string? path3 = dirInfo.FullName;
         if (null == path3)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): path3=null");
            return false;
         }
         dirInfo = Directory.GetParent(path3);
         if (null == dirInfo)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): dirInfo=null");
            return false;
         }
         //----------------------------------
         string? path4 = dirInfo.FullName;
         if (null == path4)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): path4=null");
            return false;
         }
         dirInfo = Directory.GetParent(path4);
         if (null == dirInfo)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): dirInfo=null");
            return false;
         }
         //----------------------------------
         myFileName = dirInfo.FullName + "\\Config\\" + Territories.FILENAME;
         return true;
      }
      private bool CreateEllipse(ITerritory territory, IMapPoint mp)
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipse(): myCanvasTank=null");
            return false;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipse(): myCanvasMain=null");
            return false;
         }
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush{ Color = Colors.Black };
         Ellipse aEllipse = new Ellipse
         {
            Name = territory.Name,
            Fill = aSolidColorBrush1,
            StrokeThickness = 1,
            Stroke = Brushes.Red,
            Width = theEllipseDiameter,
            Height = theEllipseDiameter
         };
         System.Windows.Point p = new System.Windows.Point(territory.CenterPoint.X, territory.CenterPoint.Y);
         p.X -= theEllipseOffset;
         p.Y -= theEllipseOffset;
         Canvas.SetLeft(aEllipse, mp.X);
         Canvas.SetTop(aEllipse, mp.Y);
         if ( "Main" == territory.CanvasName)
            myCanvasMain.Children.Add(aEllipse);
         else
            myCanvasTank.Children.Add(aEllipse);
         myEllipses.Add(aEllipse);
         return true;
      }
      private bool CreateEllipses()
      {
         myEllipses.Clear();
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvasTank=null");
            return false;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvasMain=null");
            return false;
         }
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // nearly transparent but slightly colored
         foreach (Territory t in Territories.theTerritories)
         {
            if( true == myIsBattleMapShown)
            {
               if (("A" == t.Type) || ("B" == t.Type) || ("C" == t.Type) || ("D" == t.Type) || ("E" == t.Type) )
                  continue;
            }
            else 
            {
               if ("Battle" == t.Type) 
                  continue;
            }
            if (("1" == t.Type) || ("2" == t.Type) || ("3" == t.Type) || ("4" == t.Type) || ("5" == t.Type) || ("6" == t.Type) || ("7" == t.Type) || ("8" == t.Type) || ("9" == t.Type) || ("10" == t.Type) || ("11" == t.Type) || ("12" == t.Type) || ("13" == t.Type) || ("14" == t.Type) || ("15" == t.Type) || ("16" == t.Type) || ("17" == t.Type) || ("18" == t.Type))
            {
               if (myTankNum.ToString() != t.Type)
                  continue;
            }
            Ellipse aEllipse = new Ellipse () { Name = t.Name };
            aEllipse.Fill = aSolidColorBrush0;
            aEllipse.StrokeThickness = 1;
            aEllipse.Stroke = Brushes.Red;
            aEllipse.Width = theEllipseDiameter;
            aEllipse.Height = theEllipseDiameter;
            System.Windows.Point p = new System.Windows.Point(t.CenterPoint.X, t.CenterPoint.Y);
            p.X -= theEllipseOffset;
            p.Y -= theEllipseOffset;
            Canvas.SetLeft(aEllipse, p.X);
            Canvas.SetTop(aEllipse, p.Y);
            myEllipses.Add(aEllipse);
            //-------------------------
            Label aLabel = new Label() { Foreground = Brushes.Red, FontFamily = myFontFam, FontWeight = FontWeights.Bold, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = t.Name };
            p.X -= theEllipseOffset;
            p.Y -= 2 * theEllipseOffset;
            Canvas.SetLeft(aLabel, p.X);
            Canvas.SetTop(aLabel, p.Y);
            //-------------------------
            if ("Main" == t.CanvasName)
            {
               myCanvasMain.Children.Add(aEllipse);
               myCanvasMain.Children.Add(aLabel);
            }
            else
            {
               myCanvasTank.Children.Add(aEllipse);
               //myCanvasTank.Children.Add(aLabel);
            } 
         }
         return true;
      }
      private bool DeleteEllipses()
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "DeleteEllipses(): myCanvasTank=null");
            return false;
         }
         //-------------------------------------------
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvasTank.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myCanvasTank.Children.Remove(ui1);
         //-------------------------------------------
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "DeleteEllipses(): myCanvasMain=null");
            return false;
         }
         //-------------------------------------------
         List<UIElement> results1 = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Ellipse)
               results1.Add(ui);
         }
         foreach (UIElement ui1 in results1)
            myCanvasMain.Children.Remove(ui1);
         //-------------------------------------------
         myEllipses.Clear();
         return true;
      }
      private bool CreateXml(ITerritories territories)
      {
         if (null == myFileName)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): myFileName=null");
            return false;
         }
         try
         {
            System.IO.File.Delete(myFileName);           // Delete Existing Territories.xml file and create a new one based on myGameEngine.Territories container
            //-----------------------------------------------------
            System.Xml.XmlDocument aXmlDocument = new XmlDocument();
            aXmlDocument.LoadXml("<Territories></Territories>");
            if (null == aXmlDocument.DocumentElement)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): aXmlDocument.DocumentElement=null");
               return false;
            }
            GameLoadMgr loadMgr = new GameLoadMgr();
            if (false == loadMgr.CreateXmlTerritories(aXmlDocument, territories))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories() returned false");
               return false;
            }
            //-----------------------------------------------------
            using (FileStream writer = new FileStream(myFileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
               XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
               using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
               {
                  aXmlDocument.Save(xmlWriter);
               }
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): exeption=\n" + e.Message);
            return false;
         }
         return true;
      }
      private bool ShowAdjacents(ITerritories territories)
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowAdjacents(): myCanvasTank=null");
            return false;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowAdjacents(): myCanvasMain=null");
            return false;
         }
         myAnchorTerritory = null;
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // completely clear
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) }; // almost clear
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };     // black
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         SolidColorBrush aSolidColorBrush4 = new SolidColorBrush { Color = Colors.Yellow };
         foreach (Territory anchorTerritory in territories)
         {
            if ("Tank" == anchorTerritory.CanvasName)
               continue;
            if (true == myIsBattleMapShown)
            {
               if ("Battle" != anchorTerritory.Type)
                  continue;
            }
            else
            {
               if ("Battle" == anchorTerritory.Type)
                  continue;
            }
            Ellipse? anchorEllipse = null; // Find the corresponding ellipse for this anchor territory
            foreach (UIElement ui in myCanvasMain.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  if (anchorTerritory.Name == ellipse.Name)
                  {
                     anchorEllipse = ellipse;
                     break;
                  }
               }
            }
            if (null == anchorEllipse)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowAdjacents(): anchorEllipse=null for " + anchorTerritory.Name);
               return false;
            }
            if (0 < anchorTerritory.Adjacents.Count)
               anchorEllipse.Fill = aSolidColorBrush4;
            // At this point, the anchorEllipse and the anchorTerritory are found.
            foreach (string s in anchorTerritory.Adjacents)
            {
               ITerritory? adjacentTerritory = null;
               foreach (ITerritory t in territories) // Find the River Territory corresponding to this name
               {
                  if (t.Name == s)
                  {
                     adjacentTerritory = t;
                     break;
                  }
               }
               if (null == adjacentTerritory)
               {
                  MessageBox.Show("ShowAdjacents(): Not Found s=" + s);
                  return false;
               }
               string adjacentName = Utilities.RemoveSpaces(adjacentTerritory.Name);
               Ellipse? adjacentEllipse = null; // Find the corresponding ellipse for this territory
               foreach (UIElement ui in myCanvasMain.Children)
               {
                  if (ui is Ellipse)
                  {
                     Ellipse ellipse = (Ellipse)ui;
                     if (adjacentName == ellipse.Name)
                     {
                        adjacentEllipse = ellipse;
                        break;
                     }
                  }
               }
               if (null == adjacentEllipse)
               {
                  Logger.Log(LogEnum.LE_ERROR, adjacentName);
                  MessageBox.Show(anchorTerritory.ToString());
                  return false;
               }
               // Search the Adjacent Territory  List to make sure the 
               // anchor territory is in that list. It should be bi directional.
               bool isReturnFound = false;
               foreach (String s1 in adjacentTerritory.Adjacents)
               {
                  string returnName = s1;
                  if (returnName == anchorTerritory.Name)
                  {
                     isReturnFound = true; // Yes the adjacent River has a entry to return the River back to the anchor territory
                     break;
                  }
               }
               // Anchor Property not found in the adjacent property territory.  This is an error condition.
               if (false == isReturnFound)
               {
                  anchorEllipse.Fill = aSolidColorBrush3; // change color of two ellipses to signify error
                  adjacentEllipse.Fill = aSolidColorBrush2;
                  StringBuilder sb = new StringBuilder("anchor="); 
                  sb.Append(anchorTerritory.Name); 
                  sb.Append(" NOT in list for adjacent="); 
                  sb.Append(adjacentName);
                  MessageBox.Show(sb.ToString());
                  return false;
               }
            }
         }
         return true;
      }
      //--------------------------------------------------------------------
      void MouseLeftButtonDownDeleteTerritory(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownDeleteTerritory(): myCanvasTank=null");
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownDeleteTerritory(): myCanvasMain=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         //--------------------------------------------
         Ellipse? selectedEllipse = null;
         foreach (UIElement ui in myCanvasTank.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ellipse.IsMouseOver)
               {
                  selectedEllipse = ellipse;
                  break;
               }
            }
         }
         if( null == selectedEllipse)
         {
            foreach (UIElement ui in myCanvasMain.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  if (true == ellipse.IsMouseOver)
                  {
                     selectedEllipse = ellipse;
                     break;
                  }
               }
            }
         }
         if (null == selectedEllipse)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownDeleteTerritory(): selectedEllipse=null");
            return;
         }
         //--------------------------------------------
         ITerritory? t = Territories.theTerritories.Find(selectedEllipse.Name, myTankNum.ToString());
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownDeleteTerritory(): t=null for ellipse.Name=" + selectedEllipse.Name);
            return;
         }
         Territories.theTerritories.Remove(t);
         MessageBox.Show(selectedEllipse.Name);
         myEllipses.Remove(selectedEllipse);
         myCanvasMain.Children.Remove(selectedEllipse);
      }
      void MouseLeftButtonDownCreateTerritory(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         if( p.X < 0.0 )
            p = e.GetPosition(myCanvasTank);
         TerritoryCreateDialog dialog = new TerritoryCreateDialog(myCanvasMain, myCanvasTank); // Get the name from user
         dialog.myTextBoxName.Focus();
         if (true == dialog.ShowDialog())
         {
            Territory territory = new Territory(dialog.myTextBoxName.Text) { CenterPoint = new MapPoint(p.X, p.Y) };
            territory.CanvasName = TerritoryCreateDialog.theParentChecked;
            if( "Main" == territory.CanvasName)
               territory.Type = TerritoryCreateDialog.theTypeChecked;
            else
               territory.Type = TerritoryCreateDialog.theCardChecked;
            Territories.theTerritories.Add(territory);
            if ( false == CreateEllipse(territory, territory.CenterPoint))
            {
               Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownCreateTerritory(): CreateEllipse() returned false");
               return;
            }
         }
      }
      void MouseDownEllipseSetCenterPoint(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseSetCenterPoint(): myCanvasTank=null");
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseSetCenterPoint(): myCanvasMain=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         Console.WriteLine("TerritoryUnitTest.MouseDown(): {0}", p.ToString());
         foreach (UIElement ui in myCanvasTank.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  if (false == myIsDraggingTank)
                  {
                     string showText = ellipse.Name + ":" + myTankNum.ToString();
                     MessageBox.Show(showText);
                     this.myIsDraggingTank = true;
                     this.myEllipseSelected = ui;
                  }
               }
            }
         }
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  if (false == myIsDraggingMain)
                  {
                     MessageBox.Show(ellipse.Name);
                     this.myIsDraggingMain = true;
                     this.myEllipseSelected = ui;
                  }
               }
            }
         }
      }
      void MouseMove(object sender, MouseEventArgs e)
      {
         if (true == myIsDraggingMain)
         {
            if (null != myEllipseSelected)
            {
               System.Windows.Point newPoint = e.GetPosition(myCanvasMain);
               Canvas.SetTop(myEllipseSelected, newPoint.Y - theEllipseOffset);
               Canvas.SetLeft(myEllipseSelected, newPoint.X - theEllipseOffset);
            }
         }
         else if (true == myIsDraggingTank)
         {
            if (null != myEllipseSelected)
            {
               System.Windows.Point newPoint = e.GetPosition(myCanvasTank);
               Canvas.SetTop(myEllipseSelected, newPoint.Y - theEllipseOffset);
               Canvas.SetLeft(myEllipseSelected, newPoint.X - theEllipseOffset);
            }
         }
      }
      void MouseUp(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point newPoint = new Point();
         if( true == myIsDraggingMain)
            newPoint = e.GetPosition(myCanvasMain);
         else
            newPoint = e.GetPosition(myCanvasTank);
         this.myIsDraggingTank = false;
         this.myIsDraggingMain = false;
         if (null != this.myEllipseSelected)
         {
            if (this.myEllipseSelected is Ellipse)
            {
               Ellipse? ellipse = (Ellipse)myEllipseSelected;
               if( null == ellipse)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseUp(): ellipse=null");
                  return;
               }
               string? name1 = ellipse.Name;
               if (null == name1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseUp(): name1=null");
                  return;
               }
               foreach (Territory t in Territories.theTerritories)
               {
                  if ( (name1 == t.Name) && (myTankNum.ToString() == t.Type) )
                  {
                     t.CenterPoint.X = newPoint.X;
                     t.CenterPoint.Y = newPoint.Y;
                     this.myEllipseSelected = null;
                     break;
                  }
               }
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.MouseUp() this.myEllipseSelected=null");
         }
      }
      void MouseDownEllipseVerify(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): myCanvasTank=null");
            return;
         }
         foreach (UIElement ui in myCanvasTank.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  string? name = ellipse.Name;
                  if (null == name)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): name=null");
                     return;
                  }
                  foreach (Territory t in Territories.theTerritories)
                  {
                     string tName = t.Name;
                     string tType = t.Type;
                     if ((tName == name) && (tType == myTankNum.ToString()))   
                     {
                        TerritoryVerifyDialog dialog = new TerritoryVerifyDialog(t);
                        dialog.myButtonOk.Focus();
                        if (true == dialog.ShowDialog())
                        {
                           t.CanvasName = dialog.RadioOutputParent;
                           t.Type = dialog.RadioOutputType;
                           return;
                        }
                     }
                  }
               }
            }
         }
         //-----------------------------------------------------------------------
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): myCanvasMain=null");
            return;
         }
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  string? name = ellipse.Name;
                  if (null == name)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipseVerify(): name=null");
                     return;
                  }
                  foreach (Territory t in Territories.theTerritories)
                  {
                     string? name1 = t.Name;
                     if (name1 == name)
                     {
                        TerritoryVerifyDialog dialog = new TerritoryVerifyDialog(t);
                        dialog.myButtonOk.Focus();
                        if (true == dialog.ShowDialog())
                        {
                           t.CanvasName = dialog.RadioOutputParent;
                           t.Type = dialog.RadioOutputType;
                           return;
                        }
                     }
                  }
               }
            }
         }
      }
      void MouseLeftButtonDownSetAdjacents(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetAdjacents(): myCanvasMain=null");
            return;
         }
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) };
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse selectedEllipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  Territory? selectedTerritory = null;  // Find the corresponding Territory that user selected
                  foreach (Territory t in Territories.theTerritories)
                  {
                     if ( selectedEllipse.Name == t.Name )
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
                  if (selectedTerritory == null) // Check for error
                  {
                     MessageBox.Show("Unable to find " + selectedEllipse.Name);
                     return;
                  }
                  if (null == myAnchorTerritory)  // If there is no anchor territory. Set it.
                  {
                     StringBuilder sb = new StringBuilder("Anchoring: ");
                     sb.Append(selectedEllipse.Name);
                     sb.Append(" ");
                     sb.Append(selectedTerritory.ToString());
                     sb.Append(" ");
                     Console.WriteLine("Anchoring {0} ", selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = selectedTerritory;
                     myAnchorTerritory.Adjacents.Clear();
                     selectedEllipse.Fill = aSolidColorBrush3;
                     return;
                  }
                  if (selectedTerritory.ToString() != myAnchorTerritory.ToString())
                  {
                     // If the matching territory is not the anchor territory, change its color.
                     selectedEllipse.Fill = aSolidColorBrush2;
                     // Find if the territory is already in the list. Only add it if it is not already added.
                     IEnumerable<string> results = from s in myAnchorTerritory.Adjacents where s == selectedTerritory.ToString() select s;
                     if (0 == results.Count())
                     {
                        Console.WriteLine("Adding {0} ", selectedTerritory.ToString());
                        myAnchorTerritory.Adjacents.Add(selectedTerritory.ToString());
                     }
                  }
                  else
                  {
                     // If this is the matching territory is the anchor territory, the user is requesting that it they are done adding 
                     // to the adjacents ellipse. Clear the data so another one can be selected.
                     StringBuilder sb = new StringBuilder("Saving"); 
                     sb.Append(selectedEllipse.Name); 
                     sb.Append(" "); sb.Append(myAnchorTerritory.ToString());
                     sb.Append(" "); sb.Append(selectedTerritory.ToString()); 
                     sb.Append(" ");
                     Console.WriteLine("Saving {0} ", selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = null;
                     foreach (UIElement ui1 in myCanvasMain.Children)
                     {
                        if (ui1 is Ellipse)
                        {
                           Ellipse ellipse1 = (Ellipse)ui1;
                           foreach (Territory t in Territories.theTerritories)
                           {
                              if ( (ellipse1.Name == t.Name) && (t.Type == myTankNum.ToString()) )
                              {
                                 if (0 == t.Adjacents.Count)
                                    ellipse1.Fill = aSolidColorBrush0;
                                 else
                                    ellipse1.Fill = aSolidColorBrush1;
                                 break;
                              }
                           }
                        }
                     }
                  } // else (selectedTerritory.ToString() != myAnchorTerritory.ToString())
               } // if (true == ui.IsMouseOver)
            } // if (ui is Ellipse)
         }  // foreach (UIElement ui in myCanvasMain.Children)
      }
   }
}

