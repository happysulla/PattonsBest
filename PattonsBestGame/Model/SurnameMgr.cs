using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   public static class SurnameMgr
   {
      private static List<string> theSurnames = new List<string>();
      private static List<int> theProbabilities = new List<int>();
      public static bool SetInitial() // read in name and probability name exists
      {
         string filename = ConfigFileReader.theConfigDirectory + @"Surnames.txt";
         try
         {
            StreamReader sr = File.OpenText(filename);
            int i = 0;
            while (i < 1500)
            {
               string? line = sr.ReadLine();
               if (null == line)
                  break;
               line = line.Trim();
               if (0 < line.Length)
               {
                  string[] aStringArray = line.Split(new char[] { ',' });
                  theSurnames.Add(aStringArray[0]);
                  theProbabilities.Add(int.Parse(aStringArray[1]));
               }
            }
            return true;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "InitNames(): e=" + e.ToString());
            return false;
         }
      }
      public static string GetSurname()
      {
         int randomNum = Utilities.RandomGenerator.Next(1000000);
         //int randomNum = Utilities.RandomGenerator.Next(8000);
         string retValue = theSurnames[0];
         for (int i = 0; i < theProbabilities.Count; ++i) // step through probablities - find when the random number is less than probablility for that number. In that event, return that name.
         {
            if (randomNum < theProbabilities[i])
               return retValue;
            retValue = theSurnames[i];
         }
         return theSurnames[999];
      }
      public static bool AppendGenerationalSuffix(IGameInstance gi, ICrewMember cmOriginal)
      {
         string[] crewmembers = new string[5] { "Commander", "Gunner", "Loader", "Driver", "Assistant" }; 
         foreach (string role1 in crewmembers)
         {
            ICrewMember? cmCompare = gi.GetCrewMemberByRole(role1);
            if (null == cmCompare)
            {
               Logger.Log(LogEnum.LE_ERROR, "AppendGenerationalSuffix(): returned error with cm=null for role=" + role1);
               return false;
            }
            if (role1 == cmOriginal.Role)
               continue;
            if (cmOriginal.Name == cmCompare.Name)
            {
               if (true == cmCompare.Name.Contains(" Jr"))
               {
                  char[] charsToTrim = { 'J', 'r' };
                  cmOriginal.Name = cmCompare.Name.TrimEnd(charsToTrim);
                  cmOriginal.Name += "II";
               }
               else if (true == cmCompare.Name.Contains(" I"))
               {
                  cmOriginal.Name += "I";
               }
               else
               {
                  cmOriginal.Name += " Jr";
               }
               break;
            }
         }
         return true;
      }
   }
}
