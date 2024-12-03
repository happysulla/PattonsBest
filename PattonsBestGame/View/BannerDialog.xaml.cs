using System.IO;
using System.Windows;
using System.Windows.Controls;
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
      private TextBlock myTextBlockDisplay;
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
         BitmapImage img = MapItem.theMapImages.GetBitmapImage("Parchment");
         ImageBrush brush = new ImageBrush(img);
         this.Background = brush;
         //-------------------------------
         Image imageAxes = new Image() { Source = MapItem.theMapImages.GetBitmapImage("CrossedAxes") };
         myButtonClose.Content = imageAxes;
         //-------------------------------
         myCheckBoxFont.IsChecked = theIsCheckBoxChecked;
         //-------------------------------
         try
         {
            XmlTextReader xr = new XmlTextReader(sr);
            myTextBlockDisplay = (TextBlock)XamlReader.Load(xr); // TextBox created in RuleManager.ShowRule()
            myScrollViewerTextBlock.Content = myTextBlockDisplay;
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
      private void BannerLoaded(object sender, EventArgs e)
      {
         myScrollViewerTextBlock.Height = myDockPanel.ActualHeight - myButtonClose.Height - 50;
         myTextBlockDisplay.Height = myTextBlockDisplay.ActualHeight;
      }
      private void ButtonClose_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         myIsDragging = true;
         myOffsetInBannerWindow = e.GetPosition(this);
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
      private void myCheckBoxFont_Unchecked(object sender, RoutedEventArgs e)
      {
         theIsCheckBoxChecked = false;
         myIsReopen = true;
         Close();
      }
      private void myCheckBoxFont_Click(object sender, RoutedEventArgs e)
      {
         theIsCheckBoxChecked = true;
         myIsReopen = true;
         Close();
      }
   }
}
