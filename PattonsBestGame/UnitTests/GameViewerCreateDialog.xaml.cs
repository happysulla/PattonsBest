using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Pattons_Best
{
   public partial class GameViewerCreateDialog : System.Windows.Window
   {
      public bool CtorError { get; } = false;
      private bool myIsFirstShowing = true;
      private DockPanel? TopPanel { get; set; } = null;
      Menu? myMenu = null;
      StatusBar? myStatusBar = null;
      DockPanel? myDockPanelInside = null;
      DockPanel? myDockPanelControls = null;
      Canvas? myCanvasTank = null;
      Image? myImageTank = null;
      ScrollViewer? myScrollViewerText = null;
      TextBlock? myTextBlock = null;
      //-----------------------------------
      ScrollViewer? myScrollViewerMap = null;
      Canvas? myCanvasMap = null;
      Image? myImageMap= null;
      //--------------------------------------------------------------------------------------
      public GameViewerCreateDialog(DockPanel topPanel)
      {
         if (null == topPanel)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog() dockPanel=null");
            CtorError = true;
            return;
         }
         TopPanel = topPanel;
         foreach (UIElement ui0 in TopPanel.Children) // top panel holds myMainMenu, myDockePanelInside, and myStatusBar
         {
            if (ui0 is Menu)
               myMenu = (Menu)ui0;
            else if (ui0 is StatusBar)
               myStatusBar = (StatusBar)ui0;
            else if (ui0 is DockPanel) 
            {
               myDockPanelInside = (DockPanel)ui0;
               foreach (UIElement ui1 in myDockPanelInside.Children)  // myDockPanelInside holds myStackPanelControl and myScrollViewerMap
               {
                  if (ui1 is DockPanel)
                  {
                     myDockPanelControls = ui1 as DockPanel; 
                     if (null != myDockPanelControls)
                     {
                        foreach (UIElement ui2 in myDockPanelControls.Children)  // myStackPanelControl holds myCanvasTank and myScrollViewerTextBox
                        {
                           if (ui2 is Canvas)
                           {
                              myCanvasTank = (Canvas)ui2;
                              foreach (UIElement ui3 in myCanvasTank.Children)
                              {
                                 if (ui3 is Image)
                                 {
                                    myImageTank = (Image)ui3;
                                    break;
                                 }
                              }
                           }
                           else if (ui2 is ScrollViewer)  // myScrollViewerTextBox holds myTextBoxDisplay
                           {
                              myScrollViewerText = (ScrollViewer)ui2;
                              if (myScrollViewerText.Content is TextBlock)
                                 myTextBlock = (TextBlock)myScrollViewerText.Content;
                           }
                        }
                     }
                  }
                  if (ui1 is ScrollViewer)
                  {
                     myScrollViewerMap = (ScrollViewer)ui1; // myScrollViewerMap holds myCanvasMap
                     if (myScrollViewerMap.Content is Canvas)
                     {
                        myCanvasMap = (Canvas)myScrollViewerMap.Content;
                        foreach (UIElement ui2 in myCanvasMap.Children)
                        {
                           if (ui2 is Image)
                           {
                              myImageMap = (Image)ui2;
                              break;
                           }
                        }
                     }
                  }
               }
            }
         }
         if (null == myMenu)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): myMenu=null");
            CtorError = true;
            return;
         }
         if (null == myStatusBar)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): myStatusBar=null");
            CtorError = true;
            return;
         }
         if (null == myScrollViewerMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): myScrollViewerMap=null");
            CtorError = true;
            return;
         }
         if (null == myDockPanelInside)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): myDockPanelInside=null");
            CtorError = true;
            return;
         }
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): image=myCanvasMap");
            CtorError = true;
            return;
         }
         if (null == myImageMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): myImageMap=null");
            CtorError = true;
            return;
         }
         if (null == myDockPanelControls)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): myDockPanelControls=null");
            CtorError = true;
            return;
         }
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): image=myCanvasTank");
            CtorError = true;
            return;
         }
         if (null == myImageTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): myImageTank=null");
            CtorError = true;
            return;
         }
         if (null == myScrollViewerText)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): myScrollViewerText=null");
            CtorError = true;
            return;
         }
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): myTextBlock=null");
            return;
         }
         InitializeComponent();
         //--------------------------------------------------------------------------
         ShowSettings();
         myCanvasMap.MouseLeftButtonDown += this.MouseLeftButtonDown_Canvas;
         myCanvasMap.MouseRightButtonDown += this.MouseRIghtButtonDown_Canvas;
      }
      private void ShowSettings()
      {
         if (null == myMenu)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSettings(): myMenu=null");
            return;
         }
         if (null == myStatusBar)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSettings(): myStatusBar=null");
            return;
         }
         if (null == myScrollViewerMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSettings(): myScrollViewerMap=null");
            return;
         }
         if (null == myDockPanelInside)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSettings(): myDockPanelInside=null");
            return;
         }
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSettings(): image=myCanvasMap");
            return;
         }
         if (null == myImageMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSettings(): myImageMap=null");
            return;
         }
         if (null == myDockPanelControls)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSettings(): myDockPanelControls=null");
            return;
         }
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSettings(): image=myCanvasTank");
            return;
         }
         if (null == myImageTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSettings(): myImageTank=null");
            return;
         }
         if (null == myScrollViewerText)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSettings(): myScrollViewerText=null");
            return;
         }
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSettings(): myTextBlock=null");
            return;
         }
         //---------------------------------------------------------------
         myTextBoxScaleTransform.Text = Utilities.ZoomCanvas.ToString();
         myTextBoxScreenSizeX.Text = System.Windows.SystemParameters.PrimaryScreenWidth.ToString("00.0");
         myTextBoxScreenSizeY.Text = System.Windows.SystemParameters.PrimaryScreenHeight.ToString("00.0");
         //---------------------------------------------------------------
         myTextBoxTopPanelSizeX.Text = this.TopPanel.ActualWidth.ToString("00.0");
         myTextBoxTopPanelSizeY.Text = this.TopPanel.ActualHeight.ToString("00.0");
         myTextBoxMenuSizeY.Text = myMenu.ActualHeight.ToString("00.0");
         myTextBoxStatusBarSizeY.Text = myStatusBar.ActualHeight.ToString("00.0");
         myTextBoxInsideDockPanelSizeX.Text = myDockPanelInside.ActualWidth.ToString("00.0");
         myTextBoxInsideDockPanelSizeY.Text = myDockPanelInside.ActualHeight.ToString("00.0");
         myTextBoxVerticalThumbSizeX.Text = System.Windows.SystemParameters.VerticalScrollBarButtonHeight.ToString("00.0");
         myTextBoxVerticalThumbSizeY.Text = System.Windows.SystemParameters.VerticalScrollBarWidth.ToString("00.0");
         //--------------------------------------------------------------------------
         // myDockPanelControl
         myTextBoxDockPanelControlSizeX.Text = myDockPanelControls.ActualWidth.ToString("00.0");
         myTextBoxDockPanelControlSizeY.Text = myDockPanelControls.ActualHeight.ToString("00.0");
         myTextBoxCanvasTankSizeX.Text = myCanvasTank.ActualWidth.ToString("00.0");
         myTextBoxCanvasTankSizeY.Text = myCanvasTank.ActualHeight.ToString("00.0");
         myTextBoxImageTankSizeX.Text = myImageTank.ActualWidth.ToString("00.0");
         myTextBoxImageTankSizeY.Text = myImageTank.ActualHeight.ToString("00.0");
         myTextBoxScrollViewerTextSizeX.Text = myScrollViewerText.ActualWidth.ToString("00.0");
         myTextBoxScrollViewerTextSizeY.Text = myScrollViewerText.ActualHeight.ToString("00.0");
         myTextBoxTextBoxSizeX.Text = myTextBlock.ActualWidth.ToString("00.0");
         myTextBoxTextBoxSizeY.Text = myTextBlock.ActualHeight.ToString("00.0");
         //---------------------------------------------------------------
         // myScrollViewerMap, myCanvasMap, myImageMap
         myTextBoxScrollViewerMapSizeX.Text = myScrollViewerMap.ActualWidth.ToString("00.0");
         myTextBoxScrollViewerMapSizeY.Text = myScrollViewerMap.ActualHeight.ToString("00.0");
         myTextBoxCanvasMapSizeX.Text = myCanvasMap.ActualWidth.ToString("00.0");
         myTextBoxCanvasMapSizeY.Text = myCanvasMap.ActualHeight.ToString("00.0");
         myTextBoxImageMapSizeX.Text = myImageMap.ActualWidth.ToString("00.0");
         myTextBoxImageMapSizeY.Text = myImageMap.ActualHeight.ToString("00.0");
         //--------------------------------------------------------------------------
         // ScrollViewer Vertical
         myTextBoxVScrollableHeight.Text = myScrollViewerMap.ScrollableHeight.ToString("00.0");
         myTextBoxVerticalOffset.Text = myScrollViewerMap.VerticalOffset.ToString("00.0");
         if (0.0 == myScrollViewerMap.ScrollableHeight)
         {
            myTextBoxScrollableHeightPercent.Text = "N/A";
         }
         else
         {
            int heightPercent = (int)(100.0 * (myScrollViewerMap.VerticalOffset / myScrollViewerMap.ScrollableHeight));
            myTextBoxScrollableHeightPercent.Text = heightPercent.ToString("00.0");
         }
         double heightNormalized = myScrollViewerMap.ScrollableHeight / Utilities.ZoomCanvas;
         myTextBoxScrollableHeightNormalized.Text = heightNormalized.ToString("00.0");
         //--------------------------------------------------------------------------
         // ScrollViewer Horizontal
         myTextBoxScrollableWidth.Text = myScrollViewerMap.ScrollableWidth.ToString("00.0");
         myTextBoxHorizontalOffset.Text = myScrollViewerMap.HorizontalOffset.ToString("00.0");
         if (0.0 == myScrollViewerMap.ScrollableWidth)
         {
            myTextBoxScrollableWidthPercent.Text = "N/A";
         }
         else
         {
            double widthPercent = (int)(100.0 * (myScrollViewerMap.HorizontalOffset / myScrollViewerMap.ScrollableWidth));
            myTextBoxScrollableWidthPercent.Text = widthPercent.ToString("00.0");
         }
         double widthNormalized = myScrollViewerMap.ScrollableWidth / Utilities.ZoomCanvas;
         myTextBoxScrollableWidthNormalized.Text = widthNormalized.ToString("00.0");

      }
      private void ButtonApply_Click(object sender, RoutedEventArgs e)
      {
         if (null == myMenu)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): myMenu=null");
            return;
         }
         if (null == myStatusBar)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): myStatusBar=null");
            return;
         }
         if (null == myScrollViewerMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): myScrollViewerMap=null");
            return;
         }
         if (null == myDockPanelInside)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): myDockPanelInside=null");
            return;
         }
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): image=myCanvasMap");
            return;
         }
         if (null == myImageMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): myImageMap=null");
            return;
         }
         if (null == myDockPanelControls)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): myDockPanelControls=null");
            return;
         }
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): image=myCanvasTank");
            return;
         }
         if (null == myImageTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): myImageTank=null");
            return;
         }
         if (null == myScrollViewerText)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): myScrollViewerText=null");
            return;
         }
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): myTextBlock=null");
            return;
         }
         myDockPanelInside.Height = Double.Parse(myTextBoxInsideDockPanelSizeY.Text);
         myDockPanelInside.Width = Double.Parse(myTextBoxInsideDockPanelSizeX.Text);
         //----------------------------------------------------------------------
         myDockPanelControls.Width = Double.Parse(myTextBoxDockPanelControlSizeX.Text);
         myCanvasTank.Height = Double.Parse(myTextBoxCanvasTankSizeY.Text);
         myCanvasTank.Width = Double.Parse(myTextBoxCanvasTankSizeX.Text);
         myImageTank.Height = Double.Parse(myTextBoxImageTankSizeY.Text);
         myImageTank.Width = Double.Parse(myTextBoxImageTankSizeX.Text);
         double height = myDockPanelInside.Height - myCanvasTank.Height;
         myScrollViewerText.Height = height;
         myScrollViewerText.Width = Double.Parse(myTextBoxScrollViewerTextSizeX.Text);
         myTextBlock.Height = height;
         myTextBlock.Width = myScrollViewerText.ActualWidth - System.Windows.SystemParameters.VerticalScrollBarWidth; // textbox is linked to scrollviewer size
         //----------------------------------------------------------------------
         myScrollViewerMap.Height = Double.Parse(myTextBoxScrollViewerMapSizeY.Text);
         myScrollViewerMap.Width = Double.Parse(myTextBoxScrollViewerMapSizeX.Text);
         myCanvasMap.Height = Double.Parse(myTextBoxCanvasMapSizeY.Text);
         myCanvasMap.Width = Double.Parse(myTextBoxCanvasMapSizeX.Text);
         myImageMap.Height = Double.Parse(myTextBoxImageMapSizeY.Text);
         myImageMap.Width = Double.Parse(myTextBoxImageMapSizeX.Text);
      }
      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void TextBoxScaleTransform_TextChanged(object sender, TextChangedEventArgs e)
      {
         if (null == TopPanel)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBoxScaleTransform_TextChanged() TopPanel=null");
            return;
         }
         Canvas? canvas = null;
         foreach (UIElement ui0 in TopPanel.Children)
         {
            if (ui0 is DockPanel myDockPanelInside)
            {
               myDockPanelInside = (DockPanel)ui0;
               foreach (UIElement ui1 in myDockPanelInside.Children)
               {
                  if (ui1 is ScrollViewer scrollViewer)
                  {
                     if (scrollViewer.Content is Canvas)
                     {
                        canvas = (Canvas)scrollViewer.Content;
                        break;
                     }
                  }
               }
            }
         }
         if (null == canvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBoxScaleTransform_TextChanged() canvas=null");
            return;
         }
         if (false == myIsFirstShowing) // do not zoom when window is first shown
         {
            Utilities.ZoomCanvas = Double.Parse(myTextBoxScaleTransform.Text);
            canvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
         }
         else
         {
            myIsFirstShowing = false;
         }
      }
      private void MouseLeftButtonDown_Canvas(object sender, MouseButtonEventArgs e)
      {
         if (null == myScrollViewerMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDown_Canvas() myScrollViewerMap=null");
            return;
         }
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDown_Canvas(): myCanvas=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvasMap);
         double percentHeightB = (p.Y / myCanvasMap.ActualHeight);
         double percentHeight = percentHeightB;
         double percentToScroll = 0.0;
         if (percentHeight < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentHeight)
            percentToScroll = 1.0;
         else
            percentToScroll = percentHeight / 0.5 - 0.5;
         double amountToScroll = percentToScroll * myScrollViewerMap.ScrollableHeight;
         myScrollViewerMap.ScrollToVerticalOffset(amountToScroll);
         StringBuilder sb = new StringBuilder();
         sb.Append(" %B=");
         sb.Append(percentHeightB.ToString("#.##"));
         sb.Append(" % =");
         if( 0.0 == percentHeight)
            sb.Append("0.00");
         else
            sb.Append(percentHeight.ToString("#.##"));
         sb.Append(" %Scroll=");
         if (0.0 == percentToScroll)
            sb.Append("0.00");
         else
            sb.Append(percentToScroll.ToString("#.##"));
         sb.Append(" amountToScroll=");
         if (0.0 == amountToScroll)
            sb.Append("0.00");
         else
            sb.Append(amountToScroll.ToString("####.#"));
         sb.Append(" out of ");
         sb.Append(myScrollViewerMap.ScrollableHeight.ToString("####.#"));
         Console.WriteLine(sb.ToString());

         double percentWidthB = (p.X / myCanvasMap.ActualWidth);
         double percentWidth = percentWidthB;
         percentToScroll = 0.0;
         if (percentWidth < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentWidth)
            percentToScroll = 1.0;
         else
            percentToScroll = percentWidth / 0.5 - 0.5;
          amountToScroll = percentToScroll * myScrollViewerMap.ScrollableWidth;
         myScrollViewerMap.ScrollToHorizontalOffset(amountToScroll);
         e.Handled = true;
      }
      private void MouseRIghtButtonDown_Canvas(object sender, MouseButtonEventArgs e)
      {
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseRIghtButtonDown_Canvas(): myCanvas=null");
            return;
         }
         System.Windows.Point p = e.GetPosition(myCanvasMap);
         double percentHeight = 100.0 * (p.Y / myCanvasMap.ActualHeight);
         double percentWidth = 100.0 * (p.X / myCanvasMap.ActualWidth);
         StringBuilder sb = new StringBuilder();
         sb.Append("X=");
         sb.Append(p.X.ToString("##.#"));
         sb.Append("\t%=");
         string spWidth = percentWidth.ToString("##");
         sb.Append(spWidth);
         sb.Append("\nY=");
         sb.Append(p.Y.ToString("##.#"));
         sb.Append("\t%=");
         string spHeight = percentHeight.ToString("##");
         sb.Append(spHeight);
         //-------------------------------
         MessageBox.Show(sb.ToString());
         e.Handled = true;
      }
   }
}
