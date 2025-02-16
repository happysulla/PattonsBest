using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Pattons_Best
{
   public class GameEngine : IGameEngine
   {
      public static List<CombatCalenderEntry> theCombatCalenderEntries = new List<CombatCalenderEntry>();
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
         CreateTables();
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
      //--------------------------------------------------------------
      private void CreateTables()
      {
         theCombatCalenderEntries.Add( new CombatCalenderEntry("07/27/44", EnumScenario.Advance, 3, EnumResistance.Light, "Corba Breakout"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("07/28/44", EnumScenario.Battle, 4, EnumResistance.Medium, "Coutances"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("07/29/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("07/30/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Avranches"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/01/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/02/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/03/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/01/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/02/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/03/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/01/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/02/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/03/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/01/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/02/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/03/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/04/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/05/44", EnumScenario.Advance, 4, EnumResistance.Heavy, "Vannes"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/06/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/07/44", EnumScenario.Advance, 4, EnumResistance.Heavy, "Lorient"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/08/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/09/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/10/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/11/44", EnumScenario.Advance, 4, EnumResistance.Light, "Nantes"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/12/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/13/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/14/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/15/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/16/44", EnumScenario.Advance, 3, EnumResistance.Medium, "Orleans"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/17/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/18/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/19/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/20/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/21/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/22/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/23/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/24/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/25/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/26/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/27/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/28/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/29/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/30/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("08/31/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Commery"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/01/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/02-09/10 1943", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/11/44", EnumScenario.Advance, 5, EnumResistance.Heavy, "Moselle Crossing"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/12/44", EnumScenario.Counterattack, 5, EnumResistance.Medium, "Moselle Crossing"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/13/44", EnumScenario.Counterattack, 5, EnumResistance.Medium, "Moselle Crossing"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/14/44", EnumScenario.Advance, 3, EnumResistance.Medium));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/15/44", EnumScenario.Battle, 9, EnumResistance.Medium, "Crevic adn Maixe"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/16/44", EnumScenario.Battle, 9, EnumResistance.Medium, "Luneville"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/17/44", EnumScenario.Advance, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/18/44", EnumScenario.Advance, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/19/44", EnumScenario.Battle, 9, EnumResistance.Medium, "Arracourt"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/20/44", EnumScenario.Advance, 3, EnumResistance.Light, "Arracourt"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/21/44", EnumScenario.Battle, 9, EnumResistance.Medium, "Arracourt"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/22/44", EnumScenario.Battle, 9, EnumResistance.Medium, "Arracourt"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/23-09/24 1943", EnumScenario.Retrofit, 10, EnumResistance.None));
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/25/44", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Counter Attack"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/26/44", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Counter Attack"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/27/44", EnumScenario.Battle, 6, EnumResistance.Medium, "Hill 318"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/28/44", EnumScenario.Battle, 5, EnumResistance.Medium, "Hill 318"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/29/44", EnumScenario.Counterattack, 9, EnumResistance.Heavy, "Arracourt"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("09/30/44", EnumScenario.Counterattack, 9, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/01/44", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/02/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/03/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/04/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/05/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/06/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/07/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/08/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/09/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/10/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/11/44", EnumScenario.Counterattack, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("10/12-11/08 1943", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/09/44", EnumScenario.Battle, 5, EnumResistance.Medium));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/10/44", EnumScenario.Advance, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/11/44", EnumScenario.Advance, 5, EnumResistance.Medium, "Fonteny"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/12/44", EnumScenario.Counterattack, 8, EnumResistance.Heavy, "Counterattack at Rodable"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/13/44", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/14/44", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/15/44", EnumScenario.Advance, 4, EnumResistance.Heavy));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/16/44", EnumScenario.Advance, 4, EnumResistance.Medium));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/17/44", EnumScenario.Advance, 4, EnumResistance.Medium));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/18/44", EnumScenario.Advance, 4, EnumResistance.Heavy, "Dieuze and Rodable"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/19/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/20/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/21/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/22/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/23/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/24/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Crossed Saare River at Romeifling"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/25/44", EnumScenario.Counterattack, 7, EnumResistance.Medium, "Counterattacks"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/26/44", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/27/44", EnumScenario.Advance, 3, EnumResistance.Medium, "Wolfskirchen"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/28/44", EnumScenario.Advance, 2, EnumResistance.Light, "Cleared zone of responsibility"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/29/44", EnumScenario.Advance, 2, EnumResistance.Light, "Cleared zone of responsibility"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("11/30/44", EnumScenario.Advance, 2, EnumResistance.Light, "Cleared zone of responsibility"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/01/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Attacked Saare Union"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/02/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Attacked Saare Union"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/03/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/04/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/05/44", EnumScenario.Battle, 9, EnumResistance.Heavy, "Battle of Bining"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/06/44", EnumScenario.Battle, 9, EnumResistance.Heavy, "Battle of Bining"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/07-12/20 1943", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/21/44", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/22/44", EnumScenario.Advance, 2, EnumResistance.Light, "Martelange"));
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/23/44", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/24/44", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/25/44", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/26/44", EnumScenario.Advance, 9, EnumResistance.Heavy, "Into Bastogne"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/27/44", EnumScenario.Advance, 4, EnumResistance.Medium));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/28/44", EnumScenario.Advance, 4, EnumResistance.Medium));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/29/44", EnumScenario.Advance, 4, EnumResistance.Medium, "Open Arion-Bastogne Highway"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/30/44", EnumScenario.Counterattack, 3, EnumResistance.Medium));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("12/31/44", EnumScenario.Counterattack, 3, EnumResistance.Medium));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/01/45", EnumScenario.Counterattack, 3, EnumResistance.Medium));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/02/45", EnumScenario.Counterattack, 3, EnumResistance.Medium));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/03-01/08 1944", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/09/45", EnumScenario.Advance, 2, EnumResistance.Light, "Noville"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/10/45", EnumScenario.Advance, 2, EnumResistance.Light, "Bourcy"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/11/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/12/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/13/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/14/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/15/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/16/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/17/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/18/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/19/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/20/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/21/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/22/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/23/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/24/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/25/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/26/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/27/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/28/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/29/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/30/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("01/31/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("02/01/45", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("02/02/45", EnumScenario.Advance, 2, EnumResistance.Light, "Hosdorf"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("02/03-02/21 1944", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("02/22/45", EnumScenario.Battle, 8, EnumResistance.Medium, "Geichlingen"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("02/23/45", EnumScenario.Battle, 6, EnumResistance.Medium, "Sinspenit"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("02/24/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("02/25/45", EnumScenario.Battle, 5, EnumResistance.Medium, "Rittersdorf"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("02/26/45", EnumScenario.Battle, 5, EnumResistance.Medium, "Bitburg"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("02/27/45", EnumScenario.Battle, 5, EnumResistance.Medium, "Matzen and Fleissen"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("02/28/45", EnumScenario.Counterattack, 2, EnumResistance.Medium));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/01/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/02/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/03-03/04 1944", EnumScenario.Retrofit, 10, EnumResistance.None));
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/05/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/06/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/07/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/08/45", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/09/45", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/10/45", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/11/45", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/12-03/13 1944", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/14/45", EnumScenario.Advance, 9, EnumResistance.Medium, "Attack out of Moselle Bridgehead"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/15/45", EnumScenario.Advance, 7, EnumResistance.Medium, "Bad Kreuznauch"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/16/45", EnumScenario.Advance, 5, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/17/45", EnumScenario.Advance, 3, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/18/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/19/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/20/45", EnumScenario.Advance, 2, EnumResistance.Light, "Worms on the Rhine"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/21-03/23 1944", EnumScenario.Retrofit, 10, EnumResistance.None));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/24/45", EnumScenario.Advance, 2, EnumResistance.Light, "Cross the Rhine"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/25/45", EnumScenario.Advance, 2, EnumResistance.Light, "Hanau adn Darmstadt"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/26/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/27/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/28/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/29/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/30/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("03/31/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/01/45", EnumScenario.Advance, 4, EnumResistance.Light, "Crezburg"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/02/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/03/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/04/45", EnumScenario.Advance, 4, EnumResistance.Light, "Gotha"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/05/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/06/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/07/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/08/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/09/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/10/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/11/45", EnumScenario.Advance, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/12/45", EnumScenario.Advance, 2, EnumResistance.Light, "Crossed Saale River"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/13/45", EnumScenario.Advance, 2, EnumResistance.Light, "Wolkenburg"));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/14/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/15/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/16/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/17/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("04/18/45", EnumScenario.Counterattack, 2, EnumResistance.Light));
         theCombatCalenderEntries.Add(new CombatCalenderEntry("Drive into Czechoslavakia", EnumScenario.Retrofit, 10, EnumResistance.None));
         //--------------------------------------------------------------------
      }
   }
}
