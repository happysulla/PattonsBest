using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Windows.Media;
using Windows.Perception.Spatial;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Diagnostics;

namespace Pattons_Best
{
   [Serializable]
   public class Territory : ITerritory
   {
      public string Name { get; set; } = "Offboard";
      public string CanvasName { get; set; } = "Main";
      public string Type { get; set; } = "ERROR";
      public IMapPoint CenterPoint { get; set; } = new MapPoint();
      public List<IMapPoint> Points { get; set; } = new List<IMapPoint>();
      public List<string> Adjacents { get; set; } = new List<string>();
      //---------------------------------------------------------------
      public static IMapPoint GetRandomPoint(ITerritory t, double offset) // return the top left location of a MapItem, not the center point
      {
         if (0 == t.Points.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetRandomPoint(): t.Points.Count=0 for t.Name=" + t.Name);
            return t.CenterPoint;
         }
         //----------------------------------------------------
         // Make a StreamGeometry object from t.Points 
         StreamGeometry geometry = new StreamGeometry();
         using (StreamGeometryContext ctx = geometry.Open())
         {
            IMapPoint mp0 = t.Points[0];
            System.Windows.Point point0 = new System.Windows.Point(mp0.X, mp0.Y);
            ctx.BeginFigure(point0, true, true); //  filled and closed
            for (int i = 1; i < t.Points.Count; ++i)
            {
               IMapPoint mpI = t.Points[i];
               System.Windows.Point pointI = new System.Windows.Point(mpI.X, mpI.Y);
               ctx.LineTo(pointI, true, false);
            }
            geometry.Freeze();
         }
         System.Windows.Rect rect = geometry.Bounds;
         //----------------------------------------------------
         int count = 20;
         while (0 < --count) // offset is the difference between MapItem location on screen and the center of the  MapItem.
         {
            double XCenter = Utilities.RandomGenerator.Next((int)rect.Left, (int)rect.Right) + offset; // Get a random point in the bounding box
            double YCenter = Utilities.RandomGenerator.Next((int)rect.Top, (int)rect.Bottom) + offset;
            System.Windows.Point pCenter = new System.Windows.Point(XCenter, YCenter);
            if (true == geometry.FillContains(pCenter))
            {
               System.Windows.Point p1 = new System.Windows.Point(XCenter - offset, YCenter - offset);
               System.Windows.Point p2 = new System.Windows.Point(XCenter + offset, YCenter - offset);
               System.Windows.Point p3 = new System.Windows.Point(XCenter - offset, YCenter + offset);
               System.Windows.Point p4 = new System.Windows.Point(XCenter + offset, YCenter + offset);
               bool isP1In = geometry.FillContains(p1);
               bool isP2In = geometry.FillContains(p2);
               bool isP3In = geometry.FillContains(p3);
               bool isP4In = geometry.FillContains(p4);
               if (false == isP1In && false == isP2In)  // try to adjust location so that four corners are inside the region
               {
                  YCenter += offset;
               }
               else if (false == isP3In && false == isP4In)
               {
                  YCenter -= offset;
               }
               else if (false == isP1In && false == isP3In)
               {
                  XCenter += offset;
               }
               else if (false == isP2In && false == isP4In)
               {
                  XCenter -= offset;
               }
               else if (false == isP1In && true == isP2In)
               {
                  XCenter += offset;
               }
               else if (true == isP1In && false == isP2In)
               {
                  XCenter -= offset;
               }
               else if (true == isP3In && false == isP4In)
               {
                  YCenter -= offset;
               }
               else if (false == isP3In && true == isP4In)
               {
                  YCenter -= offset;
               }
               System.Windows.Point p5 = new System.Windows.Point(XCenter - offset, YCenter - offset); // do a final check to make sure point is in region
               if (true == geometry.FillContains(p5))
                  return new MapPoint(p5.X, p5.Y);
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "GetRandomPoint(): Cannot find a random point in t.Name=" + t.Name + " rect=" + rect.ToString());
         return new MapPoint(t.CenterPoint.X - offset, t.CenterPoint.Y - offset);
      }
      public static int GetSmokeCount(IGameInstance gi, char sector, char range)
      {
         int numSmokeMarkers = 0;
         IStack? stack = gi.BattleStacks.Find(gi.Home);
         if (null == stack)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetSmokeCount():  stack=null for " + gi.Home.Name);
            return -10000;
         }
         foreach (IMapItem smoke in stack.MapItems)
         {
            if (true == smoke.Name.Contains("Smoke"))
               numSmokeMarkers++;
         }
         stack = gi.BattleStacks.Find("B" + sector + "C");
         if (null != stack)
         {
            foreach (IMapItem smoke in stack.MapItems)
            {
               if (true == smoke.Name.Contains("Smoke"))
                  numSmokeMarkers++;
            }
         }
         if (('M' == range) || ('L' == range))
         {
            stack = gi.BattleStacks.Find("B" + sector + "M");
            if (null != stack)
            {
               foreach (IMapItem smoke in stack.MapItems)
               {
                  if (true == smoke.Name.Contains("Smoke"))
                     numSmokeMarkers++;
               }
            }
         }
         if ('L' == range)
         {
            string tName = "B" + sector + "L";
            stack = gi.BattleStacks.Find(tName);
            if (null != stack)
            {
               foreach (IMapItem smoke in stack.MapItems)
               {
                  if (true == smoke.Name.Contains("Smoke"))
                     numSmokeMarkers++;
               }
            }
         }
         return numSmokeMarkers;
      }
      public static List<String>? GetSpottedTerritories(IGameInstance gi, ICrewMember cm)
      {
         List<string> spottedTerritories = new List<string>();
         //---------------------------------------------------
         if( (true == cm.IsIncapacitated) || (true == cm.IsKilled)) // GetSpottedTerritories() - return nothing if incapacitated
            return spottedTerritories;
         //---------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetSpottingResult(): lastReport=null");
            return null;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //---------------------------------------------------
         bool isCloseRangeOnly = false;
         if ((true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")))
            isCloseRangeOnly = true;
         switch (cm.Role)
         {
            case "Commander":
               if ( (true == cm.IsButtonedUp) && (false == card.myIsVisionCupola) ) // any one sector
               {
                  foreach (IStack stack in gi.BattleStacks) // only view one sector already chosen in battle prep
                  {
                     foreach (IMapItem mi in stack.MapItems)
                     {
                        if (true == mi.Name.Contains("CommanderSpot"))
                        {
                           string tName = mi.TerritoryCurrent.Name;
                           if (6 != tName.Length)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "GetSpottedTerritories(): tName != 6 for " + mi.TerritoryCurrent.Name);
                              return null;
                           }
                           char sector = tName[tName.Length - 1];
                           spottedTerritories.Add("B" + sector + "C");
                           if (false == isCloseRangeOnly)
                           {
                              spottedTerritories.Add("B" + sector + "M");
                              spottedTerritories.Add("B" + sector + "L");
                           }
                        }
                     }
                  }
               }
               else // all sectors
               {
                  if ((14 == lastReport.TankCardNum) || (16 == lastReport.TankCardNum)) // split hatch with vision cupola - split hatch excludes left rear
                  {
                     switch (gi.Sherman.RotationHull)
                     {
                        case 0:
                           spottedTerritories.Add("B1C");
                           spottedTerritories.Add("B2C");
                           spottedTerritories.Add("B4C");
                           spottedTerritories.Add("B6C");
                           spottedTerritories.Add("B9C");
                           if (false == isCloseRangeOnly)
                           {
                              spottedTerritories.Add("B1M");
                              spottedTerritories.Add("B2M");
                              spottedTerritories.Add("B4M");
                              spottedTerritories.Add("B6M");
                              spottedTerritories.Add("B9M");
                              spottedTerritories.Add("B1L");
                              spottedTerritories.Add("B2L");
                              spottedTerritories.Add("B4L");
                              spottedTerritories.Add("B6L");
                              spottedTerritories.Add("B9L");
                           }
                           break;
                        case 60:
                           spottedTerritories.Add("B1C");
                           spottedTerritories.Add("B2C");
                           spottedTerritories.Add("B3C");
                           spottedTerritories.Add("B6C");
                           spottedTerritories.Add("B9C");
                           if (false == isCloseRangeOnly)
                           {
                              spottedTerritories.Add("B1M");
                              spottedTerritories.Add("B2M");
                              spottedTerritories.Add("B3M");
                              spottedTerritories.Add("B6M");
                              spottedTerritories.Add("B9M");
                              spottedTerritories.Add("B1L");
                              spottedTerritories.Add("B2L");
                              spottedTerritories.Add("B3L");
                              spottedTerritories.Add("B6L");
                              spottedTerritories.Add("B9L");
                           }
                           break;
                        case 120:
                           spottedTerritories.Add("B1C");
                           spottedTerritories.Add("B2C");
                           spottedTerritories.Add("B3C");
                           spottedTerritories.Add("B4C");
                           spottedTerritories.Add("B9C");
                           if (false == isCloseRangeOnly)
                           {
                              spottedTerritories.Add("B1M");
                              spottedTerritories.Add("B2M");
                              spottedTerritories.Add("B3M");
                              spottedTerritories.Add("B4M");
                              spottedTerritories.Add("B9M");
                              spottedTerritories.Add("B1L");
                              spottedTerritories.Add("B2L");
                              spottedTerritories.Add("B3L");
                              spottedTerritories.Add("B4L");
                              spottedTerritories.Add("B9L");
                           }
                           break;
                        case 180:
                           spottedTerritories.Add("B1C");
                           spottedTerritories.Add("B2C");
                           spottedTerritories.Add("B3C");
                           spottedTerritories.Add("B4C");
                           spottedTerritories.Add("B6C");
                           if (false == isCloseRangeOnly)
                           {
                              spottedTerritories.Add("B1M");
                              spottedTerritories.Add("B2M");
                              spottedTerritories.Add("B3M");
                              spottedTerritories.Add("B4M");
                              spottedTerritories.Add("B6M");
                              spottedTerritories.Add("B1L");
                              spottedTerritories.Add("B2L");
                              spottedTerritories.Add("B3L");
                              spottedTerritories.Add("B4L");
                              spottedTerritories.Add("B6L");
                           }
                           break;
                        case 240:
                           spottedTerritories.Add("B2C");
                           spottedTerritories.Add("B3C");
                           spottedTerritories.Add("B4C");
                           spottedTerritories.Add("B6C");
                           spottedTerritories.Add("B9C");
                           if (false == isCloseRangeOnly)
                           {
                              spottedTerritories.Add("B2M");
                              spottedTerritories.Add("B3M");
                              spottedTerritories.Add("B4M");
                              spottedTerritories.Add("B6M");
                              spottedTerritories.Add("B9M");
                              spottedTerritories.Add("B2L");
                              spottedTerritories.Add("B3L");
                              spottedTerritories.Add("B4L");
                              spottedTerritories.Add("B6L");
                              spottedTerritories.Add("B9L");
                           }
                           break;
                        case 300:
                           spottedTerritories.Add("B1C");
                           spottedTerritories.Add("B3C");
                           spottedTerritories.Add("B4C");
                           spottedTerritories.Add("B6C");
                           spottedTerritories.Add("B9C");
                           if (false == isCloseRangeOnly)
                           {
                              spottedTerritories.Add("B1M");
                              spottedTerritories.Add("B3M");
                              spottedTerritories.Add("B4M");
                              spottedTerritories.Add("B6M");
                              spottedTerritories.Add("B9M");
                              spottedTerritories.Add("B1L");
                              spottedTerritories.Add("B3L");
                              spottedTerritories.Add("B4L");
                              spottedTerritories.Add("B6L");
                              spottedTerritories.Add("B9L");
                           }
                           break;
                        default:
                           Logger.Log(LogEnum.LE_ERROR, "GetSpottedTerritories(): 2-reached default for RotationHull=" + gi.Sherman.RotationHull.ToString());
                           return null;
                     }

                  }
                  else // split hatch excludes left rear
                  {
                     GetAllTerritories(ref spottedTerritories, isCloseRangeOnly);
                  }
               }
               break;
            case "Gunner":
               double rotation = gi.Sherman.RotationHull + gi.Sherman.RotationTurret;
               if (359 < rotation)
                  rotation -= 360.0;
               switch (rotation)
               {
                  case 0:
                     spottedTerritories.Add("B6C");
                     if (false == isCloseRangeOnly)
                     {
                        spottedTerritories.Add("B6M");
                        spottedTerritories.Add("B6L");
                     }
                     break;
                  case 60:
                     spottedTerritories.Add("B9C");
                     if (false == isCloseRangeOnly)
                     {
                        spottedTerritories.Add("B9M");
                        spottedTerritories.Add("B9L");
                     }
                     break;
                  case 120:
                     spottedTerritories.Add("B1C");
                     if (false == isCloseRangeOnly)
                     {
                        spottedTerritories.Add("B1M");
                        spottedTerritories.Add("B1L");
                     }
                     break;
                  case 180:
                     spottedTerritories.Add("B2C");
                     if (false == isCloseRangeOnly)
                     {
                        spottedTerritories.Add("B2M");
                        spottedTerritories.Add("B2L");
                     }
                     break;
                  case 240:
                     spottedTerritories.Add("B3C");
                     if (false == isCloseRangeOnly)
                     {
                        spottedTerritories.Add("B3M");
                        spottedTerritories.Add("B3L");
                     }
                     break;
                  case 300:
                     spottedTerritories.Add("B4C");
                     if (false == isCloseRangeOnly)
                     {
                        spottedTerritories.Add("B4M");
                        spottedTerritories.Add("B4L");
                     }
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "GetSpottedTerritories(): 1-reached default for rotation=" + rotation.ToString());
                     return null;
               }
               break;
            case "Loader":
               if(true == cm.IsButtonedUp)
               {
                  foreach (IStack stack in gi.BattleStacks) // any one sector already selected
                  {
                     foreach (IMapItem mi in stack.MapItems)
                     {
                        if (true == mi.Name.Contains("LoaderSpot"))
                        {
                           string tName = mi.TerritoryCurrent.Name;
                           if (5 != tName.Length)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "GetSpottedTerritories(): tName != 5 for " + mi.TerritoryCurrent.Name);
                              return null;
                           }
                           char sector = tName[tName.Length - 1];
                           spottedTerritories.Add("B" + sector + "C");
                           if (false == isCloseRangeOnly)
                           {
                              spottedTerritories.Add("B" + sector + "M");
                              spottedTerritories.Add("B" + sector + "L");
                           }
                        }
                     }
                  }
               }
               else
               {
                  GetAllTerritories(ref spottedTerritories, isCloseRangeOnly);
               }
               break;
            case "Driver":
            case "Assistant":
               if (true == cm.IsButtonedUp) // Tank Front Only 
               {
                  switch (gi.Sherman.RotationHull)
                  {
                     case 0:
                        spottedTerritories.Add("B6C");
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B6M");
                           spottedTerritories.Add("B6L");
                        }
                        break;
                     case 60:
                        spottedTerritories.Add("B9C");
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B9M");
                           spottedTerritories.Add("B9L");
                        }
                        break;
                     case 120:
                        spottedTerritories.Add("B1C");
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B1M");
                           spottedTerritories.Add("B1L");
                        }
                        break;
                     case 180:
                        spottedTerritories.Add("B2C");
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B2M");
                           spottedTerritories.Add("B2L");
                        }
                        break;
                     case 240:
                        spottedTerritories.Add("B3C");
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B3M");
                           spottedTerritories.Add("B3L");
                        }
                        break;
                     case 300:
                        spottedTerritories.Add("B4C");
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B4M");
                           spottedTerritories.Add("B4L");
                        }
                        break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "GetSpottedTerritories(): 2-reached default for RotationHull=" + gi.Sherman.RotationHull.ToString());
                        return null;
                  }
               }
               else // all sectors except rear
               {
                  switch (gi.Sherman.RotationHull)
                  {
                     case 0:
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B1M");
                           spottedTerritories.Add("B1L");
                           spottedTerritories.Add("B3M");
                           spottedTerritories.Add("B3L");
                           spottedTerritories.Add("B4M");
                           spottedTerritories.Add("B4L");
                           spottedTerritories.Add("B6M");
                           spottedTerritories.Add("B6L");
                           spottedTerritories.Add("B9M");
                           spottedTerritories.Add("B9L");
                        }
                        spottedTerritories.Add("B1C");
                        spottedTerritories.Add("B3C");
                        spottedTerritories.Add("B4C");
                        spottedTerritories.Add("B6C");
                        spottedTerritories.Add("B9C");
                        break;
                     case 60:
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B1M");
                           spottedTerritories.Add("B1L");
                           spottedTerritories.Add("B2M");
                           spottedTerritories.Add("B2L");
                           spottedTerritories.Add("B4M");
                           spottedTerritories.Add("B4L");
                           spottedTerritories.Add("B6M");
                           spottedTerritories.Add("B6L");
                           spottedTerritories.Add("B9M");
                           spottedTerritories.Add("B9L");
                        }
                        spottedTerritories.Add("B1C");
                        spottedTerritories.Add("B2C");
                        spottedTerritories.Add("B4C");
                        spottedTerritories.Add("B6C");
                        spottedTerritories.Add("B9C");
                        break;
                     case 120:
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B1M");
                           spottedTerritories.Add("B1L");
                           spottedTerritories.Add("B2M");
                           spottedTerritories.Add("B2L");
                           spottedTerritories.Add("B3M");
                           spottedTerritories.Add("B3L");
                           spottedTerritories.Add("B6M");
                           spottedTerritories.Add("B6L");
                           spottedTerritories.Add("B9M");
                           spottedTerritories.Add("B9L");
                        }
                        spottedTerritories.Add("B1C");
                        spottedTerritories.Add("B2C");
                        spottedTerritories.Add("B3C");
                        spottedTerritories.Add("B6C");
                        spottedTerritories.Add("B9C");
                        break;
                     case 180:
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B1M");
                           spottedTerritories.Add("B1L");
                           spottedTerritories.Add("B2M");
                           spottedTerritories.Add("B2L");
                           spottedTerritories.Add("B3M");
                           spottedTerritories.Add("B3L");
                           spottedTerritories.Add("B4M");
                           spottedTerritories.Add("B4L");
                           spottedTerritories.Add("B9M");
                           spottedTerritories.Add("B9L");
                        }
                        spottedTerritories.Add("B1C");
                        spottedTerritories.Add("B2C");
                        spottedTerritories.Add("B3C");
                        spottedTerritories.Add("B4C");
                        spottedTerritories.Add("B9C");
                        break;
                     case 240:
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B1M");
                           spottedTerritories.Add("B1L");
                           spottedTerritories.Add("B2M");
                           spottedTerritories.Add("B2L");
                           spottedTerritories.Add("B3M");
                           spottedTerritories.Add("B3L");
                           spottedTerritories.Add("B4M");
                           spottedTerritories.Add("B4L");
                           spottedTerritories.Add("B6M");
                           spottedTerritories.Add("B6L");
                        }
                        spottedTerritories.Add("B1C");
                        spottedTerritories.Add("B2C");
                        spottedTerritories.Add("B3C");
                        spottedTerritories.Add("B4C");
                        spottedTerritories.Add("B6C");
                        break;
                     case 300:
                        if (false == isCloseRangeOnly)
                        {
                           spottedTerritories.Add("B2M");
                           spottedTerritories.Add("B2L");
                           spottedTerritories.Add("B3M");
                           spottedTerritories.Add("B3L");
                           spottedTerritories.Add("B4M");
                           spottedTerritories.Add("B4L");
                           spottedTerritories.Add("B6M");
                           spottedTerritories.Add("B6L");
                           spottedTerritories.Add("B9M");
                           spottedTerritories.Add("B9L");
                        }
                        spottedTerritories.Add("B2C");
                        spottedTerritories.Add("B3C");
                        spottedTerritories.Add("B4C");
                        spottedTerritories.Add("B6C");
                        spottedTerritories.Add("B9C");
                        break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "GetSpottedTerritories(): 3-reached default for RotationHull=" + gi.Sherman.RotationHull.ToString());
                        return null;
                  }
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetSpottedTerritories(): reached default for cm=" + cm.Name);
               return null;
         }
         //------------------------------------
         List<string> returnedTerritories = new List<string>(); // filter list to only unspotted/spotted enemy units
         foreach ( string tName in spottedTerritories )
         {
            int count = tName.Length;
            if (3 != count)
            {
               Logger.Log(LogEnum.LE_ERROR, "GetSpottedTerritories(): length not 3 for tName=" + tName);
               return null;
            }
            IStack? stack = gi.BattleStacks.Find(tName);
            if (null != stack)
            {
               foreach (IMapItem mi in stack.MapItems)
               {
                  if ((true == mi.Name.Contains("ATG")) || (true == mi.Name.Contains("TANK")) || (true == mi.Name.Contains("SPG")))
                  {
                     if ( (EnumSpottingResult.IDENTIFIED != mi.Spotting) && (EnumSpottingResult.HIDDEN != mi.Spotting))
                     {
                        returnedTerritories.Add(tName); // only want key off one MapItem in each territory
                        break;
                     }
                  }
               }
            }
         }
         return returnedTerritories;
      }
      private static void GetAllTerritories(ref List<string> spottedTerritories, bool isCloseRangeOnly)
      {
         spottedTerritories.Add("B1C");
         spottedTerritories.Add("B2C");
         spottedTerritories.Add("B3C");
         spottedTerritories.Add("B4C");
         spottedTerritories.Add("B6C");
         spottedTerritories.Add("B9C");
         if (false == isCloseRangeOnly)
         {
            spottedTerritories.Add("B1M");
            spottedTerritories.Add("B2M");
            spottedTerritories.Add("B3M");
            spottedTerritories.Add("B4M");
            spottedTerritories.Add("B6M");
            spottedTerritories.Add("B9M");
            spottedTerritories.Add("B1L");
            spottedTerritories.Add("B2L");
            spottedTerritories.Add("B3L");
            spottedTerritories.Add("B4L");
            spottedTerritories.Add("B6L");
            spottedTerritories.Add("B9L");
         }
      }
      public static string GetMainGunSector(IGameInstance gi)
      {
         double originalRotation = gi.Sherman.RotationHull + gi.Sherman.RotationTurret;
         double rotation = originalRotation;
         if (rotation < 0)
            rotation += 360.0;
         if( 359 < rotation )
            rotation -= 360.0;
         switch( rotation )
         {
            case 0.0:
               return "6";
            case 60.0:
               return "9";
            case 120.0:
               return "1";
            case 180.0:
               return "2";
            case 240.0:
               return "3";
            case 300.0:
               return "4";
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetMainGunSector() reached default rotation=" + rotation.ToString() + " or=" + originalRotation.ToString() + " hr=" + gi.Sherman.RotationHull.ToString() + " tr=" + gi.Sherman.RotationTurret.ToString());
               return "ERROR";
         }
      }
      public static bool IsEnemyUnitInSector(IGameInstance gi, string sector)
      {
         string tName = "B" + sector + "C";
         IStack? stack = gi.BattleStacks.Find(tName);
         if (null != stack)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.IsEnemyUnit())
                  return true;
            }
         }
         //--------------------------------------
         tName = "B" + sector + "M";
         stack = gi.BattleStacks.Find(tName);
         if (null != stack)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.IsEnemyUnit())
                  return true;
            }
         }
         //--------------------------------------
         tName = "B" + sector + "L";
         stack = gi.BattleStacks.Find(tName);
         if (null != stack)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.IsEnemyUnit())
                  return true;
            }
         }
         return false;
      }
      //---------------------------------------------------------------
      public Territory()
      {

      }
      public Territory(string name) { Name = name; }
      public override string ToString()
      {
         return Name + ":" + Type;
      }
      public ITerritory Find(List<ITerritory> territories, string name)
      {
         IEnumerable<ITerritory> results = from territory in territories
                                           where territory.Name == name
                                           select territory;
         if (0 < results.Count())
            return results.First();
         else
            throw new Exception("Territory.Find(): Unknown Territory=" + name);
      }
   }
   //---------------------------------------------------------------
   [Serializable]
   public class Territories : IEnumerable, ITerritories
   {
      [NonSerialized] public const string FILENAME = "Territories.xml";
      [NonSerialized] static public ITerritories theTerritories = new Territories();
      private readonly ArrayList myList;
      public Territories() { myList = new ArrayList(); }
      public void Add(ITerritory t) { myList.Add(t); }
      public void Insert(int index, ITerritory t) { myList.Insert(index, t); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(ITerritory t)
      {
         foreach (object o in myList)
         {
            ITerritory t1 = (ITerritory)o;
            if (Utilities.RemoveSpaces(t.Name) == Utilities.RemoveSpaces(t1.Name)) // match on name
               return true;
         }
         return false;
      }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(ITerritory t) { return myList.IndexOf(t); }
      public void Remove(ITerritory t) { myList.Remove(t); }
      public ITerritory? Find(string tName)
      {
         foreach (object o in myList)
         {
            ITerritory t = (ITerritory)o;
            if (tName == Utilities.RemoveSpaces(t.Name))
               return t;
         }
         return null;
      }
      public ITerritory? Find(string tName, string tType)
      {
         foreach (object o in myList)
         {
            ITerritory t = (ITerritory)o;
            string territoryName = Utilities.RemoveSpaces(t.Name);
            string territoryType = Utilities.RemoveSpaces(t.Type);
            if ((tName == territoryName) && (tType == territoryType) )
               return t;
         }
         return null;
      }
      public ITerritory? RemoveAt(int index)
      {
         ITerritory? t = myList[index] as ITerritory;
         myList.RemoveAt(index);
         return t;
      }
      public ITerritory? Remove(string tName)
      {
         foreach (object o in myList)
         {
            ITerritory t = (ITerritory)o;
            if (tName == t.Name)
            {
               myList.Remove(t);
               return t;
            }
         }
         return null;
      }
      public ITerritory? this[int index]
      {
         get
         {
            ITerritory? t = myList[index] as ITerritory;
            return t;
         }
         set { myList[index] = value; }
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[ ");
         foreach (object o in myList)
         {
            ITerritory t = (ITerritory)o;
            sb.Append(t.Name);
            sb.Append(" ");
         }
         sb.Append("]");
         return sb.ToString();
      }
   }
   //---------------------------------------------------------------
   public static class TerritoryExtensions
   {
      public static ITerritory? Find(this IList<ITerritory> territories, string name)
      {
         try
         {
            IEnumerable<ITerritory> results = from territory in territories where territory.Name == name select territory;
            if (0 < results.Count())
               return results.First();
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "MyTerritoryExtensions.Find(list, name): name=" + name + " e.Message=\n" + e.ToString()); ;
         }
         return null;
      }
      public static ITerritory? Find(this IList<ITerritory> territories, string name, string type)
      {
         try
         {
            IEnumerable<ITerritory> results = from territory in territories where (territory.Name == name && territory.Type == type) select territory;
            if (0 < results.Count())
               return results.First();
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "MyTerritoryExtensions.Find(list, name): name=" + name + " type=" + type + " e.Message=\n" + e.ToString()); ;
         }
         return null;
      }
   }
}
