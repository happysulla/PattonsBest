using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Pattons_Best
{
   [Serializable]
   public class GameStatistic
   {
      public string Key { get; set; } = string.Empty;
      public int Value { get; set; } = 0;
      public GameStatistic()
      {
      }
      public GameStatistic(string name)
      {
         Key = name;
      }
      public GameStatistic(string name, int value)
      {
         Key = name;
         Value = value;
      }
      //----------------------------------
      public GameStatistic Clone()
      {
         return new GameStatistic(this.Key, this.Value);
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("(k=");
         sb.Append(this.Key.ToString());
         sb.Append("->");
         sb.Append(this.Value.ToString());
         sb.Append(")");
         return sb.ToString();
      }
   }
   //========================================
   [XmlInclude(typeof(GameStatistic))]
   [Serializable]
   public class GameStatistics : IEnumerable // Must have prefix Num, Max, or Min
   {
      [NonSerialized]
      public static string[] theDefaults =
      {
         "NumDays",
         "NumGames",
         "NumWins",
         //------------
         "NumKillLwFriendlyFire",
         "NumKillMgFriendlyFire",
         "NumKillTruckFriendlyFire",
         "NumKillPswFriendlyFire",
         "NumKillSpwFriendlyFire",
         "NumKillPzIVFriendlyFire",
         "NumKillPzVFriendlyFire",
         "NumKillPzVIeFriendlyFire",
         "NumKillPzVIbFriendlyFire",
         "NumKillMarderIIFriendlyFire",
         "NumKillMarderIIIFriendlyFire",
         "NumKillSTuGIIIgFriendlyFire",
         "NumKillJgdPzIVFriendlyFire",
         "NumKillJgdPz38tFriendlyFire",
         "NumKillPak38FriendlyFire",
         "NumKillPak40FriendlyFire",
         "NumKillPak43FriendlyFire",
         //------------
         "NumKillLwYourFire",
         "NumKillMgYourFire",
         "NumKillTruckYourFire",
         "NumKillPswYourFire",
         "NumKillSpwYourFire",
         "NumKillPzIVYourFire",
         "NumKillPzVYourFire",
         "NumKillPzVIeYourFire",
         "NumKillPzVIbYourFire",
         "NumKillMarderIIYourFire",
         "NumKillMarderIIIYourFire",
         "NumKillSTuGIIIgYourFire",
         "NumKillJgdPzIVYourFire",
         "NumKillJgdPz38tYourFire",
         "NumKillPak38YourFire",
         "NumKillPak40YourFire",
         "NumKillPak43YourFire",
         //------------
         "NumPurpleHearts",
         "NumBronzeStars",
         "NumSilverStars",
         "NumDistinguishedCrosses",
         "NumMedalOfHonors",
         //------------
         "NumOfBattles",
         "NumOfKilledCrewman",
         "NumShermanExplodes",
         "NumShermanBurns",
         "NumShermanPenetration",
         "NumPanzerfaustAttack",
         "NumPanzerfaustDeath",
         "NumMineAttack",
         "NumMineImmobilization",
         //--------------
         "MaxRollsForAirSupport",
         "MaxRollsForArtillerySupport",
         "MaxDayBetweenCombat",
         "MaxEnemiesInOneBattle",
         "MaxRoundsOfCombat",
         "MaxCrewRatingWin",
         "MinCrewRatingWin"
      };
      [NonSerialized] public static string theGameStatisticsDirectory = "";
      public static string GetStatisticMessage(GameStatistic stat)
      {
         StringBuilder sb = new StringBuilder();
         switch (stat.Key)
         {
            case "EndCampaignGame":
               sb.Append("Complete campaign game ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "EndCampaignGameOnTime":
               sb.Append("Lasted all days of a campaign game ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "EndCampaignGameWin":
               sb.Append("Campaign game won ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "EndGameExplode":
               sb.Append("Game ends with tank explosion ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "EndGameCmdrKilled":
               sb.Append("Game ends with commander killed ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            //------------
            case "NumShermanExplodes":
               sb.Append("Sherman exploded ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumShermanBurns":
               sb.Append("Sherman brewup ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumShermanPenetration":
               sb.Append("Sherman killed by penetration ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            //------------
            case "HvssTrained":
               sb.Append("Trained on HVSS that allows firing when moving ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "RepairMain":
               sb.Append("Repair a disabled main gun in battle ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "RepairMg":
               sb.Append("Repair a disabled MG in battle ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "FireMortar":
               sb.Append("Sherman fires a mortar ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "ThrowSmoke":
               sb.Append("Throw smoke out of tank hatch ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            //------------
            case "NumKillLwFriendlyFire":
               sb.Append("Killed LW units with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMgFriendlyFire":
               sb.Append("Killed MG Teams with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillTruckFriendlyFire":
               sb.Append("Killed trucks with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPswFriendlyFire":
               sb.Append("Killed PSW232 AFVs with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillSpwFriendlyFire":
               sb.Append("Killed SPW251 AFVs with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzIVFriendlyFire":
               sb.Append("Killed PzIV Tanks with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVFriendlyFire":
               sb.Append("Killed PzV Tanks with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVIeFriendlyFire":
               sb.Append("Killed PzVIe Tanks with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVIbFriendlyFire":
               sb.Append("Killed PzVIb Tanks with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMarderIIFriendlyFire":
               sb.Append("Killed MarderII SPGs with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMarderIIIFriendlyFire":
               sb.Append("Killed MarderIII SPGs with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillSTuGIIIgFriendlyFire":
               sb.Append("Killed STuGIIIg SPGs with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillJgdPzIVFriendlyFire":
               sb.Append("Killed JgdPzIV SPGs with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillJgdPz38tFriendlyFire":
               sb.Append("Killed JgdPz38t SPGs with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak38FriendlyFire":
               sb.Append("Killed Pak38 ATGs with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak40FriendlyFire":
               sb.Append("Killed Pak40 ATGs with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak43FriendlyFire":
               sb.Append("Killed Pak43 ATGs with friendly fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            //------------
            case "NumKillLwYourFire":
               sb.Append("Killed LW units with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMgYourFire":
               sb.Append("Killed MG Teams with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillTruckYourFire":
               sb.Append("Killed trucks with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPswYourFire":
               sb.Append("Killed PSW232 AFVs with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillSpwYourFire":
               sb.Append("Killed SPW251 AFVs with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzIVYourFire":
               sb.Append("Killed PzIV Tanks with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVYourFire":
               sb.Append("Killed PzV Tanks with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVIeYourFire":
               sb.Append("Killed PzVIe Tanks with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVIbYourFire":
               sb.Append("Killed PzVIb Tanks with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMarderIIYourFire":
               sb.Append("Killed MarderII SPGs with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMarderIIIYourFire":
               sb.Append("Killed MarderIII SPGs with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillSTuGIIIgYourFire":
               sb.Append("Killed STuGIIIg SPGs with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillJgdPzIVYourFire":
               sb.Append("Killed JgdPzIV SPGs with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillJgdPz38tYourFire":
               sb.Append("Killed JgdPz38t SPGs with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak38YourFire":
               sb.Append("Killed Pak38 ATGs with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak40YourFire":
               sb.Append("Killed Pak40 ATGs with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak43YourFire":
               sb.Append("Killed Pak43 ATGs with your fire ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            //------------
            case "NumPurpleHearts":
               sb.Append("Receive Purple Heart medal ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumBronzeStars":
               sb.Append("Receive Bronze Star medal ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumSilverStars":
               sb.Append("Receive Siler Star medal ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumDistinguishedCrosses":
               sb.Append("Receive Distinguished Cross medal ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            case "NumMedalOfHonors":
               sb.Append("Receive Medal of Honor ");
               sb.Append(stat.Value.ToString());
               sb.Append(" times");
               return sb.ToString();
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetStatisticMessage(): Unknown key=" + stat.Key);
               return "UNKNOWN: " + stat.Key;
         }
      }
      private readonly ArrayList myList;
      public GameStatistics() { myList = new ArrayList(); }
      public int Count { get => myList.Count; }
      public void Add(GameStatistic o) { myList.Add(o); }
      public void Add(object o) { myList.Add(o); }
      public void Insert(int index, GameStatistic o) { myList.Insert(index, o); }
      public void Clear() { myList.Clear(); }
      public bool Contains(GameStatistic o) { return myList.Contains(o); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(GameStatistic o) { return myList.IndexOf(o); }
      public GameStatistic Find(string key)
      {
         int i = 0;
         foreach (object o in myList)
         {
            GameStatistic? stat1 = (GameStatistic)o;
            if (null == stat1)
               continue;
            if (key == stat1.Key)
               return stat1;
            ++i;
         }
         Logger.Log(LogEnum.LE_ERROR, "GameStatistics.Find(): null for key=" + key + " in " + this.ToString());
         GameStatistic stat = new GameStatistic(key);
         this.myList.Add(stat);
         return stat;
      }
      public GameStatistic? RemoveAt(int index)
      {
         GameStatistic? feat = myList[index] as GameStatistic;
         myList.RemoveAt(index);
         return feat;
      }
      public GameStatistic? this[int index]
      {
         get { GameStatistic? o = myList[index] as GameStatistic; return o; }
         set { myList[index] = value; }
      }
      public GameStatistics Clone()
      {
         GameStatistics copy = new GameStatistics();
         foreach (object o in myList)
         {
            GameStatistic stat = (GameStatistic)o;
            GameStatistic copyStat = new GameStatistic(stat.Key, stat.Value);
            copy.Add(copyStat);
         }
         return copy;
      }
      public void SetOriginalGameStatistics()
      {
         Clear();
         for (int i = 0; i < theDefaults.Length; i++)
            Add(new GameStatistic(theDefaults[i]));
      }
      public void SyncGameStatistics()
      {

      }
      public void SetValue(string key, int value)
      {
         GameStatistic? o = Find(key);
         if (null == o)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetValue(): null for key=" + key);
            o = new GameStatistic(key);
            this.myList.Add(o);
         }
         o.Value = value;
      }
      public void AddOne(string key)
      {
         GameStatistic? o = Find(key);
         if (null == o)
         {
            Logger.Log(LogEnum.LE_ERROR, "Add_One(): null for key=" + key);
            o = new GameStatistic(key);
            this.myList.Add(o);
         }
         o.Value++;
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[");
         foreach (Object obj in myList)
         {
            GameStatistic feat = (GameStatistic)obj;
            sb.Append(feat.ToString());
         }
         sb.Append("]");
         return sb.ToString();
      }
   }
}
