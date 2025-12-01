using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Pattons_Best
{
   [Serializable]
   public class GameFeat
   {
      public string Key { get; set; } = string.Empty;
      public int Value { get; set; } = 0;
      public GameFeat()
      {
      }
      public GameFeat(string name)
      {
         Key = name;
      }
      public GameFeat(string name, int value)
      {
         Key = name;
         Value = value;
      }
      //----------------------------------
      public GameFeat Clone()
      {
         return new GameFeat(this.Key, this.Value);
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
   [XmlInclude(typeof(GameFeat))]
   [Serializable]
   public class GameFeats : IEnumerable
   {
      [NonSerialized]
      public static string[,] theDefaults = 
      {
         {"EndGameExplode", "Game ended due to tank explosion killing crew" },
         {"EndGameWounds", "Game ended due to Commander dieing from wounds" },
         {"EndGameCmdrKilled", "Game ended due to Commander KIA in Brew up"},
         {"EndGame", "Reached the end of a campaign game"},
         {"EndGameWin", "Scored positive points an end of campaign"},
         {"DecorPurpleHeart", "Commander received Purple Heart"},
         {"DecorPurpleBronze", "Commander received Bronze Star"},
         {"DecorPurpleSilver", "Commander received Silver Star"},
         {"DecorPurpleCross", "Commander received Distinguished Cross"},
         {"DecorHonor", "Commander received Medal of Honor"},
         {"KillLw", "Killed Light Infantry"},
         {"KillMg", "Killed Machine Gun Squad"},
         {"KillTruck", "Killed Truck"},
         {"KillPzIV", "Killed PzIV Tank"},
         {"KillPzV", "Killed PzV Tank"},
         {"KillPzVIe", "Killed PzVIe (Tiger) Tank"},
         {"KillPzVIb", "Killed PzVIb Tank"},
         {"KillMII", "Killed Marder II SPG"},
         {"KillMIII", "Killed Marder III SPG"},
         {"KillStug", "Killed STuGIIIg SPG"},
         {"KillLJgdPzIV", "Killed JgdPzIV SPG"},
         {"KillJgdPz38t", "Killed JgdPz38t SPG"},
         {"KillPSW", "Killed PSW232 AFV"},
         {"KillSPW", "Killed SPW251 AFV"},
         {"KillPak38", "Killed Pak38 ATG"},
         {"KillPak40", "Killed Pak40 ATG"},
         {"KillPak43", "Killed Pak43 ATG"},
         {"Hvss", "Crew trained on HVSS Use"},
         {"RepairMain", "Successfully repaired Main Gun"},
         {"RepairMg", "Successfully repaired Machine Gun"},
         {"FireMortar", "Fired Mortar in Combat"},
         {"ThrowSmoke", "Threw Smoke from Hatch"}
      };
      [NonSerialized] public static string theGameFeatDirectory = "";
      private readonly ArrayList myList;
      public GameFeats() { myList = new ArrayList(); }
      public int Count { get => myList.Count; }
      public void Add(GameFeat o) { myList.Add(o); }
      public void Add(object o) { myList.Add(o); }
      public void Insert(int index, GameFeat o) { myList.Insert(index, o); }
      public void Clear() { myList.Clear(); }
      public bool Contains(GameFeat o) { return myList.Contains(o); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(GameFeat o) { return myList.IndexOf(o); }
      public GameFeat? Find(string key)
      {
         int i = 0;
         foreach (object o in myList)
         {
            GameFeat? feat = (GameFeat)o;
            if (null == feat)
               continue;
            if (key == feat.Key)
               return feat;
            ++i;
         }
         return null;
      }
      public GameFeat? RemoveAt(int index)
      {
         GameFeat? feat = myList[index] as GameFeat;
         myList.RemoveAt(index);
         return feat;
      }
      public GameFeat? this[int index]
      {
         get { GameFeat? o = myList[index] as GameFeat; return o; }
         set { myList[index] = value; }
      }
      public GameFeats Clone()
      {
         GameFeats copy = new GameFeats();
         foreach (object o in myList)
         {
            GameFeat feat = (GameFeat)o;
            GameFeat copyFeat = new GameFeat(feat.Key, feat.Value);
            copy.Add(copyFeat);
         }
         return copy;
      }
      public bool GetFeatChange(GameFeats feats, out GameFeat outFeat)
      {
         outFeat = new GameFeat();
         if (feats.Count != this.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): (feats.Count=" + feats.Count.ToString() + ") != (this.Count=" + this.Count.ToString() + ")");
            return false;
         }
         for (int i = 0; i < feats.Count; ++i)
         {
            GameFeat? right = feats[i];
            if (null == right)
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): right=null for i=" + i.ToString());
               return false;
            }
            GameFeat? left = this[i];
            if (null == left)
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): left=null for i=" + i.ToString());
               return false;
            }
            if (left.Value != right.Value)
            {
               right.Value = right.Value;
               outFeat = left;
               return true;
            }
         }
         return true;
      }
      public void SetOriginalGameFeats()
      {
         Clear();
         for (int row=0; row < theDefaults.Length; row++)
            Add(new GameFeat(theDefaults[row,0]));
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[");
         foreach (Object obj in myList)
         {
            GameFeat feat = (GameFeat)obj;
            sb.Append(feat.ToString());
         }
         sb.Append("]");
         return sb.ToString();
      }
   }
}
