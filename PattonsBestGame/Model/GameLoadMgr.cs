using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection.PortableExecutable;

namespace Pattons_Best
{
   internal class GameLoadMgr
   {
      public static string theGamesDirectory = "";
      public static bool theIsCheckFileExist = false;
      //--------------------------------------------------
      public static string? AssemblyDirectory
      {
         get
         {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().Location;
            UriBuilder uri = new UriBuilder(codeBase);
            string? path = Uri.UnescapeDataString(uri.Path);
            if (null == path)
            {
               Logger.Log(LogEnum.LE_ERROR, "AssemblyDirectory(): path=null for codeBase=" + codeBase);
               return null;
            }
            return Path.GetDirectoryName(path);
         }
      }
      //--------------------------------------------------
      public static IGameInstance? OpenGame()
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
            string filename = theGamesDirectory + "Checkpoint.bpg";
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(GameInstance)); // read from file
            if( null != serializer )
            {
               IGameInstance? gi = serializer.Deserialize(fileStream) as GameInstance;
               fileStream.Close();
               return gi;
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "OpenGame(): serializer=null");
               return null;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGame(): path=" + theGamesDirectory + " e =" + e.ToString());
            return null;
         }
      }
      //--------------------------------------------------
      public static bool SaveGameToFile(IGameInstance gi)
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameToFile(): path=" + theGamesDirectory + " e=" + e.ToString());
            return false;
         }
         try
         {
            string filename = theGamesDirectory + "Checkpoint.bpg";
            FileStream fileStream = File.OpenWrite(filename);
            XmlWriter writer = XmlWriter.Create (fileStream);
            //XmlSerializer serializer = new XmlSerializer(typeof(GameInstance));
            //serializer.Serialize(writer, gi);
            //writer.Close();
            theIsCheckFileExist = true;
            return true;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameToFile(): path=" + theGamesDirectory + " e =" + ex.ToString());
            Console.WriteLine(ex.ToString());
            return false;
         }
      }
      //--------------------------------------------------
      public static IGameInstance OpenGameFromFile()
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
            Directory.SetCurrentDirectory(theGamesDirectory);
         }
         catch(Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): path=" + theGamesDirectory + " e=" + e.ToString());
            return new GameInstance();
         }
         try
         {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = theGamesDirectory;
            dlg.RestoreDirectory = true;
            dlg.Filter = "Barbarin Prince Games|*.bpg";
            IGameInstance? gi = null;
            if (true == dlg.ShowDialog())
            {
               FileStream? fileStream = new FileStream(dlg.FileName, FileMode.Open);
               XmlSerializer formatter = new XmlSerializer(typeof(GameInstance));
               if (null != formatter)
               {
                  XmlReader xw = XmlReader.Create(fileStream);
                  gi = formatter.Deserialize(xw) as GameInstance;
                  if( null == gi )
                     return new GameInstance();
                  Logger.Log(LogEnum.LE_GAME_INIT, "OpenGameFromFile(): gi=" + gi.ToString());
                  fileStream.Close();
                  string? gamesDirName = Path.GetDirectoryName(dlg.FileName);
                  if( null == gamesDirName )
                     Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): gamesDirName=null");
                  else
                     theGamesDirectory = gamesDirName; // save off the directory user chosen
                  theGamesDirectory += "\\";
                  if (null == AssemblyDirectory)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): AssemblyDirectory=null");
                     return new GameInstance();
                  }
                  Directory.SetCurrentDirectory(AssemblyDirectory);
               }
               if (null == gi)
                  return new GameInstance();
               return gi;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): path=" + theGamesDirectory + " e =" + e.ToString());
         }
         string? path = AssemblyDirectory;
         if( null == path )
            Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): AssemblyDirectory=null");
         else
            Directory.SetCurrentDirectory(path);
         Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): Reached default - Returning new GameInstance()");
         return new GameInstance();
      }
      //--------------------------------------------------
      public static bool SaveGameAsToFile(IGameInstance gi)
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): path=" + theGamesDirectory + " e=" + e.ToString());
            return false;
         }
         try
         {
            SaveFileDialog dlg = new SaveFileDialog();
            string filename = GetFileName(gi);
            dlg.FileName = filename;
            dlg.InitialDirectory = theGamesDirectory;
            dlg.RestoreDirectory = true;
            if (true == dlg.ShowDialog())
            {
               FileStream fileStream = File.OpenWrite(theGamesDirectory + filename);
               XmlWriter writer = XmlWriter.Create(fileStream);
               XmlSerializer serializer = new XmlSerializer(typeof(GameInstance)); // write to file
               serializer.Serialize(writer, gi);
               writer.Close();
               string? gamesDir = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               if( null == gamesDir )
               {
                  Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): gamesDir=null filename=" + filename);
                  Console.WriteLine("SaveGameAsToFile(): gamesDir=null filename=" + filename);
                  return false;
               }
               else
               {
                  theGamesDirectory = gamesDir;
                  theGamesDirectory += "\\";
               }

            }
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): path=" + theGamesDirectory + " e =" + ex.ToString());
            Console.WriteLine(ex.ToString());
            return false;
         }
         return true;
      }
      //--------------------------------------------------
      private static string GetFileName(IGameInstance gi)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append(DateTime.Now.ToString("yyyyMMdd-HHmmss"));
         sb.Append(".pbg");
         return sb.ToString();
      }
   }
}
