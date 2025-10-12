using System.Collections.Generic;

namespace Pattons_Best
{
   struct MapPathCount
   {
      public int myCount;
      public ITerritory myTerritory;
      public MapPathCount(int count, ITerritory territory)
      {
         myCount = count;
         myTerritory = territory;   
      }
   }
   public interface IMapPath
   {
      string Name { get; set; }
      double Metric { get; set; }
      List<ITerritory> Territories { get; }
   }
   //---------------------------------------
   public interface IMapPaths : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IMapPath path);
      void Insert(int index, IMapPath path);
      void Clear();
      bool Contains(IMapPath path);
      int IndexOf(IMapPath path);
      void Remove(IMapPath pathName);
      IMapPath? Find(string pathName);
      IMapPath? Find(IMapPath pathToMatch);
      IMapPath? Remove(string pathName);
      IMapPath? RemoveAt(int index);
      IMapPath? this[int index] { get; set; }
   }
}
