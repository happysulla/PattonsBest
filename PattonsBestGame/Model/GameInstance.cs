
using System;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using Pattons_Best.Model;
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
      public bool IsHatchesActive { set; get; } = false;
      public bool IsPrepActive { set; get; } = false;
      public string EventActive { get; set; } = "e000";
      public string EventDisplayed { set; get; } = "e000";
      public string EventStart { set; get; } = "e000";
      public List<string> Events { set; get; } = new List<string>();
      private Dictionary<string, int[]> myDieResults = new Dictionary<string, int[]>();
      public Dictionary<string, int[]> DieResults { get => myDieResults; }
      //------------------------------------------------
      public int GameTurn { get; set; } = 0;
      public GamePhase GamePhase { get; set; } = GamePhase.GameSetup;
      public int Day { get; set; } = 0;
      public IAfterActionReports Reports { get; set; } = new AfterActionReports();
      public GameAction DieRollAction { get; set; } = GameAction.DieRollActionNone;
      public bool IsUndoCommandAvailable { set; get; } = false;
      public String EndGameReason { set; get; } = "";
      //---------------------------------------------------------------
      public IMapItems NewMembers { set; get; } = new MapItems();
      public IMapItems ReadyRacks { set; get; } = new MapItems();
      public IMapItems Hatches { set; get; } = new MapItems();
      public IMapItems GunLoads { set; get; } = new MapItems();
      public IMapItem? Turret { set; get; } = null;
      //------------------------------------------------
      public ITerritory Home { get; set; } = new Territory();
      public ITerritory? NewTerritory { set; get; } = null;
      public ITerritory? EnemyStrengthCheck { get; set; } = null;
      public ITerritory? ArtillerySupportCheck { get; set; } = null;
      public ITerritory? AirStrikeCheck { get; set; } = null;
      public ITerritory? EnteredArea { get; set; } = null;
      //---------------------------------------------------------------

      public bool IsHulledDown { set; get; } = false;
      public bool IsMoving { set; get; } = false;
      public bool IsLeadTank { set; get; } = false;
      public bool IsTurretActive { set; get; } = false;
      //------------------------------------------------
      public IMapItemMoves MapItemMoves { get; set; } = new MapItemMoves();
      public IStacks Stacks { get; set; } = new Stacks();
      private List<EnteredHex> myEnteredHexes = new List<EnteredHex>();
      public List<EnteredHex> EnteredHexes { get => myEnteredHexes; }
      //---------------------------------------------------------------
      [NonSerialized] private List<IUnitTest> myUnitTests = new List<IUnitTest>();
      public List<IUnitTest> UnitTests { get => myUnitTests; }
      //==============================================================
      public GameInstance() // Constructor - set log levels
      {
         if ( false == Logger.SetInitial()) // tsetup logger
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
            if( null == reader )
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
               this.Home = tHome;

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
         this.Options = newGameOptions;
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder("[");
         sb.Append("t=");
         sb.Append(this.GameTurn.ToString());
         sb.Append(",p=");
         sb.Append(this.GamePhase.ToString());
         sb.Append("]");
         return sb.ToString();
      }
      //---------------------------------------------------------------
      public ICrewMember? GetCrewMember(string name)
      {
         IAfterActionReport? report = this.Reports.GetLast();
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
   }
}

