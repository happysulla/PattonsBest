using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Transactions;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
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
         mySize = r.Next(8) + 5;
         myLeft = r.Next(range);
         myTop = r.Next(range);
      }
      public BloodSpot(int size, double left, double top)
      {
         mySize = size;
         myLeft = left;
         myTop = top;
      }
   }
   [Serializable]
   public class MapItem : IMapItem
   {
      [NonSerialized] private static Random theRandom = new Random();
      [NonSerialized] public static IMapImages theMapImages = new MapImages();
      [NonSerialized] private static BitmapImage? theBloodSpot = theMapImages.GetBitmapImage("OBlood1");
      [NonSerialized] private static BitmapImage? theMoving = theMapImages.GetBitmapImage("c13Moving");
      [NonSerialized] private static BitmapImage? theHullDown = theMapImages.GetBitmapImage("c14HullDown");
      [NonSerialized] private static BitmapImage? theTurret = theMapImages.GetBitmapImage("c16Turret");
      private const double PERCENT_MAPITEM_COVERED = 40.0;
      //--------------------------------------------------
      public string Name { get; set; } = string.Empty;
      public string TopImageName { get; set; } = string.Empty;
      public string BottomImageName { get; set; } = string.Empty;
      public string OverlayImageName { get; set; } = string.Empty;
      public List<BloodSpot> myWoundSpots = new List<BloodSpot>();
      public List<BloodSpot> WoundSpots { get => myWoundSpots; }
      public double Zoom { get; set; } = 1.0;
      public bool IsHidden { get; set; } = false;
      public bool IsAnimated
      {
         set
         {
            if (null == this.TopImageName)
               return;
            IMapImage? mii = theMapImages.Find(this.TopImageName);
            if (null == mii)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsAnimated.set() could not find map image for " + this.TopImageName);
               return;
            }
            mii.IsAnimated = value;
         }
         get
         {
            if (null == this.TopImageName)
               return false;
            IMapImage? mii = theMapImages.Find(this.TopImageName);
            if (null == mii)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsAnimated.get() could not find map image for " + this.TopImageName);
               return false;
            }
            return mii.IsAnimated;
         }
      }
      public bool IsMoved { get; set; } = false;
      public int Count { get; set; } = 0;
      //--------------------------------------------------
      public IMapPoint Location { get; set; } = new MapPoint(0.0, 0.0);
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
      //--------------------------------------------------
      private bool myIsFlipped = false;
      //----------------------------------------------------------------------------
      protected MapItem(string name)
      {
         this.Name = name;
      }
      protected MapItem(string aName, double zoom, bool isHidden, bool isAnimated, string topImageName)
      {
         try
         {
            this.Name = aName;
            this.Zoom = zoom;
            this.IsHidden = isHidden;
            this.TopImageName = topImageName;
            IMapImage? mii = theMapImages.Find(topImageName);
            if (null == mii)
            {
               mii = new MapImage(topImageName);
               theMapImages.Add(mii);
            }
            this.IsAnimated = isAnimated;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItem(): aName=" + aName + "\n Ex=" + ex.ToString());
            return;
         }
      }
      protected MapItem(string aName, double zoom, bool isHidden, bool isAnimated, string topImageName, string buttomImageName)
      {
         try
         {
            this.Name = aName;
            this.Zoom = zoom;
            this.IsHidden = isHidden;
            this.TopImageName = topImageName;
            IMapImage? miiTop = theMapImages.Find(topImageName);
            if (null == miiTop)
            {
               miiTop = new MapImage(topImageName);
               theMapImages.Add(miiTop);
            }
            this.TopImageName = topImageName;
            IMapImage? miiBottom = theMapImages.Find(buttomImageName);
            if (null == miiBottom)
            {
               miiBottom = new MapImage(buttomImageName);
               theMapImages.Add(miiBottom);
            }
            this.IsAnimated = isAnimated;
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
      public MapItem(string name, double zoom, string topImageName, ITerritory territory, bool isRandomLocation=false) :
         this(name, zoom, false, false, topImageName)
      {
         TerritoryCurrent = territory;
         TerritoryStarting = territory;
         this.Location.X = territory.CenterPoint.X - zoom * Utilities.theMapItemOffset;
         this.Location.Y = territory.CenterPoint.Y - zoom * Utilities.theMapItemOffset;
      }
      //----------------------------------------------------------------------------
      public void SetLocation(IMapPoint mp, int counterCount = 0)
      {
         double delta = Utilities.theMapItemOffset + (counterCount * Utilities.STACK);
         this.Location.X = mp.X - delta;
         this.Location.Y = mp.Y - delta;
      }
      //----------------------------------------------------------------------------
      public void Flip()
      {
         if (false == myIsFlipped)
         {
            myIsFlipped = true;
            string temp = TopImageName;
            TopImageName = BottomImageName;
            BottomImageName = temp;
         }
      }
      public void Unflip()
      {
         if (true == myIsFlipped)
         {
            myIsFlipped = false;
            string temp = TopImageName;
            TopImageName = BottomImageName;
            BottomImageName = temp;
         }
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder("Name=<");
         sb.Append(this.Name);
         sb.Append(">T=<");
         sb.Append(this.TerritoryCurrent.Name);
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
                  IMapItem randomIndex = (IMapItem)mapItems[index];
                  mapItems.RemoveAt(index);
                  newOrder.Add(randomIndex);
               }
            }
            mapItems = newOrder;
         }
      }
      public static void SetButtonContent(Button b, IMapItem mi, bool isBloodSpotsShown = true)
      {
         Grid g = new Grid() { };
         if (false == mi.IsAnimated)
         {
            Image img = new Image() { Source = theMapImages.GetBitmapImage(mi.TopImageName), Stretch = Stretch.Fill };
            img.Source = theMapImages.GetBitmapImage(mi.TopImageName);
            g.Children.Add(img);
            //----------------------------------------------------
            Canvas c = new Canvas() { };
            if (true == isBloodSpotsShown)
            {
               foreach (BloodSpot bs in mi.WoundSpots) // create wound spot on canvas
               {
                  Image spotImg = new Image() { Stretch = Stretch.Fill, Height = bs.mySize, Width = bs.mySize, Source = theBloodSpot };
                  c.Children.Add(spotImg);
                  Canvas.SetLeft(spotImg, bs.myLeft);
                  Canvas.SetTop(spotImg, bs.myTop);
               }
            }
            g.Children.Add(c);
            //----------------------------------------------------
            if (true == mi.IsMoving)
            {
               double width = 0.4 * mi.Zoom * Utilities.theMapItemOffset;
               double height = 1.33 * width;
               Image imgMoving = new Image() { Height = height, Width = width, Source = theMoving };
               c.Children.Add(imgMoving);
               Canvas.SetLeft(imgMoving, mi.Zoom * Utilities.theMapItemOffset - 0.5 * width);
               Canvas.SetTop(imgMoving, -0.5 * height);
            }
            if (true == mi.IsHullDown)
            {
               double width = 0.5 * mi.Zoom * Utilities.theMapItemSize;
               double height = width / 2.0;
               Image imgHullDown = new Image() { Height = height, Width = width, Source = theHullDown };
               c.Children.Add(imgHullDown);
               Canvas.SetLeft(imgHullDown, 0.5 * width );
               Canvas.SetTop(imgHullDown, - 0.5 * height);
            }
            if (true == mi.IsTurret)
            {
               double width = mi.Zoom * Utilities.theMapItemSize;
               double height = width;
               Image imgHullDown = new Image() { Height = height, Width = width, Source = theTurret };
               RotateTransform rotateTransform = new RotateTransform();
               imgHullDown.RenderTransformOrigin = new Point(0.5, 0.5);
               rotateTransform.Angle = mi.Count * 60.0;
               imgHullDown.RenderTransform = rotateTransform;
               c.Children.Add(imgHullDown);
               Canvas.SetLeft(imgHullDown, 0);
               Canvas.SetTop(imgHullDown, 0);
            }
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
      public CrewMember(string role, string rank, string topImageName)
         : base(SurnameMgr.GetSurname(), 1.0, false, false, topImageName)
      {
         Role = role;
         Rank = rank;
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
         foreach (IMapItem item in mapItems) { this.Add(item); }
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
         foreach (Object o in myList)
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
         foreach (Object o in myList)
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
            Object? temp = myList[0];
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
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[ ");
         foreach (Object o in myList)
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
