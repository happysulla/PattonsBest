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
      public int Threshold { get; set; } = 1;
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
         if( 0 < this.Value)
         {
            sb.Append("(k=");
            sb.Append(this.Key.ToString());
            sb.Append("->");
            sb.Append(this.Value.ToString());
            sb.Append(")");
         }
         return sb.ToString();
      }
      public static string GetFeatMessage(GameFeat feat)
      {
         switch(feat.Key)
         {
            case "EndGameExplode":  return "Tank Explodes Killing Crew";
            case "EndGameWounds":    return "Game Ends with Commander Killed";
            case "EndGameCmdrKilled":    return "Game Ends with Commander Killed";
            case "EndGame":    return "Completed a Campaign Game";
            case "EndGameWin":    return "Win a Campaign Game";
            //------------
            case "NumKillLwFriendlyFire": return "Kill " + feat.Value.ToString() + " LWs with Friendly Fire";
            case "NumKillMgFriendlyFire": return "Kill " + feat.Value.ToString() + " MGs with Friendly Fire";
            case "NumKillTruckFriendlyFire": return "Kill " + feat.Value.ToString() + " Trucks with Friendly Fire";
            case "NumKillPswFriendlyFire": return "Kill " + feat.Value.ToString() + " PSW 232 with Friendly Fire";
            case "NumKillSpwFriendlyFire": return "Kill " + feat.Value.ToString() + " SPW 251 with Friendly Fire";
            case "NumKillPzIVFriendlyFire": return "Kill " + feat.Value.ToString() + " Pz IV with Friendly Fire";
            case "NumKillPzVFriendlyFire": return "Kill " + feat.Value.ToString() + " Pz V with Friendly Fire";
            case "NumKillPzVIeFriendlyFire": return "Kill " + feat.Value.ToString() + " Pz VIe with Friendly Fire";
            case "NumKillPzVIbFriendlyFire": return "Kill " + feat.Value.ToString() + " Pz VIb with Friendly Fire";
            case "NumKillMarderIIFriendlyFire": return "Kill " + feat.Value.ToString() + " Marder II with Friendly Fire";
            case "NumKillMarderIIIFriendlyFire": return "Kill " + feat.Value.ToString() + " Marder III with Friendly Fire";
            case "NumKillSTuGIIIgFriendlyFire": return "Kill " + feat.Value.ToString() + " STuG IIIg with Friendly Fire";
            case "NumKillJgdPzIVFriendlyFire": return "Kill " + feat.Value.ToString() + " JgdPz IV with Friendly Fire";
            case "NumKillJgdPz38tFriendlyFire": return "Kill " + feat.Value.ToString() + " JgdPz 38t with Friendly Fire";
            case "NumKillPak38FriendlyFire": return "Kill " + feat.Value.ToString() + " Pak38 with Friendly Fire";
            case "NumKillPak40FriendlyFire": return "Kill " + feat.Value.ToString() + " Pak40 with Friendly Fire";
            case "NumKillPak43FriendlyFire": return "Kill " + feat.Value.ToString() + " Pak43 with Friendly Fire";
            //------------
            case "NumKillLwYourFire":  return "Kill " + feat.Value.ToString() + " LWs with your Tank";
            case "NumKillMgYourFire": return "Kill " + feat.Value.ToString() + " MGs with your Tank";
            case "NumKillTruckYourFire": return "Kill " + feat.Value.ToString() + " Trucks with your Tank";
            case "NumKillPswYourFire": return "Kill " + feat.Value.ToString() + " PSW 232 with your Tank";
            case "NumKillSpwYourFire": return "Kill " + feat.Value.ToString() + " SPW 251 with your Tank";
            case "NumKillPzIVYourFire": return "Kill " + feat.Value.ToString() + " Pz IV with your Tank";
            case "NumKillPzVYourFire": return "Kill " + feat.Value.ToString() + " Pz V with your Tank";
            case "NumKillPzVIeYourFire": return "Kill " + feat.Value.ToString() + " Pz VIe with your Tank";
            case "NumKillPzVIbYourFire": return "Kill " + feat.Value.ToString() + " Pz VIb with your Tank";
            case "NumKillMarderIIYourFire": return "Kill " + feat.Value.ToString() + " Marder II with your Tank";
            case "NumKillMarderIIIYourFire": return "Kill " + feat.Value.ToString() + " Marder III with your Tank";
            case "NumKillSTuGIIIgYourFire": return "Kill " + feat.Value.ToString() + " STuG IIIg with your Tank";
            case "NumKillJgdPzIVYourFire": return "Kill " + feat.Value.ToString() + " JgdPz IV with your Tank";
            case "NumKillJgdPz38tYourFire": return "Kill " + feat.Value.ToString() + " JgdPz 38t with your Tank";
            case "NumKillPak38YourFire": return "Kill " + feat.Value.ToString() + " Pak38 with your Tank";
            case "NumKillPak40YourFire": return "Kill " + feat.Value.ToString() + " Pak40 with your Tank";
            case "NumKillPak43YourFire": return "Kill " + feat.Value.ToString() + " Pak43 with your Tank";
            //------------
            case "NumPurpleHearts": return "Receive " + feat.Value.ToString() + " Purple Hearts";
            case "NumBronzeStars": return "Receive " + feat.Value.ToString() + " Bronze Stars";
            case "NumSilverStars": return "Receive " + feat.Value.ToString() + " Silver Stars";
            case "NumDistinguishedCrosses": return "Receive " + feat.Value.ToString() + " Distinguished Crosses";
            case "NumMedalOfHonors":    return "Receive " + feat.Value.ToString() +" Medal of Honor";
            //------------
            case "Hvss":    return "Use Main Gun when Moving";
            case "RepairMain":    return "Repair the Main Gun in Battle";
            case "RepairMg":    return "Repair a disabled MG in Battle";
            case "FireMortar":    return "Sherman Fires a Mortar";
            case "ThrowSmoke" :    return "Throw Smoke out of Tank Hatch";

            default:
               Logger.Log(LogEnum.LE_ERROR, "GetFeatMessage(): Unknown key=" + feat.Key);
               return feat.Key;
         }
      }
   }
   //========================================
   [XmlInclude(typeof(GameFeat))]
   [Serializable]
   public class GameFeats : IEnumerable
   {
      [NonSerialized]
      public static string[] theDefaults = 
      {
         "EndGameExplode",
         "EndGameWounds",
         "EndGameCmdrKilled", 
         "EndGame",
         "EndGameWin",
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
         "Hvss",
         "RepairMain",
         "RepairMg",
         "FireMortar",
         "ThrowSmoke"
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
      public bool GetFeatChange(GameFeats rightFeats, out GameFeat outFeat)
      {
         outFeat = new GameFeat();
         if (this.Count < rightFeats.Count) // sync up the two lists
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): (rightFeats.Count=" + rightFeats.Count.ToString() + ") > (this.Count=" + this.Count.ToString() + ")");
            return false;
         }
         if (rightFeats.Count < this.Count) // sync up the two lists
         {
            while (rightFeats.Count < this.Count)
            {
               Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Get_FeatChange(): (rightFeats.Count=" + rightFeats.Count.ToString() + ") <  (this.Count=" + this.Count.ToString() + ")");
               for (int i = 0; i < rightFeats.Count; ++i)
               {
                  GameFeat? right = rightFeats[i];
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
                  if (left.Key == right.Key)
                     continue;
                  rightFeats.Insert(i, right);
                  break;
               }
            }
         }
         //--------------------------------------------
         for (int i = 0; i < rightFeats.Count; ++i)
         {
            GameFeat? right = rightFeats[i];
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
            if (left.Key != right.Key)
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): left.key=" + left.Key + " right.key=" + right.Key);
               return false;
            }
            if (left.Value != right.Value)
            {
               Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Get_FeatChange(): Key=" + right.Key + " (left.Value=" + left.Value.ToString() + ") != (right.Value =" + right.Value.ToString() + ")");
               if( 0 == left.Value % left.Threshold) // when the value reaches an iterative threshold, show feat
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
            Add(new GameFeat(s));
      }
      public void SetValue(string key, int value)
      {
         GameFeat? o = Find(key);
         if (null == o)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetValue(): null for key=" + key);
            o = new GameFeat(key);
            this.myList.Add(o);
         }
         o.Value = value;
      }
      public void AddOne(string key)
      {
         GameFeat? o = Find(key);
         if (null == o)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddOne(): null for key=" + key);
            o = new GameFeat(key);
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
            GameFeat feat = (GameFeat)obj;
            sb.Append(feat.ToString());
         }
         sb.Append("]");
         return sb.ToString();
      }
   }
}
