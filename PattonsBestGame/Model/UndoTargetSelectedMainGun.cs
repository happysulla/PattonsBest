using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   internal class UndoTargetSelectedMainGun : IUndo
   {
      IMapItem? myPreviousTarget;
      public UndoTargetSelectedMainGun(IMapItem? previousTarget)
      {
         myPreviousTarget = previousTarget;
      }
      public bool Undo(IGameInstance gi, IGameEngine ge, GameViewerWindow gvw)
      {
         gi.TargetMainGun = myPreviousTarget;
         IGameCommand? cmd = gi.GameCommands.RemoveLast(); // Remove last command
         if (null == cmd)
         {
            Logger.Log(LogEnum.LE_ERROR, "UndoTargetSelectedMainGun.Undo(): cmd=null");
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
