using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pattons_Best
{
   internal class UndoMoveTaskForceToNewArea : IUndo
   {
      private ITerritory myPrevious;
      private int myMinsToRemove;
      public UndoMoveTaskForceToNewArea(ITerritory previous, int minsToRemove)
      {
         myPrevious = previous;
         myMinsToRemove = minsToRemove;
      }
      public bool Undo(IGameInstance gi, IGameEngine ge, GameViewerWindow gvw)
      {
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UndoMoveTaskForceToNewArea.Undo(): report=null");
            return false;
         }
         report.SunriseMin -= myMinsToRemove;
         if (report.SunriseMin < 0)
         {
            report.SunriseHour--;
            report.SunriseMin += 60;
         }
         //-----------------------------------
         IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
         if (null == taskForce)
         {
            Logger.Log(LogEnum.LE_ERROR, "UndoMoveTaskForceToNewArea.Undo(): taskForce=null");
            return false;
         }
         //-----------------------------------
         gi.MoveStacks.Remove(taskForce); // remove from existing stack
         taskForce.TerritoryCurrent = taskForce.TerritoryStarting = myPrevious; // Move TaskForce counter back to original area
         IMapPoint mp = Territory.GetRandomPoint(myPrevious, taskForce.Zoom * Utilities.theMapItemOffset);
         taskForce.Location.X = mp.X;
         taskForce.Location.Y = mp.Y;
         gi.MoveStacks.Add(taskForce); // add to previous stack
         //-----------------------------------
         Logger.Log(LogEnum.LE_VIEW_MIM_CLEAR, "UndoMoveTaskForceToNewArea.Undo():  MapItemMoves.Clear()");
         gi.MapItemMoves.Clear();
         gi.EnteredHexes.RemoveAt(gi.EnteredHexes.Count - 1);
         //-----------------------------------
         gi.GameCommands.RemoveLast(); // Remove last command
         IGameCommand? cmd = gi.GameCommands.GetLast(); // Repeat this command
         if (null == cmd)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView_ForNewGame(): cmd=null");
            return false;
         }
         GameAction nextAction = cmd.Action;
         gi.GamePhase = cmd.Phase;
         gi.DieRollAction = cmd.ActionDieRoll;
         gi.EventDisplayed = gi.EventActive = cmd.EventActive;
         ge.PerformAction(ref gi, ref nextAction, Utilities.NO_RESULT);
         return true;
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("UndoMoveTaskForceToNewArea (previous=");
         sb.Append(this.myPrevious.ToString());
         sb.Append(",minsReturned=");
         sb.Append(myMinsToRemove.ToString());
         sb.Append(")");
         return sb.ToString();
      }
   }
}
