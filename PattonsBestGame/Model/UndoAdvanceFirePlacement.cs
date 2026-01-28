using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   internal class UndoAdvanceFirePlacement : IUndo
   {
      public UndoAdvanceFirePlacement()
      {
      }
      public bool Undo(IGameInstance gi, IGameEngine ge, GameViewerWindow gvw)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "UndoMoveTaskForceToNewArea.Undo(): report=null");
            return false;
         }
         //----------------------------
         IMapItems removals = new MapItems();
         foreach(IStack stack in gi.BattleStacks)
         {
            foreach(IMapItem mi in stack.MapItems)
            {
               if( true == mi.Name.Contains("AdvanceFire"))
                  removals.Add(mi);
            }
         }
         foreach (IMapItem mi in removals)
            gi.BattleStacks.Remove(mi);
         //----------------------------
         gi.AdvancingFireMarkerCount = 6 - (int)Math.Ceiling(lastReport.VictoryPtsFriendlyTank / 3.0);  // six minus friendly tank/3 (rounded up)
         //----------------------------
         IGameCommand? cmd = gi.GameCommands.RemoveLast(); // Remove last command
         if (null == cmd)
         {
            Logger.Log(LogEnum.LE_ERROR, "UndoAdvanceFirePlacement.Undo(): cmd=null");
            return false;
         }
         GameAction nextAction = cmd.Action;
         gi.GamePhase = cmd.Phase;
         gi.DieRollAction = cmd.ActionDieRoll;
         gi.EventDisplayed = gi.EventActive = cmd.EventActive;
         ge.PerformAction(ref gi, ref nextAction, 0);
         return true;
      }
   }
}
