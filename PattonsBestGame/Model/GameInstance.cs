
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Pattons_Best
{
   [XmlRootAttribute("GameInstance",IsNullable = false)]
   [Serializable]
   public class GameInstance : IGameInstance
   {
      [NonSerialized] static public Logger Logger = new Logger();
      public bool IsTalkRoll { get; set; } = false;
      public Options Options { get; set; } = new Options();
      public GameStat Statistic { get; set; } = new GameStat();
      //------------------------------------------------
      public bool CtorError { get; } = false;
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
         //------------------------------------------------------------------------------------
         ITerritory territory = Territory.theTerritories.Find("0101");
         myPrince= new MapItem("Prince", 1.0, false, false, false, "c07Prince", "c07Prince", territory, 9, 8, 0);
         PartyMembers.Add(myPrince);
      }
      public GameInstance(Options newGameOptions) // Constructor - set log levels
      {
         this.Options = newGameOptions;
      }
      //----------------------------------------------
      public bool IsGridActive { set; get; } = false;
      //----------------------------------------------
      public int GameTurn { get; set; } = 0;
      public bool IsUndoCommandAvailable { set; get; } = false;
      public GamePhase GamePhase { get; set; } = GamePhase.GameSetup;
      public GameAction DieRollAction { get; set; } = GameAction.DieRollActionNone;
      //---------------------------------------------------------------
      public IMapItemMoves MapItemMoves { get; set; } = new MapItemMoves();
      public IMapItemMove PreviousMapItemMove { get; set; } = new MapItemMove();
      //---------------------------------------------------------------
      public IStacks Stacks { get; set; } = new Stacks();
      [NonSerialized] private List<IUnitTest> myUnitTests = new List<IUnitTest>();
      public List<IUnitTest> UnitTests { get => myUnitTests; }
      //---------------------------------------------------------------
      private Dictionary<string, int[]> myDieResults = new Dictionary<string, int[]>();
      public Dictionary<string, int[]> DieResults { get => myDieResults; }
      //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
      //---------------------------------------------------------------
      public ITerritories ReadTerritoriesXml()
      {
         ITerritories territories = new Territories();
         XmlTextReader reader = null;
         try
         {
            // Load the reader with the data file and ignore all white space nodes.
            string filename = ConfigFileReader.theConfigDirectory + "Territories.xml";
            reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
            while (reader.Read())
            {
               if (reader.Name == "Territory")
               {
                  if (reader.IsStartElement())
                  {
                     string name = reader.GetAttribute("value");
                     Territory t = new Territory(name);
                     reader.Read(); // read the type
                     string typeOfTerritory = reader.GetAttribute("value");
                     t.Type = typeOfTerritory;
                     reader.Read(); // read the center point
                     string value = reader.GetAttribute("X");
                     Double X = Double.Parse(value);
                     value = reader.GetAttribute("Y");
                     Double Y = Double.Parse(value);
                     t.CenterPoint = new MapPoint(X, Y);
                     while (reader.Read())
                     {
                        if ((reader.Name == "adjacent" && (reader.IsStartElement())))
                        {
                           value = reader.GetAttribute("value");
                           t.Adjacents.Add(value);
                        }
                        else if ((reader.Name == "regionPoint" && (reader.IsStartElement())))
                        {
                           value = reader.GetAttribute("X");
                           Double X1 = Double.Parse(value);
                           value = reader.GetAttribute("Y");
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
            } // end while
            return territories;
         } // try
         catch (Exception e)
         {
            Console.WriteLine("ReadTerritoriesXml(): Exception:  e.Message={0} while reading reader.Name={1}", e.Message, reader.Name);
            return territories;
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder("[");
         sb.Append("t=");
         sb.Append(this.GameTurn.ToString());
         sb.Append(",p=");
         sb.Append(this.GamePhase.ToString());
         sb.Append(",c=");
         sb.Append(this.SunriseChoice.ToString());
         sb.Append(",st=");
         sb.Append(this.Prince.TerritoryStarting.Name);
         sb.Append(",t=");
         sb.Append(this.Prince.Territory.Name);
         sb.Append("]");
         return sb.ToString();
      }
   }
}

