using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Point = System.Windows.Point;

namespace Pattons_Best
{
   public class TerritoryRegionUnitTest : IUnitTest
   {
      private static Double theEllipseDiameter = 20;
      private static Double theEllipseOffset = theEllipseDiameter / 2.0;
      //--------------------------------------------------------
      private IGameInstance? myGameInstance = null;
      private string? myFileName = null;
      private DockPanel? myDockPanel = null;
      private Canvas? myCanvasTank = null;
      private Canvas? myCanvasMain = null;
      private int myTankNum = 1;
      private CanvasImageViewer? myCanvasImageViewer = null;
      private bool myIsBattleMapShown = false;
      ITerritory? myAnchorTerritory = null;
      private List<Ellipse> myEllipses = new List<Ellipse>();
      private List<IMapPoint> myPoints = new List<IMapPoint>();
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //--------------------------------------------------------
      public bool CtorError { get; } = false;
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      //--------------------------------------------------------
      public TerritoryRegionUnitTest(DockPanel dp, IGameInstance gi, CanvasImageViewer civ)
      {
         myIndexName = 0;
         myHeaderNames.Add("03-Delete Regions");
         myHeaderNames.Add("03-Switch Map");
         myHeaderNames.Add("03-Switch Tank");
         myHeaderNames.Add("03-Add Regions");
         myHeaderNames.Add("03-Select Random Pt");
         myHeaderNames.Add("03-Finish");
         //------------------------------------
         myCommandNames.Add("00-Regions");
         myCommandNames.Add("01-Switch Map");
         myCommandNames.Add("02-Switch Tank");
         myCommandNames.Add("03-Add Regions");
         myCommandNames.Add("04-Random Pt");
         myCommandNames.Add("05-Cleanup");
         //------------------------------------
         myDockPanel = dp;
         //------------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryRegionUnitTest(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //------------------------------------
         if (null == civ)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryRegionUnitTest(): civ=null");
            CtorError = true;
            return;
         }
         myCanvasImageViewer = civ;
         //------------------------------------
         foreach (UIElement ui0 in myDockPanel.Children)
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
         //-------------------------------------
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryRegionUnitTest(): myCanvasTank=null");
            CtorError = true;
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryRegionUnitTest(): myCanvasMain=null");
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
            Logger.Log(LogEnum.LE_ERROR, "TerritoryRegionUnitTest(): gi.Reports.GetLast() returned null");
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
         if (false == SetFileName())
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryRegionUnitTest(): SetFileName() returned false");
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
         if (CommandName == myCommandNames[0])
         {
            //--------------------------------------------
            // Remove all Ellipse and Polygons
            if( false == DeleteEllipsesAndPolygons())
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryRegionUnitTest.Command(): DeleteEllipsesAndPolygons() returned false");
               return false;
            }
            foreach (ITerritory t in Territories.theTerritories)
               t.Points.Clear();
            if (false == NextTest(ref gi)) // automatically move next test
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryRegionUnitTest.Command(): NextTest() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[1])
         {
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
            if (false == CreateEllipses(true))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreateEllipses() returned false");
               return false;
            }
            if (false == CreatePolygons())
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreatePolygons() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[2])
         {
            if (false == DeleteEllipsesAndPolygons())
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): DeleteEllipsesAndPolygons() returned false");
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
            if (false == CreateEllipses(true))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreateEllipses() returned false");
               return false;
            }
            if (false == CreatePolygons())
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): CreatePolygons() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[3])
         {

         }
         else if (CommandName == myCommandNames[4])
         {

         }
         else
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Cleanup() returned false");
               return false;
            }
         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi) // Move to the next test in this class's unit tests
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myCanvasMain=null");
            return false;
         }
         if (HeaderName == myHeaderNames[0])
         {
            ++myIndexName;
            if (false == CreateEllipses(true))
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): CreateEllipses() returned false");
               return false;
            }
            if (false == CreatePolygons())
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): CreatePolygons() returned false");
               return false;
            }
         }
         else if (HeaderName == myHeaderNames[1])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[2])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3])
         {
            ++myIndexName;
            if (false == DeleteEllipsesAndPolygons())
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): DeleteEllipsesAndPolygons() returned false");
               return false;
            }
            if (false == CreateEllipses(false))
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): CreateEllipses() returned false");
               return false;
            }
         }
         else if (HeaderName == myHeaderNames[4])
         {
            ++myIndexName;
         }
         else
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup() returned false");
               return false;
            }
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi) // Remove an elipses from the canvas and save off Territories.xml file
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvasMain=null");
            return false;
         }
         //--------------------------------------------------
         if( false == DeleteEllipsesAndPolygons())
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): DeleteEllipsesAndPolygons() returned false");
            return false;
         }
         //--------------------------------------------------
         if (false == CreateXml(Territories.theTerritories))
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): CreateXml() returned false");
            return false;
         }
         //--------------------------------------------------
         ++gi.GameTurn;
         return true;
      }
      //--------------------------------------------------------
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
      private bool CreatePoint(IMapPoint mp)
      {
         if (null != myAnchorTerritory) // Add points to the anchor territy that define the region
         {
            string tType = myTankNum.ToString();
            // Do an intersection with any other points that
            // are part of any other region.  If a point is found
            // that is very close, assume that is the correct
            // point to add instead of the mouse click.
            double minDistance = 5;
            IMapPoint selectedMp = mp;
            foreach (String s in myAnchorTerritory.Adjacents)
            {
               ITerritory? adjacentTerritory = Territories.theTerritories.Find(s, tType);
               if (null == adjacentTerritory) // Check for error
               {
                  MessageBox.Show("Unable to find " + s);
                  return false;
               }
               foreach (IMapPoint mp1 in adjacentTerritory.Points)
               {
                  double distance = getRange(mp, mp1);
                  // Find the minimum distance between this point and any adjacent territory point.
                  // Use that point if it is below a set amount.
                  if (distance < minDistance)
                  {
                     minDistance = distance;
                     selectedMp.X = mp1.X;
                     selectedMp.Y = mp1.Y;
                     System.Diagnostics.Debug.WriteLine("\t\t==> {0} from {1} with d={2}", selectedMp.ToString(), adjacentTerritory.Name, distance);
                  }
               }
            }  // end foreach()
            System.Diagnostics.Debug.WriteLine("\t\t++>{0} to {1}", selectedMp.ToString(), myAnchorTerritory.Name); // An adjacent point was not found.  Add the mouse click point as a new point.
            myPoints.Add(selectedMp);
         }
         return true;
      }
      private bool DeleteEllipsesAndPolygons()
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "DeleteEllipsesAndPolygons(): myCanvasTank=null");
            return false;
         }
         //-------------------------------------------
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvasTank.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
            if (ui is Polygon)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myCanvasTank.Children.Remove(ui1);
         //-------------------------------------------
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "DeleteEllipsesAndPolygons(): myCanvasMain=null");
            return false;
         }
         //-------------------------------------------
         List<UIElement> results1 = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Ellipse)
               results1.Add(ui);
            if (ui is Polygon)
               results1.Add(ui);
         }
         foreach (UIElement ui1 in results1)
            myCanvasMain.Children.Remove(ui1);
         //-------------------------------------------
         myEllipses.Clear();
         return true;
      }
      private bool CreateEllipses(bool isFirst)
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
            if (true == myIsBattleMapShown)
            {
               if (("A" == t.Type) || ("B" == t.Type) || ("C" == t.Type) || ("D" == t.Type) || ("E" == t.Type))
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
            Ellipse aEllipse = new Ellipse() { Name = t.Name };
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
            if( true == isFirst)
            {
               aEllipse.Fill = aSolidColorBrush0;
               aEllipse.MouseDown += this.MouseDownEllipse;
            }
            else
            {
               aEllipse.Fill = Brushes.Pink;
               aEllipse.MouseDown += this.MouseDownEllipse2;
            }
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
               //myCanvasMain.Children.Add(aLabel);
            }
            else
            {
               myCanvasTank.Children.Add(aEllipse);
               //myCanvasTank.Children.Add(aLabel);
            }
         }
         return true;
      }
      private bool CreatePolygons()
      {
         myPoints.Clear();
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
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvasMain=null");
            return false;
         }
         foreach (Territory t in Territories.theTerritories)
         {
            if (true == myIsBattleMapShown)
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
            if (1 < t.Points.Count)
            {
               PointCollection points = new PointCollection();
               foreach (IMapPoint mp1 in t.Points)
                  points.Add(new System.Windows.Point(mp1.X, mp1.Y));
               Polygon aPolygon = new Polygon { Fill= Utilities.theBrushRegion, Points = points, Name = t.Name, Visibility= Visibility.Visible };
               aPolygon.MouseDown += this.MouseDownPolygon;
               Canvas.SetZIndex(aPolygon, 0);
               if ("Main" == t.CanvasName)
                  myCanvasMain.Children.Add(aPolygon);
               else
                  myCanvasTank.Children.Add(aPolygon);
            }
         }
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
      private bool SetRandomPoint(ITerritory t)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetRandomPoint(): myCanvasMain=null for t.Name=" + t.Name);
            return false;
         }
         if (0 == t.Points.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetRandomPoint(): t.Points.Count=0 for t.Name=" + t.Name);
            return false;
         }
         //----------------------------------------------------
         // Make a StreamGeometry object from t.Points 
         StreamGeometry geometry = new StreamGeometry();
         using (StreamGeometryContext ctx = geometry.Open())
         {
            IMapPoint mp0 = t.Points[0];
            System.Windows.Point point0 = new System.Windows.Point(mp0.X, mp0.Y);
            ctx.BeginFigure(point0, true, true); //  filled and closed
            for (int i = 1; i < t.Points.Count; ++i)
            {
               IMapPoint mpI = t.Points[i];
               System.Windows.Point pointI = new System.Windows.Point(mpI.X, mpI.Y);
               ctx.LineTo(pointI, true, false);
            }
            geometry.Freeze();
         }
         System.Windows.Rect rect = geometry.Bounds;
         System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
         path.Fill = Utilities.theBrushRegion;
         path.Stroke = Brushes.Black;
         path.StrokeThickness = 1;
         path.Data = geometry;
         myCanvasMain.Children.Add(path);
         //----------------------------------------------------
         int count = 20;
         while (0 < --count)
         {
            double XCenter = (double)Utilities.RandomGenerator.Next((int)rect.Left, (int)rect.Right) + Utilities.theMapItemOffset; // Get a random point in the bounding box
            double YCenter = (double)Utilities.RandomGenerator.Next((int)rect.Top, (int)rect.Bottom) + Utilities.theMapItemOffset;
            Ellipse ellipse = new Ellipse() { Fill = Brushes.Black, Stroke = Brushes.Black, Width = 10, Height = 10, StrokeThickness = 1 };
            Canvas.SetLeft(ellipse, XCenter - 5);
            Canvas.SetTop(ellipse, YCenter - 5);
            myCanvasMain.Children.Add(ellipse);
            System.Windows.Point pCenter = new System.Windows.Point(XCenter, YCenter);
            if ( true == geometry.FillContains(pCenter) )
            {
               System.Windows.Point p1 = new System.Windows.Point(XCenter - Utilities.theMapItemOffset, YCenter - Utilities.theMapItemOffset);
               System.Windows.Point p2 = new System.Windows.Point(XCenter + Utilities.theMapItemOffset, YCenter - Utilities.theMapItemOffset);
               System.Windows.Point p3 = new System.Windows.Point(XCenter - Utilities.theMapItemOffset, YCenter + Utilities.theMapItemOffset);
               System.Windows.Point p4 = new System.Windows.Point(XCenter + Utilities.theMapItemOffset, YCenter + Utilities.theMapItemOffset);
               bool isP1In = geometry.FillContains(p1);
               bool isP2In = geometry.FillContains(p2);
               bool isP3In = geometry.FillContains(p3);
               bool isP4In = geometry.FillContains(p4);
               Rectangle rect0 = new Rectangle() { Fill=Utilities.theBrushRegion, Stroke = Brushes.Black, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize, StrokeThickness = 1 };
               Canvas.SetLeft(rect0, XCenter - Utilities.theMapItemOffset);
               Canvas.SetTop(rect0, YCenter - Utilities.theMapItemOffset);
               if (false == isP1In && false == isP2In)
               {
                  YCenter += Utilities.theMapItemOffset;
               }
               else if (false == isP3In && false == isP4In)
               {
                  YCenter -= Utilities.theMapItemOffset;
               }
               else if (false == isP1In && false == isP3In)
               {
                  XCenter += Utilities.theMapItemOffset;
               }
               else if (false == isP2In && false == isP4In)
               {
                  XCenter -= Utilities.theMapItemOffset;
               }
               else if (false == isP1In && true == isP2In)
               {
                  XCenter += Utilities.theMapItemOffset;
               }
               else if (true == isP1In && false == isP2In)
               {
                  XCenter -= Utilities.theMapItemOffset;
               }
               else if (true == isP3In && false == isP4In)
               {
                  YCenter -= Utilities.theMapItemOffset;
               }
               else if (false == isP3In && true == isP4In)
               {
                  YCenter -= Utilities.theMapItemOffset;
               }
               System.Windows.Point p5 = new System.Windows.Point(XCenter - Utilities.theMapItemOffset, YCenter - Utilities.theMapItemOffset);
               if (true == geometry.FillContains(p5))
               {
                  Rectangle rectFinal = new Rectangle() { Fill = Brushes.Red, Stroke = Brushes.Red, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize, StrokeThickness = 1 };
                  Canvas.SetLeft(rectFinal, XCenter - Utilities.theMapItemOffset);
                  Canvas.SetTop(rectFinal, YCenter - Utilities.theMapItemOffset);
                  myCanvasMain.Children.Add(rectFinal);
                  myCanvasMain.Children.Add(rect0);
                  return true;
               }
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "SetRandomPoint(): Cannot find a random point in t.Name=" + t.Name + " rect=" + rect.ToString());
         return false;
      }
      //--------------------------------------------------------
      void MouseDownEllipse(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse(): myCanvasTank=null");
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse(): myCanvasMain=null");
            return;
         }
         string tType = myTankNum.ToString();
         //------------------------------------------------
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         bool isMainCanvas = true;
         if (p.X < 0.0)
         {
            p = e.GetPosition(myCanvasTank);
            isMainCanvas = false;
         }
         IMapPoint mp = new MapPoint(p.X, p.Y);
         System.Diagnostics.Debug.WriteLine("MouseDownEllipse(): {0}", mp.ToString());
         Ellipse mousedEllipse = (Ellipse)sender;
         ITerritory? matchingTerritory = Territories.theTerritories.Find(mousedEllipse.Name, tType);
         if (null == matchingTerritory) // Check for error
         {
            MessageBox.Show("Unable to find " + mousedEllipse.Name);
            return;
         }
         if (null == myAnchorTerritory)
         {
            MessageBox.Show("Anchoring " + mousedEllipse.Name);
            myAnchorTerritory = matchingTerritory; // If there is no anchor territory. Set it.
            mousedEllipse.Fill = Brushes.Red;
            if (true == isMainCanvas)
               myCanvasMain.MouseDown += MouseDownCanvas;
            else
               myCanvasTank.MouseDown += MouseDownCanvas;
            return;
         }
         if (matchingTerritory.Name == myAnchorTerritory.Name)
         {
            // If the matching territory is the anchor territory, the user
            // is requesting that they are done adding points for
            // defining the Region.  The Region is used set as part of the Territory. 
            MessageBox.Show("Saving " + mousedEllipse.Name);
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in myPoints)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Brushes.Red, Points = points, Name = matchingTerritory.Name };
            aPolygon.MouseDown += this.MouseDownPolygon;
            aPolygon.Fill = Brushes.Black;
            mousedEllipse.Fill = Brushes.Black;
            myAnchorTerritory.Points = new List<IMapPoint>(myPoints);
            myPoints.Clear();
            myAnchorTerritory = null;
            if (true == isMainCanvas)
            {
               myCanvasMain.MouseDown -= MouseDownCanvas;
               myCanvasMain.Children.Add(aPolygon);
            }
            else
            {
               myCanvasTank.MouseDown -= MouseDownCanvas;
               myCanvasTank.Children.Add(aPolygon);
            }
         }
         e.Handled = true;
      }
      void MouseDownEllipse2(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse2(): myCanvasMain=null");
            return;
         }
         string tType = myTankNum.ToString();
         //------------------------------------------------
         Ellipse mousedEllipse = (Ellipse)sender;
         ITerritory? matchingTerritory = Territories.theTerritories.Find(mousedEllipse.Name, tType);
         if (null == matchingTerritory) // Check for error
         {
            MessageBox.Show("Unable to find " + mousedEllipse.Name);
            return;
         }
         if( false == SetRandomPoint(matchingTerritory))
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse2(): SetRandomPoint() returned false");
            return;
         }
         e.Handled = true;
      }
      void MouseDownCanvas(object sender, MouseButtonEventArgs e)
      {
         // This function adds points to the myPoints collection when an anchor territory is active.
         // The points to add are either new ones or ones that exist from adjacent territories.
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         if (p.X < 0.0)
            p = e.GetPosition(myCanvasTank);
         IMapPoint mp = new MapPoint(p.X, p.Y);
         if (false == CreatePoint(mp))
            Logger.Log(LogEnum.LE_ERROR, "MouseDownCanvas->CreatePoint()");
         e.Handled = true;
      }
      void MouseDownPolygon(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipse(): myCanvasTank=null");
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygon(): myCanvasMain=null");
            return;
         }
         string tType = myTankNum.ToString();
         //------------------------------------------------
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         bool isMainCanvas = true;
         if (p.X < 0.0)
         {
            p = e.GetPosition(myCanvasTank);
            isMainCanvas = false;
         }
         IMapPoint mp = new MapPoint(p.X, p.Y);
         System.Diagnostics.Debug.WriteLine("TerritoryRegionUnitTest.MouseDownPolygon(): {0}", mp.ToString());
         if (null == myAnchorTerritory)
         {
            // This function removes an existing polygon when it is clicked if no achor territory exists
            Polygon aPolygon = (Polygon)sender;
            ITerritory? matchingTerritory = Territories.theTerritories.Find(aPolygon.Name, tType);
            if (null == matchingTerritory) // Check for error
            {
               MessageBox.Show("Unable to find " + aPolygon.Name);
            }
            else if ((null == myAnchorTerritory) || matchingTerritory.Name == myAnchorTerritory.Name)
            {
               matchingTerritory.Points.Clear();
               if (true == isMainCanvas)
                  myCanvasMain.Children.Remove(aPolygon);
               else
                  myCanvasTank.Children.Remove(aPolygon);
            }
         }
         else
         {
            if (false == CreatePoint(mp))
               Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygon->CreatePoint()");
         }
         e.Handled = true;
      }
      double getRange(IMapPoint p1, IMapPoint p2)
      {
         double d1 = Math.Abs(p1.X - p2.X);
         double d2 = Math.Abs(p1.Y - p2.Y);
         double distance = Math.Sqrt(Math.Pow(d1, 2.0) + Math.Pow(d2, 2.0));
         return distance;
      }
   }
}


