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
      public static bool SetInitial()
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
         string retValue = theSurnames[0];
         for (int i = 0; i < theProbabilities.Count; ++i)
         {
            if (randomNum < theProbabilities[i])
               return retValue;
            retValue = theSurnames[i];
         }
         return theSurnames[999];
      }
   }
}
