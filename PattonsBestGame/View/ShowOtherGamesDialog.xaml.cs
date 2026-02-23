using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Pattons_Best
{
   public partial class ShowOtherGamesDialog : Window
   {
      public bool CtorError { get; } = false;
      public ShowOtherGamesDialog()
      {
         InitializeComponent();
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
   }
}
