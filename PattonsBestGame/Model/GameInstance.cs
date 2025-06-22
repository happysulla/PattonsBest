
using System;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

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
      //---------------------------------------------------------------
      public IMapItems NewMembers { set; get; } = new MapItems();
      public IMapItems ReadyRacks { set; get; } = new MapItems();
      public IMapItems Hatches { set; get; } = new MapItems();
      public IMapItems CrewActions { set; get; } = new MapItems();
      public IMapItems GunLoads { set; get; } = new MapItems();
      public IMapItem Sherman { set; get; } = new MapItem("Sherman1", 2.0, "t001", new Territory());
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
      public bool IsTurretActive { set; get; } = false;
      public bool IsHatchesActive { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsHulledDown { set; get; } = false;
      public bool IsMoving { set; get; } = false;
      public bool IsLeadTank { set; get; } = false;
      public bool IsAirStrikePending { set; get; } = false;
      public bool IsAdvancingFireChosen { set; get; } = false;
      public bool IsShermanFiring { set; get; } = false;
      public bool IsShermanFiringAtFront { set; get; } = false;
      public bool IsBrokenMainGun { set; get; } = false;
      public bool IsBrokenGunsight { set; get; } = false;
      public bool IsBrokenMgCoaxial { set; get; } = false;
      public bool IsBrokenMgBow { set; get; } = false;
      public bool IsBrokenMgAntiAircraft { set; get; } = false;
      public bool IsBrokenMgSub { set; get; } = false;
      public bool IsCommanderRescuePerformed { set; get; } = false;
      //---------------------------------------------------------------
      public bool IsMinefieldAttack { set; get; } = false;
      public bool IsHarrassingFire { set; get; } = false;
      public bool IsFlankingFire { set; get; } = false;
      public bool IsEnemyAdvanceComplete { set; get; } = false;
      public bool IsPromoted { set; get; } = false;
      //---------------------------------------------------------------
      public int VictoryPtsTotalCampaign { get; set; } = 0;
      public int PromotionPointNum { get; set; } = 0;
      public int PromotionDay { get; set; } = -1;
      public int NumPurpleHeart { get; set; } = 0;
      //---------------------------------------------------------------
      public int AdvancingFireMarkerCount { set; get; } = 0;
      public EnumResistance BattleResistance { set; get; } = EnumResistance.None;
      public Dictionary<string, bool> BrokenPeriscopes { set; get; } = new Dictionary<string, bool>();
      public Dictionary<string, bool> FirstShots { set; get; } = new Dictionary<string, bool>();
      public Dictionary<string, int> AcquiredShots { set; get; } = new Dictionary<string, int>();
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

