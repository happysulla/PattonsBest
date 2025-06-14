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
      public string myModel = "M4-A";
      public string myChasis = "M4";
      public string myTurret = "A";
      public string myMainGun = "75";
      public string myArmorClass = "I";
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
            case 2:
               myChasis = "M4";
               myTurret = "B";
               myMainGun = "75";
               myArmorClass = "I";
               myNumMainGunRound = 97;
               myRateOfFire = 30;
               myMaxReadyRackCount = 8;
               myIsLoaderHatch = true;
               myIsVisionCupola = false;
               myIsSmokeMortar = true;
               myIsWetStowage = false;
               myIsHvss = false;
               break;
            case 3:
               myChasis = "M4";
               myTurret = "C";
               myMainGun = "75";
               myArmorClass = "I";
               myNumMainGunRound = 97;
               myRateOfFire = 30;
               myMaxReadyRackCount = 8;
               myIsLoaderHatch = true;
               myIsVisionCupola = true;
               myIsSmokeMortar = true;
               myIsWetStowage = false;
               myIsHvss = false;
               break;
            case 4:
               myChasis = "M4A1";
               myTurret = "A";
               myMainGun = "75";
               myArmorClass = "II";
               myNumMainGunRound = 91;
               myRateOfFire = 30;
               myMaxReadyRackCount = 8;
               myIsLoaderHatch = false;
               myIsVisionCupola = false;
               myIsSmokeMortar = false;
               myIsWetStowage = false;
               myIsHvss = false;
               break;
            case 5:
               myChasis = "M4A1";
               myTurret = "B";
               myMainGun = "75";
               myArmorClass = "II";
               myNumMainGunRound = 91;
               myRateOfFire = 30;
               myMaxReadyRackCount = 8;
               myIsLoaderHatch = true;
               myIsVisionCupola = false;
               myIsSmokeMortar = true;
               myIsWetStowage = false;
               myIsHvss = false;
               break;
            case 6:
               myChasis = "M4A1";
               myTurret = "C";
               myMainGun = "75";
               myArmorClass = "II";
               myNumMainGunRound = 91;
               myRateOfFire = 30;
               myMaxReadyRackCount = 8;
               myIsLoaderHatch = true;
               myIsVisionCupola = true;
               myIsSmokeMortar = true;
               myIsWetStowage = false;
               myIsHvss = false;
               break;
            case 7:
               myChasis = "M4A3";
               myTurret = "A";
               myMainGun = "75";
               myArmorClass = "I";
               myNumMainGunRound = 97;
               myRateOfFire = 30;
               myMaxReadyRackCount = 8;
               myIsLoaderHatch = false;
               myIsVisionCupola = false;
               myIsSmokeMortar = false;
               myIsWetStowage = false;
               myIsHvss = false;
               break;
            case 8:
               myChasis = "M4A3";
               myTurret = "B";
               myMainGun = "75";
               myArmorClass = "I";
               myNumMainGunRound = 97;
               myRateOfFire = 30;
               myMaxReadyRackCount = 8;
               myIsLoaderHatch = true;
               myIsVisionCupola = false;
               myIsSmokeMortar = true;
               myIsWetStowage = false;
               myIsHvss = false;
               break;
            case 9:
               myChasis = "M4A3";
               myTurret = "C";
               myMainGun = "75";
               myArmorClass = "I";
               myNumMainGunRound = 97;
               myRateOfFire = 30;
               myMaxReadyRackCount = 8;
               myIsLoaderHatch = true;
               myIsVisionCupola = true;
               myIsSmokeMortar = true;
               myIsWetStowage = false;
               myIsHvss = false;
               break;
            case 10:
               myChasis = "M4A3(75)";
               myTurret = "D";
               myMainGun = "75";
               myArmorClass = "II";
               myNumMainGunRound = 104;
               myRateOfFire = 30;
               myMaxReadyRackCount = 4;
               myIsLoaderHatch = true;
               myIsVisionCupola = false;
               myIsSmokeMortar = true;
               myIsWetStowage = false;
               myIsHvss = false;
               break;
            case 11:
               myChasis = "M4A3(75)W";
               myTurret = "E";
               myMainGun = "75";
               myArmorClass = "II";
               myNumMainGunRound = 104;
               myRateOfFire = 30;
               myMaxReadyRackCount = 4;
               myIsLoaderHatch = true;
               myIsVisionCupola = true;
               myIsSmokeMortar = true;
               myIsWetStowage = true;
               myIsHvss = true;
               break;
            case 12:
               myChasis = "M4A3WE2(75)W";
               myTurret = "F";
               myMainGun = "75";
               myArmorClass = "III";
               myNumMainGunRound = 104;
               myRateOfFire = 30;
               myMaxReadyRackCount = 4;
               myIsLoaderHatch = true;
               myIsVisionCupola = true;
               myIsSmokeMortar = true;
               myIsWetStowage = true;
               myIsHvss = true;
               break;
            case 13:
               myChasis = "M4A3WE2(76)W";
               myTurret = "F";
               myMainGun = "76L";
               myArmorClass = "III";
               myNumMainGunRound = 71;
               myRateOfFire = 20;
               myMaxReadyRackCount = 6;
               myIsLoaderHatch = true;
               myIsVisionCupola = true;
               myIsSmokeMortar = true;
               myIsWetStowage = true;
               myIsHvss = true;
               break;
            case 14:
               myChasis = "M4A1(76)W";
               myTurret = "G";
               myMainGun = "76L";
               myArmorClass = "II";
               myNumMainGunRound = 71;
               myRateOfFire = 20;
               myMaxReadyRackCount = 6;
               myIsLoaderHatch = true;
               myIsVisionCupola = true;
               myIsSmokeMortar = true;
               myIsWetStowage = true;
               myIsHvss = true;
               break;
            case 15:
               myChasis = "M4A1(76)W";
               myTurret = "H";
               myMainGun = "76L";
               myArmorClass = "II";
               myNumMainGunRound = 71;
               myRateOfFire = 20;
               myMaxReadyRackCount = 6;
               myIsLoaderHatch = true;
               myIsVisionCupola = true;
               myIsSmokeMortar = true;
               myIsWetStowage = true;
               myIsHvss = true;
               break;
            case 16:
               myChasis = "M4A3(76)W";
               myTurret = "G";
               myMainGun = "76L";
               myArmorClass = "II";
               myNumMainGunRound = 71;
               myRateOfFire = 20;
               myMaxReadyRackCount = 6;
               myIsLoaderHatch = true;
               myIsVisionCupola = true;
               myIsSmokeMortar = true;
               myIsWetStowage = true;
               myIsHvss = true;
               break;
            case 17:
               myChasis = "M4A3(76)W";
               myTurret = "H";
               myMainGun = "76L";
               myArmorClass = "II";
               myNumMainGunRound = 71;
               myRateOfFire = 20;
               myMaxReadyRackCount = 6;
               myIsLoaderHatch = true;
               myIsVisionCupola = true;
               myIsSmokeMortar = true;
               myIsWetStowage = true;
               myIsHvss = true;
               break;
            case 18:
               myChasis = "VC";
               myTurret = "I";
               myMainGun = "76LL";
               myArmorClass = "II";
               myNumMainGunRound = 78;
               myRateOfFire = 20;
               myMaxReadyRackCount = 5;
               myIsLoaderHatch = true;
               myIsSmokeMortar = true;
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
