﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;

namespace Pattons_Best
{
   public class ConfigFileReader
   {
      public string[] theTables = new string[39] 
           { "Activation", "Ammo", "AP To Kill (75)", "AP To Kill (76)", "Bail Out", "Bogged Down", "Brew Up", "Calendar", "Collateral", "Deployment",
             "Enemy Advance", "Enemy Appearance", "Enemy AP To Hit", "Enemy AP To Kill", "Enemy Battle",  "Enemy Counterattack",
             "Exit Areas", "Explosion",  "Friendly Action",  "Gun Malfunction", "HE to Kill (75)", "HE to Kill (76)",
             "Hit Location Crew", "Hit Location", "Hit Location Tank", "Minefield", "Movement", "Panzerfaust", "Placement", "Random Events", "Rate of Fire",
             "Resistance","Sherman MG", "Spotting", "Time", "To Hit Target", "To Kill Infantry", "Weather", "Wounds"
           };
      public static string theConfigDirectory = "";
      public bool CtorError { get; } = false;
      private Dictionary<string, string> myEntries = new Dictionary<string, string>();
      public Dictionary<string, string> Entries { get => myEntries; }
      private Dictionary<string, string> myRecordTitles = new Dictionary<string, string>();
      public Dictionary<string, string> RecordTitles { get => myRecordTitles; }
      private StreamReader? myStreamReader = null;
         //-------------------------------------------------------------
      public ConfigFileReader(string filename)
      {
         if( true == filename.Contains("Tables"))
         {
            foreach( string s in theTables )
            {
               string filename1 = ConfigFileReader.theConfigDirectory + s + ".xml";
               if (false == CreateTable(s, filename1))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ConfigFileReader(): CreateTable() returned false for s=" + s);
                  CtorError = true;
                  return;
               }
            }
            return;
         }
         const int MAX_RECORDS_IN_FILE = 1000; // number of rows in spreadsheet
         try
         {
            if (false == ReplaceDoubleQuotesWithSingleQuote(filename)) // First remove all double quotes and replace with a single quote  "" ==> '
            {
               Logger.Log(LogEnum.LE_ERROR, "ConfigFileReader(): ReplaceDoubleQuotesWithSingleQuote() returned false");
               CtorError = true;
               return;
            }
            myStreamReader = File.OpenText(filename);
            int i = 0;
            for (i = MAX_RECORDS_IN_FILE; 0 < i; --i)
            {
               int nextChar = myStreamReader.Peek();
               if (-1 == nextChar)  // break if reach end of file
                  return;
               if (false == CreateRecord(ref myStreamReader))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ConfigFileReader(): CreateRecord() returned false");
                  CtorError = true;
                  return;
               }
            }
            myStreamReader.Close();
            if (MAX_RECORDS_IN_FILE == i)
            {
               Logger.Log(LogEnum.LE_ERROR, "ConfigFileReader():reached EOF and still records left to read");
               CtorError = true;
               return;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "ConfigFileReader(): e=" + e.ToString());
            CtorError = true;
            return;
         }
      }
      //------------------------------------------------------------
      private bool CreateTable(string key, string filename)
      {
         string text = "ERROR";
         try
         {

            using StreamReader reader = new(filename);  // Open the text file using a stream reader.
            text = reader.ReadToEnd(); // Read the stream as a string.
         }
         catch (IOException e)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateTable(): e=" + e.ToString());
            return false;
         }
         try
         {
            myEntries.Add(key, text); // create dictionary entry
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateTable(): key=" + key + " e=" + e.ToString());
            return false;
         }
         return true;
      }
      private bool ReplaceDoubleQuotesWithSingleQuote(string filename)
      {
         try
         {
            // Open file and search for double quotes. Replace with single quot
            // Save file back to original name
            StreamReader sr = File.OpenText(filename);
            string fileContent = sr.ReadToEnd();
            sr.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fileContent.Length; ++i)
            {
               if ('"' == fileContent[i])
               {
                  if (fileContent.Length <= (i + 1))
                     return true;
                  if ('"' == fileContent[i + 1]) // skip appending two characters if double double quotes, i.e. ""
                  {
                     ++i; // advance past the second quote
                     sb.Append('\''); // replace with single quote
                     continue;
                  }

               }
               sb.Append(fileContent[i]);
            }
            // Write stringbuilder to file.
            StreamWriter wr = File.CreateText(filename);
            wr.Write(sb.ToString());
            wr.Close();
            return true;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReplaceDoubleQuotesWithSingleQuote(): e=" + e.ToString());
            return false;
         }
      }
      private bool CreateRecord(ref StreamReader sr)
      {
         // This method attempts to create a dictionary entry 
         // from a StreamReader. The format of the stream is
         // r203  "XAML text"
         // r203  "XAML text"
         // r214  "XAML text"
         // The first value is the key. The 2nd value is a text string
         // indicating an incomplete XAML string. To make the XAML string complete,
         // a SringBuilder class is used to create the opening and closing tags.
         // Note that the dictionary value can be multiple lines of text until
         // the second quote is found. Also, the enclosing XAML text cannot have any
         // quotation marks.
         StringBuilder sb = new StringBuilder();
         string key = "";
         const int TOTAL_LINES_IN_RECORD = 2000;  // number of lines in the XAML 
         int count = TOTAL_LINES_IN_RECORD; // record should be less than this many lines
         string? aLine = null;
         //----------------------------------------------------
         while (0 < --count) // Find the key for the dictionary
         {
            int nextChar = sr.Peek();
            if (-1 == nextChar)  // break if reach end of file
               return true;
            aLine = sr.ReadLine(); // The first line is always assumed to have the first quotation mark and it should always have one quotation mark
            if (null == aLine)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateRecord(): aLine=null");
               return false;
            }
            string[] aStringArray1 = aLine.Split('"');
            if (2 == aStringArray1.Length)
            {
               key = aStringArray1[0].Trim();
               sb.Append(aStringArray1[1]);
               break;
            }
         }
         if (0 == count) // should break out of while loop prior to count reaching zero
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateRecord(): when findng first line count>" + TOTAL_LINES_IN_RECORD.ToString());
            return false;
         }
         if ("" == key)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateRecord(): key is empty");
            return false;
         }
         //----------------------------------------------------
         // Build up the string until another quote is found
         bool isEndOfRecordReached = false;
         count = TOTAL_LINES_IN_RECORD;
         while (0 < --count) // create the value for the dictionary
         {
            aLine = sr.ReadLine();
            if (null == aLine)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateRecord(): aLine=null");
               return false;
            }
            string[] aStringArray2 = aLine.Split('"'); // loop for quotation mark to find end of record
            switch (aStringArray2.Length)
            {
               case 1:
                  sb.Append(aLine); // add entire line
                  continue;
               case 2:
                  sb.Append(aStringArray2[0]); // append everthing prior to the quotation mark
                  isEndOfRecordReached = true; // reached end of record so break out of for loop
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "CreateRecord(): reached default with aLine=" + aLine);
                  return false;
            }
            if (true == isEndOfRecordReached)
               break;
         }
         if (0 == count) // should break out of while loop prior to count reaching zero
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateRecord(): when reading lines count>" + TOTAL_LINES_IN_RECORD.ToString());
            return false;
         }
         try
         {
            myEntries.Add(key, sb.ToString()); // create dictionary entry
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateRecord(): key=" + key + " e=" + e.ToString());
            return false;
         }
         return true;
      }
   }
}
