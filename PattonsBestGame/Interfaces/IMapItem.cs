﻿using System;
using System.Collections.Generic;
using System.Windows.Controls;

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
      List<BloodSpot> PoisonSpots { get; }
      double Zoom { get; set; }
      bool IsHidden { get; set; }
      bool IsAnimated { get; set; }
      IMapPoint Location { get; set; }
      //----------------------------------------
      int MovementUsed { get; set; }
      //----------------------------------------
      ITerritory Territory { get; set; }
      ITerritory TerritoryStarting { get; set; }
      //----------------------------------------
      void SetLocation(int counterCount);
      void Reset();
      void ResetPartial();
      void Flip();
      void Unflip();
   }
   public interface IMapItems : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IMapItem mi);
      IMapItem RemoveAt(int index);
      void Insert(int index, IMapItem mi);
      void Clear();
      bool Contains(IMapItem mi);
      int IndexOf(IMapItem mi);
      void Remove(IMapItem miName);
      void Reverse();
      IMapItem Remove(string miName);
      IMapItem Find(string miName);
      IMapItem this[int index] { get; set; }
      IMapItems Shuffle();
      void Rotate(int numOfRotates);
   }
}