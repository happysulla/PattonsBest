
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
namespace Pattons_Best
{
   [XmlRootAttribute("GameInstance",IsNullable = false)]
   [Serializable]
   public class GameInstance : IGameInstance
   {
      [NonSerialized] static public Logger Logger = new Logger();
      public bool CtorError { get; } = false;
      public Options Options { get; set; } = new Options();
      public GameStat Statistic { get; set; } = new GameStat();
      //------------------------------------------------
      public bool IsGridActive { set; get; } = false;
      public string EventActive { get; set; } = "e000";
      public string EventDisplayed { set; get; } = "e000";
      public string EventStart { set; get; } = "e000";
      public List<string> Events { set; get; } = new List<string>();
      private Dictionary<string, int[]> myDieResults = new Dictionary<string, int[]>();
      public Dictionary<string, int[]> DieResults { get => myDieResults; }
      //------------------------------------------------
      public int GameTurn { get; set; } = 0;
      public GamePhase GamePhase { get; set; } = GamePhase.GameSetup;
      public GameAction DieRollAction { get; set; } = GameAction.DieRollActionNone;
      public bool IsUndoCommandAvailable { set; get; } = false;
      public String EndGameReason { set; get; } = "";
      //---------------------------------------------------------------
      public IMapItems MapItems { set; get; } = new MapItems();
      public IMapItemMoves MapItemMoves { get; set; } = new MapItemMoves();
      public IStacks Stacks { get; set; } = new Stacks();
      //------------------------------------------------
      public ITerritory? NewTerritory { set; get; } = null;
      private List<EnteredHex> myEnteredHexes = new List<EnteredHex>();
      public List<EnteredHex> EnteredHexes { get => myEnteredHexes; }
      //---------------------------------------------------------------
      public int Day { get; set; } = 0;
      //---------------------------------------------------------------
      [NonSerialized] private List<IUnitTest> myUnitTests = new List<IUnitTest>();
      public List<IUnitTest> UnitTests { get => myUnitTests; }
      //------------------------------------------------
      public GameInstance() // Constructor - set log levels
      {
         if( false == Logger.SetInitial()) // tsetup logger
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
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadTerritoriesXml() exception=\n" + e.ToString());
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
   }
}

