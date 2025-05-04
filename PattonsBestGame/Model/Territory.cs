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
using System.Windows.Controls.Primitives;

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
      public List<string> Adjacents { get; set; } = new List<string>();
      //---------------------------------------------------------------
      public static IMapPoint GetRandomPoint(ITerritory t, double offset) // return the top left location of a MapItem, not the center point
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
         //----------------------------------------------------
         int count = 20;
         while (0 < --count) // offset is the difference between MapItem location on screen and the center of the  MapItem.
         {
            double XCenter = Utilities.RandomGenerator.Next((int)rect.Left, (int)rect.Right) + offset; // Get a random point in the bounding box
            double YCenter = Utilities.RandomGenerator.Next((int)rect.Top, (int)rect.Bottom) + offset;
            System.Windows.Point pCenter = new System.Windows.Point(XCenter, YCenter);
            if (true == geometry.FillContains(pCenter))
            {
               System.Windows.Point p1 = new System.Windows.Point(XCenter - offset, YCenter - offset);
               System.Windows.Point p2 = new System.Windows.Point(XCenter + offset, YCenter - offset);
               System.Windows.Point p3 = new System.Windows.Point(XCenter - offset, YCenter + offset);
               System.Windows.Point p4 = new System.Windows.Point(XCenter + offset, YCenter + offset);
               bool isP1In = geometry.FillContains(p1);
               bool isP2In = geometry.FillContains(p2);
               bool isP3In = geometry.FillContains(p3);
               bool isP4In = geometry.FillContains(p4);
               if (false == isP1In && false == isP2In)  // try to adjust location so that four corners are inside the region
               {
                  YCenter += offset;
               }
               else if (false == isP3In && false == isP4In)
               {
                  YCenter -= offset;
               }
               else if (false == isP1In && false == isP3In)
               {
                  XCenter += offset;
               }
               else if (false == isP2In && false == isP4In)
               {
                  XCenter -= offset;
               }
               else if (false == isP1In && true == isP2In)
               {
                  XCenter += offset;
               }
               else if (true == isP1In && false == isP2In)
               {
                  XCenter -= offset;
               }
               else if (true == isP3In && false == isP4In)
               {
                  YCenter -= offset;
               }
               else if (false == isP3In && true == isP4In)
               {
                  YCenter -= offset;
               }
               System.Windows.Point p5 = new System.Windows.Point(XCenter - offset, YCenter - offset); // do a final check to make sure center point is in region
               if (true == geometry.FillContains(p5))
                  return new MapPoint(p5.X, p5.Y);
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "GetRandomPoint(): Cannot find a random point in t.Name=" + t.Name + " rect=" + rect.ToString());
         return new MapPoint(t.CenterPoint.X - offset, t.CenterPoint.Y - offset);
      }
      //---------------------------------------------------------------
      public Territory()
      {

      }
      public Territory(string name) { Name = name; }
      public override string ToString()
      {
         return Name;
      }
      public ITerritory Find(List<ITerritory> territories, string name)
      {
         IEnumerable<ITerritory> results = from territory in territories
                                           where territory.Name == name
                                           select territory;
         if (0 < results.Count())
            return results.First();
         else
            throw new Exception("Territory.Find(): Unknown Territory=" + name);
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
         foreach (object o in myList)
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
         foreach (object o in myList)
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
         foreach (object o in myList)
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
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[ ");
         foreach (object o in myList)
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
      public static ITerritory? Find(this IList<ITerritory> territories, string name)
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
