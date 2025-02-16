using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   public enum EnumCrewRole
   {
      Commander,
      Gunner,
      Loader,
      Driver,
      Assistant
   };
   public enum EnumCrewRank
   {
      Private,
      Corporeal,
      Sargent,
      Second_Lt,
      First_Lt,
      Captain
   };
   public struct CrewMember
   {
      public EnumCrewRole myRole;
      public EnumCrewRank myRank;
      public string myName;
      public int myRating;
      public CrewMember(EnumCrewRole role, EnumCrewRank rank, int rating)
      {
         myRole = role;
         myRank = rank;
         myName = SurnameMgr.GetSurname();
         myRating = rating;
      }
   };
   public enum EnumDecoration
   {
      BronzeStar,
      SilverStar,
      DistinguisedServiceCross,
      MedalOfHonor,
      PurpleHeart,
      EuropeanCampain,
      WW2Victory
   };
   public interface IAfterActionReport
   {
      string Day { get; set; }
      EnumScenario Situation { get; set; }
      int Probability { get; set; }
      EnumResistance Resistance { get; set; }
      string Name { get; set; }
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
      int Periscope { get; set; }  
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
   }
}
