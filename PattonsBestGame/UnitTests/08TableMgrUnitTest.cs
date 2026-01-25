using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Pattons_Best
{
   internal class TableMgrUnitTest : IUnitTest
   {
      //--------------------------------------------------------------------
      private IGameInstance? myGameInstance = null;
      private GameViewerWindow? myGameViewerWindow = null;
      private DockPanel? myDockPanelTop = null;
      private CanvasImageViewer? myCanvasImageViewer = null;
      private ScrollViewer? myScrollViewerCanvas = null;
      private Canvas? myCanvasMain = null;
      private IMapItem? myATG = null;
      private IMapItem? mySPG = null;
      private IMapItem? myPzIV = null;
      private IMapItem? myTank = null;
      //--------------------------------------------------------------------
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public bool CtorError { get; } = false;
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      public TableMgrUnitTest(DockPanel dp, IGameInstance gi, CanvasImageViewer civ, GameViewerWindow gvw)
      {
         //------------------------------------
         myIndexName = 0;
         myHeaderNames.Add("08-Show Battle Map");
         myHeaderNames.Add("08-1-ToHitNumberModsYourTank");
         myHeaderNames.Add("08-2-ToHitNumberModsYourTank");
         myHeaderNames.Add("08-3-ToHitNumberModsYourTank");
         myHeaderNames.Add("08-4-ToHitNumberModsYourTank");
         myHeaderNames.Add("08-Finish");
         //------------------------------------
         myCommandNames.Add("Show Battle Map");
         myCommandNames.Add("ATG");
         myCommandNames.Add("SPG");
         myCommandNames.Add("PzIV");
         myCommandNames.Add("TANK");
         myCommandNames.Add("Finish");
         //------------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgrUnitTest(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //------------------------------------
         if (null == gvw)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgrUnitTest(): gvw=null");
            CtorError = true;
            return;
         }
         myGameViewerWindow = gvw;
         //------------------------------------
         if (null == civ)
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgrUnitTest(): civ=null");
            CtorError = true;
            return;
         }
         myCanvasImageViewer = civ;
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
                        myCanvasMain = (Canvas)myScrollViewerCanvas.Content;  // Find the Canvas in the visual tree
                  }
               }
            }
         }
         if (null == myCanvasMain) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "TableMgrUnitTest(): myCanvas=null");
            CtorError = true;
            return;
         }
      }
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myGameInstance=null");
            return false;
         }
         if (null == myCanvasImageViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasImageViewer=null");
            return false;
         }
         if (null == myGameViewerWindow)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myGameViewerWindow=null");
            return false;
         }
         if (null == myDockPanelTop)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myDockPanelTop=null");
            return false;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasMain=null");
            return false;
         }
         if (null == myScrollViewerCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myScrollViewerCanvas=null");
            return false;
         }
         //---------------------------------------------------
         if (CommandName == myCommandNames[0])
         {
            List<UIElement> elements = new List<UIElement>();
            foreach (UIElement ui in myCanvasMain.Children)
            {
               if (ui is Image img)
                  elements.Add(ui);
            }
            foreach (UIElement ui1 in elements)
               myCanvasMain.Children.Remove(ui1);
            CanvasImageViewer.theMainImage = EnumMainImage.MI_Battle;
            myCanvasImageViewer.ShowBattleMap(false, myCanvasMain);
            //------------------------------------
            string shermanName = "Sherman75" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            gi.Sherman = new MapItem(shermanName, 2.0, "t01", gi.Home);
            myGameInstance.BattleStacks.Add(gi.Sherman);
            //------------------------------------
            string name = "ATG" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            ITerritory? t = Territories.theTerritories.Find("B2M");
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t=null for B6M");
               return false;
            }
            myATG = new MapItem(name, Utilities.ZOOM + 0.1, "c76UnidentifiedAtg", t);
            IMapPoint mp = Territory.GetRandomPoint(t, myATG.Zoom * Utilities.theMapItemOffset);
            myATG.Location = mp;
            myGameInstance.BattleStacks.Add(myATG);
            if (false == myATG.SetMapItemRotation(myGameInstance.Sherman))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Set_MapItemRotation() returned false");
               return false;
            }
            if (false == myATG.UpdateMapRotation("Side"))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Update_MapRotation() returned false");
               return false;
            }
            //------------------------------------
            name = "SPG" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            t = Territories.theTerritories.Find("B3M");
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t=null for B4M");
               return false;
            }
            mySPG = new MapItem(name, Utilities.ZOOM + 0.5, "c77UnidentifiedSpg", t);
            mp = Territory.GetRandomPoint(t, mySPG.Zoom * Utilities.theMapItemOffset);
            mySPG.Location = mp;
            myGameInstance.BattleStacks.Add(mySPG);
            if (false == mySPG.SetMapItemRotation(myGameInstance.Sherman))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Set_MapItemRotation() returned false");
               return false;
            }
            if (false == mySPG.UpdateMapRotation("Rear"))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Update_MapRotation() returned false");
               return false;
            }
            //------------------------------------
            name = "PzIV" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            t = Territories.theTerritories.Find("B2M");
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t=null for B9M");
               return false;
            }
            myPzIV = new MapItem(name, Utilities.ZOOM + 0.5, "c79PzIV", t);
            mp = Territory.GetRandomPoint(t, myPzIV.Zoom * Utilities.theMapItemOffset);
            myPzIV.Location = mp;
            myGameInstance.BattleStacks.Add(myPzIV);
            if (false == myPzIV.SetMapItemRotation(myGameInstance.Sherman))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Set_MapItemRotation() returned false");
               return false;
            }
            if (false == myPzIV.SetMapItemRotationTurret(myGameInstance.Sherman))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Set_MapItemRotation() returned false");
               return false;
            }
            //------------------------------------
            name = "TANK" + Utilities.MapItemNum.ToString();
            Utilities.MapItemNum++;
            t = Territories.theTerritories.Find("B1M");
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t=null for B9M");
               return false;
            }
            myTank = new MapItem(name, Utilities.ZOOM + 0.5, "c78UnidentifiedTank", t);
            mp = Territory.GetRandomPoint(t, myTank.Zoom * Utilities.theMapItemOffset);
            myTank.Location = mp;
            myGameInstance.BattleStacks.Add(myTank);
            if (false == myTank.SetMapItemRotation(myGameInstance.Sherman))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Set_MapItemRotation() returned false");
               return false;
            }
            if (false == myTank.UpdateMapRotation("Rear"))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Update_MapRotation() returned false");
               return false;
            }
            ++myIndexName;
         } 
         //-----------------------------------------
         else if (CommandName == myCommandNames[1])
         {
            if (null == myATG)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): myATG=null");
               return false;
            }
            Option option = gi.Options.Find("AtgCoveredArc");
            option.IsEnabled = true;
            int modifier = TableMgr.GetEnemyToHitNumberModifierForYourTank(myGameInstance, myATG);
            GameAction outAction = GameAction.UpdateBattleBoard;
            myGameViewerWindow.UpdateView(ref myGameInstance, outAction);
         }
         //-----------------------------------------
         else if (CommandName == myCommandNames[2])
         {
            if (null == mySPG)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): mySPG=null");
               return false;
            }
            Option option = gi.Options.Find("SpgCoveredArc");
            option.IsEnabled = true;
            int modifier = TableMgr.GetEnemyToHitNumberModifierForYourTank(myGameInstance, mySPG);
            GameAction outAction = GameAction.UpdateBattleBoard;
            myGameViewerWindow.UpdateView(ref myGameInstance, outAction);
         }
         else if (CommandName == myCommandNames[3])
         {
            if (null == myPzIV)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): myPzIV=null");
               return false;
            }
            Option option = gi.Options.Find("TankCoveredArc");
            option.IsEnabled = true;
            int modifier = TableMgr.GetEnemyToHitNumberModifierForYourTank(myGameInstance, myPzIV);
            GameAction outAction = GameAction.UpdateBattleBoard;
            myGameViewerWindow.UpdateView(ref myGameInstance, outAction);
         }
         else if (CommandName == myCommandNames[4])
         {
            if (null == myTank)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): myTank=null");
               return false;
            }
            Option option = gi.Options.Find("SlowTransverseCoveredArc");
            option.IsEnabled = true;
            int modifier = TableMgr.GetEnemyToHitNumberModifierForYourTank(myGameInstance, myTank);
            GameAction outAction = GameAction.UpdateBattleBoard;
            myGameViewerWindow.UpdateView(ref myGameInstance, outAction);
         }
         else if (CommandName == myCommandNames[5])
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
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myCanvasMain=null");
            return false;
         }
         //--------------------------------------
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
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[4])
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
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvasMain=null");
            return false;
         }
         CleanCanvas(gi, myCanvasMain);
         //--------------------------------------------------
         Application.Current.Shutdown();
         return true;
      }
      //--------------------------------------------------------------------
      private void CleanCanvas(IGameInstance gi, Canvas canvas)
      {
         //--------------------------------------------------
         // Remove any existing UI elements from the Canvas
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in canvas.Children)
         {
            if (ui is Polygon polygon)
               elements.Add(ui);
            if (ui is Polyline polyline)
               elements.Add(ui);
            if (ui is Ellipse ellipse)
               elements.Add(ui);
            if (ui is Image img)
            {
               if (true == img.Name.Contains("Map")) 
                  continue;
               elements.Add(ui);
            }
            if (ui is TextBlock tb)
               elements.Add(ui);
            if (ui is Button button)
            {
               if (true == button.Name.Contains("Die"))  // die buttons never disappear - only one copy of them
                  continue;
               gi.BattleStacks.Remove(button.Name);
            }
         }
         foreach (UIElement ui1 in elements)
            canvas.Children.Remove(ui1);
      }
   }
}
