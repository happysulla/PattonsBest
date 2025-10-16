using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Collections.Generic;
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
               //--------------------------------
               if (false == CreateXmReports(aXmlDocument, gi.Reports))
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlMapItemsWoundSpots() returned false");
                  return false;
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
         return aXmlDocument;
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
         for (int k=0; k < reports.Count; k++)
         {
            IAfterActionReport? report = reports[k];
            if( null == report )
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): report=null");
               return false;
            }
            XmlElement? reportElem = aXmlDocument.CreateElement("Report");
            if (null == reportElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Report) returned false");
               return false;
            }
            XmlNode? reportNode = reportsNode.AppendChild(reportElem);
            if (null == reportNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(reportNode) returned false");
               return false;
            }
            //------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("Day");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(Day) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Day);
            XmlNode? node = root.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(Day) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Scenario");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(Scenario) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Scenario.ToString());
            node = root.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(Scenario) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Probability");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(Probability) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Probability.ToString());
            node = root.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(Probability) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Resistance");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(Resistance) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Resistance.ToString());
            node = root.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(Resistance) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Name");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(Name) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Name);
            node = root.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(Name) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("TankCardNum");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(TankCardNum) returned false");
               return false;
            }
            elem.SetAttribute("value", report.TankCardNum.ToString());
            node = root.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(TankCardNum) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Weather");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): CreateElement(Weather) returned false");
               return false;
            }
            elem.SetAttribute("value", report.Weather);
            node = root.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Xml(): AppendChild(Weather) returned false");
               return false;
            }
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
      public bool CreateXmlMapItems(XmlDocument aXmlDocument, IMapItems MapItems, string attribute )
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlElement? mapItemsElem = aXmlDocument.CreateElement("MapItems");
         if (null == mapItemsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(MapItemsElem) returned null");
            return false;
         }
         mapItemsElem.SetAttribute("value", attribute);
         mapItemsElem.SetAttribute("count", MapItems.Count.ToString());
         XmlNode? mapItemsNode = root.AppendChild(mapItemsElem);
         if (null == mapItemsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(MapItemsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (IMapItem mi in MapItems)
         {
            XmlElement? miElem = aXmlDocument.CreateElement("MapItem");
            if (null == miElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(miElem) returned null");
               return false;
            }
            XmlNode? node = mapItemsNode.AppendChild(miElem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(miNode) returned null");
               return false;
            }
            //--------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("Name");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Name) returned null");
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
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(TopImageName) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.TopImageName);
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(TopImageName) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("BottomImageName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(BottomImageName) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.BottomImageName);
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(BottomImageName) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("OverlayImageName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(OverlayImageName) returned null");
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
            if( false == CreateXmlMapItemsWoundSpots(aXmlDocument, mapItemsNode, mi.WoundSpots))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlMapItemsWoundSpots() returned false");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Zoom");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Zoom) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Zoom.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(Zoom) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsMoved");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsMoved) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsMoved.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsMoved) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Count");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Count) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Count.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(Count) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("RotationOffsetHull");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(RotationOffsetHull) returned null");
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
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(RotationHull) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.RotationHull.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(RotationHull) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("RotationOffsetTurret");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(RotationOffsetTurret) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.RotationOffsetTurret.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(RotationOffsetTurret) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("RotationTurret");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(RotationTurret) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.RotationTurret.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(RotationTurret) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("LocationX");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(LocationX) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Location.X.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(LocationX) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("LocationY");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(LocationY) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Location.Y.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(LocationY) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("TerritoryCurrent");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(TerritoryCurrent) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.TerritoryCurrent.Name);
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(TerritoryCurrent) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("TerritoryStarting");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(TerritoryStarting) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.TerritoryStarting.Name);
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(TerritoryStarting) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsMoving");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsMoving) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsMoving.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsMoving) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsHullDown");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsHullDown) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsMoving.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsHullDown) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsTurret");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsTurret) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsTurret.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsTurret) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsKilled");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsKilled) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsKilled.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsKilled) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsUnconscious");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsUnconscious) returned null");
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
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsIncapacitated) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsIncapacitated.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsIncapacitated) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsFired");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsFired) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsFired.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsFired) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsSpotted");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsSpotted) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsSpotted.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsSpotted) returned null");
               return false;
            }
            //--------------------------------
            if (false == CreateXmlMapItemsEnemyAcquiredShots(aXmlDocument, mapItemsNode, mi.EnemyAcquiredShots))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlMapItemsWoundSpots() returned false");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsVehicle");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsVehicle) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsVehicle.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsVehicle) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsMovingInOpen");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsMovingInOpen) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsMovingInOpen.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsMovingInOpen) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsWoods");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsWoods) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsWoods.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsWoods) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsBuilding");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsBuilding) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsBuilding.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsBuilding) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsFortification");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsFortification) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsFortification.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsFortification) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsThrownTrack");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsThrownTrack) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsThrownTrack.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsThrownTrack) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsBoggedDown");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsBoggedDown) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsBoggedDown.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsBoggedDown) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsAssistanceNeeded");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsAssistanceNeeded) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsAssistanceNeeded.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsAssistanceNeeded) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsHeHit");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsHeHit) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsHeHit.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsHeHit) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsApHit");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsApHit) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsApHit.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsApHit) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Spotting");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Spotting) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Spotting.ToString());
            node = mapItemsNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(Spotting) returned null");
               return false;
            }
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
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(spotElem) returned null");
               return false;
            }
            XmlNode? spotNode = woundSpotsNode.AppendChild(spotElem);
            if (null == spotNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(miNode) returned null");
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
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): CreateElement(enemyShotsElem) returned null");
            return false;
         }
         enemyShotsElem.SetAttribute("count", enemyAcquiredShots.Count.ToString());
         XmlNode? enemyAcquireShotsNode = topNode.AppendChild(enemyShotsElem);
         if (null == enemyAcquireShotsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemsWoundSpots(): AppendChild(enemyAcquireShotsNode) returned null");
            return false;
         }
         foreach(var kvp in enemyAcquiredShots)
         {
            XmlElement? enemyAcqShotElem = aXmlDocument.CreateElement("EnemyAcqShot");
            if (null == enemyAcqShotElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(spotElem) returned null");
               return false;
            }
            enemyAcqShotElem.SetAttribute("enemy", kvp.Key);
            enemyAcqShotElem.SetAttribute("value", kvp.Value.ToString());
            XmlNode? enemyAcqShotNode = enemyAcquireShotsNode.AppendChild(enemyAcqShotElem);
            if (null == enemyAcqShotNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(miNode) returned null");
               return false;
            }
         }
         return true;
      }
   }
}
