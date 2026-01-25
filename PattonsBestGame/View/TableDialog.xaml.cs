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
      private static SolidColorBrush theBrushBlue = new SolidColorBrush(Colors.LightBlue);
      private static SolidColorBrush theBrushGreen = new SolidColorBrush(Colors.LightGreen);
      private static SolidColorBrush theBrushTan = new SolidColorBrush(Colors.AntiqueWhite);
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
            case "Activation":
               this.Title = "Activation Tables";
               this.Background = theBrushTan;
               this.Width = this.MinWidth = this.MaxWidth = 470;
               this.MinHeight = this.MaxHeight = 510;
               break;
            case "Ammo":
               this.Title = "Ammo Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 620;
               this.MinHeight = this.MaxHeight = 400;
               break;
            case "AP To Kill (75)":
               this.Title = "AP To Kill (75)";
               this.Background = theBrushGreen;
               this.Width = this.MinWidth = this.MaxWidth = 680;
               this.MinHeight = this.MaxHeight = 510;
               break;
            case "AP To Kill (76L)":
               this.Title = "AP To Kill (76L)";
               this.Background = theBrushGreen;
               this.Width = this.MinWidth = this.MaxWidth = 680;
               this.MinHeight = this.MaxHeight = 510;
               break;
            case "AP To Kill (76LL)":
               this.Title = "AP To Kill (76LL)";
               this.Background = theBrushGreen;
               this.Width = this.MinWidth = this.MaxWidth = 680;
               this.MinHeight = this.MaxHeight = 510;
               break;
            case "Bail Out":
               this.Title = "Bail Out Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 310;
               this.MinHeight = this.MaxHeight = 230;
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
            case "Calendar":
               this.Title = "Combat Calendar";
               this.Width = this.MinWidth = this.MaxWidth = 1300;
               this.MinHeight = this.MaxHeight = 950;
               break;
            case "Collateral":
               this.Title = "Collateral Damage Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 470;
               this.MinHeight = this.MaxHeight = 380;
               break;
            case "Decorations":
               this.Title = "Decorations Table";
               this.Background = theBrushTan;
               this.Width = this.MinWidth = this.MaxWidth = 550;
               this.MinHeight = this.MaxHeight = 300;
               break;
            case "Deployment":
               this.Title = "Deployment Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 540;
               this.MinHeight = this.MaxHeight = 410;
               break;
            case "Enemy Advance":
               this.Title = "Enemy Action: Advance Scenario";
               this.Background = theBrushBlue;
               this.Width = this.MinWidth = this.MaxWidth = 710;
               this.MinHeight = this.MaxHeight = 810;
               break;
            case "Enemy AP To Hit":
               this.Title = "Enemy AP To Hit";
               this.Background = theBrushBlue;
               this.Width = this.MinWidth = this.MaxWidth = 550;
               this.MinHeight = this.MaxHeight = 460;
               break;
            case "Enemy AP To Kill":
               this.Title = "Enemy AP % To Kill";
               this.Background = theBrushBlue;
               this.Width = this.MinWidth = this.MaxWidth = 760;
               this.MinHeight = this.MaxHeight = 500;
               break;
            case "Enemy Appearance":
               this.Title = "Enemy Vehicle/Gun Appearance Table";
               this.Background = theBrushTan;
               this.Width = this.MinWidth = this.MaxWidth = 570;
               this.MinHeight = this.MaxHeight = 390;
               break;
            case "Enemy Battle":
               this.Title = "Enemy Action: Battle Scenario";
               this.Background = theBrushBlue;
               this.Width = this.MinWidth = this.MaxWidth = 575;
               this.MinHeight = this.MaxHeight = 820;
               break;
            case "Enemy Counterattack":
               this.Title = "Enemy Action: CounterAttack Scenario";
               this.Background = theBrushBlue;
               this.Width = this.MinWidth = this.MaxWidth = 570;
               this.MinHeight = this.MaxHeight = 740;
               break;
            case "Exit Areas":
               this.Title = "Exit Areas";
               this.Background = theBrushTan;
               this.Width = this.MinWidth = this.MaxWidth = 640;
               this.MinHeight = this.MaxHeight = 340;
               break;
            case "Explosion":
               this.Title = "Tank Explosion Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 340;
               this.MinHeight = this.MaxHeight = 210;
               break;
            case "Friendly Action":
               this.Title = "Friendly Action";
               this.Background = theBrushBlue;
               this.Width = this.MinWidth = this.MaxWidth = 550;
               this.MinHeight = this.MaxHeight = 700;
               break;
            case "Gun Malfunction":
               this.Title = "Gun Malfunction Repair Table";
               this.Background = theBrushGreen;
               this.Width = this.MinWidth = this.MaxWidth = 470;
               this.MinHeight = this.MaxHeight = 430;
               break;
            case "HE to Kill (75)":
               this.Title = "HE to Kill (75) Vehicles";
               this.Background = theBrushGreen;
               this.Width = this.MinWidth = this.MaxWidth = 680;
               this.MinHeight = this.MaxHeight = 510;
               break;
            case "HE to Kill (76)":
               this.Title = "HE to Kill (75) Vehicles";
               this.Background = theBrushGreen;
               this.Width = this.MinWidth = this.MaxWidth = 680;
               this.MinHeight = this.MaxHeight = 510;
               break;
            case "Hit Location Crew":
               this.Title = "Hit Location Crew Wound Effects";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 620;
               this.MinHeight = this.MaxHeight = 360;
               break;
            case "Hit Location":
               this.Title = "Hit Location Table";
               this.Background = theBrushGreen;
               this.Width = this.MinWidth = this.MaxWidth = 390;
               this.MinHeight = this.MaxHeight = 320;
               break;
            case "Hit Location Tank":
               this.Title = "Hit Location Table";
               this.Background = theBrushBlue;
               this.Width = this.MinWidth = this.MaxWidth = 390;
               this.MinHeight = this.MaxHeight = 320;
               break;
            case "Minefield":
               this.Title = "Minefield Attack Table";
               this.Background = theBrushBlue;
               this.Width = this.MinWidth = this.MaxWidth = 630;
               this.MinHeight = this.MaxHeight = 280;
               break;
            case "Movement":
               this.Title = "Movement Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 810;
               this.MinHeight = this.MaxHeight = 930;
               break;
            case "Panzerfaust":
               this.Title = "Panzerfaust Attack Tables";
               this.Background = theBrushBlue;
               this.Width = this.MinWidth = this.MaxWidth = 510;
               this.MinHeight = this.MaxHeight = 620;
               break;
            case "Placement":
               this.Title = "Battle Board Placement Tables";
               this.Background = theBrushTan;
               this.Width = this.MinWidth = this.MaxWidth = 790;
               this.MinHeight = this.MaxHeight = 630;
               break;
            case "Random Events":
               this.Title = "Random Events Table";
               this.Background = theBrushBlue;
               this.Width = this.MinWidth = this.MaxWidth = 670;
               this.MinHeight = this.MaxHeight = 560;
               break;
            case "Rate of Fire":
               this.Title = "Rate of Fire Table";
               this.Background = theBrushGreen;
               this.Width = this.MinWidth = this.MaxWidth = 460;
               this.MinHeight = this.MaxHeight = 350;
               break;
            case "Replacement":
               this.Title = "Tank Replacement Table";
               this.Background = theBrushTan;
               this.Width = this.MinWidth = this.MaxWidth = 720;
               this.MinHeight = this.MaxHeight = 450;
               break;
            case "Resistance":
               this.Title = "Resistance Table";
               this.Background = theBrushTan;
               this.Width = this.MinWidth = this.MaxWidth = 470;
               this.MinHeight = this.MaxHeight = 270;
               break;
            case "Sherman MG":
               this.Title = "Sherman Machine Guns vs Infantry Targets";
               this.Background = theBrushGreen;
               this.Width = this.MinWidth = this.MaxWidth = 560;
               this.MinHeight = this.MaxHeight = 650;
               break;
            case "Spotting":
               this.Title = "Spotting Table";
               this.Background = theBrushTan;
               this.Width = this.MinWidth = this.MaxWidth = 660;
               this.MinHeight = this.MaxHeight = 460;
               break;
            case "Time":
               this.Title = "Time Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 630;
               this.MinHeight = this.MaxHeight = 620;
               break;
            case "To Hit Target":
               this.Title = "To Hit Target";
               this.Background = theBrushGreen;
               this.Width = this.MinWidth = this.MaxWidth = 600;
               this.MinHeight = this.MaxHeight = 770;
               break;
            case "To Kill Infantry":
               this.Title = "To Kill Infantry Targets";
               this.Background = theBrushGreen;
               this.Width = this.MinWidth = this.MaxWidth = 600;
               this.MinHeight = this.MaxHeight = 380;
               break;
            case "Weather":
               this.Title = "Weather Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 700;
               this.MinHeight = this.MaxHeight = 730;
               break;
            case "Wounds":
               this.Title = "Wounds Tables";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 660;
               this.MinHeight = this.MaxHeight = 510;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "TableDialog_Loaded(): reached default key=" + Key);
               break;
         }
      }
    }
}
