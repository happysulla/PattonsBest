using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Pattons_Best
{
   [Serializable]
   public class Option
   {
      public string Name { get; set; } = string.Empty;
      public bool IsEnabled { get; set; } = false;
      public Option()
      {
      }
      public Option(string name, bool isEnabled)
      {
         Name = name;
         IsEnabled = isEnabled;
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("(name=");
         sb.Append(this.Name.ToString());
         sb.Append("->");
         sb.Append(this.IsEnabled.ToString());
         sb.Append(")");
         return sb.ToString();
      }
   }
   [XmlInclude(typeof(Option))]
   [Serializable]
   public class Options : IEnumerable
   {
      [NonSerialized]
      public static string[] theDefaults = new string[] 
      {
         "OriginalGame",
         "Generalv25No3",
         "OtherGame",
         "CustomGame",
         //---------------------
         "SingleDayScenario",
         "GameEndsOnCommanderDeath",
         //---------------------
         "SkipTutorial0",
         "SkipTutorial1",
         "SkipTutorial2",
         "SkipTutorial3",
         "SkipTutorial4",
         "SkipTutorial5",
         //---------------------
         "AutoRollNewMembers",
         "AutoRollAmmoLoad",
         "AutoPreparation",
         "AutoRollEnemyActivation",
         "AutoRollBowMgFire",
         //---------------------
         "AirInterdiction",
         "TerrainPointValue",
         "TankCoveredArc",
         "SlowTransverseCoveredArc",
         "SpgCoveredArc",
         "AtgCoveredArc",
         //---------------------
         "EnemyContinueMove",
         "EnemyRearFacingOnMove",
         "ShermanIncreaseMoveChances",
         "ShermanEnhanceMgFire",
         "ShermanSurpressingMgFire"
      };
      private readonly ArrayList myList;
      public Options() { myList = new ArrayList(); }
      public int Count { get => myList.Count; }
      public void Add(Option o) { myList.Add(o); }
      public void Add(object o) { myList.Add(o); }
      public void Insert(int index, Option o) { myList.Insert(index, o); }
      public void Clear() { myList.Clear(); }
      public bool Contains(Option o) { return myList.Contains(o); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(Option o) { return myList.IndexOf(o); }
      public Option Find(string name)
      {
         int i = 0;
         foreach (object o in myList)
         {
            Option? option = (Option)o;
            if (null == option)
               continue;
            if (name == option.Name)
               return option;
            ++i;
         }
         Option option1 = new Option(name, false);
         this.myList.Add(option1);
         return option1;
      }
      public Option? RemoveAt(int index)
      {
         Option? option = myList[index] as Option;
         myList.RemoveAt(index);
         return option;
      }
      public Option? this[int index]
      {
         get { Option? o = myList[index] as Option; return o; }
         set { myList[index] = value; }
      }
      public Options Clone()
      {
         Options copy = new Options();
         foreach (object o in myList)
         {
            Option option = (Option)o;
            Option copyO = new Option(option.Name, option.IsEnabled);
            copy.Add(copyO);
         }
         return copy;
      }
      public int GetGameIndex()
      {
         Option option = this.Find("CustomGame");
         if (true == option.IsEnabled)
            return 3;
         option = this.Find("OtherGame");
         if (true == option.IsEnabled)
            return 2;
         option = this.Find("Generalv25No3");
         if (true == option.IsEnabled)
            return 1;
         return 0;
      }
      public void SetOriginalGameOptions()
      {
         Clear();
         foreach (string s in theDefaults)
            Add(new Option(s, false));
      }
      public void SetValue(string key, bool value)
      {
         Option o = Find(key);
         o.IsEnabled = value;
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[");
         foreach (Object obj in myList )
         {
            Option option = (Option)obj;
            sb.Append(option.ToString());
         }
         sb.Append("]");
         return sb.ToString();
      }
   }
}
