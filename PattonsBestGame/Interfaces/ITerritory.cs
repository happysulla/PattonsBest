using System;
using System.Collections.Generic;
namespace Pattons_Best
{
   public interface ITerritory
   {
      string Name { get; set; }
      string CanvasName { get; set; }
      string Type { get; set; }
      IMapPoint CenterPoint { get; set; }
      List<IMapPoint> Points { set; get; }
      List<String> Adjacents { get; }
   }
   //--------------------------------------------------------
   public interface ITerritories : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(ITerritory t);
      void Insert(int index, ITerritory t);
      void Clear();
      bool Contains(ITerritory t);
      int IndexOf(ITerritory t);
      void Remove(ITerritory tName);
      ITerritory? Find(string tName);
      ITerritory? Find(string tName, string tType);
      ITerritory? Remove(string tName);
      ITerritory? RemoveAt(int index);
      ITerritory? this[int index] { get; set; }
   }
}
