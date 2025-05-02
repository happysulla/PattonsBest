using Pattons_Best.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pattons_Best
{
   public interface IGameInstance
   {
      bool CtorError { get; }
      Options Options { get; set; }
      GameStat Statistic { get; set; }
      //----------------------------------------------
      bool IsMultipleSelectForDieResult { set; get; } // In EventViewer, show buttons instead of die results for user to choose from
      bool IsGridActive { set; get; } // True if there is some EventViewer manager active
      bool IsPrepActive { set; get; } // True if user can open hatches
      string EventActive { set; get; }
      string EventDisplayed { set; get; }
      string EventStart { set; get; } // Event ID when encounter starts
      List<string> Events { set; get; }
      Dictionary<string, int[]> DieResults { get; }
      //----------------------------------------------
      int GameTurn { set; get; }
      GamePhase GamePhase { set; get; }
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
      IMapItem Sherman { set; get; }
      //------------------------------------------------
      ITerritory Home { get; set; }
      ITerritory? EnemyStrengthCheckTerritory { get; set; }
      ITerritory? ArtillerySupportCheck { get; set; }
      ITerritory? AirStrikeCheckTerritory { get; set; }
      ITerritory? EnteredArea { get; set; }
      ITerritory? AdvanceFire { get; set; }
      //------------------------------------------------
      bool IsLeadTank { set; get; }
      bool IsTurretActive { set; get; }
      bool IsAirStrikePending { set; get; }
      bool IsAdvancingFireChosen { set; get; }
      bool IsAmbush { set; get; }
      bool IsShermanFiring { set; get; } 
      bool IsShermanFiringAtFront { set; get; }
      int AdvancingFireMarkerCount { set; get; }
      int FriendlyTankLossCount { set; get; }
      EnumResistance BattleResistance { set; get; }
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
   }
}
