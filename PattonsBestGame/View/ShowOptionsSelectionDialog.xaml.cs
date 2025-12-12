using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Windows.System;



namespace Pattons_Best
{
   public partial class ShowOptionsSelectionDialog : Window
   {
      public bool CtorError { get; }
      public IGameInstance? myGameInstance = null;
      private Options myOptions { get; set; } = new Options();
      public Options NewOptions { get => myOptions; }
      //---------------------------------------------
      public ShowOptionsSelectionDialog(Options options)
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
         myRadioButtonOriginal.ToolTip = "Original game with original rules.";
         myRadioButtonGeneralv25No3.ToolTip = "Rules from General magazine, version 25, Issue 3. Magazine provided with this distribution under '<user>/AppData/Roaming/PattonBest/Docs' folder.";
         myRadioButtonOther.ToolTip = "Game creator added rules to make game more appealing.";
         myRadioButtonCustom.ToolTip = "Custom combination of General v25 Issue3 rules and other rules.";
         //-----------------------------
         myRadioButtonSingleDay.ToolTip = "Game only last one day. You are given the option what tank you want to use.";
         myRadioButtonCampaign.ToolTip = "Game last from June 1944 to April 1945 highlighting the 4th armor movement through Europe.";
         myCheckBoxEndOnCmdrDeath.ToolTip = "Game ends with a loss when commander dies.";
         //-----------------------------
         myCheckBoxSkipOpening.ToolTip = "Skip opening screen with tank image. Contains button to begin game or read rules.";
         myCheckBoxSkipHistorical.ToolTip = "Skip screen with map of 4th division historical path through France and Germany.";
         myCheckBoxSkipMoveBoard.ToolTip = "Skip showing description of Movement Board. Introduces concept of what Movement Board looks like.";
         myCheckBoxPrinceBattleBoard.ToolTip = "Skip showing description of Battle Board. Introduces concept of what Battle Board looks like.";
         myCheckBoxPrinceTankCard.ToolTip = "Skip showing description of Tank Card. Introduces Tank Card boxes and starting tank model.";
         myCheckBoxStartAfterActionReport.ToolTip = "Skip showing description of After Action Report. Allows changing names of tank and crew names.";
         //-----------------------------
         myCheckBoxAutoRollRatings.ToolTip = "Roll a single die and automatically roll all other die for new crew ratings.";
         myCheckBoxAutoRollAmmoLoad.ToolTip = "Automatically roll die rolls for Ammo Loads. Setup ammo with default levels.";
         myCheckBoxAutoRollAmmoLoad.ToolTip = "Automatically roll die rolls for Ammo Loads. Setup ammo with default levels.";
         myCheckBoxAutoPreparation.ToolTip = "Use the same setup from previous battle preparations. Only enabled if previous setup performed, and all similar conditions (such as uninjuried crewmen) exists.";
         myCheckBoxAutoRollEnemySetup.ToolTip = "Automatically roll die rolls for sector, terrain, and facing when activating an enemy.";
         //-----------------------------
         myCheckBoxAirInterdiction.ToolTip = "Air strikes were rarely used to lay smoke. If that result is rolled, replaced with no enemy movement due to interdiction.";
         myCheckBoxTerrainPointValue.ToolTip = "Increase incentive for attacking strategic areas: Area A = 1 point, Area B = 1 point, Area C = 3 points, Area D = 2 points.";
         myCheckBoxTankCoverArc.ToolTip = "Ingore 'First Shot +10' die roll modifier (DRM). Use +10 DRM when enemy is side facing. Use +20 DRM when using rear facing.";
         myCheckBoxSlowTransverseCoverArc.ToolTip = "Tiger and King Tiger add additional +15 DRM since they are slow turrent-transverse AFVs.";
         myCheckBoxSpgCoverArc.ToolTip = "Self Propelled Guns (SPGs) is ingored if the current facing is side or rear.";
         myCheckBoxAtgCoverArc.ToolTip = "Anti-Tank Guns (ATGs) using facings equivalent to AFVs. Pake38/Pak40 add +25 to change facing. Pak43 are on 360-degree mounts and only add +10 dice roll modifer per sector.";
         //-----------------------------
         myCheckBoxContinueMove.ToolTip = "Enemy units tend to move in straight line. Increase chances that same movement order occurs in consecutive moves.";
         myCheckBoxRearFacingAfterMoveAway.ToolTip = "Enemy units when retreating have increase chances of rear facing.";
         myCheckBoxIncreasedMove.ToolTip = "Ever consecutive turn the Sherman attempts to move, increase chance it does move.";
         myCheckBoxEnhancedMgFire.ToolTip = "Increase the MG fire effectivity against close and medium range targets.";
         myCheckBoxMgSurpressingFire.ToolTip = "Sherman MG fire on LWs/MGs reduce their fire effectiveness and cause them to not move if called on to move.";
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
         bool isAllGeneralChecked = true;
         bool isAllOtherChecked = true;
         bool isAtLeastOneGeneralChecked = false;
         bool isAtLeastOneOtherChecked = false;
         //------------------------------
         Option option = myOptions.Find("SingleDayScenario");
         if( true == option.IsEnabled)
         {
            myRadioButtonSingleDay.IsChecked = true;
            myRadioButtonCampaign.IsChecked = false;
         }
         else
         {
            myRadioButtonSingleDay.IsChecked = false;
            myRadioButtonCampaign.IsChecked = true;
         }
         option = options.Find("GameEndsOnCommanderDeath");
         myCheckBoxEndOnCmdrDeath.IsChecked = option.IsEnabled;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         option = options.Find("SkipTutorial0");
         myCheckBoxSkipOpening.IsChecked = option.IsEnabled;
         option = options.Find("SkipTutorial1");
         myCheckBoxSkipHistorical.IsChecked = option.IsEnabled;
         option = options.Find("SkipTutorial2");
         myCheckBoxSkipMoveBoard.IsChecked = option.IsEnabled;
         option = options.Find("SkipTutorial3");
         myCheckBoxPrinceBattleBoard.IsChecked = option.IsEnabled;
         option = options.Find("SkipTutorial4");
         myCheckBoxPrinceTankCard.IsChecked = option.IsEnabled;
         option = options.Find("SkipTutorial5");
         myCheckBoxStartAfterActionReport.IsChecked = option.IsEnabled;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         option = options.Find("AutoRollNewMembers");
         myCheckBoxAutoRollRatings.IsChecked = option.IsEnabled;
         option = options.Find("AutoRollAmmoLoad");
         myCheckBoxAutoRollAmmoLoad.IsChecked = option.IsEnabled;
         option = options.Find("AutoPreparation");
         myCheckBoxAutoPreparation.IsChecked = option.IsEnabled;
         option = options.Find("AutoRollEnemyActivation");
         myCheckBoxAutoRollEnemySetup.IsChecked = option.IsEnabled;
         option = options.Find("AutoRollBowMgFire");
         myCheckBoxAutoRollBowMg.IsChecked = option.IsEnabled;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         option = options.Find("AirInterdiction");
         myCheckBoxAirInterdiction.IsChecked = option.IsEnabled;
         if (false == option.IsEnabled)
            isAllGeneralChecked = false;
         else
            isAtLeastOneGeneralChecked = true;
         option = options.Find("TerrainPointValue");
         myCheckBoxTerrainPointValue.IsChecked = option.IsEnabled;
         if (false == option.IsEnabled)
            isAllGeneralChecked = false;
         else
            isAtLeastOneGeneralChecked = true;
         option = options.Find("TankCoveredArc");
         myCheckBoxTankCoverArc.IsChecked = option.IsEnabled;
         if (false == option.IsEnabled)
            isAllGeneralChecked = false;
         else
            isAtLeastOneGeneralChecked = true;
         option = options.Find("SlowTransverseCoveredArc");
         myCheckBoxSlowTransverseCoverArc.IsChecked = option.IsEnabled;
         if (false == option.IsEnabled)
            isAllGeneralChecked = false;
         else
            isAtLeastOneGeneralChecked = true;
         option = options.Find("SpgCoveredArc");
         myCheckBoxSpgCoverArc.IsChecked = option.IsEnabled;
         if (false == option.IsEnabled)
            isAllGeneralChecked = false;
         else
            isAtLeastOneGeneralChecked = true;
         option = options.Find("AtgCoveredArc");
         myCheckBoxAtgCoverArc.IsChecked = option.IsEnabled;
         if (false == option.IsEnabled)
            isAllGeneralChecked = false;
         else
            isAtLeastOneGeneralChecked = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         option = options.Find("EnemyContinueMove");
         myCheckBoxContinueMove.IsChecked = option.IsEnabled;
         if (false == option.IsEnabled)
            isAllOtherChecked = false;
         else
            isAtLeastOneOtherChecked = true;
         option = options.Find("EnemyRearFacingOnMove");
         myCheckBoxRearFacingAfterMoveAway.IsChecked = option.IsEnabled;
         if (false == option.IsEnabled)
            isAllOtherChecked = false;
         else
            isAtLeastOneOtherChecked = true;
         option = options.Find("ShermanIncreaseMoveChances");
         myCheckBoxIncreasedMove.IsChecked = option.IsEnabled;
         if (false == option.IsEnabled)
            isAllOtherChecked = false;
         else
            isAtLeastOneOtherChecked = true;
         option = options.Find("ShermanEnhanceMgFire");
         myCheckBoxEnhancedMgFire.IsChecked = option.IsEnabled;
         if (false == option.IsEnabled)
            isAllOtherChecked = false;
         else
            isAtLeastOneOtherChecked = true;
         option = options.Find("ShermanSurpressingMgFire");
         myCheckBoxMgSurpressingFire.IsChecked = option.IsEnabled;
         if (false == option.IsEnabled)
            isAllOtherChecked = false;
         else
            isAtLeastOneOtherChecked = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Summary Selection
         if ( (false == isAtLeastOneGeneralChecked) && (false == isAtLeastOneOtherChecked) && (false == isAllGeneralChecked) && (false == isAllOtherChecked) )
         {
            option = options.Find("OriginalGame");
            option.IsEnabled = true;
            myRadioButtonOriginal.IsChecked = true;
            myRadioButtonGeneralv25No3.IsChecked = false;
            myRadioButtonOther.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         else if ((true == isAllGeneralChecked) && (false == isAllOtherChecked) && (false == isAtLeastOneOtherChecked) )
         {
            option = options.Find("Generalv25No3");
            option.IsEnabled = true;
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonGeneralv25No3.IsChecked = true;
            myRadioButtonOther.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         else if ((true == isAllGeneralChecked) &&  (true == isAllOtherChecked))
         {
            option = options.Find("OtherGame");
            option.IsEnabled = true;
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonGeneralv25No3.IsChecked = false;
            myRadioButtonOther.IsChecked = true;
            myRadioButtonCustom.IsChecked = false;
         }
         else
         {
            option = options.Find("CustomGame");
            option.IsEnabled = true;
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonGeneralv25No3.IsChecked = false;
            myRadioButtonOther.IsChecked = false;
            myRadioButtonCustom.IsChecked = true;
         }
         return true;
      }
      //----------------------------------
      private void ResetGameType()
      {
         Option option = myOptions.Find("OriginalGame");
         option.IsEnabled = false;
         option = myOptions.Find("Generalv25No3Game");
         option.IsEnabled = false;
         option = myOptions.Find("OtherGame");
         option.IsEnabled = false;
         option = myOptions.Find("CustomGame");
         option.IsEnabled = false;
      }
      private void ResetGameGeneralV25No3Options( bool value=false )
      {
         Option option = myOptions.Find("AirInterdiction");
         option.IsEnabled = value;
         option = myOptions.Find("TerrainPointValue");
         option.IsEnabled = value;
         option = myOptions.Find("TankCoveredArc");
         option.IsEnabled = value;
         option = myOptions.Find("SlowTransverseCoveredArc");
         option.IsEnabled = value;
         option = myOptions.Find("SpgCoveredArc");
         option.IsEnabled = value;
         option = myOptions.Find("AtgCoveredArc");
         option.IsEnabled = value;
      }
      private void ResetGameOther(bool value = false)
      {
         Option option = myOptions.Find("EnemyContinueMove");
         option.IsEnabled = value;
         option = myOptions.Find("EnemyRearFacingOnMove");
         option.IsEnabled = value;
         option = myOptions.Find("ShermanIncreaseMoveChances");
         option.IsEnabled = value;
         option = myOptions.Find("ShermanEnhanceMgFire");
         option.IsEnabled = value;
         option = myOptions.Find("ShermanSurpressingMgFire");
         option.IsEnabled = value;
      }
      //----------------------CONTROLLER FUNCTIONS----------------------
      private void StackPanelGameType_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;
         ResetGameType();
         ResetGameGeneralV25No3Options();
         ResetGameOther();
         myRadioButtonOriginal.IsChecked = false;
         myRadioButtonGeneralv25No3.IsChecked = false;
         myRadioButtonOther.IsChecked = false;
         myRadioButtonCustom.IsChecked = false;
         Option option;
         switch (rb.Name)
         {
            case "myRadioButtonOriginal":
               ResetGameGeneralV25No3Options(false);
               ResetGameOther(false);
               option = myOptions.Find("OriginalGame");
               option.IsEnabled = true;
               myRadioButtonOriginal.IsChecked = true;
               break;
            case "myRadioButtonGeneralv25No3": 
               ResetGameGeneralV25No3Options(true);
               ResetGameOther(false);
               option = myOptions.Find("Generalv25No3Game");
               option.IsEnabled = true;
               myRadioButtonGeneralv25No3.IsChecked = true;
               break;
            case "myRadioButtonOther":
               ResetGameGeneralV25No3Options(true);
               ResetGameOther(true);
               option = myOptions.Find("OtherGame");
               option.IsEnabled = true;
               myRadioButtonOther.IsChecked = true;
               break;
            case "myRadioButtonCustom":
               option = myOptions.Find("CustomGame");
               option.IsEnabled = true;
               myRadioButtonCustom.IsChecked = true;
               break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): reached default name=" + rb.Name); return;
         }
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): UpdateDisplay() returned false for name=" + rb.Name);

      }
      private void StackPanelScenario_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;
         Option option = myOptions.Find("SingleDayScenario");
         option.IsEnabled = false;
         switch (rb.Name)
         {
            case "myRadioButtonCampaign":
               option.IsEnabled = false;
               break;
            case "myRadioButtonSingleDay":
               option.IsEnabled = true;
               break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelScenario_Click(): reached default name=" + rb.Name); return;
         }
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelScenario_Click(): UpdateDisplay() returned false for name=" + rb.Name);
      }
      private void StackPanelTutorial_Click(object sender, RoutedEventArgs e)
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
      private void StackPanelAutoRolls_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option? option = null;
         switch (cb.Name)
         {
            case "myCheckBoxAutoRollRatings":
               option = myOptions.Find("AutoRollNewMembers");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelAutoRolls_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            case "myCheckBoxAutoRollAmmoLoad":
               option = myOptions.Find("AutoRollAmmoLoad");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelAutoRolls_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            case "myCheckBoxAutoPreparation":
               option = myOptions.Find("AutoPreparation");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelAutoRolls_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            case "myCheckBoxAutoRollEnemySetup":
               option = myOptions.Find("AutoRollEnemyActivation");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelAutoRolls_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelAutoRolls_Click(): reached default name=" + cb.Name); return;
         }
      }
      private void StackPanelGeneralV25No3_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option? option = null;
         switch (cb.Name)
         {
            case "myCheckBoxAirInterdiction": option = myOptions.Find("AirInterdiction"); break;
            case "myCheckBoxTerrainPointValue": option = myOptions.Find("TerrainPointValue"); break;
            case "myCheckBoxTankCoverArc": option = myOptions.Find("TankCoveredArc"); break;
            case "myCheckBoxSlowTransverseCoverArc": option = myOptions.Find("SlowTransverseCoveredArc"); break;
            case "myCheckBoxSpgCoverArc": option = myOptions.Find("SpgCoveredArc"); break;
            case "myCheckBoxAtgCoverArc": option = myOptions.Find("AtgCoveredArc"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelGeneralV25No3_Click(): reached default name=" + cb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOtherRules_Click(): myOptions.Find() for name=" + cb.Name);
         else
            option.IsEnabled = !option.IsEnabled;
         //----------------------------------
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOtherRules_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void StackPanelGameOtherRules_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option? option = null;
         switch (cb.Name)
         {
            case "myCheckBoxContinueMove": option = myOptions.Find("EnemyContinueMove"); break;
            case "myCheckBoxRearFacingAfterMoveAway": option = myOptions.Find("EnemyRearFacingOnMove"); break;
            case "myCheckBoxIncreasedMove": option = myOptions.Find("ShermanIncreaseMoveChances"); break;
            case "myCheckBoxEnhancedMgFire": option = myOptions.Find("ShermanEnhanceMgFire"); break;
            case "myCheckBoxMgSurpressingFire": option = myOptions.Find("ShermanSurpressingMgFire"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOtherRules_Click(): reached default name=" + cb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOtherRules_Click(): myOptions.Find() for name=" + cb.Name);
         else
            option.IsEnabled = !option.IsEnabled;
         //----------------------------------
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOtherRules_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = true;
      }
      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
