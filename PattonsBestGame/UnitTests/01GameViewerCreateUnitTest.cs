using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Pattons_Best
{
   public class GameViewerCreateUnitTest : IUnitTest
   {
      //--------------------------------------------------------------------
      private DockPanel? myDockPanelTop = null;
      private ScrollViewer? myScrollViewerCanvas = null;
      private Canvas? myCanvasMap = null;
      private Canvas? myCanvasTank = null;
      private double myScrollingTime = 12.0;
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //--------------------------------------------------------------------
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public bool CtorError { get; } = false;
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      //--------------------------------------------------------------------
      public GameViewerCreateUnitTest(DockPanel dp)
      {
         //------------------------------------
         myIndexName = 0;
         myHeaderNames.Add("01-Frame Sizes");
         myHeaderNames.Add("01-Show Stats");
         myHeaderNames.Add("01-Canvas CenterPoint");
         myHeaderNames.Add("01-Finish");
         //------------------------------------
         myCommandNames.Add("Show Dialog");
         myCommandNames.Add("Show Stats");
         myCommandNames.Add("Show Center");
         myCommandNames.Add("Finish");
         //------------------------------------
         myDockPanelTop = dp; // top most dock panel that holds menu, statusbar, left dockpanel, and right dockpanel
         foreach (UIElement ui0 in dp.Children)
         {
            if (ui0 is DockPanel dockPanelInside) // DockPanel showing main play area
            {
               foreach (UIElement ui1 in dockPanelInside.Children)
               {
                  if (ui1 is ScrollViewer)
                  {
                     myScrollViewerCanvas = (ScrollViewer)ui1;
                     if (myScrollViewerCanvas.Content is Canvas)
                        myCanvasMap = (Canvas)myScrollViewerCanvas.Content;  // Find the Canvas in the visual tree
                  }
                  if (ui1 is DockPanel dockPanelControl) // DockPanel that holds the Map Image
                  {
                     foreach (UIElement ui2 in dockPanelControl.Children)
                     {
                        if (ui2 is Canvas)
                           myCanvasTank = (Canvas)ui2;
                     }
                  }
               }
            }
         }
         if (null == myCanvasMap) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myCanvas=null");
            CtorError = true;
            return;
         }
         if (null == myCanvasTank) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myCanvasTank=null");
            CtorError = true;
            return;
         }
      }
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if( null == myDockPanelTop)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myDockPanelTop=null");
            return false;
         }
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvas=null");
            return false;
         }
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasTank=null");
            return false;
         }
         if (null == myScrollViewerCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myScrollViewerCanvas=null");
            return false;
         }
         //-----------------------------------------------------
         if (CommandName == myCommandNames[0])
         {
            GameViewerCreateDialog dialog = new GameViewerCreateDialog(myDockPanelTop); // Get the name from user
            if( true == dialog.CtorError )
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): dialog.CtorError=true");
               return false;
            }
            dialog.Show();
         }
         else if (CommandName == myCommandNames[1])
         {
            CreateMarquee(myCanvasMap);
         }
         else if (CommandName == myCommandNames[2])
         {
            IMapPoint mp = GetCanvasCenter(myScrollViewerCanvas, myCanvasMap);
            CreateEllipse(mp.X, mp.Y); // Add new elipses
         }
         else if (CommandName == myCommandNames[3])
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Cleanup() return falsed");
               return false;
            }
         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi) // Move to the next test in this class's unit tests
      {
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myCanvas=null");
            return false;
         }
         if (HeaderName == myHeaderNames[0])
         {
            ++myIndexName;
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
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup() return falsed");
               return false;
            }
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi) // Remove an elipses from the canvas and save off Territories.xml file
      {
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvas=null");
            return false;
         }
         //--------------------------------------------------
         // Remove any existing UI elements from the Canvas
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMap.Children)
         {
            if (ui is Polygon polygon)
               elements.Add(ui);
            if (ui is Polyline polyline)
               elements.Add(ui);
            if (ui is Ellipse ellipse)
               elements.Add(ui);
            if (ui is Image img)
               elements.Add(ui);
            if (ui is TextBlock tb)
               elements.Add(ui);
         }
         foreach (UIElement ui1 in elements)
            myCanvasMap.Children.Remove(ui1);
         //--------------------------------------------------
         Image imageMap = new Image() { Name = "Map", Width = 1115, Height = 880, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("MapMovement") };
         myCanvasMap.Children.Add(imageMap);
         Canvas.SetLeft(imageMap, 0);
         Canvas.SetTop(imageMap, 0);
         //--------------------------------------------------
         ++gi.GameTurn;
         return true;
      }
      //--------------------------------------------------------------------
      private IMapPoint GetCanvasCenter(ScrollViewer scrollViewer, Canvas canvas)
      {
         double x = 0.0;
         if (canvas.ActualWidth < scrollViewer.ActualWidth / Utilities.ZoomCanvas)
            x = canvas.ActualWidth / 2 + scrollViewer.HorizontalOffset;
         else
            x = scrollViewer.ActualWidth / (2 * Utilities.ZoomCanvas) + scrollViewer.HorizontalOffset / Utilities.ZoomCanvas;
         double y = 0.0;
         if (canvas.ActualHeight < scrollViewer.ActualHeight / Utilities.ZoomCanvas)
            y = canvas.ActualHeight / 2 + scrollViewer.VerticalOffset;
         else
            y = scrollViewer.ActualHeight / (2 * Utilities.ZoomCanvas) + scrollViewer.VerticalOffset / Utilities.ZoomCanvas;
         IMapPoint mp = (IMapPoint)new MapPoint(x, y);
         return mp;
      }
      private void CreateEllipse(double x, double y)
      {
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEllipse(): myCanvas=null");
            return;
         }
         List<UIElement> results = new List<UIElement>(); // Remove old ellipse
         foreach (UIElement ui in myCanvasMap.Children)
         {
            if (ui is Ellipse)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myCanvasMap.Children.Remove(ui1);
         //-------------------------------
         Ellipse aEllipse = new Ellipse
         {
            Name = Utilities.RemoveSpaces("CenterPoint"),
            Fill = Brushes.Black,
            StrokeThickness = 1,
            Stroke = Brushes.Black,
            Width = 30,
            Height = 30
         };
         Canvas.SetLeft(aEllipse, x);
         Canvas.SetTop(aEllipse, y);
         myCanvasMap.Children.Add(aEllipse);
      }
      private void CreateMarquee(Canvas canvas)
      {
         if (null == canvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMarquee(): myCanvas=null");
            return;
         }
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in canvas.Children)
         {
            if (ui is Polygon polygon)
               elements.Add(ui);
            if (ui is Polyline polyline)
               elements.Add(ui);
            if (ui is Ellipse ellipse)
               elements.Add(ui);
            if (ui is Image img)
               elements.Add(ui);
            if (ui is TextBlock tb)
               elements.Add(ui);
         }
         foreach (UIElement ui1 in elements)
            canvas.Children.Remove(ui1);
         //-------------------------------
         TextBlock tbMarquee = new TextBlock() { Foreground = Brushes.Blue, FontFamily = myFontFam, FontSize = 24 };
         tbMarquee.Inlines.Add(new Run("Current Game Statistics:") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic, TextDecorations = TextDecorations.Underline });
         tbMarquee.Inlines.Add(new LineBreak());
         tbMarquee.Inlines.Add(new Run("Test 1"));
         tbMarquee.Inlines.Add(new LineBreak());
         tbMarquee.Inlines.Add(new Run("Test 2"));
         tbMarquee.Inlines.Add(new LineBreak());
         tbMarquee.Inlines.Add(new Run("Test 3"));
         canvas.ClipToBounds = true;
         canvas.Children.Add(tbMarquee);
         //-------------------------------
         DoubleAnimation doubleAnimation = new DoubleAnimation();
         doubleAnimation.From = -tbMarquee.ActualHeight;
         doubleAnimation.To = canvas.ActualHeight;
         doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
         doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(myScrollingTime));
         tbMarquee.BeginAnimation(Canvas.BottomProperty, doubleAnimation);
      }
      private void TopToBottomMarquee(Canvas c, TextBlock tb)
      {
         double width = c.ActualWidth - tb.ActualWidth;
         tb.Margin = new Thickness(width / 2, 0, 0, 0);
         DoubleAnimation doubleAnimation = new DoubleAnimation();
         doubleAnimation.From = -tb.ActualHeight;
         doubleAnimation.To = c.ActualHeight;
         doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
         doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(myScrollingTime));
         tb.BeginAnimation(Canvas.TopProperty, doubleAnimation);
      }
   }
}

