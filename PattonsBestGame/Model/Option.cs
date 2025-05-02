using System;
using System.Collections;
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
   }
   [XmlInclude(typeof(Option))]
   [Serializable]
   public class Options : IEnumerable
   {
      [NonSerialized]
      public static string[] theDefaults = new string[0] // first 16 entries must be persons
      {

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
   }
}
