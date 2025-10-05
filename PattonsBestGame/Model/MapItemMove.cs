using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using Point = System.Windows.Point;

namespace Pattons_Best
{
   [Serializable]
   public class MapItemMove : IMapItemMove
   {
      public bool CtorError { get; } = false;
      public IMapItem MapItem { set; get; } = new MapItem();       // Represents the map item that is being moved
      public ITerritory? OldTerritory { set; get; } = null; // Represents the old territory that the MapItem is being moved from.
      public ITerritory? NewTerritory { set; get; } = null; // Represents the new territory that the MapItem is being moved to.
      public IMapPath? BestPath { set; get; } = null;
      //------------------------------------------------------------------------------
      public MapItemMove() // Default Contructor
      {
      }
      public MapItemMove(IMapItemMove mim) // Copy Contructor
      {
         MapItem = mim.MapItem;
         OldTerritory = mim.OldTerritory;
         NewTerritory = mim.NewTerritory;
         BestPath = mim.BestPath;
      }
      public MapItemMove(ITerritories territories, IMapItem movingMapItem, ITerritory newTerritory) 
      {
         MapItem = movingMapItem;
         OldTerritory = movingMapItem.TerritoryCurrent;
         if (null == OldTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemMove(): OldTerritory=null");
            CtorError = true;
            return;
         }
         if (null == newTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemMove(): newTerritory=null");
            CtorError = true;
            return;
         }
         BestPath = Territory.GetBestPath(territories, OldTerritory, newTerritory, 100);
         if (null == BestPath)
         {
            string msg = "MapItemMove():BestPath=null for";
            msg += MapItem.ToString();
            msg += " from ";
            msg += OldTerritory.Name;
            Logger.Log(LogEnum.LE_ERROR, "MapItemMove(): Not able to find best path");
            CtorError = true;
            return;
         }
         // Remove last territory if exceeds stacking limit.
         int countOfTerritories = BestPath.Territories.Count;
         NewTerritory = BestPath.Territories[countOfTerritories - 1];
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder("");
         sb.Append("mi=");
         sb.Append(MapItem.Name);
         sb.Append(",oT=");
         if (null == OldTerritory)
            sb.Append("null");
         else
            sb.Append(OldTerritory.Name);
         sb.Append(",nT=");
         if (null == NewTerritory)
            sb.Append("null");
         else
            sb.Append(NewTerritory.Name);
         return sb.ToString();
      }
   }
   //-------------------------------------------------------
   [Serializable]
   public class MapItemMoves : IMapItemMoves
   {
      private readonly ArrayList myList;
      public MapItemMoves() { myList = new ArrayList(); }
      public void Add(IMapItemMove mim) { myList.Add(mim); }
      public void Insert(int index, IMapItemMove mim) { myList.Insert(index, mim); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMapItemMove mim) { return myList.Contains(mim); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMapItemMove mim) { return myList.IndexOf(mim); }
      public void Remove(IMapItemMove mim) { myList.Remove(mim); }
      public IMapItemMove? Find(IMapItem mi)
      {
         foreach (object o in myList)
         {
            IMapItemMove mim = (IMapItemMove)o;
            if (mi.Name == mim.MapItem.Name)
               return mim;
         }
         return null;
      }
      public IMapItemMove? Remove(IMapItem mi)
      {
         foreach (object o in myList)
         {
            IMapItemMove mim = (IMapItemMove)o;
            if (mi.Name == mim.MapItem.Name)
            {
               myList.Remove(mim);
               return mim;
            }
         }
         return null;
      }
      public IMapItemMove? RemoveAt(int index)
      {
         IMapItemMove? mim = myList[index] as IMapItemMove;
         if (null == mim)
            return null;
         myList.RemoveAt(index);
         return mim;
      }
      public IMapItemMove? this[int index]
      {
         get
         {
            IMapItemMove? mim = myList[index] as IMapItemMove;
            return mim;
         }
         set { myList[index] = value; }
      }
      public IMapItemMoves Shuffle()
      {
         IMapItemMoves newOrder = new MapItemMoves();
         // Random select card in myCards list and
         // remove it.  Then add it to new list. 
         int count = myList.Count;
         for (int i = 0; i < count; i++)
         {
            int index = Utilities.RandomGenerator.Next(myList.Count);
            if (index < myList.Count)
            {
               IMapItemMove? randomMim = myList[index] as IMapItemMove;
               myList.RemoveAt(index);
               if (null != randomMim)
                  newOrder.Add(randomMim);
            }
         }
         return newOrder;
      }

   }
}
