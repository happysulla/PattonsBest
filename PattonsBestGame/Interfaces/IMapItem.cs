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
      double RotationTurretOld { get; set; } // to track how much the turret changes before firing - can only be 0, 60, 120, 180, 240, 300 degrees
      bool IsTurretRotated { get; set; } // For Sherman. Turret rotated without firing main gun causes target acquisition = 0
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
      bool IsInterdicted { get; set; }
      Dictionary<string, int> EnemyAcquiredShots { set; get; } // Enemies that have acquired on this MapItem <string=Firer, int=number of shots>
      //----------------------------------------
      bool IsVehicle { get; set; }
      bool IsAntiTankGun { get; set; }
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
      void Sync(IMapItem mi); // synchronize most of the data but not all
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
      void Add(IMapItem mi);
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
      string Wound { get; set; }
      int WoundDaysUntilReturn { get; set; }
   }
   //==========================================
   public interface ICrewMembers : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(ICrewMember cm);
      void Insert(int index, ICrewMember cm);
      void Clear();
      bool Contains(ICrewMember cm);
      int IndexOf(ICrewMember cm);
      void Remove(ICrewMember cmName);
      void Reverse();
      ICrewMember? Remove(string cmName);
      ICrewMember? RemoveAt(int index);
      ICrewMember? Find(string cmName);
      ICrewMember? this[int index] { get; set; }

   }
}
