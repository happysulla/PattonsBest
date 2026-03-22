using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PattonsBest
{
   public partial class ShowAboutDialog : Window
   {
      public delegate void EndShowAboutDialogCallback();
      private EndShowAboutDialogCallback myCallback;
      public bool CtorError { get; } = false;
      public ShowAboutDialog(EndShowAboutDialogCallback callback)
      {
         myCallback = callback;
         InitializeComponent();
         //--------------------------------------
         StringBuilder sb = new StringBuilder();
         sb.Append("Verson: ");
         Version? version = Assembly.GetExecutingAssembly().GetName().Version;
         if( null == version )
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowAboutDialog(): version=null");
            CtorError = true;
            return;
         }
         sb.Append(version.ToString());
         sb.Append("_");
         DateTime linkerTime = Utilities.GetBuildDate(Assembly.GetExecutingAssembly());
         sb.Append(linkerTime.ToString());
         myTextBox.Text = sb.ToString();  
      }

      //-------------------------------------------------------------------------------
      private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
      {
         try
         {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Hyperlink_RequestNavigate(): failed e.URI=" + e.Uri.ToString() + "\n" + ex.ToString());
         }
         e.Handled = true;
      }
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void Window_Closed(object sender, EventArgs e)
      {
         myCallback();
      }
   }
}
