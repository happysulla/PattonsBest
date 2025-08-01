﻿
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace Pattons_Best
{
   public enum EnumMainImage
   {
      MI_Battle,
      MI_Move,
      MI_Other
   }
   public class CanvasImageViewer : IView
   {
      public bool CtorError { get; } = false;
      public static EnumMainImage theMainImage = EnumMainImage.MI_Other;
      private Canvas? myCanvas = null;
      private IDieRoller? myDieRoller = null;
      private System.Windows.Input.Cursor? myTargetCursor = null;
      private AfterActionReportUserControl? myAarUserControl = null;
      //-------------------------------------------------
      public CanvasImageViewer(Canvas? c, IDieRoller? dr)
      {
         if (null == c)
         {
            Logger.Log(LogEnum.LE_ERROR, "CanvasImageViewer(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "CanvasImageViewer(): dr=null");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
      }
      //-------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myCanvas=null");
            return;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myDieRoller=null");
            return;
         }
         switch (action)
         {
            case GameAction.UpdateBattleBoard:
               break;
            case GameAction.UpdateStatusBar:
               if (null != myTargetCursor) // increase/decrease size of cursor when zoom in or out
               {
                  myTargetCursor.Dispose();
                  double sizeCursor = Utilities.ZoomCanvas * Utilities.ZOOM * Utilities.theMapItemSize;
                  System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, sizeCursor * 0.5); // set the center of the MapItem as the hot point for the cursor
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c44AdvanceFire"), Width = sizeCursor, Height = sizeCursor };
                  myTargetCursor = Utilities.ConvertToCursor(img1, hotPoint);
                  this.myCanvas.Cursor = myTargetCursor;
               }
               break;
            case GameAction.UpdateNewGame:
            case GameAction.UpdateLoadingGame:
            case GameAction.UpdateUndo:
               if (null != myTargetCursor)
                  myTargetCursor.Dispose();
               myTargetCursor = null;
               this.myCanvas.Cursor = System.Windows.Input.Cursors.Arrow; // get rid of the canvas cursor
               break;
            case GameAction.UpdateAfterActionReport:
               if (null != myAarUserControl)
                  myAarUserControl.UpdateReport();
               return;
            case GameAction.UpdateTankExplosion:
               ShowTankExploding(gi, myCanvas);
               break;
            case GameAction.UpdateTankBrewUp:
               ShowTankBrewUp(gi, myCanvas);
               break;
            case GameAction.RemoveSplashScreen:
               theMainImage = EnumMainImage.MI_Other;
               ShowInitialScreen(myCanvas);
               break;
            case GameAction.SetupShowMapHistorical:
               theMainImage = EnumMainImage.MI_Other;
               ShowHistoricalMap(myCanvas);
               break;
            case GameAction.SetupShowMovementBoard:
            case GameAction.MovementStartAreaSet:
            case GameAction.TestingStartMovement:
            case GameAction.MovementEnemyStrengthChoice:
               theMainImage = EnumMainImage.MI_Move;
               ShowMovementMap(myCanvas);
               break;
            case GameAction.SetupShowBattleBoard:
            case GameAction.PreparationsDeployment:
            case GameAction.TestingStartPreparations:
            case GameAction.TestingStartBattle:
            case GameAction.TestingStartAmbush:
            case GameAction.BattleStart:
            case GameAction.BattleActivation:
               myDieRoller.HideDie();
               theMainImage = EnumMainImage.MI_Battle;
               ShowBattleMap(myCanvas);
               if( true == gi.IsAdvancingFireChosen )
               {
                  double sizeCursor = Utilities.ZoomCanvas * Utilities.ZOOM * Utilities.theMapItemSize;
                  System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, sizeCursor * 0.5); // set the center of the MapItem as the hot point for the cursor
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c44AdvanceFire"), Width = sizeCursor, Height = sizeCursor };
                  myTargetCursor = Utilities.ConvertToCursor(img1, hotPoint);
                  this.myCanvas.Cursor = myTargetCursor; // set the cursor in the canvas
               }
               else
               {
                  if (null != myTargetCursor)
                     myTargetCursor.Dispose();
                  myTargetCursor = null;
                  this.myCanvas.Cursor = Cursors.Arrow; // get rid of the canvas cursor
               }
               break;
            case GameAction.SetupShowAfterActionReport:
               myDieRoller.HideDie();
               theMainImage = EnumMainImage.MI_Other;
               if (false == ShowAfterActionReportDialog(gi, myCanvas, true))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ShowAfterActionReportDialog() returned false for a=" + action.ToString());
               break;
            case GameAction.TestingStartMorningBriefing:
            case GameAction.MorningBriefingBegin:
            case GameAction.MorningBriefingWeatherRollEnd:
            case GameAction.MorningBriefingTimeCheck:
            case GameAction.MorningBriefingTimeCheckRoll:
            case GameAction.EveningDebriefingRatingImprovement:
            case GameAction.EveningDebriefingRatingImprovementEnd:
               theMainImage = EnumMainImage.MI_Other;
               if ( false == ShowAfterActionReportDialog(gi, myCanvas, false))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ShowAfterActionReportDialog() returned false for a=" + action.ToString());
               break;
            case GameAction.SetupShowCombatCalendarCheck:
               theMainImage = EnumMainImage.MI_Other;
               ShowCombatCalendarDialog(myCanvas);
               break;
            case GameAction.EndGameWin:
               theMainImage = EnumMainImage.MI_Other;
               ShowEndGameSuccess(myCanvas);
               break;
            case GameAction.EndGameLost:
               theMainImage = EnumMainImage.MI_Other;
               ShowEndGameFail(myCanvas);
               break;
            default:
               break;
         }
      }
      //-------------------------------------------------
      public void CleanCanvas(Canvas c, bool IsBattleMap = false)
      {
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in c.Children)
         {
            if (ui is AfterActionReportUserControl)
               elements.Add(ui);
            if (ui is Polygon polygon)
               elements.Add(ui);
            if (ui is Polyline polyline)
               elements.Add(ui);
            if (ui is Ellipse ellipse)
            {
               if ("CenterPoint" != ellipse.Name) // CenterPoint is a unit test ellipse
                  elements.Add(ui);
            }
            if (ui is System.Windows.Controls.Label label)  // A Game Feat Label
               elements.Add(ui);
            if (ui is Image img)
            {
               if (true == img.Name.Contains("Die"))
                  continue;
               elements.Add(ui);
            }
            if (ui is TextBlock tb)
               elements.Add(ui);
         }
         foreach (UIElement ui1 in elements)
            c.Children.Remove(ui1);
      }
      public void ShowMovementMap(Canvas c)
      {
         CleanCanvas(c);
         Image img = new Image() { Name = "CanvasMain", Width = 1115, Height = 880, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("MapMovement") };
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
         Canvas.SetZIndex(img, 0);
      }
      public void ShowBattleMap(Canvas c)
      {
         CleanCanvas(c, true);
         Image img = new Image() { Name = "CanvasMain", Width = 1000, Height = 890, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("MapBattle") };
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
         Canvas.SetZIndex(img, 0);
      }
      //-------------------------------------------------
      private void ShowInitialScreen(Canvas c)
      {
         Image img = new Image() { Name = "CanvasMain", Width = 1617, Height = 880, Source = MapItem.theMapImages.GetBitmapImage("Sherman3") };
         c.Children.Add(img);
         double x = (c.ActualWidth - img.Width) * 0.5;
         double y = (c.ActualHeight - img.Height) * 0.5;
         Canvas.SetLeft(img, x);
         Canvas.SetTop(img, y);
      }
      private void ShowHistoricalMap(Canvas c)
      {
         CleanCanvas(c);
         Image img = new Image() { Name = "CanvasMain", Width = 1115, Height = 880, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("MapHistorical") };
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
      }
      private bool ShowAfterActionReportDialog(IGameInstance gi, Canvas c, bool isEditable)
      {
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowAfterActionReportDialog():  gi.Reports.GetLast()=null");
            return false;
         }
         myAarUserControl = new AfterActionReportUserControl(report, isEditable);
         CleanCanvas(c);
         c.Children.Add(myAarUserControl);
         c.UpdateLayout();
         double x = (c.ActualWidth - myAarUserControl.ActualWidth) * 0.5;
         double y = (c.ActualHeight - myAarUserControl.ActualHeight) * 0.5;
         Canvas.SetLeft(myAarUserControl, x);
         Canvas.SetTop(myAarUserControl, y);
         return true;
      }
      private void ShowCombatCalendarDialog(Canvas c)
      {
         CleanCanvas(c);
         Image img = new Image() { Name = "CanvasMain", Width = 1115, Height = 880, Source = MapItem.theMapImages.GetBitmapImage("CombatCalendar") };
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
      }
      //-------------------------------------------------
      private void ShowTankExploding(IGameInstance gi, Canvas c)
      {
         BitmapImage bmi2 = new BitmapImage();
         bmi2.BeginInit();
         bmi2.UriSource = new Uri(MapImage.theImageDirectory + "ShermanExploding.gif", UriKind.Absolute);
         bmi2.EndInit();
         double size = 1.5 * gi.Sherman.Zoom * Utilities.theMapItemSize;
         Image img = new Image { Name="ShermanExploding", Source = bmi2, Height = size, Width = size, Stretch = Stretch.Fill };
         ImageBehavior.SetAnimatedSource(img, bmi2);
         //-------------------
         RotateTransform rotateTransform = new RotateTransform();
         img.RenderTransformOrigin = new Point(0.5, 0.5);
         rotateTransform.Angle = gi.Sherman.RotationHull;
         img.RenderTransform = rotateTransform;
         //-------------------
         c.Children.Add(img);
         double left = (double)gi.Sherman.Location.X - 0.25 * gi.Sherman.Zoom * Utilities.theMapItemSize;
         double top = (double)gi.Sherman.Location.Y  - 0.25 * gi.Sherman.Zoom * Utilities.theMapItemSize;
         Canvas.SetLeft(img, left);
         Canvas.SetTop(img, top);
         Canvas.SetZIndex(img, 99999);
      }
      //-------------------------------------------------
      private void ShowTankBrewUp(IGameInstance gi, Canvas c)
      {
         BitmapImage bmi2 = new BitmapImage();
         bmi2.BeginInit();
         bmi2.UriSource = new Uri(MapImage.theImageDirectory + "ShermanBrewUp.gif", UriKind.Absolute);
         bmi2.EndInit();
         double size = 1.1 * gi.Sherman.Zoom * Utilities.theMapItemSize;
         Image img = new Image { Name = "ShermanBrewUp", Source = bmi2, Height = 1.6 * size, Width = size, Stretch = Stretch.Fill };
         ImageBehavior.SetAnimatedSource(img, bmi2);
         //-------------------
         c.Children.Add(img);
         double left = (double)gi.Sherman.Location.X - 0.1 * gi.Sherman.Zoom * Utilities.theMapItemSize;
         double top = (double)gi.Sherman.Location.Y - 1.0 * gi.Sherman.Zoom * Utilities.theMapItemSize;
         Canvas.SetLeft(img, left);
         Canvas.SetTop(img, top);
         Canvas.SetZIndex(img, 99999);
      }
      private void ShowEndGameSuccess(Canvas c)
      {
         c.LayoutTransform = new ScaleTransform(1.0, 1.0);
         BitmapImage bmi1 = new BitmapImage();
         int randomNum = Utilities.RandomGenerator.Next(5);
         bmi1.BeginInit();
         if (0 == randomNum)
            bmi1.UriSource = new Uri(MapImage.theImageDirectory + "EndGameSuccess.gif", UriKind.Absolute);
         else
            bmi1.UriSource = new Uri(MapImage.theImageDirectory + "EndGameSuccess2.gif", UriKind.Absolute);
         bmi1.EndInit();
         Image img = new Image { Source = bmi1, Height = c.ActualHeight, Width = c.ActualWidth, Stretch = Stretch.Fill };
         ImageBehavior.SetAnimatedSource(img, bmi1);
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
         Canvas.SetZIndex(img, 99999);
      }
      private void ShowEndGameFail(Canvas c)
      {
         c.LayoutTransform = new ScaleTransform(1.0, 1.0);
         BitmapImage bmi1 = new BitmapImage();
         bmi1.BeginInit();
         bmi1.UriSource = new Uri(MapImage.theImageDirectory + "EndGameFail.gif", UriKind.Absolute);
         bmi1.EndInit();
         Image img = new Image { Source = bmi1, Height = c.ActualHeight, Width = c.ActualWidth, Stretch = Stretch.Fill };
         ImageBehavior.SetAnimatedSource(img, bmi1);
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
         Canvas.SetZIndex(img, 99999);
      }
   }
}
