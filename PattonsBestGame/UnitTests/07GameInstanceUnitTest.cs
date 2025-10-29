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
      private IGameInstance? myGameInstanceSave = null;
      private IGameInstance? myGameInstanceLoad = null;
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
         myHeaderNames.Add("07-Load Game");
         myHeaderNames.Add("07-Compare");
         myHeaderNames.Add("07-Finish");
         //------------------------------------
         myCommandNames.Add("Save Game");
         myCommandNames.Add("Load Game");
         myCommandNames.Add("Compare");
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
         //----------------------------------------------------b-
         if (CommandName == myCommandNames[0])
         {
            myGameInstanceSave = new GameInstance();
            myGameInstanceSave.EventActive = "e001";
            myGameInstanceSave.EventDisplayed = "e002";
            myGameInstanceSave.Day = 01;
            myGameInstanceSave.GameTurn = 02;
            myGameInstanceSave.GamePhase = GamePhase.UnitTest;
            myGameInstanceSave.EndGameReason = "03";
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
               myGameInstanceSave.Reports.Add(report);
            }
            //-----------------------
            myGameInstanceSave.Day = 01;
            myGameInstanceSave.BattlePhase = BattlePhase.Ambush;
            myGameInstanceSave.CrewActionPhase = CrewActionPhase.CrewSwitch;
            myGameInstanceSave.MovementEffectOnSherman = "04";
            myGameInstanceSave.MovementEffectOnEnemy = "05";
            myGameInstanceSave.FiredAmmoType = "06";
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
            myGameInstanceSave.ReadyRacks.Add(rr1);
            //-----------------------
            IMapItem mi = new MapItem("Driver_OpenHatch", 1.0, "c15OpenHatch", t);
            myGameInstanceSave.Hatches.Add(mi);
            //-----------------------
            IMapItem crewAction = new MapItem("Commander_ThrowGrenade", 1.0, "c70ThrowSmokeGrenade", t);
            myGameInstanceSave.CrewActions.Add(crewAction);
            //-----------------------
            string enemyName = "LW" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            IMapItem enemy = new MapItem(enemyName, Utilities.ZOOM, "c91Lw", t);
            myGameInstanceSave.Targets.Add(enemy);
            //-----------------------
            myGameInstanceSave.AdvancingEnemies.Add(enemy);
            myGameInstanceSave.ShermanAdvanceOrRetreatEnemies.Add(enemy);
            //----------------------------------------------
            ICrewMember commander = new CrewMember("Commander", "Sgt", "c07Commander");
            commander.Name = "Burtt";
            myGameInstanceSave.NewMembers.Add(commander);
            ICrewMember driver = new CrewMember("Driver", "Pvt", "c08Driver");
            driver.Name = "Alice";
            myGameInstanceSave.NewMembers.Add(driver);
            //----------------------------------------------
            myGameInstanceSave.InjuredCrewMembers.Add(commander);
            ICrewMember loader = new CrewMember("Loader", "Cpl", "c09Loader");
            loader.Name = "Ethel";
            myGameInstanceSave.InjuredCrewMembers.Add(loader);
            //----------------------------------------------
            enemyName = "LW" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            enemy = new MapItem(enemyName, Utilities.ZOOM, "c91Lw", t);
            myGameInstanceSave.TargetMainGun = enemy;
            enemyName = "LW" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            enemy = new MapItem(enemyName, Utilities.ZOOM, "c91Lw", t);
            myGameInstanceSave.TargetMg = enemy;
            //----------------------------------------------
            myGameInstanceSave.ShermanHvss = new MapItem("ShermanHvss555", 1.0, "c75Hvss", t);
            //----------------------------------------------
            commander = new CrewMember("Commander", "Sgt", "c07Commander");
            commander.Name = "Burtt2";
            myGameInstanceSave.ReturningCrewman = commander;
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
            myGameInstanceSave.AreaTargets.Add(t1);
            myGameInstanceSave.AreaTargets.Add(t2);
            myGameInstanceSave.AreaTargets.Add(t3);
            //----------------------------------------------
            myGameInstanceSave.CounterattachRetreats.Add(t1);
            myGameInstanceSave.CounterattachRetreats.Add(t2);
            myGameInstanceSave.CounterattachRetreats.Add(t3);
            //----------------------------------------------
            myGameInstanceSave.EnemyStrengthCheckTerritory = t1;
            myGameInstanceSave.ArtillerySupportCheck = t2;
            myGameInstanceSave.AirStrikeCheckTerritory = t3;
            myGameInstanceSave.EnteredArea = t1;
            myGameInstanceSave.AdvanceFire = t2;
            myGameInstanceSave.FriendlyAdvance = t3;
            myGameInstanceSave.EnemyAdvance = t3;
            //----------------------------------------------
            myGameInstanceSave.IsHatchesActive = false;
            myGameInstanceSave.IsRetreatToStartArea = true;
            myGameInstanceSave.IsShermanAdvancingOnMoveBoard = false;
            //----------------------------------------------
            myGameInstanceSave.SwitchedCrewMember = "Frankie";
            myGameInstanceSave.AssistantOriginalRating = 100;
            myGameInstanceSave.IsShermanFiringAtFront = true;
            myGameInstanceSave.IsShermanDeliberateImmobilization = false;
            myGameInstanceSave.ShermanTypeOfFire = "nickle";
            myGameInstanceSave.NumSmokeAttacksThisRound = 101;
            //----------------------------------------------
            myGameInstanceSave.IsMalfunctionedMainGun = false;
            myGameInstanceSave.IsMainGunRepairAttempted = true;
            myGameInstanceSave.IsBrokenMainGun = false;
            myGameInstanceSave.IsBrokenGunSight = true;
            myGameInstanceSave.FirstShots.Add("one");
            myGameInstanceSave.FirstShots.Add("two");
            myGameInstanceSave.FirstShots.Add("three");
            myGameInstanceSave.TrainedGunners.Add("four");
            myGameInstanceSave.TrainedGunners.Add("five");
            myGameInstanceSave.TrainedGunners.Add("size");
            //----------------------------------------------
            ShermanAttack attack1 = new ShermanAttack("one", "WP", true, false);
            ShermanAttack attack2 = new ShermanAttack("two", "AP", false, false);
            ShermanAttack attack3 = new ShermanAttack("three", "WP", true, true);
            myGameInstanceSave.ShermanHits.Add(attack1);
            myGameInstanceSave.ShermanHits.Add(attack2);
            myGameInstanceSave.ShermanHits.Add(attack3);
            //----------------------------------------------
            myGameInstanceSave.Death = new ShermanDeath(enemy);
            //----------------------------------------------
            myGameInstanceSave.IdentifiedTank = "seven";
            myGameInstanceSave.IdentifiedAtg = "eight";
            myGameInstanceSave.IdentifiedSpg = "nine";
            //----------------------------------------------
            myGameInstanceSave.IsShermanFiringAaMg = false;
            myGameInstanceSave.IsShermanFiringBowMg = true;
            myGameInstanceSave.IsShermanFiringCoaxialMg = false;
            myGameInstanceSave.IsShermanFiringSubMg = true;
            myGameInstanceSave.IsCommanderDirectingMgFire = false;
            myGameInstanceSave.IsShermanFiredAaMg = true;
            myGameInstanceSave.IsShermanFiredBowMg = false;
            myGameInstanceSave.IsShermanFiredCoaxialMg = true;
            myGameInstanceSave.IsShermanFiredSubMg = false;
            //----------------------------------------------
            myGameInstanceSave.IsMalfunctionedMgCoaxial = false;
            myGameInstanceSave.IsMalfunctionedMgBow = true;
            myGameInstanceSave.IsMalfunctionedMgAntiAircraft = false;
            myGameInstanceSave.IsCoaxialMgRepairAttempted = true;
            myGameInstanceSave.IsBowMgRepairAttempted = false;
            myGameInstanceSave.IsAaMgRepairAttempted = true;
            myGameInstanceSave.IsBrokenMgAntiAircraft = false;
            myGameInstanceSave.IsBrokenMgBow = true;
            myGameInstanceSave.IsBrokenMgCoaxial = false;
            //----------------------------------------------
            myGameInstanceSave.IsBrokenPeriscopeDriver = false;
            myGameInstanceSave.IsBrokenPeriscopeLoader = true;
            myGameInstanceSave.IsBrokenPeriscopeAssistant = false;
            myGameInstanceSave.IsBrokenPeriscopeGunner = true;
            myGameInstanceSave.IsBrokenPeriscopeCommander = false;
            //----------------------------------------------
            myGameInstanceSave.IsShermanTurretRotated = true;
            myGameInstanceSave.ShermanRotationTurretOld = 555.55;
            //----------------------------------------------
            myGameInstanceSave.IsCounterattackAmbush = false;
            myGameInstanceSave.IsLeadTank = true;
            myGameInstanceSave.IsAirStrikePending = false;
            myGameInstanceSave.IsAdvancingFireChosen = true;
            myGameInstanceSave.AdvancingFireMarkerCount = 555;
            myGameInstanceSave.BattleResistance = EnumResistance.Heavy;
            //----------------------------------------------
            myGameInstanceSave.IsMinefieldAttack = false;
            myGameInstanceSave.IsHarrassingFireBonus = true;
            myGameInstanceSave.IsFlankingFire = false;
            myGameInstanceSave.IsEnemyAdvanceComplete = true;
            myGameInstanceSave.Panzerfaust = new PanzerfaustAttack(enemy);
            myGameInstanceSave.NumCollateralDamage = 777;
            //----------------------------------------------
            GameLoadMgr loadMgr = new GameLoadMgr();
            if (false == loadMgr.SaveGameAsToFile(myGameInstanceSave))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): GameLoadMgr.SaveGameAs() returned false");
               return false;
            }
         }
         else if( CommandName == myCommandNames[1])
         {
            GameLoadMgr loadMgr = new GameLoadMgr();
            myGameInstanceLoad = loadMgr.OpenGameFromFile();
            if (null == myGameInstanceLoad)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): GameLoadMgr.OpenGameFromFile() returned null");
               return false;
            }
         }
         else if (CommandName == myCommandNames[12])
         {
            GameLoadMgr loadMgr = new GameLoadMgr();
            myGameInstanceLoad = loadMgr.OpenGameFromFile();
            if (false == IsEqual(myGameInstanceSave, myGameInstanceLoad))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): GameLoadMgr.OpenGameFromFile() returned null");
               return false;
            }

         }
         else if (CommandName == myCommandNames[3])
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
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[2])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3])
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
      //--------------------------------------------------------------------
      private bool IsEqual(IGameInstance? left, IGameInstance? right)
      {
         if( null == left )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left=null");
            return false;
         }
         if (null == right )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right=null");
            return false;
         }
         if(left.EventActive != right.EventActive )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EventActive != right.EventActive");
            return false;
         }
         if(left.EventDisplayed != right.EventDisplayed )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EventDisplayed != right.EventDisplayed");
            return false;
         }
         if( left.Day != right.Day )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Day != right.Day");
            return false;
         }
         if (left.GameTurn != right.GameTurn )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.GameTurn != right.GameTurn");
            return false;
         }
         if (left.GamePhase != right.GamePhase )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.GamePhase != right.GamePhase");
            return false;
         }
         if(left.EndGameReason != right.EndGameReason )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EndGameReason != right.EndGameReason");
            return false;
         }
         if ( left.Reports.Count != right.Reports.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Reports.Count != right.Reports.Count");
            return false;
         }
         if(left.BattlePhase != right.BattlePhase )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.BattlePhase != right.BattlePhase");
            return false;
         }
         if (left.CrewActionPhase != right.CrewActionPhase )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.CrewActionPhase != right.CrewActionPhase");
            return false;
         }  
         if(left.MovementEffectOnSherman != right.MovementEffectOnSherman)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.MovementEffectOnSherman != right.MovementEffectOnSherman");
            return false;
         }
         if( left.MovementEffectOnEnemy != right.MovementEffectOnEnemy )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.MovementEffectOnEnemy != right.MovementEffectOnEnemy");
            return false;
         }
         if( left.FiredAmmoType != right.FiredAmmoType )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.FiredAmmoType != right.FiredAmmoType");
            return false;
         }
         if( left.ReadyRacks.Count != right.ReadyRacks.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ReadyRacks.Count != right.ReadyRacks.Count");
            return false;
         }
         if( left.Hatches.Count != right.Hatches.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Hatches.Count != right.Hatches.Count");
            return false;
         }  
         if( left.CrewActions.Count != right.CrewActions.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.CrewActions.Count != right.CrewActions.Count");
            return false;
         }
         if(left.Targets.Count != right.Targets.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Targets.Count != right.Targets.Count");
            return false;
         }
         if( left.AdvancingEnemies.Count != right.AdvancingEnemies.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.AdvancingEnemies.Count != right.AdvancingEnemies.Count");
            return false;
         }
         if(left.ShermanAdvanceOrRetreatEnemies.Count != right.ShermanAdvanceOrRetreatEnemies.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ShermanAdvanceOrRetreatEnemies.Count != right.ShermanAdvanceOrRetreatEnemies.Count");
            return false;
         }
         if(left.NewMembers.Count != right.NewMembers.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.NewMembers.Count != right.NewMembers.Count");
            return false;
         }
         if(left.InjuredCrewMembers.Count != right.InjuredCrewMembers.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.InjuredCrewMembers.Count != right.InjuredCrewMembers.Count");
            return false;
         }
         //------------------------------------------------------------
         if (false == IsEqual(left.Sherman, right.Sherman))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Sherman != right.Sherman");
            return false;
         }
         //------------------------------------------------------------
         if ( null == left.TargetMainGun && null != right.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.TargetMainGun=null");
            return false;
         }
         if (null != left.TargetMainGun && null == right.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.TargetMainGun=null");
            return false;
         }
         if( null != left.TargetMainGun && null != right.TargetMainGun)
         {
            if (left.TargetMainGun.Name != right.TargetMainGun.Name)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.TargetMainGun.Name != right.TargetMainGun.Name");
               return false;
            }
         }
         //------------------------------------------------------------
         if (null == left.TargetMg && null != right.TargetMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.TargetMg=null");
            return false;
         }
         if (null != left.TargetMg && null == right.TargetMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.TargetMg=null");
            return false;
         }
         if (null != left.TargetMg && null != right.TargetMg)
         {
            if (left.TargetMg.Name != right.TargetMg.Name)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.TargetMg.Name != right.TargetMg.Name");
               return false;
            }
         }
         //------------------------------------------------------------
         if (null == left.ShermanHvss && null != right.ShermanHvss)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ShermanHvss=null");
            return false;
         }
         if (null != left.ShermanHvss && null == right.ShermanHvss)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.ShermanHvss=null");
            return false;
         }
         if (null != left.ShermanHvss && null != right.ShermanHvss)
         {
            if (left.ShermanHvss.Name != right.ShermanHvss.Name)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ShermanHvss.Name != right.ShermanHvss.Name");
               return false;
            }
         }
         //------------------------------------------------------------
         if (null == left.ReturningCrewman && null != right.ReturningCrewman)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ReturningCrewman=null");
            return false;
         }
         if (null != left.ReturningCrewman && null == right.ReturningCrewman)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.ReturningCrewman=null");
            return false;
         }
         if (null != left.ReturningCrewman && null != right.ReturningCrewman)
         {
            if (left.ReturningCrewman.Name != right.ReturningCrewman.Name)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ReturningCrewman.Name != right.ReturningCrewman.Name");
               return false;
            }
         }
         //------------------------------------------------------------
         if (left.CounterattachRetreats.Count != right.CounterattachRetreats.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.CounterattachRetreats.Count != right.CounterattachRetreats.Count");
            return false;
         }

         return true;
      }
      private bool IsEqual(IMapItem? left, IMapItem? right)
      {
         if (null == left)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left=null");
            return false;
         }
         if (null == right)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right=null");
            return false;
         }
         if( left.Name != right.Name )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Name != right.Name");
            return false;
         }
         return true;
      }
   }
}
