using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
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

   }
   //========================================
   [XmlInclude(typeof(GameFeat))]
   [Serializable]
   public class GameFeats : IEnumerable
   {
      [NonSerialized] public static string theGameFeatDirectory = "";
      [NonSerialized]
      public static string[] theDefaults =
   {
         "EndCampaignGame",
         "EndCampaignGameOnTime",
         "EndCampaignGameWin",
         "EndSingleDayWin",
         "EndGameExplode",
         "EndGameCmdrKilled", 
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
         "HvssTrained",
         "RepairMain",
         "RepairMg",
         "FireMortar",
         "ThrowSmoke",
         "NumCriticalHitWithMG",
         "NumCriticalHitWithMainGun",
         //------------
         "NumShermanExplodes",
         "NumShermanBurns",
         "NumShermanPenetration",
         "NumPanzerfaustDeath",
         "NumMineImmobilization"
      };
      private readonly ArrayList myList;
      public static string GetFeatMessage(GameFeat feat, bool isThreshold = false)
      {
         StringBuilder sb = new StringBuilder();
         switch (feat.Key)
         {
            case "EndCampaignGame": return "Complete a campaign game";
            case "EndCampaignGameOnTime": return "Last all days of a campaign";
            case "EndCampaignGameWin": return "Win a campaign game";
            case "EndSingleDayWin": return "Win a Single Day Game";
            case "EndGameExplode": return "Game ends with tank explosion";
            case "EndGameCmdrKilled": return "Game ends with commander killed";
            //------------
            case "NumKillLwFriendlyFire":
               sb.Append("Killed LW units with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMgFriendlyFire":
               sb.Append("Killed MG Teams with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillTruckFriendlyFire":
               sb.Append("Killed trucks with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPswFriendlyFire":
               sb.Append("Killed PSW232 AFVs with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillSpwFriendlyFire":
               sb.Append("Killed SPW251 AFVs with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzIVFriendlyFire":
               sb.Append("Killed PzIV Tanks with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVFriendlyFire":
               sb.Append("Killed PzV Tanks with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVIeFriendlyFire":
               sb.Append("Killed PzVIe Tanks with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVIbFriendlyFire":
               sb.Append("Killed PzVIb Tanks with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMarderIIFriendlyFire":
               sb.Append("Killed MarderII SPGs with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMarderIIIFriendlyFire":
               sb.Append("Killed MarderIII SPGs with friendly fire " );
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillSTuGIIIgFriendlyFire":
               sb.Append("Killed STuGIIIg SPGs with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillJgdPzIVFriendlyFire":
               sb.Append("Killed JgdPzIV SPGs with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillJgdPz38tFriendlyFire":
               sb.Append("Killed JgdPz38t SPGs with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak38FriendlyFire":
               sb.Append("Killed Pak38 ATGs with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak40FriendlyFire":
               sb.Append("Killed Pak40 ATGs with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak43FriendlyFire":
               sb.Append("Killed Pak43 ATGs with friendly fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            //------------
            case "NumKillLwYourFire":
               sb.Append("Killed LW units with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMgYourFire":
               sb.Append("Killed MG Teams with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillTruckYourFire":
               sb.Append("Killed trucks with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPswYourFire":
               sb.Append("Killed PSW232 AFVs with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillSpwYourFire":
               sb.Append("Killed SPW251 AFVs with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzIVYourFire":
               sb.Append("Killed PzIV Tanks with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVYourFire":
               sb.Append("Killed PzV Tanks with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVIeYourFire":
               sb.Append("Killed PzVIe Tanks with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPzVIbYourFire":
               sb.Append("Killed PzVIb Tanks with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMarderIIYourFire":
               sb.Append("Killed MarderII SPGs with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillMarderIIIYourFire":
               sb.Append("Killed MarderIII SPGs with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillSTuGIIIgYourFire":
               sb.Append("Killed STuGIIIg SPGs with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillJgdPzIVYourFire":
               sb.Append("Killed JgdPzIV SPGs with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillJgdPz38tYourFire":
               sb.Append("Killed JgdPz38t SPGs with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak38YourFire":
               sb.Append("Killed Pak38 ATGs with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak40YourFire":
               sb.Append("Killed Pak40 ATGs with your fire ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumKillPak43YourFire":
               sb.Append("Killed Pak43 ATGs with your fire " );
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();            
               //------------
            case "NumPurpleHearts":
               sb.Append("Receive Purple Heart medal ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumBronzeStars":
               sb.Append("Receive Bronze Star medal ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumSilverStars":
               sb.Append("Receive Silver Star medal ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
                  sb.Append(" out of 1");
               sb.Append(" times");
               return sb.ToString();
            case "NumDistinguishedCrosses":
               sb.Append("Receive Distinguished Cross medal ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
                  sb.Append(" out of 1");
               sb.Append(" times");
               return sb.ToString();
            case "NumMedalOfHonors":
               sb.Append("!!!!!!!!!!!!Receive Medal of Honor ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
                  sb.Append(" out of 1");
               sb.Append(" times!!!!!!!!!!!!");
               return sb.ToString();
            //------------
            case "HvssTrained":
               sb.Append("Trained on HVSS that allows firing when moving ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
                  sb.Append(" out of 1");
               sb.Append(" times");
               return sb.ToString();
            case "RepairMain":
               sb.Append("Repair the main gun in battle ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
                  sb.Append(" out of 1");
               sb.Append(" times");
               return sb.ToString();
            case "RepairMg":
               sb.Append("Repair the disabled MG in battle ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
                  sb.Append(" out of 1");
               sb.Append(" times");
               return sb.ToString();
            case "FireMortar":
               sb.Append("Sherman fires a mortar ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
                  sb.Append(" out of 1");
               sb.Append(" times");
               return sb.ToString();
            case "ThrowSmoke":
               sb.Append("Throw smoke out of tank hatch ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
                  sb.Append(" out of 1");
               sb.Append(" times");
               return sb.ToString();
            case "NumCriticalHitWithMG":
               sb.Append("Critical Hit Kill with MG ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
                  sb.Append(" out of 1");
               sb.Append(" times");
               return sb.ToString();
            case "NumCriticalHitWithMainGun":
               sb.Append("Critical Hit Kill with Main Gun ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
                  sb.Append(" out of 1");
               sb.Append(" times");
               return sb.ToString();
            //------------
            case "NumShermanExplodes":
               sb.Append("Sherman exploded ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumShermanBurns":
               sb.Append("Sherman brewup ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumShermanPenetration":
               sb.Append("Sherman killed by penetration ");
               sb.Append(feat.Value.ToString());
               if (true == isThreshold)
               {
                  sb.Append(" out of ");
                  sb.Append(feat.Threshold.ToString());
               }
               sb.Append(" times");
               return sb.ToString();
            case "NumPanzerfaustDeath": return "Tank destroyed from Panzerfaust attack";
            case "NumMineImmobilization": return "Mine attack causes Sherman immobilization";
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetFeatMessage(): Unknown key=" + feat.Key);
               return "UNKNOWN: " + feat.Key;
         }
      }
      //---------------------------------------------------
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
      public bool GetFeatChange(GameFeats rightFeats, out GameFeat changedFeat)
      {
         changedFeat = new GameFeat();
         if (this.Count < rightFeats.Count) // this should not happen
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): (rightFeats.Count=" + rightFeats.Count.ToString() + ") > (this.Count=" + this.Count.ToString() + ")");
            return false;
         }
         if (rightFeats.Count < this.Count)
         {
            for (int i = rightFeats.Count; i < this.Count; ++i)
            {
               GameFeat? feat = this[i];
               if (null == feat)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): feat is null for i=" + i.ToString());
                  return false;
               }
               rightFeats.Add(feat);
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
               if ( (0 == left.Threshold) && (1 == left.Value) ) // only want to show this GameFeat one time when value is 1
               {
                  Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Get_FeatChange(): No Threshold Key=" + right.Key + " (left.Value=" + left.Value.ToString() + ") != (right.Value =" + right.Value.ToString() + ")");
                  changedFeat = left;
                  return true;
               }
               else if (0 == left.Value % left.Threshold) // when the value reaches an iterative threshold, show feat
               {
                  Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Get_FeatChange(): Reached Threshold=" + left.Threshold.ToString() + " Key=" + right.Key + " (left.Value=" + left.Value.ToString() + ") != (right.Value =" + right.Value.ToString() + ")");
                  changedFeat = left;
                  return true;
               }
               else
               {
                  right.Value = left.Value; // if not at threshold, ignore but update to current value
               }
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
            int threshold = 0;
            switch (feat.Key)
            {
               case "NumShermanExplodes": threshold = 3; break;
               case "NumShermanBurns": threshold = 5; break;
               case "NumShermanPenetration": threshold = 8; break;
               //-------------------------------------------
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
               //-------------------------------------------
               case "NumPurpleHearts": threshold = 5; break;
               case "NumBronzeStars": threshold = 2; break;
               default: threshold = 0; break;
            }
            feat.Threshold = threshold;
         }
      }
      public void SetValue(string key, int value)
      {
         GameFeat? o = Find(key);
         if (null == o)
         {
            Logger.Log(LogEnum.LE_ERROR, "Set_Value(): null for key=" + key);
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
            Logger.Log(LogEnum.LE_ERROR, "Add_One(): null for key=" + key);
            o = new GameFeat(key);
            this.myList.Add(o);
         }
         o.Value++;
         Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Add_One():  key=" + o.Key + " value=" + o.Value);
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
