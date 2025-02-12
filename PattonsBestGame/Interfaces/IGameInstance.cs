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
      bool IsGridActive { set; get; } // True if there is some EventViewer manager active
      string EventActive { set; get; }
      string EventDisplayed { set; get; }
      string EventStart { set; get; } // Event ID when encounter starts
      List<string> Events { set; get; }
      Dictionary<string, int[]> DieResults { get; }
      //----------------------------------------------
      int GameTurn { set; get; }
      GamePhase GamePhase { set; get; }
      GameAction DieRollAction { set; get; } // Used in EventViewerPanel when die roll happens to indicate next event for die roll
      bool IsUndoCommandAvailable { set; get; } // Allow user to back up if selected wrong user action
      String EndGameReason { set; get; }
      //----------------------------------------------
      IMapItems MapItems { set; get; }
      IMapItemMoves MapItemMoves { set; get; }
      IStacks Stacks { set; get; }
      //------------------------------------------------
      ITerritory? NewTerritory { set; get; }
      List<EnteredHex> EnteredHexes { get; }
      //---------------------------------------------------------------
      int Day { get; set; }
      //----------------------------------------------
      List<IUnitTest> UnitTests { get; }
   }
}
