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
using Windows.ApplicationModel.Appointments;

namespace Pattons_Best
{
   public class DayData
   {
      public bool myIsDecorationGiven = false;
      public bool myIsPurpleHeartGiven = false;
      public bool myIsPromoVictoryPointsCalculated = false;
      public DayData(bool isDecoration, bool isPurpleHeart, bool isVpCalculated)
      {
         myIsDecorationGiven = isDecoration;
         myIsPurpleHeartGiven = isPurpleHeart;
         myIsPromoVictoryPointsCalculated = isVpCalculated;
      }
   }
   public class DaySaves : Dictionary<int, DayData> // data separated by day
   {
   }
   public class GameSaveMgr
   {
      static public OrderedDictionary theGameSaves = new OrderedDictionary();
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
            gameSave[aDay] = new DayData(true, false, false);
            theGameSaves[guid] = gameSave;
         }
         else
         {
            bool isPurpleHeart = gameSave[aDay].myIsPurpleHeartGiven;
            bool isVpCalculated = gameSave[aDay].myIsPromoVictoryPointsCalculated;
            gameSave[aDay] = new DayData(true, isPurpleHeart, isVpCalculated);
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
            gameSave[aDay] = new DayData(false, true, false);
            theGameSaves[guid] = gameSave;
         }
         else
         {
            bool isDecoration = gameSave[aDay].myIsDecorationGiven;
            bool isVpCalculated = gameSave[aDay].myIsPromoVictoryPointsCalculated;
            gameSave[aDay] = new DayData(isDecoration, true, isVpCalculated);
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
            gameSave[aDay] = new DayData(false, false, true);
            theGameSaves[guid] = gameSave;
         }
         else
         {
            bool isDecoration = gameSave[aDay].myIsDecorationGiven;
            bool isHeart = gameSave[aDay].myIsPurpleHeartGiven;
            gameSave[aDay] = new DayData(isDecoration, isHeart, true);
         }
      }
   }

}
