using System;
using System.Collections.Generic;

namespace Pattons_Best
{
   public class ShermanDeath
   {
      public bool myCtorError = false;
      public IMapItem myEnemyUnit;
      public string myHitLocation = "";
      public string myEnemyFireDirection = "";
      public int myDay = 0;
      public string myCause = "";
      public bool myIsAmbush = false;
      public bool myIsExplosion = false;
      public bool myIsBailout = false;
      public bool myIsBrewUp = false;
      public ShermanDeath(IGameInstance gi, IMapItem eu, string loc, string cause)
      {
         myEnemyUnit = eu;
         myHitLocation = loc;
         if ("ERROR" == loc)
         {
            myCtorError = true;
            Logger.Log(LogEnum.LE_ERROR, "ShermanDeath(): loc=ERROR");
         }
         myCause = cause;
         myDay = gi.Day;
         myIsAmbush = ((BattlePhase.Ambush == gi.BattlePhase) || (BattlePhase.AmbushRandomEvent == gi.BattlePhase) );
         myEnemyFireDirection = TableMgr.GetEnemyFireDirection(gi, eu, myHitLocation);
         if("ERROR" == myEnemyFireDirection)
         {
            myCtorError = true;
            Logger.Log(LogEnum.LE_ERROR, "ShermanDeath(): GetEnemyFireDirection() returned ERROR");
         }
      }
   }
   public class PanzerfaustAttack
   {
      public IMapItem myEnemyUnit;
      public int myDay = 0;
      public bool myIsShermanMoving = false;
      public bool myIsLeadTank = false;
      public bool myIsAdvancingFireZone = false;
      public char mySector;
      public PanzerfaustAttack( IGameInstance gi, IMapItem enemyUnit, bool isAdvanceFire, char sector)
      {
         myEnemyUnit = enemyUnit;
         myDay = gi.Day;
         myIsShermanMoving = gi.Sherman.IsMoving;
         myIsLeadTank = gi.IsLeadTank;
         myIsAdvancingFireZone = isAdvanceFire;
         mySector = sector;
      }
   }
   public class ShermanAttack
   {
      public string myAttackType; // area or direct fire
      public string myAmmoType;   // He, Ap, Hbci, Wp,
      public bool myIsCriticalHit;
      public string myHitLocation = ""; // Turret, Hull, Thrown Track
      public bool myIsNoChance = false;
      public bool myIsImmobilization = false;
      public ShermanAttack( string attack, string ammo, bool critical, bool immobilization )
      {
         myAttackType = attack;
         myAmmoType = ammo;
         myIsCriticalHit = critical;
         myIsImmobilization = immobilization;
      }
   }
   //-------------------------------------------------
   public interface IGameInstance
   {
      bool CtorError { get; }
      Options Options { get; set; }
      GameStat Statistic { get; set; }
      //----------------------------------------------
      bool IsMultipleSelectForDieResult { set; get; } // In EventViewer, show buttons instead of die results for user to choose from
      bool IsGridActive { set; get; } // True if there is some EventViewer manager active
      string EventActive { set; get; }
      string EventDisplayed { set; get; }
      string EventStart { set; get; } // Event ID when encounter starts
      List<string> Events { set; get; }
      Dictionary<string, int[]> DieResults { get; }
      //----------------------------------------------
      int Day { get; set; }
      int GameTurn { set; get; }
      GamePhase GamePhase { set; get; }
      GameAction DieRollAction { set; get; } // Used in EventViewerPanel when die roll happens to indicate next event for die roll
      bool IsUndoCommandAvailable { set; get; } // Allow user to back up if selected wrong user action
      String EndGameReason { set; get; }
      //----------------------------------------------
      IAfterActionReports Reports { get; set; }
      BattlePhase BattlePhase { set; get; }
      CrewActionPhase CrewActionPhase { set; get; }
      string MovementEffectOnSherman { set; get; }
      string MovementEffectOnEnemy { set; get; }
      string FiredAmmoType { set; get; }  
      //----------------------------------------------
      IMapItems NewMembers { set; get; }
      IMapItems ReadyRacks { set; get; }
      IMapItems Hatches { set; get; }
      IMapItems CrewActions { set; get; }
      IMapItems GunLoads { set; get; }
      IMapItem Sherman { set; get; }
      IMapItems Targets { set; get; }
      IMapItem? TargetMainGun { set; get; }
      IMapItem? TargetMg { set; get; }
      IMapItems AdvancingEnemies { set; get; }
      IMapItems ShermanAdvanceOrRetreatEnemies { set; get; }
      IMapItems InjuredCrewMembers { set; get; }
      IMapItem? ShermanHvss { set; get; }
      ICrewMember? ReturningCrewman { set; get; }
      //------------------------------------------------
      ITerritory Home { get; set; }
      ITerritory? EnemyStrengthCheckTerritory { get; set; }
      ITerritory? ArtillerySupportCheck { get; set; }
      ITerritory? AirStrikeCheckTerritory { get; set; }
      ITerritory? EnteredArea { get; set; }
      ITerritory? AdvanceFire { get; set; }
      ITerritory? FriendlyAdvance { get; set; }
      ITerritory? EnemyAdvance { get; set; }
      ITerritories AreaTargets { get; set; }
      //------------------------------------------------
      bool IsHatchesActive { set; get; }
      bool IsRetreatToStartArea { set; get; }
      //------------------------------------------------
      string SwitchedCrewMember { set; get; }
      int AssistantOriginalRating { set; get; }
      //------------------------------------------------
      bool IsShermanFirstShot { set; get; }
      bool IsShermanFiringAtFront { set; get; }
      bool IsShermanDeliberateImmobilization { set; get; }
      string ShermanTypeOfFire { set; get; }
      int NumSmokeAttacksThisRound { set; get; }
      bool IsMalfunctionedMainGun { set; get; }
      bool IsMainGunRepairAttempted { set; get; }
      bool IsBrokenMainGun { set; get; }
      bool IsBrokenGunSight { set; get; }
      Dictionary<string, bool> FirstShots { set; get; }
      Dictionary<string, int> AcquiredShots { set; get; }
      List<string> TrainedGunners { get; } // trained in use of HVSS 
      List<ShermanAttack> ShermanHits { set; get; }
      ShermanDeath? Death { set; get; }
      //------------------------------------------------
      string IdentifiedTank { set; get; }
      string IdentifiedAtg { set; get; }
      string IdentifiedSpg { set; get; }
      //------------------------------------------------
      bool IsShermanFiringAaMg { set; get; }
      bool IsShermanFiringBowMg { set; get; }
      bool IsShermanFiringCoaxialMg { set; get; }
      bool IsShermanFiringSubMg { set; get; }
      bool IsCommanderDirectingMgFire { set; get; }
      bool IsShermanFiredAaMg { set; get; }
      bool IsShermanFiredBowMg { set; get; }
      bool IsShermanFiredCoaxialMg { set; get; }
      bool IsShermanFiredSubMg { set; get; }
      //------------------------------------------------
      bool IsMalfunctionedMgCoaxial { set; get; }
      bool IsMalfunctionedMgBow { set; get; }
      bool IsMalfunctionedMgAntiAircraft { set; get; }
      bool IsCoaxialMgRepairAttempted { set; get; }
      bool IsBowMgRepairAttempted { set; get; }
      bool IsAaMgRepairAttempted { set; get; }
      bool IsBrokenMgAntiAircraft { set; get; }
      bool IsBrokenMgBow { set; get; }
      bool IsBrokenMgCoaxial { set; get; }
      //------------------------------------------------
      bool IsBrokenPeriscopeDriver { set; get; }
      bool IsBrokenPeriscopeLoader { set; get; }
      bool IsBrokenPeriscopeAssistant { set; get; }
      bool IsBrokenPeriscopeGunner { set; get; }
      bool IsBrokenPeriscopeCommander { set; get; }
      //------------------------------------------------
      bool IsShermanTurretRotated { set; get; }
      double ShermanRotationTurretOld { set; get; }
      //------------------------------------------------
      bool IsLeadTank { set; get; }
      bool IsAirStrikePending { set; get; }
      bool IsAdvancingFireChosen { set; get; }
      int AdvancingFireMarkerCount { set; get; }
      EnumResistance BattleResistance { set; get; }
      //------------------------------------------------
      bool IsMinefieldAttack { set; get; }
      bool IsHarrassingFireBonus { set; get; }
      bool IsFlankingFire { set; get; }
      bool IsEnemyAdvanceComplete { set; get; }
      PanzerfaustAttack? Panzerfaust { set; get; }
      int NumCollateralDamage { set; get; }
      //------------------------------------------------
      int VictoryPtsTotalCampaign { get; set; }
      int PromotionPointNum { get; set; }
      int PromotionDay { get; set; }
      int NumPurpleHeart { get; set; }
      bool IsCommanderRescuePerformed { set; get; }
      bool IsCommanderKilled { set; get; }
      bool IsPromoted { set; get; }
      //------------------------------------------------
      IMapItemMoves MapItemMoves { set; get; }
      IStacks MoveStacks { set; get; }
      IStacks BattleStacks { set; get; }
      List<EnteredHex> EnteredHexes { get; }
      //------------------------------------------------
      List<IUnitTest> UnitTests { get; }
      //=========================================================
      bool IsCrewActionSelectable(string crewRole, out bool isGiven);
      bool IsCrewActionPossibleButtonUp(string crewRole, string crewAction);
      ICrewMember? GetCrewMemberByRole(string role);
      ICrewMember? GetCrewMemberByName(string name);
      void SetIncapacitated(ICrewMember crewmember);
      bool SetCrewActionTerritory(ICrewMember cm);
      bool SwitchMembers(string switchingMemberRole);
      void ClearCrewActions(string caller);
      //------------------------------------------------
      string GetGunLoadType();
      bool SetGunLoadTerritory(string ammoType);
      bool ReloadGun();
      string GetAmmoReloadType();
      bool FireAndReloadGun();
      bool IsReadyRackReload();
      int GetReadyRackReload(string ammoType);
      int GetReadyRackTotalLoad();
      bool SetReadyRackReload(string name, int value);
      //------------------------------------------------
      bool IsDaylightLeft(IAfterActionReport report);
      bool IsTaskForceInStartArea(out bool isStartArea);
      bool IsTaskForceInExitArea(out bool isExitAreaReached);
      void KillSherman(IAfterActionReport report, string reason);
      void ScoreYourVictoryPoint(IAfterActionReport report, IMapItem enemy);
      void ScoreFriendlyVictoryPoint(IAfterActionReport report, IMapItem enemy);
   }
}
