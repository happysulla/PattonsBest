using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Pattons_Best
{
   internal class GameLoadMgr
   {
      public static string theGamesDirectory = "";
      public static bool theIsCheckFileExist = false;
      private IMapItems myMapItems = new MapItems();  // used to store all mapitems when reading from XML file
      //--------------------------------------------------
      public GameLoadMgr() { }
      //--------------------------------------------------
      public IGameInstance? OpenGame()
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
            string filename = theGamesDirectory + "Checkpoint.bpg";
            IGameInstance? gi = ReadXml(filename);
            if (null == gi)
            {
               Logger.Log(LogEnum.LE_ERROR, "OpenGame(): ReadXml() returned null for " + filename);
               return null;
            }
            Logger.Log(LogEnum.LE_GAME_INIT, "OpenGame(): gi=" + gi.ToString());
            return gi;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGame(): path=" + theGamesDirectory + " e =" + e.ToString());
            return new GameInstance();
         }
      }
      //--------------------------------------------------
      public bool SaveGameToFile(IGameInstance gi)
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
            XmlDocument? aXmlDocument = CreateXml(gi); // create a new XML document 
            if (null == aXmlDocument)
            {
               Logger.Log(LogEnum.LE_ERROR, "SaveGameToFile(): CreateXml() returned null for path=" + theGamesDirectory);
               return false;
            }
            using (FileStream writer = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
               XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
               using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
               {
                  aXmlDocument.Save(xmlWriter);
               }
            }
            theIsCheckFileExist = true;
            return true;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameToFile(): path=" + theGamesDirectory + " e =" + ex.ToString());
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return false;
         }
      }
      //--------------------------------------------------
      public IGameInstance? OpenGameFromFile()
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
            Directory.SetCurrentDirectory(theGamesDirectory);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): path=" + theGamesDirectory + " e=" + e.ToString());
            return null;
         }
         try
         {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = theGamesDirectory;
            dlg.RestoreDirectory = true;
            dlg.Filter = "Barbarin Prince Games|*.bpg";
            if (true == dlg.ShowDialog())
            {
               IGameInstance? gi = ReadXml(dlg.FileName);
               if (null == gi)
               {
                  Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
                  Logger.Log(LogEnum.LE_ERROR, "OpenGame(): ReadXml(=) returned null for " + dlg.FileName);
                  return null;
               }
               Logger.Log(LogEnum.LE_GAME_INIT, "OpenGameFromFile(): gi=" + gi.ToString());
               string? gamePath = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               if (null == gamePath)
               {
                  Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
                  Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): Path.GetDirectoryName() returned null for fn=" + dlg.FileName);
                  return null;
               }
               theGamesDirectory = gamePath;
               theGamesDirectory += "\\";
               Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
               return gi;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): path=" + theGamesDirectory + " e =" + e.ToString());
         }
         Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
         return null;
      }
      //--------------------------------------------------
      public bool SaveGameAsToFile(IGameInstance gi)
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
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            string filename = GetFileName(gi);
            dlg.FileName = filename;
            dlg.InitialDirectory = theGamesDirectory;
            dlg.RestoreDirectory = true;
            if (true == dlg.ShowDialog())
            {
               XmlDocument? aXmlDocument = CreateXml(gi); // create a new XML document 
               if (null == aXmlDocument)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): CreateXml() returned null for path=" + theGamesDirectory);
                  return false;
               }
               using (FileStream writer = new FileStream(dlg.FileName, FileMode.OpenOrCreate, FileAccess.Write))
               {
                  XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
                  using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
                  {
                     aXmlDocument.Save(xmlWriter);
                  }
               }
               string? gamePath = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               if (null == gamePath)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): Path.GetDirectoryName() returned null for fn=" + dlg.FileName);
                  return false;
               }
               theGamesDirectory = gamePath; // save off the directory user chosen
               theGamesDirectory += "\\";
            }
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): path=" + theGamesDirectory + " e =" + ex.ToString());
            return false;
         }
         return true;
      }
      //--------------------------------------------------
      private string GetFileName(IGameInstance gi)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append(DateTime.Now.ToString("yyyyMMdd-HHmmss"));
         sb.Append("-D");
         int Day = gi.Day + 1;
         if (Day < 100)
            sb.Append("0");
         if (Day < 10)
            sb.Append("0");
         sb.Append(Day.ToString());
         sb.Append(".pbg");
         return sb.ToString();
      }
      //--------------------------------------------------
      private int GetMajorVersion()
      {
         Assembly assembly = Assembly.GetExecutingAssembly();
         if (null == assembly)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): Assembly.GetExecutingAssembly()=null");
            return -1;
         }
         Version? versionRunning = assembly.GetName().Version;
         if (null == versionRunning)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml():  assembly.GetName().Version=null");
            return -1;
         }
         return versionRunning.Major;
      }
      public bool ReadXmlTerritories(XmlReader reader, ITerritories territories)
      {
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(Territories)=false");
            return false;
         }
         if (reader.Name != "Territories")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Territories != (node=" + reader.Name + ")");
            return false;
         }
         //-----------------------------------------------------------------
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Territories.Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         //-----------------------------------------------------------------
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(Parent)=false count=" + count.ToString() + " i=" + i.ToString() );
               return false;
            }
            if (reader.Name != "Territory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Territory != (node=" + reader.Name + ")");
               return false;
            }
            string? tName = reader.GetAttribute("value");
            if (null == tName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute() returned false");
               return false;
            }
            ITerritory territory = new Territory(tName);
            //--------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(Parent)=false tName=" + tName);
               return false;
            }
            if (reader.Name != "Parent")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Parent != (node=" + reader.Name + ")");
               return false;
            }
            string? sAttribute = reader.GetAttribute("value");
            if (null == sAttribute)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(Parent)=null");
               return false;
            }
            territory.CanvasName = sAttribute;
            //--------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(Type)=false");
               return false;
            }
            if (reader.Name != "Type")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Type != (node=" + reader.Name + ")");
               return false;
            }
            string? sAttribute1 = reader.GetAttribute("value");
            if (null == sAttribute1)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(Type)=null");
               return false;
            }
            territory.Type = sAttribute1;
            //--------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(CenterPoint)=false");
               return false;
            }
            if (reader.Name != "CenterPoint")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): CenterPoint != (node=" + reader.Name + ")");
               return false;
            }
            string? sX = reader.GetAttribute("X");
            if (null == sX)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sX)=null");
               return false;
            }
            territory.CenterPoint.X = double.Parse(sX);
            string? sY = reader.GetAttribute("Y");
            if (null == sY)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sX)=null");
               return false;
            }
            territory.CenterPoint.Y = double.Parse(sY);
            //--------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(Points)=false");
               return false;
            }
            if (reader.Name != "Points")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Points != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount0 = reader.GetAttribute("count");
            if (null == sCount0)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sCount0)=null");
               return false;
            }
            int count0 = int.Parse(sCount0);
            for (int i1 = 0; i1 < count0; ++i1)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(regionPoint)=false");
                  return false;
               }
               if (reader.Name != "regionPoint")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): regionPoint != (node=" + reader.Name + ")");
                  return false;
               }
               string? sX1 = reader.GetAttribute("X");
               if (null == sX1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sX1)=null");
                  return false;
               }
               string? sY1 = reader.GetAttribute("Y");
               if (null == sY1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sY1)=null");
                  return false;
               }
               double x = double.Parse(sX1);
               double y = double.Parse(sY1);
               IMapPoint mp = new MapPoint(x, y);
               territory.Points.Add(mp);
            }
            if (0 < count0)
               reader.Read(); // get past </Points> tag
            //--------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(Adjacents)=false");
               return false;
            }
            if (reader.Name != "Adjacents")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Adjacents != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount3 = reader.GetAttribute("count");
            if (null == sCount3)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sCount3)=null");
               return false;
            }
            int count3 = int.Parse(sCount3);
            for (int i3 = 0; i3 < count3; ++i3)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(adjacent)=false");
                  return false;
               }
               if (reader.Name != "adjacent")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): adjacent != (node=" + reader.Name + ")");
                  return false;
               }
               string? sAdjacent = reader.GetAttribute("value");
               if (null == sAdjacent)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sAdjacent)=null");
                  return false;
               }
               territory.Adjacents.Add(sAdjacent);
            }
            if (0 < count3)
               reader.Read(); // get past </Adjacents> tag
            //--------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(PavedRoads)=false");
               return false;
            }
            if (reader.Name != "PavedRoads")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): PavedRoads != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount4 = reader.GetAttribute("count");
            if (null == sCount4)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sCount4)=null");
               return false;
            }
            int count4 = int.Parse(sCount4);
            for (int i4 = 0; i4 < count4; ++i4)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(paved)=false");
                  return false;
               }
               if (reader.Name != "paved")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): paved != (node=" + reader.Name + ")");
                  return false;
               }
               string? sPaved= reader.GetAttribute("value");
               if (null == sPaved)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sPaved)=null");
                  return false;
               }
               territory.PavedRoads.Add(sPaved);
            }
            if (0 < count4)
               reader.Read(); // get past </PavedRoads> tag
            //--------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(UnpavedRoads)=false");
               return false;
            }
            if (reader.Name != "UnpavedRoads")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): UnpavedRoads != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount5 = reader.GetAttribute("count");
            if (null == sCount5)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sCount5)=null");
               return false;
            }
            int count5 = int.Parse(sCount5);
            for (int i5 = 0; i5 < count5; ++i5)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(unpaved)=false");
                  return false;
               }
               if (reader.Name != "unpaved")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): unpaved != (node=" + reader.Name + ")");
                  return false;
               }
               string? sUnpaved = reader.GetAttribute("value");
               if (null == sUnpaved)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sUnpaved)=null");
                  return false;
               }
               territory.UnpavedRoads.Add(sUnpaved);
            }
            if (0 < count5)
               reader.Read(); // get past </UnpavedRoads> tag
            //--------------------------------------
            territories.Add(territory);
            reader.Read(); // get past </Territory> tag
         }
         if (0 < count)
            reader.Read(); // get past </Territories> tag
         return true;
      }
      public bool CreateXmlTerritories(XmlDocument aXmlDocument, ITerritories territories)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlAttribute xmlAttribute = aXmlDocument.CreateAttribute("count");
         xmlAttribute.Value = territories.Count.ToString();
         if (null == root.Attributes)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root.Attributes is null");
            return false;
         }
         root.Attributes.Append(xmlAttribute);
         //--------------------------------
         foreach (Territory t in territories)
         {
            XmlElement? terrElem = aXmlDocument.CreateElement("Territory");  // name of territory
            if (null == terrElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(terrElem) returned null");
               return false;
            }
            terrElem.SetAttribute("value", t.Name);
            XmlNode? territoryNode = root.AppendChild(terrElem);
            if (null == territoryNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(territoryNode) returned null");
               return false;
            }
            //---------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("Parent");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(terrElem) returned null");
               return false;
            }
            elem.SetAttribute("value", t.CanvasName);
            XmlNode? node = territoryNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //---------------------------------
            elem = aXmlDocument.CreateElement("Type");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(terrElem) returned null");
               return false;
            }
            elem.SetAttribute("value", t.Type);
            node = territoryNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //---------------------------------
            elem = aXmlDocument.CreateElement("CenterPoint");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(CenterPoint) returned null");
               return false;
            }
            elem.SetAttribute("X", t.CenterPoint.X.ToString("0000.00"));
            elem.SetAttribute("Y", t.CenterPoint.Y.ToString("0000.00"));
            node = territoryNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //---------------------------------
            XmlElement? elemPoints = aXmlDocument.CreateElement("Points");
            if (null == elemPoints)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(elemPoints) returned null");
               return false;
            }
            elemPoints.SetAttribute("count", t.Points.Count.ToString());
            XmlNode? nodePoints = territoryNode.AppendChild(elemPoints);
            if (null == nodePoints)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(nodePoints) returned null");
               return false;
            }
            //---------------------------------
            foreach (IMapPoint mp in t.Points)
            {
               elem = aXmlDocument.CreateElement("regionPoint");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(terrElem) returned null");
                  return false;
               }
               elem.SetAttribute("X", mp.X.ToString("0000.00"));
               elem.SetAttribute("Y", mp.Y.ToString("0000.00"));
               node = nodePoints.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
                  return false;
               }
            }
            //-----------------------------------------------------------
            XmlElement? elemAdjacents = aXmlDocument.CreateElement("Adjacents");
            if (null == elemAdjacents)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(elemAdjacents) returned null");
               return false;
            }
            elemAdjacents.SetAttribute("count", t.Adjacents.Count.ToString());
            XmlNode? nodeAdjacents = territoryNode.AppendChild(elemAdjacents);
            if (null == nodeAdjacents)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(nodePoints) returned null");
               return false;
            }
            //---------------------------------
            foreach (string s in t.Adjacents)
            {
               elem = aXmlDocument.CreateElement("adjacent");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(adjacent) returned null");
                  return false;
               }
               elem.SetAttribute("value", s);
               node = nodeAdjacents.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(nodeAdjacents) returned null");
                  return false;
               }
            }
            //-----------------------------------------------------------
            XmlElement? elemPavedRoads = aXmlDocument.CreateElement("PavedRoads");
            if (null == elemPavedRoads)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(elemPavedRoads) returned null");
               return false;
            }
            elemPavedRoads.SetAttribute("count", t.PavedRoads.Count.ToString());
            XmlNode? nodePavedRoads = territoryNode.AppendChild(elemPavedRoads);
            if (null == nodePavedRoads)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(nodePavedRoads) returned null");
               return false;
            }
            //---------------------------------
            foreach (string s in t.PavedRoads)
            {
               elem = aXmlDocument.CreateElement("paved");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(paved) returned null");
                  return false;
               }
               elem.SetAttribute("value", s);
               node = nodePavedRoads.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(paved) returned null");
                  return false;
               }
            }
            //-----------------------------------------------------------
            XmlElement? elemUnpavedRoads = aXmlDocument.CreateElement("UnpavedRoads");
            if (null == elemUnpavedRoads)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(elemUnpavedRoads) returned null");
               return false;
            }
            elemUnpavedRoads.SetAttribute("count", t.UnpavedRoads.Count.ToString());
            XmlNode? nodeUnpavedRoads = territoryNode.AppendChild(elemUnpavedRoads);
            if (null == nodeUnpavedRoads)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(nodeUnpavedRoads) returned null");
               return false;
            }
            //---------------------------------
            foreach (string s in t.UnpavedRoads)
            {
               elem = aXmlDocument.CreateElement("unpaved");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(unpaved) returned null");
                  return false;
               }
               elem.SetAttribute("value", s);
               node = nodeUnpavedRoads.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(unpaved) returned null");
                  return false;
               }
            }
         }
         return true;
      }
      //--------------------------------------------------
      private IGameInstance? ReadXml(string filename)
      {
         IGameInstance gi = new GameInstance();
         IMapItems mapItems1 = new MapItems();
         ITerritories territories = new Territories();
         XmlTextReader? reader = null;
         try
         {
            // Load the reader with the data file and ignore all white space nodes.
            reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
            reader.Read();
            if (true == reader.IsStartElement())
            {
               if (reader.Name != "GameInstance")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): first node is not GameInstance");
                  return null;
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "Version")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sVersion = reader.GetAttribute("value");
                  if (null == sVersion)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): version=null");
                     return null;
                  }
                  int version = int.Parse(sVersion);
                  if (version != GetMajorVersion())
                  {
                     System.Windows.MessageBox.Show("Unable to open due to version mismatch. File v" + version + " does not match running v" + GetMajorVersion() + ".");
                     return null;
                  }
               }
            }
            //----------------------------------------------
            if (false == ReadXmlOptions(reader, gi.Options))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlOptions() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlGameStat(reader, gi.Statistic))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlGameStat() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlGameMapItems(reader, gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlGameMapItems() returned false");
               return null;
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "EventActive")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? eventActive = reader.GetAttribute("value");
                  if (null == eventActive)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(EventActive)=null");
                     return null;
                  }
                  gi.EventActive = eventActive;
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "EventDisplayed")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? eventDisplayed = reader.GetAttribute("value");
                  if (null == eventDisplayed)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(EventDisplayed)=null");
                     return null;
                  }
                  gi.EventDisplayed = eventDisplayed;
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "GameTurn")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(GameTurn)=null");
                     return null;
                  }
                  gi.GameTurn = int.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            return gi;
         } // try
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml():\n" + e.ToString());
            return null;
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
      private bool ReadXmlOptions(XmlReader reader, Options options)
      {
         options.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): reader.IsStartElement() = false");
            return false;
         }
         if (reader.Name != "Options")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Options != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Count=null");
            return false;
         }
         //-------------------------------------
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (true == reader.IsStartElement())
            {
               if (reader.Name != "Option")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Option != " + reader.Name);
                  return false;
               }
               string? name = reader.GetAttribute("Name");
               if (name == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Name=null");
                  return false;
               }
               string? sEnabled = reader.GetAttribute("IsEnabled");
               if (sEnabled == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): IsEnabled=null");
                  return false;
               }
               bool isEnabled = bool.Parse(sEnabled);
               Option option = new Option(name, isEnabled);
               options.Add(option);
            }
         }
         reader.Read(); // get past </Options>
         return true;
      }
      private bool ReadXmlGameStat(XmlReader reader, GameStat statistic)
      {
         return true;
      }
      private bool ReadXmlGameMapItems(XmlReader reader, IGameInstance gi)
      {
         return true;
      }
      private bool ReadXmlGameMapItemsTerritories(XmlReader reader, ITerritories territories, string nodeName)
      {
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != nodeName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): " + nodeName + " != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Count=null");
               return false;
            }
            int count = int.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Territory")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameMapItems(): MapItem != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? tName = reader.GetAttribute("value");
                  if (null == tName)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameMapItems(): GetAttribute() returned false");
                     return false;
                  }
                  ITerritory? territory = Territories.theTerritories.Find(tName);
                  if (null == territory)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameMapItems(): Find() returned null for tName=" + tName);
                     return false;
                  }
                  territories.Add(territory);
               }
            }
            if (0 < count)
               reader.Read(); // get past end tag
         }
         return true;
      }
      private bool ReadXmlTerritories(XmlReader reader, ITerritories territories, string attribute)
      {
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != "Territories")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Territories != (node=" + reader.Name + ")");
               return false;
            }
            string? attributeRead = reader.GetAttribute("value");
            if (attribute != attributeRead)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): attributeRead=null for attribute=" + attribute);
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Count=null");
               return false;
            }
            int count = int.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Territory")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Territory != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? tName = reader.GetAttribute("value");
                  if (null == tName)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute() returned false");
                     return false;
                  }
                  ITerritory? territory = Territories.theTerritories.Find(tName);
                  if (null == territory)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Find() returned null for tName=" + tName);
                     return false;
                  }
                  territories.Add(territory);
               }
            }
            if (0 < count)
               reader.Read(); // get past </Territories> tag
         }
         return true;
      }
      //--------------------------------------------------
      private XmlDocument? CreateXml(IGameInstance gi)
      {
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<GameInstance></GameInstance>");
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): aXmlDocument.DocumentElement=null");
            return null;
         }
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): root is null");
            return null;
         }
         //------------------------------------------
         XmlElement? versionElem = aXmlDocument.CreateElement("Version");
         if (null == versionElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): aXmlDocument.DocumentElement.LastChild=null");
            return null;
         }
         int majorVersion = GetMajorVersion();
         if (majorVersion < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml():  0 > majorVersion=" + majorVersion.ToString());
            return null;
         }
         versionElem.SetAttribute("value", majorVersion.ToString());
         XmlNode? versionNode = root.AppendChild(versionElem);
         if (null == versionNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(versionNode) returned null");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlGameOptions(aXmlDocument, gi.Options))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlOptions() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlGameStat(aXmlDocument, gi.Statistic))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlGameStat() returned false");
            return null;
         }
         //------------------------------------------
         XmlElement? elem = aXmlDocument.CreateElement("EventActive");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(EventActive) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EventActive);
         XmlNode? node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(EventActive) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EventDisplayed");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(EventDisplayed) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EventDisplayed);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(EventDisplayed) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("Day");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(Day) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.Day.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(Day) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("GameTurn");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(GameTurn) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.GameTurn.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(GameTurn) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("GamePhase");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(GamePhase) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.GamePhase.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(GamePhase) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EndGameReason");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(EndGameReason) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EndGameReason.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(EndGameReason) returned null");
            return null;
         }
         //------------------------------------------
         if( false == CreateXmlGameReports(aXmlDocument, gi.Reports))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlGameReports() returned false");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("BattlePhase");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(BattlePhase) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.BattlePhase.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(BattlePhase) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("CrewActionPhase");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(CrewActionPhase) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.CrewActionPhase.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(CrewActionPhase) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("MovementEffectOnSherman");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(MovementEffectOnSherman) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.MovementEffectOnSherman);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(MovementEffectOnSherman) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("MovementEffectOnEnemy");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(MovementEffectOnEnemy) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.MovementEffectOnEnemy);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(MovementEffectOnEnemy) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("FiredAmmoType");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(FiredAmmoType) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.FiredAmmoType);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(FiredAmmoType) returned null");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, gi.NewMembers, "NewMembers"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItems(NewMembers) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, gi.ReadyRacks, "ReadyRacks"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItems(ReadyRacks) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, gi.Hatches, "Hatches"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItems(Hatches) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, gi.CrewActions, "CrewActions"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItems(CrewActions) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, gi.GunLoads, "GunLoads"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItems(GunLoads) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, gi.Targets, "Targets"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItems(Targets) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, gi.AdvancingEnemies, "AdvancingEnemies"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItems(AdvancingEnemies) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, gi.ShermanAdvanceOrRetreatEnemies, "ShermanAdvanceOrRetreatEnemies"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItems(ShermanAdvanceOrRetreatEnemies) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, gi.InjuredCrewMembers, "InjuredCrewMembers"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItems(InjuredCrewMembers) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItem(aXmlDocument, root, gi.Sherman))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItem(Sherman) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItem(aXmlDocument, root, gi.TargetMainGun))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItem(TargetMainGun) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItem(aXmlDocument, root, gi.TargetMg))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItem(TargetMg) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItem(aXmlDocument, root, gi.ShermanHvss))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItem(ShermanHvss) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItem(aXmlDocument, root, gi.ReturningCrewman))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlMapItem(ReturningCrewman) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, gi.AreaTargets, "AreaTargets"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlTerritories(AreaTargets) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, gi.CounterattachRetreats, "CounterattachRetreats"))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlTerritories(CounterattachRetreats) returned false");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("Home");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(Home) returned null");
            return null;
         }
         elem.SetAttribute("value", "Home");
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(Home) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EnemyStrengthCheckTerritory");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(EnemyStrengthCheckTerritory) returned null");
            return null;
         }
         if( null == gi.EnemyStrengthCheckTerritory)
            elem.SetAttribute("value", "null");
         else
            elem.SetAttribute("value", gi.EnemyStrengthCheckTerritory.Name);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(EnemyStrengthCheckTerritory) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("ArtillerySupportCheck");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(ArtillerySupportCheck) returned null");
            return null;
         }
         if (null == gi.ArtillerySupportCheck)
            elem.SetAttribute("value", "null");
         else
            elem.SetAttribute("value", gi.ArtillerySupportCheck.Name);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(ArtillerySupportCheck) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("AirStrikeCheckTerritory");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(AirStrikeCheckTerritory) returned null");
            return null;
         }
         if (null == gi.AirStrikeCheckTerritory)
            elem.SetAttribute("value", "null");
         else
            elem.SetAttribute("value", gi.AirStrikeCheckTerritory.Name);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(AirStrikeCheckTerritory) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EnteredArea");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(EnteredArea) returned null");
            return null;
         }
         if (null == gi.EnteredArea)
            elem.SetAttribute("value", "null");
         else
            elem.SetAttribute("value", gi.EnteredArea.Name);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(EnteredArea) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("AdvanceFire");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(AdvanceFire) returned null");
            return null;
         }
         if (null == gi.AdvanceFire)
            elem.SetAttribute("value", "null");
         else
            elem.SetAttribute("value", gi.AdvanceFire.Name);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(AdvanceFire) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("FriendlyAdvance");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(FriendlyAdvance) returned null");
            return null;
         }
         if (null == gi.FriendlyAdvance)
            elem.SetAttribute("value", "null");
         else
            elem.SetAttribute("value", gi.FriendlyAdvance.Name);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(FriendlyAdvance) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EnemyAdvance");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(EnemyAdvance) returned null");
            return null;
         }
         if (null == gi.EnemyAdvance)
            elem.SetAttribute("value", "null");
         else
            elem.SetAttribute("value", gi.EnemyAdvance.Name);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(EnemyAdvance) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsHatchesActive");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsHatchesActive) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsHatchesActive.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsHatchesActive) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsRetreatToStartArea");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsRetreatToStartArea) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsRetreatToStartArea.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsRetreatToStartArea) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanAdvancingOnMoveBoard");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanAdvancingOnMoveBoard) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanAdvancingOnMoveBoard.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanAdvancingOnMoveBoard) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("SwitchedCrewMember");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(SwitchedCrewMember) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.SwitchedCrewMember);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(SwitchedCrewMember) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("AssistantOriginalRating");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(AssistantOriginalRating) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.AssistantOriginalRating.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(AssistantOriginalRating) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiringAtFront");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanFiringAtFront) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiringAtFront.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanFiringAtFront) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanDeliberateImmobilization");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanDeliberateImmobilization) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanDeliberateImmobilization.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanDeliberateImmobilization) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("ShermanTypeOfFire");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(ShermanTypeOfFire) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.ShermanTypeOfFire);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(ShermanTypeOfFire) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("NumSmokeAttacksThisRound");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(NumSmokeAttacksThisRound) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.NumSmokeAttacksThisRound.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(NumSmokeAttacksThisRound) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMalfunctionedMainGun");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsMalfunctionedMainGun) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMalfunctionedMainGun.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsMalfunctionedMainGun) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMainGunRepairAttempted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsMainGunRepairAttempted) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMainGunRepairAttempted.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsMainGunRepairAttempted) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenMainGun");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsBrokenMainGun) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenMainGun.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsBrokenMainGun) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenGunSight");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsBrokenGunSight) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenGunSight.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsBrokenGunSight) returned null");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlFirstShots(aXmlDocument, gi.FirstShots))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlFirstShots(FirstShots) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTrainedGunners(aXmlDocument, gi.TrainedGunners))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlFirstShots(TrainedGunners) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTrainedGunners(aXmlDocument, gi.TrainedGunners))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlFirstShots(TrainedGunners) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlShermanHits(aXmlDocument, gi.ShermanHits))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlShermanHits(ShermanHits) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlShermanDeath(aXmlDocument, gi.Death))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlShermanDeath(Death) returned false");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IdentifiedTank");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IdentifiedTank) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IdentifiedTank);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IdentifiedTank) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IdentifiedAtg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IdentifiedAtg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IdentifiedAtg);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IdentifiedAtg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IdentifiedSpg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IdentifiedSpg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IdentifiedSpg);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IdentifiedSpg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiringAaMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanFiringAaMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiringAaMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanFiringAaMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiringBowMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanFiringBowMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiringBowMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanFiringBowMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiringCoaxialMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanFiringCoaxialMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiringCoaxialMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanFiringCoaxialMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiringSubMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanFiringSubMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiringSubMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanFiringSubMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsCommanderDirectingMgFire");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsCommanderDirectingMgFire) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsCommanderDirectingMgFire.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsCommanderDirectingMgFire) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiredAaMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanFiredAaMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiredAaMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanFiredAaMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiredBowMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanFiredBowMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiredBowMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanFiredBowMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiredCoaxialMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanFiredCoaxialMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiredCoaxialMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanFiredCoaxialMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiredSubMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanFiredSubMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiredSubMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanFiredSubMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMalfunctionedMgCoaxial");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsMalfunctionedMgCoaxial) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMalfunctionedMgCoaxial.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsMalfunctionedMgCoaxial) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMalfunctionedMgBow");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsMalfunctionedMgBow) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMalfunctionedMgBow.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsMalfunctionedMgBow) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMalfunctionedMgAntiAircraft");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsMalfunctionedMgAntiAircraft) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMalfunctionedMgAntiAircraft.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsMalfunctionedMgAntiAircraft) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsCoaxialMgRepairAttempted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsCoaxialMgRepairAttempted) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsCoaxialMgRepairAttempted.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsCoaxialMgRepairAttempted) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBowMgRepairAttempted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsBowMgRepairAttempted) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBowMgRepairAttempted.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsBowMgRepairAttempted) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsAaMgRepairAttempted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsAaMgRepairAttempted) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsAaMgRepairAttempted.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsAaMgRepairAttempted) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenMgAntiAircraft");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsBrokenMgAntiAircraft) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenMgAntiAircraft.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsBrokenMgAntiAircraft) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenMgBow");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsBrokenMgBow) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenMgBow.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsBrokenMgBow) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenMgCoaxial");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsBrokenMgCoaxial) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenMgCoaxial.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsBrokenMgCoaxial) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenPeriscopeDriver");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsBrokenPeriscopeDriver) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenPeriscopeDriver.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsBrokenPeriscopeDriver) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenPeriscopeLoader");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsBrokenPeriscopeLoader) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenPeriscopeLoader.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsBrokenPeriscopeLoader) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenPeriscopeAssistant");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsBrokenPeriscopeAssistant) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenPeriscopeAssistant.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsBrokenPeriscopeAssistant) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenPeriscopeGunner");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsBrokenPeriscopeGunner) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenPeriscopeGunner.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsBrokenPeriscopeGunner) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenPeriscopeCommander");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsBrokenPeriscopeCommander) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenPeriscopeCommander.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsBrokenPeriscopeCommander) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanTurretRotated");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsShermanTurretRotated) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanTurretRotated.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsShermanTurretRotated) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("ShermanRotationTurretOld");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(ShermanRotationTurretOld) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.ShermanRotationTurretOld.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(ShermanRotationTurretOld) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsCounterattackAmbush");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsCounterattackAmbush) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsCounterattackAmbush.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsCounterattackAmbush) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsLeadTank");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsLeadTank) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsLeadTank.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsLeadTank) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsAirStrikePending");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsAirStrikePending) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsAirStrikePending.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsAirStrikePending) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsAdvancingFireChosen");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsAdvancingFireChosen) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsAdvancingFireChosen.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsAdvancingFireChosen) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("AdvancingFireMarkerCount");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(AdvancingFireMarkerCount) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.AdvancingFireMarkerCount.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(AdvancingFireMarkerCount) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("BattleResistance");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(BattleResistance) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.BattleResistance.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(BattleResistance) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMinefieldAttack");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsMinefieldAttack) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMinefieldAttack.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsMinefieldAttack) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsHarrassingFireBonus");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsHarrassingFireBonus) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsHarrassingFireBonus.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsHarrassingFireBonus) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsFlankingFire");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsFlankingFire) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsFlankingFire.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsFlankingFire) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsEnemyAdvanceComplete");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsEnemyAdvanceComplete) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsEnemyAdvanceComplete.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsEnemyAdvanceComplete) returned null");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlPanzerfaustAttack(aXmlDocument, gi.Panzerfaust))
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateXmlPanzerfaustAttack(Panzerfaust) returned false");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("NumCollateralDamage");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(NumCollateralDamage) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.NumCollateralDamage.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(NumCollateralDamage) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("TankReplacementNumber");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(TankReplacementNumber) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.TankReplacementNumber.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(TankReplacementNumber) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("VictoryPtsTotalCampaign");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(VictoryPtsTotalCampaign) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.VictoryPtsTotalCampaign.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(VictoryPtsTotalCampaign) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("PromotionPointNum");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(PromotionPointNum) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.PromotionPointNum.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(PromotionPointNum) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("PromotionDay");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(PromotionDay) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.PromotionDay.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(PromotionDay) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("NumPurpleHeart");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(NumPurpleHeart) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.NumPurpleHeart.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(NumPurpleHeart) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsCommanderRescuePerformed");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsCommanderRescuePerformed) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsCommanderRescuePerformed.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsCommanderRescuePerformed) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsCommanderKilled");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsCommanderKilled) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsCommanderKilled.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsCommanderKilled) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsPromoted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(IsPromoted) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsPromoted.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(IsPromoted) returned null");
            return null;
         }
         return aXmlDocument;
      }
      private bool CreateXmlGameOptions(XmlDocument aXmlDocument, Options options)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlElement? optionsElem = aXmlDocument.CreateElement("Options");
         if (null == optionsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Options) returned null");
            return false;
         }
         optionsElem.SetAttribute("count", options.Count.ToString());
         XmlNode? optionsNode = root.AppendChild(optionsElem);
         if (null == optionsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(optionsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (Option option in options)
         {
            XmlElement? optionElem = aXmlDocument.CreateElement("Option");
            if (null == optionElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Option) returned null");
               return false;
            }
            optionElem.SetAttribute("Name", option.Name);
            optionElem.SetAttribute("IsEnabled", option.IsEnabled.ToString());
            XmlNode? optionNode = optionsNode.AppendChild(optionElem);
            if (null == optionNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(optionNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlGameStat(XmlDocument aXmlDocument, GameStat stat)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlElement? gameStatElem = aXmlDocument.CreateElement("GameStat");
         if (null == gameStatElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(gameStatElem) returned null");
            return false;
         }
         XmlNode? gameStatNode = root.AppendChild(gameStatElem);
         if (null == gameStatNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(gameStatNode) returned null");
            return false;
         }
         return true;
      }
      private bool CreateXmlGameReports(XmlDocument aXmlDocument, IAfterActionReports reports)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): root is null");
            return false;
         }
         XmlElement? reportsElem = aXmlDocument.CreateElement("Reports");
         if (null == reportsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Reports) returned null");
            return false;
         }
         reportsElem.SetAttribute("count", reports.Count.ToString());
         XmlNode? reportsNode = root.AppendChild(reportsElem);
         if (null == reportsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(reportsNode) returned null");
            return false;
         }
         //--------------------------------
         for (int k = 0; k < reports.Count; k++)
         {
            IAfterActionReport? report = reports[k];
            if (null == report)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): report=null");
               return false;
            }
            XmlElement? reportElem = aXmlDocument.CreateElement("Report");
            if (null == reportElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Report) returned false");
               return false;
            }
            XmlNode? reportNode = reportsNode.AppendChild(reportElem);
            if (null == reportNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(reportNode) returned false");
               return false;
            }
            //------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("Day");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Day) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Day);
            XmlNode? node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Day) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Scenario");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Scenario) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Scenario.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Scenario) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Probability");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Probability) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Probability.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Probability) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Resistance");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Resistance) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Resistance.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Resistance) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Name");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Name) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Name);
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Name) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("TankCardNum");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(TankCardNum) returned false");
               return false;
            }
            elem.SetAttribute("value", report.TankCardNum.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(TankCardNum) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Weather");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Weather) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Weather);
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Weather) returned false");
               return false;
            }
            //------------------------------------------
            if (false == CreateXmlCrewMember(aXmlDocument, reportNode, report.Commander))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateXmlCrewMember(Commander) returned false");
               return false;
            }
            //------------------------------------------
            if (false == CreateXmlCrewMember(aXmlDocument, reportNode, report.Gunner))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateXmlCrewMember(Gunner) returned false");
               return false;
            }
            //------------------------------------------
            if (false == CreateXmlCrewMember(aXmlDocument, reportNode, report.Loader))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateXmlCrewMember(Loader) returned false");
               return false;
            }
            //------------------------------------------
            if (false == CreateXmlCrewMember(aXmlDocument, reportNode, report.Driver))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateXmlCrewMember(Driver) returned false");
               return false;
            }
            //------------------------------------------
            if (false == CreateXmlCrewMember(aXmlDocument, reportNode, report.Assistant))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateXmlCrewMember(Assistant) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("SunriseHour");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(SunriseHour) returned false");
               return false;
            }
            elem.SetAttribute("value", report.SunriseHour.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(SunriseHour) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("SunriseMin");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(SunriseMin) returned false");
               return false;
            }
            elem.SetAttribute("value", report.SunriseMin.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(SunriseMin) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("SunsetHour");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(SunsetHour) returned false");
               return false;
            }
            elem.SetAttribute("value", report.SunsetHour.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(SunsetHour) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("SunsetMin");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(SunsetMin) returned false");
               return false;
            }
            elem.SetAttribute("value", report.SunsetMin.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(SunsetMin) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Ammo30CalibreMG");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Ammo30CalibreMG) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Ammo30CalibreMG.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Ammo30CalibreMG) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Ammo50CalibreMG");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Ammo50CalibreMG) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Ammo50CalibreMG.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Ammo50CalibreMG) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("AmmoSmokeBomb");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(AmmoSmokeBomb) returned false");
               return false;
            }
            elem.SetAttribute("value", report.AmmoSmokeBomb.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(AmmoSmokeBomb) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("AmmoSmokeGrenade");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(AmmoSmokeGrenade) returned false");
               return false;
            }
            elem.SetAttribute("value", report.AmmoSmokeGrenade.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(AmmoSmokeGrenade) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("AmmoPeriscope");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(AmmoPeriscope) returned false");
               return false;
            }
            elem.SetAttribute("value", report.AmmoPeriscope.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(AmmoPeriscope) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("MainGunHE");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(MainGunHE) returned false");
               return false;
            }
            elem.SetAttribute("value", report.MainGunHE.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(MainGunHE) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("MainGunAP");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(MainGunAP) returned false");
               return false;
            }
            elem.SetAttribute("value", report.MainGunAP.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(MainGunAP) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("MainGunWP");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(MainGunWP) returned false");
               return false;
            }
            elem.SetAttribute("value", report.MainGunWP.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(MainGunWP) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("MainGunHBCI");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(MainGunHBCI) returned false");
               return false;
            }
            elem.SetAttribute("value", report.MainGunHBCI.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(MainGunHBCI) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("MainGunHVAP");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(MainGunHVAP) returned false");
               return false;
            }
            elem.SetAttribute("value", report.MainGunHVAP.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(MainGunHVAP) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsFriendlyKiaLightWeapon");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsFriendlyKiaLightWeapon) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsFriendlyKiaLightWeapon.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsFriendlyKiaLightWeapon) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsFriendlyKiaTruck");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsFriendlyKiaTruck) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsFriendlyKiaTruck.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsFriendlyKiaTruck) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsFriendlyKiaSpwOrPsw");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsFriendlyKiaSpwOrPsw) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsFriendlyKiaSpwOrPsw.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsFriendlyKiaSpwOrPsw) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsFriendlyKiaSPGun");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsFriendlyKiaSPGun) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsFriendlyKiaSPGun.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsFriendlyKiaSPGun) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsFriendlyKiaPzIV");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsFriendlyKiaPzIV) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsFriendlyKiaPzIV.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsFriendlyKiaPzIV) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsFriendlyKiaPzV");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsFriendlyKiaPzV) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsFriendlyKiaPzV.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsFriendlyKiaPzV) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsFriendlyKiaPzVI");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsFriendlyKiaPzVI) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsFriendlyKiaPzVI.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsFriendlyKiaPzVI) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsFriendlyKiaAtGun");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsFriendlyKiaAtGun) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsFriendlyKiaAtGun.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsFriendlyKiaAtGun) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsFriendlyKiaFortifiedPosition");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsFriendlyKiaFortifiedPosition) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsFriendlyKiaFortifiedPosition.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsFriendlyKiaFortifiedPosition) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsYourKiaLightWeapon");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsYourKiaLightWeapon) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsYourKiaLightWeapon.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsYourKiaLightWeapon) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsYourKiaTruck");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsYourKiaTruck) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsYourKiaTruck.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsYourKiaTruck) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsYourKiaSpwOrPsw");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsYourKiaSpwOrPsw) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsYourKiaSpwOrPsw.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsYourKiaSpwOrPsw) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsYourKiaSPGun");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsYourKiaSPGun) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsYourKiaSPGun.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsYourKiaSPGun) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsYourKiaPzIV");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsYourKiaPzIV) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsYourKiaPzIV.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsYourKiaPzIV) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsYourKiaPzV");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsYourKiaPzV) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsYourKiaPzV.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsYourKiaPzV) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsYourKiaPzVI");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsYourKiaPzVI) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsYourKiaPzVI.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsYourKiaPzVI) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsYourKiaAtGun");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsYourKiaAtGun) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsYourKiaAtGun.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsYourKiaAtGun) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsYourKiaFortifiedPosition");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsYourKiaFortifiedPosition) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsYourKiaFortifiedPosition.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsYourKiaFortifiedPosition) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsCaptureArea");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsCaptureArea) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsCaptureArea.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsCaptureArea) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsCapturedExitArea");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsCapturedExitArea) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsCapturedExitArea.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsCapturedExitArea) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsLostArea");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsLostArea) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsLostArea.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsLostArea) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsFriendlyTank");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsFriendlyTank) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsFriendlyTank.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsFriendlyTank) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsFriendlySquad");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsFriendlySquad) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsFriendlySquad.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsFriendlySquad) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsTotalYourTank");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsTotalYourTank) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsTotalYourTank.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsTotalYourTank) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsTotalFriendlyForces");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsTotalFriendlyForces) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsTotalFriendlyForces.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsTotalFriendlyForces) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsTotalTerritory");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsTotalTerritory) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsTotalTerritory.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsTotalTerritory) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("VictoryPtsTotalEngagement");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPtsTotalEngagement) returned false");
               return false;
            }
            elem.SetAttribute("value", report.VictoryPtsTotalEngagement.ToString());
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPtsTotalEngagement) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Decorations");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Decorations) returned false");
               return false;
            }
            elem.SetAttribute("count", report.Decorations.Count.ToString());
            XmlNode? decorationsNode = reportsNode.AppendChild(elem);
            if (null == decorationsNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Decorations) returned false");
               return false;
            }
            for (int k1 = 0; k1 < report.Decorations.Count; ++k1)
            {
               elem = aXmlDocument.CreateElement("Decoration");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Decoration) returned false");
                  return false;
               }
               elem.SetAttribute("value", report.Decorations[k1].ToString());
               XmlNode? decorationNode = decorationsNode.AppendChild(elem);
               if (null == decorationNode)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(decorationNode) returned false");
                  return false;
               }
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Notes");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Notes) returned false");
               return false;
            }
            elem.SetAttribute("count", report.Notes.Count.ToString());
            XmlNode? notesNode = reportsNode.AppendChild(elem);
            if (null == notesNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Notes) returned false");
               return false;
            }
            for (int k1 = 0; k1 < report.Notes.Count; ++k1)
            {
               elem = aXmlDocument.CreateElement("Note");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Decoration) returned false");
                  return false;
               }
               elem.SetAttribute("value", report.Notes[k1].ToString());
               XmlNode? noteNode = notesNode.AppendChild(elem);
               if (null == noteNode)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(noteNode) returned false");
                  return false;
               }
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("DayEndedTime");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(DayEndedTime) returned false");
               return false;
            }
            elem.SetAttribute("value", report.DayEndedTime);
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(DayEndedTime) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Breakdown");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Breakdown) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Breakdown);
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Breakdown) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("KnockedOut");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(KnockedOut) returned false");
               return false;
            }
            elem.SetAttribute("value", report.KnockedOut);
            node = reportsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(KnockedOut) returned false");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlMapItems(XmlDocument aXmlDocument, IMapItems mapItems, string attribute )
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItems(): root is null");
            return false;
         }
         XmlElement? mapItemsElem = aXmlDocument.CreateElement("MapItems");
         if (null == mapItemsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItems(): CreateElement(MapItemsElem) returned null");
            return false;
         }
         mapItemsElem.SetAttribute("value", attribute);
         mapItemsElem.SetAttribute("count", mapItems.Count.ToString());
         XmlNode? mapItemsNode = root.AppendChild(mapItemsElem);
         if (null == mapItemsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItems(): AppendChild(MapItemsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (IMapItem mi in mapItems)
         {
            if( false == CreateXmlMapItem(aXmlDocument, mapItemsNode, mi))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItems(): CreateXmlMapItem() returned false");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlMapItem(XmlDocument aXmlDocument, XmlNode mapItemsNode, IMapItem? mi)
      {
         XmlElement? miElem = aXmlDocument.CreateElement("MapItem");
         if (null == miElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(miElem) returned null");
            return false;
         }
         XmlNode? node = mapItemsNode.AppendChild(miElem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(miNode) returned null");
            return false;
         }
         if( null == mi )
         {
            miElem.SetAttribute("value", "null");
            return true;
         }
         //--------------------------------
         XmlElement? elem = aXmlDocument.CreateElement("Name");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(Name) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.Name);
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("TopImageName");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(TopImageName) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.TopImageName);
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(TopImageName) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("BottomImageName");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(BottomImageName) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.BottomImageName);
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(BottomImageName) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("OverlayImageName");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(OverlayImageName) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.OverlayImageName);
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(OverlayImageName) returned null");
            return false;
         }
         //--------------------------------
         if (false == CreateXmlMapItemsWoundSpots(aXmlDocument, mapItemsNode, mi.WoundSpots))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateXmlMapItemsWoundSpots() returned false");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("Zoom");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(Zoom) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.Zoom.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(Zoom) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsMoved");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsMoved) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsMoved.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsMoved) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("Count");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(Count) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.Count.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(Count) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("RotationOffsetHull");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(RotationOffsetHull) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.RotationOffsetHull.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(RotationOffsetHull) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("RotationHull");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(RotationHull) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.RotationHull.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(RotationHull) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("RotationOffsetTurret");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(RotationOffsetTurret) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.RotationOffsetTurret.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(RotationOffsetTurret) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("RotationTurret");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(RotationTurret) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.RotationTurret.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(RotationTurret) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("LocationX");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(LocationX) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.Location.X.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(LocationX) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("LocationY");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(LocationY) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.Location.Y.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(LocationY) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("TerritoryCurrent");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(TerritoryCurrent) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.TerritoryCurrent.Name);
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(TerritoryCurrent) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("TerritoryStarting");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(TerritoryStarting) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.TerritoryStarting.Name);
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(TerritoryStarting) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsMoving");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsMoving) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsMoving.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsMoving) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsHullDown");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsHullDown) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsMoving.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsHullDown) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsTurret");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsTurret) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsTurret.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsTurret) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsKilled");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsKilled) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsKilled.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsKilled) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsUnconscious");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsUnconscious) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsUnconscious.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsUnconscious) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsIncapacitated");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsIncapacitated) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsIncapacitated.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsIncapacitated) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsFired");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsFired) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsFired.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsFired) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsSpotted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsSpotted) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsSpotted.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsSpotted) returned null");
            return false;
         }
         //--------------------------------
         if (false == CreateXmlMapItemsEnemyAcquiredShots(aXmlDocument, mapItemsNode, mi.EnemyAcquiredShots))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateXmlMapItemsWoundSpots() returned false");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsVehicle");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsVehicle) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsVehicle.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsVehicle) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsMovingInOpen");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsMovingInOpen) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsMovingInOpen.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsMovingInOpen) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsWoods");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsWoods) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsWoods.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsWoods) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsBuilding");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsBuilding) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsBuilding.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsBuilding) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsFortification");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsFortification) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsFortification.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsFortification) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsThrownTrack");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsThrownTrack) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsThrownTrack.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsThrownTrack) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsBoggedDown");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsBoggedDown) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsBoggedDown.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsBoggedDown) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsAssistanceNeeded");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsAssistanceNeeded) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsAssistanceNeeded.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsAssistanceNeeded) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsHeHit");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsHeHit) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsHeHit.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsHeHit) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsApHit");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsApHit) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.IsApHit.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsApHit) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("Spotting");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(Spotting) returned null");
            return false;
         }
         elem.SetAttribute("value", mi.Spotting.ToString());
         node = mapItemsNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(Spotting) returned null");
            return false;
         }
         return true;
      }
      private bool CreateXmlCrewMember(XmlDocument aXmlDocument, XmlNode parent, ICrewMember cm)
      {
         XmlElement? cmElem = aXmlDocument.CreateElement("CrewMember");
         if (null == cmElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlCrewMember(): CreateElement(miElem) returned null");
            return false;
         }
         cmElem.SetAttribute("value", cm.Role);
         XmlNode? cmNode = parent.AppendChild(cmElem);
         if (null == cmNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlCrewMember(): AppendChild(node) returned null");
            return false;
         }
         //--------------------------------
         IMapItem mi = (IMapItem)cm;
         if( false == CreateXmlMapItem(aXmlDocument, cmNode, mi))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlCrewMember(): CreateXmlMapItem() returned false");
            return false;
         }
         //--------------------------------
         XmlElement? elem = aXmlDocument.CreateElement("Rank");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(Rank) returned null");
            return false;
         }
         elem.SetAttribute("value", cm.Rank);
         XmlNode? node = cmNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(Rank) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("Rating");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(Rating) returned null");
            return false;
         }
         elem.SetAttribute("value", cm.Rating.ToString());
         node = cmNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(Rating) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("IsButtonedUp");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(IsButtonedUp) returned null");
            return false;
         }
         elem.SetAttribute("value", cm.IsButtonedUp.ToString());
         node = cmNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(IsButtonedUp) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("Sector");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(Sector) returned null");
            return false;
         }
         elem.SetAttribute("value", cm.Sector.ToString());
         node = cmNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(Sector) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("Action");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(Action) returned null");
            return false;
         }
         elem.SetAttribute("value", cm.Action);
         node = cmNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(Action) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("Wound");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(Wound) returned null");
            return false;
         }
         elem.SetAttribute("value", cm.Wound);
         node = cmNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(Wound) returned null");
            return false;
         }
         //--------------------------------
         elem = aXmlDocument.CreateElement("WoundDaysUntilReturn");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(WoundDaysUntilReturn) returned null");
            return false;
         }
         elem.SetAttribute("value", cm.WoundDaysUntilReturn.ToString());
         node = cmNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(WoundDaysUntilReturn) returned null");
            return false;
         }
         return true;
      }
      private bool CreateXmlMapItemsWoundSpots(XmlDocument aXmlDocument, XmlNode topNode, List<BloodSpot> woundSpots)
      {
         XmlElement? woundSpotsElem = aXmlDocument.CreateElement("WoundSpots");
         if (null == woundSpotsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): CreateElement(woundsElement) returned null");
            return false;
         }
         woundSpotsElem.SetAttribute("count", woundSpots.Count.ToString());
         XmlNode? woundSpotsNode = topNode.AppendChild(woundSpotsElem);
         if (null == woundSpotsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): AppendChild(woundSpotsNode) returned null");
            return false;
         }
         for ( int k=0; k<woundSpots.Count; ++k)
         {
            XmlElement? spotElem = aXmlDocument.CreateElement("WoundSpot");
            if (null == spotElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): CreateElement(spotElem) returned null");
               return false;
            }
            XmlNode? spotNode = woundSpotsNode.AppendChild(spotElem);
            if (null == spotNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): AppendChild(miNode) returned null");
               return false;
            }
            //--------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("mySize");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): CreateElement(mySize) returned null");
               return false;
            }
            XmlNode? node = spotNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): AppendChild(mySize) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("myLeft");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): CreateElement(myLeft) returned null");
               return false;
            }
            node = spotNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): AppendChild(myLeft) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("myTop");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): CreateElement(myTop) returned null");
               return false;
            }
            node = spotNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): AppendChild(myTop) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlMapItemsEnemyAcquiredShots(XmlDocument aXmlDocument, XmlNode topNode, Dictionary<string, int> enemyAcquiredShots)
      {
         XmlElement? enemyShotsElem = aXmlDocument.CreateElement("EnemyAcquiredShots");
         if (null == enemyShotsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsEnemyAcquiredShots(): CreateElement(enemyShotsElem) returned null");
            return false;
         }
         enemyShotsElem.SetAttribute("count", enemyAcquiredShots.Count.ToString());
         XmlNode? enemyAcquireShotsNode = topNode.AppendChild(enemyShotsElem);
         if (null == enemyAcquireShotsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsEnemyAcquiredShots(): AppendChild(enemyAcquireShotsNode) returned null");
            return false;
         }
         int count = 0;
         foreach(var kvp in enemyAcquiredShots)
         {
            XmlElement? enemyAcqShotElem = aXmlDocument.CreateElement("EnemyAcqShot");
            if (null == enemyAcqShotElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsEnemyAcquiredShots(): CreateElement(spotElem) returned null");
               return false;
            }
            enemyAcqShotElem.SetAttribute("enemy", kvp.Key);
            enemyAcqShotElem.SetAttribute("value", kvp.Value.ToString());
            XmlNode? enemyAcqShotNode = enemyAcquireShotsNode.AppendChild(enemyAcqShotElem);
            if (null == enemyAcqShotNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsEnemyAcquiredShots(): AppendChild(miNode) returned null");
               return false;
            }
            count++;
         }
         if (count != enemyAcquiredShots.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsEnemyAcquiredShots(): count=" + count.ToString() + " enemyAcquiredShots=" + enemyAcquiredShots.Count.ToString());
            return false;
         }
         return true;
      }
      private bool CreateXmlTerritories(XmlDocument aXmlDocument, ITerritories territories, string attribute)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlElement? territoriesElem = aXmlDocument.CreateElement("Territories");
         if (null == territoriesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Territories) returned null");
            return false;
         }
         territoriesElem.SetAttribute("value", attribute);
         territoriesElem.SetAttribute("count", territories.Count.ToString());
         XmlNode? territoriesNode = root.AppendChild(territoriesElem);
         if (null == territoriesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(territoriesNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (Territory t in territories)
         {
            XmlElement? terrElem = aXmlDocument.CreateElement("Territory");  // name of territory
            if (null == terrElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(terrElem) returned null");
               return false;
            }
            terrElem.SetAttribute("value", t.Name);
            XmlNode? territoryNode = territoriesNode.AppendChild(terrElem);
            if (null == territoryNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(territoryNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlFirstShots(XmlDocument aXmlDocument, Dictionary<string, bool> firstShots)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlFirstShots(): root is null");
            return false;
         }
         XmlElement? firstShotsElem = aXmlDocument.CreateElement("FirstShots");
         if (null == firstShotsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlFirstShots(): CreateElement(FirstShots) returned null");
            return false;
         }
         firstShotsElem.SetAttribute("count", firstShots.Count.ToString());
         XmlNode? firstShotsNode = root.AppendChild(firstShotsElem);
         if (null == firstShotsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlFirstShots(): AppendChild(firstShotsNode) returned null");
            return false;
         }
         int count = 0;
         foreach (var kvp in firstShots)
         {
            XmlElement? firstShotElem = aXmlDocument.CreateElement("FirstShot");
            if (null == firstShotElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlFirstShots(): CreateElement(firstShotElem) returned null");
               return false;
            }
            firstShotElem.SetAttribute("enemy", kvp.Key);
            firstShotElem.SetAttribute("value", kvp.Value.ToString());
            XmlNode? firstShotNode = firstShotsNode.AppendChild(firstShotElem);
            if (null == firstShotNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlFirstShots(): AppendChild(firstShotElem) returned null");
               return false;
            }
            count++;
         }
         if( count != firstShots.Count  )
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlFirstShots(): count=" + count.ToString() + " firstShotsCount=" + firstShots.Count.ToString());
            return false;
         }
         return true;
      }
      private bool CreateXmlTrainedGunners(XmlDocument aXmlDocument, List<string> trainedGunners)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlTrainedGunners(): root is null");
            return false;
         }
         XmlElement? trainedGunnersElem = aXmlDocument.CreateElement("TrainedGunners");
         if (null == trainedGunnersElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlTrainedGunners(): CreateElement(TrainedGunners) returned null");
            return false;
         }
         trainedGunnersElem.SetAttribute("count", trainedGunners.Count.ToString());
         XmlNode? trainedGunnersNode = root.AppendChild(trainedGunnersElem);
         if (null == trainedGunnersNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlTrainedGunners(): AppendChild(trainedGunnersNode) returned null");
            return false;
         }
         for(int i=0; i < trainedGunners.Count; ++i )
         {
            XmlElement? trainedGunnerElem = aXmlDocument.CreateElement("FirstShot");
            if (null == trainedGunnerElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlTrainedGunners(): CreateElement(trainedGunnerElem) returned null");
               return false;
            }
            trainedGunnerElem.SetAttribute("value", trainedGunners[i]);
            XmlNode? trainedGunnerNode = trainedGunnersNode.AppendChild(trainedGunnerElem);
            if (null == trainedGunnerNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlTrainedGunners(): AppendChild(trainedGunnerNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlShermanHits(XmlDocument aXmlDocument, List<ShermanAttack> shermanHits)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): root is null");
            return false;
         }
         XmlElement? shermanHitsElem = aXmlDocument.CreateElement("TrainedGunners");
         if (null == shermanHitsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(TrainedGunners) returned null");
            return false;
         }
         shermanHitsElem.SetAttribute("count", shermanHits.Count.ToString());
         XmlNode? shermanHitsNode = root.AppendChild(shermanHitsElem);
         if (null == shermanHitsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(trainedGunnersNode) returned null");
            return false;
         }
         for (int i = 0; i < shermanHits.Count; ++i)
         {
            ShermanAttack shermanAttack = shermanHits[i];
            XmlElement? shermanHitElem = aXmlDocument.CreateElement("ShermanHit");
            if (null == shermanHitElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(ShermanHit) returned null");
               return false;
            }
            shermanHitElem.SetAttribute("myAttackType", shermanAttack.myAttackType);
            shermanHitElem.SetAttribute("myAmmoType", shermanAttack.myAmmoType);
            shermanHitElem.SetAttribute("myIsCriticalHit", shermanAttack.myIsCriticalHit.ToString());
            shermanHitElem.SetAttribute("myHitLocation", shermanAttack.myHitLocation);
            shermanHitElem.SetAttribute("myIsNoChance", shermanAttack.myIsNoChance.ToString());
            shermanHitElem.SetAttribute("myIsImmobilization", shermanAttack.myIsImmobilization.ToString());
            XmlNode? shermanHitNode = shermanHitsNode.AppendChild(shermanHitElem);
            if (null == shermanHitNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(shermanHitNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlShermanDeath(XmlDocument aXmlDocument, ShermanDeath? death)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): root is null");
            return false;
         }
         XmlElement? shermanDeathElem = aXmlDocument.CreateElement("TrainedGunners");
         if (null == shermanDeathElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(TrainedGunners) returned null");
            return false;
         }
         XmlNode? shermanDeathNode = root.AppendChild(shermanDeathElem);
         if (null == shermanDeathNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(trainedGunnersNode) returned null");
            return false;
         }
         if (null == death)
         {
            shermanDeathElem.SetAttribute("value", "null");
            return true;
         }
         //------------------------------------------------
         if( false == CreateXmlMapItem(aXmlDocument, shermanDeathNode, death.myEnemyUnit))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateXmlMapItem() returned null");
            return false;
         }
         //------------------------------------------------
         XmlElement? elem = aXmlDocument.CreateElement("HitLocation");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(HitLocation) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myHitLocation);
         XmlNode? node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(HitLocation) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("EnemyFireDirection");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(EnemyFireDirection) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myEnemyFireDirection);
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(EnemyFireDirection) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("Day");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(Day) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myDay.ToString());
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(Day) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("Cause");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(Cause) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myCause);
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(Cause) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsAmbush");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(IsAmbush) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myIsAmbush.ToString());
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(IsAmbush) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsExplosion");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(IsExplosion) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myIsExplosion.ToString());
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(IsExplosion) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsBailout");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(IsBailout) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myIsBailout.ToString());
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(IsBailout) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrewUp");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(IsBrewUp) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myIsBrewUp.ToString());
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(IsBrewUp) returned null");
            return false;
         }
         return true;
      }
      private bool CreateXmlPanzerfaustAttack(XmlDocument aXmlDocument, PanzerfaustAttack? pfAttack)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): root is null");
            return false;
         }
         XmlElement? pfAttackElem = aXmlDocument.CreateElement("PanzerfaustAttack");
         if (null == pfAttackElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): CreateElement(PanzerfaustAttack) returned null");
            return false;
         }
         XmlNode? pfAttackNode = root.AppendChild(pfAttackElem);
         if (null == pfAttackNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): AppendChild(PanzerfaustAttack) returned null");
            return false;
         }
         if (null == pfAttack)
         {
            pfAttackElem.SetAttribute("value", "null");
            return true;
         }
         //------------------------------------------------
         if (false == CreateXmlMapItem(aXmlDocument, pfAttackNode, pfAttack.myEnemyUnit))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): CreateXmlMapItem() returned null");
            return false;
         }
         //------------------------------------------------
         XmlElement? elem = aXmlDocument.CreateElement("Day");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): CreateElement(Day) returned null");
            return false;
         }
         elem.SetAttribute("value", pfAttack.myDay.ToString());
         XmlNode? node = pfAttackNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): AppendChild(Day) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanMoving");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(IsShermanMoving) returned null");
            return false;
         }
         elem.SetAttribute("value", pfAttack.myIsShermanMoving.ToString());
         node = pfAttackNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(IsShermanMoving) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsLeadTank");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(IsLeadTank) returned null");
            return false;
         }
         elem.SetAttribute("value", pfAttack.myIsLeadTank.ToString());
         node = pfAttackNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(IsLeadTank) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsAdvancingFireZone");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(IsAdvancingFireZone) returned null");
            return false;
         }
         elem.SetAttribute("value", pfAttack.myIsAdvancingFireZone.ToString());
         node = pfAttackNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(IsAdvancingFireZone) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("Sector");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): CreateElement(Sector) returned null");
            return false;
         }
         elem.SetAttribute("value", pfAttack.mySector.ToString());
         node = pfAttackNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlShermanHits(): AppendChild(Sector) returned null");
            return false;
         }
         return true;
      }
   }
}
