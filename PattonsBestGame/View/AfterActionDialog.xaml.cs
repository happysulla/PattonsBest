
using System;
using System.Windows;
namespace Pattons_Best
{
   public partial class AfterActionDialog : Window
   {
      public delegate void EndAfterActionDialogCallback();
      public bool CtorError { get; } = false;
      private EndAfterActionDialogCallback myCallback;
      private AfterActionReportUserControl myAfterActionReportControl;
      //-------------------------------------------------------------------------------------
      public AfterActionDialog(IGameInstance gi, EndAfterActionDialogCallback callback)
      { 
         myCallback = callback;
         InitializeComponent();
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "AfterActionDialog(): lastReport=null");
            CtorError = true;
            return;
         }
         Title = "After Action Report for " + lastReport.Day;
         //-------------------------------
         myAfterActionReportControl = new AfterActionReportUserControl(gi);
         if (true == CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "AfterActionDialog(): AfterActionReportUserControl() error");
            CtorError = true;
            return;
         }
         myScrollViewerClient.Content = myAfterActionReportControl;
      }
      public void UpdateReport(IGameInstance gi)
      {
         myAfterActionReportControl.UpdateReport(gi);
      }
      private void Window_Closed(object sender, EventArgs e)
      {
         myCallback();
      }
   }
}
