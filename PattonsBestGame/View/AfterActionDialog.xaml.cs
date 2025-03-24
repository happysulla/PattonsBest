using Pattons_Best.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.Graphics.Printing3D;

namespace Pattons_Best
{
   public partial class AfterActionDialog : Window
   {
      public delegate void EndAfterActionDialogCallback();
      public bool CtorError { get; } = false;
      private EndAfterActionDialogCallback myCallback;
      private AfterActionReportUserControl myAfterActionReportControl;
      //-------------------------------------------------------------------------------------
      public AfterActionDialog(IAfterActionReport report, EndAfterActionDialogCallback callback)
      { 
         myCallback = callback;
         InitializeComponent();
         Title = "After Action Report for " + report.Day;
         //-------------------------------
         myAfterActionReportControl = new AfterActionReportUserControl(report);
         if (true == CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "AfterActionDialog(): AfterActionReportUserControl() error");
            CtorError = true;
            return;
         }
         myScrollViewerClient.Content = myAfterActionReportControl;
      }
      public void UpdateReport()
      {
         myAfterActionReportControl.UpdateReport();
      }
      private void Window_Closed(object sender, EventArgs e)
      {
         myCallback();
      }
   }
}
