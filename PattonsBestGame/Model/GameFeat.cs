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
         sb.Append("(k=");
         sb.Append(this.Key.ToString());
         sb.Append("->");
         sb.Append(this.Value.ToString());
         sb.Append(")");
         return sb.ToString();
      }
      public static string GetFeatMessage(GameFeat feat, bool isThreshold = false)
      {
         StringBuilder sb = new StringBuilder("Kill ");
         switch (feat.Key)
         {
            case "EndGameExplode":  return "Tank Explodes Killing Crew";
            case "EndGameWounds":    return "Game Ends with Commander Killed";
            case "EndGameCmdrKilled":    return "Game Ends with Commander Killed";
            case "EndGame":    return "Completed a Campaign Game";
            case "EndGameWin":    return "Win a Campaign Game";
            //------------
            case "NumKillLwFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " LW units with Friendly Fire");
               return sb.ToString();
            case "NumKillMgFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " MG Teams with Friendly Fire");
               return sb.ToString();
            case "NumKillTruckFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " Trucks with Friendly Fire");
               return sb.ToString();
            case "NumKillPswFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( "  PSW232s with Friendly Fire");
               return sb.ToString();
            case "NumKillSpwFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " SPW251s with Friendly Fire");
               return sb.ToString();
            case "NumKillPzIVFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " Pz IV tanks with Friendly Fire");
               return sb.ToString();
            case "NumKillPzVFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " Pz V tanks with Friendly Fire");
               return sb.ToString();
            case "NumKillPzVIeFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " Pz VIe tanks with Friendly Fire");
               return sb.ToString();
            case "NumKillPzVIbFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " Pz VIb tanks with Friendly Fire");
               return sb.ToString();
            case "NumKillMarderIIFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " Marder II SPGs with Friendly Fire");
               return sb.ToString();
            case "NumKillMarderIIIFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " Marder III SPGs with Friendly Fire");
               return sb.ToString();
            case "NumKillSTuGIIIgFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " STuG IIIg SPGs with Friendly Fire");
               return sb.ToString();
            case "NumKillJgdPzIVFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " JgdPz IV SPGs with Friendly Fire");
               return sb.ToString();
            case "NumKillJgdPz38tFriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " JgdPz 38t SPGs with Friendly Fire");
               return sb.ToString();
            case "NumKillPak38FriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " Pak38 ATGs with Friendly Fire");
               return sb.ToString();
            case "NumKillPak40FriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " Pak40 ATGs with Friendly Fire");
               return sb.ToString();
            case "NumKillPak43FriendlyFire": 
               sb.Append( isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append( " Pak43 ATGs with Friendly Fire");
               return sb.ToString();
            //------------
            case "NumKillLwYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" LW units with Your Fire");
               return sb.ToString();
            case "NumKillMgYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" MG Teams with Your Fire");
               return sb.ToString();
            case "NumKillTruckYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" Trucks with Your Fire");
               return sb.ToString();
            case "NumKillPswYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append("  PSW232s with Your Fire");
               return sb.ToString();
            case "NumKillSpwYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" SPW251s with Your Fire");
               return sb.ToString();
            case "NumKillPzIVYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" Pz IV tanks with Your Fire");
               return sb.ToString();
            case "NumKillPzVYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" Pz V tanks with Your Fire");
               return sb.ToString();
            case "NumKillPzVIeYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" Pz VIe tanks with Your Fire");
               return sb.ToString();
            case "NumKillPzVIbYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" Pz VIb tanks with Your Fire");
               return sb.ToString();
            case "NumKillMarderIIYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" Marder II SPGs with Your Fire");
               return sb.ToString();
            case "NumKillMarderIIIYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" Marder III SPGs with Your Fire");
               return sb.ToString();
            case "NumKillSTuGIIIgYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" STuG IIIg SPGs with Your Fire");
               return sb.ToString();
            case "NumKillJgdPzIVYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" JgdPz IV SPGs with Your Fire");
               return sb.ToString();
            case "NumKillJgdPz38tYourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" JgdPz 38t SPGs with Your Fire");
               return sb.ToString();
            case "NumKillPak38YourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" Pak38 ATGs with Your Fire");
               return sb.ToString();
            case "NumKillPak40YourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" Pak40 ATGs with Your Fire");
               return sb.ToString();
            case "NumKillPak43YourFire":
               sb.Append(isThreshold ? feat.Threshold.ToString() : feat.Value.ToString());
               sb.Append(" Pak43 ATGs with Your Fire");
               return sb.ToString();
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
         if (this.Count != rightFeats.Count) // sync up the two lists
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): (rightFeats.Count=" + rightFeats.Count.ToString() + ") > (this.Count=" + this.Count.ToString() + ")");
            return false;
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
               if (0 == left.Value % left.Threshold) // when the value reaches an iterative threshold, show feat
                  outFeat = left;
               right.Value = left.Value - 1; // starting feat is one behind current feat - gets set to same value when user acknowledges feat
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
      public void SetGameFeatThreshold()
      {
         foreach( GameFeat feat in this)
         {
            int threshold = 1;
            switch (feat.Key)
            {
               case "NumKillLwFriendlyFire": threshold = 12; break;
               case "NumKillMgFriendlyFire": threshold = 10; break;
               case "NumKillTruckFriendlyFire": threshold = 10; break;
               case "NumKillPswFriendlyFire": threshold = 8; break;
               case "NumKillSpwFriendlyFire": threshold = 8; break;
               case "NumKillPzIVFriendlyFire": threshold = 5; break;
               case "NumKillPzVFriendlyFire": threshold = 4; break;
               case "NumKillPzVIeFriendlyFire": threshold = 3; break;
               case "NumKillPzVIbFriendlyFire": threshold = 1; break;
               case "NumKillMarderIIFriendlyFire": threshold = 3; break;
               case "NumKillMarderIIIFriendlyFire": threshold = 3; break;
               case "NumKillSTuGIIIgFriendlyFire": threshold = 5; break;
               case "NumKillJgdPzIVFriendlyFire": threshold = 2; break;
               case "NumKillJgdPz38tFriendlyFire": threshold = 1; break;
               case "NumKillPak38FriendlyFire": threshold = 8; break;
               case "NumKillPak40FriendlyFire": threshold = 6; break;
               case "NumKillPak43FriendlyFire": threshold = 4; break;
               //-------------------------------------------
               case "NumKillLwYourFire": threshold = 6; break;
               case "NumKillMgYourFire": threshold = 5; break;
               case "NumKillTruckYourFire": threshold = 5; break;
               case "NumKillPswYourFire": threshold = 4; break;
               case "NumKillSpwYourFire": threshold = 4; break;
               case "NumKillPzIVYourFire": threshold = 3; break;
               case "NumKillPzVYourFire": threshold = 2; break;
               case "NumKillPzVIeYourFire": threshold = 1; break;
               case "NumKillPzVIbYourFire": threshold = 1; break;
               case "NumKillMarderIIYourFire": threshold = 2; break;
               case "NumKillMarderIIIYourFire": threshold = 1; break;
               case "NumKillSTuGIIIgYourFire": threshold = 3; break;
               case "NumKillJgdPzIVYourFire": threshold = 1; break;
               case "NumKillJgdPz38tYourFire": threshold = 1; break;
               case "NumKillPak38YourFire": threshold = 4; break;
               case "NumKillPak40YourFire": threshold = 3; break;
               case "NumKillPak43YourFire": threshold = 2; break;
               default: threshold = 1; break;
            }
            feat.Threshold = threshold;
         }
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
         Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "AddOne():  key=" + o.Key + " value=" + o.Value);
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
