using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Printing3D;

namespace Pattons_Best
{
   public class TableMgr
   {
      public static ICombatCalanderEntries theCombatCalendarEntries = new CombatCalendarEntries();
      public static int[,] theExits = new int[10,10];
      //-------------------------------------------
      public TableMgr() 
      {
         CreateCombatCalender();
         CreateExitTable();
      }
      //-------------------------------------------
      public static string GetWeather(int dieRoll)
      {
         return "Clear";
      }
      public static string GetMonth( int day )
      {
         if (day < 5)
            return "Jul";
         if (day < 37)
            return "Aug";
         if (day < 58)
            return "Sep";
         if (day < 70)
            return "Oct";
         if (day < 92)
            return "Nov";
         if (day < 110)
            return "Dec";
         if (day < 137)
            return "Jan";
         if (day < 147)
            return "Feb";
         if (day < 174)
            return "Mar";
         if (day < 193)
            return "Apr";
         Logger.Log(LogEnum.LE_ERROR, "GetMonth(): reached default day=" + day.ToString());
         return "ERROR";
      }
      public static bool SetTimeTrack(IAfterActionReport lastReport, int day)
      {
         switch (GetMonth(day))
         {
            case "Jul":
            case "Aug":
               lastReport.SunriseHour = 5;
               lastReport.SunriseMin = 0;
               lastReport.SunsetHour = 19;
               lastReport.SunsetMin = 15;
               break;
            case "Sep":
               lastReport.SunriseHour = 5;
               lastReport.SunriseMin = 30;
               lastReport.SunsetHour = 18;
               lastReport.SunsetMin = 15;
               break;
            case "Oct":
               lastReport.SunriseHour = 6;
               lastReport.SunriseMin = 30;
               lastReport.SunsetHour = 17;
               lastReport.SunsetMin = 15;
               break;
            case "Nov":
               lastReport.SunriseHour = 7;
               lastReport.SunriseMin = 15;
               lastReport.SunsetHour = 16;
               lastReport.SunsetMin = 15;
               break;
            case "Dec":
               lastReport.SunriseHour = 7;
               lastReport.SunriseMin = 45;
               lastReport.SunsetHour = 16;
               lastReport.SunsetMin = 00;
               break;
            case "Jan":
               lastReport.SunriseHour = 7;
               lastReport.SunriseMin = 45;
               lastReport.SunsetHour = 16;
               lastReport.SunsetMin = 30;
               break;
            case "Feb":
               lastReport.SunriseHour = 7;
               lastReport.SunriseMin = 15;
               lastReport.SunsetHour = 17;
               lastReport.SunsetMin = 30;
               break;
            case "Mar":
               lastReport.SunriseHour = 6;
               lastReport.SunriseMin = 15;
               lastReport.SunsetHour = 18;
               lastReport.SunsetMin = 00;
               break;
            case "Apr":
               lastReport.SunriseHour = 5;
               lastReport.SunriseMin = 15;
               lastReport.SunsetHour = 19;
               lastReport.SunsetMin = 00;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GameStateMorningBriefing.PerformAction(MorningBriefingTimeCheckRoll): reached default day=" + day.ToString());
               return false;
         }

         return true;
      }
      //-------------------------------------------
      private void CreateCombatCalender()
      {
         theCombatCalendarEntries.Add(new CombatCalendarEntry("07/27/44", EnumScenario.Advance, 3, EnumResistance.Light, "Corba Breakout"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("07/28/44", EnumScenario.Battle, 4, EnumResistance.Medium, "Coutances"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("07/29/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("07/30/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Avranches"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("07/31/44", EnumScenario.Advance, 4, EnumResistance.Light)); // Day=4
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/01/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/02/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/03/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/03/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/04/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/05/44", EnumScenario.Advance, 4, EnumResistance.Heavy, "Vannes"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/06/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/07/44", EnumScenario.Advance, 4, EnumResistance.Heavy, "Lorient"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/08/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/09/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/10/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/11/44", EnumScenario.Advance, 4, EnumResistance.Light, "Nantes"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/12/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/13/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/14/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/15/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/16/44", EnumScenario.Advance, 3, EnumResistance.Medium, "Orleans"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/17/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/18/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/19/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/20/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/21/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/22/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/23/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/24/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/25/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/26/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/27/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/28/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/29/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/30/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/31/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Commery")); // Day=36
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/01/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/02-09/10 1943", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/11/44", EnumScenario.Advance, 5, EnumResistance.Heavy, "Moselle Crossing"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/12/44", EnumScenario.Counterattack, 5, EnumResistance.Medium, "Moselle Crossing"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/13/44", EnumScenario.Counterattack, 5, EnumResistance.Medium, "Moselle Crossing"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/14/44", EnumScenario.Advance, 3, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/15/44", EnumScenario.Battle, 9, EnumResistance.Medium, "Crevic adn Maixe"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/16/44", EnumScenario.Battle, 9, EnumResistance.Medium, "Luneville"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/17/44", EnumScenario.Advance, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/18/44", EnumScenario.Advance, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/19/44", EnumScenario.Battle, 9, EnumResistance.Medium, "Arracourt"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/20/44", EnumScenario.Advance, 3, EnumResistance.Light, "Arracourt"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/21/44", EnumScenario.Battle, 9, EnumResistance.Medium, "Arracourt"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/22/44", EnumScenario.Battle, 9, EnumResistance.Medium, "Arracourt"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/23-09/24 1943", EnumScenario.Retrofit, 10, EnumResistance.None));  // Day=51
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/25/44", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Counter Attack"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/26/44", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Counter Attack"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/27/44", EnumScenario.Battle, 6, EnumResistance.Medium, "Hill 318"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/28/44", EnumScenario.Battle, 5, EnumResistance.Medium, "Hill 318"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/29/44", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Arracourt"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/30/44", EnumScenario.Counterattack, 9, EnumResistance.Light)); // Day=57
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/01/44", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/02/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/03/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/04/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/05/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/06/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/07/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/08/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/09/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/10/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/11/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/12-11/08 1943", EnumScenario.Retrofit, 10, EnumResistance.None)); // Day=69
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/09/44", EnumScenario.Battle, 5, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/10/44", EnumScenario.Advance, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/11/44", EnumScenario.Advance, 5, EnumResistance.Medium, "Fonteny"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/12/44", EnumScenario.Counterattack, 8, EnumResistance.Heavy, "Counterattack at Rodable"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/13/44", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/14/44", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/15/44", EnumScenario.Advance, 4, EnumResistance.Heavy));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/16/44", EnumScenario.Advance, 4, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/17/44", EnumScenario.Advance, 4, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/18/44", EnumScenario.Advance, 4, EnumResistance.Heavy, "Dieuze and Rodable"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/19/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/20/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/21/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/22/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/23/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/24/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Crossed Saare River at Romeifling"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/25/44", EnumScenario.Counterattack, 7, EnumResistance.Medium, "Counterattacks"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/26/44", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/27/44", EnumScenario.Advance, 3, EnumResistance.Medium, "Wolfskirchen"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/28/44", EnumScenario.Advance, 2, EnumResistance.Light, "Cleared zone of responsibility"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/29/44", EnumScenario.Advance, 2, EnumResistance.Light, "Cleared zone of responsibility"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/30/44", EnumScenario.Advance, 2, EnumResistance.Light, "Cleared zone of responsibility")); // Day=91
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/01/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Attacked Saare Union"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/02/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Attacked Saare Union"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/03/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/04/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/05/44", EnumScenario.Battle, 9, EnumResistance.Heavy, "Battle of Bining"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/06/44", EnumScenario.Battle, 9, EnumResistance.Heavy, "Battle of Bining"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/07-12/20 1943", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/21/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/22/44", EnumScenario.Advance, 2, EnumResistance.Light, "Martelange")); // Day=100
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/23/44", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/24/44", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/25/44", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/26/44", EnumScenario.Advance, 9, EnumResistance.Heavy, "Into Bastogne"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/27/44", EnumScenario.Advance, 4, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/28/44", EnumScenario.Advance, 4, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/29/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Open Arion-Bastogne Highway"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/30/44", EnumScenario.Counterattack, 3, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/31/44", EnumScenario.Counterattack, 3, EnumResistance.Medium)); // Day=109
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/01/45", EnumScenario.Counterattack, 3, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/02/45", EnumScenario.Counterattack, 3, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/03-01/08 1944", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/09/45", EnumScenario.Advance, 2, EnumResistance.Light, "Noville"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/10/45", EnumScenario.Advance, 2, EnumResistance.Light, "Bourcy"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/11/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/12/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/13/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/14/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/15/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/16/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/17/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/18/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/19/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/20/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/21/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/22/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/23/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/24/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/25/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/26/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/27/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/28/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/29/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/30/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/31/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions")); // Day=136
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/01/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/02/45", EnumScenario.Advance, 2, EnumResistance.Light, "Hosdorf"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/03-02/21 1944", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/22/45", EnumScenario.Battle, 8, EnumResistance.Medium, "Geichlingen"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/23/45", EnumScenario.Battle, 6, EnumResistance.Medium, "Sinspenit"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/24/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/25/45", EnumScenario.Battle, 5, EnumResistance.Medium, "Rittersdorf"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/26/45", EnumScenario.Battle, 5, EnumResistance.Medium, "Bitburg"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/27/45", EnumScenario.Battle, 5, EnumResistance.Medium, "Matzen and Fleissen"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/28/45", EnumScenario.Counterattack, 2, EnumResistance.Medium)); // Day=146
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/01/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/02/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/03-03/04 1944", EnumScenario.Retrofit, 10, EnumResistance.None)); // Day=149
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/05/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/06/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/07/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/08/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/09/45", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/10/45", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/11/45", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/12-03/13 1944", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/14/45", EnumScenario.Advance, 9, EnumResistance.Medium, "Attack out of Moselle Bridgehead"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/15/45", EnumScenario.Advance, 7, EnumResistance.Medium, "Bad Kreuznauch"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/16/45", EnumScenario.Advance, 5, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/17/45", EnumScenario.Advance, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/18/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/19/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/20/45", EnumScenario.Advance, 2, EnumResistance.Light, "Worms on the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/21-03/23 1944", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/24/45", EnumScenario.Advance, 2, EnumResistance.Light, "Cross the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/25/45", EnumScenario.Advance, 2, EnumResistance.Light, "Hanau adn Darmstadt"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/26/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/27/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/28/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/29/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/30/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/31/45", EnumScenario.Advance, 2, EnumResistance.Light)); // Day=173
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/01/45", EnumScenario.Advance, 4, EnumResistance.Light, "Crezburg"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/02/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/03/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/04/45", EnumScenario.Advance, 4, EnumResistance.Light, "Gotha"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/05/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/06/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/07/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/08/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/09/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/10/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/11/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/12/45", EnumScenario.Advance, 2, EnumResistance.Light, "Crossed Saale River"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/13/45", EnumScenario.Advance, 2, EnumResistance.Light, "Wolkenburg"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/14/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/15/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/16/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/17/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("04/18/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("Drive into Czechoslavakia", EnumScenario.Retrofit, 10, EnumResistance.None)); // Day=192
         //--------------------------------------------------------------------
      }
      private void CreateExitTable()
      {
         theExits[0,0] = 8;
         theExits[0,1] = 8;
         theExits[0,2] = 8;
         theExits[0,3] = 8;
         theExits[0,4] = 8;
         theExits[0,5] = 8;
         theExits[0,6] = 5;
         theExits[0,7] = 5;
         theExits[0,8] = 5;
         theExits[0,9] = 5;
         //----------------
         theExits[1,0] = 8;
         theExits[1,1] = 8;
         theExits[1,2] = 8;
         theExits[1,3] = 8;
         theExits[1,4] = 8;
         theExits[1,5] = 4;
         theExits[1,6] = 4;
         theExits[1,7] = 5;
         theExits[1,8] = 5;
         theExits[1,9] = 5;
         //----------------
         theExits[2,0] = 7;
         theExits[2,1] = 7;
         theExits[2,2] = 7;
         theExits[2,3] = 7;
         theExits[2,4] = 7;
         theExits[2,5] = 3;
         theExits[2,6] = 3;
         theExits[2,7] = 4;
         theExits[2,8] = 4;
         theExits[2,9] = 4;
         //----------------
         theExits[3,0] = 7;
         theExits[3,1] = 6;
         theExits[3,2] = 6;
         theExits[3,3] = 6;
         theExits[3,4] = 3;
         theExits[3,5] = 2;
         theExits[3,6] = 2;
         theExits[3,7] = 4;
         theExits[3,8] = 4;
         theExits[3,9] = 4;
         //----------------
         theExits[4,0] = 6;
         theExits[4,1] = 5;
         theExits[4,2] = 5;
         theExits[4,3] = 2;
         theExits[4,4] = 2;
         theExits[4,5] = 2;
         theExits[4,6] = 2;
         theExits[4,7] = 3;
         theExits[4,8] = 3;
         theExits[4,9] = 3;
         //----------------
         theExits[5,0] = 5;
         theExits[5,1] = 5;
         theExits[5,2] = 5;
         theExits[5,3] = 1;
         theExits[5,4] = 1;
         theExits[5,5] = 1;
         theExits[5,6] = 1;
         theExits[5,7] = 2;
         theExits[5,8] = 2;
         theExits[5,9] = 8;
         //----------------
         theExits[6,0] = 5;
         theExits[6,1] = 4;
         theExits[6,2] = 10;
         theExits[6,3] = 10;
         theExits[6,4] = 10;
         theExits[6,5] = 10;
         theExits[6,6] = 10;
         theExits[6,7] = 2;
         theExits[6,8] = 2;
         theExits[6,9] = 7;
         //----------------
         theExits[7,0] = 4;
         theExits[7,1] = 4;
         theExits[7,2] = 10;
         theExits[7,3] = 10;
         theExits[7,4] = 10;
         theExits[7,5] = 10;
         theExits[7,6] = 10;
         theExits[7,7] = 1;
         theExits[7,8] = 7;
         theExits[7,9] = 6;
         //----------------
         theExits[8,0] = 4;
         theExits[8,1] = 3;
         theExits[8,2] = 9;
         theExits[8,3] = 9;
         theExits[8,4] = 9;
         theExits[8,5] = 9;
         theExits[8,6] = 9;
         theExits[8,7] = 10;
         theExits[8,8] = 6;
         theExits[8,9] = 5;
         //----------------
         theExits[9,0] = 3;
         theExits[9,1] = 9;
         theExits[9,2] = 9;
         theExits[9,3] = 9;
         theExits[9,4] = 9;
         theExits[9,5] = 9;
         theExits[9,6] = 9;
         theExits[9,7] = 6;
         theExits[9,8] = 5;
         theExits[9,9] = 5;
      }
   }
}
