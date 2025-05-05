using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace Pattons_Best
{
   class StatusBarViewer : IView
   {
      private readonly StatusBar myStatusBar;
      private IGameInstance myGameInstance;
      private IGameEngine myGameEngine;
      private Canvas myCanvas;
      private Cursor? myTargetCursor;
      //---------------------------------------------
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //------------------------------------------------------------------------------------------------------
      public StatusBarViewer(StatusBar sb, IGameEngine ge, IGameInstance gi, Canvas c)
      {
         myStatusBar = sb;
         myGameInstance = gi;
         myGameEngine = ge;
         myCanvas = c;
      }
      //-----------------------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         if ((GameAction.UpdateLoadingGame == action) || (GameAction.UpdateNewGame == action))
         {
            myGameInstance = gi;
         }
         //-------------------------------------------------------
         if ((null != myTargetCursor) && (GameAction.UpdateStatusBar == action)) // increase/decrease size of cursor when zoom in or out
         {
            myTargetCursor.Dispose();
            double sizeCursor = Utilities.ZoomCanvas * Utilities.ZOOM * Utilities.theMapItemSize;
            System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, sizeCursor * 0.5); // set the center of the MapItem as the hot point for the cursor
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Target"), Width = sizeCursor, Height = sizeCursor };
            myTargetCursor = Utilities.ConvertToCursor(img1, hotPoint);
            this.myCanvas.Cursor = myTargetCursor;
         }
         //-------------------------------------------------------
         Logger.Log(LogEnum.LE_VIEW_UPDATE_STATUS_BAR, "---------------StatusBarViewer::UpdateView() ==> a=" + action.ToString());
         switch (action)
         {
            case GameAction.UpdateNewGame:
            case GameAction.UpdateLoadingGame:
            case GameAction.UpdateUndo:
               if (null != myTargetCursor)
                  myTargetCursor.Dispose();
               myTargetCursor = null;
               this.myCanvas.Cursor = Cursors.Arrow; // get rid of the canvas cursor
               break;
            case GameAction.UpdateBattleBoard:
               break;
            case GameAction.MovementAirStrikeChoice:
               break;
            default:
               break;
         }
         //--------------------------------------------
         myStatusBar.Items.Clear();
         System.Windows.Controls.Button buttonZoomIn = new System.Windows.Controls.Button { Content=" - ", FontFamily=myFontFam1, Height = 15, Width = 30 };
         buttonZoomIn.Click += ButtonZoomIn_Click;
         myStatusBar.Items.Add(buttonZoomIn);
         Label labelOr = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = "or" };
         myStatusBar.Items.Add(labelOr);
         System.Windows.Controls.Button buttonZoomOut = new System.Windows.Controls.Button { Content = " + ", FontFamily = myFontFam1, Height = 15, Width = 30 };
         buttonZoomOut.Click += ButtonZoomOut_Click;
         myStatusBar.Items.Add(buttonZoomOut);
         StringBuilder sbZ = new StringBuilder("Zoom=");
         sbZ.Append(Utilities.ZoomCanvas.ToString("#.##"));
         Label labelZoom = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbZ.ToString() };
         myStatusBar.Items.Add(labelZoom);
         //--------------------------------------------
         myStatusBar.Items.Add(new Separator());
         Label labelGoto = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = "Goto:" };
         myStatusBar.Items.Add(labelGoto);
         System.Windows.Controls.Button buttonGoto = new System.Windows.Controls.Button { Content = myGameInstance.EventActive, FontFamily = myFontFam1, Height = 15, Width = 40 };
         if (true == gi.IsGridActive)
            buttonGoto.IsEnabled = false;
         else
            buttonGoto.IsEnabled = true;
         buttonGoto.Click += ButtonEventActive_Click;
         myStatusBar.Items.Add(buttonGoto);
         //--------------------------------------------
         if (true == gi.IsAirStrikePending)
         {
            myStatusBar.Items.Add(new Separator());
            StringBuilder sbAir = new StringBuilder("Pending" );
            Label labelAirStrike = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbAir.ToString() };
            Image imgAirStrike = new Image { Source = MapItem.theMapImages.GetBitmapImage("AirStrike"), Width = 40, Height = 20 };
            myStatusBar.Items.Add(labelAirStrike);
            myStatusBar.Items.Add(imgAirStrike);
         }
         //-------------------------------------------------------
         if (true == gi.IsLeadTank)
         {
            myStatusBar.Items.Add(new Separator());
            Image imgLead = new Image { Source = MapItem.theMapImages.GetBitmapImage("ShermanLead"), Width = 60, Height = 40 };
            myStatusBar.Items.Add(imgLead);
         }
         //-------------------------------------------------------
         if (true == gi.IsAdvancingFireChosen)
         {
            myStatusBar.Items.Add(new Separator());
            Label labelAF = new Label() { FontFamily = myFontFam, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = gi.AdvancingFireMarkerCount.ToString() };
            Image imgAF = new Image { Source = MapItem.theMapImages.GetBitmapImage("c44AdvanceFire"), Width = 30, Height = 30 };
            myStatusBar.Items.Add(labelAF);
            myStatusBar.Items.Add(imgAF);
         }
      }
      //-----------------------------------------------------------------
      private void ButtonEventActive_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
      {
         Utilities.ZoomCanvas += 0.25;
         myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
         UpdateView(ref myGameInstance, GameAction.UpdateStatusBar);
      }
      private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
      {
         Utilities.ZoomCanvas -= 0.25;
         myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
         UpdateView(ref myGameInstance, GameAction.UpdateStatusBar);
      }
   }
}
