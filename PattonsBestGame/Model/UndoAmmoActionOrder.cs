using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   internal class UndoAmmoActionOrder : IUndo
   {
      private bool myIsShermanMoving;
      private bool myIsShermanMoved;
      private int myConsecutiveMoveAttempts;
      private IMapItem? myTargetMainGun;
      private Dictionary<string, int> myEnemyAcquiredShots = new Dictionary<string, int>();
      private IMapItems myGunLoads = new MapItems();
      public UndoAmmoActionOrder(IGameInstance gi)
      {
         foreach (IMapItem gunload in gi.GunLoads)
            myGunLoads.Add(gunload);
         //----------------------------------------
         myIsShermanMoving = gi.Sherman.IsMoving;
         myIsShermanMoved = gi.Sherman.IsMoved;
         myConsecutiveMoveAttempts = gi.ShermanConsectiveMoveAttempt;
         myTargetMainGun = gi.TargetMainGun;
         if (null != myTargetMainGun)
         {
            foreach (KeyValuePair<string, int> kvp in myTargetMainGun.EnemyAcquiredShots)
               this.myEnemyAcquiredShots.Add(kvp.Key, kvp.Value);
         }
      }
      public bool Undo(IGameInstance gi, IGameEngine ge, GameViewerWindow gvw)
      {
         gi.GunLoads.Clear();
         foreach (IMapItem gunload in myGunLoads)
            gi.GunLoads.Add(gunload);
         gi.Sherman.IsMoving = myIsShermanMoving;
         gi.Sherman.IsMoved = myIsShermanMoved;
         gi.ShermanConsectiveMoveAttempt = myConsecutiveMoveAttempts;
         if( null != myTargetMainGun )
         {
            gi.TargetMainGun = myTargetMainGun;
            myTargetMainGun.EnemyAcquiredShots.Clear();
            foreach (KeyValuePair<string, int> kvp in this.myEnemyAcquiredShots)
               myTargetMainGun.EnemyAcquiredShots.Add(kvp.Key, kvp.Value);
         }
         //----------------------------
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
