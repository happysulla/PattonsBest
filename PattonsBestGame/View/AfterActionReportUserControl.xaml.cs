﻿using System;
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
         if (true == myIsEditable)
         {
            mySpanTankName.IsEnabled = true;
            mySpanTankName.Background = theBrushInActive;
         }
         //----------------------------------
         s = AddSpaces(report.Model.ToString(), HEADER_INFO_LEN);
         mySpanTankModel.Inlines.Clear();
         mySpanTankModel.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(report.Situation.ToString(), HEADER_INFO_LEN);
         mySpanSituation.Inlines.Clear();
         mySpanSituation.Inlines.Add(new Run(s));
         //----------------------------------
         s = AddSpaces(report.Weather.ToString(), HEADER_INFO_LEN);
         mySpanWeather.Inlines.Clear();
         mySpanWeather.Inlines.Add(new Run(s));
         //----------------------------------
         myRunCommanderRank.Text = report.Commander.myRank.ToString();
         myRunCommanderRating.Text = report.Commander.myRating.ToString();
         s = AddSpaces(report.Commander.myName, CREW_NAME_LEN);
         mySpanCommanderName.Inlines.Clear();
         mySpanCommanderName.Inlines.Add(new Run(s));
         if (true == myIsEditable)
         {
            mySpanCommanderName.IsEnabled = true;
            mySpanCommanderName.Background = theBrushInActive;
         }
         //----------------------------------
         myRunGunnerRating.Text = report.Gunner.myRating.ToString();
         s = AddSpaces(report.Gunner.myName, CREW_NAME_LEN);
         mySpanGunnerName.Inlines.Clear();
         mySpanGunnerName.Inlines.Add(new Run(s));
         if (true == myIsEditable)
         {
            mySpanGunnerName.IsEnabled = true;
            mySpanGunnerName.Background = theBrushInActive;
         }
         //----------------------------------
         myRunLoaderRating.Text = report.Loader.myRating.ToString();
         s = AddSpaces(report.Loader.myName, CREW_NAME_LEN);
         mySpanLoaderName.Inlines.Clear();
         mySpanLoaderName.Inlines.Add(new Run(s));
         if (true == myIsEditable)
         {
            mySpanLoaderName.IsEnabled = true;
            mySpanLoaderName.Background = theBrushInActive;
         }
         //----------------------------------
         myRunDriverRating.Text = report.Driver.myRating.ToString();
         s = AddSpaces(report.Driver.myName, CREW_NAME_LEN);
         mySpanDriverName.Inlines.Clear();
         mySpanDriverName.Inlines.Add(new Run(s));
         if (true == myIsEditable)
         {
            mySpanDriverName.IsEnabled = true;
            mySpanDriverName.Background = theBrushInActive;
         }
         //----------------------------------
         myRunAssistantRating.Text = report.Assistant.myRating.ToString();
         s = AddSpaces(report.Assistant.myName, CREW_NAME_LEN);
         mySpanAssistantName.Inlines.Clear();
         mySpanAssistantName.Inlines.Add(new Run(s));
         if (true == myIsEditable)
         {
            mySpanAssistantName.IsEnabled = true;
            mySpanAssistantName.Background = theBrushInActive;
         }
         //----------------------------------
         myRunAmmo30Calibre.Text = report.Ammo30CalibreMG.ToString();
         myRunAmmo50Calibre.Text = report.Ammo50CalibreMG.ToString();
         myRunAmmoSmokeBombs.Text = report.AmmoSmokeBomb.ToString();
         myRunAmmoSmokeGrenades.Text = report.AmmoSmokeGrenade.ToString();
         myRunAmmoPeriscopes.Text = report.AmmoPeriscope.ToString();
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
      //-----------------------------------------------------------------------------------
      public string AddSpaces(string s, int length)
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
            CrewMember commander = Report.Commander;
            if (0 < mySpanCommanderName.Inlines.Count)
            {
               Inline inline = mySpanCommanderName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is TextBox textbox)
                     commander.myName = textbox.Text;
               }
            }
            mySpanCommanderName.Background = theBrushInActive;
            mySpanCommanderName.Inlines.Clear();
            String s = AddSpaces(commander.myName, CREW_NAME_LEN);
            mySpanCommanderName.Inlines.Clear();
            mySpanCommanderName.Inlines.Add(new Run(s));
         }
         if (true == myIsEditableGunnerName)
         {
            myIsEditableGunnerName = false;
            CrewMember gunner = Report.Gunner;
            if (0 < mySpanGunnerName.Inlines.Count)
            {
               Inline inline = mySpanGunnerName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is TextBox textbox)
                     gunner.myName = textbox.Text;
               }
            }
            mySpanGunnerName.Background = theBrushInActive;
            mySpanGunnerName.Inlines.Clear();
            String s = AddSpaces(gunner.myName, CREW_NAME_LEN);
            mySpanGunnerName.Inlines.Clear();
            mySpanGunnerName.Inlines.Add(new Run(s));
         }
         if (true == myIsEditableLoaderName)
         {
            myIsEditableLoaderName = false;
            CrewMember loader = Report.Loader;
            if (0 < mySpanLoaderName.Inlines.Count)
            {
               Inline inline = mySpanLoaderName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is TextBox textbox)
                     loader.myName = textbox.Text;
               }
            }
            mySpanLoaderName.Background = theBrushInActive;
            mySpanLoaderName.Inlines.Clear();
            String s = AddSpaces(loader.myName, CREW_NAME_LEN);
            mySpanLoaderName.Inlines.Clear();
            mySpanLoaderName.Inlines.Add(new Run(s));
         }
         if (true == myIsEditableDriverName)
         {
            myIsEditableDriverName = false;
            CrewMember driver = Report.Driver;
            if (0 < mySpanDriverName.Inlines.Count)
            {
               Inline inline = mySpanDriverName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is TextBox textbox)
                     driver.myName = textbox.Text;
               }
            }
            mySpanDriverName.Background = theBrushInActive;
            mySpanDriverName.Inlines.Clear();
            String s = AddSpaces(driver.myName, CREW_NAME_LEN);
            mySpanDriverName.Inlines.Clear();
            mySpanDriverName.Inlines.Add(new Run(s));
         }
         if (true == myIsEditableAssistantName)
         {
            myIsEditableAssistantName = false;
            CrewMember assistant = Report.Assistant;
            if (0 < mySpanAssistantName.Inlines.Count)
            {
               Inline inline = mySpanAssistantName.Inlines.FirstInline;
               if (inline is InlineUIContainer uiContainer)
               {
                  if (uiContainer.Child is TextBox textbox)
                     assistant.myName = textbox.Text;
               }
            }
            mySpanAssistantName.Background = theBrushInActive;
            mySpanAssistantName.Inlines.Clear();
            String s = AddSpaces(assistant.myName, CREW_NAME_LEN);
            mySpanAssistantName.Inlines.Clear();
            mySpanAssistantName.Inlines.Add(new Run(s));
         }
      }
   }
}
