using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;

namespace Pattons_Best
{
   public class TableMgr
   {
      public const int NO_CHANCE = 55555;
      public const int KIA = 10000;
      public const int MIA = 10001;
      public const int FN_ERROR = -1000;
      public const int THROWN_TRACK = 1001;
      public static ICombatCalanderEntries theCombatCalendarEntries = new CombatCalendarEntries();
      public static int[,] theExits = new int[10, 10];
      //public static int[,,,] theApToKills = new int[3,3,3,2]; // armor class, facing, range, T/H
      public static Dictionary<string, int[,,,]> theApToKills = new Dictionary<string, int[,,,]>(); // gun -> armorclass, facing, range, turret
      //-------------------------------------------
      public TableMgr()
      {
         CreateCombatCalender();
         CreateEnemyApToKill();
         CreateExitTable();
      }
      //-------------------------------------------
      public static string GetDate(int day)
      {
         if (day < 0)
            return "Boot Camp";
         StringBuilder sb = new StringBuilder();
         if (day < 5)
         {
            sb.Append("07/");
            int dayOfMonth = day + 27;
            if( dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1944");
         }
         else if (day < 36) // starts day=5
         {
            sb.Append("08/");
            int dayOfMonth = day - 4;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1944");
         }
         else if (day < 57)
         {
            if (37 == day)
               return "09/02-09/10 1943";
            if (50 == day)
               return "09/23-09/24 1943";
            sb.Append("09/");
            int dayOfMonth = day - 35;
            if ( 37 < day )
               dayOfMonth = day - 27;
            if (50 < day)
               dayOfMonth = day - 26;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1944");
         }
         else if (day < 69)
         {
            if (68 == day)
               return "10/12-11/08 1943";
            sb.Append("10/");
            int dayOfMonth = day - 56;
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1944");
         }
         else if (day < 91)
         {
            sb.Append("11/");
            int dayOfMonth = day - 60;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1944");
         }
         else if (day < 109)
         {
            if (97 == day)
               return "12/07-12/20 1943";
            sb.Append("12/");
            int dayOfMonth = day - 90;
            if (97 < day)
               dayOfMonth = day - 77;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1944");
         }
         else if (day < 135)
         {
            if (111 == day)
               return "01/03-01/08 1944";
            sb.Append("01/");
            int dayOfMonth = day - 108;
            if (111 < day)
               dayOfMonth = day - 103;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1945");
         }
         else if (day < 145)
         {
            if (137 == day)
               return "02/03-02/21 1944";
            sb.Append("02/");
            int dayOfMonth = day - 116;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1945");
         }
         else if (day < 172)
         {
            if (147 == day)
               return "03/03-03/04 1944";
            if (155 == day)
               return "03/12-03/13 1944";
            if (163 == day)
               return "03/21-03/23 1944";
            sb.Append("03/");
            int dayOfMonth = day - 144;
            if (149 < day)
               dayOfMonth = day - 143;
            if (155 < day)
               dayOfMonth = day - 139;
            if (163 < day)
               dayOfMonth = day - 140;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1945");
         }
         else if (day < 191)
         {
            sb.Append("04/");
            int dayOfMonth = day - 171;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1945");
         }
         return sb.ToString();
      }
      public static string GetMonth(int day)
      {
         if (day < 5)
            return "Jul";
         if (day < 36)
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
      public static string GetTime(IAfterActionReport aar)
      {
         StringBuilder sb = new StringBuilder();
         if (aar.SunriseHour < 10)
            sb.Append("0");
         sb.Append(aar.SunriseHour);
         sb.Append(":");
         if (aar.SunriseMin < 10)
            sb.Append("0");
         sb.Append(aar.SunriseMin);
         return sb.ToString();
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
      public static string GetWeather(int day, int dieRoll)
      {
         string month = GetMonth(day);
         switch (month)
         {
            case "Jul":
            case "Aug":
               if (dieRoll < 70)
                  return "Clear";
               else if (dieRoll < 89)
                  return "Overcast";
               else if (dieRoll < 92)
                  return "Fog";
               else if (dieRoll < 98)
                  return "Mud";
               else
                  return "Mud/Overcast";
            case "Sep":
            case "Oct":
            case "Nov":
               if (dieRoll < 62)
                  return "Clear";
               else if (dieRoll < 73)
                  return "Overcast";
               else if (dieRoll < 76)
                  return "Fog";
               else if (dieRoll < 92)
                  return "Mud";
               else if (dieRoll < 98)
                  return "Mud/Overcast";
               else
               {
                  if ("Nov" == month)
                     return "Snow";
                  else
                     return "Overcast";
               }
            case "Dec":
            case "Jan":
            case "Feb":
               if (dieRoll < 45)
                  return "Clear";
               else if (dieRoll < 51)
                  return "Overcast";
               else if (dieRoll < 59)
                  return "Mud/Overcast";
               else
                  return "Snow";
            case "Mar":
            case "Apr":
               if (dieRoll < 54)
                  return "Clear";
               else if (dieRoll < 72)
                  return "Overcast";
               else if (dieRoll < 76)
                  return "Fog";
               else if (dieRoll < 92)
                  return "Mud";
               else if (dieRoll < 98)
                  return "Mud/Overcast";
               else
               {
                  if ("Mar" == month)
                     return "Snow";
                  else
                     return "Overcast";
               }
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetWeather(): Reached Default month=" + month);
               return "ERROR";
         }
      }
      public static string GetWeatherSnow(int day, int dieRoll)
      {
         string month = GetMonth(day);
         if ("Nov" == month)
            --dieRoll;
         else if ("Dec" == month || "Jan" == month || "Feb" == month)
            ++dieRoll;
         switch (dieRoll)
         {
            case 0:
            case 1:
               return "Falling Snow";
            case 2:
            case 3:
            case 6:
            case 7:
               return "Ground Snow";
            case 4:
            case 5:
               return "Falling and Ground Snow";
            case 8:
            case 9:
               return "Deep Snow";
            case 10:
            case 11:
               return "Falling and Deep Snow";
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetWeatherSnow(): Reached Default month=" + dieRoll.ToString());
               return "ERROR";
         }
      }
      public static string SetEnemyUnit(EnumScenario situation, int day, int dieRoll)
      {
         const int Feb1945 = 136;
         const int Mar1945 = 146;
         string month = GetMonth(day);
         if (dieRoll < 6)
         {
            if (EnumScenario.Advance == situation && Feb1945 < day)
               return "LW";
            else
               return "SPG";
         }
         else if (dieRoll < 11)
         {
            return "MG";
         }
         else if (dieRoll < 16)
         {
            return "LW";
         }
         else if (dieRoll < 21)
         {
            if (EnumScenario.Advance == situation)
               return "TRUCK";
            else if (EnumScenario.Battle == situation)
               return "LW";
            else
               return "TANK";
         }
         else if (dieRoll < 26)
         {
            if (EnumScenario.Advance == situation && Mar1945 < day)
               return "MG";
            else if (EnumScenario.Battle == situation)
               return "ATG";
            else if (EnumScenario.Counterattack == situation)
               return "LW";
            else
               return "ATG";
         }
         else if (dieRoll < 31)
         {
            if (EnumScenario.Advance == situation && Mar1945 < day)
               return "MG";
            else if (EnumScenario.Battle == situation && Feb1945 < day)
               return "LW";
            else if (EnumScenario.Counterattack == situation && Feb1945 < day)
               return "LW";
            else
               return "SPG";
         }
         else if (dieRoll < 36)
         {
            return "MG";
         }
         else if (dieRoll < 41)
         {
            return "LW";
         }
         else if (dieRoll < 46)
         {
            if (EnumScenario.Advance == situation && Feb1945 < day)
               return "LW";
            else if (EnumScenario.Battle == situation)
               return "TANK";
            else if (EnumScenario.Counterattack == situation && Mar1945 < day)
               return "MG";
            else
               return "TANK";
         }
         else if (dieRoll < 51)
         {
            if (EnumScenario.Advance == situation)
               return "TRUCK";
            else if (EnumScenario.Battle == situation)
               return "MG";
            else
               return "LW";
         }
         else if (dieRoll < 56)
         {
            if (EnumScenario.Advance == situation && Feb1945 < day)
               return "LW";
            else if (EnumScenario.Battle == situation)
               return "ATG";
            else if (EnumScenario.Counterattack == situation && Mar1945 < day)
               return "MG";
            else if (EnumScenario.Advance == situation)
               return "ATG";
            else
               return "SPG";
         }
         else if (dieRoll < 61)
         {
            return "LW";
         }
         else if (dieRoll < 66)
         {
            if (EnumScenario.Advance == situation)
               return "TANK";
            else if (EnumScenario.Battle == situation)
               return "LW";
            else if (EnumScenario.Counterattack == situation && Mar1945 < day)
               return "MG";
            else
               return "TANK";
         }
         else if (dieRoll < 71)
         {
            if (EnumScenario.Advance == situation)
               return "ATG";
            else if (EnumScenario.Battle == situation && Feb1945 < day)
               return "LW";
            else if (EnumScenario.Counterattack == situation)
               return "LW";
            else
               return "ATG";
         }
         else if (dieRoll < 76)
         {
            if (EnumScenario.Advance == situation)
               return "PSW/SPW";
            else if (EnumScenario.Battle == situation && Mar1945 < day)
               return "MG";
            else if (EnumScenario.Counterattack == situation && Feb1945 < day)
               return "LW";
            else if (EnumScenario.Counterattack == situation)
               return "TANK";
            else
               return "ATG";
         }
         else if (dieRoll < 81)
         {
            if (EnumScenario.Advance == situation)
               return "ATG";
            else if (EnumScenario.Battle == situation && Mar1945 < day)
               return "MG";
            else if (EnumScenario.Counterattack == situation)
               return "MG";
            else
               return "ATG";
         }
         else if (dieRoll < 86)
         {
            if (EnumScenario.Battle == situation)
               return "MG";
            else
               return "LW";
         }
         else if (dieRoll < 91)
         {
            if (EnumScenario.Advance == situation)
               return "TRUCK";
            else if (EnumScenario.Battle == situation)
               return "LW";
            else if (EnumScenario.Counterattack == situation && Feb1945 < day)
               return "LW";
            else
               return "TANK";
         }
         else if (dieRoll < 96)
         {
            if (EnumScenario.Advance == situation && Feb1945 < day)
               return "LW";
            else if (EnumScenario.Battle == situation)
               return "ATG";
            else if (EnumScenario.Counterattack == situation)
               return "LW";
            else
               return "PSW/SPW";
         }
         else if (dieRoll < 101)
         {
            return "SPG";
         }
         Logger.Log(LogEnum.LE_ERROR, "SetEnemyUnit(): Reached Default with dr=" + dieRoll.ToString() + " situation=" + situation.ToString() + " on day=" + day.ToString());
         return "ERROR";
      }
      public static string GetEnemyRange(string areaType, string enemyUnit, int dieRoll)
      {
         if ("C" == areaType)
            dieRoll -= 3;
         else if ("D" == areaType)
            dieRoll -= 2;
         switch (enemyUnit)
         {
            case "ATG":
            case "Pak43":
            case "Pak38":
            case "Pak40":
               if (dieRoll < 3)
                  return "C";
               else if (dieRoll < 8)
                  return "M";
               else
                  return "L";
            case "LW":
               if (dieRoll < 7)
                  return "C";
               else if (dieRoll < 11)
                  return "M";
               else
                  return "L";  // test purposes only
            case "MG":
               if (dieRoll < 4)
                  return "C";
               else if (dieRoll < 9)
                  return "M";
               else
                  return "L";
            case "PSW":
            case "SPW":
            case "TANK":
            case "PzVIe":
            case "PzIV":
            case "PzV":
            case "PzVIb":
            case "TRUCK":
               if (dieRoll < 4)
                  return "C";
               else if (dieRoll < 8)
                  return "M";
               else
                  return "L";
            case "SPG":
            case "STuGIIIg":
            case "MARDERII":
            case "MARDERIII":
            case "JdgPzIV":
            case "JdgPz38t":
               if (dieRoll < 3)
                  return "C";
               else if (dieRoll < 7)
                  return "M";
               else
                  return "L";
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetEnemyRange(): Reached Default enemy=" + enemyUnit);
               return "ERROR";
         }
      }
      public static string GetEnemyNewFacing(string enemyUnit, int dieRoll)
      {
         switch (enemyUnit)
         {
            case "SPG":
            case "STuGIIIg":
            case "MARDERII":
            case "MARDERIII":
            case "JdgPzIV":
            case "JdgPz38t":
               if (dieRoll < 7)
                  return "Front";
               else if (dieRoll < 10)
                  return "Side";
               else
                  return "Rear";
            case "TANK":
            case "PzVIe":
            case "PzIV":
            case "PzV":
            case "PzVIb":
               if (dieRoll < 6)
                  return "Front";
               else if (dieRoll < 10)
                  return "Side";
               else
                  return "Rear";
            case "PSW":
            case "SPW":
            case "TRUCK":
               if (dieRoll < 4)
                  return "Front";
               else if (dieRoll < 8)
                  return "Side";
               else
                  return "Rear";
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetEnemyNewFacing(): Reached Default enemy=" + enemyUnit);
               return "ERROR";
         }
      }
      public static string GetEnemyTerrain(EnumScenario situation, int day, string areaType, string enemyUnit, int dieRoll)
      {
         if (EnumScenario.Counterattack == situation)
            dieRoll += 2;
         string month = GetMonth(day);
         switch (enemyUnit)
         {
            case "LW":
               switch (areaType)
               {
                  case "A":
                     if (dieRoll < 5)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Building";
                     else if (dieRoll == 10)
                        return "Moving in Open";
                     else
                        return "Open";
                  case "B":
                     if (dieRoll < 4)
                        return "Woods";
                     else if (dieRoll < 8 && "Nov" != month && "Feb" != month)
                        return "Woods";
                     else if (dieRoll < 8)
                        return "Fortification";
                     else if (dieRoll == 10)
                        return "Moving in Open";
                     else
                        return "Open";
                  case "C":
                     if (dieRoll < 3)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Building";
                     else if (dieRoll == 10)
                        return "Moving in Open";
                     else
                        return "Open";
                  case "D":
                     if (dieRoll < 4)
                        return "Woods";
                     else if (dieRoll < 9 && "Nov" != month && "Feb" != month)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Fortification";
                     else if (dieRoll == 10)
                        return "Moving in Open";
                     else
                        return "Open";
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "GetEnemyTerrain(): Reached Default areaType=" + areaType);
                     return "ERROR";
               }
            case "MG":
               switch (areaType)
               {
                  case "A":
                     if (dieRoll < 5)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Building";
                     else
                        return "Open";
                  case "B":
                     if (dieRoll < 4)
                        return "Woods";
                     else if (dieRoll < 8 && "Nov" != month && "Feb" != month)
                        return "Woods";
                     else if (dieRoll < 8)
                        return "Fortification";
                     else
                        return "Open";
                  case "C":
                     if (dieRoll < 3)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Building";
                     else
                        return "Open";
                  case "D":
                     if (dieRoll < 4)
                        return "Woods";
                     else if (dieRoll < 9 && "Nov" != month && "Feb" != month)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Fortification";
                     else
                        return "Open";
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "GetEnemyTerrain(): Reached Default areaType=" + areaType);
                     return "ERROR";
               }
            case "ATG":
            case "Pak43":
            case "Pak38":
            case "Pak40":
               switch (areaType)
               {
                  case "A":
                     if (dieRoll < 5)
                        return "Woods";
                     else if (dieRoll < 9 && "Nov" != month && "Feb" != month)
                        return "Open";
                     else if (dieRoll < 9)
                        return "Fortification";
                     else
                        return "Open";
                  case "B":
                     if (dieRoll < 4)
                        return "Woods";
                     else if (dieRoll < 8 && "Nov" != month && "Feb" != month)
                        return "Woods";
                     else if (dieRoll < 8)
                        return "Fortification";
                     else
                        return "Open";
                  case "C":
                     if (dieRoll < 3)
                        return "Woods";
                     else if (dieRoll < 9 && "Nov" != month && "Feb" != month)
                        return "Open";
                     else if (dieRoll < 9)
                        return "Fortification";
                     else
                        return "Open";
                  case "D":
                     if (dieRoll < 4)
                        return "Woods";
                     else if (dieRoll < 9 && "Nov" != month && "Feb" != month)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Fortification";
                     else
                        return "Open";
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "GetEnemyTerrain(): Reached Default areaType=" + areaType);
                     return "ERROR";
               }
            case "SPG":
            case "STuGIIIg":
            case "TANK":
            case "PzVIe":
            case "PzIV":
            case "PzV":
            case "PzVIb":
            case "PSW":
            case "SPW":
            case "TRUCK":
            case "MARDERII":
            case "MARDERIII":
            case "JdgPzIV":
            case "JdgPz38t":
               switch (areaType)
               {
                  case "A":
                     if (dieRoll < 5)
                        return "Hull Down";
                     else if (dieRoll < 7)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Open";
                     else
                        return "Moving in Open";
                  case "B":
                     if (dieRoll < 3)
                        return "Hull Down";
                     else if (dieRoll < 5)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Open";
                     else
                        return "Moving in Open";
                  case "C":
                     if (dieRoll < 6)
                        return "Hull Down";
                     else if (dieRoll < 7)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Open";
                     else
                        return "Moving in Open";
                  case "D":
                     if (dieRoll < 3)
                        return "Hull Down";
                     else if (dieRoll < 8)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Open";
                     else
                        return "Moving in Open";
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "GetEnemyTerrain(): Reached Default areaType=" + areaType);
                     return "ERROR";
               }
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetEnemyTerrain(): Reached Default enemy=" + enemyUnit);
               return "ERROR";
         }
      }
      public static int GetEnemyActionModifier(IGameInstance gi, IMapItem mi)
      {
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyActionModifier(): unknown enemyUnit=" + mi.Name);
            return -100;
         }
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyActionModifier(): lastReport=null");
            return -100;
         }
         //----------------------------------------------------
         int modifier = 0;
         switch (enemyUnit)
         {
            case "LW":
            case "MG":
               if (BattlePhase.Ambush == gi.BattlePhase)
                  modifier += 20;
               break;
            case "ATG":
            case "Pak43":
            case "Pak38":
            case "Pak40":
               if (BattlePhase.Ambush == gi.BattlePhase)
                  modifier += 20;
               break;
            case "TRUCK":
            case "PSW":
            case "SPW":
               if (BattlePhase.Ambush == gi.BattlePhase)
                  modifier += 10;
               break;
            case "TANK":
            case "PzIV":
            case "PzV":
            case "PzVIe":
            case "PzVIb":
            case "STuGIIIg":
            case "SPG":
            case "MARDERII":
            case "MARDERIII":
            case "JdgPzIV":
            case "JdgPz38t":
               if (BattlePhase.Ambush == gi.BattlePhase)
                  modifier += 10;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetEnemyActionModifier(): reached default with enemyUnit=" + enemyUnit);
               return -1;
         }
         return modifier;
      }
      public static string SetEnemyActionResult(IGameInstance gi, IMapItem mi, int dieRoll)
      {
         bool isThrownTrack = mi.IsThrownTrack;
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetEnemyActionResult(): unknown enemyUnit=" + mi.Name);
            return "ERROR";
         }
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetEnemyActionResult(): lastReport=null");
            return "ERROR";
         }
         bool isDoNothingInsteadOfFiring = false;  // If fog or falling snow, do not fire if not at close range
         string name = mi.TerritoryCurrent.Name;
         if ('C' != name[name.Length - 1])
         {
            string weather = lastReport.Weather;
            if (true == weather.Contains("Fog") || true == weather.Contains("Falling"))
               isDoNothingInsteadOfFiring = true;
         }
         //----------------------------------------------------
         bool isDoNothingInsteadOfFiringAtYourTank = false;
         if (EnumSpottingResult.HIDDEN == mi.Spotting)
            isDoNothingInsteadOfFiringAtYourTank = true;
         //----------------------------------------------------
         int modifier = GetEnemyActionModifier(gi, mi);
         if (modifier < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetEnemyActionResult(): GetEnemyActionModifier() returned error");
            return "ERROR";
         }
         dieRoll += modifier;
         //----------------------------------------------------
         if (EnumScenario.Advance == lastReport.Scenario)
         {
            switch (enemyUnit)
            {
               case "LW":
               case "MG":
                  if (dieRoll < 11)
                     return "Do Nothing";
                  if (dieRoll < 21)
                     return "Move-F";
                  if (dieRoll < 31)
                     return "Move-L";
                  if (dieRoll < 41)
                     return "Move-R";
                  if (dieRoll < 61)
                     return "Move-B";
                  if (dieRoll < 96)
                  {
                     if (true == isDoNothingInsteadOfFiring)
                        return "Do Nothing";
                     return "Fire-Infantry";
                  }
                  if (true == isDoNothingInsteadOfFiring)
                     return "Do Nothing";
                  return "Collateral";
               case "ATG":
               case "Pak43":
               case "Pak38":
               case "Pak40":
                  if (dieRoll < 21)
                     return "Move-B";
                  if (dieRoll < 31)
                     return "Do Nothing";
                  if (dieRoll < 66)
                  {
                     if (true == isDoNothingInsteadOfFiring)
                        return "Do Nothing";
                     else if ((null != gi.TargetMainGun) && (mi.Name == gi.TargetMainGun.Name))
                     {
                        if (true == isDoNothingInsteadOfFiringAtYourTank)
                           return "Do Nothing";
                        return "Fire-Your Tank";
                     }
                     else
                        return "Fire-Any Tank";
                  }
                  if (dieRoll < 71)
                  {
                     if (true == isDoNothingInsteadOfFiring)
                        return "Do Nothing";
                     return "Fire-Any Tank";
                  }
                  if (true == isDoNothingInsteadOfFiring)
                     return "Do Nothing";
                  if ((true == isDoNothingInsteadOfFiringAtYourTank) && (true == gi.IsLeadTank)) 
                     return "Do Nothing";
                  return "Fire-Lead Tank";
               case "TRUCK":
                  if (dieRoll < 31)
                     return "Do Nothing";
                  if (dieRoll < 41)
                     return "Move-F";
                  if (dieRoll < 51)
                     return "Move-L";
                  if (dieRoll < 61)
                     return "Move-R";
                  if (dieRoll < 91)
                     return "Move-B";
                  return "Do Nothing";
               case "PSW":
               case "SPW":
                  if (dieRoll < 31)
                     return "Do Nothing";
                  if (dieRoll < 41)
                  {
                     if( true == isThrownTrack )
                        return "Do Nothing";
                     else 
                        return "Move-F";
                  }
                  if (dieRoll < 51)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-L";
                  }
                  if (dieRoll < 61)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-R";
                  }
                  if (dieRoll < 91)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-B";
                  }
                  if (dieRoll < 96)
                  {
                     if (true == isDoNothingInsteadOfFiring)
                        return "Do Nothing";
                     return "Fire-Infantry";
                  }
                  if (true == isDoNothingInsteadOfFiring)
                     return "Do Nothing";
                  return "Collateral";
               case "TANK":
               case "PzIV":
               case "PzV":
               case "PzVIe":
               case "PzVIb":
               case "STuGIIIg":
               case "SPG":
               case "MARDERII":
               case "MARDERIII":
               case "JdgPzIV":
               case "JdgPz38t":
                  if (dieRoll < 11)
                     return "Do Nothing";
                  if (dieRoll < 21)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-F";
                  }
                  if (dieRoll < 31)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-L";
                  }
                  if (dieRoll < 41)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-R";
                  }
                  if (dieRoll < 61)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-B";
                  }
                  if (dieRoll < 66)
                     return "Fire-Infantry";
                  if (dieRoll < 81)
                  {
                     if (true == gi.IsShermanFiringAtFront)
                     {
                        if (true == isDoNothingInsteadOfFiring)
                           return "Do Nothing";
                        if (true == isDoNothingInsteadOfFiringAtYourTank)
                           return "Do Nothing";
                        return "Fire-Your Tank";
                     }
                     else if ((null != gi.TargetMainGun) && (mi.Name == gi.TargetMainGun.Name))
                     {
                        if (true == isThrownTrack)
                           return "Do Nothing";
                        else
                           return "Move-B";
                     }
                     else
                     {
                        if (true == isDoNothingInsteadOfFiring)
                           return "Do Nothing";
                        return "Fire-Any Tank";
                     }
                  }
                  if (dieRoll < 86)
                  {
                     if (true == isDoNothingInsteadOfFiring)
                        return "Do Nothing";
                     if (true == isDoNothingInsteadOfFiringAtYourTank)
                        return "Do Nothing";
                     return "Fire-Your Tank";
                  }
                  if (true == isDoNothingInsteadOfFiring)
                     return "Do Nothing";
                  if ((true == isDoNothingInsteadOfFiringAtYourTank) && (true == gi.IsLeadTank))
                     return "Do Nothing";
                  return "Fire-Lead Tank";
               default:
                  Logger.Log(LogEnum.LE_ERROR, "SetEnemyActionResult(): reached default with enemyUnit=" + enemyUnit);
                  return "ERROR";
            }
         }
         else if (EnumScenario.Battle == lastReport.Scenario)
         {
            switch (enemyUnit)
            {
               case "LW":
               case "MG":
                  if (dieRoll < 11)
                     return "Do Nothing";
                  if (dieRoll < 21)
                     return "Move-F";
                  if (dieRoll < 31)
                     return "Move-L";
                  if (dieRoll < 41)
                     return "Move-R";
                  if (dieRoll < 61)
                     return "Move-B";
                  if (dieRoll < 96)
                     return "Fire-Infantry";
                  return "Collateral";
               case "ATG":
               case "Pak43":
               case "Pak38":
               case "Pak40":
                  if (dieRoll < 21)
                     return "Move-B";
                  if (dieRoll < 31)
                     return "Do Nothing";
                  if (dieRoll < 81)
                  {
                     if (true == isDoNothingInsteadOfFiring)
                        return "Do Nothing";
                     else if ((null != gi.TargetMainGun) && (mi.Name == gi.TargetMainGun.Name))
                     {
                        if (true == isDoNothingInsteadOfFiringAtYourTank)
                           return "Do Nothing";
                        return "Fire-Your Tank";
                     }
                     else
                        return "Fire-Any Tank";
                  }
                  if (dieRoll < 91)
                  {
                     if (true == isDoNothingInsteadOfFiring)
                        return "Do Nothing";
                     return "Fire-Any Tank";
                  }
                  if (true == isDoNothingInsteadOfFiring)
                     return "Do Nothing";
                  if ((true == isDoNothingInsteadOfFiringAtYourTank) && (true == gi.IsLeadTank))
                     return "Do Nothing";
                  return "Fire-Lead Tank";
               case "TANK":
               case "PzIV":
               case "PzV":
               case "PzVIe":
               case "PzVIb":
               case "STuGIIIg":
               case "SPG":
               case "MARDERII":
               case "MARDERIII":
               case "JdgPzIV":
               case "JdgPz38t":
                  if (dieRoll < 11)
                     return "Do Nothing";
                  if (dieRoll < 16)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-F";
                  }
                  if (dieRoll < 21)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-L";
                  }
                  if (dieRoll < 26)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-R";
                  }
                  if (dieRoll < 36)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-B";
                  }
                  if (dieRoll < 41)
                     return "Fire-Infantry";
                  if (dieRoll < 86)
                  {
                     if (true == gi.IsShermanFiringAtFront)
                     {
                        if (true == isDoNothingInsteadOfFiring)
                           return "Do Nothing";
                        if (true == isDoNothingInsteadOfFiringAtYourTank)
                           return "Do Nothing";
                        return "Fire-Your Tank";
                     }
                     else if ((null != gi.TargetMainGun) && (mi.Name == gi.TargetMainGun.Name))
                     {
                        if (true == isThrownTrack)
                           return "Do Nothing";
                        else
                           return "Move-B";
                     }
                     else
                     {
                        if (true == isDoNothingInsteadOfFiring)
                           return "Do Nothing";
                        return "Fire-Any Tank";
                     }
                  }
                  if (dieRoll < 91)
                  {
                     if (true == isDoNothingInsteadOfFiring)
                        return "Do Nothing";
                     if (true == isDoNothingInsteadOfFiringAtYourTank)
                        return "Do Nothing";
                     return "Fire-Your Tank";
                  }
                  if (true == isDoNothingInsteadOfFiring)
                     return "Do Nothing";
                  if ((true == isDoNothingInsteadOfFiringAtYourTank) && (true == gi.IsLeadTank))
                     return "Do Nothing";
                  return "Fire-Lead Tank";
               default:
                  Logger.Log(LogEnum.LE_ERROR, "SetEnemyActionResult(): Battle - reached default with enemyUnit=" + enemyUnit);
                  return "ERROR";
            }
         }
         else if (EnumScenario.Counterattack == lastReport.Scenario)
         {
            switch (enemyUnit)
            {
               case "LW":
               case "MG":
                  if (dieRoll < 11)
                     return "Do Nothing";
                  if (dieRoll < 41)
                     return "Move-F";
                  if (dieRoll < 51)
                     return "Move-L";
                  if (dieRoll < 61)
                     return "Move-R";
                  if (dieRoll < 71)
                     return "Move-B";
                  if (dieRoll < 76)
                  {
                     if (true == isDoNothingInsteadOfFiring)
                        return "Do Nothing";
                     return "Fire-Infantry";
                  }
                  if (true == isDoNothingInsteadOfFiring)
                     return "Do Nothing";
                  return "Collateral";
               case "TANK":
               case "PzIV":
               case "PzV":
               case "PzVIe":
               case "PzVIb":
               case "STuGIIIg":
               case "SPG":
               case "MARDERII":
               case "MARDERIII":
               case "JdgPzIV":
               case "JdgPz38t":
                  if (dieRoll < 11)
                     return "Do Nothing";
                  if (dieRoll < 41)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-F";
                  }
                  if (dieRoll < 51)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-L";
                  }
                  if (dieRoll < 61)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-R";
                  }
                  if (dieRoll < 71)
                  {
                     if (true == isThrownTrack)
                        return "Do Nothing";
                     else
                        return "Move-B";
                  }
                  if (dieRoll < 76)
                     return "Fire-Infantry";
                  if (dieRoll < 96)
                  {
                     if (true == gi.IsShermanFiringAtFront)
                     {
                        if (true == isDoNothingInsteadOfFiring)
                           return "Do Nothing";
                        if (true == isDoNothingInsteadOfFiringAtYourTank)
                           return "Do Nothing";
                        return "Fire-Your Tank";
                     }
                     else if ((null != gi.TargetMainGun) && (mi.Name == gi.TargetMainGun.Name))
                     {
                        if (true == isThrownTrack)
                           return "Do Nothing";
                        else
                           return "Move-B";
                     }
                     else
                     {
                        if (true == isDoNothingInsteadOfFiring)
                           return "Do Nothing";
                        return "Fire-Any Tank";
                     }
                  }
                  if (true == isDoNothingInsteadOfFiring)
                     return "Do Nothing";
                  if (true == isDoNothingInsteadOfFiringAtYourTank)
                     return "Do Nothing";
                  return "Fire-Your Tank";
               default:
                  Logger.Log(LogEnum.LE_ERROR, "SetEnemyActionResult(): Counterattack - reached default with enemyUnit=" + enemyUnit);
                  return "ERROR";
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "SetEnemyActionResult(): reached default");
         return "ERROR";
      }
      public static ITerritory? SetNewTerritory(IMapItem mi, string move)
      {
         ITerritory oldT = mi.TerritoryCurrent;
         string? newTerritoryName = null;
         switch (oldT.Name)
         {
            case "B1C":
               if ("Move-F" == move)
                  newTerritoryName = "B4C";
               else if ("Move-L" == move)
                  newTerritoryName = "B9C";
               else if ("Move-R" == move)
                  newTerritoryName = "B2C";
               else if ("Move-B" == move)
                  newTerritoryName = "B1M";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B1M":
               if ("Move-F" == move)
                  newTerritoryName = "B1C";
               else if ("Move-L" == move)
                  newTerritoryName = "B9M";
               else if ("Move-R" == move)
                  newTerritoryName = "B2M";
               else if ("Move-B" == move)
                  newTerritoryName = "B1L";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B1L":
               if ("Move-F" == move)
                  newTerritoryName = "B1M";
               else if ("Move-L" == move)
                  newTerritoryName = "B9L";
               else if ("Move-R" == move)
                  newTerritoryName = "B2L";
               else if ("Move-B" == move)
                  newTerritoryName = "OffBottomRight";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B2C":
               if ("Move-F" == move)
                  newTerritoryName = "B6C";
               else if ("Move-L" == move)
                  newTerritoryName = "B1C";
               else if ("Move-R" == move)
                  newTerritoryName = "B3C";
               else if ("Move-B" == move)
                  newTerritoryName = "B2M";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B2M":
               if ("Move-F" == move)
                  newTerritoryName = "B2C";
               else if ("Move-L" == move)
                  newTerritoryName = "B1M";
               else if ("Move-R" == move)
                  newTerritoryName = "B3M";
               else if ("Move-B" == move)
                  newTerritoryName = "B2L";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B2L":
               if ("Move-F" == move)
                  newTerritoryName = "B2M";
               else if ("Move-L" == move)
                  newTerritoryName = "B1L";
               else if ("Move-R" == move)
                  newTerritoryName = "B3L";
               else if ("Move-B" == move)
                  newTerritoryName = "OffBottomRight";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B3C":
               if ("Move-F" == move)
                  newTerritoryName = "B9C";
               else if ("Move-L" == move)
                  newTerritoryName = "B2C";
               else if ("Move-R" == move)
                  newTerritoryName = "B4C";
               else if ("Move-B" == move)
                  newTerritoryName = "B3M";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B3M":
               if ("Move-F" == move)
                  newTerritoryName = "B3C";
               else if ("Move-L" == move)
                  newTerritoryName = "B2M";
               else if ("Move-R" == move)
                  newTerritoryName = "B4M";
               else if ("Move-B" == move)
                  newTerritoryName = "B3L";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B3L":
               if ("Move-F" == move)
                  newTerritoryName = "B3M";
               else if ("Move-L" == move)
                  newTerritoryName = "B2L";
               else if ("Move-R" == move)
                  newTerritoryName = "B4L";
               else if ("Move-B" == move)
                  newTerritoryName = "OffBottomLeft";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B4C":
               if ("Move-F" == move)
                  newTerritoryName = "B1C";
               else if ("Move-L" == move)
                  newTerritoryName = "B3C";
               else if ("Move-R" == move)
                  newTerritoryName = "B6C";
               else if ("Move-B" == move)
                  newTerritoryName = "B4M";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B4M":
               if ("Move-F" == move)
                  newTerritoryName = "B4C";
               else if ("Move-L" == move)
                  newTerritoryName = "B3M";
               else if ("Move-R" == move)
                  newTerritoryName = "B6M";
               else if ("Move-B" == move)
                  newTerritoryName = "B4L";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B4L":
               if ("Move-F" == move)
                  newTerritoryName = "B4M";
               else if ("Move-L" == move)
                  newTerritoryName = "B3L";
               else if ("Move-R" == move)
                  newTerritoryName = "B6L";
               else if ("Move-B" == move)
                  newTerritoryName = "OffTopLeft";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B6C":
               if ("Move-F" == move)
                  newTerritoryName = "B2C";
               else if ("Move-L" == move)
                  newTerritoryName = "B4C";
               else if ("Move-R" == move)
                  newTerritoryName = "B9C";
               else if ("Move-B" == move)
                  newTerritoryName = "B6M";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B6M":
               if ("Move-F" == move)
                  newTerritoryName = "B6C";
               else if ("Move-L" == move)
                  newTerritoryName = "B4M";
               else if ("Move-R" == move)
                  newTerritoryName = "B9M";
               else if ("Move-B" == move)
                  newTerritoryName = "B6L";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B6L":
               if ("Move-F" == move)
                  newTerritoryName = "B6M";
               else if ("Move-L" == move)
                  newTerritoryName = "B4L";
               else if ("Move-R" == move)
                  newTerritoryName = "B9L";
               else if ("Move-B" == move)
                  newTerritoryName = "OffTopRight";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B9C":
               if ("Move-F" == move)
                  newTerritoryName = "B3C";
               else if ("Move-L" == move)
                  newTerritoryName = "B6C";
               else if ("Move-R" == move)
                  newTerritoryName = "B1C";
               else if ("Move-B" == move)
                  newTerritoryName = "B9M";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B9M":
               if ("Move-F" == move)
                  newTerritoryName = "B9C";
               else if ("Move-L" == move)
                  newTerritoryName = "B6M";
               else if ("Move-R" == move)
                  newTerritoryName = "B1M";
               else if ("Move-B" == move)
                  newTerritoryName = "B9L";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            case "B9L":
               if ("Move-F" == move)
                  newTerritoryName = "B9M";
               else if ("Move-L" == move)
                  newTerritoryName = "B6L";
               else if ("Move-R" == move)
                  newTerritoryName = "B1L";
               else if ("Move-B" == move)
                  newTerritoryName = "OffTopRight";
               else
                  Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default move=" + move + " for oldT=" + oldT.Name);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): reached default oldT=" + oldT.Name);
               return null;
         }
         if (null == newTerritoryName)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): newTerritoryName = null move=" + move + " for oldT=" + oldT.Name);
            return null;
         }
         ITerritory? newT = Territories.theTerritories.Find(newTerritoryName);
         if (null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetNewTerritory(): newT=null newTerritoryName=" + newTerritoryName);
            return null;
         }
         return newT;
      }
      public static ITerritory? SetNewTerritoryShermanMove(IMapItem sherman, IMapItem mi, string originalMove)
      {
         if (EnumSpottingResult.HIDDEN == mi.Spotting)
            mi.Spotting = EnumSpottingResult.UNSPOTTED;
         ITerritory oldT = mi.TerritoryCurrent;
         string? newTerritoryName = null;
         double rotation = sherman.RotationHull;
         string move = originalMove;
         if( "A" == move )
         {
            if (180.0 == sherman.RotationHull)
            {
               move = "B";
               rotation = 0.0;
            }
            if (240 == sherman.RotationHull)
            {
               move = "B";
               rotation = 60.0;
            }
            if (120 == sherman.RotationHull)
            {
               move = "B";
               rotation = 300.0;
            }
         }
         else if ("B" == move)
         {
            if (180.0 == sherman.RotationHull)
            {
               move = "A";
               rotation = 0.0;
            }
            if (240 == sherman.RotationHull)
            {
               move = "A";
               rotation = 60.0;
            }
            if (120 == sherman.RotationHull)
            {
               move = "A";
               rotation = 300.0;
            }
         }
         else 
         {
            Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move);
            return null;
         }
         //----------------------------------
         switch (rotation)
         {
            case 0.0:
               switch(oldT.Name)
               {
                  case "B1C":
                     if ("A" == move)
                        newTerritoryName = "B1M";
                     else if ("B" == move)
                        newTerritoryName = "B9C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B1M":
                     if ("A" == move)
                        newTerritoryName = "B1L";
                     else if ("B" == move)
                        newTerritoryName = "B9M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B1L":
                     if ("A" == move)
                        newTerritoryName = "OffBottomRight";
                     else if ("B" == move)
                        newTerritoryName = "B9L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B2C":
                     if ("A" == move)
                        newTerritoryName = "B2M";
                     else if ("B" == move)
                        newTerritoryName = "B6C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B2M":
                     if ("A" == move)
                        newTerritoryName = "B2L";
                     else if ("B" == move)
                        newTerritoryName = "B2C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B2L":
                     if ("A" == move)
                        newTerritoryName = "OffBottomLeft";
                     else if ("B" == move)
                        newTerritoryName = "B2M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B3C":
                     if ("A" == move)
                        newTerritoryName = "B3M";
                     else if ("B" == move)
                        newTerritoryName = "B4C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B3M":
                     if ("A" == move)
                        newTerritoryName = "B3L";
                     else if ("B" == move)
                        newTerritoryName = "B4M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B3L":
                     if ("A" == move)
                        newTerritoryName = "OffBottomLeft";
                     else if ("B" == move)
                        newTerritoryName = "B4L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B4C":
                     if ("A" == move)
                        newTerritoryName = "B3C";
                     else if ("B" == move)
                        newTerritoryName = "B4M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B4M":
                     if ("A" == move)
                        newTerritoryName = "B3M";
                     else if ("B" == move)
                        newTerritoryName = "B4L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B4L":
                     if ("A" == move)
                        newTerritoryName = "B3L";
                     else if ("B" == move)
                        newTerritoryName = "OffTopLeft";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B6C":
                     if ("A" == move)
                        newTerritoryName = "B2C";
                     else if ("B" == move)
                        newTerritoryName = "B6M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B6M":
                     if ("A" == move)
                        newTerritoryName = "B6C";
                     else if ("B" == move)
                        newTerritoryName = "B6L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B6L":
                     if ("A" == move)
                        newTerritoryName = "B6M";
                     else if ("B" == move)
                        newTerritoryName = "OffTopRight";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B9C":
                     if ("A" == move)
                        newTerritoryName = "B1C";
                     else if ("B" == move)
                        newTerritoryName = "B9M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B9M":
                     if ("A" == move)
                        newTerritoryName = "B1M";
                     else if ("B" == move)
                        newTerritoryName = "B9L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "B9L":
                     if ("A" == move)
                        newTerritoryName = "B1L";
                     else if ("B" == move)
                        newTerritoryName = "OffTopRight";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
                  case "Home":
                     if ("A" == move)
                        newTerritoryName = "B2C";
                     else if ("B" == move)
                        newTerritoryName = "B6C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + rotation.ToString());
                     break;
               }
               break;
            case 60.0:
               switch (oldT.Name)
               {
                  case "B1C":
                     if ("A" == move)
                        newTerritoryName = "B2C";
                     else if ("B" == move)
                        newTerritoryName = "B1M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B1M":
                     if ("A" == move)
                        newTerritoryName = "B2M";
                     else if ("B" == move)
                        newTerritoryName = "B1L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B1L":
                     if ("A" == move)
                        newTerritoryName = "B2L";
                     else if ("B" == move)
                        newTerritoryName = "OffBottomRight";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B2C":
                     if ("A" == move)
                        newTerritoryName = "B2M";
                     else if ("B" == move)
                        newTerritoryName = "B1C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B2M":
                     if ("A" == move)
                        newTerritoryName = "B2L";
                     else if ("B" == move)
                        newTerritoryName = "B1M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B2L":
                     if ("A" == move)
                        newTerritoryName = "OffBottomLeft";
                     else if ("B" == move)
                        newTerritoryName = "B1L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B3C":
                     if ("A" == move)
                        newTerritoryName = "B3M";
                     else if ("B" == move)
                        newTerritoryName = "B9C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B3M":
                     if ("A" == move)
                        newTerritoryName = "B3L";
                     else if ("B" == move)
                        newTerritoryName = "B3C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B3L":
                     if ("A" == move)
                        newTerritoryName = "OffBottomLeft";
                     else if ("B" == move)
                        newTerritoryName = "B3M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B4C":
                     if ("A" == move)
                        newTerritoryName = "B4M";
                     else if ("B" == move)
                        newTerritoryName = "B6C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B4M":
                     if ("A" == move)
                        newTerritoryName = "B4L";
                     else if ("B" == move)
                        newTerritoryName = "B6M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B4L":
                     if ("A" == move)
                        newTerritoryName = "OffTopLeft";
                     else if ("B" == move)
                        newTerritoryName = "B6L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B6C":
                     if ("A" == move)
                        newTerritoryName = "B4C";
                     else if ("B" == move)
                        newTerritoryName = "B6M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B6M":
                     if ("A" == move)
                        newTerritoryName = "B4M";
                     else if ("B" == move)
                        newTerritoryName = "B6L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B6L":
                     if ("A" == move)
                        newTerritoryName = "B4L";
                     else if ("B" == move)
                        newTerritoryName = "OffTopLeft";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B9C":
                     if ("A" == move)
                        newTerritoryName = "B3C";
                     else if ("B" == move)
                        newTerritoryName = "B9M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B9M":
                     if ("A" == move)
                        newTerritoryName = "B9C";
                     else if ("B" == move)
                        newTerritoryName = "B9L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B9L":
                     if ("A" == move)
                        newTerritoryName = "B9M";
                     else if ("B" == move)
                        newTerritoryName = "OffTopRight";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "Home":
                     if ("A" == move)
                        newTerritoryName = "B3C";
                     else if ("B" == move)
                        newTerritoryName = "B9C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
               }
               break;
            case 300.0:
               switch (oldT.Name)
               {
                  case "B1C":
                     if ("A" == move)
                        newTerritoryName = "B1M";
                     else if ("B" == move)
                        newTerritoryName = "B4C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B1M":
                     if ("A" == move)
                        newTerritoryName = "B1L";
                     else if ("B" == move)
                        newTerritoryName = "B1C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B1L":
                     if ("A" == move)
                        newTerritoryName = "OffBottomRight";
                     else if ("B" == move)
                        newTerritoryName = "B1M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B2C":
                     if ("A" == move)
                        newTerritoryName = "B2M";
                     else if ("B" == move)
                        newTerritoryName = "B3C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B2M":
                     if ("A" == move)
                        newTerritoryName = "B2L";
                     else if ("B" == move)
                        newTerritoryName = "B3M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B2L":
                     if ("A" == move)
                        newTerritoryName = "OffBottomLeft";
                     else if ("B" == move)
                        newTerritoryName = "B3L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B3C":
                     if ("A" == move)
                        newTerritoryName = "B2C";
                     else if ("B" == move)
                        newTerritoryName = "B3M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B3M":
                     if ("A" == move)
                        newTerritoryName = "B2M";
                     else if ("B" == move)
                        newTerritoryName = "B3L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B3L":
                     if ("A" == move)
                        newTerritoryName = "B2L";
                     else if ("B" == move)
                        newTerritoryName = "OffBottomLeft";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B4C":
                     if ("A" == move)
                        newTerritoryName = "B1C";
                     else if ("B" == move)
                        newTerritoryName = "B4M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B4M":
                     if ("A" == move)
                        newTerritoryName = "B4C";
                     else if ("B" == move)
                        newTerritoryName = "B4L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B4L":
                     if ("A" == move)
                        newTerritoryName = "B4M";
                     else if ("B" == move)
                        newTerritoryName = "OffTopLeft";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B6C":
                     if ("A" == move)
                        newTerritoryName = "B9C";
                     else if ("B" == move)
                        newTerritoryName = "B6M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B6M":
                     if ("A" == move)
                        newTerritoryName = "B9M";
                     else if ("B" == move)
                        newTerritoryName = "B6L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B6L":
                     if ("A" == move)
                        newTerritoryName = "B9L";
                     else if ("B" == move)
                        newTerritoryName = "OffTopLeft";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B9C":
                     if ("A" == move)
                        newTerritoryName = "B9M";
                     else if ("B" == move)
                        newTerritoryName = "B6C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B9M":
                     if ("A" == move)
                        newTerritoryName = "B9L";
                     else if ("B" == move)
                        newTerritoryName = "B6M";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "B9L":
                     if ("A" == move)
                        newTerritoryName = "OffTopRight";
                     else if ("B" == move)
                        newTerritoryName = "B6L";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  case "Home":
                     if ("A" == move)
                        newTerritoryName = "B1C";
                     else if ("B" == move)
                        newTerritoryName = "B4C";
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default move=" + move + " for oldT=" + oldT.Name + " rot=" + sherman.RotationHull.ToString());
                     break;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): reached default rot=" + sherman.RotationHull.ToString() + " oldT=" + oldT.Name + " move=" + move + " rot=" + sherman.RotationHull.ToString());
               return null;
         }
         //----------------------------------
         if (null == newTerritoryName)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): newTerritoryName=null move=" + move + " for oldT=" + oldT.Name);
            return null;
         }
         ITerritory? newT = Territories.theTerritories.Find(newTerritoryName);
         if (null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetNewTerritoryShermanMove(): newT=null newTerritoryName=" + newTerritoryName);
            return null;
         }
         Logger.Log(LogEnum.LE_SHOW_SHERMAN_MOVE, "SetNewTerritoryShermanMove(): " + oldT.Name + "-->" + newTerritoryName + " m=" + originalMove + " r=" + sherman.RotationHull.ToString("F0"));
         return newT;
      }
      //-------------------------------------------
      public static double GetEnemyToKillNumberInfantry(IGameInstance gi, IMapItem mi, char sector, char range)
      {
         double toKillNum = 0;
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberInfantry(): unknown enemyUnit=" + mi.Name);
            return FN_ERROR;
         }
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberInfantry(): lastReport=null");
            return FN_ERROR;
         }
         //----------------------------------------------------
         if ( (EnumScenario.Advance == lastReport.Scenario) || (EnumScenario.Battle == lastReport.Scenario) )
         {
            switch (enemyUnit)
            {
               case "LW":
               case "SPW":
                  if ('C' == range)
                     toKillNum = 30;
                  else if ('M' == range)
                     toKillNum = 20;
                  else if ('L' == range)
                     toKillNum = 03;
                  break;
               case "MG":
               case "PSW":
                  if ('C' == range)
                     toKillNum = 55;
                  else if ('M' == range)
                     toKillNum = 30;
                  else if ('L' == range)
                     toKillNum = 03;
                  break;
               case "TANK":
               case "PzIV":
               case "PzV":
               case "PzVIe":
               case "PzVIb":
               case "STuGIIIg":
               case "SPG":
               case "MARDERII":
               case "MARDERIII":
               case "JdgPzIV":
               case "JdgPz38t":
                  if ('C' == range)
                     toKillNum = 65;
                  else if ('M' == range)
                     toKillNum = 40;
                  else if ('L' == range)
                     toKillNum = 10;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberInfantry(): Advance | Battle - reached default with enemyUnit=" + enemyUnit);
                  return FN_ERROR;
            }
         }
         else if (EnumScenario.Counterattack == lastReport.Scenario)
         {
            switch (enemyUnit)
            {
               case "LW":
                  if ('C' == range)
                     toKillNum = 20;
                  else if ('M' == range)
                     toKillNum = 10;
                  else if ('L' == range)
                     toKillNum = 03;
                  break;
               case "MG":
                  if ('C' == range)
                     toKillNum = 45;
                  else if ('M' == range)
                     toKillNum = 20;
                  else if ('L' == range)
                     toKillNum = 03;
                  break;
               case "TANK":
               case "PzIV":
               case "PzV":
               case "PzVI":
               case "STuGIIIg":
               case "SPG":
               case "MARDER":
               case "JdgPz":
                  if ('C' == range)
                     toKillNum = 55;
                  else if ('M' == range)
                     toKillNum = 30;
                  else if ('L' == range)
                     toKillNum = 05;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberInfantry(): Counterattack - reached default with enemyUnit=" + enemyUnit);
                  return FN_ERROR;
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberInfantry():  reached default with scearno=" + lastReport.Scenario);
            return FN_ERROR;
         }
         //------------------------------------
         int numSmokeMarkers = Territory.GetSmokeCount(gi, sector, range);
         if (numSmokeMarkers < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberTank(): GetSmokeCount() returned error");
            return FN_ERROR;
         }
         if ( 0 < numSmokeMarkers)
         {
            double multiplier = Math.Pow(0.5, numSmokeMarkers);
            toKillNum *= multiplier;
         }
         //------------------------------------
         if ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")))
            toKillNum *= 0.5;
         //------------------------------------
         return toKillNum;
      }
      public static double GetEnemyToKillNumberTank(IGameInstance gi, IMapItem mi, char sector, char range)
      {
         double toKillNum = 0.0;
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberTank(): unknown enemyUnit=" + mi.Name);
            return toKillNum;
         }
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberTank(): lastReport=null");
            return toKillNum;
         }
         //----------------------------------------------------
         if ( (EnumScenario.Advance == lastReport.Scenario) || (EnumScenario.Battle == lastReport.Scenario) )
         {
            switch (enemyUnit)
            {
               case "Pak38": // 50L
                  if ('C' == range)
                     toKillNum = 15;
                  else if ('M' == range)
                     toKillNum = 05;
                  else if ('L' == range)
                     toKillNum = 01;
                  break;
               case "Pak40": // 75L
               case "PzIV": 
               case "STuGIIIg":
               case "SPG":
               case "MARDERII":
               case "MARDERIII":
               case "JdgPzIV":
               case "JdgPz38t":
                  if ('C' == range)
                     toKillNum = 52;
                  else if ('M' == range)
                     toKillNum = 40;
                  else if ('L' == range)
                     toKillNum = 22;
                  break;
               case "PzV": // 75LL
                  if ('C' == range)
                     toKillNum = 68;
                  else if ('M' == range)
                     toKillNum = 66;
                  else if ('L' == range)
                     toKillNum = 61;
                  break;
               case "PzVIe":
               case "TANK": // 88L
                  if ('C' == range)
                     toKillNum = 68;
                  else if ('M' == range)
                     toKillNum = 63;
                  else if ('L' == range)
                     toKillNum = 43;
                  break;
               case "ATG":   // 88LL
               case "Pak43":
               case "PzVIb":
                  if ('C' == range)
                     toKillNum = 68;
                  else if ('M' == range)
                     toKillNum = 66;
                  else if ('L' == range)
                     toKillNum = 61;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberTank(): Advance | Battle - reached default with enemyUnit=" + enemyUnit);
                  return FN_ERROR;
            }
         }
         else if (EnumScenario.Counterattack == lastReport.Scenario)
         {
            switch (enemyUnit)
            {
               case "Pak40": // 75L
               case "PzIV":
               case "STuGIIIg":
               case "SPG":
               case "MARDERII":
               case "MARDERIII":
               case "JdgPzIV":
               case "JdgPz38t":
                  if ('C' == range)
                     toKillNum = 42;
                  else if ('M' == range)
                     toKillNum = 30;
                  else if ('L' == range)
                     toKillNum = 12;
                  break;
               case "PzV": // 75LL
                  if ('C' == range)
                     toKillNum = 58;
                  else if ('M' == range)
                     toKillNum = 56;
                  else if ('L' == range)
                     toKillNum = 51;
                  break;
               case "PzVIe":
               case "TANK": // 88L
                  if ('C' == range)
                     toKillNum = 58;
                  else if ('M' == range)
                     toKillNum = 53;
                  else if ('L' == range)
                     toKillNum = 33;
                  break;
               case "ATG":   // 88LL
               case "Pak43":
               case "PzVIb":
                  if ('C' == range)
                     toKillNum = 58;
                  else if ('M' == range)
                     toKillNum = 56;
                  else if ('L' == range)
                     toKillNum = 51;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberTank(): Counterattack - reached default with enemyUnit=" + enemyUnit);
                  return FN_ERROR;
            }
         }
         else 
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberTank(): reached default scenario=" + lastReport.Scenario);
            return FN_ERROR;
         }
         //------------------------------------
         int numSmokeMarkers = Territory.GetSmokeCount(gi, sector, range);
         if (numSmokeMarkers < 0 )
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberTank(): GetSmokeCount() returned error");
            return FN_ERROR;
         }
         if (0 < numSmokeMarkers)
         {
            double multiplier = Math.Pow(0.5, numSmokeMarkers);
            toKillNum *= multiplier;
         }
         //------------------------------------
         if ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")))
            toKillNum *= 0.5;
         //------------------------------------
         return toKillNum;
      }
      public static string GetCollateralDamage(IGameInstance gi, int dieRoll)
      {
         if (dieRoll < 51)
            return "No Effect";
         if (dieRoll < 53)
         {
            gi.IsBrokenPeriscopeDriver = true;
            return "Driver Periscope Broken";
         }
         if (dieRoll < 55)
         {
            gi.IsBrokenPeriscopeAssistant = true;
            return "Assistant Periscope Broken";
         }
         if (dieRoll < 58)
         {
            gi.IsBrokenPeriscopeGunner = true;
            return "Gunner Periscope Broken";
         }
         if (dieRoll < 61)
         {
            gi.IsBrokenPeriscopeLoader = true;
            return "Loader Periscope Broken";
         }
         if (dieRoll < 64)
         {
            gi.IsBrokenPeriscopeCommander = true;
            return "Commander Periscope Broken";
         }
         if (dieRoll < 66)
         {
            gi.IsBrokenGunSight = true;
            return "Gunsight Broken";
         }
         if (dieRoll < 71)
         {
            gi.IsBrokenMgAntiAircraft = true;
            return "AA MG Broken";
         }
         if (dieRoll < 76)
         {
            IMapItem? hatch = gi.Hatches.Find("Driver_OpenHatch");
            if (null != hatch)
               return "Driver Wounds";
            else
               return "No Effect";
         }
         if (dieRoll < 81)
         {
            IMapItem? hatch = gi.Hatches.Find("Asssistant_OpenHatch");
            if (null != hatch)
               return "Asssistant Wounds";
            else
               return "No Effect";
         }
         if (dieRoll < 91)
         {
            IMapItem? hatch = gi.Hatches.Find("Loader_OpenHatch");
            if (null != hatch)
               return "Loader Wounds";
            else
               return "No Effect";
         }
         if (dieRoll < 101)
         {
            IMapItem? hatch = gi.Hatches.Find("Commander_OpenHatch");
            if (null != hatch)
               return "Commander Wounds";
            else
               return "No Effect";
         }
         return "ERROR";
      }
      //-------------------------------------------
      public static string GetEnemyFireDirection(IGameInstance gi, IMapItem enemyUnit, string hitLocation)
      {
         int count = enemyUnit.TerritoryCurrent.Name.Count();
         if (3 != count)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyFireDirection(): 3 != enemyUnit.TerritoryCurrent.Name=" + enemyUnit.TerritoryCurrent.Name + " eu=" + enemyUnit.Name);
            return "ERROR";
         }
         char enemySector = enemyUnit.TerritoryCurrent.Name[count - 2];
         double rotation = 0.0;
         switch (enemySector)
         {
            case '6': rotation = 0.0; break;
            case '9': rotation = 60.0; break;
            case '1': rotation = 120.0; break;
            case '2': rotation = 180.0; break;
            case '3': rotation = 240.0; break;
            case '4': rotation = 300.0; break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetEnemyFireDirection(): reached default enemySector=" + enemySector + " eu=" + enemyUnit.Name);
               return "ERROR";
         }
         //--------------------------------------------------------------
         double totalRotation = rotation - gi.Sherman.RotationHull;
         double or = totalRotation;
         if ("Hull" == hitLocation)
         {
            while (totalRotation < 0.0)
               totalRotation += 360.0;
            while (359.9 < totalRotation)
               totalRotation -= 360.0;
            Logger.Log(LogEnum.LE_SHOW_FIRE_DIRECTION, "GetEnemyFireDirection(): hull: (total=" + totalRotation.ToString("F1") + ") = (r=" + rotation.ToString("F1") + ") - (hr=" + gi.Sherman.RotationHull.ToString("F1") + ") - (tr=" + gi.Sherman.RotationTurret.ToString("F1") + ")  or=" + or.ToString("F1") + " eu=" + enemyUnit.Name);
            switch (totalRotation)
            {
               case 0.0: return "H F";
               case 60.0: return "H FR";
               case 120.0: return "H BR";
               case 180.0: return "H B";
               case 240.0: return "H BL";
               case 300.0: return "H FL";
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemyFireDirection(): hull: (total=" + totalRotation.ToString("F1") + ") = (r=" + rotation.ToString("F1") + ") - (hr=" + gi.Sherman.RotationHull.ToString("F1") + ") - (tr=" + gi.Sherman.RotationTurret.ToString("F1") + ")  or=" + or.ToString("F1") + " eu=" + enemyUnit.Name);
                  return "ERROR";
            }
         }
         if ("Turret" == hitLocation)
         {
            totalRotation -= gi.Sherman.RotationTurret;
            while (totalRotation < 0.0)
               totalRotation += 360.0;
            while (359.9 < totalRotation)
               totalRotation -= 360.0;
            Logger.Log(LogEnum.LE_SHOW_FIRE_DIRECTION, "GetEnemyFireDirection(): turret: (total=" + totalRotation.ToString("F1") + ") = (r=" + rotation.ToString("F1") + ") - (hr=" + gi.Sherman.RotationHull.ToString("F1") + ") - (tr=" + gi.Sherman.RotationTurret.ToString("F1") + ")  or=" + or.ToString("F1") + " eu=" + enemyUnit.Name);
            switch(totalRotation)
            {
               case 0.0: return "T F";
               case 60.0: return "T R";
               case 120.0: return "T R";
               case 180.0: return "T B";
               case 240.0: return "T L";
               case 300.0: return "T L";
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemyFireDirection(): turret: (total=" + totalRotation.ToString("F1") + ") = (r=" + rotation.ToString("F1") + ") - (hr=" + enemyUnit.RotationHull.ToString("F1") + ") - (tr=" + enemyUnit.RotationTurret.ToString("F1") + ")  or=" + or.ToString("F1") + " eu=" + enemyUnit.Name);
                  return "ERROR";
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "GetEnemyFireDirection(): reached default hitLocation=" + hitLocation);
         return "ERROR";
      }
      public static int GetEnemyToHitNumberModifierForYourTank(IGameInstance gi, IMapItem mi, char range)
      {
         int toHitModifierNum = 0;
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemy_ToHitNumberYourTank(): unknown enemyUnit=" + mi.Name);
            return FN_ERROR;
         }
         if( false == gi.FirstShots.ContainsKey(mi.Name))
         {
            toHitModifierNum += 10; // add 10 if first shot
            Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetEnemy_ToHitNumberYourTank(): eu=" + mi.Name + " +10 mod=" + toHitModifierNum.ToString() + " firstShot");
         }
         //-----------------------------------------------
         if (true == gi.AcquiredShots.ContainsKey(mi.Name))  // GetEnemy_ToHitNumberYourTank()
         {
            if (1 < gi.AcquiredShots[mi.Name]) // GetEnemy_ToHitNumberYourTank()
            {
               if ('C' == range)
               {
                  toHitModifierNum -= 10;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetEnemy_ToHitNumberYourTank(): eu=" + mi.Name + " -10 mod=" + toHitModifierNum.ToString() + " acquire-2 close");
               }
               else if ('M' == range)
               {
                  toHitModifierNum -= 10;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetEnemy_ToHitNumberYourTank(): eu=" + mi.Name + " -20 mod=" + toHitModifierNum.ToString() + " acquire-2 medium");
               }
               else if ('L' == range)
               {
                  toHitModifierNum -= 30;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetEnemy_ToHitNumberYourTank(): eu=" + mi.Name + " -30 mod=" + toHitModifierNum.ToString() + " acquire-2 long");
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemy_ToHitNumberYourTank(): 1-unknown range=" + range);
                  return FN_ERROR;
               }
            }
            else // acquired 1 marker
            {
               if ('C' == range)
               {
                  toHitModifierNum -= 5;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetEnemy_ToHitNumberYourTank(): eu=" + mi.Name + " -5 mod=" + toHitModifierNum.ToString() + " acquire-1 close");
               }
               else if ('M' == range)
               {
                  toHitModifierNum -= 10;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetEnemy_ToHitNumberYourTank(): eu=" + mi.Name + " -10 mod=" + toHitModifierNum.ToString() + " acquire-1 medium");
               }
               else if ('L' == range)
               {
                  toHitModifierNum -= 15;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetEnemy_ToHitNumberYourTank(): eu=" + mi.Name + " -15 mod=" + toHitModifierNum.ToString() + " acquire-1 long");
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemy_ToHitNumberYourTank(): 2-unknown range=" + range);
                  return FN_ERROR;
               }
            }
         }
         //-----------------------------------------------
         if( true == gi.Sherman.IsMoving )
         {
            toHitModifierNum += 20;
            Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetEnemy_ToHitNumberYourTank(): eu=" + mi.Name + " +20 mod=" + toHitModifierNum.ToString() + " moving");
         }
         return toHitModifierNum;
      }
      public static double GetEnemyToHitNumberYourTank(IGameInstance gi, IMapItem mi, char sector, char range)
      {
         double toHitNum = 0.0;
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberTank(): lastReport=null");
            return FN_ERROR;
         }
         //----------------------------------------------------
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToHitNumberYourTank(): unknown enemyUnit=" + mi.Name);
            return FN_ERROR;
         }
         switch (enemyUnit)
         {
            case "Pak38":
               if ('C' == range)
                  toHitNum = 97;
               else if ('M' == range)
                  toHitNum = 80;
               else if ('L' == range)
                  toHitNum = 34;
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemyToHitNumberYourTank(): unknown range=" + range.ToString());
                  return FN_ERROR;
               }
               break;
            case "Pak40":
            case "SPG":
            case "STuGIIIg":
            case "TANK":
            case "PzVIe":
            case "PzIV":
            case "MARDERII":
            case "MARDERIII":
            case "JdgPzIV":
            case "JdgPz38t":
               if ('C' == range)
                  toHitNum = 97;
               else if ('M' == range)
                  toHitNum = 89;
               else if ('L' == range)
                  toHitNum = 67;
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemyToHitNumberYourTank(): unknown range=" + range.ToString());
                  return FN_ERROR;
               }
               break;
            case "ATG":
            case "Pak43":
            case "PzV":
            case "PzVIb":
               if ('C' == range)
                  toHitNum = 97;
               else if ('M' == range)
                  toHitNum = 89;
               else if ('L' == range)
                  toHitNum = 79;
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemyToHitNumberYourTank(): unknown range=" + range.ToString());
                  return FN_ERROR;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetEnemyToHitNumberYourTank(): Reached Default enemyUnit=" + enemyUnit);
               return FN_ERROR;
         }
         //------------------------------------
         int numSmokeMarkers = Territory.GetSmokeCount(gi, sector, range);
         if (numSmokeMarkers < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberTank(): GetSmokeCount() returned error");
            return FN_ERROR;
         }
         if (0 < numSmokeMarkers)
         {
            double multiplier = Math.Pow(0.5, numSmokeMarkers);
            toHitNum *= multiplier;
         }
         //------------------------------------
         if ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")))
            toHitNum *= 0.5;
         return toHitNum;
      }
      public static string GetEnemyHitLocationYourTank(IGameInstance gi, int dieRoll)
      {
         if( true == gi.Sherman.IsHullDown )
         {
            if (dieRoll < 6)
               return "Turret";
            else if( dieRoll < 11)
               return "Miss";
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyHitLocationYourTank(): 1-dieRoll=" + dieRoll.ToString());
            return "ERROR";
         }
         else
         {
            if (dieRoll < 5)
               return "Turret";
            else if (dieRoll < 10)
               return "Hull";
            else if (dieRoll == 10)
               return "Track";
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyHitLocationYourTank(): 2-dieRoll=" + dieRoll.ToString());
            return "ERROR";
         }
      }
      public static double GetEnemyToKillNumberYourTank(IGameInstance gi, IMapItem mi, string facing, char range, string hitLocation)
      {
         double toKillNum = 0.0;
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberYourTank(): unknown enemyUnit=" + mi.Name);
            return FN_ERROR;
         }
         //----------------------------------------------------
         string gun = "Unknown";
         switch (enemyUnit)
         {
            case "Pak38":
               gun = "50L";
               break;
            case "Pak40":
            case "SPG":
            case "STuGIIIg":
            case "PzIV":
            case "MARDERII":
            case "MARDERIII":
            case "JdgPzIV":
            case "JdgPz38t":
               gun = "75L";
               break;
            case "PzV":
               gun = "75LL";
               break;
            case "TANK":
            case "PzVIe":
               gun = "88L";
               break;
            case "ATG":
            case "Pak43":
            case "PzVIb":
               gun = "88LL";
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetEnemyToHitNumberYourTank(): Reached Default enemyUnit=" + enemyUnit);
               return FN_ERROR;
         }
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyToKillNumberYourTank(): lastReport=null");
            return FN_ERROR;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         int armorclass = 0;
         if ("II" == card.myArmorClass)
            armorclass = 1;
         else if ("III" == card.myArmorClass)
            armorclass = 2;
         //----------------------------------------------------
         int facingNum = 0;
         if ("Side" == facing)
            facingNum = 1;
         else if ("Rear" == facing)
            facingNum = 2;
         //----------------------------------------------------
         int rangeNum = 0;
         if ('M' == range)
            rangeNum = 1;
         else if ('L' == range)
            rangeNum = 2;
         //----------------------------------------------------
         int locationNum = 0;
         if ("Turret" == hitLocation)
            locationNum = 1;
         //----------------------------------------------------
         int[,,,] table = theApToKills[gun];
         toKillNum = table[armorclass, facingNum, rangeNum, locationNum];
         return toKillNum;
      }
      public static int GetExplosionModifier(IGameInstance gi, IMapItem mi, string hitLocation)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ExplosionModifier(): lastReport=null");
            return FN_ERROR;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //----------------------------------
         int modifier = 0;
         //----------------------------------
         if( "Hull" == hitLocation )
            modifier += 5;
         //----------------------------------
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ExplosionModifier(): GetEnemyUnit() returned error for mi=" + mi.Name);
            return FN_ERROR;
         }
         if (("ATG" == enemyUnit) || ("Pak43" == enemyUnit) || ("PzVIb" == enemyUnit) || ("TANK" == enemyUnit) || ("PzVIe" == enemyUnit) )
            modifier += 5;
         //----------------------------------
         int roundsOnBoard = lastReport.MainGunHE + lastReport.MainGunAP + lastReport.MainGunWP + lastReport.MainGunHBCI + lastReport.MainGunHVAP;
         int diff = roundsOnBoard - card.myNumMainGunRound;
         if( 0 < diff )
         {
            int extraAmmoMod = (int)Math.Ceiling((double)diff / 2.0);
            modifier += extraAmmoMod;
         }
         //----------------------------------
         if (false == card.myIsWetStowage)
            modifier += 5;
         return modifier;
      }
      //-------------------------------------------
      public static int GetWoundsModifier(IGameInstance gi, ICrewMember cm, bool isGunFire, bool isBailout, bool isCollateralDamage)
      {
         int modifier = 0;
         //----------------------------------
         if( true == isGunFire )
         {
            ShermanDeath? death = gi.Death;
            if (null != death)
            {
               switch (death.myEnemyFireDirection)
               {
                  case "T F":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                     {
                        modifier -= 20;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-20 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     break;
                  case "T R":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                     {
                        modifier -= 20;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-20 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     if ("Gunner" == cm.Role)
                     {
                        modifier += 10;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod+10 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     if ("Loader" == cm.Role)
                     {
                        modifier -= 10;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-10 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     break;
                  case "T L":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                     {
                        modifier -= 20;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-20 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     if ("Gunner" == cm.Role)
                     {
                        modifier -= 10;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-10 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     if ("Loader" == cm.Role)
                     {
                        modifier += 10;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod+10 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     break;
                  case "T B":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                     {
                        modifier -= 20;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-20 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     if ("Commander" == cm.Role)
                     {
                        modifier += 10;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod+10 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     break;
                  case "H F":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                     {
                        modifier += 10;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod+10 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     if ("Commander" == cm.Role)
                     {
                        modifier -= 10;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-10 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     break;
                  case "H FR":
                     if (("Driver" == cm.Role) || ("Loader" == cm.Role))
                     {
                        modifier += 10;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod+10 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     if (("Assistant" == cm.Role) || ("Gunner" == cm.Role))
                     {
                        modifier += 10;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod+10 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     break;
                  case "H BR":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                     {
                        modifier -= 40;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-40 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     else
                     {
                        modifier -= 30;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-30 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     break;
                  case "H FL":
                     if (("Driver" == cm.Role) || ("Loader" == cm.Role))
                     {
                        modifier += 10;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod+10 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     if (("Assistant" == cm.Role) || ("Gunner" == cm.Role))
                     {
                        modifier -= 10;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-10 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     break;
                  case "H BL":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                     {
                        modifier -= 40;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-40 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     else
                     {
                        modifier -= 30;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-30 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     break;
                  case "H B":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                     {
                        modifier -= 40;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-40 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     else
                     {
                        modifier -= 30;
                        Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-30 direction=" + death.myEnemyFireDirection + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
                     }
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "Get_ExplosionModifier(): reached default for direction=" + death.myEnemyFireDirection);
                     return FN_ERROR;
               }
            }
         }

         //----------------------------------
         if( true == isBailout)
         {
            modifier -= cm.Rating;
            Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-rating=" + cm.Rating.ToString() + " mod=" + modifier.ToString() + " for cm=" + cm.Role);
         }
         if (true == gi.IsMinefieldAttack)
         {
            modifier -= 20;
            Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-20 IsMinefieldAttack mod=" + modifier.ToString() + " for cm=" + cm.Role);
         }
         if (true == gi.IsHarrassingFireBonus)
         {
            modifier -= 10;
            Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod-10 IsHarrassingFire_Bonus mod=" + modifier.ToString() + " for cm=" + cm.Role);
         }
         //----------------------------------
         if( true == isCollateralDamage )
         {
            if ((true == cm.Action.Contains("FireSubMg")) || (true == cm.Action.Contains("FireAaMg")))
            {
               modifier += 5;
               Logger.Log(LogEnum.LE_SHOW_WOUND_MOD, "Get_ExplosionModifier(): mod+5 MG and isCollateralDamage=true mod=" + modifier.ToString() + " for cm=" + cm.Role);
            }
         }
         return modifier;
      }
      public static string GetWounds(IGameInstance gi, ICrewMember cm, int dieRoll, bool isGunFire, bool isBailout, bool isCollateralDamage)
      {
         if (100 == dieRoll) // unmodified die roll 100 is always a kill
            return "Killed";
         int modifier = GetWoundsModifier(gi, cm, isGunFire, isBailout, isCollateralDamage);
         if (modifier < -100)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetWounds(): GetWoundsModifier() returned error");
            return "ERROR";
         }
         dieRoll += modifier;
         if (dieRoll < 42)
            return "Near Miss";
         else if (dieRoll < 48)
         {
            if ((true == gi.IsMinefieldAttack) || (null == gi.Death))
               return "Unconscious";
            else
               return "Light Wound";
         }
         else if (dieRoll < 73)
         {
            return "Light Wound";
         }
         else if (dieRoll < 88)
         {
            return "Light Wound";
         }
         else if (dieRoll < 93)
         {
            return "Serious Wound";
         }
         else if (dieRoll < 98)
         {
            return "Serious Wound";
         }
         else
         {
            return "Killed";
         }
      }
      public static string SetWounds(IGameInstance gi, ICrewMember cm, int dieRoll, int modifier)
      {
         if( 100 == dieRoll ) // unmodified die roll 100 is always a kill
         {
            gi.SetIncapacitated(cm);
            cm.SetBloodSpots(40);
            cm.IsKilled = true;
            if ("Commander" == cm.Role)
               gi.IsCommanderKilled = true; // SetWounds()
            cm.Wound = "Killed";
            return "Killed";
         }
         dieRoll += modifier;
         if (dieRoll < 42)
            return "Near Miss";
         else if (dieRoll < 48)
         {
            cm.SetBloodSpots(5);
            if ((true == gi.IsMinefieldAttack) || (null == gi.Death))
            {
               gi.SetIncapacitated(cm);
               cm.IsUnconscious = true;
               return "Unconscious";
            }
            else
            {
               if ("None" == cm.Wound)
                  cm.Wound = "Light Wound";
               return "Light Wound";
            }
         }
         else if (dieRoll < 73)
         {
            cm.SetBloodSpots(10);
            if ("None" == cm.Wound)
               cm.Wound = "Light Wound";
            return "Light Wound";
         }
         else if (dieRoll < 88)
         {
            gi.SetIncapacitated(cm);
            cm.SetBloodSpots(20);
            if ("None" == cm.Wound)
               cm.Wound = "Light Wound";
            return "Light Wound";
         }
         else if (dieRoll < 93)
         {
            gi.SetIncapacitated(cm);
            cm.SetBloodSpots(30);
            if ("Killed" != cm.Wound)
               cm.Wound = "Serious Wound";
            return "Serious Wound";
         }
         else if (dieRoll < 98)
         {
            gi.SetIncapacitated(cm);
            cm.SetBloodSpots(35);
            gi.SetIncapacitated(cm);
            if ("Killed" != cm.Wound)
               cm.Wound = "Serious Wound";
            return "Serious Wound";
         }
         else
         {
            gi.SetIncapacitated(cm);
            cm.SetBloodSpots(40);
            cm.IsKilled = true;
            cm.Wound = "Killed";
            if ("Commander" == cm.Role)
               gi.IsCommanderKilled = true; // SetWounds()
            return "Killed";
         }
      }
      public static string SetWoundEffect(IGameInstance gi, ICrewMember cm, int dieRoll, int modifier)
      {
         if (100 == dieRoll) // unmodified die roll 100 is always a kill
         {
            cm.WoundDaysUntilReturn = KIA;
            return "Incapacitated";
         }
         dieRoll += modifier;
         if (dieRoll < 42)
         {
            return "None";
         }
         else if (dieRoll < 48)
         {
            if ((true == gi.IsMinefieldAttack) || (null == gi.Death))
            {
               cm.WoundDaysUntilReturn = 0; // may return the next day
               return "Incapacitated";
            }
            else
            {
               return "None";
            }
         }
         else if (dieRoll < 73)
         {
            return "None";
         }
         else if (dieRoll < 88)
         {
            cm.WoundDaysUntilReturn = 7; 
            return "Out 1 week";
         }
         else if (dieRoll < 93)
         {
            cm.WoundDaysUntilReturn = 70; 
            return "Out 10 weeks";
         }
         else if (dieRoll < 98)
         {
            cm.WoundDaysUntilReturn = MIA;
            return "Sent Home";
         }
         else
         {
            cm.WoundDaysUntilReturn = KIA;
            return "Incapacitated";
         }
      }
      public static string GetBailoutEffectResult(IGameInstance gi, ICrewMember cm, int dieRoll, int modifier)
      {
         if (100 == dieRoll) // unmodified die roll 100 is always a kill
            return "Cannot Bail";
         dieRoll += modifier;
         if (dieRoll < 42)
            return "None";
         else if (dieRoll < 48)
         {
            if ((true == gi.IsMinefieldAttack) || (null == gi.Death))
               return "Cannot Bail";
            else
               return "None";
         }
         else if (dieRoll < 73)
         {
            return "None";
         }
         else if (dieRoll < 88)
         {
            return "None";
         }
         else if (dieRoll < 93)
         {
            return "Bail out +2";
         }
         else if (dieRoll < 98)
         {
            return "Cannot Bail";
         }
         else
         {
            return "Cannot Bail";
         }
      }
      public static int GetBrewUpNumber(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_BrewUpNumber(): lastReport=null");
            return FN_ERROR;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         if (null == gi.Death)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_BrewUpNumber(): gi.Death=null");
            return FN_ERROR;
         }
         //-----------------------------------------------
         bool isWetStorage = card.myIsWetStowage;
         if( true == isWetStorage) // wet stowage only applies if not carrying extra rounds
         {
            int roundsOnBoard = lastReport.MainGunHE + lastReport.MainGunAP + lastReport.MainGunWP + lastReport.MainGunHBCI + lastReport.MainGunHVAP;
            int diff = roundsOnBoard - card.myNumMainGunRound;
            if (0 < diff) // extra rounds on board if positive
               isWetStorage = false;
         }
         //-----------------------------------------------
         ShermanDeath death = gi.Death;
         if( true == death.myCause.Contains("Panzerfaust"))
         {
            if (true == isWetStorage)
               return 19;
            else if (true == card.myChasis.Contains("M4A1"))
               return 79;
            else if (true == card.myChasis.Contains("M4A3"))
               return 74;
            else if (true == card.myChasis.Contains("M4"))
               return 84;
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_BrewUpNumber(): reached default for Panzerfaust myChassis=" + card.myChasis);
               return FN_ERROR;
            }
         }
         else // gunfire
         {
            if (true == isWetStorage)
               return 15;
            else if (true == card.myChasis.Contains("M4A1"))
               return 75;
            else if (true == card.myChasis.Contains("M4A3"))
               return 69;
            else if (true == card.myChasis.Contains("M4"))
               return 79;
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_BrewUpNumber(): reached default for Panzerfaut myChassis=" + card.myChasis);
               return FN_ERROR;
            }
         }
      }
      //-------------------------------------------
      public static string GetRandomEvent(EnumScenario scenario, int dieRoll)
      {
         //dieRoll = 80; // <cgs> TEST - Random Event
         switch(scenario)
         {
            case EnumScenario.Advance:
               if (dieRoll < 6)
                  return "Time Passes";
               if (dieRoll < 16)
                  return "Friendly Artillery";
               if (dieRoll < 21)
                  return "Enemy Artillery";
               if (dieRoll < 26)
                  return "Mines";
               if (dieRoll < 31)
                  return "Panzerfaust";
               if (dieRoll < 36)
                  return "Harrassing Fire";
               if (dieRoll < 61)
                  return "Friendly Advance";
               if (dieRoll < 81)
                  return "Enemy Reinforce";
               return "Flanking Fire";
            case EnumScenario.Battle:
               if (dieRoll < 6)
                  return "Time Passes";
               if (dieRoll < 21)
                  return "Friendly Artillery";
               if (dieRoll < 31)
                  return "Enemy Artillery";
               if (dieRoll < 36)
                  return "Mines";
               if (dieRoll < 41)
                  return "Panzerfaust";
               if (dieRoll < 46)
                  return "Harrassing Fire";
               if (dieRoll < 61)
                  return "Friendly Advance";
               if (dieRoll < 81)
                  return "Enemy Reinforce";
               if (dieRoll < 86)
                  return "Enemy Advance";
               return "Flanking Fire";
            case EnumScenario.Counterattack:
               if (dieRoll < 6)
                  return "Time Passes";
               if (dieRoll < 26)
                  return "Friendly Artillery";
               if (dieRoll < 36)
                  return "Enemy Artillery";
               if (dieRoll < 41)
                  return "Panzerfaust";
               if (dieRoll < 46)
                  return "Harrassing Fire";
               if (dieRoll < 51)
                  return "Friendly Advance";
               if (dieRoll < 76)
                  return "Enemy Reinforce";
               if (dieRoll < 91)
                  return "Enemy Advance";
               return "Flanking Fire";
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetRandomEvent(): reached default scenario=" + scenario.ToString());
               return "ERROR";
         }
      }
      //-------------------------------------------
      public static int GetSpottingModifier(IGameInstance gi, IMapItem mi, ICrewMember cm, char sector, char range)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetSpottingModifier(): lastReport=null");
            return FN_ERROR;
         }
         //--------------------------------------------------------------
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetSpottingModifier(): unknown enemyUnit=" + mi.Name);
            return FN_ERROR;
         }
         //-------------------------------------------------------
         Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): cm=" + cm.Name + " at eu=" + enemyUnit + " ----------------------------------------------");
         TankCard card = new TankCard(lastReport.TankCardNum);
         int spottingModifer = 0;
         if (true == cm.IsButtonedUp)
         {
            if( ("Commander" == cm.Role ) && (true == card.myIsVisionCupola) )
            {
               spottingModifer += 2;
               Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): bu=+2 mod=" + spottingModifer.ToString());
            }
            else
            {
               spottingModifer += 3;
               Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): bu=+3 mod=" + spottingModifer.ToString());
            }
         }
         if( true == gi.Sherman.IsMoving )
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): ==> move+1 mod=" + spottingModifer.ToString());
         }
         if( true == mi.IsWoods )
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): woods+1 mod=" + spottingModifer.ToString());
         }
         if (true == mi.IsBuilding)
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): build+1 mod=" + spottingModifer.ToString());
         }
         if (true == mi.IsFortification)
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): fort+1 mod=" + spottingModifer.ToString());
         }
         if (true == mi.IsHullDown)
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): hull+1 mod=" + spottingModifer.ToString());
         }
         if ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")))
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): fog+1 mod=" + spottingModifer.ToString());
         }
         int numSmokeMarkers = Territory.GetSmokeCount(gi, sector, range);
         if (numSmokeMarkers < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetSpottingModifier(): GetSmokeCount() returned error");
            return FN_ERROR;
         }
         spottingModifer += numSmokeMarkers;
         Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): smoke+" + numSmokeMarkers.ToString() + " mod=" + spottingModifer.ToString());
         //----------------------------------------------------
         switch (enemyUnit)
         {
            case "ATG":
            case "Pak38":
            case "Pak40":
            case "Pak43":
            case "PzIV":
            case "MARDERII":
            case "MARDERIII":
               break;
            case "SPG":
            case "STuGIIIg":
            case "JdgPzIV":
            case "JdgPz38t":
               spottingModifer += 1;
               Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): size+1 mod=" + spottingModifer.ToString());
               break;
            case "TANK":
            case "PzVIe":
            case "PzV":
               spottingModifer -= 1;
               Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): size-1 mod=" + spottingModifer.ToString());
               break;
            case "PzVIb":
               spottingModifer -= 2;
               Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): size-2 mod=" + spottingModifer.ToString());
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetSpottingModifier(): Reached Default enemyUnit=" + enemyUnit);
               return FN_ERROR;
         }

         //----------------------------------------------------
         if ( 'M' == range )
         {
            spottingModifer -= 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): range-1 mod=" + spottingModifer.ToString());
         }
         else if ('C' == range)
         {
            spottingModifer -= 2;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): range-2 mod=" + spottingModifer.ToString());
         }
         //----------------------------------------------------
         if (true == mi.IsFired)
         {
            spottingModifer -= 2;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): fired-2 mod=" + spottingModifer.ToString());
         }
         //----------------------------------------------------
         if ( true == mi.IsMoving )
         {
            spottingModifer -= 3;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): move-3 mod=" + spottingModifer.ToString());
         }
         //----------------------------------------------------
         if (true == mi.IsSpotted)
         {
            spottingModifer -= 3;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): spot-3 mod=" + spottingModifer.ToString());
         }
         return spottingModifer;
      }
      public static string GetSpottingResult(IGameInstance gi, IMapItem mi, ICrewMember cm, char sector, char range, int dieRoll)
      {
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetSpottingResult(): lastReport=null");
            return "ERROR";
         }
         if ( ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling"))) && ('C' != range) )
         {
            Logger.Log(LogEnum.LE_SHOW_SPOT_RESULT, "GetSpottingResult(): mi=" + mi.Name + " Unspotted - Fog/Falling & range=" + range.ToString());
            return "Unspotted";
         }
         //----------------------------------------------------
         if ( 1 == dieRoll ) // an unmodified roll of 1 always spots and ids it
         {
            mi.Spotting = EnumSpottingResult.IDENTIFIED;
            Logger.Log(LogEnum.LE_SHOW_SPOT_RESULT, "GetSpottingResult(): mi=" + mi.Name + " Identified dr=" + dieRoll.ToString());
            return "Identified";
         }
         if ((9 == dieRoll) || (10 == dieRoll)) // an unmodified roll of 9-10 means target is hidden
         {
            if ( (EnumSpottingResult.IDENTIFIED != mi.Spotting) && (EnumSpottingResult.SPOTTED != mi.Spotting))
            {
               Logger.Log(LogEnum.LE_SHOW_SPOT_RESULT, "GetSpottingResult(): mi=" + mi.Name + " Hidden - dr=" + dieRoll.ToString() + " mi.Spotting=" + mi.Spotting.ToString() + "-->HIDDEN");
               mi.Spotting = EnumSpottingResult.HIDDEN; // only applies if not already spotted or identified
               return "Hidden";
            }
         }
         //-------------------------------
         int modifier = GetSpottingModifier(gi, mi, cm, sector, range);
         if( TableMgr.FN_ERROR == modifier )
         {
            Logger.Log(LogEnum.LE_ERROR, "GetSpottingResult(): GetSpottingModifier() returned error");
            return "ERROR";
         }
         int total = dieRoll + modifier;
         Logger.Log(LogEnum.LE_SHOW_SPOT_RESULT, "GetSpottingResult(): mi=" + mi.Name + " dr=" + dieRoll.ToString() + " modifier=" + modifier.ToString() + " = " + total.ToString());
         //-------------------------------
         if(total <= cm.Rating )
         {
            int halfRating = (int)Math.Ceiling(cm.Rating * 0.5);
            if (total <= halfRating )
            {
               Logger.Log(LogEnum.LE_SHOW_SPOT_RESULT, "GetSpottingResult(): mi=" + mi.Name + " Identified - dr=" + dieRoll.ToString() );
               mi.Spotting = EnumSpottingResult.IDENTIFIED;
               return "Identified";
            }
            Logger.Log(LogEnum.LE_SHOW_SPOT_RESULT, "GetSpottingResult(): mi=" + mi.Name + " Spotted - dr=" + dieRoll.ToString());
            mi.Spotting = EnumSpottingResult.SPOTTED;
            return "Spotted";
         }
         if( EnumSpottingResult.SPOTTED == mi.Spotting )
         {
            Logger.Log(LogEnum.LE_SHOW_SPOT_RESULT, "GetSpottingResult(): mi=" + mi.Name + " Already Spotted - dr=" + dieRoll.ToString());
            return "Spotted";
         }
         return "Unspotted";
      }
      public static IMapItem? GetAppearingUnit(IGameInstance gi, IMapItem mi)
      {
         IMapItem? appearingMapItem = null;
         string name = "";
         if (true == mi.Name.Contains("ATG"))
         {
            if (true == String.IsNullOrEmpty(gi.IdentifiedAtg))
               return null;
            switch( gi.IdentifiedAtg )
            {
               case "Pak38":
                  name = "Pak38" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c93Pak38", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnit(): eu=" + name);
                  return appearingMapItem;
               case "Pak40":
                  name = "Pak40" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c94Pak40", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnit(): eu=" + name);
                  return appearingMapItem;
               case "Pak43":
                  name = "Pak43" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c95Pak43", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnit(): eu=" + name);
                  return appearingMapItem;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetAppearingUnit(): reached default with gi.IdentifiedAtg =" + gi.IdentifiedAtg);
                  return null;
            }
         }
         else if (true == mi.Name.Contains("TANK"))
         {
            if (true == String.IsNullOrEmpty(gi.IdentifiedTank))
               return null;
            switch (gi.IdentifiedTank)
            {
               case "PzIV":
                  name = "PzIV" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c79PzIV", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnit(): eu=" + name);
                  return appearingMapItem;
               case "PzV":
                  name = "PzV" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c80PzV", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnit(): eu=" + name);
                  return appearingMapItem;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetAppearingUnit(): reached default with gi.IdentifiedTank =" + gi.IdentifiedTank);
                  return null;
            }
         }
         else if (true == mi.Name.Contains("SPG"))
         {
            if (true == String.IsNullOrEmpty(gi.IdentifiedSpg))
               return null;
            switch (gi.IdentifiedSpg)
            {
               case "STuGIIIg":
                  name = "STuGIIIg" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c85STuGIIIg", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnit(): eu=" + name);
                  return appearingMapItem;
               case "MARDERII":
                  name = "MARDERII" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c83MarderII", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnit(): eu=" + name);
                  return appearingMapItem;
               case "MARDERIII":
                  name = "MARDERIII" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c84MarderIII", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnit(): eu=" + name);
                  return appearingMapItem;
               case "JdgPzIV":
                  name = "JdgPzIV" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c86JgdPzIV", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnit(): eu=" + name);
                  return appearingMapItem;
               case "JdgPz38t":
                  name = "JdgPz38t" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c87JgdPz38t", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnit(): eu=" + name);
                  return appearingMapItem;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetAppearingUnit(): reached default with gi.IdentifiedSpg =" + gi.IdentifiedSpg);
                  return null;
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "GetAppearingUnit(): reached default mi=" + mi.Name);
         return null;
      }
      public static IMapItem? GetAppearingUnitNew(IGameInstance gi, IMapItem mi, int dieRoll)
      {
         IMapItem? appearingMapItem = null;
         if( true == mi.Name.Contains("ATG"))
         {
            if( dieRoll < 4 )
            {
               string name = "Pak38" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c93Pak38", mi.TerritoryCurrent);
               Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
               gi.IdentifiedAtg = "Pak38";
               return appearingMapItem;
            }
            else if(dieRoll < 9)
            {
               string name = "Pak40" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c94Pak40", mi.TerritoryCurrent);
               Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
               gi.IdentifiedAtg = "Pak40";
               return appearingMapItem;
            }
            else 
            {
               string name = "Pak43" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c95Pak43", mi.TerritoryCurrent);
               Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
               gi.IdentifiedAtg = "Pak43";
               return appearingMapItem;
            }
         }
         else if (true == mi.Name.Contains("TANK"))
         {
            if (dieRoll < 6)
            {
               string name = "PzIV" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c79PzIV", mi.TerritoryCurrent);
               Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
               gi.IdentifiedTank = "PzIV";
               return appearingMapItem;
            }
            else if (dieRoll < 10)
            {
               string name = "PzV" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c80PzV", mi.TerritoryCurrent);
               Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
               gi.IdentifiedTank = "PzV";
               return appearingMapItem;
            }
            else
            {
               int diceRoll = 0;
               int die1 = Utilities.RandomGenerator.Next(0, 10);
               int die2 = Utilities.RandomGenerator.Next(0, 10);
               if (0 == die1 && 0 == die2)
                  diceRoll = 100;
               else
                  diceRoll = die1 + 10 * die2;
               if (diceRoll < 81)
               {
                  string name = "PzV" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c80PzV", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
                  gi.IdentifiedTank = "PzV";
                  return appearingMapItem;
               }
               else if (diceRoll < 95)
               {
                  string name = "PzVIe" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c81PzVIe", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
                  return appearingMapItem;
               }
               else
               {
                  string name = "PzVIb" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c82PzVIb", mi.TerritoryCurrent);
                  Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
                  return appearingMapItem;
               }
            }
         }
         else if (true == mi.Name.Contains("SPG"))
         {
            if (dieRoll < 4)
            {
               string name = "STuGIIIg" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c85STuGIIIg", mi.TerritoryCurrent);
               Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
               gi.IdentifiedSpg = "STuGIIIg";
               return appearingMapItem;
            }
            else if (dieRoll < 5)
            {
               string name = "MARDERII" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c83MarderII", mi.TerritoryCurrent);
               Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
               gi.IdentifiedSpg = "MARDERII";
               return appearingMapItem;
            }
            else if (dieRoll < 7)
            {
               string name = "MARDERIII" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c84MarderIII", mi.TerritoryCurrent);
               Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
               gi.IdentifiedSpg = "MARDERIII";
               return appearingMapItem;
            }
            else if (dieRoll < 9)
            {
               string name = "JdgPzIV" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c86JgdPzIV", mi.TerritoryCurrent);
               Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
               gi.IdentifiedSpg = "JdgPzIV";
               return appearingMapItem;
            }
            else
            {
               string name = "JdgPz38t" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c87JgdPz38t", mi.TerritoryCurrent);
               Logger.Log(LogEnum.LE_SHOW_APPEARING_UNITS, "GetAppearingUnitNew(): eu=" + name + " dr=" + dieRoll.ToString());
               gi.IdentifiedSpg = "JdgPz38t";
               return appearingMapItem;
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "GetAppearingUnitNew(): reached default mi=" + mi.Name);
         return null;
      }
      //-------------------------------------------
      public static int GetMovingModifier(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMovingModifier(): lastReport=null");
            return FN_ERROR;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //-------------------------------------------------
         ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
         if(null == commander)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMovingModifier(): commander=null");
            return FN_ERROR;
         }
         //-------------------------------------------------
         ICrewMember? driver = gi.GetCrewMemberByRole("Driver");
         if (null == driver)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMovingModifier(): driver=null");
            return FN_ERROR;
         }
         //-------------------------------------------------
         bool isCommanderDirectingMovement = false;
         foreach(IMapItem crewAction in gi.CrewActions)
         {
            if ("Commander_Move" == crewAction.Name)
               isCommanderDirectingMovement = true;
         }
         //-------------------------------------------------
         int movingModifier = 0;
         movingModifier -= driver.Rating;
         //-------------------------------------------------
         if(true == isCommanderDirectingMovement)
         {
            if (false == commander.IsButtonedUp)
               movingModifier -= commander.Rating;
            else if( true == card.myIsVisionCupola)
               movingModifier -= (int) Math.Floor(commander.Rating/2.0);
         }
         //-------------------------------------------------
         if( null != gi.ShermanHvss )
            movingModifier -= 2;
         //-------------------------------------------------
         if( true == driver.IsButtonedUp )
            movingModifier += 5;
         //-------------------------------------------------
         if( true == lastReport.Weather.Contains("Ground Snow"))
            movingModifier += 3;
         else if (true == lastReport.Weather.Contains("Falling Snow"))
            movingModifier += 6;
         else if (true == lastReport.Weather.Contains("Mud"))
            movingModifier += 9;
         return movingModifier;
      }
      public static string GetMovingResultSherman(IGameInstance gi, int dieRoll)
      {
         gi.Sherman.IsMoving = false;
         gi.Sherman.IsHullDown = false;
         if (100 == dieRoll) // unmodified dieroll=100 is bogged down
         {
            gi.Sherman.IsBoggedDown = true;
            return "Bogged Down";
         }
         int modifier = GetMovingModifier(gi);
         if( modifier < -100)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMovingResultSherman(): GetMovingModifier() returned error");
            return "ERROR";
         }
         dieRoll += modifier;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if (true== crewAction.Name.Contains("Driver"))
            {
               switch(crewAction.Name)
               {
                  case "Driver_Forward":
                  case "Driver_Reverse":
                     if (dieRoll < 98)
                     {
                        gi.Sherman.IsMoving = true;
                        return "No Accident";
                     }
                     else if (dieRoll < 100)
                     {
                        gi.Sherman.IsThrownTrack = true;
                        return "Thrown Track";
                     }
                     else
                     {
                        gi.Sherman.IsBoggedDown = true;
                        return "Bogged Down";
                     }
                  case "Driver_ForwardToHullDown":
                     if (dieRoll < 21)
                     {
                        gi.Sherman.IsHullDown = true;
                        return "Hull Down";
                     }
                     else if (dieRoll < 98)
                     {
                        gi.Sherman.IsMoving = true;
                        return "No Accident";
                     }
                     else if (dieRoll < 100)
                     {
                        gi.Sherman.IsThrownTrack = true;
                        return "Thrown Track";
                     }
                     else
                     {
                        gi.Sherman.IsBoggedDown = true;
                        return "Bogged Down";
                     }
                  case "Driver_ReverseToHullDown":
                     if (dieRoll < 11)
                     {
                        gi.Sherman.IsHullDown = true;
                        return "Hull Down";
                     }
                     else if (dieRoll < 98)
                     {
                        gi.Sherman.IsMoving = true;
                        return "No Accident";
                     }
                     else if (dieRoll < 100)
                     {
                        gi.Sherman.IsThrownTrack = true;
                        return "Thrown Track";
                     }
                     else
                     {
                        gi.Sherman.IsBoggedDown = true;
                        return "Bogged Down";
                     }
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "GetMovingResultSherman(): reached default crewaction=" + crewAction.Name);
                     return "ERROR";
               }
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "GetMovingResultSherman(): reached default");
         return "ERROR";
      }
      public static string GetMovingResultEnemy(IGameInstance gi)
      {
         int dieRoll = DieRoller.WhiteDie;
         if( dieRoll < 1 || 10 < dieRoll )
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMovingResultEnemy(): invalid dieRoll=" + dieRoll.ToString());
            return "ERROR";
         }
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if (true == crewAction.Name.Contains("Driver"))
            {
               switch (crewAction.Name)
               {
                  case "Driver_Forward":
                     if (dieRoll < 5)
                        return "A";
                     if (dieRoll < 6)
                        return "C";
                     return "None";
                  case "Driver_Reverse":
                     if (dieRoll < 3)
                        return "B";
                     if (dieRoll < 5)
                        return "C";
                     return "None";
                  case "Driver_ForwardToHullDown":
                     if (dieRoll < 3)
                        return "A";
                     if (dieRoll < 4)
                        return "C";
                     return "None";
                  case "Driver_ReverseToHullDown":
                     if (dieRoll < 2)
                        return "B";
                     if (dieRoll < 3)
                        return "C";
                     return "None";
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "GetMovingResultEnemy(): reached default crewaction=" + crewAction.Name);
                     return "ERROR";
               }
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "GetMovingResultEnemy(): reached default");
         return "ERROR";
      }
      public static int GetBoggedDownModifier(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMovingModifier(): lastReport=null");
            return FN_ERROR;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //-------------------------------------------------
         ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
         if (null == commander)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMovingModifier(): commander=null");
            return FN_ERROR;
         }
         //-------------------------------------------------
         ICrewMember? driver = gi.GetCrewMemberByRole("Driver");
         if (null == driver)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetMovingModifier(): driver=null");
            return FN_ERROR;
         }
         //-------------------------------------------------
         bool isCommanderDirectingMovement = false;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if ("Commander_Move" == crewAction.Name)
               isCommanderDirectingMovement = true;
         }
         //-------------------------------------------------
         int movingModifier = 0;
         movingModifier -= driver.Rating;
         //-------------------------------------------------
         if (true == isCommanderDirectingMovement)
         {
            if (false == commander.IsButtonedUp)
               movingModifier -= commander.Rating;
         }
         //-------------------------------------------------
         if (null != gi.ShermanHvss)
            movingModifier -= 5;
         //-------------------------------------------------
         if (true == driver.IsButtonedUp)
            movingModifier += 10;
         //-------------------------------------------------
         return movingModifier;
      }
      //-------------------------------------------
      public static int GetFriendlyActionModifier(IGameInstance gi, IMapItem mi, int numUsControlledSector, bool isAdvancingFire, bool isArtilleryFire, bool isAirStrike)
      {
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetFriendlyActionModifier(): unknown enemyUnit=" + mi.Name);
            return -100;
         }
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetFriendlyActionModifier(): lastReport=null");
            return -100;
         }
         //----------------------------------------------------
         IStack? stack = gi.BattleStacks.Find(mi.TerritoryCurrent);
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetFriendlyActionModifier(): stack=null for t=" + mi.TerritoryCurrent.Name);
            return -100;
         }
         //----------------------------------------------------
         int numSmokeInTargetZone = 0;
         foreach (IMapItem smoke in stack.MapItems)
         {
            if (true == smoke.Name.Contains("Smoke"))
               numSmokeInTargetZone++;
         }
         int modifier = numSmokeInTargetZone * 10;
         //----------------------------------------------------
         bool isTargetVehicle = false;
         bool isTargetInWoods = false;
         int friendlySquadsLost = 0;
         int friendlyTanksLost = 0;
         switch (enemyUnit)
         {
            case "LW":
            case "MG":
               isTargetInWoods = mi.IsWoods;
               friendlySquadsLost = lastReport.VictoryPtsFriendlySquad;
               break;
            case "ATG":
            case "Pak43":
            case "Pak38":
            case "Pak40":
               isTargetInWoods = mi.IsWoods;
               break;
            case "TRUCK":
               friendlyTanksLost = lastReport.VictoryPtsFriendlyTank;
               break;
            case "SPG":
            case "STuGIIIg":
            case "TANK":
            case "PzVIe":
            case "PzIV":
            case "PzV":
            case "PzVIb":
            case "PSW":
            case "SPW":
            case "MARDERII":
            case "MARDERIII":
            case "JdgPzIV":
            case "JdgPz38t":
               isTargetVehicle = true;
               friendlyTanksLost = lastReport.VictoryPtsFriendlyTank;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetFriendlyActionModifier(): reached default with enemyUnit=" + enemyUnit);
               return -1;
         }
         //----------------------------------------------------
         if (true == gi.IsFlankingFire)
            modifier -= 10;
         //----------------------------------------------------
         if (true == isTargetVehicle && true == isAirStrike)
            modifier -= 10;
         //----------------------------------------------------
         modifier -= 3 * numUsControlledSector;
         //----------------------------------------------------
         if (true == isTargetInWoods)
            modifier -= 3;
         //----------------------------------------------------
         modifier += friendlySquadsLost;
         //----------------------------------------------------
         modifier += 2 * friendlyTanksLost;
         //----------------------------------------------------
         if (true == lastReport.Weather.Contains("Fog") || true == lastReport.Weather.Contains("Falling"))
            modifier += 10;
         //----------------------------------------------------
         if (true == isTargetVehicle && (true == isAdvancingFire || true == isArtilleryFire))
            modifier += 10;
         //----------------------------------------------------
         return modifier;
      }
      public static string SetFriendlyActionResult(IGameInstance gi, IMapItem mi, int dieRoll, int numUsControlledSector, bool isAdvancingFire, bool isArtilleryFire, bool isAirStrike)
      {
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetFriendlyActionResult(): lastReport=null");
            return "ERROR";
         }
         if (false == isArtilleryFire && (true == lastReport.Weather.Contains("Fog") || true == lastReport.Weather.Contains("Falling")))
         {
            if ("B6M" == mi.TerritoryCurrent.Name || "B6L" == mi.TerritoryCurrent.Name) // no Friendly action allowed in these regions unless artillery
               return "ERROR";
         }
         //----------------------------------------------------
         if (dieRoll < 4) // always a kill if below 4 regardless of modifiers
         {
            mi.IsKilled = true;
            mi.SetBloodSpots();
            return "KO";
         }
         //----------------------------------------------------
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetFriendlyActionResult(): unknown enemyUnit=" + mi.Name);
            return "ERROR";
         }
         //----------------------------------------------------
         IStack? stack = gi.BattleStacks.Find(mi.TerritoryCurrent);
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetFriendlyActionResult(): stack=null for t=" + mi.TerritoryCurrent.Name);
            return "ERROR";
         }
         bool isSmokeAddedToTerritory = false;
         switch (enemyUnit) // modifiers do not apply for smoke
         {
            case "LW":
            case "MG":
            case "TRUCK":
            case "PSW":
            case "SPW":
            case "MARDERII":
            case "MARDERIII":
               // no smoke possible
               break;
            case "ATG":
            case "Pak43":
            case "Pak38":
            case "Pak40":
            case "PzV":
            case "JdgPzIV":
            case "JdgPz38t":
               if (79 < dieRoll)
                  isSmokeAddedToTerritory = true;
               break;
            case "PzIV":
            case "STuGIIIg":
            case "SPG":
               if (89 < dieRoll)
                  isSmokeAddedToTerritory = true;
               break;
            case "TANK":
            case "PzVIe":
            case "PzVIb":
               if (59 < dieRoll)
                  isSmokeAddedToTerritory = true;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "SetFriendlyActionResult(): reached default with enemyUnit=" + enemyUnit);
               return "ERROR";
         }
         if (true == isSmokeAddedToTerritory)
         {
            string miName = "SmokeWhite" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem smoke = new MapItem(miName, Utilities.ZOOM + 0.75, "c108Smoke1", mi.TerritoryCurrent);
            IMapPoint mp = Territory.GetRandomPoint(mi.TerritoryCurrent, mi.Zoom * Utilities.theMapItemOffset);
            smoke.Location = mp;
            stack.MapItems.Add(smoke);
            return "Smoke"; // if smoke occurs, no chance of killing target
         }
         //----------------------------------------------------
         int modifier = GetFriendlyActionModifier(gi, mi, numUsControlledSector, isAdvancingFire, isArtilleryFire, isAirStrike);
         if (modifier < -99)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetFriendlyActionResult(): GetFriendlyActionModifier() returned error");
            return "ERROR";
         }
         dieRoll += modifier;
         //----------------------------------------------------
         switch (enemyUnit) // modifiers do not apply for smoke
         {
            case "LW":
            case "MG":
            case "PzIV":
            case "STuGIIIg":
            case "SPG":
               if (dieRoll < 31)
                  mi.IsKilled = true;
               break;
            case "ATG":
            case "Pak43":
            case "Pak38":
            case "Pak40":
            case "PzV":
            case "JdgPzIV":
            case "JdgPz38t":
               if (dieRoll < 21)
                  mi.IsKilled = true;
               break;
            case "TRUCK":
               if (dieRoll < 61)
                  mi.IsKilled = true;
               break;
            case "PSW":
            case "SPW":
               if (dieRoll < 41)
                  mi.IsKilled = true;
               break;
            case "MARDERII":
            case "MARDERIII":
               if (dieRoll < 51)
                  mi.IsKilled = true;
               break;
            case "PzVIe":
            case "PzVIb":
            case "TANK":
               if (dieRoll < 11)
                  mi.IsKilled = true;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "SetFriendlyActionResult(): reached default with enemyUnit=" + enemyUnit);
               return "ERROR";
         }
         if (true == mi.IsKilled)
         {
            mi.SetBloodSpots();
            return "KO";
         }
         return "None";
      }
      //-------------------------------------------
      public static bool GetNewSherman(IGameInstance gi, int dieRoll)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetNewSherman(): lastReport=null");
            return false;
         }
         lastReport.Name = Utilities.GetNickName(); // Get a new nickname
         lastReport.MainGunHBCI = 0;
         lastReport.MainGunHVAP = 0;   
         //--------------------------------
         string month = GetMonth(gi.Day);
         if ("ERROR" == month)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetNextTank(): GetMonth() returned ERROR");
            return false;
         }
         //--------------------------------
         switch (month)
         {
            case "Jun":
            case "Jul":
               lastReport.TankCardNum = 1;
               break;
            case "Aug":
               if (dieRoll < 7)
                  lastReport.TankCardNum = 1;
               else if (dieRoll < 21)
                  lastReport.TankCardNum = 2;
               else if (dieRoll < 28)
                  lastReport.TankCardNum = 4;
               else if (dieRoll < 46)
                  lastReport.TankCardNum = 5;
               else if (dieRoll < 51)
                  lastReport.TankCardNum = 7;
               else if (dieRoll < 71)
                  lastReport.TankCardNum = 8;
               else if (dieRoll < 81)
                  lastReport.TankCardNum = 14;
               else
                  lastReport.TankCardNum = 16;
               break;
            case "Sep":
               if (dieRoll < 6)
                  lastReport.TankCardNum = 1;
               else if (dieRoll < 16)
                  lastReport.TankCardNum = 2;
               else if (dieRoll < 22)
                  lastReport.TankCardNum = 4;
               else if (dieRoll < 36)
                  lastReport.TankCardNum = 5;
               else if (dieRoll < 38)
                  lastReport.TankCardNum = 7;
               else if (dieRoll < 41)
                  lastReport.TankCardNum = 8;
               else if (dieRoll < 59)
                  lastReport.TankCardNum = 10;
               else if (dieRoll < 61)
                  lastReport.TankCardNum = 11;
               else if (dieRoll < 66)
                  lastReport.TankCardNum = 12;
               else if (dieRoll < 76)
                  lastReport.TankCardNum = 14;
               else
                  lastReport.TankCardNum = 16;
               break;
            case "Oct":
               if (dieRoll < 4)
                  lastReport.TankCardNum = 1;
               else if (dieRoll < 11)
                  lastReport.TankCardNum = 2;
               else if (dieRoll < 16)
                  lastReport.TankCardNum = 4;
               else if (dieRoll < 26)
                  lastReport.TankCardNum = 5;
               else if (dieRoll < 28)
                  lastReport.TankCardNum = 7;
               else if (dieRoll < 31)
                  lastReport.TankCardNum = 8;
               else if (dieRoll < 50)
                  lastReport.TankCardNum = 10;
               else if (dieRoll < 55)
                  lastReport.TankCardNum = 11;
               else if (dieRoll < 60)
                  lastReport.TankCardNum = 12;
               else if (dieRoll < 61)
                  lastReport.TankCardNum = 13;
               else if (dieRoll < 76)
                  lastReport.TankCardNum = 14;
               else
                  lastReport.TankCardNum = 16;
               break;
            case "Nov":
               if (dieRoll < 2)
                  lastReport.TankCardNum = 1;
               else if (dieRoll < 5)
                  lastReport.TankCardNum = 2;
               else if (dieRoll < 6)
                  lastReport.TankCardNum = 3;
               else if (dieRoll < 9)
                  lastReport.TankCardNum = 4;
               else if (dieRoll < 16)
                  lastReport.TankCardNum = 5;
               else if (dieRoll < 17)
                  lastReport.TankCardNum = 6;
               else if (dieRoll < 18)
                  lastReport.TankCardNum = 7;
               else if (dieRoll < 21)
                  lastReport.TankCardNum = 8;
               else if (dieRoll < 22)
                  lastReport.TankCardNum = 9;
               else if (dieRoll < 43)
                  lastReport.TankCardNum = 10;
               else if (dieRoll < 52)
                  lastReport.TankCardNum = 11;
               else if (dieRoll < 57)
                  lastReport.TankCardNum = 12;
               else if (dieRoll < 58)
                  lastReport.TankCardNum = 13;
               else if (dieRoll < 72)
                  lastReport.TankCardNum = 14;
               else if (dieRoll < 73)
                  lastReport.TankCardNum = 15;
               else if (dieRoll < 98)
                  lastReport.TankCardNum = 16;
               else
                  lastReport.TankCardNum = 17;
               break;
            case "Dec":
               if (dieRoll < 2)
                  lastReport.TankCardNum = 1;
               else if (dieRoll < 5)
                  lastReport.TankCardNum = 2;
               else if (dieRoll < 6)
                  lastReport.TankCardNum = 3;
               else if (dieRoll < 7)
                  lastReport.TankCardNum = 4;
               else if (dieRoll < 10)
                  lastReport.TankCardNum = 5;
               else if (dieRoll < 11)
                  lastReport.TankCardNum = 6;
               else if (dieRoll < 12)
                  lastReport.TankCardNum = 7;
               else if (dieRoll < 15)
                  lastReport.TankCardNum = 8;
               else if (dieRoll < 16)
                  lastReport.TankCardNum = 9;
               else if (dieRoll < 35)
                  lastReport.TankCardNum = 10;
               else if (dieRoll < 48)
                  lastReport.TankCardNum = 11;
               else if (dieRoll < 53)
                  lastReport.TankCardNum = 12;
               else if (dieRoll < 54)
                  lastReport.TankCardNum = 13;
               else if (dieRoll < 66)
                  lastReport.TankCardNum = 14;
               else if (dieRoll < 69)
                  lastReport.TankCardNum = 15;
               else if (dieRoll < 95)
                  lastReport.TankCardNum = 16;
               else
                  lastReport.TankCardNum = 17;
               break;
            case "Jan":
               if (dieRoll < 2)
                  lastReport.TankCardNum = 1;
               else if (dieRoll < 4)
                  lastReport.TankCardNum = 2;
               else if (dieRoll < 5)
                  lastReport.TankCardNum = 3;
               else if (dieRoll < 6)
                  lastReport.TankCardNum = 4;
               else if (dieRoll < 9)
                  lastReport.TankCardNum = 5;
               else if (dieRoll < 10)
                  lastReport.TankCardNum = 6;
               else if (dieRoll < 11)
                  lastReport.TankCardNum = 7;
               else if (dieRoll < 14)
                  lastReport.TankCardNum = 8;
               else if (dieRoll < 15)
                  lastReport.TankCardNum = 9;
               else if (dieRoll < 31)
                  lastReport.TankCardNum = 10;
               else if (dieRoll < 47)
                  lastReport.TankCardNum = 11;
               else if (dieRoll < 52)
                  lastReport.TankCardNum = 12;
               else if (dieRoll < 53)
                  lastReport.TankCardNum = 13;
               else if (dieRoll < 63)
                  lastReport.TankCardNum = 14;
               else if (dieRoll < 68)
                  lastReport.TankCardNum = 15;
               else if (dieRoll < 91)
                  lastReport.TankCardNum = 16;
               else
                  lastReport.TankCardNum = 17;
               break;
            case "Feb":
               if (dieRoll < 2)
                  lastReport.TankCardNum = 1;
               else if (dieRoll < 3)
                  lastReport.TankCardNum = 2;
               else if (dieRoll < 4)
                  lastReport.TankCardNum = 3;
               else if (dieRoll < 5)
                  lastReport.TankCardNum = 4;
               else if (dieRoll < 7)
                  lastReport.TankCardNum = 5;
               else if (dieRoll < 8)
                  lastReport.TankCardNum = 6;
               else if (dieRoll < 9)
                  lastReport.TankCardNum = 7;
               else if (dieRoll < 12)
                  lastReport.TankCardNum = 8;
               else if (dieRoll < 13)
                  lastReport.TankCardNum = 9;
               else if (dieRoll < 25)
                  lastReport.TankCardNum = 10;
               else if (dieRoll < 45)
                  lastReport.TankCardNum = 11;
               else if (dieRoll < 50)
                  lastReport.TankCardNum = 12;
               else if (dieRoll < 51)
                  lastReport.TankCardNum = 13;
               else if (dieRoll < 60)
                  lastReport.TankCardNum = 14;
               else if (dieRoll < 66)
                  lastReport.TankCardNum = 15;
               else if (dieRoll < 87)
                  lastReport.TankCardNum = 16;
               else
                  lastReport.TankCardNum = 17;
               break;
            case "Mar":
               if (dieRoll < 2)
                  lastReport.TankCardNum = 1;
               else if (dieRoll < 3)
                  lastReport.TankCardNum = 2;
               else if (dieRoll < 4)
                  lastReport.TankCardNum = 3;
               else if (dieRoll < 5)
                  lastReport.TankCardNum = 4;
               else if (dieRoll < 6)
                  lastReport.TankCardNum = 5;
               else if (dieRoll < 7)
                  lastReport.TankCardNum = 6;
               else if (dieRoll < 8)
                  lastReport.TankCardNum = 7;
               else if (dieRoll < 10)
                  lastReport.TankCardNum = 8;
               else if (dieRoll < 11)
                  lastReport.TankCardNum = 9;
               else if (dieRoll < 21)
                  lastReport.TankCardNum = 10;
               else if (dieRoll < 45)
                  lastReport.TankCardNum = 11;
               else if (dieRoll < 50)
                  lastReport.TankCardNum = 12;
               else if (dieRoll < 51)
                  lastReport.TankCardNum = 13;
               else if (dieRoll < 58)
                  lastReport.TankCardNum = 14;
               else if (dieRoll < 65)
                  lastReport.TankCardNum = 15;
               else if (dieRoll < 83)
                  lastReport.TankCardNum = 16;
               else
                  lastReport.TankCardNum = 17;
               break;
            case "Apr":
               if (dieRoll < 2)
                  lastReport.TankCardNum = 1;
               else if (dieRoll < 3)
                  lastReport.TankCardNum = 2;
               else if (dieRoll < 4)
                  lastReport.TankCardNum = 3;
               else if (dieRoll < 5)
                  lastReport.TankCardNum = 4;
               else if (dieRoll < 6)
                  lastReport.TankCardNum = 5;
               else if (dieRoll < 7)
                  lastReport.TankCardNum = 6;
               else if (dieRoll < 8)
                  lastReport.TankCardNum = 7;
               else if (dieRoll < 9)
                  lastReport.TankCardNum = 8;
               else if (dieRoll < 10)
                  lastReport.TankCardNum = 9;
               else if (dieRoll < 16)
                  lastReport.TankCardNum = 10;
               else if (dieRoll < 44)
                  lastReport.TankCardNum = 11;
               else if (dieRoll < 49)
                  lastReport.TankCardNum = 12;
               else if (dieRoll < 50)
                  lastReport.TankCardNum = 13;
               else if (dieRoll < 55)
                  lastReport.TankCardNum = 14;
               else if (dieRoll < 64)
                  lastReport.TankCardNum = 15;
               else if (dieRoll < 79)
                  lastReport.TankCardNum = 16;
               else
                  lastReport.TankCardNum = 17;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetNextTank(): reached default month=" + month);
               return false;
         }
         return true;
      }
      public static string GetShermanFireDirection(IGameInstance gi, IMapItem enemyUnit, string hitLocation)
      {
         if ("Thrown Track" == hitLocation)
            return "Thrown Track";
         int count = enemyUnit.TerritoryCurrent.Name.Count();
         if (3 != count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanFireDirection(): 3 != enemyUnit.TerritoryCurrent.Name=" + enemyUnit.TerritoryCurrent.Name);
            return "ERROR";
         }
         char enemySector = enemyUnit.TerritoryCurrent.Name[count - 2];
         double rotation = 0.0;
         switch (enemySector)
         {
            case '6': rotation = 0.0; break;
            case '9': rotation = 60.0; break;
            case '1': rotation = 120.0; break;
            case '2': rotation = 180.0; break;
            case '3': rotation = 240.0; break;
            case '4': rotation = 300.0; break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "Get_ShermanFireDirection(): reached default enemySector=" + enemySector);
               return "ERROR";
         }
         double totalRotation = rotation - enemyUnit.RotationHull;
         double or = totalRotation;
         if ("Hull" == hitLocation)
         {
            if (totalRotation < 0.0)
               totalRotation += 360.0;
            if (359.9 < totalRotation)
               totalRotation = totalRotation - 360.0;
            Logger.Log(LogEnum.LE_SHOW_FIRE_DIRECTION, "Get_ShermanFireDirection(): turret: (total=" + totalRotation.ToString("F1") + ") = (r=" + rotation.ToString("F1") + ") - (hr=" + enemyUnit.RotationHull.ToString("F1") + ")  or=" + or.ToString("F1"));
            switch (totalRotation)
            {
               case 0.0: return "Rear";
               case 60.0: return "Side";
               case 120.0: return "Side";
               case 180.0: return "Front";
               case 240.0: return "Side";
               case 300.0: return "Side";
               default:
                  Logger.Log(LogEnum.LE_ERROR, "Get_ShermanFireDirection(): 2-reached default total=" + totalRotation.ToString("F1") + " r=" + rotation.ToString("F1") + " hr=" + enemyUnit.RotationHull.ToString("F1") + " tr=" + enemyUnit.RotationTurret.ToString("F1"));
                  return "ERROR";
            }
         }
         else if ("Turret" == hitLocation)
         {
            totalRotation -= enemyUnit.RotationTurret;
            if (totalRotation < 0.0)
               totalRotation += 360.0;
            if (359.9 < totalRotation)
               totalRotation = totalRotation - 360.0;
            Logger.Log(LogEnum.LE_SHOW_FIRE_DIRECTION, "Get_ShermanFireDirection(): turret: (total=" + totalRotation.ToString("F1") + ") = (r=" + rotation.ToString("F1") + ") - (hr=" + enemyUnit.RotationHull.ToString("F1") + ") - (tr=" + enemyUnit.RotationTurret.ToString("F1") + ")  or=" + or.ToString("F1"));
            switch (totalRotation)
            {
               case 0.0: return "Rear";
               case 60.0: return "Side";
               case 120.0: return "Side";
               case 180.0: return "Front";
               case 240.0: return "Side";
               case 300.0: return "Side";
               default:
                  Logger.Log(LogEnum.LE_ERROR, "Get_ShermanFireDirection(): reached default total=" + totalRotation.ToString("F1") + " r=" + rotation.ToString("F1") + " hr=" + enemyUnit.RotationHull.ToString("F1") + " tr=" + enemyUnit.RotationTurret.ToString("F1"));
                  return "ERROR";
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "Get_ShermanFireDirection(): reached default hitLocation=" + hitLocation);
         return "ERROR";
      }
      public static int GetShermanToHitModifier(IGameInstance gi, IMapItem enemyUnit)
      {
         //------------------------------------
         if ( 3 != enemyUnit.TerritoryCurrent.Name.Length)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): 3 != TerritoryCurrent.Name.Length=" + enemyUnit.TerritoryCurrent.Name);
            return FN_ERROR;
         }
         char range = enemyUnit.TerritoryCurrent.Name[2];
         //------------------------------------
         int toHitModifierNum = 0;
         bool isCommanderDirectingFire = false;
         bool isShermanMoving = false;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if ("Commander_MainGunFire" == crewAction.Name)
               isCommanderDirectingFire = true;
            if ("Driver_Forward" == crewAction.Name)
               isShermanMoving = true;
            if ("Driver_ForwardToHullDown" == crewAction.Name)
               isShermanMoving = true;
            if ("Driver_Reverse" == crewAction.Name)
               isShermanMoving = true;
            if ("Driver_ReverseToHullDown" == crewAction.Name)
               isShermanMoving = true;
            if ("Driver_PivotTank" == crewAction.Name)
               isShermanMoving = true;
         }
         //------------------------------------
         ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
         if (null == commander)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): commander=null");
            return FN_ERROR;
         }
         ICrewMember? gunner = gi.GetCrewMemberByRole("Gunner");
         if (null == gunner)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): gunner=null");
            return FN_ERROR;
         }
         //------------------------------------
         if( null == gi.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): gi.TargetMainGun=null");
            return FN_ERROR;
         }
         int numShots = 0;
         if (true == gi.TargetMainGun.EnemyAcquiredShots.ContainsKey("Sherman"))
            numShots = gi.TargetMainGun.EnemyAcquiredShots["Sherman"];
         if (0 == numShots)
         {
            Logger.Log(LogEnum.LE_SHOW_NUM_SHERMAN_SHOTS, "GetShermanToHitModifier(): numShots=" + numShots.ToString());
            if ((false == isCommanderDirectingFire) || (true == commander.IsButtonedUp))
            {
               Logger.Log(LogEnum.LE_SHOW_NUM_SHERMAN_SHOTS, "GetShermanToHitModifier(): numShots=" + numShots.ToString() + "isCommanderDirectingFire=" + isCommanderDirectingFire.ToString() + "commander.IsButtonedUp=" + commander.IsButtonedUp.ToString());
               toHitModifierNum += 10;
               Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): first shot at close range +10 mod=" + toHitModifierNum.ToString());
            }
         }
         else if (1 == numShots)
         {
            Logger.Log(LogEnum.LE_SHOW_NUM_SHERMAN_SHOTS, "GetShermanToHitModifier(): +1 acq numShots=" + numShots.ToString());
            if ( 'C' == range )
            {
               toHitModifierNum -= 5;
               Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): acq1 at Close range -5 mod=" + toHitModifierNum.ToString());
            }
            else if ('M' == range)
            {
               toHitModifierNum -= 10;
               Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): acq1 at Medium range -10 mod=" + toHitModifierNum.ToString());
            }
            else if ('L' == range)
            {
               toHitModifierNum -= 15;
               Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): acq1 at Long range -15 mod=" + toHitModifierNum.ToString());
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): reached default range=" + range);
               return FN_ERROR;
            }
         }
         else if (1 < numShots)
         {
            Logger.Log(LogEnum.LE_SHOW_NUM_SHERMAN_SHOTS, "GetShermanToHitModifier(): +2 acq numShots=" + numShots.ToString());
            if ('C' == range)
            {
               toHitModifierNum -= 10;
               Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): acq2 at Close range -10 mod=" + toHitModifierNum.ToString());
            }
            else if ('M' == range)
            {
               toHitModifierNum -= 20;
               Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): acq2 at Medium range -20 mod=" + toHitModifierNum.ToString());
            }
            else if ('L' == range)
            {
               toHitModifierNum -= 30;
               Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier():  acq2 at Long range -30 mod=" + toHitModifierNum.ToString());
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): reached default range=" + range);
               return FN_ERROR;
            }
         }
         //------------------------------------
         if ((true == enemyUnit.IsVehicle) && (true == enemyUnit.IsMoving))
         {
            if ('C' == range)
            {
               toHitModifierNum += 20;
               Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): Target.IsMoving close range +20 mod=" + toHitModifierNum.ToString());
            }
            else if ('M' == range)
            {
               toHitModifierNum += 25;
               Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): Target.IsMoving medium rating +25 mod=" + toHitModifierNum.ToString());
            }
            else if ('L' == range)
            {
               toHitModifierNum += 25;
               Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): Target.IsMoving Long Range +25 mod=" + toHitModifierNum.ToString());
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): reached default range=" + range);
               return FN_ERROR;
            }
         }
         //------------------------------------
         if( true == isCommanderDirectingFire )
         {
            toHitModifierNum -= commander.Rating;
            Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): cmdr rating -" + commander.Rating.ToString() + "  mod=" + toHitModifierNum.ToString());
         }
         toHitModifierNum -= gunner.Rating;
         Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): gunner rating -" + gunner.Rating.ToString() + "  mod=" + toHitModifierNum.ToString());
         //------------------------------------
         double t1 = 360 - gi.Sherman.RotationTurret;
         double t2 = gi.ShermanRotationTurretOld;
         double totalAngle = t1 + t2;
         if (360.0 < totalAngle)
            totalAngle = totalAngle - 360;
         if (180.0 < totalAngle)
            totalAngle = 360 - totalAngle;
         int numRotations = (int)(totalAngle / 60.0);
         int turretMod = (int)(10.0 * numRotations);
         toHitModifierNum += turretMod;
         Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): tNew=" + t1.ToString() + " tOld=" + t2.ToString() + " #r=" + numRotations.ToString() + " turretMod= +" + turretMod.ToString() + " mod=" + toHitModifierNum.ToString());
         //------------------------------------
         if (true == enemyUnit.Name.Contains("Pak43"))
         {
            toHitModifierNum += 10;
            Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): Pak43 +10 mod=" + toHitModifierNum.ToString());
         }
         else if ((true == enemyUnit.Name.Contains("Pak40")) || (true == enemyUnit.Name.Contains("Pak38")))
         {
            toHitModifierNum += 20;
            Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): Pak38/40 +20 mod=" + toHitModifierNum.ToString());
         }
         //==================================
         Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): ------------>>>>>>>>>>>gi.ShermanTypeOfFire=" + gi.ShermanTypeOfFire);
         if ("Direct" == gi.ShermanTypeOfFire) // TableMgr.GetShermanToHitModifier()
         {
            if (true == enemyUnit.IsVehicle)
            {
               if (true == gi.IsShermanDeliberateImmobilization)
               {
                  if ('C' == range)
                  {
                     toHitModifierNum += 65;
                     Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): Is_ShermanDeliberateImmobilization close range +65 mod=" + toHitModifierNum.ToString());
                  }
                  else if ('M' == range)
                  {
                     toHitModifierNum += 55;
                     Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): Is_ShermanDeliberateImmobilization medium rating +55 mod=" + toHitModifierNum.ToString());
                  }
                  else if ('L' == range)
                  {
                     toHitModifierNum += 45;
                     Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): Is_ShermanDeliberateImmobilization Long Range +45 mod=" + toHitModifierNum.ToString());
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): reached default range=" + range);
                     return FN_ERROR;
                  }
               }
               //----------------------------
               string enemyUnitType = enemyUnit.GetEnemyUnit();
               if ("ERROR" == enemyUnitType)
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): unknown enemyUnit=" + enemyUnit.Name);
                  return FN_ERROR;
               }
               switch (enemyUnitType)
               {
                  case "SPG":
                  case "STuGIIIg": // small size
                  case "JdgPzIV":
                  case "JdgPz38t":
                  case "SPW":
                     toHitModifierNum += 10;
                     Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): small size +10 mod=" + toHitModifierNum.ToString());
                     break;
                  case "PzIV":  // average size
                  case "MARDERII":
                  case "MARDERIII":
                  case "TRUCK":
                  case "PSW":
                     break;
                  case "TANK":
                  case "PzV":  // large size
                  case "PzVIe":
                     if ('C' == range)
                     {
                        toHitModifierNum -= 5;
                        Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): large size close range -5 mod=" + toHitModifierNum.ToString());
                     }
                     else if ('M' == range)
                     {
                        toHitModifierNum -= 10;
                        Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): large size medium rating -10 mod=" + toHitModifierNum.ToString());
                     }
                     else if ('L' == range)
                     {
                        toHitModifierNum -= 15;
                        Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): large size Long Range -15 mod=" + toHitModifierNum.ToString());
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): reached default range=" + range);
                        return FN_ERROR;
                     }
                     break;
                  case "PzVIb": // very large size
                     if ('C' == range)
                     {
                        toHitModifierNum -= 10;
                        Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): large size close range -10 mod=" + toHitModifierNum.ToString());
                     }
                     else if ('M' == range)
                     {
                        toHitModifierNum -= 20;
                        Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): large size medium rating -20 mod=" + toHitModifierNum.ToString());
                     }
                     else if ('L' == range)
                     {
                        toHitModifierNum -= 30;
                        Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): large size Long Range -30 mod=" + toHitModifierNum.ToString());
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): reached default range=" + range);
                        return FN_ERROR;
                     }
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): Reached Default enemyUnitType=" + enemyUnitType);
                     return FN_ERROR;
               }
               
            }
            //----------------------------
            if (true == isShermanMoving)
            {
               toHitModifierNum += 25;
               Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): Sherman moving +25 mod=" + toHitModifierNum.ToString());
            }
            //----------------------------
            if (true == enemyUnit.IsWoods)
            {
               if ('C' == range)
               {
                  toHitModifierNum += 5;
                  Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): woods +5 close range mod=" + toHitModifierNum.ToString());
               }
               else if ('M' == range)
               {
                  toHitModifierNum += 10;
                  Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): woods +10 medium range mod=" + toHitModifierNum.ToString());
               }
               else if ('L' == range)
               {
                  toHitModifierNum += 15;
                  Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): woods +15 long range mod=" + toHitModifierNum.ToString());
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): reached default range=" + range);
                  return FN_ERROR;
               }
            }
            if ((true == enemyUnit.IsBuilding) && (false == enemyUnit.IsVehicle))
            {
               if ('C' == range)
               {
                  toHitModifierNum += 10;
                  Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): building +10 close range mod=" + toHitModifierNum.ToString());
               }
               else if ('M' == range)
               {
                  toHitModifierNum += 15;
                  Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): building +15 medium range mod=" + toHitModifierNum.ToString());
               }
               else if ('L' == range)
               {
                  toHitModifierNum += 25;
                  Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): building +25 long range mod=" + toHitModifierNum.ToString());
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): reached default range=" + range);
                  return FN_ERROR;
               }
            }
            if ((true == enemyUnit.IsFortification) && (false == enemyUnit.IsVehicle))
            {
               if ('C' == range)
               {
                  toHitModifierNum += 15;
                  Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): fort +15 close range mod=" + toHitModifierNum.ToString());
               }
               else if ('M' == range)
               {
                  toHitModifierNum += 25;
                  Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): fort +25 medium range mod=" + toHitModifierNum.ToString());
               }
               else if ('L' == range)
               {
                  toHitModifierNum += 35;
                  Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitModifier(): fort +35 long range mod=" + toHitModifierNum.ToString());
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitModifier(): reached default range=" + range);
                  return FN_ERROR;
               }
            }
         }
         return toHitModifierNum;
      }
      public static double GetShermanToHitBaseNumber(IGameInstance gi, IMapItem enemyUnit)
      {
         double toHitNum = 0.0;
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): lastReport=null");
            return FN_ERROR;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         string guntype = card.myMainGun;
         //------------------------------------
         if (3 != enemyUnit.TerritoryCurrent.Name.Length)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): 3 != TerritoryCurrent.Name.Length=" + enemyUnit.TerritoryCurrent.Name);
            return FN_ERROR;
         }
         char sector = enemyUnit.TerritoryCurrent.Name[1];
         char range = enemyUnit.TerritoryCurrent.Name[2];
         //----------------------------------------------------
         string enemyUnitType = enemyUnit.GetEnemyUnit();
         if ("ERROR" == enemyUnitType)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): unknown enemyUnit=" + enemyUnit.Name);
            return FN_ERROR;
         }
         switch (enemyUnitType)
         {
            case "LW":
            case "MG":
            case "TRUCK":
            case "ATG":
            case "Pak38":
            case "Pak40":
            case "Pak43":
               if( "75" ==  guntype )
               {
                  if( "Direct" == gi.ShermanTypeOfFire) // TableMgr.GetShermanToHitBaseNumber()
                  {
                     if ('C' == range)
                        toHitNum = 55;
                     else if ('M' == range)
                        toHitNum = 30;
                     else if ('L' == range)
                        toHitNum = 00;
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): unknown range=" + range.ToString());
                        return FN_ERROR;
                     }
                  }
                  else if ("Area" == gi.ShermanTypeOfFire)
                  {
                     if ('C' == range)
                        toHitNum = 45;
                     else if ('M' == range)
                        toHitNum = 60;
                     else if ('L' == range)
                        toHitNum = 50;
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): unknown range=" + range.ToString());
                        return FN_ERROR;
                     }
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): ShermanTypeOfFire=" + gi.ShermanTypeOfFire);
                     return FN_ERROR;
                  }
               }
               else if ("76L" == guntype)
               {
                  if ("Direct" == gi.ShermanTypeOfFire) // TableMgr.GetShermanToHitBaseNumber()
                  {
                     if ('C' == range)
                        toHitNum = 55;
                     else if ('M' == range)
                        toHitNum = 45;
                     else if ('L' == range)
                        toHitNum = 05;
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): unknown range=" + range.ToString());
                        return FN_ERROR;
                     }
                  }
                  else if ("Area" == gi.ShermanTypeOfFire) // TableMgr.GetShermanToHitBaseNumber()
                  {
                     if ('C' == range)
                        toHitNum = 45;
                     else if ('M' == range)
                        toHitNum = 60;
                     else if ('L' == range)
                        toHitNum = 55;
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): unknown range=" + range.ToString());
                        return FN_ERROR;
                     }
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): ShermanTypeOfFire=" + gi.ShermanTypeOfFire);
                     return FN_ERROR;
                  }
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): unknown guntype=" + guntype);
                  return FN_ERROR;
               }
               break;
            case "PSW":
            case "SPW":
            case "PzIV":
            case "PzV":
            case "TANK":
            case "PzVIe":
            case "PzVIb":
            case "SPG":
            case "STuGIIIg":
            case "MARDERII":
            case "MARDERIII":
            case "JdgPzIV":
            case "JdgPz38t":
               if ("75" == guntype)
               {
                  if ("Direct" == gi.ShermanTypeOfFire) // TableMgr.GetShermanToHitBaseNumber()
                  {
                     if ('C' == range)
                        toHitNum = 75;
                     else if ('M' == range)
                        toHitNum = 55;
                     else if ('L' == range)
                        toHitNum = 20;
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): unknown range=" + range.ToString());
                        return FN_ERROR;
                     }
                  }
                  else if ("Area" == gi.ShermanTypeOfFire) // TableMgr.GetShermanToHitBaseNumber()
                  {
                     if ('C' == range)
                        toHitNum = 45;
                     else if ('M' == range)
                        toHitNum = 60;
                     else if ('L' == range)
                        toHitNum = 50;
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): unknown range=" + range.ToString());
                        return FN_ERROR;
                     }
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): ShermanTypeOfFire=" + gi.ShermanTypeOfFire);
                     return toHitNum;
                  }
               }
               else if ("76L" == guntype)
               {
                  if ("Direct" == gi.ShermanTypeOfFire) // TableMgr.GetShermanToHitBaseNumber()
                  {
                     if ('C' == range)
                        toHitNum = 75;
                     else if ('M' == range)
                        toHitNum = 65;
                     else if ('L' == range)
                        toHitNum = 40;
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): unknown range=" + range.ToString());
                        return FN_ERROR;
                     }
                  }
                  else if ("Area" == gi.ShermanTypeOfFire) // TableMgr.GetShermanToHitBaseNumber()
                  {
                     if ('C' == range)
                        toHitNum = 45;
                     else if ('M' == range)
                        toHitNum = 60;
                     else if ('L' == range)
                        toHitNum = 55;
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): unknown range=" + range.ToString());
                        return FN_ERROR;
                     }
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): ShermanTypeOfFire=" + gi.ShermanTypeOfFire);
                     return FN_ERROR;
                  }
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): unknown guntype=" + guntype);
                  return FN_ERROR;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): 2-Reached Default enemyUnit=" + enemyUnit);
               return FN_ERROR;
         }
         //------------------------------------
         Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitBaseNumber(): Original To Hit base#=" + toHitNum.ToString());
         int numSmokeMarkers = Territory.GetSmokeCount(gi, sector, range);
         if (numSmokeMarkers < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): GetSmokeCount() returned error");
            return FN_ERROR;
         }
         if (0 < numSmokeMarkers)
         {
            double multiplier = Math.Pow(0.5, numSmokeMarkers);
            toHitNum *= multiplier;
         }
         //------------------------------------
         if ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")))
            toHitNum *= 0.5;
         //------------------------------------
         Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "GetShermanToHitBaseNumber(): After Smoke/Fog base#=" + toHitNum.ToString());
         return toHitNum;
      }
      public static int GetShermanRateOfFireModifier(IGameInstance gi)
      {
         int rateOfFireModifier = 0;
         //-------------------------------------------------
         ICrewMember? gunner = gi.GetCrewMemberByRole("Gunner");
         if (null == gunner)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanRateOfFireModifier(): gunner=null");
            return FN_ERROR;
         }
         rateOfFireModifier -= gunner.Rating;
         //-------------------------------------------------
         ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
         if (null == loader)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanRateOfFireModifier(): loader=null");
            return FN_ERROR;
         }
         rateOfFireModifier -= loader.Rating;
         //-------------------------------------------------
         if( true == gi.IsReadyRackReload())
         {
            rateOfFireModifier -= 10;
         }
         else
         {
            bool isAssistantPassesAmmo = false;
            foreach (IMapItem crewAction in gi.CrewActions)
            {
               if ("Assistant_PassAmmo" == crewAction.Name)
                  isAssistantPassesAmmo = true;
            }
            if (true == isAssistantPassesAmmo)
            {
               ICrewMember? assistant = gi.GetCrewMemberByRole("Assistant");
               if (null == assistant)
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetShermanRateOfFireModifier(): assistant=null");
                  return FN_ERROR;
               }
               rateOfFireModifier -= assistant.Rating;
            }
         }
         return rateOfFireModifier;
      }
      public static int GetShermanRateOfFire(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): lastReport=null");
            return FN_ERROR;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //---------------------------------------------------
         int rateOfFireNumber = 0;
         if ("75" == card.myMainGun)
         {
            rateOfFireNumber = 30;
         }
         else if ("75" == card.myMainGun)
         {
            rateOfFireNumber = 20;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToHitBaseNumber(): reached default guntype=" + card.myMainGun);
            return FN_ERROR;
         }
         rateOfFireNumber -= GetShermanRateOfFireModifier(gi);
         return rateOfFireNumber;
      }
      public static int GetShermanToKillInfantryModifier(IGameInstance gi, IMapItem enemyUnit, ShermanAttack hit)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToKillInfantryModifier(): lastReport=null");
            return FN_ERROR;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //------------------------------------
         if(("Direct" == hit.myAttackType) && ("He" != hit.myAmmoType ) )
         {
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "GetShermanToKillInfantryModifier(): Direct fire with AP or HVAP does nothing against infantry targets");
            hit.myIsNoChance = true;
            return NO_CHANCE;
         }
         //------------------------------------
         int toKillModifierNum = 0;
         if (("Direct" != hit.myAttackType) || (true == hit.myIsCriticalHit))
         {
            if (true == enemyUnit.IsBuilding)
            {
               if (false == hit.myIsCriticalHit)
               {
                  toKillModifierNum += 15;
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "GetShermanToKillInfantryModifier(): Building +15 mod=" + toKillModifierNum.ToString());
               }
               else
               {
                  toKillModifierNum -= 15;
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "GetShermanToKillInfantryModifier(): Building -15 mod=" + toKillModifierNum.ToString());
               }
            }
            if (true == enemyUnit.IsWoods)
            {
               if (false == hit.myIsCriticalHit)
               {
                  toKillModifierNum += 10;
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "GetShermanToKillInfantryModifier(): Woods +10 mod=" + toKillModifierNum.ToString());
               }
               else
               {
                  toKillModifierNum -= 10;
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "GetShermanToKillInfantryModifier(): Woods -10 mod=" + toKillModifierNum.ToString());
               }
            }
            if (true == enemyUnit.IsFortification) 
            {
               if (false == hit.myIsCriticalHit)
               {
                  toKillModifierNum += 20;
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "GetShermanToKillInfantryModifier(): Fort +20 mod=" + toKillModifierNum.ToString());
               }
               else
               {
                  toKillModifierNum -= 20;
                  Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "GetShermanToKillInfantryModifier(): Fort -20 mod=" + toKillModifierNum.ToString());
               }
            }
         }
         //------------------------------------
         if ((true == enemyUnit.Name.Contains("ATG")) || (true == enemyUnit.Name.Contains("Pak43")) || (true == enemyUnit.Name.Contains("Pak40")) || (true == enemyUnit.Name.Contains("Pak38")))
         {
            if (false == hit.myIsCriticalHit)
            {
               toKillModifierNum += 15;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "GetShermanToKillInfantryModifier(): ATG +15 mod=" + toKillModifierNum.ToString());
            }
            else
            {
               toKillModifierNum = KIA; // automatically kill ATG on critical hit
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "GetShermanToKillInfantryModifier(): ATG +1000 mod=" + toKillModifierNum.ToString());
            }
         }
         //------------------------------------
         if (true == enemyUnit.IsMovingInOpen)
         {
            toKillModifierNum -= 10;
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "GetShermanToKillInfantryModifier(): Moving In Open -10 mod=" + toKillModifierNum.ToString());
         }
         //------------------------------------
         if (true == lastReport.Weather.Contains("Deep Snow") || true == lastReport.Weather.Contains("Mud"))
         {
            toKillModifierNum += 5;
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "GetShermanToKillInfantryModifier(): Snow/Mud +5 mod=" + toKillModifierNum.ToString());
         }
         return toKillModifierNum;
      }
      public static int GetShermanToKillInfantryBaseNumber(IGameInstance gi, IMapItem enemyUnit, ShermanAttack hit)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToKillInfantryBaseNumber(): lastReport=null");
            return FN_ERROR;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //---------------------------------------------------
         int toKillNum = 0;
         if( "75" == card.myMainGun )
         {
            if ("Direct" == hit.myAttackType)
            {
               if (false == hit.myIsCriticalHit)
                  toKillNum = 55;
               else
                  toKillNum = 80;
            }
            else if ("Area" == hit.myAttackType)
            {
               if (false == hit.myIsCriticalHit)
                  toKillNum = 35;
               else
                  toKillNum = 55;
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "GetShermanToKillInfantryBaseNumber(): hit.myAttackType=" + hit.myAttackType);
               return FN_ERROR;
            }
         }
         else if ("76L" == card.myMainGun)
         {
            if ("Direct" == hit.myAttackType)
            {
               if (false == hit.myIsCriticalHit)
                  toKillNum = 30;
               else
                  toKillNum = 55;
            }
            else if ("Area" == hit.myAttackType)
            {
               if (false == hit.myIsCriticalHit)
                  toKillNum = 20;
               else
                  toKillNum = 30;
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "GetShermanToKillInfantryBaseNumber(): hit.myAttackType=" + hit.myAttackType);
               return FN_ERROR;
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToKillInfantryBaseNumber(): card.myMainGun=" + card.myMainGun);
            return FN_ERROR;
         }
         return toKillNum;
      }
      public static int GetShermanToKill75ApVehicleBaseNumber(IGameInstance gi, IMapItem enemyUnit, ShermanAttack hit)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75ApVehicleBaseNumber(): lastReport=null");
            return FN_ERROR;
         }
         //---------------------------------------------------
         if (3 != enemyUnit.TerritoryCurrent.Name.Length)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75ApVehicleBaseNumber(): 3 != TerritoryCurrent.Name.Length=" + enemyUnit.TerritoryCurrent.Name);
            return FN_ERROR;
         }
         char range = enemyUnit.TerritoryCurrent.Name[2];
         if( ('C' != range) && ('M' != range) && ('L' != range) )
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75ApVehicleBaseNumber(): invald range=" + range.ToString());
            return FN_ERROR;
         }
         //---------------------------------------------------
         string facing = GetShermanFireDirection(gi, enemyUnit, hit.myHitLocation);   // Get_ShermanToKill75ApVehicleBaseNumber()
         if ( "ERROR" == facing )
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75ApVehicleBaseNumber(): Get_ShermanFireDirection() returned error");
            return FN_ERROR;
         }
         if( "Thrown Track" == facing)
         {
            return THROWN_TRACK;
         }
         if (("Side" != facing) && ("Front" != facing) && ("Rear" != facing))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75ApVehicleBaseNumber(): invald facing=" + facing);
            return FN_ERROR;
         }
         //----------------------------------------------------
         string enemyUnitType = enemyUnit.GetEnemyUnit();
         if ("ERROR" == enemyUnitType)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75ApVehicleBaseNumber(): unknown enemyUnit=" + enemyUnit.Name);
            return FN_ERROR;
         }
         //---------------------------------------------------
         int toKillNum = 0;
         if (true == hit.myIsCriticalHit) // CRITICAL HIT
         {
            if ("PzV" == enemyUnitType)
            {
               if ("Turret" == hit.myHitLocation)
               {
                  if ('C' == range) toKillNum = 100;
                  else if ('M' == range) toKillNum = 100;
                  else toKillNum = 97;
               }
               else
               {
                  if ('C' == range) toKillNum = 83;
                  else if ('M' == range) toKillNum = 72;
                  else toKillNum = 58;
               }
            }
            else if ("PzVIe" == enemyUnitType)
            {
               if ('C' == range) toKillNum = 100;
               else if ('M' == range) toKillNum = 100;
               else toKillNum = 97;
            }
            else if (("PzVIb" == enemyUnitType) || ("TANK" == enemyUnitType))
            {
               if ("Turret" == hit.myHitLocation)
               {
                  if ('C' == range) toKillNum = 83;
                  else if ('M' == range) toKillNum = 72;
                  else toKillNum = 58;
               }
               else
               {
                  toKillNum = NO_CHANCE;
                  hit.myIsNoChance = true;
               }
            }
            else
            {
               toKillNum = KIA;
            }
            return toKillNum;
         }
         //---------------------------------------------------
         switch (enemyUnitType)
         {
            case "PzIV":
               if ("Front" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 58;
                     else if ('M' == range) toKillNum = 42;
                     else toKillNum = 28;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 28;
                     else if ('M' == range) toKillNum = 17;
                     else toKillNum = 03;
                  }
               }
               else if ("Side" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 83;
                     else if ('M' == range) toKillNum = 72;
                     else toKillNum = 58;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 92;
                     else if ('M' == range) toKillNum = 83;
                     else toKillNum = 72;
                  }
               }
               else  // rear
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 92;
                     else if ('M' == range) toKillNum = 83;
                     else toKillNum = 72;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 92;
                     else toKillNum = 83;
                  }
               }
               break;
            case "PzV":
               if ("Front" == facing)
               {
                  toKillNum = NO_CHANCE;  // no chance
                  hit.myIsNoChance = true;
               }
               else if ("Side" == facing)
               {
                  if ('C' == range) toKillNum = 58;
                  else if ('M' == range) toKillNum = 42;
                  else toKillNum = 28;
               }
               else  // rear
               {
                  if ('C' == range) toKillNum = 72;
                  else if ('M' == range) toKillNum = 58;
                  else toKillNum = 42;
               }
               break;
            case "PzVIe":
               if ("Front" == facing)
               {
                  toKillNum = NO_CHANCE;  // no chance
                  hit.myIsNoChance = true;
               }
               else if ("Side" == facing)
               {
                  if ('C' == range) toKillNum = 28;
                  else if ('M' == range) toKillNum = 17;
                  else toKillNum = 08;
               }
               else  // rear
               {
                  if ('C' == range) toKillNum = 42;
                  else if ('M' == range) toKillNum = 28;
                  else toKillNum = 17;
               }
               break;
            case "TANK":
            case "PzVIb":
               if ("Front" == facing)
               {
                  toKillNum = NO_CHANCE;  // no chance
                  hit.myIsNoChance = true;
               }
               else if ("Side" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 03;
                     else if ('M' == range)
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                     else
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 28;
                     else if ('M' == range) toKillNum = 17;
                     else toKillNum = 08;
                  }
               }
               else  // rear
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 08;
                     else if ('M' == range) toKillNum = 03;
                     else
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 42;
                     else if ('M' == range) toKillNum = 28;
                     else toKillNum = 17;
                  }
               }
               break;
            case "SPG":
            case "STuGIIIg":
               if ("Front" == facing)
               {
                  if ('C' == range) toKillNum = 28;
                  else if ('M' == range) toKillNum = 17;
                  else toKillNum = 08;
               }
               else if ("Side" == facing)
               {
                  if ('C' == range) toKillNum = 92;
                  else if ('M' == range) toKillNum = 83;
                  else toKillNum = 72;
               }
               else  // rear
               {
                  if ('C' == range) toKillNum = 95;
                  else if ('M' == range) toKillNum = 92;
                  else toKillNum = 83;
               }
               break;
            case "MARDERII":
               if ("Front" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 92;
                     else toKillNum = 83;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 92;
                     else if ('M' == range) toKillNum = 83;
                     else toKillNum = 72;
                  }
               }
               else if ("Side" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     toKillNum = 95;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 95;
                     else toKillNum = 92;
                  }
               }
               else  // rear
               {
                  toKillNum = 95;
               }
               break;
            case "MARDERIII":
               if ("Front" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 95;
                     else toKillNum = 92;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 92;
                     else toKillNum = 83;
                  }
               }
               else if ("Side" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     toKillNum = 95;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 95;
                     else toKillNum = 92;
                  }
               }
               else  // rear
               {
                  toKillNum = KIA;
               }
               break;
            case "JdgPzIV":
               if ("Front" == facing)
               {
                  toKillNum = NO_CHANCE;
                  hit.myIsNoChance = true;
               }
               else if ("Side" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 83;
                     else if ('M' == range) toKillNum = 72;
                     else toKillNum = 58;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 92;
                     else if ('M' == range) toKillNum = 83;
                     else toKillNum = 72;
                  }
               }
               else  // rear
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 92;
                     else if ('M' == range) toKillNum = 83;
                     else toKillNum = 72;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 92;
                     else toKillNum = 83;
                  }
               }
               break;
            case "JdgPz38t":
               if ("Front" == facing)
               {
                  toKillNum = NO_CHANCE;
                  hit.myIsNoChance = true;
               }
               else if ("Side" == facing)
               {
                  if ('C' == range) toKillNum = 92;
                  else if ('M' == range) toKillNum = 83;
                  else toKillNum = 72;
               }
               else  // rear
               {
                  if ('C' == range) toKillNum = 97;
                  else if ('M' == range) toKillNum = 92;
                  else toKillNum = 83;
               }
               break;
            case "PSW":
            case "SPW":
               toKillNum = 95;
               break;
            case "TRUCK":
               toKillNum = 75;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75ApVehicleBaseNumber(): 2-Reached Default enemyUnit=" + enemyUnit);
               return FN_ERROR;
         }
         //--------------------------------------
         return toKillNum;
      }
      public static int GetShermanToKill76ApVehicleBaseNumber(IGameInstance gi, IMapItem enemyUnit, ShermanAttack hit)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76ApVehicleBaseNumber(): lastReport=null");
            return FN_ERROR;
         }
         //---------------------------------------------------
         if (3 != enemyUnit.TerritoryCurrent.Name.Length)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76ApVehicleBaseNumber(): 3 != TerritoryCurrent.Name.Length=" + enemyUnit.TerritoryCurrent.Name);
            return FN_ERROR;
         }
         char range = enemyUnit.TerritoryCurrent.Name[2];
         if (('C' != range) && ('M' != range) && ('L' != range))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76ApVehicleBaseNumber(): invald range=" + range.ToString());
            return FN_ERROR;
         }
         //---------------------------------------------------
         string facing = GetShermanFireDirection(gi, enemyUnit, hit.myHitLocation);    // Get_ShermanToKill76ApVehicleBaseNumber()
         if ("ERROR" == facing)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76ApVehicleBaseNumber(): Get_ShermanFireDirection() returned error");
            return FN_ERROR;
         }
         if ("Thrown Track" == facing)
         {
            return THROWN_TRACK;
         }
         if (("Side" != facing) && ("Front" != facing) && ("Rear" != facing))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76ApVehicleBaseNumber(): invald facing=" + facing);
            return FN_ERROR;
         }
         //----------------------------------------------------
         string enemyUnitType = enemyUnit.GetEnemyUnit();
         if ("ERROR" == enemyUnitType)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76ApVehicleBaseNumber(): unknown enemyUnit=" + enemyUnit.Name);
            return FN_ERROR;
         }
         //---------------------------------------------------
         int toKillNum = 0;
         if (true == hit.myIsCriticalHit) // CRITICAL HIT
         {
            if ( (("TANK" == enemyUnitType) || ("PzVIb" == enemyUnitType)) && ("Front" == facing) && ("Hull" == hit.myHitLocation))
            {
               if ('C' == range) toKillNum = 58;
               else if ('M' == range) toKillNum = 42;
               else toKillNum = 28;
            }
            else
            {
               toKillNum = KIA;
            }
            return toKillNum;
         }
         //---------------------------------------------------
         switch (enemyUnitType)
         {
            case "PzIV":
               if ("Front" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 92;
                     else if ('M' == range) toKillNum = 83;
                     else toKillNum = 72;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 72;
                     else if ('M' == range) toKillNum = 58;
                     else toKillNum = 42;
                  }
               }
               else if ("Side" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 92;
                     else toKillNum = 83;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 95;
                     else toKillNum = 92;
                  }
               }
               else  // rear
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 95;
                     else toKillNum = 92;
                  }
                  else
                  {
                     toKillNum = 95;
                  }
               }
               break;
            case "PzV":
               if ("Front" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 03;
                     else if ('M' == range)
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                     else
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                  }
                  else
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
               }
               else if ("Side" == facing)
               {
                  if ('C' == range) toKillNum = 92;
                  else if ('M' == range) toKillNum = 83;
                  else toKillNum = 72;
               }
               else  // rear
               {
                  if ('C' == range) toKillNum = 95;
                  else if ('M' == range) toKillNum = 92;
                  else toKillNum = 83;
               }
               break;
            case "PzVIe":
               if ("Front" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 03;
                     else if ('M' == range)
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                     else
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                  }
                  else
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
               }
               else if ("Side" == facing)
               {
                  if ('C' == range) toKillNum = 72;
                  else if ('M' == range) toKillNum = 58;
                  else toKillNum = 42;
               }
               else  // rear
               {
                  if ('C' == range) toKillNum = 83;
                  else if ('M' == range) toKillNum = 72;
                  else toKillNum = 58;
               }
               break;
            case "TANK":
            case "PzVIb":
               if ("Front" == facing)
               {
                  toKillNum = NO_CHANCE;  // no chance
                  hit.myIsNoChance = true;
               }
               else if ("Side" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 28;
                     else if ('M' == range) toKillNum = 17;
                     else toKillNum = 08;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 72;
                     else if ('M' == range) toKillNum = 58;
                     else toKillNum = 42;
                  }
               }
               else  // rear
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 42;
                     else if ('M' == range) toKillNum = 28;
                     else toKillNum = 17;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 83;
                     else if ('M' == range) toKillNum = 72;
                     else toKillNum = 58;
                  }
               }
               break;
            case "SPG":
            case "STuGIIIg":
               if ("Front" == facing)
               {
                  if ('C' == range) toKillNum = 72;
                  else if ('M' == range) toKillNum = 58;
                  else toKillNum = 42;
               }
               else if ("Side" == facing)
               {
                  toKillNum = 83;
               }
               else  // rear
               {
                  toKillNum = 95;
               }
               break;
            case "MARDERII":
               toKillNum = 95;
               break;
            case "MARDERIII":
               toKillNum = 95;
               break;
            case "JdgPzIV":
               if ("Front" == facing)
               {
                  if ('C' == range) toKillNum = 03;
                  else if ('M' == range)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
               }
               else if ("Side" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 95;
                     else toKillNum = 92;
                  }
                  else
                  {
                     toKillNum = 95;
                  }
               }
               else  // rear
               {
                  toKillNum = 95;
               }
               break;
            case "JdgPz38t":
               if ("Front" == facing)
               {
                  if ('C' == range) toKillNum = 03;
                  else if ('M' == range)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
               }
               else if ("Side" == facing)
               {
                  toKillNum = 95;
               }
               else  // rear
               {
                  toKillNum = 95;
               }
               break;
            case "PSW":
            case "SPW":
               toKillNum = 95;
               break;
            case "TRUCK":
               toKillNum = 75;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76ApVehicleBaseNumber(): 2-Reached Default enemyUnit=" + enemyUnit);
               return FN_ERROR;
         }
         //--------------------------------------
         return toKillNum;
      }
      public static int GetShermanToKill76HvapVehicleBaseNumber(IGameInstance gi, IMapItem enemyUnit, ShermanAttack hit)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76HvapVehicleBaseNumber(): lastReport=null");
            return FN_ERROR;
         }
         //---------------------------------------------------
         if (3 != enemyUnit.TerritoryCurrent.Name.Length)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76HvapVehicleBaseNumber(): 3 != TerritoryCurrent.Name.Length=" + enemyUnit.TerritoryCurrent.Name);
            return FN_ERROR;
         }
         char range = enemyUnit.TerritoryCurrent.Name[2];
         if (('C' != range) && ('M' != range) && ('L' != range))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76HvapVehicleBaseNumber(): invald range=" + range.ToString());
            return FN_ERROR;
         }
         //---------------------------------------------------
         string facing = GetShermanFireDirection(gi, enemyUnit, hit.myHitLocation);   // Get_ShermanToKill76HvapVehicleBaseNumber()
         if ("ERROR" == facing)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76HvapVehicleBaseNumber(): lastReport=null");
            return FN_ERROR;
         }
         if ("Thrown Track" == facing)
         {
            return THROWN_TRACK;
         }
         if (("Side" != facing) && ("Front" != facing) && ("Rear" != facing))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76HvapVehicleBaseNumber(): invald facing=" + facing);
            return FN_ERROR;
         }
         //----------------------------------------------------
         string enemyUnitType = enemyUnit.GetEnemyUnit();
         if ("ERROR" == enemyUnitType)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76HvapVehicleBaseNumber(): unknown enemyUnit=" + enemyUnit.Name);
            return FN_ERROR;
         }
         //---------------------------------------------------
         int toKillNum = 0;
         if (true == hit.myIsCriticalHit) // CRITICAL HIT
         {
            toKillNum = KIA;
            return toKillNum;
         }
         //---------------------------------------------------
         switch (enemyUnitType)
         {
            case "PzIV":
               if ("Front" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 95;
                     else toKillNum = 92;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 95;
                     else toKillNum = 72;
                  }
               }
               else if ("Side" == facing)
               {
                  toKillNum = 95;
               }
               else  // rear
               {
                  toKillNum = 95;
               }
               break;
            case "PzV":
               if ("Front" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 83;
                     else if ('M' == range) toKillNum = 42;
                     else toKillNum = 03;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 28;
                     else if ('M' == range) toKillNum = 03;
                     else
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                  }
               }
               else if ("Side" == facing)
               {
                  if ('C' == range) toKillNum = 95;
                  else if ('M' == range) toKillNum = 95;
                  else toKillNum = 92;
               }
               else  // rear
               {
                  toKillNum = 95;
               }
               break;
            case "PzVIe":
               if ("Front" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 83;
                     else if ('M' == range) toKillNum = 42;
                     else toKillNum = 03;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 83;
                     else toKillNum = 28;
                  }
               }
               else if ("Side" == facing)
               {
                  if ('C' == range) toKillNum = 95;
                  else if ('M' == range) toKillNum = 95;
                  else toKillNum = 72;
               }
               else  // rear
               {
                  if ('C' == range) toKillNum = 95;
                  else if ('M' == range) toKillNum = 95;
                  else toKillNum = 72;
               }
               break;
            case "TANK":
            case "PzVIb":
               if ("Front" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 28;
                     else if ('M' == range) toKillNum = 03;
                     else
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                  }
                  else
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
               }
               else if ("Side" == facing)
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 83;
                     else toKillNum = 28;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 95;
                     else toKillNum = 72;
                  }
               }
               else  // rear
               {
                  if ("Turret" == hit.myHitLocation)
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 95;
                     else toKillNum = 42;
                  }
                  else
                  {
                     if ('C' == range) toKillNum = 95;
                     else if ('M' == range) toKillNum = 95;
                     else toKillNum = 83;
                  }
               }
               break;
            case "SPG":
            case "STuGIIIg":
               if ("Front" == facing)
               {
                  if ('C' == range) toKillNum = 95;
                  else if ('M' == range) toKillNum = 95;
                  else toKillNum = 72;
               }
               else if ("Side" == facing)
               {
                  toKillNum = 95;
               }
               else  // rear
               {
                  toKillNum = 95;
               }
               break;
            case "MARDERII":
               toKillNum = 95;
               break;
            case "MARDERIII":
               toKillNum = 95;
               break;
            case "JdgPzIV":
               if ("Front" == facing)
               {
                  if ('C' == range) toKillNum = 83;
                  else if ('M' == range) toKillNum = 42;
                  else toKillNum = 03;
               }
               else if ("Side" == facing)
               {
                  toKillNum = 95;
               }
               else  // rear
               {
                  toKillNum = 95;
               }
               break;
            case "JdgPz38t":
               if ("Front" == facing)
               {
                  if ('C' == range) toKillNum = 83;
                  else if ('M' == range) toKillNum = 42;
                  else toKillNum = 03;
               }
               else if ("Side" == facing)
               {
                  toKillNum = 95;
               }
               else  // rear
               {
                  toKillNum = 95;
               }
               break;
            case "PSW":
            case "SPW":
               toKillNum = 95;
               break;
            case "TRUCK":
               toKillNum = 75;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76HvapVehicleBaseNumber(): 2-Reached Default enemyUnit=" + enemyUnit);
               return FN_ERROR;
         }
         //--------------------------------------
         return toKillNum;
      }
      public static int GetShermanToKill75HeVehicleBaseNumber(IGameInstance gi, IMapItem enemyUnit, ShermanAttack hit)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75HeVehicleBaseNumber(): lastReport=null");
            return FN_ERROR;
         }
         //---------------------------------------------------
         if (3 != enemyUnit.TerritoryCurrent.Name.Length)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75HeVehicleBaseNumber(): 3 != TerritoryCurrent.Name.Length=" + enemyUnit.TerritoryCurrent.Name);
            return FN_ERROR;
         }
         char range = enemyUnit.TerritoryCurrent.Name[2];
         if (('C' != range) && ('M' != range) && ('L' != range))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75HeVehicleBaseNumber(): invald range=" + range.ToString());
            return FN_ERROR;
         }
         //---------------------------------------------------
         string facing = GetShermanFireDirection(gi, enemyUnit, hit.myHitLocation);   // Get_ShermanToKill75HeVehicleBaseNumber()
         if ("ERROR" == facing)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75HeVehicleBaseNumber(): lastReport=null");
            return FN_ERROR;
         }
         if ("Thrown Track" == facing)
         {
            return THROWN_TRACK;
         }
         if (("Side" != facing) && ("Front" != facing) && ("Rear" != facing))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75HeVehicleBaseNumber(): invald facing=" + facing);
            return FN_ERROR;
         }
         //----------------------------------------------------
         string enemyUnitType = enemyUnit.GetEnemyUnit();
         if ("ERROR" == enemyUnitType)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75HeVehicleBaseNumber(): unknown enemyUnit=" + enemyUnit.Name);
            return FN_ERROR;
         }
         //---------------------------------------------------
         int toKillNum = 0;
         if( false == hit.myIsCriticalHit )
         {
            switch (enemyUnitType)
            {
               case "PzIV":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 03;
                     else
                        toKillNum = 08;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 08;
                     else
                        toKillNum = 17;
                  }
                  break;
               case "PzV":
               case "PzVIe":
               case "TANK":
               case "PzVIb":
                  toKillNum = NO_CHANCE;
                  hit.myIsNoChance = true;
                  break;
               case "SPG":
               case "STuGIIIg":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 08;
                  }
                  else  // rear
                  {
                     toKillNum = 17;
                  }
                  break;
               case "MARDERII":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 17;
                     else
                        toKillNum = 08;
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 42;
                     else
                        toKillNum = 28;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 58;
                     else
                        toKillNum = 42;
                  }
                  break;
               case "MARDERIII":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 28;
                     else
                        toKillNum = 17;
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 42;
                     else
                        toKillNum = 28;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 58;
                     else
                        toKillNum = 42;
                  }
                  break;
               case "JdgPzIV":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 03;
                     else
                        toKillNum = 08;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 08;
                     else
                        toKillNum = 17;
                  }
                  break;
               case "JdgPz38t":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 08;
                  }
                  else  // rear
                  {
                     toKillNum = 17;
                  }
                  break;
               case "PSW":
               case "SPW":
                  if ("Front" == facing)
                  {
                     toKillNum = 28;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 28;
                  }
                  else  // rear
                  {
                     toKillNum = 42;
                  }
                  break;
               case "TRUCK":
                  toKillNum = 95;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75HeVehicleBaseNumber(): 2-Reached Default enemyUnit=" + enemyUnit);
                  return FN_ERROR;
            }
         }
         //---------------------------------------------------
         else
         {
            switch (enemyUnitType)
            {
               case "PzIV":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 58;
                     else
                        toKillNum = 42;
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 83;
                     else
                        toKillNum = 92;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 92;
                     else
                        toKillNum = 97;
                  }
                  break;
               case "PzV":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 58;
                  }
                  else  // rear
                  {
                     toKillNum = 72;
                  }
                  break;
               case "PzVIe":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                     else
                     {
                        toKillNum = 03;
                     }
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 28;
                  }
                  else  // rear
                  {
                     toKillNum = 42;
                  }
                  break;
               case "TANK":
               case "PzVIb":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 03;
                     else
                        toKillNum = 28;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 08;
                     else
                        toKillNum = 42;
                  }
                  break;
               case "SPG":
               case "STuGIIIg":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 97;
                     else
                        toKillNum = 92;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 100;
                  }
                  else  // rear
                  {
                     toKillNum = 100;
                  }
                  break;
               case "MARDERII":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 97;
                     else
                        toKillNum = 92;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 100;
                  }
                  else  // rear
                  {
                     toKillNum = 100;
                  }
                  break;
               case "MARDERIII":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 100;
                     else
                        toKillNum = 97;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 100;
                  }
                  else  // rear
                  {
                     toKillNum = 100;
                  }
                  break;
               case "JdgPzIV":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 83;
                     else
                        toKillNum = 92;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 92;
                     else
                        toKillNum = 97;
                  }
                  break;
               case "JdgPz38t":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 92;
                  }
                  else  // rear
                  {
                     toKillNum = 97;
                  }
                  break;
               case "PSW":
               case "SPW":
                  toKillNum = 100;
                  break;
               case "TRUCK":
                  toKillNum = 100;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill75HeVehicleBaseNumber(): 2-Reached Default enemyUnit=" + enemyUnit);
                  return FN_ERROR;
            }
         }
         //--------------------------------------
         return toKillNum;
      }
      public static int GetShermanToKill76HeVehicleBaseNumber(IGameInstance gi, IMapItem enemyUnit, ShermanAttack hit)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76HeVehicleBaseNumber(): lastReport=null");
            return FN_ERROR;
         }
         //---------------------------------------------------
         if (3 != enemyUnit.TerritoryCurrent.Name.Length)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76HeVehicleBaseNumber(): 3 != TerritoryCurrent.Name.Length=" + enemyUnit.TerritoryCurrent.Name);
            return FN_ERROR;
         }
         char range = enemyUnit.TerritoryCurrent.Name[2];
         if (('C' != range) && ('M' != range) && ('L' != range))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76HeVehicleBaseNumber(): invald range=" + range.ToString());
            return FN_ERROR;
         }
         //---------------------------------------------------
         string facing = GetShermanFireDirection(gi, enemyUnit, hit.myHitLocation); // Get_ShermanToKill76HeVehicleBaseNumber
         if ("ERROR" == facing)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanToKill76HeVehicleBaseNumber(): lastReport=null");
            return FN_ERROR;
         }
         if ("Thrown Track" == facing)
         {
            return THROWN_TRACK;
         }
         if (("Side" != facing) && ("Front" != facing) && ("Rear" != facing))
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToKill76HeVehicleBaseNumber(): invald facing=" + facing);
            return FN_ERROR;
         }
         //----------------------------------------------------
         string enemyUnitType = enemyUnit.GetEnemyUnit();
         if ("ERROR" == enemyUnitType)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanToKill76HeVehicleBaseNumber(): unknown enemyUnit=" + enemyUnit.Name);
            return FN_ERROR;
         }
         //---------------------------------------------------
         int toKillNum = 0;
         if (false == hit.myIsCriticalHit)
         {
            switch (enemyUnitType)
            {
               case "PzIV":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                     else
                     {
                        toKillNum = 02;
                     }
                  }
                  break;
               case "PzV":
               case "PzVIe":
               case "PzVIb":
               case "TANK":
                  toKillNum = NO_CHANCE;
                  hit.myIsNoChance = true;
                  break;
               case "SPG":
               case "STuGIIIg":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else  // rear
                  {
                     toKillNum = 02;
                  }
                  break;
               case "MARDERII":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                     {
                        toKillNum = 02;
                     }
                     else
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 27;
                     else
                        toKillNum = 13;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 43;
                     else
                        toKillNum = 27;
                  }
                  break;
               case "MARDERIII":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 13;
                     else
                        toKillNum = 02;
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 27;
                     else
                        toKillNum = 13;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 43;
                     else
                        toKillNum = 27;
                  }
                  break;
               case "JdgPzIV":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }   
                     else
                     {
                        toKillNum = 02;
                     }
                  }
                  break;
               case "JdgPz38t":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 02;
                  }
                  else  // rear
                  {
                     toKillNum = 02;
                  }
                  break;
               case "PSW":
               case "SPW":
                  if ("Front" == facing)
                  {
                     toKillNum = 13;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 13;
                  }
                  else  // rear
                  {
                     toKillNum = 27;
                  }
                  break;
               case "TRUCK":
                  toKillNum = 95;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetShermanToKill75HeVehicleBaseNumber(): 2-Reached Default enemyUnit=" + enemyUnit);
                  return FN_ERROR;
            }
         }
         //---------------------------------------------------
         else
         {
            switch (enemyUnitType)
            {
               case "PzIV":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 43;
                     else
                        toKillNum = 27;
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 88;
                     else
                        toKillNum = 77;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 77;
                     else
                        toKillNum = 82;
                  }
                  break;
               case "PzV":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 43;
                  }
                  else  // rear
                  {
                     toKillNum = 57;
                  }
                  break;
               case "PzVIe":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 13;
                  }
                  else  // rear
                  {
                     toKillNum = 27;
                  }
                  break;
               case "TANK":
               case "PzVIb":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                     else
                     {
                        toKillNum = 13;
                     }
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                     {
                        toKillNum = NO_CHANCE;
                        hit.myIsNoChance = true;
                     }
                     else
                     {
                        toKillNum = 27;
                     }
                  }
                  break;
               case "SPG":
               case "STuGIIIg":
                  if ("Front" == facing)
                  {
                     toKillNum = 13;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 77;
                  }
                  else  // rear
                  {
                     toKillNum = 82;
                  }
                  break;
               case "MARDERII":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 82;
                     else
                        toKillNum = 77;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 100;
                  }
                  else  // rear
                  {
                     toKillNum = 100;
                  }
                  break;
               case "MARDERIII":
                  if ("Front" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 100;
                     else
                        toKillNum = 82;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 100;
                  }
                  else  // rear
                  {
                     toKillNum = 100;
                  }
                  break;
               case "JdgPzIV":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 67;
                     else
                        toKillNum = 77;
                  }
                  else  // rear
                  {
                     if ("Turret" == hit.myHitLocation)
                        toKillNum = 77;
                     else
                        toKillNum = 82;
                  }
                  break;
               case "JdgPz38t":
                  if ("Front" == facing)
                  {
                     toKillNum = NO_CHANCE;
                     hit.myIsNoChance = true;
                  }
                  else if ("Side" == facing)
                  {
                     toKillNum = 77;
                  }
                  else  // rear
                  {
                     toKillNum = 82;
                  }
                  break;
               case "PSW":
               case "SPW":
                  toKillNum = 100;
                  break;
               case "TRUCK":
                  toKillNum = 100;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetShermanToKill75HeVehicleBaseNumber(): 2-Reached Default enemyUnit=" + enemyUnit);
                  return FN_ERROR;
            }
         }
         //--------------------------------------
         return toKillNum;
      }
      public static int GetShermanMgToKillModifier(IGameInstance gi, IMapItem enemyUnit, string mgType, bool isAdvancingFire=false)
      {
         //------------------------------------
         if (3 != enemyUnit.TerritoryCurrent.Name.Length)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): 3 != TerritoryCurrent.Name.Length=" + enemyUnit.TerritoryCurrent.Name);
            return FN_ERROR;
         }
         char range = enemyUnit.TerritoryCurrent.Name[2];
         if (('C' != range) && ('M' != range) && ('L' != range))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): unknown range=" + range.ToString());
            return FN_ERROR;
         }
         //------------------------------------
         string enemyUnitType = enemyUnit.GetEnemyUnit();
         if ("ERROR" == enemyUnitType)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): GetEnemyUnit() returned error for enemyUnit=" + enemyUnit.Name);
            return FN_ERROR;
         }
         if (("LW" != enemyUnitType) && ("MG" != enemyUnitType) && ("ATG" != enemyUnitType) && ("Pak38" != enemyUnitType) && ("Pak40" != enemyUnitType) && ("Pak43" != enemyUnitType) && ("TRUCK" != enemyUnitType))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): unknown enemyType=" + enemyUnitType);
            return FN_ERROR;
         }
         //------------------------------------
         int toKillModifierNum = 0;
         bool isShermanMovingOrPivoting = false;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if (("Commander_FireAaMg" == crewAction.Name) && ("Aa" == mgType))
            {
               ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
               if (null == commander)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): commander=null");
                  return FN_ERROR;
               }
               toKillModifierNum -= commander.Rating;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): commander rating -" + commander.Rating.ToString() + "  mod=" + toKillModifierNum.ToString());
            }
            if (("Commander_FireSubMg" == crewAction.Name) && ("Sub" == mgType))
            {
               ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
               if (null == commander)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): commander=null");
                  return FN_ERROR;
               }
               toKillModifierNum -= commander.Rating;
            }
            if (("Loader_FireSubMg" == crewAction.Name) && ("Sub" == mgType))
            {
               ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
               if (null == loader)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): loader=null");
                  return FN_ERROR;
               }
               toKillModifierNum -= loader.Rating;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): gunner rating -" + loader.Rating.ToString() + "  mod=" + toKillModifierNum.ToString());
            }
            if (("Loader_FireAaMg" == crewAction.Name) && ("Aa" == mgType))
            {
               ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
               if (null == loader)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): loader=null");
                  return FN_ERROR;
               }
               toKillModifierNum -= loader.Rating;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): gunner rating -" + loader.Rating.ToString() + "  mod=" + toKillModifierNum.ToString());
            }
            if (("Gunner_FireCoaxialMg" == crewAction.Name) && ("Coaxial" == mgType))
            {
               ICrewMember? gunner = gi.GetCrewMemberByRole("Gunner");
               if (null == gunner)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): gunner=null");
                  return FN_ERROR;
               }
               toKillModifierNum -= gunner.Rating;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): gunner rating -" + gunner.Rating.ToString() + "  mod=" + toKillModifierNum.ToString());
            }
            if (("Assistant_FireBowMg" == crewAction.Name) && ("Bow" == mgType))
            {
               ICrewMember? assistant = gi.GetCrewMemberByRole("Assistant");
               if (null == assistant)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): assistant=null");
                  return FN_ERROR;
               }
               toKillModifierNum -= assistant.Rating;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): Assistant rating -" + assistant.Rating.ToString() + "  mod=" + toKillModifierNum.ToString());
            }
            if ("Driver_Forward" == crewAction.Name)
               isShermanMovingOrPivoting = true;
            if ("Driver_ForwardToHullDown" == crewAction.Name)
               isShermanMovingOrPivoting = true;
            if ("Driver_Reverse" == crewAction.Name)
               isShermanMovingOrPivoting = true;
            if ("Driver_ReverseToHullDown" == crewAction.Name)
               isShermanMovingOrPivoting = true;
            if ("Driver_PivotTank" == crewAction.Name)
               isShermanMovingOrPivoting = true;
         }
         //------------------------------------
         if (true == gi.IsCommanderDirectingMgFire) // Get_ShermanMgToKillModifier()
         {
            ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
            if (null == commander)
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): commander=null");
               return FN_ERROR;
            }
            toKillModifierNum -= commander.Rating;
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): commander directing fire rating -" + commander.Rating.ToString() + "  mod=" + toKillModifierNum.ToString());
         }
         //------------------------------------
         if ((true == enemyUnit.IsMovingInOpen) && ("LW" == enemyUnit.GetEnemyUnit()) )
         {
            if ("Bow" == mgType)
            {
               toKillModifierNum -= 10;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): Box moving in open -10 mod=" + toKillModifierNum.ToString());
            }
            else if ("Coaxial" == mgType)
            {
               toKillModifierNum -= 15;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): Coaxial moving in open -15 mod=" + toKillModifierNum.ToString());
            }
            else if ("Aa" == mgType)
            {
               toKillModifierNum -= 15;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): AA moving in open -15 mod=" + toKillModifierNum.ToString());
            }
            else if ("Sub" == mgType)
            {
               toKillModifierNum -= 5;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): Sub moving in open -5 mod=" + toKillModifierNum.ToString());
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): unknown mgType=" + mgType);
               return FN_ERROR;
            }
         }
         //------------------------------------
         if (true == isAdvancingFire)
         {
            if ("Bow" == mgType)
            {
               toKillModifierNum += 10;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): advancing fire +10 mod=" + toKillModifierNum.ToString());
            }
            else if ("Coaxial" == mgType)
            {
               toKillModifierNum += 10;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier():  advancing fire +10 mod=" + toKillModifierNum.ToString());
            }
            else if ("Aa" == mgType)
            {
               toKillModifierNum += 10;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier():  advancing fire +10 mod=" + toKillModifierNum.ToString());
            }
            else if ("Sub" == mgType)
            {
               toKillModifierNum -= 10;
               Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier():  advancing fire -10 mod=" + toKillModifierNum.ToString());
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillModifier(): unknown mgType=" + mgType);
               return FN_ERROR;
            }
         }
         //------------------------------------
         if (true == isShermanMovingOrPivoting)
         {
            toKillModifierNum += 10;
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): sherman moving +10 mod=" + toKillModifierNum.ToString());
         }
         //------------------------------------
         if (true == enemyUnit.IsWoods) 
         {
            toKillModifierNum += 10;
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): in woods +10 mod=" + toKillModifierNum.ToString());
         }
         //------------------------------------
         if (true == enemyUnit.IsBuilding) 
         {
            toKillModifierNum += 15;
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): in building +15 mod=" + toKillModifierNum.ToString());
         }
         //------------------------------------
         if (("ATG" == enemyUnitType) || ("Pak38" == enemyUnitType) || ("Pak40" == enemyUnitType) || ("Pak43" == enemyUnitType))
         {
            toKillModifierNum += 15;
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): ATG +15 mod=" + toKillModifierNum.ToString());
         }
         //------------------------------------
         if (true == enemyUnit.IsFortification) 
         {
            toKillModifierNum += 20;
            Logger.Log(LogEnum.LE_SHOW_TO_KILL_MODIFIER, "Get_ShermanMgToKillModifier(): in IsFortification +20 mod=" + toKillModifierNum.ToString());
         }
         return toKillModifierNum;
      }
      public static int GetShermanMgToKillNumber(IGameInstance gi, IMapItem enemyUnit, string mgType) 
      {
         //------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillNumber(): lastReport=null");
            return FN_ERROR;
         }
         //------------------------------------
         if (3 != enemyUnit.TerritoryCurrent.Name.Length)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanMgToKillModifier(): 3 != TerritoryCurrent.Name.Length=" + enemyUnit.TerritoryCurrent.Name);
            return FN_ERROR;
         }
         char sector = enemyUnit.TerritoryCurrent.Name[1];
         char range = enemyUnit.TerritoryCurrent.Name[2];
         if (('C' != range) && ('M' != range) && ('L' != range))
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanMgToKillModifier(): unknown range=" + range.ToString());
            return FN_ERROR;
         }
         //------------------------------------
         double toKillNumber = 0.0;
         if ("Bow" == mgType)
         {
            if ('C' == range)
               toKillNumber = 20.0;
            else if ('M' == range)
               toKillNumber = 0.0;
            else
               toKillNumber = 0.0;
         }
         else if ("Coaxial" == mgType)
         {
            if ('C' == range)
               toKillNumber = 25.0;
            else if ('M' == range)
               toKillNumber = 20.0;
            else
               toKillNumber = 15.0;
         }
         else if ("Aa" == mgType)
         {
            if ('C' == range)
               toKillNumber = 30.0;
            else if ('M' == range)
               toKillNumber = 20.0;
            else
               toKillNumber = 10.0;
         }
         else
         {
            if ('C' == range)
               toKillNumber = 10.0;
            else if ('M' == range)
            {
               toKillNumber = NO_CHANCE;
            }
            else
            {
               toKillNumber = NO_CHANCE;
            }
         }
         //------------------------------------
         Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "Get_ShermanMgToKillNumber(): 1-base#=" + toKillNumber.ToString());
         int numSmokeMarkers = Territory.GetSmokeCount(gi, sector, range);
         if (numSmokeMarkers < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShermanMgToKillNumber(): GetSmokeCount() returned error");
            return FN_ERROR;
         }
         if (0 < numSmokeMarkers)
         {
            double multiplier = Math.Pow(0.5, numSmokeMarkers);
            toKillNumber *= multiplier;
         }
         //------------------------------------
         if ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")))
            toKillNumber *= 0.5;
         return (int)toKillNumber;
      }
      //-------------------------------------------
      private void CreateCombatCalender()
      {
         theCombatCalendarEntries.Add(new CombatCalendarEntry("07/27/44", EnumScenario.Advance, 3, EnumResistance.Light, "Corba Breakout")); // Day=0
         theCombatCalendarEntries.Add(new CombatCalendarEntry("07/28/44", EnumScenario.Battle, 4, EnumResistance.Medium, "Coutances"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("07/29/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("07/30/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Avranches"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("07/31/44", EnumScenario.Advance, 4, EnumResistance.Light)); // Day=4
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/01/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/02/44", EnumScenario.Advance, 2, EnumResistance.Light));
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
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/30/44", EnumScenario.Advance, 2, EnumResistance.Light)); //
         theCombatCalendarEntries.Add(new CombatCalendarEntry("08/31/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Commery"));            // Day=35 - 0 based counting
         //--------------------------------------------------------------------------------------------------------------- 
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/01/44", EnumScenario.Advance, 2, EnumResistance.Light));                        // Day=36
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/02-09/10 1943", EnumScenario.Retrofit, 10, EnumResistance.None));               // +++++++++++++++++++++++++++++++++Day=37
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
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/23-09/24 1943", EnumScenario.Retrofit, 10, EnumResistance.None));                // +++++++++++++++++++++++++++++++++Day=50 -- 3 days
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/25/44", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Counter Attack"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/26/44", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Counter Attack"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/27/44", EnumScenario.Battle, 6, EnumResistance.Medium, "Hill 318"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/28/44", EnumScenario.Battle, 5, EnumResistance.Medium, "Hill 318"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/29/44", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Arracourt"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("09/30/44", EnumScenario.Counterattack, 9, EnumResistance.Light));                   // Day=56
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/01/44", EnumScenario.Counterattack, 2, EnumResistance.Light));     //************Oct 1st
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
         theCombatCalendarEntries.Add(new CombatCalendarEntry("10/12-11/08 1943", EnumScenario.Retrofit, 10, EnumResistance.None));                // +++++++++++++++++++++++++++++++++Day=68
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
         theCombatCalendarEntries.Add(new CombatCalendarEntry("11/30/44", EnumScenario.Advance, 2, EnumResistance.Light, "Cleared zone of responsibility")); // Day=90
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/01/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Attacked Saare Union"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/02/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Attacked Saare Union"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/03/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/04/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/05/44", EnumScenario.Battle, 9, EnumResistance.Heavy, "Battle of Bining"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/06/44", EnumScenario.Battle, 9, EnumResistance.Heavy, "Battle of Bining"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/07-12/20 1943", EnumScenario.Retrofit, 10, EnumResistance.None));                         //  +++++++++++++++++++++++++++++++++Day=97
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/21/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/22/44", EnumScenario.Advance, 2, EnumResistance.Light, "Martelange")); // Day=99
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/23/44", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/24/44", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/25/44", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/26/44", EnumScenario.Advance, 9, EnumResistance.Heavy, "Into Bastogne"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/27/44", EnumScenario.Advance, 4, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/28/44", EnumScenario.Advance, 4, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/29/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Open Arion-Bastogne Highway"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/30/44", EnumScenario.Counterattack, 3, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("12/31/44", EnumScenario.Counterattack, 3, EnumResistance.Medium)); // Day=108
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/01/45", EnumScenario.Counterattack, 3, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/02/45", EnumScenario.Counterattack, 3, EnumResistance.Medium));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/03-01/08 1944", EnumScenario.Retrofit, 10, EnumResistance.None)); //  +++++++++++++++++++++++++++++++++Day=111
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
         theCombatCalendarEntries.Add(new CombatCalendarEntry("01/31/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions")); // Day=134
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/01/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/02/45", EnumScenario.Advance, 2, EnumResistance.Light, "Hosdorf"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/03-02/21 1944", EnumScenario.Retrofit, 10, EnumResistance.None));                   //  +++++++++++++++++++++++++++++++++Day=137
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/22/45", EnumScenario.Battle, 8, EnumResistance.Medium, "Geichlingen"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/23/45", EnumScenario.Battle, 6, EnumResistance.Medium, "Sinspenit"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/24/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/25/45", EnumScenario.Battle, 5, EnumResistance.Medium, "Rittersdorf"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/26/45", EnumScenario.Battle, 5, EnumResistance.Medium, "Bitburg"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/27/45", EnumScenario.Battle, 5, EnumResistance.Medium, "Matzen and Fleissen"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("02/28/45", EnumScenario.Counterattack, 2, EnumResistance.Medium)); //  +++++++++++++++++++++++++++++++++ Day=144
         //---------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/01/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/02/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/03-03/04 1944", EnumScenario.Retrofit, 10, EnumResistance.None)); //  +++++++++++++++++++++++++++++++++ Day=147
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/05/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/06/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/07/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/08/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/09/45", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/10/45", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/11/45", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/12-03/13 1944", EnumScenario.Retrofit, 10, EnumResistance.None));                            //  +++++++++++++++++++++++++++++++++ Day=155
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/14/45", EnumScenario.Advance, 9, EnumResistance.Medium, "Attack out of Moselle Bridgehead"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/15/45", EnumScenario.Advance, 7, EnumResistance.Medium, "Bad Kreuznauch"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/16/45", EnumScenario.Advance, 5, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/17/45", EnumScenario.Advance, 3, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/18/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/19/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/20/45", EnumScenario.Advance, 2, EnumResistance.Light, "Worms on the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/21-03/23 1944", EnumScenario.Retrofit, 10, EnumResistance.None));                         //  +++++++++++++++++++++++++++++++++ Day=163
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/24/45", EnumScenario.Advance, 2, EnumResistance.Light, "Cross the Rhine"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/25/45", EnumScenario.Advance, 2, EnumResistance.Light, "Hanau and Darmstadt"));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/26/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/27/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/28/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/29/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/30/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalendarEntries.Add(new CombatCalendarEntry("03/31/45", EnumScenario.Advance, 2, EnumResistance.Light)); // Day=171
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
         theCombatCalendarEntries.Add(new CombatCalendarEntry("Drive into Czechoslavakia", EnumScenario.Retrofit, 10, EnumResistance.None)); // Day=190  -- TOTAL 191 days
         //--------------------------------------------------------------------
      }
      private void CreateExitTable()
      {
         theExits[0, 0] = 8;
         theExits[0, 1] = 8;
         theExits[0, 2] = 8;
         theExits[0, 3] = 8;
         theExits[0, 4] = 8;
         theExits[0, 5] = 8;
         theExits[0, 6] = 5;
         theExits[0, 7] = 5;
         theExits[0, 8] = 5;
         theExits[0, 9] = 5;
         //----------------
         theExits[1, 0] = 8;
         theExits[1, 1] = 8;
         theExits[1, 2] = 8;
         theExits[1, 3] = 8;
         theExits[1, 4] = 8;
         theExits[1, 5] = 4;
         theExits[1, 6] = 4;
         theExits[1, 7] = 5;
         theExits[1, 8] = 5;
         theExits[1, 9] = 5;
         //----------------
         theExits[2, 0] = 7;
         theExits[2, 1] = 7;
         theExits[2, 2] = 7;
         theExits[2, 3] = 7;
         theExits[2, 4] = 7;
         theExits[2, 5] = 3;
         theExits[2, 6] = 3;
         theExits[2, 7] = 4;
         theExits[2, 8] = 4;
         theExits[2, 9] = 4;
         //----------------
         theExits[3, 0] = 7;
         theExits[3, 1] = 6;
         theExits[3, 2] = 6;
         theExits[3, 3] = 6;
         theExits[3, 4] = 3;
         theExits[3, 5] = 2;
         theExits[3, 6] = 2;
         theExits[3, 7] = 4;
         theExits[3, 8] = 4;
         theExits[3, 9] = 4;
         //----------------
         theExits[4, 0] = 6;
         theExits[4, 1] = 5;
         theExits[4, 2] = 5;
         theExits[4, 3] = 2;
         theExits[4, 4] = 2;
         theExits[4, 5] = 2;
         theExits[4, 6] = 2;
         theExits[4, 7] = 3;
         theExits[4, 8] = 3;
         theExits[4, 9] = 3;
         //----------------
         theExits[5, 0] = 5;
         theExits[5, 1] = 5;
         theExits[5, 2] = 5;
         theExits[5, 3] = 1;
         theExits[5, 4] = 1;
         theExits[5, 5] = 1;
         theExits[5, 6] = 1;
         theExits[5, 7] = 2;
         theExits[5, 8] = 2;
         theExits[5, 9] = 8;
         //----------------
         theExits[6, 0] = 5;
         theExits[6, 1] = 4;
         theExits[6, 2] = 10;
         theExits[6, 3] = 10;
         theExits[6, 4] = 10;
         theExits[6, 5] = 10;
         theExits[6, 6] = 10;
         theExits[6, 7] = 2;
         theExits[6, 8] = 2;
         theExits[6, 9] = 7;
         //----------------
         theExits[7, 0] = 4;
         theExits[7, 1] = 4;
         theExits[7, 2] = 10;
         theExits[7, 3] = 10;
         theExits[7, 4] = 10;
         theExits[7, 5] = 10;
         theExits[7, 6] = 10;
         theExits[7, 7] = 1;
         theExits[7, 8] = 7;
         theExits[7, 9] = 6;
         //----------------
         theExits[8, 0] = 4;
         theExits[8, 1] = 3;
         theExits[8, 2] = 9;
         theExits[8, 3] = 9;
         theExits[8, 4] = 9;
         theExits[8, 5] = 9;
         theExits[8, 6] = 9;
         theExits[8, 7] = 10;
         theExits[8, 8] = 6;
         theExits[8, 9] = 5;
         //----------------
         theExits[9, 0] = 3;
         theExits[9, 1] = 9;
         theExits[9, 2] = 9;
         theExits[9, 3] = 9;
         theExits[9, 4] = 9;
         theExits[9, 5] = 9;
         theExits[9, 6] = 9;
         theExits[9, 7] = 6;
         theExits[9, 8] = 5;
         theExits[9, 9] = 5;
      }
      private void CreateEnemyApToKill()
      {
         // armor class, facing, range, hull
         //======================================================================
         int[,,,] entry50L = new int[3, 3, 3, 2];
         theApToKills["50L"] = entry50L;
         entry50L[0, 0, 0, 0] = 17;  // ac=1, facing=front, range=C, turret
         entry50L[0, 0, 1, 0] = 08;  // ac=1, facing=front, range=M, turret
         entry50L[0, 0, 2, 0] = 03;  // ac=1, facing=front, range=L, turret
         entry50L[0, 0, 0, 1] = 17;  // ac=1, facing=front, range=C, hull
         entry50L[0, 0, 1, 1] = 08;  // ac=1, facing=front, range=M, hull
         entry50L[0, 0, 2, 1] = 03;  // ac=1, facing=front, range=L, hull
         //----------------------------
         entry50L[0, 1, 0, 0] = 42;  // ac=1, facing=side, range=C, turret
         entry50L[0, 1, 1, 0] = 28;  // ac=1, facing=side, range=M, turret
         entry50L[0, 1, 2, 0] = 17;  // ac=1, facing=side, range=L, turret
         entry50L[0, 1, 0, 1] = 72;  // ac=1, facing=side, range=C, hull
         entry50L[0, 1, 1, 1] = 58;  // ac=1, facing=side, range=M, hull
         entry50L[0, 1, 2, 1] = 42;  // ac=1, facing=side, range=L, hull
         //----------------------------
         entry50L[0, 2, 0, 0] = 58;  // ac=1, facing=rear, range=C, turret
         entry50L[0, 2, 1, 0] = 42;  // ac=1, facing=rear, range=M, turret
         entry50L[0, 2, 2, 0] = 28;  // ac=1, facing=rear, range=L, turret
         entry50L[0, 2, 0, 1] = 83;  // ac=1, facing=rear, range=C, hull
         entry50L[0, 2, 1, 1] = 72;  // ac=1, facing=rear, range=M, hull
         entry50L[0, 2, 2, 1] = 58;  // ac=1, facing=rear, range=L, hull
         //----------------------------
         entry50L[1, 0, 0, 0] = 17;  // ac=2, facing=front, range=C, turret
         entry50L[1, 0, 1, 0] = 08;  // ac=2, facing=front, range=M, turret
         entry50L[1, 0, 2, 0] = 03;  // ac=2, facing=front, range=L, turret
         entry50L[1, 0, 0, 1] = 03;  // ac=2, facing=front, range=C, hull
         entry50L[1, 0, 1, 1] = 03;  // ac=2, facing=front, range=M, hull
         entry50L[1, 0, 2, 1] = 03;  // ac=2, facing=front, range=L, hull
         //----------------------------
         entry50L[1, 1, 0, 0] = 42;  // ac=2, facing=side, range=C, turret
         entry50L[1, 1, 1, 0] = 28;  // ac=2, facing=side, range=M, turret
         entry50L[1, 1, 2, 0] = 17;  // ac=2, facing=side, range=L, turret
         entry50L[1, 1, 0, 1] = 72;  // ac=2, facing=side, range=C, hull
         entry50L[1, 1, 1, 1] = 58;  // ac=2, facing=side, range=M, hull
         entry50L[1, 1, 2, 1] = 42;  // ac=2, facing=side, range=L, hull
         //----------------------------
         entry50L[1, 2, 0, 0] = 58;  // ac=2, facing=rear, range=C, turret
         entry50L[1, 2, 1, 0] = 42;  // ac=2, facing=rear, range=M, turret
         entry50L[1, 2, 2, 0] = 28;  // ac=2, facing=rear, range=L, turret
         entry50L[1, 2, 0, 1] = 83;  // ac=2, facing=rear, range=C, hull
         entry50L[1, 2, 1, 1] = 72;  // ac=2, facing=rear, range=M, hull
         entry50L[1, 2, 2, 1] = 58;  // ac=2, facing=rear, range=L, hull
         //----------------------------
         entry50L[2, 0, 0, 0] = 02;  // ac=3, facing=front, range=C, turret
         entry50L[2, 0, 1, 0] = 01;  // ac=3, facing=front, range=M, turret
         entry50L[2, 0, 2, 0] = 01;  // ac=3, facing=front, range=L, turret
         entry50L[2, 0, 0, 1] = 02;  // ac=3, facing=front, range=C, hull
         entry50L[2, 0, 1, 1] = 01;  // ac=3, facing=front, range=M, hull
         entry50L[2, 0, 2, 1] = 01;  // ac=3, facing=front, range=L, hull
         //----------------------------
         entry50L[2, 1, 0, 0] = 03;  // ac=3, facing=side, range=C, turret
         entry50L[2, 1, 1, 0] = 03;  // ac=3, facing=side, range=M, turret
         entry50L[2, 1, 2, 0] = 03;  // ac=3, facing=side, range=L, turret
         entry50L[2, 1, 0, 1] = 17;  // ac=3, facing=side, range=C, hull
         entry50L[2, 1, 1, 1] = 08;  // ac=3, facing=side, range=M, hull
         entry50L[2, 1, 2, 1] = 03;  // ac=3, facing=side, range=L, hull
         //----------------------------
         entry50L[2, 2, 0, 0] = 03;  // ac=3, facing=rear, range=C, turret
         entry50L[2, 2, 1, 0] = 03;  // ac=3, facing=rear, range=M, turret
         entry50L[2, 2, 2, 0] = 03;  // ac=3, facing=rear, range=L, turret
         entry50L[2, 2, 0, 1] = 28;  // ac=3, facing=rear, range=C, hull
         entry50L[2, 2, 1, 1] = 17;  // ac=3, facing=rear, range=M, hull
         entry50L[2, 2, 2, 1] = 08;  // ac=3, facing=rear, range=L, hull
         //======================================================================
         int[,,,] entry75L = new int[3, 3, 3, 2];
         theApToKills["75L"] = entry75L;
         entry75L[0, 0, 0, 0] = 72;  // ac=1, facing=front, range=C, turret
         entry75L[0, 0, 1, 0] = 58;  // ac=1, facing=front, range=M, turret
         entry75L[0, 0, 2, 0] = 42;  // ac=1, facing=front, range=L, turret
         entry75L[0, 0, 0, 1] = 72;  // ac=1, facing=front, range=C, hull
         entry75L[0, 0, 1, 1] = 58;  // ac=1, facing=front, range=M, hull
         entry75L[0, 0, 2, 1] = 42;  // ac=1, facing=front, range=L, hull
         //----------------------------
         entry75L[0, 1, 0, 0] = 92;  // ac=1, facing=side, range=C, turret
         entry75L[0, 1, 1, 0] = 83;  // ac=1, facing=side, range=M, turret
         entry75L[0, 1, 2, 0] = 72;  // ac=1, facing=side, range=L, turret
         entry75L[0, 1, 0, 1] = 95;  // ac=1, facing=side, range=C, hull
         entry75L[0, 1, 1, 1] = 95;  // ac=1, facing=side, range=M, hull
         entry75L[0, 1, 2, 1] = 92;  // ac=1, facing=side, range=L, hull
         //----------------------------
         entry75L[0, 2, 0, 0] = 95;  // ac=1, facing=rear, range=C, turret
         entry75L[0, 2, 1, 0] = 92;  // ac=1, facing=rear, range=M, turret
         entry75L[0, 2, 2, 0] = 83;  // ac=1, facing=rear, range=L, turret
         entry75L[0, 2, 0, 1] = 95;  // ac=1, facing=rear, range=C, hull
         entry75L[0, 2, 1, 1] = 95;  // ac=1, facing=rear, range=M, hull
         entry75L[0, 2, 2, 1] = 95;  // ac=1, facing=rear, range=L, hull
         //----------------------------
         entry75L[1, 0, 0, 0] = 72;  // ac=2, facing=front, range=C, turret
         entry75L[1, 0, 1, 0] = 58;  // ac=2, facing=front, range=M, turret
         entry75L[1, 0, 2, 0] = 42;  // ac=2, facing=front, range=L, turret
         entry75L[1, 0, 0, 1] = 28;  // ac=2, facing=front, range=C, hull
         entry75L[1, 0, 1, 1] = 17;  // ac=2, facing=front, range=M, hull
         entry75L[1, 0, 2, 1] = 08;  // ac=2, facing=front, range=L, hull
         //----------------------------
         entry75L[1, 1, 0, 0] = 92;  // ac=2, facing=side, range=C, turret
         entry75L[1, 1, 1, 0] = 83;  // ac=2, facing=side, range=M, turret
         entry75L[1, 1, 2, 0] = 72;  // ac=2, facing=side, range=L, turret
         entry75L[1, 1, 0, 1] = 95;  // ac=2, facing=side, range=C, hull
         entry75L[1, 1, 1, 1] = 95;  // ac=2, facing=side, range=M, hull
         entry75L[1, 1, 2, 1] = 92;  // ac=2, facing=side, range=L, hull
         //----------------------------
         entry75L[1, 2, 0, 0] = 92;  // ac=2, facing=rear, range=C, turret
         entry75L[1, 2, 1, 0] = 92;  // ac=2, facing=rear, range=M, turret
         entry75L[1, 2, 2, 0] = 83;  // ac=2, facing=rear, range=L, turret
         entry75L[1, 2, 0, 1] = 95;  // ac=2, facing=rear, range=C, hull
         entry75L[1, 2, 1, 1] = 95;  // ac=2, facing=rear, range=M, hull
         entry75L[1, 2, 2, 1] = 95;  // ac=2, facing=rear, range=L, hull
         //----------------------------
         entry75L[2, 0, 0, 0] = 03;  // ac=3, facing=front, range=C, turret
         entry75L[2, 0, 1, 0] = 03;  // ac=3, facing=front, range=M, turret
         entry75L[2, 0, 2, 0] = 03;  // ac=3, facing=front, range=L, turret
         entry75L[2, 0, 0, 1] = 03;  // ac=3, facing=front, range=C, hull
         entry75L[2, 0, 1, 1] = 03;  // ac=3, facing=front, range=M, hull
         entry75L[2, 0, 2, 1] = 03;  // ac=3, facing=front, range=L, hull
         //----------------------------
         entry75L[2, 1, 0, 0] = 28;  // ac=3, facing=side, range=C, turret
         entry75L[2, 1, 1, 0] = 17;  // ac=3, facing=side, range=M, turret
         entry75L[2, 1, 2, 0] = 08;  // ac=3, facing=side, range=L, turret
         entry75L[2, 1, 0, 1] = 72;  // ac=3, facing=side, range=C, hull
         entry75L[2, 1, 1, 1] = 58;  // ac=3, facing=side, range=M, hull
         entry75L[2, 1, 2, 1] = 42;  // ac=3, facing=side, range=L, hull
         //----------------------------
         entry75L[2, 2, 0, 0] = 42;  // ac=3, facing=rear, range=C, turret
         entry75L[2, 2, 1, 0] = 28;  // ac=3, facing=rear, range=M, turret
         entry75L[2, 2, 2, 0] = 17;  // ac=3, facing=rear, range=L, turret
         entry75L[2, 2, 0, 1] = 83;  // ac=3, facing=rear, range=C, hull
         entry75L[2, 2, 1, 1] = 72;  // ac=3, facing=rear, range=M, hull
         entry75L[2, 2, 2, 1] = 58;  // ac=3, facing=rear, range=L, hull
         //======================================================================
         int[,,,] entry75LL = new int[3, 3, 3, 2];
         theApToKills["75LL"] = entry75LL;
         entry75LL[0, 0, 0, 0] = 95;  // ac=1, facing=front, range=C, turret
         entry75LL[0, 0, 1, 0] = 95;  // ac=1, facing=front, range=M, turret
         entry75LL[0, 0, 2, 0] = 95;  // ac=1, facing=front, range=L, turret
         entry75LL[0, 0, 0, 1] = 95;  // ac=1, facing=front, range=C, hull
         entry75LL[0, 0, 1, 1] = 95;  // ac=1, facing=front, range=M, hull
         entry75LL[0, 0, 2, 1] = 95;  // ac=1, facing=front, range=L, hull
         //----------------------------
         entry75LL[0, 1, 0, 0] = 95;  // ac=1, facing=side, range=C, turret
         entry75LL[0, 1, 1, 0] = 95;  // ac=1, facing=side, range=M, turret
         entry75LL[0, 1, 2, 0] = 95;  // ac=1, facing=side, range=L, turret
         entry75LL[0, 1, 0, 1] = 95;  // ac=1, facing=side, range=C, hull
         entry75LL[0, 1, 1, 1] = 95;  // ac=1, facing=side, range=M, hull
         entry75LL[0, 1, 2, 1] = 95;  // ac=1, facing=side, range=L, hull
         //----------------------------
         entry75LL[0, 2, 0, 0] = 95;  // ac=1, facing=rear, range=C, turret
         entry75LL[0, 2, 1, 0] = 95;  // ac=1, facing=rear, range=M, turret
         entry75LL[0, 2, 2, 0] = 95;  // ac=1, facing=rear, range=L, turret
         entry75LL[0, 2, 0, 1] = 95;  // ac=1, facing=rear, range=C, hull
         entry75LL[0, 2, 1, 1] = 95;  // ac=1, facing=rear, range=M, hull
         entry75LL[0, 2, 2, 1] = 95;  // ac=1, facing=rear, range=L, hull
         //----------------------------
         entry75LL[1, 0, 0, 0] = 95;  // ac=2, facing=front, range=C, turret
         entry75LL[1, 0, 1, 0] = 95;  // ac=2, facing=front, range=M, turret
         entry75LL[1, 0, 2, 0] = 95;  // ac=2, facing=front, range=L, turret
         entry75LL[1, 0, 0, 1] = 95;  // ac=2, facing=front, range=C, hull
         entry75LL[1, 0, 1, 1] = 93;  // ac=2, facing=front, range=M, hull
         entry75LL[1, 0, 2, 1] = 83;  // ac=2, facing=front, range=L, hull
         //----------------------------
         entry75LL[1, 1, 0, 0] = 95;  // ac=2, facing=side, range=C, turret
         entry75LL[1, 1, 1, 0] = 95;  // ac=2, facing=side, range=M, turret
         entry75LL[1, 1, 2, 0] = 95;  // ac=2, facing=side, range=L, turret
         entry75LL[1, 1, 0, 1] = 95;  // ac=2, facing=side, range=C, hull
         entry75LL[1, 1, 1, 1] = 95;  // ac=2, facing=side, range=M, hull
         entry75LL[1, 1, 2, 1] = 95;  // ac=2, facing=side, range=L, hull
         //----------------------------
         entry75LL[1, 2, 0, 0] = 95;  // ac=2, facing=rear, range=C, turret
         entry75LL[1, 2, 1, 0] = 95;  // ac=2, facing=rear, range=M, turret
         entry75LL[1, 2, 2, 0] = 95;  // ac=2, facing=rear, range=L, turret
         entry75LL[1, 2, 0, 1] = 95;  // ac=2, facing=rear, range=C, hull
         entry75LL[1, 2, 1, 1] = 95;  // ac=2, facing=rear, range=M, hull
         entry75LL[1, 2, 2, 1] = 95;  // ac=2, facing=rear, range=L, hull
         //----------------------------
         entry75LL[2, 0, 0, 0] = 17;  // ac=3, facing=front, range=C, turret
         entry75LL[2, 0, 1, 0] = 08;  // ac=3, facing=front, range=M, turret
         entry75LL[2, 0, 2, 0] = 03;  // ac=3, facing=front, range=L, turret
         entry75LL[2, 0, 0, 1] = 17;  // ac=3, facing=front, range=C, hull
         entry75LL[2, 0, 1, 1] = 08;  // ac=3, facing=front, range=M, hull
         entry75LL[2, 0, 2, 1] = 03;  // ac=3, facing=front, range=L, hull
         //----------------------------
         entry75LL[2, 1, 0, 0] = 95;  // ac=3, facing=side, range=C, turret
         entry75LL[2, 1, 1, 0] = 92;  // ac=3, facing=side, range=M, turret
         entry75LL[2, 1, 2, 0] = 83;  // ac=3, facing=side, range=L, turret
         entry75LL[2, 1, 0, 1] = 95;  // ac=3, facing=side, range=C, hull
         entry75LL[2, 1, 1, 1] = 95;  // ac=3, facing=side, range=M, hull
         entry75LL[2, 1, 2, 1] = 95;  // ac=3, facing=side, range=L, hull
         //----------------------------
         entry75LL[2, 2, 0, 0] = 95;  // ac=3, facing=rear, range=C, turret
         entry75LL[2, 2, 1, 0] = 95;  // ac=3, facing=rear, range=M, turret
         entry75LL[2, 2, 2, 0] = 92;  // ac=3, facing=rear, range=L, turret
         entry75LL[2, 2, 0, 1] = 95;  // ac=3, facing=rear, range=C, hull
         entry75LL[2, 2, 1, 1] = 95;  // ac=3, facing=rear, range=M, hull
         entry75LL[2, 2, 2, 1] = 95;  // ac=3, facing=rear, range=L, hull
         //======================================================================
         int[,,,] entry88L = new int[3, 3, 3, 2];
         theApToKills["88L"] = entry88L;
         entry88L[0, 0, 0, 0] = 95;  // ac=1, facing=front, range=C, turret
         entry88L[0, 0, 1, 0] = 92;  // ac=1, facing=front, range=M, turret
         entry88L[0, 0, 2, 0] = 83;  // ac=1, facing=front, range=L, turret
         entry88L[0, 0, 0, 1] = 95;  // ac=1, facing=front, range=C, hull
         entry88L[0, 0, 1, 1] = 92;  // ac=1, facing=front, range=M, hull
         entry88L[0, 0, 2, 1] = 83;  // ac=1, facing=front, range=L, hull
         //----------------------------
         entry88L[0, 1, 0, 0] = 95;  // ac=1, facing=side, range=C, turret
         entry88L[0, 1, 1, 0] = 95;  // ac=1, facing=side, range=M, turret
         entry88L[0, 1, 2, 0] = 95;  // ac=1, facing=side, range=L, turret
         entry88L[0, 1, 0, 1] = 95;  // ac=1, facing=side, range=C, hull
         entry88L[0, 1, 1, 1] = 95;  // ac=1, facing=side, range=M, hull
         entry88L[0, 1, 2, 1] = 95;  // ac=1, facing=side, range=L, hull
         //----------------------------
         entry88L[0, 2, 0, 0] = 95;  // ac=1, facing=rear, range=C, turret
         entry88L[0, 2, 1, 0] = 95;  // ac=1, facing=rear, range=M, turret
         entry88L[0, 2, 2, 0] = 95;  // ac=1, facing=rear, range=L, turret
         entry88L[0, 2, 0, 1] = 95;  // ac=1, facing=rear, range=C, hull
         entry88L[0, 2, 1, 1] = 95;  // ac=1, facing=rear, range=M, hull
         entry88L[0, 2, 2, 1] = 95;  // ac=1, facing=rear, range=L, hull
         //----------------------------
         entry88L[1, 0, 0, 0] = 95;  // ac=2, facing=front, range=C, turret
         entry88L[1, 0, 1, 0] = 92;  // ac=2, facing=front, range=M, turret
         entry88L[1, 0, 2, 0] = 83;  // ac=2, facing=front, range=L, turret
         entry88L[1, 0, 0, 1] = 72;  // ac=2, facing=front, range=C, hull
         entry88L[1, 0, 1, 1] = 58;  // ac=2, facing=front, range=M, hull
         entry88L[1, 0, 2, 1] = 12;  // ac=2, facing=front, range=L, hull
         //----------------------------
         entry88L[1, 1, 0, 0] = 95;  // ac=2, facing=side, range=C, turret
         entry88L[1, 1, 1, 0] = 95;  // ac=2, facing=side, range=M, turret
         entry88L[1, 1, 2, 0] = 95;  // ac=2, facing=side, range=L, turret
         entry88L[1, 1, 0, 1] = 95;  // ac=2, facing=side, range=C, hull
         entry88L[1, 1, 1, 1] = 95;  // ac=2, facing=side, range=M, hull
         entry88L[1, 1, 2, 1] = 95;  // ac=2, facing=side, range=L, hull
         //----------------------------
         entry88L[1, 2, 0, 0] = 95;  // ac=2, facing=rear, range=C, turret
         entry88L[1, 2, 1, 0] = 95;  // ac=2, facing=rear, range=M, turret
         entry88L[1, 2, 2, 0] = 95;  // ac=2, facing=rear, range=L, turret
         entry88L[1, 2, 0, 1] = 95;  // ac=2, facing=rear, range=C, hull
         entry88L[1, 2, 1, 1] = 95;  // ac=2, facing=rear, range=M, hull
         entry88L[1, 2, 2, 1] = 95;  // ac=2, facing=rear, range=L, hull
         //----------------------------
         entry88L[2, 0, 0, 0] = 03;  // ac=3, facing=front, range=C, turret
         entry88L[2, 0, 1, 0] = 03;  // ac=3, facing=front, range=M, turret
         entry88L[2, 0, 2, 0] = 03;  // ac=3, facing=front, range=L, turret
         entry88L[2, 0, 0, 1] = 03;  // ac=3, facing=front, range=C, hull
         entry88L[2, 0, 1, 1] = 03;  // ac=3, facing=front, range=M, hull
         entry88L[2, 0, 2, 1] = 03;  // ac=3, facing=front, range=L, hull
         //----------------------------
         entry88L[2, 1, 0, 0] = 72;  // ac=3, facing=side, range=C, turret
         entry88L[2, 1, 1, 0] = 58;  // ac=3, facing=side, range=M, turret
         entry88L[2, 1, 2, 0] = 06;  // ac=3, facing=side, range=L, turret
         entry88L[2, 1, 0, 1] = 95;  // ac=3, facing=side, range=C, hull
         entry88L[2, 1, 1, 1] = 92;  // ac=3, facing=side, range=M, hull
         entry88L[2, 1, 2, 1] = 83;  // ac=3, facing=side, range=L, hull
         //----------------------------
         entry88L[2, 2, 0, 0] = 83;  // ac=3, facing=rear, range=C, turret
         entry88L[2, 2, 1, 0] = 72;  // ac=3, facing=rear, range=M, turret
         entry88L[2, 2, 2, 0] = 58;  // ac=3, facing=rear, range=L, turret
         entry88L[2, 2, 0, 1] = 95;  // ac=3, facing=rear, range=C, hull
         entry88L[2, 2, 1, 1] = 95;  // ac=3, facing=rear, range=M, hull
         entry88L[2, 2, 2, 1] = 92;  // ac=3, facing=rear, range=L, hull
         //======================================================================
         int[,,,] entry88LL = new int[3, 3, 3, 2];
         theApToKills["88LL"] = entry88LL;
         entry88LL[0, 0, 0, 0] = 95;  // ac=1, facing=front, range=C, turret
         entry88LL[0, 0, 1, 0] = 95;  // ac=1, facing=front, range=M, turret
         entry88LL[0, 0, 2, 0] = 95;  // ac=1, facing=front, range=L, turret
         entry88LL[0, 0, 0, 1] = 95;  // ac=1, facing=front, range=C, hull
         entry88LL[0, 0, 1, 1] = 95;  // ac=1, facing=front, range=M, hull
         entry88LL[0, 0, 2, 1] = 95;  // ac=1, facing=front, range=L, hull
         //----------------------------
         entry88LL[0, 1, 0, 0] = 95;  // ac=1, facing=side, range=C, turret
         entry88LL[0, 1, 1, 0] = 95;  // ac=1, facing=side, range=M, turret
         entry88LL[0, 1, 2, 0] = 95;  // ac=1, facing=side, range=L, turret
         entry88LL[0, 1, 0, 1] = 95;  // ac=1, facing=side, range=C, hull
         entry88LL[0, 1, 1, 1] = 95;  // ac=1, facing=side, range=M, hull
         entry88LL[0, 1, 2, 1] = 95;  // ac=1, facing=side, range=L, hull
         //----------------------------
         entry88LL[0, 2, 0, 0] = 95;  // ac=1, facing=rear, range=C, turret
         entry88LL[0, 2, 1, 0] = 95;  // ac=1, facing=rear, range=M, turret
         entry88LL[0, 2, 2, 0] = 95;  // ac=1, facing=rear, range=L, turret
         entry88LL[0, 2, 0, 1] = 95;  // ac=1, facing=rear, range=C, hull
         entry88LL[0, 2, 1, 1] = 95;  // ac=1, facing=rear, range=M, hull
         entry88LL[0, 2, 2, 1] = 95;  // ac=1, facing=rear, range=L, hull
         //----------------------------
         entry88LL[1, 0, 0, 0] = 95;  // ac=2, facing=front, range=C, turret
         entry88LL[1, 0, 1, 0] = 95;  // ac=2, facing=front, range=M, turret
         entry88LL[1, 0, 2, 0] = 95;  // ac=2, facing=front, range=L, turret
         entry88LL[1, 0, 0, 1] = 95;  // ac=2, facing=front, range=C, hull
         entry88LL[1, 0, 1, 1] = 95;  // ac=2, facing=front, range=M, hull
         entry88LL[1, 0, 2, 1] = 95;  // ac=2, facing=front, range=L, hull
         //----------------------------
         entry88LL[1, 1, 0, 0] = 95;  // ac=2, facing=side, range=C, turret
         entry88LL[1, 1, 1, 0] = 95;  // ac=2, facing=side, range=M, turret
         entry88LL[1, 1, 2, 0] = 95;  // ac=2, facing=side, range=L, turret
         entry88LL[1, 1, 0, 1] = 95;  // ac=2, facing=side, range=C, hull
         entry88LL[1, 1, 1, 1] = 95;  // ac=2, facing=side, range=M, hull
         entry88LL[1, 1, 2, 1] = 95;  // ac=2, facing=side, range=L, hull
         //----------------------------
         entry88LL[1, 2, 0, 0] = 95;  // ac=2, facing=rear, range=C, turret
         entry88LL[1, 2, 1, 0] = 95;  // ac=2, facing=rear, range=M, turret
         entry88LL[1, 2, 2, 0] = 95;  // ac=2, facing=rear, range=L, turret
         entry88LL[1, 2, 0, 1] = 95;  // ac=2, facing=rear, range=C, hull
         entry88LL[1, 2, 1, 1] = 95;  // ac=2, facing=rear, range=M, hull
         entry88LL[1, 2, 2, 1] = 95;  // ac=2, facing=rear, range=L, hull
         //----------------------------
         entry88LL[2, 0, 0, 0] = 72;  // ac=3, facing=front, range=C, turret
         entry88LL[2, 0, 1, 0] = 58;  // ac=3, facing=front, range=M, turret
         entry88LL[2, 0, 2, 0] = 42;  // ac=3, facing=front, range=L, turret
         entry88LL[2, 0, 0, 1] = 72;  // ac=3, facing=front, range=C, hull
         entry88LL[2, 0, 1, 1] = 58;  // ac=3, facing=front, range=M, hull
         entry88LL[2, 0, 2, 1] = 42;  // ac=3, facing=front, range=L, hull
         //----------------------------
         entry88LL[2, 1, 0, 0] = 95;  // ac=3, facing=side, range=C, turret
         entry88LL[2, 1, 1, 0] = 95;  // ac=3, facing=side, range=M, turret
         entry88LL[2, 1, 2, 0] = 95;  // ac=3, facing=side, range=L, turret
         entry88LL[2, 1, 0, 1] = 95;  // ac=3, facing=side, range=C, hull
         entry88LL[2, 1, 1, 1] = 95;  // ac=3, facing=side, range=M, hull
         entry88LL[2, 1, 2, 1] = 95;  // ac=3, facing=side, range=L, hull
         //----------------------------
         entry88LL[2, 2, 0, 0] = 95;  // ac=3, facing=rear, range=C, turret
         entry88LL[2, 2, 1, 0] = 95;  // ac=3, facing=rear, range=M, turret
         entry88LL[2, 2, 2, 0] = 95;  // ac=3, facing=rear, range=L, turret
         entry88LL[2, 2, 0, 1] = 95;  // ac=3, facing=rear, range=C, hull
         entry88LL[2, 2, 1, 1] = 95;  // ac=3, facing=rear, range=M, hull
         entry88LL[2, 2, 2, 1] = 95;  // ac=3, facing=rear, range=L, hull
      }
   }
}
