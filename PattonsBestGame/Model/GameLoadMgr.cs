using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

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
            if( null == gi )
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
         catch(Exception e)
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
               if( null == gamePath)
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
               if( null == aXmlDocument)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): CreateXml() returned null for path=" + theGamesDirectory );
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
         if ( Day < 10 )
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
         int count = Int32.Parse(sCount);
         //-----------------------------------------------------------------
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(Parent)=false");
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
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): IsStartElement(Parent)=false");
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
            switch (sAttribute)
            {
               case "Tank": territory.Parent = Territory.TerritoryEnum.Tank; break;
               case "Movement": territory.Parent = Territory.TerritoryEnum.Movement; break;
               case "Battle": territory.Parent = Territory.TerritoryEnum.Battle; break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): reached default sAttribute=" + sAttribute);
                  return false;
            }
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
            territory.CenterPoint.X = Double.Parse(sX);
            string? sY = reader.GetAttribute("Y");
            if (null == sY)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute(sX)=null");
               return false;
            }
            territory.CenterPoint.Y = Double.Parse(sY);
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
            int count0 = Int32.Parse(sCount0);
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
               double x = Double.Parse(sX1);
               double y = Double.Parse(sY1);
               IMapPoint mp = new MapPoint(x,y);
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
            int count3 = Int32.Parse(sCount3);
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
         if( null == root.Attributes )
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
            elem.SetAttribute("value", t.Parent.ToString());
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
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(elemPoints) returned null");
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
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(terrElem) returned null");
                  return false;
               }
               elem.SetAttribute("value", s);
               node = nodeAdjacents.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
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
            if( true == reader.IsStartElement())
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
                  int version = Int32.Parse(sVersion);
                  if ( version != GetMajorVersion() )
                  {
                     System.Windows.MessageBox.Show("Unable to open due to version mismatch. File v" + version + " does not match running v" + GetMajorVersion() + ".");
                     return null;
                  }
               }
            }
            //----------------------------------------------
            if( false == ReadXmlOptions(reader, gi.Options))
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
               if (reader.Name != "EventStart")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? eventStart = reader.GetAttribute("value");
                  if (null == eventStart)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(EventStart)=null");
                     return null;
                  }
                  gi.EventStart = eventStart;
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
                  gi.GameTurn = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            if (false == ReadXmlEnteredHexes(reader, gi.EnteredHexes))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlEnteredHexes() returned false");
               return null;
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
         if( null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Count=null");
            return false;
         }
         //-------------------------------------
         int count = Int32.Parse(sCount);
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
               bool isEnabled = Boolean.Parse(sEnabled);
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
            int count = Int32.Parse(sCount);
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
                  if( null == tName )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameMapItems(): GetAttribute() returned false");
                     return false;
                  }
                  ITerritory? territory = Territories.theTerritories.Find(tName);
                  if( null == territory )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameMapItems(): Find() returned null for tName=" + tName);
                     return false;
                  }
                  territories.Add(territory);   
               }
            }
            if( 0 < count )
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
            int count = Int32.Parse(sCount);
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
      private bool ReadXmlEnteredHexes(XmlReader reader, List<EnteredHex> enteredHexes)
      {
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != "EnteredHexes")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EnteredHexes != (node=" + reader.Name + ")");
               return false;
            }
            string? sTheIdentifier = reader.GetAttribute("ID");
            if (null == sTheIdentifier)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sIdentifier=null");
               return false;
            }
            EnteredHex.theId = Int32.Parse(sTheIdentifier);
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Count=null");
               return false;
            }
            int countEnterHex = Int32.Parse(sCount);
            for (int i = 0; i < countEnterHex; ++i)
            {
               string? sIdentifier = null;
               int day = 0;
               string? sHexName = null;
               bool isEncounter = false;
               int position = 0;
               ColorActionEnum colorAction = ColorActionEnum.CAE_START;
               List<string> eventNames = new List<string>();
               List<string> partyNames = new List<string>();   
               //----------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "EnteredHex")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EnteredHex != (node=" + reader.Name + ")");
                     return false;
                  }
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Identifier")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Identifier != (node=" + reader.Name + ")");
                     return false;
                  }
                  sIdentifier = reader.GetAttribute("value");
                  if (null == sIdentifier)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sIdentifier) returned null");
                     return false;
                  }
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Day")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Day != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sDay = reader.GetAttribute("value");
                  if (null == sDay)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sDay) returned null");
                     return false;
                  }
                  day = Int32.Parse(sDay);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "HexName")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): HexName != (node=" + reader.Name + ")");
                     return false;
                  }
                  sHexName = reader.GetAttribute("value");
                  if (null == sHexName)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sHexName) returned null");
                     return false;
                  }
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsEncounter")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsEncounter != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsEncounter = reader.GetAttribute("value");
                  if (null == sIsEncounter)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sIsEncounter) returned null");
                     return false;
                  }
                  isEncounter = Boolean.Parse(sIsEncounter);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Position")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Position != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sPosition = reader.GetAttribute("value");
                  if (null == sPosition)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sPosition) returned null");
                     return false;
                  }
                  position = Int32.Parse(sPosition);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "ColorAction")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): ColorAction != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sColorAction = reader.GetAttribute("value");
                  if (null == sColorAction)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sColorAction) returned null");
                     return false;
                  }
                  switch (sColorAction)
                  {
                     case "CAE_START": colorAction = ColorActionEnum.CAE_START; break;
                     case "CAE_LOST": colorAction = ColorActionEnum.CAE_LOST; break;
                     case "CAE_REST": colorAction = ColorActionEnum.CAE_REST; break;
                     case "CAE_JAIL": colorAction = ColorActionEnum.CAE_JAIL; break;
                     case "CAE_TRAVEL": colorAction = ColorActionEnum.CAE_TRAVEL; break;
                     case "CAE_TRAVEL_AIR": colorAction = ColorActionEnum.CAE_TRAVEL_AIR; break;
                     case "CAE_TRAVEL_RAFT": colorAction = ColorActionEnum.CAE_TRAVEL_RAFT; break;
                     case "CAE_TRAVEL_DOWNRIVER": colorAction = ColorActionEnum.CAE_TRAVEL_DOWNRIVER; break;
                     case "CAE_ESCAPE": colorAction = ColorActionEnum.CAE_ESCAPE; break;
                     case "CAE_FOLLOW": colorAction = ColorActionEnum.CAE_FOLLOW; break;
                     case "CAE_SEARCH": colorAction = ColorActionEnum.CAE_SEARCH; break;
                     case "CAE_SEARCH_RUINS": colorAction = ColorActionEnum.CAE_SEARCH_RUINS; break;
                     case "CAE_SEEK_NEWS": colorAction = ColorActionEnum.CAE_SEEK_NEWS; break;
                     case "CAE_HIRE": colorAction = ColorActionEnum.CAE_HIRE; break;
                     case "CAE_AUDIENCE": colorAction = ColorActionEnum.CAE_AUDIENCE; break;
                     case "CAE_OFFERING": colorAction = ColorActionEnum.CAE_OFFERING; break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): reached default with sColorAction=" + sColorAction);
                        return false;
                  }
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "EventNames")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EventNames != (node=" + reader.Name + ")");
                     return false;
                  }
                  sCount = reader.GetAttribute("count");
                  if (null == sCount)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sCount) returned null");
                     return false;
                  }
                  int countEventName = Int32.Parse(sCount);
                  for(int k=0; k< countEventName; ++k )
                  {
                     reader.Read();
                     if (reader.Name != "EventName")
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EventName != (node=" + reader.Name + ")");
                        return false;
                     }
                     string? sEventName= reader.GetAttribute("value");
                     if (null == sEventName)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sEventName) returned null");
                        return false;
                     }
                     eventNames.Add(sEventName);
                  }
                  if (0 < countEventName)
                     reader.Read(); // get past </EventNames> tag
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "PartyNames")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): PartyNames != (node=" + reader.Name + ")");
                     return false;
                  }
                  sCount = reader.GetAttribute("count");
                  if (null == sCount)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sCount) returned null");
                     return false;
                  }
                  int countPartyName = Int32.Parse(sCount);
                  for (int k = 0; k < countPartyName; ++k)
                  {
                     reader.Read();
                     if (reader.Name != "PartyName")
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): PartyName != (node=" + reader.Name + ")");
                        return false;
                     }
                     string? sPartyName = reader.GetAttribute("value");
                     if (null == sPartyName)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sPartyName) returned null");
                        return false;
                     }
                     partyNames.Add(sPartyName);
                  }
                  if( 0 < countPartyName)
                     reader.Read(); // get past </PartyNames> tag
               }
               //--------------------------------------
               if(null == sIdentifier)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sIdentifier=null");
                  return false;
               }
               if (null == sHexName)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sHexName=null");
                  return false;
               }
               EnteredHex enteredHex = new EnteredHex(sIdentifier, day, sHexName, isEncounter, position, colorAction, eventNames, partyNames);
               enteredHexes.Add(enteredHex);
               reader.Read(); // get past </EnteredHex> tag
            }
            if (0 < countEnterHex)
               reader.Read(); // get past </EnteredHexes> tag
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
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): aXmlDocument.DocumentElement=null");
            return null;
         }
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return null;
         }
         //------------------------------------------
         XmlElement? versionElem = aXmlDocument.CreateElement("Version"); 
         if( null == versionElem )
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): aXmlDocument.DocumentElement.LastChild=null");
            return null;
         }
         int majorVersion = GetMajorVersion();
         if (majorVersion < 0 )
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml():  0 > majorVersion=" + majorVersion.ToString());
            return null;
         }
         versionElem.SetAttribute("value", majorVersion.ToString());
         XmlNode? versionNode = root.AppendChild(versionElem);
         if (null == versionNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(versionNode) returned null");
            return null;
         }
         //------------------------------------------
         if ( false == CreateXmlGameOptions(aXmlDocument, gi.Options))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlOptions() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlGameStat(aXmlDocument, gi.Statistic))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlGameStat() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItems(aXmlDocument, gi.MapItems))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlGameStat() returned false");
            return null;
         }
         //------------------------------------------
         XmlElement? elem = aXmlDocument.CreateElement("EventActive");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(EventActive) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EventActive);
         XmlNode? node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(EventActive) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EventDisplayed");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(EventDisplayed) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EventDisplayed);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(EventDisplayed) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EventStart");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(EventStart) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EventStart);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(EventStart) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("GameTurn");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(GameTurn) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.GameTurn.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(GameTurn) returned null");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlEnteredHexes(aXmlDocument, gi.EnteredHexes))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlEnteredHexes() returned false");
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
            if( null == optionElem )
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
      public bool CreateXmlMapItems(XmlDocument aXmlDocument, IMapItems MapItems)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlElement? MapItemsElem = aXmlDocument.CreateElement("MapItems");
         if (null == MapItemsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(MapItemsElem) returned null");
            return false;
         }
         MapItemsElem.SetAttribute("count", MapItems.Count.ToString());
         XmlNode? MapItemsNode = root.AppendChild(MapItemsElem);
         if (null == MapItemsNode)
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
            XmlNode? miNode = MapItemsNode.AppendChild(miElem);
            if (null == miNode)
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
            XmlNode? node = miNode.AppendChild(elem);
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
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }

            //--------------------------------
            elem = aXmlDocument.CreateElement("MovementUsed");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(MovementUsed) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.MovementUsed.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
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
      private bool CreateXmlEnteredHexes(XmlDocument aXmlDocument, List<EnteredHex> hexes)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         //--------------------------------------------------------------------
         XmlElement? enteredHexesElem = aXmlDocument.CreateElement("EnteredHexes");
         if (null == enteredHexesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Territories) returned null");
            return false;
         }
         enteredHexesElem.SetAttribute("ID", EnteredHex.theId.ToString());
         enteredHexesElem.SetAttribute("count", hexes.Count.ToString());
         XmlNode? enteredHexesNode = root.AppendChild(enteredHexesElem);
         if (null == enteredHexesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(enteredHexesNode) returned null");
            return false;
         }
         //--------------------------------------------------------------------
         foreach (EnteredHex hex in hexes)
         {
            XmlElement? hexElem = aXmlDocument.CreateElement("EnteredHex");  
            if (null == hexElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(hexElem) returned null");
               return false;
            }
            XmlNode? hexNode = enteredHexesNode.AppendChild(hexElem);
            if (null == hexNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(hexNode) returned null");
               return false;
            }
            //-------------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("Identifier");  
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Identifier) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.Identifer.ToString());
            XmlNode? node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("Day");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Day) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.Day.ToString());
            node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("HexName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(HexName) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.HexName);
            node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("IsEncounter");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsEncounter) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.IsEncounter.ToString());
            node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("Position");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Position) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.Position.ToString());
            node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("ColorAction");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(ColorAction) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.ColorAction.ToString());
            node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            XmlElement? eventNamesElem = aXmlDocument.CreateElement("EventNames");
            if (null == eventNamesElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(EventNames) returned null");
               return false;
            }
            eventNamesElem.SetAttribute("count", hex.EventNames.Count.ToString());
            XmlNode? eventNamesNode = hexNode.AppendChild(eventNamesElem);
            if (null == eventNamesNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(eventNamesNode) returned null");
               return false;
            }
            foreach( string sEventName in hex.EventNames )
            {
               elem = aXmlDocument.CreateElement("EventName");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(EventName) returned null");
                  return false;
               }
               elem.SetAttribute("value", sEventName);
               node = eventNamesNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
                  return false;
               }
            }
            //-------------------------------------------------
            XmlElement? partyNamesElem = aXmlDocument.CreateElement("PartyNames");
            if (null == partyNamesElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(PartyNames) returned null");
               return false;
            }
            partyNamesElem.SetAttribute("count", hex.Party.Count.ToString());
            XmlNode? partyNamesNode = hexNode.AppendChild(partyNamesElem);
            if (null == partyNamesNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(partyNamesNode) returned null");
               return false;
            }
            foreach (string sPartyName in hex.Party)
            {
               elem = aXmlDocument.CreateElement("PartyName");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(PartyName) returned null");
                  return false;
               }
               elem.SetAttribute("value", sPartyName);
               node = partyNamesNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
                  return false;
               }
            }
         }
         return true;
      }
   }
}
