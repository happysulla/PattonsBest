using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best 
{
   internal class UndoCmdCrewActionOrder : IUndo
   {
      public UndoCmdCrewActionOrder() { }
      public bool Undo(IGameInstance gi, IGameEngine ge)
      {
         gi.GameCommands.RemoveLast(); // Remove last command
         IGameCommand? cmd = gi.GameCommands.GetLast(); // Repeat this command
         if (null == cmd)
         {
            Logger.Log(LogEnum.LE_ERROR, "UndoCrewOrder.Undo(): cmd=null");
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
