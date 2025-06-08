using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pattons_Best
{
   public class ShermanDeath
   {
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
         myCause = cause;
         myDay = gi.Day;
         myIsAmbush = ((BattlePhase.Ambush == gi.BattlePhase) || (BattlePhase.AmbushRandomEvent == gi.BattlePhase) );
         myEnemyFireDirection = TableMgr.GetEnemyFireDirection(gi, eu, myHitLocation);
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
      int GameTurn { set; get; }
      GamePhase GamePhase { set; get; }
      BattlePhase BattlePhase { set; get; }
      int Day { get; set; }
      IAfterActionReports Reports { get; set; }
      GameAction DieRollAction { set; get; } // Used in EventViewerPanel when die roll happens to indicate next event for die roll
      bool IsUndoCommandAvailable { set; get; } // Allow user to back up if selected wrong user action
      String EndGameReason { set; get; }
      //----------------------------------------------
      IMapItems NewMembers { set; get; }
      IMapItems ReadyRacks { set; get; }
      IMapItems Hatches { set; get; }
      IMapItems GunLoads { set; get; }
      IMapItems CrewActions { set; get; }
      IMapItem Sherman { set; get; }
      //------------------------------------------------
      ITerritory Home { get; set; }
      ITerritory? EnemyStrengthCheckTerritory { get; set; }
      ITerritory? ArtillerySupportCheck { get; set; }
      ITerritory? AirStrikeCheckTerritory { get; set; }
      ITerritory? EnteredArea { get; set; }
      ITerritory? AdvanceFire { get; set; }
      //------------------------------------------------
      bool IsTurretActive { set; get; }
      bool IsHatchesActive { set; get; }
      //------------------------------------------------
      bool IsLeadTank { set; get; }
      bool IsAirStrikePending { set; get; }
      bool IsAdvancingFireChosen { set; get; }
      bool IsShermanFiring { set; get; } 
      bool IsShermanFiringAtFront { set; get; }
      bool IsBrokenGunsight { set; get; }
      bool IsBrokenMgAntiAircraft { set; get; }
      //------------------------------------------------
      bool IsMinefieldAttack { set; get; }
      bool IsHarrassingFire { set; get; }
      //------------------------------------------------
      int VictoryPtsTotalCampaign { get; set; }
      int PromotionPoints { get; set; }
      int PromotionDate { get; set; }
      //------------------------------------------------
      int AdvancingFireMarkerCount { set; get; }
      EnumResistance BattleResistance { set; get; }
      Dictionary<string,bool> BrokenPeriscopes { set; get; }
      Dictionary<string, bool> FirstShots { set; get; }
      Dictionary<string, int> AcquiredShots { set; get; }
      ShermanDeath? Death { set; get; }
      PanzerfaustAttack? Panzerfaust { set; get; }
      int NumCollateralDamage { set; get; }
      //------------------------------------------------
      IMapItemMoves MapItemMoves { set; get; }
      IStacks TankStacks { set; get; }
      IStacks MoveStacks { set; get; }
      IStacks BattleStacks { set; get; }
      List<EnteredHex> EnteredHexes { get; }
      //------------------------------------------------
      List<IUnitTest> UnitTests { get; }
      //=========================================================
      ICrewMember? GetCrewMember(string name);
      bool IsDaylightLeft(IAfterActionReport report);
      bool IsExitArea(out bool isExitAreaReached);
      void KillSherman(IAfterActionReport report, string reason);
   }
}
