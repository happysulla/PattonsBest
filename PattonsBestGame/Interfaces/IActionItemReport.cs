using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   public interface IAfterActionReport
   {

      string Day { get; set; }
      EnumScenario Scenario { get; set; }
      int Probability { get; set; }
      EnumResistance Resistance { get; set; }
      string Name { get; set; }
      int TankCardNum { get; set; }
      string Weather { get; set; }
      ICrewMember Commander { get; set; }
      ICrewMember Gunner { get; set; }
      ICrewMember Loader { get; set; }
      ICrewMember Driver { get; set; }
      ICrewMember Assistant { get; set; }
      //----------------------------------------
      int SunriseHour { get; set; }
      int SunriseMin { get; set; }
      int SunsetHour { get; set; }
      int SunsetMin { get; set; }
      //----------------------------------------
      int Ammo30CalibreMG { get; set; }
      int Ammo50CalibreMG { get; set; }
      int AmmoSmokeBomb { get; set; }
      int AmmoSmokeGrenade { get; set; }
      int AmmoPeriscope { get; set; }  
      int MainGunHE { get; set; }
      int MainGunAP { get; set; }
      int MainGunWP { get; set; }
      int MainGunHBCI { get; set; }
      int MainGunHVAP { get; set; }
      //----------------------------------------
      int VictoryPtsFriendlyKiaLightWeapon { get; set; }
      int VictoryPtsFriendlyKiaTruck { get; set; }
      int VictoryPtsFriendlyKiaSpwOrPsw { get; set; }
      int VictoryPtsFriendlyKiaSPGun { get; set; }
      int VictoryPtsFriendlyKiaPzIV { get; set; }
      int VictoryPtsFriendlyKiaPzV { get; set; }
      int VictoryPtsFriendlyKiaPzVI { get; set; }
      int VictoryPtsFriendlyKiaAtGun { get; set; }
      int VictoryPtsFriendlyKiaFortifiedPosition { get; set; }
      //----------------------------------------
      int VictoryPtsYourKiaLightWeapon { get; set; }
      int VictoryPtsYourKiaTruck { get; set; }
      int VictoryPtsYourKiaSpwOrPsw { get; set; }
      int VictoryPtsYourKiaSPGun { get; set; }
      int VictoryPtsYourKiaPzIV { get; set; }
      int VictoryPtsYourKiaPzV { get; set; }
      int VictoryPtsYourKiaPzVI { get; set; }
      int VictoryPtsYourKiaAtGun { get; set; }
      int VictoryPtsYourKiaFortifiedPosition { get; set; }
      //----------------------------------------
      int VictoryPtsCaptureArea{ get; set; }
      int VictoryPtsCapturedExitArea { get; set; }
      int VictoryPtsLostArea { get; set; }
      int VictoryPtsFriendlyTank { get; set; }
      int VictoryPtsFriendlySquad { get; set; }
      //----------------------------------------
      int VictoryPtsTotalYourTank { get; set; }
      int VictoryPtsTotalFriendlyForces { get; set; }
      int VictoryPtsTotalTerritory { get; set; }
      int VictoryPtsTotalEngagement { get; set; }
      //----------------------------------------
      List<EnumDecoration> Decorations { get; set; }
      //----------------------------------------
      List<String> Notes { get; set; }
      //----------------------------------------
      string DayEndedTime { get; set; }
      string Breakdown { get; set; }
      string KnockedOut { get; set; }
   }
   public interface IAfterActionReports : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IAfterActionReport aar);

      void Insert(int index, IAfterActionReport aar);
      void Clear();
      bool Contains(IAfterActionReport aar);
      IAfterActionReport? Find(string day);
      IAfterActionReport? GetLast();
      int IndexOf(IAfterActionReport aar);
      void Remove(IAfterActionReport aar);
      IAfterActionReport? Remove(string day);
      IAfterActionReport? RemoveAt(int index);
      IAfterActionReport? RemoveLast();
      IAfterActionReport? this[int index] { get; set; }
   }
}
