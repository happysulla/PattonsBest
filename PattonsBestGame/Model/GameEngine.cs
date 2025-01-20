using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Pattons_Best
{
   public class GameEngine : IGameEngine
   {
      //---------------------------------------------------------------------
      private readonly MainWindow myMainWindow;
      private readonly List<IView> myViews = new List<IView>();
      public List<IView> Views
      {
         get { return myViews; }
      }
      //---------------------------------------------------------------
      public GameEngine(MainWindow mainWindow)
      {
         myMainWindow = mainWindow;
      }
      public void RegisterForUpdates(IView view)
      {
         myViews.Add(view);
      }
      public void PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         IGameState? state = GameState.GetGameState(gi.GamePhase); // First get the current game state. Then call performNextAction() on the game state.
         if (null == state)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameEngine.PerformAction(): s=null for p=" + gi.GamePhase.ToString());
            return;
         }
         string returnStatus = state.PerformAction(ref gi, ref action, dieRoll); // Perform the next action
         if ("OK" != returnStatus)
         {
            StringBuilder sb1 = new StringBuilder("<<<<ERROR3:::::: GameEngine.PerformAction(): ");
            sb1.Append(" a="); sb1.Append(action.ToString());
            sb1.Append(" dr="); sb1.Append(dieRoll.ToString());
            sb1.Append(" r="); sb1.Append(returnStatus);
            Logger.Log(LogEnum.LE_ERROR, sb1.ToString());
         }
         myMainWindow.UpdateViews(gi, action); // Update all registered views when performNextAction() is called
      }
      public bool CreateUnitTests(IGameInstance gi, DockPanel dp, EventViewer ev, IDieRoller dr)
      {
         //-----------------------------------------------------------------------------
         IUnitTest ut1 = new GameViewerCreateUnitTest(dp);
         if (true == ut1.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): GameViewerCreateUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut1);
         //-----------------------------------------------------------------------------
         IUnitTest ut2 = new TerritoryCreateUnitTest(dp, gi, Territories.MOVE_TERRITORIES);
         if (true == ut2.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): TerritoryCreateUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut2);
         //-----------------------------------------------------------------------------
         IUnitTest ut3 = new TerritoryRegionUnitTest(dp, gi);
         if (true == ut3.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): TerritoryRegionUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut3);
         //-----------------------------------------------------------------------------
         IUnitTest ut4 = new PolylineCreateUnitTest(dp, gi);
         if (true == ut4.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): PolylineCreateUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut4);
         //-----------------------------------------------------------------------------
         //IUnitTest ut5 = new ConfigMgrUnitTest(dp, ev);
         //if (true == ut5.CtorError)
         //{
         //   Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): ConfigMgrUnitTest() ctor error");
         //   return false;
         //}
         //gi.UnitTests.Add(ut5);
         //-----------------------------------------------------------------------------
         IUnitTest ut7 = new DiceRollerUnitTest(dp, dr);
         if (true == ut7.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): DiceRollerUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut7);
         //-----------------------------------------------------------------------------
         return true;
      }
   }
}
