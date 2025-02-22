using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Point = System.Windows.Point;

namespace Pattons_Best
{
   public partial class TableDialog : Window
   {
      private static SolidColorBrush theBrushOrange = new SolidColorBrush(Colors.Orange);
      public bool CtorError { get; } = false;
      private string myKey = "";
      public string Key { get => myKey; }
      private FlowDocument? myFlowDocumentContent = null;
      public FlowDocument? FlowDocumentContent { get => myFlowDocumentContent; }
      public TableDialog(string key, StringReader sr)
      {
         InitializeComponent();
         try
         {
            XmlTextReader xr = new XmlTextReader(sr);
            myFlowDocumentContent = (FlowDocument)XamlReader.Load(xr);
            myFlowDocumentScrollViewer.Document = myFlowDocumentContent;
            myKey = key;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, " e=" + e.ToString() + " sr.content=\n" + sr.ToString());
            CtorError = true;
            return;
         }
      }
      //-------------------------------------------------------------------------
      private void ButtonClose_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }

      private void TableDialog_Loaded(object sender, RoutedEventArgs e)
      {
         switch (Key)
         {
            case "Calendar":
               this.Title = "Combat Calendar";
               this.myFlowDocumentScrollViewer.Width = 1300;
               this.myFlowDocumentScrollViewer.Height = 900;
               break;
            case "Ammo":
               this.Title = "Ammo Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 620;
               this.MinHeight = this.MaxHeight = 400;
               break;
            case "Bail Out":
               this.Title = "Bail Out Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 310;
               this.MinHeight = this.MaxHeight = 220;
               break;
            case "Bogged Down":
               this.Title = "Bogged Down Movement Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 370;
               this.MinHeight = this.MaxHeight = 300;
               break;
            case "Brew Up":
               this.Title = "Brew Up Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 375;
               this.MinHeight = this.MaxHeight = 260;
               break;
            case "Collateral":
               this.Title = "Collateral Damage Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 470;
               this.MinHeight = this.MaxHeight = 380;
               break;
            case "Deployment":
               this.Title = "Deployment Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 540;
               this.MinHeight = this.MaxHeight = 400;
               break;
            case "Explosion":
               this.Title = "Tank Explosion Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 340;
               this.MinHeight = this.MaxHeight = 210;
               break;
            case "Hit Location":
               this.Title = "Hit Location Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 620;
               this.MinHeight = this.MaxHeight = 360;
               break;
            case "Minefield":
               this.Title = "Minefield Attack Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 810;
               this.MinHeight = this.MaxHeight = 930;
               break;
            case "Movement":
               this.Title = "Movement Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 810;
               this.MinHeight = this.MaxHeight = 930;
               break;
            case "Time":
               this.Title = "Time Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 620;
               this.MinHeight = this.MaxHeight = 610;
               break;
            case "Weather":
               this.Title = "Weather Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 700;
               this.MinHeight = this.MaxHeight = 710;
               break;
            case "Wounds":
               this.Title = "Wounds Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 660;
               this.MinHeight = this.MaxHeight = 510;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowTable(): reached default key=" + Key);
               break;
         }
      }
    }
}
