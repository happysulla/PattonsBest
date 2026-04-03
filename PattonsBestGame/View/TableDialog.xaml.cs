using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Point = System.Windows.Point;

namespace PattonsBest
{
   public partial class TableDialog : Window
   {
      private static double HEADER_PLUS_SCROLL_HEIGHT = 50;
      public static double SCROLL_WIDTH = 20;
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
         double heightOfConcern = 0.0;
         double widthOfConcern = 0.0;
         switch (Key)
         {
            case "Activation":
               this.Title = "Activation Tables";
               this.Background = theBrushTan;
               heightOfConcern = 460.0;
               widthOfConcern = 450.0;
               break;
            case "Ammo":
               this.Title = "Ammo Tables";
               this.Background = theBrushOrange;
               heightOfConcern = 370.0;
               widthOfConcern = 610.0;
               break;
            case "AP To Kill (75)":
               this.Title = "AP To Kill (75)";
               this.Background = theBrushGreen;
               heightOfConcern = 450.0;
               widthOfConcern = 670.0;
               break;
            case "AP To Kill (76L)":
               this.Title = "AP To Kill (76L)";
               this.Background = theBrushGreen;
               heightOfConcern = 450.0;
               widthOfConcern = 670.0;
               break;
            case "AP To Kill (76LL)":
               this.Title = "AP To Kill (76LL)";
               this.Background = theBrushGreen;
               heightOfConcern = 420.0;
               widthOfConcern = 670.0;
               break;
            case "Bail Out":
               this.Title = "Bail Out Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 310;
               this.MinHeight = this.MaxHeight = 230;
               heightOfConcern = 200.0;
               widthOfConcern = 300.0;
               break;
            case "Bogged Down":
               this.Title = "Bogged Down Movement Table";
               this.Background = theBrushOrange;
               heightOfConcern = 250.0;
               widthOfConcern = 350.0;
               break;
            case "Brew Up":
               this.Title = "Brew Up Table";
               this.Background = theBrushOrange;
               heightOfConcern = 210.0;
               widthOfConcern = 365.0;
               break;
            case "Calendar":
               this.Title = "Combat Calendar";
               heightOfConcern = 880.0;
               widthOfConcern = 1280.0;
               break;
            case "Collateral":
               this.Title = "Collateral Damage Table";
               this.Background = theBrushOrange;
               this.Width = this.MinWidth = this.MaxWidth = 470;
               this.MinHeight = this.MaxHeight = 380;
               heightOfConcern = 350.0;
               widthOfConcern = 460.0;
               break;
            case "Decorations":
               this.Title = "Decorations Table";
               this.Background = theBrushTan;
               heightOfConcern = 250.0;
               widthOfConcern = 530.0;
               break;
            case "Deployment":
               this.Title = "Deployment Tables";
               this.Background = theBrushOrange;
               heightOfConcern = 365.0;
               widthOfConcern = 530.0;
               break;
            case "Enemy Advance":
               this.Title = "Enemy Action: Advance Scenario";
               this.Background = theBrushBlue;
               heightOfConcern = 770.0;
               widthOfConcern = 690.0;
               break;
            case "Enemy AP To Hit":
               this.Title = "Enemy AP To Hit";
               this.Background = theBrushBlue;
               heightOfConcern = 430.0;
               widthOfConcern = 530.0;
               break;
            case "Enemy AP To Kill":
               this.Title = "Enemy AP % To Kill";
               this.Background = theBrushBlue;
               heightOfConcern = 440.0;
               widthOfConcern = 750.0;
               break;
            case "Enemy Appearance":
               this.Title = "Enemy Vehicle/Gun Appearance Table";
               this.Background = theBrushTan;
               heightOfConcern = 350.0;
               widthOfConcern = 560.0;
               break;
            case "Enemy Battle":
               this.Title = "Enemy Action: Battle Scenario";
               this.Background = theBrushBlue;
               heightOfConcern = 770.0;
               widthOfConcern = 550.0;
               break;
            case "Enemy Counterattack":
               this.Title = "Enemy Action: CounterAttack Scenario";
               this.Background = theBrushBlue;
               heightOfConcern = 690.0;
               widthOfConcern = 550.0;
               break;
            case "Exit Areas":
               this.Title = "Exit Areas";
               this.Background = theBrushTan;
               heightOfConcern = 310.0;
               widthOfConcern = 630.0;
               break;
            case "Explosion":
               this.Title = "Tank Explosion Table";
               this.Background = theBrushOrange;
               heightOfConcern = 160.0;
               widthOfConcern = 320.0;
               break;
            case "Friendly Action":
               this.Title = "Friendly Action";
               this.Background = theBrushBlue;
               heightOfConcern = 650.0;
               widthOfConcern = 530.0;
               break;
            case "Gun Malfunction":
               this.Title = "Gun Malfunction Repair Table";
               this.Background = theBrushGreen;
               heightOfConcern = 380.0;
               widthOfConcern = 450.0;
               break;
            case "HE To Kill (75)":
               this.Title = "HE To Kill (75) Vehicles";
               this.Background = theBrushGreen;
               heightOfConcern = 430.0;
               widthOfConcern = 670.0;
               break;
            case "HE To Kill (76)":
               this.Title = "HE To Kill (76) Vehicles";
               this.Background = theBrushGreen;
               heightOfConcern = 430.0;
               widthOfConcern = 670.0;
               break;
            case "Hit Location Crew":
               this.Title = "Hit Location Crew Wound Effects";
               this.Background = theBrushOrange;
               heightOfConcern = 320.0;
               widthOfConcern = 610.0;
               break;
            case "Hit Location":
               this.Title = "Hit Location Table";
               this.Background = theBrushGreen;
               heightOfConcern = 280.0;
               widthOfConcern = 370.0;
               break;
            case "Hit Location Tank":
               this.Title = "Hit Location Table";
               this.Background = theBrushBlue;
               heightOfConcern = 275.0;
               widthOfConcern = 370.0;
               break;
            case "Minefield":
               this.Title = "Minefield Attack Table";
               this.Background = theBrushBlue;
               heightOfConcern = 230.0;
               widthOfConcern = 600.0;
               break;
            case "Movement":
               this.Title = "Movement Tables";
               this.Background = theBrushOrange;
               heightOfConcern = 880.0;
               widthOfConcern = 795.0;
               break;
            case "Panzerfaust":
               this.Title = "Panzerfaust Attack Tables";
               this.Background = theBrushBlue;
               heightOfConcern = 570.0;
               widthOfConcern = 490.0;
               break;
            case "Placement":
               this.Title = "Battle Board Placement Tables";
               this.Background = theBrushTan;
               heightOfConcern = 590.0;
               widthOfConcern = 770.0;
               break;
            case "Random Events":
               this.Title = "Random Events Table";
               this.Background = theBrushBlue;
               heightOfConcern = 510.0;
               widthOfConcern = 650.0;
               break;
            case "Rate of Fire":
               this.Title = "Rate of Fire Table";
               this.Background = theBrushGreen;
               heightOfConcern = 300.0;
               widthOfConcern = 440.0;
               break;
            case "Replacement":
               this.Title = "Tank Replacement Table";
               this.Background = theBrushTan;
               heightOfConcern = 420.0;
               widthOfConcern = 710.0;
               break;
            case "Resistance":
               this.Title = "Resistance Table";
               this.Background = theBrushTan;
               heightOfConcern = 240.0;
               widthOfConcern = 460.0;
               break;
            case "Sherman MG":
               this.Title = "Sherman Machine Guns vs Infantry Targets";
               this.Background = theBrushGreen;
               heightOfConcern = 610.0;
               widthOfConcern = 540.0;
               break;
            case "Spotting":
               this.Title = "Spotting Table";
               this.Background = theBrushTan;
               heightOfConcern = 410.0;
               widthOfConcern = 650.0;
               break;
            case "Time":
               this.Title = "Time Tables";
               this.Background = theBrushOrange;
               heightOfConcern = 580.0;
               widthOfConcern = 620.0;
               break;
            case "To Hit Target":
               this.Title = "To Hit Target";
               this.Background = theBrushGreen;
               heightOfConcern = 730.0;
               widthOfConcern = 590.0;
               break;
            case "To Kill Infantry":
               this.Title = "To Kill Infantry Targets";
               this.Background = theBrushGreen;
               heightOfConcern = 340.0;
               widthOfConcern = 580.0;
               break;
            case "Weather":
               this.Title = "Weather Tables";
               this.Background = theBrushOrange;
               heightOfConcern = 690.0;
               widthOfConcern = 680.0;
               break;
            case "Wounds":
               this.Title = "Wounds Tables";
               this.Background = theBrushOrange;
               heightOfConcern = 460.0;
               widthOfConcern = 630.0;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "TableDialog_Loaded(): reached default key=" + Key);
               break;
         }
         myFlowDocumentScrollViewer.MinHeight = myFlowDocumentScrollViewer.Height = heightOfConcern;
         myFlowDocumentScrollViewer.MinWidth = myFlowDocumentScrollViewer.Width = widthOfConcern;
         myScrollViewer.Height = heightOfConcern;
         myScrollViewer.Width = widthOfConcern;
         this.Height = Math.Min(heightOfConcern + HEADER_PLUS_SCROLL_HEIGHT, System.Windows.SystemParameters.PrimaryScreenWidth);
         this.Width = Math.Min(widthOfConcern + SCROLL_WIDTH, System.Windows.SystemParameters.PrimaryScreenWidth);
         this.MinHeight = heightOfConcern * 0.5;
         this.MinWidth = widthOfConcern * 0.5;
         this.MaxHeight = Math.Min(3 * heightOfConcern, System.Windows.SystemParameters.PrimaryScreenWidth);
         this.MaxWidth = Math.Min(3 * widthOfConcern, System.Windows.SystemParameters.PrimaryScreenWidth);
      }
      private void TableDialog_ContentRendered(object sender, EventArgs e)
      {
      }
      private void TableDialog_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         myScrollViewer.Height = this.ActualHeight- HEADER_PLUS_SCROLL_HEIGHT;
         myScrollViewer.Width = this.ActualWidth- SCROLL_WIDTH;
      }
   }
}
