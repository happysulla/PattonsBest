using System;
using System.Windows;
using System.Windows.Controls;
using static Pattons_Best.Territory;
namespace Pattons_Best
{
   public partial class TerritoryVerifyDialog : Window
   {
      public String RadioOutputText { get; set; } = "ERROR";
      public String RadioOutputParent { get; set; } = "ERROR";
      public String CenterPointX { get; set; } = "";
      public String CenterPointY { get; set; } = "";
      public TerritoryVerifyDialog(ITerritory t, double anX)
      {
         InitializeComponent();
         myTextBoxName.Text = t.Name;
         myTextBoxCenterPointX.Text = t.CenterPoint.X.ToString("000");
         myTextBoxCenterPointY.Text = t.CenterPoint.Y.ToString("000");
         switch (t.Type)
         {
            case "A":
               myRadioButtonA.IsChecked = true;
               break;
            case "B":
               myRadioButtonB.IsChecked = true;
               break;
            case "C":
               myRadioButtonC.IsChecked = true;
               break;
            case "D":
               myRadioButtonD.IsChecked = true;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "TerritoryVerifyDialog(): unk type=" + t.Type);
               break;
         }
         switch (t.Parent)
         {
            case TerritoryEnum.Movement:
               myRadioButtonE.IsChecked = true;
               break;
            case TerritoryEnum.Tank:
               myRadioButtonF.IsChecked = true;
               break;
            case TerritoryEnum.Battle:
               myRadioButtonG.IsChecked = true;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "TerritoryVerifyDialog(): unk type=" + t.Type);
               break;
         }
      }
      private void OkButton_Click(object sender, RoutedEventArgs e)
      {
         this.DialogResult = true;
      }
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
               RadioOutputText = (string)radioButton.Content;
         }
      }
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
               RadioOutputParent = (string)radioButton.Content;
         }
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
