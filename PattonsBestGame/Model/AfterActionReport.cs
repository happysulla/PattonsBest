using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best.Model
{
   public class AfterActionReport : IAfterActionReport
   {
      public string Day { get; set; } = "07/27/44";
      public EnumScenario Situation { get; set; } = EnumScenario.Advance;
      public int Probability { get; set; } = 3;
      public EnumResistance Resistance { get; set; }
      public string Name { get; set; } = Utilities.GetNickName();
      public EnumWeather Weather { get; set; } = EnumWeather.Clear;
      public CrewMember Commander { get; set; } = new CrewMember(EnumCrewRole.Commander, EnumCrewRank.Sargent, 4);
      public CrewMember Gunner { get; set; } = new CrewMember(EnumCrewRole.Gunner, EnumCrewRank.Corporeal, 3);
      public CrewMember Loader { get; set; } = new CrewMember(EnumCrewRole.Loader, EnumCrewRank.Corporeal, 1);
      public CrewMember Driver { get; set; } = new CrewMember(EnumCrewRole.Driver, EnumCrewRank.Private, 1);
      public CrewMember Assistant { get; set; } = new CrewMember(EnumCrewRole.Assistant, EnumCrewRank.Private, 1);
      //----------------------------------------
      public int TimeOfDay { get; set; } = 0500;
      public int Ammo30CalibreMG { get; set; } = 30;
      public int Ammo50CalibreMG { get; set; } = 6;
      public int AmmoSmokeBomb { get; set; } = 14;
      public int AmmoSmokeGrenade { get; set; } = 6;
      public int Periscope { get; set; } = 6;
      public int MainGunHE { get; set; } = 0;
      public int MainGunAP { get; set; } = 0;
      public int MainGunWP { get; set; } = 0;
      public int MainGunHCBI { get; set; } = 0;
      public int MainGunHVAP { get; set; } = 0;
      //----------------------------------------
      public int VictoryPtsKiaLightWeapon { get; set; } = 0;
      public int VictoryPtsKiaTruck { get; set; } = 0;
      public int VictoryPtsKiaSpwOrPsw { get; set; } = 0;
      public int VictoryPtsKiaSPGun { get; set; } = 0;
      public int VictoryPtsKiaPzIV { get; set; } = 0;
      public int VictoryPtsKiaPzV { get; set; } = 0;
      public int VictoryPtsKiaPzVI { get; set; } = 0;
      public int VictoryPtsKiaAtGun { get; set; } = 0;
      public int VictoryPtsKiaFortifiedPosition { get; set; } = 0;
      public int VictoryPtsCaptureArea { get; set; } = 0;
      public int VictoryPtsKiaExitArea { get; set; } = 0;
      public int VictoryPtsFriendlyTank { get; set; } = 0;
      public int VictoryPtsFriendlySquad { get; set; } = 0;
      //----------------------------------------
      public List<EnumDecoration> Decorations { get; set; } = new List<EnumDecoration>();
      //----------------------------------------
      public List<String> Notes { get; set; } = new List<String>();
      //---------------------------------------------------------------------------------
      public AfterActionReport (CombatCalenderEntry entry)
      {
         Day = entry.myDate;
         Situation = entry.mySituation;
         Probability = entry.myProbability;  
         Resistance = entry.myResistance;
      }
      public AfterActionReport(CombatCalenderEntry entry, IAfterActionReport aar)
      {
         Day = entry.myDate;
         Situation = entry.mySituation;
         Probability = entry.myProbability;
         Resistance = entry.myResistance;
         //------------------------------
         this.Name = aar.Name;
         this.Commander = aar.Commander;
         this.Gunner = aar.Gunner;  
         this.Loader = aar.Loader;
         this.Driver = aar.Driver;
         this.Assistant = aar.Assistant;  
         this.Decorations = aar.Decorations; 
      }
   }
}
