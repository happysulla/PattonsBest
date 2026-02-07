using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   public class AfterActionReport : IAfterActionReport
   {
      public bool IsActionThisDay { get; set; } = true;
      public string Day { get; set; } = "07/27/44";
      public EnumScenario Scenario { get; set; } = EnumScenario.Advance;
      public int Probability { get; set; } = 3;
      public EnumResistance Resistance { get; set; }
      public string Name { get; set; } = Utilities.GetNickName();
      public int TankCardNum { get; set; } = 1;  
      public string Weather { get; set; } = "Clear";
      public ICrewMember Commander { get; set; } = new CrewMember("Commander", "Sgt", "c07Commander");
      public ICrewMember Gunner { get; set; } = new CrewMember("Gunner", "Cpl", "c11Gunner");
      public ICrewMember Loader { get; set; } = new CrewMember("Loader", "Cpl", "c09Loader");
      public ICrewMember Driver { get; set; } = new CrewMember("Driver", "Pvt", "c08Driver");
      public ICrewMember Assistant { get; set; } = new CrewMember("Assistant", "Pvt", "c10Assistant");
      //----------------------------------------
      public int SunriseHour { get; set; } = 5;
      public int SunriseMin { get; set; } = 0;
      public int SunsetHour { get; set; } = 19;
      public int SunsetMin { get; set; } = 15;
      //----------------------------------------
      public int Ammo30CalibreMG { get; set; } = 30;
      public int Ammo50CalibreMG { get; set; } = 6;
      public int AmmoSmokeBomb { get; set; } = 14;
      public int AmmoSmokeGrenade { get; set; } = 6;
      public int AmmoPeriscope { get; set; } = 6;
      public int MainGunHE { get; set; } = 0;
      public int MainGunAP { get; set; } = 0;
      public int MainGunWP { get; set; } = 0;
      public int MainGunHBCI { get; set; } = 0;
      public int MainGunHVAP { get; set; } = 0;
      //----------------------------------------
      public int VictoryPtsFriendlyKiaLightWeapon { get; set; } = 0;
      public int VictoryPtsFriendlyKiaTruck { get; set; } = 0;
      public int VictoryPtsFriendlyKiaSpwOrPsw { get; set; } = 0;
      public int VictoryPtsFriendlyKiaSPGun { get; set; } = 0;
      public int VictoryPtsFriendlyKiaPzIV { get; set; } = 0;
      public int VictoryPtsFriendlyKiaPzV { get; set; } = 0;
      public int VictoryPtsFriendlyKiaPzVI { get; set; } = 0;
      public int VictoryPtsFriendlyKiaAtGun { get; set; } = 0;
      public int VictoryPtsFriendlyKiaFortifiedPosition { get; set; } = 0;
      //----------------------------------------
      public int VictoryPtsYourKiaLightWeapon { get; set; } = 0;
      public int VictoryPtsYourKiaTruck { get; set; } = 0;
      public int VictoryPtsYourKiaSpwOrPsw { get; set; } = 0;
      public int VictoryPtsYourKiaSPGun { get; set; } = 0;
      public int VictoryPtsYourKiaPzIV { get; set; } = 0;
      public int VictoryPtsYourKiaPzV { get; set; } = 0;
      public int VictoryPtsYourKiaPzVI { get; set; } = 0;
      public int VictoryPtsYourKiaAtGun { get; set; } = 0;
      public int VictoryPtsYourKiaFortifiedPosition { get; set; } = 0;
      //----------------------------------------
      public int VictoryPtsCaptureArea { get; set; } = 0;
      public int VictoryPtsCapturedExitArea { get; set; } = 0;
      public int VictoryPtsLostArea { get; set; } = 0;
      //----------------------------------------
      public int VictoryPtsFriendlyTank { get; set; } = 0;
      public int VictoryPtsFriendlySquad { get; set; } = 0;
      //----------------------------------------
      public int VictoryPtsTotalYourTank { get; set; } = 0;
      public int VictoryPtsTotalFriendlyForces { get; set; } = 0;
      public int VictoryPtsTotalTerritory { get; set; } = 0;
      //----------------------------------------
      public int VictoryPtsTotalEngagement { get; set; } = 0;
      //----------------------------------------
      public List<EnumDecoration> Decorations { get; set; } = new List<EnumDecoration>();
      //----------------------------------------
      public List<string> Notes { get; set; } = new List<string>();
      //----------------------------------------
      public string DayEndedTime { get; set; } = "";
      public string Breakdown { get; set; } = "No";
      public string KnockedOut { get; set; } = "No";
      //---------------------------------------------------------------------------------
      public AfterActionReport() { }
      public AfterActionReport(ICombatCalendarEntry entry)
      {
         Day = entry.Date;
         Scenario = entry.Scenario;
         Probability = entry.Probability;
         Resistance = entry.Resistance;
      }
      public AfterActionReport(ICombatCalendarEntry entry, IAfterActionReport aar)
      {
         IsActionThisDay = false;
         TankCardNum = aar.TankCardNum;
         //------------------------------
         Day = entry.Date;
         Scenario = entry.Scenario;
         Probability = entry.Probability;
         Resistance = entry.Resistance;
         //------------------------------
         Name = aar.Name;
         Commander = aar.Commander;
         Gunner = aar.Gunner;
         Loader = aar.Loader;
         Driver = aar.Driver;
         Assistant = aar.Assistant;
         //------------------------------
         MainGunHBCI = aar.MainGunHBCI;
         MainGunHVAP = aar.MainGunHVAP;
         //------------------------------
         Decorations = aar.Decorations;
      }
      public bool CaptureArea(IGameInstance gi, ITerritory area)
      {
         Option optionTerrainPointValue = gi.Options.Find("TerrainPointValue");

         if( false == optionTerrainPointValue.IsEnabled )
         {
            this.VictoryPtsCaptureArea++;
         }
         else
         {
            switch( area.Type )
            {
               case "A":
               case "B":
                  this.VictoryPtsCaptureArea++;
                  break;
               case "C":
                  this.VictoryPtsCaptureArea += 3;
                  break;
               case "D":
                  this.VictoryPtsCaptureArea += 2;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "Capture_Area(): reached default with area.Type=" + area.Type);
                  return false;
            }
         }
         //----------------------------
         Option optionTerrainPointValueForCenter = gi.Options.Find("TerrainPointValueForCenter");
         if ((true == optionTerrainPointValueForCenter.IsEnabled) && ("M018" == area.Name) )
            this.VictoryPtsCaptureArea += Utilities.RandomGenerator.Next(0, 3);
         return true;
      }
      public bool LoseArea(IGameInstance gi, ITerritory area)
      {
         Option optionTerrainPointValue = gi.Options.Find("TerrainPointValue");
         if (false == optionTerrainPointValue.IsEnabled)
         {
            this.VictoryPtsCaptureArea--;
         }
         else
         {
            switch (area.Type)
            {
               case "A":
               case "B":
                  this.VictoryPtsCaptureArea--;
                  break;
               case "C":
                  this.VictoryPtsCaptureArea -= 3;
                  break;
               case "D":
                  this.VictoryPtsCaptureArea -= 2;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "Lose_Area(): reached default with area.Type=" + area.Type);
                  return false;
            }
         }
         Option optionTerrainPointValueForCenter = gi.Options.Find("TerrainPointValueForCenter");
         if ((true == optionTerrainPointValueForCenter.IsEnabled) && ("M018" == area.Name))
            this.VictoryPtsCaptureArea -= Utilities.RandomGenerator.Next(0, 3);
         return true;
      }
   }
   //===================================================================
   public class AfterActionReports : IEnumerable, IAfterActionReports
   {
      private readonly ArrayList myList;
      public AfterActionReports() { myList = new ArrayList(); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int Count { get { return myList.Count; } }
      public IAfterActionReport? Find(string day)
      {
         foreach (object o in myList)
         {
            IAfterActionReport aar = (IAfterActionReport)o;
            if (day == Utilities.RemoveSpaces(aar.Day))
               return aar;
         }
         return null;
      }
      public IAfterActionReport? GetLast()
      {
         if (0 == myList.Count)
            return null;
         IAfterActionReport? lastReport = this[myList.Count - 1];
         return lastReport;
      }
      public void Add(IAfterActionReport aar) { myList.Add(aar); }
      public IAfterActionReport? RemoveAt(int index)
      {
         IAfterActionReport? aar = myList[index] as IAfterActionReport;
         myList.RemoveAt(index);
         return aar;
      }
      public void Insert(int index, IAfterActionReport aar) { myList.Insert(index, aar); }
      public void Clear() { myList.Clear(); }
      public bool Contains(IAfterActionReport aar)
      {
         foreach (object o in myList)
         {
            IAfterActionReport aar1 = (IAfterActionReport)o;
            if (aar.Day == aar1.Day) // match on name
               return true;
         }
         return false;
      }
      public int IndexOf(IAfterActionReport aar) { return myList.IndexOf(aar); }
      public void Remove(IAfterActionReport aar)
      {
         foreach (object o in myList)
         {
            IAfterActionReport aar1 = (IAfterActionReport)o;
            if (aar.Day == Utilities.RemoveSpaces(aar1.Day))
            {
               myList.Remove(aar1);
               return;
            }
         }
      }
      public IAfterActionReport? Remove(string day)
      {
         foreach (object o in myList)
         {
            IAfterActionReport aar = (IAfterActionReport)o;
            if (day == aar.Day)
            {
               myList.Remove(aar);
               return aar;
            }
         }
         return null;
      }
      public IAfterActionReport? RemoveLast()
      {
         if (0 == myList.Count)
            return null;
         IAfterActionReport? lastReport = RemoveAt(myList.Count - 1);
         return lastReport;
      }
      public IAfterActionReport? this[int index]
      {
         get
         {
            if (null == myList[index])
               return null;
            IAfterActionReport? aar = myList[index] as IAfterActionReport;
            return aar;
         }
         set { myList[index] = value; }
      }
   }
}
