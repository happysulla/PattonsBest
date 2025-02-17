using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            case GameAction.SetupShowAfterActionReport:
               if( false == ShowAfterActionReport(gi, myCanvas))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ShowAfterActionReport() returned false");
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
      private void ShowInitialScreen(Canvas c)
      {
         Image imageStart = new Image() { Name = "Map", Width = 1617, Height = 880, Source = MapItem.theMapImages.GetBitmapImage("Sherman3") };
         c.Children.Add(imageStart);
         double x = (c.ActualWidth - imageStart.Width) * 0.5;
         double y = (c.ActualHeight - imageStart.Height) * 0.5;
         Canvas.SetLeft(imageStart, x);
         Canvas.SetTop(imageStart, y);

         c.LayoutTransform = new ScaleTransform(1.0, 1.0);
         BitmapImage bmi1 = new BitmapImage();
         Image img = new Image { Source = bmi1, Height = c.ActualHeight, Width = c.ActualWidth, Stretch = Stretch.Fill };
         ImageBehavior.SetAnimatedSource(img, bmi1);
         c.Children.Add(img);
         Canvas.SetLeft(img, 0);
         Canvas.SetTop(img, 0);
         Canvas.SetZIndex(img, 99999);
      }
      private void ShowHistoricalMap(Canvas c)
      {
         Image imageMap = new Image() { Name = "Map", Width = 1115, Height = 880, Stretch = Stretch.Fill, Source = MapItem.theMapImages.GetBitmapImage("MapHistorical") };
         c.Children.Add(imageMap);
         Canvas.SetLeft(imageMap, 0);
         Canvas.SetTop(imageMap, 0);
      }
      private bool ShowAfterActionReport(IGameInstance gi, Canvas c)
      {
         if( 0 == gi.Reports.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowAfterActionReport(): gi.Reports.Count = 0");
            return false;
         }
         IAfterActionReport report = gi.Reports[gi.Reports.Count - 1];
         AfterActionReportUserControl aarControl = new AfterActionReportUserControl(report, true);
         c.Children.Clear();
         c.Children.Add(aarControl);
         c.UpdateLayout();  
         double x = (c.ActualWidth - aarControl.ActualWidth) * 0.5;
         double y = (c.ActualHeight - aarControl.ActualHeight) * 0.5;
         Canvas.SetLeft(aarControl, x);
         Canvas.SetTop(aarControl, y);
         return true;
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
