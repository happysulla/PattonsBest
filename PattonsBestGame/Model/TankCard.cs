using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Pattons_Best
{
   public struct TankCard
   {
      public static List<String>? GetSpottedTerritories(IGameInstance gi, ICrewMember cm)
      {
         List<string> spottedTerritories = new List<string>();
         switch (cm.Name)
         {
            case "Commander":
               if (false == cm.IsButtonedUp) // any one sector
               {
                  foreach (IStack stack in gi.BattleStacks)
                  {
                     foreach (IMapItem mi in stack.MapItems)
                     {
                        if (true == mi.Name.Contains("CommanderSpot"))
                        {
                           string tName = mi.TerritoryCurrent.Name;
                           if (3 != tName.Length)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Button_Click(): tName != 3 for " + mi.TerritoryCurrent.Name);
                              return null;
                           }
                           char sector = tName[tName.Length - 2];
                           spottedTerritories.Add("B" + sector + "C");
                           spottedTerritories.Add("B" + sector + "M");
                           spottedTerritories.Add("B" + sector + "L");
                        }
                     }
                  }
               }
               else // all sectors
               {
                  spottedTerritories.Add("B1C");
                  spottedTerritories.Add("B1M");
                  spottedTerritories.Add("B1L");
                  spottedTerritories.Add("B2C");
                  spottedTerritories.Add("B2M");
                  spottedTerritories.Add("B2L");
                  spottedTerritories.Add("B3C");
                  spottedTerritories.Add("B3M");
                  spottedTerritories.Add("B3L");
                  spottedTerritories.Add("B4C");
                  spottedTerritories.Add("B4M");
                  spottedTerritories.Add("B4L");
                  spottedTerritories.Add("B6C");
                  spottedTerritories.Add("B6M");
                  spottedTerritories.Add("B6L");
                  spottedTerritories.Add("B9C");
                  spottedTerritories.Add("B9M");
                  spottedTerritories.Add("B9L");
               }
               break;
            case "Gunner":
               switch (gi.Sherman.RotationBase)
               {
                  case 0:
                     spottedTerritories.Add("B6C");
                     spottedTerritories.Add("B6M");
                     spottedTerritories.Add("B6L");
                     break;
                  case 60:
                     spottedTerritories.Add("B9C");
                     spottedTerritories.Add("B9M");
                     spottedTerritories.Add("B9L");
                     break;
                  case 120:
                     spottedTerritories.Add("B1C");
                     spottedTerritories.Add("B1M");
                     spottedTerritories.Add("B1L");
                     break;
                  case 180:
                     spottedTerritories.Add("B2C");
                     spottedTerritories.Add("B2M");
                     spottedTerritories.Add("B2L");
                     break;
                  case 240:
                     spottedTerritories.Add("B3C");
                     spottedTerritories.Add("B3M");
                     spottedTerritories.Add("B3L");
                     break;
                  case 300:
                     spottedTerritories.Add("B4C");
                     spottedTerritories.Add("B4M");
                     spottedTerritories.Add("B4L");
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "Button_Click(): 1-reached default for RotationBase=" + gi.Sherman.RotationBase.ToString());
                     return null;
               }
               break;
            case "Loader":
               foreach (IStack stack in gi.BattleStacks) // any one sector already selected
               {
                  foreach (IMapItem mi in stack.MapItems)
                  {
                     if (true == mi.Name.Contains("LoaderSpot"))
                     {
                        string tName = mi.TerritoryCurrent.Name;
                        if (3 != tName.Length)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "Button_Click(): tName != 3 for " + mi.TerritoryCurrent.Name);
                           return null;
                        }
                        char sector = tName[tName.Length - 2];
                        spottedTerritories.Add("B" + sector + "C");
                        spottedTerritories.Add("B" + sector + "M");
                        spottedTerritories.Add("B" + sector + "L");
                     }
                  }
               }
               break;
            case "Driver":
            case "Assistant":
               if (true == cm.IsButtonedUp) // Tank Front Only 
               {
                  switch (gi.Sherman.RotationBase)
                  {
                     case 0:
                        spottedTerritories.Add("B6C");
                        spottedTerritories.Add("B6M");
                        spottedTerritories.Add("B6L");
                        break;
                     case 60:
                        spottedTerritories.Add("B9C");
                        spottedTerritories.Add("B9M");
                        spottedTerritories.Add("B9L");
                        break;
                     case 120:
                        spottedTerritories.Add("B1C");
                        spottedTerritories.Add("B1M");
                        spottedTerritories.Add("B1L");
                        break;
                     case 180:
                        spottedTerritories.Add("B2C");
                        spottedTerritories.Add("B2M");
                        spottedTerritories.Add("B2L");
                        break;
                     case 240:
                        spottedTerritories.Add("B3C");
                        spottedTerritories.Add("B3M");
                        spottedTerritories.Add("B3L");
                        break;
                     case 300:
                        spottedTerritories.Add("B4C");
                        spottedTerritories.Add("B4M");
                        spottedTerritories.Add("B4L");
                        break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "Button_Click(): 2-reached default for RotationBase=" + gi.Sherman.RotationBase.ToString());
                        return null;
                  }
               }
               else // all sectors except rear
               {
                  switch (gi.Sherman.RotationBase)
                  {
                     case 0:
                        spottedTerritories.Add("B1M");
                        spottedTerritories.Add("B1L");
                        spottedTerritories.Add("B3C");
                        spottedTerritories.Add("B3M");
                        spottedTerritories.Add("B3L");
                        spottedTerritories.Add("B4C");
                        spottedTerritories.Add("B4M");
                        spottedTerritories.Add("B4L");
                        spottedTerritories.Add("B6C");
                        spottedTerritories.Add("B6M");
                        spottedTerritories.Add("B6L");
                        spottedTerritories.Add("B9C");
                        spottedTerritories.Add("B9M");
                        spottedTerritories.Add("B9L");
                        break;
                     case 60:
                        spottedTerritories.Add("B1M");
                        spottedTerritories.Add("B1L");
                        spottedTerritories.Add("B2C");
                        spottedTerritories.Add("B2M");
                        spottedTerritories.Add("B2L");
                        spottedTerritories.Add("B4C");
                        spottedTerritories.Add("B4M");
                        spottedTerritories.Add("B4L");
                        spottedTerritories.Add("B6C");
                        spottedTerritories.Add("B6M");
                        spottedTerritories.Add("B6L");
                        spottedTerritories.Add("B9C");
                        spottedTerritories.Add("B9M");
                        spottedTerritories.Add("B9L");
                        break;
                     case 120:
                        spottedTerritories.Add("B1C");
                        spottedTerritories.Add("B1M");
                        spottedTerritories.Add("B1L");
                        spottedTerritories.Add("B2C");
                        spottedTerritories.Add("B2M");
                        spottedTerritories.Add("B2L");
                        spottedTerritories.Add("B3C");
                        spottedTerritories.Add("B3M");
                        spottedTerritories.Add("B3L");
                        spottedTerritories.Add("B6C");
                        spottedTerritories.Add("B6M");
                        spottedTerritories.Add("B6L");
                        spottedTerritories.Add("B9C");
                        spottedTerritories.Add("B9M");
                        spottedTerritories.Add("B9L");
                        break;
                     case 180:
                        spottedTerritories.Add("B1C");
                        spottedTerritories.Add("B1M");
                        spottedTerritories.Add("B1L");
                        spottedTerritories.Add("B2C");
                        spottedTerritories.Add("B2M");
                        spottedTerritories.Add("B2L");
                        spottedTerritories.Add("B3C");
                        spottedTerritories.Add("B3M");
                        spottedTerritories.Add("B3L");
                        spottedTerritories.Add("B4C");
                        spottedTerritories.Add("B4M");
                        spottedTerritories.Add("B4L");
                        spottedTerritories.Add("B9C");
                        spottedTerritories.Add("B9M");
                        spottedTerritories.Add("B9L");
                        break;
                     case 240:
                        spottedTerritories.Add("B1C");
                        spottedTerritories.Add("B1M");
                        spottedTerritories.Add("B1L");
                        spottedTerritories.Add("B2C");
                        spottedTerritories.Add("B2M");
                        spottedTerritories.Add("B2L");
                        spottedTerritories.Add("B3C");
                        spottedTerritories.Add("B3M");
                        spottedTerritories.Add("B3L");
                        spottedTerritories.Add("B4C");
                        spottedTerritories.Add("B4M");
                        spottedTerritories.Add("B4L");
                        spottedTerritories.Add("B6C");
                        spottedTerritories.Add("B6M");
                        spottedTerritories.Add("B6L");
                        break;
                     case 300:
                        spottedTerritories.Add("B2C");
                        spottedTerritories.Add("B2M");
                        spottedTerritories.Add("B2L");
                        spottedTerritories.Add("B3C");
                        spottedTerritories.Add("B3M");
                        spottedTerritories.Add("B3L");
                        spottedTerritories.Add("B4C");
                        spottedTerritories.Add("B4M");
                        spottedTerritories.Add("B4L");
                        spottedTerritories.Add("B6C");
                        spottedTerritories.Add("B6M");
                        spottedTerritories.Add("B6L");
                        spottedTerritories.Add("B9C");
                        spottedTerritories.Add("B9M");
                        spottedTerritories.Add("B9L");
                        break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "Button_Click(): 3-reached default for RotationBase=" + gi.Sherman.RotationBase.ToString());
                        return null;
                  }
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): reached default for cm=" + cm.Name);
               return null;
         }
         return spottedTerritories;
      }
      public string myModel = "M4-A";
      public string myChasis = "M4";
      public string myTurret = "A";
      public string myMainGun = "75";
      public string myArmorClass = "A";
      public int myNumMainGunRound = 97;
      public int myRateOfFire = 30;
      public int myMaxReadyRackCount = 8;
      public bool myIsLoaderHatch = false;
      public bool myIsSmokeMortar = false;
      public bool myIsVisionCupola = false;
      public bool myIsWetStowage = false;
      public bool myIsHvss = false;
      //-------------------------------
      public TankCard(int num)
      {
         switch (num)
         {
            case 1:
               break;
            case 2:
               break;
            case 3:
               break;
            case 4:
               break;
            case 5:
               break;
            case 6:
               break;
            case 7:
               break;
            case 8:
               break;
            case 9:
               break;
            case 10:
               break;
            case 11:
               break;
            case 12:
               break;
            case 13:
               break;
            case 14:
               break;
            case 15:
               break;
            case 16:
               break;
            case 17:
               break;
            case 18:
               myChasis = "M4";
               myTurret = "A";
               myMainGun = "75";
               myArmorClass = "I";
               myNumMainGunRound = 97;
               myRateOfFire = 30;
               myMaxReadyRackCount = 8;
               myIsLoaderHatch = false;
               myIsSmokeMortar = false;
               myIsVisionCupola = false;
               myIsWetStowage = false;
               myIsHvss = false;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "Reached default: num=" + num.ToString());
               return;
         }
      }
   }
}
