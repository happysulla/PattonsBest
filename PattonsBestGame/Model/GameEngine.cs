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
         theCombatCalenderEntries[77] = new CombatCalenderEntry("11/09/43", EnumScenario.Battle, 5, EnumResistance.Medium);
         theCombatCalenderEntries[78] = new CombatCalenderEntry("11/10/43", EnumScenario.Advance, 3, EnumResistance.Light);
         theCombatCalenderEntries[79] = new CombatCalenderEntry("11/11/43", EnumScenario.Advance, 5, EnumResistance.Medium, "Fonteny");
         theCombatCalenderEntries[80] = new CombatCalenderEntry("11/12/43", EnumScenario.Counterattack, 8, EnumResistance.Heavy, "Counterattack at Rodable");
         theCombatCalenderEntries[81] = new CombatCalenderEntry("11/13/43", EnumScenario.Counterattack, 2, EnumResistance.Light);
         theCombatCalenderEntries[82] = new CombatCalenderEntry("11/14/43", EnumScenario.Counterattack, 2, EnumResistance.Light);
         theCombatCalenderEntries[83] = new CombatCalenderEntry("11/15/43", EnumScenario.Advance, 4, EnumResistance.Heavy);
         theCombatCalenderEntries[84] = new CombatCalenderEntry("11/16/43", EnumScenario.Advance, 4, EnumResistance.Medium);
         theCombatCalenderEntries[85] = new CombatCalenderEntry("11/17/43", EnumScenario.Advance, 4, EnumResistance.Medium);
         theCombatCalenderEntries[86] = new CombatCalenderEntry("11/18/43", EnumScenario.Advance, 4, EnumResistance.Heavy, "Dieuze and Rodable");
         theCombatCalenderEntries[87] = new CombatCalenderEntry("11/19/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[88] = new CombatCalenderEntry("11/20/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[89] = new CombatCalenderEntry("11/21/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[91] = new CombatCalenderEntry("11/22/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[92] = new CombatCalenderEntry("11/23/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[93] = new CombatCalenderEntry("11/24/43", EnumScenario.Advance, 4, EnumResistance.Medium, "Crossed Saare River at Romeifling");
         theCombatCalenderEntries[94] = new CombatCalenderEntry("11/25/43", EnumScenario.Counterattack, 7, EnumResistance.Medium, "Counterattacks");
         theCombatCalenderEntries[95] = new CombatCalenderEntry("11/26/43", EnumScenario.Counterattack, 2, EnumResistance.Light);
         theCombatCalenderEntries[96] = new CombatCalenderEntry("11/27/43", EnumScenario.Advance, 3, EnumResistance.Medium, "Wolfskirchen");
         theCombatCalenderEntries[97] = new CombatCalenderEntry("11/28/43", EnumScenario.Advance, 2, EnumResistance.Light, "Cleared zone of responsibility");
         theCombatCalenderEntries[98] = new CombatCalenderEntry("11/29/43", EnumScenario.Advance, 2, EnumResistance.Light, "Cleared zone of responsibility");
         theCombatCalenderEntries[99] = new CombatCalenderEntry("11/30/43", EnumScenario.Advance, 2, EnumResistance.Light, "Cleared zone of responsibility");
         theCombatCalenderEntries[100] = new CombatCalenderEntry("12/01/43", EnumScenario.Advance, 4, EnumResistance.Medium, "Attacked Saare Union");
         theCombatCalenderEntries[101] = new CombatCalenderEntry("12/02/43", EnumScenario.Advance, 4, EnumResistance.Medium, "Attacked Saare Union");
         theCombatCalenderEntries[102] = new CombatCalenderEntry("12/03/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[103] = new CombatCalenderEntry("12/04/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[104] = new CombatCalenderEntry("12/05/43", EnumScenario.Battle, 9, EnumResistance.Heavy, "Battle of Bining");
         theCombatCalenderEntries[105] = new CombatCalenderEntry("12/06/43", EnumScenario.Battle, 9, EnumResistance.Heavy, "Battle of Bining");
         theCombatCalenderEntries[106] = new CombatCalenderEntry("12/07-12/20 1943", EnumScenario.Retrofit, 10, EnumResistance.None);
         theCombatCalenderEntries[107] = new CombatCalenderEntry("12/21/43", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[108] = new CombatCalenderEntry("12/22/43", EnumScenario.Advance, 2, EnumResistance.Light, "Martelange");
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalenderEntries[109] = new CombatCalenderEntry("12/23/43", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont");
         theCombatCalenderEntries[110] = new CombatCalenderEntry("12/24/43", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont");
         theCombatCalenderEntries[111] = new CombatCalenderEntry("12/25/43", EnumScenario.Advance, 7, EnumResistance.Heavy, "Battle for Chaumont");
         theCombatCalenderEntries[112] = new CombatCalenderEntry("12/26/43", EnumScenario.Advance, 9, EnumResistance.Heavy, "Into Bastogne");
         theCombatCalenderEntries[113] = new CombatCalenderEntry("12/27/43", EnumScenario.Advance, 4, EnumResistance.Medium);
         theCombatCalenderEntries[113] = new CombatCalenderEntry("12/28/43", EnumScenario.Advance, 4, EnumResistance.Medium);
         theCombatCalenderEntries[114] = new CombatCalenderEntry("12/29/43", EnumScenario.Advance, 4, EnumResistance.Medium, "Open Arion-Bastogne Highway");
         theCombatCalenderEntries[115] = new CombatCalenderEntry("12/30/43", EnumScenario.Counterattack, 3, EnumResistance.Medium);
         theCombatCalenderEntries[116] = new CombatCalenderEntry("12/31/43", EnumScenario.Counterattack, 3, EnumResistance.Medium);
         theCombatCalenderEntries[117] = new CombatCalenderEntry("01/01/44", EnumScenario.Counterattack, 3, EnumResistance.Medium);
         theCombatCalenderEntries[118] = new CombatCalenderEntry("01/02/44", EnumScenario.Counterattack, 3, EnumResistance.Medium);
         theCombatCalenderEntries[119] = new CombatCalenderEntry("01/03-01/08 1944", EnumScenario.Retrofit, 10, EnumResistance.None);
         theCombatCalenderEntries[120] = new CombatCalenderEntry("01/09/44", EnumScenario.Advance, 2, EnumResistance.Light, "Noville");
         theCombatCalenderEntries[121] = new CombatCalenderEntry("01/10/44", EnumScenario.Advance, 2, EnumResistance.Light, "Bourcy");
         theCombatCalenderEntries[122] = new CombatCalenderEntry("01/11/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[123] = new CombatCalenderEntry("01/12/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[124] = new CombatCalenderEntry("01/13/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[125] = new CombatCalenderEntry("01/14/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[126] = new CombatCalenderEntry("01/15/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[127] = new CombatCalenderEntry("01/16/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[128] = new CombatCalenderEntry("01/17/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[129] = new CombatCalenderEntry("01/18/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[130] = new CombatCalenderEntry("01/19/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[131] = new CombatCalenderEntry("01/20/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[132] = new CombatCalenderEntry("01/21/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[133] = new CombatCalenderEntry("01/22/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[134] = new CombatCalenderEntry("01/23/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[135] = new CombatCalenderEntry("01/24/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[136] = new CombatCalenderEntry("01/25/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[137] = new CombatCalenderEntry("01/26/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[138] = new CombatCalenderEntry("01/27/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[139] = new CombatCalenderEntry("01/28/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[140] = new CombatCalenderEntry("01/29/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[141] = new CombatCalenderEntry("01/30/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[142] = new CombatCalenderEntry("01/31/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[143] = new CombatCalenderEntry("02/01/44", EnumScenario.Counterattack, 2, EnumResistance.Light, "Defensive positions");
         theCombatCalenderEntries[144] = new CombatCalenderEntry("02/02/44", EnumScenario.Advance, 2, EnumResistance.Light, "Hosdorf");
         theCombatCalenderEntries[145] = new CombatCalenderEntry("02/03-02/21 1944", EnumScenario.Retrofit, 10, EnumResistance.None);
         theCombatCalenderEntries[146] = new CombatCalenderEntry("02/22/44", EnumScenario.Battle, 8, EnumResistance.Medium, "Geichlingen");
         theCombatCalenderEntries[147] = new CombatCalenderEntry("02/23/44", EnumScenario.Battle, 6, EnumResistance.Medium, "Sinspenit");
         theCombatCalenderEntries[148] = new CombatCalenderEntry("02/24/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[149] = new CombatCalenderEntry("02/25/44", EnumScenario.Battle, 5, EnumResistance.Medium, "Rittersdorf");
         theCombatCalenderEntries[150] = new CombatCalenderEntry("02/26/44", EnumScenario.Battle, 5, EnumResistance.Medium, "Bitburg");
         theCombatCalenderEntries[151] = new CombatCalenderEntry("02/27/44", EnumScenario.Battle, 5, EnumResistance.Medium, "Matzen and Fleissen");
         theCombatCalenderEntries[152] = new CombatCalenderEntry("02/28/44", EnumScenario.Counterattack, 2, EnumResistance.Medium);
         theCombatCalenderEntries[152] = new CombatCalenderEntry("03/01/44", EnumScenario.Counterattack, 2, EnumResistance.Light);
         theCombatCalenderEntries[152] = new CombatCalenderEntry("03/02/44", EnumScenario.Counterattack, 2, EnumResistance.Light);
         theCombatCalenderEntries[153] = new CombatCalenderEntry("03/03-03/04 1944", EnumScenario.Retrofit, 10, EnumResistance.None);
         //---------------------------------------------------------------------------------------------------------------------
         theCombatCalenderEntries[154] = new CombatCalenderEntry("03/05/44", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine");
         theCombatCalenderEntries[155] = new CombatCalenderEntry("03/06/44", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine");
         theCombatCalenderEntries[156] = new CombatCalenderEntry("03/07/44", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine");
         theCombatCalenderEntries[157] = new CombatCalenderEntry("03/08/44", EnumScenario.Advance, 2, EnumResistance.Light, "To the Rhine");
         theCombatCalenderEntries[158] = new CombatCalenderEntry("03/09/44", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up");
         theCombatCalenderEntries[159] = new CombatCalenderEntry("03/10/44", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up");
         theCombatCalenderEntries[160] = new CombatCalenderEntry("03/11/44", EnumScenario.Advance, 2, EnumResistance.Light, "Regroup and mop up");
         theCombatCalenderEntries[161] = new CombatCalenderEntry("03/12-03/13 1944", EnumScenario.Retrofit, 10, EnumResistance.None);
         theCombatCalenderEntries[162] = new CombatCalenderEntry("03/14/44", EnumScenario.Advance, 9, EnumResistance.Medium, "Attack out of Moselle Bridgehead");
         theCombatCalenderEntries[163] = new CombatCalenderEntry("03/15/44", EnumScenario.Advance, 7, EnumResistance.Medium, "Bad Kreuznauch");
         theCombatCalenderEntries[164] = new CombatCalenderEntry("03/16/44", EnumScenario.Advance, 5, EnumResistance.Light);
         theCombatCalenderEntries[165] = new CombatCalenderEntry("03/17/44", EnumScenario.Advance, 3, EnumResistance.Light);
         theCombatCalenderEntries[166] = new CombatCalenderEntry("03/18/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[167] = new CombatCalenderEntry("03/19/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[168] = new CombatCalenderEntry("03/20/44", EnumScenario.Advance, 2, EnumResistance.Light, "Worms on the Rhine");
         theCombatCalenderEntries[169] = new CombatCalenderEntry("03/21-03/23 1944", EnumScenario.Retrofit, 10, EnumResistance.None);
         theCombatCalenderEntries[170] = new CombatCalenderEntry("03/24/44", EnumScenario.Advance, 2, EnumResistance.Light, "Cross the Rhine");
         theCombatCalenderEntries[171] = new CombatCalenderEntry("03/25/44", EnumScenario.Advance, 2, EnumResistance.Light, "Hanau adn Darmstadt");
         theCombatCalenderEntries[172] = new CombatCalenderEntry("03/26/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[173] = new CombatCalenderEntry("03/27/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[174] = new CombatCalenderEntry("03/28/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[175] = new CombatCalenderEntry("03/29/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[176] = new CombatCalenderEntry("03/30/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[177] = new CombatCalenderEntry("03/31/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[178] = new CombatCalenderEntry("04/01/44", EnumScenario.Advance, 4, EnumResistance.Light, "Crezburg");
         theCombatCalenderEntries[179] = new CombatCalenderEntry("04/02/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[180] = new CombatCalenderEntry("04/03/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[181] = new CombatCalenderEntry("04/04/44", EnumScenario.Advance, 4, EnumResistance.Light, "Gotha");
         theCombatCalenderEntries[182] = new CombatCalenderEntry("04/05/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[183] = new CombatCalenderEntry("04/06/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[184] = new CombatCalenderEntry("04/07/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[185] = new CombatCalenderEntry("04/08/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[186] = new CombatCalenderEntry("04/09/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[187] = new CombatCalenderEntry("04/10/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[188] = new CombatCalenderEntry("04/11/44", EnumScenario.Advance, 2, EnumResistance.Light);
         theCombatCalenderEntries[189] = new CombatCalenderEntry("04/12/44", EnumScenario.Advance, 2, EnumResistance.Light, "Crossed Saale River");
         theCombatCalenderEntries[190] = new CombatCalenderEntry("04/13/44", EnumScenario.Advance, 2, EnumResistance.Light, "Wolkenburg");
         theCombatCalenderEntries[191] = new CombatCalenderEntry("04/14/44", EnumScenario.Counterattack, 2, EnumResistance.Light);
         theCombatCalenderEntries[192] = new CombatCalenderEntry("04/15/44", EnumScenario.Counterattack, 2, EnumResistance.Light);
         theCombatCalenderEntries[193] = new CombatCalenderEntry("04/16/44", EnumScenario.Counterattack, 2, EnumResistance.Light);
         theCombatCalenderEntries[194] = new CombatCalenderEntry("04/17/44", EnumScenario.Counterattack, 2, EnumResistance.Light);
         theCombatCalenderEntries[195] = new CombatCalenderEntry("04/18/44", EnumScenario.Counterattack, 2, EnumResistance.Light);
         theCombatCalenderEntries[196] = new CombatCalenderEntry("Drive into Czechoslavakia", EnumScenario.Retrofit, 10, EnumResistance.None);
      }
   }
}
