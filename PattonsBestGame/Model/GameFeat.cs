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
      public static string[] theDefaults = new string[32]
      {
         "End Game from Tank Explosion",
         "End Game from Commander Wounds",
         "End Game from Commander KIA in Brew up",
         "End Campaign Game",
         "Win Campaign Game",
         "Receive Purple Heart",
         "Receive Bronze Star",
         "Receive Silver Star",
         "Receive Distinguished Cross",
         "Receive Medal of Honor",
         "Kill Light Infantry",
         "Kill Machine Gun Squad",
         "Kill Truck",
         "Kill PzIV Tank",
         "Kill PzV Tank",
         "Kill PzVIe (Tiger) Tank",
         "Kill PzVIb Tank",
         "Kill Marder II SPG",
         "Kill Marder III SPG",
         "Kill STuGIIIg SPG",
         "Kill JgdPzIV SPG",
         "Kill JgdPz38t SPG",
         "Kill PSW232 AFV",
         "Kill SPW251 AFV",
         "Kill Pak38 ATG",
         "Kill Pak40 ATG",
         "Kill Pak43 ATG",
         "Train on HVSS Use",
         "Successfully Repair Main Gun",
         "Successfully Repair Machine Gun",
         "Fire Mortar in Combat",
         "Throw Smoke from Hatch"
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
            Logger.Log(LogEnum.LE_ERROR, "GetFeatChange(): (feats.Count=" + feats.Count.ToString() + ") != (this.Count=" + this.Count.ToString() + ")");
            return false;
         }
         for (int i = 0; i < feats.Count; ++i)
         {
            GameFeat? right = feats[i];
            if (null == right)
            {
               Logger.Log(LogEnum.LE_ERROR, "GetFeatChange(): right=null for i=" + i.ToString());
               return false;
            }
            GameFeat? left = this[i];
            if (null == left)
            {
               Logger.Log(LogEnum.LE_ERROR, "GetFeatChange(): left=null for i=" + i.ToString());
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
         foreach (string s in theDefaults)
            Add(new GameFeat(s, 0));
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
