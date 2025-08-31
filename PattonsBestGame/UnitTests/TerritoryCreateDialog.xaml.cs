using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Pattons_Best
{
   public partial class TerritoryCreateDialog : Window
   {
      public static string theTypeChecked = "A";
      public static string theCardChecked = "1";
      public static string theParentChecked = "Main";
      private Canvas? myCanvasMain = null;
      private Canvas? myCanvasTank = null;
      public TerritoryCreateDialog(Canvas? main, Canvas? tank)
      {
         if (null == main)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryVerifyDialog(): main=null");
            return;
         }
         myCanvasMain = main;
         if (null == tank)
         {
            Logger.Log(LogEnum.LE_ERROR, "TerritoryVerifyDialog(): tank=null");
            return;
         }
         myCanvasTank = tank;
         //-----------------------------------------
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
      void UpdateTankCard(int tankNum)
      {
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateTankCard(): myCanvasTank=null");
            return;
         }
         if (theCardChecked != tankNum.ToString())
         {
            //----------------------------------
            List<UIElement> elements = new List<UIElement>();
            foreach (UIElement ui in myCanvasTank.Children) // Clean the Canvas of all marks
            {
               if (ui is System.Windows.Shapes.Ellipse ellipse)
                  elements.Add(ui);
               if (ui is Image img)
                  elements.Add(ui);
            }
            foreach (UIElement ui1 in elements)
               myCanvasTank.Children.Remove(ui1);
            //----------------------------------
            theCardChecked = tankNum.ToString();
            Image imageTank = new Image() { Name = "TankMat", Width = 600, Height = 500, Stretch = Stretch.Fill };
            switch (tankNum)
            {
               case 1: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m01"); break;
               case 2: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m02"); break;
               case 3: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m03"); break;
               case 4: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m04"); break;
               case 5: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m05"); break;
               case 6: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m06"); break;
               case 7: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m07"); break;
               case 8: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m08"); break;
               case 9: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m09"); break;
               case 10: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m10"); break;
               case 11: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m11"); break;
               case 12: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m12"); break;
               case 13: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m13"); break;
               case 14: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m14"); break;
               case 15: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m15"); break;
               case 16: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m16"); break;
               case 17: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m17"); break;
               case 18: imageTank.Source = MapItem.theMapImages.GetBitmapImage("m18"); break;
               default: Logger.Log(LogEnum.LE_ERROR, "UpdateTankCard(): reached default tankNum=" + tankNum.ToString()); return;
            }
            myCanvasTank.Children.Add(imageTank);
            Canvas.SetLeft(imageTank, 0);
            Canvas.SetTop(imageTank, 0);
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
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "RadioButton_Checked(): myCanvasMain=null");
            return;
         }
         RadioButton? radioButton = (RadioButton)sender;
         if (null == radioButton)
         {
            Logger.Log(LogEnum.LE_ERROR, "RadioButton_Checked(): radioButton=null");
            return;
         }
         if (null == radioButton.Content)
         {
            Logger.Log(LogEnum.LE_ERROR, "RadioButton_Checked(): radioButton=null");
            return;
         }
         string content = (string)radioButton.Content;
         theTypeChecked = content;
         switch (content)
         {
            case "A":
            case "B":
            case "C":
            case "D":
            case "E":
            case "R":
            case "S":
            case "T":
            case "Battle":
               break;
            default:
               int tankNum = Int32.Parse(content);
               UpdateTankCard(tankNum);
               break;
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
            if (true == radioButton.Content is String)
            {
               String output = (String)radioButton.Content;
               if (null != output)
                  theParentChecked = output;
            }
         }
         UpdateView();
         e.Handled = true;
      }
   }
}
