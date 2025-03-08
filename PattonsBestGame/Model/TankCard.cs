using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattons_Best
{
   public struct TankCard
   {
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
         switch( num )
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
