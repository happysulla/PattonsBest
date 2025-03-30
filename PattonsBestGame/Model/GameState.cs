using Pattons_Best.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Windows.Controls;
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
         if (null != version)
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
         if (null != screen)
         {
            var dpi = screen.Bounds.Width / System.Windows.SystemParameters.PrimaryScreenWidth;
            sb.Append("\n\tDPI=(");
            sb.Append(dpi.ToString("000.0"));
         }
         sb.Append(")\n\tAppDir=");
         sb.Append(MainWindow.theAssemblyDirectory);
         Logger.Log(LogEnum.LE_GAME_INIT_VERSION, sb.ToString());
      }
      protected void AdvanceTime(IAfterActionReport report, int minToAdd)
      {
         report.SunriseMin += minToAdd;
         if (59 < report.SunriseMin)
         {
            report.SunriseHour++;
            report.SunriseMin %= 60;
         }
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
            case GameAction.UpdateEventViewerDisplay: // Only change active event
               break;
            case GameAction.TestingStartPreparations:
               gi.GamePhase = GamePhase.Preparations;
               gi.EventDisplayed = gi.EventActive = "e011";
               gi.DieRollAction = GameAction.PreparationsDeploymentRoll;
               gi.Stacks.Add(new MapItem("Sherman1", 2.0, "t001", gi.Home));
               break;
            case GameAction.TestingStartMovement:
               gi.GamePhase = GamePhase.Movement;
               gi.EventDisplayed = gi.EventActive = "e017";
               gi.DieRollAction = GameAction.MovementStartAreaSetRoll;
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
               if (null == entry)
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
               if (null == report)
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
               if (false == AssignNewCrewMembers(gi))
               {
                  returnStatus = "AssignNewCrewMembers() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(SetupAssignCrewRating): " + returnStatus);
               }
               break;
            case GameAction.EndGameClose:
               gi.GamePhase = GamePhase.EndGame;
               break;
            default:
               returnStatus = "reached default action=" + action.ToString();
               Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
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
         IAfterActionReport? lastReport = gi.Reports.GetLast();
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
               case GameAction.ShowGameFeats:
               case GameAction.ShowRuleListingDialog:
               case GameAction.ShowEventListingDialog:
               case GameAction.ShowTableListing:
               case GameAction.ShowReportErrorDialog:
               case GameAction.ShowAboutDialog:
               case GameAction.EndGameShowFeats:
               case GameAction.UpdateEventViewerDisplay: // Only change active event
                  break;
               case GameAction.MorningBriefingAmmoLoad:
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
                  gi.DieRollAction = GameAction.DieRollActionNone;
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
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (false == TableMgr.SetTimeTrack(lastReport, gi.Day))
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
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.PreparationsDeployment:
                  gi.GamePhase = GamePhase.Preparations;
                  gi.EventDisplayed = gi.EventActive = "e011";
                  gi.DieRollAction = GameAction.PreparationsDeploymentRoll;
                  gi.Stacks.Add(new MapItem("Sherman1", 2.0, "t001", gi.Home));
                  if (false == SetWeather(gi, gi.DieResults["e007"][0]))
                  {
                     returnStatus = "SetWeather() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMorningBriefing.PerformAction(): " + returnStatus);
                  }
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
                  returnStatus = "reached default action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateMorningBriefing.PerformAction(): " + returnStatus);
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
      private bool SetWeather(IGameInstance gi, int dieRoll)
      {
         IAfterActionReport? report = gi.Reports.GetLast(); // remove it from list
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetWeatherCounters(): report=null");
            return false;
         }
         string weatherRolled = TableMgr.GetWeather(dieRoll);
         ITerritory? w1 = Territories.theTerritories.Find("Weather1");
         if (null == w1)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetWeatherCounters(): w1=null");
            return false;
         }
         ITerritory? w2 = Territories.theTerritories.Find("Weather2");
         if (null == w2)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetWeatherCounters(): w2=null");
            return false;
         }
         const double zoom = 1.3;
         switch (weatherRolled)
         {
            case "Clear": gi.Stacks.Add(new MapItem("Clear", zoom, "c20Clear", w1)); break;
            case "Overcast": gi.Stacks.Add(new MapItem("Overcast", zoom, "c21Overcast", w1)); break;
            case "Fog": gi.Stacks.Add(new MapItem("Fog", zoom, "c22Fog", w1)); break;
            case "Mud": gi.Stacks.Add(new MapItem("Mud", zoom, "c23Mud", w1)); break;
            case "Mud/Overcast": gi.Stacks.Add(new MapItem("Mud", zoom, "c23Mud", w1)); gi.Stacks.Add(new MapItem("Overcast", 1.0, "c21Overcast", w2)); break;
            case "Falling Snow": gi.Stacks.Add(new MapItem("Falling Snow", zoom, "c26SnowFalling", w1)); break;
            case "Ground Snow": gi.Stacks.Add(new MapItem("Ground Snow", zoom, "c27SnowGround", w1)); break;
            case "Deep Snow": gi.Stacks.Add(new MapItem("Deep Snow", zoom, "c25SnowDeep", w1)); break;
            case "Ground/Falling Snow": gi.Stacks.Add(new MapItem("Ground Snow", zoom, "c27SnowGround", w1)); gi.Stacks.Add(new MapItem("Falling Snow", 1.0, "c26SnowFalling", w2)); break;
            case "Deep/Falling Snow": gi.Stacks.Add(new MapItem("Deep Snow", zoom, "c25SnowDeep", w1)); gi.Stacks.Add(new MapItem("Falling Snow", 1.0, "c26SnowFalling", w2)); break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "SetWeatherCounters(): reached default weatherRoll=" + weatherRolled);
               return false;
         }
         report.Weather = weatherRolled;
         return true;
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
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            returnStatus = "lastReport=null";
            Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(): " + returnStatus);
         }
         else
         {
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
               case GameAction.UpdateEventViewerDisplay: // Only change active event
                  break;
               case GameAction.PreparationsLoaderSpotSet:
               case GameAction.PreparationsCommanderSpotSet:
                  break;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.EndGameClose:
                  gi.GamePhase = GamePhase.EndGame;
                  break;
               case GameAction.PreparationsDeploymentRoll:
                  dieRoll = 88; // <cgs> TEST
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.IsPrepActive = true;
                  if (false == SetDeployment(gi, dieRoll))
                  {
                     returnStatus = "SetDeployment() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.PreparationsHatches:
                  gi.EventDisplayed = gi.EventActive = "e012";
                  break;
               case GameAction.PreparationsGunLoad:
                  gi.EventDisplayed = gi.EventActive = "e013";
                  ITerritory? t = Territories.theTerritories.Find("GunLoadHe");
                  if (null == t)
                  {
                     returnStatus = "theTerritories.Find() returned null for GunLoadHe";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(PreparationsGunLoad): " + returnStatus);
                  }
                  else
                  {
                     IMapItem gunLoad = new MapItem("GunLoad", 1.0, "c17GunLoad", t);
                     gi.GunLoads.Add(gunLoad);
                  }
                  break;
               case GameAction.PreparationsGunLoadSelect:
                  break;
               case GameAction.PreparationsTurret:
                  gi.IsTurretActive = true;
                  gi.EventDisplayed = gi.EventActive = "e014";
                  gi.Stacks.Add(new MapItem("Turret", 2.0, "c16Turret", gi.Home));
                  break;
               case GameAction.PreparationsTurretRotateLeft:
                  IMapItem? turretL = gi.Stacks.FindMapItem("Turret");
                  if (null == turretL)
                  {
                     returnStatus = "turret=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(PreparationsTurretRotateRight): " + returnStatus);
                  }
                  else
                  {
                     turretL.Count--;
                     if (turretL.Count < 0)
                        turretL.Count = 5;
                  }
                  break;
               case GameAction.PreparationsTurretRotateRight:
                  IMapItem? turretR = gi.Stacks.FindMapItem("Turret");
                  if (null == turretR)
                  {
                     returnStatus = "turret=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(PreparationsTurretRotateLeft): " + returnStatus);
                  }
                  else
                  {
                     turretR.Count++;
                     if (5 < turretR.Count)
                        turretR.Count = 0;
                  }
                  break;
               case GameAction.PreparationsLoaderSpot:
                  gi.IsTurretActive = false;
                  if (null == lastReport.Loader)
                  {
                     returnStatus = "lastReport.Loader=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(PreparationsLoaderSpot): " + returnStatus);
                  }
                  else
                  {
                     if (true == lastReport.Loader.IsButtonedUp)
                     {
                        gi.EventDisplayed = gi.EventActive = "e015";
                     }
                     else
                     {
                        if (null == lastReport.Commander)
                        {
                           returnStatus = "lastReport.Commander=null";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(PreparationsLoaderSpot): " + returnStatus);
                        }
                        else
                        {
                           TankCard card = new TankCard(lastReport.TankCardNum);
                           if ((true == lastReport.Commander.IsButtonedUp) && (false == card.myIsVisionCupola))
                              gi.EventDisplayed = gi.EventActive = "e016";
                           else
                              gi.EventDisplayed = gi.EventActive = "e017";
                        }
                     }
                  }
                  break;
               case GameAction.PreparationsCommanderSpot:
                  if (null == lastReport.Commander)
                  {
                     returnStatus = "lastReport.Commander=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(PreparationsLoaderSpot): " + returnStatus);
                  }
                  else
                  {
                     TankCard card = new TankCard(lastReport.TankCardNum);
                     if ((true == lastReport.Commander.IsButtonedUp) && (false == card.myIsVisionCupola))
                     {
                        gi.EventDisplayed = gi.EventActive = "e016";
                     }
                     else
                     {
                        action = GameAction.PreparationsFinal;
                        gi.GamePhase = GamePhase.Movement;
                        gi.EventDisplayed = gi.EventActive = "e017";
                        gi.DieRollAction = GameAction.MovementStartAreaSetRoll;
                        if (false == SetUsControlOnBattleMap(gi))
                        {
                           returnStatus = "SetUsControlOnBattleMap() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(): " + returnStatus);
                        }
                     }
                  }
                  break;
               case GameAction.PreparationsFinal:
                  gi.GamePhase = GamePhase.Movement;
                  gi.EventDisplayed = gi.EventActive = "e017";
                  gi.DieRollAction = GameAction.MovementStartAreaSetRoll;
                  if (false == SetUsControlOnBattleMap(gi))
                  {
                     returnStatus = "SetUsControlOnBattleMap() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(): " + returnStatus);
                  }
                  break;
               default:
                  returnStatus = "reached default for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(): " + returnStatus);
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
      private bool SetDeployment(IGameInstance gi, int dieRoll)
      {
         ITerritory? tState = Territories.theTerritories.Find("DeploymentState");
         if (null == tState)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetDeployment(): tState=null");
            return false;
         }
         IAfterActionReport? report = gi.Reports.GetLast(); // remove it from list
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetDeployment(): gi.Reports.GetLast() returned null");
            return false;
         }
         if ((12 == report.TankCardNum) || (13 == report.TankCardNum))
         {
            if (dieRoll < 9)
            {
               gi.IsHulledDown = true;
               gi.IsMoving = false;
               gi.IsLeadTank = false;
               gi.Stacks.Add(new MapItem("HullDown", 0.85, "c14HullDown", tState));
            }
            else if (dieRoll < 37)
            {
               gi.IsHulledDown = false;
               gi.IsMoving = false;
               if ((31 < dieRoll) && (dieRoll < 37))
                  gi.IsLeadTank = true;
               else
                  gi.IsLeadTank = false;
            }
            else if (dieRoll < 101)
            {
               gi.IsHulledDown = false;
               gi.IsMoving = true;
               if (63 < dieRoll)
                  gi.IsLeadTank = true;
               else
                  gi.IsLeadTank = false;
               gi.Stacks.Add(new MapItem("Moving", 0.85, "c13Moving", tState));
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "SetDeployment(): 12-13 reached default dieRoll=" + dieRoll.ToString());
               return false;
            }
         }
         else
         {
            if (dieRoll < 21)
            {
               gi.IsHulledDown = true;
               gi.IsMoving = false;
               gi.IsLeadTank = false;
               IMapItem miHulledDown = new MapItem("HullDown", 0.85, "c14HullDown", tState);
               gi.Stacks.Add(miHulledDown);
            }
            else if (dieRoll < 58)
            {
               gi.IsHulledDown = false;
               gi.IsMoving = false;
               if (57 == dieRoll)
                  gi.IsLeadTank = true;
               else
                  gi.IsLeadTank = false;
            }
            else if (dieRoll < 101)
            {
               gi.IsHulledDown = false;
               gi.IsMoving = true;
               if (90 < dieRoll)
                  gi.IsLeadTank = true;
               else
                  gi.IsLeadTank = false;
               IMapItem miMoving = new MapItem("Moving", 0.85, "c13Moving", tState);
               gi.Stacks.Add(miMoving);
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "SetDeployment(): 12-13 reached default dieRoll=" + dieRoll.ToString());
               return false;
            }
         }
         if (true == gi.IsLeadTank)
            report.Notes.Add("You are the Lead Tank!");
         return true;
      }
      private bool SetUsControlOnBattleMap(IGameInstance gi)
      {
         string name = "B1M";
         ITerritory? t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetUsControlOnBattleMap(): tState=" + name);
            return false;
         }
         gi.Stacks.Add(new MapItem("UsControl1", 1.0, "c28UsControl", t));
         //--------------------------------------
         name = "B2M";
         t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetUsControlOnBattleMap(): tState=" + name);
            return false;
         }
         gi.Stacks.Add(new MapItem("UsControl2", 1.0, "c28UsControl", t));
         //--------------------------------------
         name = "B3M";
         t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetUsControlOnBattleMap(): tState=" + name);
            return false;
         }
         gi.Stacks.Add(new MapItem("UsControl3", 1.0, "c28UsControl", t));
         return true;
      }
   }
   //-----------------------------------------------------
   class GameStateMovement : GameState
   {
      private static bool theIs1stEnemyStrengthCheck = true;
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string key = gi.EventActive;
         string returnStatus = "OK";
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            returnStatus = "lastReport=null";
            Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
         }
         else
         {
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
               case GameAction.UpdateEventViewerDisplay: // Only change active event
                  break;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.MovementStartAreaSet:
                  theIs1stEnemyStrengthCheck = true;
                  gi.EventDisplayed = gi.EventActive = "e018";
                  break;
               case GameAction.MovementStartAreaRestart:
                  if (false == MovementPhaseRestart(gi, lastReport))
                  {
                     returnStatus = "SetStartArea() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }

                  break;
               case GameAction.MovementStartAreaSetRoll:
                  gi.DieResults[key][0] = dieRoll;
                  if (false == SetStartArea(gi, dieRoll))
                  {
                     returnStatus = "SetStartArea() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementExitAreaSet:
                  gi.EventDisplayed = gi.EventActive = "e019";
                  gi.DieRollAction = GameAction.MovementExitAreaSetRoll;
                  break;
               case GameAction.MovementExitAreaSetRoll:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (false == SetExitArea(gi, dieRoll))
                  {
                     returnStatus = "SetExitArea() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementEnemyStrengthChoice:
                  gi.EventDisplayed = gi.EventActive = "e020";
                  gi.DieRollAction = GameAction.MovementExitAreaSetRoll;
                  break;
               case GameAction.MovementEnemyStrengthCheck:
                  if (false == theIs1stEnemyStrengthCheck)
                     AdvanceTime(lastReport, 15);
                  gi.EventDisplayed = gi.EventActive = "e021";
                  gi.DieRollAction = GameAction.MovementEnemyStrengthCheckRoll;
                  break;
               case GameAction.MovementEnemyStrengthCheckRoll:
                  theIs1stEnemyStrengthCheck = false;
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (false == SetEnemyStrengthCounter(gi, dieRoll))
                  {
                     returnStatus = "SetEnemyStrengthCounter() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementChooseOption:
                  SetChoicesForOperations(gi);
                  break;
               case GameAction.MovementArtillerySupportChoice:
                  gi.EventDisplayed = gi.EventActive = "e023";
                  break;
               case GameAction.MovementArtillerySupportCheck:
                  gi.EventDisplayed = gi.EventActive = "e024";
                  AdvanceTime(lastReport, 15);
                  gi.DieRollAction = GameAction.MovementArtillerySupportCheckRoll;
                  break;
               case GameAction.MovementArtillerySupportCheckRoll:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (false == SetArtillerySupportCounter(gi, dieRoll))
                  {
                     returnStatus = "SetArtillerySupportCounter() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementAirStrikeChoice:
                  gi.EventDisplayed = gi.EventActive = "e025";
                  break;
               case GameAction.MovementAirStrikeCheck:
                  gi.EventDisplayed = gi.EventActive = "e026";
                  AdvanceTime(lastReport, 30);
                  gi.DieRollAction = GameAction.MovementAirStrikeCheckRoll;
                  break;
               case GameAction.MovementAirStrikeCheckRoll:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (false == SetAirStrikeCounter(gi, dieRoll))
                  {
                     returnStatus = "SetAirStrikeCounter() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementAirStrikeCancel:
                  gi.IsAirStrikePending = false;
                  SetChoicesForOperations(gi);
                  break;
               case GameAction.MovementResupplyCheck:
                  AdvanceTime(lastReport, 60);
                  gi.EventDisplayed = gi.EventActive = "e027";
                  gi.DieRollAction = GameAction.MovementResupplyCheckRoll;
                  break;
               case GameAction.MovementResupplyCheckRoll:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.MovementEnterArea:
                  AdvanceTime(lastReport, 60);
                  gi.EventDisplayed = gi.EventActive = "e028";
                  break;
               case GameAction.MovementAdvanceFireCheck:
                  gi.EventDisplayed = gi.EventActive = "e029";
                  if (null == gi.EnteredArea)
                  {
                     returnStatus = "gi.EnteredArea=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(MovementEnterArea): " + returnStatus);
                  }
                  else
                  {
                     IMapItem? taskForce = gi.Stacks.FindMapItem("TaskForce");
                     if (null == taskForce)
                     {
                        returnStatus = "taskForce=null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(MovementEnterArea): " + returnStatus);
                     }
                     else if (false == AddMapItemMove(gi, taskForce, gi.EnteredArea))
                     {
                        returnStatus = "AddMapItemMove() returned faulse";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(MovementEnterArea): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.MovementAdvanceFire:
                  gi.IsAdvancingFireChosen = true;
                  if (false == EnterBoardArea(gi))
                  {
                     returnStatus = "SetAirStrikeCounter() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementAdvanceFireSkip:
                  if (false == EnterBoardArea(gi))
                  {
                     returnStatus = "SetAirStrikeCounter() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementStrengthRollBattleBoard:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.MovementResistanceCheckRoll;
                  if (false == SetEnemyStrengthCounter(gi, dieRoll))
                  {
                     returnStatus = "SetEnemyStrengthCounter() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementResistanceCheck:
                  gi.EventDisplayed = gi.EventActive = "e031";
                  gi.DieRollAction = GameAction.MovementResistanceCheckRoll;
                  break;
               case GameAction.MovementResistanceCheckRoll:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  switch (gi.BattleResistance)
                  {
                     case EnumResistance.Light:
                        if (7 < dieRoll)
                        {
                           if (false == SkipBattleBoard(gi, lastReport))
                           {
                              returnStatus = "ExitBattle() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        else // battle
                        {
                           if (false == EnterBattle(gi, lastReport))
                           {
                              returnStatus = "EnterBattle() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        break;
                     case EnumResistance.Medium:
                        if (5 < dieRoll)
                        {
                           if (false == SkipBattleBoard(gi, lastReport))
                           {
                              returnStatus = "ExitBattle() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        else // battle
                        {
                           if (false == EnterBattle(gi, lastReport))
                           {
                              returnStatus = "EnterBattle() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        break;
                     case EnumResistance.Heavy:
                        if (3 < dieRoll)
                        {
                           if (false == SkipBattleBoard(gi, lastReport))
                           {
                              returnStatus = "ExitBattle() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        else // battle
                        {
                           if (false == EnterBattle(gi, lastReport))
                           {
                              returnStatus = "EnterBattle() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        break;
                     default:
                        returnStatus = "reached default with resistance=" + gi.BattleResistance.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                        break;
                  }
                  break;
               case GameAction.EndGameClose:
                  gi.GamePhase = GamePhase.EndGame;
                  break;
               default:
                  returnStatus = "reached default for action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  break;
            }
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
      private bool SetStartArea(IGameInstance gi, int dieRoll)
      {
         string name = "M" + dieRoll.ToString() + "E";
         ITerritory? t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartArea(): startArea not found for " + name);
            return false;
         }
         IMapItem startArea = new MapItem("StartArea", 1.0, "c33StartArea", t);
         startArea.Count = dieRoll;
         gi.Stacks.Add(startArea);
         //-----------------------------------------
         if (0 == t.Adjacents.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartArea(): no adjacents for start area=" + name);
            return false;
         }
         ITerritory? adjacent = Territories.theTerritories.Find(t.Adjacents[0]); // should only be one adjacent to start area
         if (null == adjacent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartArea(): taskForceArea adjacent=" + t.Adjacents[0]);
            return false;
         }
         IMapItem taskForceArea = new MapItem("TaskForce", 1.0, "c35TaskForce", adjacent);
         gi.Stacks.Add(taskForceArea);
         //-----------------------------------------
         string name1 = t.Adjacents[0] + "R";
         ITerritory? controlled = Territories.theTerritories.Find(name1); // should only be one adjacent to start area
         if (null == controlled)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartArea(): controlled not found name=" + name1);
            return false;
         }
         name1 += ("UsControl" + Utilities.MapItemNum.ToString());
         Utilities.MapItemNum++;
         IMapItem usControl = new MapItem(name1, 1.0, "c28UsControl", controlled);
         usControl.Count = 0; // 0=us  1=light  2=medium  3=heavy
         gi.Stacks.Add(usControl);
         return true;
      }
      private bool SetExitArea(IGameInstance gi, int dieRoll)
      {
         --dieRoll;
         if (dieRoll < 0 || 9 < dieRoll)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetExitArea(): invalid dr=" + dieRoll.ToString());
            return false;
         }
         //-------------------------------------
         IMapItem? miStart = gi.Stacks.FindMapItem("StartArea");
         if (null == miStart)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetExitArea(): mi=null for StartArea");
            return false;
         }
         int sa = miStart.Count - 1;
         if (sa < 0 || 9 < sa)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetExitArea(): invalid dr=" + dieRoll.ToString());
            return false;
         }
         //-------------------------------------
         int exitArea = TableMgr.theExits[dieRoll, sa];
         string name = "M" + exitArea.ToString() + "E";
         ITerritory? t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetExitArea(): tState=" + name);
            return false;
         }
         IMapItem miExit = new MapItem("ExitArea", 1.0, "c34ExitArea", t);
         miExit.Count = exitArea;
         gi.Stacks.Add(miExit);
         return true;
      }
      private void SetChoicesForOperations(IGameInstance gi)
      {
         gi.DieResults["e021"][0] = Utilities.NO_RESULT;
         gi.DieResults["e024"][0] = Utilities.NO_RESULT;
         gi.DieResults["e026"][0] = Utilities.NO_RESULT;
         gi.DieResults["e027"][0] = Utilities.NO_RESULT;
         gi.EnemyStrengthCheck = null;
         gi.ArtillerySupportCheck = null;
         if (false == gi.IsAirStrikePending)
            gi.AirStrikeCheck = null;
         gi.EventDisplayed = gi.EventActive = "e022";
      }
      private bool SetEnemyStrengthCounter(IGameInstance gi, int dieRoll)
      {
         if (null == gi.EnemyStrengthCheck)
         {
            IMapItem? taskForce = gi.Stacks.FindMapItem("TaskForce");
            if (null == taskForce)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetEnemyStrengthCounter(): taskForce=null");
               return false;
            }
            gi.EnemyStrengthCheck = taskForce.TerritoryStarting;
         }
         if ("A" == gi.EnemyStrengthCheck.Type)
            dieRoll += 1;
         if ("C" == gi.EnemyStrengthCheck.Type)
            dieRoll += 2;
         //-----------------------------------------------
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetEnemyStrengthCounter(): report=null");
            return false;
         }
         //-----------------------------------------------
         EnumResistance resistance = EnumResistance.Heavy;
         switch (report.Resistance)
         {
            case EnumResistance.Light:
               if (dieRoll < 8)
                  resistance = EnumResistance.Light;
               else
                  resistance = EnumResistance.Medium;
               break;
            case EnumResistance.Medium:
               if (dieRoll < 6)
                  resistance = EnumResistance.Light;
               else if (dieRoll < 10)
                  resistance = EnumResistance.Medium;
               break;
            case EnumResistance.Heavy:
               if (dieRoll < 5)
                  resistance = EnumResistance.Light;
               else if (dieRoll < 9)
                  resistance = EnumResistance.Medium;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "SetEnemyStrengthCounter(): reached default resistence=" + report.Resistance.ToString());
               return false;
         }
         //-------------------------------------
         string name = gi.EnemyStrengthCheck.Name;
         gi.BattleResistance = resistance;
         if (EnumResistance.Light == resistance)
         {
            name += ("StrengthLight" + Utilities.MapItemNum.ToString());
            IMapItem strengthMarker = new MapItem(name, 1.0, "c36Light", gi.EnemyStrengthCheck);
            strengthMarker.Count = 1;
            gi.Stacks.Add(strengthMarker);

         }
         else if (EnumResistance.Medium == resistance)
         {
            name += ("StrengthMedium" + Utilities.MapItemNum.ToString());
            IMapItem strengthMarker = new MapItem(name, 1.0, "c37Medium", gi.EnemyStrengthCheck);
            strengthMarker.Count = 2;
            gi.Stacks.Add(strengthMarker);
         }
         else if (EnumResistance.Heavy == resistance)
         {
            name += ("StrengthHeavy" + Utilities.MapItemNum.ToString());
            IMapItem strengthMarker = new MapItem(name, 1.0, "c38Heavy", gi.EnemyStrengthCheck);
            strengthMarker.Count = 3;
            gi.Stacks.Add(strengthMarker);
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "SetEnemyStrengthCounter(): reached default resistence=" + resistance.ToString());
            return false;
         }
         Utilities.MapItemNum++;
         if (true == gi.IsAirStrikePending)
         {
            if (null == gi.AirStrikeCheck)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetArtillerySupportCounter(): gi.AirStrikeCheck=null");
               return false;
            }
            gi.IsAirStrikePending = false;
            string nameAir = gi.AirStrikeCheck.Name + "Air" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            IMapItem airStrikeMarker = new MapItem(nameAir, 1.0, "c40AirStrike", gi.AirStrikeCheck);
            gi.Stacks.Add(airStrikeMarker);
         }
         return true;
      }
      private bool SetArtillerySupportCounter(IGameInstance gi, int dieRoll)
      {
         if (null == gi.ArtillerySupportCheck)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetArtillerySupportCounter(): gi.ArtillerySupportCheck=null");
            return false;
         }
         if (dieRoll < 8)
         {
            IMapItems artillerySupports = new MapItems();
            foreach (IStack stack in gi.Stacks)
            {
               foreach (IMapItem mi in stack.MapItems)
               {
                  if (true == mi.Name.Contains("Artillery"))
                     artillerySupports.Add(mi);
               }
            }
            if (2 < artillerySupports.Count) // May only have three artillery supports. Remove last one.
            {
               IMapItem? artillerySupport = artillerySupports[0];
               if (null == artillerySupport)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SetArtillerySupportCounter(): artillerySupport=null");
                  return false;
               }
               gi.Stacks.Remove(artillerySupport);
            }
            string name = gi.ArtillerySupportCheck.Name + "Artillery" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            IMapItem artillerSupportMarker = new MapItem(name, 1.0, "c39ArtillerySupport", gi.ArtillerySupportCheck);
            gi.Stacks.Add(artillerSupportMarker);
         }
         if (true == gi.IsAirStrikePending)
         {
            if (null == gi.AirStrikeCheck)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetArtillerySupportCounter(): gi.AirStrikeCheck=null");
               return false;
            }
            gi.IsAirStrikePending = false;
            string name = gi.AirStrikeCheck.Name + "Air" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            IMapItem airStrikeMarker = new MapItem(name, 1.0, "c40AirStrike", gi.AirStrikeCheck);
            gi.Stacks.Add(airStrikeMarker);
         }
         return true;
      }
      private bool SetAirStrikeCounter(IGameInstance gi, int dieRoll)
      {
         if (null == gi.AirStrikeCheck)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetAirStrikeCounter(): gi.AirStrikeCheck=null");
            return false;
         }
         if (dieRoll < 5)
         {
            IMapItems airStrikes = new MapItems();
            foreach (IStack stack in gi.Stacks)
            {
               foreach (IMapItem mi in stack.MapItems)
               {
                  if (true == mi.Name.Contains("Air"))
                     airStrikes.Add(mi);
               }
            }
            if (1 < airStrikes.Count) // May only have two air strikes. Remove last one.
            {
               IMapItem? airStrike = airStrikes[0];
               if (null == airStrike)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SetAirStrikeCounter(): mi=null");
                  return false;
               }
               gi.Stacks.Remove(airStrike);
            }
            gi.IsAirStrikePending = true;
         }
         return true;
      }
      private bool EnterBoardArea(IGameInstance gi)
      {
         IMapItem? taskForce = gi.Stacks.FindMapItem("TaskForce");
         if (null == taskForce)
         {
            Logger.Log(LogEnum.LE_ERROR, "EnterBoardArea(): taskForce= null");
            return false;
         }
         string name = taskForce.TerritoryStarting + "R";
         ITerritory? t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "EnterBoardArea(): t=null for " + name);
            return false;
         }
         IStack? stack = gi.Stacks.Find(t);
         if (null == stack)
         {
            gi.EventDisplayed = gi.EventActive = "e030";
            gi.DieRollAction = GameAction.MovementStrengthRollBattleBoard;
         }
         else
         {
            gi.EventDisplayed = gi.EventActive = "e031";
            gi.DieRollAction = GameAction.MovementResistanceCheckRoll;
         }
         return true;
      }
      private bool SkipBattleBoard(IGameInstance gi, IAfterActionReport report)
      {
         int basePoints = 1;
         if ((EnumScenario.Advance == report.Situation) || (EnumScenario.Battle == report.Situation))
            basePoints *= 2;
         report.VictoryPtsCaptureArea += basePoints;
         //------------------------------------
         gi.EventDisplayed = gi.EventActive = "e032";
         gi.DieRollAction = GameAction.DieRollActionNone;
         IMapItem? taskForce = gi.Stacks.FindMapItem("TaskForce");
         if (null == taskForce)
         {
            Logger.Log(LogEnum.LE_ERROR, "ExitBattle(): taskForce= null");
            return false;
         }
         string name = taskForce.TerritoryStarting + "R";
         ITerritory? t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "ExitBattle(): t=null for " + name);
            return false;
         }
         IStack? stack = gi.Stacks.Find(t);
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "ExitBattle(): taskForce= null");
            return false;
         }
         gi.Stacks.Remove(stack);
         //------------------------------------
         name += ("UsControl" + Utilities.MapItemNum.ToString());
         Utilities.MapItemNum++;
         IMapItem usControl = new MapItem(name, 1.0, "c28UsControl", t);
         usControl.Count = 0; // 0=us  1=light  2=medium  3=heavy
         gi.Stacks.Add(usControl);
         return true;
      }
      private bool EnterBattle(IGameInstance gi, IAfterActionReport report)
      {
         AdvanceTime(report, 15);
         gi.GamePhase = GamePhase.Battle;
         gi.EventDisplayed = gi.EventActive = "e032";
         return true;
      }
      private bool MovementPhaseRestart(IGameInstance gi, IAfterActionReport report)
      {
         int basePoints = 30;
         if ( (EnumScenario.Advance == report.Situation) || (EnumScenario.Battle == report.Situation) )
            basePoints *= 2;
         report.VictoryPtsKiaExitArea += basePoints;
         //--------------------------------------------------------------
         theIs1stEnemyStrengthCheck = true;
         gi.EventDisplayed = gi.EventActive = "e018";
         gi.DieRollAction = GameAction.DieRollActionNone;
         //--------------------------------------------------------------
         gi.EnemyStrengthCheck = null;
         gi.ArtillerySupportCheck = null;
         gi.AirStrikeCheck = null;
         gi.EnteredArea = null;
         //--------------------------------------------------------------
         gi.IsAirStrikePending = false;
         gi.IsAdvancingFireChosen = false;
         gi.BattleResistance = EnumResistance.None;
         gi.Stacks.Clear();
         gi.MapItemMoves.Clear();
         gi.EnteredHexes.Clear();
         //--------------------------------------------------------------
         try
         {
            Logger.Log(LogEnum.LE_RESET_ROLL_STATE, "MovementPhaseRestart(): resetting die rolls");
            foreach (KeyValuePair<string, int[]> kvp in gi.DieResults)
            {
               for (int i = 0; i < 3; ++i)
                  kvp.Value[i] = Utilities.NO_RESULT;
            }
         }
         catch (Exception)
         {
            Logger.Log(LogEnum.LE_ERROR, "MovementPhaseRestart(): reset rolls");
            return false;
         }
         return true;
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
            case GameAction.ShowCombatCalendarDialog:
            case GameAction.ShowAfterActionReportDialog:
            case GameAction.ShowInventoryDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
               break;
            case GameAction.UpdateEventViewerDisplay: // Only change active event
               break;
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
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
            case GameAction.ShowCombatCalendarDialog:
            case GameAction.ShowAfterActionReportDialog:
            case GameAction.ShowInventoryDialog:
            case GameAction.ShowRuleListingDialog:
            case GameAction.ShowEventListingDialog:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
               break;
            case GameAction.UpdateEventViewerDisplay: // Only change active event
               break;
            case GameAction.UpdateEventViewerActive: // Only change active event
               gi.EventDisplayed = gi.EventActive; // next screen to show
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