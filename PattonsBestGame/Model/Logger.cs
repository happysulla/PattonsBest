using System;
using System.IO;
using System.Threading;

namespace Pattons_Best
{
   public enum LogEnum
   {
      LE_ERROR,
      LE_GAME_INIT,
      LE_GAME_INIT_VERSION,
      LE_GAME_END,
      LE_GAME_END_CHECK,
      LE_NEXT_ACTION,
      LE_UNDO_COMMAND,
      LE_MOVE_STACKING,
      LE_MOVE_COUNT,
      LE_SHOW_STACK_ADD,
      LE_SHOW_STACK_DEL,
      LE_SHOW_STACK_VIEW,
      LE_SHOW_MAIN_CLEAR,
      LE_SHOW_ENTERED_HEX,
      LE_SHOW_BUTTON_MOVE,
      //-------------
      LE_VIEW_SHOW_OPTIONS,
      LE_VIEW_SHOW_FEATS,
      LE_VIEW_SHOW_STATS,
      LE_VIEW_SHOW_SETTINGS,
      //-------------
      LE_SHOW_CREW_NAME,
      LE_SHOW_CREW_BU,
      //-------------
      LE_SHOW_ROUND_COMBAT,
      LE_SHOW_START_AREA,
      LE_SHOW_ACTION_REPORT_NEW,
      LE_SHOW_AUTOSETUP_BATTLEPREP,
      LE_SHOW_ENEMY_STRENGTH,
      LE_SHOW_OVERRUN_TO_PREVIOUS_AREA,
      LE_SHOW_RETREAT_TO_PREVIOUS_AREA,
      LE_SHOW_ENEMY_ON_MOVE_BOARD,
      LE_SHOW_RESISTANCE,
      LE_SHOW_GUN_LOAD_PREP,
      LE_SHOW_RANDOM_PT,
      LE_SHOW_ROTATION,
      LE_SHOW_MAPITEM_TANK,
      LE_SHOW_MAPITEM_CREWACTION,
      LE_SHOW_BATTLE_PHASE,
      LE_SHOW_START_BATTLE,
      LE_SHOW_HIT_YOU_MOD,
      LE_SHOW_WOUND_MOD,
      LE_SHOW_SPOT_MOD,
      LE_SHOW_SPOT_RESULT,
      LE_SHOW_ORDERS_MENU,
      LE_SHOW_AMMMO_MENU,
      LE_SHOW_SHERMAN_MOVE,
      LE_SHOW_APPEARING_UNITS,
      LE_SHOW_FRIENDLY_ACTION_MOD,
      LE_SHOW_TO_HIT_MODIFIER,
      LE_SHOW_TO_HIT_ATTACK,
      LE_SHOW_MAIN_GUN_BREAK,
      LE_SHOW_MG_BREAK,
      LE_SHOW_MG_FIRE,
      LE_SHOW_MG_CMDR_DIRECT_FIRE,
      LE_SHOW_TO_KILL_MODIFIER,
      LE_SHOW_TO_KILL_ATTACK_INF,
      LE_SHOW_TO_KILL_ATTACK,
      LE_SHOW_TO_KILL_MG_ATTACK,
      LE_SHOW_CONDUCT_CREW_ACTION,
      LE_SHOW_CREW_SWITCH,
      LE_SHOW_CREW_REPLACE,
      LE_SHOW_CREW_RETURN,
      LE_SHOW_BATTLE_ROUND_START,
      LE_SHOW_RESET_ROUND,
      LE_SHOW_TANK_BUTTONS,
      LE_SHOW_NUM_SHERMAN_SHOTS,
      LE_SHOW_NUM_ENEMY_SHOTS,
      LE_SHOW_GUN_RELOAD,
      LE_SHOW_FIRE_DIRECTION,
      LE_SHOW_IMMOBILIZATION,
      LE_SHOW_PROMOTION,
      //-------------
      LE_SHOW_TIME_ADVANCE,
      LE_SHOW_TIME_GAME,
      LE_SHOW_VP_FRIENDLY_FORCES,
      LE_SHOW_VP_CAPTURED_AREA,
      LE_SHOW_VP_TOTAL,
      //-------------
      LE_VIEW_ADV_FIRE_RESOLVE,
      LE_VIEW_ART_FIRE_RESOLVE,
      LE_VIEW_AIR_FIRE_RESOLVE,
      LE_VIEW_ROTATION,
      //-------------
      LE_RESET_ROLL_STATE,
      LE_VIEW_DICE_MOVING,
      LE_VIEW_UPDATE_MENU,
      LE_VIEW_UPDATE_STATUS_BAR,
      LE_VIEW_UPDATE_EVENTVIEWER,
      LE_VIEW_APPEND_EVENT,
      LE_VIEW_MIM,
      LE_VIEW_MIM_ADD,
      LE_VIEW_MIM_CLEAR,
      LE_VIEW_TIME_TRACK,
      LE_VIEW_CONTROL_NAME,
      //-------------
      LE_EVENT_VIEWER_BATTLE_SETUP,
      LE_EVENT_VIEWER_ENEMY_ACTION,
      LE_EVENT_VIEWER_TANK_DESTROYED,
      LE_EVENT_VIEWER_TANK_DESTROYED_BAILOUT,
      LE_EVENT_VIEWER_RANDOM_EVENT,
      LE_EVENT_VIEWER_SPOTTING,
      LE_EVENT_VIEWER_ORDERS,
      LE_END_ENUM
   }
   //----------------------------------------------------------------------------
   public class Logger
   {
      const int NUM_LOG_LEVELS = (int)LogEnum.LE_END_ENUM;
      public static bool[] theLogLevel = new bool[NUM_LOG_LEVELS];
      public static string theLogDirectory = "";
      private static string theFileName = "";
      private static bool theIsLogFileCreated = false;
      private static Mutex theMutex = new Mutex();
      //--------------------------------------------------
      static public bool SetInitial()
      {
         try // create the file
         {
            if (false == Directory.Exists(theLogDirectory))
               Directory.CreateDirectory(theLogDirectory);
            theFileName = theLogDirectory + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
            FileInfo f = new FileInfo(theFileName);
            f.Create();
            theIsLogFileCreated = true;
         }
         catch (DirectoryNotFoundException dirException)
         {
            Console.WriteLine("SetInitial(): create file\n" + dirException.ToString());
         }
         catch (FileNotFoundException fileException)
         {
            Console.WriteLine("SetInitial(): create file\n" + fileException.ToString());
         }
         catch (IOException ioException)
         {
            Console.WriteLine("SetInitial(): create file\n" + ioException.ToString());
         }
         catch (Exception ex)
         {
            Console.WriteLine("SetInitial(): create file\n" + ex.ToString());
         }
         SetOn(LogEnum.LE_ERROR);
         //SetOn(LogEnum.LE_GAME_INIT);
         //SetOn(LogEnum.LE_GAME_INIT_VERSION);
         SetOn(LogEnum.LE_NEXT_ACTION);
         SetOn(LogEnum.LE_GAME_END);
         SetOn(LogEnum.LE_GAME_END_CHECK);
         //SetOn(LogEnum.LE_SHOW_STACK_ADD);
         //SetOn(LogEnum.LE_SHOW_STACK_DEL);
         //SetOn(LogEnum.LE_SHOW_STACK_VIEW);
         //SetOn(LogEnum.LE_SHOW_MAIN_CLEAR);
         //SetOn(LogEnum.LE_SHOW_ENTERED_HEX);
         //SetOn(LogEnum.LE_SHOW_BUTTON_MOVE);
         //-------------------------------
         //SetOn(LogEnum.LE_VIEW_SHOW_OPTIONS);
         //SetOn(LogEnum.LE_VIEW_SHOW_FEATS);
         //SetOn(LogEnum.LE_VIEW_SHOW_STATS);
         //SetOn(LogEnum.LE_VIEW_SHOW_SETTINGS);
         //-------------------------------
         //SetOn(LogEnum.LE_SHOW_ROUND_COMBAT);
         //SetOn(LogEnum.LE_SHOW_CREW_NAME);
         //SetOn(LogEnum.LE_SHOW_CREW_BU);
         //-------------------------------
         //SetOn(LogEnum.LE_SHOW_ACTION_REPORT_NEW);
         //SetOn(LogEnum.LE_SHOW_AUTOSETUP_BATTLEPREP);
         SetOn(LogEnum.LE_SHOW_START_AREA);
         //SetOn(LogEnum.LE_SHOW_ENEMY_STRENGTH);
         SetOn(LogEnum.LE_SHOW_OVERRUN_TO_PREVIOUS_AREA);
         SetOn(LogEnum.LE_SHOW_RETREAT_TO_PREVIOUS_AREA);
         //SetOn(LogEnum.LE_SHOW_ENEMY_ON_MOVE_BOARD);
         //SetOn(LogEnum.LE_SHOW_RESISTANCE);
         //SetOn(LogEnum.LE_SHOW_RANDOM_PT);
         //SetOn(LogEnum.LE_SHOW_ROTATION);
         //SetOn(LogEnum.LE_SHOW_GUN_LOAD_PREP);
         //SetOn(LogEnum.LE_SHOW_MAPITEM_TANK);
         //SetOn(LogEnum.LE_SHOW_MAPITEM_CREWACTION);
         SetOn(LogEnum.LE_SHOW_BATTLE_PHASE);
         //SetOn(LogEnum.LE_SHOW_START_BATTLE);
         //SetOn(LogEnum.LE_SHOW_BATTLE_ROUND_START);
         //SetOn(LogEnum.LE_SHOW_RESET_ROUND);
         //SetOn(LogEnum.LE_SHOW_HIT_YOU_MOD);
         //SetOn(LogEnum.LE_SHOW_WOUND_MOD);
         //SetOn(LogEnum.LE_SHOW_SPOT_MOD);
         //SetOn(LogEnum.LE_SHOW_SPOT_RESULT);
         //SetOn(LogEnum.LE_SHOW_ORDERS_MENU);
         //SetOn(LogEnum.LE_SHOW_CREW_SWITCH);
         //SetOn(LogEnum.LE_SHOW_CREW_REPLACE);
         SetOn(LogEnum.LE_SHOW_CREW_RETURN);
         //SetOn(LogEnum.LE_SHOW_AMMMO_MENU);
         //SetOn(LogEnum.LE_SHOW_TANK_BUTTONS);
         SetOn(LogEnum.LE_SHOW_GUN_RELOAD);
         SetOn(LogEnum.LE_SHOW_PROMOTION);
         //-------------------------------
         //SetOn(LogEnum.LE_SHOW_CONDUCT_CREW_ACTION);
         //SetOn(LogEnum.LE_SHOW_SHERMAN_MOVE); 
         //SetOn(LogEnum.LE_SHOW_APPEARING_UNITS);
         //SetOn(LogEnum.LE_SHOW_FRIENDLY_ACTION_MOD);
         //-------------------------------
         SetOn(LogEnum.LE_SHOW_TO_HIT_MODIFIER);
         SetOn(LogEnum.LE_SHOW_TO_HIT_ATTACK);
         SetOn(LogEnum.LE_SHOW_MAIN_GUN_BREAK);
         SetOn(LogEnum.LE_SHOW_MG_BREAK);
         //SetOn(LogEnum.LE_SHOW_MG_FIRE);
         //SetOn(LogEnum.LE_SHOW_MG_CMDR_DIRECT_FIRE);
         SetOn(LogEnum.LE_SHOW_TO_KILL_MODIFIER);
         SetOn(LogEnum.LE_SHOW_TO_KILL_ATTACK_INF);
         SetOn(LogEnum.LE_SHOW_TO_KILL_ATTACK);
         //SetOn(LogEnum.LE_SHOW_TO_KILL_MG_ATTACK);
         SetOn(LogEnum.LE_SHOW_NUM_SHERMAN_SHOTS);
         //SetOn(LogEnum.LE_SHOW_NUM_ENEMY_SHOTS);
         SetOn(LogEnum.LE_SHOW_FIRE_DIRECTION);
         SetOn(LogEnum.LE_SHOW_IMMOBILIZATION);
         //-------------------------------
         SetOn(LogEnum.LE_SHOW_TIME_ADVANCE);
         //SetOn(LogEnum.LE_SHOW_TIME_GAME);
         //SetOn(LogEnum.LE_SHOW_VP_FRIENDLY_FORCES);
         //SetOn(LogEnum.LE_SHOW_VP_CAPTURED_AREA);
         //SetOn(LogEnum.LE_SHOW_VP_TOTAL);
         //-------------------------------
         //SetOn(LogEnum.LE_VIEW_ADV_FIRE_RESOLVE);
         //SetOn(LogEnum.LE_VIEW_ART_FIRE_RESOLVE);
         //SetOn(LogEnum.LE_VIEW_AIR_FIRE_RESOLVE);
         //SetOn(LogEnum.LE_VIEW_ROTATION);
         //-------------------------------
         SetOn(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER);
         //SetOn(LogEnum.LE_VIEW_MIM);
         //SetOn(LogEnum.LE_VIEW_MIM_ADD);
         //SetOn(LogEnum.LE_VIEW_MIM_CLEAR);
         //SetOn(LogEnum.LE_VIEW_TIME_TRACK);
         //SetOn(LogEnum.LE_VIEW_CONTROL_NAME);
         //-------------------------------
         //SetOn(LogEnum.LE_EVENT_VIEWER_BATTLE_SETUP);
         //SetOn(LogEnum.LE_EVENT_VIEWER_ENEMY_ACTION);
         //SetOn(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED);
         //SetOn(LogEnum.LE_EVENT_VIEWER_TANK_DESTROYED_BAILOUT);
         //SetOn(LogEnum.LE_EVENT_VIEWER_SPOTTING);
         //SetOn(LogEnum.LE_EVENT_VIEWER_ORDERS);
         return true;
      }
      static public void SetOn(LogEnum logLevel)
      {
         if ((int)logLevel < NUM_LOG_LEVELS)
            theLogLevel[(int)logLevel] = true;
      }
      static public void SetOff(LogEnum logLevel)
      {
         if ((int)logLevel < NUM_LOG_LEVELS)
            theLogLevel[(int)logLevel] = false;
      }
      static public void Log(LogEnum logLevel, string description)
      {
         if (true == theLogLevel[(int)logLevel])
         {
            theMutex.WaitOne();
            System.Diagnostics.Debug.WriteLine("{0} {1}", logLevel.ToString(), description);
            if (false == theIsLogFileCreated)
            {
               theMutex.ReleaseMutex();
               return;
            }
            try
            {
               FileInfo file = new FileInfo(theFileName);
               if (true == File.Exists(theFileName))
               {
                  StreamWriter swriter = File.AppendText(theFileName);
                  swriter.Write(logLevel.ToString());
                  swriter.Write(" ");
                  swriter.Write(description);
                  swriter.Write("\n");
                  swriter.Close();
               }
            }
            catch (FileNotFoundException fileException)
            {
               //System.Diagnostics.Debug.WriteLine("Log(): ll=" + logLevel.ToString() + " desc=" + description + "\n" + fileException.ToString());
            }
            catch (IOException ioException)
            {
               //System.Diagnostics.Debug.WriteLine("Log(): ll=" + logLevel.ToString() + " desc=" + description + "\n" + ioException.ToString());
            }
            catch (Exception ex)
            {
               //System.Diagnostics.Debug.WriteLine("Log(): ll=" + logLevel.ToString() + " desc=" + description + "\n" + ex.ToString());
            }
            theMutex.ReleaseMutex();
         }
      }
   }
}
