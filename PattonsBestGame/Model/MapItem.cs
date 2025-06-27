using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Transactions;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using Windows.Graphics.Printing3D;
using WpfAnimatedGif;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using Point = System.Windows.Point;

namespace Pattons_Best
{
   [Serializable]
   public struct BloodSpot
   {
      public int mySize;      // diameter  of blood spot
      public double myLeft;   // left of where blood spot exists on canvas
      public double myTop;    // top of where blood spot exists on canvas
      public BloodSpot(int range, Random r)
      {
         mySize = r.Next(5) + 3;
         myLeft = r.Next(0, range - mySize);
         myTop  = r.Next(0, range - mySize);
      }
   }
   [Serializable]
   public class MapItem : IMapItem
   {
      [NonSerialized] private static Random theRandom = new Random();
      [NonSerialized] public static IMapImages theMapImages = new MapImages();
      [NonSerialized] protected static BitmapImage? theBloodSpot = theMapImages.GetBitmapImage("OBlood1");
      [NonSerialized] protected static BitmapImage? theMoving = theMapImages.GetBitmapImage("c13Moving");
      [NonSerialized] protected static BitmapImage? theHullDown = theMapImages.GetBitmapImage("c14HullDown");
      [NonSerialized] protected static BitmapImage? theWood = theMapImages.GetBitmapImage("OWoods");
      [NonSerialized] protected static BitmapImage? theFort = theMapImages.GetBitmapImage("OFort");
      [NonSerialized] protected static BitmapImage? theBuild = theMapImages.GetBitmapImage("OBuild");
      [NonSerialized] protected static BitmapImage? theThrownTrack = theMapImages.GetBitmapImage("OTrack");
      [NonSerialized] protected static BitmapImage? theHeHit = theMapImages.GetBitmapImage("OHeHit");
      [NonSerialized] protected static BitmapImage? theSherman75Turret = theMapImages.GetBitmapImage("c16TurretSherman75");
      [NonSerialized] protected static BitmapImage? thePzIVTurret = theMapImages.GetBitmapImage("c79PzIVTurret");
      [NonSerialized] protected static BitmapImage? thePzVTurret = theMapImages.GetBitmapImage("c80PzVTurret");
      [NonSerialized] protected static BitmapImage? thePzVIbTurret = theMapImages.GetBitmapImage("c82PzVIbTurret");
      [NonSerialized] protected static BitmapImage? thePzVIeTurret = theMapImages.GetBitmapImage("c82PzVIeTurret");
      //--------------------------------------------------
      public string Name { get; set; } = string.Empty;
      public string TopImageName { get; set; } = string.Empty;
      public string BottomImageName { get; set; } = string.Empty;
      public string OverlayImageName { get; set; } = string.Empty;
      public List<BloodSpot> myWoundSpots = new List<BloodSpot>();
      public List<BloodSpot> WoundSpots { get => myWoundSpots; }
      public double Zoom { get; set; } = 1.0;
      public bool IsAnimated
      {
         set
         {
            if (null == TopImageName)
               return;
            IMapImage? mii = theMapImages.Find(TopImageName);
            if (null == mii)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsAnimated.set() could not find map image for " + TopImageName);
               return;
            }
            mii.IsAnimated = value;
         }
         get
         {
            if (null == TopImageName)
               return false;
            IMapImage? mii = theMapImages.Find(TopImageName);
            if (null == mii)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsAnimated.get() could not find map image for " + TopImageName);
               return false;
            }
            return mii.IsAnimated;
         }
      }
      public bool IsMoved { get; set; } = false;
      public int Count { get; set; } = 0;
      public double RotationOffset { get; set; } = 0.0;
      public double RotationTurret { get; set; } = 0.0;
      public double RotationHull { get; set; } = 0.0;
      //--------------------------------------------------
      private IMapPoint myLocation = new MapPoint();  // top left corner of MapItem
      public IMapPoint Location 
      {
         get => myLocation;
         set => myLocation = value;
      }
      protected ITerritory myTerritoryCurrent = new Territory("Offboard");
      public ITerritory TerritoryCurrent
      {
         get => myTerritoryCurrent;
         set => myTerritoryCurrent = value;
      }
      protected ITerritory myTerritoryStarting = new Territory("Offboard");
      public ITerritory TerritoryStarting { get => myTerritoryStarting; set => myTerritoryStarting = value; }
      //--------------------------------------------------
      public bool IsMoving { get; set; } = false;  
      public bool IsHullDown { get; set; } = false;
      public bool IsTurret { get; set; } = false;
      public bool IsKilled { get; set; } = false;
      public bool IsFired { get; set; } = false;
      //--------------------------------------------------
      public bool IsSpotted { get; set; } = false;
      public bool IsVehicle { get; set; } = false;
      public bool IsWoods { get; set; } = false;
      public bool IsBuilding { get; set; } = false;
      public bool IsFortification { get; set; } = false;
      public bool IsThrownTrack { get; set; } = false;
      public bool IsBoggedDown { get; set; } = false;
      //--------------------------------------------------
      public int NumHeHit { get; set; } = 0;
      public EnumSpottingResult Spotting { get; set; } = EnumSpottingResult.UNSPOTTED;
      //----------------------------------------------------------------------------
      protected MapItem(string name)
      {
         Name = name;
      }
      protected MapItem(string aName, double zoom, bool isAnimated, string topImageName)
      {
         try
         {
            Name = aName;
            Zoom = zoom;
            TopImageName = topImageName;
            IMapImage? mii = theMapImages.Find(topImageName);
            if (null == mii)
            {
               mii = new MapImage(topImageName);
               theMapImages.Add(mii);
            }
            IsAnimated = isAnimated;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItem(): aName=" + aName + "\n Ex=" + ex.ToString());
            return;
         }
      }
      protected MapItem(string aName, double zoom, bool isAnimated, string topImageName, string buttomImageName)
      {
         try
         {
            Name = aName;
            Zoom = zoom;
            TopImageName = topImageName;
            IMapImage? miiTop = theMapImages.Find(topImageName);
            if (null == miiTop)
            {
               miiTop = new MapImage(topImageName);
               theMapImages.Add(miiTop);
            }
            TopImageName = topImageName;
            IMapImage? miiBottom = theMapImages.Find(buttomImageName);
            if (null == miiBottom)
            {
               miiBottom = new MapImage(buttomImageName);
               theMapImages.Add(miiBottom);
            }
            IsAnimated = isAnimated;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItem(): aName=" + aName + "\n Ex=" + ex.ToString());
            return;
         }
      }
      //----------------------------------------------------------------------------
      public MapItem()  // used in MapItemMove constructor
      {
      }
      public MapItem(string name, double zoom, string topImageName, ITerritory territory) :
         this(name, zoom, false, topImageName)
      {
         TerritoryCurrent = territory;
         TerritoryStarting = territory;
         Location.X = territory.CenterPoint.X - zoom * Utilities.theMapItemOffset;
         Location.Y = territory.CenterPoint.Y - zoom * Utilities.theMapItemOffset;
      }
      //----------------------------------------------------------------------------
      public void Clone(IMapItem mi)
      {
         this.IsMoved = mi.IsMoved;
         this.Count = mi.Count;
         this.RotationOffset = mi.RotationOffset;
         this.RotationTurret = mi.RotationTurret;
         this.RotationHull = mi.RotationHull;
         this.Location = mi.Location;
         this.TerritoryStarting = mi.TerritoryStarting;
         this.IsMoving = mi.IsMoving;
         this.IsHullDown = mi.IsHullDown;
         this.IsTurret = mi.IsTurret;
         this.IsKilled = mi.IsKilled;
         this.IsFired = mi.IsFired;
         this.IsVehicle = mi.IsVehicle;
         this.IsWoods = mi.IsWoods;
         this.IsBuilding = mi.IsBuilding;
         this.IsFortification = mi.IsFortification;
         this.Spotting = mi.Spotting;  
      }
      public bool IsEnemyUnit()
      {
         if (true == this.Name.Contains("LW"))
            return true;
         else if (true == this.Name.Contains("ATG") || true == this.Name.Contains("Pak"))
            return true;
         else if (true == this.Name.Contains("SPG") || true == this.Name.Contains("STuGIIIg"))
            return true;
         else if (true == this.Name.Contains("TANK") || true == this.Name.Contains("PzVI"))
            return true;
         else if (true == this.Name.Contains("MG"))
            return true;
         else if (true == this.Name.Contains("TRUCK"))
            return true;
         else if (true == this.Name.Contains("PSW") || true == this.Name.Contains("SPW"))
            return true;
         else if (true == this.Name.Contains("MARDER"))
            return true;
         else if (true == this.Name.Contains("PzIV"))
            return true;
         else if (true == this.Name.Contains("PzV"))
            return true;
         else if (true == this.Name.Contains("PzVIb"))
            return true;
         else if (true == this.Name.Contains("PzVIe"))
            return true;
         else if (true == this.Name.Contains("JdgPzIV"))
            return true;
         else if (true == this.Name.Contains("JdgPz38t"))
            return true;
         return false;
      }
      public string GetEnemyUnit()
      {
         string enemyUnit = "ERROR";
         if (true == this.Name.Contains("TANK"))
            enemyUnit = "TANK";
         else if (true == this.Name.Contains("ATG"))
            enemyUnit = "ATG";
         else if (true == this.Name.Contains("SPG"))
            enemyUnit = "SPG";
         else if (true == this.Name.Contains("LW"))
            enemyUnit = "LW";
         else if (true == this.Name.Contains("MG"))
            enemyUnit = "MG";
         else if (true == this.Name.Contains("TRUCK"))
            enemyUnit = "TRUCK";
         else if (true == this.Name.Contains("PSW"))
            enemyUnit = "PSW";
         else if (true == this.Name.Contains("SPW"))
            enemyUnit = "SPW";
         else if (true == this.Name.Contains("Pak38") )
            enemyUnit = "Pak38";
         else if (true == this.Name.Contains("Pak40"))
            enemyUnit = "Pak40";
         else if (true == this.Name.Contains("PzIV"))
            enemyUnit = "PzIV";
         else if (true == this.Name.Contains("PzV"))
            enemyUnit = "PzV";
         else if (true == this.Name.Contains("PzVIb"))
            enemyUnit = "PzVIb";
         else if (true == this.Name.Contains("PzVIe"))
            enemyUnit = "PzVIe";
         else if (true == this.Name.Contains("STuGIIIg"))
            enemyUnit = "STuGIIIg";
         else if (true == this.Name.Contains("MARDERII"))
            enemyUnit = "MARDERII";
         else if (true == this.Name.Contains("MARDERIII"))
            enemyUnit = "MARDERIII";
         else if (true == this.Name.Contains("JdgPzIV") )
            enemyUnit = "JdgPzIV";
         else if (true == this.Name.Contains("JdgPz38t") )
            enemyUnit = "JdgPz38t";
         else
            Logger.Log(LogEnum.LE_ERROR, "GetEnemyUnit() no assigned unit for mi.Name=" + this.Name);
         return enemyUnit;
      }
      public void SetBloodSpots(int percent=30)
      {
         for (int spots = 0; spots < percent; ++spots) // splatter the MapItem with random blood spots
         {
            int range = (int)(Utilities.theMapItemSize);
            BloodSpot spot = new BloodSpot(range, theRandom);
            myWoundSpots.Add(spot);
         }
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder("Name=<");
         sb.Append(Name);
         sb.Append(">T=<");
         sb.Append(TerritoryCurrent.Name);
         return sb.ToString();
      }
      //---------------------------------------------------------------------------- static functions
      public static void Shuffle(ref List<IMapItem> mapItems)
      {
         for (int j = 0; j < 10; ++j)
         {
            List<IMapItem> newOrder = new List<IMapItem>();
            // Random select card in myCards list and remove it.  Then add it to new list. 
            int count = mapItems.Count;
            for (int i = 0; i < count; i++)
            {
               int index = Utilities.RandomGenerator.Next(mapItems.Count);
               if (index < mapItems.Count)
               {
                  IMapItem randomIndex = mapItems[index];
                  mapItems.RemoveAt(index);
                  newOrder.Add(randomIndex);
               }
            }
            mapItems = newOrder;
         }
      }
      public static void SetButtonContent(Button b, IMapItem mi, bool isMapItemZoom = false, bool isDecoration = true, bool isBloodSpotsShown = true)
      {
         double zoom = Utilities.ZOOM;
         if( true == isMapItemZoom )
            zoom = mi.Zoom;
         //----------------------------
         Grid g = new Grid() { };
         if (false == mi.IsAnimated)
         {
            Image img = new Image() { Source = theMapImages.GetBitmapImage(mi.TopImageName), Stretch = Stretch.Fill };
            img.Source = theMapImages.GetBitmapImage(mi.TopImageName);
            g.Children.Add(img);
            //----------------------------------------------------
            if (true == mi.IsTurret)
            {
               double width = zoom * Utilities.theMapItemSize;
               double height = width;
               Image? imgTurret = null;
               if (true == mi.Name.Contains("Sherman"))
                  imgTurret = new Image() { Height = height, Width = width, Source = theSherman75Turret };
               else if (true == mi.Name.Contains("TANK") || true == mi.Name.Contains("PzVIe"))
                  imgTurret = new Image() { Height = height, Width = width, Source = thePzVIbTurret };
               else if (true == mi.Name.Contains("PzIV"))
                  imgTurret = new Image() { Height = height, Width = width, Source = thePzIVTurret };
               else if (true == mi.Name.Contains("PzV"))
                  imgTurret = new Image() { Height = height, Width = width, Source = thePzVTurret };
               else if (true == mi.Name.Contains("PzVIe"))
                  imgTurret = new Image() { Height = height, Width = width, Source = thePzVIeTurret };
               if (null == imgTurret)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SetButtonContent(): turret=null mi=" + mi.Name);
               }
               else
               {
                  RotateTransform rotateTransform = new RotateTransform();
                  imgTurret.RenderTransformOrigin = new Point(0.5, 0.5);
                  rotateTransform.Angle = mi.RotationTurret;
                  imgTurret.RenderTransform = rotateTransform;
                  g.Children.Add(imgTurret);
                  Canvas.SetLeft(imgTurret, 0);
                  Canvas.SetTop(imgTurret, 0);
               }
            }
            Canvas c = new Canvas() { };
            if (true == isBloodSpotsShown)
            {
               foreach (BloodSpot bs in mi.WoundSpots) // create wound spot on canvas
               {
                  double size = bs.mySize * zoom;
                  Image spotImg = new Image() { Stretch = Stretch.Fill, Height = size, Width = size, Source = theBloodSpot };
                  c.Children.Add(spotImg);
                  Canvas.SetLeft(spotImg, bs.myLeft * zoom);
                  Canvas.SetTop(spotImg, bs.myTop * zoom);
               }
            }
            g.Children.Add(c);
            //----------------------------------------------------
            if (true == isDecoration)
            {
               //----------------------------------------------------
               if (true == mi.IsMoving)
               {
                  double width = 0.4 * zoom * Utilities.theMapItemOffset;
                  double height = 1.33 * width;
                  Image imgTerrain = new Image() { Height = height, Width = width, Source = theMoving };
                  c.Children.Add(imgTerrain);
                  Canvas.SetLeft(imgTerrain, zoom * Utilities.theMapItemOffset - 0.5 * width);
                  Canvas.SetTop(imgTerrain, -0.5 * height);
               }
               else if (true == mi.IsHullDown)
               {
                  double width = 0.5 * zoom * Utilities.theMapItemSize;
                  double height = width / 2.0;
                  Image imgTerrain = new Image() { Height = height, Width = width, Source = theHullDown };
                  c.Children.Add(imgTerrain);
                  Canvas.SetLeft(imgTerrain, 0.5 * width);
                  Canvas.SetTop(imgTerrain, -0.5 * height);
               }
               else if (true == mi.IsWoods)
               {
                  double width = zoom * Utilities.theMapItemSize;
                  double height = width;
                  Image imgTerrain = new Image() { Height = height, Width = width, Source = theWood };
                  c.Children.Add(imgTerrain);
                  Canvas.SetLeft(imgTerrain, 0);
                  Canvas.SetTop(imgTerrain, 0);
               }
               else if (true == mi.IsFortification)
               {
                  double width = zoom * Utilities.theMapItemSize;
                  double height = width;
                  Image imgTerrain = new Image() { Height = height, Width = width, Source = theFort };
                  c.Children.Add(imgTerrain);
                  Canvas.SetLeft(imgTerrain, 0);
                  Canvas.SetTop(imgTerrain, 0);
               }
               else if (true == mi.IsBuilding)
               {
                  double width = zoom * Utilities.theMapItemSize;
                  double height = width;
                  Image imgTerrain = new Image() { Height = height, Width = width, Source = theBuild };
                  c.Children.Add(imgTerrain);
                  Canvas.SetLeft(imgTerrain, 0);
                  Canvas.SetTop(imgTerrain, 0);
               }
               else if (true == mi.IsThrownTrack)
               {
                  double width = zoom * Utilities.theMapItemSize;
                  double height = width;
                  Image imgTrack = new Image() { Height = height, Width = width, Source = theThrownTrack };
                  g.Children.Add(imgTrack);
                  Canvas.SetLeft(imgTrack, 0);
                  Canvas.SetTop(imgTrack, 0);
               }
               if ((EnumSpottingResult.SPOTTED == mi.Spotting) || (true == mi.IsSpotted)) // if Spotted now or previous round
               {
                  if ( (true == mi.Name.Contains("TANK")) || (true == mi.Name.Contains("ATG")) || (true == mi.Name.Contains("SPG") ) )
                  {
                     Image overlay = new Image() { Stretch = Stretch.Fill, Source = theMapImages.GetBitmapImage("OSPOT") };
                     g.Children.Add(overlay);
                  }
               }
               if (EnumSpottingResult.HIDDEN == mi.Spotting)
               {
                  Image overlay1 = new Image() { Stretch = Stretch.Fill, Source = theMapImages.GetBitmapImage("OHIDE") };
                  RotateTransform rotateTransform = new RotateTransform();
                  overlay1.RenderTransformOrigin = new Point(0.5, 0.5);
                  rotateTransform.Angle = -(mi.RotationHull + mi.RotationOffset);
                  overlay1.RenderTransform = rotateTransform;
                  g.Children.Add(overlay1);
               }
               if (0 < mi.NumHeHit ) 
               {
                  double width = zoom * Utilities.theMapItemSize;
                  double height = width;
                  Image imgTrack = new Image() { Height = height, Width = width, Source = theHeHit };
                  g.Children.Add(imgTrack);
                  Canvas.SetLeft(imgTrack, 0);
                  Canvas.SetTop(imgTrack, 0);
               }
            }
            //----------------------------------
            if ("" != mi.OverlayImageName)
            {
               Image overlay = new Image() { Stretch = Stretch.Fill, Source = theMapImages.GetBitmapImage(mi.OverlayImageName) };
               g.Children.Add(overlay);
            }
         }
         else
         {
            IMapImage? mii = theMapImages.Find(mi.TopImageName);
            if (null == mii)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetButtonContent(): mii=null");
            }
            else
            {
               g.Children.Add(mii.ImageControl);
            }
         }
         b.Content = g;
      }
   }
   //----------------------------------------------------------------------------
   public class CrewMember : MapItem, ICrewMember
   {
      public string Role { get; set; } = string.Empty;
      public string Rank { get; set; } = string.Empty;
      public int Rating { get; set; } = 0;
      public bool IsButtonedUp { get; set; } = true;
      public int Sector { get; set; } = 0;
      public string Action { get; set; } = "None";
      public string Wound { get; set; } = "None";
      public bool IsUnconscious { get; set; } = false;
      public bool IsIncapacitated { get; set; } = false;
      public CrewMember(string role, string rank, string topImageName)
         : base(SurnameMgr.GetSurname(), 1.0, false, topImageName)
      {
         Role = role;
         Rank = rank;
      }
      public static void SetButtonContent(Button b, ICrewMember cm, bool isMapItemZoom = true, bool isDecoration = true, bool isBloodSpotsShown = true)
      {
         double zoom = Utilities.ZOOM;
         if (true == isMapItemZoom)
            zoom = cm.Zoom;
         Grid g = new Grid() { };
         if (false == cm.IsAnimated)
         {
            Image img = new Image() { Source = theMapImages.GetBitmapImage(cm.TopImageName), Stretch = Stretch.Fill };
            img.Source = theMapImages.GetBitmapImage(cm.TopImageName);
            g.Children.Add(img);
            //----------------------------------------------------
            Canvas c = new Canvas() { };
            if (true == isBloodSpotsShown)
            {
               foreach (BloodSpot bs in cm.WoundSpots) // create wound spot on canvas
               {
                  double size = bs.mySize * zoom;
                  Image spotImg = new Image() { Stretch = Stretch.Fill, Height = size, Width = size, Source = theBloodSpot };
                  c.Children.Add(spotImg);
                  Canvas.SetLeft(spotImg, bs.myLeft * zoom);
                  Canvas.SetTop(spotImg, bs.myTop * zoom);
               }
            }
            g.Children.Add(c);
            //----------------------------------------------------
            if (true == isDecoration)
            {
               if (true == cm.IsMoving)
               {
                  double width = 0.4 * zoom * Utilities.theMapItemOffset;
                  double height = 1.33 * width;
                  Image imgTerrain = new Image() { Height = height, Width = width, Source = theMoving };
                  c.Children.Add(imgTerrain);
                  Canvas.SetLeft(imgTerrain, zoom * Utilities.theMapItemOffset - 0.5 * width);
                  Canvas.SetTop(imgTerrain, -0.5 * height);
               }
               else if (true == cm.IsHullDown)
               {
                  double width = 0.5 * zoom * Utilities.theMapItemSize;
                  double height = width / 2.0;
                  Image imgTerrain = new Image() { Height = height, Width = width, Source = theHullDown };
                  c.Children.Add(imgTerrain);
                  Canvas.SetLeft(imgTerrain, 0.5 * width);
                  Canvas.SetTop(imgTerrain, -0.5 * height);
               }
               else if (true == cm.IsWoods)
               {
                  double width = zoom * Utilities.theMapItemSize;
                  double height = width;
                  Image imgTerrain = new Image() { Height = height, Width = width, Source = theWood };
                  c.Children.Add(imgTerrain);
                  Canvas.SetLeft(imgTerrain, 0);
                  Canvas.SetTop(imgTerrain, 0);
               }
               else if (true == cm.IsFortification)
               {
                  double width = zoom * Utilities.theMapItemSize;
                  double height = width;
                  Image imgTerrain = new Image() { Height = height, Width = width, Source = theFort };
                  c.Children.Add(imgTerrain);
                  Canvas.SetLeft(imgTerrain, 0);
                  Canvas.SetTop(imgTerrain, 0);
               }
               else if (true == cm.IsBuilding)
               {
                  double width = zoom * Utilities.theMapItemSize;
                  double height = width;
                  Image imgTerrain = new Image() { Height = height, Width = width, Source = theBuild };
                  c.Children.Add(imgTerrain);
                  Canvas.SetLeft(imgTerrain, 0);
                  Canvas.SetTop(imgTerrain, 0);
               }
            }
            if (true == cm.IsKilled)
            {
               Image overlay = new Image() { Stretch = Stretch.Fill, Source = theMapImages.GetBitmapImage("OKIA") };
               g.Children.Add(overlay);
            }
            else if (true == cm.IsUnconscious)
            {
               Image overlay = new Image() { Stretch = Stretch.Fill, Source = theMapImages.GetBitmapImage("OUNC") };
               g.Children.Add(overlay);
            }
            else if (true == cm.IsIncapacitated)
            {
               Image overlay = new Image() { Stretch = Stretch.Fill, Source = theMapImages.GetBitmapImage("OINC") };
               g.Children.Add(overlay);
            }
            else if ("" != cm.OverlayImageName)
            {
               Image overlay = new Image() { Stretch = Stretch.Fill, Source = theMapImages.GetBitmapImage(cm.OverlayImageName) };
               g.Children.Add(overlay);
            }
         }
         else
         {
            IMapImage? mii = theMapImages.Find(cm.TopImageName);
            if (null == mii)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetButtonContent(): mii=null");
            }
            else
            {
               g.Children.Add(mii.ImageControl);
            }
         }
         b.Content = g;
      }
   }
   //--------------------------------------------------------------------------
   [Serializable]
   public class MapItems : IEnumerable, IMapItems
   {
      private readonly ArrayList myList;
      public MapItems() { myList = new ArrayList(); }
      public MapItems(IMapItems mapItems)
      {
         myList = new ArrayList();
         foreach (IMapItem item in mapItems) { Add(item); }
      }
      public void Add(IMapItem? mi) { myList.Add(mi); }
      public void Insert(int index, IMapItem mi) { myList.Insert(index, mi); }
      public int Count { get { return myList.Count; } }
      public void Reverse() { myList.Reverse(); }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMapItem mi) { return myList.Contains(mi); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMapItem mi) { return myList.IndexOf(mi); }
      public void Remove(IMapItem mi) { myList.Remove(mi); }
      public IMapItem? Find(string miName)
      {
         foreach (object o in myList)
         {
            IMapItem? mi = o as IMapItem;
            if (null == mi)
               return null;
            if (miName == Utilities.RemoveSpaces(mi.Name))
               return mi;
         }
         return null;
      }
      public IMapItem? Remove(string miName)
      {
         foreach (object o in myList)
         {
            IMapItem? mi = (IMapItem)o;
            if (null == mi) return null;
            if (miName == mi.Name)
            {
               myList.Remove(mi);
               return mi;
            }
         }
         return null;
      }
      public IMapItem? RemoveAt(int index)
      {
         IMapItem? mi = myList[index] as IMapItem;
         if (null == mi)
            return null;
         myList.RemoveAt(index);
         return mi;
      }
      public IMapItem? this[int index]
      {
         get
         {
            IMapItem? mi = myList[index] as IMapItem;
            return mi;
         }
         set { myList[index] = value; }
      }
      public IMapItems Shuffle()
      {
         IMapItems newOrder = new MapItems();
         int count = myList.Count;
         for (int i = 0; i < count; i++)
         {
            int index = Utilities.RandomGenerator.Next(myList.Count);
            if (index < myList.Count)
            {
               IMapItem? randomMapItem = myList[index] as IMapItem;
               myList.RemoveAt(index);
               if (randomMapItem == null)
                  Logger.Log(LogEnum.LE_ERROR, "Shuffle(): randomMapItem=null");
               else
                  newOrder.Add(randomMapItem);
            }
         }
         return newOrder;
      }
      public void Rotate(int numOfRotates)
      {
         for (int j = 0; j < numOfRotates; j++)
         {
            object? temp = myList[0];
            if (temp == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "Rotate(): myList[0]=null");
               return;
            }
            else
            {
               for (int i = 0; i < myList.Count - 1; i++)
                  myList[i] = myList[i + 1];
               myList[myList.Count - 1] = temp;
            }
         }
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[ ");
         foreach (object o in myList)
         {
            IMapItem mi = (IMapItem)o;
            sb.Append(mi.Name);
            sb.Append(" ");
         }
         sb.Append("]");
         return sb.ToString();
      }
   }
   //--------------------------------------------------------------------------
   public static class MyMapItemExtensions
   {
      public static IMapItem? Find(this IList<IMapItem> list, string miName)
      {
         IEnumerable<IMapItem> results = from mi in list where mi.Name == miName select mi;
         if (0 < results.Count())
            return results.First();
         else
            return null;
      }
   }
}
