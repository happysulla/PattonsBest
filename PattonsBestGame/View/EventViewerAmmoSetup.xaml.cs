﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace Pattons_Best
{
   public partial class EventViewerAmmoSetup : UserControl
   {
      public delegate bool EndAmmoLoadCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      private const int MAX_VOLUNTARY_ROUNDS = 30;
      public enum E162Enum
      {
         SET_HBCI_COUNT,
         LOAD_NORMAL,
         LOAD_EXTRA_CHECK,
         LOAD_EXTRA_CHECK_SHOW,
         LOAD_EXTRA,
         LOAD_EXTRA_VOLUNTARILY,
         READY_RACK,
         END
      };
      public bool CtorError { get; } = false;
      private EndAmmoLoadCallback? myCallback = null;
      private E162Enum myState = E162Enum.LOAD_NORMAL;
      private bool myIsRollInProgress = false;
      //---------------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myDieRoll = Utilities.NO_RESULT;
         }
      };
      private GridRow[] myGridRows = new GridRow[4];
      //---------------------------------------------------
      private IGameEngine? myGameEngine;
      private IGameInstance? myGameInstance;
      private readonly Canvas? myCanvas;
      private readonly ScrollViewer? myScrollViewer;
      private RuleDialogViewer? myRulesMgr;
      private IDieRoller? myDieRoller;
      private string myDieRollResult="";
      //---------------------------------------------------
      private string myMainGun = "75";
      private int myUnassignedCount = 0;
      private int myApRoundCount = 0;
      private int myHeRoundCount = 0;
      private int myWpRoundCount = 0;
      private int myHbciRoundCount = 0;
      private int myHbciRoundCountOriginal = 0;
      private int myHvapRoundCount = 0;
      private int myHvapRoundCountOringal = 0;
      private int myExtraAmmo = -1;
      private int myUnassignedReadyRack = 0;
      private int myHeReadyRackCount = 0;
      private int myApReadyRackCount = 0;
      private int myWpReadyRackCount = 0;
      private int myHbciReadyRackCount = 0;
      private int myHvapReadyRackCount = 0;
      //---------------------------------------------------
      private readonly Thickness myMarginAssignPanel = new Thickness(20, 0, 0, 0);
      private readonly Thickness myMarginLeft = new Thickness(0, 0, 5, 0);
      private readonly Thickness myMarginRight = new Thickness(5, 0, 0, 0);
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-------------------------------------------------------------------------------------
      public EventViewerAmmoSetup(IGameEngine? ge, IGameInstance? gi, Canvas? c, ScrollViewer? sv, RuleDialogViewer? rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == ge) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerAmmoSetup(): ge=null");
            CtorError = true;
            return;
         }
         myGameEngine = ge;
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerAmmoSetup(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerAmmoSetup(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerAmmoSetup(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerAmmoSetup(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerAmmoSetup(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool LoadAmmo(EndAmmoLoadCallback callback)
      {
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): myGameEngine=null");
            return false;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): myGameInstance=null");
            return false;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): myCanvas=null");
            return false;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): myScrollViewer=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): myRulesMgr=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): myDieRoller=null");
            return false;
         }
         //--------------------------------------------------
         myState = E162Enum.SET_HBCI_COUNT;
         myIsRollInProgress = false;
         myCallback = callback;
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast(); // remove it from list
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): myGameInstance=null");
            return false;
         }
         //--------------------------------------------------
         TankCard card = new TankCard(lastReport.TankCardNum);
         myMainGun = card.myMainGun;
         myUnassignedReadyRack = card.myMaxReadyRackCount;
         myUnassignedCount = card.myNumMainGunRound;
         myApRoundCount = 0;
         myHeRoundCount = 0;
         myWpRoundCount = 0;
         myHbciRoundCount = 0;
         myHvapRoundCount = 0;
         myExtraAmmo = -1;
         myDieRollResult = "";
         myHbciRoundCountOriginal = 0;
         myHbciRoundCount = 0;
         //--------------------------------------------------
         if ("75" != myMainGun)
         {
            if( false == SetLoadNormalState())
            {
               Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): SetLoadNormalState() return false");
               return false;
            }
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): UpdateGrid() return false");
            return false;
         }
         myScrollViewer.Content = myGrid;
         return true;
      }
      private bool SetLoadNormalState()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): myGameInstance=null");
            return false;
         }
         myState = E162Enum.LOAD_NORMAL;
         //--------------------------------------------------
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast(); // remove it from list
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): myGameInstance=null");
            return false;
         }
         //--------------------------------------------------
         ITerritory? t = Territories.theTerritories.Find("ReadyRackHe0");
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): t=null for ReadyRackHe0");
            return false;
         }
         IMapItem rr1 = new MapItem("ReadyRackHe", 0.9, "c12RoundsLeft", t);
         //--------------------------------------------------
         myGameInstance.ReadyRacks.Add(rr1);
         t = Territories.theTerritories.Find("ReadyRackAp0");
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): t=null for ReadyRackAp0");
            return false;
         }
         IMapItem rr2 = new MapItem("ReadyRackAp", 0.9, "c12RoundsLeft", t);
         //--------------------------------------------------
         myGameInstance.ReadyRacks.Add(rr2);
         if ("75" == myMainGun)
         {
            myUnassignedCount -= myHbciRoundCount;
            myWpRoundCount = 5;
            myUnassignedCount -= myWpRoundCount;
            t = Territories.theTerritories.Find("ReadyRackWp0");
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): t=null for ReadyRackWp0");
               return false;
            }
            IMapItem rr3 = new MapItem("ReadyRackWp", 0.9, "c12RoundsLeft", t);
            //--------------------------------------------------
            myGameInstance.ReadyRacks.Add(rr3);
            t = Territories.theTerritories.Find("ReadyRackHbci0");
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): t=null for ReadyRackHbci0");
               return false;
            }
            IMapItem rr4 = new MapItem("ReadyRackHbci", 0.9, "c12RoundsLeft", t);
            myGameInstance.ReadyRacks.Add(rr4);
         }
         else
         {
            myHvapRoundCountOringal = myHvapRoundCount = lastReport.MainGunHVAP;
            myUnassignedCount -= myHvapRoundCount;
            t = Territories.theTerritories.Find("ReadyRackHvap0");
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "LoadAmmo(): t=null for ReadyRackHvap0");
               return false;
            }
            IMapItem rr3 = new MapItem("ReadyRackHvap", 0.9, "c12RoundsLeft", t);
            myGameInstance.ReadyRacks.Add(rr3);
         }
         // Assign 60% or rounds to HE
         myHeRoundCount = (int)Math.Ceiling((double)myUnassignedCount * 0.6);
         myUnassignedCount -= myHeRoundCount;
         myApRoundCount = myUnassignedCount; // assign remaining rounds to AP
         myUnassignedCount = 0;
         // Assign default rack with 60% HE
         myHeReadyRackCount = (int)Math.Ceiling((double)myUnassignedReadyRack * 0.6);
         myUnassignedReadyRack -= myHeReadyRackCount;
         myApReadyRackCount = myUnassignedReadyRack;
         myUnassignedReadyRack = 0;
         myWpReadyRackCount = 0;
         myHbciReadyRackCount = 0;
         myHvapReadyRackCount = 0;
         if (false == UpdateReadyRack())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): UpdateReadyRack returned false");
            return false;
         }
         return true;
      }
      private bool UpdateGrid()
      {
         if (false == UpdateEndState())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateEndState() returned false");
            return false;
         }
         if (E162Enum.END == myState)
            return true;
         if (false == UpdateUserInstructions())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
            return false;
         }
         if (false == UpdateAssignablePanel())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
            return false;
         }
         if (false == UpdateGridRows())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
            return false;
         }
         return true;
      }
      private bool UpdateEndState()
      {
         if (E162Enum.END == myState)
         {
            if( false == UpdateAmmoLoad())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): UpdateAmmoLoad() returned false");
               return false;
            }
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            if (false == myCallback())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateUserInstructions()
      {
         myTextBlockInstructions.Inlines.Clear();
         switch (myState)
         {
            case E162Enum.SET_HBCI_COUNT:
               myTextBlockInstructions.Inlines.Add(new Run("Roll 1D for available HBCI Rounds."));
               break;
            case E162Enum.LOAD_NORMAL:
               if( 0 < myUnassignedCount )
                  myTextBlockInstructions.Inlines.Add(new Run("Adjust normal ammo type."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Adjust normal ammo type or click gun round image to continue."));
               break;
            case E162Enum.LOAD_EXTRA_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll to determine if extra ammo required."));
               myR1621.Visibility = Visibility.Hidden;
               myR1622.Visibility = Visibility.Visible;
               break;
            case E162Enum.LOAD_EXTRA_CHECK_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case E162Enum.LOAD_EXTRA:
               if (0 < myUnassignedCount)
                  myTextBlockInstructions.Inlines.Add(new Run("Adjust normal ammo type."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Adjust normal ammo type or click gun round image to continue."));
               myR1621.Visibility = Visibility.Hidden;
               myR1622.Visibility = Visibility.Visible;
               break;
            case E162Enum.LOAD_EXTRA_VOLUNTARILY:
                  myTextBlockInstructions.Inlines.Add(new Run("Voluntarily add extra rounds --or-- Click gun round image to continue."));
               break;
            case E162Enum.READY_RACK:
               if (0 < myUnassignedReadyRack)
                  myTextBlockInstructions.Inlines.Add(new Run("Adjust ready rack."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Adjust ready rack --or-- Click AAR image to continue."));
               myR1622.Visibility = Visibility.Hidden;
               myR1623.Visibility = Visibility.Visible;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default state=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = myMarginAssignPanel};
         switch (myState)
         {
            case E162Enum.LOAD_NORMAL:
            case E162Enum.LOAD_EXTRA:
               if ( 0 < myUnassignedCount )
               {
                  Rectangle r = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r);
               }
               else
               {
                  Image img1 = new Image { Name = "MainGunRound", Source = MapItem.theMapImages.GetBitmapImage("MainGunRound"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img1);
               }
               label.Content = "Unassigned Ammo = " + myUnassignedCount.ToString();
               myStackPanelAssignable.Children.Add(label);
               break;
            case E162Enum.SET_HBCI_COUNT:
            case E162Enum.LOAD_EXTRA_CHECK:
               BitmapImage bitMapDieRoll = new BitmapImage();
               bitMapDieRoll.BeginInit();
               bitMapDieRoll.UriSource = new Uri(MapImage.theImageDirectory + "DieRollWhite.gif", UriKind.Absolute);
               bitMapDieRoll.EndInit();
               Image imgDieRoll = new Image { Name = "DieRoll", Source = bitMapDieRoll, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(imgDieRoll, bitMapDieRoll);
               myStackPanelAssignable.Children.Add(imgDieRoll);
               break;
            case E162Enum.LOAD_EXTRA_CHECK_SHOW:
               label.Content = myDieRollResult;
               myStackPanelAssignable.Children.Add(label);
               Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r1);
               break;
            case E162Enum.LOAD_EXTRA_VOLUNTARILY:
               Image img2 = new Image { Name = "MainGunRound", Source = MapItem.theMapImages.GetBitmapImage("MainGunRound"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img2);
               label.Content = "Unassigned Ammo = " + myUnassignedCount.ToString();
               myStackPanelAssignable.Children.Add(label);
               break;
            case E162Enum.READY_RACK:
               if (0 < myUnassignedReadyRack)
               {
                  Rectangle r2 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r2);
               }
               else
               {
                  Image img1 = new Image { Name = "AfterActionReport", Source = MapItem.theMapImages.GetBitmapImage("AfterActionReport"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img1);
               }
               label.Content = "Main Ammo=" + myUnassignedReadyRack.ToString();
               myStackPanelAssignable.Children.Add(label);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateGridRows()
      {
         if( E162Enum.READY_RACK == myState )
         {
            if( false == UpdateGridRowsReadyRack())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsReadyRack() returned false");
               return false;
            }
         }
         else if (E162Enum.SET_HBCI_COUNT == myState)
         {
            List<UIElement> results = new List<UIElement>();
            foreach (UIElement ui in myGrid.Children)
            {
               int rowNum0 = Grid.GetRow(ui);
               if (STARTING_ASSIGNED_ROW <= rowNum0)
                  results.Add(ui);
            }
            foreach (UIElement ui1 in results)
               myGrid.Children.Remove(ui1);
         }
         else
         {
            if (false == UpdateGridRowsAmmo())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsAmmo() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateGridRowsAmmo()
      {
         //------------------------------------------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum0 = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum0)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //=========================================
         int rowNum = 0 + STARTING_ASSIGNED_ROW;
         Label labelforHeName = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "HE" };
         myGrid.Children.Add(labelforHeName);
         Grid.SetRow(labelforHeName, rowNum);
         Grid.SetColumn(labelforHeName, 0);
         //-----------------------------------------
         StackPanel stackpanelHe = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
         Button bMinusHe10 = new Button() { Name = "bMinusHe10", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset + 5, FontFamily = myFontFam1, Content = "-10", Margin = myMarginLeft };
         if ((9 < myHeRoundCount) && ( (E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState) ) )
         {
            bMinusHe10.Click += ButtonAmmoChange_Click;
            bMinusHe10.IsEnabled = true;
         }
         stackpanelHe.Children.Add(bMinusHe10);
         Button bMinusHe = new Button() { Name = "bMinusHe", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
         if ((0 < myHeRoundCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
         {
            bMinusHe.Click += ButtonAmmoChange_Click;
            bMinusHe.IsEnabled = true;
         }
         stackpanelHe.Children.Add(bMinusHe);
         Label labelforHe = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myHeRoundCount.ToString() };
         if (myHeRoundCount < 10)
            labelforHe.Content = "0" + myHeRoundCount.ToString();
         stackpanelHe.Children.Add(labelforHe);
         Button bPlusHe = new Button() { Name = "bPlusHe", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
         if ((0 < myUnassignedCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
         {
            bPlusHe.Click += ButtonAmmoChange_Click;
            bPlusHe.IsEnabled = true;
         }
         stackpanelHe.Children.Add(bPlusHe);
         Button bPlusHe10 = new Button() { Name = "bPlusHe10", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset + 5, FontFamily = myFontFam1, Content = "+10", Margin = myMarginRight };
         if ((9 < myUnassignedCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
         {
            bPlusHe10.Click += ButtonAmmoChange_Click;
            bPlusHe10.IsEnabled = true;
         }
         stackpanelHe.Children.Add(bPlusHe10);
         myGrid.Children.Add(stackpanelHe);
         Grid.SetRow(stackpanelHe, rowNum);
         Grid.SetColumn(stackpanelHe, 1);
         //=========================================
         rowNum = 1 + STARTING_ASSIGNED_ROW;
         Label labelforApName = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "AP" };
         myGrid.Children.Add(labelforApName);
         Grid.SetRow(labelforApName, rowNum);
         Grid.SetColumn(labelforApName, 0);
         //-----------------------------------------
         StackPanel stackpanelAp = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
         Button bMinusAp10 = new Button() { Name = "bMinusAp10", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset + 5, FontFamily = myFontFam1, Content = "-10", Margin = myMarginLeft };
         if ((9 < myApRoundCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
         {
            bMinusAp10.Click += ButtonAmmoChange_Click;
            bMinusAp10.IsEnabled = true;
         }
         stackpanelAp.Children.Add(bMinusAp10);
         Button bMinusAp = new Button() { Name = "bMinusAp", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
         if ((0 < myApRoundCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
         {
            bMinusAp.Click += ButtonAmmoChange_Click;
            bMinusAp.IsEnabled = true;
         }
         stackpanelAp.Children.Add(bMinusAp);
         Label labelforAp = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myApRoundCount.ToString() };
         if (myApRoundCount < 10)
            labelforAp.Content = "0" + myApRoundCount.ToString();
         stackpanelAp.Children.Add(labelforAp);
         Button bPlusAp = new Button() { Name = "bPlusAp", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
         if ((0 < myUnassignedCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
         {
            bPlusAp.Click += ButtonAmmoChange_Click;
            bPlusAp.IsEnabled = true;
         }
         stackpanelAp.Children.Add(bPlusAp);
         Button bPlusAp10 = new Button() { Name = "bPlusAp10", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset + 5, FontFamily = myFontFam1, Content = "+10", Margin = myMarginRight };
         if ((9 < myUnassignedCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
         {
            bPlusAp10.Click += ButtonAmmoChange_Click;
            bPlusAp10.IsEnabled = true;
         }
         stackpanelAp.Children.Add(bPlusAp10);
         myGrid.Children.Add(stackpanelAp);
         Grid.SetRow(stackpanelAp, rowNum);
         Grid.SetColumn(stackpanelAp, 1);
         //------------------------------------------------------------
         if ("75" == myMainGun)
         {
            //=========================================
            rowNum = 2 + STARTING_ASSIGNED_ROW;
            Label labelforWpName = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "WP" };
            myGrid.Children.Add(labelforWpName);
            Grid.SetRow(labelforWpName, rowNum);
            Grid.SetColumn(labelforWpName, 0);
            //-----------------------------------------
            StackPanel stackpanelWp = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
            Button bMinusWp10 = new Button() { Name = "bMinusWp10", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset + 5, FontFamily = myFontFam1, Content = "-05", Margin = myMarginLeft };
            if ((4 < myWpRoundCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
            {
               bMinusWp10.Click += ButtonAmmoChange_Click;
               bMinusWp10.IsEnabled = true;
            }
            stackpanelWp.Children.Add(bMinusWp10);
            Button bMinusWp = new Button() { Name = "bMinusWp", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
            if ((0 < myWpRoundCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
            {
               bMinusWp.Click += ButtonAmmoChange_Click;
               bMinusWp.IsEnabled = true;
            }
            stackpanelWp.Children.Add(bMinusWp);
            Label labelforWp = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myWpRoundCount.ToString() };
            if ((myWpRoundCount < 10) && (E162Enum.LOAD_EXTRA_CHECK != myState))
               labelforWp.Content = "0" + myWpRoundCount.ToString();
            stackpanelWp.Children.Add(labelforWp);
            Button bPlusWp = new Button() { Name = "bPlusWp", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
            if ((0 < myUnassignedCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
            {
               bPlusWp.Click += ButtonAmmoChange_Click;
               bPlusWp.IsEnabled = true;
            }
            stackpanelWp.Children.Add(bPlusWp);
            Button bPlusWp10 = new Button() { Name = "bPlusWp10", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset + 5, FontFamily = myFontFam1, Content = "+05", Margin = myMarginRight };
            if ((4 < myUnassignedCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
            {
               bPlusWp10.Click += ButtonAmmoChange_Click;
               bPlusWp10.IsEnabled = true;
            }
            stackpanelWp.Children.Add(bPlusWp10);
            myGrid.Children.Add(stackpanelWp);
            Grid.SetRow(stackpanelWp, rowNum);
            Grid.SetColumn(stackpanelWp, 1);
            //=========================================
            rowNum = 3 + STARTING_ASSIGNED_ROW;
            Label labelforHbciName = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "HBCI" };
            myGrid.Children.Add(labelforHbciName);
            Grid.SetRow(labelforHbciName, rowNum);
            Grid.SetColumn(labelforHbciName, 0);
            //-----------------------------------------
            StackPanel stackpanelHbci = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
            Button bMinusHbci = new Button() { Name = "bMinusHbci", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
            if ((0 < myHbciRoundCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
            {
               bMinusHbci.Click += ButtonAmmoChange_Click;
               bMinusHbci.IsEnabled = true;
            }
            stackpanelHbci.Children.Add(bMinusHbci);
            Label labelforHbci = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myHbciRoundCount.ToString() };
            if (myHbciRoundCount < 10)
               labelforHbci.Content = "0" + myHbciRoundCount.ToString();
            stackpanelHbci.Children.Add(labelforHbci);
            Button bPlusHbci = new Button() { Name = "bPlusHbci", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
            if ((0 < myUnassignedCount) && (myHbciRoundCount < myHbciRoundCountOriginal) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
            {
               bPlusHbci.Click += ButtonAmmoChange_Click;
               bPlusHbci.IsEnabled = true;
            }
            stackpanelHbci.Children.Add(bPlusHbci);
            myGrid.Children.Add(stackpanelHbci);
            Grid.SetRow(stackpanelHbci, rowNum);
            Grid.SetColumn(stackpanelHbci, 1);
         }
         else
         {
            //=========================================
            rowNum = 2 + STARTING_ASSIGNED_ROW;
            Label labelforHvapName = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "HVAP" };
            myGrid.Children.Add(labelforHvapName);
            Grid.SetRow(labelforHvapName, rowNum);
            Grid.SetColumn(labelforHvapName, 0);
            //-----------------------------------------
            StackPanel stackpanelHvap = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
            Button bMinusHvap = new Button() { Name = "bMinusHvap", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
            if ((0 < myHvapRoundCount) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
            {
               bMinusHvap.Click += ButtonAmmoChange_Click;
               bMinusHvap.IsEnabled = true;
            }
            stackpanelHvap.Children.Add(bMinusHvap);
            Label labelforHvap = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myHvapRoundCount.ToString() };
            if (myHvapRoundCount < 10)
               labelforHvap.Content = "0" + myHvapRoundCount.ToString();
            stackpanelHvap.Children.Add(labelforHvap);
            Button bPlusHvap = new Button() { Name = "bPlusHvap", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
            if ((0 < myUnassignedCount) && (myHvapRoundCount < myHvapRoundCountOringal) && ((E162Enum.LOAD_EXTRA_CHECK != myState) && (E162Enum.LOAD_EXTRA_CHECK_SHOW != myState)))
            {
               bPlusHvap.Click += ButtonAmmoChange_Click;
               bPlusHvap.IsEnabled = true;
            }
            stackpanelHvap.Children.Add(bPlusHvap);
            //------------------------------------
            myGrid.Children.Add(stackpanelHvap);
            Grid.SetRow(stackpanelHvap, rowNum);
            Grid.SetColumn(stackpanelHvap, 1);
         }
         return true;
      }
      private bool UpdateGridRowsReadyRack()
      {
         //------------------------------------------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum0 = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum0)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //=========================================
         int rowNum = 0 + STARTING_ASSIGNED_ROW;
         Label labelforHeName = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "HE" };
         myGrid.Children.Add(labelforHeName);
         Grid.SetRow(labelforHeName, rowNum);
         Grid.SetColumn(labelforHeName, 0);
         //-----------------------------------------
         StackPanel stackpanelHe = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
         Button bMinusHe = new Button() { Name = "bMinusHe", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
         if (0 < myHeReadyRackCount) 
         {
            bMinusHe.Click += ButtonReadyRackChange_Click;
            bMinusHe.IsEnabled = true;
         }
         stackpanelHe.Children.Add(bMinusHe);
         Label labelforHe = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myHeReadyRackCount.ToString() };
         if (myHeReadyRackCount < 10)
            labelforHe.Content = "0" + myHeReadyRackCount.ToString();
         stackpanelHe.Children.Add(labelforHe);
         Button bPlusHe = new Button() { Name = "bPlusHe", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
         if (0 < myUnassignedReadyRack)
         {
            bPlusHe.Click += ButtonReadyRackChange_Click;
            bPlusHe.IsEnabled = true;
         }
         stackpanelHe.Children.Add(bPlusHe);
         myGrid.Children.Add(stackpanelHe);
         Grid.SetRow(stackpanelHe, rowNum);
         Grid.SetColumn(stackpanelHe, 1);
         //=========================================
         rowNum = 1 + STARTING_ASSIGNED_ROW;
         Label labelforApName = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "AP" };
         myGrid.Children.Add(labelforApName);
         Grid.SetRow(labelforApName, rowNum);
         Grid.SetColumn(labelforApName, 0);
         //-----------------------------------------
         StackPanel stackpanelAp = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
         Button bMinusAp = new Button() { Name = "bMinusAp", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
         if (0 < myApReadyRackCount) 
         {
            bMinusAp.Click += ButtonReadyRackChange_Click;
            bMinusAp.IsEnabled = true;
         }
         stackpanelAp.Children.Add(bMinusAp);
         Label labelforAp = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myApReadyRackCount.ToString() };
         if (myApReadyRackCount < 10)
            labelforAp.Content = "0" + myApReadyRackCount.ToString();
         stackpanelAp.Children.Add(labelforAp);
         Button bPlusAp = new Button() { Name = "bPlusAp", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
         if (0 < myUnassignedReadyRack) 
         {
            bPlusAp.Click += ButtonReadyRackChange_Click;
            bPlusAp.IsEnabled = true;
         }
         stackpanelAp.Children.Add(bPlusAp);
         myGrid.Children.Add(stackpanelAp);
         Grid.SetRow(stackpanelAp, rowNum);
         Grid.SetColumn(stackpanelAp, 1);
         //------------------------------------------------------------
         if ("75" == myMainGun)
         {
            //=========================================
            rowNum = 2 + STARTING_ASSIGNED_ROW;
            Label labelforWpName = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "WP" };
            myGrid.Children.Add(labelforWpName);
            Grid.SetRow(labelforWpName, rowNum);
            Grid.SetColumn(labelforWpName, 0);
            //-----------------------------------------
            StackPanel stackpanelWp = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
            Button bMinusWp = new Button() { Name = "bMinusWp", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
            if (0 < myWpReadyRackCount) 
            {
               bMinusWp.Click += ButtonReadyRackChange_Click;
               bMinusWp.IsEnabled = true;
            }
            stackpanelWp.Children.Add(bMinusWp);
            Label labelforWp = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myWpRoundCount.ToString() };
            if ((myWpReadyRackCount < 10) && (E162Enum.LOAD_EXTRA_CHECK != myState))
               labelforWp.Content = "0" + myWpReadyRackCount.ToString();
            stackpanelWp.Children.Add(labelforWp);
            Button bPlusWp = new Button() { Name = "bPlusWp", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
            if (0 < myUnassignedReadyRack) 
            {
               bPlusWp.Click += ButtonReadyRackChange_Click;
               bPlusWp.IsEnabled = true;
            }
            stackpanelWp.Children.Add(bPlusWp);
            myGrid.Children.Add(stackpanelWp);
            Grid.SetRow(stackpanelWp, rowNum);
            Grid.SetColumn(stackpanelWp, 1);
            //=========================================
            rowNum = 3 + STARTING_ASSIGNED_ROW;
            Label labelforHbciName = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "HBCI" };
            myGrid.Children.Add(labelforHbciName);
            Grid.SetRow(labelforHbciName, rowNum);
            Grid.SetColumn(labelforHbciName, 0);
            //-----------------------------------------
            StackPanel stackpanelHbci = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
            Button bMinusHbci = new Button() { Name = "bMinusHbci", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
            if (0 < myHbciReadyRackCount) 
            {
               bMinusHbci.Click += ButtonReadyRackChange_Click;
               bMinusHbci.IsEnabled = true;
            }
            stackpanelHbci.Children.Add(bMinusHbci);
            Label labelforHbci = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myHbciReadyRackCount.ToString() };
            if (myHbciReadyRackCount < 10)
               labelforHbci.Content = "0" + myHbciReadyRackCount.ToString();
            stackpanelHbci.Children.Add(labelforHbci);
            Button bPlusHbci = new Button() { Name = "bPlusHbci", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
            if ((0 < myUnassignedReadyRack) && (myHbciReadyRackCount < myHbciRoundCountOriginal))
            {
               bPlusHbci.Click += ButtonReadyRackChange_Click;
               bPlusHbci.IsEnabled = true;
            }
            stackpanelHbci.Children.Add(bPlusHbci);
            myGrid.Children.Add(stackpanelHbci);
            Grid.SetRow(stackpanelHbci, rowNum);
            Grid.SetColumn(stackpanelHbci, 1);
         }
         else
         {
            //=========================================
            rowNum = 2 + STARTING_ASSIGNED_ROW;
            Label labelforHvapName = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "HVAP" };
            myGrid.Children.Add(labelforHvapName);
            Grid.SetRow(labelforHvapName, rowNum);
            Grid.SetColumn(labelforHvapName, 0);
            //-----------------------------------------
            StackPanel stackpanelHvap = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal };
            Button bMinusHvap = new Button() { Name = "bMinusHvap", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "-" };
            if (0 < myHvapReadyRackCount) 
            {
               bMinusHvap.Click += ButtonReadyRackChange_Click;
               bMinusHvap.IsEnabled = true;
            }
            stackpanelHvap.Children.Add(bMinusHvap);
            Label labelforHvap = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myHvapReadyRackCount.ToString() };
            if (myHvapReadyRackCount < 10)
               labelforHvap.Content = "0" + myHvapReadyRackCount.ToString();
            stackpanelHvap.Children.Add(labelforHvap);
            Button bPlusHvap = new Button() { Name = "bPlusHvap", IsEnabled = false, Height = Utilities.theMapItemOffset, Width = Utilities.theMapItemOffset, FontFamily = myFontFam1, Content = "+" };
            if ((0 < myUnassignedReadyRack) && (myHvapReadyRackCount < myHvapRoundCountOringal))
            {
               bPlusHvap.Click += ButtonReadyRackChange_Click;
               bPlusHvap.IsEnabled = true;
            }
            stackpanelHvap.Children.Add(bPlusHvap);
            //------------------------------------
            myGrid.Children.Add(stackpanelHvap);
            Grid.SetRow(stackpanelHvap, rowNum);
            Grid.SetColumn(stackpanelHvap, 1);
         }
         return true;
      }
      private bool UpdateAmmoLoad()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateAmmoLoad(): myGameInstance=null");
            return false;
         }
         IAfterActionReport? report = myGameInstance.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateAmmoLoad(): report=null");
            return false;
         }
         report.MainGunHE = myHeRoundCount;
         report.MainGunAP = myApRoundCount;
         report.MainGunWP = myWpRoundCount;
         report.MainGunHBCI = myHbciRoundCount;
         report.MainGunHVAP = myHvapRoundCount;
         report.Ammo30CalibreMG = 30;
         if (-1 < myExtraAmmo)
            report.Ammo30CalibreMG += 10;
         return true;
      }
      private bool UpdateReadyRack()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): myGameInstance=null");
            return false;
         }
         IMapItem? rrHe = myGameInstance.ReadyRacks[0];
         if( null == rrHe )
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): rrHe=null");
            return false;
         }
         rrHe.Count = myHeReadyRackCount;
         string tName = rrHe.Name + rrHe.Count.ToString();
         ITerritory? newT = Territories.theTerritories.Find(tName);
         if (null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): newT=null for " + tName);
            return false;
         }
         if (false == SetTerritory(rrHe, newT))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): SetTerritory() returned false");
            return false;
         }
         //------------------------------------------
         IMapItem? rrAp = myGameInstance.ReadyRacks[1];
         if (null == rrAp)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): rrAp=null");
            return false;
         }
         rrAp.Count = myApReadyRackCount;
         tName = rrAp.Name + rrAp.Count.ToString();
         newT = Territories.theTerritories.Find(tName);
         if (null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): newT=null for " + tName);
            return false;
         }
         if (false == SetTerritory(rrAp, newT))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): SetTerritory() returned false");
            return false;
         }
         //------------------------------------------
         if ( "75" == myMainGun )
         {
            IMapItem? rrWp = myGameInstance.ReadyRacks[2];
            if (null == rrWp)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): rrAp=null");
               return false;
            }
            rrWp.Count = myWpReadyRackCount;
            tName = rrWp.Name + rrWp.Count.ToString();
            newT = Territories.theTerritories.Find(tName);
            if (null == newT)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): newT=null for " + tName);
               return false;
            }
            if (false == SetTerritory(rrWp, newT))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): SetTerritory() returned false");
               return false;
            }
            //------------------------------------------
            IMapItem? rrHbci = myGameInstance.ReadyRacks[3];
            if (null == rrHbci)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): rrAp=null");
               return false;
            }
            rrHbci.Count = myHbciReadyRackCount;
            tName = rrHbci.Name + rrHbci.Count.ToString();
            newT = Territories.theTerritories.Find(tName);
            if (null == newT)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): newT=null for " + tName);
               return false;
            }
            if (false == SetTerritory(rrHbci, newT))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): SetTerritory() returned false");
               return false;
            }
         }
         else
         {
            IMapItem? rrHvap = myGameInstance.ReadyRacks[2];
            if (null == rrHvap)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): rrHvap=null");
               return false;
            }
            rrHvap.Count = myHvapReadyRackCount;
            tName = rrHvap.Name + rrHvap.Count.ToString();
            newT = Territories.theTerritories.Find(tName);
            if (null == newT)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): newT=null for " + tName);
               return false;
            }
            if( false == SetTerritory(rrHvap, newT))
            {        
               Logger.Log(LogEnum.LE_ERROR, "UpdateReadyRack(): SetTerritory() returned false");
               return false;
            }
         }
         //------------------------------------------
         return true;
      }
      private bool SetTerritory(IMapItem mi, ITerritory newT)
      {
         mi.TerritoryCurrent = newT;
         double offset = mi.Zoom * Utilities.theMapItemOffset;
         mi.Location.X = newT.CenterPoint.X - offset;
         mi.Location.Y = newT.CenterPoint.Y - offset;
         return true;
      }
      //------------------------------------------------------------------------------------
      public void ShowDieResults(int dieRoll)
      {
         StringBuilder sb = new StringBuilder(dieRoll.ToString());
         sb.Append(": ");
         if ( null == myGameInstance )
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): myGameInstance=null");
            return;
         }
         if(E162Enum.SET_HBCI_COUNT == myState )
         {
            myHbciRoundCountOriginal = myHbciRoundCount = dieRoll;
            if ( false == SetLoadNormalState())
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): ShowDieResults(=null) returned false");
               return;
            } 
         }
         else
         {
            ICombatCalendarEntry? entry = TableMgr.theCombatCalendarEntries[myGameInstance.Day];
            if (null == entry)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): ICombatCalendarEntry=null");
               return;
            }
            if (6 < dieRoll)
            {
               switch (entry.Scenario)
               {
                  case EnumScenario.Advance:
                     myExtraAmmo = 30;
                     sb.Append("Required to take 30 extra ammo");
                     break;
                  case EnumScenario.Battle:
                     myExtraAmmo = 10;
                     sb.Append("Required to take 10 extra ammo");
                     break;
                  case EnumScenario.Counterattack:
                     sb.Append("May voluntarily take extra ammo");
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default=" + entry.ToString());
                     return;
               }
            }
            else if (2 < dieRoll)
            {
               switch (entry.Scenario)
               {
                  case EnumScenario.Advance:
                     myExtraAmmo = 20;
                     sb.Append("Required to take extra 20 extra ammo");
                     break;
                  case EnumScenario.Battle:
                  case EnumScenario.Counterattack:
                     sb.Append("May voluntarily take 10 extra ammo");
                     myExtraAmmo = 0;
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default=" + entry.ToString());
                     return;
               }
            }
            else
            {
               sb.Append("May voluntarily take extra ammo");
            }
            myDieRollResult = sb.ToString();
            if (0 < myExtraAmmo)
            {
               myUnassignedCount += myExtraAmmo;
               myState = E162Enum.LOAD_EXTRA;
            }
            else
            {
               myUnassignedCount += MAX_VOLUNTARY_ROUNDS;
            }
            myState = E162Enum.LOAD_EXTRA_CHECK_SHOW;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //---------------------Controller Function--------------------------------------------
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
            return;
         }
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (true == key.StartsWith("r")) // rules based click
         {
            if (false == myRulesMgr.ShowRule(key))
               Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false key=" + key);
         }
         else
         {
            if (false == myRulesMgr.ShowTable(key))
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowTable() returned false for key=" + key);
         }
      }
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myGameEngine=null");
            return;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myGameInstance=null");
            return;
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myCanvas=null");
            return;
         }
         if (null == myScrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myScrollViewer=null");
            return;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myRulesMgr=null");
            return;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myDieRoller=null");
            return;
         }
         if( E162Enum.LOAD_EXTRA_CHECK_SHOW == myState)
         {
            if (0 < myExtraAmmo)
               myState = E162Enum.LOAD_EXTRA;
            else
               myState = E162Enum.LOAD_EXTRA_VOLUNTARILY;
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         //--------------------------------------------------
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            if (ui is StackPanel panel)
            {
               foreach (UIElement ui1 in panel.Children)
               {
                  if (ui1 is Image img) // Check all images within the myStackPanelAssignable
                  {
                     if (result.VisualHit == img)
                     {
                        if ("MainGunRound" == img.Name)
                        {
                           if( E162Enum.LOAD_NORMAL == myState )
                           {
                              myState = E162Enum.LOAD_EXTRA_CHECK;
                           }
                           else
                           {
                              myState = E162Enum.READY_RACK;
                              GameAction action = GameAction.MorningBriefingAmmoReadyRackLoad;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           }
                        }
                        else if ("AfterActionReport" == img.Name)
                        {
                           myState = E162Enum.END;
                        }
                        if ("DieRoll" == img.Name)
                        {
                           if (false == myIsRollInProgress)
                           {
                              myIsRollInProgress = true;
                              RollEndCallback callback = ShowDieResults;
                              myDieRoller.RollMovingDie(myCanvas, callback);
                              img.Visibility = Visibility.Hidden;
                           }
                           return;
                        }
                        if (false == UpdateGrid())
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                        return;
                     }
                  }
               }
            }
            if (ui is Image img1) // next check all images within the Grid Rows
            {
               if (result.VisualHit == img1)
               {
                  if (false == myIsRollInProgress)
                  {
                     myIsRollInProgress = true;
                     RollEndCallback callback = ShowDieResults;
                     myDieRoller.RollMovingDie(myCanvas, callback);
                     img1.Visibility = Visibility.Hidden;
                  }
                  return;
               }
            }
         }
      }
      private void ButtonAmmoChange_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         switch(b.Name)
         {
            case "bMinusHe10":
               myHeRoundCount -= 10;
               myUnassignedCount += 10;
               break;
            case "bMinusHe":
               myHeRoundCount -= 1;
               myUnassignedCount += 1;
               break;
            case "bPlusHe":
               myHeRoundCount += 1;
               myUnassignedCount -= 1;
               break;
            case "bPlusHe10":
               myHeRoundCount += 10;
               myUnassignedCount -= 10;
               break;
            case "bMinusAp10":
               myApRoundCount -= 10;
               myUnassignedCount += 10;
               break;
            case "bMinusAp":
               myApRoundCount -= 1;
               myUnassignedCount += 1;
               break;
            case "bPlusAp":
               myApRoundCount += 1;
               myUnassignedCount -= 1;
               break;
            case "bPlusAp10":
               myApRoundCount += 10;
               myUnassignedCount -= 10;
               break;
            case "bMinusWp10":
               myWpRoundCount -= 5;
               myUnassignedCount += 5;
               break;
            case "bMinusWp":
               myWpRoundCount -= 1;
               myUnassignedCount += 1;
               break;
            case "bPlusWp":
               myWpRoundCount += 1;
               myUnassignedCount -= 1;
               break;
            case "bPlusWp10":
               myWpRoundCount += 5;
               myUnassignedCount -= 5;
               break;
            case "bMinusHbci":
               myHbciRoundCount -= 1;
               myUnassignedCount += 1;
               break;
            case "bPlusHbci":
               myHbciRoundCount += 1;
               myUnassignedCount -= 1;
               break;
            case "bMinusHvap":
               myHvapRoundCount -= 1;
               myUnassignedCount += 1;
               break;
            case "bPlusHvap":
               myHvapRoundCount += 1;
               myUnassignedCount -= 1;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ButtonAmmoChange_Click(): reached default with key=" + b.Name);
               break;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonAmmoChange_Click(): UpdateGrid() return false");
      }
      private void ButtonReadyRackChange_Click(object sender, RoutedEventArgs e)
      {
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonReadyRackChange_Click(): myGameEngine=null");
            return;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonReadyRackChange_Click(): myGameInstance=null");
            return;
         }
         IMapItem? rr = null;
         bool isCountIncrease = false;
         Button b = (Button)sender;
         switch (b.Name)
         {
            case "bMinusHe":
               rr = myGameInstance.ReadyRacks[0];
               myHeReadyRackCount -= 1;
               myUnassignedReadyRack += 1;
               break;
            case "bPlusHe":
               rr = myGameInstance.ReadyRacks[0];
               isCountIncrease = true;
               myHeReadyRackCount += 1;
               myUnassignedReadyRack -= 1;
               break;
            case "bMinusAp":
               rr = myGameInstance.ReadyRacks[1];
               myApReadyRackCount -= 1;
               myUnassignedReadyRack += 1;
               break;
            case "bPlusAp":
               rr = myGameInstance.ReadyRacks[1];
               isCountIncrease = true;
               myApReadyRackCount += 1;
               myUnassignedReadyRack -= 1;
               break;
            case "bMinusWp":
               rr = myGameInstance.ReadyRacks[2];
               myWpReadyRackCount -= 1;
               myUnassignedReadyRack += 1;
               break;
            case "bPlusWp":
               rr = myGameInstance.ReadyRacks[2];
               isCountIncrease = true;
               myWpReadyRackCount += 1;
               myUnassignedReadyRack -= 1;
               break;
            case "bMinusHbci":
               rr = myGameInstance.ReadyRacks[3];
               myHbciReadyRackCount -= 1;
               myUnassignedReadyRack += 1;
               break;
            case "bPlusHbci":
               rr = myGameInstance.ReadyRacks[3];
               isCountIncrease = true;
               myHbciReadyRackCount += 1;
               myUnassignedReadyRack -= 1;
               break;
            case "bMinusHvap":
               rr = myGameInstance.ReadyRacks[2];
               myHvapReadyRackCount -= 1;
               myUnassignedReadyRack += 1;
               break;
            case "bPlusHvap":
               rr = myGameInstance.ReadyRacks[2];
               isCountIncrease = true;
               myHvapReadyRackCount += 1;
               myUnassignedReadyRack -= 1;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ButtonAmmoChange_Click(): reached default with key=" + b.Name);
               break;
         }
         if (null == rr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonReadyRackChange_Click(): rr=null");
            return;
         }
         if (true == isCountIncrease)
            rr.Count++;
         else
            rr.Count--;
         string tName = rr.Name + rr.Count.ToString();
         ITerritory? newT = Territories.theTerritories.Find(tName);
         if (null == newT)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonReadyRackChange_Click(): newT=null for " + tName);
            return;
         }
         if( false == SetTerritory(rr, newT))
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonReadyRackChange_Click(): SetTerritory() returned false for " + tName);
            return;
         }
         GameAction action = GameAction.MorningBriefingAmmoReadyRackLoad;
         myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonAmmoChange_Click(): UpdateGrid() return false");
      }
   }
}
