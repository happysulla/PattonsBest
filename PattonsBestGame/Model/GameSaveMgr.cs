using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using Windows.ApplicationModel.Appointments;

namespace Pattons_Best
{
   public class DayData
   {
      public bool myIsDecorationGiven = false;
      public bool myIsPurpleHeartGiven = false;
      public bool myIsPromoVictoryPointsCalculated = false;
      public bool myIsEngagementCounted = false;
      public DayData(bool isDecoration, bool isPurpleHeart, bool isVpCalculated, bool isGameCounted)
      {
         myIsDecorationGiven = isDecoration;
         myIsPurpleHeartGiven = isPurpleHeart;
         myIsPromoVictoryPointsCalculated = isVpCalculated;
         myIsEngagementCounted = isGameCounted;
      }
   }
   public class DaySaves : Dictionary<int, DayData> // data separated by day
   {
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[");
         foreach(KeyValuePair<int,DayData> kvp in this)
         {
            sb.Append(kvp.Key.ToString());
            sb.Append("=(");
            sb.Append(kvp.Value.myIsDecorationGiven.ToString());
            sb.Append(",");
            sb.Append(kvp.Value.myIsPurpleHeartGiven.ToString());
            sb.Append(",");
            sb.Append(kvp.Value.myIsPromoVictoryPointsCalculated.ToString());
            sb.Append(",");
            sb.Append(kvp.Value.myIsEngagementCounted.ToString());
            sb.Append(")");
         }
         sb.Append("]");
         return sb.ToString();
      }
   }
   public class GameSaveMgr
   {
      static public OrderedDictionary theGameSaves = new OrderedDictionary();
      static public string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("{");
         foreach (DictionaryEntry e in theGameSaves)
            sb.Append(e.ToString());
         sb.Append("}");
         return sb.ToString();
      }
      //------------------------------------------------------
      static public bool GetDecorationGiven(Guid guid, int aDay)
      {
         DaySaves? gameSave = null;
         foreach (DictionaryEntry entry in theGameSaves)
         {
            Guid guid1 = (Guid)entry.Key;
            if (guid1 == guid)
            {
               if (null != entry.Value)
                  gameSave = (DaySaves)entry.Value;
               break;
            }
         }
         if (null == gameSave)
            return false;
         if (false == gameSave.ContainsKey(aDay))
            return false;
         return gameSave[aDay].myIsDecorationGiven;
      }
      static public void SetDecorationGiven(Guid guid, int aDay)
      {
         DaySaves? gameSave = null;
         foreach (DictionaryEntry entry in theGameSaves)
         {
            Guid guid1 = (Guid)entry.Key;
            if (guid1 == guid)
            {
               if (null != entry.Value)
                  gameSave = (DaySaves)entry.Value;
               break;
            }
         }
         if (null == gameSave)
         {
            gameSave = new DaySaves();
            gameSave[aDay] = new DayData(true, false, false, false);
            theGameSaves[guid] = gameSave;
         }
         else
         {
            if (false == gameSave.ContainsKey(aDay))
            {
               gameSave[aDay] = new DayData(true, false, false, false);
            }
            else
            {
               bool isPurpleHeart = gameSave[aDay].myIsPurpleHeartGiven;
               bool isVpCalculated = gameSave[aDay].myIsPromoVictoryPointsCalculated;
               bool isEngagementCounted = gameSave[aDay].myIsEngagementCounted;
               gameSave[aDay] = new DayData(true, isPurpleHeart, isVpCalculated, isVpCalculated);
            }

         }
      }
      //------------------------------------------------------
      static public bool GetHeartGiven(Guid guid, int aDay)
      {
         DaySaves? gameSave = null;
         foreach (DictionaryEntry entry in theGameSaves)
         {
            Guid guid1 = (Guid)entry.Key;
            if (guid1 == guid)
            {
               if (null != entry.Value)
                  gameSave = (DaySaves)entry.Value;
               break;
            }
         }
         if (null == gameSave)
            return false;
         if (false == gameSave.ContainsKey(aDay))
            return false;
         return gameSave[aDay].myIsPurpleHeartGiven;
      }
      static public void SetHeartGiven(Guid guid, int aDay)
      {
         DaySaves? gameSave = null;
         foreach (DictionaryEntry entry in theGameSaves)
         {
            Guid guid1 = (Guid)entry.Key;
            if (guid1 == guid)
            {
               if (null != entry.Value)
                  gameSave = (DaySaves)entry.Value;
               break;
            }
         }
         if (null == gameSave)
         {
            gameSave = new DaySaves();
            gameSave[aDay] = new DayData(false, true, false, false);
            theGameSaves[guid] = gameSave;
         }
         else
         {
            if (false == gameSave.ContainsKey(aDay))
            {
               gameSave[aDay] = new DayData(false, true, false, false);
            }
            else
            {
               bool isDecoration = gameSave[aDay].myIsDecorationGiven;
               bool isVpCalculated = gameSave[aDay].myIsPromoVictoryPointsCalculated;
               bool isEngagementCounted = gameSave[aDay].myIsEngagementCounted;
               gameSave[aDay] = new DayData(isDecoration, true, isVpCalculated, isEngagementCounted);
            }
         }
      }
      //------------------------------------------------------
      static public bool GetVpCalculated(Guid guid, int aDay)
      {
         DaySaves? gameSave = null;
         foreach (DictionaryEntry entry in theGameSaves)
         {
            Guid guid1 = (Guid)entry.Key;
            if (guid1 == guid)
            {
               if (null != entry.Value)
                  gameSave = (DaySaves)entry.Value;
               break;
            }
         }
         if (null == gameSave)
            return false;
         if (false == gameSave.ContainsKey(aDay))
            return false;
         return gameSave[aDay].myIsPromoVictoryPointsCalculated;
      }
      static public void SetVpCalculated(Guid guid, int aDay)
      {
         DaySaves? gameSave = null;
         foreach (DictionaryEntry entry in theGameSaves)
         {
            Guid guid1 = (Guid)entry.Key;
            if (guid1 == guid)
            {
               if (null != entry.Value)
                  gameSave = (DaySaves)entry.Value;
               break;
            }
         }
         if (null == gameSave)
         {
            gameSave = new DaySaves();
            gameSave[aDay] = new DayData(false, false, true, false);
            theGameSaves[guid] = gameSave;
         }
         else
         {
            if (false == gameSave.ContainsKey(aDay))
            {
               gameSave[aDay] = new DayData(false, false, true, false);
            }
            else
            {
               bool isDecoration = gameSave[aDay].myIsDecorationGiven;
               bool isHeart = gameSave[aDay].myIsPurpleHeartGiven;
               bool isEngagementCounted = gameSave[aDay].myIsEngagementCounted;
               gameSave[aDay] = new DayData(isDecoration, isHeart, true, isEngagementCounted);
            }
         }
      }
      //------------------------------------------------------
      static public bool GetEngagementCounted(Guid guid, int aDay)
      {
         DaySaves? gameSave = null;
         foreach (DictionaryEntry entry in theGameSaves)
         {
            Guid guid1 = (Guid)entry.Key;
            if (guid1 == guid)
            {
               if (null != entry.Value)
                  gameSave = (DaySaves)entry.Value;
               break;
            }
         }
         if (null == gameSave)
            return false;
         if (false == gameSave.ContainsKey(aDay))
            return false;
         return gameSave[aDay].myIsEngagementCounted;
      }
      static public void SetEngagementCounted(Guid guid, int aDay)
      {
         DaySaves? gameSave = null;
         foreach (DictionaryEntry entry in theGameSaves)
         {
            Guid guid1 = (Guid)entry.Key;
            if (guid1 == guid)
            {
               if (null != entry.Value)
                  gameSave = (DaySaves)entry.Value;
               break;
            }
         }
         if (null == gameSave)
         {
            gameSave = new DaySaves();
            gameSave[aDay] = new DayData(false, false, false, true);
            theGameSaves[guid] = gameSave;
         }
         else
         {
            if (false == gameSave.ContainsKey(aDay))
            {
               gameSave[aDay] = new DayData(false, false, false, true);
            }
            else
            {
               bool isDecoration = gameSave[aDay].myIsDecorationGiven;
               bool isHeart = gameSave[aDay].myIsPurpleHeartGiven;
               bool isVpCalculated = gameSave[aDay].myIsPromoVictoryPointsCalculated;
               gameSave[aDay] = new DayData(isDecoration, isHeart, isVpCalculated, true);
            }
         }
      }
   }

}
