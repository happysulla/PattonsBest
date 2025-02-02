﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
namespace Pattons_Best
{
   public class PolylineCreateUnitTest : IUnitTest
   {
      public static Double theEllipseOffset = 8;
      //--------------------------------------------------------
      private DockPanel? myDockPanel = null;
      private Canvas? myCanvas = null;
      private IGameInstance? myGameInstance = null;
      private List<Ellipse> myEllipses = new List<Ellipse>();
      private List<IMapPoint> myPoints = new List<IMapPoint>();
      private Dictionary<string, Polyline> myPolyLines = new Dictionary<string, Polyline>();
      //--------------------------------------------------------
      private SolidColorBrush mySolidColorBrush3 = new SolidColorBrush { Color = Colors.DeepSkyBlue };
      //--------------------------------------------------------
      public bool CtorError { get; } = false;
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      //--------------------------------------------------------
      public PolylineCreateUnitTest(DockPanel dp, IGameInstance gi)
      {
         myIndexName = 0;
         myHeaderNames.Add("04-Start Polyline Test");
         myHeaderNames.Add("04-Finish");
         //------------------------------------
         myCommandNames.Add("00-Start");
         myCommandNames.Add("Cleanup");
         //------------------------------------
         myDockPanel = dp;
         foreach (UIElement ui0 in myDockPanel.Children)
         {
            if (ui0 is DockPanel dockPanelInside)
            {
               foreach (UIElement ui1 in dockPanelInside.Children)
               {
                  if (ui1 is ScrollViewer sv)
                  {
                     if (sv.Content is Canvas canvas)
                        myCanvas = canvas;  // Find the Canvas in the visual tree
                  }
               }
            }
         }
         if (null == myCanvas) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest() myCanvas=null");
            CtorError = true;
            return;
         }
         //------------------------------------
         this.myGameInstance = gi;
      }
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvas=null");
            return false;
         }
         if (CommandName == myCommandNames[0])
         {
            myCanvas.MouseDown += MouseDownCanvas;
            CreateEllipses();
            CreateTriangles();
            if( false == NextTest(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command() NextTest() returned false");
               return false;
            }
         }
         else
         {
            myCanvas.MouseDown -= MouseDownCanvas;
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
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvas=null");
            return false;
         }
         // Remove any existing UI elements from the Canvas
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
            if (ui is Polyline)
               results.Add(ui);
            if (ui is Polygon p)
               p.Fill = Utilities.theBrushRegionClear;
         }
         foreach (UIElement ui1 in results)
            myCanvas.Children.Remove(ui1);
         myCanvas.MouseDown -= MouseDownCanvas;
         ++gi.GameTurn;
         return true;
      }
      //--------------------------------------------------------
      private bool CreatePoint(IMapPoint mp)
      {
         double minDistance = 10;
         IMapPoint selectedMp = mp;
         foreach (Territory t in Territories.theTerritories)
         {
            foreach (IMapPoint mp1 in t.Points)
            {
               double d1 = Math.Abs(mp.X - mp1.X);
               double d2 = Math.Abs(mp.Y - mp1.Y);
               double distance = Math.Sqrt(Math.Pow(d1, 2.0) + Math.Pow(d2, 2.0));
               if (distance < minDistance)
               {
                  minDistance = distance;
                  selectedMp.X = mp1.X;
                  selectedMp.Y = mp1.Y;
                  Console.WriteLine("\t\t==> {0} from {1} with d={2}", selectedMp.ToString(), t.Name, distance);
                  myPoints.Add(selectedMp);
                  return true;
               }
            }
         }  // end foreach()
         Console.WriteLine("\t\t++>{0}", selectedMp.ToString()); // An territory point was not found.  Add the mouse click point as a new point.
         myPoints.Add(selectedMp);
         return true;
      }
      private void CreateTriangles()
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateTriangles(): myCanvas=null");
            return;
         }
         const double SIZE = 6.0;
         foreach (KeyValuePair<string, Polyline> kvp in myPolyLines)
         {
            int i = 0;
            double X1 = 0.0;
            double Y1 = 0.0;
            foreach (System.Windows.Point p in kvp.Value.Points)
            {
               if (0 == i)
               {
                  X1 = p.X;
                  Y1 = p.Y;
               }
               else
               {
                  double Xcenter = X1 + (p.X - X1) / 2.0;
                  double Ycenter = Y1 + (p.Y - Y1) / 2.0;
                  PointCollection points = new PointCollection();
                  System.Windows.Point one = new System.Windows.Point(Xcenter - SIZE, Ycenter - SIZE);
                  System.Windows.Point two = new System.Windows.Point(Xcenter + SIZE, Ycenter);
                  System.Windows.Point three = new System.Windows.Point(Xcenter - SIZE, Ycenter + SIZE);
                  points.Add(one);
                  points.Add(two);
                  points.Add(three);
                  Polygon triangle = new Polygon() { Name = "River", Points = points, Stroke = mySolidColorBrush3, Fill = mySolidColorBrush3, Visibility = Visibility.Visible };
                  double rotateAngle = GetRotateAngle(X1, Y1, p.X, p.Y);
                  triangle.RenderTransform = new RotateTransform(rotateAngle, Xcenter, Ycenter);
                  myCanvas.Children.Add(triangle);
                  X1 = p.X;
                  Y1 = p.Y;
               }
               ++i;
            }
         }
      }
      private double GetRotateAngle(double X1, double Y1, double X2, double Y2)
      {
         double rotateAngle = 0.0;
         if (Math.Abs(X2 - X1) < 10.0)
            X2 = X1;
         if (Math.Abs(Y2 - Y1) < 10.0)
            Y2 = Y1;
         if (Math.Abs(Y2 - Y1) < 10.0)
         {
            if (X2 < X1)
               rotateAngle = 180.0;
         }
         else if( X1 < X2 )
         {
            if (Y1 < Y2)
               rotateAngle = 60.0;
            else
               rotateAngle = -60.0;
         }
         else 
         {
            if (Y1 < Y2)
               rotateAngle = 120;
            else
               rotateAngle = -120.0;
         }
         return rotateAngle;
      }
      private void CreateEllipses()
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvas=null");
            return;
         }
         Ellipse aEllipseStart = new Ellipse();
         aEllipseStart.Name = "Start";
         aEllipseStart.Fill = Brushes.AliceBlue;
         aEllipseStart.StrokeThickness = 1;
         aEllipseStart.Stroke = Brushes.Red;
         aEllipseStart.Width = 50;
         aEllipseStart.Height = 50;
         System.Windows.Point pStart = new System.Windows.Point(50, 50);
         pStart.X -= theEllipseOffset;
         pStart.Y -= theEllipseOffset;
         Canvas.SetLeft(aEllipseStart, pStart.X);
         Canvas.SetTop(aEllipseStart, pStart.Y);
         myCanvas.Children.Add(aEllipseStart);
         myEllipses.Add(aEllipseStart);
         aEllipseStart.MouseDown += this.MouseDownEllipse;
         //-----------------------------------------------------------------------
         Ellipse aEllipseEnd = new Ellipse ();
         aEllipseEnd.Name = "End";
         aEllipseEnd.Fill = Brushes.Orchid;
         aEllipseEnd.StrokeThickness = 1;
         aEllipseEnd.Stroke = Brushes.Red;
         aEllipseEnd.Width = 50;
         aEllipseEnd.Height = 50;
         System.Windows.Point pEnd = new System.Windows.Point(50, 150);
         pEnd.X -= theEllipseOffset;
         pEnd.Y -= theEllipseOffset;
         Canvas.SetLeft(aEllipseEnd, pEnd.X);
         Canvas.SetTop(aEllipseEnd, pEnd.Y);
         myCanvas.Children.Add(aEllipseEnd);
         myEllipses.Add(aEllipseEnd);
         aEllipseEnd.MouseDown += this.MouseDownEllipse;
      }
      private XmlDocument CreateXml()
      {
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<Rivers></Rivers>");
         if( null == aXmlDocument.DocumentElement )
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): aXmlDocument.DocumentElement=null");
            return aXmlDocument;
         }
         foreach (KeyValuePair<string, Polyline> kvp in myPolyLines)
         {
            XmlElement nameElem = aXmlDocument.CreateElement("River");  // name of river
            nameElem.SetAttribute("value", kvp.Key);
            aXmlDocument.DocumentElement.AppendChild(nameElem);
            foreach(System.Windows.Point p in kvp.Value.Points)
            {
               XmlElement pointElem = aXmlDocument.CreateElement("point");
               pointElem.SetAttribute("X", p.X.ToString());
               pointElem.SetAttribute("Y", p.Y.ToString());
               XmlNode? lastChild = aXmlDocument.DocumentElement.LastChild;
               if (null == lastChild)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): lastChild=null");
                  return aXmlDocument;
               }
               else
               {
                  lastChild.AppendChild(pointElem);
               }

            }
         }
         return aXmlDocument;
      }
      //----------------------------------------------------------
      void MouseDownEllipse(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse(): myCanvas=null");
            return;
         }
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
         IMapPoint mp = new MapPoint(canvasPoint.X, canvasPoint.Y);
         Ellipse mousedEllipse = (Ellipse)sender;
         //----------------------------------------
         string? name = null;
         switch (myIndexName-1)
         {
            case 0: name = "Dienstal Branch"; break;
            case 1: name = "Largos River"; break;
            case 2: name = "Nesser River"; break;
            case 3: name = "Greater Nesser River"; break;
            case 4: name = "Lesser Nesser River"; break;
            case 5: name = "Trogoth River"; break;
            default: Logger.Log(LogEnum.LE_ERROR, "MouseDownEllipse(): reached default with invalid ellipse index=" + myIndexName.ToString()); return;
         }
         //----------------------------------------
         if ("Start" == mousedEllipse.Name)
         {
            if( true == myPolyLines.ContainsKey(name))
            {
               Polyline polyline = myPolyLines[name];
               myCanvas.Children.Remove(polyline);
               myPolyLines.Remove(name);
            }
            myPoints.Clear();
         }
         else if ("End" == mousedEllipse.Name)
         {
            PointCollection points = new PointCollection();
            foreach (IMapPoint mp1 in myPoints)
               points.Add(new System.Windows.Point(mp1.X, mp1.Y));
            Polyline polyline = new Polyline { Points = points, Stroke = mySolidColorBrush3, StrokeThickness = 4, Visibility = Visibility.Visible };
            myPolyLines[name] = polyline;
            Canvas.SetZIndex(polyline, 0);
            myCanvas.Children.Add(polyline);
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
         System.Windows.Point canvasPoint = e.GetPosition(myCanvas);
         IMapPoint mp = new MapPoint(canvasPoint.X, canvasPoint.Y);
         if (false == CreatePoint(mp))
            Logger.Log(LogEnum.LE_ERROR, "MouseDownCanvas->CreatePoint()");
         e.Handled = true;
      }
   }
}