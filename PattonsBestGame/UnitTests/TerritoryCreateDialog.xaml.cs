using System;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
namespace Pattons_Best
{
   public partial class TerritoryCreateDialog : Window
   {
      private static string theTypeChecked = "A";
      private static string theCardChecked = "1";
      private static string theParentChecked = "Main";
      public String RadioButtonType { get; set; } = new string(String.Empty);
      public String RadioButtonParent { get; set; } = new string(String.Empty);
      public bool IsTown { get; set; } = false;
      public TerritoryCreateDialog()
      {
         InitializeComponent();
         UpdateView();
         foreach (UIElement ui in myStackPanelTankCard.Children)
         {
            if (ui is RadioButton rb)
               rb.Checked += RadioButtonType_Checked;
         }
         foreach (UIElement ui in myStackPanelTerritory.Children)
         {
            if (ui is RadioButton rb)
               rb.Checked += RadioButtonType_Checked;
         }
         myRadioButtonMain.Checked += RadioButtonParent_Checked;
         myRadioButtonTank.Checked += RadioButtonParent_Checked;
      }
      //-----------------------------------------------------
      void UpdateView()
      {
         if ("Tank" == theParentChecked)
         {
            myRadioButtonTank.IsChecked = true;
            myRadioButtonMain.IsChecked = false;
            foreach (UIElement ui in myStackPanelTankCard.Children)
            {
               if (ui is RadioButton rb)
               {
                  rb.Visibility = Visibility.Visible;
                  if (rb.Content is string)
                  {
                     string s = (string)rb.Content;
                     if (theCardChecked == s)
                        rb.IsChecked = true;
                  }
               }
            }
            foreach (UIElement ui in myStackPanelTerritory.Children)
            {
               if (ui is RadioButton rb)
                  rb.Visibility = Visibility.Hidden;
            }
         }
         else
         {
            myRadioButtonTank.IsChecked = false;
            myRadioButtonMain.IsChecked = true;
            foreach (UIElement ui in myStackPanelTerritory.Children)
            {
               if (ui is RadioButton rb)
               {
                  if (rb.Content is string)
                  {
                     rb.Visibility = Visibility.Visible;
                     string s = (string)rb.Content;
                     if (theTypeChecked == s)
                        rb.IsChecked = true;
                  }
               }
            }
            foreach (UIElement ui in myStackPanelTankCard.Children)
            {
               if (ui is RadioButton rb)
                  rb.Visibility = Visibility.Hidden;
            }
         }
      }
      //---------------CONTROLLER FUNCTIONS------------------
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
            if (true == radioButton.Content is String)
            {
               String output = (String)radioButton.Content;
               if (null != output)
               {
                  RadioButtonType = output;
                  if ("Tank" == theParentChecked)
                     theCardChecked = output;
                  else
                     theTypeChecked = output;
               }
            }
         }
         e.Handled = true;
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
            if (true == radioButton.Content is String)
            {
               String output = (String)radioButton.Content;
               if (null != output)
                  theParentChecked = RadioButtonParent = output;
            }
         }
         UpdateView();
         e.Handled = true;
      }
   }
}
