using System;

namespace Pattons_Best
{
   public interface IStack
   {
      ITerritory Territory { get; set; }
      IMapItems MapItems { get; set; }
      void Rotate();
      void Shuffle();
   }
   public interface IStacks : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IStack stack);

      void Insert(int index, IStack stack);
      void Clear();
      bool Contains(IStack stack);
      int IndexOf(IStack stack);
      void Remove(IStack stack);
      IStack? Find(ITerritory t);
      IStack? Find(IMapItem mi);
      IStack? Find(String name);
      void Remove(IMapItem mi);
      void Remove(string miName);
      IStack? RemoveAt(int index);
      IStack? this[int index] { get; set; }
      IStacks Shuffle();
   }
}
