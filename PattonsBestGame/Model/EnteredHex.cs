using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pattons_Best
{
   [Serializable]
   public enum ColorActionEnum
   {
      CAE_START,
      CAE_ENTER,
      CAE_STOP
   };
   [Serializable]
   public class EnteredHex
   {
      public static int theId = 0;
      public bool CtorError { get; } = false;
      public string Identifer { get; set; } = "";
      public int Day { get; set; } = 0;
      public string Date { get; set; } = "";
      public string Time { get; set; } = "";
      public string TerritoryName { get; set; } = "";
      public int Position { get; set; } = 0;         // postion in the hex - if 1+ elispes exist in same hex, they are offset by position
      public ColorActionEnum ColorAction { get; set; } = ColorActionEnum.CAE_START;
      //------------------------------------------------------------------------------------------------
      public EnteredHex(IGameInstance gi, ITerritory t, ColorActionEnum colorAction)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateMorningBriefing.PerformAction(): lastReport=null");
            CtorError = true;
            return;
         }
         ++theId;
         Identifer = "Hex#" + theId.ToString();
         Day = gi.Day + 1; ;
         Date = TableMgr.GetDate(gi.Day);
         Time = TableMgr.GetTime(lastReport);
         TerritoryName = t.Name;
         ColorAction = colorAction;
         //-----------------------------------------------
         Position = 0;
         foreach (EnteredHex hex in gi.EnteredHexes.AsEnumerable().Reverse())
         {
            if (hex.TerritoryName == t.Name)
            {
               Position = hex.Position + 1;
               break;
            }
         }
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("(Id=");
         sb.Append(this.Identifer.ToString());
         sb.Append(",Day=");
         sb.Append(Day.ToString());
         sb.Append(",t=");
         sb.Append(TerritoryName);
         sb.Append(")");
         return sb.ToString();
      }
   };
   public static class EnteredHexExtensions
   {
      public static string toString(this IList<EnteredHex> enteredHexes)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[");
         foreach (EnteredHex hex in enteredHexes)
            sb.Append(hex.ToString());
         sb.Append("]");
         return sb.ToString();
      }
   }
}
