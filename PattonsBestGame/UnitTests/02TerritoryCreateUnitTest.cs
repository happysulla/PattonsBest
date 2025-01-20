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
      private DockPanel myDockPanel;
      private IGameInstance? myGameInstance = null;
      Canvas? myCanvas = null;
      public bool isDragging = false;
      UIElement? myItem = null;
      private System.Windows.Point myPreviousLocation;
      String? myTerritoriesName = null;
      ITerritories? myTerritories = null;
      Territory? myAnchorTerritory = null;
      private double myXColumn = 0;
      private Dictionary<string, Polyline> myRivers = new Dictionary<string, Polyline>();
      private List<Ellipse> myEllipses = new List<Ellipse>();
      public static Double theEllipseDiameter = 30;
      public static Double theEllipseOffset = theEllipseDiameter / 2.0;
      private int myIndexRaft = 0;
      private int myIndexDownRiver = 0;
      private readonly SolidColorBrush mySolidColorBrushWaterBlue = new SolidColorBrush { Color = Colors.DeepSkyBlue };
      //-----------------------------------------
      private int myIndexName = 0;
      public bool CtorError { get; } = false;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      public TerritoryCreateUnitTest(DockPanel dp, IGameInstance gi, string tName)
      {
         myIndexName = 0;
         myHeaderNames.Add("02-Delete Territories");
         myHeaderNames.Add("02-New Territories");
         myHeaderNames.Add("02-Set CenterPoints");
         myHeaderNames.Add("02-Verify Territories");
         myHeaderNames.Add("02-Set Adjacents");
         myHeaderNames.Add("02-Final");
         //------------------------------------
         myCommandNames.Add("00-Delete File");
         myCommandNames.Add("01-Click Center of Hex");
         myCommandNames.Add("02-Click Elispse to Move");
         myCommandNames.Add("03-Click Ellispe to Verify");
         myCommandNames.Add("04-Verify Adjacents");
         myCommandNames.Add("05-Cleanup");
         //------------------------------------
         myGameInstance = gi;
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
         if( null == myCanvas )
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): myCanvas=null");
            CtorError = true;
            return;
         }
         //----------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //----------------------------------
         if (null == tName)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): tName=null");
            CtorError = true;
            return;
         }
         switch (tName)
         {
            case Territories.MOVE_TERRITORIES:
               myTerritories = Territories.theMoveTerritories;
               break;
            case Territories.TANK_TERRITORIES:
               myTerritories = Territories.theTankTerritories;
               break;
            case Territories.BATTLE_TERRITORIES:
               myTerritories = Territories.theBattleTerritories;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest(): reached default with " + tName);
               CtorError = true;
               return;
         }
         myTerritoriesName = tName;
         //----------------------------------
      }
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvas=null");
            return false;
         }
         if (null == myTerritoriesName)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myTerritoriesName=null");
            return false;
         }
         if (null == myTerritories)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myTerritories=null");
            return false;
         }
         //-----------------------------------
         if (CommandName == myCommandNames[0])
         {
            string filename = ConfigFileReader.theConfigDirectory + myTerritoriesName;
            System.IO.File.Delete(filename);  // delete old file
            if (false == NextTest(ref gi)) // automatically move next test
            {
               Console.WriteLine("TerritoryCreateUnitTest.Command(): NextTest() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[1]) //  Create territories on Move Map
         {

         }
         else if (CommandName == myCommandNames[2]) // set centerpoints
         {

         }
         else if (CommandName == myCommandNames[3])
         {
            myXColumn = 0.0; // When set to zero, it indicates that use existing value instead of value from previous entry
         }
         else if (CommandName == myCommandNames[4])
         {
            if (false == ShowAdjacents(myTerritories))
            {
               Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.Command(): ShowAdjacents() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[9])
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
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myCanvas=null");
            return false;
         }
         if (null == myTerritoriesName)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myTerritoriesName=null");
            return false;
         }
         if (null == myTerritories)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myTerritories=null");
            return false;
         }
         //---------------------------------
         if (HeaderName == myHeaderNames[0])
         {
            CreateEllipses(myTerritories);
            myCanvas.MouseLeftButtonDown += this.MouseLeftButtonDownCreateTerritory;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[1])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownCreateTerritory;
            myCanvas.MouseLeftButtonDown += this.MouseDownSetCenterPoint;
            myCanvas.MouseMove += MouseMove;
            myCanvas.MouseUp += MouseUp;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[2])
         {
            myCanvas.MouseMove -= MouseMove;
            myCanvas.MouseUp -= MouseUp;
            myCanvas.MouseLeftButtonDown -= this.MouseDownSetCenterPoint;
            myCanvas.MouseLeftButtonDown += this.MouseLeftButtonDownVerifyTerritory;
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3]) // 
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownVerifyTerritory;
            myCanvas.MouseLeftButtonDown += this.MouseLeftButtonDownSetAdjacents;
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownSetAdjacents;
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
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvas=null");
            return false;
         }
         if (null == myTerritoriesName)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myTerritoriesName=null");
            return false;
         }
         if (null == myTerritories)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myTerritories=null");
            return false;
         }
         //---------------------------------
         if (HeaderName == myHeaderNames[1])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownCreateTerritory;
         }
         else if (HeaderName == myHeaderNames[2])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseDownSetCenterPoint;
            myCanvas.MouseMove -= MouseMove;
            myCanvas.MouseUp -= MouseUp;
         }
         else if (HeaderName == myHeaderNames[3])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownVerifyTerritory;
         }
         else if (HeaderName == myHeaderNames[4])
         {
            myCanvas.MouseLeftButtonDown -= this.MouseLeftButtonDownSetAdjacents;
         }
         //--------------------------------------------------
         // Remove any existing UI elements from the Canvas
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myCanvas.Children.Remove(ui1);
         //--------------------------------------------------
         // Delete Existing Territories.xml file and create a new one based on myGameEngine.Territories container
         try
         {
            string filename = ConfigFileReader.theConfigDirectory + myTerritoriesName;
            System.IO.File.Delete(filename);  // delete old file
            XmlDocument aXmlDocument = CreateXml(myTerritories); // create a new XML document based on Territories
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
            Console.WriteLine("Cleanup(): exeption={0}", e.Message);
            return false;
         }
         //--------------------------------------------------
         ++gi.GameTurn;
         return true;
      }
      //--------------------------------------------------------------------
      public void CreateEllipse(ITerritory territory)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipse(): myCanvas=null");
            return;
         }
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush{ Color = Color.FromArgb(100, 255, 255, 0) };
         string territoryName = territory.Name;
         Ellipse aEllipse = new Ellipse
         {
            Tag = Utilities.RemoveSpaces(territoryName),
            Fill = aSolidColorBrush1,
            StrokeThickness = 1,
            Stroke = Brushes.Red,
            Width = theEllipseDiameter,
            Height = theEllipseDiameter
         };
         System.Windows.Point p = new System.Windows.Point(territory.CenterPoint.X, territory.CenterPoint.Y);
         Canvas.SetLeft(aEllipse, p.X);
         Canvas.SetTop(aEllipse, p.Y);
         p.X -= theEllipseOffset;
         p.Y -= theEllipseOffset;
         myCanvas.Children.Add(aEllipse);
         myEllipses.Add(aEllipse);
      }
      public void CreateEllipses(ITerritories territories)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipses(): myCanvas=null");
            return;
         }
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) }; // nearly transparent but slightly colored
         foreach (Territory t in territories)
         {
            Ellipse aEllipse = new Ellipse { Tag = Utilities.RemoveSpaces(t.ToString()) };
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
            myCanvas.Children.Add(aEllipse);
            myEllipses.Add(aEllipse);
         }
      }
      public XmlDocument CreateXml(ITerritories territories)
      {
         XmlDocument aXmlDocument = new XmlDocument();
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): aXmlDocument.DocumentElement=null");
            return aXmlDocument;
         }
         aXmlDocument.LoadXml("<Territories></Territories>");
         foreach (Territory t in territories)
         {
            XmlElement? terrElem = aXmlDocument.CreateElement("Territory");  // name of territory
            terrElem.SetAttribute("value", t.Name);
            aXmlDocument.DocumentElement.LastChild.AppendChild(terrElem);
            XmlElement typeElem = aXmlDocument.CreateElement("type"); 
            typeElem.SetAttribute("value", t.Type);
            aXmlDocument.DocumentElement.AppendChild(typeElem);
            XmlElement pointElem = aXmlDocument.CreateElement("point"); // center point for this territory
            pointElem.SetAttribute("X", t.CenterPoint.X.ToString());
            pointElem.SetAttribute("Y", t.CenterPoint.Y.ToString());
            if (null == aXmlDocument.DocumentElement.LastChild)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): aXmlDocument.DocumentElement.LastChild=null");
               return aXmlDocument;
            }
            aXmlDocument.DocumentElement.LastChild.AppendChild(pointElem);
            foreach (string s in t.Adjacents) // List of adjacent territories
            {
               XmlElement adjacentElem = aXmlDocument.CreateElement("adjacent");
               adjacentElem.SetAttribute("value", s);
               aXmlDocument.DocumentElement.LastChild.AppendChild(adjacentElem);
            }
            foreach (IMapPoint p in t.Points) // Points that make up the polygon of this territory
            {
               System.Windows.Point point = new System.Windows.Point(p.X, p.Y);
               XmlElement regionPointElem = aXmlDocument.CreateElement("regionPoint");
               regionPointElem.SetAttribute("X", p.X.ToString());
               regionPointElem.SetAttribute("Y", p.Y.ToString());
               aXmlDocument.DocumentElement.LastChild.AppendChild(regionPointElem);
            }
         }
         return aXmlDocument;
      }
      public bool ShowAdjacents(ITerritories territories)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowAdjacents(): myCanvas=null");
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
            string anchorName = Utilities.RemoveSpaces(anchorTerritory.ToString());
            Ellipse? anchorEllipse = null; // Find the corresponding ellipse for this anchor territory
            foreach (UIElement ui in myCanvas.Children)
            {
               if (ui is Ellipse)
               {
                  Ellipse ellipse = (Ellipse)ui;
                  if (anchorName == ellipse.Tag.ToString())
                  {
                     anchorEllipse = ellipse;
                     break;
                  }
               }
            }
            if (null == anchorEllipse)
            {
               Logger.Log(LogEnum.LE_ERROR, anchorTerritory.ToString());
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
                  if (t.ToString() == s)
                  {
                     adjacentTerritory = t;
                     break;
                  }
               }
               if (null == adjacentTerritory)
               {
                  MessageBox.Show("Not Found s=" + s);
                  return false;
               }
               string adjacentName = Utilities.RemoveSpaces(adjacentTerritory.Name);
               Ellipse? adjacentEllipse = null; // Find the corresponding ellipse for this territory
               foreach (UIElement ui in myCanvas.Children)
               {
                  if (ui is Ellipse)
                  {
                     Ellipse ellipse = (Ellipse)ui;
                     if (adjacentName == ellipse.Tag.ToString())
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
                  string returnName = Utilities.RemoveSpaces(s1);
                  if (returnName == anchorName)
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
                  StringBuilder sb = new StringBuilder("anchor="); sb.Append(anchorName); sb.Append(" NOT in list for adjacent="); sb.Append(adjacentName);
                  MessageBox.Show(sb.ToString());
                  return false;
               }
            }
         }
         return true;
      }
      //--------------------------------------------------------------------
      void MouseLeftButtonDownCreateTerritory(object sender, MouseButtonEventArgs e)
      {
         if (null == myTerritories)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownCreateTerritory(): myTerritories=null");
            return;
         }
         SolidColorBrush aSolidColorBrush = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) };
         System.Windows.Point p = e.GetPosition(myCanvas);
         TerritoryCreateDialog dialog = new TerritoryCreateDialog(); // Get the name from user
         dialog.myTextBoxName.Focus();
         if (true == dialog.ShowDialog())
         {
            Territory territory = new Territory(dialog.myTextBoxName.Text) { CenterPoint = new MapPoint(p.X, p.Y) };
            CreateEllipse(territory);
            territory.Type = dialog.RadioOutputText;
            myTerritories.Add(territory);
         }
      }
      void MouseDownSetCenterPoint(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseDownSetCenterPoint(): myCanvas=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvas);
         Console.WriteLine("TerritoryUnitTest.MouseDown(): {0}", p.ToString());
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  if (false == isDragging)
                  {
                     MessageBox.Show(ellipse.Tag.ToString());
                     myPreviousLocation = p;
                     this.isDragging = true;
                     this.myItem = ui;
                  }
               }
            }
         }
      }
      void MouseMove(object sender, MouseEventArgs e)
      {
         if (true == isDragging)
         {
            if (null != myItem)
            {
               System.Windows.Point newPoint = e.GetPosition(myCanvas);
               Canvas.SetTop(myItem, newPoint.Y - theEllipseOffset);
               Canvas.SetLeft(myItem, newPoint.X - theEllipseOffset);
            }
         }
      }
      void MouseUp(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point newPoint = e.GetPosition(myCanvas);
         this.isDragging = false;
         if (this.myItem != null)
         {
            if (this.myItem is Ellipse)
            {
               Ellipse ellipse = (Ellipse)myItem;
               if( null == ellipse.Tag)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseUp(): ellipse.Tag=null");
                  return;
               }
               string? tag = ellipse.Tag.ToString();
               if (null == tag)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MouseUp(): tag=null");
                  return;
               }
               foreach (Territory t in myTerritories)
               {
                  string name = Utilities.RemoveSpaces(t.ToString());
                  if (tag == name)
                  {
                     t.CenterPoint.X = newPoint.X;
                     t.CenterPoint.Y = newPoint.Y;
                     this.myItem = null;
                     break;
                  }
               }
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.MouseUp() this.myItem != null");
         }
      }
      void MouseLeftButtonDownVerifyTerritory(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownVerifyTerritory(): myCanvas=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvas);
         Console.WriteLine("TerritoryCreateUnitTest.MouseLeftButtonDownVerifyTerritory(): {0}", p.ToString());
         ITerritory? selectedTerritory = null;
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse ellipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  string? tag = ellipse.Tag.ToString();
                  if (null == tag)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownVerifyTerritory(): tag=null");
                     return;
                  }
                  foreach (Territory t in myTerritories)
                  {
                     string name = Utilities.RemoveSpaces(t.ToString());
                     if (tag == name)
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
                  if (null != selectedTerritory)
                     break;
               }
            }
         }
         if (null == selectedTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryCreateUnitTest.MouseUp() this.myItem != null");
            return;
         }
         TerritoryVerifyDialog dialog = new TerritoryVerifyDialog(selectedTerritory, myXColumn);
         dialog.myButtonOk.Focus();
         if (true == dialog.ShowDialog())
         {
            selectedTerritory.CenterPoint.X = Double.Parse(dialog.CenterPointX);
            myXColumn = selectedTerritory.CenterPoint.X; // Want the same X value as specified in the last dialog. This lines up dots.
            selectedTerritory.CenterPoint.Y = Double.Parse(dialog.CenterPointY);
         }
      }
      void MouseLeftButtonDownSetAdjacents(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetAdjacents(): myCanvas=null");
            return;
         }
         SolidColorBrush aSolidColorBrush0 = new SolidColorBrush { Color = Color.FromArgb(100, 100, 100, 0) };
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush { Color = Color.FromArgb(010, 255, 100, 0) };
         SolidColorBrush aSolidColorBrush2 = new SolidColorBrush { Color = Color.FromArgb(255, 0, 0, 0) };
         SolidColorBrush aSolidColorBrush3 = new SolidColorBrush { Color = Colors.Red };
         System.Windows.Point p = e.GetPosition(myCanvas);
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Ellipse)
            {
               Ellipse selectedEllipse = (Ellipse)ui;
               if (true == ui.IsMouseOver)
               {
                  Territory? selectedTerritory = null;  // Find the corresponding Territory that user selected
                  foreach (Territory t in myTerritories)
                  {
                     if (selectedEllipse.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
                     {
                        selectedTerritory = t;
                        break;
                     }
                  }
                  if (selectedTerritory == null) // Check for error
                  {
                     MessageBox.Show("Unable to find " + selectedEllipse.Tag.ToString());
                     return;
                  }
                  if (null == myAnchorTerritory)  // If there is no anchor territory. Set it.
                  {
                     StringBuilder sb = new StringBuilder("Anchoring: ");
                     sb.Append(selectedEllipse.Tag.ToString());
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
                     StringBuilder sb = new StringBuilder("Saving"); sb.Append(selectedEllipse.Tag.ToString()); sb.Append(" "); sb.Append(myAnchorTerritory.ToString());
                     sb.Append(" "); sb.Append(selectedTerritory.ToString()); sb.Append(" ");
                     Console.WriteLine("Saving {0} ", selectedTerritory.ToString());
                     MessageBox.Show(sb.ToString());
                     myAnchorTerritory = null;

                     foreach (UIElement ui1 in myCanvas.Children)
                     {
                        if (ui1 is Ellipse)
                        {
                           Ellipse ellipse1 = (Ellipse)ui1;
                           foreach (Territory t in myTerritories)
                           {
                              if (ellipse1.Tag.ToString() == Utilities.RemoveSpaces(t.ToString()))
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
         }  // foreach (UIElement ui in myCanvas.Children)
      }
   }
}

