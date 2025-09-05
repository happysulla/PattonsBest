using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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
      protected bool LoadGame(ref IGameInstance gi, ref GameAction action)
      {
         gi.MoveStacks.Clear();
         gi.BattleStacks.Clear();
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
      protected void AdvanceTime(IAfterActionReport report, int minToAdd)
      {
         report.SunriseMin += minToAdd;
         if (59 < report.SunriseMin)
         {
            report.SunriseHour++;
            report.SunriseMin %= 60;
         }
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
                  if (false == gi.SetCrewActionTerritory(aar.Commander))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewMembers(): SetCrewActionTerritory(Commander) returned false");
                     return false;
                  }
                  break;
               case "Gunner":
                  aar.Gunner = crewMember;
                  if (false == gi.SetCrewActionTerritory(aar.Gunner))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewMembers(): SetCrewActionTerritory(Gunner) returned false");
                     return false;
                  }
                  break;
               case "Loader":
                  aar.Loader = crewMember;
                  if (false == gi.SetCrewActionTerritory(aar.Loader))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewMembers(): SetCrewActionTerritory(Loader) returned false");
                     return false;
                  }
                  break;
               case "Driver":
                  aar.Driver = crewMember;
                  if (false == gi.SetCrewActionTerritory(aar.Driver))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewMembers(): SetCrewActionTerritory(Driver) returned false");
                     return false;
                  }
                  break;
               case "Assistant":
                  aar.Assistant = crewMember;
                  if (false == gi.SetCrewActionTerritory(aar.Assistant))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewMembers(): SetCrewActionTerritory(Assistant) returned false");
                     return false;
                  }
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "AssignNewCrewMembers(): Reached Default with role= " + crewMember.Role);
                  return false;
            }
         }
         return true;
      }
      protected bool ReplaceInjuredCrewmen(IGameInstance gi, out bool isCrewmanReplaced, string caller)
      {
         Logger.Log(LogEnum.LE_SHOW_CREW_REPLACE, "Replace_InjuredCrewmen(): ++++++++++called by " + caller); 
         isCrewmanReplaced = false;
         //--------------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Replace_InjuredCrewmen(): lastReport=null");
            return false;
         }
         //--------------------------------------------------------
         if (false == gi.SwitchMembers("Assistant")) // return assistant back to original position if moved
         {
            Logger.Log(LogEnum.LE_ERROR, "Replace_InjuredCrewmen(): SwitchMembers() returned false");
            return false;
         }
         //--------------------------------------------------------
         gi.CrewActions.Clear();
         //--------------------------------------------------------
         string[] crewmembers = new string[5] { "Commander", "Gunner", "Loader", "Driver", "Assistant" }; // switch incapacitated members with new crew members
         foreach (string crewmember in crewmembers)
         {
            ICrewMember? cm = gi.GetCrewMemberByRole(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "Replace_InjuredCrewmen(): cm=null for name=" + crewmember);
               return false;
            }
            if (true == cm.IsIncapacitated)
            {
               if ((GamePhase.EveningDebriefing == gi.GamePhase) && (0 == cm.WoundDaysUntilReturn)) // crewmen are not replaced if light wound and evening debrief
               {
                  cm.IsIncapacitated = false;
                  cm.Wound = "";
                  cm.SetBloodSpots(0);
                  continue;
               }
               isCrewmanReplaced = true;
               gi.InjuredCrewMembers.Add(cm);
               switch (cm.Role)
               {
                  case "Driver":
                     lastReport.Driver = new CrewMember("Driver", "Pvt", "c08Driver");
                     gi.NewMembers.Add(lastReport.Driver);
                     if (false == gi.SetCrewActionTerritory(lastReport.Driver))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Replace_InjuredCrewmen(): SetCrewActionTerritory(Driver) returned false");
                        return false;
                     }
                     Logger.Log(LogEnum.LE_SHOW_CREW_REPLACE, "Replace_InjuredCrewmen(): Replacing cm=" + cm.Name + " with new=" + lastReport.Driver.Name);
                     break;
                  case "Loader":
                     lastReport.Loader = new CrewMember("Loader", "Cpl", "c09Loader");
                     gi.NewMembers.Add(lastReport.Loader);
                     if (false == gi.SetCrewActionTerritory(lastReport.Loader))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Replace_InjuredCrewmen(): SetCrewActionTerritory(Loader) returned false");
                        return false;
                     }
                     Logger.Log(LogEnum.LE_SHOW_CREW_REPLACE, "Replace_InjuredCrewmen(): Replacing cm=" + cm.Name + " with new=" + lastReport.Loader.Name);
                     break;
                  case "Assistant":
                     lastReport.Assistant = new CrewMember("Assistant", "Pvt", "c10Assistant");
                     gi.NewMembers.Add(lastReport.Assistant);
                     if (false == gi.SetCrewActionTerritory(lastReport.Assistant))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Replace_InjuredCrewmen(): SetCrewActionTerritory(Assistant) returned false");
                        return false;
                     }
                     Logger.Log(LogEnum.LE_SHOW_CREW_REPLACE, "Replace_InjuredCrewmen(): Replacing cm=" + cm.Name + " with new=" + lastReport.Assistant.Name);
                     break;
                  case "Gunner":
                     lastReport.Gunner = new CrewMember("Gunner", "Cpl", "c11Gunner");
                     gi.NewMembers.Add(lastReport.Gunner);
                     if (false == gi.SetCrewActionTerritory(lastReport.Gunner))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Replace_InjuredCrewmen(): SetCrewActionTerritory(Gunner) returned false");
                        return false;
                     }
                     Logger.Log(LogEnum.LE_SHOW_CREW_REPLACE, "Replace_InjuredCrewmen(): Replacing cm=" + cm.Name + " with new=" + lastReport.Gunner.Name);
                     break;
                  case "Commander":
                     lastReport.Commander = new CrewMember("Commander", "Sgt", "c07Commander");
                     gi.NewMembers.Add(lastReport.Commander);
                     if (false == gi.SetCrewActionTerritory(lastReport.Commander))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Replace_InjuredCrewmen(): SetCrewActionTerritory(Commander) returned false");
                        return false;
                     }
                     Logger.Log(LogEnum.LE_SHOW_CREW_REPLACE, "Replace_InjuredCrewmen(): Replacing cm=" + cm.Name + " with new=" + lastReport.Commander.Name);
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "Replace_InjuredCrewmen(): cm=null for name=" + cm.Role);
                     return false;
               }
            }
         }
         if ((true == isCrewmanReplaced) && (GamePhase.EveningDebriefing != gi.GamePhase)) // replacing crewmen takes 30 minutes
         {
            AdvanceTime(lastReport, 30);
            Logger.Log(LogEnum.LE_SHOW_CREW_REPLACE, "Replace_InjuredCrewmen(): advancing time by 30 min gp=" + gi.GamePhase.ToString());
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
            case "Falling and Deep Snow": gi.BattleStacks.Add(new MapItem("Deep Snow", zoom, "c25SnowDeep", w1)); gi.BattleStacks.Add(new MapItem("FallingSnow", zoom, "c26SnowFalling", w2)); break;
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
         string tType = report.TankCardNum.ToString();
         TankCard card = new TankCard(report.TankCardNum);
         //-------------------------------------------
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
         //-------------------------------------------
         string[] crewmembers = new string[4] { "Driver", "Assistant", "Commander", "Loader"};
         foreach (string crewmember in crewmembers)
         {
            if ((crewmember == "Loader") && (false == card.myIsLoaderHatch))
               continue;
            ICrewMember? cm = gi.GetCrewMemberByRole(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetDeployment(): cm=null for " + crewmember);
               return false;
            }
            if (false == cm.IsButtonedUp) 
            {
               bool isHatchFound = false;
               foreach(IMapItem hatch in gi.Hatches)
               {
                  if (true == hatch.Name.Contains(cm.Role))
                     isHatchFound = true;
               }
               if( false == isHatchFound) // if no hatch found and not button up, then add the open hatch counter
               {
                  string tName = crewmember + "_Hatch";
                  ITerritory? t = Territories.theTerritories.Find(tName, tType);
                  if (null == t)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "SetDeployment(): cannot find tName=" + tName + " tType=" + tType);
                     return false;
                  }
                  IMapItem mi = new MapItem(cm.Role + "_OpenHatch", 1.0, "c15OpenHatch", t);
                  gi.Hatches.Add(mi);
               }
            }
         }
         //-------------------------------------------
         if (true == gi.IsLeadTank)
         {
            StringBuilder sb = new StringBuilder("At ");
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
         string mapItemName = "UsControl" + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         gi.BattleStacks.Add(new MapItem(mapItemName, Utilities.ZOOM, "c28UsControl", t));
         //--------------------------------------
         name = "B2M";
         t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetUsControlOnBattleMap(): tState=" + name);
            return false;
         }
         mapItemName = "UsControl" + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         gi.BattleStacks.Add(new MapItem(mapItemName, Utilities.ZOOM, "c28UsControl", t));
         //--------------------------------------
         name = "B3M";
         t = Territories.theTerritories.Find(name);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetUsControlOnBattleMap(): tState=" + name);
            return false;
         }
         mapItemName = "UsControl" + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         gi.BattleStacks.Add(new MapItem(mapItemName, Utilities.ZOOM, "c28UsControl", t));
         return true;
      }
      protected bool SetStartArea(IGameInstance gi, int dieRoll)
      {
         string name = "M" + dieRoll.ToString() + "E";
         ITerritory? tStartNum = Territories.theTerritories.Find(name);
         if (null == tStartNum)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartArea(): startArea not found for " + name);
            return false;
         }
         IMapItem startArea = new MapItem("StartArea", 1.0, "c33StartArea", tStartNum);
         startArea.Count = dieRoll;
         gi.MoveStacks.Add(startArea);
         //-----------------------------------------
         if (0 == tStartNum.Adjacents.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartArea(): no adjacents for start area=" + name);
            return false;
         }
         ITerritory? tStart = Territories.theTerritories.Find(tStartNum.Adjacents[0]); // should only be one adjacent to start area
         if (null == tStart)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartArea(): taskForceArea tStart=" + tStartNum.Adjacents[0]);
            return false;
         }
         IMapItem taskForce = new MapItem("TaskForce", 1.3, "c35TaskForce", tStart); // add task for to adjacent area
         gi.MoveStacks.Add(taskForce);
         //---------------------------------------------
         double offset = taskForce.Zoom * Utilities.theMapItemOffset;
         IMapPoint mp = new MapPoint(taskForce.Location.X + offset, taskForce.Location.Y + offset);
         EnteredHex firstHex = new EnteredHex(gi, tStart, ColorActionEnum.CAE_START, mp); // SetStartArea()
         if (true == firstHex.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartArea(): newHex.Ctor=true");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_ENTERED_HEX, "SetStartArea(): adding firstHex=" + firstHex.ToString());
         gi.EnteredHexes.Add(firstHex); // first one is StartArea
         //-----------------------------------------
         string miName = "UsControl" + Utilities.MapItemNum.ToString(); // Add USControl marker to area adjacent to StartArea
         Utilities.MapItemNum++;
         IMapItem usControl = new MapItem(miName, 1.0, "c28UsControl", tStart);
         usControl.Count = 0; // 0=us  1=light  2=medium  3=heavy
         IMapPoint mpUsControl = Territory.GetRandomPoint(tStart, usControl.Zoom * Utilities.theMapItemOffset);
         usControl.Location = mpUsControl;
         gi.MoveStacks.Add(usControl);
         //---------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Set_StartArea(): lastReport=null");
            return false;
         }
         lastReport.VictoryPtsCaptureArea++;
         Logger.Log(LogEnum.LE_SHOW_VP_CAPTURED_AREA, "Set_StartArea(): Get VPs for Start Area t=" + tStart.Name + " #=" + lastReport.VictoryPtsCaptureArea.ToString());
         //---------------------------------------------
         //if( false == SetStartAreaExtraEnemy(gi))  // <cgs> TEST - Set extra units on move board during setup
         //{
         //   Logger.Log(LogEnum.LE_ERROR, "SetStartArea(): SetStartAreaExtraEnemy() returned false");
         //   return false;
         //}
         return true;
      }
      protected bool SetStartAreaExtraEnemy(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartAreaExtraEnemy():  lastReport=null");
            return false;
         }
         //-----------------------------------
         IMapItem? startArea = null;
         foreach (IStack stack in gi.MoveStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.Name.Contains("Start"))
               {
                  startArea = mi;
                  break;
               }
            }
         }
         if (null == startArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartAreaExtraEnemy():  startArea=null");
            return false;
         }
         //-----------------------------------
         if (0 == startArea.TerritoryCurrent.Adjacents.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartAreaExtraEnemy(): no adjacents for start area=" + startArea.TerritoryCurrent.Name);
            return false;
         }
         ITerritory? tStart = Territories.theTerritories.Find(startArea.TerritoryCurrent.Adjacents[0]); // should only be one adjacent to start area
         if (null == tStart)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetStartAreaExtraEnemy(): taskForceArea tStart=" + startArea.TerritoryCurrent.Adjacents[0]);
            return false;
         }
         //-----------------------------------
         foreach (string s in tStart.Adjacents) // Add Enemy Units to adjacent areas in the Move Board next to tStart.
         {
            if (true == s.EndsWith("E"))
               continue;
            ITerritory? t1 = Territories.theTerritories.Find(s);
            if (null == t1)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetStartAreaExtraEnemy(): t=null for " + s);
               return false;
            }
            int diceRoll = 0;
            int die1 = Utilities.RandomGenerator.Next(0, 10);
            int die2 = Utilities.RandomGenerator.Next(0, 10);
            if (0 == die1 && 0 == die2)
               diceRoll = 100;
            else
               diceRoll = die1 + 10 * die2;
            string enemyUnit = TableMgr.SetEnemyUnit(lastReport.Scenario, gi.Day, diceRoll);
            IMapItem? mi = null;
            string nameEnemy = enemyUnit + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            switch (enemyUnit)
            {
               case "ATG":
                  mi = new MapItem(nameEnemy, 1.0, "c76UnidentifiedAtg", t1);
                  break;
               case "LW":
                  mi = new MapItem(nameEnemy, 1.0, "c91Lw", t1);
                  mi.IsSpotted = true;
                  break;
               case "MG":
                  mi = new MapItem(nameEnemy, 1.0, "c92MgTeam", t1);
                  mi.IsSpotted = true;
                  break;
               case "PSW/SPW":
                  enemyUnit = "SPW";
                  nameEnemy = enemyUnit + Utilities.MapItemNum;
                  Utilities.MapItemNum++;
                  mi = new MapItem(nameEnemy, 1.0, "c89Psw232", t1);
                  mi.IsVehicle = true;
                  mi.IsSpotted = true;
                  break;
               case "SPG":
                  mi = new MapItem(nameEnemy, 1.0, "c77UnidentifiedSpg", t1);
                  mi.IsVehicle = true;
                  break;
               case "TANK":
                  mi = new MapItem(nameEnemy, 1.0, "c78UnidentifiedTank", t1);
                  mi.IsVehicle = true;
                  mi.IsTurret = true;
                  break;
               case "TRUCK":
                  mi = new MapItem(nameEnemy, 1.0, "c88Truck", t1);
                  mi.IsVehicle = true;
                  mi.IsSpotted = true;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "SetStartAreaExtraEnemy(): reached default with enemyUnit=" + enemyUnit);
                  return false;
            }
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetStartAreaExtraEnemy(): mi=null");
               return false;
            }
            IMapPoint mpEnemy = Territory.GetRandomPoint(t1, mi.Zoom * Utilities.theMapItemOffset);
            mi.Location = mpEnemy;
            gi.MoveStacks.Add(mi);
         }
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
         Logger.Log(LogEnum.LE_SHOW_RESISTANCE, "SetEnemyStrengthCounter(): report resistance=" + report.Resistance.ToString() + " dr=" + dieRoll.ToString());
         switch (report.Resistance)
         {
            case EnumResistance.Light:
               if (dieRoll < 8)
                  gi.BattleResistance = EnumResistance.Light;
               else
                  gi.BattleResistance = EnumResistance.Medium;
               break;
            case EnumResistance.Medium:
               if (dieRoll < 6)
                  gi.BattleResistance = EnumResistance.Light;
               else if (dieRoll < 10)
                  gi.BattleResistance = EnumResistance.Medium;
               else
                  gi.BattleResistance = EnumResistance.Heavy;
               break;
            case EnumResistance.Heavy:
               if (dieRoll < 5)
                  gi.BattleResistance = EnumResistance.Light;
               else if (dieRoll < 9)
                  gi.BattleResistance = EnumResistance.Medium;
               else
                  gi.BattleResistance = EnumResistance.Heavy;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "SetEnemyStrengthCounter(): reached default report.resistence=" + report.Resistance.ToString());
               return false;
         }
         //-------------------------------------
         if (EnumResistance.Light == gi.BattleResistance)
         {
            string name = "StrengthLight" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem strengthMarker = new MapItem(name, 1.0, "c36Light", gi.EnemyStrengthCheckTerritory);
            strengthMarker.Count = 1;
            IMapPoint mp = Territory.GetRandomPoint(gi.EnemyStrengthCheckTerritory, strengthMarker.Zoom * Utilities.theMapItemOffset);
            strengthMarker.Location = mp;
            gi.MoveStacks.Add(strengthMarker);
         }
         else if (EnumResistance.Medium == gi.BattleResistance)
         {
            string name = "StrengthMedium" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem strengthMarker = new MapItem(name, 1.0, "c37Medium", gi.EnemyStrengthCheckTerritory);
            strengthMarker.Count = 2;
            IMapPoint mp = Territory.GetRandomPoint(gi.EnemyStrengthCheckTerritory, strengthMarker.Zoom * Utilities.theMapItemOffset);
            strengthMarker.Location = mp;
            gi.MoveStacks.Add(strengthMarker);
         }
         else if (EnumResistance.Heavy == gi.BattleResistance)
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
            Logger.Log(LogEnum.LE_ERROR, "SetEnemyStrengthCounter(): reached default gi.BattleResistance=" + gi.BattleResistance.ToString());
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
         else
         {
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
      protected bool ResetDieResults(IGameInstance gi)
      {
         try
         {
            Logger.Log(LogEnum.LE_RESET_ROLL_STATE, "ResetDieResults(): resetting die rolls");
            foreach (KeyValuePair<string, int[]> kvp in gi.DieResults)
            {
               for (int i = 0; i < 3; ++i)
                  kvp.Value[i] = Utilities.NO_RESULT;
            }
         }
         catch (Exception)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResetDieResults(): reset rolls");
            return false;
         }
         return true;
      }
      protected bool ResolveEmptyBattleBoard(IGameInstance gi, IAfterActionReport report)
      {
         //--------------------------------
         if (null == gi.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_EmptyBattleBoard(): gi.EnteredArea=null");
            return false;
         }
         IStack? stack = gi.MoveStacks.Find(gi.EnteredArea);
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_EmptyBattleBoard(): stack=null");
            return false;
         }
         //--------------------------------
         IMapItems removals = new MapItems();
         foreach (IMapItem mi in stack.MapItems) // remove all enemy units that may have been here due to retreat or enemy overrun
         {
            if (true == mi.IsEnemyUnit())
               removals.Add(mi);
         }
         foreach (IMapItem removal in removals)
            gi.MoveStacks.Remove(removal);
         //-----------------------------------
         bool isCounterInStack = false;
         foreach (IMapItem mi1 in stack.MapItems) // Remove Enemy Strength Counter in area
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
            Logger.Log(LogEnum.LE_ERROR, "Resolve_EmptyBattleBoard(): isCounterInStack=false");
            return false;
         }
         //-----------------------------------
         string miName = "UsControl" + Utilities.MapItemNum.ToString(); // Add US Control Marker in area
         Utilities.MapItemNum++;
         IMapItem usControl = new MapItem(miName, 1.0, "c28UsControl", gi.EnteredArea);
         usControl.Count = 0; // 0=us  1=light  2=medium  3=heavy
         IMapPoint mp = Territory.GetRandomPoint(gi.EnteredArea, usControl.Zoom * Utilities.theMapItemOffset);
         usControl.Location = mp;
         gi.MoveStacks.Add(usControl);
         //-----------------------------------
         bool isExitArea;
         if (false == gi.IsExitArea(out isExitArea)) // determine if this is exit area
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_EmptyBattleBoard(): IsExitArea() returned false");
            return false;
         }
         if (true == isExitArea)
         {
            report.VictoryPtsCapturedExitArea++;
            Logger.Log(LogEnum.LE_SHOW_VP_CAPTURED_AREA, "Resolve_EmptyBattleBoard(): captured exit area t=" + gi.EnteredArea.Name + " #=" + report.VictoryPtsCapturedExitArea.ToString());
         }
         else
         {
            report.VictoryPtsCaptureArea++;
            Logger.Log(LogEnum.LE_SHOW_VP_CAPTURED_AREA, "Resolve_EmptyBattleBoard(): captured area t=" + gi.EnteredArea.Name + " #=" + report.VictoryPtsCaptureArea.ToString());
         }
         //---------------------------------
         if (false == EnemiesOverrunToPreviousArea(gi))  // Resolve_EmptyBattleBoard()
         {
            Logger.Log(LogEnum.LE_ERROR, "Resolve_EmptyBattleBoard(): EnemiesOverrun_ToPreviousArea() returned false");
            return false;
         }
         //---------------------------------
         bool isInjuredCrewmanReplaced;
         if (false == ReplaceInjuredCrewmen(gi, out isInjuredCrewmanReplaced, "ResolveEmptyBattleBoard()")) // Resolve_EmptyBattleBoard
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveEmptyBattleBoard(): Replace_InjuredCrewmen() returned false");
            return false;
         }
         if (true == isInjuredCrewmanReplaced)
         {
            gi.EventDisplayed = gi.EventActive = "e062";
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         else
         {
            if (false == ResetToPrepareForBattle(gi, report)) // Resolve_EmptyBattleBoard()
            {
               Logger.Log(LogEnum.LE_ERROR, "Resolve_EmptyBattleBoard(): ResetToPrepareForBattle() returned false");
               return false;
            }
         }
         return true;
      }
      protected bool EnemiesOverrunToPreviousArea(IGameInstance gi)
      {
         if (0 < gi.AdvancingEnemies.Count) // only do this if advancing enemy
         {
            IAfterActionReport? lastReport = gi.Reports.GetLast();
            if (null == lastReport)
            {
               Logger.Log(LogEnum.LE_ERROR, "EnemiesOverrun_ToPreviousArea(): lastReport=null");
               return false;
            }
            if (0 == gi.EnteredHexes.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "EnemiesOverrun_ToPreviousArea(): gi.EnteredHexes.Count=0");
               return false;
            }
            //-------------------------------------------
            if (gi.EnteredHexes.Count < 2) // This is the start area so any enemy units advancing go to unknown location
            {
               Logger.Log(LogEnum.LE_SHOW_ENTERED_HEX, "EnemiesOverrun_ToPreviousArea(): gi.EnteredHexes.Count=" + gi.EnteredHexes.Count.ToString() + " from hexes=" + gi.EnteredHexes.toString());
               Logger.Log(LogEnum.LE_SHOW_OVERRUN_TO_PREVIOUS_AREA, "EnemiesOverrun_ToPreviousArea(): gi.EnteredHexes.Count=0");
               return true;
            }
            EnteredHex enteredHex = gi.EnteredHexes[gi.EnteredHexes.Count - 2];
            ITerritory? t = Territories.theTerritories.Find(enteredHex.TerritoryName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "EnemiesOverrun_ToPreviousArea(): Territories.theTerritories.Find() returned null for tName=" + enteredHex.TerritoryName);
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_ENTERED_HEX, "EnemiesOverrun_ToPreviousArea(): Getting last enterHex=" + enteredHex.ToString() + " from hexes=" + gi.EnteredHexes.toString());
            IStack? stack = gi.MoveStacks.Find(t);
            if (null == stack)
            {
               Logger.Log(LogEnum.LE_ERROR, "EnemiesOverrun_ToPreviousArea(): gi.MoveStacks.Find(enteredHex.TerritoryName) returned null for tNmae=" + enteredHex.TerritoryName);
               return false;
            }
            //------------------------------------
            foreach (IMapItem mi in stack.MapItems) // Remove any US Control marker. If one found, ensure victory points reflect losing one area
            {
               if (true == mi.Name.Contains("UsControl"))
               {
                  Logger.Log(LogEnum.LE_SHOW_OVERRUN_TO_PREVIOUS_AREA, "EnemiesOverrun_ToPreviousArea(): Removing UsControl from t=" + t.Name);
                  stack.MapItems.Remove(mi);
                  lastReport.VictoryPtsLostArea++;
                  Logger.Log(LogEnum.LE_SHOW_VP_CAPTURED_AREA, "EnemiesOverrun_ToPreviousArea(): Removing UsControl from t=" + t.Name + " #=" + lastReport.VictoryPtsLostArea.ToString());
                  break;
               }
            }
            bool isEnemyStrengthCheckFound = false;
            foreach (IMapItem mi in stack.MapItems) // If there is no enemy strength marker found, add one
            {
               if (true == mi.Name.Contains("Strength"))
               {
                  isEnemyStrengthCheckFound = true;
                  break;
               }
            }
            if (false == isEnemyStrengthCheckFound)
            {
               Logger.Log(LogEnum.LE_SHOW_OVERRUN_TO_PREVIOUS_AREA, "EnemiesOverrun_ToPreviousArea(): Adding Strenght Marker to t=" + t.Name);
               string name = "StrengthLight" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem strengthMarker = new MapItem(name, 1.0, "c36Light", t);
               stack.MapItems.Add(strengthMarker);
            }
            //------------------------------------
            foreach (IMapItem enemyUnit in gi.AdvancingEnemies)
            {
               Logger.Log(LogEnum.LE_SHOW_OVERRUN_TO_PREVIOUS_AREA, "EnemiesOverrun_ToPreviousArea(): Adding eu=" + enemyUnit.Name + " to " + t.Name);
               enemyUnit.IsMoving = false;
               enemyUnit.IsHullDown = false;
               enemyUnit.IsSpotted = false;
               enemyUnit.IsMovingInOpen = false;
               enemyUnit.IsWoods = false;
               enemyUnit.IsBuilding = false;
               enemyUnit.IsFortification = false;
               enemyUnit.IsThrownTrack = false;
               enemyUnit.Zoom = 1.0;
               enemyUnit.TerritoryCurrent = t;
               IMapPoint mp = Territory.GetRandomPoint(t, enemyUnit.Zoom * Utilities.theMapItemOffset); // add enemy unit to random location in area
               enemyUnit.Location.X = mp.X;
               enemyUnit.Location.Y = mp.Y;
               gi.MoveStacks.Add(enemyUnit);
               Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "EnemiesOverrun_ToPreviousArea(): Adding mi=" + enemyUnit.Name + " t=" + enemyUnit.TerritoryCurrent.Name + " to " + gi.MoveStacks.ToString());
            }
            gi.AdvancingEnemies.Clear();
         }
         return true;
      }
      protected bool ResetToPrepareForBattle(IGameInstance gi, IAfterActionReport report)
      {
         IMapItems removals = new MapItems();
         foreach (IStack stackR in gi.BattleStacks) // remove everything but the Sherman
         {
            foreach (IMapItem mi in stackR.MapItems)
            {
               if (false == mi.Name.Contains("Sherman"))
                  removals.Add(mi);
            }
         }
         foreach (IMapItem removal in removals)
            gi.BattleStacks.Remove(removal);
         //---------------------------------
         gi.AdvancingEnemies.Clear();
         //---------------------------------
         gi.IsHatchesActive = false;
         gi.IsRetreatToStartArea = false;
         //---------------------------------
         gi.IsShermanFirstShot = false;
         gi.IsShermanFiringAtFront = false;
         gi.IsShermanDeliberateImmobilization = false;
         gi.ShermanTypeOfFire = "";
         gi.NumSmokeAttacksThisRound = 0;
         gi.IsMainGunRepairAttempted = false;
         //---------------------------------
         gi.IsShermanTurretRotated = false; // Reset_ToPrepareForBattle()
         gi.ShermanRotationTurretOld = 0.0;
         //---------------------------------
         gi.IsMinefieldAttack = false;
         gi.IsHarrassingFireBonus = false; 
         gi.IsFlankingFire = false;
         gi.IsEnemyAdvanceComplete = false;
         gi.Panzerfaust = null;
         gi.NumCollateralDamage = 0;
         //---------------------------------
         gi.Sherman.RotationOffsetHull = 0.0;
         gi.Sherman.RotationTurret = 0.0;
         gi.Sherman.RotationHull = 0.0;
         gi.Sherman.IsMoving = false;
         gi.Sherman.IsHullDown = false;
         gi.Sherman.IsBoggedDown = false;
         //---------------------------------
         gi.BattlePhase = BattlePhase.None;
         //---------------------------------
         if ((true == gi.IsBrokenMainGun) || (true == gi.IsBrokenGunSight))
         {
            gi.GamePhase = GamePhase.EveningDebriefing;
            gi.EventDisplayed = gi.EventActive = "e100a";
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         else if (true == gi.IsDaylightLeft(report))
         {
            gi.GamePhase = GamePhase.Preparations;
            gi.EventDisplayed = gi.EventActive = "e011";
            gi.DieRollAction = GameAction.PreparationsDeploymentRoll;
         }
         else
         {
            gi.GamePhase = GamePhase.EveningDebriefing;
            gi.EventDisplayed = gi.EventActive = "e100";
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         return true;
      }
      protected bool FriendlyAdvanceCheck(IGameInstance gi, ref GameAction outAction)
      {
         gi.DieRollAction = GameAction.DieRollActionNone;
         //--------------------------------------------
         List<ITerritory> usControlledTerritory = new List<ITerritory>();
         foreach (IStack stack in gi.BattleStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.Name.Contains("UsControl"))
                  usControlledTerritory.Add(stack.Territory);
            }
         }
         //--------------------------------------------
         foreach (ITerritory t in usControlledTerritory)
         {
            if (3 != t.Name.Length)
            {
               Logger.Log(LogEnum.LE_ERROR, "FriendlyAdvanceCheck(): 3 != t.Name.Length for t=" + t.Name);
               return false;
            }
            string sector = t.Name[1].ToString();
            bool isEmptySectorAdjacentToUsControl = false;
            string adjName;
            string adjSector;
            ITerritory? adjTerritory = null;
            switch (sector)
            {
               case "1":
                  adjName = "B9M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  adjName = "B2M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  break;
               case "2":
                  adjName = "B1M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  adjName = "B3M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  break;
               case "3":
                  adjName = "B2M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  adjName = "B4M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  break;
               case "4":
                  adjName = "B3M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  adjName = "B6M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  break;
               case "6":
                  adjName = "B4M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  adjName = "B9M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  break;
               case "9":
                  adjName = "B6M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  adjName = "B1M";
                  adjSector = adjName[1].ToString();
                  adjTerritory = usControlledTerritory.Find(adjName);
                  if ((null == adjTerritory) && (false == Territory.IsEnemyUnitInSector(gi, adjSector)))
                     isEmptySectorAdjacentToUsControl = true;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "FriendlyAdvanceCheck(): reached default sector=" + sector.ToString());
                  return false;
            }
            if (true == isEmptySectorAdjacentToUsControl)
            {
               gi.EventDisplayed = gi.EventActive = "e046";
               outAction = GameAction.BattleRoundSequenceFriendlyAdvance;
               return true;
            }
         }
         gi.EventDisplayed = gi.EventActive = "e046a";
         return true;
      }
      protected bool HarrassingFireCheck(IGameInstance gi)
      {
         bool isEnemyLwOrMgAtMediumOrCloseRange = false;
         foreach (IStack stack in gi.BattleStacks)
         {
            ITerritory t = stack.Territory;
            if ("Home" == t.Name)
               continue;
            if (3 != t.Name.Length)
            {
               Logger.Log(LogEnum.LE_ERROR, "HarrassingFireCheck(): t.Name.Length=" + t.Name.Length.ToString() + " for t=" + t.Name);
               return false;
            }
            char range = t.Name[t.Name.Length - 1];
            foreach (IMapItem mi in stack.MapItems)
            {
               if( true == mi.IsEnemyUnit() )
               {
                  string enemyUnit = mi.GetEnemyUnit();
                  if ((("LW" == enemyUnit) || ("MG" == enemyUnit)) && (('C' == range) || ('M' == range)))
                  {
                     isEnemyLwOrMgAtMediumOrCloseRange = true;
                     break;
                  }
               }
            }
         }
         if (false == isEnemyLwOrMgAtMediumOrCloseRange)
         {
            gi.IsHarrassingFireBonus = true;
            return true;
         }
         //-------------------------------------------
         gi.IsHarrassingFireBonus = true;
         string[] closeTerritories = new string[6] { "B1C", "B2C", "B3C", "B4C", "B6C", "B9C" };
         foreach(string territory in closeTerritories)
         {
            IStack? stack = gi.BattleStacks.Find(territory);
            if( null == stack )
            {
               gi.IsHarrassingFireBonus = false;
               return true;
            }
            bool isControlled = false;
            foreach(IMapItem mapItem in stack.MapItems)
            {
               if ((true == mapItem.Name.Contains("UsControl")) || (true == mapItem.Name.Contains("AdvanceFire")))
               {
                  isControlled = true;
                  break;
               }
            }
            if( false == isControlled )
            {
               gi.IsHarrassingFireBonus = false;
               return true;
            }
         }
         return true;
      }
      protected bool EnemyAdvanceCheck(IGameInstance gi, ref GameAction outAction)
      {
         gi.DieRollAction = GameAction.DieRollActionNone;
         //--------------------------------------------
         List<ITerritory> usControlledTerritory = new List<ITerritory>();
         foreach (IStack stack in gi.BattleStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.Name.Contains("UsControl"))
                  usControlledTerritory.Add(stack.Territory);
            }
         }
         //--------------------------------------------
         List<string> possibleEnemyCapture = new List<string>();
         foreach (ITerritory t in usControlledTerritory)
         {
            if (3 != t.Name.Length)
            {
               Logger.Log(LogEnum.LE_ERROR, "EnemyAdvanceCheck(): 3 != t.Name.Length for t=" + t.Name);
               return false;
            }
            string sector = t.Name[1].ToString();
            string adjName;
            string adjSector;
            switch (sector)
            {
               case "1":
                  adjName = "B9M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  adjName = "B2M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  break;
               case "2":
                  adjName = "B1M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  adjName = "B3M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  break;
               case "3":
                  adjName = "B2M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  adjName = "B4M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  break;
               case "4":
                  adjName = "B3M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  adjName = "B6M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  break;
               case "6":
                  adjName = "B4M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  adjName = "B9M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  break;
               case "9":
                  adjName = "B6M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  adjName = "B1M";
                  adjSector = adjName[1].ToString();
                  if (true == Territory.IsEnemyUnitInSector(gi, adjSector))
                     possibleEnemyCapture.Add(t.Name);
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "EnemyAdvanceCheck(): reached default sector=" + sector.ToString());
                  return false;
            }
         }
         if (0 == possibleEnemyCapture.Count)
         {
            gi.EventDisplayed = gi.EventActive = "e048a";
            return true;
         }
         else
         {
            gi.EventDisplayed = gi.EventActive = "e048";
            outAction = GameAction.BattleRoundSequenceEnemyAdvance;
            int randNum = Utilities.RandomGenerator.Next(possibleEnemyCapture.Count);
            string tName = possibleEnemyCapture[randNum];
            IStack? stack = gi.BattleStacks.Find(tName);
            if (null == stack)
            {
               Logger.Log(LogEnum.LE_ERROR, "EnemyAdvanceCheck(): stack=null for tName=" + tName);
               return false;
            }
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.Name.Contains("UsControl"))
               {
                  gi.EnemyAdvance = stack.Territory;
                  return true;
               }
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "EnemyAdvanceCheck(): reached default");
         return false;
      }
      protected bool CreateMapItemMove(IGameInstance gi, IMapItem mi, ITerritory newT)
      {
         MapItemMove mim = new MapItemMove(Territories.theTerritories, mi, newT);
         if (true == mim.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): mim.CtorError=true for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): Invalid Parameter mim.NewTerritory=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (null == mim.BestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): Invalid Parameter mim.BestPath=null" + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         if (0 == mim.BestPath.Territories.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateMapItemMove(): Invalid State Territories.Count=" + mim.BestPath.Territories.Count.ToString() + " for start=" + mi.TerritoryStarting.ToString() + " for newT=" + newT.Name);
            return false;
         }
         Logger.Log(LogEnum.LE_VIEW_MIM_ADD, "CreateMapItemMove(): mi=" + mi.Name + " moving to t=" + newT.Name);
         gi.MapItemMoves.Insert(0, mim); // add at front
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
            case GameAction.ShowTankForcePath:
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
                  gi.EventDisplayed = gi.EventActive = "e008";
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
                     gi.EventDisplayed = gi.EventActive = "e034"; // TestingStartBattle
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
                  gi.NewMembers.Add(report.Commander); // GameStateSetup.PerformAction(SetupAssignCrewRating)
                  gi.NewMembers.Add(report.Gunner);    // GameStateSetup.PerformAction(SetupAssignCrewRating)
                  gi.NewMembers.Add(report.Loader);    // GameStateSetup.PerformAction(SetupAssignCrewRating)
                  gi.NewMembers.Add(report.Driver);    // GameStateSetup.PerformAction(SetupAssignCrewRating)
                  gi.NewMembers.Add(report.Assistant); // GameStateSetup.PerformAction(SetupAssignCrewRating)
               }
               break;
            case GameAction.SetupShowCombatCalendarCheck:
               gi.NewMembers.Clear();
               gi.GamePhase = GamePhase.MorningBriefing;
               gi.EventDisplayed = gi.EventActive = "e006";  // Setup_ShowCombatCalendarCheck
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
         gi.NewMembers.Clear();    //PerformAutoSetupSkipCrewAssignments()
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipCrewAssignments(): gi.Reports.GetLast() returned null");
            return false;
         }
         report.TankCardNum = 1; // <cgs> TEST - Set the TankCardNum
         //-------------------------------
         int randNum = Utilities.RandomGenerator.Next(3);
         if (0 == randNum)
            report.Scenario = EnumScenario.Advance;
         else if (1 == randNum)
            report.Scenario = EnumScenario.Battle;
         else
            report.Scenario = EnumScenario.Counterattack;
         report.Scenario = EnumScenario.Battle; // <cgs> TEST - KillYourTank - choose scenario
         //-------------------------------
         gi.NewMembers.Add(report.Commander);   // PerformAutoSetupSkipCrewAssignments()
         gi.NewMembers.Add(report.Gunner);      // PerformAutoSetupSkipCrewAssignments()
         gi.NewMembers.Add(report.Loader);      // PerformAutoSetupSkipCrewAssignments()
         gi.NewMembers.Add(report.Driver);      // PerformAutoSetupSkipCrewAssignments()
         gi.NewMembers.Add(report.Assistant);   // PerformAutoSetupSkipCrewAssignments()
         foreach (IMapItem mi in gi.NewMembers) // PerformAutoSetupSkipCrewAssignments() - assign crew ratings randomly
         {
            ICrewMember? cm = mi as ICrewMember;
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupCrewRatings(): cm=null");
               return false;
            }
            if (false == gi.SetCrewActionTerritory(cm))
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupCrewRatings(): SetCrewMemberTerritory() returned false");
               return false;
            }
            int dieRoll = Utilities.RandomGenerator.Next(1, 11);
            cm.Rating = (int)Math.Ceiling(dieRoll / 2.0);
         }
         gi.NewMembers.Clear();
         return true;
      }
      private bool PerformAutoSetupSkipMorningBriefing(IGameInstance gi)
      {
         if (false == PerformAutoSetupSkipCrewAssignments(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): PerformAutoSetupSkipCrewAssignments() return false");
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
         string tType = lastReport.TankCardNum.ToString();
         TankCard card = new TankCard(lastReport.TankCardNum);
         //---------------------------------
         dieRoll = Utilities.RandomGenerator.Next(1, 11);             // assign weather randomly
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
         int unassignedCount = card.myNumMainGunRound;
         if( true == gi.Sherman.Name.Contains("Sherman75"))
         {
            lastReport.MainGunHVAP = 0;
            lastReport.MainGunWP = Utilities.RandomGenerator.Next(5, 15); // assign gun loads and ready rack randomly
            unassignedCount -= lastReport.MainGunWP;
            lastReport.MainGunHBCI = Utilities.RandomGenerator.Next(1, 11);
            unassignedCount -= (lastReport.MainGunWP + lastReport.MainGunHBCI);
         }
         else
         {
            lastReport.MainGunWP = 0;
            lastReport.MainGunHBCI = 0;
            lastReport.MainGunHVAP = Utilities.RandomGenerator.Next(2, 4);
            unassignedCount -= lastReport.MainGunHVAP;
         }
         //--------------------------------------------------
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
         //lastReport.MainGunHE = 5;    // <cgs> Limit initial gun loads
         //lastReport.MainGunAP = 3;    // <cgs> Limit initial gun loads
         //lastReport.MainGunWP = 0;    // <cgs> Limit initial gun loads
         //lastReport.MainGunHBCI = 3;  // <cgs> Limit initial gun loads
         //--------------------------------------------------
         int count = 2;
         string tName = "ReadyRackAp" + count.ToString();
         ITerritory? t = Territories.theTerritories.Find(tName, tType);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): t=null for " + tName);
            return false;
         }
         IMapItem rr1 = new MapItem("ReadyRackAp", 0.9, "c12RoundsLeft", t);
         rr1.Count = count;
         gi.ReadyRacks.Add(rr1);
         //--------------------------------------------------
         if (true == gi.Sherman.Name.Contains("Sherman75"))
         {
            count = 1;
            tName = "ReadyRackWp" + count.ToString();
            t = Territories.theTerritories.Find(tName, tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): t=null for " + tName);
               return false;
            }
            rr1 = new MapItem("ReadyRackWp", 0.9, "c12RoundsLeft", t);
            rr1.Count = count;
            gi.ReadyRacks.Add(rr1);
            //--------------------------------------------------
            count = 1;
            tName = "ReadyRackHbci" + count.ToString();
            t = Territories.theTerritories.Find(tName, tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): t=null for " + tName);
               return false;
            }
            rr1 = new MapItem("ReadyRackHbci", 0.9, "c12RoundsLeft", t);
            rr1.Count = count;
            gi.ReadyRacks.Add(rr1);
         }
         else
         {
            count = 2;
            tName = "ReadyRackHvap" + count.ToString();
            t = Territories.theTerritories.Find(tName, tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): t=null for " + tName);
               return false;
            }
            rr1 = new MapItem("ReadyRackHvap", 0.9, "c12RoundsLeft", t);
            rr1.Count = count;
            gi.ReadyRacks.Add(rr1);
         }
         //--------------------------------------------------
         count = card.myMaxReadyRackCount - 2;
         tName = "ReadyRackHe" + count.ToString();
         t = Territories.theTerritories.Find(tName, tType);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMorningBriefing(): t=null for " + tName);
            return false;
         }
         rr1 = new MapItem("ReadyRackHe", 0.9, "c12RoundsLeft", t);
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
         //-------------------------------------------------
         gi.Sherman.TerritoryCurrent = gi.Home;
         int delta = (int)(gi.Sherman.Zoom * Utilities.theMapItemOffset);
         gi.Sherman.Location.X = gi.Home.CenterPoint.X - delta;
         gi.Sherman.Location.Y = gi.Home.CenterPoint.Y - delta;
         return true;
      }
      private bool PerformAutoSetupSkipPreparations(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): lastReport=null");
            return false;
         }
         string tType = lastReport.TankCardNum.ToString();
         //------------------------------------
         bool isCommanderButtonUp = false;
         bool isDriverButtonUp = false;
         bool isAssistantButtonUp = false;
         if (false == PerformAutoSetupSkipMorningBriefing(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): PerformAutoSetupStartMorningBriefing() returned false");
            return false;
         }
         //------------------------------------
         int dieRoll = Utilities.RandomGenerator.Next(1, 11);
         //dieRoll = 38; // <cgs> TEST - no hull down
         if (false == SetDeployment(gi, dieRoll))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): SetDeployment() returned false");
            return false;
         }
         //------------------------------------
         ICrewMember? cm = gi.GetCrewMemberByRole("Commander");
         if (null == cm)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): cm is null for Commander");
            return false;
         }
         cm.IsButtonedUp = isCommanderButtonUp;
         if (false == cm.IsButtonedUp)
         {
            ITerritory? t = Territories.theTerritories.Find("Commander_Hatch", tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): t null for Commander_Hatch");
               return false;
            }
            IMapItem mi = new MapItem(cm.Role + "_OpenHatch", 1.0, "c15OpenHatch", t);
            int delta0 = (int)(mi.Zoom * Utilities.theMapItemOffset);
            mi.Location.X = t.CenterPoint.X - delta0;
            mi.Location.Y = t.CenterPoint.Y - delta0;
            gi.Hatches.Add(mi);
         }
         //------------------------------------
         cm = gi.GetCrewMemberByRole("Driver");
         if (null == cm)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): cm is null for Driver");
            return false;
         }
         cm.IsButtonedUp = isDriverButtonUp;
         if (false == cm.IsButtonedUp)
         {
            ITerritory? t = Territories.theTerritories.Find("Driver_Hatch", tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): t null for Driver_Hatch");
               return false;
            }
            IMapItem mi = new MapItem(cm.Role + "_OpenHatch", 1.0, "c15OpenHatch", t);
            int delta1 = (int)(mi.Zoom * Utilities.theMapItemOffset);
            mi.Location.X = t.CenterPoint.X - delta1;
            mi.Location.Y = t.CenterPoint.Y - delta1;
            gi.Hatches.Add(mi);
         }
         //------------------------------------
         cm = gi.GetCrewMemberByRole("Assistant");
         if (null == cm)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): cm is null for Assistant");
            return false;
         }
         cm.IsButtonedUp = isAssistantButtonUp;
         if (false == cm.IsButtonedUp)
         {
            ITerritory? t = Territories.theTerritories.Find("Assistant_Hatch", tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): t null for Assistant_Hatch");
               return false;
            }
            IMapItem mi = new MapItem(cm.Role + "_OpenHatch", 1.0, "c15OpenHatch", t);
            int delta3 = (int)(mi.Zoom * Utilities.theMapItemOffset);
            mi.Location.X = t.CenterPoint.X - delta3;
            mi.Location.Y = t.CenterPoint.Y - delta3;
            gi.Hatches.Add(mi);
         }
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
         gi.GunLoads.Clear();
         IMapItem gunLoad = new MapItem("GunLoadInGun", 1.0, "c17GunLoad", new Territory());
         gi.GunLoads.Add(gunLoad);
         if (false == gi.SetGunLoadTerritory("He"))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipPreparations(): SetGunLoad() returned false");
            return false;
         }
         //------------------------------------
         gi.Sherman.IsTurret = true;
         gi.Sherman.TerritoryCurrent = gi.Home;
         int delta = (int)(gi.Sherman.Zoom * Utilities.theMapItemOffset);
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
         IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce"); // Set Task Force into two areas in
         if (null == taskForce)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): taskForce=null");
            return false;
         }
         int index = Utilities.RandomGenerator.Next(0, taskForce.TerritoryCurrent.Adjacents.Count);
         string tName = taskForce.TerritoryCurrent.Adjacents[index];
         int count = 10;
         while (true == tName.EndsWith("E") && 0 < --count)
         {
            index = Utilities.RandomGenerator.Next(0, taskForce.TerritoryCurrent.Adjacents.Count);
            tName = taskForce.TerritoryCurrent.Adjacents[index];
         }
         if (true == tName.EndsWith("E"))
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): unable to find random territory after count=" + count.ToString() + " tries - choosing tName=" + tName);
            tName = "M030";
         }
         gi.EnteredArea = Territories.theTerritories.Find(tName); // Moving to hex adjacent to TaskForce Area
         if (null == gi.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): gi.EnteredArea =null for " + tName);
            return false;
         }
         //---------------------------------------------
         taskForce.TerritoryCurrent = taskForce.TerritoryStarting = gi.EnteredArea;
         taskForce.Location = gi.EnteredArea.CenterPoint;
         //---------------------------------------------
         double offset = taskForce.Zoom * Utilities.theMapItemOffset;
         IMapPoint mp = new MapPoint(taskForce.Location.X + offset, taskForce.Location.Y + offset);
         EnteredHex newHex = new EnteredHex(gi, gi.EnteredArea, ColorActionEnum.CAE_ENTER, mp); // AutoSetupSkipMovement()
         if (true == newHex.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): newHex.Ctor=true");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_ENTERED_HEX, "PerformAutoSetupSkipMovement(): adding newHex=" + newHex.ToString() + " to hexes=" + gi.EnteredHexes.toString());
         gi.EnteredHexes.Add(newHex); // EnterBoardArea() - moving to new area
         //---------------------------------------------
         dieRoll = Utilities.RandomGenerator.Next(1, 11);
         if (false == SetEnemyStrengthCounter(gi, dieRoll)) // set strength in current territory
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): SetEnemyStrengthCounter() returned false");
            return false;
         }
         //---------------------------------------------
         gi.ArtillerySupportCheck = gi.EnteredArea;
         for (int i = 0; i < 0; ++i)
         {
            dieRoll = Utilities.RandomGenerator.Next(1, 11); 
            //dieRoll = 10; // <cgs> TEST - number of artillery support counters in hext
            if (false == SetArtillerySupportCounter(gi, dieRoll)) // set strength in current territory
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipMovement(): SetArtillerySupportCounter() returned false");
               return false;
            }
         }
         //---------------------------------------------
         gi.AirStrikeCheckTerritory = gi.EnteredArea;
         for (int i = 0; i < 0; ++i)
         {
            dieRoll = Utilities.RandomGenerator.Next(1, 11); 
            //dieRoll = 10; // <cgs> TEST - number of air strikes counters in hex
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
         int numEnemyUnitsAppearing = Utilities.RandomGenerator.Next(1, 3);
         //numEnemyUnitsAppearing = 5; // <cgs> TEST - number of enemy units appearing
         for (int k = 0; k < numEnemyUnitsAppearing; k++)
         {
            int die1 = Utilities.RandomGenerator.Next(0, 3);
            //die1 += 3; // <cgs> TEST - create enemy in US Sectors
            int die2 = Utilities.RandomGenerator.Next(0, 3);
            //die2 = 2; // <cgs> TEST - AdvanceRetreat - Start at long range
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
            else if (3 == die1)
            {
               if (0 == die2)
                  tName = "B1C";
               else if (1 == die2)
                  tName = "B1M";
               else if (2 == die2)
                  tName = "B1L";
            }
            else if (4 == die1)
            {
               if (0 == die2)
                  tName = "B2C";
               else if (1 == die2)
                  tName = "B2M";
               else if (2 == die2)
                  tName = "B2L";
            }
            else if (5 == die1)
            {
               if (0 == die2)
                  tName = "B3C";
               else if (1 == die2)
                  tName = "B3M";
               else if (2 == die2)
                  tName = "B3L";
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
            //diceRoll = 11; // <cgs> TEST - AdvanceRetreat - infantry appearing
            diceRoll = 45; // <cgs> TEST -  KillYourTank - tanks appearing
            //diceRoll = 51; // <cgs> TEST -  ATG appearing
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
                  mi.IsSpotted = true;
                  break;
               case "MG":
                  mi = new MapItem(name, Utilities.ZOOM, "c92MgTeam", t);
                  mi.IsSpotted = true;
                  break;
               case "PSW/SPW":
                  enemyUnit = "SPW";
                  name = enemyUnit + Utilities.MapItemNum;
                  Utilities.MapItemNum++;
                  mi = new MapItem(name, Utilities.ZOOM + 0.2, "c89Psw232", t);
                  mi.IsVehicle = true;
                  mi.IsSpotted = true;
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
                  mi.IsSpotted = true;
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
               if (false == mi.SetMapItemRotation(gi.Sherman))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipBattleSetup(): SetMapItemRotation() returned false");
                  return false;
               }
               die1 = Utilities.RandomGenerator.Next(1, 11);
               string facing = TableMgr.GetEnemyNewFacing(enemyUnit, die1);
               if ("ERROR" == facing)
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipBattleSetup(): GetEnemyNewFacing() returned error");
                  return false;
               }
               if (false == mi.UpdateMapRotation(facing))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipBattleSetup(): UpdateMapRotation() returned false");
                  return false;
               }
            }
            //-----------------------------------------
            die1 = Utilities.RandomGenerator.Next(1, 11);
            string enemyTerrain = TableMgr.GetEnemyTerrain(lastReport.Scenario, gi.Day, "A", enemyUnit, die1);
            mi.IsHullDown = false;
            mi.IsWoods = false;
            mi.IsFortification = false;
            mi.IsBuilding = false;
            mi.IsMovingInOpen = false;
            switch (enemyTerrain)
            {
               case "Hull Down":
                  mi.IsHullDown = true;
                  break;
               case "Woods":
                  mi.IsWoods = true;
                  break;
               case "Fortification":
                  mi.IsFortification = true;
                  break;
               case "Building":
                  mi.IsBuilding = true;
                  break;
               case "Open":
                  break;
               case "Moving in Open":
                  mi.IsMovingInOpen = true;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "PerformAutoSetupSkipBattleSetup(): reached default terrain=" + enemyTerrain);
                  return false;
            }
         }
         return true;
      }
      private bool AddStartingTestingOptions(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddStartingTestingOptions(): lastReport=null");
            return false;
         }
         //--------------------------------
         gi.Day = 100; // Afetr Nov 1944
         //--------------------------------
         //lastReport.Driver.SetBloodSpots(20);              // <cgs> TEST - wounded crewmen
         //lastReport.Driver.Wound = "Light Wound";          // <cgs> TEST - wounded crewmen
         //lastReport.Driver.WoundDaysUntilReturn = 7;       // <cgs> TEST - wounded crewmen
         //gi.SetIncapacitated(lastReport.Driver);           // <cgs> TEST - wounded crewmen
         //--------------------------------
         //lastReport.Commander.SetBloodSpots(10);           // <cgs> TEST - wounded crewmen
         //lastReport.Commander.Wound = "Light Wound";       // <cgs> TEST - wounded crewmen
         //lastReport.Commander.WoundDaysUntilReturn = 0;    // <cgs> TEST - wounded crewmen
         //gi.SetIncapacitated(lastReport.Commander);        // <cgs> TEST - wounded crewmen
         //--------------------------------
         //gi.IsAdvancingFireChosen = false; // <cgs> TEST
         //--------------------------------
         //gi.IsLeadTank = true;
         //--------------------------------
         //gi.Sherman.RotationHull = 180; // <cgs> TEST
         //gi.Sherman.RotationTurret = 60;
         //gi.Sherman.IsMoving = false;
         //gi.Sherman.IsHullDown = false;
         //gi.Sherman.IsBoggedDown = true;
         //gi.Sherman.IsAssistanceNeeded = true;
         //--------------------------------
         //lastReport.AmmoPeriscope = 3;
         //gi.IsBrokenPeriscopeDriver = true;
         //gi.IsBrokenPeriscopeLoader = true;
         //gi.IsBrokenPeriscopeAssistant = true;
         //gi.IsBrokenPeriscopeGunner = true;
         //gi.IsBrokenPeriscopeCommander = true;
         //--------------------------------
         //gi.IsMalfunctionedMainGun = true;
         //gi.IsBrokenGunSight = true;
         //gi.IsMalfunctionedMgAntiAircraft = true;
         //gi.IsMalfunctionedMgBow = true;
         //gi.IsMalfunctionedMgCoaxial = true;
         //--------------------------------
         //ITerritory? t = Territories.theTerritories.Find("B6M");
         //if( null == t )
         //{
         //   Logger.Log(LogEnum.LE_ERROR, "AddStartingTestingOptions(): t=null");
         //   return false;
         //}
         //for (int i = 0; i < 3; i++)
         //{
         //   string miName1 = "SmokeWhite" + Utilities.MapItemNum;
         //   Utilities.MapItemNum++;
         //   double zoom = Utilities.ZOOM + 1.25;
         //   IMapItem smoke1 = new MapItem(miName1, zoom, "c108Smoke1", t);
         //   IMapPoint mp1 = Territory.GetRandomPoint(t, Utilities.theMapItemOffset);
         //   smoke1.Location = mp1;
         //   gi.BattleStacks.Add(smoke1);
         //}
         //for (int i = 0; i < 1; i++)
         //{
         //   string miNameGrenade = "SmokeWhite" + Utilities.MapItemNum;
         //   Utilities.MapItemNum++;
         //   double zoom = Utilities.ZOOM + 2.2;
         //   IMapItem smokeGrenade = new MapItem(miNameGrenade, zoom, "c108Smoke1", gi.Home);
         //   IMapPoint mp1 = Territory.GetRandomPoint(gi.Home, 5);
         //   smokeGrenade.Location.X = gi.Home.CenterPoint.X - zoom * Utilities.theMapItemOffset;
         //   smokeGrenade.Location.Y = mp1.Y - zoom * Utilities.theMapItemOffset;
         //   gi.BattleStacks.Add(smokeGrenade);
         //}
         //--------------------------------
         //gi.PromotionPointNum = 400; // <cgs> TEST - EndOfDay - Force Promo at end of day
         //--------------------------------
         //lastReport.SunriseHour = 18;  // <cgs> TEST - EndOfDay - start at end of day
         //lastReport.SunriseMin = 30;   // <cgs> TEST - EndOfDay - start at end of day
         //--------------------------------
         //ICrewMember loader = new CrewMember("Loader", "Cpl", "c09Loader");
         //loader.Rating = 7;
         //loader.SetBloodSpots(35);              // <cgs> TEST - healing crewmen
         //loader.Wound = "Serious Wound";        // <cgs> TEST - healing crewmen
         //loader.WoundDaysUntilReturn = 3;       // <cgs> TEST - healing crewmen
         //gi.InjuredCrewMembers.Add(loader);     // <cgs> TEST - healing crewmen
         ////--------------------------------
         //ICrewMember driver = new CrewMember("Driver", "Pvt", "c08Driver");
         //driver.Rating = 8;
         //driver.SetBloodSpots(10);              // <cgs> TEST - healing crewmen
         //driver.Wound = "Light Wound";          // <cgs> TEST - healing crewmen
         //driver.WoundDaysUntilReturn = 1;       // <cgs> TEST - healing crewmen
         //gi.InjuredCrewMembers.Add(driver);     // <cgs> TEST - healing crewmen
         //--------------------------------
         //loader = new CrewMember("Loader", "Cpl", "c09Loader");
         //loader.Rating = 3;
         //loader.SetBloodSpots(35);                   // <cgs> TEST - healing crewmen
         //loader.Wound = "Serious Wound";             // <cgs> TEST - healing crewmen
         //loader.WoundDaysUntilReturn = TableMgr.MIA; // <cgs> TEST - healing crewmen
         //gi.InjuredCrewMembers.Add(loader);          // <cgs> TEST - healing crewmen
         //--------------------------------
         //loader = new CrewMember("Loader", "Cpl", "c09Loader");
         //loader.Rating = 3;
         //loader.SetBloodSpots(35);                   // <cgs> TEST - healing crewmen
         //loader.Wound = "Serious Wound";             // <cgs> TEST - healing crewmen
         //loader.WoundDaysUntilReturn = TableMgr.KIA; // <cgs> TEST - healing crewmen
         //gi.InjuredCrewMembers.Add(loader);          // <cgs> TEST - healing crewmen
         return true;
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
            Logger.Log(LogEnum.LE_SHOW_CREW_NAME, "GameStateMorningBriefing.PerformAction(): crew=" + lastReport.Commander.Name);
            switch (action)
            {
               case GameAction.ShowTankForcePath:
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
               case GameAction.MorningBriefingBegin:
                  if(0 < gi.InjuredCrewMembers.Count)
                  {
                     gi.EventDisplayed = gi.EventActive = "e007a"; // Healing Crewmen
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     if (false == CheckCrewReturning(gi, lastReport))
                     {
                        returnStatus = "Check_HealedCrewman() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(MorningBriefingBegin): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.MorningBriefingCrewmanHealing:
                  if (false == CheckCrewReturning(gi, lastReport))
                  {
                     returnStatus = "Check_HealedCrewman() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(MorningBriefingCrewmanHealing): " + returnStatus);
                  }
                  break;
               case GameAction.MorningBriefingExistingCrewman:
                  if (null == gi.ReturningCrewman)
                  {
                     returnStatus = "gi.ReturningCrewman=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(MorningBriefingExistingCrewman): " + returnStatus);
                  }
                  else
                  {
                     gi.ReturningCrewman.WoundDaysUntilReturn = TableMgr.MIA; // this guy will never return
                     if (false == CheckCrewReturning(gi, lastReport))
                     {
                        returnStatus = "Check_HealedCrewman() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(MorningBriefingExistingCrewman): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.MorningBriefingReturningCrewman:
                  if (null == gi.ReturningCrewman)
                  {
                     returnStatus = "gi.ReturningCrewman=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(MorningBriefingExistingCrewman): " + returnStatus);
                  }
                  else
                  {
                     if (false == ReturnHealedCrewman(gi))
                     {
                        returnStatus = "ReturnHealedCrewman() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(MorningBriefingExistingCrewman): " + returnStatus);
                     }
                     else if (false == CheckCrewReturning(gi, lastReport))
                     {
                        returnStatus = "Check_HealedCrewman() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(MorningBriefingExistingCrewman): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.MorningBriefingTankReplacementRoll:
                  if ( Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     gi.DieResults[key][0] = dieRoll;
                     if( false == TableMgr.GetNewSherman(gi, dieRoll))
                     {
                        returnStatus = "GetNewSherman() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(MorningBriefingTankReplacementRoll): " + returnStatus);
                     }
                     //----------------------------------
                     gi.BattleStacks.Remove(gi.Sherman);
                     string tankImageName = "t";
                     if (9 < lastReport.TankCardNum)
                        tankImageName += lastReport.TankCardNum.ToString();
                     else
                        tankImageName += ("0" + lastReport.TankCardNum.ToString());
                     string shermanName = "Sherman" + lastReport.TankCardNum.ToString();
                     gi.Sherman = new MapItem(shermanName, 2.0, tankImageName, gi.Home);
                     gi.BattleStacks.Add(gi.Sherman);
                  }
                  else
                  {
                     if( (10 < lastReport.TankCardNum) && (18 != lastReport.TankCardNum) && (69 < gi.Day) ) // must be Nov 44 before tanks have HVSS
                     {
                        gi.EventDisplayed = gi.EventActive = "e007e";  // HVSS Roll
                        gi.DieRollAction = GameAction.MorningBriefingTankReplacementHvssRoll;
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = "e008";
                        gi.DieRollAction = GameAction.MorningBriefingWeatherRoll;
                     }
                  }
                  break;
               case GameAction.MorningBriefingTankReplacementHvssRoll:
                  if(Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     dieRoll = 1; // <cgs> - TEST - Show HVSS
                     gi.DieResults[key][0] = dieRoll;
                     gi.IsShermanHvss = false;
                     gi.ShermanHvss = null;
                     if ( (dieRoll < 4) || ((11 < lastReport.TankCardNum) && (dieRoll < 6)) )
                     {
                        string tType = lastReport.TankCardNum.ToString();
                        ITerritory? t = Territories.theTerritories.Find("Hvss", tType);
                        if(null == t )
                        {
                           returnStatus = "Territories.theTerritories.Find() returned null for tName=Hvss and tType=" + tType;
                           Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(MorningBriefingTankReplacementHvssRoll): " + returnStatus);
                        }
                        else
                        {
                           string mapItemName = "Hvss" + Utilities.MapItemNum.ToString();
                           Utilities.MapItemNum++;
                           gi.ShermanHvss = new MapItem(mapItemName, 1.0, "c75Hvss", t);
                        }
                     }
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e008";
                     gi.DieRollAction = GameAction.MorningBriefingWeatherRoll;
                  }
                  break;
               case GameAction.SetupCombatCalendarRoll: // only applies when coming from Setup GamePhase
               case GameAction.MorningBriefingCalendarRoll:
                  if (false == HealedCrewmanDayDecrease(gi))
                  {
                     returnStatus = "Healed_CrewmanDayDecrease() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(MorningBriefingCalendarRoll): " + returnStatus);
                  }
                  gi.DieResults[key][0] = dieRoll; // clicking on image either restarts next day or continues with MorningBriefingBegin
                  break;
               case GameAction.MorningBriefingWeatherRoll:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  lastReport.Weather = TableMgr.GetWeather(gi.Day, dieRoll);
                  break;
               case GameAction.MorningBriefingWeatherRollEnd:
                  if (true == lastReport.Weather.Contains("Snow"))
                  {
                     gi.EventDisplayed = gi.EventActive = "e008a"; // first need to roll for snow
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
               case GameAction.MorningBriefingDeployment:
                  gi.GamePhase = GamePhase.Preparations;
                  gi.EventDisplayed = gi.EventActive = "e011";
                  gi.DieRollAction = GameAction.PreparationsDeploymentRoll;
                  gi.Sherman.TerritoryCurrent = gi.Home;
                  int delta = (int)(gi.Sherman.Zoom * Utilities.theMapItemOffset);
                  gi.Sherman.Location.X = gi.Home.CenterPoint.X - delta;
                  gi.Sherman.Location.Y = gi.Home.CenterPoint.Y - delta;
                  gi.BattleStacks.Add(gi.Sherman);
                  if (false == SetWeatherCounters(gi))
                  {
                     returnStatus = "SetWeatherCounters() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMorningBriefing.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.MorningBriefingDayOfRest:
                  ++gi.Day;
                  gi.NewMembers.Clear();
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
                     gi.EventDisplayed = gi.EventActive = "e006"; // MorningBriefing_DayOfRest
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
      protected bool HealedCrewmanDayDecrease(IGameInstance gi)
      {
         foreach (IMapItem mi in gi.InjuredCrewMembers)
         {
            ICrewMember? cm = mi as ICrewMember;
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "Healed_CrewmanDayDecrease(): cm=null for mi.Name=" + mi.Name);
               return false;
            }
            if( (TableMgr.MIA != cm.WoundDaysUntilReturn) && (TableMgr.KIA != cm.WoundDaysUntilReturn) )
               cm.WoundDaysUntilReturn--;
         }
         return true;
      }
      protected bool CheckCrewReturning(IGameInstance gi, IAfterActionReport lastReport)
      {
         gi.ReturningCrewman = null;
         foreach (IMapItem mi in gi.InjuredCrewMembers)
         {
            ICrewMember? cm = mi as ICrewMember;
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "Check_HealedCrewman(): cm=null for mi.Name=" + mi.Name);
               return false;
            }
            if (cm.WoundDaysUntilReturn <= 0)
            {
               gi.ReturningCrewman = cm;
               break;
            }
         }
         if (null != gi.ReturningCrewman)
         {
            gi.EventDisplayed = gi.EventActive = "e007c";  // Returning Crewmen
            gi.DieRollAction = GameAction.DieRollActionNone;
            return true;
         }
         //------------------------------------------------------------------------
         ICombatCalendarEntry? newEntry = TableMgr.theCombatCalendarEntries[gi.Day]; // add new report for today activities after replacing crewmen
         if (null == newEntry)
         {
            Logger.Log(LogEnum.LE_ERROR, "Check_CrewReturning(): newEntry=null");
            return false;
         }
         IAfterActionReport newReport = new AfterActionReport(newEntry, lastReport);
         gi.Reports.Add(newReport);
         //------------------------------------------------------------------------
         if ( true == gi.Sherman.IsKilled )
         {
            gi.EventDisplayed = gi.EventActive = "e007d";  // Returning Crewmen
            gi.DieRollAction = GameAction.MorningBriefingTankReplacementRoll;
            return true;
         }
         else
         {
            gi.EventDisplayed = gi.EventActive = "e008";  // Weather Roll
            gi.DieRollAction = GameAction.MorningBriefingWeatherRoll;
         }
         return true;
      }
      protected bool ReturnHealedCrewman(IGameInstance gi)
      {
         if (null == gi.ReturningCrewman)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReturnHealedCrewman(): gi.ReturningCrewman=null");
            return false;
         }
         gi.ReturningCrewman.IsUnconscious = false;
         gi.ReturningCrewman.IsIncapacitated = false;
         gi.ReturningCrewman.SetBloodSpots(0);
         //---------------------------------------------
         gi.InjuredCrewMembers.Remove(gi.ReturningCrewman);
         //---------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReturnHealedCrewman(): lastReport=null");
            return false;
         }
         switch (gi.ReturningCrewman.Role)
         {
            case "Driver":
               lastReport.Driver = gi.ReturningCrewman;
               break;
            case "Loader":
               lastReport.Loader = gi.ReturningCrewman;
               break;
            case "Assistant":
               lastReport.Assistant = gi.ReturningCrewman;
               break;
            case "Gunner":
               lastReport.Gunner = gi.ReturningCrewman;
               break;
            case "Commander":
               lastReport.Commander = gi.ReturningCrewman;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ReturnHealedCrewman(): reached default role=" + gi.ReturningCrewman.Role);
               return false;
         }
         if (false == gi.SetCrewActionTerritory(gi.ReturningCrewman))
         {
            Logger.Log(LogEnum.LE_ERROR, "ReturnHealedCrewman(): SetCrewActionTerritory() returned false");
            return false;
         }
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
            Logger.Log(LogEnum.LE_SHOW_CREW_NAME, "GameStateBattlePrep.PerformAction(): crew=" + lastReport.Commander.Name);
            TankCard card = new TankCard(lastReport.TankCardNum);
            switch (action)
            {
               case GameAction.ShowTankForcePath:
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
                  gi.GunLoads.Clear();
                  gi.IsHatchesActive = false;
                  int totalAmmo = lastReport.MainGunHE + lastReport.MainGunAP + lastReport.MainGunWP + lastReport.MainGunHBCI + lastReport.MainGunHVAP;
                  if (0 < totalAmmo)
                  {
                     gi.EventDisplayed = gi.EventActive = "e013";
                     IMapItem gunLoad = new MapItem("GunLoadInGun", 1.0, "c17GunLoad", new Territory());
                     gi.GunLoads.Add(gunLoad);
                     bool isSetGunLoadTerritoryGood = true;
                     if (0 < lastReport.MainGunHE)
                        isSetGunLoadTerritoryGood = gi.SetGunLoadTerritory("He");
                     else if (0 < lastReport.MainGunAP)
                        isSetGunLoadTerritoryGood = gi.SetGunLoadTerritory("Ap");
                     else if (0 < lastReport.MainGunHVAP)
                        isSetGunLoadTerritoryGood = gi.SetGunLoadTerritory("Hvap");
                     else if (0 < lastReport.MainGunWP)
                        isSetGunLoadTerritoryGood = gi.SetGunLoadTerritory("Wp");
                     else if (0 < lastReport.MainGunHBCI)
                        isSetGunLoadTerritoryGood = gi.SetGunLoadTerritory("Hbci");
                     else
                        isSetGunLoadTerritoryGood = false;
                     if (false == isSetGunLoadTerritoryGood)
                     {
                        returnStatus = "gi.SetGunLoadTerritory() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(PreparationsGunLoad): " + returnStatus);
                     }
                     Logger.Log(LogEnum.LE_SHOW_GUN_LOAD_PREP, "GameStateBattlePrep.PerformAction(PreparationsGunLoad): gunLoad=" + gunLoad.Name + " t=" + gunLoad.TerritoryCurrent.Name);
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e013a";
                  }
                  break;
               case GameAction.PreparationsGunLoadSelect:
                  break;
               case GameAction.PreparationsTurret:
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
                           if (true == lastReport.Commander.IsButtonedUp && false == card.myIsVisionCupola)
                           {
                              action = GameAction.PreparationsCommanderSpot;
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
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(PreparationsLoaderSpot): " + returnStatus);
                              }
                           }
                        }
                     }
                  }
                  break;
               case GameAction.PreparationsLoaderSpotSet:
               case GameAction.PreparationsCommanderSpotSet:
                  break;
               case GameAction.PreparationsCommanderSpot:
                  if (null == lastReport.Commander)
                  {
                     returnStatus = "lastReport.Commander=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(PreparationsLoaderSpot): " + returnStatus);
                  }
                  else
                  {
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
               case GameAction.BattleEmptyResolve:
                  if (false == ResolveEmptyBattleBoard(gi, lastReport)) // GameStateBattlePrep.PerformAction(BattleEmptyResolve)
                  {
                     returnStatus = "Resolve_EmptyBattleBoard() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattlePrep.PerformAction(): " + returnStatus);
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
            Logger.Log(LogEnum.LE_SHOW_CREW_NAME, "GameStateMovement.PerformAction(): crew=" + lastReport.Commander.Name);
            string key = gi.EventActive;
            switch (action)
            {
               case GameAction.ShowTankForcePath:
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
               case GameAction.UpdateTankCard:
               case GameAction.UpdateShowRegion:
               case GameAction.UpdateEventViewerDisplay: // Only change active event
               case GameAction.MorningBriefingAssignCrewRating: // handled in EventViewer by showing dialog
                  break;
               case GameAction.UpdateBattleBoard: // Do not log event
                  return returnStatus;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.MovementStartAreaSet:
               case GameAction.MovementStartAreaRestart: // No fight occurs
               case GameAction.MovementStartAreaRestartAfterBattle: // Win a battle resulting in empty battle board
                  theIs1stEnemyStrengthCheckTerritory = true; // do not do enemy strength check on first area which is the start area
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
                  if ( "He" ==  gi.GetGunLoadType())
                  {
                     gi.EventDisplayed = gi.EventActive = "e029";
                     gi.DieRollAction = GameAction.MovementAdvanceFireAmmoUseRoll;
                  }
                  else
                  {
                     gi.EventDisplayed = gi.EventActive = "e029a";
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
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
                  //dieRoll = 10; // <cgs> TEST - enforce combat
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (false == ResolveBattleCheckRoll(gi, dieRoll))
                  {
                     returnStatus = "ResolveBattleCheckRoll() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateMovement.PerformAction(): " + returnStatus);
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
         else
         {

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
         gi.GamePhase = GamePhase.Movement;
         gi.EventDisplayed = gi.EventActive = "e033"; // SkipBattleBoard()
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
         Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "Skip_BattleBoard(): Added mi=" + usControl.Name + " t=" + taskForce.TerritoryCurrent.Name + " to " + gi.MoveStacks.ToString());
         //------------------------------------
         foreach (IMapItem mi in stack.MapItems)
         {
            if (true == mi.Name.Contains("Strength"))
            {
               stack.MapItems.Remove(mi);
               break;
            }
         }
         //------------------------------------
         bool isExitArea;
         if (false == gi.IsExitArea(out isExitArea))
         {
            Logger.Log(LogEnum.LE_ERROR, "Skip_BattleBoard(): gi.IsExitArea() returned false");
            return false;
         }
         if (true == isExitArea)
         {
            report.VictoryPtsCapturedExitArea++;
            Logger.Log(LogEnum.LE_SHOW_VP_CAPTURED_AREA, "Skip_BattleBoard(): captured area t=" + taskForce.TerritoryCurrent.Name + " #=" + report.VictoryPtsCapturedExitArea.ToString());
         }
         else
         {
            report.VictoryPtsCaptureArea++;
            Logger.Log(LogEnum.LE_SHOW_VP_CAPTURED_AREA, "Skip_BattleBoard(): captured area t=" + taskForce.TerritoryCurrent.Name + " #=" + report.VictoryPtsCaptureArea.ToString());
         }
         return true;
      }
      private bool ResolveBattleCheckRoll(IGameInstance gi, int dieRoll)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveBattleCheckRoll(): lastReport=null");
            return false;
         }
         //-------------------------------------------------
         if (null == gi.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveBattleCheckRoll(): gi.EnteredArea=null");
            return false;
         }
         //-------------------------------------------------
         IStack? stack = gi.MoveStacks.Find(gi.EnteredArea);
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveBattleCheckRoll(): stack=null");
            return false;
         }
         IMapItems enemyOnMoveBoard = new MapItems();
         foreach (IMapItem mi in stack.MapItems) // check if any enemy units have been moved to from Battle Board to Move Board
         {
            if (true == mi.IsEnemyUnit())
            {
               Logger.Log(LogEnum.LE_SHOW_ENEMY_ON_MOVE_BOARD, "ResolveBattleCheckRoll(): ENEMY UNIT=" + mi.Name);
               enemyOnMoveBoard.Add(mi);
            }
         }
         //-------------------------------------------------
         switch (gi.BattleResistance)
         {
            case EnumResistance.Light:
               if ((7 < dieRoll) || (0 < enemyOnMoveBoard.Count)) // battle
               {
                  if (false == StartBattle(gi, lastReport))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ResolveBattleCheckRoll(): StartBattle() returned false");
                     return false;
                  }
               }
               else
               {
                  if (false == SkipBattleBoard(gi, lastReport))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ResolveBattleCheckRoll(): SkipBattleBoard() returned false");
                     return false;
                  }
               }
               break;
            case EnumResistance.Medium:
               if ((5 < dieRoll) || (0 < enemyOnMoveBoard.Count)) // battle
               {
                  if (false == StartBattle(gi, lastReport))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ResolveBattleCheckRoll(): StartBattle() returned false");
                     return false;
                  }
               }
               else
               {
                  if (false == SkipBattleBoard(gi, lastReport))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ResolveBattleCheckRoll(): SkipBattleBoard() returned false");
                     return false;
                  }
               }
               break;
            case EnumResistance.Heavy:
               if ((3 < dieRoll) || (0 < enemyOnMoveBoard.Count)) // battle
               {
                  if (false == StartBattle(gi, lastReport))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ResolveBattleCheckRoll(): StartBattle() returned false");
                     return false;
                  }
               }
               else
               {
                  if (false == SkipBattleBoard(gi, lastReport))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ResolveBattleCheckRoll(): SkipBattleBoard() returned false");
                     return false;
                  }
               }
               Logger.Log(LogEnum.LE_SHOW_STACK_VIEW, "ResolveBattleCheckRoll(): " + gi.MoveStacks.ToString());
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ResolveBattleCheckRoll(): reached default with resistance=" + gi.BattleResistance.ToString());
               return false;
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
         if (false == CreateMapItemMove(gi, taskForce, gi.EnteredArea))
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
         Logger.Log(LogEnum.LE_SHOW_START_BATTLE, "StartBattle(): +++++++++++++++++++++++++++++++++++");
         if (null == gi.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "StartBattle(): gi.EnteredArea=null");
            return false;
         }
         AdvanceTime(report, 15);
         gi.GamePhase = GamePhase.Battle;
         Logger.Log(LogEnum.LE_SHOW_START_BATTLE, "StartBattle(): -----------------------------------");
         return true;
      }
      private bool MovementPhaseRestart(IGameInstance gi, IAfterActionReport report)
      {
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
         gi.BattleResistance = EnumResistance.None;                      // MovementPhaseRestart()
         gi.MoveStacks.Clear();
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "MovementPhaseRestart(): gi.MapItemMoves.Clear()");
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
            Logger.Log(LogEnum.LE_SHOW_CREW_NAME, "GameStateBattle.PerformAction(): crew=" + lastReport.Commander.Name);
            string key = gi.EventActive;
            switch (action)
            {
               case GameAction.ShowTankForcePath:
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
               case GameAction.UpdateTankCard:
               case GameAction.UpdateShowRegion:
               case GameAction.UpdateAfterActionReport:
               case GameAction.UpdateEventViewerDisplay: // Only change active event
               case GameAction.MovementEnemyStrengthChoice:
               case GameAction.MorningBriefingAssignCrewRating: // handled in EventViewer by showing dialog
                  break;
               case GameAction.UpdateBattleBoard: // Do not log event
                  return returnStatus;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.BattleStart:
                  gi.EventDisplayed = gi.EventActive = "e034"; // GameStateBattle.PerformAction(BattleStart)
                  break;
               case GameAction.BattleActivation: 
               case GameAction.BattleResolveArtilleryFire:
               case GameAction.BattleResolveAirStrike:
               case GameAction.BattleResolveAdvanceFire:
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
                  dieRoll = 10; // <cgs> TEST - NO AMBUSH!!!!!
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (dieRoll < 8)
                  {
                     Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "GameStateBattle.PerformAction(BattleAmbushRoll): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.Ambush");
                     gi.BattlePhase = BattlePhase.Ambush;
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "GameStateBattle.PerformAction(BattleAmbushRoll): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.Spotting");
                     gi.GamePhase = GamePhase.BattleRoundSequence;
                     gi.BattlePhase = BattlePhase.Spotting;
                  }
                  break;
               case GameAction.BattleEmpty:
                  gi.GamePhase = GamePhase.Preparations;
                  gi.EventDisplayed = gi.EventActive = "e036";
                  break;
               case GameAction.BattleEmptyResolve:
                  if (false == ResolveEmptyBattleBoard(gi, lastReport)) // GameStateBattle.PerformAction(BattleEmptyResolve)
                  {
                     returnStatus = "Resolve_EmptyBattleBoard() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleCrewReplaced:
                  if (false == ResetToPrepareForBattle(gi, lastReport)) // GameStateBattle.PerformAction(Battle_CrewReplaced)
                  {
                     returnStatus = "Reset_ToPrepareForBattle() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleAmbush: // Handled with EventViewerBattleAmbush class
               case GameAction.BattleCollateralDamageCheck: // Handled with EventViewerTankCollateral class
                  break;
               case GameAction.BattleRandomEvent:
                  Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "GameStateBattle.PerformAction(BattleRandomEvent): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.AmbushRandomEvent");
                  gi.BattlePhase = BattlePhase.AmbushRandomEvent;
                  if (EnumScenario.Advance == lastReport.Scenario)
                     gi.EventActive = gi.EventDisplayed = "e039a";
                  else if (EnumScenario.Battle == lastReport.Scenario)
                     gi.EventActive = gi.EventDisplayed = "e039b";              // GameStateBattle.PerformAction(BattleRandomEvent)
                  else if (EnumScenario.Counterattack == lastReport.Scenario)
                     gi.EventActive = gi.EventDisplayed = "e039c";
                  else
                  {
                     returnStatus = "unkonwn sceanrio=" + lastReport.Scenario;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                  }
                  gi.DieResults["e039b"][0] = Utilities.NO_RESULT;
                  gi.DieRollAction = GameAction.BattleRandomEventRoll;
                  break;
               case GameAction.BattleRandomEventRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 01; // <cgs> TEST - AdvanceRetreat - Random Event - Time passes
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
                           if (true == gi.Sherman.IsMoving)
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
                           gi.NumCollateralDamage++;
                           if (false == HarrassingFireCheck(gi))
                           {
                              returnStatus = "HarrassingFireCheck() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                           }
                           break;
                        case "Friendly Advance":
                           if (false == FriendlyAdvanceCheck(gi, ref action))
                           {
                              returnStatus = "FriendlyAdvanceCheck() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                           }
                           break;
                        case "Enemy Reinforce":
                           action = GameAction.BattleActivation;
                           break;
                        case "Enemy Advance":
                           if (false == EnemyAdvanceCheck(gi, ref action))
                           {
                              returnStatus = "EnemyAdvanceCheck() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                           }
                           break;
                        case "Flanking Fire":
                           action = GameAction.BattleResolveArtilleryFire;
                           gi.IsFlankingFire = true;
                           break;
                        default:
                           returnStatus = "reached default with randomEvent=" + randomEvent;
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                           break;
                     }
                  }
                  break;
               case GameAction.BattleShermanKilled:
                  gi.ClearCrewActions("GameStateBattle.PerformAction(BattleShermanKilled)");   // GameStateBattle.PerformAction(BattleShermanKilled)
                  break;
               case GameAction.UpdateTankExplosion:
               case GameAction.UpdateTankBrewUp: 
                  gi.ClearCrewActions("GameStateBattle.PerformAction(UpdateTankBrewUp)");  // GameStateBattle.PerformAction(UpdateTankExplosion) or GameStateBattle.PerformAction(UpdateTankBrewUp)
                  gi.BattleStacks.Remove(gi.Sherman);
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
                  Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                  break;
            }
         }
         StringBuilder sb12 = new StringBuilder();
         if ("OK" != returnStatus)
            sb12.Append("<<<<ERROR2::::::GameStateBattle.PerformAction(): ");
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
            Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
         }
         else
         {
            Logger.Log(LogEnum.LE_SHOW_CREW_NAME, "GameStateBattleRoundSequence.PerformAction(): crew=" + lastReport.Commander.Name);
            string key = gi.EventActive;
            switch (action)
            {
               case GameAction.ShowTankForcePath:
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
               case GameAction.UpdateTankCard:
               case GameAction.UpdateShowRegion:
               case GameAction.UpdateAfterActionReport:
               case GameAction.UpdateEventViewerDisplay: // Only change active event
               case GameAction.MorningBriefingAssignCrewRating: // handled in EventViewer by showing dialog
                  break;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.UpdateBattleBoard: // Do not log event
                  return returnStatus;
               case GameAction.BattleRoundSequenceStart:
                  if (false == BattleRoundSequenceStart(gi, ref action))
                  {
                     returnStatus = "BattleRoundSequenceStart() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceStart): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceSmokeDepletionEnd:
                  IMapItems removals = new MapItems();
                  IMapItems additions = new MapItems();
                  foreach (IStack stack in gi.BattleStacks)
                  {
                     bool isRemovalOccurredForThisTerritory = false;
                     foreach (IMapItem mi in stack.MapItems)
                     {
                        if (true == mi.Name.Contains("Smoke")) // remove white and gray smoke counters
                        {

                           if (false == isRemovalOccurredForThisTerritory)
                           {
                              removals.Add(mi);
                              if (true == mi.Name.Contains("SmokeWhite")) // exchange white with gray counters
                              {
                                 string smokeName = "SmokeGrey" + Utilities.MapItemNum;
                                 Utilities.MapItemNum++;
                                 double newZoom = mi.Zoom * 1.1;
                                 double zoomDiff = newZoom - mi.Zoom;
                                 IMapItem newSmoke = new MapItem(smokeName, newZoom, "c111Smoke1", mi.TerritoryCurrent);
                                 double diff = 0.5 * zoomDiff * Utilities.theMapItemOffset;
                                 newSmoke.Location.X = mi.Location.X - diff;
                                 newSmoke.Location.Y = mi.Location.Y - diff;
                                 additions.Add(newSmoke);
                              }
                              isRemovalOccurredForThisTerritory = true;
                           }
                           else if ("Home" != mi.TerritoryCurrent.Name) // change size of existing smoke counters
                           {
                              removals.Add(mi);
                              string smokeName = "SmokeWhite" + Utilities.MapItemNum;
                              string imgName = "c108Smoke1";
                              if (true == mi.Name.Contains("Grey"))
                              {
                                 smokeName = "SmokeGrey" + Utilities.MapItemNum;
                                 imgName = "c111Smoke1";
                              }
                              Utilities.MapItemNum++;
                              double zoomChange = mi.Zoom * 1.1;
                              double zoomDiff = zoomChange - mi.Zoom;
                              IMapItem newSmoke = new MapItem(smokeName, zoomChange, imgName, mi.TerritoryCurrent);
                              double diff = 0.5 * zoomDiff * Utilities.theMapItemOffset;
                              newSmoke.Location.X = mi.Location.X - diff;
                              newSmoke.Location.Y = mi.Location.Y - diff;
                              additions.Add(newSmoke);
                           }
                        }
                     }
                  }
                  foreach (IMapItem removal in removals)
                  {
                     gi.BattleStacks.Remove(removal);
                     Logger.Log(LogEnum.LE_SHOW_STACK_DEL, "GameStateBattleRoundSequence.PerformAction(): removing mi=" + removal.Name + " from BattleStacks=" + gi.BattleStacks.ToString());
                  }
                  foreach (IMapItem addition in additions)
                  {
                     gi.BattleStacks.Add(addition);
                     Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "GameStateBattleRoundSequence.PerformAction(): adding mi=" + addition.Name + " from BattleStacks=" + gi.BattleStacks.ToString());
                  }
                  //----------------------------------------------
                  if (false == SpottingPhaseBegin(gi, ref action, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceSmokeDepletionEnd)"))
                  {
                     returnStatus = "SpottingPhaseBegin() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceSmokeDepletionEnd): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceSpottingEnd:
                  action = GameAction.BattleRoundSequenceCrewOrders;
                  Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceSpottingEnd): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.MarkCrewAction");
                  gi.BattlePhase = BattlePhase.MarkCrewAction;
                  gi.EventDisplayed = gi.EventActive = "e038";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.ClearCrewActions("GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceSpottingEnd)");           // GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceSpottingEnd)
                  break;
               case GameAction.BattleRoundSequenceCrewOrders:
                  Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceCrewOrders): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.MarkCrewAction");
                  gi.BattlePhase = BattlePhase.MarkCrewAction;
                  return returnStatus;
               case GameAction.BattleRoundSequenceAmmoOrders:
                  gi.IsHatchesActive = false;
                  if (false == SetDefaultCrewActions(gi))
                  {
                     returnStatus = "SetDefaultCrewActions() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceAmmoOrders): " + returnStatus);
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "GameStateBattle.GameStateBattleRoundSequence(BattleRoundSequenceAmmoOrders): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.MarkAmmoReload");
                     gi.BattlePhase = BattlePhase.MarkAmmoReload;
                     int totalAmmo = lastReport.MainGunHE + lastReport.MainGunAP + lastReport.MainGunWP + lastReport.MainGunHBCI + lastReport.MainGunHVAP;
                     if (0 == totalAmmo)
                     {
                        gi.EventDisplayed = gi.EventActive = "e050d";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                     }
                     else if (1 == totalAmmo)
                     {
                        gi.EventDisplayed = gi.EventActive = "e050c";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = "e050";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                     }
                  }
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
                     if (false == CheckCrewMemberExposed(gi, ref action)) // calls SpottingPhaseBegin() if no collateral
                     {
                        returnStatus = "CheckCrewMemberExposed() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceEnemyArtilleryRoll): " + returnStatus);
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
                        if (false == NextStepAfterRandomEvent(gi, ref action))
                        {
                           returnStatus = "NextStepAfterRandomEvent() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldRoll): " + returnStatus);
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
                        if (false == NextStepAfterRandomEvent(gi, ref action))
                        {
                           returnStatus = "NextStepAfterRandomEvent() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldRoll): " + returnStatus);
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
                        if (false == NextStepAfterRandomEvent(gi, ref action))
                        {
                           returnStatus = "NextStepAfterRandomEvent() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldRoll): " + returnStatus);
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
                     ICrewMember? cm = gi.GetCrewMemberByRole("Driver");
                     if (null == cm)
                     {
                        returnStatus = "GetCrewMemberByRole(Driver) returned null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldDriverWoundRoll): " + returnStatus);
                     }
                     else
                     {
                        string woundResult = TableMgr.GetWounds(gi, cm, dieRoll, false, false, false);
                        if ("ERROR" == woundResult)
                        {
                           returnStatus = "GetWounds(Driver) returned ERROR";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldDriverWoundRoll): " + returnStatus);
                        }
                        StringBuilder sb1 = new StringBuilder("At ");
                        sb1.Append(TableMgr.GetTime(lastReport));
                        sb1.Append(", ");
                        sb1.Append(cm.Name);
                        sb1.Append(" suffered ");
                        sb1.Append(woundResult);
                        lastReport.Notes.Add(sb1.ToString());
                        //----------------------------------
                        if (false == NextStepAfterRandomEvent(gi, ref action))
                        {
                           returnStatus = "NextStepAfterRandomEvent() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldRoll): " + returnStatus);
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
                     ICrewMember? cm = gi.GetCrewMemberByRole("Assistant");
                     if (null == cm)
                     {
                        returnStatus = "GetCrewMemberByRole(Assistant) returned null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldAssistantWoundRoll): " + returnStatus);
                     }
                     else
                     {
                        string woundResult = TableMgr.GetWounds(gi, cm, dieRoll, false, false, false);
                        if ("ERROR" == woundResult)
                        {
                           returnStatus = "GetWounds() returned ERROR";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldAssistantWoundRoll): " + returnStatus);
                        }
                        StringBuilder sb1 = new StringBuilder("At ");
                        sb1.Append(TableMgr.GetTime(lastReport));
                        sb1.Append(", ");
                        sb1.Append(cm.Name);
                        sb1.Append(" suffered ");
                        sb1.Append(woundResult);
                        lastReport.Notes.Add(sb1.ToString());
                        //----------------------------------
                        if (false == NextStepAfterRandomEvent(gi, ref action))
                        {
                           returnStatus = "NextStepAfterRandomEvent() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldRoll): " + returnStatus);
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequencePanzerfaustSectorRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
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
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePanzerfaustSectorRoll): " + returnStatus);
                           break;
                     }
                     if ('0' != sector)
                     {
                        string tName1 = "B" + sector + "M";
                        string tName2 = "B" + sector + "C";
                        ITerritory? tPanzerfault = Territories.theTerritories.Find(tName2);
                        if (null == tPanzerfault)
                        {
                           returnStatus = "Unable to find tName=" + tName1;
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePanzerfaustSectorRoll): " + returnStatus);
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
                              if (false == NextStepAfterRandomEvent(gi, ref action))
                              {
                                 returnStatus = "NextStepAfterRandomEvent() returned false";
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldRoll): " + returnStatus);
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
                              string name = "Panzerfaust" + Utilities.MapItemNum;
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
                     //dieRoll = 1; // <cgs> TEST - Panzerfaust To Attack
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     if (null == gi.Panzerfaust)
                     {
                        returnStatus = "gi.Panzerfaust=null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePanzerfaustAttackRoll): " + returnStatus);
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
                           if (false == NextStepAfterRandomEvent(gi, ref action))
                           {
                              returnStatus = "NextStepAfterRandomEvent() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldRoll): " + returnStatus);
                           }
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequencePanzerfaustToHitRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 1; // <cgs> TEST - Panzerfaust To Hit
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     if (null == gi.Panzerfaust)
                     {
                        returnStatus = "gi.Panzerfaust=null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePanzerfaustToHitRoll): " + returnStatus);
                     }
                     else
                     {
                        int modifierToHit = 0;
                        if (true == gi.Panzerfaust.myIsShermanMoving)
                           modifierToHit += 2;
                        if (true == gi.Panzerfaust.myIsAdvancingFireZone)
                           modifierToHit += 3;
                        int combo = gi.DieResults[key][0] + modifierToHit;
                        if (combo < 8)
                        {
                           gi.EventDisplayed = gi.EventActive = "e044c";
                           gi.DieRollAction = GameAction.BattleRoundSequencePanzerfaustToKillRoll;
                        }
                        else
                        {
                           if (false == NextStepAfterRandomEvent(gi, ref action))
                           {
                              returnStatus = "NextStepAfterRandomEvent() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldRoll): " + returnStatus);
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
                     if (null == gi.Panzerfaust)
                     {
                        returnStatus = "gi.Panzerfaust=null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePanzerfaustToKillRoll): " + returnStatus);
                     }
                     else
                     {
                        if (gi.DieResults[key][0] < 9)
                        {
                           gi.KillSherman(lastReport, "Panzerfaust");
                           string hitLocation = "Hull";
                           if (0 == Utilities.RandomGenerator.Next(2))
                              hitLocation = "Turret";
                           gi.Death = new ShermanDeath(gi, gi.Panzerfaust.myEnemyUnit, hitLocation, "Panzerfault");
                           if (true == gi.Death.myCtorError)
                           {
                              returnStatus = "gi.Death.myCtorError= true";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePanzerfaustToKillRoll): " + returnStatus);
                           }
                           else
                           {
                              action = GameAction.BattleRoundSequenceShermanKilled;
                           }
                        }
                        else
                        {
                           if (false == NextStepAfterRandomEvent(gi, ref action))
                           {
                              returnStatus = "NextStepAfterRandomEvent() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldRoll): " + returnStatus);
                           }
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceFriendlyAdvance:
               case GameAction.BattleRoundSequenceEnemyAdvance:
               case GameAction.BattleActivation:
                  break;
               case GameAction.BattleCollateralDamageCheck:
                  break;
               case GameAction.BattleRoundSequenceFriendlyAdvanceSelected:
                  if (null == gi.FriendlyAdvance)
                  {
                     returnStatus = "myGameInstance.FriendlyAdvance=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFriendlyAdvanceSelected): " + returnStatus);
                  }
                  else
                  {
                     string name = "UsControl" + Utilities.MapItemNum.ToString();
                     ++Utilities.MapItemNum;
                     gi.BattleStacks.Add(new MapItem(name, Utilities.ZOOM, "c28UsControl", gi.FriendlyAdvance));
                  }
                  break;
               case GameAction.BattleRoundSequenceEnemyAdvanceEnd:
                  if (null == gi.EnemyAdvance)
                  {
                     returnStatus = "gi.BattleStacks.Find()  gi.EnemyAdvance=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceEnemyAdvanceEnd): " + returnStatus);
                  }
                  else
                  {
                     gi.IsEnemyAdvanceComplete = true;
                     IStack? stack = gi.BattleStacks.Find(gi.EnemyAdvance);
                     if (null == stack)
                     {
                        returnStatus = "gi.BattleStacks.Find() is null for " + gi.EnemyAdvance.Name;
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceEnemyAdvanceEnd): " + returnStatus);
                     }
                     else
                     {
                        foreach (IMapItem mi in stack.MapItems)
                        {
                           if (true == mi.Name.Contains("UsControl"))
                           {
                              gi.BattleStacks.Remove(mi);
                              break;
                           }
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceHarrassingFire:
                  break;
               case GameAction.BattleRoundSequenceConductCrewAction:
                  gi.CrewActionPhase = CrewActionPhase.Movement;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceConductCrewAction): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequencePivotLeft:
                  gi.Sherman.IsMoved = true;
                  gi.Sherman.IsHullDown = false;
                  gi.Sherman.RotationHull -= 60;
                  if (gi.Sherman.RotationHull < 0)
                     gi.Sherman.RotationHull = 300;
                  break;
               case GameAction.BattleRoundSequencePivotRight:
                  gi.Sherman.IsMoved = true;
                  gi.Sherman.IsHullDown = false;
                  gi.Sherman.RotationHull += 60;
                  if (359 < gi.Sherman.RotationHull)
                     gi.Sherman.RotationHull = 0;
                  break;
               case GameAction.BattleRoundSequenceMovementPivotEnd:
                  gi.CrewActionPhase = CrewActionPhase.TankMainGunFire;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMovementPivotEnd): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceMovementRoll:
                  if (false == MoveSherman(gi, ref action, dieRoll))
                  {
                     returnStatus = "MoveSherman() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceBoggedDownRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     if (100 == gi.DieResults[key][0]) //  unmodified roll of 100 means assistance needed
                     {
                        gi.Sherman.IsAssistanceNeeded = true;
                     }
                     else
                     {
                        int modifer = TableMgr.GetBoggedDownModifier(gi);
                        if (TableMgr.FN_ERROR == modifer)
                        {
                           returnStatus = "GetBoggedDownModifier() returned error";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceBoggedDownRoll): " + returnStatus);
                        }
                        else
                        {
                           int combo51a = gi.DieResults[key][0] + modifer;
                           if (combo51a < 11)
                           {
                              gi.Sherman.IsBoggedDown = false;
                           }
                           else if (combo51a < 81)
                           {
                              // do nothing
                           }
                           else if (combo51a < 91)
                           {
                              gi.Sherman.IsThrownTrack = true;
                           }
                           else
                           {
                              gi.Sherman.IsAssistanceNeeded = true;
                           }
                           //---------------------------------------------------
                           gi.CrewActionPhase = CrewActionPhase.TankMainGunFire;
                           if (false == ConductCrewAction(gi, ref action))
                           {
                              returnStatus = "ConductCrewAction() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceBoggedDownRoll): " + returnStatus);
                           }
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceChangeFacingEnd:
                  gi.CrewActionPhase = CrewActionPhase.TankMainGunFire;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceChangeFacingEnd): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceTurretEnd:
                  gi.CrewActionPhase = CrewActionPhase.TankMainGunFire;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceTurretEnd): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceTurretEndRotateLeft:
                  gi.Sherman.RotationTurret -= 60;
                  if (gi.Sherman.RotationTurret < 0)
                     gi.Sherman.RotationTurret = 300;
                  break;
               case GameAction.BattleRoundSequenceTurretEndRotateRight:
                  gi.Sherman.RotationTurret += 60;
                  if (359 < gi.Sherman.RotationTurret)
                     gi.Sherman.RotationTurret = 0;
                  break;
               case GameAction.BattleRoundSequenceShermanFiringSelectTarget:
                  break;
               case GameAction.BattleRoundSequenceShermanFiringMainGun:
                  Logger.Log(LogEnum.LE_SHOW_TO_HIT_ATTACK, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanFiringMainGun): Target Selected");
                  gi.EventDisplayed = gi.EventActive = "e053b";
                  gi.DieRollAction = GameAction.BattleRoundSequenceShermanToHitRoll;
                  break;
               case GameAction.BattleRoundSequenceShermanFiringMainGunEnd:
                  if ((98 == gi.DieResults[key][0]) || (99 == gi.DieResults[key][0]) || (100 == gi.DieResults[key][0]))
                  {
                     gi.IsMalfunctionedMainGun = true;
                     Logger.Log(LogEnum.LE_SHOW_MAIN_GUN_BREAK, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanFiringMainGunEnd): Main Gun Malfunctioned dr=" + gi.DieResults[key][0].ToString());
                  }
                  if (false == gi.FireAndReloadGun())  // BattleRoundSequenceShermanFiringMainGunEnd
                  {
                     returnStatus = "gi.FireAndReloadGun() returned false for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanFiringMainGunEnd): " + returnStatus);
                  }
                  else
                  {
                     gi.CrewActionPhase = CrewActionPhase.TankMgFire;
                     if (false == ConductCrewAction(gi, ref action))
                     {
                        returnStatus = "ConductCrewAction() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanFiringMainGunEnd): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceShermanFiringMainGunNot:
                  gi.CrewActionPhase = CrewActionPhase.TankMgFire;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanFiringMainGunEnd): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceShermanSkipRateOfFire:
                  if (null == gi.TargetMainGun)
                  {
                     returnStatus = "gi.TargetMainGun=null for a=" + action.ToString();
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanSkipRateOfFire): " + returnStatus);
                  }
                  else
                  {
                     if ((true == gi.TargetMainGun.Name.Contains("LW")) || (true == gi.TargetMainGun.Name.Contains("MG")) || (true == gi.TargetMainGun.Name.Contains("Pak")) || (true == gi.TargetMainGun.Name.Contains("ATG")))
                        gi.EventDisplayed = gi.EventActive = "e053d"; // resolve attack
                     else
                        gi.EventDisplayed = gi.EventActive = "e053e"; // resolve attack
                     gi.DieRollAction = GameAction.BattleRoundSequenceShermanToKillRoll;
                  }
                  break;
               case GameAction.BattleRoundSequenceShermanToHitRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 98; // <cgs> TEST - Sherman To Hit Roll
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                     gi.FiredAmmoType = gi.GetGunLoadType();  // used in EventViewer.UpdateEventContentToGetToHit()
                  }
                  else
                  {
                     if (false == FireMainGunAtEnemyUnits(gi, ref action, gi.DieResults[key][0]))
                     {
                        returnStatus = "FireMainGunAtEnemyUnits() returned false";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanToHitRoll): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceLoadMainGun:
                  break;
               case GameAction.BattleRoundSequenceLoadMainGunEnd:
                  gi.EventDisplayed = gi.EventActive = "e060";
                  if (false == gi.ReloadGun())
                  {
                     returnStatus = "ReloadGun() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceLoadMainGun): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceShermanToHitRollNothing:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 98; // <cgs> TEST - Sherman To Hit Roll
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     if ((98 == gi.DieResults[key][0]) || (99 == gi.DieResults[key][0]) || (100 == gi.DieResults[key][0]))
                     {
                        gi.IsMalfunctionedMainGun = true;
                        Logger.Log(LogEnum.LE_SHOW_MAIN_GUN_BREAK, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanToHitRollNothing): Main Gun Malfunctioned dr=" + gi.DieResults[key][0].ToString());
                     }
                     if (false == gi.FireAndReloadGun()) // BattleRoundSequenceShermanToHitRollNothing
                     {
                        returnStatus = "gi.FireAndReloadGun() returned false for a=" + action.ToString();
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanToHitRollNothing): " + returnStatus);
                     }
                     else
                     {
                        gi.CrewActionPhase = CrewActionPhase.TankMgFire;
                        if (false == ConductCrewAction(gi, ref action))
                        {
                           returnStatus = "ConductCrewAction() returned false";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanToHitRollNothing): " + returnStatus);
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceShermanToKillRoll:
                  if (false == ResolveToKillEnemyUnit(gi, ref action, dieRoll))
                  {
                     returnStatus = "ResolveToKillEnemyUnit() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanToKillRoll): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceShermanToKillRollMiss: 
                  if( false == ResolveToKillEnemyUnitCleanup(gi, ref action))
                  {
                     returnStatus = "ResolveToKillEnemyUnit() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanToKillRollMiss): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceFireAaMg:
                  action = GameAction.BattleRoundSequenceShermanFiringSelectTargetMg;
                  gi.EventDisplayed = gi.EventActive = "e054"; // resolve attack
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.TargetMg = null;
                  gi.IsShermanFiringAaMg = true;
                  gi.IsShermanFiringBowMg = false;
                  gi.IsShermanFiringCoaxialMg = false;
                  gi.IsShermanFiringSubMg = false;
                  if (false == GetShermanMgTargets(gi, "Aa"))
                  {
                     returnStatus = "GetShermanMgTargets() returned faulse";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireAaMg): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceFireBowMg:
                  action = GameAction.BattleRoundSequenceShermanFiringSelectTargetMg;
                  gi.EventDisplayed = gi.EventActive = "e054"; // resolve attack
                  gi.TargetMg = null;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.IsShermanFiringAaMg = false;
                  gi.IsShermanFiringBowMg = true;
                  gi.IsShermanFiringCoaxialMg = false;
                  gi.IsShermanFiringSubMg = false;
                  if (true == gi.Sherman.IsHullDown)
                  {
                     returnStatus = "gi.Sherman.IsHullDown=true when firing Bow MG";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireBowMg): " + returnStatus);
                  }
                  else
                  {
                     if (false == GetShermanMgTargets(gi, "Bow"))
                     {
                        returnStatus = "GetShermanMgTargets() returned faulse";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireBowMg): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceFireCoaxialMg:
                  action = GameAction.BattleRoundSequenceShermanFiringSelectTargetMg;
                  gi.EventDisplayed = gi.EventActive = "e054"; // resolve attack
                  gi.TargetMg = null;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.IsShermanFiringAaMg = false;
                  gi.IsShermanFiringBowMg = false;
                  gi.IsShermanFiringCoaxialMg = true;
                  gi.IsShermanFiringSubMg = false;
                  if (false == GetShermanMgTargets(gi, "Coaxial"))
                  {
                     returnStatus = "GetShermanMgTargets() returned faulse";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireCoaxialMg): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceFireSubMg:
                  action = GameAction.BattleRoundSequenceShermanFiringSelectTargetMg;
                  gi.EventDisplayed = gi.EventActive = "e054"; // resolve attack
                  gi.TargetMg = null;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  gi.IsShermanFiringAaMg = false;
                  gi.IsShermanFiringBowMg = false;
                  gi.IsShermanFiringCoaxialMg = false;
                  gi.IsShermanFiringSubMg = true;
                  gi.TargetMg = null;
                  if (false == GetShermanMgTargets(gi, "Sub"))
                  {
                     returnStatus = "GetShermanMgTargets() returned faulse";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireSubMg): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceShermanFiringMachineGun:
                  gi.EventDisplayed = gi.EventActive = "e054a";
                  gi.DieRollAction = GameAction.BattleRoundSequenceFireMachineGunRoll;
                  if (true == gi.IsShermanFiringAaMg) gi.IsShermanFiredAaMg = true;
                  else if (true == gi.IsShermanFiringBowMg) gi.IsShermanFiredBowMg = true;
                  else if (true == gi.IsShermanFiringCoaxialMg) gi.IsShermanFiredCoaxialMg = true;
                  else if (true == gi.IsShermanFiringSubMg) gi.IsShermanFiredSubMg = true;
                  else
                  {
                     returnStatus = "reached default no MG selected";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanFiringMachineGun): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceFireMachineGunRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                     gi.Targets.Clear();  // BattleRoundSequenceFireMachineGunRoll
                     if (null == gi.TargetMg)
                     {
                        returnStatus = " gi.TargetMg=null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMachineGunRoll): " + returnStatus);
                     }
                     else
                     {
                        if ((1 == DieRoller.BlueDie) || (2 == DieRoller.BlueDie) || (3 == DieRoller.BlueDie)) // expend ammo if blue die = 1, 2, or 3
                        {
                           if (true == gi.IsShermanFiringAaMg) lastReport.Ammo50CalibreMG--;
                           else if (true == gi.IsShermanFiringBowMg) lastReport.Ammo30CalibreMG--;
                           else if (true == gi.IsShermanFiringCoaxialMg) lastReport.Ammo30CalibreMG--;
                           else if (true == gi.IsShermanFiringSubMg) { } // do nothing
                           else
                           {
                              returnStatus = "unknown MG";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMachineGunRoll): " + returnStatus);
                           }
                        }
                        //------------------------------------------------------------------
                        bool isError = false;
                        string mgType = "None";
                        if (true == gi.IsShermanFiringAaMg)
                           mgType = "Aa";
                        else if (true == gi.IsShermanFiringBowMg)
                           mgType = "Bow";
                        else if (true == gi.IsShermanFiringCoaxialMg)
                           mgType = "Coaxial";
                        else if (true == gi.IsShermanFiringSubMg)
                           mgType = "Sub";
                        else
                        {
                           returnStatus = "Get_ShermanMgToKillNumber(): unknown MG firing";
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMachineGunRoll): " + returnStatus);
                           isError = true;
                        }
                        if (false == isError)
                        {
                           int toKillNum = TableMgr.GetShermanMgToKillNumber(gi, gi.TargetMg, mgType);
                           if (TableMgr.FN_ERROR == toKillNum)
                           {
                              returnStatus = "Get_ShermanMgToKillNumber() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMachineGunRoll): " + returnStatus);
                           }
                           else if (TableMgr.NO_CHANCE == toKillNum)
                           {
                              // do nothing
                           }
                           else
                           {
                              int modifier = TableMgr.GetShermanMgToKillModifier(gi, gi.TargetMg, mgType);
                              if (TableMgr.FN_ERROR == modifier)
                              {
                                 returnStatus = "Get_ShermanMgToKillModifier() returned false";
                                 Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMachineGunRoll): " + returnStatus);
                              }
                              else
                              {
                                 int total = toKillNum - modifier;
                                 Logger.Log(LogEnum.LE_SHOW_TO_KILL_MG_ATTACK, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMachineGunRoll): (toKillNum=" + toKillNum.ToString() + ") + (mod=" + modifier.ToString() + ") = (total=" + total.ToString() + ") dr=" + gi.DieResults[key][0].ToString());
                                 if ((gi.DieResults[key][0] <= total) || (gi.DieResults[key][0] < 4))
                                 {
                                    gi.ScoreYourVictoryPoint(lastReport, gi.TargetMg);
                                    gi.TargetMg.IsKilled = true;
                                    gi.TargetMg.IsMoving = false;
                                    gi.TargetMg.SetBloodSpots();
                                 }
                                 else if (97 < gi.DieResults[key][0])
                                 {
                                    if (true == gi.IsShermanFiringAaMg)
                                       gi.IsMalfunctionedMgAntiAircraft = true;      // GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMachineGunRoll)
                                    else if (true == gi.IsShermanFiringBowMg)
                                       gi.IsMalfunctionedMgBow = true;               // GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMachineGunRoll)
                                    else if (true == gi.IsShermanFiringCoaxialMg)
                                       gi.IsMalfunctionedMgCoaxial = true;           // GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMachineGunRoll)
                                    else if (true == gi.IsShermanFiringSubMg) { }    // do nothing
                                    else
                                    {
                                       returnStatus = "reached default no MG fired";
                                       Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMachineGunRoll): " + returnStatus);
                                    }
                                 }
                              }
                           }
                        }
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceFireMachineGunRollEnd:
                  gi.DieResults["e054a"][0] = Utilities.NO_RESULT;
                  IMapItems shermanRemovals = new MapItems();
                  foreach (IStack stack in gi.BattleStacks)
                  {
                     foreach (IMapItem mapItem in stack.MapItems)
                     {
                        if (true == mapItem.IsKilled)
                           shermanRemovals.Add(mapItem);
                     }
                  }
                  foreach (IMapItem mi in shermanRemovals)
                     gi.BattleStacks.Remove(mi);
                  //----------------------------------------------
                  gi.CrewActionPhase = CrewActionPhase.TankMgFire;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMachineGunRollEnd): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceFireMgSkip:
                  gi.TargetMg = null;
                  gi.Targets.Clear();  // BattleRoundSequenceFireMgSkip
                  gi.AreaTargets.Clear();
                  gi.CrewActionPhase = CrewActionPhase.ReplacePeriscope;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMgSkip): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequencePlaceAdvanceFire:
                  if (null == gi.AdvanceFire)
                  {
                     returnStatus = "gi.AdvanceFire=null";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePlaceAdvanceFire): " + returnStatus);
                  }
                  else
                  {
                     IMapPoint mpAdvance = Territory.GetRandomPoint(gi.AdvanceFire, Utilities.theMapItemOffset);
                     string miName;
                     IMapItem? advanceMg = null;
                     if (true == gi.IsShermanFiringAaMg)
                     {
                        miName = "Aa_MgAdvanceFire" + Utilities.MapItemNum;
                        gi.IsShermanFiredAaMg = true;
                        advanceMg = new MapItem(miName, 1.0, "c45AdvanceFireAaMg", gi.AdvanceFire);
                     }
                     else if (true == gi.IsShermanFiringBowMg)
                     {
                        miName = "Bow_MgAdvanceFire" + Utilities.MapItemNum;
                        gi.IsShermanFiredBowMg = true;
                        advanceMg = new MapItem(miName, 1.0, "c46AdvanceFireBowMg", gi.AdvanceFire);
                     }
                     else if (true == gi.IsShermanFiringCoaxialMg)
                     {
                        miName = "Coaxial_MgAdvanceFire" + Utilities.MapItemNum;
                        gi.IsShermanFiredCoaxialMg = true;
                        advanceMg = new MapItem(miName, 1.0, "c47AdvanceFireCoaxialMg", gi.AdvanceFire);
                     }
                     else if (true == gi.IsShermanFiringSubMg)
                     {
                        miName = "Sub_MgAdvanceFire" + Utilities.MapItemNum;
                        gi.IsShermanFiredSubMg = true;
                        advanceMg = new MapItem(miName, 1.0, "c46AdvanceFireSubMg", gi.AdvanceFire);
                     }
                     Utilities.MapItemNum++;
                     if (null == advanceMg)
                     {
                        returnStatus = "advanceMg = null";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePlaceAdvanceFire): " + returnStatus);
                     }
                     else
                     {
                        advanceMg.Location = mpAdvance;
                        gi.BattleStacks.Add(advanceMg);
                        gi.AdvanceFire = null;
                        gi.AreaTargets.Clear();
                        gi.EventDisplayed = gi.EventActive = "e054b";
                        gi.DieRollAction = GameAction.BattleRoundSequencePlaceAdvanceFireRoll;
                     }
                  }
                  break;
               case GameAction.BattleRoundSequencePlaceAdvanceFireRoll:
                  gi.DieResults[key][0] = dieRoll;
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  if (gi.DieResults[key][0] < 31) // Assume that sub MG do not use ammo
                  {
                     if (true == gi.IsShermanFiringAaMg)
                        lastReport.Ammo50CalibreMG--;
                     else if ((true == gi.IsShermanFiringBowMg) || (true == gi.IsShermanFiringCoaxialMg))
                        lastReport.Ammo30CalibreMG--;
                  }
                  else if (97 < gi.DieResults[key][0])
                  {
                     if (true == gi.IsShermanFiringAaMg)
                        gi.IsMalfunctionedMgAntiAircraft = true;      // GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePlaceAdvanceFireRoll)
                     else if (true == gi.IsShermanFiringBowMg)
                        gi.IsMalfunctionedMgBow = true;               // GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePlaceAdvanceFireRoll)
                     else if (true == gi.IsShermanFiringCoaxialMg)
                        gi.IsMalfunctionedMgCoaxial = true;           // GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePlaceAdvanceFireRoll)
                     else if (true == gi.IsShermanFiringSubMg) { }  // do nothing
                     else
                     {
                        returnStatus = "reached default no MG fired";
                        Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequencePlaceAdvanceFireRoll): " + returnStatus);
                     }
                  }
                  break;
               case GameAction.BattleRoundSequencePlaceAdvanceFireRollEnd:
                  gi.DieResults[key][0] = Utilities.NO_RESULT;
                  gi.CrewActionPhase = CrewActionPhase.TankMgFire;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFireMgSkip): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceRepairMainGunRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 99; // <cgs> TEST - break main gun
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else if (false == RepairMainGunAttempt(gi, ref action))
                  {
                     returnStatus = "RepairMainGunAttempt() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceRepairMainGunRoll): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceRepairAaMgRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 01; // <cgs> TEST - break AAMG
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else if (false == RepairAntiAircraftMgAttempt(gi, ref action))
                  {
                     returnStatus = "RepairAntiAircraftMgAttempt() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceRepairMainGunRoll): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceRepairBowMgRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 01; // <cgs> TEST - break BMG
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else if (false == RepairBowMgAttempt(gi, ref action))
                  {
                     returnStatus = "RepairBowMgAttempt() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceRepairBowMgRoll): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceRepairCoaxialMgRoll:
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 01; // <cgs> TEST - break CMG
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else if (false == RepairCoaxialMgAttempt(gi, ref action))
                  {
                     returnStatus = "RepairCoaxialMgAttempt() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceRepairCoaxialMgRoll): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReplacePeriscopes:
                  foreach (IMapItem crewAction in gi.CrewActions)
                  {
                     if (true == crewAction.Name.Contains("Driver_RepairScope"))
                     {
                        lastReport.AmmoPeriscope--;
                        gi.IsBrokenPeriscopeDriver = false;
                     }
                     if (true == crewAction.Name.Contains("Loader_RepairScope"))
                     {
                        lastReport.AmmoPeriscope--;
                        gi.IsBrokenPeriscopeLoader = false;
                     }
                     if (true == crewAction.Name.Contains("Assistant_RepairScope"))
                     {
                        lastReport.AmmoPeriscope--;
                        gi.IsBrokenPeriscopeAssistant = false;
                     }
                     if (true == crewAction.Name.Contains("Gunner_RepairScope"))
                     {
                        lastReport.AmmoPeriscope--;
                        gi.IsBrokenPeriscopeGunner = false;
                     }
                     if (true == crewAction.Name.Contains("Commander_RepairScope"))
                     {
                        lastReport.AmmoPeriscope--;
                        gi.IsBrokenPeriscopeCommander = false;
                     }
                  }
                  if (lastReport.AmmoPeriscope < 0)
                  {
                     returnStatus = "lastReport.AmmoPeriscope < 0";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceReplacePeriscopes): " + returnStatus);
                  }
                  gi.CrewActionPhase = CrewActionPhase.RepairGun;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceReplacePeriscopes): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceShermanFiringMortar:
                  if (false == FireMortarIntoTurretFront(gi, ref action))
                  {
                     returnStatus = "FireMortarIntoTurrentFront() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanFiringMortar): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceShermanThrowGrenade:
                  string miNameGrenade = "SmokeWhite" + Utilities.MapItemNum;
                  Utilities.MapItemNum++;
                  double zoom = Utilities.ZOOM + 3.0;
                  IMapItem smokeGrenade = new MapItem(miNameGrenade, zoom, "c108Smoke1", gi.Home);
                  IMapPoint mp1 = Territory.GetRandomPoint(gi.Home, 10);
                  smokeGrenade.Location.X = gi.Home.CenterPoint.X - zoom * Utilities.theMapItemOffset;
                  smokeGrenade.Location.Y = mp1.Y - zoom * Utilities.theMapItemOffset;
                  gi.BattleStacks.Add(smokeGrenade);
                  //--------------------------------------------------
                  gi.CrewActionPhase = CrewActionPhase.RestockReadyRack;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceShermanThrowGrenade): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReadyRackHeMinus:
                  if (false == UpdateReadyRackMinus(gi, "He"))
                  {
                     returnStatus = "UpdateReadyRackMinus() returned false for He";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReadyRackApMinus:
                  if (false == UpdateReadyRackMinus(gi, "Ap"))
                  {
                     returnStatus = "UpdateReadyRackMinus() returned false for Ap";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReadyRackWpMinus:
                  if (false == UpdateReadyRackMinus(gi, "Wp"))
                  {
                     returnStatus = "UpdateReadyRackMinus() returned false for Wp";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReadyRackHbciMinus:
                  if (false == UpdateReadyRackMinus(gi, "Hbci"))
                  {
                     returnStatus = "UpdateReadyRackMinus() returned false for Hbci";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReadyRackHvapMinus:
                  if (false == UpdateReadyRackMinus(gi, "Hvap"))
                  {
                     returnStatus = "UpdateReadyRackMinus() returned false for Hvap";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReadyRackHePlus:
                  if (false == UpdateReadyRackPlus(gi, "He", lastReport.MainGunHE))
                  {
                     returnStatus = "UpdateReadyRackPlus() returned false for He";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReadyRackApPlus:
                  if (false == UpdateReadyRackPlus(gi, "Ap", lastReport.MainGunAP))
                  {
                     returnStatus = "UpdateReadyRackPlus() returned false for Ap";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReadyRackWpPlus:
                  if (false == UpdateReadyRackPlus(gi, "Wp", lastReport.MainGunWP))
                  {
                     returnStatus = "UpdateReadyRackPlus() returned false for Wp";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReadyRackHbciPlus:
                  if (false == UpdateReadyRackPlus(gi, "Hbci", lastReport.MainGunHBCI))
                  {
                     returnStatus = "UpdateReadyRackPlus() returned false for Hbci";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReadyRackHvapPlus:
                  if (false == UpdateReadyRackPlus(gi, "Hvap", lastReport.MainGunHVAP))
                  {
                     returnStatus = "UpdateReadyRackPlus() returned false for Hvap";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceReadyRackEnd: 
                  gi.CrewActionPhase = CrewActionPhase.CrewSwitch;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceReadyRackEnd): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceCrewSwitchEnd:  
                  gi.CrewActionPhase = CrewActionPhase.None;
                  if (false == ConductCrewAction(gi, ref action))
                  {
                     returnStatus = "ConductCrewAction() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceCrewSwitchEnd): " + returnStatus);
                  }
                  break;
               case GameAction.BattleCrewReplaced:
                  if (false == ResetToPrepareForBattle(gi, lastReport)) // GameStateBattle.PerformAction(Battle_CrewReplaced)
                  {
                     returnStatus = "Reset_ToPrepareForBattle() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattle.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceCrewReplaced:
                  if (false == MoveShermanAdvanceOrRetreat(gi)) // GameStateBattleRoundSequence.PerformAction(BattleRoundSequence_CrewReplaced) 
                  {
                     returnStatus = "MoveSherman_AdvanceOrRetreat() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(Battle_CrewReplaced): " + returnStatus);
                  }
                  break;
               case GameAction.BattleRoundSequenceEnemyAction:
                  Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceEnemyAction): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.EnemyAction");
                  gi.BattlePhase = BattlePhase.EnemyAction;
                  break;
               case GameAction.BattleRoundSequenceFriendlyAction:
                  Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceFriendlyAction): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.FriendlyAction");
                  gi.BattlePhase = BattlePhase.FriendlyAction;
                  break;
               case GameAction.BattleRoundSequenceCollateralDamageCheck: // Handled with EventViewerTankCollateral class
                  break;
               case GameAction.BattleResolveArtilleryFire:
                  break;
               case GameAction.BattleRoundSequenceRandomEvent:
                  Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "GameStateBattleRoundSequence.PerformAction(BattleRandomEvent): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.AmbushRandomEvent");
                  gi.BattlePhase = BattlePhase.RandomEvent;
                  if (EnumScenario.Advance == lastReport.Scenario)
                     gi.EventActive = gi.EventDisplayed = "e039a";
                  else if (EnumScenario.Battle == lastReport.Scenario)
                     gi.EventActive = gi.EventDisplayed = "e039b";                // GameStateBattleRoundSequence.PerformAction(BattleRandomEvent)
                  else if (EnumScenario.Counterattack == lastReport.Scenario)
                     gi.EventActive = gi.EventDisplayed = "e039c";
                  else
                  {
                     returnStatus = "unkonwn sceanrio=" + lastReport.Scenario;
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRandomEvent): " + returnStatus);
                  }
                  gi.DieResults["e039b"][0] = Utilities.NO_RESULT;
                  gi.DieRollAction = GameAction.BattleRandomEventRoll;
                  break;
               case GameAction.BattleRandomEventRoll:
                  bool isEnemyUnitLeft = false;
                  foreach (IStack stack in gi.BattleStacks)
                  {
                     foreach (IMapItem mi in stack.MapItems)
                     {
                        if (true == mi.IsEnemyUnit())
                        {
                           isEnemyUnitLeft = true;
                           break;
                        }
                     }
                  }
                  //-----------------------------------------------
                  if (Utilities.NO_RESULT == gi.DieResults[key][0])
                  {
                     //dieRoll = 01; // <cgs> TEST - AdvanceRetreat - Random Event - Time passes
                     gi.DieResults[key][0] = dieRoll;
                     gi.DieRollAction = GameAction.DieRollActionNone;
                  }
                  else
                  {
                     string randomEvent = TableMgr.GetRandomEvent(lastReport.Scenario, gi.DieResults[key][0]);
                     switch (randomEvent)
                     {
                        case "Time Passes":
                           gi.EventDisplayed = gi.EventActive = "e040";
                           AdvanceTime(lastReport, 15);
                           break;
                        case "Friendly Artillery":
                           if (true == isEnemyUnitLeft)
                           {
                              action = GameAction.BattleResolveArtilleryFire;
                           }
                           else if (false == NextStepAfterRandomEvent(gi, ref action))
                           {
                              returnStatus = "NextStepAfterRandomEvent() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRandomEventRoll): Friendly Artillery" + returnStatus);
                           }
                           break;
                        case "Enemy Artillery":
                           gi.EventDisplayed = gi.EventActive = "e042";
                           gi.DieRollAction = GameAction.BattleRoundSequenceEnemyArtilleryRoll;
                           break;
                        case "Mines":
                           if (true == gi.Sherman.IsMoving)
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
                           gi.NumCollateralDamage++;
                           if( false == HarrassingFireCheck(gi))
                           {
                              returnStatus = "HarrassingFireCheck() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRandomEventRoll): " + returnStatus);
                           }
                           break;
                        case "Friendly Advance":
                           if (false == FriendlyAdvanceCheck(gi, ref action))
                           {
                              returnStatus = "FriendlyAdvanceCheck() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRandomEventRoll): " + returnStatus);
                           }
                           break;
                        case "Enemy Reinforce":
                           action = GameAction.BattleActivation;
                           break;
                        case "Enemy Advance":
                           if (false == EnemyAdvanceCheck(gi, ref action))
                           {
                              returnStatus = "EnemyAdvanceCheck() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRandomEventRoll): " + returnStatus);
                           }
                           break;
                        case "Flanking Fire":
                           if (true == isEnemyUnitLeft)
                           {
                              action = GameAction.BattleResolveArtilleryFire;
                              gi.IsFlankingFire = true;
                           }
                           else if (false == NextStepAfterRandomEvent(gi, ref action))
                           {
                              returnStatus = "NextStepAfterRandomEvent() returned false";
                              Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceMinefieldRoll): " + returnStatus);
                           }
                           break;
                        default:
                           returnStatus = "reached default with randomEvent=" + randomEvent;
                           Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRandomEventRoll): " + returnStatus);
                           break;
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceBackToSpotting:
                  if (false == ResetRound(gi, "GameStateBattleRoundSequence(BattleRoundSequenceBackToSpotting)"))    // BattleRoundSequenceBackToSpotting
                  {
                     returnStatus = "ResetRound() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceBackToSpotting): " + returnStatus);
                  }
                  else if (("None" == gi.GetGunLoadType()) && (BattlePhase.BackToSpotting == gi.BattlePhase))
                  {
                     int totalAmmoBackToSpotting = lastReport.MainGunHE + lastReport.MainGunAP + lastReport.MainGunWP + lastReport.MainGunHBCI + lastReport.MainGunHVAP;
                     if (0 < totalAmmoBackToSpotting)
                     {
                        action = GameAction.BattleRoundSequenceLoadMainGun;
                        gi.EventDisplayed = gi.EventActive = "e050a";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                     }
                     else
                     {
                        gi.EventDisplayed = gi.EventActive = "e050b";
                        gi.DieRollAction = GameAction.DieRollActionNone;
                     }
                  }
                  break;
               case GameAction.BattleRoundSequenceNextActionAfterRandomEvent:
                  if (false == NextStepAfterRandomEvent(gi, ref action))
                  {
                     returnStatus = "NextStepAfterRandomEvent() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleRoundSequenceNextActionAfterRandomEvent): " + returnStatus);
                  }
                  break;
               case GameAction.BattleShermanKilled:
                  gi.ClearCrewActions("GameStateBattleRoundSequence.PerformAction(BattleShermanKilled)"); // GameStateBattleRoundSequence.PerformAction(BattleShermanKilled)
                  break;
               case GameAction.UpdateTankExplosion:
               case GameAction.UpdateTankBrewUp:
                  gi.ClearCrewActions("GameStateBattleRoundSequence.PerformAction(UpdateTankBrewUp)");
                  gi.BattleStacks.Remove(gi.Sherman);
                  break;
               case GameAction.BattleEmpty:
                  gi.EventDisplayed = gi.EventActive = "e036";
                  break;
               case GameAction.BattleEmptyResolve:
                  gi.ClearCrewActions("GameStateBattleRoundSequence.PerformAction(BattleEmptyResolve)");            // GameStateBattleRoundSequence.PerkformAction(BattleEmptyResolve)
                  if (false == ResetRound(gi, "GameStateBattleRoundSequence(BattleEmptyResolve)"))     // GameStateBattleRoundSequence.PerkformAction(BattleEmptyResolve)
                  {
                     returnStatus = "ResetRound() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(BattleEmptyResolve): " + returnStatus);
                  }
                  if (false == ResolveEmptyBattleBoard(gi, lastReport)) // GameStateBattleRoundSequence.PerformAction(BattleEmptyResolve)
                  {
                     returnStatus = "Resolve_EmptyBattleBoard() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): " + returnStatus);
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
      private bool BattleRoundSequenceStart(IGameInstance gi, ref GameAction action)
      {
         //-------------------------------------------------------
         if (false == ResetRound(gi, "BattleRoundSequenceStart()", false))
         {
            Logger.Log(LogEnum.LE_ERROR, "BattleRoundSequenceStart(): ResetRound() returned false");
            return false;
         }
         //-------------------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "BattleRoundSequenceStart(): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.Spotting");
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
            if (false == SpottingPhaseBegin(gi, ref action, "BattleRoundSequenceStart()"))
            {
               Logger.Log(LogEnum.LE_ERROR, "BattleRoundSequenceStart(): SpottingPhaseBegin() returned false");
               return false;
            }
         }
         return true;
      }
      private bool CheckCrewMemberExposed(IGameInstance gi, ref GameAction outAction)
      {
         bool isCrewExposed = false;
         string[] crewmembers = new string[5] { "Driver", "Assistant", "Commander", "Loader", "Gunner" };
         foreach (string crewmember in crewmembers)
         {
            ICrewMember? cm = gi.GetCrewMemberByRole(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "CheckCrewMemberExposed(): gi.GetCrewMemberByRole() returned null");
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
         else if (false == SpottingPhaseBegin(gi, ref outAction, "CheckCrewMemberExposed()"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckCrewMemberExposed(): SpottingPhaseBegin() returned false");
            return false;
         }
         return true;
      }
      private bool SpottingPhaseBegin(IGameInstance gi, ref GameAction outAction, string caller)
      {
         string[] crewmembers = new string[5] { "Driver", "Assistant", "Commander", "Loader", "Gunner" };
         foreach (string crewmember in crewmembers)
         {
            ICrewMember? cm = gi.GetCrewMemberByRole(crewmember);
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "SpottingPhaseBegin(): gi.GetCrewMemberByRole() returned null");
               return false;
            }
            else
            {
               List<string>? spottedTerritories = Territory.GetSpottedTerritories(gi, cm);
               if (null == spottedTerritories)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SpottingPhaseBegin(): GetSpottedTerritories() returned null");
                  return false;
               }
               if (true == Logger.theLogLevel[(int)LogEnum.LE_EVENT_VIEWER_SPOTTING])
               {
                  StringBuilder sb = new StringBuilder("SpottingPhaseBegin(): " + caller + ": cm=");
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
                  Logger.Log(LogEnum.LE_EVENT_VIEWER_SPOTTING, sb.ToString());
               }
               if (0 < spottedTerritories.Count)
               {
                  outAction = GameAction.BattleRoundSequenceSpotting;
                  return true;
               }
            }
         }
         gi.ClearCrewActions("SpottingPhaseBegin()");         // SpottingPhaseBegin() - skipping spotting b/c nobody to find
         outAction = GameAction.BattleRoundSequenceCrewOrders;
         Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "SpottingPhaseBegin(): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.MarkCrewAction");
         gi.BattlePhase = BattlePhase.MarkCrewAction;
         gi.EventDisplayed = gi.EventActive = "e038";
         gi.DieRollAction = GameAction.DieRollActionNone;
         return true;
      }
      private bool SetDefaultCrewActions(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Conduct_CrewAction(): lastReport=null");
            return false;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         string tType = lastReport.TankCardNum.ToString();
         //----------------------------------------------------
         bool isDriverDefaultNeeded = true;
         bool isLoaderDefaultNeeded = true;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if (true == crewAction.Name.Contains("Driver"))
               isDriverDefaultNeeded = false;
            if (true == crewAction.Name.Contains("Loader"))
               isLoaderDefaultNeeded = false;
         }
         //----------------------------------------------------
         ICrewMember? driver = gi.GetCrewMemberByRole("Driver");
         if( null == driver )
         {
            Logger.Log(LogEnum.LE_ERROR, "SetDefaultCrewActions(): gi.GetCrewMemberByRole(Driver) returned null");
            return false;
         }
         if ( (true == isDriverDefaultNeeded) && (false == driver.IsIncapacitated)) // SetDefaultCrewActions() - if not incapacitated
         {
            ITerritory? t = Territories.theTerritories.Find("DriverAction", tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetDefaultCrewActions(): t=null for tName=DriverAction");
               return false;
            }
            IMapItem mi = new MapItem("Driver_Stop", 1.0, "c61DStop", t);
            gi.CrewActions.Add(mi);
            Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "SetDefaultCrewActions(): ++++++++++++++++adding ca=" + mi.Name);
         }
         //----------------------------------------------------
         ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
         if (null == loader)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetDefaultCrewActions(): gi.GetCrewMemberByRole(Loader) returned null");
            return false;
         }
         if ( (true == isLoaderDefaultNeeded) && (false == loader.IsIncapacitated)) // SetDefaultCrewActions() - if not incapacitated
         {
            ITerritory? t = Territories.theTerritories.Find("LoaderAction", tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetDefaultCrewActions(): t=null for tName=LoaderAction");
               return false;
            }
            IMapItem mi = new MapItem("Loader_Load", 1.0, "c54LLoad", t);
            gi.CrewActions.Add(mi);
            Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "SetDefaultCrewActions(): +++++++++++++++adding ca=" + mi.Name);
         }
         return true;
      }
      private bool ConductCrewAction(IGameInstance gi, ref GameAction outAction)
      {
         Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): Entering++++++++++++ bp=" + gi.BattlePhase.ToString() + " cp=" + gi.CrewActionPhase.ToString());
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Conduct_CrewAction(): lastReport=null");
            return false;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //---------------------------------------------------------
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if ("Loader_ChangeGunLoad" == crewAction.Name)
            {
               gi.GunLoads.Clear();
               break;
            }
         }
         //---------------------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "Conduct_CrewAction(): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.ConductCrewAction");
         gi.BattlePhase = BattlePhase.ConductCrewAction;
         if (CrewActionPhase.Movement == gi.CrewActionPhase)
         {
            gi.CrewActionPhase = CrewActionPhase.TankMainGunFire;
            bool isTankMoving = false;
            bool isTankPivoting = false;
            foreach (IMapItem crewAction in gi.CrewActions)
            {
               if ("Driver_Forward" == crewAction.Name)
                  isTankMoving = true;
               if ("Driver_ForwardToHullDown" == crewAction.Name)
                  isTankMoving = true;
               if ("Driver_Reverse" == crewAction.Name)
                  isTankMoving = true;
               if ("Driver_ReverseToHullDown" == crewAction.Name)
                  isTankMoving = true;
               if ("Driver_PivotTank" == crewAction.Name)
               {
                  isTankPivoting = true;
                  isTankMoving = true;
               }
            }
            if (false == isTankMoving)
            {
               gi.Sherman.IsMoving = false;
            }
            else
            {
               gi.CrewActionPhase = CrewActionPhase.Movement;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 1-phase=" + gi.CrewActionPhase.ToString() + "  isMove=" + isTankMoving.ToString() + " isPivot=" + isTankPivoting.ToString());
               gi.Sherman.IsMoving = true;
               gi.Sherman.IsMoved = true;
               //--------------------------------------------------
               if ((false == gi.IsShermanHvss) && (null != gi.TargetMainGun) ) // if tank moves or pivots, acquired modifer drops to zero
               {
                  Logger.Log(LogEnum.LE_SHOW_NUM_SHERMAN_SHOTS, "Conduct_CrewAction(): zero NumOfAcquiredMarker=" + gi.TargetMainGun.NumOfAcquiredMarker.ToString());
                  gi.TargetMainGun.NumOfAcquiredMarker = 0; // Conduct_CrewAction() - Tank moves wihtout HVSS
               }
               //--------------------------------------------------
               foreach (IStack stack in gi.BattleStacks)
               {
                  foreach (IMapItem mi in stack.MapItems)
                  {
                     if (EnumSpottingResult.HIDDEN == mi.Spotting)
                        mi.Spotting = EnumSpottingResult.UNSPOTTED;
                  }
               }
               //--------------------------------------------------
               if (true == isTankPivoting)
               {
                  gi.EventDisplayed = gi.EventActive = "e052";
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
               else
               {
                  if (false == gi.Sherman.IsBoggedDown)
                  {
                     gi.EventDisplayed = gi.EventActive = "e051";
                     gi.DieRollAction = GameAction.BattleRoundSequenceMovementRoll;
                  }
                  else
                  {
                     gi.Sherman.IsMoving = false;
                     gi.EventDisplayed = gi.EventActive = "e051a";
                     gi.DieRollAction = GameAction.BattleRoundSequenceBoggedDownRoll;
                  }
               }
            }
         }
         //---------------------------------------------------------
         if (CrewActionPhase.TankMainGunFire == gi.CrewActionPhase)
         {
            gi.CrewActionPhase = CrewActionPhase.TankMgFire;
            bool isTankFiringMainGun = false;
            bool isRotateTurret = false;
            foreach (IMapItem crewAction in gi.CrewActions)
            {
               if ("Gunner_FireMainGun" == crewAction.Name)
                  isTankFiringMainGun = true;
               if ("Gunner_RotateFireMainGun" == crewAction.Name)
               {
                  isTankFiringMainGun = true;
                  isRotateTurret = true;
               }
               if ("Gunner_RotateTurret" == crewAction.Name)
                  isRotateTurret = true;
            }
            if ((true == isRotateTurret) && (false == gi.IsShermanTurretRotated))
            {
               gi.IsShermanTurretRotated = true;
               if ((false == isTankFiringMainGun) && (null != gi.TargetMainGun) )
                  gi.TargetMainGun.NumOfAcquiredMarker = 0;  // Conduct_CrewAction() - Turret rotated without firing main gun causes target acquisition = 0
               gi.CrewActionPhase = CrewActionPhase.TankMainGunFire;
               gi.EventDisplayed = gi.EventActive = "e052a";
               gi.DieRollAction = GameAction.DieRollActionNone;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 3-phase=" + gi.CrewActionPhase.ToString());
            }
            else if ((true == isTankFiringMainGun) && ((false == gi.Sherman.IsMoved) || (true == gi.IsShermanHvss)))
            {
               if (false == GetShermanTargets(gi, ref outAction))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Conduct_CrewAction(): GetShermanTargets() returned false");
                  return false;
               }
               if (0 < gi.Targets.Count)
               {
                  outAction = GameAction.BattleRoundSequenceShermanFiringSelectTarget;
                  gi.CrewActionPhase = CrewActionPhase.TankMainGunFire;
                  gi.EventDisplayed = gi.EventActive = "e053";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  Logger.Log(LogEnum.LE_SHOW_TO_HIT_ATTACK, "Conduct_CrewAction(): Select Target");
                  Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 4-phase=" + gi.CrewActionPhase.ToString());
               }
               else
               {
                  gi.CrewActionPhase = CrewActionPhase.TankMainGunFire;
                  gi.EventDisplayed = gi.EventActive = "e053a";
                  gi.DieRollAction = GameAction.BattleRoundSequenceShermanToHitRollNothing;
                  Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 5-phase=" + gi.CrewActionPhase.ToString());
               }
            }
         }
         //---------------------------------------------------------
         if (CrewActionPhase.TankMgFire == gi.CrewActionPhase)
         {
            gi.CrewActionPhase = CrewActionPhase.ReplacePeriscope;
            bool isMgFire = false;
            foreach (IMapItem crewAction in gi.CrewActions)
            {
               if (("Loader_FireSubMg" == crewAction.Name) && (false == gi.IsShermanFiredSubMg))
                  isMgFire = true;
               if (("Loader_FireAaMg" == crewAction.Name) && (false == gi.IsShermanFiredAaMg))
                  isMgFire = true;
               if (("Gunner_FireCoaxialMg" == crewAction.Name) && (false == gi.IsShermanFiredCoaxialMg))
                  isMgFire = true;
               if (("Assistant_FireBowMg" == crewAction.Name) && (false == gi.IsShermanFiredBowMg))
                  isMgFire = true;
               if (("Commander_FireSubMg" == crewAction.Name) && (false == gi.IsShermanFiredSubMg))
                  isMgFire = true;
               if (("Commander_FireAaMg" == crewAction.Name) && (false == gi.IsShermanFiredAaMg))
                  isMgFire = true;
            }
            if (true == isMgFire)
            {
               gi.EventDisplayed = gi.EventActive = "e054";
               gi.DieRollAction = GameAction.DieRollActionNone;
               gi.CrewActionPhase = CrewActionPhase.TankMgFire;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 6-phase=" + gi.CrewActionPhase.ToString());
            }
         }
         //---------------------------------------------------------
         if (CrewActionPhase.ReplacePeriscope == gi.CrewActionPhase)
         {
            gi.CrewActionPhase = CrewActionPhase.RepairGun;
            bool isReplacePeriscope = false;
            foreach (IMapItem crewAction in gi.CrewActions)
            {
               if ("Loader_RepairScope" == crewAction.Name)
                  isReplacePeriscope = true;
               if ("Driver_RepairScope" == crewAction.Name)
                  isReplacePeriscope = true;
               if ("Gunner_RepairScope" == crewAction.Name)
                  isReplacePeriscope = true;
               if ("Assistant_RepairScope" == crewAction.Name)
                  isReplacePeriscope = true;
               if ("Commander_RepairScope" == crewAction.Name)
                  isReplacePeriscope = true;
            }
            if (true == isReplacePeriscope)
            {
               gi.CrewActionPhase = CrewActionPhase.ReplacePeriscope;
               gi.EventDisplayed = gi.EventActive = "e055";
               gi.DieRollAction = GameAction.DieRollActionNone;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 10-phase=" + gi.CrewActionPhase.ToString());
            }
         }
         //---------------------------------------------------------
         if (CrewActionPhase.RepairGun == gi.CrewActionPhase)
         {
            gi.CrewActionPhase = CrewActionPhase.FireMortar;
            bool isRepairMainGun = false;
            bool isRepairAaMg = false;
            bool isRepairCoaxialMg = false;
            bool isRepairBowMg = false;
            foreach (IMapItem crewAction in gi.CrewActions)
            {
               if ("Loader_RepairMainGun" == crewAction.Name)
                  isRepairMainGun = true;
               if ("Loader_RepairCoaxialMg" == crewAction.Name)
                  isRepairCoaxialMg = true;
               if ("Assistant_RepairBowMg" == crewAction.Name)
                  isRepairBowMg = true;
               if ("Loader_RepairAaMg" == crewAction.Name)
                  isRepairAaMg = true;
               if ("Commander_RepairAaMg" == crewAction.Name)
                  isRepairAaMg = true;
            }
            if ((true == isRepairMainGun) && (false == gi.IsMainGunRepairAttempted))
            {
               gi.IsMainGunRepairAttempted = true;
               gi.EventDisplayed = gi.EventActive = "e056";
               gi.DieRollAction = GameAction.BattleRoundSequenceRepairMainGunRoll;
               gi.CrewActionPhase = CrewActionPhase.RepairGun;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 11-phase=" + gi.CrewActionPhase.ToString());
            }
            else if ((true == isRepairAaMg) && (false == gi.IsAaMgRepairAttempted))
            {
               gi.IsAaMgRepairAttempted = true;
               gi.EventDisplayed = gi.EventActive = "e056a";
               gi.DieRollAction = GameAction.BattleRoundSequenceRepairAaMgRoll;
               gi.CrewActionPhase = CrewActionPhase.RepairGun;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 12-phase=" + gi.CrewActionPhase.ToString());
            }
            else if ((true == isRepairBowMg) && (false == gi.IsBowMgRepairAttempted))
            {
               gi.IsBowMgRepairAttempted = true;
               gi.EventDisplayed = gi.EventActive = "e056b";
               gi.DieRollAction = GameAction.BattleRoundSequenceRepairCoaxialMgRoll;
               gi.CrewActionPhase = CrewActionPhase.RepairGun;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 13-phase=" + gi.CrewActionPhase.ToString());
            }
            else if ((true == isRepairCoaxialMg) && (false == gi.IsCoaxialMgRepairAttempted))
            {
               gi.IsCoaxialMgRepairAttempted = true;
               gi.EventDisplayed = gi.EventActive = "e056c";
               gi.DieRollAction = GameAction.BattleRoundSequenceRepairBowMgRoll;
               gi.CrewActionPhase = CrewActionPhase.RepairGun;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 14-phase=" + gi.CrewActionPhase.ToString());
            }
         }
         //---------------------------------------------------------
         if (CrewActionPhase.FireMortar == gi.CrewActionPhase)
         {
            gi.CrewActionPhase = CrewActionPhase.ThrowGrenades;
            bool isFireMortar = false;
            foreach (IMapItem crewAction in gi.CrewActions)
            {
               if ("Loader_FireMortar" == crewAction.Name)
                  isFireMortar = true;
            }
            if (true == isFireMortar)
            {
               gi.EventDisplayed = gi.EventActive = "e057";
               gi.CrewActionPhase = CrewActionPhase.FireMortar;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 15-phase=" + gi.CrewActionPhase.ToString());
            }
         }
         //---------------------------------------------------------
         if (CrewActionPhase.ThrowGrenades == gi.CrewActionPhase)
         {
            gi.CrewActionPhase = CrewActionPhase.RestockReadyRack;
            bool isThrowGrenade = false;
            foreach (IMapItem crewAction in gi.CrewActions)
            {
               if ("Gunner_ThrowGrenade" == crewAction.Name)
                  isThrowGrenade = true;
               if ("Commander_ThrowGrenade" == crewAction.Name)
                  isThrowGrenade = true;
            }
            if (true == isThrowGrenade)
            {
               gi.EventDisplayed = gi.EventActive = "e058";
               gi.CrewActionPhase = CrewActionPhase.ThrowGrenades;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 16-phase=" + gi.CrewActionPhase.ToString());
            }
         }
         //---------------------------------------------------------
         if (CrewActionPhase.RestockReadyRack == gi.CrewActionPhase)
         {
            gi.CrewActionPhase = CrewActionPhase.CrewSwitch;
            bool isRestockReady = false;
            foreach (IMapItem crewAction in gi.CrewActions)
            {
               if ("Loader_RestockReadyRack" == crewAction.Name)
                  isRestockReady = true;
            }
            if (true == isRestockReady)
            {
               gi.EventDisplayed = gi.EventActive = "e059";
               gi.CrewActionPhase = CrewActionPhase.RestockReadyRack;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 17-phase=" + gi.CrewActionPhase.ToString());
            }
         }
         //---------------------------------------------------------
         if (CrewActionPhase.CrewSwitch == gi.CrewActionPhase)
         {
            gi.CrewActionPhase = CrewActionPhase.None;
            bool isLoaderSwitch = false;
            bool isDriverSwitch = false;
            bool isGunnerSwitch = false;
            bool isCommanderSwitch = false;
            bool isAssistantSwitch = false;
            foreach (IMapItem crewAction in gi.CrewActions)
            {
               if ("Assistant_SwitchLdr" == crewAction.Name)
                  isLoaderSwitch = true;
               if ("Assistant_SwitchDvr" == crewAction.Name)
                  isDriverSwitch = true;
               if ("Assistant_SwitchGunr" == crewAction.Name)
                  isGunnerSwitch = true;
               if ("Assistant_SwitchCmdr" == crewAction.Name)
                  isCommanderSwitch = true;
               if ("Assistant_SwitchAsst" == crewAction.Name)
                  isAssistantSwitch = true;
            }
            if (true == isLoaderSwitch)
            {
               gi.EventDisplayed = gi.EventActive = "e061";
               gi.CrewActionPhase = CrewActionPhase.CrewSwitch;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 18a-phase=" + gi.CrewActionPhase.ToString());
               if (false == gi.SwitchMembers("Loader"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Conduct_CrewAction(): SwitchedCrewMember() returned false for cm=Loader");
                  return false;
               }
            }
            else if (true == isDriverSwitch)
            {
               gi.EventDisplayed = gi.EventActive = "e061";
               gi.CrewActionPhase = CrewActionPhase.CrewSwitch;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 18b-phase=" + gi.CrewActionPhase.ToString());
               if (false == gi.SwitchMembers("Driver"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Conduct_CrewAction(): SwitchedCrewMember() returned false for cm=Driver");
                  return false;
               }
            }
            else if (true == isGunnerSwitch)
            {
               gi.EventDisplayed = gi.EventActive = "e061";
               gi.CrewActionPhase = CrewActionPhase.CrewSwitch;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 18c-phase=" + gi.CrewActionPhase.ToString());
               if (false == gi.SwitchMembers("Gunner"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Conduct_CrewAction(): SwitchedCrewMember() returned false for cm=Gunner");
                  return false;
               }
            }
            else if (true == isCommanderSwitch)
            {
               gi.EventDisplayed = gi.EventActive = "e061";
               gi.CrewActionPhase = CrewActionPhase.CrewSwitch;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 18d-phase=" + gi.CrewActionPhase.ToString());
               if (false == gi.SwitchMembers("Commander"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Conduct_CrewAction(): SwitchedCrewMember() returned false for cm=Commander");
                  return false;
               }
            }
            else if (true == isAssistantSwitch)
            {
               gi.EventDisplayed = gi.EventActive = "e061a";
               gi.CrewActionPhase = CrewActionPhase.CrewSwitch;
               Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): 18d-phase=" + gi.CrewActionPhase.ToString());
               if (false == gi.SwitchMembers("Assistant"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Conduct_CrewAction(): SwitchedCrewMember() returned false for switching back assistant");
                  return false;
               }
            }
         }
         //---------------------------------------------------------
         if (CrewActionPhase.None == gi.CrewActionPhase)
         {
            Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "Conduct_CrewAction(): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.RandomEvent");
            gi.BattlePhase = BattlePhase.RandomEvent;              // Skip to RandomEvent if no enemy units
            if (EnumScenario.Advance == lastReport.Scenario)
               gi.EventActive = gi.EventDisplayed = "e039a";
            else if (EnumScenario.Battle == lastReport.Scenario)
               gi.EventActive = gi.EventDisplayed = "e039b";            //ConductCrewAction()
            else if (EnumScenario.Counterattack == lastReport.Scenario)
               gi.EventActive = gi.EventDisplayed = "e039c";
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "Conduct_CrewAction(): unknown scenario=" + lastReport.Scenario);
               return false;
            }
            gi.DieResults["e039b"][0] = Utilities.NO_RESULT;
            gi.DieRollAction = GameAction.BattleRandomEventRoll;
            outAction = GameAction.BattleRoundSequenceRandomEvent; // random event if no more enemy units
            foreach (IStack stack in gi.BattleStacks)
            {
               foreach (IMapItem mi in stack.MapItems)
               {
                  if (true == mi.IsEnemyUnit())
                  {
                     Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "Conduct_CrewAction(): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.EnemyAction");
                     gi.BattlePhase = BattlePhase.EnemyAction;
                     outAction = GameAction.BattleRoundSequenceEnemyAction;
                     break;
                  }
               }
               if (BattlePhase.EnemyAction == gi.BattlePhase)  // if enemys exist, next phase is enemy action
                  break;
            }
         }
         Logger.Log(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION, "Conduct_CrewAction(): Exiting------------ bp=" + gi.BattlePhase.ToString() + " cp=" + gi.CrewActionPhase.ToString());
         return true;
      }
      private bool MoveSherman(IGameInstance gi, ref GameAction outAction, int dieRoll)
      {
         string key = gi.EventActive;
         if (Utilities.NO_RESULT == gi.DieResults[key][0])
         {
            //dieRoll = 01;           // <cgs> TEST - AdvanceRetreat - Cause Movement
            //DieRoller.WhiteDie = 1; // <cgs> TEST - AdvanceRetreat - Cause Movement
            gi.DieResults[key][0] = dieRoll;
            gi.DieRollAction = GameAction.DieRollActionNone;
            gi.MovementEffectOnSherman = TableMgr.GetMovingResultSherman(gi, dieRoll);
            gi.MovementEffectOnEnemy = TableMgr.GetMovingResultEnemy(gi);
            if ("ERROR" == gi.MovementEffectOnEnemy)
            {
               Logger.Log(LogEnum.LE_ERROR, "MoveSherman(): GetMovingResultEnemy() returned ERROR");
               return false;
            }
            else
            {
               if (("A" == gi.MovementEffectOnEnemy) || ("B" == gi.MovementEffectOnEnemy))
               {
                  if (false == MoveShermanMoveEnemyUnits(gi))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "MoveSherman(): MoveShermanMoveEnemyUnits(gi) returned false for a=" + outAction.ToString());
                     return false;
                  }
               }
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "MoveSherman(): gi.MapItemMoves.Clear()");
            gi.MapItemMoves.Clear();
            bool isAnyEnemyLeft = false;
            gi.AdvancingEnemies.Clear();
            foreach (IStack stack in gi.BattleStacks)
            {
               foreach (IMapItem mi in stack.MapItems)
               {
                  if (true == mi.TerritoryCurrent.Name.Contains("Off")) // BattleRoundSequenceMovementRoll -  Sherman Movement cause enemy to move off
                     gi.ShermanAdvanceOrRetreatEnemies.Add(mi);
                  else if (true == mi.IsEnemyUnit())
                     isAnyEnemyLeft = true;
               }
            }
            foreach (IMapItem mi in gi.ShermanAdvanceOrRetreatEnemies)
               gi.BattleStacks.Remove(mi);
            //----------------------------------------------
            if (false == isAnyEnemyLeft)
            {
               bool isInjuredCrewmanReplaced;
               if(false == ReplaceInjuredCrewmen(gi, out isInjuredCrewmanReplaced, "MoveSherman()")) // Move_Sherman() - instead of AdvanceRetreat
               {
                  Logger.Log(LogEnum.LE_ERROR, "MoveSherman(): Replace_InjuredCrewmen() returned false");
                  return false;
               }
               if (true == isInjuredCrewmanReplaced)
               {
                  gi.EventDisplayed = gi.EventActive = "e062a";
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
               else
               {
                  if (false == MoveShermanAdvanceOrRetreat(gi)) // Move_Sherman() - no enemy left when Sherman moves
                  {
                     Logger.Log(LogEnum.LE_ERROR, "MoveSherman(): MoveSherman_AdvanceOrRetreat() returned false");
                     return false;
                  }
               }
            }
            else
            {
               if (false == EnemiesFacingCheck(gi, ref outAction))
               {
                  Logger.Log(LogEnum.LE_ERROR, "MoveSherman(): EnemyFacingCheck() returned false for a=" + outAction.ToString());
                  return false;
               }
            }
         }
         return true;
      }
      private bool MoveShermanMoveEnemyUnits(IGameInstance gi)
      {
         IMapItems enemyUnits = new MapItems();
         foreach (IStack stack in gi.BattleStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.IsEnemyUnit())
                  enemyUnits.Add(mi);
               if (true == mi.Name.Contains("Smoke"))
                  enemyUnits.Add(mi);
            }
         }
         foreach (IMapItem mi in enemyUnits)
         {
            ITerritory? newT = TableMgr.SetNewTerritoryShermanMove(gi.Sherman, mi, gi.MovementEffectOnEnemy);
            if (null == newT)
            {
               Logger.Log(LogEnum.LE_ERROR, "MoveSherman_MoveEnemyUnits(): SetNewTerritoryShermanMove() returned null");
               return false;
            }
            //--------------------------------------------
            if (3 == newT.Name.Length)
            {
               char sector = newT.Name[newT.Name.Length - 2];
               string tName = "B" + sector + "M";
               IStack? stack = gi.BattleStacks.Find(tName);
               if (null != stack)
               {
                  IMapItems removals = new MapItems();
                  foreach (MapItem removal in stack.MapItems)
                  {
                     if (true == removal.Name.Contains("UsControl"))
                        removals.Add(removal);
                  }
                  foreach (IMapItem removal in removals)
                     gi.BattleStacks.Remove(removal);
               }
            }
            if (false == CreateMapItemMove(gi, mi, newT))
            {
               Logger.Log(LogEnum.LE_ERROR, "MoveSherman_MoveEnemyUnits(): CreateMapItemMove() returned false");
               return false;
            }
            //--------------------------------------------
         }
         return true;
      }
      private bool MoveShermanAdvanceOrRetreat(IGameInstance gi)
      {
         //--------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveSherman_AdvanceOrRetreat(): lastReport=null");
            return false;
         }
         //---------------------------------
         if (false == EnemiesOverrunToPreviousArea(gi)) // MoveSherman_AdvanceOrRetreat() - enemy show up in previous area if they move to OffBottom
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveSherman_AdvanceOrRetreat(): EnemiesOverrun_ToPreviousArea() returned false");
            return false;
         }
         //--------------------------------
         if (null == gi.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveSherman_AdvanceOrRetreat(): gi.EnteredArea=null");
            return false;
         }
         IStack? stack = gi.MoveStacks.Find(gi.EnteredArea);
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveSherman_AdvanceOrRetreat(): stack=null");
            return false;
         }
         //--------------------------------
         Logger.Log(LogEnum.LE_SHOW_ENEMY_ON_MOVE_BOARD, "MoveSherman_AdvanceOrRetreat(): rh=" + gi.Sherman.RotationHull.ToString() + " gi.MovementEffectOnEnemy=" + gi.MovementEffectOnEnemy);
         if ((((270.0 < gi.Sherman.RotationHull) || (gi.Sherman.RotationHull < 90.0)) && ("A" == gi.MovementEffectOnEnemy)) ||
              (((90.0 < gi.Sherman.RotationHull) && (gi.Sherman.RotationHull < 270.0)) && ("B" == gi.MovementEffectOnEnemy)))
         {
            // do nothing
         }
         else
         {
            if (gi.EnteredHexes.Count < 2) // retreat into your start area means call for another start area
            {
               Logger.Log(LogEnum.LE_SHOW_ENTERED_HEX, "MoveSherman_AdvanceOrRetreat(): gi.EnteredHexes.Count=" + gi.EnteredHexes.Count.ToString() + " for hexes=" + gi.EnteredHexes.toString());
               //-----------------------------------
               if (false == ResetRound(gi, "MoveSherman_AdvanceOrRetreat() gi.EnteredHexes.Count < 2")) // MoveSherman_AdvanceOrRetreat() - Retreating
               {
                  Logger.Log(LogEnum.LE_ERROR, "MoveSherman_AdvanceOrRetreat(): ResetRound() = false");
                  return false;
               }
               //-----------------------------------
               if (false == ResetToPrepareForBattle(gi, lastReport)) // MoveSherman_AdvanceOrRetreat()
               {
                  Logger.Log(LogEnum.LE_ERROR, "MoveSherman_AdvanceOrRetreat(): ResetToPrepareForBattle() returned false");
                  return false;
               }
               //-----------------------------------
               foreach (IStack stk in gi.MoveStacks)
               {
                  foreach (IMapItem mi in stk.MapItems) // Remove the start area which causes generation of MovementStartAreaSet
                  {
                     if (true == mi.Name.Contains("StartArea"))
                     {
                        stk.MapItems.Remove(mi);
                        return true;
                     }
                  }
               }
               Logger.Log(LogEnum.LE_ERROR, "MoveSherman_AdvanceOrRetreat(): unable to find start area in MoveStacks=" + gi.MoveStacks.ToString());
               return false;
            }
            //--------------------------------------------------------
            EnteredHex enteredHex = gi.EnteredHexes[gi.EnteredHexes.Count - 2];
            ITerritory? newT = Territories.theTerritories.Find(enteredHex.TerritoryName);
            if (null == newT)
            {
               Logger.Log(LogEnum.LE_ERROR, "MoveSherman_AdvanceOrRetreat(): Territories.theTerritories.Find() returned null for tName=" + enteredHex.TerritoryName);
               return false;
            }
            //--------------------------------------------------------
            Logger.Log(LogEnum.LE_SHOW_ENTERED_HEX, "MoveSherman_AdvanceOrRetreat(): Getting last enterHex=" + enteredHex.ToString() + " from hexes=" + gi.EnteredHexes.toString());
            IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
            if (null == taskForce)
            {
               Logger.Log(LogEnum.LE_ERROR, "MoveSherman_AdvanceOrRetreat(): taskForce=null");
               return false;
            }
            taskForce.TerritoryCurrent = newT;
            IMapPoint mp = Territory.GetRandomPoint(newT, taskForce.Zoom * Utilities.theMapItemOffset); // add enemy unit to random location in area
            taskForce.Location.X = mp.X;
            taskForce.Location.Y = mp.Y;
            gi.MoveStacks.Add(taskForce);
            Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "MoveSherman_AdvanceOrRetreat(): Adding mi=" + taskForce.Name + " t=" + taskForce.TerritoryCurrent.Name + " to " + gi.MoveStacks.ToString());
            //--------------------------------------------------------
            double offset = taskForce.Zoom * Utilities.theMapItemOffset;
            IMapPoint mpTaskForce = new MapPoint(taskForce.Location.X + offset, taskForce.Location.Y + offset);
            EnteredHex newHex = new EnteredHex(gi, taskForce.TerritoryCurrent, ColorActionEnum.CAE_RETREAT, mpTaskForce);
            if (true == newHex.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "MovePathAnimate(): newHex.Ctor=true");
               return false;
            }
            gi.EnteredHexes.Add(newHex);  // MoveTaskForceToNewArea()
         }
         //--------------------------------------------------------
         foreach (IMapItem enemyUnit in gi.ShermanAdvanceOrRetreatEnemies)// add in enemy units that were on BattleBoard when battle ended
         {
            Logger.Log(LogEnum.LE_SHOW_ENEMY_ON_MOVE_BOARD, "MoveSherman_AdvanceOrRetreat(): Adding eu=" + enemyUnit.Name + " to " + gi.EnteredArea.ToString());
            enemyUnit.IsMoving = false;
            enemyUnit.IsHullDown = false;
            enemyUnit.IsSpotted = false;
            enemyUnit.IsMovingInOpen = false;
            enemyUnit.IsWoods = false;
            enemyUnit.IsBuilding = false;
            enemyUnit.IsFortification = false;
            enemyUnit.IsThrownTrack = false;
            enemyUnit.Zoom = 1.0;
            enemyUnit.TerritoryCurrent = gi.EnteredArea;
            IMapPoint mp = Territory.GetRandomPoint(gi.EnteredArea, enemyUnit.Zoom * Utilities.theMapItemOffset); // add enemy unit to random location in area
            enemyUnit.Location.X = mp.X;
            enemyUnit.Location.Y = mp.Y;
            gi.MoveStacks.Add(enemyUnit);
            Logger.Log(LogEnum.LE_SHOW_STACK_ADD, "MoveSherman_AdvanceOrRetreat(): Adding mi=" + enemyUnit.Name + " t=" + enemyUnit.TerritoryCurrent.Name + " to " + gi.MoveStacks.ToString());
            gi.MoveStacks.Add(enemyUnit);
         }
         gi.ShermanAdvanceOrRetreatEnemies.Clear();
         //--------------------------------------------------------
         if (false == ResetRound(gi, "MoveSherman_AdvanceOrRetreat()"))
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveSherman_AdvanceOrRetreat(): ResetRound() = false");
            return false;
         }
         //--------------------------------------------------------
         if (false == ResetToPrepareForBattle(gi, lastReport)) // MoveSherman_AdvanceOrRetreat()
         {
            Logger.Log(LogEnum.LE_ERROR, "MoveSherman_AdvanceOrRetreat(): ResetToPrepareForBattle() returned false");
            return false;
         }
         return true;
      }
      private bool EnemiesFacingCheck(IGameInstance gi, ref GameAction outAction)
      {
         if (("A" == gi.MovementEffectOnEnemy) || ("B" == gi.MovementEffectOnEnemy) || ("C" == gi.MovementEffectOnEnemy))
         {
            foreach (IStack stack in gi.BattleStacks)
            {
               foreach (IMapItem mi in stack.MapItems)
               {
                  if ((true == mi.IsEnemyUnit()) && (true == mi.IsVehicle) && (false == mi.IsThrownTrack)) // vehicles with thrown track cannot change facing 
                  {
                     outAction = GameAction.BattleRoundSequenceChangeFacing;
                     return true;
                  }
               }
            }
         }
         //------------------------------------------------
         gi.CrewActionPhase = CrewActionPhase.TankMainGunFire;
         if (false == ConductCrewAction(gi, ref outAction))
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): ConductCrewAction() return false");
            return false;
         }
         return true;
      }
      private bool GetShermanTargets(IGameInstance gi, ref GameAction outAction)
      {
         gi.Targets.Clear(); //  GetShermanTargets()
         gi.AreaTargets.Clear();
         List<String> tNames = new List<String>();
         double rotation = gi.Sherman.RotationHull + gi.Sherman.RotationTurret;
         if (359 < rotation)
            rotation -= 360.0;
         switch (rotation)
         {
            case 0:
               tNames.Add("B6C");
               tNames.Add("B6M");
               tNames.Add("B6L");
               break;
            case 60:
               tNames.Add("B9C");
               tNames.Add("B9M");
               tNames.Add("B9L");
               break;
            case 120:
               tNames.Add("B1C");
               tNames.Add("B1M");
               tNames.Add("B1L");
               break;
            case 180:
               tNames.Add("B2C");
               tNames.Add("B2M");
               tNames.Add("B2L");
               break;
            case 240:
               tNames.Add("B3C");
               tNames.Add("B3M");
               tNames.Add("B3L");
               break;
            case 300:
               tNames.Add("B4C");
               tNames.Add("B4M");
               tNames.Add("B4L");
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetShermanTargets(): reached default for rotation=" + rotation.ToString());
               return false;
         }
         foreach (string tName in tNames)
         {
            IStack? stack = gi.BattleStacks.Find(tName);
            if (null != stack)
            {
               foreach (IMapItem mi in stack.MapItems)
               {
                  if ((true == mi.IsEnemyUnit()) && (true == mi.IsSpotted) && (false == mi.IsKilled))
                     gi.Targets.Add(mi);
               }
            }
         }
         return true;
      }
      private bool FireMainGunAtEnemyUnits(IGameInstance gi, ref GameAction outAction, int dieRoll)
      {
         if (null == gi.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "FireMainGunAtEnemyUnitson(): Target=null");
            return false;
         }
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "FireMainGunAtEnemyUnits(): lastReport=null");
            return false;
         }
         //---------------------------------------------------------------
         if (true == gi.TargetMainGun.IsVehicle)
         {
            string facingOfTarget = TableMgr.GetShermanFireDirection(gi, gi.TargetMainGun, "Hull");  // Use HULL to determine if IsShermanFiringAtFront
            if ("ERROR" == facingOfTarget)
            {
               Logger.Log(LogEnum.LE_ERROR, "FireMainGunAtEnemyUnits(): GetEnemyFireDirection() returned error");
               return false;
            }
            if ("Front" == facingOfTarget)
            {
               Logger.Log(LogEnum.LE_SHOW_FIRE_DIRECTION, "GetShermanFireDirection(): SETTTING gi.IsShermanFiringAtFront=TRUE");
               gi.IsShermanFiringAtFront = true; // Sherman is firing at front of target
            }
            else
            {
               Logger.Log(LogEnum.LE_SHOW_FIRE_DIRECTION, "GetShermanFireDirection(): SETTTING gi.IsShermanFiringAtFront=FALSE");
               gi.IsShermanFiringAtFront = false; // Sherman is firing at front of target
            }
         }
         //---------------------------------------------------------------
         double toHitNumber = TableMgr.GetShermanToHitBaseNumber(gi, gi.TargetMainGun);  // determine the To Hit number
         if (TableMgr.FN_ERROR == toHitNumber)
         {
            Logger.Log(LogEnum.LE_ERROR, "FireMainGunAtEnemyUnits(): GetShermanToHitBaseNumber() returned error");
            return false;
         }
         double modifier = TableMgr.GetShermanToHitModifier(gi, gi.TargetMainGun);  // determine the To Hit number
         if (TableMgr.FN_ERROR == modifier)
         {
            Logger.Log(LogEnum.LE_ERROR, "FireMainGunAtEnemyUnits(): GetShermanToHitModifier() returned error");
            return false;
         }
         double combo = toHitNumber - modifier;
         //---------------------------------------------------------------
         string gunLoadType = gi.GetGunLoadType();
         if (false == gi.FireAndReloadGun()) // FireMainGunAtEnemyUnits - gi.ShermanTypeOfFire get reset in FireAndReloadGun
         {
            Logger.Log(LogEnum.LE_ERROR, "FireMainGunAtEnemyUnits(): FireAndReloadGun() returned false");
            return false;
         }
         //---------------------------------------------------------------
         bool isCriticalHit = false;
         if (dieRoll < 4)
            isCriticalHit = true;
         if (97 < dieRoll)
         {
            gi.IsMalfunctionedMainGun = true;
            Logger.Log(LogEnum.LE_SHOW_MAIN_GUN_BREAK, "FireMainGunAtEnemyUnits(): Main Gun Malfunctioned dr=" + dieRoll.ToString());
         }
         //---------------------------------------------------------------
         if (combo < dieRoll) // Miss Target - move to next Crew Action
         {
            Logger.Log(LogEnum.LE_SHOW_TO_HIT_ATTACK, "FireMainGunAtEnemyUnits(): Miss Target - Main Gun NO ROF Follow Up for (base=" + toHitNumber.ToString() + ") + (mod=" + modifier.ToString() + ") = (total=" + combo.ToString() + ") dr=" + dieRoll.ToString());
            if (0 == gi.ShermanHits.Count)
            {
               gi.CrewActionPhase = CrewActionPhase.TankMgFire;
               if (false == ConductCrewAction(gi, ref outAction))
               {
                  Logger.Log(LogEnum.LE_ERROR, "FireMainGunAtEnemyUnits(): ConductCrewAction() returned error");
                  return false;
               }
            }
            else
            {
               if ((true == gi.TargetMainGun.Name.Contains("LW")) || (true == gi.TargetMainGun.Name.Contains("MG")) || (true == gi.TargetMainGun.Name.Contains("Pak")) || (true == gi.TargetMainGun.Name.Contains("ATG")))
                  gi.EventDisplayed = gi.EventActive = "e053d"; // resolve attack
               else
                  gi.EventDisplayed = gi.EventActive = "e053e"; // resolve attack
               gi.DieRollAction = GameAction.BattleRoundSequenceShermanToKillRoll;
            }
            return true;
         }
         //---------------------------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_TO_HIT_ATTACK, "FireMainGunAtEnemyUnits(): Hit Target (base=" + toHitNumber.ToString() + ") +  (mod=" + modifier.ToString() + ") = (total=" + combo.ToString() + ") dr=" + dieRoll.ToString());
         ShermanAttack hit = new ShermanAttack(gi.ShermanTypeOfFire, gunLoadType, isCriticalHit);
         gi.ShermanHits.Add(hit);
         switch (gunLoadType) // mark off hit
         {
            case "He":
               gi.TargetMainGun.IsHeHit = true;
               break;
            case "Ap":
               gi.TargetMainGun.IsApHit = true;
               break;
            case "Hvap":
               gi.TargetMainGun.IsApHit = true;
               break;
            case "Hbci": // Lay two smoke in the target's zone
               gi.NumSmokeAttacksThisRound++;
               for (int i = 0; i < 2; i++)
               {
                  string miName1 = "SmokeWhite" + Utilities.MapItemNum;
                  Utilities.MapItemNum++;
                  IMapItem smoke1 = new MapItem(miName1, Utilities.ZOOM + 0.75, "c108Smoke1", gi.TargetMainGun.TerritoryCurrent);
                  IMapPoint mp1 = Territory.GetRandomPoint(gi.TargetMainGun.TerritoryCurrent, gi.TargetMainGun.Zoom * Utilities.theMapItemOffset);
                  smoke1.Location = mp1;
                  gi.BattleStacks.Add(smoke1);
               }
               break;
            case "Wp":   // Lay smoke in the target's zone
               gi.NumSmokeAttacksThisRound++;
               string miName = "SmokeWhite" + Utilities.MapItemNum;
               Utilities.MapItemNum++;
               IMapItem smoke = new MapItem(miName, Utilities.ZOOM + 0.75, "c108Smoke1", gi.TargetMainGun.TerritoryCurrent);
               IMapPoint mp = Territory.GetRandomPoint(gi.TargetMainGun.TerritoryCurrent, gi.TargetMainGun.Zoom * Utilities.theMapItemOffset);
               smoke.Location = mp;
               gi.BattleStacks.Add(smoke);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GameStateBattleRoundSequence.PerformAction(): GetShermanToHitBaseNumber() reached default gunload=" + gunLoadType + " for a=" + outAction.ToString());
               return false;
         }
         //---------------------------------------------------------------
         gi.ShermanTypeOfFire = "";                                 // FireMainGunAtEnemyUnits()
         int rateOfFireNumber = TableMgr.GetShermanRateOfFire(gi);  // Hit target - determine if reach rate of fire
         if (TableMgr.FN_ERROR == rateOfFireNumber)
         {
            Logger.Log(LogEnum.LE_ERROR, "FireMainGunAtEnemyUnits(): GetShermanRateOfFire() returned error");
            return false;
         }
         //---------------------------------------------------------------
         if ((dieRoll <= rateOfFireNumber) && (false == gi.IsMalfunctionedMainGun) && ("None" != gi.GetGunLoadType()))
         {
            gi.DieResults["e053c"][0] = gi.DieResults["e053b"][0];
            gi.DieResults["e053b"][0] = Utilities.NO_RESULT;
            gi.EventDisplayed = gi.EventActive = "e053c";  // achieve rate of fire - show event
            gi.DieRollAction = GameAction.DieRollActionNone;
            Logger.Log(LogEnum.LE_SHOW_TO_HIT_ATTACK, "FireMainGunAtEnemyUnits(): Main Gun Maintains ROF");
         }
         else
         {
            Logger.Log(LogEnum.LE_SHOW_TO_HIT_ATTACK, "FireMainGunAtEnemyUnits(): Main Gun NO ROF Follow Up rof#=" + rateOfFireNumber.ToString() + " < dr=" + dieRoll.ToString());
            if (0 == gi.ShermanHits.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "FireMainGunAtEnemyUnits(): gi.ShermanHits.Count=0");
               return false;
            }
            ShermanAttack hitToResolve = gi.ShermanHits[0];
            if (("Wp" != hitToResolve.myAmmoType) && ("Hbci" != hitToResolve.myAmmoType))
            {
               if ((true == gi.TargetMainGun.Name.Contains("LW")) || (true == gi.TargetMainGun.Name.Contains("MG")) || (true == gi.TargetMainGun.Name.Contains("Pak")) || (true == gi.TargetMainGun.Name.Contains("ATG")))
                  gi.EventDisplayed = gi.EventActive = "e053d"; // resolve attack
               else
                  gi.EventDisplayed = gi.EventActive = "e053e"; // resolve attack
               gi.DieRollAction = GameAction.BattleRoundSequenceShermanToKillRoll;
            }
            else
            {
               gi.EventDisplayed = gi.EventActive = "e053f";
               gi.DieRollAction = GameAction.DieRollActionNone;
               gi.DieRollAction = GameAction.BattleRoundSequenceShermanToKillRoll;
            }
         }
         return true;
      }
      private bool ResolveToKillEnemyUnit(IGameInstance gi, ref GameAction outAction, int dieRoll)
      {
         string key = gi.EventActive;
         Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnit(): Entering dr=" + dieRoll.ToString());
         if (null == gi.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): gi.TargetMainGun=null");
            return false;
         }
         if (0 == gi.ShermanHits.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): gi.ShermanHits.Count=0");
            return false;
         }
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): lastReport=null");
            return false;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //-----------------------------------------------------------------------------------
         ShermanAttack hit = gi.ShermanHits[0];
         if (("Hbci" == hit.myAmmoType) || ("Wp" == hit.myAmmoType))
         {
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnit(): first time thru with SSSSSSSSSSSSSSSSSSSSSSSSSSSSS");
            if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
            {
               Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
               return false;
            }
         }
         //-----------------------------------------------------------------------------------
         else if (Utilities.NO_RESULT == gi.DieResults[key][0])
         {
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnit(): 1st time thru ----->hit.myAmmoType=" + hit.myAmmoType + "<--------- d1=" + dieRoll.ToString() + " v?=" + gi.TargetMainGun.IsVehicle + " hulldown?=" + gi.TargetMainGun.IsHullDown);
            gi.DieResults[key][0] = dieRoll;
            if (true == gi.TargetMainGun.IsVehicle) // first die is hit location - even no chance hits could hit could cause thrown track
            {
               gi.DieRollAction = GameAction.BattleRoundSequenceShermanToKillRoll;
               if (true == gi.TargetMainGun.IsHullDown)
               {
                  if (5 < gi.DieResults[key][0])
                     hit.myHitLocation = "MISS";
                  else
                     hit.myHitLocation = "Turret";
               }
               else
               {
                  if ((9 < gi.DieResults[key][0]) || (true == gi.IsShermanDeliberateImmobilization))
                  {
                     hit.myHitLocation = "Thrown Track";
                     if (true == gi.TargetMainGun.Name.Contains("Truck"))
                        hit.myHitLocation = "Hull";
                     else
                        gi.TargetMainGun.IsThrownTrack = true;
                     gi.TargetMainGun.IsMoving = false;
                     gi.TargetMainGun.IsApHit = false;
                  }
                  else if (4 < gi.DieResults[key][0])
                  {
                     hit.myHitLocation = "Hull";
                  }
                  else
                  {
                     hit.myHitLocation = "Turret";
                  }
               }
            }
            else
            {
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnit(): 1st time through to kill infantry - call  ResolveToKillEnemyUnitKill() - I I I I I I I I I I I I I I I I I I I I I I I I I I I I ");
               gi.DieRollAction = GameAction.DieRollActionNone;
               if (false == ResolveToKillEnemyUnitKill(gi, ref outAction, gi.DieResults[key][0]))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitKill() returned false");
                  return false;
               }
            }
         }
         //-----------------------------------------------------------------------------------
         else if ((Utilities.NO_RESULT == gi.DieResults[key][1]) && (true == gi.TargetMainGun.IsVehicle)) // 2nd time through for vehicles hits this branch
         {
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnit(): 2nd time through to kill ------V V V V V V V V V V V V V V V V V V V V V V V V V V V V V V V V V V V V ");
            //dieRoll = 96; // <cgs> TEST - do not want to kill targets
            gi.DieResults[key][1] = dieRoll;
            gi.DieRollAction = GameAction.DieRollActionNone;
            if (false == ResolveToKillEnemyUnitKill(gi, ref outAction, gi.DieResults[key][1]))
            {
               Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitKill() returned false");
               return false;
            }
         }
         //-----------------------------------------------------------------------------------
         else // third time thru for vehicles hits this branch
         {
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnit(): Finalize Kill Cleanup --------C C C C C C C C C C C C C C C C C C C C C C C C");
            IMapItems shermanKills = new MapItems(); // remove any killed units
            foreach (IStack stack in gi.BattleStacks)
            {
               foreach (IMapItem mapItem in stack.MapItems)
               {
                  if (true == mapItem.IsKilled)
                     shermanKills.Add(mapItem);
               }
            }
            foreach (IMapItem mi in shermanKills)
               gi.BattleStacks.Remove(mi);
            //--------------------------------------
            if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
            {
               Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
               return false;
            }
         }
         return true;
      }
      private bool ResolveToKillEnemyUnitKill(IGameInstance gi, ref GameAction outAction, int dieRoll)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitKill(): lastReport=null");
            return false;
         }
         //-------------------------------------------------
         if (null == gi.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitKill(): Target=null");
            return false;
         }
         //-------------------------------------------------
         if (0 == gi.ShermanHits.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitKill(): gi.ShermanHits.Count=0");
            return false;
         }
         ShermanAttack hit = gi.ShermanHits[0];
         //-------------------------------------------------
         int toKillNum = 0;
         if ((true == gi.TargetMainGun.Name.Contains("LW")) || (true == gi.TargetMainGun.Name.Contains("MG")) || (true == gi.TargetMainGun.Name.Contains("Pak")) || (true == gi.TargetMainGun.Name.Contains("ATG")))
         {
            toKillNum = TableMgr.GetShermanToKillInfantryBaseNumber(gi, gi.TargetMainGun, hit);
            int modifier = TableMgr.GetShermanToKillInfantryModifier(gi, gi.TargetMainGun, hit);
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): vs Infantry target tokill=" + toKillNum.ToString() + " mod=" + modifier.ToString() + " IIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII");
            if (TableMgr.FN_ERROR == modifier)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitKill(): GetShermanToKillInfantryModifier() returned error");
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): attacking infantry=" + gi.TargetMainGun.Name + " toHit=" + toKillNum.ToString() + " modifier=" + modifier.ToString());
            if (TableMgr.KIA == modifier)  // automatic KILL
            {
               gi.ScoreYourVictoryPoint(lastReport, gi.TargetMainGun);
               gi.TargetMainGun.IsHeHit = false;
               gi.TargetMainGun.IsApHit = false;
               gi.TargetMainGun.IsKilled = true;
               gi.TargetMainGun.IsMoving = false;
               gi.TargetMainGun.SetBloodSpots();
               return true;
            }
            if (TableMgr.NO_CHANCE == modifier)
            {
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): vs Infantry target -- No Chance - NC NC NC NC NC NC NC NC NC NC");
               if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                  return false;
               }
               return true;
            }
            toKillNum += modifier;
         }
         else // attack vehicle
         {
            TankCard card = new TankCard(lastReport.TankCardNum);
            if ("75" == card.myMainGun)
            {
               if ("He" == hit.myAmmoType)
               {
                  toKillNum = TableMgr.GetShermanToKill75HeVehicleBaseNumber(gi, gi.TargetMainGun, hit);
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): attacking 1-vehicle=" + gi.TargetMainGun.Name + " toHit=" + toKillNum.ToString());
                  if (TableMgr.NO_CHANCE == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): 75 HE No Chance - NC NC NC NC NC NC NC NC NC NC");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
                  else if (TableMgr.THROWN_TRACK == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): 75 HE No Chance - TT TT TT TT TT TT TT TT TT TT TT TT TT TT ");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
               }
               else if ("Ap" == hit.myAmmoType)
               {
                  toKillNum = TableMgr.GetShermanToKill75ApVehicleBaseNumber(gi, gi.TargetMainGun, hit);
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): attacking 2-vehicle=" + gi.TargetMainGun.Name + " toHit=" + toKillNum.ToString());
                  if (TableMgr.KIA == toKillNum) // automatic KILL
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): AUTO KIlled KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK");
                     gi.ScoreYourVictoryPoint(lastReport, gi.TargetMainGun);
                     gi.TargetMainGun.IsHeHit = false;
                     gi.TargetMainGun.IsApHit = false;
                     gi.TargetMainGun.IsKilled = true;
                     gi.TargetMainGun.IsMoving = false;
                     gi.TargetMainGun.SetBloodSpots();
                     return true;
                  }
                  else if (TableMgr.NO_CHANCE == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): 75 AP No Chance - NC NC NC NC NC NC NC NC NC NC");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
                  else if (TableMgr.THROWN_TRACK == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): 75 AP No Chance - TT TT TT TT TT TT TT TT TT TT TT TT TT TT ");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitKill(): reached default unknown gunload=" + hit.myAmmoType);
                  return false;
               }
            }
            else if ("76L" == card.myMainGun)
            {
               if ("He" == hit.myAmmoType)
               {
                  toKillNum = TableMgr.GetShermanToKill76HeVehicleBaseNumber(gi, gi.TargetMainGun, hit);
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): attacking 3-vehicle=" + gi.TargetMainGun.Name + " toHit=" + toKillNum.ToString());
                  if (TableMgr.NO_CHANCE == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): HE No Chance - NC NC NC NC NC NC NC NC NC NC");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
                  else if (TableMgr.THROWN_TRACK == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): 75 HE No Chance - TT TT TT TT TT TT TT TT TT TT TT TT TT TT ");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
               }
               else if ("Ap" == hit.myAmmoType)
               {
                  toKillNum = TableMgr.GetShermanToKill76ApVehicleBaseNumber(gi, gi.TargetMainGun, hit);
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): attacking 4-vehicle=" + gi.TargetMainGun.Name + " toHit=" + toKillNum.ToString());
                  if (TableMgr.KIA == toKillNum) // automatic KILL
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): AUTO KIlled KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK");
                     gi.ScoreYourVictoryPoint(lastReport, gi.TargetMainGun);
                     gi.TargetMainGun.IsHeHit = false;
                     gi.TargetMainGun.IsApHit = false;
                     gi.TargetMainGun.IsKilled = true;
                     gi.TargetMainGun.IsMoving = false;
                     gi.TargetMainGun.SetBloodSpots();
                     return true;
                  }
                  else if (TableMgr.NO_CHANCE == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): 76L AP No Chance - NC NC NC NC NC NC NC NC NC NC");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
                  else if (TableMgr.THROWN_TRACK == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): 75 AP No Chance - TT TT TT TT TT TT TT TT TT TT TT TT TT TT ");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
               }
               else if ("Hvap" == hit.myAmmoType)
               {
                  toKillNum = TableMgr.GetShermanToKill76HvapVehicleBaseNumber(gi, gi.TargetMainGun, hit);
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): attacking 5-vehicle=" + gi.TargetMainGun.Name + " toHit=" + toKillNum.ToString());
                  if (TableMgr.KIA == toKillNum) // automatic KILL
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): AUTO KIlled KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK");
                     gi.ScoreYourVictoryPoint(lastReport, gi.TargetMainGun);
                     gi.TargetMainGun.IsHeHit = false;
                     gi.TargetMainGun.IsApHit = false;
                     gi.TargetMainGun.IsKilled = true;
                     gi.TargetMainGun.IsMoving = false;
                     gi.TargetMainGun.SetBloodSpots();
                     return true;
                  }
                  else if (TableMgr.NO_CHANCE == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill():76L  Hvap No Chance - NC NC NC NC NC NC NC NC NC NC");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
                  else if (TableMgr.THROWN_TRACK == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): 75 AP No Chance - TT TT TT TT TT TT TT TT TT TT TT TT TT TT ");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitKill(): reached default unknown gunload=" + hit.myAmmoType);
                  return false;
               }
            }
            else if ("76LL" == card.myMainGun)
            {
               if ("He" == hit.myAmmoType)
               {
                  toKillNum = TableMgr.GetShermanToKill76HeVehicleBaseNumber(gi, gi.TargetMainGun, hit);
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): attacking 6-vehicle=" + gi.TargetMainGun.Name + " toHit=" + toKillNum.ToString());
                  if (TableMgr.NO_CHANCE == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill():76L  Hvap No Chance - NC NC NC NC NC NC NC NC NC NC");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
                  else if (TableMgr.THROWN_TRACK == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): 75 AP No Chance - TT TT TT TT TT TT TT TT TT TT TT TT TT TT ");
                     if (false == ResolveToKillEnemyUnitCleanup(gi, ref outAction))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnit(): ResolveToKillEnemyUnitCleanup() returned false");
                        return false;
                     }
                     return true;
                  }
               }
               else if ("Ap" == hit.myAmmoType)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitKill(): not implemented card.myMainGun=" + card.myMainGun);
                  return false;
               }
               else if ("Hvap" == hit.myAmmoType)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitKill(): not implemented card.myMainGun=" + card.myMainGun);
                  return false;
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitKill(): reached default unknown gunload=" + hit.myAmmoType);
                  return false;
               }
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitKill(): reached default card.myMainGun=" + card.myMainGun);
               return false;
            }
         }
         //---------------------------------------------------------------
         if (TableMgr.FN_ERROR == toKillNum)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitKill(): toKillNum=FN_ERROR");
            return false;
         }
         //---------------------------------------------------------------
         if (toKillNum < dieRoll) // No Effect
            return true;
         Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitKill(): KIlled KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK");
         switch (hit.myAmmoType) // kill the unit
         {
            case "He":
            case "Ap":
            case "Hvap":
               gi.ScoreYourVictoryPoint(lastReport, gi.TargetMainGun);
               gi.TargetMainGun.IsHeHit = false;
               gi.TargetMainGun.IsApHit = false;
               gi.TargetMainGun.IsKilled = true;
               gi.TargetMainGun.IsMoving = false;
               gi.TargetMainGun.SetBloodSpots();
               break;
            default:
               break;
         }
         return true;
      }
      private bool ResolveToKillEnemyUnitCleanup(IGameInstance gi, ref GameAction outAction)
      {
         if (null == gi.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitCleanup(): gi.TargetMainGun=null");
            return false;
         }
         //-------------------------------
         string key = gi.EventActive;
         gi.DieResults[key][0] = Utilities.NO_RESULT;
         gi.DieResults[key][1] = Utilities.NO_RESULT;
         gi.TargetMainGun.IsHeHit = false; // turn off overlay image
         gi.TargetMainGun.IsApHit = false;
         //-------------------------------
         gi.ShermanHits.RemoveAt(0);
         Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitCleanup(): CCCCCCCCCCCCCCCCCCCCCCCCC ShermanHit.Count=" + gi.ShermanHits.Count);
         if ((0 < gi.ShermanHits.Count) && (false == gi.TargetMainGun.IsKilled))
         {
            bool isInfantryTarget = ((true == gi.TargetMainGun.Name.Contains("LW")) || (true == gi.TargetMainGun.Name.Contains("MG")) || (true == gi.TargetMainGun.Name.Contains("Pak")) || (true == gi.TargetMainGun.Name.Contains("ATG")));
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitCleanup(): Setup to roll kill against infantry?=" + isInfantryTarget.ToString());
            if (true == isInfantryTarget)
               gi.EventDisplayed = gi.EventActive = "e053d"; // main gun against infantry
            else
               gi.EventDisplayed = gi.EventActive = "e053e"; // main gun against vehicles
            gi.DieRollAction = GameAction.BattleRoundSequenceShermanToKillRoll;
         }
         else
         {
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_ATTACK, "ResolveToKillEnemyUnitCleanup(): ConductCrewAction(TankMgFire)");
            gi.ShermanHits.Clear();
            gi.CrewActionPhase = CrewActionPhase.TankMgFire;
            if (false == ConductCrewAction(gi, ref outAction))
            {
               Logger.Log(LogEnum.LE_ERROR, "ResolveToKillEnemyUnitCleanup(): ConductCrewAction() returned false");
               return false;
            }
         }
         return true;
      }
      private bool GetShermanMgTargets(IGameInstance gi, string mgType)
      {
         gi.Targets.Clear(); // GetShermanMgTargets()
         gi.TargetMg = null;
         gi.AreaTargets.Clear();
         List<String> tNames = new List<String>();
         if (("Aa" == mgType) || (("Sub" == mgType)))
         {
            tNames.Add("B6C");
            tNames.Add("B6M");
            tNames.Add("B6L");
            tNames.Add("B9C");
            tNames.Add("B9M");
            tNames.Add("B9L");
            tNames.Add("B1C");
            tNames.Add("B1M");
            tNames.Add("B1L");
            tNames.Add("B2C");
            tNames.Add("B2M");
            tNames.Add("B2L");
            tNames.Add("B3C");
            tNames.Add("B3M");
            tNames.Add("B3L");
            tNames.Add("B4C");
            tNames.Add("B4M");
            tNames.Add("B4L");
         }
         else if (("Coaxial" == mgType) || ("Bow" == mgType))
         {
            double rotation = gi.Sherman.RotationHull;
            if ("Coaxial" == mgType)
               rotation += gi.Sherman.RotationTurret;
            if (359 < rotation)
               rotation -= 360.0;
            switch (rotation)
            {
               case 0:
                  tNames.Add("B6C");
                  tNames.Add("B6M");
                  tNames.Add("B6L");
                  break;
               case 60:
                  tNames.Add("B9C");
                  tNames.Add("B9M");
                  tNames.Add("B9L");
                  break;
               case 120:
                  tNames.Add("B1C");
                  tNames.Add("B1M");
                  tNames.Add("B1L");
                  break;
               case 180:
                  tNames.Add("B2C");
                  tNames.Add("B2M");
                  tNames.Add("B2L");
                  break;
               case 240:
                  tNames.Add("B3C");
                  tNames.Add("B3M");
                  tNames.Add("B3L");
                  break;
               case 300:
                  tNames.Add("B4C");
                  tNames.Add("B4M");
                  tNames.Add("B4L");
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetShermanTargets(): reached default for rotation=" + rotation.ToString());
                  return false;
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanTargets(): reached default for mgType=" + mgType);
            return false;
         }
         //---------------------------------------------
         foreach (string tName in tNames)
         {
            ITerritory? t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "GetShermanTargets(): t=null for tName=" + tName);
               return false;
            }
            gi.AreaTargets.Add(t);
            IStack? stack = gi.BattleStacks.Find(t);
            if (null != stack)
            {
               foreach (IMapItem mi in stack.MapItems)
               {
                  bool isVehicleTarget = ((true == mi.IsVehicle) && (false == mi.Name.Contains("TRUCK"))); // allowed to show MG at trucks
                  if ((true == mi.IsEnemyUnit()) && (true == mi.IsSpotted) && (false == mi.IsKilled) && (false == isVehicleTarget))
                     gi.Targets.Add(mi);
               }
            }
         }
         return true;
      }
      private bool RepairMainGunAttempt(IGameInstance gi, ref GameAction outAction)
      {
         string key = gi.EventActive;
         int combo = gi.DieResults[key][0];
         //-------------------------------------------------------
         ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
         if (null == loader)
         {
            Logger.Log(LogEnum.LE_ERROR, "RepairMainGunAttempt(): loader=null");
            return false;
         }
         combo -= loader.Rating;
         //-------------------------------------------------------
         bool isGunnerHelpingRepair = false;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if ("Gunner_RepairMainGun" == crewAction.Name)
               isGunnerHelpingRepair = true;
         }
         if (true == isGunnerHelpingRepair)
         {
            ICrewMember? gunner = gi.GetCrewMemberByRole("Gunner");
            if (null == gunner)
            {
               Logger.Log(LogEnum.LE_ERROR, "RepairMainGunAttempt(): gunner=null");
               return false;
            }
            combo -= gunner.Rating;
         }
         //-------------------------------------------------------
         if (combo < 21)
         {
            gi.IsMalfunctionedMainGun = false;
            Logger.Log(LogEnum.LE_SHOW_MAIN_GUN_BREAK, "RepairMainGunAttempt(): Main Gun Repaired with combo=" + combo.ToString());
         }
         else if ((90 < combo) || (97 < gi.DieResults[key][0])) // gun automatically breaks on unmodified die roll greater than 97
         {
            gi.IsMalfunctionedMainGun = false;
            gi.IsBrokenMainGun = true;
            Logger.Log(LogEnum.LE_SHOW_MAIN_GUN_BREAK, "RepairMainGunAttempt(): Main Gun Broken with combo=" + combo.ToString());
         }
         gi.CrewActionPhase = CrewActionPhase.RepairGun;
         if (false == ConductCrewAction(gi, ref outAction))
         {
            Logger.Log(LogEnum.LE_ERROR, "RepairMainGunAttempt(): ConductCrewAction() returned false");
            return false;
         }
         gi.DieResults[key][0] = Utilities.NO_RESULT;
         return true;
      }
      private bool RepairAntiAircraftMgAttempt(IGameInstance gi, ref GameAction outAction)
      {
         string key = gi.EventActive;
         int combo = gi.DieResults[key][0];
         //-------------------------------------------------------
         bool isCommanderRepairing = false;
         bool isLoaderRepairing = false;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if ("Commander_RepairAaMg" == crewAction.Name)
               isCommanderRepairing = true;
            if ("Loader_RepairAaMg" == crewAction.Name)
               isLoaderRepairing = true;
         }
         //-------------------------------------------------------
         if (true == isLoaderRepairing)
         {
            ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
            if (null == loader)
            {
               Logger.Log(LogEnum.LE_ERROR, "RepairAntiAircraftMgAttempt(): loader=null");
               return false;
            }
            combo -= loader.Rating;
         }
         //-------------------------------------------------------
         if (true == isCommanderRepairing)
         {
            ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
            if (null == commander)
            {
               Logger.Log(LogEnum.LE_ERROR, "RepairAntiAircraftMgAttempt(): commander=null");
               return false;
            }
            combo -= commander.Rating;
         }
         //-------------------------------------------------------
         if (combo < 21)
         {
            gi.IsMalfunctionedMgAntiAircraft = false;
         }
         else if ((90 < combo) || (97 < gi.DieResults[key][0])) // gun automatically breaks on unmodified die roll greater than 97
         {
            gi.IsMalfunctionedMgAntiAircraft = false;
            gi.IsBrokenMgAntiAircraft = true;
         }
         //-------------------------------------------------------
         gi.CrewActionPhase = CrewActionPhase.RepairGun;
         if (false == ConductCrewAction(gi, ref outAction))
         {
            Logger.Log(LogEnum.LE_ERROR, "RepairAntiAircraftMgAttempt(): ConductCrewAction() returned false");
            return false;
         }
         gi.DieResults[key][0] = Utilities.NO_RESULT;
         return true;
      }
      private bool RepairBowMgAttempt(IGameInstance gi, ref GameAction outAction)
      {
         string key = gi.EventActive;
         int combo = gi.DieResults[key][0];
         //-------------------------------------------------------
         ICrewMember? assistant = gi.GetCrewMemberByRole("Assistant");
         if (null == assistant)
         {
            Logger.Log(LogEnum.LE_ERROR, "RepairBowMgAttempt(): assistant=null");
            return false;
         }
         combo -= assistant.Rating;
         //-------------------------------------------------------
         if (combo < 21)
         {
            gi.IsMalfunctionedMgBow = false;
         }
         else if ((90 < combo) || (97 < gi.DieResults[key][0])) // gun automatically breaks on unmodified die roll greater than 97
         {
            gi.IsMalfunctionedMgBow = false;
            gi.IsBrokenMgBow = true;
         }
         //-------------------------------------------------------
         gi.CrewActionPhase = CrewActionPhase.RepairGun;
         if (false == ConductCrewAction(gi, ref outAction))
         {
            Logger.Log(LogEnum.LE_ERROR, "RepairBowMgAttempt(): ConductCrewAction() returned false");
            return false;
         }
         gi.DieResults[key][0] = Utilities.NO_RESULT;
         return true;
      }
      private bool RepairCoaxialMgAttempt(IGameInstance gi, ref GameAction outAction)
      {
         string key = gi.EventActive;
         int combo = gi.DieResults[key][0];
         //-------------------------------------------------------
         ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
         if (null == loader)
         {
            Logger.Log(LogEnum.LE_ERROR, "RepairBowCoaxialAttempt(): loader=null");
            return false;
         }
         combo -= loader.Rating;
         //-------------------------------------------------------
         if (combo < 21)
         {
            gi.IsMalfunctionedMgCoaxial = false;
         }
         else if ((90 < combo) || (97 < gi.DieResults[key][0])) // gun automatically breaks on unmodified die roll greater than 97
         {
            gi.IsMalfunctionedMgCoaxial = false;
            gi.IsBrokenMgCoaxial = true;
         }
         //-------------------------------------------------------
         gi.CrewActionPhase = CrewActionPhase.RepairGun;
         if (false == ConductCrewAction(gi, ref outAction))
         {
            Logger.Log(LogEnum.LE_ERROR, "RepairBowCoaxialAttempt(): ConductCrewAction() returned false");
            return false;
         }
         gi.DieResults[key][0] = Utilities.NO_RESULT;
         return true;
      }
      private bool FireMortarIntoTurretFront(IGameInstance gi, ref GameAction outAction)
      {
         double rotation = gi.Sherman.RotationHull;
         rotation += gi.Sherman.RotationTurret;
         if (359 < rotation)
            rotation -= 360.0;
         string? tName = null;
         switch (rotation)
         {
            case 0:
               tName = "B6C";
               break;
            case 60:
               tName = "B9C";
               break;
            case 120:
               tName = "B1C";
               break;
            case 180:
               tName = "B2C";
               break;
            case 240:
               tName = "B3C";
               break;
            case 300:
               tName = "B4C";
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "FireMortarIntoTurretFront(): reached default for rotation=" + rotation.ToString());
               return false;
         }
         if (null == tName)
         {
            Logger.Log(LogEnum.LE_ERROR, "FireMortarIntoTurretFront(): tName=null");
            return false;
         }
         ITerritory? t = Territories.theTerritories.Find(tName);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "FireMortarIntoTurretFront(): t=null for tName=" + tName);
            return false;
         }
         //--------------------------------------------------
         string miName1 = "SmokeWhite" + Utilities.MapItemNum;
         Utilities.MapItemNum++;
         IMapItem smoke1 = new MapItem(miName1, Utilities.ZOOM, "c108Smoke1", t);
         IMapPoint mp1 = Territory.GetRandomPoint(t, Utilities.theMapItemOffset);
         smoke1.Location = mp1;
         gi.BattleStacks.Add(smoke1);
         //--------------------------------------------------
         gi.CrewActionPhase = CrewActionPhase.ThrowGrenades;
         if (false == ConductCrewAction(gi, ref outAction))
         {
            Logger.Log(LogEnum.LE_ERROR, "FireMortarIntoTurretFront(): ConductCrewAction() returned false");
            return false;
         }
         return true;
      }
      private bool UpdateReadyRackMinus(IGameInstance gi, string ammoType)
      {
         int readyRackLoadCount = gi.GetReadyRackReload(ammoType);
         if (0 == readyRackLoadCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): Invalid state - readyRackLoadCount=0 for " + ammoType);
            return false;
         }
         readyRackLoadCount--;
         Logger.Log(LogEnum.LE_SHOW_GUN_RELOAD, "UpdateReadyRack(): Setting readyRackLoadCount=" + readyRackLoadCount.ToString());
         if (false == gi.SetReadyRackReload(ammoType, readyRackLoadCount))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): SetReadyRackReload() returned false");
            return false;
         }
         return true;
      }
      private bool UpdateReadyRackPlus(IGameInstance gi, string ammoType, int ammoAvailable)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): lastReport=null");
            return false;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //---------------------------------------------------
         int readyRackLoadCount = gi.GetReadyRackReload(ammoType);
         ammoAvailable -= readyRackLoadCount;
         string gunLoadType = gi.GetGunLoadType();
         if (ammoType == gunLoadType) // subtract one if a round is int the gun tube
            ammoAvailable--;
         if ((ammoAvailable < 1) || (card.myMaxReadyRackCount <= gi.GetReadyRackTotalLoad()))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): Invalid state - maxReload reached for " + ammoType + " ammo=" + ammoAvailable.ToString());
            return false;
         }
         else
         {
            readyRackLoadCount++;
            Logger.Log(LogEnum.LE_SHOW_GUN_RELOAD, "UpdateReadyRack(): Setting readyRackLoadCount=" + readyRackLoadCount.ToString() + "--> ammoCount=" + ammoAvailable.ToString());
            if (false == gi.SetReadyRackReload(ammoType, readyRackLoadCount))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): SetReadyRackReload() returned false");
               return false;
            }
         }
         return true;
      }
      private bool NextStepAfterRandomEvent(IGameInstance gi, ref GameAction outAction)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextStepAfterRandomEvent(): lastReport=null");
            return false;
         }
         //--------------------------------------------------
         if (BattlePhase.AmbushRandomEvent == gi.BattlePhase)
         {
            if (false == SpottingPhaseBegin(gi, ref outAction, "NextStepAfterRandomEvent()"))
            {
               Logger.Log(LogEnum.LE_ERROR, "NextStepAfterRandomEvent(): SpottingPhaseBegin() returned false");
               return false;
            }
         }
         else
         {
            if (false == ResetRound(gi, "NextStepAfterRandomEvent()"))    // NextStepAfterRandomEvent
            {
               Logger.Log(LogEnum.LE_ERROR, "NextStepAfterRandomEvent(): ResetRound() returned false");
               return false;
            }
            else if (("None" == gi.GetGunLoadType()) && (BattlePhase.BackToSpotting == gi.BattlePhase))
            {
               int totalAmmoFriendlyArtillery = lastReport.MainGunHE + lastReport.MainGunAP + lastReport.MainGunWP + lastReport.MainGunHBCI + lastReport.MainGunHVAP;
               if (0 < totalAmmoFriendlyArtillery)
               {
                  outAction = GameAction.BattleRoundSequenceLoadMainGun;
                  gi.EventDisplayed = gi.EventActive = "e050a";
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
               else
               {
                  gi.EventDisplayed = gi.EventActive = "e050b";
                  gi.DieRollAction = GameAction.DieRollActionNone;
               }
            }
         }
         return true;
      }
      private bool ResetRound(IGameInstance gi, string caller, bool isAmmoReset = false)
      {
         Logger.Log(LogEnum.LE_SHOW_RESET_ROUND, "ResetRound(): %%%%%%%%%%%%%%%%%% entering - called by " + caller);
         if (false == isAmmoReset)
         {
            IMapItems ammoRemovals = new MapItems();
            foreach (IMapItem mi in gi.GunLoads)
            {
               if (true == mi.Name.Contains("AmmoReload"))
                  ammoRemovals.Add(mi);
               if (true == mi.Name.Contains("ReadyRack"))
                  ammoRemovals.Add(mi);
            }
            foreach (IMapItem mi in ammoRemovals)
               gi.GunLoads.Remove(mi);
         }
         //-------------------------------------------------------
         gi.DieRollAction = GameAction.DieRollActionNone;
         if (false == ResetDieResults(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "ResetRound(): ResetDieResults() returned false");
            return false;
         }
         //-------------------------------------------------------
         IMapItems removals = new MapItems();
         foreach (IStack stack in gi.BattleStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.Name.Contains("Advance"))
                  removals.Add(mi);
               if (true == mi.Name.Contains("Panzerfaust"))
                  removals.Add(mi);
            }
         }
         foreach (IMapItem mi in removals)
            gi.BattleStacks.Remove(mi);
         //-------------------------------------------------------
         foreach(IMapItem mi in gi.Hatches) // sync crewmember status to what hatches shows
         {
            string[] aStringArray1 = mi.Name.Split('_');
            if (2 != aStringArray1.Length)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResetRound(): aStringArray1.Length not equal to two for mi.Name=" + mi.Name);
               return false;
            }
            string role = aStringArray1[0];
            ICrewMember? cm = gi.GetCrewMemberByRole(role);
            if( null == cm )
            {
               Logger.Log(LogEnum.LE_ERROR, "ResetRound(): cm=null mi.Name=" + mi.Name);
               return false;
            }
            cm.IsButtonedUp = false;
         }
         //-------------------------------------------------------
         gi.CrewActionPhase = CrewActionPhase.Movement;
         gi.MovementEffectOnSherman = "unit";
         gi.MovementEffectOnEnemy = "unit";
         //-------------------------------------------------------
         gi.IsHatchesActive = false;
         //-------------------------------------------------------
         gi.ShermanTypeOfFire = "";                    // Reset Round()
         gi.IsShermanDeliberateImmobilization = false; // Reset Round()
         gi.NumSmokeAttacksThisRound = 0;
         gi.ShermanHits.Clear();
         //-------------------------------------------------------
         gi.IsCommanderDirectingMgFire = false;
         gi.IsShermanFiringAaMg = false;
         gi.IsShermanFiringBowMg = false;
         gi.IsShermanFiringCoaxialMg = false;
         gi.IsShermanFiringSubMg = false;
         gi.IsShermanFiredAaMg = false;
         gi.IsShermanFiredBowMg = false;
         gi.IsShermanFiredCoaxialMg = false;
         gi.IsShermanFiredSubMg = false;
         //-------------------------------------------------------
         gi.IsMainGunRepairAttempted = false;
         gi.IsAaMgRepairAttempted = false;
         gi.IsBowMgRepairAttempted = false;
         gi.IsCoaxialMgRepairAttempted = false;
         //-------------------------------------------------------
         gi.IsShermanTurretRotated = false;       // Reset Round()
         gi.ShermanRotationTurretOld = gi.Sherman.RotationTurret;
         //-------------------------------------------------------
         gi.IsAirStrikePending = false;
         gi.IsAdvancingFireChosen = false;
         gi.AdvancingFireMarkerCount = 0;
         //-------------------------------------------------------
         gi.IsMinefieldAttack = false;
         gi.IsHarrassingFireBonus = false;
         gi.IsFlankingFire = false;
         gi.IsEnemyAdvanceComplete = false;
         //-------------------------------------------------------
         gi.Death = null;
         gi.Panzerfaust = null;
         gi.NumCollateralDamage = 0;
         if (null != gi.TargetMainGun)
         {
            if (true == gi.TargetMainGun.IsKilled)
            {
               gi.TargetMainGun = null;           // if target is killed in this round
               gi.IsShermanFiringAtFront = false; // if target is killed in this round
            }
         }
         //-------------------------------------------------------
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "ResetRound(): gi.MapItemMoves.Clear()");
         gi.MapItemMoves.Clear();
         gi.Sherman.IsMoved = false;
         //-------------------------------------------------------
         bool isBattleBoardEmpty = true;
         foreach (IStack stack in gi.BattleStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.IsEnemyUnit())
               {
                  isBattleBoardEmpty = false;
                  break;
               }
            }
         }
         if (false == isBattleBoardEmpty)
         {
            gi.EventDisplayed = gi.EventActive = "e060";
            Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "ResetRound(): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.BackToSpotting");
            gi.BattlePhase = BattlePhase.BackToSpotting;
         }
         else
         {
            gi.EventDisplayed = gi.EventActive = "e036";
         }
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
               case GameAction.UpdateTankCard:
               case GameAction.UpdateShowRegion:
               case GameAction.UpdateAfterActionReport:
               case GameAction.UpdateEventViewerDisplay: // Only change active event
                  break;
               case GameAction.UpdateBattleBoard: // Do not log event
                  return returnStatus;
               case GameAction.UpdateEventViewerActive: // Only change active event
                  gi.EventDisplayed = gi.EventActive; // next screen to show
                  break;
               case GameAction.EveningDebriefingStart: // Only change active event
                  gi.EventDisplayed = gi.EventActive = "e100";
                  gi.DieRollAction = GameAction.DieRollActionNone;
                  break;
               case GameAction.EveningDebriefingRatingImprovement: // Only change active event
                  gi.BattleStacks.Clear();
                  if( false == gi.SwitchMembers("Assistant")) // return assistant back to original position prior to improvement checks
                  {
                     returnStatus = "SwitchMembers() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(EveningDebriefingRatingImprovement): " + returnStatus);
                  }
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
               case GameAction.EveningDebriefingCrewReplacedEnd:
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
                  if (false == UpdatePromotion(gi, lastReport))
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
                  if (false == UpdateDecoration(gi, lastReport, action))
                  {
                     returnStatus = "UpdateDecoration() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(): " + returnStatus);
                  }
                  break;
               case GameAction.EventDebriefDecorationHeart:
                  if (false == EveningDebriefingResetDay(gi, lastReport))
                  {
                     returnStatus = "EveningDebriefing_ResetDay() returned false";
                     Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(EventDebriefDecorationHeart): " + returnStatus);
                  }
                  break;
               case GameAction.EndGameClose:
                  gi.GamePhase = GamePhase.EndGame;
                  break;
               default:
                  returnStatus = "reached default with action=" + action.ToString();
                  Logger.Log(LogEnum.LE_ERROR, "GameStateEveningDebriefing.PerformAction(): " + returnStatus);
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
         if (EnumScenario.Advance == report.Scenario)
         {
            scenarioMultiplierKOGermanUnit = 1;
            scenarioMultiplierCapturedMapArea = 2;
            scenarioMultiplierLoseMapArea = 1;
         }
         else if (EnumScenario.Battle == report.Scenario)
         {
            scenarioMultiplierKOGermanUnit = 1;
            scenarioMultiplierCapturedMapArea = 2;
            scenarioMultiplierLoseMapArea = 3;
         }
         else if (EnumScenario.Counterattack == report.Scenario)
         {
            scenarioMultiplierKOGermanUnit = 2;
            scenarioMultiplierCapturedMapArea = 0;
            scenarioMultiplierLoseMapArea = 2;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateFor_EveningDebriefing(): report.Scenario=" + report.Scenario.ToString());
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_VP_TOTAL, "UpdateFor_EveningDebriefing(): koX=" + scenarioMultiplierKOGermanUnit.ToString() + " captureX=" + scenarioMultiplierCapturedMapArea.ToString() + " lostX=" + scenarioMultiplierLoseMapArea.ToString());
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
         Logger.Log(LogEnum.LE_SHOW_VP_TOTAL, "UpdateFor_EveningDebriefing(): report.VictoryPtsTotalYourTank=" + report.VictoryPtsTotalYourTank.ToString());
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
         Logger.Log(LogEnum.LE_SHOW_VP_TOTAL, "UpdateFor_EveningDebriefing(): report.VictoryPtsTotalFriendlyForces=" + report.VictoryPtsTotalFriendlyForces.ToString());
         //----------------------------------
         report.VictoryPtsTotalTerritory = (report.VictoryPtsCaptureArea + 10 * report.VictoryPtsCapturedExitArea) * scenarioMultiplierCapturedMapArea;
         report.VictoryPtsTotalTerritory -= (report.VictoryPtsLostArea * scenarioMultiplierLoseMapArea);
         Logger.Log(LogEnum.LE_SHOW_VP_TOTAL, "UpdateFor_EveningDebriefing(): report.VictoryPtsTotalTerritory=" + report.VictoryPtsTotalTerritory.ToString());
         //----------------------------------
         report.VictoryPtsTotalEngagement = report.VictoryPtsTotalYourTank + report.VictoryPtsTotalFriendlyForces + report.VictoryPtsTotalTerritory;
         gi.VictoryPtsTotalCampaign += report.VictoryPtsTotalEngagement;
         gi.PromotionPointNum += report.VictoryPtsTotalYourTank;
         //----------------------------------
         report.DayEndedTime = TableMgr.GetTime(report);
         //----------------------------------
         return true;
      }
      public bool UpdatePromotion(IGameInstance gi, IAfterActionReport report)
      {
         string oldRank = report.Commander.Rank;
         switch (oldRank)
         {
            case "Sgt":
               if (99 < gi.PromotionPointNum)
               {
                  gi.PromotionDay = gi.Day;
                  report.Commander.Rank = "Ssg";
               }
               break;
            case "2Lt":
               if (199 < gi.PromotionPointNum)
               {
                  gi.PromotionDay = gi.Day;
                  report.Commander.Rank = "2Lt";
               }
               break;
            case "1Lt":
               if (299 < gi.PromotionPointNum)
               {
                  gi.PromotionDay = gi.Day;
                  report.Commander.Rank = "1Lt";
               }
               break;
            case "Cpt":
               if (399 < gi.PromotionPointNum)
               {
                  gi.PromotionDay = gi.Day;
                  report.Commander.Rank = "Cpt";
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdatePromotion(): reached default cmdrRank=" + oldRank);
               return false;
         }
         string promoDate = TableMgr.GetDate(gi.PromotionDay);
         if ("Boot Camp" == promoDate)
         {
            gi.PromotionDay = gi.Day;
            return true;
         }
         string currentDate = TableMgr.GetDate(gi.Day);
         int diff = Utilities.DiffInDates(promoDate, currentDate);
         if (diff < -999)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdatePromotion(): Utilities.DiffInDates() returned error");
            return false;
         }
         if (("07/27/1944" == promoDate) || (29 < diff))  // cannot get promoted until 30 days past since last promotion
         {
            gi.PromotionDay = gi.Day;
            gi.IsPromoted = true;
         }
         else
         {
            report.Commander.Rank = oldRank;
         }
         return true;
      }
      public bool UpdateDecoration(IGameInstance gi, IAfterActionReport report, GameAction action)
      {
         ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
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
            StringBuilder sb2 = new StringBuilder(commander.Name);
            sb2.Append(" received the Purple Heart.");
            report.Notes.Add(sb1.ToString());
         }
         else
         {
            if (false == EveningDebriefingResetDay(gi, report))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateDecoration(): EveningDebriefing_ResetDay(=null) returned false");
               return false;
            }
         }
         return true;
      }
      public bool EveningDebriefingResetDay(IGameInstance gi, IAfterActionReport report)
      {
         //-------------------------------------------------------
         bool isCrewmanReplaced;
         if (false == ReplaceInjuredCrewmen(gi, out isCrewmanReplaced, "EveningDebriefingResetDay()"))  // Reset_Day()
         {
            Logger.Log(LogEnum.LE_ERROR, "EveningDebriefing_ResetDay(): Replace_InjuredCrewmen() returned false");
            return false;
         }
         if (true == isCrewmanReplaced)
         {
            gi.EventDisplayed = gi.EventActive = "e007b"; // Crew Replacement
            gi.DieRollAction = GameAction.DieRollActionNone;
         }
         else
         {
            gi.EventDisplayed = gi.EventActive = "e006";   // Reset_Day()
            gi.DieRollAction = GameAction.MorningBriefingCalendarRoll;
         }
         //-------------------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_BATTLE_PHASE, "EveningDebriefing_ResetDay(): phase=" + gi.BattlePhase.ToString() + "-->BattlePhase.Ambush");
         gi.BattlePhase = BattlePhase.Ambush;
         gi.CrewActionPhase = CrewActionPhase.Movement;
         gi.MovementEffectOnSherman = "unit";
         gi.MovementEffectOnEnemy = "unit";
         //-------------------------------------------------------
         gi.ReadyRacks.Clear();
         gi.Hatches.Clear();
         gi.CrewActions.Clear();            // Reset_Day()
         Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "EveningDebriefing_ResetDay(): clear all crewactions");
         gi.GunLoads.Clear();
         gi.TargetMainGun = null;           // Reset_Day()
         gi.IsShermanFiringAtFront = false; // Reset_Day()
         //-------------------------------------------------------
         gi.EnemyStrengthCheckTerritory = null;
         gi.ArtillerySupportCheck = null;
         gi.AirStrikeCheckTerritory = null;
         gi.EnteredArea = null;
         gi.AdvanceFire = null;
         //-------------------------------------------------------
         gi.IsHatchesActive = false;
         //-------------------------------------------------------
         gi.IsMinefieldAttack = false;                    // Reset_Day()
         gi.IsHarrassingFireBonus = false;
         gi.IsFlankingFire = false;
         gi.IsEnemyAdvanceComplete = false;
         //-------------------------------------------------------
         gi.IsShermanFirstShot = false;
         gi.IsShermanFiringAtFront = false;              // Reset_Day()
         gi.IsShermanDeliberateImmobilization = false;
         gi.ShermanTypeOfFire = "";
         gi.NumSmokeAttacksThisRound = 0;
         gi.IsMainGunRepairAttempted = false;
         //---------------------------------
         gi.IsMinefieldAttack = false;
         gi.IsHarrassingFireBonus = false;
         gi.IsFlankingFire = false;
         gi.IsEnemyAdvanceComplete = false;
         gi.Panzerfaust = null;
         gi.NumCollateralDamage = 0;
         //-------------------------------------------------------
         gi.TargetMainGun = null;                      // Reset_Day()
         if( (true == gi.IsMalfunctionedMainGun) || (true == gi.IsBrokenMainGun) )
            Logger.Log(LogEnum.LE_SHOW_MAIN_GUN_BREAK, "EveningDebriefing_ResetDay(): Main Gun Repaired");
         gi.IsMalfunctionedMainGun = false;
         gi.IsBrokenMainGun = false;
         gi.IsBrokenGunSight = false;
         gi.FirstShots.Clear();                     // Reset_Day()
         gi.AcquiredShots.Clear();                  // Reset_Day()
         gi.ShermanHits.Clear();
         //-------------------------------------------------------
         gi.IsCommanderDirectingMgFire = false;
         gi.IsShermanFiringAaMg = false;
         gi.IsShermanFiringBowMg = false;
         gi.IsShermanFiringCoaxialMg = false;
         gi.IsShermanFiringSubMg = false;
         gi.IsShermanFiredAaMg = false;
         gi.IsShermanFiredBowMg = false;
         gi.IsShermanFiredCoaxialMg = false;
         gi.IsShermanFiredSubMg = false;
         gi.IsMalfunctionedMgAntiAircraft = false;
         gi.IsMalfunctionedMgBow = false;
         gi.IsMalfunctionedMgCoaxial = false;
         gi.IsBrokenMgAntiAircraft = false;
         gi.IsBrokenMgCoaxial = false;
         gi.IsBrokenMgAntiAircraft = false;
         //-------------------------------------------------------
         gi.IsBrokenPeriscopeDriver = false;
         gi.IsBrokenPeriscopeLoader = false;
         gi.IsBrokenPeriscopeAssistant = false;
         gi.IsBrokenPeriscopeGunner = false;
         gi.IsBrokenPeriscopeCommander = false;
         //-------------------------------------------------------
         gi.IsShermanTurretRotated = false;  // Reset_Day()
         gi.ShermanRotationTurretOld = 0.0;
         //-------------------------------------------------------
         gi.IsLeadTank = false;
         gi.IsAirStrikePending = false;
         gi.IsAdvancingFireChosen = false;
         gi.AdvancingFireMarkerCount = 0;
         //-------------------------------------------------------
         gi.IsMinefieldAttack = false;
         gi.IsHarrassingFireBonus = false;
         gi.IsFlankingFire = false;
         gi.IsEnemyAdvanceComplete = false;
         //-------------------------------------------------------
         gi.IsCommanderRescuePerformed = false;
         gi.IsPromoted = false;
         //-------------------------------------------------------
         gi.BattleResistance = EnumResistance.None;                      // Reset_Day()
         gi.Death = null;
         gi.Panzerfaust = null;
         gi.NumCollateralDamage = 0;
         //-------------------------------------------------------
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "EveningDebriefing_ResetDay(): gi.MapItemMoves.Clear()");
         gi.MapItemMoves.Clear();
         gi.MoveStacks.Clear();
         gi.BattleStacks.Clear();
         gi.EnteredHexes.Clear();
         //-------------------------------------------------------
         gi.Sherman.IsMoved = false;
         gi.Sherman.RotationOffsetHull = 0.0;
         gi.Sherman.RotationTurret = 0.0;
         gi.Sherman.RotationHull = 0.0;
         gi.Sherman.IsMoving = false;
         gi.Sherman.IsHullDown = false;
         gi.Sherman.IsBoggedDown = false;
         gi.Sherman.IsAssistanceNeeded = false;
         //-------------------------------------------------------
         ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
         if (null == commander)
         {
            Logger.Log(LogEnum.LE_ERROR, "EveningDebriefing_ResetDay(): commander=null");
            return false;
         }
         if (true == commander.IsKilled)
            gi.PromotionPointNum = 0;
         //-------------------------------------------------------
         if (false == ResetDieResults(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "EveningDebriefing_ResetDay(): ResetDieResults() returned false");
            return false;
         }
         //-------------------------------------------------------
         ++gi.Day;
         gi.GameTurn++;
         gi.GamePhase = GamePhase.MorningBriefing;
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
            case GameAction.ShowTankForcePath:
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
            case GameAction.UpdateTankCard:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateAfterActionReport:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
               break;
            case GameAction.UpdateBattleBoard: // Do not log event
               return returnStatus;
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
            case GameAction.ShowTankForcePath:
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
            case GameAction.UpdateTankCard:
            case GameAction.UpdateShowRegion:
            case GameAction.UpdateAfterActionReport:
            case GameAction.UpdateEventViewerDisplay: // Only change active event
               break;
            case GameAction.UpdateBattleBoard: // Do not log event
               return returnStatus;
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
               returnStatus = "Reached Default ERROR action=" + action.ToString();
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