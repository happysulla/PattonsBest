using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using Windows.Graphics.Printing3D;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;

namespace Pattons_Best
{
   public class TableMgr
   {
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
         else if (day < 58)
         {
            sb.Append("09/");
            int dayOfMonth = day-36;
            if ( 37 < day )
               dayOfMonth = day - 27;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1944");
         }
         else if (day < 70)
         {
            sb.Append("10/");
            int dayOfMonth = day - 57;
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1944");
         }
         else if (day < 92)
         {
            sb.Append("11/");
            int dayOfMonth = day - 62;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1944");
         }
         else if (day < 110)
         {
            sb.Append("12/");
            int dayOfMonth = day - 91;
            if (97 < day)
               dayOfMonth = day - 78;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1944");
         }
         else if (day < 137)
         {
            sb.Append("01/");
            int dayOfMonth = day - 109;
            if (112 < day)
               dayOfMonth = day - 103;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1945");
         }
         else if (day < 147)
         {
            sb.Append("02/");
            int dayOfMonth = day - 136;
            if (139 < day)
               dayOfMonth = day - 118;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1945");
         }
         else if (day < 174)
         {
            sb.Append("03/");
            int dayOfMonth = day - 146;
            if (149 < day)
               dayOfMonth = day - 145;
            if (157 < day)
               dayOfMonth = day - 136;
            if (dayOfMonth < 10)
               sb.Append("0");
            sb.Append(dayOfMonth.ToString());
            sb.Append("/1945");
         }
         else if (day < 193)
         {
            sb.Append("04/");
            int dayOfMonth = day - 173;
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
               else
                  return "M";
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
      public static string GetEnemyFacing(string enemyUnit, int dieRoll)
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
               Logger.Log(LogEnum.LE_ERROR, "GetEnemyFacing(): Reached Default enemy=" + enemyUnit);
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
                        return "Moving";
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
                        return "Moving";
                     else
                        return "Open";
                  case "C":
                     if (dieRoll < 3)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Building";
                     else if (dieRoll == 10)
                        return "Moving";
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
                        return "Moving";
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
                        return "Moving";
                  case "B":
                     if (dieRoll < 3)
                        return "Hull Down";
                     else if (dieRoll < 5)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Open";
                     else
                        return "Moving";
                  case "C":
                     if (dieRoll < 6)
                        return "Hull Down";
                     else if (dieRoll < 7)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Open";
                     else
                        return "Moving";
                  case "D":
                     if (dieRoll < 3)
                        return "Hull Down";
                     else if (dieRoll < 8)
                        return "Woods";
                     else if (dieRoll < 9)
                        return "Open";
                     else
                        return "Moving";
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "GetEnemyTerrain(): Reached Default areaType=" + areaType);
                     return "ERROR";
               }
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetEnemyTerrain(): Reached Default enemy=" + enemyUnit);
               return "ERROR";
         }
      }
      public static int GetFriendlyActionModifier(IGameInstance gi, IMapItem mi, int numUsControlledSector, bool isAdvancingFire, bool isArtilleryFire, bool isAirStrike, bool isFlankingFire)
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
         if (true == isFlankingFire)
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
      public static string SetFriendlyActionResult(IGameInstance gi, IMapItem mi, int dieRoll, int numUsControlledSector, bool isAdvancingFire, bool isArtilleryFire, bool isAirStrike, bool isFlankingFire)
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
         int modifier = GetFriendlyActionModifier(gi, mi, numUsControlledSector, isAdvancingFire, isArtilleryFire, isAirStrike, isFlankingFire);
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
                     else if (true == gi.IsShermanFiring)
                        return "Fire-Your Tank";
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
                     return "Move-F";
                  if (dieRoll < 51)
                     return "Move-L";
                  if (dieRoll < 61)
                     return "Move-R";
                  if (dieRoll < 91)
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
                     return "Move-F";
                  if (dieRoll < 31)
                     return "Move-L";
                  if (dieRoll < 41)
                     return "Move-R";
                  if (dieRoll < 61)
                     return "Move-B";
                  if (dieRoll < 66)
                     return "Fire-Infantry";
                  if (dieRoll < 81)
                  {
                     if (true == gi.IsShermanFiringAtFront)
                     {
                        if (true == isDoNothingInsteadOfFiring)
                           return "Do Nothing";
                        return "Fire-Your Tank";
                     }
                     else if (true == gi.IsShermanFiring)
                     {
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
                     return "Fire-Your Tank";
                  }
                  if (true == isDoNothingInsteadOfFiring)
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
                     else if (true == gi.IsShermanFiring)
                        return "Fire-Your Tank";
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
                     return "Move-F";
                  if (dieRoll < 21)
                     return "Move-L";
                  if (dieRoll < 26)
                     return "Move-R";
                  if (dieRoll < 36)
                     return "Move-B";
                  if (dieRoll < 41)
                     return "Fire-Infantry";
                  if (dieRoll < 86)
                  {
                     if (true == gi.IsShermanFiringAtFront)
                     {
                        if (true == isDoNothingInsteadOfFiring)
                           return "Do Nothing";
                        return "Fire-Your Tank";
                     }
                     else if (true == gi.IsShermanFiring)
                     {
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
                     return "Fire-Your Tank";
                  }
                  if (true == isDoNothingInsteadOfFiring)
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
                     return "Move-F";
                  if (dieRoll < 51)
                     return "Move-L";
                  if (dieRoll < 61)
                     return "Move-R";
                  if (dieRoll < 71)
                     return "Move-B";
                  if (dieRoll < 76)
                     return "Fire-Infantry";
                  if (dieRoll < 96)
                  {
                     if (true == gi.IsShermanFiringAtFront)
                     {
                        if (true == isDoNothingInsteadOfFiring)
                           return "Do Nothing";
                        return "Fire-Your Tank";
                     }
                     else if (true == gi.IsShermanFiring)
                     {
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
      //-------------------------------------------
      public static double GetToKillNumberInfantry(IGameInstance gi, IMapItem mi, char sector, char range)
      {
         double toKillNum = -1000.0;
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberInfantry(): unknown enemyUnit=" + mi.Name);
            return toKillNum;
         }
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberInfantry(): lastReport=null");
            return toKillNum;
         }
         //----------------------------------------------------
         if ( (EnumScenario.Advance == lastReport.Scenario) || (EnumScenario.Counterattack == lastReport.Scenario) )
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
                  Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberInfantry(): Advance - reached default with enemyUnit=" + enemyUnit);
                  return toKillNum;
            }
         }
         else if (EnumScenario.Battle == lastReport.Scenario)
         {
            switch (enemyUnit)
            {
               case "LW":
                  if ('C' == range)
                     toKillNum = 30;
                  else if ('M' == range)
                     toKillNum = 20;
                  else if ('L' == range)
                     toKillNum = 03;
                  break;
               case "MG":
                  if ('C' == range)
                     toKillNum = 55;
                  else if ('M' == range)
                     toKillNum = 30;
                  else if ('L' == range)
                     toKillNum = 03;
                  break;
               case "PzIV":
               case "PzV":
               case "PzVI":
               case "STuGIIIg":
               case "SPG":
               case "MARDER":
               case "JdgPz":
                  if ('C' == range)
                     toKillNum = 65;
                  else if ('M' == range)
                     toKillNum = 40;
                  else if ('L' == range)
                     toKillNum = 10;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberInfantry(): Battle - reached default with enemyUnit=" + enemyUnit);
                  return toKillNum;
            }
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberInfantry():  reached default with scearno=" + lastReport.Scenario);
            return toKillNum;
         }
         //------------------------------------
         int numSmokeMarkers = Territory.GetSmokeCount(gi, sector, range);
         if (numSmokeMarkers < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberTank(): GetSmokeCount() returned error");
            return toKillNum;
         }
         if ( 0 < numSmokeMarkers)
            toKillNum = toKillNum * numSmokeMarkers * 0.5;
         //------------------------------------
         if ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")))
            toKillNum = toKillNum * 0.5;
         //------------------------------------
         return toKillNum;
      }
      public static double GetToKillNumberTank(IGameInstance gi, IMapItem mi, char sector, char range)
      {
         double toKillNum = -1000.0;
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberTank(): unknown enemyUnit=" + mi.Name);
            return toKillNum;
         }
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberTank(): lastReport=null");
            return toKillNum;
         }
         //----------------------------------------------------
         if ( (EnumScenario.Advance == lastReport.Scenario) || (EnumScenario.Counterattack == lastReport.Scenario) )
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
                  Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberTank(): Advance - reached default with enemyUnit=" + enemyUnit);
                  return toKillNum;
            }
         }
         else if (EnumScenario.Battle == lastReport.Scenario)
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
                  Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberTank(): Advance - reached default with enemyUnit=" + enemyUnit);
                  return toKillNum;
            }
         }
         else 
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberTank(): reached default scenario=" + lastReport.Scenario);
            return toKillNum;
         }
         //------------------------------------
         int numSmokeMarkers = Territory.GetSmokeCount(gi, sector, range);
         if (numSmokeMarkers < 0 )
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberTank(): GetSmokeCount() returned error");
            return toKillNum;
         }
         if (0 < numSmokeMarkers)
            toKillNum = toKillNum * numSmokeMarkers * 0.5;
         //------------------------------------
         if ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")))
            toKillNum = toKillNum * 0.5;
         //------------------------------------
         return toKillNum;
      }
      public static string GetCollateralDamage(IGameInstance gi, int dieRoll)
      {
         if (dieRoll < 51)
            return "No Effect";
         if (dieRoll < 53)
         {
            gi.BrokenPeriscopes["Driver"] = true;
            return "Driver Periscope Broken";
         }
         if (dieRoll < 55)
         {
            gi.BrokenPeriscopes["Assistant"] = true;
            return "Assistant Periscope Broken";
         }
         if (dieRoll < 58)
         {
            gi.BrokenPeriscopes["Gunner"] = true;
            return "Gunner Periscope Broken";
         }
         if (dieRoll < 61)
         {
            gi.BrokenPeriscopes["Loader"] = true;
            return "Loader Periscope Broken";
         }
         if (dieRoll < 64)
         {
            gi.BrokenPeriscopes["Commander"] = true;
            return "Commander Periscope Broken";
         }
         if (dieRoll < 66)
         {
            gi.IsBrokenGunsight = true;
            return "Gunsight Broken";
         }
         if (dieRoll < 71)
         {
            gi.IsBrokenMgAntiAircraft = true;
            return "AA MG Broken";
         }
         if (dieRoll < 76)
         {
            IMapItem? hatch = gi.Hatches.Find("DriverOpenHatch");
            if (null != hatch)
               return "Driver Wounds";
            else
               return "No Effect";
         }
         if (dieRoll < 81)
         {
            IMapItem? hatch = gi.Hatches.Find("AsssistantOpenHatch");
            if (null != hatch)
               return "Asssistant Wounds";
            else
               return "No Effect";
         }
         if (dieRoll < 91)
         {
            IMapItem? hatch = gi.Hatches.Find("LoaderOpenHatch");
            if (null != hatch)
               return "Loader Wounds";
            else
               return "No Effect";
         }
         if (dieRoll < 101)
         {
            IMapItem? hatch = gi.Hatches.Find("CommanderOpenHatch");
            if (null != hatch)
               return "Commander Wounds";
            else
               return "No Effect";
         }
         return "ERROR";
      }
      //-------------------------------------------
      public static int GetToHitNumberModifierForYourTank(IGameInstance gi, IMapItem mi, char range)
      {
         int toHitModifierNum = 0;
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToHitNumberYourTank(): unknown enemyUnit=" + mi.Name);
            return -1000;
         }
         if( false == gi.FirstShots.ContainsKey(mi.Name))
         {
            toHitModifierNum += 10; // add 10 if first shot
            Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetToHitNumberYourTank(): eu=" + mi.Name + " +10 mod=" + toHitModifierNum.ToString() + " firstShot");
         }
         //-----------------------------------------------
         if (true == gi.AcquiredShots.ContainsKey(mi.Name))
         {
            if (1 < gi.AcquiredShots[mi.Name])
            {
               if ('C' == range)
               {
                  toHitModifierNum -= 10;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetToHitNumberYourTank(): eu=" + mi.Name + " -10 mod=" + toHitModifierNum.ToString() + " acquire-2 close");
               }
               else if ('M' == range)
               {
                  toHitModifierNum -= 10;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetToHitNumberYourTank(): eu=" + mi.Name + " -20 mod=" + toHitModifierNum.ToString() + " acquire-2 medium");
               }
               else if ('L' == range)
               {
                  toHitModifierNum -= 30;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetToHitNumberYourTank(): eu=" + mi.Name + " -30 mod=" + toHitModifierNum.ToString() + " acquire-2 long");
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetToHitNumberYourTank(): 1-unknown range=" + range);
                  return -1000;
               }
            }
            else // acquired 1 marker
            {
               if ('C' == range)
               {
                  toHitModifierNum -= 5;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetToHitNumberYourTank(): eu=" + mi.Name + " -5 mod=" + toHitModifierNum.ToString() + " acquire-1 close");
               }
               else if ('M' == range)
               {
                  toHitModifierNum -= 10;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetToHitNumberYourTank(): eu=" + mi.Name + " -10 mod=" + toHitModifierNum.ToString() + " acquire-1 medium");
               }
               else if ('L' == range)
               {
                  toHitModifierNum -= 15;
                  Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetToHitNumberYourTank(): eu=" + mi.Name + " -15 mod=" + toHitModifierNum.ToString() + " acquire-1 long");
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetToHitNumberYourTank(): 2-unknown range=" + range);
                  return -1000;
               }
            }
         }
         //-----------------------------------------------
         if( true == gi.Sherman.IsMoving )
         {
            toHitModifierNum += 20;
            Logger.Log(LogEnum.LE_SHOW_HIT_YOU_MOD, "GetToHitNumberYourTank(): eu=" + mi.Name + " +20 mod=" + toHitModifierNum.ToString() + " moving");
         }
         return toHitModifierNum;
      }
      public static double GetToHitNumberYourTank(IGameInstance gi, IMapItem mi, char sector, char range)
      {
         double toHitNum = -1000.0;
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberTank(): lastReport=null");
            return toHitNum;
         }
         //----------------------------------------------------
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToHitNumberYourTank(): unknown enemyUnit=" + mi.Name);
            return toHitNum;
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
                  Logger.Log(LogEnum.LE_ERROR, "GetToHitNumberYourTank(): unknown enemyUnit=" + enemyUnit);
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
                  Logger.Log(LogEnum.LE_ERROR, "GetToHitNumberYourTank(): unknown enemyUnit=" + enemyUnit);
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
                  Logger.Log(LogEnum.LE_ERROR, "GetToHitNumberYourTank(): unknown enemyUnit=" + enemyUnit);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetToHitNumberYourTank(): Reached Default enemyUnit=" + enemyUnit);
               return toHitNum;
         }
         //------------------------------------
         int numSmokeMarkers = Territory.GetSmokeCount(gi, sector, range);
         if (numSmokeMarkers < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberTank(): GetSmokeCount() returned error");
            return toHitNum;
         }
         if (0 < numSmokeMarkers)
            toHitNum = toHitNum * numSmokeMarkers * 0.5;
         //------------------------------------
         if ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")))
            toHitNum = toHitNum * 0.5;
         //------------------------------------
         //int modifier = GetToHitNumberModifierForYourTank(gi, mi, range);
         //if (modifier < -100)
         //{
         //   Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberTank() GetToHitNumberModifierForYourTank() returned error ");
         //   return toHitNum;
         //}
         //toHitNum -= modifier;
         //------------------------------------
         return toHitNum;
      }
      public static string GetHitLocationYourTank(IGameInstance gi, int dieRoll)
      {
         if( true == gi.Sherman.IsHullDown )
         {
            if (dieRoll < 6)
               return "Turret";
            else if( dieRoll < 11)
               return "Miss";
            Logger.Log(LogEnum.LE_ERROR, "GetHitLocationYourTank(): 1-dieRoll=" + dieRoll.ToString());
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
            Logger.Log(LogEnum.LE_ERROR, "GetHitLocationYourTank(): 2-dieRoll=" + dieRoll.ToString());
            return "ERROR";
         }
      }
      public static string GetEnemyFireDirection(IGameInstance gi, IMapItem enemyUnit, string hitLocation)
      {
         int count = enemyUnit.TerritoryCurrent.Name.Count();
         if ( 3 != count)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyFireDirection(): 3 != enemyUnit.TerritoryCurrent.Name=" + enemyUnit.TerritoryCurrent.Name);
            return "ERROR";
         }
         char enemySector = enemyUnit.TerritoryCurrent.Name[count - 2];
         double rotation = 0.0;
         switch (enemySector)
         {
            case '6':
               rotation = 0.0;
               break;
            case '9':
               rotation = 60.0;
               break;
            case '1':
               rotation = 120.0;
               break;
            case '2':
               rotation = 180.0;
               break;
            case '3':
               rotation = 240.0;
               break;
            case '4':
               rotation = 300.0;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetEnemyFireDirection(): reached default enemySector=" + enemySector);
               return "ERROR";
         }
         rotation -= gi.Sherman.RotationHull;
         if ( "Turret" == hitLocation )
         {
            rotation -= gi.Sherman.RotationTurret;
            switch (rotation)
            {
               case 0.0:   return "T F";
               case 60.0:  return "T R";
               case 120.0: return "T R";
               case 180.0: return "T B";
               case 240.0: return "T L";
               case 300.0: return "T L";
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemyFireDirection(): reached default rotation=" + rotation.ToString("F1") + " hr=" + gi.Sherman.RotationHull.ToString("F1"));
                  return "ERROR";
            }
         }
         else if ("Hull" == hitLocation)
         {
            rotation -= gi.Sherman.RotationHull;
            switch (rotation)
            {
               case 0.0:   return "H F";
               case 60.0:  return "H FR";
               case 120.0: return "H FB";
               case 180.0: return "H B";
               case 240.0: return "H BL";
               case 300.0: return "H FL";
               default:
                  Logger.Log(LogEnum.LE_ERROR, "GetEnemyFireDirection(): reached default r=" + rotation.ToString("F1") + " hr=" + gi.Sherman.RotationHull.ToString("F1") + " tr=" + gi.Sherman.RotationTurret.ToString("F1"));
                  return "ERROR";
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "GetEnemyFireDirection(): reached default hitLocation=" + hitLocation);
         return "ERROR";
      }
      public static double GetToKillNumberYourTank(IGameInstance gi, IMapItem mi, string facing, char range, string hitLocation)
      {
         double toKillNum = -1000.0;
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberYourTank(): unknown enemyUnit=" + mi.Name);
            return toKillNum;
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
               Logger.Log(LogEnum.LE_ERROR, "GetToHitNumberYourTank(): Reached Default enemyUnit=" + enemyUnit);
               return toKillNum;
         }
         //----------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetToKillNumberYourTank(): lastReport=null");
            return toKillNum;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         int armorclass = 0;
         if ("II" == card.myArmorClass)
            armorclass = 1;
         else if ("III" == card.myArmorClass)
            armorclass = 3;
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
            Logger.Log(LogEnum.LE_ERROR, "GetExplosionModifier(): lastReport=null");
            return -1000;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //----------------------------------
         int modifier = 0;
         //----------------------------------
         if( "Hull" == hitLocation )
            modifier += 5;
         //----------------------------------
         string enemyUnit = mi.GetEnemyUnit();
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
                        modifier -= 20;
                     break;
                  case "T R":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                        modifier -= 20;
                     if ("Gunner" == cm.Role)
                        modifier += 10;
                     if ("Loader" == cm.Role)
                        modifier -= 10;
                     break;
                  case "T L":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                        modifier -= 20;
                     if ("Gunner" == cm.Role)
                        modifier -= 10;
                     if ("Loader" == cm.Role)
                        modifier += 10;
                     break;
                  case "T B":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                        modifier -= 20;
                     if ("Commander" == cm.Role)
                        modifier += 10;
                     break;
                  case "H F":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                        modifier += 10;
                     if ("Commander" == cm.Role)
                        modifier -= 10;
                     break;
                  case "H FR":
                     if (("Driver" == cm.Role) || ("Loader" == cm.Role))
                        modifier -= 10;
                     if (("Assistant" == cm.Role) || ("Gunner" == cm.Role))
                        modifier += 10;
                     break;
                  case "H BR":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                        modifier -= 40;
                     else
                        modifier -= 30;
                     break;
                  case "H FL":
                     if (("Driver" == cm.Role) || ("Loader" == cm.Role))
                        modifier += 10;
                     if (("Assistant" == cm.Role) || ("Gunner" == cm.Role))
                        modifier -= 10;
                     break;
                  case "H BL":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                        modifier -= 40;
                     else
                        modifier -= 30;
                     break;
                  case "H B":
                     if (("Driver" == cm.Role) || ("Assistant" == cm.Role))
                        modifier -= 40;
                     else
                        modifier -= 30;
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "GetExplosionModifier(): reached default for direction=" + death.myEnemyFireDirection);
                     return -1000;
               }
            }
         }

         //----------------------------------
         if( true == isBailout)
            modifier -= cm.Rating;
         if (true == gi.IsMinefieldAttack)
            modifier -= 20;
         if (true == gi.IsHarrassingFire)
            modifier -= 20;
         //----------------------------------
         if( true == isCollateralDamage )
         {
            if ((true == cm.Action.Contains("FireSubMg")) || (true == cm.Action.Contains("FireAaMg")))
               modifier += 5;
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
            cm.SetBloodSpots(40);
            cm.IsKilled = true;
            cm.Wound = "Killed";
            return "Killed";
         }
         dieRoll += modifier;
         if (dieRoll < 42)
            return "Near Miss";
         else if (dieRoll < 48)
         {
            cm.SetBloodSpots(5);
            if( (true == gi.IsMinefieldAttack) || (null == gi.Death) )
            {
               cm.IsUnconscious = true;
               cm.IsIncapacitated = true;
               return "Unconscious";
            }
            else
            {
               if("None" ==  cm.Wound )
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
            cm.SetBloodSpots(20);
            cm.IsIncapacitated = true;
            if ("None" == cm.Wound)
               cm.Wound = "Light Wound";
            return "Light Wound";
         }
         else if (dieRoll < 93)
         {
            cm.SetBloodSpots(30);
            cm.IsIncapacitated = true;
            if ("Killed" != cm.Wound)
               cm.Wound = "Serious Wound";
            return "Serious Wound";
         }
         else if (dieRoll < 98)
         {
            cm.SetBloodSpots(35);
            cm.IsIncapacitated = true;
            if ("Killed" != cm.Wound)
               cm.Wound = "Serious Wound";
            return "Serious Wound";
         }
         else
         {
            cm.SetBloodSpots(40);
            cm.IsKilled = true;
            cm.Wound = "Killed";
            return "Killed";
         }
      }
      public static string GetWoundEffect(IGameInstance gi, ICrewMember cm, int dieRoll, int modifier)
      {
         if (100 == dieRoll) // unmodified die roll 100 is always a kill
            return "Incapacitated";
         dieRoll += modifier;
         if (dieRoll < 42)
            return "None";
         else if (dieRoll < 48)
         {
            if ((true == gi.IsMinefieldAttack) || (null == gi.Death))
               return "Incapacitated";
            else
               return "None";
         }
         else if (dieRoll < 73)
         {
            return "None";
         }
         else if (dieRoll < 88)
         {
            return "Out 1 week";
         }
         else if (dieRoll < 93)
         {
            return "Out 10 weeks";
         }
         else if (dieRoll < 98)
         {
            return "Sent Home";
         }
         else
         {
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
            Logger.Log(LogEnum.LE_ERROR, "GetBrewUpNumber(): lastReport=null");
            return -1000;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         if (null == gi.Death)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetBrewUpNumber(): gi.Death=null");
            return -1000;
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
         if( true == death.myCause.Contains("Panzerfault"))
         {
            if (true == isWetStorage)
               return 19;
            else if( "M4" == card.myChasis )
               return 84;
            else if ("M4A1" == card.myChasis)
               return 79;
            else if ("M4A3" == card.myChasis)
               return 74;
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "GetBrewUpNumber(): reached default for Panzerfaut myChassis=" + card.myChasis);
               return -1000;
            }
         }
         else // gunfire
         {
            if (true == isWetStorage)
               return 15;
            else if ("M4" == card.myChasis)
               return 79;
            else if ("M4A1" == card.myChasis)
               return 75;
            else if ("M4A3" == card.myChasis)
               return 69;
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "GetBrewUpNumber(): reached default for Panzerfaut myChassis=" + card.myChasis);
               return -1000;
            }
         }
      }
      //-------------------------------------------
      public static string GetRandomEvent(EnumScenario scenario, int dieRoll)
      {
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
                  return "Enemy Reinfore";
               return "Flanking Fire";
            case EnumScenario.Battle:
               if (dieRoll < 6)
                  return "Time Passes";
               if (dieRoll < 16)
                  return "Friendly Artillery";
               if (dieRoll < 21)
                  return "Enemy Artillery";
               if (dieRoll < 31)
                  return "Mines";
               if (dieRoll < 36)
                  return "Panzerfaust";
               if (dieRoll < 41)
                  return "Harrassing Fire";
               if (dieRoll < 46)
                  return "Friendly Advance";
               if (dieRoll < 61)
                  return "Enemy Reinfore";
               if (dieRoll < 81)
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
                  return "Enemy Reinfore";
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
            return -1000;
         }
         //--------------------------------------------------------------
         string enemyUnit = mi.GetEnemyUnit();
         if ("ERROR" == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetSpottingModifier(): unknown enemyUnit=" + mi.Name);
            return -1000;
         }
         //-------------------------------------------------------
         TankCard card = new TankCard(lastReport.TankCardNum);
         int spottingModifer = 0;
         if (true == cm.IsButtonedUp)
         {
            if( ("Commander" == cm.Role ) && (true == card.myIsVisionCupola) )
            {
               spottingModifer += 2;
               Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): bu=+2 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
            }
            else
            {
               spottingModifer += 3;
               Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): bu=+3 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
            }
         }
         if( true == gi.Sherman.IsMoving )
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): ==> move+1 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
         }
         if( true == mi.IsWoods )
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): woods+1 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
         }
         if (true == mi.IsBuilding)
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): build+1 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
         }
         if (true == mi.IsFortification)
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): fort+1 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
         }
         if (true == mi.IsHullDown)
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): hull+1 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
         }
         if ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")))
         {
            spottingModifer += 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): fog+1 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
         }
         int numSmokeMarkers = Territory.GetSmokeCount(gi, sector, range);
         if (numSmokeMarkers < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetSpottingModifier(): GetSmokeCount() returned error");
            return -1000;
         }
         spottingModifer += numSmokeMarkers;
         Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): smoke+" + numSmokeMarkers.ToString() + " enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
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
               Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): size+1 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
               break;
            case "TANK":
            case "PzVIe":
            case "PzV":
               spottingModifer -= 1;
               Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): size-1 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
               break;
            case "PzVIb":
               spottingModifer -= 2;
               Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): size-2 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetSpottingModifier(): Reached Default enemyUnit=" + enemyUnit);
               return -1000;
         }

         //----------------------------------------------------
         if ( 'M' == range )
         {
            spottingModifer -= 1;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): range-1 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
         }
         else if ('C' == range)
         {
            spottingModifer -= 2;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): range-2 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
         }
         //----------------------------------------------------
         if (true == mi.IsFired)
         {
            spottingModifer -= 2;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): fired-2 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
         }
         //----------------------------------------------------
         if ( true == mi.IsMoving )
         {
            spottingModifer -= 3;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): move-3 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
         }
         //----------------------------------------------------
         if (true == mi.IsSpotted)
         {
            spottingModifer -= 3;
            Logger.Log(LogEnum.LE_SHOW_SPOT_MOD, "GetSpottingModifier(): spot-3 enemyUnit=" + enemyUnit + " mod=" + spottingModifer.ToString());
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
            if ((EnumSpottingResult.IDENTIFIED != mi.Spotting) || (EnumSpottingResult.SPOTTED != mi.Spotting))
            {
               Logger.Log(LogEnum.LE_SHOW_SPOT_RESULT, "GetSpottingResult(): mi=" + mi.Name + " Hidden - dr=" + dieRoll.ToString() + " mi.Spotting=" + mi.Spotting.ToString() );
               mi.Spotting = EnumSpottingResult.HIDDEN; // only applies if not already spotted or identified
               return "Hidden";
            }
         }
         //-------------------------------
         int modifier = GetSpottingModifier(gi, mi, cm, sector, range);
         if( modifier < -100 )
         {
            Logger.Log(LogEnum.LE_ERROR, "GetSpottingResult(): GetSpottingModifier() returned error");
            return "ERROR";
         }
         dieRoll += modifier;
         //-------------------------------
         if( dieRoll <= cm.Rating )
         {
            int halfRating = (int)Math.Ceiling(cm.Rating * 0.5);
            if ( dieRoll <= halfRating )
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
         if (true == mi.Name.Contains("ATG"))
         {
            foreach (IStack stack in gi.BattleStacks)
            {
               foreach (IMapItem mapItem in stack.MapItems)
               {
                  if (true == mapItem.Name.Contains("Pak38"))
                  {
                     string name = "Pak39" + Utilities.MapItemNum.ToString();
                     Utilities.MapItemNum++;
                     appearingMapItem = new MapItem(name, mi.Zoom, "c93Pak38", mi.TerritoryCurrent);
                     return appearingMapItem;
                  }
                  else if (true == mapItem.Name.Contains("Pak40"))
                  {
                     string name = "Pak40" + Utilities.MapItemNum.ToString();
                     Utilities.MapItemNum++;
                     appearingMapItem = new MapItem(name, mi.Zoom, "c94Pak40", mi.TerritoryCurrent);
                     return appearingMapItem;
                  }
                  else if (true == mapItem.Name.Contains("Pak43"))
                  {
                     string name = "Pak43" + Utilities.MapItemNum.ToString();
                     Utilities.MapItemNum++;
                     appearingMapItem = new MapItem(name, mi.Zoom, "c95Pak43", mi.TerritoryCurrent);
                     return appearingMapItem;
                  }
               }
            }
         }
         else if (true == mi.Name.Contains("TANK"))
         {
            foreach (IStack stack in gi.BattleStacks)
            {
               foreach (IMapItem mapItem in stack.MapItems)
               {
                  if (true == mapItem.Name.Contains("PzIV"))
                  {
                     string name = "PzIV" + Utilities.MapItemNum.ToString();
                     Utilities.MapItemNum++;
                     appearingMapItem = new MapItem(name, mi.Zoom, "c79PzIV", mi.TerritoryCurrent);
                     return appearingMapItem;
                  }
                  else if (true == mapItem.Name.Contains("PzV"))
                  {
                     string name = "PzV" + Utilities.MapItemNum.ToString();
                     Utilities.MapItemNum++;
                     appearingMapItem = new MapItem(name, mi.Zoom, "c80PzV", mi.TerritoryCurrent);
                     return appearingMapItem;
                  }
               }
            }
         }
         else if (true == mi.Name.Contains("SPG"))
         {
            foreach (IStack stack in gi.BattleStacks)
            {
               foreach (IMapItem mapItem in stack.MapItems)
               {
                  if (true == mapItem.Name.Contains("STuGIIIg"))
                  {
                     string name = "STuGIIIg" + Utilities.MapItemNum.ToString();
                     Utilities.MapItemNum++;
                     appearingMapItem = new MapItem(name, mi.Zoom, "c85STuGIIIg", mi.TerritoryCurrent);
                     return appearingMapItem;
                  }
                  else if (true == mapItem.Name.Contains("MARDERII"))
                  {
                     string name = "MARDERII" + Utilities.MapItemNum.ToString();
                     Utilities.MapItemNum++;
                     appearingMapItem = new MapItem(name, mi.Zoom, "c83MarderII", mi.TerritoryCurrent);
                     return appearingMapItem;
                  }
                  else if (true == mapItem.Name.Contains("MARDERIII"))
                  {
                     string name = "MARDERIII" + Utilities.MapItemNum.ToString();
                     Utilities.MapItemNum++;
                     appearingMapItem = new MapItem(name, mi.Zoom, "c84MarderIII", mi.TerritoryCurrent);
                     return appearingMapItem;
                  }
                  else if (true == mapItem.Name.Contains("JdgPzIV"))
                  {
                     string name = "JdgPzIV" + Utilities.MapItemNum.ToString();
                     Utilities.MapItemNum++;
                     appearingMapItem = new MapItem(name, mi.Zoom, "c86JgdPzIV", mi.TerritoryCurrent);
                     return appearingMapItem;
                  }
                  else if (true == mapItem.Name.Contains("JdgPz38t"))
                  {
                     string name = "JdgPz38t" + Utilities.MapItemNum.ToString();
                     Utilities.MapItemNum++;
                     appearingMapItem = new MapItem(name, mi.Zoom, "c87JgdPz38t", mi.TerritoryCurrent);
                     return appearingMapItem;
                  }
               }
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
               string name = "Pak39" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c93Pak38", mi.TerritoryCurrent);
               return appearingMapItem;
            }
            else if(dieRoll < 9)
            {
               string name = "Pak40" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c94Pak40", mi.TerritoryCurrent);
               return appearingMapItem;
            }
            else 
            {
               string name = "Pak43" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c95Pak43", mi.TerritoryCurrent);
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
               return appearingMapItem;
            }
            else if (dieRoll < 10)
            {
               string name = "PzV" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c80PzV", mi.TerritoryCurrent);
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
                  return appearingMapItem;
               }
               else if (diceRoll < 95)
               {
                  string name = "PzVIe" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c81PzVIe", mi.TerritoryCurrent);
                  return appearingMapItem;
               }
               else
               {
                  string name = "PzVIb" + Utilities.MapItemNum.ToString();
                  Utilities.MapItemNum++;
                  appearingMapItem = new MapItem(name, mi.Zoom, "c82PzVIb", mi.TerritoryCurrent);
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
               return appearingMapItem;
            }
            else if (dieRoll < 5)
            {
               string name = "MARDERII" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c83MarderII", mi.TerritoryCurrent);
               return appearingMapItem;
            }
            else if (dieRoll < 7)
            {
               string name = "MARDERIII" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c84MarderIII", mi.TerritoryCurrent);
               return appearingMapItem;
            }
            else if (dieRoll < 9)
            {
               string name = "JdgPzIV" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c86JgdPzIV", mi.TerritoryCurrent);
               return appearingMapItem;
            }
            else
            {
               string name = "JdgPz38t" + Utilities.MapItemNum.ToString();
               Utilities.MapItemNum++;
               appearingMapItem = new MapItem(name, mi.Zoom, "c87JgdPz38t", mi.TerritoryCurrent);
               return appearingMapItem;
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "GetAppearingUnitNew(): reached default mi=" + mi.Name);
         return null;
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
