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
               theMainImage = EnumMainImage.MI_Move;
               ShowMovementMap(myCanvas);
               break;
            case GameAction.SetupShowBattleBoard:
            case GameAction.PreparationsDeployment:
            case GameAction.UpdateToPreparations:
               myDieRoller.HideDie();
               theMainImage = EnumMainImage.MI_Battle;
               ShowBattleMap(myCanvas);
               break;
            case GameAction.SetupShowAfterActionReport:
               myDieRoller.HideDie();
               theMainImage = EnumMainImage.MI_Other;
               if (false == ShowAfterActionReportDialog(gi, myCanvas, true))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ShowAfterActionReportDialog() returned false for a=" + action.ToString());
               break;
            case GameAction.MorningBriefingBegin:
            case GameAction.MorningBriefingWeatherRollEnd:
            case GameAction.MorningBriefingTimeCheck:
            case GameAction.MorningBriefingTimeCheckRoll:
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
      }
      public void ShowBattleMap(Canvas c)
      {
         CleanCanvas(c, true);
         Image img = new Image() { Name = "CanvasMain", Width = 1000, Height = 890, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("MapBattle") };
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
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
         Image img = new Image() { Name = "CanvasMain", Width = 1115, Height = 880, Source = MapItem.theMapImages.GetBitmapImage("CombatCalendar") };
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
