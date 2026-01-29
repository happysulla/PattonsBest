using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   internal class UndoAmmoActionOrder : IUndo
   {
      public UndoAmmoActionOrder() { }
      public bool Undo(IGameInstance gi, IGameEngine ge, GameViewerWindow gvw)
      {
         gi.GameCommands.RemoveLast(); // Remove last command
         IGameCommand? cmd = gi.GameCommands.GetLast(); // Repeat this command
         if (null == cmd)
         {
            Logger.Log(LogEnum.LE_ERROR, "UndoAmmoActionOrder.Undo(): cmd=null");
            return false;
         }
         Logger.Log(LogEnum.LE_UNDO_COMMAND, "UndoAmmoActionOrder.Undo(): a=" + cmd.Action.ToString() + " dra=" + cmd.ActionDieRoll.ToString() + " e=" + cmd.EventActive + " ca=" + gi.CrewActions.ToString());
         //----------------------------
         gvw.UpdateViewAmmoOrderButtons(gi);
         //----------------------------
         GameAction nextAction = cmd.Action;
         gi.GamePhase = cmd.Phase;
         gi.DieRollAction = cmd.ActionDieRoll;
         gi.EventDisplayed = gi.EventActive = cmd.EventActive;
         ge.PerformAction(ref gi, ref nextAction, 0);
         return true;
      }
   }
}
