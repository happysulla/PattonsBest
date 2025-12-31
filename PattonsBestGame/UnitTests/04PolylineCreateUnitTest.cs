using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
namespace Pattons_Best
{
   public class PolylineCreateUnitTest : IUnitTest
   {
      public static Double theEllipseOffset = 8;
      //--------------------------------------------------------
      private IGameInstance myGameInstance;
      private DockPanel? myDockPanel = null;
      private Canvas? myCanvasTank = null;
      private Canvas? myCanvasMain = null;
      private CanvasImageViewer? myCanvasImageViewer = null;
      private bool myIsBattleMapShown = false;
      private int myIndexRoad = 0;
      private bool myIsPavedRoad = false;
      private List<Ellipse> myEllipses = new List<Ellipse>();
      private List<IMapPoint> myPoints = new List<IMapPoint>();
      private Dictionary<string, Polyline> myPolyLines = new Dictionary<string, Polyline>();
      //--------------------------------------------------------
      private SolidColorBrush mySolidColorBrushPaved = new SolidColorBrush { Color = Colors.Blue };
      private SolidColorBrush mySolidColorBrushUnpaved = new SolidColorBrush { Color = Colors.LawnGreen };
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      //--------------------------------------------------------
      public bool CtorError { get; } = false;
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      //--------------------------------------------------------
      public PolylineCreateUnitTest(DockPanel dp, IGameInstance gi, CanvasImageViewer civ)
      {
         myGameInstance = gi;
         myIndexName = 0;
         myHeaderNames.Add("04-Switch Map");
         myHeaderNames.Add("04-Make Road");
         myHeaderNames.Add("04-Finish");
         //------------------------------------
         myCommandNames.Add("00-Switch Map");
         myCommandNames.Add("01-Make Road");
         myCommandNames.Add("Cleanup");
         //------------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "PolylineCreateUnitTest(): gi=null");
            CtorError = true;
            return;
         }
         //------------------------------------
         if (null == civ)
         {
            Logger.Log(LogEnum.LE_ERROR, "PolylineCreateUnitTest(): civ=null");
            CtorError = true;
            return;
         }
         myCanvasImageViewer = civ;
         //------------------------------------
         myDockPanel = dp;
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
            Logger.Log(LogEnum.LE_ERROR, "PolylineCreateUnitTest(): myCanvasTank=null");
            CtorError = true;
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "PolylineCreateUnitTest(): myCanvasMain=null");
            CtorError = true;
            return;
         }
         //-------------------------------------
         if (true == myIsBattleMapShown)
         {
            myIsBattleMapShown = false;
            myCanvasImageViewer.ShowMovementMap(myCanvasMain);
         }
         else
         {
            myIsBattleMapShown = true;
            IAfterActionReport? lastReport = gi.Reports.GetLast();
            if (null == lastReport)
            {
               ICombatCalendarEntry? entry = TableMgr.theCombatCalendarEntries[0];
               if (null == entry)
               {
                  Logger.Log(LogEnum.LE_ERROR, "PolylineCreateUnitTest(): entry=null");
                  CtorError = true;
                  return;
               }
               lastReport = new AfterActionReport(entry); 
               gi.Reports.Add(lastReport); 
            }
            myCanvasImageViewer.ShowBattleMap(lastReport, myCanvasMain);
         }
         //-------------------------------------
         myDashArray.Add(5);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines
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
         if (null == myCanvasImageViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasImageViewer=null");
            return false;
         }
         //-----------------------------------
         if (CommandName == myCommandNames[0])
         {
            if (true == myIsBattleMapShown)
            {
               myIsBattleMapShown = false;
               myCanvasImageViewer.ShowMovementMap(myCanvasMain);
            }
            else
            {
               myIsBattleMapShown = true;
               IAfterActionReport? lastReport = gi.Reports.GetLast();
               if (null == lastReport)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Command(): lastReport=null");
                  return false;
               }
               myCanvasImageViewer.ShowBattleMap(lastReport, myCanvasMain);
            }
         }
         else if (CommandName == myCommandNames[1])
         {
         }
         else
         {
            myCanvasMain.MouseDown -= MouseDownCanvas;
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
            CreateEllipses();
            if (false == ReadRoadsXml())
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): ReadRoadsXml() returned false");
               return false;
            }
            myCanvasMain.MouseDown += MouseDownCanvas;
         }
         if (HeaderName == myHeaderNames[1])
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
      public bool Cleanup(ref IGameInstance gi) 
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvasMain=null");
            return false;
         }
         try
         {
            string filename = "..\\..\\..\\Config\\Roads.xml";
            System.IO.File.Delete(filename);  // delete old file
            XmlDocument aXmlDocument = CreateRoadsXml(); // create a new XML document based on Territories
            using (FileStream writer = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
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
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): e=\n" + e.ToString());
            return false;
         }
         // Remove any existing UI elements from the Canvas
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
            if (ui is Polyline)
               results.Add(ui);
            if (ui is Polygon p)
               p.Fill = Utilities.theBrushRegionClear;
         }
         foreach (UIElement ui1 in results)
            myCanvasMain.Children.Remove(ui1);
         myCanvasMain.MouseDown -= MouseDownCanvas;
         ++gi.GameTurn;
         return true;
      }
      //--------------------------------------------------------
      private void CreateEllipses()
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvasMain=null");
            return;
         }
         Ellipse aEllipseStart = new Ellipse();  // create ellipse only as UI to start making road - located on top right
         aEllipseStart.Name = "Start";
         aEllipseStart.Fill = mySolidColorBrushUnpaved;
         aEllipseStart.StrokeThickness = 1;
         aEllipseStart.Stroke = System.Windows.Media.Brushes.Red;
         aEllipseStart.Width = 50;
         aEllipseStart.Height = 50;
         System.Windows.Point pStart = new System.Windows.Point(350, -10);
         pStart.X -= theEllipseOffset;
         pStart.Y -= theEllipseOffset;
         Canvas.SetLeft(aEllipseStart, pStart.X);
         Canvas.SetTop(aEllipseStart, pStart.Y);
         myCanvasMain.Children.Add(aEllipseStart);
         myEllipses.Add(aEllipseStart);
         aEllipseStart.MouseDown += this.MouseDownEllipse;
         //-----------------------------------------------------------------------
         Ellipse aEllipseStartPaved = new Ellipse();  // create ellipse only as UI to start making road - located on top right
         aEllipseStartPaved.Name = "StartPaved";
         aEllipseStartPaved.Fill = mySolidColorBrushPaved;
         aEllipseStartPaved.StrokeThickness = 1;
         aEllipseStartPaved.Stroke = System.Windows.Media.Brushes.Red;
         aEllipseStartPaved.Width = 50;
         aEllipseStartPaved.Height = 50;
         System.Windows.Point pStartPaved = new System.Windows.Point(380, -10);
         pStartPaved.X -= theEllipseOffset;
         pStartPaved.Y -= theEllipseOffset;
         Canvas.SetLeft(aEllipseStartPaved, pStartPaved.X);
         Canvas.SetTop(aEllipseStartPaved, pStartPaved.Y);
         myCanvasMain.Children.Add(aEllipseStartPaved);
         myEllipses.Add(aEllipseStartPaved);
         aEllipseStartPaved.MouseDown += this.MouseDownEllipse;
         //-----------------------------------------------------------------------
         Ellipse aEllipseEnd = new Ellipse ();
         aEllipseEnd.Name = "End";
         aEllipseEnd.Fill = System.Windows.Media.Brushes.Orchid;
         aEllipseEnd.StrokeThickness = 1;
         aEllipseEnd.Stroke = System.Windows.Media.Brushes.Red;
         aEllipseEnd.Width = 50;
         aEllipseEnd.Height = 50;
         System.Windows.Point pEnd = new System.Windows.Point(410, -10);
         pEnd.X -= theEllipseOffset;
         pEnd.Y -= theEllipseOffset;
         Canvas.SetLeft(aEllipseEnd, pEnd.X);
         Canvas.SetTop(aEllipseEnd, pEnd.Y);
         myCanvasMain.Children.Add(aEllipseEnd);
         myEllipses.Add(aEllipseEnd);
         aEllipseEnd.MouseDown += this.MouseDownEllipse;
      }
      private XmlDocument CreateRoadsXml()
      {
         //---------------------------------------------
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         //---------------------------------------------
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<Roads></Roads>");
         if( null == aXmlDocument.DocumentElement )
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateRoadsXml(): aXmlDocument.DocumentElement=null");
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            return aXmlDocument;
         }
         foreach (KeyValuePair<string, Polyline> kvp in myPolyLines)
         {
            XmlElement nameElem = aXmlDocument.CreateElement("Road");  // name of road
            nameElem.SetAttribute("value", kvp.Key);
            if( true == myIsPavedRoad )
               nameElem.SetAttribute("color", "Paved");
            else
               nameElem.SetAttribute("color", "Unpaved");
            aXmlDocument.DocumentElement.AppendChild(nameElem);
            foreach(System.Windows.Point p in kvp.Value.Points)
            {
               XmlElement pointElem = aXmlDocument.CreateElement("point");
               pointElem.SetAttribute("X", p.X.ToString("F3"));
               pointElem.SetAttribute("Y", p.Y.ToString("F3"));
               XmlNode? lastChild = aXmlDocument.DocumentElement.LastChild;
               if (null == lastChild)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateRoadsXml(): lastChild=null");
                  System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
                  return aXmlDocument;
               }
               else
               {
                  lastChild.AppendChild(pointElem);
               }
            }
         }
         System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         return aXmlDocument;
      }
      private bool ReadRoadsXml()
      {
         if( null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadRoadsXml(): myCanvasMain = null");
            return false;
         }
         //---------------------------------------------
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         //---------------------------------------------
         XmlTextReader? reader = null;
         PointCollection? points = null;
         string? name = null;
         string? color = null;
         try
         {
            string filename = "..\\..\\..\\Config\\Roads.xml";
            reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None }; // Load the reader with the data file and ignore all white space nodes.    
            while (reader.Read())
            {
               if (reader.Name == "Road")
               {
                  points = new PointCollection();
                  if (reader.IsStartElement())
                  {
                     name = reader.GetAttribute("value");
                     if( null == name )
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadRoadsXml(): GetAttribute(value) returned null");
                        return false;
                     }
                     color = reader.GetAttribute("color");
                     if (null == color)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadRoadsXml(): GetAttribute(color) returned null");
                        return false;
                     }
                     while (reader.Read())
                     {
                        if ((reader.Name == "point" && (reader.IsStartElement())))
                        {
                           string? value = reader.GetAttribute("X");
                           if( null == value )
                           {
                              Logger.Log(LogEnum.LE_ERROR, "ReadRoadsXml(): GetAttribute(X) returned null");
                              return false;
                           }
                           Double X1 = Double.Parse(value);
                           value = reader.GetAttribute("Y");
                           if (null == value)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "ReadRoadsXml(): GetAttribute(Y) returned null");
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
                  } // end if
                  if (null == name)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadRoadsXml(): GetAttribute(value) returned null");
                     return false;
                  }
                  System.Windows.Media.Brush? brush = null;
                  double roadThickness = 8.0;
                  if ("Paved" == color)
                  {
                     brush = mySolidColorBrushPaved;
                  }
                  else
                  {
                     brush = mySolidColorBrushUnpaved;
                     roadThickness = 5.0;
                  }
                  Polyline polyline = new Polyline { Points = points, Stroke = brush, StrokeThickness = roadThickness, StrokeDashArray = myDashArray, Visibility = Visibility.Visible };
                  myPolyLines[name] = polyline;
                  Canvas.SetZIndex(polyline, 0);
                  myCanvasMain.Children.Add(polyline);
                  myIndexRoad++;
               } // end if
            } // end while
         } // try
         catch (Exception e)
         {
            System.Diagnostics.Debug.WriteLine("ReadTerritoriesXml(): Exception:  e.Message={0}", e.Message);
         }
         finally
         {
            if (reader != null)
               reader.Close();
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         }
         System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         return true;
      }
      //----------------------------------------------------------
      void MouseDownEllipse(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse(): myCanvasMain=null");
            return;
         }
         System.Windows.Point canvasPoint = e.GetPosition(myCanvasMain);
         IMapPoint mp = new MapPoint(canvasPoint.X, canvasPoint.Y);
         Ellipse mousedEllipse = (Ellipse)sender;
         //----------------------------------------
         if ("Start" == mousedEllipse.Name)
         {
            myIndexRoad++;
            myIsPavedRoad = false;
            myPoints.Clear();
         }
         else if ("StartPaved" == mousedEllipse.Name)
         {
            myIndexRoad++;
            myIsPavedRoad = true;
            myPoints.Clear();
         }
         else if ("End" == mousedEllipse.Name)
         {
            string name = myIndexRoad.ToString();
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in myPoints)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            System.Windows.Media.Brush? brush = null;
            double roadThickness = 8.0;
            if (true == myIsPavedRoad)
            {
               brush = mySolidColorBrushPaved;
            }
            else
            {
               brush = mySolidColorBrushUnpaved;
               roadThickness = 5.0;
            }
            Polyline polyline = new Polyline { Points = points, Stroke = brush, StrokeThickness = roadThickness, StrokeDashArray = myDashArray, Visibility = Visibility.Visible };
            myPolyLines[name] = polyline;
            Canvas.SetZIndex(polyline, 0);
            myCanvasMain.Children.Add(polyline);
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse(): reached default with invalid ellipse name=" + mousedEllipse.Name);
            return;
         }
         e.Handled = true;
      }
      void MouseDownCanvas(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point canvasPoint = e.GetPosition(myCanvasMain);
         IMapPoint mp = new MapPoint(canvasPoint.X, canvasPoint.Y);
         myPoints.Add(mp);
         e.Handled = true;
      }
   }
}