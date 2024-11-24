using System;
using System.Collections.Generic;

namespace Pattons_Best
{
   public interface ITerritory
   {
      string Name { get; set; }
      string Type { get; set; }
      List<IMapPoint> Points { set; get; }
      IMapPoint CenterPoint { get; set; }
      List<String> Adjacents { get; }
   }
   //--------------------------------------------------------
   public interface ITerritories : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(ITerritory t);
      ITerritory RemoveAt(int index);
      void Insert(int index, ITerritory t);
      void Clear();
      bool Contains(ITerritory t);
      int IndexOf(ITerritory t);
      void Remove(ITerritory tName);
      ITerritory Remove(string tName);
      ITerritory Find(string tName);
      ITerritory this[int index] { get; set; }
   }
}
