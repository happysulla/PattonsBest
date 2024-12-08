﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Pattons_Best
{
   [Serializable]
   class MapPath : IMapPath
   {
      public string Name { get; set; } = string.Empty;
      private double myMetric = 0;
      public double Metric { get; set; }
      private List<ITerritory> myTerritories = new List<ITerritory>();
      public List<ITerritory> Territories { get=>myTerritories; }
      //-----------------------------------------------------------
      public MapPath()
      {
      }
      public MapPath(String pathName)
      {
         Name = pathName;
      }
      public MapPath(IMapPath path)
      {
         Name = path.Name;
         myMetric = path.Metric;
         foreach (ITerritory t in path.Territories)
            myTerritories.Add(t);
      }
      //-----------------------------------------------------------
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append(this.Name);
         sb.Append("(");
         sb.Append(this.Metric.ToString());
         sb.Append(") PATH=");
         int count = 0;
         foreach (ITerritory t in Territories)
         {
            sb.Append(t.ToString());
            if (++count < Territories.Count)
               sb.Append("->");
         }
         return sb.ToString();
      }
   }
   //---------------------------------------------------------------------------
   [Serializable]
   public class MapPaths : IEnumerable, IMapPaths
   {
      private ArrayList myList;
      public MapPaths() { myList = new ArrayList(); }
      public void Add(IMapPath path) { myList.Add(path); }

      public void Insert(int index, IMapPath path) { myList.Insert(index, path); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMapPath path) { return myList.Contains(path); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMapPath path) { return myList.IndexOf(path); }
      public void Remove(IMapPath path) { myList.Remove(path); }
      public IMapPath? Find(IMapPath pathToMatch)
      {
         foreach (Object o in myList)
         {
            IMapPath? path = o as IMapPath;
            if( null == path )
            {
               Logger.Log(LogEnum.LE_ERROR, "MapPath.Find(pathToMatch): path=null");
               return null;
            }
            if (path.Name == pathToMatch.Name)
               return path;
         }
         return null;
      }
      public IMapPath? Find(string pathName)
      {
         foreach (Object o in myList)
         {
            IMapPath? path = (IMapPath)o;
            if (null == path)
            {
               Logger.Log(LogEnum.LE_ERROR, "MapPath.Find(pathToMatch): path=null");
               return null;
            }
            if (path.Name == pathName)
               return path;
         }
         return null;
      }
      public IMapPath? Remove(string pathName)
      {
         foreach (Object o in myList)
         {
            IMapPath path = (IMapPath)o;
            if (path.Name == pathName)
            {
               Remove(path);
               return path;
            }
         }
         return null;
      }
      public IMapPath? RemoveAt(int index)
      {
         IMapPath? path = myList[index] as IMapPath;
         if (null == path)
            return null;
         myList.RemoveAt(index);
         return path;
      }
      public IMapPath? this[int index]
      {
         get 
         { 
            IMapPath? mp = myList[index] as IMapPath;
            return mp; 
         }
         set { myList[index] = value; }
      }
   }
}
