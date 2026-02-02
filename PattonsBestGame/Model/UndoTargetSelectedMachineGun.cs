using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   internal class UndoTargetSelectedMachineGun : IUndo
   {
      IMapItem? myAdvanceFireMapItem ;
      public UndoTargetSelectedMachineGun(IMapItem? mi)
      {
         myAdvanceFireMapItem = mi;
      }
      public bool Undo(IGameInstance gi, IGameEngine ge, GameViewerWindow gvw)
      {
         if( null != myAdvanceFireMapItem )
         {
            foreach(IStack stack in gi.BattleStacks)
            {
               foreach(IMapItem mi in stack.MapItems)
               {
                  if (myAdvanceFireMapItem.Name == mi.Name )
                  {
                     stack.MapItems.Remove(mi);
                     break;
                  }
               }
            }
         }
         //-----------------------------------
         gi.GameCommands.RemoveLast(); // Remove last command
         IGameCommand? cmd = gi.GameCommands.GetLast(); // Repeat this command
         if (null == cmd)
         {
            Logger.Log(LogEnum.LE_ERROR, "UndoTargetSelectedMachineGun.Undo(): cmd=null");
            return false;
         }
         //-----------------------------------
         GameAction nextAction = cmd.Action;
         gi.GamePhase = cmd.Phase;
         gi.DieRollAction = cmd.ActionDieRoll;
         gi.EventDisplayed = gi.EventActive = cmd.EventActive;
         ge.PerformAction(ref gi, ref nextAction, Utilities.NO_RESULT);
         return true;
      }
   }
}
