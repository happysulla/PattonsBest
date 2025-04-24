using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   public interface ICombatCalendarEntry
   {
      string Date { set; get; }
      EnumScenario Scenario { set; get; }
      int Probability { set; get; }
      EnumResistance Resistance { set; get; }
      string Note { set; get; }
   }
   //----------------------------------------
   public interface ICombatCalanderEntries : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(ICombatCalendarEntry ce);
      ICombatCalendarEntry? Find(string day);
      int IndexOf(ICombatCalendarEntry ce);
      ICombatCalendarEntry? this[int index] { get; set; }
   }
}
