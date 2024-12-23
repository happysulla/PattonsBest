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
      private DockPanel? TopPanelTop { get; set; } = null;
      Canvas? myCanvasMap = null;
      Canvas? myCanvasTank = null;
      ScrollViewer? myScrollViewer = null;
      public GameViewerCreateDialog(DockPanel topPanel)
      {
         if (null == topPanel)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog() dockPanel=null");
            CtorError = true;
            return;
         }
         TopPanelTop = topPanel;
         DockPanel? dockPanelInside = null;
         StackPanel? stackPanelControls = null;
         Image? imageMap = null;
         Image? imageTank = null;
         ScrollViewer? scrollViewerTextBox = null;
         TextBox? textBox = null;
         foreach (UIElement ui0 in TopPanelTop.Children) // top panel holds myMainMenu, myDockePanelInside, and myStatusBar
         {
            if (ui0 is DockPanel) // myDockPanelInside holds myScrollViewerInside (which holds canvas) and myStackPanelControl
            {
               dockPanelInside = (DockPanel)ui0;
               foreach (UIElement ui1 in dockPanelInside.Children)
               {
                  if (ui1 is ScrollViewer)
                  {
                     myScrollViewer = (ScrollViewer)ui1;
                     if (myScrollViewer.Content is Canvas)
                     {
                        myCanvasMap = (Canvas)myScrollViewer.Content;
                        foreach (UIElement ui2 in myCanvasMap.Children)
                        {
                           if (ui2 is Image)
                           {
                              imageMap = (Image)ui2;
                              break;
                           }
                        }
                     }
                  }
                  if (ui1 is StackPanel)
                  {
                     stackPanelControls = ui1 as StackPanel;
                     if (null != stackPanelControls)
                     {
                        foreach (UIElement ui2 in stackPanelControls.Children)
                        {
                           if (ui2 is Canvas)
                           {
                              myCanvasTank = (Canvas)ui2;
                              foreach (UIElement ui3 in myCanvasTank.Children)
                              {
                                 if (ui3 is Image)
                                 {
                                    imageTank = (Image)ui3;
                                    break;
                                 }
                              }
                           }
                           if (ui2 is ScrollViewer)
                           {
                              scrollViewerTextBox = (ScrollViewer)ui2;
                              if (scrollViewerTextBox.Content is TextBox)
                                 textBox = (TextBox)scrollViewerTextBox.Content;
                           }
                        }
                     }
                  }
               }
            }
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): myScrollViewer=null");
            CtorError = true;
            return;
         }
         if (null == dockPanelInside)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): dockPanelInside=null");
            CtorError = true;
            return;
         }
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): image=myCanvasMap");
            CtorError = true;
            return;
         }
         if (null == imageMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): imageMap=null");
            CtorError = true;
            return;
         }
         if (null == stackPanelControls)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): stackPanelControls=null");
            CtorError = true;
            return;
         }
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): image=myCanvasTank");
            CtorError = true;
            return;
         }
         if (null == imageTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): imageTank=null");
            CtorError = true;
            return;
         }
         if (null == scrollViewerTextBox)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): scrollViewerTextBox=null");
            CtorError = true;
            return;
         }
         if (null == textBox)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateDialog(): textBox=null");
            return;
         }
         InitializeComponent();
         //---------------------------------------------------------------
         myTextBoxScreenSizeX.Text = System.Windows.SystemParameters.PrimaryScreenWidth.ToString();
         myTextBoxScreenSizeY.Text = System.Windows.SystemParameters.PrimaryScreenHeight.ToString();
         myTextBoxScaleTransform.Text = Utilities.ZoomCanvas.ToString();
         myTextBoxTopPanelSizeX.Text = topPanel.ActualWidth.ToString();
         myTextBoxTopPanelSizeY.Text = topPanel.ActualHeight.ToString();
         myTextBoxInsideDockPanelSizeX.Text = dockPanelInside.ActualWidth.ToString();
         myTextBoxInsideDockPanelSizeY.Text = dockPanelInside.ActualHeight.ToString();
         myTextBoxVerticalThumbSizeX.Text = System.Windows.SystemParameters.VerticalScrollBarButtonHeight.ToString();
         myTextBoxVerticalThumbSizeY.Text = System.Windows.SystemParameters.VerticalScrollBarWidth.ToString();
         //---------------------------------------------------------------
         // ScrollViewer, ImageCanvas, ImageMap
         myTextBoxScrollViewerSizeX.Text = myScrollViewer.ActualWidth.ToString();
         myTextBoxScrollViewerSizeY.Text = myScrollViewer.ActualHeight.ToString();
         myTextBoxCanvasMapSizeX.Text = myCanvasMap.ActualWidth.ToString();
         myTextBoxCanvasMapSizeY.Text = myCanvasMap.ActualHeight.ToString();
         myTextBoxImageMapSizeX.Text = imageMap.ActualWidth.ToString();
         myTextBoxImageMapSizeY.Text = imageMap.ActualHeight.ToString();
         //--------------------------------------------------------------------------
         // ScrollViewer Vertical
         myTextBoxVScrollableHeight.Text = myScrollViewer.ScrollableHeight.ToString();
         myTextBoxVerticalOffset.Text = myScrollViewer.VerticalOffset.ToString();
         myTextBoxScrollableHeightPercent.Text = myScrollViewer.ScrollableHeight.ToString();
         int heightPercent = (int)(100.0 * (myScrollViewer.VerticalOffset / myScrollViewer.ScrollableHeight));
         myTextBoxScrollableHeightPercent.Text = heightPercent.ToString();
         double heightNormalized = myScrollViewer.ScrollableHeight / Utilities.ZoomCanvas;
         myTextBoxScrollableHeightNormalized.Text = heightNormalized.ToString();
         //--------------------------------------------------------------------------
         // ScrollViewer Horizontal
         myTextBoxScrollableWidth.Text = myScrollViewer.ScrollableWidth.ToString();
         myTextBoxHorizontalOffset.Text = myScrollViewer.HorizontalOffset.ToString();
         myTextBoxScrollableWidthPercent.Text = myScrollViewer.ScrollableWidth.ToString();
         double widthPercent = (int)(100.0 * (myScrollViewer.HorizontalOffset / myScrollViewer.ScrollableWidth));
         myTextBoxScrollableWidthPercent.Text = widthPercent.ToString();
         double widthNormalized = myScrollViewer.ScrollableWidth / Utilities.ZoomCanvas;
         myTextBoxScrollableWidthNormalized.Text = widthNormalized.ToString();
         //--------------------------------------------------------------------------
         // StackPanel
         myTextBoxStackPanelControlsSizeX.Text = stackPanelControls.ActualWidth.ToString();
         myTextBoxStackPanelControlsSizeY.Text = stackPanelControls.ActualHeight.ToString();
         myTextBoxCanvasTankSizeX.Text = myCanvasTank.ActualWidth.ToString();
         myTextBoxCanvasTankSizeY.Text = myCanvasTank.ActualHeight.ToString();
         myTextBoxImageTankSizeX.Text = imageTank.ActualWidth.ToString();
         myTextBoxImageTankSizeY.Text = imageTank.ActualHeight.ToString();
         myTextBoxScrollViewerTextSizeX.Text = scrollViewerTextBox.ActualWidth.ToString();
         myTextBoxScrollViewerTextSizeY.Text = scrollViewerTextBox.ActualHeight.ToString();
         textBox.Width = scrollViewerTextBox.ActualWidth;
         textBox.Height = scrollViewerTextBox.ActualHeight;
         //--------------------------------------------------------------------------
         myCanvasMap.MouseLeftButtonDown += this.MouseLeftButtonDown_Canvas;
         myCanvasMap.MouseRightButtonDown += this.MouseRIghtButtonDown_Canvas;
      }
      private void ButtonApply_Click(object sender, RoutedEventArgs e)
      {
         if (null == TopPanelTop)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click() TopPanelTop=null");
            return;
         }
         DockPanel? dockPanelInside = null;
         StackPanel? stackPanelControls = null;
         Image? imageMap = null;
         Image? imageTank = null;
         ScrollViewer? scrollViewerTextBox = null;
         TextBox? textBox = null;
         foreach (UIElement ui0 in TopPanelTop.Children) // top panel holds myMainMenu, myDockePanelInside, and myStatusBar
         {
            if (ui0 is DockPanel) // myDockPanelInside holds myScrollViewerInside (which holds canvas) and myStackPanelControl
            {
               dockPanelInside = (DockPanel)ui0;
               foreach (UIElement ui1 in dockPanelInside.Children)
               {
                  if (ui1 is ScrollViewer)
                  {
                     myScrollViewer = (ScrollViewer)ui1;
                     if (myScrollViewer.Content is Canvas)
                     {
                        myCanvasMap = (Canvas)myScrollViewer.Content;
                        foreach (UIElement ui2 in myCanvasMap.Children)
                        {
                           if (ui2 is Image)
                           {
                              imageMap = (Image)ui2;
                              break;
                           }
                        }
                     }
                  }
                  if (ui1 is StackPanel)
                  {
                     stackPanelControls = ui1 as StackPanel;
                     if (null != stackPanelControls)
                     {
                        foreach (UIElement ui2 in stackPanelControls.Children)
                        {
                           if (ui2 is Canvas)
                           {
                              myCanvasTank = (Canvas)ui2;
                              foreach (UIElement ui3 in myCanvasTank.Children)
                              {
                                 if (ui3 is Image)
                                 {
                                    imageTank = (Image)ui3;
                                    break;
                                 }
                              }
                           }
                           if (ui2 is ScrollViewer)
                           {
                              scrollViewerTextBox = (ScrollViewer)ui2;
                              if( scrollViewerTextBox.Content is TextBox )
                                 textBox = (TextBox)scrollViewerTextBox.Content; 
                           }
                        }
                     }
                  }
               }
            }
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): myScrollViewer=null");
            return;
         }
         if (null == dockPanelInside)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): dockPanelInside=null");
            return;
         }
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): image=myCanvasMap");
            return;
         }
         if (null == imageMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): imageMap=null");
            return;
         }
         if (null == stackPanelControls)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): stackPanelControls=null");
            return;
         }
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): image=myCanvasTank");
            return;
         }
         if (null == imageTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): imageTank=null");
            return;
         }
         if (null == scrollViewerTextBox)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): scrollViewerTextBox=null");
            return;
         }
         if (null == textBox)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonApply_Click(): textBox=null");
            return;
         }
         dockPanelInside.Height = Double.Parse(myTextBoxInsideDockPanelSizeY.Text);
         dockPanelInside.Width = Double.Parse(myTextBoxInsideDockPanelSizeX.Text);
         stackPanelControls.Width = Double.Parse(myTextBoxStackPanelControlsSizeX.Text);
         myScrollViewer.Height = Double.Parse(myTextBoxScrollViewerSizeY.Text);
         myScrollViewer.Width = Double.Parse(myTextBoxScrollViewerSizeX.Text);
         myCanvasMap.Height = Double.Parse(myTextBoxCanvasMapSizeY.Text);
         myCanvasMap.Width = Double.Parse(myTextBoxCanvasMapSizeX.Text);
         imageMap.Height = Double.Parse(myTextBoxImageMapSizeY.Text);
         imageMap.Width = Double.Parse(myTextBoxImageMapSizeX.Text);
         myCanvasTank.Height = Double.Parse(myTextBoxCanvasTankSizeY.Text);
         myCanvasTank.Width = Double.Parse(myTextBoxCanvasTankSizeX.Text);
         imageTank.Height = Double.Parse(myTextBoxImageTankSizeY.Text);
         imageTank.Width = Double.Parse(myTextBoxImageTankSizeX.Text);
         textBox.Width = scrollViewerTextBox.ActualWidth;
         textBox.Height = scrollViewerTextBox.ActualHeight;
      }
      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void TextBoxScaleTransform_TextChanged(object sender, TextChangedEventArgs e)
      {
         if (null == TopPanelTop)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBoxScaleTransform_TextChanged() TopPanel=null");
            return;
         }
         Canvas? canvas = null;
         foreach (UIElement ui0 in TopPanelTop.Children)
         {
            if (ui0 is DockPanel dockPanelInside)
            {
               dockPanelInside = (DockPanel)ui0;
               foreach (UIElement ui1 in dockPanelInside.Children)
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
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "MouseLeftButtonDown_Canvas() myScrollViewer=null");
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
         double amountToScroll = percentToScroll * myScrollViewer.ScrollableHeight;
         myScrollViewer.ScrollToVerticalOffset(amountToScroll);
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
         sb.Append(myScrollViewer.ScrollableHeight.ToString("####.#"));
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
          amountToScroll = percentToScroll * myScrollViewer.ScrollableWidth;
         myScrollViewer.ScrollToHorizontalOffset(amountToScroll);
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
