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
   public class GameStatistics : IEnumerable
   {
      [NonSerialized]
      public static string[] theDefaults =
      {
         "NumGames",
         "NumWins",
         "NumLostTanks",
         "NumOfBattles",
         "NumOfKilledCrewman",
         "NumOfEnemyKills",
         "NumKillLw",
         "NumKillMg",
         "NumKillTruck",
         "NumKillPzIV",
         "NumKillPzV",
         "NumKillPzVIe",
         "NumKillPzVIb",
         "NumKillMarderII",
         "NumKillMarderIII",
         "NumKillSTuGIIIg",
         "NumKillJgdPzIV",
         "NumKillJgdPz38t",
         "NumKillPsw232",
         "NumKillSpw251",
         "NumKillPak38",
         "NumKillPak40",
         "NumKillPak43",
         "NumPurpleHearts",
         "NumBronzeStars",
         "NumSilverStars",
         "NumDistinguishedCrosses",
         "NumMedalOfHonors",
         "DayMaxBetweenCombat",
         "MaxRollsForAirSupport",
         "MaxRollsForArtillerySupport"
      };
      [NonSerialized] public static string theGameStatisticsDirectory = "";
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
      public GameStatistic? Find(string key)
      {
         int i = 0;
         foreach (object o in myList)
         {
            GameStatistic? stat = (GameStatistic)o;
            if (null == stat)
               continue;
            if (key == stat.Key)
               return stat;
            ++i;
         }
         return null;
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
