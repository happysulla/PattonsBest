using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;
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
      public static string[] theDefaults = new string[10] 
      {
         "SkipTutorial0",
         "SkipTutorial1",
         "SkipTutorial2",
         "SkipTutorial3",
         "SkipTutorial4",
         "SkipTutorial5",
         "AutoRollNewMembers",
         "AutoPreparation",
         "AutoRollEnemyActivation",
         "AutoRollBowMgFire"
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
      public Option? Find(string name)
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
         return null;
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
      public void SetOriginalGameOptions()
      {
         Clear();
         foreach (string s in theDefaults)
            Add(new Option(s, false));
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
