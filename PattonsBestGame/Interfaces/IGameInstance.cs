using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pattons_Best
{
   public interface IGameInstance
   {
      bool CtorError { get; }
      bool IsTalkRoll { get; set; }
      Options Options { get; set; }
      GameStat Statistic { get; set; }
      //----------------------------------------------
      bool IsGridActive { set; get; } // True if there is some EventViewer manager active
      int GameTurn { set; get; }
      GamePhase GamePhase { set; get; }
      GameAction DieRollAction { set; get; } // Used in EventViewerPanel when die roll happens to indicate next event for die roll
      //----------------------------------------------
      ITerritory TargetHex { set; get; } // Used to highlight another hex (find closest castle or find letter location)
      ITerritory NewHex { set; get; } // this is hex moved to if not lost
      //----------------------------------------------
      List<string> Events { set; get; }
      String EndGameReason { set; get; }
      //----------------------------------------------
      bool IsUndoCommandAvailable { set; get; } // Allow user to back up if selected wrong daily action or travel hex
      //----------------------------------------------
      IMapItems PartyMembers { set; get; }
   }
}
