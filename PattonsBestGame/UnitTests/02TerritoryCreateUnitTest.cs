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
      private string? myFileName = null;
      private DockPanel myDockPanelTop;
      private IGameInstance? myGameInstance = null;
      Canvas? myCanvasTank = null;
      Canvas? myCanvasMain = null;
      public bool myIsDraggingMain = false;
      public bool myIsDraggingTank = false;
      UIElement? myItem = null;
      private System.Windows.Point myPreviousLocation;
      Territory? myAnchorTerritory = null;
      private List<Ellipse> myEllipses = new List<Ellipse>();
      public static Double theEllipseDiameter = 30;
      public static Double theEllipseOffset = theEllipseDiameter / 2.0;
      private readonly SolidColorBrush mySolidColorBrushWaterBlue = new SolidColorBrush { Color = Colors.DeepSkyBlue };
      //-----------------------------------------
      private int myIndexName = 0;
      public bool CtorError { get; } = false;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      public TerritoryCreateUnitTest(DockPanel dp, IGameInstance gi)
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
         //----------------------------------
         if( false == SetFileName())
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
         else if (CommandName == myCommandNames[1]) //  Create territories on Move Map
         {

         }
         else if (CommandName == myCommandNames[2]) // set centerpoints
         {

         }
         else if (CommandName == myCommandNames[3])
         {

         }
         else if (CommandName == myCommandNames[4])
         {
            if (false == ShowAdjacents(Territories.theTerritories))
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
            CreateEllipses(Territories.theTerritories);
            myCanvasTank.MouseLeftButtonDown += this.MouseLeftButtonDownCreateTerritory;
            myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownCreateTerritory;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[1])
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseLeftButtonDownCreateTerritory;
            myCanvasTank.MouseLeftButtonDown += this.MouseDownSetCenterPoint;
            myCanvasTank.MouseMove += MouseMove;
            myCanvasTank.MouseUp += MouseUp;
            //----------------------------
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownCreateTerritory;
            myCanvasMain.MouseLeftButtonDown += this.MouseDownSetCenterPoint;
            myCanvasMain.MouseMove += MouseMove;
            myCanvasMain.MouseUp += MouseUp;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[2])
         {
            myCanvasTank.MouseMove -= MouseMove;
            myCanvasTank.MouseUp -= MouseUp;
            myCanvasTank.MouseLeftButtonDown -= this.MouseDownSetCenterPoint;
            myCanvasTank.MouseLeftButtonDown += this.MouseLeftButtonDownVerifyTerritory;
            //----------------------------
            myCanvasMain.MouseMove -= MouseMove;
            myCanvasMain.MouseUp -= MouseUp;
            myCanvasMain.MouseLeftButtonDown -= this.MouseDownSetCenterPoint;
            myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownVerifyTerritory;
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3]) // 
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseLeftButtonDownVerifyTerritory;
            myCanvasTank.MouseLeftButtonDown += this.MouseLeftButtonDownSetAdjacents;
            //----------------------------
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownVerifyTerritory;
            myCanvasMain.MouseLeftButtonDown += this.MouseLeftButtonDownSetAdjacents;
            myAnchorTerritory = null;
            ++myIndexName;
         }
         else
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
            myCanvasTank.MouseLeftButtonDown -= this.MouseDownSetCenterPoint;
            myCanvasTank.MouseMove -= MouseMove;
            myCanvasTank.MouseUp -= MouseUp;
            myCanvasMain.MouseLeftButtonDown -= this.MouseDownSetCenterPoint;
            myCanvasMain.MouseMove -= MouseMove;
            myCanvasMain.MouseUp -= MouseUp;
         }
         else if (HeaderName == myHeaderNames[3])
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseLeftButtonDownVerifyTerritory;
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownVerifyTerritory;
         }
         else if (HeaderName == myHeaderNames[4])
         {
            myCanvasTank.MouseLeftButtonDown -= this.MouseLeftButtonDownSetAdjacents;
            myCanvasMain.MouseLeftButtonDown -= this.MouseLeftButtonDownSetAdjacents;
         }
         //--------------------------------------------------
         // Remove any existing UI elements from the Canvas
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvasTank.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myCanvasTank.Children.Remove(ui1);
         //--------------------------------------------------
         // Remove any existing UI elements from the Canvas
         results = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myCanvasMain.Children.Remove(ui1);
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
      private void CreateEllipse(ITerritory territory)
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipse(): myCanvasTank=null");
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipse(): myCanvasMain=null");
            return;
         }
         SolidColorBrush aSolidColorBrush1 = new SolidColorBrush{ Color = Colors.Black };
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
         p.X -= theEllipseOffset;
         p.Y -= theEllipseOffset;
         Canvas.SetLeft(aEllipse, p.X);
         Canvas.SetTop(aEllipse, p.Y);
         if ( "Main" == territory.CanvasName)
            myCanvasMain.Children.Add(aEllipse);
         else
            myCanvasTank.Children.Add(aEllipse);
         myEllipses.Add(aEllipse);
      }
      private void CreateEllipses(ITerritories territories)
      {
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
            if ("Main" == t.CanvasName)
               myCanvasMain.Children.Add(aEllipse);
            else
               myCanvasTank.Children.Add(aEllipse);
            myEllipses.Add(aEllipse);
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
            string anchorName = Utilities.RemoveSpaces(anchorTerritory.ToString());
            Ellipse? anchorEllipse = null; // Find the corresponding ellipse for this anchor territory
            foreach (UIElement ui in myCanvasMain.Children)
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
               foreach (UIElement ui in myCanvasMain.Children)
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
            CreateEllipse(territory);
            Territories.theTerritories.Add(territory);
         }
      }
      void MouseDownSetCenterPoint(object sender, MouseButtonEventArgs e)
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
                     MessageBox.Show(ellipse.Tag.ToString());
                     myPreviousLocation = p;
                     this.myIsDraggingTank = true;
                     this.myItem = ui;
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
                     MessageBox.Show(ellipse.Tag.ToString());
                     myPreviousLocation = p;
                     this.myIsDraggingMain = true;
                     this.myItem = ui;
                  }
               }
            }
         }
      }
      void MouseMove(object sender, MouseEventArgs e)
      {
         if (true == myIsDraggingMain)
         {
            if (null != myItem)
            {
               System.Windows.Point newPoint = e.GetPosition(myCanvasMain);
               Canvas.SetTop(myItem, newPoint.Y - theEllipseOffset);
               Canvas.SetLeft(myItem, newPoint.X - theEllipseOffset);
            }
         }
         else if (true == myIsDraggingTank)
         {
            if (null != myItem)
            {
               System.Windows.Point newPoint = e.GetPosition(myCanvasTank);
               Canvas.SetTop(myItem, newPoint.Y - theEllipseOffset);
               Canvas.SetLeft(myItem, newPoint.X - theEllipseOffset);
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
               foreach (Territory t in Territories.theTerritories)
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
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownVerifyTerritory(): myCanvasTank=null");
            return;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownVerifyTerritory(): myCanvasMain=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvasMain);
         Console.WriteLine("TerritoryCreateUnitTest.MouseLeftButtonDownVerifyTerritory(): {0}", p.ToString());
         ITerritory? selectedTerritory = null;
         foreach (UIElement ui in myCanvasMain.Children)
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
                  foreach (Territory t in Territories.theTerritories)
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
         TerritoryVerifyDialog dialog = new TerritoryVerifyDialog(selectedTerritory);
         dialog.myButtonOk.Focus();
         if (true == dialog.ShowDialog())
         {
            selectedTerritory.CenterPoint.X = Double.Parse(dialog.CenterPointX);
            selectedTerritory.CenterPoint.Y = Double.Parse(dialog.CenterPointY);
         }
      }
      void MouseLeftButtonDownSetAdjacents(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDownSetAdjacents(): myCanvasTank=null");
            return;
         }
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

                     foreach (UIElement ui1 in myCanvasMain.Children)
                     {
                        if (ui1 is Ellipse)
                        {
                           Ellipse ellipse1 = (Ellipse)ui1;
                           foreach (Territory t in Territories.theTerritories)
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
         }  // foreach (UIElement ui in myCanvasMain.Children)
      }
   }
}

