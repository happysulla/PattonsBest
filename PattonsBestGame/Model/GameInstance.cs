
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.ApplicationModel.Appointments.AppointmentsProvider;

namespace Pattons_Best
{ 
   public class GameInstance : IGameInstance
   {
      static public Logger Logger = new Logger();
      public bool CtorError { get; } = false;
      public Options Options { get; set; } = new Options();
      public GameStat Statistic { get; set; } = new GameStat();
      //------------------------------------------------
      public bool IsMultipleSelectForDieResult { set; get; } = false;
      public bool IsGridActive { set; get; } = false;
      public string EventActive { get; set; } = "e000";
      public string EventDisplayed { set; get; } = "e000";
      public string EventStart { set; get; } = "e000";
      public List<string> Events { set; get; } = new List<string>();
      private Dictionary<string, int[]> myDieResults = new Dictionary<string, int[]>();
      public Dictionary<string, int[]> DieResults { get => myDieResults; }
      //------------------------------------------------
      public int Day { get; set; } = 0;
      public int GameTurn { get; set; } = 0;
      public GamePhase GamePhase { get; set; } = GamePhase.GameSetup;
      public GameAction DieRollAction { get; set; } = GameAction.DieRollActionNone;
      public bool IsUndoCommandAvailable { set; get; } = false;
      public string EndGameReason { set; get; } = "";
      //------------------------------------------------
      public IAfterActionReports Reports { get; set; } = new AfterActionReports();
      public BattlePhase BattlePhase { set; get; } = BattlePhase.None;
      public CrewActionPhase CrewActionPhase { set; get; } = CrewActionPhase.None;
      public string MovementEffectOnSherman { set; get; } = "unitialized";
      public string MovementEffectOnEnemy { set; get; } = "unitialized";
      public string ShermanTypeOfFire { set; get; } = "";
      public string FiredAmmoType { set; get; } = "";
      //---------------------------------------------------------------
      public IMapItems NewMembers { set; get; } = new MapItems();
      public IMapItems ReadyRacks { set; get; } = new MapItems();
      public IMapItems Hatches { set; get; } = new MapItems();
      public IMapItems CrewActions { set; get; } = new MapItems();
      public IMapItems GunLoads { set; get; } = new MapItems();
      public IMapItem Sherman { set; get; } = new MapItem("Sherman75", 2.0, "t01", new Territory());
      public IMapItems Targets { set; get; } = new MapItems();
      public IMapItems AdvancingEnemies { set; get; } = new MapItems();   // enemies that appear on Move board for advancing to lower edge of board
      public IMapItems ShermanAdvanceOrRetreatEnemies { set; get; } = new MapItems(); // enemies that appear on Move board if Sherman Advances or Retreats
      public IMapItem? TargetMainGun { set; get; } = null;
      public IMapItem? TargetMg { set; get; } = null;
      public IMapItems InjuredCrewMembers { set; get; } = new MapItems();
      public IMapItem? ShermanHvss { set; get; } = null;
      public ICrewMember? ReturningCrewman { set; get; } = null;
      //------------------------------------------------
      public ITerritory Home { get; set; } = new Territory();
      public ITerritory? EnemyStrengthCheckTerritory { get; set; } = null;
      public ITerritory? ArtillerySupportCheck { get; set; } = null;
      public ITerritory? AirStrikeCheckTerritory { get; set; } = null;
      public ITerritory? EnteredArea { get; set; } = null;
      public ITerritory? AdvanceFire { get; set; } = null;
      public ITerritory? FriendlyAdvance { get; set; } = null;
      public ITerritory? EnemyAdvance { get; set; } = null;
      public ITerritories AreaTargets { get; set; } = new Territories();
      //---------------------------------------------------------------
      public bool IsHatchesActive { set; get; } = false;
      public bool IsRetreatToStartArea { set; get; } = false;
      //---------------------------------------------------------------
      public string SwitchedCrewMember { set; get; } = "";
      public int AssistantOriginalRating { set; get; } = 0;
      //---------------------------------------------------------------
      public bool IsShermanFirstShot { set; get; } = false;
      public bool IsShermanFiringAtFront { set; get; } = false;
      public bool IsShermanDeliberateImmobilization { set; get; } = false;
      public bool IsShermanHvss { set; get; } = false;
      public int NumSmokeAttacksThisRound { set; get; } = 0;
      public bool IsMalfunctionedMainGun { set; get; } = false;
      public bool IsMainGunRepairAttempted { set; get; } = false;
      public bool IsBrokenMainGun { set; get; } = false;
      public bool IsBrokenGunSight { set; get; } = false;
      public Dictionary<string, bool> FirstShots { set; get; } = new Dictionary<string, bool>();
      public Dictionary<string, int> AcquiredShots { set; get; } = new Dictionary<string, int>();
      public List<ShermanAttack> ShermanHits { set; get; } = new List<ShermanAttack>();
      public ShermanDeath? Death { set; get; } = null;
      //---------------------------------------------------------------
      public bool IsShermanFiringAaMg { set; get; } = false;
      public bool IsShermanFiringBowMg { set; get; } = false;
      public bool IsShermanFiringCoaxialMg { set; get; } = false;
      public bool IsShermanFiringSubMg { set; get; } = false;
      public bool IsCommanderDirectingMgFire { set; get; } = false;
      public bool IsShermanFiredAaMg { set; get; } = false;
      public bool IsShermanFiredBowMg { set; get; } = false;
      public bool IsShermanFiredCoaxialMg { set; get; } = false;
      public bool IsShermanFiredSubMg { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsMalfunctionedMgAntiAircraft { set; get; } = false;
      public bool IsMalfunctionedMgBow { set; get; } = false;
      public bool IsMalfunctionedMgCoaxial { set; get; } = false;
      public bool IsCoaxialMgRepairAttempted { set; get; } = false;
      public bool IsBowMgRepairAttempted { set; get; } = false;
      public bool IsAaMgRepairAttempted { set; get; } = false;
      public bool IsBrokenMgAntiAircraft { set; get; } = false;
      public bool IsBrokenMgBow { set; get; } = false;
      public bool IsBrokenMgCoaxial { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsBrokenPeriscopeDriver { set; get; } = false;
      public bool IsBrokenPeriscopeLoader { set; get; } = false;
      public bool IsBrokenPeriscopeAssistant { set; get; } = false;
      public bool IsBrokenPeriscopeGunner { set; get; } = false;
      public bool IsBrokenPeriscopeCommander { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsShermanTurretRotated { set; get; } = false;
      public double ShermanRotationTurretOld { set; get; } = 0.0;
      //---------------------------------------------------------------
      public bool IsLeadTank { set; get; } = false;
      public bool IsAirStrikePending { set; get; } = false;
      public bool IsAdvancingFireChosen { set; get; } = false;
      public int AdvancingFireMarkerCount { set; get; } = 0;
      public EnumResistance BattleResistance { set; get; } = EnumResistance.None;
      //---------------------------------------------------------------
      public bool IsMinefieldAttack { set; get; } = false;
      public bool IsHarrassingFireBonus { set; get; } = false;
      public bool IsFlankingFire { set; get; } = false;
      public bool IsEnemyAdvanceComplete { set; get; } = false;
      public PanzerfaustAttack? Panzerfaust { set; get; } = null;
      public int NumCollateralDamage { set; get; } = 0;
      //---------------------------------------------------------------
      public int VictoryPtsTotalCampaign { get; set; } = 0;
      public int PromotionPointNum { get; set; } = 0;
      public int PromotionDay { get; set; } = -1;
      public int NumPurpleHeart { get; set; } = 0;
      public bool IsCommanderRescuePerformed { set; get; } = false;
      public bool IsCommanderKilled { set; get; } = false;
      public bool IsPromoted { set; get; } = false;
      //---------------------------------------------------------------
      public IMapItemMoves MapItemMoves { get; set; } = new MapItemMoves();
      public IStacks MoveStacks { get; set; } = new Stacks();
      public IStacks BattleStacks { get; set; } = new Stacks();
      private List<EnteredHex> myEnteredHexes = new List<EnteredHex>();
      public List<EnteredHex> EnteredHexes { get => myEnteredHexes; }
      //---------------------------------------------------------------
      [NonSerialized] private List<IUnitTest> myUnitTests = new List<IUnitTest>();
      public List<IUnitTest> UnitTests { get => myUnitTests; }
      //==============================================================
      public GameInstance() // Constructor - set log levels
      {
         if (false == Logger.SetInitial()) // tsetup logger
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): SetInitial() returned false");
            CtorError = true;
            return;
         }
         try
         {
            GameLoadMgr gameLoadMgr = new GameLoadMgr();
            string filename = ConfigFileReader.theConfigDirectory + Territories.FILENAME;
            XmlTextReader? reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
            if (null == reader)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): reader=null");
               return;
            }
            if (false == gameLoadMgr.ReadXmlTerritories(reader, Territories.theTerritories))
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadTerritoriesXml() returned false");
            ITerritory? tHome = Territories.theTerritories.Find("Home");
            if (null == tHome)
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): tHome=null");
            else
               Home = tHome;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadTerritoriesXml() exception=\n" + e.ToString());
            return;
         }
         //------------------------------------------------------------------------------------
         if (false == SurnameMgr.SetInitial())
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): SurnameMgr.SetInitial() returned false");
            CtorError = true;
            return;
         }
      }
      public GameInstance(Options newGameOptions) // Constructor - set log levels
      {
         Options = newGameOptions;
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder("[");
         sb.Append("t=");
         sb.Append(GameTurn.ToString());
         sb.Append(",p=");
         sb.Append(GamePhase.ToString());
         sb.Append("]");
         return sb.ToString();
      }
      //---------------------------------------------------------------
      public bool IsCrewActionSelectable(string crewRole, out bool isGiven)
      {
         isGiven = false;
         //-----------------------------------------------
         IAfterActionReport? lastReport = this.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsCrewActionSelectable(): lastReport=null");
            return false;
         }
         int totalAmmo = lastReport.MainGunHE + lastReport.MainGunAP + lastReport.MainGunWP + lastReport.MainGunHBCI + lastReport.MainGunHVAP;
         TankCard card = new TankCard(lastReport.TankCardNum);
         //-----------------------------------------------
         ICrewMember? crewMember = this.GetCrewMemberByRole(crewRole);
         if (null == crewMember)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsCrewActionSelectable(): crewMember=null for crewRole=" + crewRole);
            return false;
         }
         //-----------------------------------------------
         bool isLoaderFireAaMg = false;
         bool isLoaderRepairAaMg = false;
         bool isLoaderFireSubMg = false;
         bool isLoaderChangingLoad = false;
         bool isTankMoving = false;
         int periscopeRepairCount = 0;
         foreach (IMapItem mi in this.CrewActions) // This menu is created on each crew action
         {
            if (true == mi.Name.Contains("Loader_ChangeGunLoad"))
               isLoaderChangingLoad = true;
            if (true == mi.Name.Contains("Loader_FireAaMg"))
               isLoaderFireAaMg = true;
            if (true == mi.Name.Contains("Loader_FireAaMg"))
               isLoaderFireAaMg = true;
            if (true == mi.Name.Contains("Loader_RepairAaMg"))
               isLoaderRepairAaMg = true;
            if (true == mi.Name.Contains("Loader_FireSubMg"))
               isLoaderFireSubMg = true;
            if (true == mi.Name.Contains("Driver_Forward"))
               isTankMoving = true;
            if (true == mi.Name.Contains("Driver_ForwardToHullDown"))
               isTankMoving = true;
            if (true == mi.Name.Contains("Driver_Reverse"))
               isTankMoving = true;
            if (true == mi.Name.Contains("Driver_ReverseToHullDown"))
               isTankMoving = true;
            if (true == mi.Name.Contains("Driver_PivotTank"))
               isTankMoving = true;
            if (true == mi.Name.Contains("RepairScope"))
               periscopeRepairCount++;
         }
         int diffPeriscopes = lastReport.AmmoPeriscope - periscopeRepairCount; // How many periscopes can be repaired
         //-----------------------------------------------
         bool isGunnerOpenHatch = false;
         bool isDriverOpenHatch = false;
         bool isCommanderOpenHatch = false;
         foreach (IMapItem mi in this.Hatches) // Loader and Driver have default actions
         {
            if (true == mi.Name.Contains("Driver"))
               isDriverOpenHatch = true;
            if (true == mi.Name.Contains("Gunner"))
               isGunnerOpenHatch = true;
            if (true == mi.Name.Contains("Commander"))
               isCommanderOpenHatch = true;
         }
         //---------------------------------
         bool isMainGunFiringAvailable = ((false == isTankMoving) && (false == this.IsMalfunctionedMainGun) && (false == this.IsBrokenMainGun) && (false == this.IsBrokenGunSight) && (0 < totalAmmo) && ("None" != this.GetGunLoadType()) && (false == isLoaderChangingLoad));
         bool isShermanMoveAvailable = ((false == this.Sherman.IsThrownTrack) && (false == this.Sherman.IsAssistanceNeeded) && (false == this.IsBrokenPeriscopeDriver) || (true == isDriverOpenHatch));
         switch (crewRole)
         {
            case "Assistant":
               if (true == crewMember.IsIncapacitated)
                  isGiven = true;
               return true;
            case "Gunner":
               bool isFixBrokenGunnerPeriscopeAvailable = ((true == this.IsBrokenPeriscopeGunner) && (0 < diffPeriscopes));
               bool isCoaxialMgFiringAvailable = ((false == this.IsBrokenPeriscopeGunner) || (true == isGunnerOpenHatch));
               if ((false == isMainGunFiringAvailable) && (false == isCoaxialMgFiringAvailable) && (false == isFixBrokenGunnerPeriscopeAvailable) || (true == crewMember.IsIncapacitated))
                  isGiven = true;
               return true;
            case "Commander":
               //-----------------------------------------------
               bool is30CalibreMGFirePossible = (0 < lastReport.Ammo30CalibreMG) && (((false == this.IsBrokenMgBow) && (false == this.IsMalfunctionedMgBow)) || ((false == this.IsBrokenMgCoaxial) && (false == this.IsMalfunctionedMgCoaxial))); // bow and coaxial MGs
               bool is50CalibreMGFirePossible = (0 < lastReport.Ammo50CalibreMG) && ((false == this.IsBrokenMgAntiAircraft) && (false == this.IsMalfunctionedMgAntiAircraft)); // subMG can always be fired
               bool isFixBrokenCmdrPeriscopeAvailable = ((true == this.IsBrokenPeriscopeGunner) && (0 < diffPeriscopes));
               bool isAntiAircraftMgAbleToFire = ((true == isCommanderOpenHatch) && (false == isLoaderFireAaMg) && (true == is50CalibreMGFirePossible));
               bool isAntiAircraftMgRepairPossible = ((true == this.IsMalfunctionedMgAntiAircraft) && (false == isLoaderRepairAaMg) && (false == this.IsBrokenMgAntiAircraft));
               bool isSubMgAbleToFire = ((true == isCommanderOpenHatch) && (false == isLoaderFireSubMg));
               if ((false == isMainGunFiringAvailable) && (false == isShermanMoveAvailable) && (false == isFixBrokenCmdrPeriscopeAvailable) && (false == is30CalibreMGFirePossible) && (false == is50CalibreMGFirePossible) && (false == isAntiAircraftMgAbleToFire) && (false == isAntiAircraftMgRepairPossible) && (false == isSubMgAbleToFire) || (true == crewMember.IsIncapacitated))
                  isGiven = true;
               return true;
            default:
               Logger.Log(LogEnum.LE_ERROR, "IsCrewActionSelectable(): reached default crewRole=" + crewRole);
               return false;
         }
      }
      public bool IsCrewActionPossibleButtonUp(string crewRole, string crewAction)
      {
         switch (crewRole)
         {
            case "Loader":
               {
                  if (("Loader_FireAaMg" == crewAction) || ("Loader_FireSubMg" == crewAction))
                     return false;
               }
               break;
            case "Driver":
               if (true == this.IsBrokenPeriscopeGunner)
               {
                  if (("Driver_ForwardToHullDown" == crewAction) || ("Driver_Forward" == crewAction) || ("Driver_Reverse" == crewAction) || ("Driver_ReverseToHullDown" == crewAction) || ("Driver_ReverseToHullDown" == crewAction))
                     return false;
               }
               break;
            case "Gunner":
               if (true == this.IsBrokenPeriscopeGunner)
               {
                  if (("Gunner_FireCoaxialMg" == crewAction) || ("Gunner_RotateTurret" == crewAction))
                     return false;
               }
               if ("Gunner_ThrowGrenade" == crewAction)
               {
                  return false;
               }
               break;
            case "Assistant":
               if (true == this.IsBrokenPeriscopeAssistant)
               {
                  if ("Assistant_FireBowMg" == crewAction)
                     return false;
               }
               break;
            case "Commander":
               IAfterActionReport? lastReport = this.Reports.GetLast();
               if (null == lastReport)
               {
                  Logger.Log(LogEnum.LE_ERROR, "IsCrewActionPossibleButtonUp(): lastReport=null");
                  return false;
               }
               TankCard card = new TankCard(lastReport.TankCardNum);
               if ((true == this.IsBrokenPeriscopeCommander) && (false == card.myIsVisionCupola))
               {
                  if (("Commander_Move" == crewAction) || ("Commander_MainGunFire" == crewAction) || ("Commander_MGFire" == crewAction))
                     return false;
               }
               if (("Commander_ThrowGrenade" == crewAction) || ("Commander_FireAaMg" == crewAction) || ("Commander_FireSubMg" == crewAction))
                  return false;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "IsCrewActionPossibleButtonUp(): reached default crew role=" + crewRole);
               break;
         }
         return true;
      }
      public ICrewMember? GetCrewMemberByRole(string role)
      {
         IAfterActionReport? report = Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetCrewMemberByRole(): report=null");
            return null;
         }
         if (role == "Driver")
            return report.Driver;
         if (role == "Assistant")
            return report.Assistant;
         if  (role == "Loader")
            return report.Loader;
         if  (role == "Gunner")
            return report.Gunner;
         if (role == "Commander")
            return report.Commander;
         Logger.Log(LogEnum.LE_ERROR, "GetCrewMemberByRole(): reached default name=" + role.ToString());
         return null;
      }
      public ICrewMember? GetCrewMemberByName(string name)
      {
         IAfterActionReport? report = Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetCrewMemberByName(): report=null");
            return null;
         }
         if (name == report.Driver.Name) 
            return report.Driver;
         if (name == report.Assistant.Name) 
            return report.Assistant;
         if (name == report.Loader.Name) 
            return report.Loader;
         if (name == report.Gunner.Name) 
            return report.Gunner;
         if (name == report.Commander.Name) 
            return report.Commander;
         Logger.Log(LogEnum.LE_ERROR, "GetCrewMemberByName(): reached default name=" + name.ToString());
         return null;
      }
      public bool SetCrewActionTerritory(ICrewMember cm)
      {
         IAfterActionReport? lastReport = this.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetCrewActionTerritory(): lastReport=null");
            return false;
         }
         string tName = cm.Role + "Action";
         string tType = lastReport.TankCardNum.ToString();
         ITerritory? t = Territories.theTerritories.Find(tName, tType);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetCrewActionTerritory(): Territories.theTerritories.Find() returned false for tName=" + tName + " type=" + tType);
            return false;
         }
         cm.TerritoryCurrent = t;
         cm.Location.X = t.CenterPoint.X - Utilities.theMapItemOffset;
         cm.Location.Y = t.CenterPoint.Y - Utilities.theMapItemOffset;
         return true;
      }
      public void SetIncapacitated(ICrewMember cm)
      { 
         cm.IsIncapacitated = true;
         bool isAlreadyIncapacitated = false;
         //-------------------------------
         IMapItems removals = new MapItems(); // Remove any coorresponding crew actions and add the hurt crewman in the crewaction box
         foreach (IMapItem ca in this.CrewActions)
         {
            if ( true == ca.Name.Contains(cm.Role) || (cm.Name == ca.Name) ) // crew action contains role or it is already incapacitated and added
               removals.Add(ca);
            if (cm.Name == ca.Name) // if crew action matches cm.Name -- it is already incapacitated and added
               isAlreadyIncapacitated = true;
         }
         foreach (IMapItem ca in removals)
         {
            this.CrewActions.Remove(ca);
            Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "Set_Incapacitated(): ---------------------removing ca=" + ca.Name);
         }
         if( false == isAlreadyIncapacitated)
         {
            this.CrewActions.Add(cm);
            Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "Set_Incapacitated(): +++++++++++++++++++++++adding cm=" + cm.Name + " cm.Role=" + cm.Role);
         }
         //-------------------------------
         removals.Clear();
         foreach (IMapItem mi in this.Hatches) // incapacitated crewmember becomes button up
         {
            if (true == mi.Name.Contains(cm.Role)) 
               removals.Add(mi);
         }
         foreach (IMapItem mi in removals)
            this.Hatches.Remove(mi);
         cm.IsButtonedUp = true;
      }
      public bool SwitchMembers(string switchingMemberRole)
      {
         //--------------------------------------------
         IAfterActionReport? report = Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetCrewMemberByRole(): report=null");
            return false;
         }
         //--------------------------------------------
         foreach (IMapItem mi in this.CrewActions) // Remove switch counter
         {
            if (true == mi.Name.Contains("Switch"))
            {
               this.CrewActions.Remove(mi);
               Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "SwitchMembers(): --------------------removing ca=" + mi.Name);
               break;
            }
         }
         //=========================================================
         if (false == String.IsNullOrEmpty(this.SwitchedCrewMember)) // Return SwitchedCrewMember and Assistant back to their original positions
         {
            ICrewMember? switchedMember = GetCrewMemberByRole("Assistant"); // assistant is in role of switched member
            if (null == switchedMember)
            {
               Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): switchedMember=null for role" + this.SwitchedCrewMember);
               return false;
            }
            switchedMember.IsButtonedUp = true;
            Logger.Log(LogEnum.LE_SHOW_CREW_SWITCH, "SwitchMembers(): Return Assistant to original position of crew member to " + this.SwitchedCrewMember);
            foreach (IMapItem mi in this.Hatches)
            {
               if (true == mi.Name.Contains(this.SwitchedCrewMember))
               {
                  this.Hatches.Remove(mi);
                  break;
               }
            }
            switch (this.SwitchedCrewMember)
            {
               case "Driver":
                  report.Assistant = report.Driver;
                  report.Driver = switchedMember;
                  report.Driver.Role = "Driver";
                  if (false == SetCrewActionTerritory(report.Driver))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): SetCrewMemberTerritory(report.Driver) returned false");
                     return false;
                  }
                  break;
               case "Loader":
                  report.Assistant = report.Loader;
                  report.Loader = switchedMember;
                  report.Loader.Role = "Loader";
                  if (false == SetCrewActionTerritory(report.Loader))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): SetCrewMemberTerritory(report.Loader) returned false");
                     return false;
                  }
                  break;
               case "Gunner":
                  report.Assistant = report.Gunner;
                  report.Gunner = switchedMember;
                  report.Gunner.Role = "Gunner";
                  if (false == SetCrewActionTerritory(report.Gunner))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): SetCrewMemberTerritory(report.Gunner) returned false");
                     return false;
                  }
                  break;
               case "Commander":
                  report.Assistant = report.Commander;
                  report.Commander = switchedMember;
                  report.Commander.Role = "Commander";
                  if (false == SetCrewActionTerritory(report.Commander))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): SetCrewMemberTerritory(report.Commander) returned false");
                     return false;
                  }
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): reached default name=" + switchedMember.Role);
                  return false;
            }
            report.Assistant.Rating = this.AssistantOriginalRating;
            report.Assistant.Role = "Assistant";
            if (false == SetCrewActionTerritory(report.Assistant))
            {
               Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): SetCrewMemberTerritory(Assistant) returned false");
               return false;
            }
         }
         this.SwitchedCrewMember = "";
         if ("Assistant" == switchingMemberRole)
            return true;
         //=========================================================
         Logger.Log(LogEnum.LE_SHOW_CREW_SWITCH, "SwitchMembers(): Switched Assistant with Crew Member=" + switchingMemberRole);
         foreach (IMapItem mi in this.Hatches) // Assistant becomes button up
         {
            if (true == mi.Name.Contains("Assistant"))
            {
               this.Hatches.Remove(mi);
               break;
            }
         }
         report.Assistant.IsButtonedUp = true;
         //--------------------------------------------
         this.AssistantOriginalRating = report.Assistant.Rating;
         if("Driver" != switchingMemberRole)  // any member being switched other than driver reduces rating by half
             report.Assistant.Rating = (int)Math.Floor((double)(report.Assistant.Rating * 0.5));
         ICrewMember? switchingMember = GetCrewMemberByRole(switchingMemberRole); // assistant is in role of switched member
         if (null == switchingMember)
         {
            Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): switchedMember=null for role=" + switchingMemberRole);
            return false;
         }
         this.SwitchedCrewMember = switchingMember.Role;
         switch (this.SwitchedCrewMember)
         {
            case "Driver":
               report.Driver = report.Assistant;
               report.Driver.Role = "Driver";
               if (false == SetCrewActionTerritory(report.Driver)) // puts assistant in proper spot
               {
                  Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): SetCrewMemberTerritory(Assistant) returned false");
                  return false;
               }
               break;
            case "Loader":
               ICrewMember loader = report.Loader;
               report.Loader = report.Assistant;
               report.Loader.Role = "Loader";
               if (false == SetCrewActionTerritory(report.Loader)) // puts assistant in proper spot
               {
                  Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): SetCrewMemberTerritory(Assistant) returned false");
                  return false;
               }
               break;
            case "Gunner":
               ICrewMember gunner = report.Gunner;
               report.Gunner = report.Assistant;
               report.Gunner.Role = "Gunner";
               if (false == SetCrewActionTerritory(report.Gunner)) // puts assistant in proper spot
               {
                  Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): SetCrewMemberTerritory(Assistant) returned false");
                  return false;
               }
               break;
            case "Commander":
               ICrewMember commander = report.Commander;
               report.Commander = report.Assistant;
               report.Commander.Role = "Commander";
               if (false == SetCrewActionTerritory(report.Commander)) // puts assistant in proper spot
               {
                  Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): SetCrewMemberTerritory(Assistant) returned false");
                  return false;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): reached default name=" + switchingMember.Role);
               break;
         }
         //-----------------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_CREW_SWITCH, "SwitchMembers(): SetCrewMemberTerritory for role=" + switchingMember.Role);
         report.Assistant = switchingMember;
         report.Assistant.Role = "Assistant";
         if (false == SetCrewActionTerritory(report.Assistant)) // put SwitchedCrewMember in proper spot
         {
            Logger.Log(LogEnum.LE_ERROR, "SwitchMembers(): SetCrewMemberTerritory(Assistant) returned false");
            return false;
         }
         return true;
      }
      public void ClearCrewActions(string caller)
      {
         IMapItems removals = new MapItems();
         foreach( IMapItem mi in this.CrewActions ) // do not want to remove crewmembers that are shown as incapacitated
         {
            if (true == mi.Name.Contains("_"))
               removals.Add(mi);
         }
         foreach (IMapItem mi in removals)
         {
            this.CrewActions.Remove(mi);
            Logger.Log(LogEnum.LE_SHOW_MAPITEM_CREWACTION, "ClearCrewActions(): "+ caller +  ": --------------------removing ca=" + mi.Name);
         }
      }
      //---------------------------------------------------------------
      public string GetGunLoadType()
      {
         foreach (IMapItem mi in this.GunLoads )
         {
            if( true == mi.Name.Contains("GunLoadInGun"))
            {
               if (true == mi.TerritoryCurrent.Name.Contains("Hvap"))
                  return "Hvap";
               if (true == mi.TerritoryCurrent.Name.Contains("He"))
                  return "He";
               if (true == mi.TerritoryCurrent.Name.Contains("Ap"))
                  return "Ap";
               if (true == mi.TerritoryCurrent.Name.Contains("Wp"))
                  return "Wp";
               if (true == mi.TerritoryCurrent.Name.Contains("Hbci"))
                  return "Hbci";
            }
         }
         return "None";
      }
      public bool SetGunLoadTerritory(string ammoType)
      {
         IAfterActionReport? lastReport = this.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetGunLoadTerritory(): lastReport=null");
            return false;
         }
         string tName = "GunLoad" + ammoType;
         string tType = lastReport.TankCardNum.ToString();
         ITerritory? newT = Territories.theTerritories.Find(tName, tType);
         if( null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetGunLoad(): unable to find territory name=" + tName + " tType=" + tType);
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_GUN_LOAD_PREP, "SetGunLoadTerritory(): Setting gun load in t=" + tName + " tType=" + tType);
         foreach (IMapItem mi in this.GunLoads)
         {
            if (true == mi.Name.Contains("GunLoadInGun"))
            {
               Logger.Log(LogEnum.LE_SHOW_GUN_LOAD_PREP, "SetGunLoadTerritory(): mi=" + mi.Name + " t=" + tName + " x=" + mi.Location.X.ToString() + " y=" + mi.Location.Y.ToString());
               mi.TerritoryCurrent = newT;
               double offset = mi.Zoom * Utilities.theMapItemOffset;
               mi.Location.X = newT.CenterPoint.X - offset;
               mi.Location.Y = newT.CenterPoint.Y - offset;
               return true;
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "SetGunLoad(): reached default");
         return false;
      }
      public string GetAmmoReloadType()
      {
         foreach (IMapItem mi in this.GunLoads)
         {
            if (true == mi.Name.Contains("AmmoReload"))
            {
               if (true == mi.TerritoryCurrent.Name.Contains("Hvap"))
                  return "Hvap";
               if (true == mi.TerritoryCurrent.Name.Contains("He"))
                  return "He";
               if (true == mi.TerritoryCurrent.Name.Contains("Ap"))
                  return "Ap";
               if (true == mi.TerritoryCurrent.Name.Contains("Wp"))
                  return "Wp";
               if (true == mi.TerritoryCurrent.Name.Contains("Hbci"))
                  return "Hbci";
            }
         }
         return "None";
      }
      public bool ReloadGun()
      {
         IAfterActionReport? lastReport = this.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReloadGun(): lastReport=null");
            return false;
         }
         //----------------------------------
         IMapItems removals = new MapItems();
         foreach( IMapItem mi in this.GunLoads)
         {
            if( true == mi.Name.Contains("AmmoReload") )
               removals.Add(mi);
         }
         foreach (IMapItem mi in removals)
            this.GunLoads.Remove(mi);
         //----------------------------------
         string gunLoadType = this.GetGunLoadType();  // This is the ammo that fired
         Logger.Log(LogEnum.LE_SHOW_GUN_RELOAD, "ReloadGun(): Gun Loaded with " + gunLoadType);
         int ammoCount = 0;
         switch (gunLoadType) // decrease on AAR the type of ammunition used
         {
            case "He": ammoCount = lastReport.MainGunHE; break;
            case "Ap": ammoCount = lastReport.MainGunAP; break;
            case "Hvap": ammoCount = lastReport.MainGunHVAP; break;
            case "Hbci": ammoCount = lastReport.MainGunHBCI; break;
            case "Wp":  ammoCount = lastReport.MainGunWP; break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ReloadGun(): 1-ReloadGun() reached default gunload=" + gunLoadType);
               return false;
         }
         //----------------------------------
         int readyRackLoadCount = this.GetReadyRackReload(gunLoadType);
         int maxReadyRackLoadCount = ammoCount - 1; // this ammo is loaded in the gun - the ready rack must be one less than ammo count
         if (maxReadyRackLoadCount <= readyRackLoadCount) // pull ammo from ready rack if ammo count less to ready rack
         {
            Logger.Log(LogEnum.LE_SHOW_GUN_RELOAD, "ReloadGun(): Setting readyRackLoadCount=" + readyRackLoadCount.ToString() + "--> ammoCount=" + maxReadyRackLoadCount.ToString());
            if (false == this.SetReadyRackReload(gunLoadType, maxReadyRackLoadCount))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReloadGun(): 2-SetReadyRackReload() returned false");
               return false;
            }
         }
         return true;
      }
      public bool FireAndReloadGun()
      {
         IAfterActionReport? lastReport = this.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Fire_AndReloadGun(): lastReport=null");
            return false;
         }
         //-----------------------------------------------
         if (null != TargetMainGun)
         {
            this.TargetMainGun.NumOfAcquiredMarker++;  // Fire_AndReloadGun() - Increase when firing at a target
            Logger.Log(LogEnum.LE_SHOW_NUM_SHERMAN_SHOTS, "Fire_AndReloadGun(): +++NumOfAcquiredMarker=" + this.TargetMainGun.NumOfAcquiredMarker.ToString());
         }
         //-----------------------------------------------
         string gunLoad = this.GetGunLoadType();  // This is the ammo that fired
         Logger.Log(LogEnum.LE_SHOW_GUN_RELOAD, "Fire_AndReloadGun(): Gun Load That Was Fired is " + gunLoad);
         switch (gunLoad) // decrease on AAR the type of ammunition used
         {
            case "He": --lastReport.MainGunHE; break;
            case "Ap": --lastReport.MainGunAP; break;
            case "Hvap": --lastReport.MainGunHVAP; break;
            case "Hbci": --lastReport.MainGunHBCI; break;
            case "Wp": --lastReport.MainGunWP; break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "Fire_AndReloadGun(): 1-ReloadGun() reached default gunload=" + gunLoad );
               return false;
         }
         //-----------------------------------------------
         string ammoReloadType = this.GetAmmoReloadType(); // This is the ammo loaded into gun after firing
         Logger.Log(LogEnum.LE_SHOW_GUN_RELOAD, "Fire_AndReloadGun(): Loading Gun with this Ammo  ----->" + ammoReloadType + "<-------------");
         int ammoCount = 0;
         switch (ammoReloadType) // get count of what is still available
         {
            case "He": ammoCount = lastReport.MainGunHE;   break;
            case "Ap": ammoCount = lastReport.MainGunAP;   break;
            case "Hvap": ammoCount = lastReport.MainGunHVAP; break;
            case "Hbci": ammoCount = lastReport.MainGunHBCI; break;
            case "Wp": ammoCount = lastReport.MainGunWP;   break;
            case "None": ammoCount = 0; break;
            default: Logger.Log(LogEnum.LE_ERROR, "Fire_AndReloadGun(): 2-Reached default ammoReloadType=" + ammoReloadType ); return false;
         }
         //-----------------------------------------------
         if (0 == ammoCount) // if count is zero, the Gun Ammo just fired is same as Reload Counter - shoudl be no ammo markers of any kind
         {
            this.GunLoads.Clear(); // Ready Rack should already be at zero
            Logger.Log(LogEnum.LE_SHOW_GUN_RELOAD, "Fire_AndReloadGun(): Clearing all Gun Load Markers ammoCount=" + ammoCount.ToString());
         }
         else if (0 < ammoCount) // if count is one, there are no reloads left
         {
            if (false == this.SetGunLoadTerritory(ammoReloadType)) // The Gun Load becomes the Ammo Reload after firing
            {
               Logger.Log(LogEnum.LE_ERROR, "Fire_AndReloadGun(): SetGunLoadTerritory() returned error for ammoReload=" + ammoReloadType);
               return false;
            }
            int readyRackLoadCount = this.GetReadyRackReload(ammoReloadType);
            int maxReadyRackLoad = ammoCount - 1;
            if (maxReadyRackLoad <= readyRackLoadCount) // pull ammo from ready rack if ammo count equal to ready rack
            {
               Logger.Log(LogEnum.LE_SHOW_GUN_RELOAD, "Fire_AndReloadGun(): Setting readyRackLoadCount=" + readyRackLoadCount.ToString() + "--> ammoCount=" + maxReadyRackLoad.ToString() );
               if (false == this.SetReadyRackReload(ammoReloadType, maxReadyRackLoad))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Fire_AndReloadGun(): 2-SetReadyRackReload() returned false");
                  return false;
               }
            }
            else
            {
               if (true == this.IsReadyRackReload()) // decrease the ready rack by the type of ammunition used
               {
                  Logger.Log(LogEnum.LE_SHOW_GUN_RELOAD, "Fire_AndReloadGun(): Loading from Ready Rack  readyRackLoadCount=" + readyRackLoadCount.ToString());
                  if (readyRackLoadCount < 1)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Fire_AndReloadGun(): GetReadyRackReload() returned 1 > load=" + readyRackLoadCount.ToString() + " for gunLoad=" + gunLoad);
                     return false;
                  }
                  readyRackLoadCount--;
                  if (false == this.SetReadyRackReload(ammoReloadType, readyRackLoadCount))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Fire_AndReloadGun(): SetReadyRackReload() returned false");
                     return false;
                  }
               }
            }
         }
         //---------------------------------------
         if (1 == ammoCount) // there is one ammo in the gun tube - there is no reload markers
         {
            Logger.Log(LogEnum.LE_SHOW_GUN_RELOAD, "Fire_AndReloadGun(): ammoCount=1 Removing AmmoReload Marker");
            foreach (IMapItem mi in this.GunLoads) // Remove the ammo reload marker
            {
               if (true == mi.Name.Contains("AmmoReload"))
               {
                  this.GunLoads.Remove(mi);
                  break;
               }
            }
            if (false == this.SetReadyRackReload(ammoReloadType, 0))
            {
               Logger.Log(LogEnum.LE_ERROR, "Fire_AndReloadGun(): 3-SetReadyRackReload() returned false");
               return false;
            }
         }
         else if ("None" != ammoReloadType)
         {
            if (0 == this.GetReadyRackReload(ammoReloadType))
            {
               Logger.Log(LogEnum.LE_SHOW_GUN_RELOAD, "Fire_AndReloadGun(): readyrack=0 for ammoReloadType=" + ammoReloadType + " causing swithing over to different AmmoReload Marker");
               foreach (IMapItem mi in this.GunLoads)
               {
                  if (true == mi.Name.Contains("ReadyRackAmmoReload"))
                  {
                     IMapItem ammoLoad = new MapItem("AmmoReload", 1.0, "c29AmmoReload", mi.TerritoryCurrent);
                     ammoLoad.Location.X = mi.Location.X;
                     ammoLoad.Location.Y = mi.Location.Y;
                     this.GunLoads.Add(ammoLoad);
                     this.GunLoads.Remove(mi);
                     break;
                  }
               }
            }
         }
         return true;
      }
      public bool IsReadyRackReload()
      {
         foreach (IMapItem mi in this.GunLoads)
         {
            if (true == mi.Name.Contains("ReadyRackAmmoReload"))
            {
               ITerritory t = mi.TerritoryCurrent;
               if (true == t.Name.Contains("GunLoadHe"))
               {
                  if (0 < GetReadyRackReload("He"))
                     return true;
                  else
                     return false;
               }
               if ( true == t.Name.Contains("GunLoadAp") )
               {
                  if (0 < GetReadyRackReload("Ap"))
                     return true;
                  else
                     return false;
               }
               if (true == t.Name.Contains("GunLoadHvap"))
               {
                  if (0 < GetReadyRackReload("Hvap"))
                     return true;
                  else
                     return false;
               }
               if (true == t.Name.Contains("GunLoadWp"))
               {
                  if (0 < GetReadyRackReload("Wp"))
                     return true;
                  else
                     return false;
               }
               if (true == t.Name.Contains("GunLoadHbci"))
               {
                  if (0 < GetReadyRackReload("Hbci"))
                     return true;
                  else
                     return false;
               }
            }
         }
         return false;
      }
      public int GetReadyRackReload(string ammoType)
      {
         foreach (IMapItem mi in this.ReadyRacks)
         {
            if (true == mi.Name.Contains(ammoType))
               return mi.Count;
         }
         return 0;
      }
      public int GetReadyRackTotalLoad()
      {
         int total = 0;
         foreach (IMapItem mi in this.ReadyRacks)
            total += mi.Count;
         return total;
      }
      public bool SetReadyRackReload(string ammoType, int value)
      {
         IAfterActionReport? lastReport = this.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetReadyRackReload(): lastReport=null");
            return false;
         }
         IMapItem? rrMarker = null;
         foreach ( IMapItem mi in this.ReadyRacks )
         {
            if( true == mi.Name.Contains(ammoType))
            {
               rrMarker = mi;
               break;
            }
         }
         if (null == rrMarker)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetReadyRackReload(): rrMarker=null for ammoType=" + ammoType);
            return false;
         }
         rrMarker.Count = value;
         //-------------------------------------------------
         string tName = rrMarker.Name + rrMarker.Count.ToString();
         string tType = lastReport.TankCardNum.ToString();
         ITerritory? newT = Territories.theTerritories.Find(tName, tType);
         if (null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetReadyRackReload(): newT=null for " + tName);
            return false;
         }
         rrMarker.TerritoryCurrent = newT;
         double offset = rrMarker.Zoom * Utilities.theMapItemOffset;
         rrMarker.Location.X = newT.CenterPoint.X - offset;
         rrMarker.Location.Y = newT.CenterPoint.Y - offset;
         return true;
      }
      //---------------------------------------------------------------
      public bool IsDaylightLeft(IAfterActionReport report)
      {
         if (report.SunsetHour < report.SunriseHour)
            return false;
         if (report.SunsetHour == report.SunriseHour)
         {
            if (report.SunsetMin <= report.SunriseMin)
               return false;
         }
         return true;
      }
      public bool IsStartArea(out bool isStartArea)
      {
         isStartArea = false;
         IMapItem? startArea = MoveStacks.FindMapItem("StartArea");
         if (null == startArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsStartArea(): startArea=null");
            return false;
         }
         if (0 == startArea.TerritoryCurrent.Adjacents.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsStartArea(): startArea.TerritoryCurrent.Adjacents.Count=0");
            return false;
         }
         string adjName = startArea.TerritoryCurrent.Adjacents[0];
         if (null == adjName)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsStartArea(): adjName=null");
            return false;
         }
         if (null == EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsStartArea(): EnteredArea=null");
            return false;
         }
         if (adjName == EnteredArea.Name)
            isStartArea = true;
         return true;
      }
      public bool IsExitArea(out bool isExitAreaReached)
      {
         isExitAreaReached = false;
         IMapItem? exitArea = MoveStacks.FindMapItem("ExitArea");
         if (null == exitArea)
            return false;
         if (0 == exitArea.TerritoryCurrent.Adjacents.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsExitArea(): exitArea.TerritoryCurrent.Adjacents.Count=0");
            return false;
         }
         string adjName = exitArea.TerritoryCurrent.Adjacents[0];
         if (null == adjName)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsExitArea(): adjName=null");
            return false;
         }
         if (null == EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsExitArea(): EnteredArea=null");
            return false;
         }
         if (adjName == EnteredArea.Name)
            isExitAreaReached = true;
         return true;
      }
      public void KillSherman(IAfterActionReport report, string reason)
      {
         this.Sherman.IsKilled = true;
         this.Sherman.IsMoving = false;
         report.KnockedOut = reason;
         report.VictoryPtsFriendlyTank++;
      }
      public void ScoreYourVictoryPoint(IAfterActionReport report, IMapItem enemy)
      {
         string enemyUnit = enemy.GetEnemyUnit();
         switch (enemyUnit)
         {
            case "LW":
            case "MG":
               report.VictoryPtsYourKiaLightWeapon++;
               break;
            case "ATG":
            case "Pak43":
            case "Pak38":
            case "Pak40":
               report.VictoryPtsYourKiaAtGun++;
               break;
            case "TRUCK":
               report.VictoryPtsYourKiaTruck++;
               break;
            case "PSW":
            case "SPW":
               report.VictoryPtsYourKiaSpwOrPsw++;
               break;
            case "PzIV":
               report.VictoryPtsYourKiaPzIV++;
               break;
            case "PzV":
               report.VictoryPtsYourKiaPzV++;
               break;
            case "TANK":
            case "PzVIe":
            case "PzVIb":
               report.VictoryPtsYourKiaPzVI++;
               break;
            case "SPG":
            case "STuGIIIg":
            case "MARDERII":
            case "MARDERIII":
            case "JdgPzIV":
            case "JdgPz38t":
               report.VictoryPtsYourKiaSPGun++;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ScoreYourVictoryPoint(): reached default with enemyUnit=" + enemyUnit);
               break;
         }
         if (true == enemy.IsFortification)
            report.VictoryPtsYourKiaFortifiedPosition++;
      }
      public void ScoreFriendlyVictoryPoint(IAfterActionReport report, IMapItem enemy)
      {
         string enemyUnit = enemy.GetEnemyUnit();
         switch (enemyUnit)
         {
            case "LW":
            case "MG":
               report.VictoryPtsFriendlyKiaLightWeapon++;
               Logger.Log(LogEnum.LE_SHOW_VP_FRIENDLY_FORCES, "ScoreFriendlyVictoryPoint(): LW|MG #=" + report.VictoryPtsFriendlyKiaLightWeapon.ToString());
               break;
            case "ATG":
            case "Pak43":
            case "Pak38":
            case "Pak40":
               report.VictoryPtsFriendlyKiaAtGun++;
               Logger.Log(LogEnum.LE_SHOW_VP_FRIENDLY_FORCES, "ScoreFriendlyVictoryPoint(): ATG #=" + report.VictoryPtsFriendlyKiaAtGun.ToString());
               break;
            case "TRUCK":
               report.VictoryPtsFriendlyKiaTruck++;
               Logger.Log(LogEnum.LE_SHOW_VP_FRIENDLY_FORCES, "ScoreFriendlyVictoryPoint(): Trk #=" + report.VictoryPtsFriendlyKiaTruck.ToString());
               break;
            case "PSW":
            case "SPW":
               report.VictoryPtsFriendlyKiaSpwOrPsw++;
               Logger.Log(LogEnum.LE_SHOW_VP_FRIENDLY_FORCES, "ScoreFriendlyVictoryPoint(): Spw or Psw #=" + report.VictoryPtsFriendlyKiaSpwOrPsw.ToString());
               break;
            case "PzIV":
               report.VictoryPtsFriendlyKiaPzIV++;
               Logger.Log(LogEnum.LE_SHOW_VP_FRIENDLY_FORCES, "ScoreFriendlyVictoryPoint(): PzIV #=" + report.VictoryPtsFriendlyKiaPzIV.ToString());
               break;
            case "PzV":
               report.VictoryPtsFriendlyKiaPzV++;
               Logger.Log(LogEnum.LE_SHOW_VP_FRIENDLY_FORCES, "ScoreFriendlyVictoryPoint(): PzV #=" + report.VictoryPtsFriendlyKiaPzV.ToString());
               break;
            case "TANK":
            case "PzVIe":
            case "PzVIb":
               report.VictoryPtsFriendlyKiaPzVI++;
               Logger.Log(LogEnum.LE_SHOW_VP_FRIENDLY_FORCES, "ScoreFriendlyVictoryPoint(): " + enemyUnit + " #=" + report.VictoryPtsFriendlyKiaPzVI.ToString());
               break;
            case "SPG":
            case "STuGIIIg":
            case "MARDERII":
            case "MARDERIII":
            case "JdgPzIV":
            case "JdgPz38t":
               report.VictoryPtsFriendlyKiaSPGun++;
               Logger.Log(LogEnum.LE_SHOW_VP_FRIENDLY_FORCES, "ScoreFriendlyVictoryPoint(): " + enemyUnit + " #=" + report.VictoryPtsFriendlyKiaSPGun.ToString());
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ScoreFriendlyVictoryPoint(): reached default with enemyUnit=" + enemyUnit);
               break;
         }
         if (true == enemy.IsFortification)
            report.VictoryPtsYourKiaFortifiedPosition++;
      }
   }
}

