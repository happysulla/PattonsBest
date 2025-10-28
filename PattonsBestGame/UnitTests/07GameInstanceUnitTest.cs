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
         myHeaderNames.Add("07-Check Game Save/Load");
         myHeaderNames.Add("07-Finish");
         //------------------------------------
         myCommandNames.Add("Show Results");
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
               report.AmmoPeriscope = i + 130;
               report.MainGunHE = i + 140;
               report.MainGunAP = i + 150;
               report.MainGunWP = i + 160;
               report.MainGunHBCI = i + 170;
               report.MainGunHVAP = i + 180;
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
            }
            //-----------------------
            gameInstance.Day = 01;
            gameInstance.BattlePhase = BattlePhase.Ambush;
            gameInstance.CrewActionPhase = CrewActionPhase.CrewSwitch;
            gameInstance.MovementEffectOnSherman = "04";
            gameInstance.MovementEffectOnEnemy = "05";
            gameInstance.FiredAmmoType = "06";
            //-----------------------
            for(int i=0; i<3; ++i )
            {
               string miName = "Test" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem mi = new MapItem(miName, );
               gi.NewMembers.Add(mi);
            }
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
