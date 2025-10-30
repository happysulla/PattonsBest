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
using System.Xml.Linq;

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
            if( false == SaveGame())
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): SaveGame() returned false");
               return false;
            }
            if (null == myGameInstanceSave)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): myGameInstanceSave=null");
               return false;
            }
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
         else if (CommandName == myCommandNames[2])
         {
            if (false == IsEqual(myGameInstanceSave, myGameInstanceLoad))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): IsEqual() returned false");
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
      private bool SaveGame()
      {
         myGameInstanceSave = new GameInstance();
         myGameInstanceSave.EventActive = "e001";
         myGameInstanceSave.EventDisplayed = "e002";
         myGameInstanceSave.Day = 01;
         myGameInstanceSave.GameTurn = 02;
         myGameInstanceSave.GamePhase = GamePhase.UnitTest;
         myGameInstanceSave.EndGameReason = "03";
         //-----------------------
         for (int i = 0; i < 3; ++i)
         {
            IAfterActionReport report = new AfterActionReport();
            report.Day = TableMgr.GetDate(i + 1);
            switch (i)
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
         return true;
      }
      private bool IsEqual(IGameInstance? left, IGameInstance? right)
      {
         if( null == left )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IGameInstance): left=null");
            return false;
         }
         if (null == right )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IGameInstance): right=null");
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
         if (false == IsEqual(left.Reports, right.Reports))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(Reports)");
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
         //------------------------------------------------------------
         if (false == IsEqual(left.ReadyRacks, right.ReadyRacks))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(ReadyRacks)");
            return false;
         }
         if (false == IsEqual(left.Hatches, right.Hatches))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(Hatches)");
            return false;
         }
         if (false == IsEqual(left.CrewActions, right.CrewActions))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(CrewActions)");
            return false;
         }
         if (false == IsEqual(left.Targets, right.Targets))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(Targets)");
            return false;
         }
         if (false == IsEqual(left.AdvancingEnemies, right.AdvancingEnemies))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(AdvancingEnemies)");
            return false;
         }
         if (false == IsEqual(left.ShermanAdvanceOrRetreatEnemies, right.ShermanAdvanceOrRetreatEnemies))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(AdvancingEnemies)");
            return false;
         }
         //------------------------------------------------------------------
         if (false == IsEqual(left.NewMembers, right.NewMembers))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(NewMembers)");
            return false;
         }
         if (false == IsEqual(left.InjuredCrewMembers, right.InjuredCrewMembers))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(InjuredCrewMembers)");
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
            if (false == IsEqual(left.TargetMainGun, right.TargetMainGun))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.TargetMainGun != right.TargetMainGun");
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
            if (false == IsEqual(left.TargetMg, right.TargetMg))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.TargetMg != right.TargetMg");
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
            if (false == IsEqual(left.ShermanHvss, right.ShermanHvss))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ShermanHvss != right.TargetMg");
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
            if (false == IsEqual(left.ReturningCrewman, right.ReturningCrewman))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(ICrewMember) returned false");
               return false;
            }
         }
         //------------------------------------------------------------
         if (false == IsEqual(left.AreaTargets, right.AreaTargets))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(AreaTargets) returned false");
            return false;
         }
         if (false == IsEqual(left.CounterattachRetreats, right.CounterattachRetreats))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): IsEqual(CounterattachRetreats) returned false");
            return false;
         }
         //------------------------------------------------------------
         if (null == left.EnemyStrengthCheckTerritory && null != right.EnemyStrengthCheckTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EnemyStrengthCheckTerritory=null");
            return false;
         }
         if (null != left.EnemyStrengthCheckTerritory && null == right.EnemyStrengthCheckTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.EnemyStrengthCheckTerritory=null");
            return false;
         }
         if (null != left.EnemyStrengthCheckTerritory && null != right.EnemyStrengthCheckTerritory)
         {
            if (left.EnemyStrengthCheckTerritory != right.EnemyStrengthCheckTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EnemyStrengthCheckTerritory != right.EnemyStrengthCheckTerritory");
               return false;
            }
         }
         //------------------------------------------------------------
         if (null == left.ArtillerySupportCheck && null != right.ArtillerySupportCheck)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ArtillerySupportCheck=null");
            return false;
         }
         if (null != left.ArtillerySupportCheck && null == right.ArtillerySupportCheck)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.ArtillerySupportCheck=null");
            return false;
         }
         if (null != left.ArtillerySupportCheck && null != right.ArtillerySupportCheck)
         {
            if (left.ArtillerySupportCheck != right.ArtillerySupportCheck)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ArtillerySupportCheck != right.ArtillerySupportCheck");
               return false;
            }
         }
         //------------------------------------------------------------
         if (null == left.AirStrikeCheckTerritory && null != right.AirStrikeCheckTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.AirStrikeCheckTerritory=null");
            return false;
         }
         if (null != left.AirStrikeCheckTerritory && null == right.AirStrikeCheckTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.AirStrikeCheckTerritory=null");
            return false;
         }
         if (null != left.AirStrikeCheckTerritory && null != right.AirStrikeCheckTerritory)
         {
            if (left.AirStrikeCheckTerritory != right.AirStrikeCheckTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.AirStrikeCheckTerritory != right.AirStrikeCheckTerritory");
               return false;
            }
         }
         //------------------------------------------------------------
         if (null == left.EnteredArea && null != right.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EnteredArea=null");
            return false;
         }
         if (null != left.EnteredArea && null == right.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.EnteredArea=null");
            return false;
         }
         if (null != left.EnteredArea && null != right.EnteredArea)
         {
            if (left.EnteredArea != right.EnteredArea)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EnteredArea != right.EnteredArea");
               return false;
            }
         }
         //------------------------------------------------------------
         if (null == left.AdvanceFire && null != right.AdvanceFire)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.AdvanceFire=null");
            return false;
         }
         if (null != left.AdvanceFire && null == right.AdvanceFire)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.AdvanceFire=null");
            return false;
         }
         if (null != left.AdvanceFire && null != right.AdvanceFire)
         {
            if (left.AdvanceFire != right.AdvanceFire)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.AdvanceFire != right.AdvanceFire");
               return false;
            }
         }
         //------------------------------------------------------------
         if (null == left.FriendlyAdvance && null != right.FriendlyAdvance)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.FriendlyAdvance=null");
            return false;
         }
         if (null != left.FriendlyAdvance && null == right.FriendlyAdvance)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.FriendlyAdvance=null");
            return false;
         }
         if (null != left.FriendlyAdvance && null != right.FriendlyAdvance)
         {
            if (left.FriendlyAdvance != right.FriendlyAdvance)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.FriendlyAdvance != right.FriendlyAdvance");
               return false;
            }
         }
         //------------------------------------------------------------
         if (null == left.EnemyAdvance && null != right.EnemyAdvance)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EnemyAdvance=null");
            return false;
         }
         if (null != left.EnemyAdvance && null == right.EnemyAdvance)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.EnemyAdvance=null");
            return false;
         }
         if (null != left.EnemyAdvance && null != right.EnemyAdvance)
         {
            if (left.EnemyAdvance != right.EnemyAdvance)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EnemyAdvance != right.EnemyAdvance");
               return false;
            }
         }
         //------------------------------------------------------------
         if (left.IsHatchesActive != right.IsHatchesActive)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsHatchesActive != right.IsHatchesActive");
            return false;
         }
         if (left.IsRetreatToStartArea != right.IsRetreatToStartArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsRetreatToStartArea != right.IsRetreatToStartArea");
            return false;
         }
         if (left.IsShermanAdvancingOnMoveBoard != right.IsShermanAdvancingOnMoveBoard)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanAdvancingOnMoveBoard != right.IsShermanAdvancingOnMoveBoard");
            return false;
         }
         //------------------------------------------------------------
         if (left.SwitchedCrewMember != right.SwitchedCrewMember)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.SwitchedCrewMember != right.SwitchedCrewMember");
            return false;
         }
         if (left.AssistantOriginalRating != right.AssistantOriginalRating)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.AssistantOriginalRating != right.SwitchedCrewMember");
            return false;
         }
         if (left.IsShermanFiringAtFront != right.IsShermanFiringAtFront)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanFiringAtFront != right.IsShermanFiringAtFront");
            return false;
         }
         if (left.IsShermanDeliberateImmobilization != right.IsShermanDeliberateImmobilization)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanDeliberateImmobilization != right.IsShermanDeliberateImmobilization");
            return false;
         }
         if (left.ShermanTypeOfFire != right.ShermanTypeOfFire)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ShermanTypeOfFire != right.ShermanTypeOfFire");
            return false;
         }
         if (left.NumSmokeAttacksThisRound != right.NumSmokeAttacksThisRound)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.NumSmokeAttacksThisRound != right.NumSmokeAttacksThisRound");
            return false;
         }
         if (left.IsMalfunctionedMainGun != right.IsMalfunctionedMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsMalfunctionedMainGun != right.IsMalfunctionedMainGun");
            return false;
         }
         if (left.IsMainGunRepairAttempted != right.IsMainGunRepairAttempted)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsMainGunRepairAttempted != right.IsMainGunRepairAttempted");
            return false;
         }
         if (left.IsBrokenMainGun != right.IsBrokenMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBrokenMainGun != right.IsBrokenMainGun");
            return false;
         }
         if (left.IsBrokenGunSight != right.IsBrokenGunSight)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBrokenGunSight != right.IsBrokenGunSight");
            return false;
         }
         if (left.IsBrokenGunSight != right.IsBrokenGunSight)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBrokenGunSight != right.IsBrokenGunSight");
            return false;
         }
         //---------------------------------------------------
         if( false == IsEqual(left.FirstShots, right.FirstShots))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.FirstShots != right.FirstShots");
            return false;
         }
         if (false == IsEqual(left.TrainedGunners, right.TrainedGunners))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.TrainedGunners != right.TrainedGunners");
            return false;
         }
         if (false == IsEqual(left.ShermanHits, right.ShermanHits))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ShermanHits != right.ShermanHits");
            return false;
         }
         if (false == IsEqual(left.Death, right.Death))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Death != right.Death");
            return false;
         }
         //---------------------------------------------------
         if (left.IdentifiedTank != right.IdentifiedTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IdentifiedTank != right.IdentifiedTank");
            return false;
         }
         if (left.IdentifiedAtg != right.IdentifiedAtg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IdentifiedAtg != right.IdentifiedAtg");
            return false;
         }
         if (left.IdentifiedSpg != right.IdentifiedSpg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IdentifiedSpg != right.IdentifiedSpg");
            return false;
         }
         //---------------------------------------------------
         if (left.IsShermanFiringAaMg != right.IsShermanFiringAaMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanFiringAaMg != right.IsShermanFiringAaMg");
            return false;
         }
         if (left.IsShermanFiringBowMg != right.IsShermanFiringBowMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanFiringBowMg != right.IsShermanFiringBowMg");
            return false;
         }
         if (left.IsShermanFiringCoaxialMg != right.IsShermanFiringCoaxialMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanFiringCoaxialMg != right.IsShermanFiringCoaxialMg");
            return false;
         }
         if (left.IsShermanFiringSubMg != right.IsShermanFiringSubMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanFiringSubMg != right.IsShermanFiringSubMg");
            return false;
         }
         if (left.IsCommanderDirectingMgFire != right.IsCommanderDirectingMgFire)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsCommanderDirectingMgFire != right.IsCommanderDirectingMgFire");
            return false;
         }
         if (left.IsShermanFiredAaMg != right.IsShermanFiredAaMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanFiredAaMg != right.IsShermanFiredAaMg");
            return false;
         }
         if (left.IsShermanFiredBowMg != right.IsShermanFiredBowMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanFiredBowMg != right.IsShermanFiredBowMg");
            return false;
         }
         if (left.IsShermanFiredCoaxialMg != right.IsShermanFiredCoaxialMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanFiredCoaxialMg != right.IsShermanFiredCoaxialMg");
            return false;
         }
         if (left.IsShermanFiredSubMg != right.IsShermanFiredSubMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanFiredSubMg != right.IsShermanFiredSubMg");
            return false;
         }
         if (left.IsMalfunctionedMgCoaxial != right.IsMalfunctionedMgCoaxial)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsMalfunctionedMgCoaxial != right.IsMalfunctionedMgCoaxial");
            return false;
         }
         if (left.IsMalfunctionedMgBow != right.IsMalfunctionedMgBow)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsMalfunctionedMgBow != right.IsMalfunctionedMgBow");
            return false;
         }
         if (left.IsMalfunctionedMgAntiAircraft != right.IsMalfunctionedMgAntiAircraft)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsMalfunctionedMgAntiAircraft != right.IsMalfunctionedMgAntiAircraft");
            return false;
         }
         if (left.IsCoaxialMgRepairAttempted != right.IsCoaxialMgRepairAttempted)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsCoaxialMgRepairAttempted != right.IsCoaxialMgRepairAttempted");
            return false;
         }
         if (left.IsBowMgRepairAttempted != right.IsBowMgRepairAttempted)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBowMgRepairAttempted != right.IsBowMgRepairAttempted");
            return false;
         }
         if (left.IsAaMgRepairAttempted != right.IsAaMgRepairAttempted)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsAaMgRepairAttempted != right.IsAaMgRepairAttempted");
            return false;
         }
         if (left.IsBrokenMgAntiAircraft != right.IsBrokenMgAntiAircraft)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBrokenMgAntiAircraft != right.IsBrokenMgAntiAircraft");
            return false;
         }
         if (left.IsBrokenMgBow != right.IsBrokenMgBow)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBrokenMgBow != right.IsBrokenMgBow");
            return false;
         }
         if (left.IsBrokenMgCoaxial != right.IsBrokenMgCoaxial)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBrokenMgCoaxial != right.IsBrokenMgCoaxial");
            return false;
         }
         if (left.IsBrokenPeriscopeDriver != right.IsBrokenPeriscopeDriver)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBrokenPeriscopeDriver != right.IsBrokenPeriscopeDriver");
            return false;
         }
         if (left.IsBrokenPeriscopeLoader != right.IsBrokenPeriscopeLoader)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBrokenPeriscopeLoader != right.IsBrokenPeriscopeLoader");
            return false;
         }
         if (left.IsBrokenPeriscopeAssistant != right.IsBrokenPeriscopeAssistant)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBrokenPeriscopeAssistant != right.IsBrokenPeriscopeAssistant");
            return false;
         }
         if (left.IsBrokenPeriscopeGunner != right.IsBrokenPeriscopeGunner)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBrokenPeriscopeGunner != right.IsBrokenPeriscopeGunner");
            return false;
         }
         if (left.IsBrokenPeriscopeCommander != right.IsBrokenPeriscopeCommander)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsBrokenPeriscopeCommander != right.IsBrokenPeriscopeCommander");
            return false;
         }
         //---------------------------------------------------
         if (left.IsShermanTurretRotated != right.IsShermanTurretRotated)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsShermanTurretRotated != right.IsShermanTurretRotated");
            return false;
         }
         if (left.ShermanRotationTurretOld != right.ShermanRotationTurretOld)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ShermanRotationTurretOld != right.ShermanRotationTurretOld");
            return false;
         }
         //---------------------------------------------------
         if (left.IsCounterattackAmbush != right.IsCounterattackAmbush)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsCounterattackAmbush != right.IsCounterattackAmbush");
            return false;
         }
         if (left.IsLeadTank != right.IsLeadTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsLeadTank != right.IsLeadTank");
            return false;
         }
         if (left.IsAirStrikePending != right.IsAirStrikePending)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsAirStrikePending != right.IsAirStrikePending");
            return false;
         }
         if (left.IsAdvancingFireChosen != right.IsAdvancingFireChosen)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsAdvancingFireChosen != right.IsAdvancingFireChosen");
            return false;
         }
         if (left.AdvancingFireMarkerCount != right.AdvancingFireMarkerCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.AdvancingFireMarkerCount != right.AdvancingFireMarkerCount");
            return false;
         }
         if (left.BattleResistance != right.BattleResistance)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.BattleResistance != right.BattleResistance");
            return false;
         }
         //---------------------------------------------------
         if (left.IsMinefieldAttack != right.IsMinefieldAttack)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsMinefieldAttack != right.IsMinefieldAttack");
            return false;
         }
         if (left.IsHarrassingFireBonus != right.IsHarrassingFireBonus)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsHarrassingFireBonus != right.IsHarrassingFireBonus");
            return false;
         }
         if (left.IsFlankingFire != right.IsFlankingFire)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsFlankingFire != right.IsFlankingFire");
            return false;
         }
         if (left.IsEnemyAdvanceComplete != right.IsEnemyAdvanceComplete)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsEnemyAdvanceComplete != right.IsEnemyAdvanceComplete");
            return false;
         }
         if (false == IsEqual(left.Panzerfaust, right.Panzerfaust))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Panzerfaust != right.Panzerfaust");
            return false;
         }
         if (left.NumCollateralDamage != right.NumCollateralDamage)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.NumCollateralDamage != right.NumCollateralDamage");
            return false;
         }
         //---------------------------------------------------
         if (left.TankReplacementNumber != right.TankReplacementNumber)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.TankReplacementNumber != right.TankReplacementNumber");
            return false;
         }
         if (left.VictoryPtsTotalCampaign != right.VictoryPtsTotalCampaign)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsTotalCampaign != right.VictoryPtsTotalCampaign");
            return false;
         }
         if (left.PromotionPointNum != right.PromotionPointNum)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.PromotionPointNum != right.PromotionPointNum");
            return false;
         }
         if (left.PromotionDay != right.PromotionDay)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.PromotionDay != right.PromotionDay");
            return false;
         }
         if (left.NumPurpleHeart != right.NumPurpleHeart)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.NumPurpleHeart != right.NumPurpleHeart");
            return false;
         }
         if (left.IsCommanderRescuePerformed != right.IsCommanderRescuePerformed)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsCommanderRescuePerformed != right.IsCommanderRescuePerformed");
            return false;
         }
         if (left.IsCommanderKilled != right.IsCommanderKilled)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsCommanderKilled != right.IsCommanderKilled");
            return false;
         }
         if (left.IsPromoted != right.IsPromoted)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsPromoted != right.IsPromoted");
            return false;
         }
         //---------------------------------------------------
         if (false == IsEqual(left.MapItemMoves, right.MapItemMoves))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.MapItemMoves != right.MapItemMoves");
            return false;
         }
         if (false == IsEqual(left.MoveStacks, right.MoveStacks))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.MoveStacks != right.MoveStacks");
            return false;
         }
         if (false == IsEqual(left.BattleStacks, right.BattleStacks))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.BattleStacks != right.BattleStacks");
            return false;
         }
         if (false == IsEqual(left.EnteredHexes, right.EnteredHexes))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.BattleStacks != right.BattleStacks");
            return false;
         }
         return true;
      }
      private bool IsEqual(IAfterActionReports left, IAfterActionReports right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Count != right.Count");
            return false;
         }
         for (int i = 0; i < left.Count; ++i)
         {
            IAfterActionReport? lReport = left[i];
            if( null == lReport )
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lReport=null");
               return false;
            }
            IAfterActionReport? rReport = right[i];
            if( null == rReport )
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): rReport=null");
               return false;
            }
            if ( false == IsEqual(lReport, rReport))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lReport != rReport");
               return false;
            }
         }
         return true;
      }
      private bool IsEqual( IAfterActionReport left, IAfterActionReport right )
      {
         if (left.Day != right.Day)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Day != right.Day");
            return false;
         }
         if (left.Scenario != right.Scenario)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Scenario != right.Scenario");
            return false;
         }
         if (left.Probability != right.Probability)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Probability != right.Probability");
            return false;
         }
         if (left.Resistance != right.Resistance)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Resistance != right.Resistance");
            return false;
         }
         if (left.Name != right.Name)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Name != right.Name");
            return false;
         }
         if (left.TankCardNum != right.TankCardNum)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.TankCardNum != right.TankCardNum");
            return false;
         }
         if (left.Weather != right.Weather)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Weather != right.Weather");
            return false;
         }
         //------------------------------------
         if (false == IsEqual(left.Commander, right.Commander))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lReport.Commander != rReport.Commander");
            return false;
         }
         if (false == IsEqual(left.Gunner, right.Gunner))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lReport.Gunner != rReport.Gunner");
            return false;
         }
         if (false == IsEqual(left.Loader, right.Loader))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lReport.Loader != rReport.Loader");
            return false;
         }
         if (false == IsEqual(left.Driver, right.Driver))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lReport.Driver != rReport.Driver");
            return false;
         }
         if (false == IsEqual(left.Assistant, right.Assistant))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lReport.Assistant != rReport.CommaAssistantnder");
            return false;
         }
         //------------------------------------
         if (left.SunriseHour != right.SunriseHour)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.SunriseHour != right.SunriseHour");
            return false;
         }
         if (left.SunriseMin != right.SunriseMin)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.SunriseMin != right.SunriseMin");
            return false;
         }
         if (left.SunsetHour != right.SunsetHour)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.SunsetHour != right.SunsetHour");
            return false;
         }
         if (left.SunsetHour != right.SunsetMin)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.SunsetMin != right.SunsetMin");
            return false;
         }
         //------------------------------------
         if (left.Ammo30CalibreMG != right.Ammo30CalibreMG)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Ammo30CalibreMG != right.Ammo30CalibreMG");
            return false;
         }
         if (left.Ammo50CalibreMG != right.Ammo50CalibreMG)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Ammo50CalibreMG != right.Ammo50CalibreMG");
            return false;
         }
         if (left.AmmoSmokeBomb != right.AmmoSmokeBomb)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.AmmoSmokeBomb != right.AmmoSmokeBomb");
            return false;
         }
         if (left.AmmoSmokeGrenade != right.AmmoSmokeGrenade)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.AmmoSmokeGrenade != right.AmmoSmokeGrenade");
            return false;
         }
         if (left.AmmoPeriscope != right.AmmoPeriscope)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.AmmoPeriscope != right.AmmoPeriscope");
            return false;
         }
         if (left.MainGunHE != right.MainGunHE)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.MainGunHE != right.MainGunHE");
            return false;
         }
         if (left.MainGunAP != right.MainGunAP)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.MainGunAP != right.MainGunAP");
            return false;
         }
         if (left.MainGunWP != right.MainGunWP)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.MainGunWP != right.MainGunWP");
            return false;
         }
         if (left.MainGunHBCI != right.MainGunHBCI)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.MainGunHBCI != right.MainGunHBCI");
            return false;
         }
         if (left.MainGunHVAP != right.MainGunHVAP)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.MainGunHVAP != right.MainGunHVAP");
            return false;
         }
         //------------------------------------
         if (left.VictoryPtsFriendlyKiaLightWeapon != right.VictoryPtsFriendlyKiaLightWeapon)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsFriendlyKiaLightWeapon != right.VictoryPtsFriendlyKiaLightWeapon");
            return false;
         }
         if (left.VictoryPtsFriendlyKiaTruck != right.VictoryPtsFriendlyKiaTruck)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsFriendlyKiaTruck != right.VictoryPtsFriendlyKiaTruck");
            return false;
         }
         if (left.VictoryPtsFriendlyKiaSpwOrPsw != right.VictoryPtsFriendlyKiaSpwOrPsw)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsFriendlyKiaSpwOrPsw != right.VictoryPtsFriendlyKiaSpwOrPsw");
            return false;
         }
         if (left.VictoryPtsFriendlyKiaSPGun != right.VictoryPtsFriendlyKiaSPGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsFriendlyKiaSPGun != right.VictoryPtsFriendlyKiaSPGun");
            return false;
         }
         if (left.VictoryPtsFriendlyKiaPzIV != right.VictoryPtsFriendlyKiaPzIV)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsFriendlyKiaPzIV != right.VictoryPtsFriendlyKiaPzIV");
            return false;
         }
         if (left.VictoryPtsFriendlyKiaPzV != right.VictoryPtsFriendlyKiaPzV)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsFriendlyKiaPzV != right.VictoryPtsFriendlyKiaPzV");
            return false;
         }
         if (left.VictoryPtsFriendlyKiaPzVI != right.VictoryPtsFriendlyKiaPzVI)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsFriendlyKiaPzVI != right.VictoryPtsFriendlyKiaPzVI");
            return false;
         }
         if (left.VictoryPtsFriendlyKiaAtGun != right.VictoryPtsFriendlyKiaAtGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsFriendlyKiaAtGun != right.VictoryPtsFriendlyKiaAtGun");
            return false;
         }
         if (left.VictoryPtsFriendlyKiaFortifiedPosition != right.VictoryPtsFriendlyKiaFortifiedPosition)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsFriendlyKiaFortifiedPosition != right.VictoryPtsFriendlyKiaFortifiedPosition");
            return false;
         }
         //------------------------------------
         if (left.VictoryPtsYourKiaLightWeapon != right.VictoryPtsYourKiaLightWeapon)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsYourKiaLightWeapon != right.VictoryPtsYourKiaLightWeapon");
            return false;
         }
         if (left.VictoryPtsYourKiaTruck != right.VictoryPtsYourKiaTruck)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsYourKiaTruck != right.VictoryPtsYourKiaTruck");
            return false;
         }
         if (left.VictoryPtsYourKiaSpwOrPsw != right.VictoryPtsYourKiaSpwOrPsw)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsYourKiaSpwOrPsw != right.VictoryPtsYourKiaSpwOrPsw");
            return false;
         }
         if (left.VictoryPtsYourKiaSPGun != right.VictoryPtsYourKiaSPGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsYourKiaSPGun != right.VictoryPtsYourKiaSPGun");
            return false;
         }
         if (left.VictoryPtsYourKiaPzIV != right.VictoryPtsYourKiaPzIV)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsYourKiaPzIV != right.VictoryPtsYourKiaPzIV");
            return false;
         }
         if (left.VictoryPtsYourKiaPzV != right.VictoryPtsYourKiaPzV)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsYourKiaPzV != right.VictoryPtsYourKiaPzV");
            return false;
         }
         if (left.VictoryPtsYourKiaPzVI != right.VictoryPtsYourKiaPzVI)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsYourKiaPzVI != right.VictoryPtsYourKiaPzVI");
            return false;
         }
         if (left.VictoryPtsYourKiaAtGun != right.VictoryPtsYourKiaAtGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsYourKiaAtGun != right.VictoryPtsYourKiaAtGun");
            return false;
         }
         if (left.VictoryPtsYourKiaFortifiedPosition != right.VictoryPtsYourKiaFortifiedPosition)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsYourKiaFortifiedPosition != right.VictoryPtsYourKiaFortifiedPosition");
            return false;
         }
         //------------------------------------
         if (left.VictoryPtsCaptureArea != right.VictoryPtsCaptureArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsCaptureArea != right.VictoryPtsCaptureArea");
            return false;
         }
         if (left.VictoryPtsCapturedExitArea != right.VictoryPtsCapturedExitArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsCapturedExitArea != right.VictoryPtsCapturedExitArea");
            return false;
         }
         if (left.VictoryPtsLostArea != right.VictoryPtsLostArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsLostArea != right.VictoryPtsLostArea");
            return false;
         }
         if (left.VictoryPtsFriendlyTank != right.VictoryPtsFriendlyTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsFriendlyTank != right.VictoryPtsFriendlyTank");
            return false;
         }
         if (left.VictoryPtsFriendlySquad != right.VictoryPtsFriendlySquad)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsFriendlySquad != right.VictoryPtsFriendlySquad");
            return false;
         }
         //------------------------------------
         if (left.VictoryPtsTotalYourTank != right.VictoryPtsTotalYourTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsTotalYourTank != right.VictoryPtsTotalYourTank");
            return false;
         }
         if (left.VictoryPtsTotalFriendlyForces != right.VictoryPtsTotalFriendlyForces)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsTotalFriendlyForces != right.VictoryPtsTotalFriendlyForces");
            return false;
         }
         if (left.VictoryPtsTotalTerritory != right.VictoryPtsTotalTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsTotalTerritory != right.VictoryPtsTotalTerritory");
            return false;
         }
         if (left.VictoryPtsTotalEngagement != right.VictoryPtsTotalEngagement)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.VictoryPtsTotalEngagement != right.VictoryPtsTotalEngagement");
            return false;
         }
         //------------------------------------
         if (left.Decorations.Count != right.Decorations.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Decorations.Count != right.Decorations.Count");
            return false;
         }
         for( int i=0; i<left.Decorations.Count; ++i )
         {
            EnumDecoration lDecoration = left.Decorations[i];
            EnumDecoration rDecoration = right.Decorations[i];
            if (lDecoration != rDecoration)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lDecoration != rDecoration");
               return false;
            }
         }
         if (left.Notes.Count != right.Notes.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Notes.Count != right.Notes.Count");
            return false;
         }
         for (int i = 0; i < left.Notes.Count; ++i)
         {
            string? lNote = left.Notes[i];
            string? rNote = right.Notes[i];
            if( null == lNote || null == rNote )
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lNote or rNoe = null");
               return false;
            }
            if (lNote != rNote)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lNote != rNote");
               return false;
            }
         }
         //------------------------------------
         if (left.DayEndedTime != right.DayEndedTime)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.DayEndedTime != right.DayEndedTime");
            return false;
         }
         if (left.Breakdown != right.Breakdown)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Breakdown != right.Breakdown");
            return false;
         }
         if (left.KnockedOut != right.KnockedOut)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.KnockedOut != right.KnockedOut");
            return false;
         }
         return true;
      }
      private bool IsEqual(IMapItems left, IMapItems right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Count != right.COunt");
            return false;
         }
         for(int i=0; i< left.Count; ++i )
         {
            if( false == IsEqual( left[i], right[i] ))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItems): left[i] != right[i]");
               return false;
            }
         }
         return true;
      }
      private bool IsEqual(IMapItem? left, IMapItem? right)
      {
         if (null == left)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left=null");
            return false;
         }
         if (null == right)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): right=null");
            return false;
         }
         if( left.Name != right.Name )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.Name != right.Name");
            return false;
         }
         if (left.TopImageName != right.TopImageName)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.TopImageName != right.TopImageName");
            return false;
         }
         if (left.BottomImageName != right.BottomImageName)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.BottomImageName != right.BottomImageName");
            return false;
         }
         if (left.OverlayImageName != right.OverlayImageName)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.OverlayImageName != right.OverlayImageName");
            return false;
         }
         if (left.WoundSpots.Count != right.WoundSpots.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMembers): left.WoundSpots.Count != right.WoundSpots.Count");
            return false;
         }
         for( int i=0; i<left.WoundSpots.Count; ++i)
         {
            if (left.WoundSpots[i].mySize != right.WoundSpots[i].mySize)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.mySize != right.mySize");
               return false;
            }
            if (left.WoundSpots[i].myLeft != right.WoundSpots[i].myLeft)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.myLeft != right.myLeft");
               return false;
            }
            if (left.WoundSpots[i].myTop != right.WoundSpots[i].myTop)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.myTop != right.myTop");
               return false;
            }
         }
         if (left.Zoom != right.Zoom)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.Zoom != right.Zoom");
            return false;
         }
         if (left.IsMoved != right.IsMoved)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsMoved != right.IsMoved");
            return false;
         }
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.Count != right.Count");
            return false;
         }
         if (left.Location.X != right.Location.X)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.Location.X != right.Location.X");
            return false;
         }
         if (left.Location.Y != right.Location.Y)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.Location.Y != right.Location.Y");
            return false;
         }
         if (left.RotationOffsetHull != right.RotationOffsetHull)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.RotationOffsetHull != right.RotationOffsetHull");
            return false;
         }
         if (left.RotationHull != right.RotationHull)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.RotationHull != right.RotationHull");
            return false;
         }
         if (left.RotationOffsetTurret != right.RotationOffsetTurret)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.RotationOffsetTurret != right.RotationOffsetTurret");
            return false;
         }
         if (left.RotationTurret != right.RotationTurret)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.RotationOffsetTurret != right.RotationOffsetTurret");
            return false;
         }
         //-------------------------------------------------
         if (left.TerritoryCurrent.Name != right.TerritoryCurrent.Name)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.TerritoryCurrent != right.TerritoryCurrent");
            return false;
         }
         if (left.TerritoryStarting.Name != right.TerritoryStarting.Name)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.TerritoryStarting != right.TerritoryStarting");
            return false;
         }
         //-------------------------------------------------
         if (left.IsMoving != right.IsMoving)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsMoving != right.IsMoving");
            return false;
         }
         if (left.IsHullDown != right.IsHullDown)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsHullDown != right.IsHullDown");
            return false;
         }
         if (left.IsTurret != right.IsTurret)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsTurret != right.IsTurret");
            return false;
         }
         if (left.IsKilled != right.IsKilled)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsKilled != right.IsKilled");
            return false;
         }
         if (left.IsUnconscious != right.IsUnconscious)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsUnconscious != right.IsUnconscious");
            return false;
         }
         if (left.IsIncapacitated != right.IsIncapacitated)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsIncapacitated != right.IsIncapacitated");
            return false;
         }
         if (left.IsFired != right.IsFired)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsFired != right.IsFired");
            return false;
         }
         if (left.IsSpotted != right.IsSpotted)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsSpotted != right.IsSpotted");
            return false;
         }
         //-------------------------------------------------
         //-------------------------------------------------
         if (left.IsVehicle != right.IsVehicle)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsVehicle != right.IsVehicle");
            return false;
         }
         if (left.IsMovingInOpen != right.IsMovingInOpen)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsMovingInOpen != right.IsMovingInOpen");
            return false;
         }
         if (left.IsWoods != right.IsWoods)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsWoods != right.IsWoods");
            return false;
         }
         if (left.IsBuilding != right.IsBuilding)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsBuilding != right.IsBuilding");
            return false;
         }
         if (left.IsFortification != right.IsFortification)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsFortification != right.IsFortification");
            return false;
         }
         if (left.IsThrownTrack != right.IsThrownTrack)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsThrownTrack != right.IsThrownTrack");
            return false;
         }
         if (left.IsBoggedDown != right.IsBoggedDown)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsBoggedDown != right.IsBoggedDown");
            return false;
         }
         if (left.IsAssistanceNeeded != right.IsAssistanceNeeded)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsBoggedDown != right.IsBoggedDown");
            return false;
         }
         //-------------------------------------------------
         if (left.IsHeHit != right.IsHeHit)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsHeHit != right.IsHeHit");
            return false;
         }
         if (left.IsApHit != right.IsApHit)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsApHit != right.IsApHit");
            return false;
         }
         if (left.Spotting != right.Spotting)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.Spotting != right.Spotting");
            return false;
         }
         return true;
      }
      private bool IsEqual(ICrewMembers left, ICrewMembers right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMembers): left.Count != right.COunt");
            return false;
         }
         for (int i = 0; i < left.Count; ++i)
         {
            if (false == IsEqual(left[i], right[i]))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMembers): left[i] != right[i]");
               return false;
            }
         }
         return true;
      }
      private bool IsEqual(ICrewMember? left, ICrewMember? right)
      {
         if (null == left)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMember): left=null");
            return false;
         }
         if (null == right)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMember): right=null");
            return false;
         }
         if (left.Name != right.Name)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMember): left.Name != right.Name");
            return false;
         }
         if (left.Role != right.Role)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMember): left.Role != right.Role");
            return false;
         }
         if (left.Rank != right.Rank)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMember): left.Rank != right.Rank");
            return false;
         }
         if (left.Rating != right.Rating)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMember): left.Rating != right.Rating");
            return false;
         }
         if (left.IsButtonedUp != right.IsButtonedUp)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMember): left.IsButtonedUp != right.IsButtonedUp");
            return false;
         }
         if (left.Action != right.Action)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMember): left.Action != right.Action");
            return false;
         }
         if (left.Wound != right.Wound)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMember): left.Wound != right.Wound");
            return false;
         }
         if (left.WoundDaysUntilReturn != right.WoundDaysUntilReturn)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ICrewMember): left.WoundDaysUntilReturn != right.WoundDaysUntilReturn");
            return false;
         }
         return true;
      }
      private bool IsEqual(ITerritories left, ITerritories right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ITerritories): left.Count != right.COunt");
            return false;
         }
         for (int i = 0; i < left.Count; ++i)
         {
            ITerritory? tLeft = left[i];
            if (null == tLeft)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(ITerritories): tLeft=null");
               return false;
            }
            ITerritory? tRight = right[i];
            if (null == tRight)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(ITerritories): tRight=null");
               return false;
            }
            if (tLeft.Name != tRight.Name)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(ITerritories): tLeft.Name != tRight.Name");
               return false;
            }
         }
         return true;
      }
      private bool IsEqual(List<string> left, List<string> right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(List<string>): left.Count != right.Count");
            return false;
         }
         for (int i = 0; i < left.Count; ++i)
         {
            string? sLeft = left[i];
            if (null == sLeft)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(ITerritories): sLeft=null");
               return false;
            }
            string? sRight = right[i];
            if (null == sRight)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(ITerritories): sRight=null");
               return false;
            }
            if (sLeft != sRight)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(ITerritories): sLeft != sRight");
               return false;
            }
         }
         return true;
      }
      private bool IsEqual(List<ShermanAttack> left, List<ShermanAttack> right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(List<string>): left.Count != right.Count");
            return false;
         }
         return true;
      }
      private bool IsEqual( ShermanDeath? left, ShermanDeath? right )
      {
         if ( (null == left) && (null == right) )
            return true;
         if (null == left)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ShermanDeath): left=null");
            return false;
         }
         if (null == right)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ShermanDeath): right=null");
            return false;
         }
         if (false == IsEqual(left.myEnemyUnit, right.myEnemyUnit))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ShermanDeath): left.myEnemyUnit != right.myEnemyUnit");
            return false;
         }
         if (left.myHitLocation != right.myHitLocation)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myHitLocation != right.myHitLocation");
            return false;
         }
         if (left.myEnemyFireDirection != right.myEnemyFireDirection)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myEnemyFireDirection != right.myEnemyFireDirection");
            return false;
         }
         if (left.myDay != right.myDay)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myDay != right.myDay");
            return false;
         }
         if (left.myCause != right.myCause)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myCause != right.myCause");
            return false;
         }
         if (left.myIsAmbush != right.myIsAmbush)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myIsAmbush != right.myIsAmbush");
            return false;
         }
         if (left.myIsExplosion != right.myIsExplosion)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myIsExplosion != right.myIsExplosion");
            return false;
         }
         if (left.myIsBailout != right.myIsBailout)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myIsBailout != right.myIsBailout");
            return false;
         }
         if (left.myIsBrewUp != right.myIsBrewUp)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myIsBrewUp != right.myIsBrewUp");
            return false;
         }
         return true;
      }
      private bool IsEqual(PanzerfaustAttack? left, PanzerfaustAttack? right)
      {
         if ((null == left) && (null == right))
            return true;
         if (null == left)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(PanzerfaustAttack): left=null");
            return false;
         }
         if (null == right)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(PanzerfaustAttack): right=null");
            return false;
         }
         if (false == IsEqual(left.myEnemyUnit, right.myEnemyUnit))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(PanzerfaustAttack): left.myEnemyUnit != right.myEnemyUnit");
            return false;
         }
         if (left.myDay != right.myDay)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myDay != right.myDay");
            return false;
         }
         if (left.myIsShermanMoving != right.myIsShermanMoving)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myIsShermanMoving != right.myIsShermanMoving");
            return false;
         }
         if (left.myIsLeadTank != right.myIsLeadTank)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myIsLeadTank != right.myIsLeadTank");
            return false;
         }
         if (left.myIsAdvancingFireZone != right.myIsAdvancingFireZone)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.myIsAdvancingFireZone != right.myIsAdvancingFireZone");
            return false;
         }
         if (left.mySector != right.mySector)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.mySector != right.mySector");
            return false;
         }
         return true;
      }
      private bool IsEqual(IMapItemMoves left, IMapItemMoves right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItemMoves): left.Count != right.Count");
            return false;
         }
         return true;
      }
      private bool IsEqual(IStacks left, IStacks right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStacks): left.Count != right.Count");
            return false;
         }
         return true;
      }
      private bool IsEqual(List<EnteredHex> left, List<EnteredHex> right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(List<EnteredHex>): left.Count != right.Count");
            return false;
         }
         for (int i = 0; i < left.Count; ++i)
         {
            EnteredHex lHex = left[i];
            EnteredHex rHex = right[i];
            if (lHex.Identifer != rHex.Identifer)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lHex.Identifer != right.Identifer");
               return false;
            }
            if (lHex.Day != rHex.Day)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lHex.Day != right.Day");
               return false;
            }
            if (lHex.Date != rHex.Date)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lHex.Date != right.Date");
               return false;
            }
            if (lHex.Time != rHex.Time)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lHex.Time != right.Time");
               return false;
            }
            if (lHex.TerritoryName != rHex.TerritoryName)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lHex.TerritoryName != right.TerritoryName");
               return false;
            }
            if (lHex.MapPoint.X != rHex.MapPoint.X)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lHex.MapPoint.x != right.MapPoint.X");
               return false;
            }
            if (lHex.MapPoint.Y != rHex.MapPoint.Y)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lHex.MapPoint.Y != right.MapPoint.Y");
               return false;
            }
            if (lHex.ColorAction != rHex.ColorAction)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): lHex.ColorAction != right.ColorAction");
               return false;
            }
         }
         return true;
      }
   }
}
