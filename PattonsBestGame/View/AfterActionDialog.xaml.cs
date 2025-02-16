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

namespace Pattons_Best
{
   public partial class AfterActionDialog : Window
   {
      public bool CtorError { get; } = false;
      public IAfterActionReport Report { get; set; } = new AfterActionReport(new CombatCalenderEntry("07/27/43", EnumScenario.Advance, 3, EnumResistance.Light, "Corba Breakout"));
      //-------------------------------------------------------------------------------------
      public AfterActionDialog(IAfterActionReport report)
      {
         InitializeComponent();
         Title = "After Action Report for " + report.Day;
         //-------------------------------
         AfterActionReportUserControl userControl = new AfterActionReportUserControl(report);
         if (true == CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "AfterActionDialog(): AfterActionReportUserControl() error");
            CtorError = true;
            return;
         }
         myScrollViewerClient.Content = userControl;
         //-------------------------------
      }

   }
}
