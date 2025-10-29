using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pattons_Best.UnitTests
{
   internal class GameInstanceUnitTest : IUnitTest
   {
      //--------------------------------------------------------------------
      private DockPanel? myDockPanelTop = null;
      private ScrollViewer? myScrollViewerCanvas = null;
      private Canvas? myCanvasMap = null;
      private Canvas? myCanvasTank = null;
      private double myScrollingTime = 12.0;
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //--------------------------------------------------------------------
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public bool CtorError { get; } = false;
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      public GameInstanceUnitTest(DockPanel dp)
      {
         //------------------------------------
         myIndexName = 0;
         myHeaderNames.Add("07-Save Game");
         myHeaderNames.Add("07-Finish");
         //------------------------------------
         myCommandNames.Add("Save Game");
         myCommandNames.Add("Finish");
         //------------------------------------
         myDockPanelTop = dp; // top most dock panel that holds menu, statusbar, left dockpanel, and right dockpanel
         foreach (UIElement ui0 in dp.Children)
         {
            if (ui0 is DockPanel dockPanelInside) // DockPanel showing main play area
            {
               foreach (UIElement ui1 in dockPanelInside.Children)
               {
                  if (ui1 is ScrollViewer)
                  {
                     myScrollViewerCanvas = (ScrollViewer)ui1;
                     if (myScrollViewerCanvas.Content is Canvas)
                        myCanvasMap = (Canvas)myScrollViewerCanvas.Content;  // Find the Canvas in the visual tree
                  }
                  if (ui1 is DockPanel dockPanelControl) // DockPanel that holds the Map Image
                  {
                     foreach (UIElement ui2 in dockPanelControl.Children)
                     {
                        if (ui2 is Canvas)
                           myCanvasTank = (Canvas)ui2;
                     }
                  }
               }
            }
         }
         if (null == myCanvasMap) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myCanvas=null");
            CtorError = true;
            return;
         }
         if (null == myCanvasTank) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myCanvasTank=null");
            CtorError = true;
            return;
         }
      }
      //--------------------------------------------------------------------
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if (null == myDockPanelTop)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myDockPanelTop=null");
            return false;
         }
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvas=null");
            return false;
         }
         if (null == myCanvasTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasTank=null");
            return false;
         }
         if (null == myScrollViewerCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myScrollViewerCanvas=null");
            return false;
         }
         //-----------------------------------------------------
         if (CommandName == myCommandNames[0])
         {
            IGameInstance gameInstance = new GameInstance();
            gameInstance.EventActive = "e001";
            gameInstance.EventDisplayed = "e002";
            gameInstance.Day = 01;
            gameInstance.GameTurn = 02;
            gameInstance.GamePhase = GamePhase.UnitTest;
            gameInstance.EndGameReason = "03";
            //-----------------------
            for (int i=0; i < 3; ++i)
            {
               IAfterActionReport report = new AfterActionReport();
               report.Day = TableMgr.GetDate(i + 1);  
               switch(i)
               {
                  case 0: 
                     report.Scenario = EnumScenario.Advance;
                     report.Weather = "Clear";
                     EnumDecoration deco1 = EnumDecoration.ED_BronzeStar;
                     EnumDecoration deco2 = EnumDecoration.ED_DistinguisedServiceCross;
                     report.Decorations.Add(deco1);
                     report.Decorations.Add(deco2);
                     report.Notes.Add("Hello1");
                     report.DayEndedTime = "12:34";
                     report.Breakdown = "B01";
                     report.KnockedOut = "KO1";
                     break;
                  case 1: 
                     report.Scenario = EnumScenario.Battle;
                     report.Weather = "Snow";
                     EnumDecoration deco3 = EnumDecoration.ED_EuropeanCampain;
                     EnumDecoration deco4 = EnumDecoration.ED_MedalOfHonor;
                     EnumDecoration deco5 = EnumDecoration.ED_PurpleHeart;
                     report.Decorations.Add(deco3);
                     report.Decorations.Add(deco4);
                     report.Decorations.Add(deco5);
                     report.Notes.Add("Hello2");
                     report.Notes.Add("Hello3");
                     report.Notes.Add("Hello4");
                     report.Notes.Add("Hello5");
                     report.DayEndedTime = "12:35";
                     report.Breakdown = "B02";
                     report.KnockedOut = "KO2";
                     break;
                  default: 
                     report.Scenario = EnumScenario.Counterattack;
                     report.Weather = "Rain";
                     EnumDecoration deco6 = EnumDecoration.ED_SilverStar;
                     EnumDecoration deco7 = EnumDecoration.ED_PurpleHeart;
                     report.Decorations.Add(deco6);
                     report.Decorations.Add(deco7);
                     report.Notes.Add("Hello6");
                     report.Notes.Add("Hello7");
                     report.DayEndedTime = "12:36";
                     report.Breakdown = "B03";
                     report.KnockedOut = "KO3";
                     break;
               }
               report.Name = "Test Report " + (i + 1).ToString();
               report.TankCardNum = i + 1;
               report.SunriseHour = i + 1;
               report.SunsetHour = i + 10;
               report.SunriseMin = i + 20;
               report.SunsetMin = i + 20;
               //----------------------------------------
               report.Ammo30CalibreMG = i + 100;
               report.Ammo50CalibreMG = i + 110;
               report.AmmoSmokeBomb = i + 120;
               report.AmmoSmokeGrenade = i + 130;
               report.AmmoPeriscope = i + 140;
               report.MainGunHE = i + 150;
               report.MainGunAP = i + 160;
               report.MainGunWP = i + 170;
               report.MainGunHBCI = i + 180;
               report.MainGunHVAP = i + 190;
               //----------------------------------------
               report.VictoryPtsFriendlyKiaLightWeapon = i + 1000;
               report.VictoryPtsFriendlyKiaTruck = i + 1100;
               report.VictoryPtsFriendlyKiaSpwOrPsw = i + 1200;
               report.VictoryPtsFriendlyKiaSPGun = i + 1300;
               report.VictoryPtsFriendlyKiaPzIV = i + 1400;
               report.VictoryPtsFriendlyKiaPzV = i + 1500;
               report.VictoryPtsFriendlyKiaPzVI = i + 1600;
               report.VictoryPtsFriendlyKiaAtGun = i + 1700;
               report.VictoryPtsFriendlyKiaFortifiedPosition = i + 1800;
               //----------------------------------------
               report.VictoryPtsYourKiaLightWeapon = i + 2100;
               report.VictoryPtsYourKiaTruck = i + 2200;
               report.VictoryPtsYourKiaSpwOrPsw = i + 2300;
               report.VictoryPtsYourKiaSPGun = i + 2400;
               report.VictoryPtsYourKiaPzIV = i + 2500;
               report.VictoryPtsYourKiaPzV = i + 2600;
               report.VictoryPtsYourKiaPzVI = i + 2700;
               report.VictoryPtsYourKiaAtGun = i + 2800;
               report.VictoryPtsYourKiaFortifiedPosition = i + 2900;
               //----------------------------------------
               report.VictoryPtsCaptureArea = i + 3100;
               report.VictoryPtsCapturedExitArea = i + 3200;
               report.VictoryPtsLostArea = i + 3300;
               report.VictoryPtsFriendlyTank = i + 3400;
               report.VictoryPtsFriendlySquad = i + 3500;
               //----------------------------------------
               report.VictoryPtsTotalYourTank = i + 4100;
               report.VictoryPtsTotalFriendlyForces = i + 4200;
               report.VictoryPtsTotalTerritory = i + 4300;
               report.VictoryPtsTotalEngagement = i + 4400;
               gameInstance.Reports.Add(report);
            }
            //-----------------------
            gameInstance.Day = 01;
            gameInstance.BattlePhase = BattlePhase.Ambush;
            gameInstance.CrewActionPhase = CrewActionPhase.CrewSwitch;
            gameInstance.MovementEffectOnSherman = "04";
            gameInstance.MovementEffectOnEnemy = "05";
            gameInstance.FiredAmmoType = "06";
            //-----------------------
            int count = 2;
            string tType = "1";
            string tName = "ReadyRackAp" + count.ToString();
            ITerritory? t = Territories.theTerritories.Find(tName, tType);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t=null for " + tName);
               return false;
            }
            IMapItem rr1 = new MapItem("ReadyRackAp", 0.9, "c12RoundsLeft", t);
            rr1.Count = 2;
            gameInstance.ReadyRacks.Add(rr1);
            //-----------------------
            IMapItem mi = new MapItem("Driver_OpenHatch", 1.0, "c15OpenHatch", t);
            gameInstance.Hatches.Add(mi);
            //-----------------------
            IMapItem crewAction = new MapItem("Commander_ThrowGrenade", 1.0, "c70ThrowSmokeGrenade", t);
            gameInstance.CrewActions.Add(crewAction);
            //-----------------------
            string enemyName = "LW" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            IMapItem enemy = new MapItem(enemyName, Utilities.ZOOM, "c91Lw", t);
            gameInstance.Targets.Add(enemy);
            //-----------------------
            gameInstance.AdvancingEnemies.Add(enemy);
            gameInstance.ShermanAdvanceOrRetreatEnemies.Add(enemy);
            //----------------------------------------------
            ICrewMember commander = new CrewMember("Commander", "Sgt", "c07Commander");
            commander.Name = "Burtt";
            gameInstance.NewMembers.Add(commander);
            ICrewMember driver = new CrewMember("Driver", "Pvt", "c08Driver");
            driver.Name = "Alice";
            gameInstance.NewMembers.Add(driver);
            //----------------------------------------------
            gameInstance.InjuredCrewMembers.Add(commander);
            ICrewMember loader = new CrewMember("Loader", "Cpl", "c09Loader");
            loader.Name = "Ethel";
            gameInstance.InjuredCrewMembers.Add(loader);
            //----------------------------------------------
            enemyName = "LW" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            enemy = new MapItem(enemyName, Utilities.ZOOM, "c91Lw", t);
            gameInstance.TargetMainGun = enemy;
            enemyName = "LW" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            enemy = new MapItem(enemyName, Utilities.ZOOM, "c91Lw", t);
            gameInstance.TargetMg = enemy;
            //----------------------------------------------
            gameInstance.ShermanHvss = new MapItem("ShermanHvss555", 1.0, "c75Hvss", t);
            //----------------------------------------------
            commander = new CrewMember("Commander", "Sgt", "c07Commander");
            commander.Name = "Burtt2";
            gameInstance.ReturningCrewman = commander;
            //----------------------------------------------
            tName = "M001";
            ITerritory? t1 = Territories.theTerritories.Find(tName);
            if (null == t1)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t1=null for " + tName);
               return false;
            }
            tName = "M002";
            ITerritory? t2 = Territories.theTerritories.Find(tName);
            if (null == t2)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t2=null for " + tName);
               return false;
            }
            tName = "M003";
            ITerritory? t3 = Territories.theTerritories.Find(tName);
            if (null == t3)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t3=null for " + tName);
               return false;
            }
            gameInstance.AreaTargets.Add(t1);
            gameInstance.AreaTargets.Add(t2);
            gameInstance.AreaTargets.Add(t3);
            //----------------------------------------------
            gameInstance.CounterattachRetreats.Add(t1);
            gameInstance.CounterattachRetreats.Add(t2);
            gameInstance.CounterattachRetreats.Add(t3);
            //----------------------------------------------
            gameInstance.EnemyStrengthCheckTerritory = t1;
            gameInstance.ArtillerySupportCheck = t2;
            gameInstance.AirStrikeCheckTerritory = t3;
            gameInstance.EnteredArea = t1;
            gameInstance.AdvanceFire = t2;
            gameInstance.FriendlyAdvance = t3;
            gameInstance.EnemyAdvance = t3;
            //----------------------------------------------
            gameInstance.IsHatchesActive = false;
            gameInstance.IsRetreatToStartArea = true;
            gameInstance.IsShermanAdvancingOnMoveBoard = false;
            //----------------------------------------------
            gameInstance.SwitchedCrewMember = "Frankie";
            gameInstance.AssistantOriginalRating = 100;
            gameInstance.IsShermanFiringAtFront = true;
            gameInstance.IsShermanDeliberateImmobilization = false;
            gameInstance.ShermanTypeOfFire = "nickle";
            gameInstance.NumSmokeAttacksThisRound = 101;
            //----------------------------------------------
            gameInstance.IsMalfunctionedMainGun = false;
            gameInstance.IsMainGunRepairAttempted = true;
            gameInstance.IsBrokenMainGun = false;
            gameInstance.IsBrokenGunSight = true;
            gameInstance.FirstShots.Add("one");
            gameInstance.FirstShots.Add("two");
            gameInstance.FirstShots.Add("three");
            gameInstance.TrainedGunners.Add("four");
            gameInstance.TrainedGunners.Add("five");
            gameInstance.TrainedGunners.Add("size");
            //----------------------------------------------
            ShermanAttack attack1 = new ShermanAttack("one", "WP", true, false);
            ShermanAttack attack2 = new ShermanAttack("two", "AP", false, false);
            ShermanAttack attack3 = new ShermanAttack("three", "WP", true, true);
            gameInstance.ShermanHits.Add(attack1);
            gameInstance.ShermanHits.Add(attack2);
            gameInstance.ShermanHits.Add(attack3);
            //----------------------------------------------
            gameInstance.Death = new ShermanDeath(enemy);
            //----------------------------------------------
            gameInstance.IdentifiedTank = "seven";
            gameInstance.IdentifiedAtg = "eight";
            gameInstance.IdentifiedSpg = "nine";
            //----------------------------------------------
            gameInstance.IsShermanFiringAaMg = false;
            gameInstance.IsShermanFiringBowMg = true;
            gameInstance.IsShermanFiringCoaxialMg = false;
            gameInstance.IsShermanFiringSubMg = true;
            gameInstance.IsCommanderDirectingMgFire = false;
            gameInstance.IsShermanFiredAaMg = true;
            gameInstance.IsShermanFiredBowMg = false;
            gameInstance.IsShermanFiredCoaxialMg = true;
            gameInstance.IsShermanFiredSubMg = false;
            //----------------------------------------------
            gameInstance.IsMalfunctionedMgCoaxial = false;
            gameInstance.IsMalfunctionedMgBow = true;
            gameInstance.IsMalfunctionedMgAntiAircraft = false;
            gameInstance.IsCoaxialMgRepairAttempted = true;
            gameInstance.IsBowMgRepairAttempted = false;
            gameInstance.IsAaMgRepairAttempted = true;
            gameInstance.IsBrokenMgAntiAircraft = false;
            gameInstance.IsBrokenMgBow = true;
            gameInstance.IsBrokenMgCoaxial = false;
            //----------------------------------------------
            gameInstance.IsBrokenPeriscopeDriver = false;
            gameInstance.IsBrokenPeriscopeLoader = true;
            gameInstance.IsBrokenPeriscopeAssistant = false;
            gameInstance.IsBrokenPeriscopeGunner = true;
            gameInstance.IsBrokenPeriscopeCommander = false;
            //----------------------------------------------
            gameInstance.IsShermanTurretRotated = true;
            gameInstance.ShermanRotationTurretOld = 555.55;
            //----------------------------------------------
            gameInstance.IsCounterattackAmbush = false;
            gameInstance.IsLeadTank = true;
            gameInstance.IsAirStrikePending = false;
            gameInstance.IsAdvancingFireChosen = true;
            gameInstance.AdvancingFireMarkerCount = 555;
            gameInstance.BattleResistance = EnumResistance.Heavy;
            //----------------------------------------------
            gameInstance.IsMinefieldAttack = false;
            gameInstance.IsHarrassingFireBonus = true;
            gameInstance.IsFlankingFire = false;
            gameInstance.IsEnemyAdvanceComplete = true;
            gameInstance.Panzerfaust = new PanzerfaustAttack(enemy);
            gameInstance.NumCollateralDamage = 777;
            //----------------------------------------------
            GameLoadMgr loadMgr = new GameLoadMgr();
            if (false == loadMgr.SaveGameAsToFile(gameInstance))
               Logger.Log(LogEnum.LE_ERROR, "Command(): GameLoadMgr.SaveGameAs() returned false");
         }
         else if (CommandName == myCommandNames[1])
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Cleanup() return falsed");
               return false;
            }
         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi) // Move to the next test in this class's unit tests
      {
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myCanvas=null");
            return false;
         }
         if (HeaderName == myHeaderNames[0])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[1])
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup() return falsed");
               return false;
            }
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi) // Remove an elipses from the canvas and save off Territories.xml file
      {
         if (null == myCanvasMap)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvas=null");
            return false;
         }
         //--------------------------------------------------
         // Remove any existing UI elements from the Canvas
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMap.Children)
         {
            if (ui is Polygon polygon)
               elements.Add(ui);
            if (ui is Polyline polyline)
               elements.Add(ui);
            if (ui is Ellipse ellipse)
               elements.Add(ui);
            if (ui is Image img)
               elements.Add(ui);
            if (ui is TextBlock tb)
               elements.Add(ui);
         }
         foreach (UIElement ui1 in elements)
            myCanvasMap.Children.Remove(ui1);
         //--------------------------------------------------
         Image imageMap = new Image() { Name = "Map", Width = 1115, Height = 880, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("MapMovement") };
         myCanvasMap.Children.Add(imageMap);
         Canvas.SetLeft(imageMap, 0);
         Canvas.SetTop(imageMap, 0);
         //--------------------------------------------------
         Application.Current.Shutdown();
         return true;
      }
   }
}
