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
   public partial class TableListingDialog : Window
   {
      public bool CtorError = false;
      private RuleDialogViewer? myRulesManager = null;
      public TableListingDialog(RuleDialogViewer rm)
      {
         InitializeComponent();
         if (null == rm)
         {
            Logger.Log(LogEnum.LE_ERROR, "RuleListingDialog(): rm=null");
            CtorError = true;
            return;
         }
         myRulesManager = rm;
         this.Title = "Table Listing";
      }
      //-------------------------------------------------------------------
      private void ButtonShowRule_Click(object sender, RoutedEventArgs e)
      {
         if (null == myRulesManager)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): myRulesManager=null");
            return;
         }
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (false == myRulesManager.ShowTable(key))
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowTable() returned false");
            return;
         }
      }
   }
}
