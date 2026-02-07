using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.Design.AxImporter;

namespace Pattons_Best
{
   internal class GameLoadMgr
   {
      public static string theGamesDirectory = "";
      public static IMapItems theMapItems = new MapItems();
      //--------------------------------------------------
      public GameLoadMgr() { }
      //--------------------------------------------------
      public IGameInstance? OpenGame(string filename)
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
            string filenamePlusFilepath = theGamesDirectory + filename;
            //-------------------------------------
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            IGameInstance? gi = ReadXmlGameInstance(filenamePlusFilepath);
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            //-------------------------------------
            if (null == gi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Open_Game(): ReadXmlGameInstance() returned null for " + filename);
               return null;
            }
            Logger.Log(LogEnum.LE_GAME_INIT, "Open_Game(): gi=" + gi.ToString());
            return gi;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Open_Game(): path=" + theGamesDirectory + " e =" + e.ToString());
            return new GameInstance();
         }
      }
      //--------------------------------------------------
      public bool SaveGame(IGameInstance gi, string filename)
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Save_Game(): path=" + theGamesDirectory + " e=" + e.ToString());
            return false;
         }
         try
         {
            //--------------------------------------
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            XmlDocument? aXmlDocument = CreateXmlGameInstance(gi); // create a new XML document
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            //--------------------------------------
            if (null == aXmlDocument)
            {
               Logger.Log(LogEnum.LE_ERROR, "Save_Game(): CreateXmlGameInstance() returned null for path=" + theGamesDirectory);
               return false;
            }
            string filenamePlusPath = theGamesDirectory + filename;
            using (FileStream writer = new FileStream(filenamePlusPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
               XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
               using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
               {
                  aXmlDocument.Save(xmlWriter);
               }
            }
            return true;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Save_Game(): path=" + theGamesDirectory + " e =" + ex.ToString());
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
            Logger.Log(LogEnum.LE_ERROR, "Open_GameFromFile(): path=" + theGamesDirectory + " e=" + e.ToString());
            return null;
         }
         try
         {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = theGamesDirectory;
            dlg.RestoreDirectory = true;
            dlg.Filter = "Patton's Best Games|*.pbg";
            if (true == dlg.ShowDialog())
            {
               CultureInfo currentCulture = CultureInfo.CurrentCulture;
               System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
               IGameInstance? gi = ReadXmlGameInstance(dlg.FileName);
               System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
               if (null == gi)
               {
                  Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
                  Logger.Log(LogEnum.LE_ERROR, "Open_GameFromFile(): ReadXmlGameInstance() returned null for " + dlg.FileName);
                  return null;
               }
               Logger.Log(LogEnum.LE_GAME_INIT, "Open_GameFromFile(): gi=" + gi.ToString());
               string? gamePath = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               if (null == gamePath)
               {
                  Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
                  Logger.Log(LogEnum.LE_ERROR, "Open_GameFromFile(): Path.GetDirectoryName() returned null for fn=" + dlg.FileName);
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
            Logger.Log(LogEnum.LE_ERROR, "Open_GameFromFile(): path=" + theGamesDirectory + " e =" + e.ToString());
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
               CultureInfo currentCulture = CultureInfo.CurrentCulture;
               System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
               XmlDocument? aXmlDocument = CreateXmlGameInstance(gi); // create a new XML document
               System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
               if (null == aXmlDocument)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): CreateXmlGameInstance() returned null for path=" + theGamesDirectory);
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
         IGameCommand? command = gi.GameCommands.GetLast();
         if( null != command )
            sb.Append("-" + command.Action.ToString());
         sb.Append(".pbg");
         return sb.ToString();
      }
      //--------------------------------------------------
      private int GetMajorVersion()
      {
         Assembly assembly = Assembly.GetExecutingAssembly();
         if (null == assembly)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): Assembly.GetExecutingAssembly()=null");
            return -1;
         }
         Version? versionRunning = assembly.GetName().Version;
         if (null == versionRunning)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance():  assembly.GetName().Version=null");
            return -1;
         }
         return versionRunning.Major;
      }
      public bool ReadXmlTerritories(XmlReader reader, ITerritories territories) // initial loading of Territories.theTerritories
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         try
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(Territories)=false");
               return false;
            }
            if (reader.Name != "Territories")
            {
               Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): Territories != (node=" + reader.Name + ")");
               return false;
            }
            //-----------------------------------------------------------------
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): Territories.Count=null");
               return false;
            }
            int count = int.Parse(sCount);
            //-----------------------------------------------------------------
            for (int i = 0; i < count; ++i)
            {
               ITerritory territory = new Territory();
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(Territory)=false count=" + count.ToString() + " i=" + i.ToString());
                  return false;
               }
               if (reader.Name != "Territory")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): Territory != (node=" + reader.Name + ")");
                  return false;
               }
               string? tName = reader.GetAttribute("value");
               if (null == tName)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute() returned false");
                  return false;
               }
               territory.Name = tName;
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(Parent)=false tName=" + tName);
                  return false;
               }
               if (reader.Name != "Parent")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): Parent != (node=" + reader.Name + ")");
                  return false;
               }
               string? sAttribute = reader.GetAttribute("value");
               if (null == sAttribute)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(Parent)=null");
                  return false;
               }
               territory.CanvasName = sAttribute;
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(Type)=false");
                  return false;
               }
               if (reader.Name != "Type")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): Type != (node=" + reader.Name + ")");
                  return false;
               }
               string? sAttribute1 = reader.GetAttribute("value");
               if (null == sAttribute1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(Type)=null");
                  return false;
               }
               territory.Type = sAttribute1;
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(CenterPoint)=false");
                  return false;
               }
               if (reader.Name != "CenterPoint")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): CenterPoint != (node=" + reader.Name + ")");
                  return false;
               }
               string? sX = reader.GetAttribute("X");
               if (null == sX)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(sX)=null");
                  return false;
               }
               territory.CenterPoint.X = double.Parse(sX);
               string? sY = reader.GetAttribute("Y");
               if (null == sY)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(sX)=null");
                  return false;
               }
               territory.CenterPoint.Y = double.Parse(sY);
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(Points)=false");
                  return false;
               }
               if (reader.Name != "Points")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): Points != (node=" + reader.Name + ")");
                  return false;
               }
               string? sCount0 = reader.GetAttribute("count");
               if (null == sCount0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(sCount0)=null");
                  return false;
               }
               int count0 = int.Parse(sCount0);
               for (int i1 = 0; i1 < count0; ++i1)
               {
                  reader.Read();
                  if (false == reader.IsStartElement())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(regionPoint)=false");
                     return false;
                  }
                  if (reader.Name != "regionPoint")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): regionPoint != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sX1 = reader.GetAttribute("X");
                  if (null == sX1)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(sX1)=null");
                     return false;
                  }
                  string? sY1 = reader.GetAttribute("Y");
                  if (null == sY1)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(sY1)=null");
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
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(Adjacents)=false");
                  return false;
               }
               if (reader.Name != "Adjacents")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): Adjacents != (node=" + reader.Name + ")");
                  return false;
               }
               string? sCount3 = reader.GetAttribute("count");
               if (null == sCount3)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(sCount3)=null");
                  return false;
               }
               int count3 = int.Parse(sCount3);
               for (int i3 = 0; i3 < count3; ++i3)
               {
                  reader.Read();
                  if (false == reader.IsStartElement())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(adjacent)=false");
                     return false;
                  }
                  if (reader.Name != "adjacent")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): adjacent != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sAdjacent = reader.GetAttribute("value");
                  if (null == sAdjacent)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(sAdjacent)=null");
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
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(PavedRoads)=false");
                  return false;
               }
               if (reader.Name != "PavedRoads")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): PavedRoads != (node=" + reader.Name + ")");
                  return false;
               }
               string? sCount4 = reader.GetAttribute("count");
               if (null == sCount4)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(sCount4)=null");
                  return false;
               }
               int count4 = int.Parse(sCount4);
               for (int i4 = 0; i4 < count4; ++i4)
               {
                  reader.Read();
                  if (false == reader.IsStartElement())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(paved)=false");
                     return false;
                  }
                  if (reader.Name != "paved")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): paved != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sPaved = reader.GetAttribute("value");
                  if (null == sPaved)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(sPaved)=null");
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
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(UnpavedRoads)=false");
                  return false;
               }
               if (reader.Name != "UnpavedRoads")
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): UnpavedRoads != (node=" + reader.Name + ")");
                  return false;
               }
               string? sCount5 = reader.GetAttribute("count");
               if (null == sCount5)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(sCount5)=null");
                  return false;
               }
               int count5 = int.Parse(sCount5);
               for (int i5 = 0; i5 < count5; ++i5)
               {
                  reader.Read();
                  if (false == reader.IsStartElement())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): IsStartElement(unpaved)=false");
                     return false;
                  }
                  if (reader.Name != "unpaved")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): unpaved != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sUnpaved = reader.GetAttribute("value");
                  if (null == sUnpaved)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Read_XmlTerritories(): GetAttribute(sUnpaved)=null");
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
         }
         finally
         {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         }
         System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         return true;
      }
      public bool CreateXmlTerritories(XmlDocument aXmlDocument, ITerritories territories) // initial creation of Territories.theTerritories during unit testing
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         try
         {
            XmlNode? root = aXmlDocument.DocumentElement;
            if (null == root)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): root is null");
               return false;
            }
            XmlAttribute xmlAttribute = aXmlDocument.CreateAttribute("count");
            xmlAttribute.Value = territories.Count.ToString();
            if (null == root.Attributes)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): root.Attributes is null");
               return false;
            }
            root.Attributes.Append(xmlAttribute);
            //--------------------------------
            foreach (Territory t in territories)
            {
               XmlElement? terrElem = aXmlDocument.CreateElement("Territory");  // name of territory
               if (null == terrElem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(terrElem) returned null");
                  return false;
               }
               terrElem.SetAttribute("value", t.Name);
               XmlNode? territoryNode = root.AppendChild(terrElem);
               if (null == territoryNode)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(territoryNode) returned null");
                  return false;
               }
               //---------------------------------
               XmlElement? elem = aXmlDocument.CreateElement("Parent");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(terrElem) returned null");
                  return false;
               }
               elem.SetAttribute("value", t.CanvasName);
               XmlNode? node = territoryNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(node) returned null");
                  return false;
               }
               //---------------------------------
               elem = aXmlDocument.CreateElement("Type");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(terrElem) returned null");
                  return false;
               }
               elem.SetAttribute("value", t.Type);
               node = territoryNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(node) returned null");
                  return false;
               }
               //---------------------------------
               elem = aXmlDocument.CreateElement("CenterPoint");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(CenterPoint) returned null");
                  return false;
               }
               elem.SetAttribute("X", t.CenterPoint.X.ToString("0000.00"));
               elem.SetAttribute("Y", t.CenterPoint.Y.ToString("0000.00"));
               node = territoryNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(node) returned null");
                  return false;
               }
               //---------------------------------
               XmlElement? elemPoints = aXmlDocument.CreateElement("Points");
               if (null == elemPoints)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(elemPoints) returned null");
                  return false;
               }
               elemPoints.SetAttribute("count", t.Points.Count.ToString());
               XmlNode? nodePoints = territoryNode.AppendChild(elemPoints);
               if (null == nodePoints)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(nodePoints) returned null");
                  return false;
               }
               //---------------------------------
               foreach (IMapPoint mp in t.Points)
               {
                  elem = aXmlDocument.CreateElement("regionPoint");
                  if (null == elem)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(terrElem) returned null");
                     return false;
                  }
                  elem.SetAttribute("X", mp.X.ToString("0000.00"));
                  elem.SetAttribute("Y", mp.Y.ToString("0000.00"));
                  node = nodePoints.AppendChild(elem);
                  if (null == node)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(node) returned null");
                     return false;
                  }
               }
               //-----------------------------------------------------------
               XmlElement? elemAdjacents = aXmlDocument.CreateElement("Adjacents");
               if (null == elemAdjacents)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(elemAdjacents) returned null");
                  return false;
               }
               elemAdjacents.SetAttribute("count", t.Adjacents.Count.ToString());
               XmlNode? nodeAdjacents = territoryNode.AppendChild(elemAdjacents);
               if (null == nodeAdjacents)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(nodePoints) returned null");
                  return false;
               }
               //---------------------------------
               foreach (string s in t.Adjacents)
               {
                  elem = aXmlDocument.CreateElement("adjacent");
                  if (null == elem)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(adjacent) returned null");
                     return false;
                  }
                  elem.SetAttribute("value", s);
                  node = nodeAdjacents.AppendChild(elem);
                  if (null == node)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(nodeAdjacents) returned null");
                     return false;
                  }
               }
               //-----------------------------------------------------------
               XmlElement? elemPavedRoads = aXmlDocument.CreateElement("PavedRoads");
               if (null == elemPavedRoads)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(elemPavedRoads) returned null");
                  return false;
               }
               elemPavedRoads.SetAttribute("count", t.PavedRoads.Count.ToString());
               XmlNode? nodePavedRoads = territoryNode.AppendChild(elemPavedRoads);
               if (null == nodePavedRoads)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(nodePavedRoads) returned null");
                  return false;
               }
               //---------------------------------
               foreach (string s in t.PavedRoads)
               {
                  elem = aXmlDocument.CreateElement("paved");
                  if (null == elem)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(paved) returned null");
                     return false;
                  }
                  elem.SetAttribute("value", s);
                  node = nodePavedRoads.AppendChild(elem);
                  if (null == node)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(paved) returned null");
                     return false;
                  }
               }
               //-----------------------------------------------------------
               XmlElement? elemUnpavedRoads = aXmlDocument.CreateElement("UnpavedRoads");
               if (null == elemUnpavedRoads)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(elemUnpavedRoads) returned null");
                  return false;
               }
               elemUnpavedRoads.SetAttribute("count", t.UnpavedRoads.Count.ToString());
               XmlNode? nodeUnpavedRoads = territoryNode.AppendChild(elemUnpavedRoads);
               if (null == nodeUnpavedRoads)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(nodeUnpavedRoads) returned null");
                  return false;
               }
               //---------------------------------
               foreach (string s in t.UnpavedRoads)
               {
                  elem = aXmlDocument.CreateElement("unpaved");
                  if (null == elem)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): CreateElement(unpaved) returned null");
                     return false;
                  }
                  elem.SetAttribute("value", s);
                  node = nodeUnpavedRoads.AppendChild(elem);
                  if (null == node)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Create_XmlTerritories(): AppendChild(unpaved) returned null");
                     return false;
                  }
               }
            }
         }
         finally
         {

         }
         return true;
      }
      private GameAction StringToGameAction(string sGameAction)
      {
         switch (sGameAction)
         {
            case "RemoveSplashScreen": return GameAction.RemoveSplashScreen;
            case "UpdateStatusBar": return GameAction.UpdateStatusBar;
            case "UpdateTankCard": return GameAction.UpdateTankCard;
            case "UpdateAfterActionReport": return GameAction.UpdateAfterActionReport;
            case "UpdateBattleBoard": return GameAction.UpdateBattleBoard;
            case "UpdateTankExplosion": return GameAction.UpdateTankExplosion;
            case "UpdateTankBrewUp": return GameAction.UpdateTankBrewUp;
            case "UpdateShowRegion": return GameAction.UpdateShowRegion;
            case "UpdateEventViewerDisplay": return GameAction.UpdateEventViewerDisplay;
            case "UpdateEventViewerActive": return GameAction.UpdateEventViewerActive;
            case "DieRollActionNone": return GameAction.DieRollActionNone;

            case "UpdateNewGame": return GameAction.UpdateNewGame;
            case "UpdateNewGameEnd": return GameAction.UpdateNewGameEnd;
            case "UpdateGameOptions": return GameAction.UpdateGameOptions;
            case "UpdateLoadingGame": return GameAction.UpdateLoadingGame;
            case "UpdateUndo": return GameAction.UpdateUndo;

            case "TestingStartMorningBriefing": return GameAction.TestingStartMorningBriefing;
            case "TestingStartPreparations": return GameAction.TestingStartPreparations;
            case "TestingStartMovement": return GameAction.TestingStartMovement;
            case "TestingStartBattle": return GameAction.TestingStartBattle;
            case "TestingStartAmbush": return GameAction.TestingStartAmbush;

            case "ShowCombatCalendarDialog": return GameAction.ShowCombatCalendarDialog;
            case "ShowAfterActionReportDialog": return GameAction.ShowAfterActionReportDialog;
            case "ShowTankForcePath": return GameAction.ShowTankForcePath;
            case "ShowMovementDiagramDialog": return GameAction.ShowMovementDiagramDialog;
            case "ShowRoads": return GameAction.ShowRoads;
            case "ShowRuleListingDialog": return GameAction.ShowRuleListingDialog;
            case "ShowEventListingDialog": return GameAction.ShowEventListingDialog;
            case "ShowTableListing": return GameAction.ShowTableListing;
            case "ShowGameFeatsDialog": return GameAction.ShowGameFeatsDialog;
            case "ShowReportErrorDialog": return GameAction.ShowReportErrorDialog;
            case "ShowAboutDialog": return GameAction.ShowAboutDialog;

            case "UnitTestStart": return GameAction.UnitTestStart;
            case "UnitTestCommand": return GameAction.UnitTestCommand;
            case "UnitTestNext": return GameAction.UnitTestNext;
            case "UnitTestTest": return GameAction.UnitTestTest;
            case "UnitTestCleanup": return GameAction.UnitTestCleanup;

            case "EndCampaignGameWin": return GameAction.EndGameWin;
            case "EndGameLost": return GameAction.EndGameLost;
            case "EndGameShowFeats": return GameAction.EndGameShowFeats;
            case "EndGameShowStats": return GameAction.EndGameShowStats;
            case "EndGameClose": return GameAction.EndGameClose;
            case "EndGameExit": return GameAction.EndGameExit;
            case "ExitGame": return GameAction.ExitGame;

            case "SetupShowMapHistorical": return GameAction.SetupShowMapHistorical;
            case "SetupShowMovementBoard": return GameAction.SetupShowMovementBoard;
            case "SetupShowBattleBoard": return GameAction.SetupShowBattleBoard;
            case "SetupShowTankCard": return GameAction.SetupShowTankCard;
            case "SetupShowAfterActionReport": return GameAction.SetupShowAfterActionReport;
            case "SetupAssignCrewRating": return GameAction.SetupAssignCrewRating;
            case "SetupShowCombatCalendarCheck": return GameAction.SetupShowCombatCalendarCheck;
            case "SetupCombatCalendarRoll": return GameAction.SetupCombatCalendarRoll;
            case "SetupFinalize": return GameAction.SetupFinalize;
            case "SetupSingleGameDay": return GameAction.SetupSingleGameDay;
            case "SetupDecreaseDate": return GameAction.SetupDecreaseDate;
            case "SetupIncreaseDate": return GameAction.SetupIncreaseDate;
            case "SetupShowSingleDayBattleStart": return GameAction.SetupShowSingleDayBattleStart;

            case "MorningBriefingBegin": return GameAction.MorningBriefingBegin;
            case "MorningBriefingCrewmanHealing": return GameAction.MorningBriefingCrewmanHealing;
            case "MorningBriefingAssignCrewRating": return GameAction.MorningBriefingAssignCrewRating;
            case "MorningBriefingExistingCrewman": return GameAction.MorningBriefingExistingCrewman;
            case "MorningBriefingReturningCrewman": return GameAction.MorningBriefingReturningCrewman;
            case "MorningBriefingAssignCrewRatingEnd": return GameAction.MorningBriefingAssignCrewRatingEnd;
            case "MorningBriefingTankReplaceChoice": return GameAction.MorningBriefingTankReplaceChoice;
            case "MorningBriefingTankKeepChoice": return GameAction.MorningBriefingTankKeepChoice;
            case "MorningBriefingTrainCrew": return GameAction.MorningBriefingTrainCrew;
            case "EveningDebriefingRatingTrainingEnd": return GameAction.EveningDebriefingRatingTrainingEnd;
            case "MorningBriefingTrainCrewHvssEnd": return GameAction.MorningBriefingTrainCrewHvssEnd;
            case "MorningBriefingTankReplacementRoll": return GameAction.MorningBriefingTankReplacementRoll;
            case "MorningBriefingTankReplacementHvssRoll": return GameAction.MorningBriefingTankReplacementHvssRoll;
            case "MorningBriefingDecreaseTankNum": return GameAction.MorningBriefingDecreaseTankNum;
            case "MorningBriefingIncreaseTankNum": return GameAction.MorningBriefingIncreaseTankNum;
            case "MorningBriefingTankReplacementEnd": return GameAction.MorningBriefingTankReplacementEnd;
            case "MorningBriefingCalendarRoll": return GameAction.MorningBriefingCalendarRoll;
            case "MorningBriefingWeatherRoll": return GameAction.MorningBriefingWeatherRoll;
            case "MorningBriefingWeatherRollEnd": return GameAction.MorningBriefingWeatherRollEnd;
            case "MorningBriefingSnowRoll": return GameAction.MorningBriefingSnowRoll;
            case "MorningBriefingSnowRollEnd": return GameAction.MorningBriefingSnowRollEnd;
            case "MorningBriefingAmmoLoad": return GameAction.MorningBriefingAmmoLoad;
            case "MorningBriefingAmmoLoadSkip": return GameAction.MorningBriefingAmmoLoadSkip;
            case "MorningBriefingAmmoReadyRackLoad": return GameAction.MorningBriefingAmmoReadyRackLoad;
            case "MorningBriefingTimeCheck": return GameAction.MorningBriefingTimeCheck;
            case "MorningBriefingTimeCheckRoll": return GameAction.MorningBriefingTimeCheckRoll;
            case "MorningBriefingDayOfRest": return GameAction.MorningBriefingDayOfRest;
            case "MorningBriefingDeployment": return GameAction.MorningBriefingDeployment;

            case "PreparationsDeploymentRoll": return GameAction.PreparationsDeploymentRoll;
            case "PreparationsHatches": return GameAction.PreparationsHatches;
            case "PreparationsShowHatchAction": return GameAction.PreparationsShowHatchAction;
            case "PreparationsGunLoad": return GameAction.PreparationsGunLoad;
            case "PreparationsLoader": return GameAction.PreparationsLoader;
            case "PreparationsGunLoadSelect": return GameAction.PreparationsGunLoadSelect;
            case "PreparationsTurret": return GameAction.PreparationsTurret;
            case "PreparationsTurretRotateLeft": return GameAction.PreparationsTurretRotateLeft;
            case "PreparationsTurretRotateRight": return GameAction.PreparationsTurretRotateRight;
            case "PreparationsLoaderSpot": return GameAction.PreparationsLoaderSpot;
            case "PreparationsLoaderSpotSet": return GameAction.PreparationsLoaderSpotSet;
            case "PreparationsCommanderSpot": return GameAction.PreparationsCommanderSpot;
            case "PreparationsCommanderSpotSet": return GameAction.PreparationsCommanderSpotSet;
            case "PreparationsFinal": return GameAction.PreparationsFinal;
            case "PreparationsFinalSkip": return GameAction.PreparationsFinalSkip;
            case "PreparationsShowFeat": return GameAction.PreparationsShowFeat;
            case "PreparationsShowFeatEnd": return GameAction.PreparationsShowFeatEnd;
            case "PreparationsRepairMainGunRoll": return GameAction.PreparationsRepairMainGunRoll;
            case "PreparationsRepairMainGunRollEnd": return GameAction.PreparationsRepairMainGunRollEnd;
            case "PreparationsRepairAaMgRoll": return GameAction.PreparationsRepairAaMgRoll;
            case "PreparationsRepairAaMgRollEnd": return GameAction.PreparationsRepairAaMgRollEnd;
            case "PreparationsRepairCoaxialMgRoll": return GameAction.PreparationsRepairCoaxialMgRoll;
            case "PreparationsRepairCoaxialMgRollEnd": return GameAction.PreparationsRepairCoaxialMgRollEnd;
            case "PreparationsRepairBowMgRoll": return GameAction.PreparationsRepairBowMgRoll;
            case "PreparationsRepairBowMgRollEnd": return GameAction.PreparationsRepairBowMgRollEnd;
            case "PreparationsCrewReplaced": return GameAction.PreparationsCrewReplaced;
            case "PreparationsReadyRackEnd": return GameAction.PreparationsReadyRackEnd;

            case "MovementStartAreaSet": return GameAction.MovementStartAreaSet;
            case "MovementStartAreaSetRoll": return GameAction.MovementStartAreaSetRoll;
            case "MovementExitAreaSet": return GameAction.MovementExitAreaSet;
            case "MovementExitAreaSetRoll": return GameAction.MovementExitAreaSetRoll;
            case "MovementEnemyStrengthChoice": return GameAction.MovementEnemyStrengthChoice;
            case "MovementEnemyStrengthCheckTerritory": return GameAction.MovementEnemyStrengthCheckTerritory;
            case "MovementEnemyStrengthCheckTerritoryRoll": return GameAction.MovementEnemyStrengthCheckTerritoryRoll;
            case "MovementEnemyCheckCounterattack": return GameAction.MovementEnemyCheckCounterattack;
            case "MovementBattleCheckCounterattackRoll": return GameAction.MovementBattleCheckCounterattackRoll;
            case "MovementCounterattackEllapsedTimeRoll": return GameAction.MovementCounterattackEllapsedTimeRoll;
            case "MovementBattlePhaseStartCounterattack": return GameAction.MovementBattlePhaseStartCounterattack;
            case "MovementChooseOption": return GameAction.MovementChooseOption;
            case "MovementArtillerySupportChoice": return GameAction.MovementArtillerySupportChoice;
            case "MovementArtillerySupportCheck": return GameAction.MovementArtillerySupportCheck;
            case "MovementArtillerySupportCheckRoll": return GameAction.MovementArtillerySupportCheckRoll;
            case "MovementAirStrikeChoice": return GameAction.MovementAirStrikeChoice;
            case "MovementAirStrikeCheckTerritory": return GameAction.MovementAirStrikeCheckTerritory;
            case "MovementAirStrikeCheckTerritoryRoll": return GameAction.MovementAirStrikeCheckTerritoryRoll;
            case "MovementAirStrikeCancel": return GameAction.MovementAirStrikeCancel;
            case "MovementResupplyCheck": return GameAction.MovementResupplyCheck;
            case "MovementResupplyCheckRoll": return GameAction.MovementResupplyCheckRoll;
            case "MovementAmmoLoad": return GameAction.MovementAmmoLoad;
            case "MovementEnterArea": return GameAction.MovementEnterArea;
            case "MovementAdvanceFireChoice": return GameAction.MovementAdvanceFireChoice;
            case "MovementAdvanceFireAmmoUseCheck": return GameAction.MovementAdvanceFireAmmoUseCheck;
            case "MovementAdvanceFireAmmoUseRoll": return GameAction.MovementAdvanceFireAmmoUseRoll;
            case "MovementAdvanceFire": return GameAction.MovementAdvanceFire;
            case "MovementAdvanceFireSkip": return GameAction.MovementAdvanceFireSkip;
            case "MovementEnterAreaUsControl": return GameAction.MovementEnterAreaUsControl;
            case "MovementStrengthBattleBoardRoll": return GameAction.MovementStrengthBattleBoardRoll;
            case "MovementBattleCheck": return GameAction.MovementBattleCheck;
            case "MovementBattleCheckRoll": return GameAction.MovementBattleCheckRoll;
            case "MovementStartAreaRestart": return GameAction.MovementStartAreaRestart;
            case "MovementStartAreaRestartAfterBattle": return GameAction.MovementStartAreaRestartAfterBattle;
            case "MovementExit": return GameAction.MovementExit;
            case "MovementRetreatStartBattle": return GameAction.MovementRetreatStartBattle;
            case "MovementBattlePhaseStartDueToAdvance": return GameAction.MovementBattlePhaseStartDueToAdvance;
            case "MovementRainRoll": return GameAction.MovementRainRoll;
            case "MovementRainRollEnd": return GameAction.MovementRainRollEnd;
            case "MovementSnowRoll": return GameAction.MovementSnowRoll;
            case "MovementSnowRollEnd": return GameAction.MovementSnowRollEnd;
            case "MovementBattlePhaseStartDueToRetreat": return GameAction.MovementBattlePhaseStartDueToRetreat;

            case "BattleAdvanceFireStart": return GameAction.BattleAdvanceFireStart;
            case "BattleActivation": return GameAction.BattleActivation;
            case "BattlePlaceAdvanceFire": return GameAction.BattlePlaceAdvanceFire;
            case "BattleResolveAdvanceFire": return GameAction.BattleResolveAdvanceFire;
            case "BattleResolveArtilleryFire": return GameAction.BattleResolveArtilleryFire;
            case "BattleResolveOffoardArtilleryFire": return GameAction.BattleResolveOffoardArtilleryFire;
            case "BattleResolveAirStrike": return GameAction.BattleResolveAirStrike;
            case "BattleAmbushStart": return GameAction.BattleAmbushStart;
            case "BattleAmbushRoll": return GameAction.BattleAmbushRoll;
            case "BattleSetupEnd": return GameAction.BattleSetupEnd;
            case "BattleAmbush": return GameAction.BattleAmbush;
            case "BattleRandomEvent": return GameAction.BattleRandomEvent;
            case "BattleRandomEventRoll": return GameAction.BattleRandomEventRoll;
            case "BattleCollateralDamageCheck": return GameAction.BattleCollateralDamageCheck;
            case "BattleCrewReplaced": return GameAction.BattleCrewReplaced;
            case "BattleEmpty": return GameAction.BattleEmpty;
            case "BattleEmptyResolve": return GameAction.BattleEmptyResolve;
            case "BattleShermanKilled": return GameAction.BattleShermanKilled;

            case "BattleRoundSequenceRoundStart": return GameAction.BattleRoundSequenceRoundStart;
            case "BattleRoundSequenceAmbushCounterattack": return GameAction.BattleRoundSequenceAmbushCounterattack;
            case "BattleRoundSequenceSmokeDepletionEnd": return GameAction.BattleRoundSequenceSmokeDepletionEnd;
            case "BattleRoundSequenceSpotting": return GameAction.BattleRoundSequenceSpotting;
            case "BattleRoundSequenceSpottingEnd": return GameAction.BattleRoundSequenceSpottingEnd;
            case "BattleRoundSequenceCrewOrders": return GameAction.BattleRoundSequenceCrewOrders;
            case "BattleRoundSequenceAmmoOrders": return GameAction.BattleRoundSequenceAmmoOrders;
            case "BattleRoundSequenceConductCrewAction": return GameAction.BattleRoundSequenceConductCrewAction;

            case "BattleRoundSequenceMovementRoll": return GameAction.BattleRoundSequenceMovementRoll;
            case "BattleRoundSequenceBoggedDownRoll": return GameAction.BattleRoundSequenceBoggedDownRoll;
            case "BattleRoundSequencePivot": return GameAction.BattleRoundSequencePivot;
            case "BattleRoundSequencePivotLeft": return GameAction.BattleRoundSequencePivotLeft;
            case "BattleRoundSequencePivotRight": return GameAction.BattleRoundSequencePivotRight;
            case "BattleRoundSequenceMovementPivotEnd": return GameAction.BattleRoundSequenceMovementPivotEnd;
            case "BattleRoundSequenceChangeFacing": return GameAction.BattleRoundSequenceChangeFacing;
            case "BattleRoundSequenceChangeFacingEnd": return GameAction.BattleRoundSequenceChangeFacingEnd;
            case "BattleRoundSequenceTurretEnd": return GameAction.BattleRoundSequenceTurretEnd;
            case "BattleRoundSequenceTurretEndRotateLeft": return GameAction.BattleRoundSequenceTurretEndRotateLeft;
            case "BattleRoundSequenceTurretEndRotateRight": return GameAction.BattleRoundSequenceTurretEndRotateRight;

            case "BattleRoundSequenceShermanFiringSelectTarget": return GameAction.BattleRoundSequenceShermanFiringSelectTarget;
            case "BattleRoundSequenceShermanFiringMainGun": return GameAction.BattleRoundSequenceShermanFiringMainGun;
            case "BattleRoundSequenceShermanFiringMainGunArea": return GameAction.BattleRoundSequenceShermanFiringMainGunArea;
            case "BattleRoundSequenceShermanFiringMainGunDirect": return GameAction.BattleRoundSequenceShermanFiringMainGunDirect;
            case "BattleRoundSequenceShermanFiringMainGunEnd": return GameAction.BattleRoundSequenceShermanFiringMainGunEnd;
            case "BattleRoundSequenceShermanFiringMainGunSkip": return GameAction.BattleRoundSequenceShermanFiringMainGunSkip;
            case "BattleRoundSequenceShermanToHitRoll": return GameAction.BattleRoundSequenceShermanToHitRoll;
            case "BattleRoundSequenceShermanSkipRateOfFire": return GameAction.BattleRoundSequenceShermanSkipRateOfFire;
            case "BattleRoundSequenceShermanMissesLastShot": return GameAction.BattleRoundSequenceShermanMissesLastShot;
            case "BattleRoundSequenceShermanToKillRoll": return GameAction.BattleRoundSequenceShermanToKillRoll;
            case "BattleRoundSequenceShermanToHitRollNothing": return GameAction.BattleRoundSequenceShermanToHitRollNothing;
            case "BattleRoundSequenceShermanToKillRollMiss": return GameAction.BattleRoundSequenceShermanToKillRollMiss;

            case "BattleRoundSequenceShermanFiringSelectTargetMg": return GameAction.BattleRoundSequenceShermanFiringSelectTargetMg;
            case "BattleRoundSequenceFireAaMg": return GameAction.BattleRoundSequenceFireAaMg;
            case "BattleRoundSequenceFireBowMg": return GameAction.BattleRoundSequenceFireBowMg;
            case "BattleRoundSequenceFireCoaxialMg": return GameAction.BattleRoundSequenceFireCoaxialMg;
            case "BattleRoundSequenceFireSubMg": return GameAction.BattleRoundSequenceFireSubMg;
            case "BattleRoundSequenceFireMgSkip": return GameAction.BattleRoundSequenceFireMgSkip;
            case "BattleRoundSequenceShermanFiringMachineGun": return GameAction.BattleRoundSequenceShermanFiringMachineGun;
            case "BattleRoundSequenceFireMachineGunRoll": return GameAction.BattleRoundSequenceFireMachineGunRoll;
            case "BattleRoundSequenceFireMachineGunRollEnd": return GameAction.BattleRoundSequenceFireMachineGunRollEnd;
            case "BattleRoundSequenceMgPlaceAdvanceFire": return GameAction.BattleRoundSequenceMgPlaceAdvanceFire;
            case "BattleRoundSequenceMgAdvanceFireRoll": return GameAction.BattleRoundSequenceMgAdvanceFireRoll;
            case "BattleRoundSequenceMgAdvanceFireRollEnd": return GameAction.BattleRoundSequenceMgAdvanceFireRollEnd;
            case "BattleRoundSequenceReplacePeriscopes": return GameAction.BattleRoundSequenceReplacePeriscopes;
            case "BattleRoundSequenceRepairMainGunRoll": return GameAction.BattleRoundSequenceRepairMainGunRoll;
            case "BattleRoundSequenceRepairAaMgRoll": return GameAction.BattleRoundSequenceRepairAaMgRoll;
            case "BattleRoundSequenceRepairCoaxialMgRoll": return GameAction.BattleRoundSequenceRepairCoaxialMgRoll;
            case "BattleRoundSequenceRepairBowMgRoll": return GameAction.BattleRoundSequenceRepairBowMgRoll;
            case "BattleRoundSequenceShermanFiringMortar": return GameAction.BattleRoundSequenceShermanFiringMortar;
            case "BattleRoundSequenceShermanThrowGrenade": return GameAction.BattleRoundSequenceShermanThrowGrenade;
            case "BattleRoundSequenceReadyRackHeMinus": return GameAction.BattleRoundSequenceReadyRackHeMinus;
            case "BattleRoundSequenceReadyRackApMinus": return GameAction.BattleRoundSequenceReadyRackApMinus;
            case "BattleRoundSequenceReadyRackWpMinus": return GameAction.BattleRoundSequenceReadyRackWpMinus;
            case "BattleRoundSequenceReadyRackHbciMinus": return GameAction.BattleRoundSequenceReadyRackHbciMinus;
            case "BattleRoundSequenceReadyRackHvapMinus": return GameAction.BattleRoundSequenceReadyRackHvapMinus;
            case "BattleRoundSequenceReadyRackHePlus": return GameAction.BattleRoundSequenceReadyRackHePlus;
            case "BattleRoundSequenceReadyRackApPlus": return GameAction.BattleRoundSequenceReadyRackApPlus;
            case "BattleRoundSequenceReadyRackWpPlus": return GameAction.BattleRoundSequenceReadyRackWpPlus;
            case "BattleRoundSequenceReadyRackHbciPlus": return GameAction.BattleRoundSequenceReadyRackHbciPlus;
            case "BattleRoundSequenceReadyRackHvapPlus": return GameAction.BattleRoundSequenceReadyRackHvapPlus;
            case "BattleRoundSequenceReadyRackEnd": return GameAction.BattleRoundSequenceReadyRackEnd;
            case "BattleRoundSequenceCrewSwitchEnd": return GameAction.BattleRoundSequenceCrewSwitchEnd;
            case "BattleRoundSequenceCrewReplaced": return GameAction.BattleRoundSequenceCrewReplaced;

            case "BattleRoundSequenceEnemyAction": return GameAction.BattleRoundSequenceEnemyAction;
            case "BattleRoundSequenceCollateralDamageCheck": return GameAction.BattleRoundSequenceCollateralDamageCheck;
            case "BattleRoundSequenceFriendlyAction": return GameAction.BattleRoundSequenceFriendlyAction;
            case "BattleRoundSequenceRandomEvent": return GameAction.BattleRoundSequenceRandomEvent;
            case "BattleRoundSequenceBackToSpotting": return GameAction.BattleRoundSequenceBackToSpotting;
            case "BattleRoundSequenceNextActionAfterRandomEvent": return GameAction.BattleRoundSequenceNextActionAfterRandomEvent;
            case "BattleRoundSequenceLoadMainGun": return GameAction.BattleRoundSequenceLoadMainGun;
            case "BattleRoundSequenceLoadMainGunEnd": return GameAction.BattleRoundSequenceLoadMainGunEnd;

            case "BattleRoundSequenceShermanKilled": return GameAction.BattleRoundSequenceShermanKilled;
            case "BattleRoundSequenceShermanBail": return GameAction.BattleRoundSequenceShermanBail;
            case "BattleRoundSequenceEnemyArtilleryRoll": return GameAction.BattleRoundSequenceEnemyArtilleryRoll;
            case "BattleRoundSequenceMinefieldRoll": return GameAction.BattleRoundSequenceMinefieldRoll;
            case "BattleRoundSequenceMinefieldDisableRoll": return GameAction.BattleRoundSequenceMinefieldDisableRoll;
            case "BattleRoundSequenceMinefieldDriverWoundRoll": return GameAction.BattleRoundSequenceMinefieldDriverWoundRoll;
            case "BattleRoundSequenceMinefieldAssistantWoundRoll": return GameAction.BattleRoundSequenceMinefieldAssistantWoundRoll;
            case "BattleRoundSequencePanzerfaustSectorRoll": return GameAction.BattleRoundSequencePanzerfaustSectorRoll;
            case "BattleRoundSequencePanzerfaustAttackRoll": return GameAction.BattleRoundSequencePanzerfaustAttackRoll;
            case "BattleRoundSequencePanzerfaustToHitRoll": return GameAction.BattleRoundSequencePanzerfaustToHitRoll;
            case "BattleRoundSequencePanzerfaustToKillRoll": return GameAction.BattleRoundSequencePanzerfaustToKillRoll;
            case "BattleRoundSequenceHarrassingFire": return GameAction.BattleRoundSequenceHarrassingFire;
            case "BattleRoundSequenceFriendlyAdvance": return GameAction.BattleRoundSequenceFriendlyAdvance;
            case "BattleRoundSequenceFriendlyAdvanceSelected": return GameAction.BattleRoundSequenceFriendlyAdvanceSelected;
            case "BattleRoundSequenceEnemyAdvance": return GameAction.BattleRoundSequenceEnemyAdvance;
            case "BattleRoundSequenceEnemyAdvanceEnd": return GameAction.BattleRoundSequenceEnemyAdvanceEnd;
            case "BattleRoundSequenceShermanAdvanceOrRetreat": return GameAction.BattleRoundSequenceShermanAdvanceOrRetreat;
            case "BattleRoundSequenceShermanAdvanceOrRetreatEnd": return GameAction.BattleRoundSequenceShermanAdvanceOrRetreatEnd;
            case "BattleRoundSequenceShermanRetreatChoice": return GameAction.BattleRoundSequenceShermanRetreatChoice;
            case "BattleRoundSequenceShermanRetreatChoiceEnd": return GameAction.BattleRoundSequenceShermanRetreatChoiceEnd;
            case "BattleRoundSequenceShowFeat": return GameAction.BattleRoundSequenceShowFeat;
            case "BattleRoundSequenceShowFeatEnd": return GameAction.BattleRoundSequenceShowFeatEnd;

            case "EveningDebriefingStart": return GameAction.EveningDebriefingStart;
            case "EveningDebriefingRatingImprovement": return GameAction.EveningDebriefingRatingImprovement;
            case "EveningDebriefingRatingImprovementEnd": return GameAction.EveningDebriefingRatingImprovementEnd;
            case "EveningDebriefingCrewReplacedEnd": return GameAction.EveningDebriefingCrewReplacedEnd;
            case "EveningDebriefingVictoryPointsCalculated": return GameAction.EveningDebriefingVictoryPointsCalculated;
            case "EventDebriefPromotion": return GameAction.EventDebriefPromotion;
            case "EventDebriefDecorationStart": return GameAction.EventDebriefDecorationStart;
            case "EventDebriefDecorationContinue": return GameAction.EventDebriefDecorationContinue;
            case "EventDebriefDecorationBronzeStar": return GameAction.EventDebriefDecorationBronzeStar;
            case "EventDebriefDecorationSilverStar": return GameAction.EventDebriefDecorationSilverStar;
            case "EventDebriefDecorationCross": return GameAction.EventDebriefDecorationCross;
            case "EventDebriefDecorationHonor": return GameAction.EventDebriefDecorationHonor;
            case "EventDebriefDecorationHeart": return GameAction.EventDebriefDecorationHeart;
            case "EveningDebriefingResetDay": return GameAction.EveningDebriefingResetDay;
            case "EveningDebriefingShowFeat": return GameAction.EveningDebriefingShowFeat;
            case "EveningDebriefingShowFeatEnd": return GameAction.EveningDebriefingShowFeatEnd;
            default: Logger.Log(LogEnum.LE_ERROR, " String_ToGameAction(): reached default sGameAction=" + sGameAction); return GameAction.Error;
         }
      }
      //--------------------------------------------------
      private IGameInstance? ReadXmlGameInstance(string filename)
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
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsStartElement(GameInstance) returned false");
               return null;
            }
            if (reader.Name != "GameInstance")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): first node is not GameInstance");
               return null;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsStartElement(Version) returned false");
               return null;
            }
            if (reader.Name != "Version")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Version != (node=" + reader.Name + ")");
               return null;
            }
            string? sVersion = reader.GetAttribute("value");
            if (null == sVersion)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): version=null");
               return null;
            }
            int version = int.Parse(sVersion);
            if (version != GetMajorVersion())
            {
               System.Windows.MessageBox.Show("Unable to open due to version mismatch. File v" + version + " does not match running v" + GetMajorVersion() + ".");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlListingMapItems(reader))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlListingMapItems() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlGameCommands(reader, gi.GameCommands))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlGameCommands() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlOptions(reader, gi.Options))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlOptions() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlGameStatistics(reader, gi.Statistics))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlGameStatistics() returned false");
               return null;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(MaxDayBetweenCombat) = false");
               return null;
            }
            if (reader.Name != "MaxDayBetweenCombat")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): MaxDayBetweenCombat != (node=" + reader.Name + ")");
               return null;
            }
            string? sMaxDayBetweenCombat = reader.GetAttribute("value");
            if (null == sMaxDayBetweenCombat)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): MaxDayBetweenCombat=null");
               return null;
            }
            gi.MaxDayBetweenCombat = Int32.Parse(sMaxDayBetweenCombat);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(MaxRollsForAirSupport) = false");
               return null;
            }
            if (reader.Name != "MaxRollsForAirSupport")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): MaxRollsForAirSupport != (node=" + reader.Name + ")");
               return null;
            }
            string? sMaxRollsForAirSupport = reader.GetAttribute("value");
            if (null == sMaxRollsForAirSupport)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): MaxRollsForAirSupport=null");
               return null;
            }
            gi.MaxRollsForAirSupport = Int32.Parse(sMaxRollsForAirSupport);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(MaxRollsForArtillerySupport) = false");
               return null;
            }
            if (reader.Name != "MaxRollsForArtillerySupport")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): MaxRollsForArtillerySupport != (node=" + reader.Name + ")");
               return null;
            }
            string? sMaxRollsForArtillerySupport = reader.GetAttribute("value");
            if (null == sMaxRollsForArtillerySupport)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sMaxRollsForArtillerySupport=null");
               return null;
            }
            gi.MaxRollsForArtillerySupport = Int32.Parse(sMaxRollsForArtillerySupport);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(MaxEnemiesInOneBattle) = false");
               return null;
            }
            if (reader.Name != "MaxEnemiesInOneBattle")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): MaxEnemiesInOneBattle != (node=" + reader.Name + ")");
               return null;
            }
            string? sMaxEnemiesInOneBattle = reader.GetAttribute("value");
            if (null == sMaxEnemiesInOneBattle)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sMaxEnemiesInOneBattle=null");
               return null;
            }
            gi.MaxEnemiesInOneBattle = Int32.Parse(sMaxEnemiesInOneBattle);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(RoundsOfCombat) = false");
               return null;
            }
            if (reader.Name != "RoundsOfCombat")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): RoundsOfCombat != (node=" + reader.Name + ")");
               return null;
            }
            string? sRoundsOfCombat = reader.GetAttribute("value");
            if (null == sRoundsOfCombat)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sRoundsOfCombat=null");
               return null;
            }
            gi.RoundsOfCombat = Int32.Parse(sRoundsOfCombat);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(IsFirstSpottingOccurred) = false");
               return null;
            }
            if (reader.Name != "IsFirstSpottingOccurred")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsFirstSpottingOccurred != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsFirstSpottingOccurred = reader.GetAttribute("value");
            if (null == sIsFirstSpottingOccurred)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsFirstSpottingOccurred=null");
               return null;
            }
            gi.IsFirstSpottingOccurred = Boolean.Parse(sIsFirstSpottingOccurred);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(Is1stEnemyStrengthCheckTerritory) = false");
               return null;
            }
            if (reader.Name != "Is1stEnemyStrengthCheckTerritory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Is1stEnemyStrengthCheckTerritory != (node=" + reader.Name + ")");
               return null;
            }
            string? sIs1stEnemyStrengthCheckTerritory = reader.GetAttribute("value");
            if (null == sIs1stEnemyStrengthCheckTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIs1stEnemyStrengthCheckTerritory=null");
               return null;
            }
            gi.Is1stEnemyStrengthCheckTerritory = Boolean.Parse(sIs1stEnemyStrengthCheckTerritory);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(IsGridActive) = false");
               return null;
            }
            if (reader.Name != "IsGridActive")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsGridActive != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsGridActive = reader.GetAttribute("value");
            if (null == sIsGridActive)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sRoundsOfCombat=null");
               return null;
            }
            gi.IsGridActive = Boolean.Parse(sIsGridActive);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(EventActive) = false");
               return null;
            }
            if (reader.Name != "EventActive")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): EventActive != (node=" + reader.Name + ")");
               return null;
            }
            string? eventActive = reader.GetAttribute("value");
            if (null == eventActive)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): eventActive=null");
               return null;
            }
            gi.EventActive = eventActive;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(EventDisplayed) = false");
               return null;
            }
            if (reader.Name != "EventDisplayed")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): EventDisplayed != (node=" + reader.Name + ")");
               return null;
            }
            string? eventDisplayed = reader.GetAttribute("value");
            if (null == eventDisplayed)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): eventDisplayed=null");
               return null;
            }
            gi.EventDisplayed = eventDisplayed;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(Day) = false");
               return null;
            }
            if (reader.Name != "Day")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Day != (node=" + reader.Name + ")");
               return null;
            }
            string? sDay = reader.GetAttribute("value");
            if (null == sDay)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): eventDisplayed=null");
               return null;
            }
            gi.Day = Int32.Parse(sDay);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(GameTurn) = false");
               return null;
            }
            if (reader.Name != "GameTurn")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): GameTurn != (node=" + reader.Name + ")");
               return null;
            }
            string? sGameTurn = reader.GetAttribute("value");
            if (null == sGameTurn)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sGameTurn=null");
               return null;
            }
            gi.GameTurn = Int32.Parse(sGameTurn);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(GamePhase) = false");
               return null;
            }
            if (reader.Name != "GamePhase")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): GamePhase != (node=" + reader.Name + ")");
               return null;
            }
            string? sGamePhase = reader.GetAttribute("value");
            if (null == sGamePhase)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sGamePhase=null");
               return null;
            }
            switch( sGamePhase )
            {
               case "GameSetup": gi.GamePhase = GamePhase.GameSetup; break;
               case "MorningBriefing": gi.GamePhase = GamePhase.MorningBriefing; break;
               case "Preparations": gi.GamePhase = GamePhase.Preparations; break;
               case "Movement": gi.GamePhase = GamePhase.Movement; break;
               case "Battle": gi.GamePhase = GamePhase.Battle; break;
               case "BattleRoundSequence": gi.GamePhase = GamePhase.BattleRoundSequence; break;
               case "EveningDebriefing": gi.GamePhase = GamePhase.EveningDebriefing; break;
               case "EndCampaignGame": gi.GamePhase = GamePhase.EveningDebriefing; break;
               case "UnitTest": gi.GamePhase = GamePhase.UnitTest; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reached default sGamePhase=" + sGamePhase); return null;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(EndGameReason) = false");
               return null;
            }
            if (reader.Name != "EndGameReason")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): EndGameReason != (node=" + reader.Name + ")");
               return null;
            }
            string? endGameReason = reader.GetAttribute("value");
            if (null == endGameReason)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): endGameReason=null");
               return null;
            }
            gi.EndGameReason = endGameReason;
            //----------------------------------------------
            if (false == ReadXmlReports(reader, gi.Reports))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlGameReports()=null");
               return null;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(BattlePhase) = false");
               return null;
            }
            if (reader.Name != "BattlePhase")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): node=" + reader.Name);
               return null;
            }
            string? sBattlePhase = reader.GetAttribute("value");
            if (null == sBattlePhase)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): BattlePhase=null");
               return null;
            }
            switch (sBattlePhase)
            {
               case "Ambush": gi.BattlePhase = BattlePhase.Ambush; break;
               case "AmbushRandomEvent": gi.BattlePhase = BattlePhase.AmbushRandomEvent; break;
               case "Spotting": gi.BattlePhase = BattlePhase.Spotting; break;
               case "MarkCrewAction": gi.BattlePhase = BattlePhase.MarkCrewAction; break;
               case "MarkAmmoReload": gi.BattlePhase = BattlePhase.MarkAmmoReload; break;
               case "ConductCrewAction": gi.BattlePhase = BattlePhase.ConductCrewAction; break;
               case "EnemyAction": gi.BattlePhase = BattlePhase.EnemyAction; break;
               case "FriendlyAction": gi.BattlePhase = BattlePhase.FriendlyAction; break;
               case "RandomEvent": gi.BattlePhase = BattlePhase.RandomEvent; break;
               case "BackToSpotting": gi.BattlePhase = BattlePhase.BackToSpotting; break;
               case "None": gi.BattlePhase = BattlePhase.None; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reached default sBattlePhase=" + sBattlePhase); return null;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(CrewActionPhase) = false");
               return null;
            }
            if (reader.Name != "CrewActionPhase")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): node=" + reader.Name);
               return null;
            }
            string? sCrewActionPhase = reader.GetAttribute("value");
            if (null == sCrewActionPhase)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sCrewActionPhase=null");
               return null;
            }
            switch (sCrewActionPhase)
            {
               case "None": gi.CrewActionPhase = CrewActionPhase.None; break;
               case "Movement": gi.CrewActionPhase = CrewActionPhase.Movement; break;
               case "TankMainGunFire": gi.CrewActionPhase = CrewActionPhase.TankMainGunFire; break;
               case "TankMgFire": gi.CrewActionPhase = CrewActionPhase.TankMgFire; break;
               case "ReplacePeriscope": gi.CrewActionPhase = CrewActionPhase.ReplacePeriscope; break;
               case "RepairGun": gi.CrewActionPhase = CrewActionPhase.RepairGun; break;
               case "FireMortar": gi.CrewActionPhase = CrewActionPhase.FireMortar; break;
               case "ThrowGrenades": gi.CrewActionPhase = CrewActionPhase.ThrowGrenades; break;
               case "RestockReadyRack": gi.CrewActionPhase = CrewActionPhase.RestockReadyRack; break;
               case "CrewSwitch": gi.CrewActionPhase = CrewActionPhase.CrewSwitch; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reached default sCrewActionPhase=" + sCrewActionPhase); return null;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "MovementEffectOnSherman")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): MovementEffectOnSherman != (node=" + reader.Name + ")");
               return null;
            }
            string? sMovementEffectOnSherman = reader.GetAttribute("value");
            if (null == sMovementEffectOnSherman)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sMovementEffectOnSherman=null");
               return null;
            }
            gi.MovementEffectOnSherman = sMovementEffectOnSherman;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "MovementEffectOnEnemy")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): MovementEffectOnEnemy != (node=" + reader.Name + ")");
               return null;
            }
            string? sMovementEffectOnEnemy = reader.GetAttribute("value");
            if (null == sMovementEffectOnEnemy)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): MovementEffectOnEnemy=null");
               return null;
            }
            gi.MovementEffectOnEnemy = sMovementEffectOnEnemy;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "FiredAmmoType")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): FiredAmmoType != (node=" + reader.Name + ")");
               return null;
            }
            string? sFiredAmmoType = reader.GetAttribute("value");
            if (null == sFiredAmmoType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): FiredAmmoType=null");
               return null;
            }
            gi.FiredAmmoType = sFiredAmmoType;
            //----------------------------------------------
            if (false == ReadXmlMapItems(reader, gi.ReadyRacks, "ReadyRacks"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItems(ReadyRacks) returned null");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlMapItems(reader, gi.Hatches, "Hatches"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItems(Hatches) returned null");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlMapItems(reader, gi.CrewActions, "CrewActions"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItems(CrewActions) returned null");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlMapItems(reader, gi.GunLoads, "GunLoads"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItems(GunLoads) returned null");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlMapItems(reader, gi.Targets, "Targets"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItems(Targets) returned null");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlMapItems(reader, gi.AdvancingEnemies, "AdvancingEnemies"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItems(AdvancingEnemies) returned null");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlMapItems(reader, gi.ShermanAdvanceOrRetreatEnemies, "ShermanAdvanceOrRetreatEnemies"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItems(ShermanAdvanceOrRetreatEnemies) returned null");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlCrewMembers(reader, gi.NewMembers, "NewMembers"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlCrewMembers(NewMembers) returned null");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlCrewMembers(reader, gi.InjuredCrewMembers, "InjuredCrewMembers"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlCrewMembers(InjuredCrewMembers) returned null");
               return null;
            }
            //----------------------------------------------
            IMapItem? mapItem = null;
            if (false == ReadXmlMapItem(reader, ref mapItem))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItem(Sherman) returned null");
               return null;
            }
            if( null == mapItem )
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItem(Sherman) mapItem = null");
               return null;
            }
            gi.Sherman = mapItem;
            //----------------------------------------------
            if (false == ReadXmlMapItem(reader, ref mapItem))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItem(TargetMainGun) returned null");
               return null;
            }
            gi.TargetMainGun = mapItem;
            //----------------------------------------------
            if (false == ReadXmlMapItem(reader, ref mapItem))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItem(TargetMg) returned null");
               return null;
            }
            gi.TargetMg = mapItem;
            //----------------------------------------------
            if (false == ReadXmlMapItem(reader, ref mapItem))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItem(ShermanHvss) returned null");
               return null;
            }
            gi.ShermanHvss = mapItem;
            //----------------------------------------------
            ICrewMember? crewMember = null;
            if (false == ReadXmlCrewMember(reader, ref crewMember))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_CrewMember(ReturningCrewman) returned null");
               return null;
            }
            gi.ReturningCrewman = crewMember;
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.AreaTargets, "AreaTargets"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlTerritories(AreaTargets) returned null");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.CounterattachRetreats, "CounterattachRetreats"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlTerritories(CounterattachRetreats) returned null");
               return null;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "Home")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Home != (node=" + reader.Name + ")");
               return null;
            }
            string? sHomeName = reader.GetAttribute("value");
            string? sHomeType = reader.GetAttribute("type");
            if (null == sHomeName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sHomeName=null");
               return null;
            }
            if (null == sHomeType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sHomeType=null");
               return null;
            }
            ITerritory? tHome = Territories.theTerritories.Find(sHomeName, sHomeType);
            if (null == tHome )
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Territories.theTerritories.Find(sHome)");
               return null;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "EnemyStrengthCheckTerritory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): EnemyStrengthCheckTerritory != (node=" + reader.Name + ")");
               return null;
            }
            string? sEnemyStrengthCheckTerritoryName = reader.GetAttribute("value");
            string? sEnemyStrengthCheckTerritoryType = reader.GetAttribute("type");
            if (null == sEnemyStrengthCheckTerritoryName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sEnemyStrengthCheckTerritoryName=null");
               return null;
            }
            if (null == sEnemyStrengthCheckTerritoryType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sEnemyStrengthCheckTerritoryType=null");
               return null;
            }
            if ( "null" == sEnemyStrengthCheckTerritoryName)
            {
               gi.EnemyStrengthCheckTerritory = null;
            }
            else
            {
               gi.EnemyStrengthCheckTerritory = Territories.theTerritories.Find(sEnemyStrengthCheckTerritoryName, sEnemyStrengthCheckTerritoryType);
               if (null == gi.EnemyStrengthCheckTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Territories.theTerritories.Find(sEnemyStrengthCheckTerritory, sEnemyStrengthCheckTerritoryType)");
                  return null;
               }
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "ArtillerySupportCheck")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ArtillerySupportCheck != (node=" + reader.Name + ")");
               return null;
            }
            string? sArtillerySupportCheckName = reader.GetAttribute("value");
            string? sArtillerySupportCheckType = reader.GetAttribute("type");
            if (null == sArtillerySupportCheckName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sArtillerySupportCheckName=null");
               return null;
            }
            if (null == sArtillerySupportCheckType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sArtillerySupportCheckName=null");
               return null;
            }
            if ("null" == sArtillerySupportCheckName)
            {
               gi.ArtillerySupportCheck = null;
            }
            else
            {
               gi.ArtillerySupportCheck = Territories.theTerritories.Find(sArtillerySupportCheckName, sArtillerySupportCheckType);
               if (null == gi.ArtillerySupportCheck)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Territories.theTerritories.Find(sArtillerySupportCheck)");
                  return null;
               }
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "AirStrikeCheckTerritory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): AirStrikeCheckTerritory != (node=" + reader.Name + ")");
               return null;
            }
            string? sAirStrikeCheckTerritoryName = reader.GetAttribute("value");
            string? sAirStrikeCheckTerritoryType = reader.GetAttribute("type");
            if (null == sAirStrikeCheckTerritoryName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sAirStrikeCheckTerritoryName=null");
               return null;
            }
            if (null == sAirStrikeCheckTerritoryType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sAirStrikeCheckTerritoryType=null");
               return null;
            }
            if ("null" == sAirStrikeCheckTerritoryName)
            {
               gi.AirStrikeCheckTerritory = null;
            }
            else
            {
               gi.AirStrikeCheckTerritory = Territories.theTerritories.Find(sAirStrikeCheckTerritoryName, sAirStrikeCheckTerritoryType);
               if (null == gi.AirStrikeCheckTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Territories.theTerritories.Find(sAirStrikeCheckTerritoryName, sAirStrikeCheckTerritoryType)");
                  return null;
               }
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "EnteredArea")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): EnteredArea != (node=" + reader.Name + ")");
               return null;
            }
            string? sEnteredAreaName = reader.GetAttribute("value");
            string? sEnteredAreaType= reader.GetAttribute("type");
            if (null == sEnteredAreaName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sEnteredAreaName=null");
               return null;
            }
            if (null == sEnteredAreaType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sEnteredAreaName=null");
               return null;
            }
            if ("null" == sEnteredAreaName)
            {
               gi.EnteredArea = null;
            }
            else
            {
               gi.EnteredArea = Territories.theTerritories.Find(sEnteredAreaName, sEnteredAreaType);
               if (null == gi.EnteredArea)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Territories.theTerritories.Find(sEnteredAreaName, sEnteredAreaType)");
                  return null;
               }
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "AdvanceFire")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): AdvanceFire != (node=" + reader.Name + ")");
               return null;
            }
            string? sAdvanceFireName = reader.GetAttribute("value");
            string? sAdvanceFireType = reader.GetAttribute("type");
            if (null == sAdvanceFireName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sAdvanceFireName=null");
               return null;
            }
            if (null == sAdvanceFireType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sAdvanceFireType=null");
               return null;
            }
            if ("null" == sAdvanceFireName)
            {
               gi.AdvanceFire = null;
            }
            else
            {
               gi.AdvanceFire = Territories.theTerritories.Find(sAdvanceFireName, sAdvanceFireType);
               if (null == gi.AdvanceFire)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Territories.theTerritories.Find(sAdvanceFireName, sAdvanceFireType)");
                  return null;
               }
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "FriendlyAdvance")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): FriendlyAdvance != (node=" + reader.Name + ")");
               return null;
            }
            string? sFriendlyAdvanceName = reader.GetAttribute("value");
            string? sFriendlyAdvanceType = reader.GetAttribute("type");
            if (null == sFriendlyAdvanceName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sFriendlyAdvanceName=null");
               return null;
            }
            if (null == sFriendlyAdvanceType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sFriendlyAdvanceName=null");
               return null;
            }
            if ("null" == sFriendlyAdvanceName)
            {
               gi.FriendlyAdvance = null;
            }
            else
            {
               gi.FriendlyAdvance = Territories.theTerritories.Find(sFriendlyAdvanceName, sFriendlyAdvanceType);
               if (null == gi.FriendlyAdvance)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Territories.theTerritories.Find(sFriendlyAdvanceName,sFriendlyAdvanceType)");
                  return null;
               }
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(EnemyAdvance) = false");
               return null;
            }
            if (reader.Name != "EnemyAdvance")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): EnemyAdvance != (node=" + reader.Name + ")");
               return null;
            }
            string? sEnemyAdvanceName = reader.GetAttribute("value");
            string? sEnemyAdvanceType = reader.GetAttribute("type");
            if (null == sEnemyAdvanceName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sEnemyAdvanceName=null");
               return null;
            }
            if (null == sEnemyAdvanceType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sEnemyAdvanceType=null");
               return null;
            }
            if ("null" == sEnemyAdvanceName)
            {
               gi.EnemyAdvance = null;
            }
            else
            {
               gi.EnemyAdvance = Territories.theTerritories.Find(sEnemyAdvanceName, sEnemyAdvanceType);
               if (null == gi.EnemyAdvance)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Territories.theTerritories.Find(sEnemyAdvanceName, sEnemyAdvanceType) sEnemyAdvanceName=" + sEnemyAdvanceName + " sEnemyAdvanceType=" + sEnemyAdvanceType);
                  return null;
               }
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsHatchesActive")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsHatchesActive != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsHatchesActive = reader.GetAttribute("value");
            if (null == sIsHatchesActive)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsHatchesActive=null");
               return null;
            }
            gi.IsHatchesActive = Convert.ToBoolean(sIsHatchesActive);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsRetreatToStartArea")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsRetreatToStartArea != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsRetreatToStartArea = reader.GetAttribute("value");
            if (null == sIsRetreatToStartArea)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsRetreatToStartArea=null");
               return null;
            }
            gi.IsRetreatToStartArea = Convert.ToBoolean(sIsRetreatToStartArea);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanAdvancingOnBattleBoard")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanAdvancingOnBattleBoard != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanAdvancingOnBattleBoard = reader.GetAttribute("value");
            if (null == sIsShermanAdvancingOnBattleBoard)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsShermanAdvancingOnBattleBoard=null");
               return null;
            }
            gi.IsShermanAdvancingOnBattleBoard = Convert.ToBoolean(sIsShermanAdvancingOnBattleBoard);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanAdvancingOnMoveBoard")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanAdvancingOnMoveBoard != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanAdvancingOnMoveBoard = reader.GetAttribute("value");
            if (null == sIsShermanAdvancingOnMoveBoard)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanAdvancingOnMoveBoard=null");
               return null;
            }
            gi.IsShermanAdvancingOnMoveBoard = Convert.ToBoolean(sIsShermanAdvancingOnMoveBoard);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(IsLoaderSpotThisTurn) = false");
               return null;
            }
            if (reader.Name != "IsLoaderSpotThisTurn")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsLoaderSpotThisTurn != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsLoaderSpotThisTurn = reader.GetAttribute("value");
            if (null == sIsLoaderSpotThisTurn)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsLoaderSpotThisTurn=null");
               return null;
            }
            gi.IsLoaderSpotThisTurn = Convert.ToBoolean(sIsLoaderSpotThisTurn);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(IsCommanderSpotThisTurn) = false");
               return null;
            }
            if (reader.Name != "IsCommanderSpotThisTurn")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsCommanderSpotThisTurn != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsCommanderSpotThisTurn = reader.GetAttribute("value");
            if (null == sIsCommanderSpotThisTurn)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsCommanderSpotThisTurn=null");
               return null;
            }
            gi.IsCommanderSpotThisTurn = Convert.ToBoolean(sIsCommanderSpotThisTurn);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(IsFallingSnowStopped) = false");
               return null;
            }
            if (reader.Name != "IsFallingSnowStopped")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsFallingSnowStopped != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsFallingSnowStopped = reader.GetAttribute("value");
            if (null == sIsFallingSnowStopped)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsFallingSnowStopped=null");
               return null;
            }
            gi.IsFallingSnowStopped = Convert.ToBoolean(sIsFallingSnowStopped);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(HoursOfRainThisDay) = false");
               return null;
            }
            if (reader.Name != "HoursOfRainThisDay")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): HoursOfRainThisDay != (node=" + reader.Name + ")");
               return null;
            }
            string? sHoursOfRainThisDay = reader.GetAttribute("value");
            if (null == sHoursOfRainThisDay)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sHoursOfRainThisDay=null");
               return null;
            }
            gi.HoursOfRainThisDay = Convert.ToInt32(sHoursOfRainThisDay);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(MinSinceLastCheck) = false");
               return null;
            }
            if (reader.Name != "MinSinceLastCheck")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): MinSinceLastCheck != (node=" + reader.Name + ")");
               return null;
            }
            string? sMinSinceLastCheck = reader.GetAttribute("value");
            if (null == sMinSinceLastCheck)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sMinSinceLastCheck=null");
               return null;
            }
            gi.MinSinceLastCheck = Convert.ToInt32(sMinSinceLastCheck);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "SwitchedCrewMemberRole")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): SwitchedCrewMemberRole != (node=" + reader.Name + ")");
               return null;
            }
            string? sSwitchedCrewMember = reader.GetAttribute("value");
            if (null == sSwitchedCrewMember)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): SwitchedCrewMemberRole=null");
               return null;
            }
            gi.SwitchedCrewMemberRole = sSwitchedCrewMember;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "AssistantOriginalRating")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): AssistantOriginalRating != (node=" + reader.Name + ")");
               return null;
            }
            string? sAssistantOriginalRating = reader.GetAttribute("value");
            if (null == sAssistantOriginalRating)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): AssistantOriginalRating=null");
               return null;
            }
            gi.AssistantOriginalRating = Convert.ToInt32(sAssistantOriginalRating);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(ShermanTurretRotationOld) = false");
               return null;
            }
            if (reader.Name != "ShermanTurretRotationOld")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ShermanTurretRotationOld != (node=" + reader.Name + ")");
               return null;
            }
            string? sShermanTurretRotationOld = reader.GetAttribute("value");
            if (null == sShermanTurretRotationOld)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sShermanTurretRotationOld=null");
               return null;
            }
            gi.ShermanTurretRotationOld = Convert.ToDouble(sShermanTurretRotationOld);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(IsShermanTurretRotatedThisRound) = false");
               return null;
            }
            if (reader.Name != "IsShermanTurretRotatedThisRound")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanTurretRotatedThisRound != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanTurretRotatedThisRound = reader.GetAttribute("value");
            if (null == sIsShermanTurretRotatedThisRound)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsShermanTurretRotatedThisRound=null");
               return null;
            }
            gi.IsShermanTurretRotatedThisRound = Convert.ToBoolean(sIsShermanTurretRotatedThisRound);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(ShermanConsectiveMoveAttempt) = false");
               return null;
            }
            if (reader.Name != "ShermanConsectiveMoveAttempt")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ShermanConsectiveMoveAttempt != (node=" + reader.Name + ")");
               return null;
            }
            string? sShermanConsectiveMoveAttempt = reader.GetAttribute("value");
            if (null == sShermanConsectiveMoveAttempt)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sShermanConsectiveMoveAttempt=null");
               return null;
            }
            gi.ShermanConsectiveMoveAttempt = Convert.ToInt32(sShermanConsectiveMoveAttempt);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanFiringAtFront")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiringAtFront != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanFiringAtFront = reader.GetAttribute("value");
            if (null == sIsShermanFiringAtFront)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiringAtFront=null");
               return null;
            }
            gi.IsShermanFiringAtFront = Convert.ToBoolean(sIsShermanFiringAtFront);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanDeliberateImmobilization")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanDeliberateImmobilization != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanDeliberateImmobilization = reader.GetAttribute("value");
            if (null == sIsShermanDeliberateImmobilization)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanDeliberateImmobilization=null");
               return null;
            }
            gi.IsShermanDeliberateImmobilization = Convert.ToBoolean(sIsShermanDeliberateImmobilization);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "ShermanTypeOfFire")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ShermanTypeOfFire != (node=" + reader.Name + ")");
               return null;
            }
            string? sShermanTypeOfFire = reader.GetAttribute("value");
            if (null == sShermanTypeOfFire)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ShermanTypeOfFire=null");
               return null;
            }
            gi.ShermanTypeOfFire = sShermanTypeOfFire;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "NumSmokeAttacksThisRound")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): NumSmokeAttacksThisRound != (node=" + reader.Name + ")");
               return null;
            }
            string? sNumSmokeAttacksThisRound = reader.GetAttribute("value");
            if (null == sNumSmokeAttacksThisRound)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): NumSmokeAttacksThisRound=null");
               return null;
            }
            gi.NumSmokeAttacksThisRound = Convert.ToInt32(sNumSmokeAttacksThisRound);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsMalfunctionedMainGun")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Is_MalfunctionedMainGun != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsMalfunctionedMainGun = reader.GetAttribute("value");
            if (null == sIsMalfunctionedMainGun)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsMalfunctionedMainGun=null");
               return null;
            }
            gi.IsMalfunctionedMainGun = Convert.ToBoolean(sIsMalfunctionedMainGun);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsMainGunRepairAttempted")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsMainGunRepairAttempted != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsMainGunRepairAttempted = reader.GetAttribute("value");
            if (null == sIsMainGunRepairAttempted)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsMainGunRepairAttempted=null");
               return null;
            }
            gi.IsMainGunRepairAttempted = Convert.ToBoolean(sIsMainGunRepairAttempted);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsBrokenMainGun")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenMainGun != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsBrokenMainGun = reader.GetAttribute("value");
            if (null == sIsBrokenMainGun)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenMainGun=null");
               return null;
            }
            gi.IsBrokenMainGun = Convert.ToBoolean(sIsBrokenMainGun);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsBrokenGunSight")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenGunSight != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsBrokenGunSight = reader.GetAttribute("value");
            if (null == sIsBrokenGunSight)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenMainGun=null");
               return null;
            }
            gi.IsBrokenGunSight = Convert.ToBoolean(sIsBrokenGunSight);
            //----------------------------------------------
            if( false == ReadXmlFirstShots(reader, gi.FirstShots))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlFirstShots() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTrainedGunners(reader, gi.TrainedGunners))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlTrainedGunners() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlShermanHits(reader, gi.ShermanHits))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlShermanHits() returned false");
               return null;
            }
            //----------------------------------------------
            ShermanDeath? death = null;
            if (false == ReadXmlShermanDeath(reader, ref death))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlShermanDeath() returned false");
               return null;
            }
            gi.Death = death;
            //----------------------------------------------
            if (false == ReadXmlShermanSetup(reader, gi.BattlePrep))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlShermanSetup() returned false");
               return null;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IdentifiedTank")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IdentifiedTank != (node=" + reader.Name + ")");
               return null;
            }
            string? sIdentifiedTank = reader.GetAttribute("value");
            if (null == sIdentifiedTank)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IdentifiedTank=null");
               return null;
            }
            gi.IdentifiedTank = sIdentifiedTank;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IdentifiedAtg")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IdentifiedAtg != (node=" + reader.Name + ")");
               return null;
            }
            string? sIdentifiedAtg = reader.GetAttribute("value");
            if (null == sIdentifiedAtg)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IdentifiedAtg=null");
               return null;
            }
            gi.IdentifiedAtg = sIdentifiedAtg;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IdentifiedSpg")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IdentifiedSpg != (node=" + reader.Name + ")");
               return null;
            }
            string? sIdentifiedSpg = reader.GetAttribute("value");
            if (null == sIdentifiedSpg)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IdentifiedSpg=null");
               return null;
            }
            gi.IdentifiedSpg = sIdentifiedSpg;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanFiringAaMg")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiringAaMg != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanFiringAaMg = reader.GetAttribute("value");
            if (null == sIsShermanFiringAaMg)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiringAaMg=null");
               return null;
            }
            gi.IsShermanFiringAaMg = Convert.ToBoolean(sIsShermanFiringAaMg);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanFiringBowMg")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiringBowMg != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanFiringBowMg = reader.GetAttribute("value");
            if (null == sIsShermanFiringBowMg)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiringAaMg=null");
               return null;
            }
            gi.IsShermanFiringBowMg = Convert.ToBoolean(sIsShermanFiringBowMg);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanFiringCoaxialMg")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsSherman_FiringCoaxialMg != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanFiringCoaxialMg = reader.GetAttribute("value");
            if (null == sIsShermanFiringCoaxialMg)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsSherman_FiringCoaxialMg=null");
               return null;
            }
            gi.IsShermanFiringCoaxialMg = Convert.ToBoolean(sIsShermanFiringCoaxialMg);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanFiringSubMg")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiringSubMg != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanFiringSubMg = reader.GetAttribute("value");
            if (null == sIsShermanFiringSubMg)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsShermanFiringSubMg=null");
               return null;
            }
            gi.IsShermanFiringSubMg = Convert.ToBoolean(sIsShermanFiringSubMg);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsCommanderDirectingMgFire")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsCommanderDirectingMgFire != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsCommanderDirectingMgFire = reader.GetAttribute("value");
            if (null == sIsCommanderDirectingMgFire)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsCommanderDirectingMgFire=null");
               return null;
            }
            gi.IsCommanderDirectingMgFire = Convert.ToBoolean(sIsCommanderDirectingMgFire);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanFiredAaMg")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiredAaMg != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanFiredAaMg = reader.GetAttribute("value");
            if (null == sIsShermanFiredAaMg)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiredAaMg=null");
               return null;
            }
            gi.IsShermanFiredAaMg = Convert.ToBoolean(sIsShermanFiredAaMg);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanFiredBowMg")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiredBowMg != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanFiredBowMg = reader.GetAttribute("value");
            if (null == sIsShermanFiredBowMg)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiredBowMg=null");
               return null;
            }
            gi.IsShermanFiredBowMg = Convert.ToBoolean(sIsShermanFiredBowMg);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanFiredCoaxialMg")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiredCoaxialMg != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanFiredCoaxialMg = reader.GetAttribute("value");
            if (null == sIsShermanFiredCoaxialMg)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiredCoaxialMg=null");
               return null;
            }
            gi.IsShermanFiredCoaxialMg = Convert.ToBoolean(sIsShermanFiredCoaxialMg);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsShermanFiredSubMg")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiredSubMg != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsShermanFiredSubMg = reader.GetAttribute("value");
            if (null == sIsShermanFiredSubMg)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsShermanFiredSubMg=null");
               return null;
            }
            gi.IsShermanFiredSubMg = Convert.ToBoolean(sIsShermanFiredSubMg);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsMalfunctionedMgCoaxial")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsMalfunctionedMgCoaxial != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsMalfunctionedMgCoaxial = reader.GetAttribute("value");
            if (null == sIsMalfunctionedMgCoaxial)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsMalfunctionedMgCoaxial=null");
               return null;
            }
            gi.IsMalfunctionedMgCoaxial = Convert.ToBoolean(sIsMalfunctionedMgCoaxial);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsMalfunctionedMgBow")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsMalfunctionedMgBow != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsMalfunctionedMgBow = reader.GetAttribute("value");
            if (null == sIsMalfunctionedMgBow)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsMalfunctionedMgBow=null");
               return null;
            }
            gi.IsMalfunctionedMgBow = Convert.ToBoolean(sIsMalfunctionedMgBow);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsMalfunctionedMgAntiAircraft")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsMalfunctionedMgAntiAircraft != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsMalfunctionedMgAntiAircraft = reader.GetAttribute("value");
            if (null == sIsMalfunctionedMgAntiAircraft)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsMalfunctionedMgAntiAircraft=null");
               return null;
            }
            gi.IsMalfunctionedMgAntiAircraft = Convert.ToBoolean(sIsMalfunctionedMgAntiAircraft);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsCoaxialMgRepairAttempted")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsCoaxialMgRepairAttempted != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsCoaxialMgRepairAttempted = reader.GetAttribute("value");
            if (null == sIsCoaxialMgRepairAttempted)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsCoaxialMgRepairAttempted=null");
               return null;
            }
            gi.IsCoaxialMgRepairAttempted = Convert.ToBoolean(sIsCoaxialMgRepairAttempted);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsBowMgRepairAttempted")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBowMgRepairAttempted != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsBowMgRepairAttempted = reader.GetAttribute("value");
            if (null == sIsBowMgRepairAttempted)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBowMgRepairAttempted=null");
               return null;
            }
            gi.IsBowMgRepairAttempted = Convert.ToBoolean(sIsBowMgRepairAttempted);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsAaMgRepairAttempted")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsAaMgRepairAttempted != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsAaMgRepairAttempted = reader.GetAttribute("value");
            if (null == sIsAaMgRepairAttempted)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsAaMgRepairAttempted=null");
               return null;
            }
            gi.IsAaMgRepairAttempted = Convert.ToBoolean(sIsAaMgRepairAttempted);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsBrokenMgAntiAircraft")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenMgAntiAircraft != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsBrokenMgAntiAircraft = reader.GetAttribute("value");
            if (null == sIsBrokenMgAntiAircraft)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenMgAntiAircraft=null");
               return null;
            }
            gi.IsBrokenMgAntiAircraft = Convert.ToBoolean(sIsBrokenMgAntiAircraft);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsBrokenMgBow")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenMgBow != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsBrokenMgBow = reader.GetAttribute("value");
            if (null == sIsBrokenMgBow)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenMgBow=null");
               return null;
            }
            gi.IsBrokenMgBow = Convert.ToBoolean(sIsBrokenMgBow);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsBrokenMgCoaxial")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenMgCoaxial != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsBrokenMgCoaxial = reader.GetAttribute("value");
            if (null == sIsBrokenMgCoaxial)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenMgCoaxial=null");
               return null;
            }
            gi.IsBrokenMgCoaxial = Convert.ToBoolean(sIsBrokenMgCoaxial);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsBrokenPeriscopeDriver")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenPeriscopeDriver != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsBrokenPeriscopeDriver = reader.GetAttribute("value");
            if (null == sIsBrokenPeriscopeDriver)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenPeriscopeDriver=null");
               return null;
            }
            gi.IsBrokenPeriscopeDriver = Convert.ToBoolean(sIsBrokenPeriscopeDriver);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsBrokenPeriscopeLoader")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenPeriscopeLoader != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsBrokenPeriscopeLoader = reader.GetAttribute("value");
            if (null == sIsBrokenPeriscopeLoader)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenPeriscopeLoader=null");
               return null;
            }
            gi.IsBrokenPeriscopeLoader = Convert.ToBoolean(sIsBrokenPeriscopeLoader);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsBrokenPeriscopeAssistant")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenPeriscopeAssistant != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsBrokenPeriscopeAssistant = reader.GetAttribute("value");
            if (null == sIsBrokenPeriscopeAssistant)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenPeriscopeAssistant=null");
               return null;
            }
            gi.IsBrokenPeriscopeAssistant = Convert.ToBoolean(sIsBrokenPeriscopeAssistant);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsBrokenPeriscopeGunner")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenPeriscopeGunner != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsBrokenPeriscopeGunner = reader.GetAttribute("value");
            if (null == sIsBrokenPeriscopeGunner)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenPeriscopeGunner=null");
               return null;
            }
            gi.IsBrokenPeriscopeGunner = Convert.ToBoolean(sIsBrokenPeriscopeGunner);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsBrokenPeriscopeCommander")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenPeriscopeCommander != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsBrokenPeriscopeCommander = reader.GetAttribute("value");
            if (null == sIsBrokenPeriscopeCommander)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsBrokenPeriscopeCommander=null");
               return null;
            }
            gi.IsBrokenPeriscopeCommander = Convert.ToBoolean(sIsBrokenPeriscopeCommander);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsCounterattackAmbush")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsCounterattackAmbush != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsCounterattackAmbush = reader.GetAttribute("value");
            if (null == sIsCounterattackAmbush)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsCounterattackAmbush=null");
               return null;
            }
            gi.IsCounterattackAmbush = Convert.ToBoolean(sIsCounterattackAmbush);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsLeadTank")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsLeadTank != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsLeadTank = reader.GetAttribute("value");
            if (null == sIsLeadTank)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsLeadTank=null");
               return null;
            }
            gi.IsLeadTank = Convert.ToBoolean(sIsLeadTank);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsAirStrikePending")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsAirStrikePending != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsAirStrikePending = reader.GetAttribute("value");
            if (null == sIsAirStrikePending)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsAirStrikePending=null");
               return null;
            }
            gi.IsAirStrikePending = Convert.ToBoolean(sIsAirStrikePending);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsAdvancingFireChosen")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsAdvancingFireChosen != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsAdvancingFireChosen = reader.GetAttribute("value");
            if (null == sIsAdvancingFireChosen)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsAdvancingFireChosen=null");
               return null;
            }
            gi.IsAdvancingFireChosen = Convert.ToBoolean(sIsAdvancingFireChosen);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "AdvancingFireMarkerCount")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): AdvancingFireMarkerCount != (node=" + reader.Name + ")");
               return null;
            }
            string? sAdvancingFireMarkerCount = reader.GetAttribute("value");
            if (null == sAdvancingFireMarkerCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsAdvancingFireChosen=null");
               return null;
            }
            gi.AdvancingFireMarkerCount = Convert.ToInt32(sAdvancingFireMarkerCount);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "BattleResistance")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): BattleResistance != (node=" + reader.Name + ")");
               return null;
            }
            string? sBattleResistance = reader.GetAttribute("value");
            if (null == sBattleResistance)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sBattleResistance=null");
               return null;
            }
            switch(sBattleResistance)
            {
               case "Light": gi.BattleResistance = EnumResistance.Light; break;
               case "Medium": gi.BattleResistance = EnumResistance.Medium; break;
               case "Heavy": gi.BattleResistance = EnumResistance.Heavy; break;
               case "None": gi.BattleResistance = EnumResistance.None; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reached default sBattleResistance=" + sBattleResistance); return null;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsMinefieldAttack")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsMinefieldAttack != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsMinefieldAttack = reader.GetAttribute("value");
            if (null == sIsMinefieldAttack)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsMinefieldAttack=null");
               return null;
            }
            gi.IsMinefieldAttack = Convert.ToBoolean(sIsMinefieldAttack);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsHarrassingFireBonus")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsHarrassingFireBonus != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsHarrassingFireBonus = reader.GetAttribute("value");
            if (null == sIsHarrassingFireBonus)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsHarrassingFireBonus=null");
               return null;
            }
            gi.IsHarrassingFireBonus = Convert.ToBoolean(sIsHarrassingFireBonus);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsFlankingFire")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsFlankingFire != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsFlankingFire = reader.GetAttribute("value");
            if (null == sIsFlankingFire)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsFlankingFire=null");
               return null;
            }
            gi.IsFlankingFire = Convert.ToBoolean(sIsFlankingFire);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement() = false");
               return null;
            }
            if (reader.Name != "IsEnemyAdvanceComplete")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsEnemyAdvanceComplete != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsEnemyAdvanceComplete = reader.GetAttribute("value");
            if (null == sIsEnemyAdvanceComplete)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsEnemyAdvanceComplete=null");
               return null;
            }
            gi.IsEnemyAdvanceComplete = Convert.ToBoolean(sIsEnemyAdvanceComplete);
            //----------------------------------------------
            PanzerfaustAttack? panzerfaustAttack = null;
            if ( false == ReadXmlPanzerfaultAttack(reader, ref panzerfaustAttack))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlPanzerfaultAttack() failed");
               return null;
            }
            gi.Panzerfaust = panzerfaustAttack;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(NumCollateralDamage) = false");
               return null;
            }
            if (reader.Name != "NumCollateralDamage")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): NumCollateralDamage != (node=" + reader.Name + ")");
               return null;
            }
            string? sNumCollateralDamage = reader.GetAttribute("value");
            if (null == sNumCollateralDamage)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): NumCollateralDamage=null");
               return null;
            }
            gi.NumCollateralDamage = Convert.ToInt32(sNumCollateralDamage);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(TankReplacementNumber) = false");
               return null;
            }
            if (reader.Name != "TankReplacementNumber")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): TankReplacementNumber != (node=" + reader.Name + ")");
               return null;
            }
            string? sTankReplacementNumber = reader.GetAttribute("value");
            if (null == sTankReplacementNumber)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): TankReplacementNumber=null");
               return null;
            }
            gi.TankReplacementNumber = Convert.ToInt32(sTankReplacementNumber);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(Fuel) = false");
               return null;
            }
            if (reader.Name != "Fuel")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Fuel != (node=" + reader.Name + ")");
               return null;
            }
            string? sFuel = reader.GetAttribute("value");
            if (null == sFuel)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sFuel=null");
               return null;
            }
            gi.Fuel = Convert.ToInt32(sFuel);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(VictoryPtsTotalCampaign) = false");
               return null;
            }
            if (reader.Name != "VictoryPtsTotalCampaign")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): VictoryPtsTotalCampaign != (node=" + reader.Name + ")");
               return null;
            }
            string? sVictoryPtsTotalCampaign = reader.GetAttribute("value");
            if (null == sVictoryPtsTotalCampaign)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): VictoryPtsTotalCampaign=null");
               return null;
            }
            gi.VictoryPtsTotalCampaign = Convert.ToInt32(sVictoryPtsTotalCampaign);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(PromotionPointNum) = false");
               return null;
            }
            if (reader.Name != "PromotionPointNum")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): PromotionPointNum != (node=" + reader.Name + ")");
               return null;
            }
            string? sPromotionPointNum = reader.GetAttribute("value");
            if (null == sPromotionPointNum)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): PromotionPointNum=null");
               return null;
            }
            gi.PromotionPointNum = Convert.ToInt32(sPromotionPointNum);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(PromotionDay) = false");
               return null;
            }
            if (reader.Name != "PromotionDay")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): PromotionDay != (node=" + reader.Name + ")");
               return null;
            }
            string? sPromotionDay = reader.GetAttribute("value");
            if (null == sPromotionDay)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): PromotionDay=null");
               return null;
            }
            gi.PromotionDay = Convert.ToInt32(sPromotionDay);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(NumPurpleHeart) = false");
               return null;
            }
            if (reader.Name != "NumPurpleHeart")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): NumPurpleHeart != (node=" + reader.Name + ")");
               return null;
            }
            string? sNumPurpleHeart = reader.GetAttribute("value");
            if (null == sNumPurpleHeart)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): NumPurpleHeart=null");
               return null;
            }
            gi.NumPurpleHeart = Convert.ToInt32(sNumPurpleHeart);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(IsCommanderRescuePerformed) = false");
               return null;
            }
            if (reader.Name != "IsCommanderRescuePerformed")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsCommanderRescuePerformed != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsCommanderRescuePerformed = reader.GetAttribute("value");
            if (null == sIsCommanderRescuePerformed)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsCommanderRescuePerformed=null");
               return null;
            }
            gi.IsCommanderRescuePerformed = Convert.ToBoolean(sIsCommanderRescuePerformed);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(IsCommanderKilled) = false");
               return null;
            }
            if (reader.Name != "IsCommanderKilled")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsCommanderKilled != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsCommanderKilled = reader.GetAttribute("value");
            if (null == sIsCommanderKilled)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsCommanderKilled=null");
               return null;
            }
            gi.IsCommanderKilled = Convert.ToBoolean(sIsCommanderKilled);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): reader.IsStartElement(IsPromoted) = false");
               return null;
            }
            if (reader.Name != "IsPromoted")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsPromoted != (node=" + reader.Name + ")");
               return null;
            }
            string? sIsPromoted = reader.GetAttribute("value");
            if (null == sIsPromoted)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sIsPromoted=null");
               return null;
            }
            gi.IsPromoted = Convert.ToBoolean(sIsPromoted);
            //----------------------------------------------
            if ( false == ReadXmlMapItemMoves(reader, gi.MapItemMoves))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlMapItemMoves() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlStacks(reader, gi.MoveStacks, "MoveStacks"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlStacks(MoveStacks) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlStacks(reader, gi.BattleStacks, "BattleStacks"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlStacks(BattleStacks) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlEnteredHexes(reader, gi.EnteredHexes))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlEnteredHexes() returned false");
               return null;
            }
            return gi;
         } // try
         //==========================================
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance():\n" + e.ToString());
            return null;
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
      private bool ReadXmlListingMapItems(XmlReader reader)
      {
         theMapItems.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsStartElement(MapItems)=null");
            return false;
         }
         if (reader.Name != "MapItems")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): MapItems != (node=" + reader.Name + ")");
            return false;
         }
         string? sNumber = reader.GetAttribute("count");
         if (null == sNumber)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Count=null");
            return false;
         }
         int number = int.Parse(sNumber);
         //=================================
         for (int i = 0; i < number; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(Name) = false");
               return false;
            }
            if (reader.Name != "MapItem")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Name != (node=" + reader.Name + ")");
               return false;
            }
            string? sName = reader.GetAttribute("value");
            if (null == sName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sName=null");
               return false;
            }
            IMapItem mi = new MapItem(sName);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(TopImageName) = false");
               return false;
            }
            if (reader.Name != "TopImageName")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): TopImageName != (node=" + reader.Name + ")");
               return false;
            }
            string? sTopImageName = reader.GetAttribute("value");
            if (null == sTopImageName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sTopImageName=null");
               return false;
            }
            mi.TopImageName = sTopImageName;
            MapItem.theMapImages.GetBitmapImage(sTopImageName); // map images should be loaded in memory for MapItem already created
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(BottomImageName) = false");
               return false;
            }
            if (reader.Name != "BottomImageName")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): BottomImageName != (node=" + reader.Name + ")");
               return false;
            }
            string? sBottomImageName = reader.GetAttribute("value");
            if (null == sBottomImageName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): BottomImageName=null");
               return false;
            }
            mi.BottomImageName = sBottomImageName;
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(OverlayImageName) = false");
               return false;
            }
            if (reader.Name != "OverlayImageName")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): OverlayImageName != (node=" + reader.Name + ")");
               return false;
            }
            string? sOverlayImageName = reader.GetAttribute("value");
            if (null == sOverlayImageName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sOverlayImageName=null");
               return false;
            }
            mi.OverlayImageName = sOverlayImageName;
            //---------------------------------------------
            if (false == ReadXmlListingMapItemsWoundSpots(reader, mi.WoundSpots))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): ReadXmlListingMapItemsWoundSpots() returned false");
               return false;
            }
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(Zoom) = false");
               return false;
            }
            if (reader.Name != "Zoom")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Zoom != (node=" + reader.Name + ")");
               return false;
            }
            string? sZoom = reader.GetAttribute("value");
            if (null == sZoom)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sZoom=null");
               return false;
            }
            mi.Zoom = Convert.ToDouble(sZoom);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsMoved) = false");
               return false;
            }
            if (reader.Name != "IsMoved")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsMoved != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsMoved = reader.GetAttribute("value");
            if (null == sIsMoved)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsMoved=null");
               return false;
            }
            mi.IsMoved = Convert.ToBoolean(sIsMoved);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(Count) = false");
               return false;
            }
            if (reader.Name != "Count")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Count != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("value");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sCount=null");
               return false;
            }
            mi.Count = Convert.ToInt32(sCount);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(RotationOffsetHull) = false");
               return false;
            }
            if (reader.Name != "RotationOffsetHull")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): RotationOffsetHull != (node=" + reader.Name + ")");
               return false;
            }
            string? sRotationOffsetHull = reader.GetAttribute("value");
            if (null == sRotationOffsetHull)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sRotationOffsetHull=null");
               return false;
            }
            mi.RotationOffsetHull = Convert.ToDouble(sRotationOffsetHull);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(RotationHull) = false");
               return false;
            }
            if (reader.Name != "RotationHull")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): RotationHull != (node=" + reader.Name + ")");
               return false;
            }
            string? sRotationHull = reader.GetAttribute("value");
            if (null == sRotationHull)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sRotationHull=null");
               return false;
            }
            mi.RotationHull = Convert.ToDouble(sRotationHull);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(RotationOffset_Turret) = false");
               return false;
            }
            if (reader.Name != "RotationOffsetTurret")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): RotationOffset_Turret != (node=" + reader.Name + ")");
               return false;
            }
            string? sRotationOffsetTurret = reader.GetAttribute("value");
            if (null == sRotationOffsetTurret)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sRotationOffsetTurret=null");
               return false;
            }
            mi.RotationOffsetTurret = Convert.ToDouble(sRotationOffsetTurret);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(RotationTurret) = false");
               return false;
            }
            if (reader.Name != "RotationTurret")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): RotationTurret != (node=" + reader.Name + ")");
               return false;
            }
            string? sRotationTurret = reader.GetAttribute("value");
            if (null == sRotationTurret)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sRotationTurret=null");
               return false;
            }
            mi.RotationTurret = Convert.ToDouble(sRotationTurret);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(LocationX) = false");
               return false;
            }
            if (reader.Name != "LocationX")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): LocationX != (node=" + reader.Name + ")");
               return false;
            }
            string? sLocationX = reader.GetAttribute("value");
            if (null == sLocationX)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sLocationX=null");
               return false;
            }
            double x = Convert.ToDouble(sLocationX);
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(LocationY) = false");
               return false;
            }
            if (reader.Name != "LocationY")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): LocationY != (node=" + reader.Name + ")");
               return false;
            }
            string? sLocationY = reader.GetAttribute("value");
            if (null == sLocationY)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sLocationY=null");
               return false;
            }
            double y = Convert.ToDouble(sLocationY);
            mi.Location = new MapPoint(x, y);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(TerritoryCurrent) = false");
               return false;
            }
            if (reader.Name != "TerritoryCurrent")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): TerritoryCurrent != (node=" + reader.Name + ")");
               return false;
            }
            string? sTerritoryCurrentName = reader.GetAttribute("value");
            if (null == sTerritoryCurrentName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sTerritoryCurrentName=null");
               return false;
            }
            string? sTerritoryCurrentType = reader.GetAttribute("type");
            if (null == sTerritoryCurrentType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sTerritoryCurrentType=null");
               return false;
            }
            if ("Offboard" == sTerritoryCurrentName)
            {
               mi.TerritoryCurrent = new Territory();
            }
            else
            {
               ITerritory? tCurrent = Territories.theTerritories.Find(sTerritoryCurrentName, sTerritoryCurrentType);
               if (null == tCurrent)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): tCurrent=null for sTerritoryCurrentName=" + sTerritoryCurrentName + " sTerritoryCurrentType=" + sTerritoryCurrentType);
                  return false;
               }
               mi.TerritoryCurrent = tCurrent;
            }
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(TerritoryStarting) = false");
               return false;
            }
            if (reader.Name != "TerritoryStarting")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): TerritoryStarting != (node=" + reader.Name + ")");
               return false;
            }
            string? sTerritoryStartingName = reader.GetAttribute("value");
            string? sTerritoryStartingType = reader.GetAttribute("type");
            if (null == sTerritoryStartingName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sTerritoryStartingName=null");
               return false;
            }
            if (null == sTerritoryStartingType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sTerritoryStartingType=null");
               return false;
            }
            if ("Offboard" == sTerritoryStartingName)
            {
               mi.TerritoryStarting = mi.TerritoryCurrent;
            }
            else
            {
               ITerritory? tStart = Territories.theTerritories.Find(sTerritoryStartingName, sTerritoryStartingType);
               if (null == tStart)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): tStart=null for sTerritoryStartingName=" + sTerritoryStartingName + " sTerritoryStartingType=" + sTerritoryStartingType);
                  return false;
               }
               mi.TerritoryStarting = tStart;
            }
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(LastMoveAction) = false");
               return false;
            }
            if (reader.Name != "LastMoveAction")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): LastMoveAction != (node=" + reader.Name + ")");
               return false;
            }
            string? sLastMoveAction = reader.GetAttribute("value");
            if (null == sLastMoveAction)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sLastMoveAction=null");
               return false;
            }
            mi.LastMoveAction = sLastMoveAction;
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsMoving) = false");
               return false;
            }
            if (reader.Name != "IsMoving")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsMoving != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsMoving = reader.GetAttribute("value");
            if (null == sIsMoving)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsMoving=null");
               return false;
            }
            mi.IsMoving = Convert.ToBoolean(sIsMoving);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsHullDown) = false");
               return false;
            }
            if (reader.Name != "IsHullDown")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsHullDown != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsHullDown = reader.GetAttribute("value");
            if (null == sIsHullDown)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsHullDown=null");
               return false;
            }
            mi.IsHullDown = Convert.ToBoolean(sIsHullDown);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsKilled) = false");
               return false;
            }
            if (reader.Name != "IsKilled")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsKilled != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsKilled = reader.GetAttribute("value");
            if (null == sIsKilled)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsKilled=null");
               return false;
            }
            mi.IsKilled = Convert.ToBoolean(sIsKilled);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsUnconscious) = false");
               return false;
            }
            if (reader.Name != "IsUnconscious")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsUnconscious != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsUnconscious = reader.GetAttribute("value");
            if (null == sIsUnconscious)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsUnconscious=null");
               return false;
            }
            mi.IsUnconscious = Convert.ToBoolean(sIsUnconscious);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsIncapacitated) = false");
               return false;
            }
            if (reader.Name != "IsIncapacitated")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsIncapacitated != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsIncapacitated = reader.GetAttribute("value");
            if (null == sIsIncapacitated)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsIncapacitated=null");
               return false;
            }
            mi.IsIncapacitated = Convert.ToBoolean(sIsIncapacitated);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsFired) = false");
               return false;
            }
            if (reader.Name != "IsFired")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsFired != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsFired = reader.GetAttribute("value");
            if (null == sIsFired)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsFired=null");
               return false;
            }
            mi.IsFired = Convert.ToBoolean(sIsFired);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsSpotted) = false");
               return false;
            }
            if (reader.Name != "IsSpotted")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsSpotted != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsSpotted = reader.GetAttribute("value");
            if (null == sIsSpotted)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsSpotted=null");
               return false;
            }
            mi.IsSpotted = Convert.ToBoolean(sIsSpotted);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsInterdicted) = false");
               return false;
            }
            if (reader.Name != "IsInterdicted")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsInterdicted != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsInterdicted = reader.GetAttribute("value");
            if (null == sIsInterdicted)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsInterdicted=null");
               return false;
            }
            mi.IsInterdicted = Convert.ToBoolean(sIsInterdicted);
            //---------------------------------------------
            if (false == ReadXmlListingMapItemsEnemyAcquiredShots(reader, mi.EnemyAcquiredShots))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): ReadXmlListingMapItemsEnemyAcquiredShots() returned false");
               return false;
            }
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsMovingInOpen) = false");
               return false;
            }
            if (reader.Name != "IsMovingInOpen")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsMovingInOpen != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsMovingInOpen = reader.GetAttribute("value");
            if (null == sIsMovingInOpen)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsMovingInOpen=null");
               return false;
            }
            mi.IsMovingInOpen = Convert.ToBoolean(sIsMovingInOpen);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsWoods) = false");
               return false;
            }
            if (reader.Name != "IsWoods")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsWoods != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsWoods = reader.GetAttribute("value");
            if (null == sIsWoods)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsWoods=null");
               return false;
            }
            mi.IsWoods = Convert.ToBoolean(sIsWoods);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsBuilding) = false");
               return false;
            }
            if (reader.Name != "IsBuilding")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsBuilding != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsBuilding = reader.GetAttribute("value");
            if (null == sIsBuilding)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsBuilding=null");
               return false;
            }
            mi.IsBuilding = Convert.ToBoolean(sIsBuilding);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsFortification) = false");
               return false;
            }
            if (reader.Name != "IsFortification")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsFortification != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsFortification = reader.GetAttribute("value");
            if (null == sIsFortification)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsFortification=null");
               return false;
            }
            mi.IsFortification = Convert.ToBoolean(sIsFortification);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(Is_ThrownTrack) = false");
               return false;
            }
            if (reader.Name != "IsThrownTrack")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Is_ThrownTrack != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsThrownTrack = reader.GetAttribute("value");
            if (null == sIsThrownTrack)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Is_ThrownTrack=null");
               return false;
            }
            mi.IsThrownTrack = Convert.ToBoolean(sIsThrownTrack);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(Is_BoggedDown) = false");
               return false;
            }
            if (reader.Name != "IsBoggedDown")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Is_BoggedDown != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsBoggedDown = reader.GetAttribute("value");
            if (null == sIsBoggedDown)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Is_BoggedDown=null");
               return false;
            }
            mi.IsBoggedDown = Convert.ToBoolean(sIsBoggedDown);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsAssistanceNeeded) = false");
               return false;
            }
            if (reader.Name != "IsAssistanceNeeded")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsAssistanceNeeded != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsAssistanceNeeded = reader.GetAttribute("value");
            if (null == sIsAssistanceNeeded)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsAssistanceNeeded=null");
               return false;
            }
            mi.IsAssistanceNeeded = Convert.ToBoolean(sIsAssistanceNeeded);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsHeHit) = false");
               return false;
            }
            if (reader.Name != "IsHeHit")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsHeHit != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsHeHit = reader.GetAttribute("value");
            if (null == sIsHeHit)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsHeHit=null");
               return false;
            }
            mi.IsHeHit = Convert.ToBoolean(sIsHeHit);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsApHit) = false");
               return false;
            }
            if (reader.Name != "IsApHit")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsApHit != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsApHit = reader.GetAttribute("value");
            if (null == sIsApHit)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsApHit=null");
               return false;
            }
            mi.IsApHit = Convert.ToBoolean(sIsApHit);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(Spotting) = false");
               return false;
            }
            if (reader.Name != "Spotting")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Spotting != (node=" + reader.Name + ")");
               return false;
            }
            string? sSpotting = reader.GetAttribute("value");
            if (null == sSpotting)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sSpotting=null");
               return false;
            }
            switch (sSpotting)
            {
               case "HIDDEN": mi.Spotting = EnumSpottingResult.HIDDEN; break;
               case "UNSPOTTED": mi.Spotting = EnumSpottingResult.UNSPOTTED; break;
               case "SPOTTED": mi.Spotting = EnumSpottingResult.SPOTTED; break;
               case "IDENTIFIED": mi.Spotting = EnumSpottingResult.IDENTIFIED; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reached default sSpotting=" + sSpotting); return false;
            }
            reader.Read(); // get past </MapItem>
            theMapItems.Add(mi);
         }
         if (0 < number)
            reader.Read(); // get past </MapItems>
         return true;
      }
      private bool ReadXmlListingMapItemsWoundSpots(XmlReader reader, List<BloodSpot> bloodSpots)
      {
         bloodSpots.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): reader.IsStartElement(WoundSpots) = false");
            return false;
         }
         if (reader.Name != "WoundSpots")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): WoundSpots != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {

            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): reader.IsStartElement(WoundSpot) = false");
               return false;
            }
            if (reader.Name != "WoundSpot")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): WoundSpot != (node=" + reader.Name + ")");
               return false;
            }
            //---------------------------------------------
            string? sSize = reader.GetAttribute("size");
            if (null == sSize)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): sSize=null");
               return false;
            }
            int size = Convert.ToInt32(sSize);
            //---------------------------------------------
            string? sLeft = reader.GetAttribute("left");
            if (null == sLeft)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): sLeft=null");
               return false;
            }
            double left = Convert.ToInt32(sLeft);
            //---------------------------------------------
            string? sTop = reader.GetAttribute("top");
            if (null == sTop)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): sTop=null");
               return false;
            }
            double top = Convert.ToInt32(sTop);
            //---------------------------------------------
            BloodSpot bloodSpot = new BloodSpot(size, left, top);
            bloodSpots.Add(bloodSpot);
         }
         if (0 < count)
            reader.Read(); // get past </WoundSpots> tag
         return true;
      }
      private bool ReadXmlListingMapItemsEnemyAcquiredShots(XmlReader reader, Dictionary<string, int> enemyAcquiredShots)
      {
         enemyAcquiredShots.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): IsStartElement(EnemyAcquiredShots) returned false");
            return false;
         }
         if (reader.Name != "EnemyAcquiredShots")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): EnemyAcquiredShots != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; i++)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): reader.IsStartElement(EnemyAcqShot) = false");
               return false;
            }
            if (reader.Name != "EnemyAcqShot")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): IsSpotted != (node=" + reader.Name + ")");
               return false;
            }
            string? sEnemy = reader.GetAttribute("enemy");
            if (null == sEnemy)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): sEnemy=null");
               return false;
            }
            string? sValue = reader.GetAttribute("value");
            if (null == sValue)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): sValue=null");
               return false;
            }
            enemyAcquiredShots[sEnemy] = Convert.ToInt32(sValue);
         }
         if (0 < count)
            reader.Read(); // get past </EnemyAcquiredShots> tag
         return true;
      }
      private bool ReadXmlGameCommands(XmlReader reader, IGameCommands gameCmds)
      {
         gameCmds.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): reader.IsStartElement(GameCommands) = false");
            return false;
         }
         if (reader.Name != "GameCommands")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): GameCommands != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): Count=null");
            return false;
         }
         //-------------------------------------
         int count = int.Parse(sCount);
         for(int i =0; i<count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): reader.IsStartElement(GameCommand) = false");
               return false;
            }
            if (reader.Name != "GameCommand")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): GameCommand != (node=" + reader.Name + ")");
               return false;
            }
            string? sAction = reader.GetAttribute("Action");
            if (sAction == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): sAction=null");
               return false;
            }
            GameAction action = StringToGameAction(sAction);
            if(GameAction.Error == action)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): String_ToGameAction() returned false");
               return false;
            }
            //------------------------------------
            string? sActionDieRoll = reader.GetAttribute("ActionDieRoll");
            if (sActionDieRoll == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): sActionDieRoll=null");
               return false;
            }
            GameAction dieRollAction = StringToGameAction(sActionDieRoll);
            if (GameAction.Error == dieRollAction)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): String_ToGameAction() returned false");
               return false;
            }
            //------------------------------------
            string? sEventActive = reader.GetAttribute("EventActive");
            if (sEventActive == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): sEventActive=null");
               return false;
            }
            //------------------------------------
            string? sGamePhase = reader.GetAttribute("Phase");
            if (null == sGamePhase)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): sGamePhase=null");
               return false;
            }
            GamePhase phase = GamePhase.Error;
            switch (sGamePhase)
            {
               case "UnitTest": phase = GamePhase.UnitTest; break;
               case "GameSetup": phase = GamePhase.GameSetup; break;
               case "MorningBriefing": phase = GamePhase.MorningBriefing; break;
               case "Preparations": phase = GamePhase.Preparations; break;
               case "Movement": phase = GamePhase.Movement; break;
               case "Battle": phase = GamePhase.Battle; break;
               case "BattleRoundSequence": phase = GamePhase.BattleRoundSequence; break;
               case "EveningDebriefing": phase = GamePhase.EveningDebriefing; break;
               case "EndGame": phase = GamePhase.EndGame; break;
               case "Error": phase = GamePhase.Error; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): reached default sGamePhase=" + sGamePhase); return false;
            }
            //------------------------------------
            string? sMainImage = reader.GetAttribute("MainImage");
            if (null == sMainImage)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): sMainImage=null");
               return false;
            }
            EnumMainImage mainImage = EnumMainImage.MI_Other;
            switch (sMainImage)
            {
               case "MI_Other": mainImage = EnumMainImage.MI_Other; break;
               case "MI_Battle": mainImage = EnumMainImage.MI_Battle; break;
               case "MI_Move": mainImage = EnumMainImage.MI_Move; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): reached default sMainImage=" + sMainImage); return false;
            }
            //------------------------------------
            IGameCommand gameCmd = new GameCommand(phase, dieRollAction, sEventActive, action, mainImage);
            gameCmds.Add(gameCmd);
         }
         if (0 < count)
            reader.Read(); // get past </GameCommands>
         return true;
      }
      private bool ReadXmlOptions(XmlReader reader, Options options)
      {
         options.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): reader.IsStartElement(Options) = false");
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
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): IsStartElement(Option) returned false");
               return false;
            }
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
         if( 0 < count )
            reader.Read(); // get past </Options>
         return true;
      }
      private bool ReadXmlGameStatistics(XmlReader reader, GameStatistics statistics)
      {
         statistics.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): reader.IsStartElement(GameStatistics) = false");
            return false;
         }
         if (reader.Name != "GameStatistics")
         {
            Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): GameStatistics != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): Count=null");
            return false;
         }
         //-------------------------------------
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): IsStartElement(GameStatistic) returned false");
               return false;
            }
            if (reader.Name != "GameStatistic")
            {
               Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): GameStatistic != " + reader.Name);
               return false;
            }
            string? key = reader.GetAttribute("Key");
            if (key == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): key=null");
               return false;
            }
            string? sValue = reader.GetAttribute("Value");
            if (sValue == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): sValue=null");
               return false;
            }
            int value = Int32.Parse(sValue);
            GameStatistic stat = new GameStatistic(key, value);
            statistics.Add(stat);
         }
         if (0 < count)
            reader.Read(); // get past </GameStatistics>
         return true;
      }
      private bool ReadXmlReports(XmlReader reader, IAfterActionReports reports)
      {
         reports.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReports(): reader.IsStartElement(Reports) returned false");
            return false;
         }
         if (reader.Name != "Reports")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReports(): Reports != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReports(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            IAfterActionReport report = new AfterActionReport();
            if( false == ReadXmlReportsReport(reader, report))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReports(): ReadXmlReportsReport() returned false");
               return false;
            }
            reports.Add(report);  // ReadXmlReports()
         }
         if (0 < count)
            reader.Read(); // get past </Reports> tag
         return true;
      }
      private bool ReadXmlReportsReport(XmlReader reader, IAfterActionReport report)
      {
         //----------------------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
            return false;
         }
         if (reader.Name != "Report")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
            return false;
         }
         string? sIsActionThisDay = reader.GetAttribute("IsActionThisDay");
         if (null == sIsActionThisDay)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sIsActionThisDay=null");
            return false;
         }
         report.IsActionThisDay = Boolean.Parse(sIsActionThisDay);
         //----------------------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
            return false;
         }
         if (reader.Name != "Day")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
            return false;
         }
         string? sDay = reader.GetAttribute("value");
         if (null == sDay)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sDay=null");
            return false;
         }
         report.Day = sDay;
         //----------------------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
            return false;
         }
         if (reader.Name != "Scenario")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
            return false;
         }
         string? sScenario = reader.GetAttribute("value");
         if (null == sScenario)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sScenario=null");
            return false;
         }
         switch (sScenario)
         {
            case "Advance": report.Scenario = EnumScenario.Advance; break;
            case "Battle": report.Scenario = EnumScenario.Battle; break;
            case "Counterattack": report.Scenario = EnumScenario.Counterattack; break;
            case "Retrofit": report.Scenario = EnumScenario.Retrofit; break;
            default: Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reached default sScenario=" + sScenario); return false;
         }
         //----------------------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): IsStartElement(Probability) returned false");
            return false;
         }
         if (reader.Name != "Probability")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
            return false;
         }
         string? sProbability = reader.GetAttribute("value");
         if (null == sProbability)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sProbability=null");
            return false;
         }
         report.Probability = Convert.ToInt32(sProbability);
         //----------------------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): IsStartElement(Resistance) returned false");
            return false;
         }
         if (reader.Name != "Resistance")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
            return false;
         }
         string? sResistance = reader.GetAttribute("value");
         if (null == sResistance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sProbability=null");
            return false;
         }
         switch (sResistance)
         {
            case "Light": report.Resistance = EnumResistance.Light; break;
            case "Medium": report.Resistance = EnumResistance.Medium; break;
            case "Heavy": report.Resistance = EnumResistance.Heavy; break;
            default: Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reached default sResistance=" + sResistance); return false;
         }
         //----------------------------------------------
         if( true == report.IsActionThisDay )
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): IsStartElement(Name) returned false");
               return false;
            }
            if (reader.Name != "Name")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
               return false;
            }
            string? sName = reader.GetAttribute("value");
            if (null == sName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sName=null");
               return false;
            }
            report.Name = sName;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): IsStartElement(TankCardNum) returned false");
               return false;
            }
            if (reader.Name != "TankCardNum")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
               return false;
            }
            string? sTankCardNum = reader.GetAttribute("value");
            if (null == sTankCardNum)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): TankCardNum=null");
               return false;
            }
            report.TankCardNum = Convert.ToInt32(sTankCardNum);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): IsStartElement(Weather) returned false");
               return false;
            }
            if (reader.Name != "Weather")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
               return false;
            }
            string? sWeather = reader.GetAttribute("value");
            if (null == sWeather)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sWeather=null");
               return false;
            }
            report.Weather = sWeather;
            //----------------------------------------------
            ICrewMember? cm = null;
            if (false == ReadXmlCrewMember(reader, ref cm))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): ReadXmlCrewMember(Commander) returned false");
               return false;
            }
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): ReadXmlCrewMember(Commander) cm = null");
               return false;
            }
            report.Commander = cm;
            //----------------------------------------------
            if (false == ReadXmlCrewMember(reader, ref cm))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): ReadXmlCrewMember(Gunner) returned false");
               return false;
            }
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): ReadXmlCrewMember(Gunner) cm = null");
               return false;
            }
            report.Gunner = cm;
            //----------------------------------------------
            if (false == ReadXmlCrewMember(reader, ref cm))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): ReadXmlCrewMember(Loader) returned false");
               return false;
            }
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): ReadXmlCrewMember(Loader) cm = null");
               return false;
            }
            report.Loader = cm;
            //----------------------------------------------
            if (false == ReadXmlCrewMember(reader, ref cm))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): ReadXmlCrewMember(Driver) returned false");
               return false;
            }
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): ReadXmlCrewMember(Driver) cm = null");
               return false;
            }
            report.Driver = cm;
            //----------------------------------------------
            if (false == ReadXmlCrewMember(reader, ref cm))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): ReadXmlCrewMember(Assistant) returned false");
               return false;
            }
            if (null == cm)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): ReadXmlCrewMember(Assistant) cm = null");
               return false;
            }
            report.Assistant = cm;
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "SunriseHour")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): SunriseHour != (node=" + reader.Name + ")");
               return false;
            }
            string? sSunriseHour = reader.GetAttribute("value");
            if (null == sSunriseHour)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sSunriseHour=null");
               return false;
            }
            report.SunriseHour = Convert.ToInt32(sSunriseHour);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "SunriseMin")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): SunriseMin != (node=" + reader.Name + ")");
               return false;
            }
            string? sSunriseMin = reader.GetAttribute("value");
            if (null == sSunriseMin)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): SunriseMin=null");
               return false;
            }
            report.SunriseMin = Convert.ToInt32(sSunriseMin);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "SunsetHour")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): SunsetHour != (node=" + reader.Name + ")");
               return false;
            }
            string? sSunsetHour = reader.GetAttribute("value");
            if (null == sSunsetHour)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): SunsetHour=null");
               return false;
            }
            report.SunsetHour = Convert.ToInt32(sSunsetHour);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "SunsetMin")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): SunsetMin != (node=" + reader.Name + ")");
               return false;
            }
            string? sSunsetMin = reader.GetAttribute("value");
            if (null == sSunsetMin)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): SunsetMin=null");
               return false;
            }
            report.SunsetMin = Convert.ToInt32(sSunsetMin);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "Ammo30CalibreMG")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): Ammo30CalibreMG != (node=" + reader.Name + ")");
               return false;
            }
            string? sAmmo30CalibreMG = reader.GetAttribute("value");
            if (null == sAmmo30CalibreMG)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): Ammo30CalibreMG=null");
               return false;
            }
            report.Ammo30CalibreMG = Convert.ToInt32(sAmmo30CalibreMG);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "Ammo50CalibreMG")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): Ammo50CalibreMG != (node=" + reader.Name + ")");
               return false;
            }
            string? sAmmo50CalibreMG = reader.GetAttribute("value");
            if (null == sAmmo50CalibreMG)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): Ammo50CalibreMG=null");
               return false;
            }
            report.Ammo50CalibreMG = Convert.ToInt32(sAmmo50CalibreMG);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "AmmoSmokeBomb")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): AmmoSmokeBomb != (node=" + reader.Name + ")");
               return false;
            }
            string? sAmmoSmokeBomb = reader.GetAttribute("value");
            if (null == sAmmoSmokeBomb)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): AmmoSmokeBomb=null");
               return false;
            }
            report.AmmoSmokeBomb = Convert.ToInt32(sAmmoSmokeBomb);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "AmmoSmokeGrenade")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): AmmoSmokeGrenade != (node=" + reader.Name + ")");
               return false;
            }
            string? sAmmoSmokeGrenade = reader.GetAttribute("value");
            if (null == sAmmoSmokeGrenade)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): AmmoSmokeGrenade=null");
               return false;
            }
            report.AmmoSmokeGrenade = Convert.ToInt32(sAmmoSmokeGrenade);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "AmmoPeriscope")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): AmmoSmokeGrenade != (node=" + reader.Name + ")");
               return false;
            }
            string? sAmmoPeriscope = reader.GetAttribute("value");
            if (null == sAmmoPeriscope)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): AmmoPeriscope=null");
               return false;
            }
            report.AmmoPeriscope = Convert.ToInt32(sAmmoPeriscope);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "MainGunHE")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): MainGunHE != (node=" + reader.Name + ")");
               return false;
            }
            string? sMainGunHE = reader.GetAttribute("value");
            if (null == sMainGunHE)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): MainGunHE=null");
               return false;
            }
            report.MainGunHE = Convert.ToInt32(sMainGunHE);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "MainGunAP")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): MainGunAP != (node=" + reader.Name + ")");
               return false;
            }
            string? sMainGunAP = reader.GetAttribute("value");
            if (null == sMainGunAP)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): MainGunAP=null");
               return false;
            }
            report.MainGunAP = Convert.ToInt32(sMainGunAP);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "MainGunWP")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): MainGunWP != (node=" + reader.Name + ")");
               return false;
            }
            string? sMainGunWP = reader.GetAttribute("value");
            if (null == sMainGunWP)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): MainGunWP=null");
               return false;
            }
            report.MainGunWP = Convert.ToInt32(sMainGunWP);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "MainGunHBCI")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): MainGunHBCI != (node=" + reader.Name + ")");
               return false;
            }
            string? sMainGunHBCI = reader.GetAttribute("value");
            if (null == sMainGunHBCI)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): MainGunHBCI=null");
               return false;
            }
            report.MainGunHBCI = Convert.ToInt32(sMainGunHBCI);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "MainGunHVAP")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): MainGunHVAP != (node=" + reader.Name + ")");
               return false;
            }
            string? sMainGunHVAP = reader.GetAttribute("value");
            if (null == sMainGunHVAP)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): MainGunHVAP=null");
               return false;
            }
            report.MainGunHVAP = Convert.ToInt32(sMainGunHVAP);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "VictoryPtsFriendlyKiaLightWeapon")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaLightWeapon != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsFriendlyKiaLightWeapon = reader.GetAttribute("value");
            if (null == sVictoryPtsFriendlyKiaLightWeapon)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sVictoryPtsFriendlyKiaLightWeapon=null");
               return false;
            }
            report.VictoryPtsFriendlyKiaLightWeapon = Convert.ToInt32(sVictoryPtsFriendlyKiaLightWeapon);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "VictoryPtsFriendlyKiaTruck")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaTruck != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsFriendlyKiaTruck = reader.GetAttribute("value");
            if (null == sVictoryPtsFriendlyKiaTruck)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sVictoryPtsFriendlyKiaTruck=null");
               return false;
            }
            report.VictoryPtsFriendlyKiaTruck = Convert.ToInt32(sVictoryPtsFriendlyKiaTruck);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement() = false");
               return false;
            }
            if (reader.Name != "VictoryPtsFriendlyKiaSpwOrPsw")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaSpwOrPsw != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsFriendlyKiaSpwOrPsw = reader.GetAttribute("value");
            if (null == sVictoryPtsFriendlyKiaSpwOrPsw)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sVictoryPtsFriendlyKiaSpwOrPsw=null");
               return false;
            }
            report.VictoryPtsFriendlyKiaSpwOrPsw = Convert.ToInt32(sVictoryPtsFriendlyKiaSpwOrPsw);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsFriendlyKiaSPGun) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsFriendlyKiaSPGun")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaSPGun != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsFriendlyKiaSPGun = reader.GetAttribute("value");
            if (null == sVictoryPtsFriendlyKiaSPGun)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaSPGun=null");
               return false;
            }
            report.VictoryPtsFriendlyKiaSPGun = Convert.ToInt32(sVictoryPtsFriendlyKiaSPGun);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsFriendlyKiaPzIV) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsFriendlyKiaPzIV")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaPzIV != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsFriendlyKiaPzIV = reader.GetAttribute("value");
            if (null == sVictoryPtsFriendlyKiaPzIV)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaPzIV=null");
               return false;
            }
            report.VictoryPtsFriendlyKiaPzIV = Convert.ToInt32(sVictoryPtsFriendlyKiaPzIV);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsFriendlyKiaPzV) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsFriendlyKiaPzV")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaPzV != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsFriendlyKiaPzV = reader.GetAttribute("value");
            if (null == sVictoryPtsFriendlyKiaPzV)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaPzV=null");
               return false;
            }
            report.VictoryPtsFriendlyKiaPzV = Convert.ToInt32(sVictoryPtsFriendlyKiaPzV);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsFriendlyKiaPzVI) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsFriendlyKiaPzVI")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaPzVI != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsFriendlyKiaPzVI = reader.GetAttribute("value");
            if (null == sVictoryPtsFriendlyKiaPzVI)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaPzVI=null");
               return false;
            }
            report.VictoryPtsFriendlyKiaPzVI = Convert.ToInt32(sVictoryPtsFriendlyKiaPzVI);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsFriendlyKiaAtGun) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsFriendlyKiaAtGun")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaAtGun != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsFriendlyKiaAtGun = reader.GetAttribute("value");
            if (null == sVictoryPtsFriendlyKiaAtGun)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaAtGun=null");
               return false;
            }
            report.VictoryPtsFriendlyKiaAtGun = Convert.ToInt32(sVictoryPtsFriendlyKiaAtGun);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsFriendlyKiaFortifiedPosition) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsFriendlyKiaFortifiedPosition")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaFortifiedPosition != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsFriendlyKiaFortifiedPosition = reader.GetAttribute("value");
            if (null == sVictoryPtsFriendlyKiaFortifiedPosition)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyKiaFortifiedPosition=null");
               return false;
            }
            report.VictoryPtsFriendlyKiaFortifiedPosition = Convert.ToInt32(sVictoryPtsFriendlyKiaFortifiedPosition);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsYourKiaLightWeapon) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsYourKiaLightWeapon")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaLightWeapon != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsYourKiaLightWeapon = reader.GetAttribute("value");
            if (null == sVictoryPtsYourKiaLightWeapon)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaLightWeapon=null");
               return false;
            }
            report.VictoryPtsYourKiaLightWeapon = Convert.ToInt32(sVictoryPtsYourKiaLightWeapon);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsYourKiaTruck) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsYourKiaTruck")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaTruck != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsYourKiaTruck = reader.GetAttribute("value");
            if (null == sVictoryPtsYourKiaTruck)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaTruck=null");
               return false;
            }
            report.VictoryPtsYourKiaTruck = Convert.ToInt32(sVictoryPtsYourKiaTruck);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsYourKiaSpwOrPsw) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsYourKiaSpwOrPsw")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaSpwOrPsw != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsYourKiaSpwOrPsw = reader.GetAttribute("value");
            if (null == sVictoryPtsYourKiaSpwOrPsw)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaSpwOrPsw=null");
               return false;
            }
            report.VictoryPtsYourKiaSpwOrPsw = Convert.ToInt32(sVictoryPtsYourKiaSpwOrPsw);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsYourKiaSPGun) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsYourKiaSPGun")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaSPGun != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsYourKiaSPGun = reader.GetAttribute("value");
            if (null == sVictoryPtsYourKiaSPGun)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaSPGun=null");
               return false;
            }
            report.VictoryPtsYourKiaSPGun = Convert.ToInt32(sVictoryPtsYourKiaSPGun);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsYourKiaPzIV) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsYourKiaPzIV")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaPzIV != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsYourKiaPzIV = reader.GetAttribute("value");
            if (null == sVictoryPtsYourKiaPzIV)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaPzIV=null");
               return false;
            }
            report.VictoryPtsYourKiaPzIV = Convert.ToInt32(sVictoryPtsYourKiaPzIV);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsYourKiaPzV) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsYourKiaPzV")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaPzV != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsYourKiaPzV = reader.GetAttribute("value");
            if (null == sVictoryPtsYourKiaPzV)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaPzV=null");
               return false;
            }
            report.VictoryPtsYourKiaPzV = Convert.ToInt32(sVictoryPtsYourKiaPzV);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsYourKiaPzVI) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsYourKiaPzVI")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaPzVI != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsYourKiaPzVI = reader.GetAttribute("value");
            if (null == sVictoryPtsYourKiaPzVI)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaPzVI=null");
               return false;
            }
            report.VictoryPtsYourKiaPzVI = Convert.ToInt32(sVictoryPtsYourKiaPzVI);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsYourKiaAtGun) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsYourKiaAtGun")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaAtGun != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsYourKiaAtGun = reader.GetAttribute("value");
            if (null == sVictoryPtsYourKiaAtGun)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaAtGun=null");
               return false;
            }
            report.VictoryPtsYourKiaAtGun = Convert.ToInt32(sVictoryPtsYourKiaAtGun);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsYourKiaFortifiedPosition) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsYourKiaFortifiedPosition")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaFortifiedPosition != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsYourKiaFortifiedPosition = reader.GetAttribute("value");
            if (null == sVictoryPtsYourKiaFortifiedPosition)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsYourKiaFortifiedPosition=null");
               return false;
            }
            report.VictoryPtsYourKiaFortifiedPosition = Convert.ToInt32(sVictoryPtsYourKiaFortifiedPosition);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsCaptureArea) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsCaptureArea")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsCaptureArea != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsCaptureArea = reader.GetAttribute("value");
            if (null == sVictoryPtsCaptureArea)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsCaptureArea=null");
               return false;
            }
            report.VictoryPtsCaptureArea = Convert.ToInt32(sVictoryPtsCaptureArea);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsCapturedExitArea) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsCapturedExitArea")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsCapturedExitArea != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsCapturedExitArea = reader.GetAttribute("value");
            if (null == sVictoryPtsCapturedExitArea)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsCapturedExitArea=null");
               return false;
            }
            report.VictoryPtsCapturedExitArea = Convert.ToInt32(sVictoryPtsCapturedExitArea);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsLostArea) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsLostArea")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsLostArea != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsLostArea = reader.GetAttribute("value");
            if (null == sVictoryPtsLostArea)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsLostArea=null");
               return false;
            }
            report.VictoryPtsLostArea = Convert.ToInt32(sVictoryPtsLostArea);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsFriendlyTank) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsFriendlyTank")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyTank != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsFriendlyTank = reader.GetAttribute("value");
            if (null == sVictoryPtsFriendlyTank)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlyTank=null");
               return false;
            }
            report.VictoryPtsFriendlyTank = Convert.ToInt32(sVictoryPtsFriendlyTank);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsFriendlySquad) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsFriendlySquad")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlySquad != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsFriendlySquad = reader.GetAttribute("value");
            if (null == sVictoryPtsFriendlySquad)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsFriendlySquad=null");
               return false;
            }
            report.VictoryPtsFriendlySquad = Convert.ToInt32(sVictoryPtsFriendlySquad);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsTotalYourTank) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsTotalYourTank")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsTotalYourTank != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsTotalYourTank = reader.GetAttribute("value");
            if (null == sVictoryPtsTotalYourTank)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsTotalYourTank=null");
               return false;
            }
            report.VictoryPtsTotalYourTank = Convert.ToInt32(sVictoryPtsTotalYourTank);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsTotalFriendlyForces) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsTotalFriendlyForces")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsTotalFriendlyForces != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsTotalFriendlyForces = reader.GetAttribute("value");
            if (null == sVictoryPtsTotalFriendlyForces)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsTotalFriendlyForces=null");
               return false;
            }
            report.VictoryPtsTotalFriendlyForces = Convert.ToInt32(sVictoryPtsTotalFriendlyForces);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsTotalTerritory) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsTotalTerritory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsTotalTerritory != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsTotalTerritory = reader.GetAttribute("value");
            if (null == sVictoryPtsTotalTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsTotalTerritory=null");
               return false;
            }
            report.VictoryPtsTotalTerritory = Convert.ToInt32(sVictoryPtsTotalTerritory);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(VictoryPtsTotalEngagement) = false");
               return false;
            }
            if (reader.Name != "VictoryPtsTotalEngagement")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsTotalEngagement != (node=" + reader.Name + ")");
               return false;
            }
            string? sVictoryPtsTotalEngagement = reader.GetAttribute("value");
            if (null == sVictoryPtsTotalEngagement)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): VictoryPtsTotalEngagement=null");
               return false;
            }
            report.VictoryPtsTotalEngagement = Convert.ToInt32(sVictoryPtsTotalEngagement);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(Decorations) = false");
               return false;
            }
            if (reader.Name != "Decorations")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
               return false;
            }
            string? sCountDecoration = reader.GetAttribute("count");
            if (null == sCountDecoration)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sCountDecoration=null");
               return false;
            }
            int countDecoration = Convert.ToInt32(sCountDecoration);
            for (int i = 0; i < countDecoration; ++i)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(Decoration) = false");
                  return false;
               }
               if (reader.Name != "Decoration")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
                  return false;
               }
               string? sDecoration = reader.GetAttribute("value");
               if (null == sDecoration)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sDecoration=null");
                  return false;
               }
               EnumDecoration decoration = EnumDecoration.WW2Victory;
               switch (sDecoration)
               {
                  case "BronzeStar": decoration = EnumDecoration.BronzeStar; break;
                  case "SilverStar": decoration = EnumDecoration.SilverStar; break;
                  case "DistinguisedServiceCross": decoration = EnumDecoration.DistinguisedServiceCross; break;
                  case "MedalOfHonor": decoration = EnumDecoration.MedalOfHonor; break;
                  case "PurpleHeart": decoration = EnumDecoration.PurpleHeart; break;
                  case "EuropeanCampain": decoration = EnumDecoration.EuropeanCampain; break;
                  case "WW2Victory": decoration = EnumDecoration.WW2Victory; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reached default sDecoration=" + sDecoration); return false;
               }
               report.Decorations.Add(decoration);
            }
            if (0 < countDecoration)
               reader.Read();
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(Notes) = false");
               return false;
            }
            if (reader.Name != "Notes")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
               return false;
            }
            string? sCountNote = reader.GetAttribute("count");
            if (null == sCountNote)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sCountNote=null");
               return false;
            }
            int countNote = Convert.ToInt32(sCountNote);
            for (int i = 0; i < countNote; ++i)
            {
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(Note) = false");
                  return false;
               }
               if (reader.Name != "Note")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): node=" + reader.Name);
                  return false;
               }
               string? sNote = reader.GetAttribute("value");
               if (null == sNote)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): sNote=null");
                  return false;
               }
               report.Notes.Add(sNote);
            }
            if (0 < countNote)
               reader.Read();
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(DayEndedTime) = false");
               return false;
            }
            if (reader.Name != "DayEndedTime")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): DayEndedTime != (node=" + reader.Name + ")");
               return false;
            }
            string? sDayEndedTime = reader.GetAttribute("value");
            if (null == sDayEndedTime)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): DayEndedTime=null");
               return false;
            }
            report.DayEndedTime = sDayEndedTime;
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(Breakdown) = false");
               return false;
            }
            if (reader.Name != "Breakdown")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): Breakdown != (node=" + reader.Name + ")");
               return false;
            }
            string? sBreakdown = reader.GetAttribute("value");
            if (null == sBreakdown)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): Breakdown=null");
               return false;
            }
            report.Breakdown = sBreakdown;
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): reader.IsStartElement(KnockedOut) = false");
               return false;
            }
            if (reader.Name != "KnockedOut")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): KnockedOut != (node=" + reader.Name + ")");
               return false;
            }
            string? sKnockedOut = reader.GetAttribute("value");
            if (null == sKnockedOut)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlReportsReport(): KnockedOut=null");
               return false;
            }
            report.KnockedOut = sKnockedOut;
         }
         reader.Read(); // get past </Report>
         return true;
      }
      private bool ReadXmlMapItems(XmlReader reader, IMapItems mapItems, string attribute)
      {
         mapItems.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): IsStartElement(MapItems)=null");
            return false;
         }
         if (reader.Name != "MapItems")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): MapItems != (node=" + reader.Name + ")");
            return false;
         }
         string? sAttribute = reader.GetAttribute("value");
         if (sAttribute != attribute)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): (sAttribute=" + sAttribute + ") != (attribute=" + attribute + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            IMapItem? mapItem = null;
            if (false == ReadXmlMapItem(reader, ref mapItem))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): ReadXml_MapItem() returned false");
               return false;
            }
            if (null == mapItem)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): mapItem=null");
               return false;
            }
            mapItems.Add(mapItem);
         }
         if (0 < count)
            reader.Read(); 
         return true;
      }
      private bool ReadXmlMapItem(XmlReader reader, ref IMapItem? mi)
      {
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItem(): reader.IsStartElement(Name) = false");
            return false;
         }
         if (reader.Name != "MapItem")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItem(): Name != (node=" + reader.Name + ")");
            return false;
         }
         string? sValue = reader.GetAttribute("value");
         if (null == sValue)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItem(): sNasValueme=null");
            return false;
         }
         string? sName = reader.GetAttribute("name");
         if (null == sName)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItem(): sName=null");
            return false;
         }
         if ("null" == sName)
         {
            mi = null;
         }
         else
         {
            mi = theMapItems.Find(sName);
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItem(): sName=null");
               return false;
            }
         }
         return true;
      }
      private bool ReadXmlCrewMembers(XmlReader reader, ICrewMembers crewMembers, string attribute)
      {
         crewMembers.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMembers(): IsStartElement(MapItems)=null");
            return false;
         }
         if (reader.Name != "CrewMembers")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMembers(): MapItems != (node=" + reader.Name + ")");
            return false;
         }
         string? sAttribute = reader.GetAttribute("value");
         if (sAttribute != attribute)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMembers(): (sAttribute=" + sAttribute + ") != (attribute=" + attribute + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMembers(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            ICrewMember? crewMember = null;
            if (false == ReadXmlCrewMember(reader, ref crewMember))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMembers(): ReadXmlCrewMember() returned false");
               return false;
            }
            if( null == crewMember )
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMembers(): crewMember=null");
               return false;
            }
            crewMembers.Add(crewMember);
         }
         if (0 < count)
            reader.Read();
         return true;
      }
      private bool ReadXmlCrewMember(XmlReader reader, ref ICrewMember? member)
      {
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): reader.IsStartElement(CrewMember) = false");
            return false;
         }
         if (reader.Name != "CrewMember")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): Name != (node=" + reader.Name + ")");
            return false;
         }
         string? sRole = reader.GetAttribute("value");
         if (null == sRole)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): sName=null");
            return false;
         }
         if( "null" == sRole )
         {
            member = null;
            return true;
         }
         //----------------------------------------------
         IMapItem? mapItem = null;
         if (false == ReadXmlMapItem(reader, ref mapItem))
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): ReadXmlMapItem() returned false");
            return false;
         }
         if( null == mapItem )
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): mapItem=null");
            return false;
         }
         //----------------------------------------------
         member = new CrewMember();
         member.Role = sRole;
         member.Copy(mapItem);
         //----------------------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): reader.IsStartElement(Rank) = false");
            return false;
         }
         if (reader.Name != "Rank")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): Rank != (node=" + reader.Name + ")");
            return false;
         }
         string? sRank = reader.GetAttribute("value");
         if (null == sRank)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): Rank=null");
            return false;
         }
         member.Rank = sRank;
         //----------------------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): reader.IsStartElement(Rating) = false");
            return false;
         }
         if (reader.Name != "Rating")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): Rating != (node=" + reader.Name + ")");
            return false;
         }
         string? sRating = reader.GetAttribute("value");
         if (null == sRating)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): Rating=null");
            return false;
         }
         member.Rating = Convert.ToInt32(sRating);
         //----------------------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): reader.IsStartElement(IsButtonedUp) = false");
            return false;
         }
         if (reader.Name != "IsButtonedUp")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): IsButtonedUp != (node=" + reader.Name + ")");
            return false;
         }
         string? sIsButtonedUp = reader.GetAttribute("value");
         if (null == sIsButtonedUp)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): IsButtonedUp=null");
            return false;
         }
         member.IsButtonedUp = Convert.ToBoolean(sIsButtonedUp);
         //----------------------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): reader.IsStartElement(Wound) = false");
            return false;
         }
         if (reader.Name != "Wound")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): Wound != (node=" + reader.Name + ")");
            return false;
         }
         string? sWound = reader.GetAttribute("value");
         if (null == sWound)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): Wound=null");
            return false;
         }
         member.Wound = sWound;
         //----------------------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): reader.IsStartElement(WoundDaysUntilReturn) = false");
            return false;
         }
         if (reader.Name != "WoundDaysUntilReturn")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): WoundDaysUntilReturn != (node=" + reader.Name + ")");
            return false;
         }
         string? sWoundDaysUntilReturn = reader.GetAttribute("value");
         if (null == sWoundDaysUntilReturn)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlCrewMember(): WoundDaysUntilReturn=null");
            return false;
         }
         member.WoundDaysUntilReturn = Convert.ToInt32(sWoundDaysUntilReturn);
         reader.Read(); // get past </CrewMember>
         return true;
      }
      private bool ReadXmlTerritories(XmlReader reader, ITerritories territories, string attribute)
      {
         territories.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories2(): IsStartElement(Territories) returned false");
            return false;
         }
         if (reader.Name != "Territories")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories2(): Territories != (node=" + reader.Name + ")");
            return false;
         }
         string? attributeRead = reader.GetAttribute("value");
         if (attribute != attributeRead)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories2(): attributeRead=null for attribute=" + attribute);
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories2(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories2(): IsStartElement(GameInstance) returned false");
               return false;
            }
            if (reader.Name != "Territory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories2(): Territory != (node=" + reader.Name + ")");
               return false;
            }
            //-------------------------------------------
            string? tName = reader.GetAttribute("value");
            if (null == tName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories2(): GetAttribute(tName) returned false");
               return false;
            }
            string? tType = reader.GetAttribute("type");
            if (null == tType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories2(): GetAttribute(tType) returned false");
               return false;
            }
            //-------------------------------------------
            ITerritory? territory = Territories.theTerritories.Find(tName, tType);
            if (null == territory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories2(): Find() returned null for tName=" + tName + " tType=" + tType);
               return false;
            }
            territories.Add(territory);
         }
         if (0 < count)
            reader.Read(); // get past </Territories> tag
         return true;
      }
      private bool ReadXmlFirstShots(XmlReader reader, List<string> firstShots)
      {
         firstShots.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlFirstShots(): IsStartElement(FirstShots) returned false");
            return false;
         }
         if (reader.Name != "FirstShots")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlFirstShots(): FirstShots != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlFirstShots(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i=0; i< count; i++)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlFirstShots(): IsStartElement(FirstShot) returned false");
               return false;
            }
            if (reader.Name != "FirstShot")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlFirstShots(): FirstShot != (node=" + reader.Name + ")");
               return false;
            }
            string? miName = reader.GetAttribute("value");
            if (null == miName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlFirstShots(): GetAttribute(miName) returned false");
               return false;
            }
            firstShots.Add(miName);
         }
         if( 0 < count )
            reader.Read(); // get past </FirstShots> tag
         return true;
      }
      private bool ReadXmlTrainedGunners(XmlReader reader, List<string> trainedGunners)
      {
         trainedGunners.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlTrainedGunners(): IsStartElement(TrainedGunners) returned false");
            return false;
         }
         if (reader.Name != "TrainedGunners")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlTrainedGunners(): TrainedGunners != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlTrainedGunners(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; i++)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTrainedGunners(): IsStartElement(TrainedGunner) returned false");
               return false;
            }
            if (reader.Name != "TrainedGunner")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTrainedGunners(): TrainedGunner != (node=" + reader.Name + ")");
               return false;
            }
            string? miName = reader.GetAttribute("value");
            if (null == miName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTrainedGunners(): GetAttribute(miName) returned false");
               return false;
            }
            trainedGunners.Add(miName);
         }
         if( 0 < count )
            reader.Read(); // get past </TrainedGunners> tag
         return true;
      }
      private bool ReadXmlShermanHits(XmlReader reader, List<ShermanAttack> hits)
      {
         hits.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlSherman_Hits(): IsStartElement(ShermanHits) returned false");
            return false;
         }
         if (reader.Name != "ShermanHits")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlSherman_Hits(): ShermanHits != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlSherman_Hits(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; i++)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlSherman_Hits(): IsStartElement(ShermanHit) returned false");
               return false;
            }
            if (reader.Name != "ShermanHit")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlSherman_Hits(): ShermanHit != (node=" + reader.Name + ")");
               return false;
            }
            //-----------------------------
            string? sAttackType = reader.GetAttribute("AttackType");
            if (null == sAttackType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlSherman_Hits(): AttackType=null");
               return false;
            }
            //-----------------------------
            string? sAmmoType = reader.GetAttribute("AmmoType");
            if (null == sAmmoType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlSherman_Hits(): AmmoType=null");
               return false;
            }
            //-----------------------------
            string? sIsCriticalHit = reader.GetAttribute("IsCriticalHit");
            if (null == sIsCriticalHit)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlSherman_Hits(): IsCriticalHit=null");
               return false;
            }
            bool isCriticalHit = Convert.ToBoolean(sIsCriticalHit);
            //-----------------------------
            string? sHitLocation = reader.GetAttribute("HitLocation");
            if (null == sHitLocation)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlSherman_Hits(): HitLocation=null");
               return false;
            }
            //-----------------------------
            string? sIsNoChance = reader.GetAttribute("IsNoChance");
            if (null == sIsNoChance)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlSherman_Hits(): IsNoChance=null");
               return false;
            }
            bool isNoChance = Convert.ToBoolean(sIsNoChance);
            //-----------------------------
            string? sIsImmobilization = reader.GetAttribute("IsImmobilization");
            if (null == sIsImmobilization)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlSherman_Hits(): IsImmobilization=null");
               return false;
            }
            bool isImmobilization = Convert.ToBoolean(sIsImmobilization);
            //-----------------------------
            ShermanAttack attack = new ShermanAttack(sAttackType, sAmmoType, isCriticalHit, isImmobilization);
            attack.myIsNoChance = isNoChance;
            attack.myHitLocation = sHitLocation;
            //---------------------------
            hits.Add(attack);
         }
         if( 0 < count )
            reader.Read(); // get past </Sherman Hits> tag
         return true;
      }
      private bool ReadXmlShermanDeath(XmlReader reader, ref ShermanDeath? death)
      {
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsStartElement(ShermanDeath) returned false");
            return false;
         }
         if (reader.Name != "ShermanDeath")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): ShermanDeath != (node=" + reader.Name + ")");
            return false;
         }
         string? value = reader.GetAttribute("value");
         if( "null" == value )
         {
            death = null;
            return true;
         }
         //----------------------------------
         IMapItem? enemyUnit = null;
         if ( false == ReadXmlMapItem(reader, ref enemyUnit))
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): ReadXmlMapItem() returned false");
            return false;
         }
         if( null == enemyUnit )
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): enemyUnit=null");
            return false;
         }
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsStartElement(HitLocation) returned false");
            return false;
         }
         if (reader.Name != "HitLocation")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): HitLocation != (node=" + reader.Name + ")");
            return false;
         }
         string? sHitLocation = reader.GetAttribute("value");
         if (null == sHitLocation)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): GetAttribute(sHitLocation) returned false");
            return false;
         }
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsStartElement(EnemyFireDirection) returned false");
            return false;
         }
         if (reader.Name != "EnemyFireDirection")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): EnemyFireDirection != (node=" + reader.Name + ")");
            return false;
         }
         string? sEnemyFireDirection = reader.GetAttribute("value");
         if (null == sEnemyFireDirection)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): GetAttribute(EnemyFireDirection) returned false");
            return false;
         }
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsStartElement(Day) returned false");
            return false;
         }
         if (reader.Name != "Day")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): Day != (node=" + reader.Name + ")");
            return false;
         }
         string? sDay = reader.GetAttribute("value");
         if (null == sDay)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): GetAttribute(sDay) returned false");
            return false;
         }
         int day = Convert.ToInt32(sDay);
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsStartElement(Cause) returned false");
            return false;
         }
         if (reader.Name != "Cause")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): Cause != (node=" + reader.Name + ")");
            return false;
         }
         string? sCause = reader.GetAttribute("value");
         if (null == sCause)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): GetAttribute(sCause) returned false");
            return false;
         }
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsStartElement(IsAmbush) returned false");
            return false;
         }
         if (reader.Name != "IsAmbush")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsAmbush != (node=" + reader.Name + ")");
            return false;
         }
         string? sIsAmbush = reader.GetAttribute("value");
         if (null == sIsAmbush)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): GetAttribute(sIsAmbush) returned false");
            return false;
         }
         bool isAmbush = Convert.ToBoolean(sIsAmbush);
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsStartElement(IsExplosion) returned false");
            return false;
         }
         if (reader.Name != "IsExplosion")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsExplosion != (node=" + reader.Name + ")");
            return false;
         }
         string? sIsExplosion = reader.GetAttribute("value");
         if (null == sIsExplosion)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): GetAttribute(sIsExplosion) returned false");
            return false;
         }
         bool isExplosion = Convert.ToBoolean(sIsExplosion);
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsStartElement(IsBailout) returned false");
            return false;
         }
         if (reader.Name != "IsBailout")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsBailout != (node=" + reader.Name + ")");
            return false;
         }
         string? sIsBailout = reader.GetAttribute("value");
         if (null == sIsBailout)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): GetAttribute(IsBailout) returned false");
            return false;
         }
         bool isBailout = Convert.ToBoolean(sIsBailout);
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsStartElement(IsBrewUp) returned false");
            return false;
         }
         if (reader.Name != "IsBrewUp")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsBrewUp != (node=" + reader.Name + ")");
            return false;
         }
         string? sIsBrewUp = reader.GetAttribute("value");
         if (null == sIsBrewUp)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): GetAttribute(IsBrewUp) returned false");
            return false;
         }
         bool isBrewUp = Convert.ToBoolean(sIsBrewUp);
         //----------------------------------
         death = new ShermanDeath(enemyUnit);
         death.myHitLocation = sHitLocation;
         death.myEnemyFireDirection = sEnemyFireDirection;
         death.myDay = day;
         death.myCause = sCause;
         death.myIsAmbush = isAmbush;
         death.myIsExplosion = isExplosion;
         death.myIsCrewBail = isBailout;
         death.myIsBrewUp = isBrewUp;
         //----------------------------------
         reader.Read(); // get past </ShermanDeath> tag
         return true;
      }
      private bool ReadXmlShermanSetup(XmlReader reader, ShermanSetup battlePrep)
      {
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): IsStartElement(ShermanSetup) returned false");
            return false;
         }
         if (reader.Name != "ShermanSetup")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlShermanDeath(): ShermanSetup != (node=" + reader.Name + ")");
            return false;
         }
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack():  IsStartElement(IsSetupPerformed) returned false");
            return false;
         }
         if (reader.Name != "IsSetupPerformed")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): IsSetupPerformed != (node=" + reader.Name + ")");
            return false;
         }
         string? sIsSetupPerformed = reader.GetAttribute("value");
         if (null == sIsSetupPerformed)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): GetAttribute(sIsSetupPerformed) returned false");
            return false;
         }
         bool isSetupPerformed = Convert.ToBoolean(sIsSetupPerformed);
         //----------------------------------------------
         IMapItems hatches = new MapItems();
         if (false == ReadXmlMapItems(reader, hatches, "Hatches"))
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXml_MapItems(Hatches) returned null");
            return false;
         }
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack():  IsStartElement(AmmoType) returned false");
            return false;
         }
         if (reader.Name != "AmmoType")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): AmmoType != (node=" + reader.Name + ")");
            return false;
         }
         string? sAmmoType = reader.GetAttribute("value");
         if (null == sAmmoType)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): GetAttribute(AmmoType) returned false");
            return false;
         }
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack():  IsStartElement(TurretRotation) returned false");
            return false;
         }
         if (reader.Name != "TurretRotation")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): TurretRotation != (node=" + reader.Name + ")");
            return false;
         }
         string? sTurretRotation = reader.GetAttribute("value");
         if (null == sTurretRotation)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): GetAttribute(sTurretRotation) returned false");
            return false;
         }
         double turretRotation = Convert.ToDouble(sTurretRotation);
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack():  IsStartElement(LoaderSpotTerritory) returned false");
            return false;
         }
         if (reader.Name != "LoaderSpotTerritory")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): LoaderSpotTerritory != (node=" + reader.Name + ")");
            return false;
         }
         string? sLoaderSpotTerritory = reader.GetAttribute("value");
         if (null == sLoaderSpotTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): GetAttribute(sLoaderSpotTerritory) returned false");
            return false;
         }
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack():  IsStartElement(CommanderSpotTerritory) returned false");
            return false;
         }
         if (reader.Name != "CommanderSpotTerritory")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): CommanderSpotTerritory != (node=" + reader.Name + ")");
            return false;
         }
         string? sCommanderSpotTerritory = reader.GetAttribute("value");
         if (null == sCommanderSpotTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): GetAttribute(sCommanderSpotTerritory) returned false");
            return false;
         }
         //----------------------------------
         battlePrep.Clear();
         battlePrep.myIsSetupPerformed = isSetupPerformed;
         foreach(IMapItem mi in hatches)
            battlePrep.myHatches.Add(mi);
         battlePrep.myAmmoType = sAmmoType;
         battlePrep.myTurretRotation = turretRotation;
         battlePrep.myLoaderSpotTerritory = sLoaderSpotTerritory;
         battlePrep.myCommanderSpotTerritory = sCommanderSpotTerritory;
         //----------------------------------
         reader.Read(); // get past </ShermanSetup> tag
         return true;
      }
      private bool ReadXmlPanzerfaultAttack(XmlReader reader, ref PanzerfaustAttack? attack)
      {
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): IsStartElement(Panzerfaust) returned false");
            return false;
         }
         if (reader.Name != "PanzerfaustAttack")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): PanzerfaustAttack != (node=" + reader.Name + ")");
            return false;
         }
         string? value = reader.GetAttribute("value");
         if ("null" == value)
         {
            attack = null;
            return true;
         }
         //----------------------------------
         IMapItem? enemyUnit = null;
         if (false == ReadXmlMapItem(reader, ref enemyUnit))
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): ReadXmlMapItem() returned false");
            return false;
         }
         if (null == enemyUnit)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): enemyUnit=null");
            return false;
         }
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack():  IsStartElement(Day) returned false");
            return false;
         }
         if (reader.Name != "Day")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): Day != (node=" + reader.Name + ")");
            return false;
         }
         string? sDay = reader.GetAttribute("value");
         if (null == sDay)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): GetAttribute(Day) returned false");
            return false;
         }
         int day = Convert.ToInt32(sDay);
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): IsStartElement(IsShermanMoving) returned false");
            return false;
         }
         if (reader.Name != "IsShermanMoving")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): IsShermanMoving != (node=" + reader.Name + ")");
            return false;
         }
         string? sIsShermanMoving = reader.GetAttribute("value");
         if (null == sIsShermanMoving)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): GetAttribute(IsShermanMoving) returned false");
            return false;
         }
         bool isShermanMoving = Convert.ToBoolean(sIsShermanMoving);
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): IsStartElement(IsLeadTank) returned false");
            return false;
         }
         if (reader.Name != "IsLeadTank")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): IsLeadTank != (node=" + reader.Name + ")");
            return false;
         }
         string? sIsLeadTank = reader.GetAttribute("value");
         if (null == sIsLeadTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): GetAttribute(IsLeadTank) returned false");
            return false;
         }
         bool isLeadTank = Convert.ToBoolean(sIsLeadTank);
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): IsStartElement(IsAdvancingFireZone) returned false");
            return false;
         }
         if (reader.Name != "IsAdvancingFireZone")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): IsAdvancingFireZone != (node=" + reader.Name + ")");
            return false;
         }
         string? sIsAdvancingFireZone = reader.GetAttribute("value");
         if (null == sIsAdvancingFireZone)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): GetAttribute(IsAdvancingFireZone) returned false");
            return false;
         }
         bool isAdvancingFireZone = Convert.ToBoolean(sIsAdvancingFireZone);
         //----------------------------------
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): IsStartElement(Sector) returned false");
            return false;
         }
         if (reader.Name != "Sector")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): Sector != (node=" + reader.Name + ")");
            return false;
         }
         string? sSector = reader.GetAttribute("value");
         if (null == sSector)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlPanzerfaultAttack(): GetAttribute(Sector) returned false");
            return false;
         }
         char sector = Convert.ToChar(sSector);
         //----------------------------------
         attack = new PanzerfaustAttack(enemyUnit);
         attack.myDay = day;
         attack.myIsShermanMoving = isShermanMoving;
         attack.myIsLeadTank = isLeadTank;
         attack.myIsAdvancingFireZone = isAdvancingFireZone;
         attack.mySector = sector;
         reader.Read(); // get past </Panzerfaust>
         return true;
      }
      private bool ReadXmlMapItemMoves(XmlReader reader, IMapItemMoves mapItemMoves)
      {
         mapItemMoves.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): IsStartElement(MapItemMoves) returned false");
            return false;
         }
         if (reader.Name != "MapItemMoves")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): MapItemMoves != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for( int i=0; i<count; ++i)
         {
            IMapItemMove mim = new MapItemMove();
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): IsStartElement(MapItemMove) returned false");
               return false;
            }
            if (reader.Name != "MapItemMove")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): MapItemMove != (node=" + reader.Name + ")");
               return false;
            }
            string? miName = reader.GetAttribute("value");
            if (null == miName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): miName=null");
               return false;
            }
            //----------------------------------------------
            IMapItem? mi = null;
            if( false == ReadXmlMapItem(reader, ref mi))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): ReadXmlMapItem() returned false");
               return false;
            }
            if( null == mi )
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): mi=null");
               return false;
            }
            mim.MapItem = mi;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): IsStartElement(OldTerritory) returned false");
               return false;
            }
            if (reader.Name != "OldTerritory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): OldTerritory != (node=" + reader.Name + ")");
               return false;
            }
            string? sOldTerritory = reader.GetAttribute("value");
            if ("null" == sOldTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sOldTerritory=*null*");
               return false;
            }
            if (null == sOldTerritory) 
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sOldTerritory=null");
               return false;
            }
            mim.OldTerritory = Territories.theTerritories.Find(sOldTerritory);
            if( null == mim.OldTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sOldTerritory=null for name=" + sOldTerritory);
               return false;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): IsStartElement(NewTerritory) returned false");
               return false;
            }
            if (reader.Name != "NewTerritory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): NewTerritory != (node=" + reader.Name + ")");
               return false;
            }
            string? sNewTerritory = reader.GetAttribute("value");
            if ("null" == sNewTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sNewTerritory=*null*");
               return false;
            }
            if (null == sNewTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sNewTerritory=null");
               return false;
            }
            mim.NewTerritory = Territories.theTerritories.Find(sNewTerritory);
            if (null == mim.NewTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sOldTerritory=null for name=" + sNewTerritory);
               return false;
            }
            //----------------------------------------------
            IMapPath? path = null; 
            if( false == ReadXmlMapItemMoveBestPath(reader, ref path))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): ReadXmlMapItemMoveBestPath() returned false");
               return false;
            }
            if (null == path)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): ReadXmlMapItemMoveBestPath() returned path=null");
               return false;
            }
            reader.Read(); // get past </MapItemMove>
            mim.BestPath = path;
            //----------------------------------------------
            mapItemMoves.Add(mim);
         }
         if ( 0 < count )
            reader.Read(); // get past </MapItemMoves>
         return true;
      }
      private bool ReadXmlMapItemMoveBestPath(XmlReader reader, ref IMapPath? path)
      {
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): IsStartElement(BestPath) returned false");
            return false;
         }
         if (reader.Name != "BestPath")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): BestPath != (node=" + reader.Name + ")");
            return false;
         }
         string? sName = reader.GetAttribute("name");
         if (null == sName)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): sName=null");
            return false;
         }
         //------------------------------
         string? sMetric = reader.GetAttribute("metric");
         if (null == sMetric)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): sMetric=null");
            return false;
         }
         double metric = Convert.ToDouble(sMetric);
         //------------------------------
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): sCount=null");
            return false;
         }
         int count = int.Parse(sCount);
         //------------------------------
         List<ITerritory> territories = new List<ITerritory>();
         for (int i=0; i<count; ++i )
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): IsStartElement(Territory) returned false");
               return false;
            }
            if (reader.Name != "Territory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): MapItemMoves != (node=" + reader.Name + ")");
               return false;
            }
            string? tName = reader.GetAttribute("name");
            if (null == tName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): tName=null");
               return false;
            }
            ITerritory? t = Territories.theTerritories.Find(tName);
            if( null == t )
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): tName=null for tName=" + tName);
               return false;
            }
            territories.Add(t);
         }
         if( 0 < count )
            reader.Read(); // get past </BestPath>
         path = new MapPath(sName);
         path.Metric = metric;
         path.Territories = territories;
         return true;
      }
      private bool ReadXmlStacks(XmlReader reader, IStacks stacks, string attribute)
      {
         stacks.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): IsStartElement(Stacks) returned false");
            return false;
         }
         if (reader.Name != "Stacks")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): Stacks != (node=" + reader.Name + ")");
            return false;
         }
         string? sName = reader.GetAttribute("value");
         if (null == sName)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): sName=null");
            return false;
         }
         if( attribute != sName )
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): sName=" + sName + " not equal to attribute=" + attribute);
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): count=null");
            return false;
         }
         int count = int.Parse(sCount);
         //---------------------------------------------
         for( int i=0; i<count; ++i )
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): IsStartElement(Stack) returned false");
               return false;
            }
            if (reader.Name != "Stack")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): Stack != (node=" + reader.Name + ")");
               return false;
            }
            string? tName = reader.GetAttribute("value");
            if (null == tName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): tName=null");
               return false;
            }
            ITerritory? t = Territories.theTerritories.Find(tName);
            if( null == t )
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): t=null for tName=" + tName );
               return false;
            }
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): IsStartElement(IsStacked) returned false");
               return false;
            }
            if (reader.Name != "IsStacked")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): IsStacked != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsStacked = reader.GetAttribute("value");
            if (null == sIsStacked)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): sIsStacked=null");
               return false;
            }
            bool isStacked = Convert.ToBoolean(sIsStacked);
            //---------------------------------------------
            IMapItems mapItems = new MapItems();
            if (false == ReadXmlMapItems(reader, mapItems, tName))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): ReadXml_MapItems() returned false");
               return false;
            }
            //--------------------------------------------
            IStack stack = new Stack(t);
            stack.IsStacked = isStacked;
            stack.MapItems = mapItems;
            stacks.Add(stack);
            reader.Read(); // get past </Stack>
         }
         if (0 < count)
            reader.Read(); // get past </Stacks>
         return true;
      }
      private bool ReadXmlEnteredHexes(XmlReader reader, List<EnteredHex> hexes)
      {
         hexes.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(EnteredHexes) returned false");
            return false;
         }
         if (reader.Name != "EnteredHexes")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EnteredHexes != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(EnteredHex) returned false");
               return false;
            }
            if (reader.Name != "EnteredHex")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EnteredHexes != (node=" + reader.Name + ")");
               return false;
            }
            string? sId = reader.GetAttribute("value");
            if (null == sId)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sId=null");
               return false;
            }
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(Day) returned false");
               return false;
            }
            if (reader.Name != "Day")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Day != (node=" + reader.Name + ")");
               return false;
            }
            string? sDay = reader.GetAttribute("value");
            if (null == sDay)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sDay=null");
               return false;
            }
            int day = Convert.ToInt32(sDay);
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(Date) returned false");
               return false;
            }
            if (reader.Name != "Date")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Date != (node=" + reader.Name + ")");
               return false;
            }
            string? date = reader.GetAttribute("value");
            if (null == date)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Date=null");
               return false;
            }
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(Time) returned false");
               return false;
            }
            if (reader.Name != "Time")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Time != (node=" + reader.Name + ")");
               return false;
            }
            string? time = reader.GetAttribute("value");
            if (null == time)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): time=null");
               return false;
            }
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(TerritoryName) returned false");
               return false;
            }
            if (reader.Name != "TerritoryName")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): TerritoryName != (node=" + reader.Name + ")");
               return false;
            }
            string? territoryName = reader.GetAttribute("value");
            if (null == territoryName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): territoryName=null");
               return false;
            }
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(MapPoint) returned false");
               return false;
            }
            if (reader.Name != "MapPoint")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): MapPoint != (node=" + reader.Name + ")");
               return false;
            }
            string? sX = reader.GetAttribute("X");
            if (null == sX)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sX=null");
               return false;
            }
            double x = Convert.ToDouble(sX);
            string? sY = reader.GetAttribute("Y");
            if (null == sY)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sY=null");
               return false;
            }
            double y = Convert.ToDouble(sY);
            IMapPoint mp = new MapPoint(x, y);
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(ColorAction) returned false");
               return false;
            }
            if (reader.Name != "ColorAction")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): ColorAction != (node=" + reader.Name + ")");
               return false;
            }
            string? sColorAction = reader.GetAttribute("value");
            if (null == sColorAction)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sColorAction=null");
               return false;
            }
            ColorActionEnum colorAction = ColorActionEnum.CAE_START;
            switch (sColorAction)
            {
               case "CAE_START": colorAction = ColorActionEnum.CAE_START; break;
               case "CAE_ENTER": colorAction = ColorActionEnum.CAE_ENTER; break;
               case "CAE_RETREAT": colorAction = ColorActionEnum.CAE_RETREAT; break;
               case "CAE_STOP": colorAction = ColorActionEnum.CAE_STOP; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): reached default sColorAction=" + sColorAction); return false;
            }
            reader.Read(); // get past </EnteredHex>
            EnteredHex hex = new EnteredHex(mp);
            hex.Identifer = sId;
            hex.Day = day;
            hex.Date = date;
            hex.Time = time;
            hex.TerritoryName = territoryName;
            hex.ColorAction = colorAction;
            hexes.Add(hex);
         }
         if( 0 < count )
            reader.Read(); // get past </EnteredHexes>
         return true;
      }
      //--------------------------------------------------
      private XmlDocument? CreateXmlGameInstance(IGameInstance gi)
      {
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<GameInstance></GameInstance>");
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): aXmlDocument.DocumentElement=null");
            return null;
         }
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): root is null");
            return null;
         }
         //------------------------------------------
         XmlElement? versionElem = aXmlDocument.CreateElement("Version");
         if (null == versionElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): aXmlDocument.DocumentElement.LastChild=null");
            return null;
         }
         int majorVersion = GetMajorVersion();
         if (majorVersion < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance():  0 > majorVersion=" + majorVersion.ToString());
            return null;
         }
         versionElem.SetAttribute("value", majorVersion.ToString());
         XmlNode? versionNode = root.AppendChild(versionElem);
         if (null == versionNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(versionNode) returned null");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlListingOfMapItems(aXmlDocument, gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): Create_XmlListingOfMapItems() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlGameCommands(aXmlDocument, gi.GameCommands))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlGameCommands() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlGameOptions(aXmlDocument, gi.Options))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlOptions() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlGameStatistics(aXmlDocument, gi.Statistics))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlGameStat() returned false");
            return null;
         }
         //------------------------------------------
         XmlElement? elem = aXmlDocument.CreateElement("MaxDayBetweenCombat");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(MaxDayBetweenCombat) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.MaxDayBetweenCombat.ToString());
         XmlNode? node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(MaxDayBetweenCombat) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("MaxRollsForAirSupport");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(MaxRollsForAirSupport) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.MaxRollsForAirSupport.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(MaxRollsForAirSupport) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("MaxRollsForArtillerySupport");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(MaxRollsForArtillerySupport) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.MaxRollsForArtillerySupport.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(MaxRollsForArtillerySupport) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("MaxEnemiesInOneBattle");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(MaxEnemiesInOneBattle) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.MaxEnemiesInOneBattle.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(MaxEnemiesInOneBattle) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("RoundsOfCombat");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(RoundsOfCombat) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.RoundsOfCombat.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(RoundsOfCombat) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsFirstSpottingOccurred");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsFirstSpottingOccurred) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsFirstSpottingOccurred.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsFirstSpottingOccurred) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("Is1stEnemyStrengthCheckTerritory");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(Is1stEnemyStrengthCheckTerritory) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.Is1stEnemyStrengthCheckTerritory.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(Is1stEnemyStrengthCheckTerritory) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsGridActive");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsGridActive) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsGridActive.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsGridActive) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EventActive");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(EventActive) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EventActive);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(EventActive) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EventDisplayed");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(EventDisplayed) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EventDisplayed);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(EventDisplayed) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("Day");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(Day) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.Day.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(Day) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("GameTurn");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(GameTurn) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.GameTurn.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(GameTurn) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("GamePhase");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(GamePhase) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.GamePhase.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(GamePhase) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EndGameReason");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(EndGameReason) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EndGameReason.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(EndGameReason) returned null");
            return null;
         }
         //------------------------------------------
         if( false == CreateXmlGameReports(aXmlDocument, gi.Reports))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlGameReports() returned false");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("BattlePhase");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(BattlePhase) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.BattlePhase.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(BattlePhase) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("CrewActionPhase");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(CrewActionPhase) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.CrewActionPhase.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(CrewActionPhase) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("MovementEffectOnSherman");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(MovementEffectOnSherman) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.MovementEffectOnSherman);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(MovementEffectOnSherman) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("MovementEffectOnEnemy");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(MovementEffectOnEnemy) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.MovementEffectOnEnemy);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(MovementEffectOnEnemy) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("FiredAmmoType");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(FiredAmmoType) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.FiredAmmoType);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(FiredAmmoType) returned null");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, root, gi.ReadyRacks, "ReadyRacks"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItems(ReadyRacks) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, root, gi.Hatches, "Hatches"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItems(Hatches) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, root, gi.CrewActions, "CrewActions"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItems(CrewActions) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, root, gi.GunLoads, "GunLoads"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItems(GunLoads) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, root, gi.Targets, "Targets"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItems(Targets) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, root, gi.AdvancingEnemies, "AdvancingEnemies"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItems(AdvancingEnemies) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, root, gi.ShermanAdvanceOrRetreatEnemies, "ShermanAdvanceOrRetreatEnemies"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItems(ShermanAdvanceOrRetreatEnemies) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlCrewMembers(aXmlDocument, root, gi.NewMembers, "NewMembers"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlCrewMembers(NewMembers) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlCrewMembers(aXmlDocument, root, gi.InjuredCrewMembers, "InjuredCrewMembers"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlCrewMembers(InjuredCrewMembers) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItem(aXmlDocument, root, gi.Sherman, "Sherman"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItem(Sherman) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItem(aXmlDocument, root, gi.TargetMainGun, "TargetMainGun"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItem(TargetMainGun) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItem(aXmlDocument, root, gi.TargetMg, "TargetMg"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItem(TargetMg) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItem(aXmlDocument, root, gi.ShermanHvss, "ShermanHvss"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItem(ShermanHvss) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlCrewMember(aXmlDocument, root, gi.ReturningCrewman))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItem(ReturningCrewman) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, gi.AreaTargets, "AreaTargets"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): Create_XmlTerritories(AreaTargets) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, gi.CounterattachRetreats, "CounterattachRetreats"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): Create_XmlTerritories(CounterattachRetreats) returned false");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("Home");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(Home) returned null");
            return null;
         }
         elem.SetAttribute("value", "Home");
         elem.SetAttribute("type", "Battle");
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(Home) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EnemyStrengthCheckTerritory");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(EnemyStrengthCheckTerritory) returned null");
            return null;
         }
         if( null == gi.EnemyStrengthCheckTerritory)
         {
            elem.SetAttribute("value", "null");
            elem.SetAttribute("type", "null");
         }
         else
         {
            elem.SetAttribute("value", gi.EnemyStrengthCheckTerritory.Name);
            elem.SetAttribute("type", gi.EnemyStrengthCheckTerritory.Type);
         }
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(EnemyStrengthCheckTerritory) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("ArtillerySupportCheck");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(ArtillerySupportCheck) returned null");
            return null;
         }
         if (null == gi.ArtillerySupportCheck)
         {
            elem.SetAttribute("value", "null");
            elem.SetAttribute("type", "null");
         }
         else
         {
            elem.SetAttribute("value", gi.ArtillerySupportCheck.Name);
            elem.SetAttribute("type", gi.ArtillerySupportCheck.Type);
         }
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(ArtillerySupportCheck) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("AirStrikeCheckTerritory");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(AirStrikeCheckTerritory) returned null");
            return null;
         }
         if (null == gi.AirStrikeCheckTerritory)
         {
            elem.SetAttribute("value", "null");
            elem.SetAttribute("type", "null");
         }
         else
         {
            elem.SetAttribute("value", gi.AirStrikeCheckTerritory.Name);
            elem.SetAttribute("type", gi.AirStrikeCheckTerritory.Type);
         }
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(AirStrikeCheckTerritory) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EnteredArea");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(EnteredArea) returned null");
            return null;
         }
         if (null == gi.EnteredArea)
         {
            elem.SetAttribute("value", "null");
            elem.SetAttribute("type", "null");
         }
         else
         {
            elem.SetAttribute("value", gi.EnteredArea.Name);
            elem.SetAttribute("type", gi.EnteredArea.Type);
         }
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(EnteredArea) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("AdvanceFire");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(AdvanceFire) returned null");
            return null;
         }
         if (null == gi.AdvanceFire)
         {
            elem.SetAttribute("value", "null");
            elem.SetAttribute("type", "null");
         }
         else
         {
            elem.SetAttribute("value", gi.AdvanceFire.Name);
            elem.SetAttribute("type", gi.AdvanceFire.Type);
         }
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(AdvanceFire) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("FriendlyAdvance");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(FriendlyAdvance) returned null");
            return null;
         }
         if (null == gi.FriendlyAdvance)
         {
            elem.SetAttribute("value", "null");
            elem.SetAttribute("type", "null");
         }
         else
         {
            elem.SetAttribute("value", gi.FriendlyAdvance.Name);
            elem.SetAttribute("type", gi.FriendlyAdvance.Type);
         }
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(FriendlyAdvance) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EnemyAdvance");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(EnemyAdvance) returned null");
            return null;
         }
         if (null == gi.EnemyAdvance)
         {
            elem.SetAttribute("value", "null");
            elem.SetAttribute("type", "null");
         }
         else
         {
            elem.SetAttribute("value", gi.EnemyAdvance.Name);
            elem.SetAttribute("type", gi.EnemyAdvance.Type);
         }
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(EnemyAdvance) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsHatchesActive");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsHatchesActive) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsHatchesActive.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsHatchesActive) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsRetreatToStartArea");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsRetreatToStartArea) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsRetreatToStartArea.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsRetreatToStartArea) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanAdvancingOnBattleBoard");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsShermanAdvancingOnBattleBoard) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanAdvancingOnBattleBoard.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsShermanAdvancingOnBattleBoard) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanAdvancingOnMoveBoard");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsShermanAdvancingOnMoveBoard) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanAdvancingOnMoveBoard.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsShermanAdvancingOnMoveBoard) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsLoaderSpotThisTurn");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsLoaderSpotThisTurn) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsLoaderSpotThisTurn.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsLoaderSpotThisTurn) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsCommanderSpotThisTurn");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsCommanderSpotThisTurn) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsCommanderSpotThisTurn.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsCommanderSpotThisTurn) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsFallingSnowStopped");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsFallingSnowStopped) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsFallingSnowStopped.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsFallingSnowStopped) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("HoursOfRainThisDay");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(HoursOfRainThisDay) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.HoursOfRainThisDay.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(HoursOfRainThisDay) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("MinSinceLastCheck");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(MinSinceLastCheck) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.MinSinceLastCheck.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(MinSinceLastCheck) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("SwitchedCrewMemberRole");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(SwitchedCrewMemberRole) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.SwitchedCrewMemberRole);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(SwitchedCrewMemberRole) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("AssistantOriginalRating");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(AssistantOriginalRating) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.AssistantOriginalRating.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(AssistantOriginalRating) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("ShermanTurretRotationOld");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(ShermanTurretRotationOld) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.ShermanTurretRotationOld.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(ShermanTurretRotationOld) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanTurretRotatedThisRound");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsShermanTurretRotatedThisRound) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanTurretRotatedThisRound.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsShermanTurretRotatedThisRound) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("ShermanConsectiveMoveAttempt");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(ShermanConsectiveMoveAttempt) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.ShermanConsectiveMoveAttempt.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(ShermanConsectiveMoveAttempt) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiringAtFront");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsShermanFiringAtFront) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiringAtFront.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsShermanFiringAtFront) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanDeliberateImmobilization");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsShermanDeliberateImmobilization) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanDeliberateImmobilization.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsShermanDeliberateImmobilization) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("ShermanTypeOfFire");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(ShermanTypeOfFire) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.ShermanTypeOfFire);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(ShermanTypeOfFire) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("NumSmokeAttacksThisRound");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(NumSmokeAttacksThisRound) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.NumSmokeAttacksThisRound.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(NumSmokeAttacksThisRound) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMalfunctionedMainGun");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(Is_MalfunctionedMainGun) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMalfunctionedMainGun.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(Is_MalfunctionedMainGun) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMainGunRepairAttempted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsMainGunRepairAttempted) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMainGunRepairAttempted.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsMainGunRepairAttempted) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenMainGun");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsBrokenMainGun) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenMainGun.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsBrokenMainGun) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenGunSight");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsBrokenGunSight) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenGunSight.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsBrokenGunSight) returned null");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlFirstShots(aXmlDocument, gi.FirstShots))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlFirstShots(FirstShots) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTrainedGunners(aXmlDocument, gi.TrainedGunners))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlFirstShots(TrainedGunners) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlShermanHits(aXmlDocument, gi.ShermanHits))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlSherman_Hits(Sherman_Hits) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlShermanDeath(aXmlDocument, gi.Death))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlShermanDeath(Death) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlShermanSetup(aXmlDocument, gi.BattlePrep))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlShermanSetup(BattlePrep) returned false");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IdentifiedTank");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IdentifiedTank) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IdentifiedTank);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IdentifiedTank) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IdentifiedAtg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IdentifiedAtg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IdentifiedAtg);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IdentifiedAtg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IdentifiedSpg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IdentifiedSpg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IdentifiedSpg);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IdentifiedSpg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiringAaMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(Is_ShermanFiringAaMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiringAaMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(Is_ShermanFiringAaMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiringBowMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsShermanFiringBowMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiringBowMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsShermanFiringBowMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiringCoaxialMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsSherman_FiringCoaxialMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiringCoaxialMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsSherman_FiringCoaxialMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiringSubMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsSherman_FiringSubMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiringSubMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsShermanFiringSubMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsCommanderDirectingMgFire");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsCommanderDirectingMgFire) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsCommanderDirectingMgFire.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsCommanderDirectingMgFire) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiredAaMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsShermanFiredAaMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiredAaMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsShermanFiredAaMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiredBowMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsShermanFiredBowMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiredBowMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsShermanFiredBowMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiredCoaxialMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsShermanFiredCoaxialMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiredCoaxialMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsShermanFiredCoaxialMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsShermanFiredSubMg");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsShermanFiredSubMg) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsShermanFiredSubMg.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsShermanFiredSubMg) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMalfunctionedMgCoaxial");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsMalfunctionedMgCoaxial) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMalfunctionedMgCoaxial.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsMalfunctionedMgCoaxial) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMalfunctionedMgBow");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsMalfunctionedMgBow) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMalfunctionedMgBow.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsMalfunctionedMgBow) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMalfunctionedMgAntiAircraft");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsMalfunctionedMgAntiAircraft) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMalfunctionedMgAntiAircraft.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsMalfunctionedMgAntiAircraft) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsCoaxialMgRepairAttempted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsCoaxialMgRepairAttempted) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsCoaxialMgRepairAttempted.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsCoaxialMgRepairAttempted) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBowMgRepairAttempted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsBowMgRepairAttempted) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBowMgRepairAttempted.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsBowMgRepairAttempted) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsAaMgRepairAttempted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsAaMgRepairAttempted) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsAaMgRepairAttempted.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsAaMgRepairAttempted) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenMgAntiAircraft");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsBrokenMgAntiAircraft) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenMgAntiAircraft.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsBrokenMgAntiAircraft) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenMgBow");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsBrokenMgBow) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenMgBow.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsBrokenMgBow) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenMgCoaxial");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsBrokenMgCoaxial) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenMgCoaxial.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsBrokenMgCoaxial) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenPeriscopeDriver");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsBrokenPeriscopeDriver) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenPeriscopeDriver.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsBrokenPeriscopeDriver) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenPeriscopeLoader");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsBrokenPeriscopeLoader) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenPeriscopeLoader.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsBrokenPeriscopeLoader) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenPeriscopeAssistant");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsBrokenPeriscopeAssistant) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenPeriscopeAssistant.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsBrokenPeriscopeAssistant) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenPeriscopeGunner");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsBrokenPeriscopeGunner) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenPeriscopeGunner.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsBrokenPeriscopeGunner) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrokenPeriscopeCommander");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsBrokenPeriscopeCommander) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBrokenPeriscopeCommander.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsBrokenPeriscopeCommander) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsCounterattackAmbush");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsCounterattackAmbush) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsCounterattackAmbush.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsCounterattackAmbush) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsLeadTank");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsLeadTank) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsLeadTank.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsLeadTank) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsAirStrikePending");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsAirStrikePending) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsAirStrikePending.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsAirStrikePending) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsAdvancingFireChosen");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsAdvancingFireChosen) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsAdvancingFireChosen.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsAdvancingFireChosen) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("AdvancingFireMarkerCount");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(AdvancingFireMarkerCount) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.AdvancingFireMarkerCount.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(AdvancingFireMarkerCount) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("BattleResistance");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(BattleResistance) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.BattleResistance.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(BattleResistance) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMinefieldAttack");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsMinefieldAttack) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMinefieldAttack.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsMinefieldAttack) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsHarrassingFireBonus");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsHarrassingFireBonus) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsHarrassingFireBonus.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsHarrassingFireBonus) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsFlankingFire");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsFlankingFire) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsFlankingFire.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsFlankingFire) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsEnemyAdvanceComplete");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsEnemyAdvanceComplete) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsEnemyAdvanceComplete.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsEnemyAdvanceComplete) returned null");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlPanzerfaustAttack(aXmlDocument, gi.Panzerfaust))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlPanzerfaustAttack(Panzerfaust) returned false");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("NumCollateralDamage");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(NumCollateralDamage) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.NumCollateralDamage.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(NumCollateralDamage) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("TankReplacementNumber");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(TankReplacementNumber) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.TankReplacementNumber.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(TankReplacementNumber) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("Fuel");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(Fuel) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.Fuel.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(Fuel) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("VictoryPtsTotalCampaign");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(VictoryPtsTotalCampaign) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.VictoryPtsTotalCampaign.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(VictoryPtsTotalCampaign) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("PromotionPointNum");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(PromotionPointNum) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.PromotionPointNum.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(PromotionPointNum) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("PromotionDay");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(PromotionDay) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.PromotionDay.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(PromotionDay) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("NumPurpleHeart");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(NumPurpleHeart) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.NumPurpleHeart.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(NumPurpleHeart) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsCommanderRescuePerformed");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsCommanderRescuePerformed) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsCommanderRescuePerformed.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsCommanderRescuePerformed) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsCommanderKilled");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsCommanderKilled) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsCommanderKilled.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsCommanderKilled) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsPromoted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateElement(IsPromoted) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsPromoted.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(IsPromoted) returned null");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItemMoves(aXmlDocument, gi.MapItemMoves))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItemMoves(MapItemMoves) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlStacks(aXmlDocument, gi.MoveStacks, "MoveStacks"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlStacks(MoveStacks) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlStacks(aXmlDocument, gi.BattleStacks, "BattleStacks"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlStacks(BattleStacks) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlEnteredHexes(aXmlDocument, gi.EnteredHexes))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlEnteredHexes() returned false");
            return null;
         }
         return aXmlDocument;
      }
      private bool CreateXmlListingOfMapItems(XmlDocument aXmlDocument, IGameInstance gi)
      {
         for (int k = 0; k < gi.Reports.Count; k++)
         {
            IAfterActionReport? report = gi.Reports[k];
            if (null == report)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): report=null");
               return false;
            }
            if (null == theMapItems.Find(report.Commander.Name))
               theMapItems.Add((IMapItem)report.Commander);
            if (null == theMapItems.Find(report.Gunner.Name))
               theMapItems.Add((IMapItem)report.Gunner);
            if (null == theMapItems.Find(report.Loader.Name))
               theMapItems.Add((IMapItem)report.Loader);
            if (null == theMapItems.Find(report.Driver.Name))
               theMapItems.Add((IMapItem)report.Driver);
            if (null == theMapItems.Find(report.Assistant.Name))
               theMapItems.Add((IMapItem)report.Assistant);
         }
         //-----------------------------------
         foreach (IMapItem mi in gi.ReadyRacks)
            theMapItems.Add(mi);
         foreach (IMapItem mi in gi.Hatches)
            theMapItems.Add(mi);
         foreach (IMapItem mi in gi.CrewActions)
         {
            if (null == theMapItems.Find(mi.Name))
               theMapItems.Add(mi);
         }
         foreach (IMapItem mi in gi.GunLoads)
            theMapItems.Add(mi);
         foreach (IMapItem mi in gi.Targets)
         {
            if (null == theMapItems.Find(mi.Name))
               theMapItems.Add(mi);
         }
         foreach (IMapItem mi in gi.AdvancingEnemies)
         {
            if (null == theMapItems.Find(mi.Name))
               theMapItems.Add(mi);
         }
         foreach (IMapItem mi in gi.ShermanAdvanceOrRetreatEnemies)
         {
            if (null == theMapItems.Find(mi.Name))
               theMapItems.Add(mi);
         }
         //-----------------------------------
         foreach (IMapItem mi in gi.NewMembers) // only saving off the IMapItem portion of ICrewMember
         {
            if (null == theMapItems.Find(mi.Name))
               theMapItems.Add(mi);
         }
         foreach (IMapItem mi in gi.InjuredCrewMembers) // only saving off the IMapItem portion of ICrewMember
         {
            if (null == theMapItems.Find(mi.Name))
               theMapItems.Add(mi);
         }
         foreach (IMapItem mi in gi.BattlePrep.myHatches) 
            theMapItems.Add(mi);
         //-----------------------------------
         if (null != gi.TargetMainGun)
            theMapItems.Add(gi.TargetMainGun);
         if (null != gi.TargetMg)
            theMapItems.Add(gi.TargetMg);
         if (null != gi.ShermanHvss)
            theMapItems.Add(gi.ShermanHvss);
         if (null != gi.ReturningCrewman)
            theMapItems.Add(gi.ReturningCrewman);
         //-----------------------------------
         foreach (IMapItemMove mim in gi.MapItemMoves)
         {
            if (null == theMapItems.Find(mim.MapItem.Name))
               theMapItems.Add(mim.MapItem);
         }
         foreach (IStack stack in gi.MoveStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (null == theMapItems.Find(mi.Name))
                  theMapItems.Add(mi);
            }
         }
         foreach (IStack stack in gi.BattleStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (null == theMapItems.Find(mi.Name))
                  theMapItems.Add(mi);
            }
         }
         theMapItems.Add(gi.Sherman);
         if (null != gi.Death)
         {
            if (null == theMapItems.Find(gi.Death.myEnemyUnit.Name))
               theMapItems.Add(gi.Death.myEnemyUnit);
         }
         if (null != gi.Panzerfaust)
         {
            if (null == theMapItems.Find(gi.Panzerfaust.myEnemyUnit.Name))
               theMapItems.Add(gi.Panzerfaust.myEnemyUnit);
         }
         //======================================================
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): root is null");
            return false;
         }
         XmlElement? mapItemsElem = aXmlDocument.CreateElement("MapItems");
         if (null == mapItemsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(MapItemsElem) returned null");
            return false;
         }
         mapItemsElem.SetAttribute("count", theMapItems.Count.ToString());
         XmlNode? mapItemsNode = root.AppendChild(mapItemsElem);
         if (null == mapItemsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(MapItemsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (IMapItem mi in theMapItems)
         {
            XmlElement? miElem = aXmlDocument.CreateElement("MapItem");
            if (null == miElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(miElem) returned null");
               return false;
            }
            XmlNode? miNode = mapItemsNode.AppendChild(miElem);
            if (null == miNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(miNode) returned null");
               return false;
            }
            if (null == mi)
            {
               miElem.SetAttribute("value", "null"); // MapItem
               return true;
            }
            else
            {
               miElem.SetAttribute("value", mi.Name);
            }
            //--------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("TopImageName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(TopImageName) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.TopImageName);
            XmlNode? node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(TopImageName) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("BottomImageName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(BottomImageName) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.BottomImageName);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(BottomImageName) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("OverlayImageName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(OverlayImageName) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.OverlayImageName);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): AppendChild(OverlayImageName) returned null");
               return false;
            }
            //--------------------------------
            if (false == CreateXmlListingOfMapItemsWoundSpots(aXmlDocument, miNode, mi.WoundSpots))
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): Create_XmlListingOfMapItemsWoundSpots() returned false");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Zoom");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(Zoom) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Zoom.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(Zoom) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsMoved");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsMoved) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsMoved.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsMoved) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Count");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(Count) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Count.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(Count) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("RotationOffsetHull");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(RotationOffsetHull) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.RotationOffsetHull.ToString("F3"));
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(RotationOffsetHull) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("RotationHull");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(RotationHull) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.RotationHull.ToString("F3"));
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(RotationHull) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("RotationOffsetTurret");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(RotationOffsetTurret) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.RotationOffsetTurret.ToString("F3"));
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(RotationOffsetTurret) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("RotationTurret");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(RotationTurret) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.RotationTurret.ToString("F3"));
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(RotationTurret) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("LocationX");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(LocationX) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Location.X.ToString("F3"));
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(LocationX) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("LocationY");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(LocationY) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Location.Y.ToString("F3"));
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(LocationY) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("TerritoryCurrent");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(TerritoryCurrent) returned null");
               return false;
            }
            if( "OffBoard" == mi.TerritoryCurrent.Name )
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): Invalid State mi.Name=" + mi.Name + " is Offbaord!");
               return false;
            }
            elem.SetAttribute("value", mi.TerritoryCurrent.Name);
            elem.SetAttribute("type", mi.TerritoryCurrent.Type);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(TerritoryCurrent) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("TerritoryStarting");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(TerritoryStarting) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.TerritoryStarting.Name);
            elem.SetAttribute("type", mi.TerritoryStarting.Type);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(TerritoryStarting) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("LastMoveAction");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(LastMoveAction) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.LastMoveAction);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(LastMoveAction) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsMoving");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsMoving) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsMoving.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsMoving) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsHullDown");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsHullDown) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsMoving.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsHullDown) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsKilled");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsKilled) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsKilled.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsKilled) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsUnconscious");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsUnconscious) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsUnconscious.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsUnconscious) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsIncapacitated");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsIncapacitated) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsIncapacitated.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsIncapacitated) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsFired");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsFired) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsFired.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsFired) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsSpotted");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsSpotted) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsSpotted.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsSpotted) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsInterdicted");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsInterdicted) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsInterdicted.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsInterdicted) returned null");
               return false;
            }
            //--------------------------------
            if (false == CreateXmlListingOfMapItemsAcquiredShots(aXmlDocument, miNode, mi.EnemyAcquiredShots))
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): Create_XmlListingOfMapItemsWoundSpots() returned false");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsMovingInOpen");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsMovingInOpen) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsMovingInOpen.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsMovingInOpen) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsWoods");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsWoods) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsWoods.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsWoods) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsBuilding");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsBuilding) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsBuilding.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsBuilding) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsFortification");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsFortification) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsFortification.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsFortification) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsThrownTrack");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(Is_ThrownTrack) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsThrownTrack.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(Is_ThrownTrack) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsBoggedDown");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(Is_BoggedDown) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsBoggedDown.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(Is_BoggedDown) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsAssistanceNeeded");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsAssistanceNeeded) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsAssistanceNeeded.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsAssistanceNeeded) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsHeHit");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsHeHit) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsHeHit.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsHeHit) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsApHit");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsApHit) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsApHit.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsApHit) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Spotting");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(Spotting) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Spotting.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(Spotting) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlListingOfMapItemsWoundSpots(XmlDocument aXmlDocument, XmlNode topNode, List<BloodSpot> woundSpots)
      {
         XmlElement? woundSpotsElem = aXmlDocument.CreateElement("WoundSpots");
         if (null == woundSpotsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsWoundSpots(): CreateElement(woundsElement) returned null");
            return false;
         }
         woundSpotsElem.SetAttribute("count", woundSpots.Count.ToString());
         XmlNode? woundSpotsNode = topNode.AppendChild(woundSpotsElem);
         if (null == woundSpotsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsWoundSpots(): AppendChild(woundSpotsNode) returned null");
            return false;
         }
         for (int k = 0; k < woundSpots.Count; ++k)
         {
            BloodSpot bloodSpot = woundSpots[k];
            XmlElement? spotElem = aXmlDocument.CreateElement("WoundSpot");
            if (null == spotElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsWoundSpots(): CreateElement(spotElem) returned null");
               return false;
            }
            spotElem.SetAttribute("size", bloodSpot.mySize.ToString());
            spotElem.SetAttribute("left", bloodSpot.myLeft.ToString());
            spotElem.SetAttribute("top", bloodSpot.myTop.ToString());
            XmlNode? spotNode = woundSpotsNode.AppendChild(spotElem);
            if (null == spotNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsWoundSpots(): AppendChild(miNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlListingOfMapItemsAcquiredShots(XmlDocument aXmlDocument, XmlNode topNode, Dictionary<string, int> enemyAcquiredShots)
      {
         XmlElement? enemyShotsElem = aXmlDocument.CreateElement("EnemyAcquiredShots");
         if (null == enemyShotsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsAcquiredShots(): CreateElement(enemyShotsElem) returned null");
            return false;
         }
         enemyShotsElem.SetAttribute("count", enemyAcquiredShots.Count.ToString());
         XmlNode? enemyAcquireShotsNode = topNode.AppendChild(enemyShotsElem);
         if (null == enemyAcquireShotsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsAcquiredShots(): AppendChild(enemyAcquireShotsNode) returned null");
            return false;
         }
         int count = 0;
         foreach (var kvp in enemyAcquiredShots)
         {
            XmlElement? enemyAcqShotElem = aXmlDocument.CreateElement("EnemyAcqShot");
            if (null == enemyAcqShotElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsAcquiredShots(): CreateElement(spotElem) returned null");
               return false;
            }
            enemyAcqShotElem.SetAttribute("enemy", kvp.Key);
            enemyAcqShotElem.SetAttribute("value", kvp.Value.ToString());
            XmlNode? enemyAcqShotNode = enemyAcquireShotsNode.AppendChild(enemyAcqShotElem);
            if (null == enemyAcqShotNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsAcquiredShots(): AppendChild(miNode) returned null");
               return false;
            }
            count++;
         }
         if (count != enemyAcquiredShots.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsAcquiredShots(): count=" + count.ToString() + " enemyAcquiredShots=" + enemyAcquiredShots.Count.ToString());
            return false;
         }
         return true;
      }
      private bool CreateXmlGameCommands(XmlDocument aXmlDocument, IGameCommands gameCommands)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): root is null");
            return false;
         }
         XmlElement? gamecmdsElem = aXmlDocument.CreateElement("GameCommands");
         if (null == gamecmdsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): CreateElement(GameCommands) returned null");
            return false;
         }
         gamecmdsElem.SetAttribute("count", gameCommands.Count.ToString());
         XmlNode? gameCmdsNode = root.AppendChild(gamecmdsElem);
         if (null == gameCmdsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): AppendChild(gameCmdsNode) returned null");
            return false;
         }
         //--------------------------------
         for (int i= 0; i < gameCommands.Count; ++i)
         {
            IGameCommand? gameCmd = gameCommands[i];
            if( null == gameCmd )
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): gameCmd=null");
               return false;
            }
            XmlElement? gameCmdElem = aXmlDocument.CreateElement("GameCommand");
            if (null == gameCmdElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): CreateElement(OptGameCommandion) returned null");
               return false;
            }
            //---------------------------------------
            gameCmdElem.SetAttribute("Action", gameCmd.Action.ToString());
            gameCmdElem.SetAttribute("ActionDieRoll", gameCmd.ActionDieRoll.ToString());
            gameCmdElem.SetAttribute("EventActive", gameCmd.EventActive.ToString());
            gameCmdElem.SetAttribute("Phase", gameCmd.Phase.ToString());
            gameCmdElem.SetAttribute("MainImage", gameCmd.MainImage.ToString());
            XmlNode? gameCmdNode = gameCmdsNode.AppendChild(gameCmdElem);
            if (null == gameCmdNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): AppendChild(gameCmdNode) returned null");
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
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameOptions(): root is null");
            return false;
         }
         XmlElement? optionsElem = aXmlDocument.CreateElement("Options");
         if (null == optionsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameOptions(): CreateElement(Options) returned null");
            return false;
         }
         optionsElem.SetAttribute("count", options.Count.ToString());
         XmlNode? optionsNode = root.AppendChild(optionsElem);
         if (null == optionsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameOptions(): AppendChild(optionsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (Option option in options)
         {
            XmlElement? optionElem = aXmlDocument.CreateElement("Option");
            if (null == optionElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameOptions(): CreateElement(Option) returned null");
               return false;
            }
            optionElem.SetAttribute("Name", option.Name);
            optionElem.SetAttribute("IsEnabled", option.IsEnabled.ToString());
            XmlNode? optionNode = optionsNode.AppendChild(optionElem);
            if (null == optionNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameOptions(): AppendChild(optionNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlGameStatistics(XmlDocument aXmlDocument, GameStatistics statistics)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlGameStatistics(): root is null");
            return false;
         }
         XmlElement? gameStatElem = aXmlDocument.CreateElement("GameStatistics");
         if (null == gameStatElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlGameStatistics(): CreateElement(gameStatElem) returned null");
            return false;
         }
         gameStatElem.SetAttribute("count", statistics.Count.ToString());
         XmlNode? statsNode = root.AppendChild(gameStatElem);
         if (null == statsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlGameStatistics(): AppendChild(statsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (GameStatistic stat in statistics)
         {
            XmlElement? statElem = aXmlDocument.CreateElement("GameStatistic");
            if (null == statElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlGameStatistics(): CreateElement(GameStatistic) returned null");
               return false;
            }
            statElem.SetAttribute("Key", stat.Key);
            statElem.SetAttribute("Value", stat.Value.ToString());
            XmlNode? statNode = statsNode.AppendChild(statElem);
            if (null == statNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlGameStatistics(): AppendChild(statNode) returned null");
               return false;
            }
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
            reportElem.SetAttribute("IsActionThisDay", report.IsActionThisDay.ToString());
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
            XmlNode? node = reportNode.AppendChild(elem);
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
            node = reportNode.AppendChild(elem);
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
            node = reportNode.AppendChild(elem);
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
            node = reportNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Resistance) returned false");
               return false;
            }
            //------------------------------------------
            if( true == report.IsActionThisDay )
            {
               elem = aXmlDocument.CreateElement("Name");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Name) returned false");
                  return false;
               }
               elem.SetAttribute("value", report.Name);
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPts_TotalTerritory) returned false");
                  return false;
               }
               //------------------------------------------
               elem = aXmlDocument.CreateElement("VictoryPtsTotalEngagement");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(VictoryPts_TotalEngagement) returned false");
                  return false;
               }
               elem.SetAttribute("value", report.VictoryPtsTotalEngagement.ToString());
               node = reportNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(VictoryPts_TotalEngagement) returned false");
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
               XmlNode? decorationsNode = reportNode.AppendChild(elem);
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
               XmlNode? notesNode = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
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
               node = reportNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(KnockedOut) returned false");
                  return false;
               }
            }
         }
         return true;
      }
      private bool CreateXmlCrewMembers(XmlDocument aXmlDocument, XmlNode parent, ICrewMembers crewMembers, string attribute)
      {
         XmlElement? crewMembersElem = aXmlDocument.CreateElement("CrewMembers");
         if (null == crewMembersElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlCrewMembers(): CreateElement(crewMembersElem) returned null");
            return false;
         }
         crewMembersElem.SetAttribute("value", attribute);
         crewMembersElem.SetAttribute("count", crewMembers.Count.ToString());
         XmlNode? crewMembersNode = parent.AppendChild(crewMembersElem);
         if (null == crewMembersNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlCrewMembers(): AppendChild(crewMembersNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (ICrewMember cm in crewMembers)
         {
            if (false == CreateXmlCrewMember(aXmlDocument, crewMembersNode, cm))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlCrewMembers(): CreateXmlCrewMember() returned false");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlCrewMember(XmlDocument aXmlDocument, XmlNode parent, ICrewMember? cm)
      {
         XmlElement? cmElem = aXmlDocument.CreateElement("CrewMember");
         if (null == cmElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlCrewMember(): CreateElement(miElem) returned null");
            return false;
         }
         XmlNode? cmNode = parent.AppendChild(cmElem);
         if (null == cmNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlCrewMember(): AppendChild(node) returned null");
            return false;
         }
         if (null == cm)
         {
            cmElem.SetAttribute("value", "null"); // CrewMember
            return true;
         }
         cmElem.SetAttribute("value", cm.Role);
         //--------------------------------
         IMapItem mi = (IMapItem)cm;
         if (false == CreateXmlMapItem(aXmlDocument, cmNode, mi, cm.Role))
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
      private bool CreateXmlMapItems(XmlDocument aXmlDocument, XmlNode parent, IMapItems mapItems, string attribute )
      {
         XmlElement? mapItemsElem = aXmlDocument.CreateElement("MapItems");
         if (null == mapItemsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_MapItems(): CreateElement(MapItemsElem) returned null");
            return false;
         }
         mapItemsElem.SetAttribute("value", attribute);
         mapItemsElem.SetAttribute("count", mapItems.Count.ToString());
         XmlNode? mapItemsNode = parent.AppendChild(mapItemsElem);
         if (null == mapItemsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_MapItems(): AppendChild(MapItemsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (IMapItem mi in mapItems)
         {
            if( false == CreateXmlMapItem(aXmlDocument, mapItemsNode, mi))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_MapItems(): CreateXmlMapItem() returned false");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlMapItem(XmlDocument aXmlDocument, XmlNode parent, IMapItem? mi, string attribute="")
      {
         XmlElement? miElem = aXmlDocument.CreateElement("MapItem");
         if (null == miElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(miElem) returned null");
            return false;
         }
         XmlNode? miNode = parent.AppendChild(miElem);
         if (null == miNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(miNode) returned null");
            return false;
         }
         miElem.SetAttribute("value", attribute);
         if ( null == mi )
            miElem.SetAttribute("name", "null");
         else
            miElem.SetAttribute("name", mi.Name);
         return true;
      }
      private bool CreateXmlTerritories(XmlDocument aXmlDocument, ITerritories territories, string attribute)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlTerritories(): root is null");
            return false;
         }
         XmlElement? territoriesElem = aXmlDocument.CreateElement("Territories");
         if (null == territoriesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlTerritories(): CreateElement(Territories) returned null");
            return false;
         }
         territoriesElem.SetAttribute("value", attribute);
         territoriesElem.SetAttribute("count", territories.Count.ToString());
         XmlNode? territoriesNode = root.AppendChild(territoriesElem);
         if (null == territoriesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlTerritories(): AppendChild(territoriesNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (Territory t in territories)
         {
            XmlElement? terrElem = aXmlDocument.CreateElement("Territory");  // name of territory
            if (null == terrElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlTerritories(): CreateElement(terrElem) returned null");
               return false;
            }
            terrElem.SetAttribute("value", t.Name);
            terrElem.SetAttribute("type", t.Type);
            XmlNode? territoryNode = territoriesNode.AppendChild(terrElem);
            if (null == territoryNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlTerritories(): AppendChild(territoryNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlFirstShots(XmlDocument aXmlDocument, List<string> firstShots)
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
         for( int i=0; i< firstShots.Count; ++i)
         {
            string miName = firstShots[i];
            XmlElement? firstShotElem = aXmlDocument.CreateElement("FirstShot");
            if (null == firstShotElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlFirstShots(): CreateElement(firstShotElem) returned null");
               return false;
            }
            firstShotElem.SetAttribute("value", miName);
            XmlNode? firstShotNode = firstShotsNode.AppendChild(firstShotElem);
            if (null == firstShotNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlFirstShots(): AppendChild(firstShotElem) returned null");
               return false;
            }
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
            XmlElement? trainedGunnerElem = aXmlDocument.CreateElement("TrainedGunner");
            if (null == trainedGunnerElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlTrainedGunners(): CreateElement(TrainedGunner) returned null");
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
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Hits(): root is null");
            return false;
         }
         XmlElement? shermanHitsElem = aXmlDocument.CreateElement("ShermanHits");
         if (null == shermanHitsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Hits(): CreateElement(Sherman_Hits) returned null");
            return false;
         }
         shermanHitsElem.SetAttribute("count", shermanHits.Count.ToString());
         XmlNode? shermanHitsNode = root.AppendChild(shermanHitsElem);
         if (null == shermanHitsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Hits(): AppendChild(Sherman_Hits) returned null");
            return false;
         }
         for (int i = 0; i < shermanHits.Count; ++i)
         {
            ShermanAttack shermanAttack = shermanHits[i];
            XmlElement? shermanHitElem = aXmlDocument.CreateElement("ShermanHit");
            if (null == shermanHitElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Hits(): CreateElement(Sherman_Hit) returned null");
               return false;
            }
            shermanHitElem.SetAttribute("AttackType", shermanAttack.myAttackType);
            shermanHitElem.SetAttribute("AmmoType", shermanAttack.myAmmoType);
            shermanHitElem.SetAttribute("IsCriticalHit", shermanAttack.myIsCriticalHit.ToString());
            shermanHitElem.SetAttribute("HitLocation", shermanAttack.myHitLocation);
            shermanHitElem.SetAttribute("IsNoChance", shermanAttack.myIsNoChance.ToString());
            shermanHitElem.SetAttribute("IsImmobilization", shermanAttack.myIsImmobilization.ToString());
            XmlNode? shermanHitNode = shermanHitsNode.AppendChild(shermanHitElem);
            if (null == shermanHitNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Hits(): AppendChild(shermanHitNode) returned null");
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
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): root is null");
            return false;
         }
         XmlElement? shermanDeathElem = aXmlDocument.CreateElement("ShermanDeath");
         if (null == shermanDeathElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): CreateElement(ShermanDeath) returned null");
            return false;
         }
         XmlNode? shermanDeathNode = root.AppendChild(shermanDeathElem);
         if (null == shermanDeathNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): AppendChild(ShermanDeath) returned null");
            return false;
         }
         if (null == death)
         {
            shermanDeathElem.SetAttribute("value", "null"); // MapItem
            return true;
         }
         //------------------------------------------------
         if( false == CreateXmlMapItem(aXmlDocument, shermanDeathNode, death.myEnemyUnit))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): CreateXmlMapItem() returned null");
            return false;
         }
         //------------------------------------------------
         XmlElement? elem = aXmlDocument.CreateElement("HitLocation");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): CreateElement(HitLocation) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myHitLocation);
         XmlNode? node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): AppendChild(HitLocation) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("EnemyFireDirection");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): CreateElement(EnemyFireDirection) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myEnemyFireDirection);
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): AppendChild(EnemyFireDirection) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("Day");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): CreateElement(Day) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myDay.ToString());
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): AppendChild(Day) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("Cause");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): CreateElement(Cause) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myCause);
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): AppendChild(Cause) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsAmbush");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): CreateElement(IsAmbush) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myIsAmbush.ToString());
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): AppendChild(IsAmbush) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsExplosion");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): CreateElement(IsExplosion) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myIsExplosion.ToString());
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): AppendChild(IsExplosion) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsBailout");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): CreateElement(IsBailout) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myIsCrewBail.ToString());
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): AppendChild(IsBailout) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsBrewUp");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): CreateElement(IsBrewUp) returned null");
            return false;
         }
         elem.SetAttribute("value", death.myIsBrewUp.ToString());
         node = shermanDeathNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlSherman_Death(): AppendChild(IsBrewUp) returned null");
            return false;
         }
         return true;
      }
      private bool CreateXmlShermanSetup(XmlDocument aXmlDocument, ShermanSetup battlePrep)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): root is null");
            return false;
         }
         XmlElement? shermanSetupElem = aXmlDocument.CreateElement("ShermanSetup");
         if (null == shermanSetupElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): CreateElement(ShermanSetup) returned null");
            return false;
         }
         XmlNode? shermanSetupNode = root.AppendChild(shermanSetupElem);
         if (null == shermanSetupNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): AppendChild(ShermanSetup) returned null");
            return false;
         }
         //------------------------------------------------
         XmlElement? elem = aXmlDocument.CreateElement("IsSetupPerformed");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): CreateElement(IsSetupPerformed) returned null");
            return false;
         }
         elem.SetAttribute("value", battlePrep.myIsSetupPerformed.ToString());
         XmlNode? node = shermanSetupNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): AppendChild(IsSetupPerformed) returned null");
            return false;
         }
         //------------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, shermanSetupNode, battlePrep.myHatches, "Hatches"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateXml_MapItems() returned false");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("AmmoType");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): CreateElement(AmmoType) returned null");
            return false;
         }
         elem.SetAttribute("value", battlePrep.myAmmoType);
         node = shermanSetupNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): AppendChild(AmmoType) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("TurretRotation");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): CreateElement(TurretRotation) returned null");
            return false;
         }
         elem.SetAttribute("value", battlePrep.myTurretRotation.ToString());
         node = shermanSetupNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): AppendChild(TurretRotation) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("LoaderSpotTerritory");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): CreateElement(LoaderSpotTerritory) returned null");
            return false;
         }
         elem.SetAttribute("value", battlePrep.myLoaderSpotTerritory);
         node = shermanSetupNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): AppendChild(LoaderSpotTerritory) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("CommanderSpotTerritory");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): CreateElement(CommanderSpotTerritory) returned null");
            return false;
         }
         elem.SetAttribute("value", battlePrep.myCommanderSpotTerritory);
         node = shermanSetupNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_ShermanSetup(): AppendChild(CommanderSpotTerritory) returned null");
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
            pfAttackElem.SetAttribute("value", "null");  // MapItem
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
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): CreateElement(IsShermanMoving) returned null");
            return false;
         }
         elem.SetAttribute("value", pfAttack.myIsShermanMoving.ToString());
         node = pfAttackNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): AppendChild(IsShermanMoving) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsLeadTank");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): CreateElement(IsLeadTank) returned null");
            return false;
         }
         elem.SetAttribute("value", pfAttack.myIsLeadTank.ToString());
         node = pfAttackNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): AppendChild(IsLeadTank) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("IsAdvancingFireZone");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): CreateElement(IsAdvancingFireZone) returned null");
            return false;
         }
         elem.SetAttribute("value", pfAttack.myIsAdvancingFireZone.ToString());
         node = pfAttackNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): AppendChild(IsAdvancingFireZone) returned null");
            return false;
         }
         //------------------------------------------------
         elem = aXmlDocument.CreateElement("Sector");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): CreateElement(Sector) returned null");
            return false;
         }
         elem.SetAttribute("value", pfAttack.mySector.ToString());
         node = pfAttackNode.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlPanzerfaustAttack(): AppendChild(Sector) returned null");
            return false;
         }
         return true;
      }
      private bool CreateXmlMapItemMoves(XmlDocument aXmlDocument, IMapItemMoves mapItemMoves)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): root is null");
            return false;
         }
         XmlElement? mapItemMovesElem = aXmlDocument.CreateElement("MapItemMoves");
         if (null == mapItemMovesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): CreateElement(MapItemMoves) returned null");
            return false;
         }
         mapItemMovesElem.SetAttribute("count", mapItemMoves.Count.ToString());
         XmlNode? mapItemMovesNode = root.AppendChild(mapItemMovesElem);
         if (null == mapItemMovesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): AppendChild(MapItemMoves) returned null");
            return false;
         }
         for (int i = 0; i < mapItemMoves.Count; ++i)
         {
            IMapItemMove? mim = mapItemMoves[i];
            if (null == mim)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): mim is null");
               return false;
            }
            XmlElement? mimElem = aXmlDocument.CreateElement("MapItemMove");
            if (null == mimElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): CreateElement(mimElem) returned null");
               return false;
            }
            mimElem.SetAttribute("value", mim.MapItem.Name);
            XmlNode? mimNode = mapItemMovesNode.AppendChild(mimElem);
            if (null == mimNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): AppendChild(MapItemMove) returned null");
               return false;
            }
            //--------------------------------------------
            if( false == CreateXmlMapItem(aXmlDocument, mimNode, mim.MapItem))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): CreateXmlMapItem() returned null");
               return false;
            }
            //--------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("OldTerritory");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateElement(OldTerritory) returned false");
               return false;
            }
            if( null == mim.OldTerritory )
            {
               elem.SetAttribute("value", "null");
               elem.SetAttribute("type", "null");
            }
            else
            {
               elem.SetAttribute("value", mim.OldTerritory.Name);
               elem.SetAttribute("type", mim.OldTerritory.Type);
            }
            XmlNode? node = mimNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): AppendChild(OldTerritory) returned false");
               return false;
            }
            //--------------------------------------------
            elem = aXmlDocument.CreateElement("NewTerritory");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateElement(NewTerritory) returned false");
               return false;
            }
            if (null == mim.NewTerritory)
            {
               elem.SetAttribute("value", "null");
               elem.SetAttribute("type", "null");
            }
            else
            {
               elem.SetAttribute("value", mim.NewTerritory.Name);
               elem.SetAttribute("type", mim.NewTerritory.Type);
            }
            node = mimNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): AppendChild(NewTerritory) returned false");
               return false;
            }
            //--------------------------------------------
            if (false == CreateXmlMapItemMovesBestPath(aXmlDocument, mimNode, mim.BestPath))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): CreateXmlMapItemMoveBestPath() returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlMapItemMovesBestPath(XmlDocument aXmlDocument, XmlNode parent, IMapPath? bestPath)
      {
         XmlElement? bestPathElem = aXmlDocument.CreateElement("BestPath");
         if (null == bestPathElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMovesBestPath(): CreateElement(MapItemMoves) returned null");
            return false;
         }
         XmlNode? mapItemMovesNode = parent.AppendChild(bestPathElem);
         if (null == mapItemMovesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMovesBestPath(): AppendChild(MapItemMoves) returned null");
            return false;
         }
         if (null == bestPath)
         {
            bestPathElem.SetAttribute("value", "null"); // BestPath
            return true;
         }
         bestPathElem.SetAttribute("name", bestPath.Name);
         bestPathElem.SetAttribute("metric", bestPath.Metric.ToString("F2"));
         bestPathElem.SetAttribute("count", bestPath.Territories.Count.ToString());
         for (int i = 0; i < bestPath.Territories.Count; ++i)
         {
            ITerritory? t = bestPath.Territories[i];
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMovesBestPath(): t is null");
               return false;
            }
            XmlElement? tElem = aXmlDocument.CreateElement("Territory");
            if (null == tElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMovesBestPath(): CreateElement(Territory) returned null");
               return false;
            }
            tElem.SetAttribute("name", t.Name);
            XmlNode? tNode = mapItemMovesNode.AppendChild(tElem);
            if (null == tNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMovesBestPath(): AppendChild(tNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlStacks(XmlDocument aXmlDocument, IStacks stacks, string attribute)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): root is null");
            return false;
         }
         XmlElement? stacksElem = aXmlDocument.CreateElement("Stacks");
         if (null == stacksElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateElement(Stacks) returned null");
            return false;
         }
         stacksElem.SetAttribute("value", attribute);
         stacksElem.SetAttribute("count", stacks.Count.ToString());
         XmlNode? stacksNode = root.AppendChild(stacksElem);
         if (null == stacksNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): AppendChild(Stacks) returned null");
            return false;
         }
         for (int i = 0; i < stacks.Count; ++i)
         {
            IStack? stack = stacks[i];
            if ( null == stack)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): stack is null");
               return false;
            }
            XmlElement? stackElem = aXmlDocument.CreateElement("Stack"); 
            if (null == stackElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateElement(stackElem) returned null");
               return false;
            }
            stackElem.SetAttribute("value", stack.Territory.Name);
            XmlNode? stackNode = stacksNode.AppendChild(stackElem);
            if (null == stackNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): AppendChild(stackNode) returned null");
               return false;
            }
            //------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("IsStacked");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateElement(IsStacked) returned false");
               return false;
            }
            elem.SetAttribute("value", stack.IsStacked.ToString());
            XmlNode? node = stackNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): AppendChild(IsStacked) returned false");
               return false;
            }
            //------------------------------------------
            if( false == CreateXmlMapItems(aXmlDocument, stackNode, stack.MapItems, stack.Territory.Name))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateXmlMapItems() returned false");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlEnteredHexes(XmlDocument aXmlDocument, List<EnteredHex> enteredHexes)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlEnteredHexes(): root is null");
            return false;
         }
         XmlElement? enteredHexesElem = aXmlDocument.CreateElement("EnteredHexes");
         if (null == enteredHexesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlEnteredHexes(): CreateElement(EnteredHexes) returned null");
            return false;
         }
         enteredHexesElem.SetAttribute("count", enteredHexes.Count.ToString());
         XmlNode? enteredHexesNode = root.AppendChild(enteredHexesElem);
         if (null == enteredHexesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlEnteredHexes(): AppendChild(EnteredHexes) returned null");
            return false;
         }
         for( int i=0; i< enteredHexes.Count; ++i )
         {
            EnteredHex enteredHex = enteredHexes[i];
            XmlElement? enteredHexElem = aXmlDocument.CreateElement("EnteredHex");  // name of territory
            if (null == enteredHexElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlTerritories(): CreateElement(enteredHexElem) returned null");
               return false;
            }
            enteredHexElem.SetAttribute("value", enteredHex.Identifer.ToString());
            XmlNode? enteredHexNode = enteredHexesNode.AppendChild(enteredHexElem);
            if (null == enteredHexNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlTerritories(): AppendChild(enteredHexNode) returned null");
               return false;
            }
            //------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("Day");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Day) returned false");
               return false;
            }
            elem.SetAttribute("value", enteredHex.Day.ToString());
            XmlNode? node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Day) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Date");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Date) returned false");
               return false;
            }
            elem.SetAttribute("value", enteredHex.Date);
            node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Date) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Time");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(Time) returned false");
               return false;
            }
            elem.SetAttribute("value", enteredHex.Time);
            node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(Time) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("TerritoryName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(TerritoryName) returned false");
               return false;
            }
            elem.SetAttribute("value", enteredHex.TerritoryName);
            node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(TerritoryName) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("MapPoint");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(MapPoint) returned false");
               return false;
            }
            elem.SetAttribute("X", enteredHex.MapPoint.X.ToString());
            elem.SetAttribute("Y", enteredHex.MapPoint.Y.ToString());
            node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(MapPoint) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("ColorAction");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): CreateElement(ColorAction) returned false");
               return false;
            }
            elem.SetAttribute("value", enteredHex.ColorAction.ToString());
            node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameReports(): AppendChild(ColorAction) returned false");
               return false;
            }
         }
         return true;
      }
      //private bool ReadXmlDieRollResults(XmlReader reader, Dictionary<string, int[]> dieResults)
      //{
      //   try // resync the gi.DieResults[] to initial conditions
      //   {
      //      foreach (string key in myRulesMgr.Events.Keys)
      //         dieResults[key] = new int[3] { Utilities.NO_RESULT, Utilities.NO_RESULT, Utilities.NO_RESULT };
      //   }
      //   catch (Exception e)
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): e=" + e.ToString());
      //      return false;
      //   }
      //   //------------------------------------------
      //   reader.Read();
      //   if (false == reader.IsStartElement())
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): IsStartElement(EnemyAcquiredShots) returned false");
      //      return false;
      //   }
      //   if (reader.Name != "DieRollResults")
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): DieRollResults != (node=" + reader.Name + ")");
      //      return false;
      //   }
      //   string? sCount = reader.GetAttribute("count");
      //   if (null == sCount)
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): Count=null");
      //      return false;
      //   }
      //   int count = int.Parse(sCount);
      //   for (int i = 0; i < count; i++)
      //   {
      //      reader.Read();
      //      if (false == reader.IsStartElement())
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): reader.IsStartElement(EnemyAcqShot) = false");
      //         return false;
      //      }
      //      if (reader.Name != "DieRollResult")
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): DieRollResult != (node=" + reader.Name + ")");
      //         return false;
      //      }
      //      //-------------------------------
      //      string? sKey = reader.GetAttribute("key");
      //      if (null == sKey)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): sKey=null");
      //         return false;
      //      }
      //      //-------------------------------
      //      string? sRoll0 = reader.GetAttribute("r0");
      //      if (null == sRoll0)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): sRoll0=null");
      //         return false;
      //      }
      //      dieResults[sKey][0] = Convert.ToInt32(sRoll0);
      //      //-------------------------------
      //      string? sRoll1 = reader.GetAttribute("r1");
      //      if (null == sRoll1)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): sRoll1=null");
      //         return false;
      //      }
      //      dieResults[sKey][1] = Convert.ToInt32(sRoll1);
      //      //-------------------------------
      //      string? sRoll2 = reader.GetAttribute("r2");
      //      if (null == sRoll2)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): sRoll2=null");
      //         return false;
      //      }
      //      dieResults[sKey][2] = Convert.ToInt32(sRoll2);
      //   }
      //   reader.Read(); // get past </DieRollResults> tag
      //   return true;
      //}
      //private bool CreateXmlDieRollResults(XmlDocument aXmlDocument, XmlNode topNode, Dictionary<string, int[]> dieResults)
      //{
      //   //------------------------------------------------------
      //   XmlElement? dieRollResultsElem = aXmlDocument.CreateElement("DieRollResults");
      //   if (null == dieRollResultsElem)
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "CreateXmlDieRollResults(): CreateElement(dieRollResultsElem) returned null");
      //      return false;
      //   }
      //   dieRollResultsElem.SetAttribute("count", dieResults.Count.ToString());
      //   XmlNode? dieRollResultsNode = topNode.AppendChild(dieRollResultsElem);
      //   if (null == dieRollResultsNode)
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "CreateXmlDieRollResults(): AppendChild(dieRollResultsNode) returned null");
      //      return false;
      //   }
      //   int count = 0;
      //   foreach (var kvp in dieResults)
      //   {
      //      XmlElement? dieRollResultElem = aXmlDocument.CreateElement("DieRollResult");
      //      if (null == dieRollResultElem)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "CreateXmlDieRollResults(): CreateElement(dieRollResultElem) returned null");
      //         return false;
      //      }
      //      dieRollResultElem.SetAttribute("key", kvp.Key);
      //      dieRollResultElem.SetAttribute("r0", kvp.Value[0].ToString());
      //      dieRollResultElem.SetAttribute("r1", kvp.Value[1].ToString());
      //      dieRollResultElem.SetAttribute("r2", kvp.Value[2].ToString());
      //      XmlNode? dieResultNode = dieRollResultsNode.AppendChild(dieRollResultElem);
      //      if (null == dieResultNode)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "CreateXmlDieRollResults(): AppendChild(dieResultNode) returned null");
      //         return false;
      //      }
      //      count++;
      //   }
      //   if (count != dieResults.Count)
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "CreateXmlDieRollResults(): count=" + count.ToString() + " dieResults.Count=" + dieResults.Count.ToString());
      //      return false;
      //   }
      //   return true;
      //}
   }
}
