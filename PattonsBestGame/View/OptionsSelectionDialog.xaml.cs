using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Windows.System;



namespace Pattons_Best
{
   public partial class OptionsSelectionDialog : Window
   {
      public bool CtorError { get; }
      public IGameInstance? myGameInstance = null;
      private Options myOptions { get; set; } = new Options();
      public Options NewOptions { get => myOptions; }
      //---------------------------------------------
      public OptionsSelectionDialog(Options options)
      {
         Logger.Log(LogEnum.LE_VIEW_SHOW_OPTIONS, "OptionSelectionDialog(): " + options.ToString());
         myOptions = new Options();
         foreach( Option o in options )
         {
            Option option = new Option(o.Name, o.IsEnabled);
            myOptions.Add(option);
         }
         InitializeComponent();
         //-----------------------------
         myCheckBoxSkipOpening.ToolTip = "Skip opening screen with tank image. Contains button to begin game or read rules.";
         myCheckBoxSkipHistorical.ToolTip = "Skip screen with map of 4th division historical path through France and Germany.";
         myCheckBoxSkipMoveBoard.ToolTip = "Skip showing description of Movement Board. Introduces concept of what Movement Board looks like.";
         myCheckBoxPrinceBattleBoard.ToolTip = "Skip showing description of Battle Board. Introduces concept of what Battle Board looks like.";
         myCheckBoxPrinceTankCard.ToolTip = "Skip showing description of Tank Card. Introduces Tank Card boxes and starting tank model.";
         myCheckBoxStartAfterActionReport.ToolTip = "Skip showing description of After Action Report. Allows changing names of tank and crew names.";
         //-----------------------------
         if (false == UpdateDisplay(myOptions))
         {
            Logger.Log(LogEnum.LE_ERROR, "OptionSelectionDialog(): UpdateDisplay() returned false");
            CtorError = true;
         }
      }
      //----------------------------------
      private bool UpdateDisplay(Options options)
      {
         string name = "SkipTutorial0";
         Option? option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxSkipOpening.IsChecked = option.IsEnabled;
         //------------------------------
         name = "SkipTutorial1";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxSkipHistorical.IsChecked = option.IsEnabled;
         //------------------------------
         name = "SkipTutorial2";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxSkipMoveBoard.IsChecked = option.IsEnabled;
         //------------------------------
         name = "SkipTutorial3";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxPrinceBattleBoard.IsChecked = option.IsEnabled;
         //------------------------------
         name = "SkipTutorial4";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxPrinceTankCard.IsChecked = option.IsEnabled;
         //------------------------------
         name = "SkipTutorial5";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxStartAfterActionReport.IsChecked = option.IsEnabled;
         return true;
      }
      //----------------------CONTROLLER FUNCTIONS----------------------
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = true;
      }
      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void StackPanelTotural_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option? option = null;
         switch (cb.Name)
         {
            case "myCheckBoxSkipOpening":
               option = myOptions.Find("SkipTutorial0");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            case "myCheckBoxSkipHistorical":
               option = myOptions.Find("SkipTutorial1");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            case "myCheckBoxSkipMoveBoard":
               option = myOptions.Find("SkipTutorial2");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            case "myCheckBoxPrinceBattleBoard":
               option = myOptions.Find("SkipTutorial3");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            case "myCheckBoxPrinceTankCard":
               option = myOptions.Find("SkipTutorial4");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            case "myCheckBoxStartAfterActionReport":
               option = myOptions.Find("SkipTutorial5");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): reached default name=" + cb.Name); return;
         }
      }
   }
}
