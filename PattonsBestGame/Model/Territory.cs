using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Windows.Media;
using Windows.Perception.Spatial;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Pattons_Best
{
   [Serializable]
   public class Territory : ITerritory
   {
      public string Name { get; set; } = "Offboard";
      public string CanvasName { get; set; } = "Main";
      public string Type { get; set; } = "ERROR";
      public IMapPoint CenterPoint { get; set; } = new MapPoint();
      public List<IMapPoint> Points { get; set; } = new List<IMapPoint>();
      public List<String> Adjacents { get; set; } = new List<String>();
      //---------------------------------------------------------------
      public static IMapPoint GetRandomPoint(ITerritory t)
      {
         if (0 == t.Points.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetRandomPoint(): t.Points.Count=0 for t.Name=" + t.Name);
            return t.CenterPoint;
         }
         //----------------------------------------------------
         // Make a StreamGeometry object from t.Points 
         StreamGeometry geometry = new StreamGeometry();
         using (StreamGeometryContext ctx = geometry.Open())
         {
            IMapPoint mp0 = t.Points[0];
            System.Windows.Point point0 = new System.Windows.Point(mp0.X, mp0.Y);
            ctx.BeginFigure(point0, true, true); //  filled and closed
            for (int i = 1; i < t.Points.Count; ++i)
            {
               IMapPoint mpI = t.Points[i];
               System.Windows.Point pointI = new System.Windows.Point(mpI.X, mpI.Y);
               ctx.LineTo(pointI, true, false);
            }
            geometry.Freeze();
         }
         System.Windows.Rect rect = geometry.Bounds;
         Path path = new Path();
         path.Fill = Brushes.Gold;
         path.Stroke = Brushes.Black;
         path.StrokeThickness = 1;
         path.Data = geometry;
         //----------------------------------------------------
         int count = 20;
         while (0 < --count)
         {
            double X = (double)Utilities.RandomGenerator.Next((int)rect.Left, (int)rect.Right); // Get a random point in the bounding box
            double Y = (double)Utilities.RandomGenerator.Next((int)rect.Top, (int)rect.Bottom);
            System.Windows.Point p = new System.Windows.Point(X + Utilities.theMapItemOffset, Y + Utilities.theMapItemOffset);
            if (true == geometry.FillContains(p))
            {
               Logger.Log(LogEnum.LE_SHOW_RANDOM_PT, "GetRandomPoint(): 1-t.Name=" + t.Name + " rect=(" + rect.Left.ToString("F1") + "," + rect.Right.ToString("F1") + "," + rect.Top.ToString("F1") + "," + rect.Bottom.ToString("F1") + ") Pt=(" + X.ToString("F1") + "," + Y.ToString("F1") + ")");
               return new MapPoint(X, Y);
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "GetRandomPoint(): Cannot find a random point in t.Name=" + t.Name + " rect=" + rect.ToString());
         return t.CenterPoint;
      }
      //---------------------------------------------------------------
      public Territory() 
      { 

      }
      public Territory(string name) { Name = name; }
      public override String ToString()
      {
         return this.Name;
      }
      public ITerritory Find(List<ITerritory> territories, string name)
      {
         IEnumerable<ITerritory> results = from territory in territories
                                           where territory.Name == name
                                           select territory;
         if (0 < results.Count())
            return results.First();
         else
            throw (new Exception("Territory.Find(): Unknown Territory=" + name));
      }
   }
   //---------------------------------------------------------------
   [Serializable]
   public class Territories : IEnumerable, ITerritories
   {
      [NonSerialized] public const string FILENAME = "Territories.xml";
      [NonSerialized] static public ITerritories theTerritories = new Territories();
      private readonly ArrayList myList;
      public Territories() { myList = new ArrayList(); }
      public void Add(ITerritory t) { myList.Add(t); }

      public void Insert(int index, ITerritory t) { myList.Insert(index, t); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(ITerritory t) 
      {
         foreach (Object o in myList)
         {
            ITerritory t1 = (ITerritory)o;
            if (Utilities.RemoveSpaces(t.Name) == Utilities.RemoveSpaces(t1.Name)) // match on name
               return true;
         }
         return false;
      }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(ITerritory t) { return myList.IndexOf(t); }
      public void Remove(ITerritory t) { myList.Remove(t); }
      public ITerritory? Find(string tName)
      {
         foreach (Object o in myList)
         {
            ITerritory t = (ITerritory)o;
            if (tName == Utilities.RemoveSpaces(t.Name))
               return t;
         }
         return null;
      }
      public ITerritory? RemoveAt(int index)
      {
         ITerritory? t = myList[index] as ITerritory;
         myList.RemoveAt(index);
         return t;
      }
      public ITerritory? Remove(string tName)
      {
         foreach (Object o in myList)
         {
            ITerritory t = (ITerritory)o;
            if (tName == t.Name)
            {
               myList.Remove(t);
               return t;
            }
         }
         return null;
      }
      public ITerritory? this[int index]
      {
         get 
         {  
            ITerritory? t = myList[index] as ITerritory;
            return t; 
         }
         set { myList[index] = value; }
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[ ");
         foreach (Object o in myList)
         {
            ITerritory t = (ITerritory)o;
            sb.Append(t.Name);
            sb.Append(" ");
         }
         sb.Append("]");
         return sb.ToString();
      }
   }
   //---------------------------------------------------------------
   public static class TerritoryExtensions
   {
      public static ITerritory? Find(this IList<ITerritory> territories, String name)
      {
         try
         {
            IEnumerable<ITerritory> results = from territory in territories where territory.Name == name select territory;
            if (0 < results.Count())
               return results.First();
         }
         catch (Exception e)
         {
            Console.WriteLine("MyTerritoryExtensions.Find(list, nameAndSector): nameAndSector={0} causes e.Message={1}", name, e.Message);
         }
         return null;
      }
   }
}
