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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pattons_Best
{
   public partial class AfterActionReportDisplay : UserControl
   {
      public bool CtorError { get; } = false;
      public AfterActionReportDisplay(IAfterActionReport report)
      {
         InitializeComponent();
         if( false == DisplayReport(report))
         {
            CtorError = true;
            Logger.Log(LogEnum.LE_ERROR, "CanvasImageViewer(): c=null");
            return;
         }
      }
      private bool DisplayReport(IAfterActionReport report)
      {
         return true;
      }
   }
}
