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
      bool IsAnimated { get; set; }
      bool IsMoved { get; set; }               // If Sherman moved, it cannot fire unless it has HVSS
      int Count { get; set; }
      IMapPoint Location { get; set; }       // top left corner of MapItem
      double RotationOffsetHull { get; set; }
      double RotationHull { get; set; }      // rotation to point at location
      double RotationOffsetTurret { get; set; }
      double RotationTurret { get; set; }
      //----------------------------------------
      ITerritory TerritoryCurrent { get; set; }
      ITerritory TerritoryStarting { get; set; }
      //----------------------------------------
      bool IsMoving { get; set; }
      bool IsHullDown { get; set; }
      bool IsTurret { get; set; }
      bool IsKilled { get; set; }
      bool IsUnconscious { get; set; }
      bool IsIncapacitated { get; set; }
      bool IsFired { get; set; }
      bool IsSpotted { get; set; }
      //----------------------------------------
      bool IsVehicle { get; set; }
      bool IsMovingInOpen { get; set; }
      bool IsWoods { get; set; }
      bool IsBuilding { get; set; }
      bool IsFortification { get; set; }
      bool IsThrownTrack { get; set; }
      bool IsBoggedDown { get; set; }
      bool IsAssistanceNeeded { get; set; }
      //----------------------------------------
      bool IsHeHit { get; set; }
      bool IsApHit { get; set; } 
      EnumSpottingResult Spotting { get; set; }
      //----------------------------------------
      void Copy(IMapItem mi);
      bool IsEnemyUnit();
      string GetEnemyUnit();
      void SetBloodSpots(int percent = 40);
      bool SetMapItemRotation(IMapItem target);
      bool SetMapItemRotationTurret(IMapItem target);
      bool UpdateMapRotation(string facing);
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
      int Sector { get; set; }
      string Action { get; set; }
      string Wound { get; set; }
      int WoundDaysUntilReturn { get; set; }
      ICrewMember Clone();
   }
}
