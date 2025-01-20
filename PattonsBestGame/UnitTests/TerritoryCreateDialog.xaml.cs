using System;
using System.Windows;
using System.Windows.Controls;
namespace Pattons_Best
{
   public partial class TerritoryCreateDialog : Window
   {
      public String? RadioOutputText { get; set; }
      public bool IsTown { get; set; } = false;
      public TerritoryCreateDialog()
      {
         InitializeComponent();
      }
      //------------------------------------------------
      private void OkButton_Click(object sender, RoutedEventArgs e)
      {
         this.DialogResult = true;
      }
      //------------------------------------------------
      private void RadioButton_Checked(object sender, RoutedEventArgs e)
      {
         RadioButton? radioButton = (RadioButton)sender;
         if (null == radioButton)
         {
            Logger.Log(LogEnum.LE_ERROR, "RadioButton_Checked(): radioButton=null");
         }
         else
         {
            if (null != radioButton.Content)
               RadioOutputText = radioButton.Content.ToString();
         }
      }
   }
}
