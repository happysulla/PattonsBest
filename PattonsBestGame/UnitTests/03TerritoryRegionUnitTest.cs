using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Point = System.Windows.Point;

namespace Pattons_Best
{
   public class TerritoryRegionUnitTest : IUnitTest
   {
      private static Double theEllipseOffset = 8;
      //--------------------------------------------------------
      private IGameInstance? myGameInstance = null;
      private string? myFileName = null;
      private DockPanel? myDockPanel = null;
      private Canvas? myCanvasTank = null;
      private Canvas? myCanvasMain = null;
      private CanvasImageViewer? myCanvasImageViewer = null;
      private bool myIsBattleMapShown = false;
      ITerritory? myAnchorTerritory = null;
      private List<Ellipse> myEllipses = new List<Ellipse>();
      private List<IMapPoint> myPoints = new List<IMapPoint>();
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
         myHeaderNames.Add("03-Add Regions");
         myHeaderNames.Add("03-Finish");
         //------------------------------------
         myCommandNames.Add("00-Regions");
         myCommandNames.Add("01-Switch Map");
         myCommandNames.Add("02-Cleanup");
         //------------------------------------
         myDockPanel = dp;
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
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): myCanvasTank=null");
            CtorError = true;
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): myCanvasMain=null");
            CtorError = true;
            return;
         }
         //----------------------------------
         if (false == SetFileName())
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
         if (CommandName == myCommandNames[0])
         {
            //--------------------------------------------
            // Remove all Ellipse and Polygons
            List<UIElement> results = new List<UIElement>();
            foreach (UIElement ui in myCanvasMain.Children)
            {
               if (ui is Ellipse)
                  results.Add(ui);
               if (ui is Polygon)
                  results.Add(ui);
            }
            foreach (UIElement ui1 in results)
               myCanvasMain.Children.Remove(ui1);
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
            CreateEllipses(Territories.theTerritories);
            CreatePolygons(Territories.theTerritories);
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
         if (HeaderName == myHeaderNames[0])
         {
            ++myIndexName;
            CreateEllipses(Territories.theTerritories);
            CreatePolygons(Territories.theTerritories);
         }
         else if (HeaderName == myHeaderNames[1])
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
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children) // Remove any existing UI elements from the Canvas
         {
            if (ui is Ellipse)
               results.Add(ui);
            if (ui is Polygon p)
               p.Fill = Utilities.theBrushRegionClear;
         }
         foreach (UIElement ui1 in results)
            myCanvasMain.Children.Remove(ui1);
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
            // Do an intersection with any other points that
            // are part of any other region.  If a point is found
            // that is very close, assume that is the correct
            // point to add instead of the mouse click.
            double minDistance = 20;
            IMapPoint selectedMp = mp;
            foreach (String s in myAnchorTerritory.Adjacents)
            {
               Territory? adjacentTerritory = null;
               foreach (Territory t in Territories.theTerritories)
               {
                  if (s == Utilities.RemoveSpaces(t.ToString()))
                  {
                     adjacentTerritory = t;
                     break;
                  }
               }
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
      private void CreateEllipses(ITerritories territories)
      {
         myEllipses.Clear();
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvasTank=null");
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvasMain=null");
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvasMain=null");
            return;
         }

         foreach (Territory t in territories)
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
            Ellipse aEllipse = new Ellipse { Tag = t.ToString() };
            aEllipse.Fill = Brushes.Pink;
            aEllipse.StrokeThickness = 1;
            aEllipse.Stroke = Brushes.Red;
            aEllipse.Width = 15;
            aEllipse.Height = 15;
            System.Windows.Point p = new System.Windows.Point(t.CenterPoint.X, t.CenterPoint.Y);
            p.X -= theEllipseOffset;
            p.Y -= theEllipseOffset;
            Canvas.SetLeft(aEllipse, p.X);
            Canvas.SetTop(aEllipse, p.Y);
            if ("Main" == t.CanvasName)
               myCanvasMain.Children.Add(aEllipse);
            else
               myCanvasTank.Children.Add(aEllipse);
            myEllipses.Add(aEllipse);
            aEllipse.MouseDown += this.MouseDownEllipse;
         }
      }
      private void CreatePolygons(ITerritories territories)
      {
         myPoints.Clear();
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvasTank=null");
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvasMain=null");
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvasMain=null");
            return;
         }
         foreach (Territory t in territories)
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
            if (1 < t.Points.Count)
            {
               PointCollection points = new PointCollection();
               foreach (IMapPoint mp1 in t.Points)
                  points.Add(new System.Windows.Point(mp1.X, mp1.Y));
               Polygon aPolygon = new Polygon { Fill= Utilities.theBrushRegion, Points = points, Tag = t.ToString(), Visibility= Visibility.Visible };
               aPolygon.MouseDown += this.MouseDownPolygon;
               Canvas.SetZIndex(aPolygon, 0);
               if ("Main" == t.CanvasName)
                  myCanvasMain.Children.Add(aPolygon);
               else
                  myCanvasTank.Children.Add(aPolygon);
            }
         }
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
      //--------------------------------------------------------
      void MouseDownEllipse(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownSetCenterPoint(): myCanvasTank=null");
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownSetCenterPoint(): myCanvasMain=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         bool isMainCanvas = true;
         if (p.X < 0.0)
         {
            p = e.GetPosition(myCanvasTank);
            isMainCanvas = false;
         }
         IMapPoint mp = new MapPoint(p.X, p.Y);
         System.Diagnostics.Debug.WriteLine("MouseDownEllipse.MouseDown(): {0}", mp.ToString());
         ITerritory? matchingTerritory = null; // Find the corresponding Territory
         Ellipse mousedEllipse = (Ellipse)sender;
         foreach (ITerritory? t in Territories.theTerritories)
         {
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse.MouseDown(): t=null");
               return;
            }
            string? tName = t.ToString();
            if (null == tName)
            {
               Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse.MouseDown(): tName=null");
               return;
            }
            if (mousedEllipse.Tag.ToString() == Utilities.RemoveSpaces(tName))
            {
               matchingTerritory = t;
               break;
            }
         }
         if (null == matchingTerritory) // Check for error
         {
            MessageBox.Show("Unable to find " + mousedEllipse.Tag.ToString());
            return;
         }
         if (null == myAnchorTerritory)
         {
            MessageBox.Show("Anchoring " + mousedEllipse.Tag.ToString());
            myAnchorTerritory = matchingTerritory; // If there is no anchor territory. Set it.
            mousedEllipse.Fill = Brushes.Red;
            if (true == isMainCanvas)
               myCanvasMain.MouseDown += MouseDownCanvas;
            else
               myCanvasTank.MouseDown += MouseDownCanvas;
            return;
         }
         if (matchingTerritory.ToString() == myAnchorTerritory.ToString())
         {
            // If the matching territory is the anchor territory, the user
            // is requesting that they are done adding points for
            // defining the Region.  The Region is used set as part of the Territory. 
            MessageBox.Show("Saving " + mousedEllipse.Tag.ToString());
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in myPoints)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polygon aPolygon = new Polygon { Fill = Brushes.Red, Points = points, Tag = matchingTerritory.ToString() };
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
            ITerritory? matchingTerritory = null;
            foreach (ITerritory t in Territories.theTerritories)
            {
               string? tName = t.ToString();
               if( null == tName )
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseDownPolygon(): tName=null");
                  continue;
               }
               if (aPolygon.Tag.ToString() == Utilities.RemoveSpaces(tName))
               {
                  matchingTerritory = t;
                  break;
               }
            }
            if (null == matchingTerritory) // Check for error
            {
               MessageBox.Show("Unable to find " + aPolygon.Tag.ToString());
            }
            else if ((null == myAnchorTerritory) || matchingTerritory.ToString() == myAnchorTerritory.ToString())
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


