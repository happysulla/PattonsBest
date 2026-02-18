
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pattons_Best
{
   public static class CustomCommands
   {
      public static readonly RoutedUICommand NewCommand = new RoutedUICommand("New", "New", typeof(CustomCommands), new InputGestureCollection() { new KeyGesture(Key.N, ModifierKeys.Control) });
   }
   class MainMenuViewer : IView
   {
      private readonly Menu myMainMenu;                     // Top level menu items: File | View | Options | Help
      private readonly MenuItem myMenuItemTopLevel1 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel2 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel3 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel4 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel21 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel22 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel23 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel31 = new MenuItem();
      private readonly MenuItem myMenuItemTopLevel36 = new MenuItem();
      private readonly IGameEngine myGameEngine;
      private IGameInstance myGameInstance;
      public bool IsPathShown { get; set; } = true;
      public bool IsRoadsShown { get; set; } = false;
      public Options? NewGameOptions { get; set; } = null;  // These options take affect when new game menu item is selected
      //-----------------------------------------------------------------------
      public MainMenuViewer(Menu mi, IGameEngine ge, IGameInstance gi) // Constructor creates default top level menus that get changed with UpdateView() based on GamePhase and GameAction
      {
         myGameEngine = ge;
         myGameInstance = gi;
         myMainMenu = mi;
         if (false == Directory.Exists(GameLoadMgr.theGamesDirectory)) // create directory if does not exists
            Directory.CreateDirectory(GameLoadMgr.theGamesDirectory);
         string filepath1 = GameLoadMgr.theGamesDirectory + "CheckpointLastDay.pbg";
         string filepath2 = GameLoadMgr.theGamesDirectory + "CheckpointLastRound.pbg";
         foreach (Control item in myMainMenu.Items) // Initialize all the menu items
         {
            if (item is MenuItem menuItem)
            {
               if (menuItem.Name == "myMenuItemTopLevel1")
               {
                  myMenuItemTopLevel1 = menuItem;
                  myMenuItemTopLevel1.Header = "_File";
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel2")
               {
                  myMenuItemTopLevel2 = menuItem;
                  myMenuItemTopLevel2.Header = "_Edit";
                  myMenuItemTopLevel2.Visibility = Visibility.Visible;
                  myMenuItemTopLevel21.Header = "_Undo";
                  myMenuItemTopLevel21.InputGestureText = "Ctrl+U";
                  myMenuItemTopLevel21.IsEnabled = false;
                  myMenuItemTopLevel21.Click += MenuItemEditUndo_Click;
                  myMenuItemTopLevel2.Items.Add(myMenuItemTopLevel21);
                  myMenuItemTopLevel22.Header = "Revert Last _Day";
                  myMenuItemTopLevel22.InputGestureText = "Ctrl+D";
                  myMenuItemTopLevel22.Click += MenuItemEditRecoverCheckpoint_Click;
                  if (true == File.Exists(filepath1))
                     myMenuItemTopLevel22.IsEnabled = true;
                  else
                     myMenuItemTopLevel22.IsEnabled = false;
                  myMenuItemTopLevel2.Items.Add(myMenuItemTopLevel22);
                  myMenuItemTopLevel23.Header = "Revert _Round";
                  myMenuItemTopLevel23.InputGestureText = "Ctrl+R";
                  myMenuItemTopLevel23.Click += MenuItemEditRecoverRound_Click;
                  if (true == File.Exists(filepath2))
                     myMenuItemTopLevel23.IsEnabled = true;
                  else
                     myMenuItemTopLevel23.IsEnabled = false;
                  myMenuItemTopLevel2.Items.Add(myMenuItemTopLevel23);
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel3")
               {
                  myMenuItemTopLevel3 = menuItem;
                  myMenuItemTopLevel3.Header = "_View";
                  myMenuItemTopLevel3.Visibility = Visibility.Visible;
                  myMenuItemTopLevel31.Header = "_Path";
                  myMenuItemTopLevel31.InputGestureText = "Ctrl+P";
                  myMenuItemTopLevel31.Click += MenuItemViewPath_Click;
                  myMenuItemTopLevel31.IsCheckable = true;
                  myMenuItemTopLevel31.IsChecked = true;
                  myMenuItemTopLevel3.Items.Add(myMenuItemTopLevel31);
                  MenuItem subItem32 = new MenuItem();
                  subItem32.Header = "_Combat Calendar...";
                  subItem32.InputGestureText = "Ctrl+Shift+C";
                  subItem32.Click += MenuItemViewCombatCalendar;
                  myMenuItemTopLevel3.Items.Add(subItem32);
                  MenuItem subItem33 = new MenuItem();
                  subItem33.Header = "_After Action Report...";
                  subItem33.InputGestureText = "Ctrl+A";
                  subItem33.Click += MenuItemViewAfterActionReport;
                  myMenuItemTopLevel3.Items.Add(subItem33);
                  MenuItem subItem34 = new MenuItem();
                  subItem34.Header = "_Movement Diagram...";
                  subItem34.InputGestureText = "Ctrl+M";
                  subItem34.Click += MenuItemViewMoveDiagram;
                  myMenuItemTopLevel3.Items.Add(subItem34);
                  MenuItem subItem35 = new MenuItem();
                  subItem35.Header = "_Game Feats...";
                  subItem35.InputGestureText = "Ctrl+G";
                  subItem35.Click += MenuItemViewFeats_Click;
                  myMenuItemTopLevel3.Items.Add(subItem35);
                  myMenuItemTopLevel36.Header = "_Roads";
                  myMenuItemTopLevel36.InputGestureText = "Ctrl+Shift+R";
                  myMenuItemTopLevel36.Click += MenuItemViewRoads_Click;
                  myMenuItemTopLevel36.IsCheckable = true;
                  myMenuItemTopLevel36.IsChecked = false;
                  myMenuItemTopLevel3.Items.Add(myMenuItemTopLevel36);
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel4")
               {
                  myMenuItemTopLevel4 = menuItem;
                  myMenuItemTopLevel4.Header = "_Help";
                  myMenuItemTopLevel4.Visibility = Visibility.Visible;
                  MenuItem subItem41 = new MenuItem();
                  subItem41.Header = "_Rules...";
                  subItem41.InputGestureText = "F1";
                  subItem41.Click += MenuItemHelpRules_Click;
                  myMenuItemTopLevel4.Items.Add(subItem41);
                  MenuItem subItem42 = new MenuItem();
                  subItem42.Header = "_Events...";
                  subItem42.InputGestureText = "F2";
                  subItem42.Click += MenuItemHelpEvents_Click;
                  myMenuItemTopLevel4.Items.Add(subItem42);
                  MenuItem subItem43 = new MenuItem();
                  subItem43.Header = "_Tables...";
                  subItem43.InputGestureText = "F3";
                  subItem43.Click += MenuItemHelpTables_Click;
                  myMenuItemTopLevel4.Items.Add(subItem43);
                  MenuItem subItem44 = new MenuItem();
                  subItem44.Header = "_Icons...";
                  subItem44.InputGestureText = "F4";
                  subItem44.Click += MenuItemHelpIcons_Click;
                  myMenuItemTopLevel4.Items.Add(subItem44);
                  MenuItem subItem45 = new MenuItem();
                  subItem45.Header = "Report Error...";
                  subItem45.InputGestureText = "F5";
                  subItem45.Click += MenuItemHelpReportError_Click;
                  myMenuItemTopLevel4.Items.Add(subItem45);
                  MenuItem subItem46 = new MenuItem();
                  subItem46.Header = "_About...";
                  subItem46.InputGestureText = "Ctrl+A";
                  subItem46.Click += MenuItemHelpAbout_Click;
                  myMenuItemTopLevel4.Items.Add(subItem46);
               }
            } // end foreach (Control item in myMainMenu.Items)
         } // end foreach (Control item in myMainMenu.Items)
#if UT1
            myMenuItemTopLevel1.Width = 300;
            myMenuItemTopLevel2.Visibility = Visibility.Hidden;
            myMenuItemTopLevel3.Visibility = Visibility.Hidden;
            myMenuItemTopLevel4.Visibility = Visibility.Hidden;
            MenuItem subItem1 = new MenuItem();
            subItem1.Click += MenuItemCommand_Click;
            myMenuItemTopLevel1.Items.Add(subItem1);
            MenuItem subItem2 = new MenuItem();
            subItem2.Header = "_NextTest";
            subItem2.Click += MenuItemNextTest_Click;
            myMenuItemTopLevel1.Items.Add(subItem2);
            MenuItem subItem3 = new MenuItem();
            subItem3.Header = "_Cleanup";
            subItem3.Click += MenuItemCleanup_Click;
            myMenuItemTopLevel1.Items.Add(subItem3);
#else
         MenuItem subItem1 = new MenuItem();
         subItem1.Header = "_New";
         subItem1.InputGestureText = "Ctrl+N";
         subItem1.Click += MenuItemNew_Click;
         myMenuItemTopLevel1.Items.Add(subItem1);
         MenuItem subItem2 = new MenuItem();
         subItem2.Header = "_Open...";
         subItem2.InputGestureText = "Ctrl+O";
         subItem2.Click += MenuItemFileOpen_Click;
         myMenuItemTopLevel1.Items.Add(subItem2);
         MenuItem subItem3 = new MenuItem();
         subItem3.Header = "_Close";
         subItem3.InputGestureText = "Ctrl+C";
         subItem3.Click += MenuItemClose_Click;
         myMenuItemTopLevel1.Items.Add(subItem3);
         MenuItem subItem4 = new MenuItem();
         subItem4.Header = "_Save As...";
         subItem4.InputGestureText = "Ctrl+S";
         subItem4.Click += MenuItemSaveAs_Click;
         myMenuItemTopLevel1.Items.Add(subItem4);
         MenuItem subItem5 = new MenuItem();
         subItem5.Header = "_Options...";
         subItem5.InputGestureText = "Ctrl+Shift+O";
         subItem5.Click += MenuItemFileOptions_Click;
         myMenuItemTopLevel1.Items.Add(subItem5);
#endif
      } // end MainMenuViewer()
      //-----------------------------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         myGameInstance = gi;
         StringBuilder sb = new StringBuilder("-----------------MainMenuViewer::UpdateView() => a="); sb.Append(action.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_MENU, sb.ToString());
         switch (gi.GamePhase)
         {
            case GamePhase.GameSetup:
               return;
            case GamePhase.UnitTest:
               IUnitTest ut = gi.UnitTests[gi.GameTurn];
               myMenuItemTopLevel1.Header = ut.HeaderName;
               if (0 < myMenuItemTopLevel1.Items.Count)
               {
                  MenuItem menuItem0 = (MenuItem)myMenuItemTopLevel1.Items[0];
                  menuItem0.Header = ut.CommandName;
               }
               break;
            default:
               if (false == Directory.Exists(GameLoadMgr.theGamesDirectory)) // create directory if does not exists
                  Directory.CreateDirectory(GameLoadMgr.theGamesDirectory);
               string filepath = GameLoadMgr.theGamesDirectory + "CheckpointLastDay.pbg";
               if (true == File.Exists(filepath))
                  myMenuItemTopLevel22.IsEnabled = true;
               else
                  myMenuItemTopLevel22.IsEnabled = false;
               //----------------------------------------
               filepath = GameLoadMgr.theGamesDirectory + "CheckpointLastRound.pbg";
               if (true == File.Exists(filepath))
                  myMenuItemTopLevel23.IsEnabled = true;
               else
                  myMenuItemTopLevel23.IsEnabled = false;
               //----------------------------------------
               if (null == myGameInstance.UndoCmd)
               {
                  myMenuItemTopLevel21.IsEnabled = false;
               }
               else
               {
                  myMenuItemTopLevel21.IsEnabled = true;
                  Logger.Log(LogEnum.LE_UNDO_COMMAND, "MainMenuViewer.UpdateView(): cmd=" + myGameInstance.UndoCmd.ToString());
               }
               return;
         }
      }
      //------------------------------CONTROLLER-------------------------------
      public void MenuItemNew_Click(object sender, RoutedEventArgs e)
      {
         if (null == NewGameOptions)
            myGameInstance = new GameInstance();
         else
            myGameInstance = new GameInstance(this.NewGameOptions);
         if (true == myGameInstance.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemNew_Click(): myGameInstance.CtorError = true");
            return;
         }
         GameAction action = GameAction.UpdateNewGame;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemClose_Click(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemClose_Click(): myGameInstance=null");
         }
         else
         {
            GameAction action = GameAction.EndGameClose;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
      }
      public void MenuItemFileOpen_Click(object sender, RoutedEventArgs e)
      {
         GameLoadMgr loadMgr = new GameLoadMgr();
         IGameInstance? gi = loadMgr.OpenGameFromFile();
         if (null != gi)
         {
            myGameInstance = gi;
            GameAction action = GameAction.UpdateLoadingGame;
            myGameEngine.PerformAction(ref gi, ref action);
         }
      }
      public void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
      {
         GameLoadMgr loadMgr = new GameLoadMgr();
         if (false == loadMgr.SaveGameAsToFile(myGameInstance))
            Logger.Log(LogEnum.LE_ERROR, "MenuItemSave_Click(): GameLoadMgr.SaveGameAs() returned false");
      }
      public void MenuItemFileOptions_Click(object sender, RoutedEventArgs e)
      {
         ShowOptionsSelectionDialog dialog = new ShowOptionsSelectionDialog(myGameInstance.Options); // Set Options in Game
         if (true == dialog.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemFileOptions_Click(): OptionSelectionDialog CtorError=true");
            return;
         }
         if (true == dialog.ShowDialog())
         {
            this.NewGameOptions = dialog.NewOptions;
            Logger.Log(LogEnum.LE_VIEW_SHOW_OPTIONS, "MenuItemFileOptions_Click(): new=" + this.NewGameOptions.ToString());
            ApplyOptionsToCurrentGame(this.NewGameOptions, myGameInstance.Options);
            Logger.Log(LogEnum.LE_VIEW_SHOW_OPTIONS, "MenuItemFileOptions_Click(): current=" + myGameInstance.Options.ToString());
            GameAction action = GameAction.UpdateGameOptions;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
      }
      public void MenuItemEditUndo_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateUndo;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemEditUndo_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         if (null == myGameInstance.UndoCmd)
            e.CanExecute = false;
         else
            e.CanExecute = true;
      }
      public void MenuItemEditRecoverCheckpoint_Click(object sender, RoutedEventArgs e)
      {
         GameLoadMgr loadMgr = new GameLoadMgr();
         IGameInstance? gi = loadMgr.OpenGame("CheckpointLastDay.pbg");
         if (null != gi)
         {
            myGameInstance = gi;
            GameAction action = GameAction.UpdateLoadingGame;
            myGameEngine.PerformAction(ref gi, ref action);
         }
      }
      public void MenuItemEditRecoverCheckpoint_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         try
         {
            if (false == Directory.Exists(GameLoadMgr.theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameLoadMgr.theGamesDirectory);
            string filepath = GameLoadMgr.theGamesDirectory + "CheckpointLastDay.pbg";
            if (true == File.Exists(filepath))
               e.CanExecute = true;
            else
               e.CanExecute = false;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Save_Game(): path=" + GameLoadMgr.theGamesDirectory + " ex=" + ex.ToString());
            e.CanExecute = false;
         }
      }
      public void MenuItemEditRecoverRound_Click(object sender, RoutedEventArgs e)
      {
         GameLoadMgr loadMgr = new GameLoadMgr();
         IGameInstance? gi = loadMgr.OpenGame("CheckpointLastRound.pbg");
         if (null != gi)
         {
            myGameInstance = gi;
            GameAction action = GameAction.UpdateLoadingGame;
            myGameEngine.PerformAction(ref gi, ref action);
         }
      }
      public void MenuItemEditRecoverRound_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         try
         {
            if (false == Directory.Exists(GameLoadMgr.theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameLoadMgr.theGamesDirectory);
            string filepath = GameLoadMgr.theGamesDirectory + "CheckpointLastRound.pbg";
            if( true == File.Exists(filepath))
               e.CanExecute = true;
            else
               e.CanExecute = false;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Save_Game(): path=" + GameLoadMgr.theGamesDirectory + " ex=" + ex.ToString());
            e.CanExecute = false;
         }
      }
      public void MenuItemViewPath_Click(object sender, RoutedEventArgs e)
      {
         IsPathShown = !IsPathShown;
         myMenuItemTopLevel31.IsChecked = IsPathShown;
         GameAction action = GameAction.ShowTankForcePath;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemViewCombatCalendar(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowCombatCalendarDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemViewAfterActionReport(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowAfterActionReportDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemViewMoveDiagram(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowMovementDiagramDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemViewFeats_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowGameFeatsDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemViewRoads_Click(object sender, RoutedEventArgs e)
      {
         IsRoadsShown = !IsRoadsShown;
         myMenuItemTopLevel36.IsChecked = IsRoadsShown;
         GameAction action = GameAction.ShowRoads;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpRules_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowRuleListingDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpEvents_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowEventListingDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpTables_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowTableListing;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpIcons_Click(object sender, RoutedEventArgs e)
      {
         ShowIconDisplayDialog dialog = new ShowIconDisplayDialog();
         dialog.Show();
      }
      public void MenuItemHelpReportError_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowReportErrorDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpAbout_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowAboutDialog;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      //----------------------------------------------------------
      public void MenuItemCommand_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UnitTestCommand;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemNextTest_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UnitTestNext;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemCleanup_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UnitTestCleanup;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      //----------------------------------------------------------
      private void ApplyOptionsToCurrentGame(Options newOptions, Options currentOptions)
      {
         string name = "SkipTutorial0";
         Option? currentOption = currentOptions.Find(name);
         if (null == currentOption)
         {
            currentOption = new Option(name, false);
            currentOptions.Add(currentOption);
         }
         Option? newOption = newOptions.Find(name);
         if (null == newOption)
         {
            newOption = new Option(name, false);
            newOptions.Add(newOption);
         }
         currentOption.IsEnabled = newOption.IsEnabled;
         //-----------------------------------------
         currentOptions.Clear();
         foreach(Option option in newOptions)
         {
            Option newbie = new Option(option.Name, option.IsEnabled);
            currentOptions.Add(newbie);
         }
      }
   }
}
