using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best.Model
{
   internal class AfterActionReport
   {
      public int Day { get; set; } = 0;
      public EnumScenario Situation { get; set; }
      public int Probability { get; set; } = 0;
      public EnumResistance Resistance { get; set; }
      public string Name { get; set; } = Utilities.GetNickName();
      public EnumWeather Weather { get; set; }
      public List<CrewMember> CrewerMembers { get; set; } = new List<CrewMember>();
      //----------------------------------------
      public int TimeOfDay { get; set; } = 0;
      public int Ammo30CalibreMG { get; set; } = 0;
      public int Ammo50CalibreMG { get; set; } = 0;
      public int AmmoSmokeBomb { get; set; } = 0;
      public int AmmoSmokeGrenade { get; set; } = 0;
      public int Periscope { get; set; } = 0;
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
   }
}
