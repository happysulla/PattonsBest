
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
      public IMapItemMoves MapItemMoves { get; set; } = new MapItemMoves();
      public IStacks Stacks { get; set; } = new Stacks();
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
            // Create the territories and the regions marking the territories.
            // Keep a list of Territories used in the game.  All the information 
            // of Territories is static and does not change.
            Territory.theTerritories = ReadTerritoriesXml();
            if (null == Territory.theTerritories)
            {
               Logger.Log(LogEnum.LE_ERROR, "GameInstance(): ReadTerritoriesXml() returned null");
               CtorError = true;
               return;
            }
         }
         catch (Exception e)
         {
            MessageBox.Show("Exception in GameEngine() e=" + e.ToString());
         }
      }
      public GameInstance(Options newGameOptions) // Constructor - set log levels
      {
         this.Options = newGameOptions;
      }
      //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
      public ITerritories ReadTerritoriesXml()
      {
         ITerritories territories = new Territories();
         string filename = ConfigFileReader.theConfigDirectory + "Territories.xml";
         try
         {
            // Load the reader with the data file and ignore all white space nodes.
            XmlTextReader reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
            if( null == reader )
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadTerritoriesXml(): reader=null;");
               return territories;
            }
            while (reader.Read())
            {
               if (reader.Name == "Territory")
               {
                  if (reader.IsStartElement())
                  {
                     string? name = reader.GetAttribute("value");
                     if( null == name )
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadTerritoriesXml(): Territory=null");
                        return null;
                     }
                     Territory t = new Territory(name);
                     reader.Read(); // read the type
                     string? typeOfTerritory = reader.GetAttribute("value");
                     if (null == typeOfTerritory)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadTerritoriesXml(): typeOfTerritory=null");
                        return null;
                     }
                     t.Type = typeOfTerritory;
                     reader.Read(); // read the center point
                     string? value = reader.GetAttribute("X");
                     if (null == value)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadTerritoriesXml(): X=null");
                        return null;
                     }
                     Double X = Double.Parse(value);
                     value = reader.GetAttribute("Y");
                     if (null == value)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadTerritoriesXml(): Y=null");
                        return null;
                     }
                     Double Y = Double.Parse(value);
                     t.CenterPoint = new MapPoint(X, Y);
                     while (reader.Read())
                     {
                        if ((reader.Name == "adjacent" && (reader.IsStartElement())))
                        {
                           value = reader.GetAttribute("value");
                           if (null == value)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "ReadTerritoriesXml(): adjacent=null");
                              return null;
                           }
                           t.Adjacents.Add(value);
                        }
                        else if ((reader.Name == "regionPoint" && (reader.IsStartElement())))
                        {
                           value = reader.GetAttribute("X");
                           if (null == value)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "ReadTerritoriesXml(): adjacent X=null");
                              return null;
                           }
                           Double X1 = Double.Parse(value);
                           value = reader.GetAttribute("Y");
                           if (null == value)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "ReadTerritoriesXml(): adjacent Y=null");
                              return null;
                           }
                           Double Y1 = Double.Parse(value);
                           t.Points.Add(new MapPoint(X1, Y1));
                        }
                        else
                        {
                           break;
                        }
                     }  // end while
                     territories.Add(t);
                  } // end if
               } // end if
            } // while (reader.Read())
            return territories;
         } // try
         catch (Exception e)
         {
            Console.WriteLine("ReadTerritoriesXml(): Exception:  e.Message={0} while reading filename={1}", e.Message, filename);
            return territories;
         }
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

