using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
namespace Pattons_Best
{
   public partial class TerritoryVerifyDialog : Window
   {
      public String RadioOutputType { get; set; } = "ERROR";
      public String RadioOutputParent { get; set; } = "ERROR";
      public TerritoryVerifyDialog(ITerritory t)
      {
         InitializeComponent();
         myTextBoxName.Text = t.Name;
         switch (t.Type)
         {
            case "A": myRadioButtonA.IsChecked = true; break;
            case "B": myRadioButtonB.IsChecked = true; break;
            case "C": myRadioButtonC.IsChecked = true; break;
            case "D": myRadioButtonD.IsChecked = true; break;
            case "E": myRadioButtonE.IsChecked = true; break;
            case "Battle": myRadioButtonF.IsChecked = true; break;
            case "1": myRadioButton1.IsChecked = true; break;
            case "2": myRadioButton2.IsChecked = true; break;
            case "3": myRadioButton3.IsChecked = true; break;
            case "4": myRadioButton4.IsChecked = true; break;
            case "5": myRadioButton5.IsChecked = true; break;
            case "6": myRadioButton6.IsChecked = true; break;
            case "7": myRadioButton7.IsChecked = true; break;
            case "8": myRadioButton8.IsChecked = true; break;
            case "9": myRadioButton9.IsChecked = true; break;
            case "10": myRadioButton10.IsChecked = true; break;
            case "11": myRadioButton11.IsChecked = true; break;
            case "12": myRadioButton12.IsChecked = true; break;
            case "13": myRadioButton13.IsChecked = true; break;
            case "14": myRadioButton14.IsChecked = true; break;
            case "15": myRadioButton15.IsChecked = true; break;
            case "16": myRadioButton16.IsChecked = true; break;
            case "17": myRadioButton17.IsChecked = true; break;
            case "18": myRadioButton18.IsChecked = true; break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "TerritoryVerifyDialog(): unk type=" + t.Type);
               break;
         }
         if ("Main" == t.CanvasName)
            myRadioButtonMain.IsChecked = true;
         else
            myRadioButtonTank.IsChecked = true;
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
               RadioOutputType = (string)radioButton.Content;
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
   }
}
