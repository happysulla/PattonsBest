using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pattons_Best
{
   public partial class AfterActionReportUserControl : UserControl
   {
      public static int HEADER_INFO_LEN = 47;
      public static int CREW_NAME_LEN = 18;
      public bool CtorError { get; } = false;
      public IAfterActionReport Report { get; set; }
      public bool myIsEditable = false;
      private bool myIsEditableTankName = false;
      private bool myIsEditableCommanderName = false;
      private bool myIsEditableGunnerName = false;
      private bool myIsEditableLoaderName = false;
      private bool myIsEditableDriverName = false;
      private bool myIsEditableAssistantName = false;
      public static SolidColorBrush theBrushActive = new SolidColorBrush() { Color = Color.FromArgb(0xFF, 0xB9, 0xEA, 0x9E) };
      public static SolidColorBrush theBrushInActive = new SolidColorBrush() { Color = Colors.LightGray };
      private readonly FontFamily myFontFam0 = new FontFamily("Arial Rounded MT Bold");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------
      public AfterActionReportUserControl(IAfterActionReport report, bool isEditable = false)
      {
         InitializeComponent();
         myIsEditable = isEditable;
         Report = report;
         if (false == UpdateReport(report))
         {
            Logger.Log(LogEnum.LE_ERROR, "AfterActionReportUserControl(): UpdateReport() returned false");
            CtorError = true;
            return;
         }
      }
      public bool UpdateReport(IAfterActionReport report)
      {
         String s = AddSpaces(report.Day, HEADER_INFO_LEN);
         mySpanDate.Inlines.Clear();
         mySpanDate.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(report.Name, HEADER_INFO_LEN);
         mySpanTankName.Inlines.Clear();
         mySpanTankName.Inlines.Add(new Run(s));
         mySpanTankName.IsEnabled = myIsEditable;
         if (true == mySpanTankName.IsEnabled)
            mySpanTankName.Background = theBrushInActive;
         //----------------------------------
         TankCard card = new TankCard(report.TankCardNum);
         s = AddSpaces(card.myModel, HEADER_INFO_LEN);
         mySpanTankModel.Inlines.Clear();
         mySpanTankModel.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(report.Situation.ToString(), HEADER_INFO_LEN);
         mySpanSituation.Inlines.Clear();
         mySpanSituation.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(report.Weather, HEADER_INFO_LEN);
         mySpanWeather.Inlines.Clear();
         mySpanWeather.Inlines.Add(new Run(s));
         //----------------------------------
         ICrewMember? commander = report.Commander as ICrewMember;
         if( null == commander )
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): commander=null");
            return false;
         }
         myRunCommanderRank.Text = commander.Rank;
         myRunCommanderRating.Text = commander.Rating.ToString();
         s = AddSpaces(commander.Name, CREW_NAME_LEN);
         mySpanCommanderName.Inlines.Clear();
         mySpanCommanderName.Inlines.Add(new Run(s));
         mySpanCommanderName.IsEnabled = myIsEditable;
         if (true == mySpanCommanderName.IsEnabled)
            mySpanCommanderName.Background = theBrushInActive;
         //----------------------------------
         ICrewMember? gunner = report.Gunner as ICrewMember;
         if (null == gunner)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): gunner=null");
            return false;
         }
         myRunGunnerRating.Text = gunner.Rating.ToString();
         s = AddSpaces(gunner.Name, CREW_NAME_LEN);
         mySpanGunnerName.Inlines.Clear();
         mySpanGunnerName.Inlines.Add(new Run(s));
         mySpanGunnerName.IsEnabled = myIsEditable;
         if (true == mySpanGunnerName.IsEnabled)
            mySpanGunnerName.Background = theBrushInActive;
         //----------------------------------
         ICrewMember? loader = report.Loader as ICrewMember;
         if (null == loader)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): loader=null");
            return false;
         }
         myRunLoaderRating.Text = loader.Rating.ToString();
         s = AddSpaces(loader.Name, CREW_NAME_LEN);
         mySpanLoaderName.Inlines.Clear();
         mySpanLoaderName.Inlines.Add(new Run(s));
         mySpanLoaderName.IsEnabled = myIsEditable;
         if (true == mySpanLoaderName.IsEnabled)
            mySpanLoaderName.Background = theBrushInActive;
         //----------------------------------
         ICrewMember? driver = report.Driver as ICrewMember;
         if (null == driver)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): loader=null");
            return false;
         }
         myRunDriverRating.Text = driver.Rating.ToString();
         s = AddSpaces(driver.Name, CREW_NAME_LEN);
         mySpanDriverName.Inlines.Clear();
         mySpanDriverName.Inlines.Add(new Run(s));
         mySpanDriverName.IsEnabled = myIsEditable;
         if (true == mySpanDriverName.IsEnabled)
            mySpanDriverName.Background = theBrushInActive;
         //----------------------------------
         ICrewMember? assistant = report.Assistant as ICrewMember;
         if (null == assistant)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): assistant=null");
            return false;
         }
         myRunAssistantRating.Text = assistant.Rating.ToString();
         s = AddSpaces(assistant.Name, CREW_NAME_LEN);
         mySpanAssistantName.Inlines.Clear();
         mySpanAssistantName.Inlines.Add(new Run(s));
         mySpanAssistantName.IsEnabled = myIsEditable;
         if (true == mySpanAssistantName.IsEnabled)
            mySpanAssistantName.Background = theBrushInActive;
         //----------------------------------
         if( false == UpdateReportTimeTrack( report ))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): UpdateReportTimeTrack() returned false");
            return false;
         }
         //----------------------------------
         myRunAmmo30Calibre.Text = report.Ammo30CalibreMG.ToString();
         myRunAmmo50Calibre.Text = report.Ammo50CalibreMG.ToString();
         myRunAmmoSmokeBombs.Text = report.AmmoSmokeBomb.ToString();
         myRunAmmoSmokeGrenades.Text = report.AmmoSmokeGrenade.ToString();
         myRunAmmoPeriscopes.Text = report.AmmoPeriscope.ToString();
         //----------------------------------
         myRunMainGunHE.Text = report.MainGunHE.ToString();
         myRunMainGunAP.Text = report.MainGunAP.ToString();
         myRunMainGunWP.Text = report.MainGunWP.ToString();
         myRunMainGunHBCI.Text = report.MainGunHBCI.ToString();
         myRunMainGunHVAP.Text = report.MainGunHVAP.ToString();
         //----------------------------------
         myRunVictoryPointsLight.Text = report.VictoryPtsKiaLightWeapon.ToString();
         myRunVictoryPointsTruck.Text = report.VictoryPtsKiaTruck.ToString();
         myRunVictoryPointsSPW.Text = report.VictoryPtsKiaSpwOrPsw.ToString();
         myRunVictoryPointsSpGun.Text = report.VictoryPtsKiaSPGun.ToString();
         myRunVictoryPointsPzIV.Text = report.VictoryPtsKiaPzIV.ToString();
         myRunVictoryPointsPzV.Text = report.VictoryPtsKiaPzV.ToString();
         myRunVictoryPointsPzVI.Text = report.VictoryPtsKiaPzVI.ToString();
         myRunVictoryPointsAtGun.Text = report.VictoryPtsKiaAtGun.ToString();
         myRunVictoryPointsPosition.Text = report.VictoryPtsKiaFortifiedPosition.ToString();
         myRunVictoryPointsCaptureArea.Text = report.VictoryPtsCaptureArea.ToString();
         myRunVictoryPointsCaptureExit.Text = report.VictoryPtsKiaExitArea.ToString();
         myRunVictoryPointsLostTank.Text = report.VictoryPtsKiaExitArea.ToString();
         myRunVictoryPointsLostInfantry.Text = report.VictoryPtsFriendlyTank.ToString();
         //----------------------------------
         myRunVictoryPointsTotalTank.Text = report.VictoryPtsTotalTank.ToString();
         myRunVictoryPointsTotalFriendly.Text = report.VictoryPtsTotalFriendly.ToString();
         myRunVictoryPointsTotalTerritory.Text = report.VictoryPtsTotalTerritory.ToString();
         //----------------------------------
         return true;
      }
      private bool UpdateReportTimeTrack(IAfterActionReport report)
      {
         int hour = 5;
         foreach (UIElement ui0 in myGridTime.Children)
         {
            if(ui0 is StackPanel sp)
            {
               if( false == UpdateReportTimeTrackRow(sp, hour, report) )
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateReportTimeTrackRow(): returned false");
                  return false;
               }
               hour++;
            }
         }
         return true;
      }
      private bool UpdateReportTimeTrackRow(StackPanel sp, int hour, IAfterActionReport report)
      {
         int min = 0;
         foreach (UIElement ui in sp.Children)
         {
            Logger.Log(LogEnum.LE_VIEW_TIME_TRACK, "UpdateReportTimeTrackRow(): Updating " + sp.Name + " min=" + min.ToString());
            if (true == ui is Rectangle)
            {
               Rectangle? rect = ui as Rectangle;
               if (null == rect)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateReportTimeTrackRow(): Rectangle not found");
                  return false;
               }
               if (hour < report.SunriseHour )
               {
                  rect.Fill = Brushes.Black;
               }
               else if (hour == report.SunriseHour)
               {
                  if( min < report.SunriseMin)
                     rect.Fill = Brushes.Black;
               }
               else
               {
                  if( report.SunsetHour < hour )
                  {
                     rect.Fill = Brushes.Black;
                  }
                  else if ( hour == report.SunsetHour)
                  {
                     if (report.SunriseMin < min)
                        rect.Fill = Brushes.Black;
                  }
               }
            }
            min += 15;
         }
         return true;
      }
      private string AddSpaces(string s, int length)
      {
         if (length < s.Length)
         {
            s = s.Substring(0, length);
            return s;
         }
         StringBuilder sb = new StringBuilder();
         sb.Append(s);
         int count = length - s.Length;
         for (int i = 0; i < count; ++i)
            sb.Append('_');
         return sb.ToString();
      }
      //-------------------------CONTROLLER FUNCTIONS--------------------------------
      private void SpanTankName_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (false == myIsEditable)
            return;
         ResetTextBoxes();
         myIsEditableTankName = true;
         mySpanTankName.Inlines.Clear();
         mySpanTankName.Background = theBrushActive;
         string text = AddSpaces(" ", HEADER_INFO_LEN);
         TextBox textbox = new TextBox() { Background = theBrushActive, IsEnabled = true, Text = text, TextWrapping = TextWrapping.NoWrap, FontFamily = myFontFam0, Focusable = true };
         mySpanTankName.Inlines.Add(new InlineUIContainer(textbox));
         textbox.PreviewTextInput += OverwriteTextBox_PreviewTextInput;
         textbox.Loaded += OverwriteTextBox_Loaded;
         e.Handled = true;
      }
      private void SpanCommanderName_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (false == myIsEditable)
            return;
         ResetTextBoxes();
         myIsEditableCommanderName = true;
         mySpanCommanderName.Inlines.Clear();
         mySpanCommanderName.Background = theBrushActive;
         string text = AddSpaces(" ", CREW_NAME_LEN);
         TextBox textbox = new TextBox() { Background = theBrushActive, IsEnabled = true, Text = text, TextWrapping = TextWrapping.NoWrap, FontFamily = myFontFam0, Focusable = true };
         mySpanCommanderName.Inlines.Add(new InlineUIContainer(textbox));
         textbox.PreviewTextInput += OverwriteTextBox_PreviewTextInput;
         textbox.Loaded += OverwriteTextBox_Loaded;
         e.Handled = true;
      }
      private void SpanGunnerName_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (false == myIsEditable)
            return;
         ResetTextBoxes();
         myIsEditableGunnerName = true;
         mySpanGunnerName.Inlines.Clear();
         mySpanGunnerName.Background = theBrushActive;
         string text = AddSpaces(" ", CREW_NAME_LEN);
         TextBox textbox = new TextBox() { Background = theBrushActive, IsEnabled = true, Text = text, TextWrapping = TextWrapping.NoWrap, FontFamily = myFontFam0, Focusable = true };
         mySpanGunnerName.Inlines.Add(new InlineUIContainer(textbox));
         textbox.PreviewTextInput += OverwriteTextBox_PreviewTextInput;
         textbox.Loaded += OverwriteTextBox_Loaded;
         e.Handled = true;
      }
      private void SpanLoaderName_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (false == myIsEditable)
            return;
         ResetTextBoxes();
         myIsEditableLoaderName = true;
         mySpanLoaderName.Inlines.Clear();
         mySpanLoaderName.Background = theBrushActive;
         string text = AddSpaces(" ", CREW_NAME_LEN);
         TextBox textbox = new TextBox() { Background = theBrushActive, IsEnabled = true, Text = text, TextWrapping = TextWrapping.NoWrap, FontFamily = myFontFam0, Focusable = true };
         mySpanLoaderName.Inlines.Add(new InlineUIContainer(textbox));
         textbox.PreviewTextInput += OverwriteTextBox_PreviewTextInput;
         textbox.Loaded += OverwriteTextBox_Loaded;
         e.Handled = true;
      }
      private void SpanDriverName_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (false == myIsEditable)
            return;
         ResetTextBoxes();
         myIsEditableDriverName = true;
         mySpanDriverName.Inlines.Clear();
         mySpanDriverName.Background = theBrushActive;
         string text = AddSpaces(" ", CREW_NAME_LEN);
         TextBox textbox = new TextBox() { Background = theBrushActive, IsEnabled = true, Text = text, TextWrapping = TextWrapping.NoWrap, FontFamily = myFontFam0, Focusable = true };
         mySpanDriverName.Inlines.Add(new InlineUIContainer(textbox));
         textbox.PreviewTextInput += OverwriteTextBox_PreviewTextInput;
         textbox.Loaded += OverwriteTextBox_Loaded;
         e.Handled = true;
      }
      private void SpanAssistantName_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (false == myIsEditable)
            return;
         ResetTextBoxes();
         myIsEditableAssistantName = true;
         mySpanAssistantName.Inlines.Clear();
         mySpanAssistantName.Background = theBrushActive;
         string text = AddSpaces(" ", CREW_NAME_LEN);
         TextBox textbox = new TextBox() { Background = theBrushActive, IsEnabled = true, Text = text, TextWrapping = TextWrapping.NoWrap, FontFamily = myFontFam0, Focusable = true };
         mySpanAssistantName.Inlines.Add(new InlineUIContainer(textbox));
         textbox.PreviewTextInput += OverwriteTextBox_PreviewTextInput;
         textbox.Loaded += OverwriteTextBox_Loaded;
         e.Handled = true;
      }
      private void Window_MouseDown(object sender, MouseButtonEventArgs e)
      {
         ResetTextBoxes();
         e.Handled = true;
      }
      private void OverwriteTextBox_Loaded(object sender, RoutedEventArgs e)
      {
         if (null == sender)
            return;
         TextBox? textBox = sender as TextBox; // Need to put the focus on TextBox after it is loaded
         if (null == textBox)
            return;
         textBox.Focus();
         Keyboard.Focus(textBox);
         e.Handled = true;
      }
      private void OverwriteTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
      {
         if (null == sender)
            return;
         TextBox? textBox = sender as TextBox;
         if (null == textBox)
            return;
         string uppercase = e.Text;
         if( true == myIsEditableTankName)
            uppercase = e.Text.ToUpper();
         int caretIndex = textBox.CaretIndex;
         if (caretIndex < textBox.Text.Length)
            textBox.Text = textBox.Text.Remove(caretIndex, 1).Insert(caretIndex, uppercase);
         else
            textBox.Text = textBox.Text.Insert(caretIndex, uppercase);
         textBox.CaretIndex = caretIndex + 1;
         e.Handled = true;
      }
      private void ResetTextBoxes()
      {
         if (true == myIsEditableTankName)
         {
            myIsEditableTankName = false;
            if (0 < mySpanTankName.Inlines.Count)
            {
               Inline inline = mySpanTankName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is TextBox textbox)
                     Report.Name = textbox.Text;
               }
            }
            mySpanTankName.Background = theBrushInActive;
            mySpanTankName.Inlines.Clear();
            String s = AddSpaces(Report.Name, HEADER_INFO_LEN);
            mySpanTankName.Inlines.Clear();
            mySpanTankName.Inlines.Add(new Run(s));
         }
         if (true == myIsEditableCommanderName)
         {
            myIsEditableCommanderName = false;
            IMapItem commander = Report.Commander;
            if (0 < mySpanCommanderName.Inlines.Count)
            {
               Inline inline = mySpanCommanderName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is TextBox textbox)
                     commander.Name = textbox.Text;
               }
            }
            mySpanCommanderName.Background = theBrushInActive;
            mySpanCommanderName.Inlines.Clear();
            String s = AddSpaces(commander.Name, CREW_NAME_LEN);
            mySpanCommanderName.Inlines.Clear();
            mySpanCommanderName.Inlines.Add(new Run(s));
         }
         if (true == myIsEditableGunnerName)
         {
            myIsEditableGunnerName = false;
            IMapItem gunner = Report.Gunner;
            if (0 < mySpanGunnerName.Inlines.Count)
            {
               Inline inline = mySpanGunnerName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is TextBox textbox)
                     gunner.Name = textbox.Text;
               }
            }
            mySpanGunnerName.Background = theBrushInActive;
            mySpanGunnerName.Inlines.Clear();
            String s = AddSpaces(gunner.Name, CREW_NAME_LEN);
            mySpanGunnerName.Inlines.Clear();
            mySpanGunnerName.Inlines.Add(new Run(s));
         }
         if (true == myIsEditableLoaderName)
         {
            myIsEditableLoaderName = false;
            IMapItem loader = Report.Loader;
            if (0 < mySpanLoaderName.Inlines.Count)
            {
               Inline inline = mySpanLoaderName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is TextBox textbox)
                     loader.Name = textbox.Text;
               }
            }
            mySpanLoaderName.Background = theBrushInActive;
            mySpanLoaderName.Inlines.Clear();
            String s = AddSpaces(loader.Name, CREW_NAME_LEN);
            mySpanLoaderName.Inlines.Clear();
            mySpanLoaderName.Inlines.Add(new Run(s));
         }
         if (true == myIsEditableDriverName)
         {
            myIsEditableDriverName = false;
            IMapItem driver = Report.Driver;
            if (0 < mySpanDriverName.Inlines.Count)
            {
               Inline inline = mySpanDriverName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is TextBox textbox)
                     driver.Name = textbox.Text;
               }
            }
            mySpanDriverName.Background = theBrushInActive;
            mySpanDriverName.Inlines.Clear();
            String s = AddSpaces(driver.Name, CREW_NAME_LEN);
            mySpanDriverName.Inlines.Clear();
            mySpanDriverName.Inlines.Add(new Run(s));
         }
         if (true == myIsEditableAssistantName)
         {
            myIsEditableAssistantName = false;
            IMapItem assistant = Report.Assistant;
            if (0 < mySpanAssistantName.Inlines.Count)
            {
               Inline inline = mySpanAssistantName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is TextBox textbox)
                     assistant.Name = textbox.Text;
               }
            }
            mySpanAssistantName.Background = theBrushInActive;
            mySpanAssistantName.Inlines.Clear();
            String s = AddSpaces(assistant.Name, CREW_NAME_LEN);
            mySpanAssistantName.Inlines.Clear();
            mySpanAssistantName.Inlines.Add(new Run(s));
         }
      }
   }
}
