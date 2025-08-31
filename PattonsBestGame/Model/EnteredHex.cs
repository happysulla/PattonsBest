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
      CAE_RETREAT,
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
      public IMapPoint MapPoint { get; set; }
      public ColorActionEnum ColorAction { get; set; } = ColorActionEnum.CAE_START;
      //------------------------------------------------------------------------------------------------
      public EnteredHex(IGameInstance gi, ITerritory t, ColorActionEnum colorAction, IMapPoint mp)
      {
         ++theId;
         Identifer = "Hex" + theId.ToString();
         Day = gi.Day + 1; ;
         Date = TableMgr.GetDate(gi.Day);
         TerritoryName = t.Name;
         MapPoint = mp;
         ColorAction = colorAction;
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameStateMorningBriefing.PerformAction(): lastReport=null");
            CtorError = true;
            return;
         }
         Time = TableMgr.GetTime(lastReport);
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
         sb.Append(",mp=");
         sb.Append(MapPoint.ToString());
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
