﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pattons_Best
{
   [Serializable]
   public class Territory : ITerritory
   {
      [NonSerialized] static public ITerritories theTerritories = new Territories();
      public string Name { get; set; } = "Offboard";
      public List<String> Adjacents { get; set; } = new List<String>();
      public IMapPoint CenterPoint { get; set; } = new MapPoint();
      public List<IMapPoint> Points { get; set; } = new List<IMapPoint>();
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
