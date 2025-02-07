using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Pattons_Best
{
   public class GameEngine : IGameEngine
   {
      public static Dictionary<int, CombatCalenderEntry> theCombatCalenderEntries = new Dictionary<int, CombatCalenderEntry>();
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
         theCombatCalenderEntries[0] = new CombatCalenderEntry("07/27/43", EnumScenario.Advance, 3, EnumResistance.Light, "Corba Breakout");
         theCombatCalenderEntries[1] = new CombatCalenderEntry("07/28/43", EnumScenario.Battle, 4, EnumResistance.Medium, "Coutances");
         theCombatCalenderEntries[2] = new CombatCalenderEntry("07/29/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[3] = new CombatCalenderEntry("07/30/43", EnumScenario.Advance, 4, EnumResistance.Medium, "Avranches");
         theCombatCalenderEntries[4] = new CombatCalenderEntry("08/01/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[5] = new CombatCalenderEntry("08/02/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[6] = new CombatCalenderEntry("08/03/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[7] = new CombatCalenderEntry("08/01/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[8] = new CombatCalenderEntry("08/02/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[9] = new CombatCalenderEntry("08/03/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[10] = new CombatCalenderEntry("08/01/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[11] = new CombatCalenderEntry("08/02/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[12] = new CombatCalenderEntry("08/03/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[13] = new CombatCalenderEntry("08/01/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[14] = new CombatCalenderEntry("08/02/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[15] = new CombatCalenderEntry("08/03/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[16] = new CombatCalenderEntry("08/04/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[17] = new CombatCalenderEntry("08/05/43", EnumScenario.Advance, 4, EnumResistance.Heavy, "Vannes");
         theCombatCalenderEntries[18] = new CombatCalenderEntry("08/06/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[19] = new CombatCalenderEntry("08/07/43", EnumScenario.Advance, 4, EnumResistance.Heavy, "Lorient");
         theCombatCalenderEntries[20] = new CombatCalenderEntry("08/08/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[21] = new CombatCalenderEntry("08/09/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[22] = new CombatCalenderEntry("08/10/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[23] = new CombatCalenderEntry("08/11/43", EnumScenario.Advance, 4, EnumResistance.Light, "Nantes");
         theCombatCalenderEntries[24] = new CombatCalenderEntry("08/12/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[25] = new CombatCalenderEntry("08/13/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[26] = new CombatCalenderEntry("08/14/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[27] = new CombatCalenderEntry("08/15/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[28] = new CombatCalenderEntry("08/16/43", EnumScenario.Advance, 3, EnumResistance.Medium, "Orleans");
         theCombatCalenderEntries[29] = new CombatCalenderEntry("08/17/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[30] = new CombatCalenderEntry("08/18/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[31] = new CombatCalenderEntry("08/19/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[32] = new CombatCalenderEntry("08/20/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[33] = new CombatCalenderEntry("08/21/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[34] = new CombatCalenderEntry("08/22/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[35] = new CombatCalenderEntry("08/23/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[36] = new CombatCalenderEntry("08/24/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[37] = new CombatCalenderEntry("08/25/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[38] = new CombatCalenderEntry("08/26/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[39] = new CombatCalenderEntry("08/27/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[40] = new CombatCalenderEntry("08/28/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[41] = new CombatCalenderEntry("08/29/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[42] = new CombatCalenderEntry("08/30/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[43] = new CombatCalenderEntry("08/31/43", EnumScenario.Advance, 4, EnumResistance.Medium, "Commery");
         theCombatCalenderEntries[44] = new CombatCalenderEntry("09/01/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[45] = new CombatCalenderEntry("09/02-09/10 1943", EnumScenario.Retrofit, 10, EnumResistance.None);
         theCombatCalenderEntries[46] = new CombatCalenderEntry("09/11/43", EnumScenario.Advance, 5, EnumResistance.Heavy, "Moselle Crossing");
         theCombatCalenderEntries[47] = new CombatCalenderEntry("09/12/43", EnumScenario.Counterattack, 5, EnumResistance.Medium, "Moselle Crossing");
         theCombatCalenderEntries[48] = new CombatCalenderEntry("09/13/43", EnumScenario.Counterattack, 5, EnumResistance.Medium, "Moselle Crossing");
         theCombatCalenderEntries[49] = new CombatCalenderEntry("09/14/43", EnumScenario.Advance, 3, EnumResistance.Medium);
         theCombatCalenderEntries[50] = new CombatCalenderEntry("09/15/43", EnumScenario.Battle, 9, EnumResistance.Medium, "Crevic adn Maixe");
         theCombatCalenderEntries[51] = new CombatCalenderEntry("09/16/43", EnumScenario.Battle, 9, EnumResistance.Medium, "Luneville");
         theCombatCalenderEntries[52] = new CombatCalenderEntry("09/17/43", EnumScenario.Advance, 3, EnumResistance.Light);
         theCombatCalenderEntries[53] = new CombatCalenderEntry("09/18/43", EnumScenario.Advance, 3, EnumResistance.Light);
         theCombatCalenderEntries[54] = new CombatCalenderEntry("09/19/43", EnumScenario.Battle, 9, EnumResistance.Medium, "Arracourt");
         theCombatCalenderEntries[55] = new CombatCalenderEntry("09/20/43", EnumScenario.Advance, 3, EnumResistance.Light, "Arracourt");
         theCombatCalenderEntries[56] = new CombatCalenderEntry("09/21/43", EnumScenario.Battle, 9, EnumResistance.Medium, "Arracourt");
         theCombatCalenderEntries[57] = new CombatCalenderEntry("09/22/43", EnumScenario.Battle, 9, EnumResistance.Medium, "Arracourt");
         theCombatCalenderEntries[58] = new CombatCalenderEntry("09/23-09/24 1943", EnumScenario.Retrofit, 10, EnumResistance.None);
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalenderEntries[59] = new CombatCalenderEntry("09/25/43", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Counter Attack");
         theCombatCalenderEntries[60] = new CombatCalenderEntry("09/26/43", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Counter Attack");
         theCombatCalenderEntries[61] = new CombatCalenderEntry("09/27/43", EnumScenario.Battle, 6, EnumResistance.Medium, "Hill 318");
         theCombatCalenderEntries[62] = new CombatCalenderEntry("09/28/43", EnumScenario.Battle, 5, EnumResistance.Medium, "Hill 318");
         theCombatCalenderEntries[63] = new CombatCalenderEntry("09/29/43", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Arracourt");
         theCombatCalenderEntries[64] = new CombatCalenderEntry("09/30/43", EnumScenario.Counterattack, 9, EnumResistance.Light);
         theCombatCalenderEntries[65] = new CombatCalenderEntry("10/01/43", EnumScenario.Counterattack, 2, EnumResistance.Light);
         theCombatCalenderEntries[66] = new CombatCalenderEntry("10/02/43", EnumScenario.Counterattack, 3, EnumResistance.Light);
         theCombatCalenderEntries[67] = new CombatCalenderEntry("10/03/43", EnumScenario.Counterattack, 3, EnumResistance.Light);
         theCombatCalenderEntries[68] = new CombatCalenderEntry("10/04/43", EnumScenario.Counterattack, 3, EnumResistance.Light);
         theCombatCalenderEntries[69] = new CombatCalenderEntry("10/05/43", EnumScenario.Counterattack, 3, EnumResistance.Light);
         theCombatCalenderEntries[70] = new CombatCalenderEntry("10/06/43", EnumScenario.Counterattack, 3, EnumResistance.Light);
         theCombatCalenderEntries[71] = new CombatCalenderEntry("10/07/43", EnumScenario.Counterattack, 3, EnumResistance.Light);
         theCombatCalenderEntries[72] = new CombatCalenderEntry("10/08/43", EnumScenario.Counterattack, 3, EnumResistance.Light);
         theCombatCalenderEntries[73] = new CombatCalenderEntry("10/09/43", EnumScenario.Counterattack, 3, EnumResistance.Light);
         theCombatCalenderEntries[74] = new CombatCalenderEntry("10/10/43", EnumScenario.Counterattack, 3, EnumResistance.Light);
         theCombatCalenderEntries[75] = new CombatCalenderEntry("10/11/43", EnumScenario.Counterattack, 3, EnumResistance.Light);
         theCombatCalenderEntries[76] = new CombatCalenderEntry("10/12-11/08 1943", EnumScenario.Retrofit, 10, EnumResistance.None);
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
         IUnitTest ut5 = new ConfigMgrUnitTest(dp, ev);
         if (true == ut5.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): ConfigMgrUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut5);
         //-----------------------------------------------------------------------------
         IUnitTest ut1 = new GameViewerCreateUnitTest(dp);
         if (true == ut1.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): GameViewerCreateUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut1);
         //-----------------------------------------------------------------------------
         IUnitTest ut2 = new TerritoryCreateUnitTest(dp, gi);
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
