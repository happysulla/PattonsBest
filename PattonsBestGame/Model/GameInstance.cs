
using System;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
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
      //---------------------------------------------------------------
      public IMapItems NewMembers { set; get; } = new MapItems();
      public IMapItems ReadyRacks { set; get; } = new MapItems();
      public IMapItems Hatches { set; get; } = new MapItems();
      public IMapItems CrewActions { set; get; } = new MapItems();
      public IMapItems GunLoads { set; get; } = new MapItems();
      public IMapItem Sherman { set; get; } = new MapItem("Sherman1", 2.0, "t001", new Territory());
      public IMapItems Targets { set; get; } = new MapItems();
      public IMapItem? Target { set; get; } = null;
      //------------------------------------------------
      public ITerritory Home { get; set; } = new Territory();
      public ITerritory? EnemyStrengthCheckTerritory { get; set; } = null;
      public ITerritory? ArtillerySupportCheck { get; set; } = null;
      public ITerritory? AirStrikeCheckTerritory { get; set; } = null;
      public ITerritory? EnteredArea { get; set; } = null;
      public ITerritory? AdvanceFire { get; set; } = null;
      public ITerritory? FriendlyAdvance { get; set; } = null;
      public ITerritory? EnemyAdvance { get; set; } = null;
      //---------------------------------------------------------------
      public bool IsHatchesActive { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsShermanFirstShot { set; get; } = false;
      public bool IsShermanFiring { set; get; } = false;
      public bool IsShermanFiringAtFront { set; get; } = false;
      public bool IsShermanDeliberateImmobilization { set; get; } = false;
      public bool IsShermanRepeatFire { set; get; } = false;
      public bool IsShermanRepeatFirePending { set; get; } = false; // change IsShermanRepeatFire = true after first To Kill roll
      public int NumOfShermanShot { set; get; } = 0;
      public bool IsBrokenMainGun { set; get; } = false;
      public bool IsBrokenGunsight { set; get; } = false;
      public Dictionary<string, bool> FirstShots { set; get; } = new Dictionary<string, bool>();
      public Dictionary<string, int> AcquiredShots { set; get; } = new Dictionary<string, int>();
      public List<ShermanAttack> ShermanHits { set; get; } = new List<ShermanAttack>();
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
      public bool IsBrokenMgAntiAircraft { set; get; } = false;
      public bool IsBrokenMgBow { set; get; } = false;
      public bool IsBrokenMgCoaxial { set; get; } = false;
      public bool IsBrokenMgSub { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsShermanTurretRotated { set; get; } = false;
      public double ShermanRotationTurretOld { set; get; } = 0.0;
      //---------------------------------------------------------------
      public bool IsLeadTank { set; get; } = false;
      public bool IsAirStrikePending { set; get; } = false;
      public bool IsAdvancingFireChosen { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsMinefieldAttack { set; get; } = false;
      public bool IsHarrassingFire { set; get; } = false;
      public bool IsFlankingFire { set; get; } = false;
      public bool IsEnemyAdvanceComplete { set; get; } = false;
      public int AdvancingFireMarkerCount { set; get; } = 0;
      //---------------------------------------------------------------
      public bool IsCommanderRescuePerformed { set; get; } = false;
      public bool IsPromoted { set; get; } = false;
      //---------------------------------------------------------------
      public int VictoryPtsTotalCampaign { get; set; } = 0;
      public int PromotionPointNum { get; set; } = 0;
      public int PromotionDay { get; set; } = -1;
      public int NumPurpleHeart { get; set; } = 0;
      //---------------------------------------------------------------
      public EnumResistance BattleResistance { set; get; } = EnumResistance.None;
      public Dictionary<string, bool> BrokenPeriscopes { set; get; } = new Dictionary<string, bool>();
      public ShermanDeath? Death { set; get; } = null;
      public PanzerfaustAttack? Panzerfaust { set; get; } = null;
      public int NumCollateralDamage { set; get; } = 0;
      //------------------------------------------------
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
            Logger.Log(LogEnum.LE_ERROR, "GameEngine(): SurnameMgr.InitNames() returned false");
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
      public ICrewMember? GetCrewMember(string name)
      {
         IAfterActionReport? report = Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetCrewMember(): report=null");
            return null;
         }
         ICrewMember? crewmember = null;
         switch (name)
         {
            case "Driver":
               crewmember = report.Driver;
               break;
            case "Assistant":
               crewmember = report.Assistant;
               break;
            case "Loader":
               crewmember = report.Loader;
               break;
            case "Gunner":
               crewmember = report.Gunner;
               break;
            case "Commander":
               crewmember = report.Commander;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetCrewMember(): reached default name=" + name);
               break;
         }
         return crewmember;
      }
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
         string tName = "GunLoad" + ammoType;
         ITerritory? newT = Territories.theTerritories.Find(tName);
         if( null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetGunLoad(): unable to find territory name=" + tName);
            return false;
         }
         foreach (IMapItem mi in this.GunLoads)
         {
            if (true == mi.Name.Contains("GunLoadInGun"))
            {
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
         //-----------------------------------------------
         string gunLoad = this.GetGunLoadType();
         switch (gunLoad) // decrease on AAR the type of ammunition used
         {
            case "He": --lastReport.MainGunHE; break;
            case "Ap": --lastReport.MainGunAP; break;
            case "Hvap": --lastReport.MainGunHVAP; break;
            case "Hbci": --lastReport.MainGunHBCI; break;
            case "Wp": --lastReport.MainGunWP; break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "FireMainGunAtEnemyUnits(): GetShermanToHitNumber() reached default gunload=" + gunLoad );
               return false;
         }
         //-----------------------------------------------
         string ammoReloadType = this.GetAmmoReloadType(); // look at the next ammunition to use
         int ammoCount = 0;
         switch (ammoReloadType) // get count of what is still available
         {
            case "He": ammoCount = lastReport.MainGunHE;   break;
            case "Ap": ammoCount = lastReport.MainGunAP;   break;
            case "Hvap": ammoCount = lastReport.MainGunHVAP; break;
            case "Hbci": ammoCount = lastReport.MainGunHBCI; break;
            case "Wp": ammoCount = lastReport.MainGunWP;   break;
            default: Logger.Log(LogEnum.LE_ERROR, "ReloadGun(): reached default gunload=" + ammoReloadType ); return false;
         }
         //-----------------------------------------------
         int readyRackLoadCount = this.GetReadyRackReload(ammoReloadType);
         if ( 0 == ammoCount ) // if count is zero, there is nothing in the gun until the gunner changes load
         {
            foreach(IMapItem mi in this.GunLoads )
            { 
               if( true == mi.Name.Contains("GunLoadInGun"))
               {
                  this.GunLoads.Remove( mi );
                  break;
               }
            }
            this.SetReadyRackReload(ammoReloadType, 0);
            readyRackLoadCount = 0;
         }
         else
         {
            if (false == this.SetGunLoadTerritory(ammoReloadType)) // The Gun Load becomes the Ammo Reload after firing
            {
               Logger.Log(LogEnum.LE_ERROR, "ReloadGun(): SetGunLoadTerritory() returned error for ammoReload=" + ammoReloadType);
               return false;
            }
            if (ammoCount <= readyRackLoadCount) // pull ammo from ready rack if ammo count equal to ready rack
            {
               if (false == this.SetReadyRackReload(ammoReloadType, ammoCount))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReloadGun(): SetReadyRackReload() returned false");
                  return false;
               }
            }
            else
            {
               if (true == this.IsReadyRackReload()) // decrease the ready rack by the type of ammunition used
               {
                  if (readyRackLoadCount < 1)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReloadGun(): GetReadyRackReload() returned 1 > load=" + readyRackLoadCount.ToString() + " for gunLoad=" + gunLoad);
                     return false;
                  }
                  readyRackLoadCount--;
                  if (false == this.SetReadyRackReload(ammoReloadType, readyRackLoadCount))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReloadGun(): SetReadyRackReload() returned false");
                     return false;
                  }
               }
            }
         }
         //---------------------------------------
         if( 0 == readyRackLoadCount)
         {
            foreach (IMapItem mi in this.GunLoads)
            {
               if (true == mi.Name.Contains("ReadyRackReload"))
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
         Logger.Log(LogEnum.LE_ERROR, "GetReadyRackReload(): reached default");
         return -100;
      }
      public bool SetReadyRackReload(string ammoType, int value)
      {
         IMapItem? rrMarker = null;
         foreach ( IMapItem mi in this.ReadyRacks)
         {
            if( true == mi.Name.Contains(ammoType))
            {
               rrMarker = mi;
               break;
            }
         }
         if (null == rrMarker)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetReadyRackReload(): rrMarker=null");
            return false;
         }
         rrMarker.Count = value;
         string tName = rrMarker.Name + rrMarker.Count.ToString();
         ITerritory? newT = Territories.theTerritories.Find(tName);
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
      public bool IsExitArea(out bool isExitAreaReached)
      {
         isExitAreaReached = false;
         IMapItem? exitArea = MoveStacks.FindMapItem("ExitArea");
         if (null == exitArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsExitArea(): exitArea=null");
            return false;
         }
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
   }
}

