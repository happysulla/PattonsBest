using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Xps.Serialization;

namespace Pattons_Best
{
   public interface IMapItem
   {
      //----------------------------------------
      // Basic Properties
      string Name { get; set; }
      string TopImageName { get; set; }
      string BottomImageName { get; set; }
      string OverlayImageName { get; set; }
      List<BloodSpot> WoundSpots { get; }
      double Zoom { get; set; }
      bool IsHidden { get; set; }
      bool IsAnimated { get; set; }
      bool IsMoved { get; set; }
      int Count { get; set; }
      IMapPoint Location { get; set; }
      //----------------------------------------
      ITerritory TerritoryCurrent { get; set; }
      ITerritory TerritoryStarting { get; set; }
      //----------------------------------------
      void SetLocation(int counterCount);
      void Flip();
      void Unflip();
   }
   //==========================================
   public interface IMapItems : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IMapItem? mi);
      void Insert(int index, IMapItem mi);
      void Clear();
      bool Contains(IMapItem mi);
      int IndexOf(IMapItem mi);
      void Remove(IMapItem miName);
      void Reverse();
      IMapItem? Remove(string miName);
      IMapItem? RemoveAt(int index);
      IMapItem? Find(string miName);
      IMapItem? this[int index] { get; set; }
      IMapItems Shuffle();
      void Rotate(int numOfRotates);
   }
   //==========================================
   public interface ICrewMember : IMapItem
   {
      string Role { get; set; }
      string Rank { get; set; }
      int Rating { get; set; }
      bool IsButtonedUp { get; set; }
   }
}
