using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace Pattons_Best
{
   internal class CanvasImageViewer : IView
   {
      public bool CtorError { get; } = false;
      private Canvas? myCanvas = null;
      //-------------------------------------------------
      public CanvasImageViewer(Canvas? c)
      {
         if (null == c)
         {
            Logger.Log(LogEnum.LE_ERROR, "CanvasImageViewer(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
      }
      //-------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myCanvas=null");
            return;
         }
         switch (action)
         {
            case GameAction.RemoveSplashScreen:
               ShowInitialScreen(myCanvas);
               break;
            case GameAction.SetupShowMapHistorical:
               ShowHistoricalMap(myCanvas);
               break;
            case GameAction.SetupShowMovementBoard:
               ShowMovementMap(myCanvas);
               break;
            case GameAction.SetupShowBattleBoard:
               ShowBattleMap(myCanvas);
               break;
            case GameAction.SetupShowAfterActionReport:
               if (false == ShowAfterActionReportDialog(gi, myCanvas, true))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ShowAfterActionReportDialog() returned false for a=" + action.ToString());
               break;
            case GameAction.MorningBriefingBegin:
               if ( false == ShowAfterActionReportDialog(gi, myCanvas, false))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ShowAfterActionReportDialog() returned false for a=" + action.ToString());
               break;
            case GameAction.SetupShowCombatCalendarCheck:
               ShowCombatCalendarDialog(myCanvas);
               break;
            case GameAction.EndGameWin:
               ShowEndGameSuccess(myCanvas);
               break;
            case GameAction.EndGameLost:
               ShowEndGameFail(myCanvas);
               break;
            default:
               break;
         }
      }
      //-------------------------------------------------
      private void CleanCanvas(Canvas c)
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
      private void ShowInitialScreen(Canvas c)
      {
         Image img = new Image() { Name = "Canvas", Width = 1617, Height = 880, Source = MapItem.theMapImages.GetBitmapImage("Sherman3") };
         c.Children.Add(img);
         double x = (c.ActualWidth - img.Width) * 0.5;
         double y = (c.ActualHeight - img.Height) * 0.5;
         Canvas.SetLeft(img, x);
         Canvas.SetTop(img, y);
      }
      private void ShowHistoricalMap(Canvas c)
      {
         CleanCanvas(c);
         Image img = new Image() { Name = "Canvas", Width = 1115, Height = 880, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("MapHistorical") };
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
      }
      private void ShowMovementMap(Canvas c)
      {
         CleanCanvas(c);
         Image img = new Image() { Name = "Canvas", Width = 1115, Height = 880, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("MapMovement") };
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
      }
      private void ShowBattleMap(Canvas c)
      {
         CleanCanvas(c);
         Image img = new Image() { Name = "Canvas", Width = 1115, Height = 880, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("MapBattle") };
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
         AfterActionReportUserControl aarControl = new AfterActionReportUserControl(report, isEditable);
         CleanCanvas(c);
         c.Children.Add(aarControl);
         c.UpdateLayout();
         double x = (c.ActualWidth - aarControl.ActualWidth) * 0.5;
         double y = (c.ActualHeight - aarControl.ActualHeight) * 0.5;
         Canvas.SetLeft(aarControl, x);
         Canvas.SetTop(aarControl, y);
         return true;
      }
      private void ShowCombatCalendarDialog(Canvas c)
      {
         CleanCanvas(c);
         Image img = new Image() { Name = "Canvas", Width = 1115, Height = 880, Source = MapItem.theMapImages.GetBitmapImage("CombatCalendar") };
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
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
