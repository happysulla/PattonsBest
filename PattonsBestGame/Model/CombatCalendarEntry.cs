using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   public class CombatCalendarEntry : ICombatCalendarEntry
   {
      public string Date { set; get; }
      public EnumScenario Scenario { set; get; }
      public int Probability { set; get; }
      public EnumResistance Resistance { set; get; }
      public string Note { set; get; }
      public CombatCalendarEntry(string d, EnumScenario s, int p, EnumResistance r, string n = "")
      {
         Date = d;
         Scenario = s;
         Probability = p;
         Resistance = r;
         Note = n;
      }
   }
   //===================================================================
   public class CombatCalendarEntries : IEnumerable, ICombatCalanderEntries
   {
      private readonly ArrayList myList;
      public CombatCalendarEntries() { myList = new ArrayList(); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int Count { get { return myList.Count; } }
      public void Add(ICombatCalendarEntry ce) { myList.Add(ce); }
      public ICombatCalendarEntry? Find(string date)
      {
         foreach (Object o in myList)
         {
            ICombatCalendarEntry ce = (ICombatCalendarEntry)o;
            if (date == Utilities.RemoveSpaces(ce.Date))
               return ce;
         }
         return null;
      }
      public int IndexOf(ICombatCalendarEntry ce) { return myList.IndexOf(ce); }
      public ICombatCalendarEntry? this[int index]
      {
         get
         {
            if (this.Count <= index)
               return null;
            if (null == myList[index])
               return null;
            ICombatCalendarEntry? ce = myList[index] as ICombatCalendarEntry;
            return ce;
         }
         set { myList[index] = value; }
      }
   }
}
