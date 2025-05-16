using System;
using System.Collections;
using System.Text;

namespace Pattons_Best
{
   //-------------------------------------------------------------------
   [Serializable]
   public class Stack : IStack
   {
      public ITerritory Territory { get; set; }
      public IMapItems MapItems { get; set; } = new MapItems();
      public bool IsStacked { get; set; } = false;
      public Stack(ITerritory t)
      {
         Territory = t;
      }
      public Stack(ITerritory t, IMapItem mi)
      {
         Territory = t;
         mi.TerritoryCurrent = t;
         MapItems.Add(mi);
      }
      public void Rotate() { MapItems.Rotate(1); }
      public void Shuffle() { MapItems = MapItems.Shuffle(); }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder("");
         sb.Append(Territory.ToString());
         sb.Append("==>(");
         int count = MapItems.Count;
         for (int i = 0; i < count; ++i)
         {
            IMapItem? mi = MapItems[i];
            if (null != mi)
            {
               sb.Append(mi.Name);
               if (i != MapItems.Count - 1)
                  sb.Append(" ");
            }
         }
         sb.Append(")");
         return sb.ToString();
      }
   }
   //-------------------------------------------------------------------
   [Serializable]
   public class Stacks : IStacks
   {
      private readonly ArrayList myList;
      public Stacks() { myList = new ArrayList(); }
      public void Add(IStack stack) { myList.Add(stack); }
      public void Add(IMapItem mi)
      {
         IStack? stack = Find(mi.TerritoryCurrent);
         if (null == stack)
         {
            stack = new Stack(mi.TerritoryCurrent, mi);
            myList.Add(stack);
         }
         else // add to top of stack
         {
            stack.MapItems.Add(mi);
         }
      }
      public void Insert(int index, IStack stack) { myList.Insert(index, stack); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(IStack stack) { return myList.Contains(stack); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IStack stack) { return myList.IndexOf(stack); }
      public void Remove(IStack stack) { myList.Remove(stack); }
      public IStack? Find(ITerritory t)
      {
         foreach (object o in myList)
         {
            IStack stack = (IStack)o;
            if (t.Name == stack.Territory.Name)
               return stack;
         }
         return null;
      }
      public IStack? Find(IMapItem mi)
      {
         foreach (object o in myList)
         {
            IStack stack = (IStack)o;
            foreach (MapItem mapItem in stack.MapItems)
            {
               if (mi.Name == mapItem.Name)
                  return stack;
            }
         }
         return null;
      }
      public IStack? Find(string territoryName)
      {
         foreach (object o in myList)
         {
            IStack stack = (IStack)o;
            if (territoryName == stack.Territory.Name)
               return stack;
         }
         return null;
      }
      public IMapItem? FindMapItem(string name)
      {
         foreach (object o in myList)
         {
            IStack stack = (IStack)o;
            foreach (MapItem mapItem in stack.MapItems)
            {
               if (mapItem.Name == name)
                  return mapItem;
            }
         }
         return null;
      }
      public void Remove(IMapItem mi)
      {
         foreach (object o in myList)
         {
            IStack stack = (IStack)o;
            bool isMapItemRemoved = false;
            foreach (MapItem mapItem in stack.MapItems)
            {

               if (mi.Name == mapItem.Name)
               {
                  isMapItemRemoved = true;
                  stack.MapItems.Remove(mapItem);
                  if (0 == stack.MapItems.Count)
                  {
                     Remove(stack);
                     return;
                  }
               }
               if (true == isMapItemRemoved) // future mapitems move up in stack
               {
                  mapItem.Location.X += Utilities.STACK;
                  mapItem.Location.Y += Utilities.STACK;
               }
            }
         }
      }
      public void Remove(string miName)
      {
         foreach (object o in myList)
         {
            IStack stack = (IStack)o;
            bool isMapItemRemoved = false;
            foreach (MapItem mapItem in stack.MapItems)
            {
               if (miName == mapItem.Name)
               {
                  isMapItemRemoved = true;
                  stack.MapItems.Remove(mapItem);
                  if (0 == stack.MapItems.Count)
                     Remove(stack);
                  return;
               }
               if (true == isMapItemRemoved) // future mapitems move up in stack
               {
                  mapItem.Location.X += Utilities.STACK;
                  mapItem.Location.Y += Utilities.STACK;
               }
            }
         }
      }
      public IStack? RemoveAt(int index)
      {
         IStack? stack = myList[index] as IStack;
         if (stack == null) return null;
         myList.RemoveAt(index);
         return stack;
      }
      public IStack? this[int index]
      {
         get { IStack? s = myList[index] as IStack; return s; }
         set { myList[index] = value; }
      }
      public IStacks Shuffle()
      {
         IStacks newStacks = new Stacks();
         int count = myList.Count + 100;
         for (int i = 0; i < count; i++) // Randomly select object. Remove it and readd at top.
         {
            int index = Utilities.RandomGenerator.Next(myList.Count);
            if (index < myList.Count)
            {
               IStack? stack = myList[index] as IStack;
               myList.RemoveAt(index);
               if (null == stack)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Shuffle(): stack=null");
               }
               else
               {
                  stack.Shuffle();
                  newStacks.Add(stack);
               }
            }
         }
         return newStacks;
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder("stacks[");
         sb.Append(Count.ToString());
         sb.Append("]=");
         foreach (IStack stack in this)
         {
            sb.Append("{");
            sb.Append(stack.ToString());
            sb.Append("}");
         }
         return sb.ToString();
      }
   }
}
