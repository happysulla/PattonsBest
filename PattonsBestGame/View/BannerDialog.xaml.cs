using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Pattons_Best
{
   public partial class BannerDialog : System.Windows.Window
   {
      public bool CtorError { get; } = false;
      private string myKey = "";
      public string Key { get => myKey; }
      private TextBlock myTextBlockDisplay = new TextBlock();
      public TextBlock TextBoxDiplay { get => myTextBlockDisplay; }
      public static bool theIsCheckBoxChecked = false;
      private bool myIsReopen = false;
      public bool IsReopen { get => myIsReopen; }
      //------------------------------------
      private bool myIsDragging = false;
      private System.Windows.Point myOffsetInBannerWindow;
      //-------------------------------------------------------------------------------------
      public BannerDialog(string key, StringReader sr)
      {
         InitializeComponent();
         myIsReopen = false; // Tell parent to reopen on font change
         BitmapImage? img = MapItem.theMapImages.GetBitmapImage("Parchment");
         if( null == img)
         {
            Logger.Log(LogEnum.LE_ERROR, "BannerDialog(): GetBitmapImage(Parchment) return null");
            CtorError = true;
            return;
         }
         ImageBrush brush = new ImageBrush(img);
         this.Background = brush;
         //-------------------------------
         Image imageRifles = new Image() { Source = MapItem.theMapImages.GetBitmapImage("CrossedRifles") };
         myButtonClose.Content = imageRifles;
         //-------------------------------
         myCheckBoxFont.IsChecked = theIsCheckBoxChecked;
         //-------------------------------
         try
         {
            XmlTextReader xr = new XmlTextReader(sr);
            myTextBlockDisplay = (TextBlock)XamlReader.Load(xr); // TextBox created in RuleManager.ShowRule()
            foreach (Inline inline in myTextBlockDisplay.Inlines)
            {
               if (inline is InlineUIContainer)
               {
                  InlineUIContainer ui = (InlineUIContainer)inline;
                  if (ui.Child is Image img1)
                  {
                     string imageName = img1.Name;
                     if (true == img1.Name.Contains("Continue"))
                        imageName = "Continue";
                     string fullImagePath = MapImage.theImageDirectory + Utilities.RemoveSpaces(imageName) + ".gif";
                     System.Windows.Media.Imaging.BitmapImage bitImage = new BitmapImage();
                     bitImage.BeginInit();
                     bitImage.UriSource = new Uri(fullImagePath, UriKind.Absolute);
                     bitImage.EndInit();
                     img1.Source = bitImage;
                  }
               }
            }
            myScrollViewerBanner.Content = myTextBlockDisplay;
            myTextBlockDisplay.MouseLeftButtonDown += Window_MouseLeftButtonDown;
            myTextBlockDisplay.MouseLeave += TextBlockDisplay_MouseLeave;
            myKey = key;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "BannerDialog(): e=" + e.ToString() + "  for key=" + key);
            CtorError = true;
            return;
         }
      }
      //-------------------------------------------------------------------------
      private void BannerLoaded(object sender, System.EventArgs e)
      {
         myScrollViewerBanner.Height = myDockPanel.ActualHeight - myButtonClose.Height - 50;
         myTextBlockDisplay.Height = myTextBlockDisplay.ActualHeight;
      }
      private void ButtonClose_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         //var currentMonitor = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle); // Get the current monitor the main window is on.
         //var source = PresentationSource.FromVisual(this); // Find out if our WPF app is being scaled by the monitor
         //double dpiScaling = (source != null && source.CompositionTarget != null ? source.CompositionTarget.TransformFromDevice.M11 : 1);
         //System.Drawing.Rectangle workArea = currentMonitor.WorkingArea; // Get the available area of the monitor
         //var workAreaWidth = (int)Math.Floor(workArea.Width * dpiScaling);
         //var workAreaHeight = (int)Math.Floor(workArea.Height * dpiScaling);
         //this.Left = (((workAreaWidth - (this.Width * dpiScaling)) / 2) + (workArea.Left * dpiScaling)); // Move the window to the center by setting the top and left coordinates.
         //this.Top = (((workAreaHeight - (this.Height * dpiScaling)) / 2) + (workArea.Top * dpiScaling));
         //-------------------------------
         myOffsetInBannerWindow = e.GetPosition(this);
         myOffsetInBannerWindow = new Point(100, 100);
         myIsDragging = true;
         e.Handled = true;
      }
      private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (false == myIsDragging)
         {
            base.OnMouseMove(e);
            return;
         }
         System.Windows.Point newPoint1 = this.PointToScreen(e.GetPosition(this));
         this.Left = newPoint1.X - myOffsetInBannerWindow.X;
         this.Top = newPoint1.Y - myOffsetInBannerWindow.Y;
         e.Handled = true;
      }
      private void Window_MouseUp(object sender, MouseButtonEventArgs e)
      {
         myIsDragging = false;
      }
      private void TextBlockDisplay_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
      {
         myIsDragging = false;
      }
      private void myCheckBoxFont_Clicked(object sender, RoutedEventArgs e)
      {
         theIsCheckBoxChecked = !theIsCheckBoxChecked;
         myIsReopen = true;
         Close();
      }
   }
}
