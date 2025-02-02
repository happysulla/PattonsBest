using System;
using System.Windows;
using System.Windows.Controls;
namespace Pattons_Best
{
   public partial class TerritoryCreateDialog : Window
   {
      public String? RadioButtonType { get; set; }
      public String? RadioButtonParent { get; set; }
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
      private void RadioButtonType_Checked(object sender, RoutedEventArgs e)
      {
         RadioButton? radioButton = (RadioButton)sender;
         if (null == radioButton)
         {
            Logger.Log(LogEnum.LE_ERROR, "RadioButton_Checked(): radioButton=null");
         }
         else
         {
            if (null != radioButton.Content)
               RadioButtonType = radioButton.Content.ToString();
         }
      }
      //------------------------------------------------
      private void RadioButtonParent_Checked(object sender, RoutedEventArgs e)
      {
         RadioButton? radioButton = (RadioButton)sender;
         if (null == radioButton)
         {
            Logger.Log(LogEnum.LE_ERROR, "RadioButton_Checked(): radioButton=null");
         }
         else
         {
            if (null != radioButton.Content)
               RadioButtonParent = radioButton.Content.ToString();
         }
      }
   }
}
