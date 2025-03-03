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
      EnumScenario Situation { get; set; }
      int Probability { get; set; }
      EnumResistance Resistance { get; set; }
      string Name { get; set; }
      EnumModel Model { get; set; }
      EnumWeather Weather { get; set; }
      CrewMember Commander { get; set; } 
      CrewMember Gunner { get; set; }
      CrewMember Loader { get; set; }
      CrewMember Driver { get; set; }
      CrewMember Assistant { get; set; }
      //----------------------------------------
      int TimeOfDay { get; set; }
      int Ammo30CalibreMG { get; set; }
      int Ammo50CalibreMG { get; set; }
      int AmmoSmokeBomb { get; set; }
      int AmmoSmokeGrenade { get; set; }
      int AmmoPeriscope { get; set; }  
      int MainGunHE { get; set; }
      int MainGunAP { get; set; }
      int MainGunWP { get; set; }
      int MainGunHCBI { get; set; }
      int MainGunHVAP { get; set; }
      //----------------------------------------
      int VictoryPtsKiaLightWeapon { get; set; }
      int VictoryPtsKiaTruck{ get; set; }
      int VictoryPtsKiaSpwOrPsw { get; set; }
      int VictoryPtsKiaSPGun { get; set; }
      int VictoryPtsKiaPzIV { get; set; }
      int VictoryPtsKiaPzV { get; set; }
      int VictoryPtsKiaPzVI { get; set; }
      int VictoryPtsKiaAtGun { get; set; }
      int VictoryPtsKiaFortifiedPosition { get; set; }
      int VictoryPtsCaptureArea{ get; set; }
      int VictoryPtsKiaExitArea { get; set; }
      int VictoryPtsFriendlyTank { get; set; }
      int VictoryPtsFriendlySquad { get; set; }
      //----------------------------------------
      List<EnumDecoration> Decorations { get; set; }
      //----------------------------------------
      List<String> Notes { get; set; }
      //----------------------------------------
      int VictoryPtsTotalTank { get; set; }
      int VictoryPtsTotalFriendly { get; set; }
      int VictoryPtsTotalTerritory { get; set; }
      //----------------------------------------
      string DayEndedTime { get; set; }
      bool IsBreakdown { get; set; }
      bool IsKnockedOut { get; set; }

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
