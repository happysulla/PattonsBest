using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Devices.Perception;
using Windows.Media.Playback;
using static Pattons_Best.EventViewerResolveRandomEvent;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;

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
            case GamePhase.BattleRoundSequence: return new GameStateBattleRoundSequence();
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
      protected bool LoadGame(ref IGameInstance gi, ref GameAction action)
      {
         gi.MoveStacks.Clear();
         gi.BattleStacks.Clear();
         gi.TankStacks.Clear();
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
         sb.Append(Environment.OSVersion.Version.Build.ToString());
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
         Screen? screen = Screen.PrimaryScreen;
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
      //------------
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
      protected bool SetWeatherCounters(IGameInstance gi)
      {
         IAfterActionReport? report = gi.Reports.GetLast(); // remove it from list
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetWeatherCounters(): report=null");
            return false;
         }
         string weatherRolled = report.Weather;
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
            case "Clear": gi.BattleStacks.Add(new MapItem("Clear", zoom, "c20Clear", w1)); break;
            case "Overcast": gi.BattleStacks.Add(new MapItem("Overcast", zoom, "c21Overcast", w1)); break;
            case "Fog": gi.BattleStacks.Add(new MapItem("Fog", zoom, "c22Fog", w1)); break;
            case "Mud": gi.BattleStacks.Add(new MapItem("Mud", zoom, "c23Mud", w1)); break;
            case "Mud/Overcast": gi.BattleStacks.Add(new MapItem("Mud", zoom, "c23Mud", w1)); gi.BattleStacks.Add(new MapItem("Overcast", 1.0, "c21Overcast", w2)); break;
            case "Falling Snow": gi.BattleStacks.Add(new MapItem("FallingSnow", zoom, "c26SnowFalling", w1)); break;
            case "Ground Snow": gi.BattleStacks.Add(new MapItem("GroundSnow", zoom, "c27SnowGround", w1)); break;
            case "Deep Snow": gi.BattleStacks.Add(new MapItem("DeepSnow", zoom, "c25SnowDeep", w1)); break;
            case "Falling and Ground Snow": gi.BattleStacks.Add(new MapItem("GroundSnow", zoom, "c27SnowGround", w1)); gi.BattleStacks.Add(new MapItem("FallingSnow", zoom, "c26SnowFalling", w2)); break;
            case "Falling and Deep Snow": gi.BattleStacks.Add(new MapItem("Dee Snow", zoom, "c25SnowDeep", w1)); gi.BattleStacks.Add(new MapItem("FallingSnow", zoom, "c26SnowFalling", w2)); break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "SetWeatherCounters(): reached default weatherRoll=" + weatherRolled);
               return false;
         }
         return true;
      }
      protected bool SetDeployment(IGameInstance gi, int dieRoll)
      {
         IAfterActionReport? report = gi.Reports.GetLast(); // remove it from list
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetDeployment(): gi.Reports.GetLast() returned null");
            return false;
         }
         if (12 == report.TankCardNum || 13 == report.TankCardNum)
         {
            if (dieRoll < 9)
            {
               gi.Sherman.IsHullDown = true;
               gi.Sherman.IsMoving = false;
               gi.IsLeadTank = false; // SetDeployment()
               gi.Sherman.IsHullDown = true;
            }
            else if (dieRoll < 37)
            {
               gi.Sherman.IsHullDown = false;
               gi.Sherman.IsMoving = false;
               if (31 < dieRoll && dieRoll < 37)
                  gi.IsLeadTank = true;
               else
                  gi.IsLeadTank = false; // SetDeployment()
            }
            else if (dieRoll < 101)
            {
               gi.Sherman.IsHullDown = false;
               gi.Sherman.IsMoving = true;
               if (63 < dieRoll)
                  gi.IsLeadTank = true;
               else
                  gi.IsLeadTank = false; // SetDeployment()
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
               gi.Sherman.IsHullDown = true;
               gi.Sherman.IsMoving = false;
               gi.IsLeadTank = false; // SetDeployment()
            }
            else if (dieRoll < 58)
            {
               gi.Sherman.IsHullDown = false;
               gi.Sherman.IsMoving = false;
               if (57 == dieRoll)
                  gi.IsLeadTank = true;
               else
                  gi.IsLeadTank = false; // SetDeployment()
            }
            else if (dieRoll < 101)
            {
               gi.Sherman.IsHullDown = false;
               gi.Sherman.IsMoving = true;
               if (90 < dieRoll)
                  gi.IsLeadTank = true;
               else
                  gi.IsLeadTank = false; // SetDeployment()
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "SetDeployment(): 12-13 reached default dieRoll=" + dieRoll.ToString());
               return false;
            }
         }
         if (true == gi.IsLeadTank)
         {
            StringBuilder sb = new StringBuilder( "At ");
            sb.Append(TableMgr.GetTime(report));
            sb.Append(", you are the Lead Tank!");
            report.Notes.Add(sb.ToString());
         }
         return true;
      }
      protected bool SetUsControlOnBattleMap(IGameInstance gi)
      {
         string name = "B1M";
         ITerritory? t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetUsControlOnBattleMap(): tState=" + name);
            return false;
         }
         gi.BattleStacks.Add(new MapItem("UsControl1", Utilities.ZOOM, "c28UsControl", t));
         //--------------------------------------
         name = "B2M";
         t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetUsControlOnBattleMap(): tState=" + name);
            return false;
         }
         gi.BattleStacks.Add(new MapItem("UsControl2", Utilities.ZOOM, "c28UsControl", t));
         //--------------------------------------
         name = "B3M";
         t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetUsControlOnBattleMap(): tState=" + name);
            return false;
         }
         gi.BattleStacks.Add(new MapItem("UsControl3", Utilities.ZOOM, "c28UsControl", t));
         return true;
      }
      protected bool SetStartArea(IGameInstance gi, int dieRoll)
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
         gi.MoveStacks.Add(startArea);
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
         IMapItem taskForceArea = new MapItem("TaskForce", 1.3, "c35TaskForce", adjacent);
         gi.MoveStacks.Add(taskForceArea);
         //-----------------------------------------
         string name1 = t.Adjacents[0];
         ITerritory? controlled = Territories.theTerritories.Find(name1); // should only be one adjacent to start area
         if (null == controlled)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartArea(): controlled not found name=" + t.Adjacents[0]);
            return false;
         }
         string miName = "UsControl" + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem usControl = new MapItem(miName, 1.0, "c28UsControl", controlled);
         usControl.Count = 0; // 0=us  1=light  2=medium  3=heavy
         IMapPoint mp = Territory.GetRandomPoint(controlled, usControl.Zoom * Utilities.theMapItemOffset);
         usControl.Location = mp;
         gi.MoveStacks.Add(usControl);
         return true;
      }
      protected bool SetExitArea(IGameInstance gi, int dieRoll)
      {
         --dieRoll;
         if (dieRoll < 0 || 9 < dieRoll)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetExitArea(): invalid dr=" + dieRoll.ToString());
            return false;
         }
         //-------------------------------------
         IMapItem? miStart = gi.MoveStacks.FindMapItem("StartArea");
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
         gi.MoveStacks.Add(miExit);
         return true;
      }
      protected bool SetEnemyStrengthCounter(IGameInstance gi, int dieRoll)
      {
         if (null == gi.EnemyStrengthCheckTerritory) // if null, set strength check territory to task force's current territory. This happens moving into a territory without a stength check marker.
         {
            IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
            if (null == taskForce)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetEnemyStrengthCounter(): taskForce=null");
               return false;
            }
            gi.EnemyStrengthCheckTerritory = taskForce.TerritoryCurrent;
         }
         if ("A" == gi.EnemyStrengthCheckTerritory.Type)
            dieRoll += 1;
         if ("C" == gi.EnemyStrengthCheckTerritory.Type)
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
         gi.BattleResistance = resistance;
         if (EnumResistance.Light == resistance)
         {
            string name = "StrengthLight" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem strengthMarker = new MapItem(name, 1.0, "c36Light", gi.EnemyStrengthCheckTerritory);
            strengthMarker.Count = 1;
            IMapPoint mp = Territory.GetRandomPoint(gi.EnemyStrengthCheckTerritory, strengthMarker.Zoom * Utilities.theMapItemOffset);
            strengthMarker.Location = mp;
            gi.MoveStacks.Add(strengthMarker);
         }
         else if (EnumResistance.Medium == resistance)
         {
            string name = "StrengthMedium" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem strengthMarker = new MapItem(name, 1.0, "c37Medium", gi.EnemyStrengthCheckTerritory);
            strengthMarker.Count = 2;
            IMapPoint mp = Territory.GetRandomPoint(gi.EnemyStrengthCheckTerritory, strengthMarker.Zoom * Utilities.theMapItemOffset);
            strengthMarker.Location = mp;
            gi.MoveStacks.Add(strengthMarker);
         }
         else if (EnumResistance.Heavy == resistance)
         {
            string name = "StrengthHeavy" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem strengthMarker = new MapItem(name, 1.0, "c38Heavy", gi.EnemyStrengthCheckTerritory);
            strengthMarker.Count = 3;
            IMapPoint mp = Territory.GetRandomPoint(gi.EnemyStrengthCheckTerritory, strengthMarker.Zoom * Utilities.theMapItemOffset);
            strengthMarker.Location = mp;
            gi.MoveStacks.Add(strengthMarker);
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "SetEnemyStrengthCounter(): reached default resistence=" + resistance.ToString());
            return false;
         }
         Utilities.MapItemNum++;
         if (true == gi.IsAirStrikePending)
         {
            if (null == gi.AirStrikeCheckTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetArtillerySupportCounter(): gi.AirStrikeCheckTerritory=null");
               return false;
            }
            gi.IsAirStrikePending = false;
            string nameAir = "Air" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem airStrikeMarker = new MapItem(nameAir, 1.0, "c40AirStrike", gi.AirStrikeCheckTerritory);
            gi.MoveStacks.Add(airStrikeMarker);
         }
         return true;
      }
      protected bool SetArtillerySupportCounter(IGameInstance gi, int dieRoll)
      {
         if (null == gi.ArtillerySupportCheck)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetArtillerySupportCounter(): gi.ArtillerySupportCheck=null");
            return false;
         }
         if (dieRoll < 8)
         {
            IMapItems artillerySupports = new MapItems();
            foreach (IStack stack in gi.MoveStacks)
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
               gi.MoveStacks.Remove(artillerySupport);
            }
            string name = gi.ArtillerySupportCheck.Name + "Artillery" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            IMapItem artillerySupportMarker = new MapItem(name, 1.0, "c39ArtillerySupport", gi.ArtillerySupportCheck);
            IMapPoint mp = Territory.GetRandomPoint(gi.ArtillerySupportCheck, artillerySupportMarker.Zoom * Utilities.theMapItemOffset);
            artillerySupportMarker.Location = mp;
            gi.MoveStacks.Add(artillerySupportMarker);
         }
         if (true == gi.IsAirStrikePending)
         {
            if (null == gi.AirStrikeCheckTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetArtillerySupportCounter(): gi.AirStrikeCheckTerritory=null");
               return false;
            }
            gi.IsAirStrikePending = false;
            string name = gi.AirStrikeCheckTerritory.Name + "Air" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            IMapItem airStrikeMarker = new MapItem(name, 1.0, "c40AirStrike", gi.AirStrikeCheckTerritory);
            IMapPoint mp = Territory.GetRandomPoint(gi.AirStrikeCheckTerritory, airStrikeMarker.Zoom * Utilities.theMapItemOffset);
            airStrikeMarker.Location = mp;
            gi.MoveStacks.Add(airStrikeMarker);
         }
         return true;
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
      protected bool ResetDieResults(IGameInstance gi)
      {
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
            case GameAction.ShowMovementDiagramDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateAfterActionReport:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
               break;
            case GameAction.TestingStartMorningBriefing:
               if (false == PerformAutoSetupSkipCrewAssignments(gi))
               {
                  returnStatus = "PerformAutoSetupSkipCrewAssignments() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               else
               {
                  gi.GamePhase = GamePhase.MorningBriefing;
                  gi.EventDisplayed = gi.EventActive = "e007";
                  gi.DieRollAction = GameAction.MorningBriefingWeatherRoll;
                  AddStartingTestingOptions(gi);
               }
               break;
            case GameAction.TestingStartPreparations:
               if (false == PerformAutoSetupSkipMorningBriefing(gi))
               {
                  returnStatus = "PerformAutoSetupSkipMorningBriefing() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               else
               {
                  gi.GamePhase = GamePhase.Preparations;
                  gi.EventDisplayed = gi.EventActive = "e011";
                  gi.DieRollAction = GameAction.PreparationsDeploymentRoll;
                  gi.Sherman.TerritoryCurrent = gi.Home;
                  gi.BattleStacks.Add(gi.Sherman);
                  AddStartingTestingOptions(gi);
               }
               break;
            case GameAction.TestingStartMovement:
               if (false == PerformAutoSetupSkipPreparations(gi))
               {
                  returnStatus = "PerformAutoSetupSkipPreparations() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               else
               {
                  gi.GamePhase = GamePhase.Movement;
                  gi.EventDisplayed = gi.EventActive = "e018";
                  gi.DieRollAction = GameAction.MovementStartAreaSetRoll;
                  AddStartingTestingOptions(gi);
               }
               break;
            case GameAction.TestingStartBattle:
               if (false == PerformAutoSetupSkipMovement(gi))
               {
                  returnStatus = "PerformAutoSetupSkipMovement() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               else
               {
                  gi.GamePhase = GamePhase.Battle;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  AddStartingTestingOptions(gi);
                  if (true == gi.IsAdvancingFireChosen)
                  {
                     gi.AdvancingFireMarkerCount = 6;
                     gi.EventDisplayed = gi.EventActive = "e033";
                     action = GameAction.BattleStart;
                  }
                  else
                  {
                     action = GameAction.BattleActivation;
                  }
               }
               break;
            case GameAction.TestingStartAmbush:
               if (false == PerformAutoSetupSkipBattleSetup(gi))
               {
                  returnStatus = "PerformAutoSetupSkipBattleSetup() returned false";
                  Logger.Log(LogEnum.LE_ERROR, "GameStateSetup.PerformAction(): " + returnStatus);
               }
               else
               {
                  gi.GamePhase = GamePhase.Battle;
                  gi.EventDisplayed = gi.EventActive = "e035";
                  gi.DieRollAction = GameAction.BattleAmbushRoll;
                  AddStartingTestingOptions(gi);
               }
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
                     if (false == PerformAutoSetup(gi, ref action))
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
      private bool PerformAutoSetup(IGameInstance gi, ref GameAction action)
      {
         return true;
      }
      private bool PerformAutoSetupSkipCrewAssignments(IGameInstance gi)
      {
         gi.NewMembers.Clear();
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipCrewAssignments(): gi.Reports.GetLast() returned null");
            return false;
         }
         gi.NewMembers.Add(report.Commander);
         gi.NewMembers.Add(report.Gunner);
         gi.NewMembers.Add(report.Loader);
         gi.NewMembers.Add(report.Driver);
         gi.NewMembers.Add(report.Assistant);
         foreach (IMapItem mi in gi.NewMembers) // assign crew ratings randomly
         {
            ICrewMember? cm = mi as ICrewMember;
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupCrewRatings(): cm=null");
               return false;
            }
            else
            {
               int dieRoll = Utilities.RandomGenerator.Next(1, 11);
               cm.Rating = (int)Math.Ceiling(dieRoll / 2.0);
            }
         }
         return true;
      }
      private bool PerformAutoSetupSkipMorningBriefing(IGameInstance gi)
      {
         if (false == PerformAutoSetupSkipCrewAssignments(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): lastReport=null");
            return false;
         }
         //---------------------------------
         int dieRoll = 0;
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): lastReport=null");
            return false;
         }
         //---------------------------------
         dieRoll = Utilities.RandomGenerator.Next(1, 11);             // assign wehater randomly
         lastReport.Weather = TableMgr.GetWeather(gi.Day, dieRoll);
         if (true == lastReport.Weather.Contains("Snow"))
         {
            dieRoll = Utilities.RandomGenerator.Next(1, 11);
            lastReport.Weather = TableMgr.GetWeatherSnow(gi.Day, dieRoll);
         }
         if (false == SetWeatherCounters(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): SetWeatherCounters() returned false");
            return false;
         }
         //--------------------------------------------------
         lastReport.MainGunWP = Utilities.RandomGenerator.Next(5, 15); // assign gun loads and ready rack randomly
         int unassignedCount = 97 - lastReport.MainGunWP;
         lastReport.MainGunHBCI = Utilities.RandomGenerator.Next(1, 11);
         unassignedCount -= lastReport.MainGunHBCI;
         int extraAmmoDieRoll = Utilities.RandomGenerator.Next(1, 11);
         if (6 < extraAmmoDieRoll)
         {
            unassignedCount += 30;
            lastReport.Ammo30CalibreMG += 10;
         }
         else if (2 < extraAmmoDieRoll)
         {
            unassignedCount += 20;
            lastReport.Ammo30CalibreMG += 10;
         }
         lastReport.MainGunHE = (int)Math.Ceiling(unassignedCount * 0.6);
         unassignedCount -= lastReport.MainGunHE;
         lastReport.MainGunAP = unassignedCount;
         //--------------------------------------------------
         int count = 4;
         string tName = "ReadyRackHe" + count.ToString();
         ITerritory? t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): t=null for " + tName);
            return false;
         }
         IMapItem rr1 = new MapItem("ReadyRackHe", 0.9, "c12RoundsLeft", t);
         rr1.Count = count;
         gi.ReadyRacks.Add(rr1);
         //--------------------------------------------------
         count = 2;
         tName = "ReadyRackAp" + count.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): t=null for " + tName);
            return false;
         }
         rr1 = new MapItem("ReadyRackAp", 0.9, "c12RoundsLeft", t);
         rr1.Count = count;
         gi.ReadyRacks.Add(rr1);
         //--------------------------------------------------
         count = 1;
         tName = "ReadyRackWp" + count.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): t=null for " + tName);
            return false;
         }
         rr1 = new MapItem("ReadyRackAWp", 0.9, "c12RoundsLeft", t);
         rr1.Count = count;
         gi.ReadyRacks.Add(rr1);
         //--------------------------------------------------
         count = 1;
         tName = "ReadyRackHbci" + count.ToString();
         t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): t=null for " + tName);
            return false;
         }
         rr1 = new MapItem("ReadyRackHbci", 0.9, "c12RoundsLeft", t);
         rr1.Count = count;
         gi.ReadyRacks.Add(rr1);
         //--------------------------------------------------
         if (false == TableMgr.SetTimeTrack(lastReport, gi.Day)) // passing sunrise and sunset
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): TableMgr.SetTimeTrack() returned false");
            return false;
         }
         dieRoll = Utilities.RandomGenerator.Next(1, 11);
         lastReport.SunriseHour += (int)Math.Floor(dieRoll * 0.5) + 1;
         lastReport.MainGunHE -= dieRoll * 2;
         lastReport.Ammo30CalibreMG -= dieRoll;
         return true;
      }
      private bool PerformAutoSetupSkipPreparations(IGameInstance gi)
      {
         if (false == PerformAutoSetupSkipMorningBriefing(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): PerformAutoSetupStartMorningBriefing() returned false");
            return false;
         }
         //------------------------------------
         int dieRoll = Utilities.RandomGenerator.Next(1, 11);
         if (false == SetDeployment(gi, dieRoll))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): SetDeployment() returned false");
            return false;
         }
         //------------------------------------
         ICrewMember? cm = gi.GetCrewMember("Commander");
         if (null == cm)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): cm is null for Commander");
            return false;
         }
         cm.IsButtonedUp = false;
         ITerritory? t = Territories.theTerritories.Find("CommanderHatch");
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): t null for CommanderHatch");
            return false;
         }
         IMapItem mi = new MapItem(cm.Role + "OpenHatch", 1.0, "c15OpenHatch", t);
         gi.Hatches.Add(mi);
         //------------------------------------
         cm = gi.GetCrewMember("Driver");
         if (null == cm)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): cm is null for Driver");
            return false;
         }
         cm.IsButtonedUp = false;
         t = Territories.theTerritories.Find("DriverHatch");
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): t null for DriverHatch");
            return false;
         }
         mi = new MapItem(cm.Role + "OpenHatch", 1.0, "c15OpenHatch", t);
         gi.Hatches.Add(mi);
         //------------------------------------
         cm = gi.GetCrewMember("Assistant");
         if (null == cm)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): cm is null for Assistant");
            return false;
         }
         cm.IsButtonedUp = false;
         t = Territories.theTerritories.Find("AssistantHatch");
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): t null for AssistantHatch");
            return false;
         }
         mi = new MapItem(cm.Role + "OpenHatch", 1.0, "c15OpenHatch", t);
         gi.Hatches.Add(mi);
         //------------------------------------
         ITerritory? t1 = Territories.theTerritories.Find("Spot4");
         if (null == t1)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): t null for Spot4");
            return false;
         }
         gi.BattleStacks.Add(new MapItem("LoaderSpot", 1.0, "c18LoaderSpot", t1));
         //------------------------------------
         if (false == SetUsControlOnBattleMap(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): SetUsControlOnBattleMap() returned false");
            return false;
         }
         //------------------------------------
         ITerritory? t11 = Territories.theTerritories.Find("GunLoadHe");
         if (null == t11)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): t11=null");
            return false;
         }
         IMapItem gunLoad = new MapItem("GunLoad", 1.0, "c17GunLoad", t11);
         gi.GunLoads.Add(gunLoad);
         //------------------------------------
         gi.Sherman.IsTurret = true;
         gi.Sherman.TerritoryCurrent = gi.Home;
         int delta = (int) (gi.Sherman.Zoom * Utilities.theMapItemOffset);
         gi.Sherman.Location.X = gi.Home.CenterPoint.X - delta;
         gi.Sherman.Location.Y = gi.Home.CenterPoint.Y - delta;
         gi.BattleStacks.Add(gi.Sherman);
         return true;
      }
      private bool PerformAutoSetupSkipMovement(IGameInstance gi)
      {
         if (false == PerformAutoSetupSkipPreparations(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): PerformAutoSetupStartPrep() returned false");
            return false;
         }
         //---------------------------------------------
         int dieRoll = Utilities.RandomGenerator.Next(1, 11);
         if (false == SetStartArea(gi, dieRoll))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): SetStartArea() returned false");
            return false;
         }
         //---------------------------------------------
         dieRoll = Utilities.RandomGenerator.Next(1, 11);
         if (false == SetExitArea(gi, dieRoll))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): SetStartArea() returned false");
            return false;
         }
         //---------------------------------------------
         IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
         if (null == taskForce)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): taskForce=null");
            return false;
         }
         int index = Utilities.RandomGenerator.Next(0, taskForce.TerritoryCurrent.Adjacents.Count);
         string tName = taskForce.TerritoryCurrent.Adjacents[index];
         int count = 10;
         while (true == tName.EndsWith("E") && --count < 0)
         {
            index = Utilities.RandomGenerator.Next(0, taskForce.TerritoryCurrent.Adjacents.Count);
            tName = taskForce.TerritoryCurrent.Adjacents[index];
         }
         if (true == tName.EndsWith("E"))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): tName =" + tName);
            tName = "M030";
         }
         gi.EnteredArea = Territories.theTerritories.Find(tName);
         if (null == gi.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): gi.EnteredArea =null for " + tName);
            return false;
         }
         taskForce.TerritoryCurrent = taskForce.TerritoryStarting = gi.EnteredArea;
         taskForce.Location = gi.EnteredArea.CenterPoint;
         //---------------------------------------------
         dieRoll = Utilities.RandomGenerator.Next(1, 11);
         if (false == SetEnemyStrengthCounter(gi, dieRoll)) // set strength in current territory
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): SetEnemyStrengthCounter() returned false");
            return false;
         }
         //---------------------------------------------
         gi.ArtillerySupportCheck = gi.EnteredArea;
         for (int i = 0; i < 3; ++i)
         {
            dieRoll = Utilities.RandomGenerator.Next(1, 11);
            if (false == SetArtillerySupportCounter(gi, dieRoll)) // set strength in current territory
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): SetArtillerySupportCounter() returned false");
               return false;
            }
         }
         //---------------------------------------------
         gi.AirStrikeCheckTerritory = gi.EnteredArea;
         for (int i = 0; i < 2; ++i)
         {
            dieRoll = Utilities.RandomGenerator.Next(1, 11);
            if (dieRoll < 5)
            {
               string nameAir = "Air" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem airStrikeMarker = new MapItem(nameAir, 1.0, "c40AirStrike", gi.AirStrikeCheckTerritory);
               gi.MoveStacks.Add(airStrikeMarker);
            }
         }
         return true;
      }
      private bool PerformAutoSetupSkipBattleSetup(IGameInstance gi)
      {

         if (false == PerformAutoSetupSkipMovement(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipBattleSetup(): PerformAutoSetupSkipMovement() returned false");
            return false;
         }
         //--------------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipBattleSetup():  lastReport=null");
            return false;
         }
         //--------------------------------------------------------
         int NumEnemyUnitsAppearing = 1; // <cgs> TEST - number of enemy units appearing
         for (int k = 0; k < NumEnemyUnitsAppearing; k++)
         {
            int die1 = Utilities.RandomGenerator.Next(0, 3);
            int die2 = Utilities.RandomGenerator.Next(0, 3);
            string? tName = null;
            if (0 == die1)
            {
               if (0 == die2)
                  tName = "B4C";
               else if (1 == die2)
                  tName = "B4M";
               else if (2 == die2)
                  tName = "B4L";
            }
            else if (1 == die1)
            {
               if (0 == die2)
                  tName = "B6C";
               else if (1 == die2)
                  tName = "B6M";
               else if (2 == die2)
                  tName = "B6L";
            }
            else if (2 == die1)
            {
               if (0 == die2)
                  tName = "B9C";
               else if (1 == die2)
                  tName = "B9M";
               else if (2 == die2)
                  tName = "B9L";
            }
            if (null == tName)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipBattleSetup():  tName=null");
               return false;
            }
            ITerritory? t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipBattleSetup():  t=null for tName=" + tName);
               return false;
            }
            //-------------------------------------------
            int diceRoll = 0;
            die1 = Utilities.RandomGenerator.Next(0, 10);
            die2 = Utilities.RandomGenerator.Next(0, 10);
            if (0 == die1 && 0 == die2)
               diceRoll = 100;
            else
               diceRoll = die1 + 10 * die2;
            diceRoll = 45; // <cgs> TEST - three tanks
            string enemyUnit = TableMgr.SetEnemyUnit(lastReport.Scenario, gi.Day, diceRoll);
            IMapItem? mi = null;
            string name = enemyUnit + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            switch (enemyUnit)
            {
               case "ATG":
                  mi = new MapItem(name, Utilities.ZOOM + 0.1, "c76UnidentifiedAtg", t);
                  break;
               case "LW":
                  mi = new MapItem(name, Utilities.ZOOM, "c91Lw", t);
                  break;
               case "MG":
                  mi = new MapItem(name, Utilities.ZOOM, "c92MgTeam", t);
                  break;
               case "PSW/SPW":
                  enemyUnit = "SPW";
                  name = enemyUnit + Utilities.MapItemNum;
                  Utilities.MapItemNum++;
                  mi = new MapItem(name, Utilities.ZOOM + 0.2, "c89Psw232", t);
                  mi.IsVehicle = true;
                  break;
               case "SPG":
                  mi = new MapItem(name, Utilities.ZOOM + 0.5, "c77UnidentifiedSpg", t);
                  mi.IsVehicle = true;
                  break;
               case "TANK":
                  mi = new MapItem(name, Utilities.ZOOM + 0.5, "c78UnidentifiedTank", t);
                  mi.IsVehicle = true;
                  mi.IsTurret = true;
                  break;
               case "TRUCK":
                  mi = new MapItem(name, Utilities.ZOOM + 0.3, "c88Truck", t);
                  mi.IsVehicle = true;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipBattleSetup(): reached default with enemyUnit=" + enemyUnit);
                  return false;
            }
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipBattleSetup(): mi=null");
               return false;
            }
            IMapPoint mp = Territory.GetRandomPoint(t, mi.Zoom * Utilities.theMapItemOffset);
            mi.Location = mp;
            gi.BattleStacks.Add(mi);
            //-----------------------------------------
            if (true == mi.IsVehicle)
            {
               double xDiff = mi.Location.X + mi.Zoom * Utilities.theMapItemOffset - gi.Home.CenterPoint.X;
               double yDiff = mi.Location.Y + mi.Zoom * Utilities.theMapItemOffset - gi.Home.CenterPoint.Y;
               mi.RotationHull = Math.Atan2(yDiff, xDiff) * 180 / Math.PI - 90;
               //-----------------------------------------
               die1 = Utilities.RandomGenerator.Next(0, 3);
               if (1 == die1)
               {
                  mi.RotationOffset = 150 + Utilities.RandomGenerator.Next(0, 60);
               }
               else if (2 == die1)
               {
                  if (0 == Utilities.RandomGenerator.Next(0, 2))
                     mi.RotationOffset = 35 + Utilities.RandomGenerator.Next(0, 115);
                  else
                     mi.RotationOffset = -35 - Utilities.RandomGenerator.Next(0, 115);
               }
            }
            //-----------------------------------------
            die1 = Utilities.RandomGenerator.Next(1, 11);
            string enemyTerrain = TableMgr.GetEnemyTerrain(lastReport.Scenario, gi.Day, "A", enemyUnit, die1);
         }
         return true;
      }
      private void AddStartingTestingOptions(IGameInstance gi)
      {
         gi.IsAdvancingFireChosen = false;
         //--------------------------------
         gi.IsLeadTank = true;
         //--------------------------------
         gi.Sherman.IsMoving = true;
         gi.Sherman.IsHullDown = false;
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
               case GameAction.ShowMovementDiagramDialog:
               case GameAction.ShowReportErrorDialog:
               case GameAction.ShowAboutDialog:
               case GameAction.EndGameShowFeats:
               case GameAction.UpdateAfterActionReport:
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
                  gi.DieResults[key][0] = dieRoll; // clicking on image either restarts next day or continues with MorningBriefingBegin
                  break;
               case GameAction.MorningBriefingBegin:
                  gi.EventDisplayed = gi.EventActive = "e007";
                  gi.DieRollAction = GameAction.MorningBriefingWeatherRoll;
                  break;
               case GameAction.MorningBriefingWeatherRoll:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  lastReport.Weather = TableMgr.GetWeather(gi.Day, dieRoll);
                  break;
               case GameAction.MorningBriefingWeatherRollEnd:
                  if (true == lastReport.Weather.Contains("Snow"))
                  {
                     gi.EventDisplayed = gi.EventActive = "e008"; // first need to roll for snow
                     gi.DieRollAction = GameAction.MorningBriefingSnowRoll;
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e009";
                  }
                  break;
               case GameAction.MorningBriefingSnowRoll:
                  lastReport.Weather = TableMgr.GetWeatherSnow(gi.Day, dieRoll);
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.MorningBriefingSnowRollEnd:
                  gi.EventDisplayed = gi.EventActive = "e009";
                  break;
               case GameAction.MorningBriefingAmmoReadyRackLoad:
                  break;
               case GameAction.MorningBriefingTimeCheck:
                  gi.EventDisplayed = gi.EventActive = "e010";
                  gi.DieRollAction = GameAction.MorningBriefingTimeCheckRoll;
                  break;
               case GameAction.MorningBriefingTimeCheckRoll:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (false == TableMgr.SetTimeTrack(lastReport, gi.Day))
                  {
                     returnStatus = "TableMgr.SetTimeTrack() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMorningBriefing.PerformAction(): " + returnStatus);
                  }
                  lastReport.SunriseHour += (int)Math.Floor(dieRoll * 0.5) + 1;
                  lastReport.MainGunHE -= dieRoll * 2;
                  lastReport.Ammo30CalibreMG -= dieRoll;
                  break;
               case GameAction.PreparationsDeployment:
                  gi.GamePhase = GamePhase.Preparations;
                  gi.EventDisplayed = gi.EventActive = "e011";
                  gi.DieRollAction = GameAction.PreparationsDeploymentRoll;
                  gi.Sherman.TerritoryCurrent = gi.Home;
                  gi.BattleStacks.Add(gi.Sherman);
                  if (false == SetWeatherCounters(gi))
                  {
                     returnStatus = "SetWeatherCounters() returned false";
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
               case GameAction.ShowMovementDiagramDialog:
               case GameAction.ShowReportErrorDialog:
               case GameAction.ShowAboutDialog:
               case GameAction.EndGameShowFeats:
               case GameAction.UpdateAfterActionReport:  
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
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.IsHatchesActive = true;
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
                  gi.IsHatchesActive = false;
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
                  gi.Sherman.IsTurret = true;
                  break;
               case GameAction.PreparationsTurretRotateLeft:
                  gi.Sherman.RotationTurret -= 60;
                  if (gi.Sherman.RotationTurret < 0)
                     gi.Sherman.RotationTurret = 300;
                  break;
               case GameAction.PreparationsTurretRotateRight:
                  gi.Sherman.RotationTurret += 60;
                  if (359 < gi.Sherman.RotationTurret)
                     gi.Sherman.RotationTurret = 0;
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
                           if (true == lastReport.Commander.IsButtonedUp && false == card.myIsVisionCupola)
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
                     if (true == lastReport.Commander.IsButtonedUp && false == card.myIsVisionCupola)
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
   }
   //-----------------------------------------------------
   class GameStateMovement : GameState
   {
      private static bool theIs1stEnemyStrengthCheckTerritory = true;
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            returnStatus = "lastReport=null";
            Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
         }
         else
         {
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
               case GameAction.ShowMovementDiagramDialog:
               case GameAction.ShowReportErrorDialog:
               case GameAction.ShowAboutDialog:
               case GameAction.EndGameShowFeats:
               case GameAction.UpdateStatusBar:
               case GameAction.UpdateBattleBoard:
               case GameAction.UpdateTankCard:
               case GameAction.UpdateShowRegion:
               case GameAction.UpdateEventViewerDisplay: // Only change active event
                  break;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.MovementStartAreaSet:
                  theIs1stEnemyStrengthCheckTerritory = true;
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
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.MovementEnemyStrengthCheckTerritory:
                  if (false == theIs1stEnemyStrengthCheckTerritory)
                     AdvanceTime(lastReport, 15);
                  gi.EventDisplayed = gi.EventActive = "e021";
                  gi.DieRollAction = GameAction.MovementEnemyStrengthCheckTerritoryRoll;
                  break;
               case GameAction.MovementEnemyStrengthCheckTerritoryRoll:
                  theIs1stEnemyStrengthCheckTerritory = false;
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (false == SetEnemyStrengthCounter(gi, dieRoll))
                  {
                     returnStatus = "SetEnemyStrengthCounter() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementChooseOption:
                  if (false == SetChoicesForOperations(gi))
                  {
                     returnStatus = "SetChoicesForOperations() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(MovementChooseOption): " + returnStatus);
                  }
                  break;
               case GameAction.MovementEnterAreaUsControl:
                  if (false == MoveTaskForceToNewArea(gi))
                  {
                     returnStatus = "MoveTaskForceToNewArea() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(MovementEnterAreaUsControl): " + returnStatus);
                  }
                  else
                  {
                     bool isCheckNeeded = false;
                     if (false == IsEnemyStrengthCheckNeededInAdjacent(gi, out isCheckNeeded))
                     {
                        returnStatus = "IsEnemyStrengthCheckNeededInAdjacent() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(MovementEnterAreaUsControl): " + returnStatus);
                     }
                     else
                     {
                        if (true == isCheckNeeded)
                        {
                           gi.EventDisplayed = gi.EventActive = "e020";
                           gi.DieRollAction = GameAction.DieRollActionNone;
                        }
                        else
                        {
                           gi.EventDisplayed = gi.EventActive = "e022";
                           gi.DieRollAction = GameAction.DieRollActionNone;
                        }
                     }
                  }
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
               case GameAction.MovementAirStrikeCheckTerritory:
                  gi.EventDisplayed = gi.EventActive = "e026";
                  AdvanceTime(lastReport, 30);
                  gi.DieRollAction = GameAction.MovementAirStrikeCheckTerritoryRoll;
                  break;
               case GameAction.MovementAirStrikeCheckTerritoryRoll:
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
                  if (false == SetChoicesForOperations(gi))
                  {
                     returnStatus = "SetChoicesForOperations() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(MovementAirStrikeCancel): " + returnStatus);
                  }
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
               case GameAction.MovementAdvanceFireChoice:
                  gi.EventDisplayed = gi.EventActive = "e029";
                  gi.DieRollAction = GameAction.MovementAdvanceFireAmmoUseRoll;
                  if (false == MoveTaskForceToNewArea(gi))
                  {
                     returnStatus = "MoveTaskForceToNewArea() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(MovementAdvanceFireChoice): " + returnStatus);
                  }
                  break;
               case GameAction.MovementAdvanceFireAmmoUseCheck:
                  gi.EventActive = gi.EventDisplayed = "e030";
                  gi.DieRollAction = GameAction.MovementAdvanceFireAmmoUseRoll;
                  gi.IsAdvancingFireChosen = true;
                  gi.AdvancingFireMarkerCount = 6 - (int)Math.Ceiling(lastReport.VictoryPtsFriendlyTank / 3.0);  // six minus friendly tank/3 (rounded up)
                  break;
               case GameAction.MovementAdvanceFireAmmoUseRoll:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  int heRoundsUsed = (int)Math.Floor(gi.DieResults[key][0] / 2.0);
                  int mgRoundsUsed = gi.DieResults[key][0];
                  lastReport.MainGunHE -= Math.Min(heRoundsUsed, lastReport.MainGunHE);
                  lastReport.Ammo30CalibreMG -= Math.Min(mgRoundsUsed, lastReport.Ammo30CalibreMG);
                  break;
               case GameAction.MovementAdvanceFire:
                  if (false == EnterBoardArea(gi))
                  {
                     returnStatus = "EnterBoardArea() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementAdvanceFireSkip:
                  if (false == EnterBoardArea(gi))
                  {
                     returnStatus = "EnterBoardArea() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementStrengthRollBattleBoard:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.MovementBattleCheckRoll;
                  if (false == SetEnemyStrengthCounter(gi, dieRoll))
                  {
                     returnStatus = "SetEnemyStrengthCounter() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MovementBattleCheck:
                  gi.EventDisplayed = gi.EventActive = "e032";
                  gi.DieRollAction = GameAction.MovementBattleCheckRoll;
                  break;
               case GameAction.MovementBattleCheckRoll:
                  Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "GameStateMovement.PerformAction(MovementBattleCheckRoll): " + gi.MoveStacks.ToString());
                  Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "GameStateMovement.PerformAction(MovementBattleCheckRoll): gi.MapItemMoves.Clear()");
                  gi.MapItemMoves.Clear();
                  dieRoll = 10; // <cgs> TEST - enforce combat
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  switch (gi.BattleResistance)
                  {
                     case EnumResistance.Light:
                        if (7 < dieRoll) // battle
                        {
                           if (false == StartBattle(gi, lastReport))
                           {
                              returnStatus = "EnterBattle() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        else
                        {
                           if (false == SkipBattleBoard(gi, lastReport))
                           {
                              returnStatus = "SkipBattleBoard() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        break;
                     case EnumResistance.Medium:
                        if (5 < dieRoll) // battle
                        {
                           if (false == StartBattle(gi, lastReport))
                           {
                              returnStatus = "EnterBattle() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        else
                        {
                           if (false == SkipBattleBoard(gi, lastReport))
                           {
                              returnStatus = "SkipBattleBoard() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        break;
                     case EnumResistance.Heavy:
                        if (3 < dieRoll) // battle
                        {
                           if (false == StartBattle(gi, lastReport))
                           {
                              returnStatus = "EnterBattle() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        else
                        {
                           if (false == SkipBattleBoard(gi, lastReport))
                           {
                              returnStatus = "SkipBattleBoard() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                           }
                        }
                        Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "GameStateMovement.PerformAction(MovementBattleCheckRoll): " + gi.MoveStacks.ToString());
                        break;
                     default:
                        returnStatus = "reached default with resistance=" + gi.BattleResistance.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
                        break;
                  }
                  break;
               case GameAction.EveningDebriefingStart:
                  gi.GamePhase = GamePhase.EveningDebriefing;
                  gi.EventDisplayed = gi.EventActive = "e100";
                  gi.DieRollAction = GameAction.DieRollActionNone;
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
      private bool SetChoicesForOperations(IGameInstance gi)
      {
         if (false == ResetDieResults(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "SetChoicesForOperations(): ResetDieResults() returned false");
            return false;
         }
         gi.EnemyStrengthCheckTerritory = null;
         gi.ArtillerySupportCheck = null;
         if (false == gi.IsAirStrikePending)
            gi.AirStrikeCheckTerritory = null;
         gi.EventDisplayed = gi.EventActive = "e022";
         return true;
      }
      private bool SetAirStrikeCounter(IGameInstance gi, int dieRoll)
      {
         if (null == gi.AirStrikeCheckTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetAirStrikeCounter(): gi.AirStrikeCheckTerritory=null");
            return false;
         }
         if (dieRoll < 5)
         {
            IMapItems airStrikes = new MapItems();
            foreach (IStack stack in gi.MoveStacks)
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
               gi.MoveStacks.Remove(airStrike);
            }
            gi.IsAirStrikePending = true;
         }
         return true;
      }
      private bool EnterBoardArea(IGameInstance gi)
      {
         if (null == gi.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "EnterBoardArea(): gi.EnteredArea=null");
            return false;
         }
         IStack? stack = gi.MoveStacks.Find(gi.EnteredArea);
         if (null != stack)
         {
            foreach (IMapItem mi1 in stack.MapItems)
            {
               if (true == mi1.Name.Contains("Strength"))
               {
                  gi.EventDisplayed = gi.EventActive = "e032";
                  gi.DieRollAction = GameAction.MovementBattleCheckRoll;
                  return true;
               }
            }
         }
         gi.EventDisplayed = gi.EventActive = "e031";
         gi.DieRollAction = GameAction.MovementStrengthRollBattleBoard;
         return true;
      }
      private bool SkipBattleBoard(IGameInstance gi, IAfterActionReport report)
      {
         int basePoints = 1;
         if (EnumScenario.Advance == report.Scenario || EnumScenario.Battle == report.Scenario)
            basePoints *= 2;
         report.VictoryPtsCaptureArea += basePoints;
         //------------------------------------
         gi.GamePhase = GamePhase.Movement;
         gi.EventDisplayed = gi.EventActive = "e033";
         gi.DieRollAction = GameAction.DieRollActionNone;
         IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
         if (null == taskForce)
         {
            Logger.Log(LogEnum.LE_ERROR, "ExitBattle(): taskForce= null");
            return false;
         }
         IStack? stack = gi.MoveStacks.Find(taskForce.TerritoryCurrent);
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "ExitBattle(): taskForce= null");
            return false;
         }
         string name = "UsControl" + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem usControl = new MapItem(name, 1.0, "c28UsControl", taskForce.TerritoryCurrent);
         usControl.Count = 0; // 0=us  1=light  2=medium  3=heavy
         IMapPoint mp = Territory.GetRandomPoint(taskForce.TerritoryCurrent, usControl.Zoom * Utilities.theMapItemOffset);
         usControl.Location = mp;
         stack.MapItems.Add(usControl);
         Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "SkipBattleBoard(): Added mi=" + usControl.Name + " t=" + taskForce.TerritoryCurrent.Name + " to " + gi.MoveStacks.ToString());
         //------------------------------------
         foreach (IMapItem mi in stack.MapItems)
         {
            if (true == mi.Name.Contains("Strength"))
            {
               stack.MapItems.Remove(mi);
               break;
            }
         }
         return true;
      }
      private bool MoveTaskForceToNewArea(IGameInstance gi)
      {
         if (null == gi.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveTaskForceToNewArea(): gi.EnteredArea=null");
            return false;
         }
         IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
         if (null == taskForce)
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveTaskForceToNewArea(): taskForce=null");
            return false;
         }
         Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "MoveTaskForceToNewArea(): TF Entering t=" + gi.EnteredArea.Name);
         if (false == AddMapItemMove(gi, taskForce, gi.EnteredArea))
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveTaskForceToNewArea(): AddMapItemMove() returned false");
            return false;
         }
         return true;
      }
      private bool IsEnemyStrengthCheckNeededInAdjacent(IGameInstance gi, out bool isCheckNeeded)
      {
         isCheckNeeded = false;
         if (null == gi.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEnemyStrengthCheckNeededInAdjacent(): gi.EnteredArea=null");
            return false;
         }
         //--------------------------------
         List<string> sTerritories = gi.EnteredArea.Adjacents;
         foreach (string s in sTerritories)  // Look at each adjacent territory
         {
            if (true == s.Contains("E")) // Ignore Entry or Exit Areas
               continue;
            Logger.Log(LogEnum.LE_SHOW_ENEMY_STRENGTH, "IsEnemyStrengthCheckNeededInAdjacent(): Checking territory=" + gi.EnteredArea.Name + " adj=" + s);
            ITerritory? t = Territories.theTerritories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEnemyStrengthCheckNeededInAdjacent(): t=null for s=" + s);
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_ENEMY_STRENGTH, "IsEnemyStrengthCheckNeededInAdjacent(): Checking territory=" + s);
            IStack? stack = gi.MoveStacks.Find(t);
            if (null == stack)
            {
               Logger.Log(LogEnum.LE_SHOW_ENEMY_STRENGTH, "IsEnemyStrengthCheckNeededInAdjacent(): no stack for=" + s + " in " + gi.MoveStacks.ToString());
               isCheckNeeded = true;
               return true;
            }
            else
            {
               bool isCounterInStack = false;
               foreach (IMapItem mi1 in stack.MapItems)
               {
                  if (true == mi1.Name.Contains("Strength") || true == mi1.Name.Contains("UsControl"))
                  {
                     Logger.Log(LogEnum.LE_SHOW_ENEMY_STRENGTH, "SetButtonStateEnemyStrength(): Found mi=" + mi1.Name + " for=" + s + " in " + gi.MoveStacks.ToString());
                     isCounterInStack = true;
                     break;
                  }
               }
               if (false == isCounterInStack)
               {
                  Logger.Log(LogEnum.LE_SHOW_ENEMY_STRENGTH, "SetButtonStateEnemyStrength(): no counter for=" + s + " in " + gi.MoveStacks.ToString());
                  isCheckNeeded = true;
                  return true;
               }
            }
         }
         return true;
      }
      private bool StartBattle(IGameInstance gi, IAfterActionReport report)
      {
         AdvanceTime(report, 15);
         gi.GamePhase = GamePhase.Battle;
         return true;
      }
      private bool MovementPhaseRestart(IGameInstance gi, IAfterActionReport report)
      {
         int basePoints = 10;
         if (EnumScenario.Advance == report.Scenario || EnumScenario.Battle == report.Scenario)
            basePoints *= 2;
         report.VictoryPtsCapturedExitArea += basePoints;
         //--------------------------------------------------------------
         theIs1stEnemyStrengthCheckTerritory = true;
         gi.EventDisplayed = gi.EventActive = "e018";
         gi.DieRollAction = GameAction.MovementStartAreaSetRoll;
         //--------------------------------------------------------------
         gi.EnemyStrengthCheckTerritory = null;
         gi.ArtillerySupportCheck = null;
         gi.AirStrikeCheckTerritory = null;
         gi.EnteredArea = null;
         //--------------------------------------------------------------
         gi.IsAirStrikePending = false;
         gi.IsAdvancingFireChosen = false;
         gi.BattleResistance = EnumResistance.None;
         gi.MoveStacks.Clear();
         gi.MapItemMoves.Clear();
         gi.EnteredHexes.Clear();
         //--------------------------------------------------------------
         if (false == ResetDieResults(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "MovementPhaseRestart(): ResetDieResults() returned false");
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
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            returnStatus = "lastReport=null";
            Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
         }
         else
         {
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
               case GameAction.ShowMovementDiagramDialog:
               case GameAction.ShowReportErrorDialog:
               case GameAction.ShowAboutDialog:
               case GameAction.EndGameShowFeats:
               case GameAction.UpdateStatusBar:
               case GameAction.UpdateBattleBoard:
               case GameAction.UpdateTankCard:
               case GameAction.UpdateShowRegion:
               case GameAction.UpdateAfterActionReport:
               case GameAction.UpdateEventViewerDisplay: // Only change active event
                  break;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.BattleStart:
                  gi.EventDisplayed = gi.EventActive = "e033";
                  break;
               case GameAction.BattleActivation:
                  break;
               case GameAction.BattlePlaceAdvanceFire:
                  if (null == gi.AdvanceFire)
                  {
                     returnStatus = "gi.AdvanceFire=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                  }
                  else
                  {
                     string name = "AdvanceFire" + Utilities.MapItemNum;
                     ++Utilities.MapItemNum;
                     IMapItem advanceFire = new MapItem(name, 1.0, "c44AdvanceFire", gi.AdvanceFire);
                     IMapPoint mp = Territory.GetRandomPoint(gi.AdvanceFire, advanceFire.Zoom * Utilities.theMapItemOffset);
                     advanceFire.Location = mp;
                     IStack? stack = gi.BattleStacks.Find(gi.AdvanceFire);
                     if (null == stack)
                     {
                        stack = new Stack(gi.AdvanceFire);
                        gi.BattleStacks.Add(stack);
                     }
                     stack.MapItems.Add(advanceFire);
                     //----------------------------------
                     --gi.AdvancingFireMarkerCount;
                     if (0 < gi.AdvancingFireMarkerCount)
                     {
                        action = GameAction.BattleStart;
                     }
                     else
                     {
                        gi.IsAdvancingFireChosen = false;
                        action = GameAction.BattleActivation;
                     }
                     //----------------------------------
                     gi.AdvanceFire = null;
                  }
                  break;
               case GameAction.BattleAmbushStart:
                  gi.EventDisplayed = gi.EventActive = "e035";
                  gi.DieRollAction = GameAction.BattleAmbushRoll;
                  break;
               case GameAction.BattleAmbushRoll:
                  if (true == lastReport.Weather.Contains("Rain") || true == lastReport.Weather.Contains("Fog") || true == lastReport.Weather.Contains("Falling"))
                     dieRoll--;
                  dieRoll = 1; // <cgs> TEST - ambush
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (dieRoll < 8)
                     gi.BattlePhase = BattlePhase.Ambush;
                  else
                     gi.BattlePhase = BattlePhase.Spotting;
                  break;
               case GameAction.BattleEmpty:
                  gi.GamePhase = GamePhase.Preparations;
                  gi.EventDisplayed = gi.EventActive = "e036";
                  break;
               case GameAction.BattleEmptyResolve:
                  if (false == ResolveEmptyBattleBoard(gi, lastReport))
                  {
                     returnStatus = "ResolveEmptyBattleBoard() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleAmbush: // Handled with EventViewerBattleAmbush class
                  break;
               case GameAction.BattleRandomEvent:
                  gi.BattlePhase = BattlePhase.AmbushRandomEvent;
                  gi.EventDisplayed = gi.EventActive = "e039";
                  gi.DieRollAction = GameAction.BattleRandomEventRoll;
                  break;
               case GameAction.BattleRandomEventRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     dieRoll = 29; // <cgs> TEST - panzerfaust attack
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     gi.GamePhase = GamePhase.BattleRoundSequence;                              // <<<<<<<<<<<<< Change to BattleRoundSequence
                     string randomEvent = TableMgr.GetRandomEvent(lastReport.Scenario, gi.DieResults[key][0]);
                     switch (randomEvent)
                     {
                        case "Time Passes":
                           gi.EventDisplayed = gi.EventActive = "e040";
                           AdvanceTime(lastReport, 15);
                           break;
                        case "Friendly Artillery":
                           action = GameAction.BattleResolveArtilleryFire;
                           break;
                        case "Enemy Artillery":
                           gi.EventDisplayed = gi.EventActive = "e042";
                           gi.DieRollAction = GameAction.BattleRoundSequenceEnemyArtilleryRoll;
                           break;
                        case "Mines":
                           if ( true == gi.Sherman.IsMoving )
                           {
                              gi.IsMinefieldAttack = true;
                              gi.EventDisplayed = gi.EventActive = "e043";
                              gi.DieRollAction = GameAction.BattleRoundSequenceMinefieldRoll;
                           }
                           else
                           {
                              gi.EventDisplayed = gi.EventActive = "e043a";
                           }
                           break;
                        case "Panzerfaust":
                           gi.EventDisplayed = gi.EventActive = "e044";
                           gi.DieRollAction = GameAction.BattleRoundSequencePanzerfaustSectorRoll;
                           break;
                        case "Harrassing Fire":
                           gi.EventDisplayed = gi.EventActive = "e045";
                           gi.DieRollAction = GameAction.DieRollActionNone;
                           break;
                        case "Friendly Advance":
                           break;
                        case "Enemy Reinfore":
                           break;
                        case "Enemy Advance":
                           break;
                        case "Flanking Fire":
                           break;
                        default:
                           returnStatus = "reached default with randomEvent=" + randomEvent;
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                           break;
                     }
                  }
                  break;
               case GameAction.BattleShermanKilled:
                  break;
               case GameAction.UpdateTankExplosion:
               case GameAction.UpdateTankBrewUp:
                  gi.BattleStacks.Remove(gi.Sherman);
                  break;
               case GameAction.EveningDebriefingStart:
                  gi.GamePhase = GamePhase.EveningDebriefing;
                  gi.EventDisplayed = gi.EventActive = "e100";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.BattleStacks.Clear();
                  break;
               case GameAction.EndGameClose:
                  gi.GamePhase = GamePhase.EndGame;
                  break;
               default:
                  returnStatus = "reached default with action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                  break;
            }
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
      private bool ResolveEmptyBattleBoard(IGameInstance gi, IAfterActionReport report)
      {
         if (null == gi.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveEmptyBattleBoard(): gi.EnteredArea=null");
            return false;
         }
         IStack? stack = gi.MoveStacks.Find(gi.EnteredArea);
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveEmptyBattleBoard(): stack=null");
            return false;
         }
         bool isCounterInStack = false;
         foreach (IMapItem mi1 in stack.MapItems)
         {
            if (true == mi1.Name.Contains("Strength"))
            {
               stack.MapItems.Remove(mi1);
               isCounterInStack = true;
               break;
            }
         }
         if (false == isCounterInStack)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveEmptyBattleBoard(): isCounterInStack=false");
            return false;
         }
         string miName = "UsControl" + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem usControl = new MapItem(miName, 1.0, "c28UsControl", gi.EnteredArea);
         usControl.Count = 0; // 0=us  1=light  2=medium  3=heavy
         IMapPoint mp = Territory.GetRandomPoint(gi.EnteredArea, usControl.Zoom * Utilities.theMapItemOffset);
         usControl.Location = mp;
         gi.MoveStacks.Add(usControl);
         //-----------------------------------
         int basePoints = 1;
         if (EnumScenario.Advance == report.Scenario || EnumScenario.Battle == report.Scenario)
            basePoints *= 2;
         report.VictoryPtsCaptureArea += basePoints;
         //-----------------------------------
         if (true == gi.IsDaylightLeft(report))
         {
            gi.GamePhase = GamePhase.Preparations;
            gi.EventDisplayed = gi.EventActive = "e011";
            gi.DieRollAction = GameAction.PreparationsDeploymentRoll;
         }
         else
         {
            gi.GamePhase = GamePhase.EveningDebriefing;
            gi.EventDisplayed = gi.EventActive = "e50";
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         return true;
      }
   }
   //-----------------------------------------------------
   class GameStateBattleRoundSequence : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         GamePhase previousPhase = gi.GamePhase;
         GameAction previousAction = action;
         GameAction previousDieAction = gi.DieRollAction;
         string previousEvent = gi.EventActive;
         string returnStatus = "OK";
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            returnStatus = "lastReport=null";
            Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
         }
         else
         {
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
               case GameAction.ShowMovementDiagramDialog:
               case GameAction.ShowReportErrorDialog:
               case GameAction.ShowAboutDialog:
               case GameAction.EndGameShowFeats:
               case GameAction.UpdateStatusBar:
               case GameAction.UpdateBattleBoard:
               case GameAction.UpdateTankCard:
               case GameAction.UpdateShowRegion:
               case GameAction.UpdateAfterActionReport:
               case GameAction.UpdateEventViewerDisplay: // Only change active event
                  break;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.BattleRoundSequenceStart:
                  gi.BattlePhase = BattlePhase.Spotting;
                  int smokeCount = 0;
                  foreach (IStack stack in gi.BattleStacks)
                  {
                     foreach (IMapItem mi in stack.MapItems)
                     {
                        if (true == mi.Name.Contains("Smoke"))
                           smokeCount++;
                     }
                  }
                  if (0 < smokeCount)
                  {
                     gi.EventDisplayed = gi.EventActive = "e037";
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     if (false == SpottingPhaseBegin(gi, ref action))
                     {
                        returnStatus = "SpottingPhaseBegin() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceSmokeDepletionEnd:
                  IMapItems removals = new MapItems();
                  IMapItems additions = new MapItems();
                  foreach (IStack stack in gi.BattleStacks)
                  {
                     foreach (IMapItem mi in stack.MapItems)
                     {
                        if (true == mi.Name.Contains("Smoke")) // remove white and gray smoke counters
                        {
                           removals.Add(mi);
                           if (true == mi.Name.Contains("SmokeWhite")) // exchange white with gray counters
                           {
                              string miName = "SmokeGrey" + Utilities.MapItemNum;
                              Utilities.MapItemNum++;
                              IMapItem smoke = new MapItem(miName, Utilities.ZOOM + 0.75, "c111Smoke1", mi.TerritoryCurrent);
                              IMapPoint mp = Territory.GetRandomPoint(mi.TerritoryCurrent, mi.Zoom * Utilities.theMapItemOffset);
                              smoke.Location = mp;
                              additions.Add(smoke);
                           }
                        }
                     }
                  }
                  foreach (IMapItem mi in removals)
                  {
                     gi.BattleStacks.Remove(mi);
                     Logger.Log(LogEnum.LE_SHOW_STACK_DEL, "GameStateBattleRoundSequence.PerformAction(): removing mi=" + mi.Name + " from BattleStacks=" + gi.BattleStacks.ToString());
                  }

                  foreach (IMapItem mi in additions)
                  {
                     gi.BattleStacks.Add(mi);
                     Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "GameStateBattleRoundSequence.PerformAction(): adding mi=" + mi.Name + " from BattleStacks=" + gi.BattleStacks.ToString());
                  }
                  //----------------------------------------------
                  if (false == SpottingPhaseBegin(gi, ref action))
                  {
                     returnStatus = "SpottingPhaseBegin() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceSpottingEnd:
                  action = GameAction.BattleRoundSequenceCrewOrders;
                  gi.BattlePhase = BattlePhase.Orders;
                  gi.EventDisplayed = gi.EventActive = "e038";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.BattleRoundSequenceCrewOrders:
                  break;
               case GameAction.BattleRoundSequenceAmmoOrders:
                  gi.EventDisplayed = gi.EventActive = "e051";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.BattleRoundSequenceEnemyArtilleryRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                     if (dieRoll < 7)
                        lastReport.VictoryPtsFriendlySquad += 1;
                     else if (dieRoll < 10)
                        lastReport.VictoryPtsFriendlySquad += 2;
                     else
                        lastReport.VictoryPtsFriendlySquad += 3;
                  }
                  else
                  {
                     if( false == CheckCrewMemberExposed(gi, ref action)) // calls SpottingPhaseBegin() if no collateral
                     {
                        returnStatus = "CheckCrewMemberExposed() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceMinefieldRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 2; // <cgs> TEST - minefield attack
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                   }
                  else
                  {
                     if (gi.DieResults[key][0] < 2)
                     {
                        lastReport.VictoryPtsFriendlyTank += 1;
                        if (false == SpottingPhaseBegin(gi, ref action))
                        {
                           returnStatus = "SpottingPhaseBegin() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                        }
                     }
                     else if (gi.DieResults[key][0] < 3)
                     {
                        gi.Sherman.IsThrownTrack = true; // BattleRoundSequenceMinefieldRoll
                        lastReport.Breakdown = "Thrown Track";
                        gi.Sherman.IsMoving = false;
                        gi.EventDisplayed = gi.EventActive = "e043b";
                        gi.DieRollAction = GameAction.BattleRoundSequenceMinefieldDisableRoll;
                     }
                     else // no effect
                     {
                        if (false == SpottingPhaseBegin(gi, ref action))
                        {
                           returnStatus = "SpottingPhaseBegin() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceMinefieldDisableRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 9; // <cgs> TEST - minefield results - driver wounded
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     if (9 == gi.DieResults[key][0])
                     {
                        gi.EventDisplayed = gi.EventActive = "e043c";
                        gi.DieRollAction = GameAction.BattleRoundSequenceMinefieldDriverWoundRoll;
                     }
                     else if (10 == gi.DieResults[key][0])
                     {
                        gi.EventDisplayed = gi.EventActive = "e043d";
                        gi.DieRollAction = GameAction.BattleRoundSequenceMinefieldAssistantWoundRoll;
                     }
                     else
                     {
                        if (false == SpottingPhaseBegin(gi, ref action))
                        {
                           returnStatus = "SpottingPhaseBegin() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceMinefieldDriverWoundRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     ICrewMember? cm = gi.GetCrewMember("Driver");
                     if(null == cm)
                     {
                        returnStatus = "GetCrewMember(Driver) returned null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        string woundResult = TableMgr.GetWounds(gi, cm, dieRoll, false, false, false);
                        if ("ERROR" == woundResult)
                        {
                           returnStatus = "GetWounds(Driver) returned ERROR";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                        }
                        StringBuilder sb1 = new StringBuilder("At ");
                        sb1.Append(TableMgr.GetTime(lastReport));
                        sb1.Append(", ");
                        sb1.Append(cm.Name);
                        sb1.Append(" suffered ");
                        sb1.Append(woundResult);
                        lastReport.Notes.Add(sb1.ToString());
                        //----------------------------------
                        if (false == SpottingPhaseBegin(gi, ref action))
                        {
                           returnStatus = "SpottingPhaseBegin() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                        }
                      }
                  }
                  break;
               case GameAction.BattleRoundSequenceMinefieldAssistantWoundRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     ICrewMember? cm = gi.GetCrewMember("Assistant");
                     if (null == cm)
                     {
                        returnStatus = "GetCrewMember(Assistant) returned null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        string woundResult = TableMgr.GetWounds(gi, cm, dieRoll, false, false, false);
                        if ("ERROR" == woundResult)
                        {
                           returnStatus = "GetWounds() returned ERROR";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                        }
                        StringBuilder sb1 = new StringBuilder("At ");
                        sb1.Append(TableMgr.GetTime(lastReport));
                        sb1.Append(", ");
                        sb1.Append(cm.Name);
                        sb1.Append(" suffered ");
                        sb1.Append(woundResult);
                        lastReport.Notes.Add(sb1.ToString());
                        //----------------------------------
                        if (false == SpottingPhaseBegin(gi, ref action))
                        {
                           returnStatus = "SpottingPhaseBegin() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequencePanzerfaustSectorRoll:
                  if( Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 6; // <cgs> Panzerfaust attack sector
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     char sector = '0';
                     switch (gi.DieResults[key][0])
                     {
                        case 1: sector = '1'; break;
                        case 2: sector = '2'; break;
                        case 3: sector = '3'; break;
                        case 4: case 5: sector = '4'; break;
                        case 6: case 7: case 8: sector = '6'; break;
                        case 9: case 10: sector = '9'; break;
                        default:
                           returnStatus = " reached default gi.DieResults[key][0]=" + gi.DieResults[key][0].ToString();
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                           break;
                     }
                     if ('0' != sector)
                     {
                        string tName1 = "B" + sector + "M";
                        string tName2 = "B" + sector + "C";
                        ITerritory? tPanzerfault = Territories.theTerritories.Find(tName2);
                        if( null == tPanzerfault )
                        {
                           returnStatus = "Unable to find tName=" + tName1;
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                        }
                        else
                        {
                           IStack? stack = gi.BattleStacks.Find(tName1);
                           bool isUsControl = false;

                           if (null != stack)
                           {
                              foreach (IMapItem mi in stack.MapItems)
                              {
                                 if (true == mi.Name.Contains("UsControl"))
                                    isUsControl = true;
                              }
                           }
                           if (true == isUsControl)
                           {
                              if (false == SpottingPhaseBegin(gi, ref action))
                              {
                                 returnStatus = "SpottingPhaseBegin() returned false";
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                              }
                           }
                           else
                           {
                              IStack? stackPanzerfaust = gi.BattleStacks.Find(tName2);
                              bool isAdvancingFire = false;
                              if (null != stackPanzerfaust)
                              {
                                 foreach (IMapItem mi11 in stackPanzerfaust.MapItems)
                                 {
                                    if (true == mi11.Name.Contains("AdvanceFire"))
                                       isAdvancingFire = true;
                                 }
                              }
                              string name = "Panzerfault" + Utilities.MapItemNum;
                              Utilities.MapItemNum++;
                              IMapItem enemyUnit = new MapItem(name, Utilities.ZOOM, "c107Panzerfaust", tPanzerfault); // add panzerfault to battleboard
                              gi.BattleStacks.Add(enemyUnit);
                              //-----------------------
                              gi.EventDisplayed = gi.EventActive = "e044a";
                              gi.DieRollAction = GameAction.BattleRoundSequencePanzerfaustAttackRoll;
                              gi.Panzerfaust = new PanzerfaustAttack(gi, enemyUnit, isAdvancingFire, sector);
                           }
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequencePanzerfaustAttackRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     dieRoll = 1; // <cgs> Panzerfaust To Attack
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     if (null == gi.Panzerfaust)
                     {
                        returnStatus = "gi.Panzerfaust=null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        int modifier = 0;
                        if (91 < gi.Panzerfaust.myDay)
                           modifier -= 1;
                        if (true == gi.Panzerfaust.myIsShermanMoving)
                           modifier -= 1;
                        if (true == gi.Panzerfaust.myIsLeadTank)
                           modifier -= 1;
                        if (true == gi.Panzerfaust.myIsAdvancingFireZone)
                           modifier += 3;
                        if (('1' == gi.Panzerfaust.mySector) || ('2' == gi.Panzerfaust.mySector) || ('3' == gi.Panzerfaust.mySector))
                           modifier -= 1;
                        int combo = gi.DieResults[key][0] + modifier;
                        int rollNeededForAttack = 0;
                        if (EnumScenario.Advance == lastReport.Scenario)
                           rollNeededForAttack = 4;
                        else if (EnumScenario.Battle == lastReport.Scenario)
                           rollNeededForAttack = 6;
                        else 
                           rollNeededForAttack = 3;
                        if (combo < rollNeededForAttack)
                        {
                           gi.EventDisplayed = gi.EventActive = "e044b";
                           gi.DieRollAction = GameAction.BattleRoundSequencePanzerfaustToHitRoll;
                        }
                        else
                        {
                           if (false == SpottingPhaseBegin(gi, ref action))
                           {
                              returnStatus = "SpottingPhaseBegin() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                           }
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequencePanzerfaustToHitRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     dieRoll = 1; // <cgs> Panzerfaust To Hit
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     if (null == gi.Panzerfaust)
                     {
                        returnStatus = "gi.Panzerfaust=null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        int modifierToHit = 0;
                        if (true == gi.Panzerfaust.myIsShermanMoving)
                           modifierToHit += 2;
                        if (true == gi.Panzerfaust.myIsAdvancingFireZone)
                           modifierToHit += 3;
                        int combo = gi.DieResults[key][0] + modifierToHit;
                        if( combo < 8 )
                        {
                           gi.EventDisplayed = gi.EventActive = "e044c";
                           gi.DieRollAction = GameAction.BattleRoundSequencePanzerfaustToKillRoll;
                        }
                        else
                        {
                           if (false == SpottingPhaseBegin(gi, ref action))
                           {
                              returnStatus = "SpottingPhaseBegin() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                           }
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequencePanzerfaustToKillRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     if( null == gi.Panzerfaust)
                     {
                        returnStatus = "gi.Panzerfaust=null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                     }
                     else
                     {
                        if (gi.DieResults[key][0] < 9)
                        {
                           gi.KillSherman(lastReport, "Panzerfaust");
                           string hitLocation = "Hull";
                           if ( 0 == Utilities.RandomGenerator.Next(2))
                              hitLocation = "Turret";
                           gi.Death = new ShermanDeath(gi, gi.Panzerfaust.myEnemyUnit, hitLocation, "Panzerfault");
                           action = GameAction.BattleRoundSequenceShermanKilled;
                        }
                        else
                        {
                           if (false == SpottingPhaseBegin(gi, ref action))
                           {
                              returnStatus = "SpottingPhaseBegin() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                           }
                        }
                     }
                  }
                  break;
               case GameAction.UpdateTankExplosion:
               case GameAction.UpdateTankBrewUp:
                  gi.BattleStacks.Remove(gi.Sherman);
                  break;
               case GameAction.BattleRoundSequenceHarrassingFire:
                  break;
               case GameAction.EveningDebriefingStart:
                  gi.GamePhase = GamePhase.EveningDebriefing;
                  gi.EventDisplayed = gi.EventActive = "e100";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.EndGameClose:
                  gi.GamePhase = GamePhase.EndGame;
                  break;
               default:
                  returnStatus = "reached default with action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  break;
            }
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateBattleRoundSequence.PerformAction():");
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
      private bool CheckCrewMemberExposed(IGameInstance gi, ref GameAction outAction)
      {
         bool isCrewExposed = false;
         string[] crewmembers = new string[5] { "Driver", "Assistant", "Commander", "Loader", "Gunner" };
         foreach (string crewmember in crewmembers)
         {
            ICrewMember? cm = gi.GetCrewMember(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "CheckCrewMemberExposed(): gi.GetCrewMember() returned null");
               return false;
            }
            foreach (IMapItem hatch in gi.Hatches)
            {
               if (true == hatch.Name.Contains(cm.Role))
               {
                  isCrewExposed = true;
                  break;
               }
            }
            if (true == isCrewExposed)
               break;
         }
         if (true == isCrewExposed)
         {
            outAction = GameAction.BattleCollateralDamageCheck;
            gi.NumCollateralDamage++; // check for collateral damage after resolving artillery roll
         }
         else if (false == SpottingPhaseBegin(gi, ref outAction))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckCrewMemberExposed(): SpottingPhaseBegin() returned false");
            return false;
         }
         return true;
      }
      private bool SpottingPhaseBegin(IGameInstance gi, ref GameAction outAction)
      {
         string[] crewmembers = new string[5] { "Driver", "Assistant", "Commander", "Loader", "Gunner" };
         foreach (string crewmember in crewmembers)
         {
            ICrewMember? cm = gi.GetCrewMember(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): gi.GetCrewMember() returned null");
               return false;
            }
            else
            {
               List<string>? spottedTerritories = Territory.GetSpottedTerritories(gi, cm);
               if (null == spottedTerritories)
               {
                  Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): GetSpottedTerritories() returned null");
                  return false;
               }
               if( true == Logger.theLogLevel[(int)LogEnum.LE_EVENT_VIEWER_SPOTTING])
               {
                  StringBuilder sb = new StringBuilder("GameStateBattle.PerformAction(): cm=");
                  sb.Append(cm.Role);
                  sb.Append(" spotting for territories=(");
                  int i = 0;
                  foreach (string s in spottedTerritories)
                  {
                     sb.Append(s);
                     if (++i != spottedTerritories.Count)
                        sb.Append(",");
                  }
                  sb.Append(")");
                  Logger.Log(LogEnum.LE_EVENT_VIEWER_SPOTTING, sb.ToString() );
               }
               if (0 < spottedTerritories.Count)
               {
                  outAction = GameAction.BattleRoundSequenceSpotting;
                  return true;
               }
            }
         }
         outAction = GameAction.BattleRoundSequenceCrewOrders;
         gi.BattlePhase = BattlePhase.Orders;
         gi.EventDisplayed = gi.EventActive = "e038";
         gi.DieRollAction = GameAction.DieRollActionNone;
         return true;
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
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            returnStatus = "lastReport=null";
            Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(): " + returnStatus);
         }
         else
         {
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
               case GameAction.ShowMovementDiagramDialog:
               case GameAction.ShowReportErrorDialog:
               case GameAction.ShowAboutDialog:
               case GameAction.EndGameShowFeats:
               case GameAction.UpdateStatusBar:
               case GameAction.UpdateBattleBoard:
               case GameAction.UpdateTankCard:
               case GameAction.UpdateShowRegion:
               case GameAction.UpdateAfterActionReport:
               case GameAction.UpdateEventViewerDisplay: // Only change active event
                  break;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.EveningDebriefingStart: // Only change active event
                  gi.EventDisplayed = gi.EventActive = "e100";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.EveningDebriefingRatingImprovement: // Only change active event
                  break;
               case GameAction.EveningDebriefingRatingImprovementEnd:
                  gi.EventDisplayed = gi.EventActive = "e101";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (false == UpdateForEveningDebriefing(gi, lastReport))
                  {
                     returnStatus = "UpdateForEveningDebriefing() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EveningDebriefingVictoryPointsCalculated:
                  gi.EventDisplayed = gi.EventActive = "e102";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  //----------------------------------------------
                  if( false == UpdatePromotion(gi, lastReport))
                  {
                     returnStatus = "UpdatePromotion() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EventDebriefPromotion:
                  gi.EventDisplayed = gi.EventActive = "e103";
                  gi.DieRollAction = GameAction.EventDebriefDecorationStart;
                  break;
               case GameAction.EventDebriefDecorationStart:
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.DieResults[key][0] = dieRoll;
                  break;
               case GameAction.EventDebriefDecorationContinue:
               case GameAction.EventDebriefDecorationBronzeStar:
               case GameAction.EventDebriefDecorationSilverStar:
               case GameAction.EventDebriefDecorationCross:
               case GameAction.EventDebriefDecorationHonor:
                  if( false == UpdateDecoration(gi, lastReport, action))
                  {
                     returnStatus = "UpdateDecoration() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EventDebriefDecorationHeart:
                  if (false == ResetDay(gi, lastReport))
                  {
                     returnStatus = "ResetDay() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EndGameClose:
                  gi.GamePhase = GamePhase.EndGame;
                  break;
               default:
                  returnStatus = "reached default with action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  break;
            }
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
      public bool UpdateForEveningDebriefing(IGameInstance gi, IAfterActionReport report)
      {
         int scenarioMultiplierKOGermanUnit = 1;
         int scenarioMultiplierCapturedMapArea = 1;
         int scenarioMultiplierLoseMapArea = 1;
         if ( EnumScenario.Advance == report.Scenario )
         {
            scenarioMultiplierKOGermanUnit = 1;
            scenarioMultiplierCapturedMapArea = 1;
            scenarioMultiplierLoseMapArea = 1;
         }
         else if (EnumScenario.Advance == report.Scenario)
         {
            scenarioMultiplierKOGermanUnit = 1;
            scenarioMultiplierCapturedMapArea = 2;
            scenarioMultiplierLoseMapArea = 3;
         }
         else if (EnumScenario.Advance == report.Scenario)
         {
            scenarioMultiplierKOGermanUnit = 2;
            scenarioMultiplierCapturedMapArea = 0;
            scenarioMultiplierLoseMapArea = 3;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateForEveningDebriefing(): report.Scenario=" + report.Scenario.ToString());
            return false;
         }
         //----------------------------------
         int totalYourKia = report.VictoryPtsYourKiaLightWeapon;
         totalYourKia += report.VictoryPtsYourKiaTruck;
         totalYourKia += report.VictoryPtsYourKiaSpwOrPsw * 2;
         totalYourKia += report.VictoryPtsYourKiaSPGun * 6;
         totalYourKia += report.VictoryPtsYourKiaPzIV * 7; 
         totalYourKia += report.VictoryPtsYourKiaPzV * 9;
         totalYourKia += report.VictoryPtsYourKiaPzVI * 12;
         totalYourKia += report.VictoryPtsYourKiaAtGun * 4;
         totalYourKia += report.VictoryPtsYourKiaFortifiedPosition * 2;
         report.VictoryPtsTotalYourTank = totalYourKia * scenarioMultiplierKOGermanUnit;
         //----------------------------------
         int totalFriendlyKia = report.VictoryPtsFriendlyKiaLightWeapon;
         totalFriendlyKia += report.VictoryPtsFriendlyKiaTruck;
         totalFriendlyKia += report.VictoryPtsFriendlyKiaSpwOrPsw * 2;
         totalFriendlyKia += report.VictoryPtsFriendlyKiaSPGun * 6;
         totalFriendlyKia += report.VictoryPtsFriendlyKiaPzIV * 7; 
         totalFriendlyKia += report.VictoryPtsFriendlyKiaPzV * 9;
         totalFriendlyKia += report.VictoryPtsFriendlyKiaPzVI * 12;
         totalFriendlyKia += report.VictoryPtsFriendlyKiaAtGun * 4;
         totalFriendlyKia += report.VictoryPtsFriendlyKiaFortifiedPosition * 2;
         totalFriendlyKia -= report.VictoryPtsFriendlyTank * 5;
         totalFriendlyKia -= report.VictoryPtsFriendlySquad * 3;
         report.VictoryPtsTotalFriendlyForces = totalFriendlyKia * scenarioMultiplierKOGermanUnit;
         //----------------------------------
         report.VictoryPtsTotalTerritory = (report.VictoryPtsCaptureArea + report.VictoryPtsCapturedExitArea) * scenarioMultiplierCapturedMapArea;
         report.VictoryPtsTotalTerritory -= (report.VictoryPtsLostArea * scenarioMultiplierLoseMapArea);
         //----------------------------------
         report.VictoryPtsTotalEngagement = report.VictoryPtsTotalYourTank + report.VictoryPtsTotalFriendlyForces + report.VictoryPtsTotalEngagement;
         gi.VictoryPtsTotalCampaign += report.VictoryPtsTotalEngagement;
         gi.PromotionPoints += report.VictoryPtsTotalEngagement;
         //----------------------------------
         report.DayEndedTime = TableMgr.GetTime(report);
         //----------------------------------
         return true;
      }
      public bool UpdatePromotion(IGameInstance gi, IAfterActionReport report)
      {
         string cmdrRank = report.Commander.Rank;
         switch (cmdrRank)
         {
            case "Sgt":
               if( 99 < gi.PromotionPoints )
               {
                  gi.PromotionDate = gi.Day;
                  report.Commander.Rank = "Ssg";
               }
               break;
            case "2Lt":
               if (199 < gi.PromotionPoints)
               {
                  gi.PromotionDate = gi.Day;
                  report.Commander.Rank = "2Lt";
               }
               break;
            case "1Lt":
               if (299 < gi.PromotionPoints)
               {
                  gi.PromotionDate = gi.Day;
                  report.Commander.Rank = "1Lt";
               }
               break;
            case "Cpt":
               if (399 < gi.PromotionPoints)
               {
                  gi.PromotionDate = gi.Day;
                  report.Commander.Rank = "Cpt";
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdatePromotion(): reached default cmdrRank=" + cmdrRank);
               return false;
         }
         return true;
      }
      public bool UpdateDecoration(IGameInstance gi, IAfterActionReport report, GameAction action)
      {
         ICrewMember? commander = gi.GetCrewMember("Commander");
         if (null == commander)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDecoration(): commander=null");
            return false;
         }
         StringBuilder sb1 = new StringBuilder(commander.Name);
         //----------------------------------------------------
         switch (action)
         {
            case GameAction.EventDebriefDecorationContinue:
               break;
            case GameAction.EventDebriefDecorationBronzeStar:
               sb1.Append(" received the Bronze Star.");
               report.Notes.Add(sb1.ToString());
               break;
            case GameAction.EventDebriefDecorationSilverStar:
               sb1.Append(" received the Silver Star.");
               report.Notes.Add(sb1.ToString());
               break;
            case GameAction.EventDebriefDecorationCross:
               sb1.Append(" received the Distinguished Cross.");
               report.Notes.Add(sb1.ToString());
               break;
            case GameAction.EventDebriefDecorationHonor:
               sb1.Append(" received the Medal of Honor.");
               report.Notes.Add(sb1.ToString());
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateDecoration(): reached default action=" + action.ToString());
               return false;
         }
         //---------------------------------------------
         if ("None" != commander.Wound)
         {
            gi.EventDisplayed = gi.EventActive = "e104";
            gi.NumPurpleHeart++;
         }
         else
         {
            if( false == ResetDay(gi, report))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateDecoration(): ResetDay(=null) returned false");
               return false;
            }
         }
         return true;
      }
      public bool ResetDay(IGameInstance gi, IAfterActionReport report)
      {
         ++gi.Day;
         ICombatCalendarEntry? newEntry = TableMgr.theCombatCalendarEntries[gi.Day];
         if (null == newEntry)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDecoration(): newEntry=null");
            return false;
         }
         IAfterActionReport newReport = new AfterActionReport(newEntry, report);
         gi.Reports.Add(newReport);
         gi.GamePhase = GamePhase.MorningBriefing;
         gi.EventDisplayed = gi.EventActive = "e006";
         gi.DieResults[gi.EventActive][0] = Utilities.NO_RESULT;
         gi.DieRollAction = GameAction.MorningBriefingCalendarRoll;
         //-------------------------------------------------------
         gi.BattlePhase = BattlePhase.Ambush;
         gi.ReadyRacks.Clear();
         gi.Hatches.Clear();
         gi.GunLoads.Clear();
         gi.CrewActions.Clear();
         gi.EnemyStrengthCheckTerritory = null;
         gi.ArtillerySupportCheck = null;
         gi.EnteredArea = null;
         gi.AdvanceFire = null;
         gi.IsTurretActive = false;
         gi.IsHatchesActive = false;
         gi.IsLeadTank = false;
         gi.IsAirStrikePending = false;
         gi.IsAdvancingFireChosen = false;
         gi.IsShermanFiring = false;
         gi.IsShermanFiringAtFront = false;
         gi.IsBrokenGunsight = false;
         gi.IsBrokenMgAntiAircraft = false;
         gi.IsCommanderRescuePerformed = false;
         gi.IsMinefieldAttack = false;
         gi.IsHarrassingFire = false;
         gi.AdvancingFireMarkerCount = 0;
         gi.BattleResistance = EnumResistance.None;
         gi.BrokenPeriscopes.Clear();
         gi.FirstShots.Clear();
         gi.AcquiredShots.Clear();
         gi.Death = null;
         gi.Panzerfaust = null;
         gi.NumCollateralDamage = 0;
         gi.MapItemMoves.Clear();
         gi.TankStacks.Clear();
         gi.MoveStacks.Clear();
         gi.BattleStacks.Clear();
         gi.EnteredHexes.Clear();
         //-------------------------------------------------------
         if ( false == ResetDieResults(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateDecoration(): ResetDieResults() returned false");
            return false;
         }
         return true;
      }
   }
   //-----------------------------------------------------
   class GameStateEnded : GameState
   {
      public override string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         string returnStatus = "OK";
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
            case GameAction.ShowMovementDiagramDialog:
            case GameAction.ShowReportErrorDialog:
            case GameAction.ShowAboutDialog:
            case GameAction.EndGameShowFeats:
            case GameAction.UpdateStatusBar:
            case GameAction.UpdateBattleBoard:
            case GameAction.UpdateTankCard:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateAfterActionReport:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
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
         string returnStatus = "OK";
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