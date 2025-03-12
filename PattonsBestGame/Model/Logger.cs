using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pattons_Best
{
   public enum LogEnum
   {
      LE_ERROR,
      LE_GAME_INIT,
      LE_GAME_INIT_VERSION,
      LE_GAME_END,
      LE_GAME_END_CHECK,
      LE_USER_ACTION,
      LE_NEXT_ACTION,
      LE_UNDO_COMMAND,
      LE_MOVE_STACKING,
      LE_MOVE_COUNT,
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
      LE_END_ENUM
   }

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
         //---------------------------------------------------------------------
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
         //---------------------------------------------------------------------
         SetOn(LogEnum.LE_ERROR);
         SetOn(LogEnum.LE_GAME_INIT);
         SetOn(LogEnum.LE_GAME_INIT_VERSION);
         SetOn(LogEnum.LE_USER_ACTION);
         SetOn(LogEnum.LE_NEXT_ACTION);
         //SetOn(LogEnum.LE_VIEW_TIME_TRACK);
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
               Console.WriteLine("Log(): ll=" + logLevel.ToString() + "desc=" + description + "\n" + fileException.ToString());
            }
            catch (IOException ioException)
            {
               Console.WriteLine("Log(): ll=" + logLevel.ToString() + "desc=" + description + "\n" + ioException.ToString());
            }
            catch (Exception ex)
            {
               Console.WriteLine("Log(): ll=" + logLevel.ToString() + "desc=" + description + "\n" + ex.ToString());
            }
            theMutex.ReleaseMutex();
         }
      }
   }
}
