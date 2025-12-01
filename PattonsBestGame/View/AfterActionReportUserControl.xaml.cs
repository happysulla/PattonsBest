
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using TextBox = System.Windows.Controls.TextBox;

namespace Pattons_Best
{
   public partial class AfterActionReportUserControl : UserControl
   {
      public static int HEADER_INFO_LEN = 47;
      public static int CREW_NAME_LEN = 18;
      public static int END_TIME_LEN = 15;
      public static int DISABLED_TANK_LEN = 15;
      public static int KO_TANK_LEN = 15;
      public bool CtorError { get; } = false;
      public IAfterActionReport Report { get; set; } = new AfterActionReport();
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
      public AfterActionReportUserControl(IGameInstance gi, bool isEditable = false)
      {
         InitializeComponent();
         myIsEditable = isEditable;
         if (false == UpdateReport(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "AfterActionReportUserControl(): UpdateReport() returned false");
            CtorError = true;
            return;
         }
      }
      public bool UpdateReport(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): lastReport=null");
            return false;
         }
         String s = AddSpaces(lastReport.Day, HEADER_INFO_LEN);
         mySpanDate.Inlines.Clear();
         mySpanDate.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(lastReport.Name, HEADER_INFO_LEN);
         mySpanTankName.Inlines.Clear();
         mySpanTankName.Inlines.Add(new Run(s));
         mySpanTankName.IsEnabled = myIsEditable;
         if (true == mySpanTankName.IsEnabled)
            mySpanTankName.Background = theBrushInActive;
         //----------------------------------
         TankCard card = new TankCard(lastReport.TankCardNum);
         string model = card.myChasis + "-" + card.myTurret;
         s = AddSpaces(model, HEADER_INFO_LEN);
         mySpanTankModel.Inlines.Clear();
         mySpanTankModel.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(lastReport.Scenario.ToString(), HEADER_INFO_LEN);
         mySpanSituation.Inlines.Clear();
         mySpanSituation.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(lastReport.Weather, HEADER_INFO_LEN);
         mySpanWeather.Inlines.Clear();
         mySpanWeather.Inlines.Add(new Run(s));
         //----------------------------------
         ICrewMember? commander = lastReport.Commander as ICrewMember;
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
         if (false == gi.SetCrewActionTerritory(commander))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): SetCrewMemberTerritory() returned false");
            return false;
         }
         //----------------------------------
         ICrewMember? gunner = lastReport.Gunner as ICrewMember;
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
         if (false == gi.SetCrewActionTerritory(gunner))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): SetCrewMemberTerritory() returned false");
            return false;
         }
         //----------------------------------
         ICrewMember? loader = lastReport.Loader as ICrewMember;
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
         if (false == gi.SetCrewActionTerritory(loader))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): SetCrewMemberTerritory() returned false");
            return false;
         }
         //----------------------------------
         ICrewMember? driver = lastReport.Driver as ICrewMember;
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
         if (false == gi.SetCrewActionTerritory(driver))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): SetCrewMemberTerritory() returned false");
            return false;
         }
         //----------------------------------
         ICrewMember? assistant = lastReport.Assistant as ICrewMember;
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
         if (false == gi.SetCrewActionTerritory(assistant))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): SetCrewMemberTerritory() returned false");
            return false;
         }
         //----------------------------------
         if ( false == UpdateReportTimeTrack(lastReport)) // Reset Time Rectangles 
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): UpdateReportTimeTrack() returned false");
            return false;
         }
         //----------------------------------
         myRunAmmo30Calibre.Text = lastReport.Ammo30CalibreMG.ToString();
         myRunAmmo50Calibre.Text = lastReport.Ammo50CalibreMG.ToString();
         myRunAmmoSmokeBombs.Text = lastReport.AmmoSmokeBomb.ToString();
         myRunAmmoSmokeGrenades.Text = lastReport.AmmoSmokeGrenade.ToString();
         myRunAmmoPeriscopes.Text = lastReport.AmmoPeriscope.ToString();
         //----------------------------------
         myRunMainGunHE.Text = lastReport.MainGunHE.ToString();
         myRunMainGunAP.Text = lastReport.MainGunAP.ToString();
         myRunMainGunWP.Text = lastReport.MainGunWP.ToString();
         myRunMainGunHBCI.Text = lastReport.MainGunHBCI.ToString();
         myRunMainGunHVAP.Text = lastReport.MainGunHVAP.ToString();
         //----------------------------------
         int pointTotal = lastReport.VictoryPtsYourKiaLightWeapon + lastReport.VictoryPtsFriendlyKiaLightWeapon;
         myRunVictoryPointsLight.Text = pointTotal.ToString();
         pointTotal = lastReport.VictoryPtsYourKiaTruck + lastReport.VictoryPtsFriendlyKiaTruck;
         myRunVictoryPointsTruck.Text = pointTotal.ToString();
         pointTotal = lastReport.VictoryPtsYourKiaSpwOrPsw + lastReport.VictoryPtsFriendlyKiaSpwOrPsw;
         myRunVictoryPointsSPW.Text = pointTotal.ToString();
         pointTotal = lastReport.VictoryPtsYourKiaSPGun + lastReport.VictoryPtsFriendlyKiaSPGun;
         myRunVictoryPointsSpGun.Text = pointTotal.ToString();
         pointTotal = lastReport.VictoryPtsYourKiaPzIV + lastReport.VictoryPtsFriendlyKiaPzIV;
         myRunVictoryPointsPzIV.Text = pointTotal.ToString();
         pointTotal = lastReport.VictoryPtsYourKiaPzV + lastReport.VictoryPtsFriendlyKiaPzV;
         myRunVictoryPointsPzV.Text = pointTotal.ToString();
         pointTotal = lastReport.VictoryPtsYourKiaPzVI + lastReport.VictoryPtsFriendlyKiaPzVI;
         myRunVictoryPointsPzVI.Text = pointTotal.ToString();
         pointTotal = lastReport.VictoryPtsYourKiaAtGun + lastReport.VictoryPtsFriendlyKiaAtGun;
         myRunVictoryPointsAtGun.Text = pointTotal.ToString();
         pointTotal = lastReport.VictoryPtsYourKiaFortifiedPosition + lastReport.VictoryPtsFriendlyKiaFortifiedPosition;
         myRunVictoryPointsPosition.Text = pointTotal.ToString();
         //----------------------------------
         myRunVictoryPointsCaptureArea.Text = lastReport.VictoryPtsCaptureArea.ToString();
         myRunVictoryPointsCaptureExit.Text = lastReport.VictoryPtsCapturedExitArea.ToString();
         //----------------------------------
         myRunVictoryPointsLostTank.Text = lastReport.VictoryPtsFriendlyTank.ToString();
         myRunVictoryPointsLostInfantry.Text = lastReport.VictoryPtsFriendlySquad.ToString();
         //----------------------------------
         myRunVictoryPointsTotalTank.Text = lastReport.VictoryPtsTotalYourTank.ToString();
         //----------------------------------
         myRunVictoryPointsTotalFriendly.Text = lastReport.VictoryPtsTotalFriendlyForces.ToString();
         //----------------------------------
         myRunVictoryPointsTotalTerritory.Text = lastReport.VictoryPtsTotalTerritory.ToString();
         //----------------------------------
         StringBuilder sb = new StringBuilder();
         foreach (String note in lastReport.Notes)
         {
            sb.Append(note);
            sb.Append("\n");
         }
         myTextBlockDisplay.Text = sb.ToString();
         //----------------------------------
         if( true == String.IsNullOrEmpty(lastReport.DayEndedTime))
         {
            mySpanTimeEnded.Inlines.Clear();
            mySpanTimeEnded.Inlines.Add(new Run("_______________"));
            mySpanBreakdown.Inlines.Clear();
            mySpanBreakdown.Inlines.Add(new Run("_______________"));
            mySpanKnockedOut.Inlines.Clear();
            mySpanKnockedOut.Inlines.Add(new Run("_______________"));
         }
         else
         {
            s = AddSpaces(lastReport.DayEndedTime, END_TIME_LEN);
            mySpanTimeEnded.Inlines.Clear();
            mySpanTimeEnded.Inlines.Add(new Run(s));
            s = AddSpaces(lastReport.Breakdown, DISABLED_TANK_LEN);
            mySpanBreakdown.Inlines.Clear();
            mySpanBreakdown.Inlines.Add(new Run(s));
            s = AddSpaces(lastReport.KnockedOut, KO_TANK_LEN);
            mySpanKnockedOut.Inlines.Clear();
            mySpanKnockedOut.Inlines.Add(new Run(s));
         }
         return true;
      }
      private bool UpdateReportTimeTrack(IAfterActionReport report)
      {
         foreach (UIElement ui0 in myGridTime.Children)  // Clear all rectangles
         {
            if (ui0 is StackPanel sp)
            {
               foreach (UIElement ui1 in sp.Children)
               {
                  if (ui1 is Rectangle rect)
                     rect.Fill = Brushes.Transparent;
               }
            }
         }
         //-----------------------------------------------
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
         myIsEditableCommanderName = false;
         myIsEditableGunnerName = false;
         myIsEditableLoaderName = false;
         myIsEditableDriverName = false;
         myIsEditableAssistantName = false;
         mySpanTankName.Inlines.Clear();
         mySpanTankName.Background = theBrushActive;
         string text = AddSpaces(" ", HEADER_INFO_LEN);
         TextBox textbox = new TextBox() { Background = theBrushActive, IsEnabled = true, Text = text, TextWrapping = TextWrapping.NoWrap, FontFamily = myFontFam0, Focusable = true };
         mySpanTankName.Inlines.Add(new InlineUIContainer(textbox));
         textbox.PreviewTextInput += OverwriteTextBox_PreviewTextInput;
         textbox.Loaded += OverwriteTextBox_Loaded;
         textbox.LostFocus += TextBox_LostFocus;
         e.Handled = true;
      }
      private void SpanCommanderName_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (false == myIsEditable)
            return;
         ResetTextBoxes();
         myIsEditableTankName = false;
         myIsEditableCommanderName = true;
         myIsEditableGunnerName = false;
         myIsEditableLoaderName = false;
         myIsEditableDriverName = false;
         myIsEditableAssistantName = false;
         mySpanCommanderName.Inlines.Clear();
         mySpanCommanderName.Background = theBrushActive;
         string text = AddSpaces(" ", CREW_NAME_LEN);
         TextBox textbox = new TextBox() { Background = theBrushActive, IsEnabled = true, Text = text, TextWrapping = TextWrapping.NoWrap, FontFamily = myFontFam0, Focusable = true };
         mySpanCommanderName.Inlines.Add(new InlineUIContainer(textbox));
         textbox.PreviewTextInput += OverwriteTextBox_PreviewTextInput;
         textbox.Loaded += OverwriteTextBox_Loaded;
         textbox.LostFocus += TextBox_LostFocus;
         e.Handled = true;
      }
      private void SpanGunnerName_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (false == myIsEditable)
            return;
         ResetTextBoxes();
         myIsEditableTankName = false;
         myIsEditableCommanderName = false;
         myIsEditableGunnerName = true;
         myIsEditableLoaderName = false;
         myIsEditableDriverName = false;
         myIsEditableAssistantName = false;
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
         myIsEditableTankName = false;
         myIsEditableCommanderName = false;
         myIsEditableGunnerName = false;
         myIsEditableLoaderName = true;
         myIsEditableDriverName = false;
         myIsEditableAssistantName = false;
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
         myIsEditableTankName = false;
         myIsEditableCommanderName = false;
         myIsEditableGunnerName = false;
         myIsEditableLoaderName = false;
         myIsEditableDriverName = true;
         myIsEditableAssistantName = false;
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
         myIsEditableTankName = false;
         myIsEditableCommanderName = false;
         myIsEditableGunnerName = false;
         myIsEditableLoaderName = false;
         myIsEditableDriverName = false;
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
         myIsEditableTankName = false;
         myIsEditableCommanderName = false;
         myIsEditableGunnerName = false;
         myIsEditableLoaderName = false;
         myIsEditableDriverName = false;
         myIsEditableAssistantName = false;
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
      private void TextBox_LostFocus(object sender, RoutedEventArgs e) 
      {
         if( null == sender )
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReport(): lastReport=null");
            return;
         }
         ResetTextBoxes();
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
                  if (uiContainer.Child is System.Windows.Controls.TextBox textbox)
                  {
                     string name = Utilities.RemoveSpaces(textbox.Text);
                     string trimString = name.Trim(new char[] { ' ', '*', '_' });
                     if ( false == String.IsNullOrEmpty(trimString) )
                        Report.Name = trimString;
                     else
                        textbox.Text = Report.Name;
                  }
               }
            }
            mySpanTankName.Background = theBrushInActive;
            mySpanTankName.Inlines.Clear();
            String s = AddSpaces(Report.Name, HEADER_INFO_LEN);
            mySpanTankName.Inlines.Clear();
            mySpanTankName.Inlines.Add(new Run(s));
         }
         //-------------------------------------------
         if (true == myIsEditableCommanderName)
         {
            myIsEditableCommanderName = false;
            IMapItem commander = Report.Commander;
            if (0 < mySpanCommanderName.Inlines.Count)
            {
               Inline inline = mySpanCommanderName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is System.Windows.Controls.TextBox textbox)
                  {
                     string name = Utilities.RemoveSpaces(textbox.Text);
                     string trimString = name.Trim(new char[] { ' ', '*', '_' });
                     if (false == String.IsNullOrEmpty(trimString))
                        commander.Name = trimString;
                     else
                        textbox.Text = commander.Name;
                  }
               }
            }
            mySpanCommanderName.Background = theBrushInActive;
            mySpanCommanderName.Inlines.Clear();
            String s = AddSpaces(commander.Name, CREW_NAME_LEN);
            mySpanCommanderName.Inlines.Clear();
            mySpanCommanderName.Inlines.Add(new Run(s));
         }
         //-------------------------------------------
         if (true == myIsEditableGunnerName)
         {
            myIsEditableGunnerName = false;
            IMapItem gunner = Report.Gunner;
            if (0 < mySpanGunnerName.Inlines.Count)
            {
               Inline inline = mySpanGunnerName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is System.Windows.Controls.TextBox textbox)
                  {
                     string name = Utilities.RemoveSpaces(textbox.Text);
                     string trimString = name.Trim(new char[] { ' ', '*', '_' });
                     if (false == String.IsNullOrEmpty(trimString))
                        gunner.Name = trimString;
                     else
                        textbox.Text = gunner.Name;
                  }
               }
            }
            mySpanGunnerName.Background = theBrushInActive;
            mySpanGunnerName.Inlines.Clear();
            String s = AddSpaces(gunner.Name, CREW_NAME_LEN);
            mySpanGunnerName.Inlines.Clear();
            mySpanGunnerName.Inlines.Add(new Run(s));
         }
         //-------------------------------------------
         if (true == myIsEditableLoaderName)
         {
            myIsEditableLoaderName = false;
            IMapItem loader = Report.Loader;
            if (0 < mySpanLoaderName.Inlines.Count)
            {
               Inline inline = mySpanLoaderName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is System.Windows.Controls.TextBox textbox)
                  {
                     string name = Utilities.RemoveSpaces(textbox.Text);
                     string trimString = name.Trim(new char[] { ' ', '*', '_' });
                     if (false == String.IsNullOrEmpty(trimString))
                        loader.Name = trimString;
                     else
                        textbox.Text = loader.Name;
                  }
               }
            }
            mySpanLoaderName.Background = theBrushInActive;
            mySpanLoaderName.Inlines.Clear();
            String s = AddSpaces(loader.Name, CREW_NAME_LEN);
            mySpanLoaderName.Inlines.Clear();
            mySpanLoaderName.Inlines.Add(new Run(s));
         }
         //-------------------------------------------
         if (true == myIsEditableDriverName)
         {
            myIsEditableDriverName = false;
            IMapItem driver = Report.Driver;
            if (0 < mySpanDriverName.Inlines.Count)
            {
               Inline inline = mySpanDriverName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is System.Windows.Controls.TextBox textbox)
                  {
                     string name = Utilities.RemoveSpaces(textbox.Text);
                     string trimString = name.Trim(new char[] { ' ', '*', '_' });
                     if (false == String.IsNullOrEmpty(trimString))
                        driver.Name = trimString;
                     else
                        textbox.Text = driver.Name;
                  }

               }
            }
            mySpanDriverName.Background = theBrushInActive;
            mySpanDriverName.Inlines.Clear();
            String s = AddSpaces(driver.Name, CREW_NAME_LEN);
            mySpanDriverName.Inlines.Clear();
            mySpanDriverName.Inlines.Add(new Run(s));
         }
         //-------------------------------------------
         if (true == myIsEditableAssistantName)
         {
            myIsEditableAssistantName = false;
            IMapItem assistant = Report.Assistant;
            if (0 < mySpanAssistantName.Inlines.Count)
            {
               Inline inline = mySpanAssistantName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is System.Windows.Controls.TextBox textbox)
                  {
                     string name = Utilities.RemoveSpaces(textbox.Text);
                     string trimString = name.Trim(new char[] { ' ', '*', '_' });
                     if (false == String.IsNullOrEmpty(trimString))
                        assistant.Name = trimString;
                     else
                        textbox.Text = assistant.Name;
                  }
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
