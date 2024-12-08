using System;
using System.Windows;
using System.Windows.Controls;
namespace Pattons_Best
{
   public partial class TerritoryVerifyDialog : Window
   {
      public String? RadioOutputText { get; set; }
      public String CenterPointX { get; set; } = "";
      public String CenterPointY { get; set; } = "";
      public bool IsTown { get; set; } = false;
      public bool IsCastle { get; set; } = false;
      public bool IsRuin { get; set; } = false;
      public bool IsTemple { get; set; } = false;
      public bool IsOasis { get; set; } = false;
      public TerritoryVerifyDialog(ITerritory t, double anX)
      {
         InitializeComponent();
         myTextBoxName.Text = t.Name;
         myTextBoxCenterPointX.Text = t.CenterPoint.X.ToString();
         myTextBoxCenterPointY.Text = t.CenterPoint.Y.ToString();
      }
      private void OkButton_Click(object sender, RoutedEventArgs e)
      {
         this.DialogResult = true;
      }
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
      private void CheckBox_Checked_IsTown(object sender, RoutedEventArgs e)
      {
         IsTown = true;
      }
      private void CheckBox_Checked_IsCastle(object sender, RoutedEventArgs e)
      {
         IsCastle = true;
      }
      private void CheckBox_Checked_IsRuin(object sender, RoutedEventArgs e)
      {
         IsRuin = true;
      }
      private void CheckBox_Checked_IsTemple(object sender, RoutedEventArgs e)
      {
         IsTemple = true;
      }
      private void CheckBox_Checked_IsOasis(object sender, RoutedEventArgs e)
      {
         IsOasis = true;
      }
      private void CheckBox_UnChecked_IsTown(object sender, RoutedEventArgs e)
      {
         IsTown = false;
      }
      private void CheckBox_UnChecked_IsCastle(object sender, RoutedEventArgs e)
      {
         IsCastle = false;
      }
      private void CheckBox_UnChecked_IsRuin(object sender, RoutedEventArgs e)
      {
         IsRuin = false;
      }
      private void CheckBox_UnChecked_IsTemple(object sender, RoutedEventArgs e)
      {
         IsTemple = false;
      }
      private void CheckBox_UnChecked_IsOasis(object sender, RoutedEventArgs e)
      {
         IsOasis = false;
      }
      private void TextBoxCenterPointX_TextChanged(object sender, TextChangedEventArgs e)
      {
         CenterPointX = myTextBoxCenterPointX.Text;
      }
      private void TextBoxCenterPointY_TextChanged(object sender, TextChangedEventArgs e)
      {
         CenterPointY = myTextBoxCenterPointY.Text;
      }
   }
}
