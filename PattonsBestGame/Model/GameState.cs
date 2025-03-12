using Pattons_Best.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Pattons_Best
{
   public abstract class GameState : IGameState
   {
      abstract public string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll); // abstract function...GameEngine calls PerformAction() 
      static public IGameState? GetGameState(GamePhase phase) // static method that returns the next GameState object based on GamePhase
      {
         switch (phase)
         {
            case GamePhase.UnitTest: return new GameStateUnitTest();
            case GamePhase.GameSetup: return new GameStateSetup();
            case GamePhase.MorningBriefing: return new GameStateMorningBriefing();
            case GamePhase.Preparations: return new GameStateBattlePrep();
            case GamePhase.Movement: return new GameStateMovement();
            case GamePhase.Battle: return new GameStateBattle();
            case GamePhase.EveningDebriefing: return new GameStateEveningDebriefing();
            case GamePhase.EndGame: return new GameStateEnded();
            default: Logger.Log(LogEnum.LE_ERROR, "GetGameState(): reached default p=" + phase.ToString()); return null;
         }
      }
      protected bool AddMapItemMove(IGameInstance gi, IMapItem mi, ITerritory newT)
      {
         //-------------------------------
         MapItemMove mim = new MapItemMove(Territories.theTerritories, mi, newT);
         if (true == mim.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddMapItemMove(): mim.CtorError=true for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddMapItemMove(): Invalid Parameter mim.NewTerritory=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddMapItemMove(): Invalid Parameter mim.BestPath=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (0 == mim.BestPath.Territories.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddMapItemMove(): Invalid State Territories.Count=" + mim.BestPath.Territories.Count.ToString() + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         gi.MapItemMoves.Insert(0, mim); // add at front
         return true;
      }
      protected bool AssignNewCrewMembers(IGameInstance gi)
      {
         IAfterActionReport? aar = gi.Reports.GetLast();
         if (null == aar)
         {
            Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewMembers(): aar is null");
            return false;
         }
         foreach (IMapItem mi in gi.NewMembers)
         {
            ICrewMember? crewMember = mi as ICrewMember;
            if (crewMember == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewMembers(): crewMember is null");
               return false;
            }
            switch (crewMember.Role)
            {
               case "Commander":
                  aar.Commander = crewMember;
                  break;
               case "Gunner":
                  aar.Gunner = crewMember;
                  break;
               case "Loader":
                  aar.Loader = crewMember;
                  break;
               case "Driver":
                  aar.Driver = crewMember;
                  break;
               case "Assistant":
                  aar.Assistant = crewMember;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewMembers(): Reached Default with role= " + crewMember.Role);
                  return false;
            }
         }
         return true;
      }
      protected bool LoadGame(ref IGameInstance gi, ref GameAction action)
      {
         gi.Stacks.Clear();
         return true;
      }
      protected void UndoCommand(ref IGameInstance gi, ref GameAction action)
      {
         Logger.Log(LogEnum.LE_UNDO_COMMAND, "UndoCommand(): cmd=" + gi.IsUndoCommandAvailable.ToString() + "-->false  a=" + action.ToString());
         gi.IsUndoCommandAvailable = false;
         gi.EventDisplayed = gi.EventActive = "e203";
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "UndoCommand():  gi.MapItemMoves.Clear()  a=" + action.ToString());
         gi.MapItemMoves.Clear();
      }
      protected void PrintDiagnosticInfoToLog()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("\n\tGameVersion=");
         Assembly assem = Assembly.GetExecutingAssembly();
         Version? version = assem.GetName().Version;
         if( null != version )
            sb.Append(version.ToString());
         var attributes = assem.CustomAttributes;
         foreach (var attribute in attributes)
         {
            if (attribute.AttributeType == typeof(TargetFrameworkAttribute))
            {
               var arg = attribute.ConstructorArguments.FirstOrDefault();
               sb.Append("\n\tTargetFramework=");
               sb.Append(arg.Value);
               break;
            }
         }
         sb.Append("\n\tOsVersion=");
         sb.Append(System.Environment.OSVersion.Version.Build.ToString());
         sb.Append("\n\tOS Desc=");
         sb.Append(RuntimeInformation.OSDescription.ToString());
         sb.Append("\n\tOS Arch=");
         sb.Append(RuntimeInformation.OSArchitecture.ToString());
         sb.Append("\n\tProcessorArch=");
         sb.Append(RuntimeInformation.ProcessArchitecture.ToString());
         sb.Append("\n\tnetVersion=");
         sb.Append(Environment.Version.ToString());
         //--------------------------------------------

         //--------------------------------------------
         System.Windows.Forms.Screen? screen = System.Windows.Forms.Screen.PrimaryScreen;
         if( null != screen )
         {
            var dpi = screen.Bounds.Width / System.Windows.SystemParameters.PrimaryScreenWidth;
            sb.Append("\n\tDPI=(");
            sb.Append(dpi.ToString("000.0"));
         }
         sb.Append(")\n\tAppDir=");
         sb.Append(MainWindow.theAssemblyDirectory);
         Logger.Log(LogEnum.LE_GAME_INIT_VERSION, sb.ToString());
      }
   }
   //-----------------------------------------------------
   class GameStateSetup : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowCombatCalendarDialog:
            case GameAction.ShowAfterActionReportDialog:
            case GameAction.ShowInventoryDialog:
            case GameAction.ShowGameFeats:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
               break;
            case GameAction.UpdateEventViewerDisplay: // Only change active event
               break;
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
               break;
            case GameAction.UpdateNewGame:
            case GameAction.RemoveSplashScreen:
               gi.Statistic.Clear();         // Clear any current statitics
               gi.Statistic.myNumGames = 1;  // Set played games to 1
               //----------------------------------------------------
               ICombatCalendarEntry? entry = TableMgr.theCombatCalendarEntries[0];
               if( null == entry )
               {
                  returnStatus = "PerformAutoSetup() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               else
               {
                  IAfterActionReport report1 = new AfterActionReport(entry);
                  gi.Reports.Add(report1);
                  //----------------------------------------------------
                  Option? option = gi.Options.Find("AutoSetup");
                  if (null == option)
                  {
                     option = new Option("AutoSetup", false);
                     gi.Options.Add(option);
                  }
                  if (true == option.IsEnabled)
                  {
                     if (false == PerformAutoSetup(ref gi, ref action))
                     {
                        returnStatus = "PerformAutoSetup() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
                     }
                     gi.EventDisplayed = gi.EventActive = "e203"; // next screen to show
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     gi.Options.SetOriginalGameOptions();
                     gi.GamePhase = GamePhase.GameSetup;
                     gi.EventDisplayed = gi.EventActive = "e000"; // next screen to show
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  AddStartingTestingOptions(gi);
                  PrintDiagnosticInfoToLog();
               }
               break;
            case GameAction.SetupShowMapHistorical:
               gi.EventDisplayed = gi.EventActive = "e001";
               break;
            case GameAction.SetupShowMovementBoard:
               gi.EventDisplayed = gi.EventActive = "e002";
               break;
            case GameAction.SetupShowBattleBoard:
               gi.EventDisplayed = gi.EventActive = "e003";
               break;
            case GameAction.SetupShowTankCard:
               gi.EventDisplayed = gi.EventActive = "e004";
               break;
            case GameAction.SetupShowAfterActionReport:
               gi.EventDisplayed = gi.EventActive = "e005";
               break;
            case GameAction.SetupAssignCrewRating:
               gi.NewMembers.Clear();
               IAfterActionReport? report = gi.Reports.GetLast();
               if( null == report )
               {
                  returnStatus = "gi.Reports.GetLast() returned null";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(SetupAssignCrewRating): " + returnStatus);
               }
               else
               {
                  gi.NewMembers.Add(report.Commander);
                  gi.NewMembers.Add(report.Gunner);
                  gi.NewMembers.Add(report.Loader);
                  gi.NewMembers.Add(report.Driver);
                  gi.NewMembers.Add(report.Assistant);
               }
               break;
            case GameAction.SetupShowCombatCalendarCheck:
               gi.GamePhase = GamePhase.MorningBriefing;
               gi.EventDisplayed = gi.EventActive = "e006";
               gi.DieRollAction = GameAction.SetupCombatCalendarRoll;
               if( false == AssignNewCrewMembers(gi))
               {
                  returnStatus = "AssignNewCrewMembers() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(SetupAssignCrewRating): " + returnStatus);
               }
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            default:
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateSetup.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
      private bool PerformAutoSetup(ref IGameInstance gi, ref GameAction action)
      {
         return true;
      }
      private void AddStartingTestingOptions(IGameInstance gi)
      {
      }
   }
   //-----------------------------------------------------
   class GameStateMorningBriefing : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         IAfterActionReport? lastReport = gi.Reports.GetLast(); // remove it from list
         if (null == lastReport)
         {
            returnStatus = "lastReport=null";
            Logger.Log(LogEnum.LE_ERROR, "GameStateMorningBriefing.PerformAction(): " + returnStatus);
         }
         else
         {
            switch (action)
            {
               case GameAction.ShowCombatCalendarDialog:
               case GameAction.ShowAfterActionReportDialog:
               case GameAction.ShowInventoryDialog:
               case GameAction.ShowRuleListingDialog:
               case GameAction.ShowEventListingDialog:
               case GameAction.ShowReportErrorDialog:
               case GameAction.ShowAboutDialog:
                  break;
               case GameAction.SetupShowMapHistorical:
               case GameAction.SetupShowMovementBoard:
               case GameAction.SetupShowBattleBoard:
               case GameAction.SetupShowTankCard:
               case GameAction.SetupShowAfterActionReport:
               case GameAction.SetupShowCombatCalendarCheck:
                  break;
               case GameAction.UpdateEventViewerDisplay: // Only change active event
                  break;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.MorningBriefingAssignCrewRating: // handled in EventViewer by showing dialog
                  break;
               case GameAction.MorningBriefingCalendarRoll:
               case GameAction.SetupCombatCalendarRoll:
                  dieRoll = 0; // <cgs> TEST
                  gi.DieResults[key][0] = dieRoll;
                  break;
               case GameAction.MorningBriefingBegin:
                  gi.EventDisplayed = gi.EventActive = "e007";
                  gi.DieRollAction = GameAction.MorningBriefingWeatherRoll;
                  break;
               case GameAction.MorningBriefingWeatherRoll:
                  dieRoll = 50; // <cgs> TEST
                  gi.DieResults[key][0] = dieRoll;
                  string weatherRolled = TableMgr.GetWeather(dieRoll);
                  lastReport.Weather = weatherRolled;
                  break;
               case GameAction.MorningBriefingWeatherRollEnd:
                  if (true == lastReport.Weather.Contains("Snow"))
                     gi.EventDisplayed = gi.EventActive = "e008"; // first need to roll for snow
                  else
                     gi.EventDisplayed = gi.EventActive = "e009";
                  break;
               case GameAction.MorningBriefingAmmoReadyRackLoad:
                  break;
               case GameAction.MorningBriefingTimeCheck:
                  gi.EventDisplayed = gi.EventActive = "e010";
                  gi.DieRollAction = GameAction.MorningBriefingTimeCheckRoll;
                  break;
               case GameAction.MorningBriefingTimeCheckRoll:
                  dieRoll = 7; // <cgs> TEST
                  gi.DieResults[key][0] = dieRoll;
                  if( false == TableMgr.SetTimeTrack(lastReport, gi.Day))
                  {
                     returnStatus = "TableMgr.SetTimeTrack() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMorningBriefing.PerformAction(): " + returnStatus);
                  }
                  lastReport.SunriseHour += (int)Math.Floor((double)dieRoll * 0.5) + 1;
                  lastReport.MainGunHE -= dieRoll * 2;
                  lastReport.Ammo30CalibreMG -= dieRoll;
                  break;
               case GameAction.MorningBriefingSnowRoll:
                  gi.DieResults[key][0] = dieRoll;
                  break;
               case GameAction.PreparationsStart:
                  gi.GamePhase = GamePhase.Preparations;
                  gi.EventDisplayed = gi.EventActive = "e011";
                  gi.DieRollAction = GameAction.PreparationsDeploymentRoll;
                  break;
               case GameAction.MorningBriefingEnd:
                  ++gi.Day;
                  ICombatCalendarEntry? newEntry = TableMgr.theCombatCalendarEntries[gi.Day];
                  if (null == newEntry)
                  {
                     returnStatus = "newEntry=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMorningBriefing.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     IAfterActionReport newReport = new AfterActionReport(newEntry, lastReport);
                     gi.Reports.Add(newReport);
                     gi.EventDisplayed = gi.EventActive = "e006";
                     gi.DieResults[gi.EventActive][0] = Utilities.NO_RESULT;
                     gi.DieRollAction = GameAction.MorningBriefingCalendarRoll;
                  }
                  break;
               case GameAction.EndGameClose:
                  gi.GamePhase = GamePhase.EndGame;
                  break;
               default:
                  break;
            }
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateMorningBriefing.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateBattlePrep : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         string key = gi.EventActive;
         IAfterActionReport? lastReport = gi.Reports.GetLast(); // remove it from list
         if (null == lastReport)
         {
            returnStatus = "lastReport=null";
            Logger.Log(LogEnum.LE_ERROR, "GameStateMorningBriefing.PerformAction(): " + returnStatus);
         }
         else
         {
            switch (action)
            {
               case GameAction.UpdateEventViewerDisplay: // Only change active event
                  break;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.EndGameClose:
                  gi.GamePhase = GamePhase.EndGame;
                  break;
               case GameAction.PreparationsDeploymentRoll:
                  dieRoll = 7; // <cgs> TEST
                  gi.DieResults[key][0] = dieRoll;
                  break;
               default:
                  break;
            }
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateBattlePrep.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateMovement : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         switch (action)
         {
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
               break;
            case GameAction.UpdateEventViewerDisplay: // Only change active event
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            default:
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateMovement.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateBattle : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         switch (action)
         {
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
               break;
            case GameAction.UpdateEventViewerDisplay: // Only change active event
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            default:
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateBattle.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateEveningDebriefing : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         switch (action)
         {
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
               break;
            case GameAction.UpdateEventViewerDisplay: // Only change active event
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            default:
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateEveningDebriefing.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateEnded : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         switch (action)
         {
            case GameAction.ShowCombatCalendarDialog:
            case GameAction.ShowAfterActionReportDialog:
            case GameAction.ShowInventoryDialog:
            case GameAction.ShowGameFeats:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowTableListing:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.UpdateEventViewerDisplay:
            case GameAction.EndGameShowFeats:
               break;
            case GameAction.EndGameWin:
               gi.EventDisplayed = gi.EventActive = "e501";
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.EndGameLost:
               gi.EventDisplayed = gi.EventActive = "e502";
               gi.DieRollAction = GameAction.DieRollActionNone;
               break;
            case GameAction.EndGameShowStats:
               gi.EventDisplayed = gi.EventActive = "e503";
               break;
            case GameAction.EndGameClose:
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.EndGameFinal:
               gi.EventDisplayed = gi.EventActive = "e504";
               break;
            case GameAction.UpdateLoadingGame:
               if (false == LoadGame(ref gi, ref action))
               {
                  returnStatus = "LoadGame() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEnded.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.EndGameExit:
               if (null != System.Windows.Application.Current)
                  System.Windows.Application.Current.Shutdown();
               break;
            default:
               returnStatus = "Reached Default ERROR";
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateEnded.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a="); sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra="); sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e="); sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
   //-----------------------------------------------------
   class GameStateUnitTest : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         String returnStatus = "OK";
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string previousStartEvent = gi.EventStart;
         switch (action)
         {
            case GameAction.RemoveSplashScreen:
               PrintDiagnosticInfoToLog();
               break;
            case GameAction.UnitTestCommand: // call the unit test's Command() function
               IUnitTest ut = gi.UnitTests[gi.GameTurn];
               if (false == ut.Command(ref gi))
               {
                  returnStatus = "Command() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UnitTestNext: // call the unit test's NextTest() function
               IUnitTest ut1 = gi.UnitTests[gi.GameTurn];
               if (false == ut1.NextTest(ref gi))
               {
                  returnStatus = "NextTest() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               }
               break;
            case GameAction.UnitTestCleanup: // Call the unit test's NextTest() function
               IUnitTest ut2 = gi.UnitTests[gi.GameTurn];
               if (false == ut2.Cleanup(ref gi))
               {
                  returnStatus = "Cleanup() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               }
               break;
            default:
               returnStatus = "Reached Default ERROR";
               Logger.Log(LogEnum.LE_ERROR, "GameStateUnitTest.PerformAction(): " + returnStatus);
               break;
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateUnitTest.PerformAction():");
         sb12.Append("===>p=");
         sb12.Append(previousPhase.ToString());
         if (previousPhase != gi.GamePhase)
         { sb12.Append("=>"); sb12.Append(gi.GamePhase.ToString()); }
         sb12.Append(" a=");
         sb12.Append(previousAction.ToString());
         if (previousAction != action)
         { sb12.Append("=>"); sb12.Append(action.ToString()); }
         sb12.Append(" dra=");
         sb12.Append(previousDieAction.ToString());
         if (previousDieAction != gi.DieRollAction)
         { sb12.Append("=>"); sb12.Append(gi.DieRollAction.ToString()); }
         sb12.Append(" e=");
         sb12.Append(previousEvent);
         if (previousEvent != gi.EventActive)
         { sb12.Append("=>"); sb12.Append(gi.EventActive); }
         sb12.Append(" dr="); sb12.Append(dieRoll.ToString());
         if ("OK" == returnStatus)
            Logger.Log(LogEnum.LE_NEXT_ACTION, sb12.ToString());
         else
            Logger.Log(LogEnum.LE_ERROR, sb12.ToString());
         return returnStatus;
      }
   }
}