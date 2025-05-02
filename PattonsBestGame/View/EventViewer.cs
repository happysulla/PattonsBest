using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using WpfAnimatedGif;
using static Pattons_Best.EventViewerEnemyAction;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using Point = System.Windows.Point;

namespace Pattons_Best
{
   public class EventViewer : IView
   {
      public bool CtorError { get; } = false;
      private IGameEngine? myGameEngine = null;
      private IGameInstance? myGameInstance = null;
      private ITerritories? myTerritories = null;
      //--------------------------------------------------------------------
      private IDieRoller? myDieRoller = null;
      public int DieRoll { set; get; } = 0;
      //--------------------------------------------------------------------
      public RuleDialogViewer? myRulesMgr;
      public AfterActionDialog? myAfterActionDialog = null;
      //--------------------------------------------------------------------
      private ScrollViewer? myScrollViewerTextBlock;
      private Canvas? myCanvasMain = null;
      private TextBlock? myTextBlock = null;
      //--------------------------------------------------------------------
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //--------------------------------------------------------------------
      public EventViewer(IGameEngine ge, IGameInstance gi, Canvas c, ScrollViewer sv, ITerritories territories, IDieRoller dr)
      {
         myDieRoller = dr;
         if (null == ge)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): c=null");
            CtorError = true;
            return;
         }
         myGameEngine = ge;
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): c=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         if (null == c)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): c=null");
            CtorError = true;
            return;
         }
         myCanvasMain = c;
         if (null == territories)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): territories=null");
            CtorError = true;
            return;
         }
         myTerritories = territories;
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewerTextBlock = sv;
         //--------------------------------------------------------
         if (myScrollViewerTextBlock.Content is TextBlock)
            myTextBlock = (TextBlock)myScrollViewerTextBlock.Content;  // Find the TextBox in the visual tree
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): myTextBlock=null");
            CtorError = true;
            return;
         }
         //--------------------------------------------------------
         myRulesMgr = new RuleDialogViewer(myGameInstance, myGameEngine);
         if (true == myRulesMgr.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): RuleDialogViewer.CtorError=true");
            CtorError = true;
            return;
         }
         //--------------------------------------------------------
         if (false == CreateEvents(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): CreateEvents() returned false");
            CtorError = true;
            return;
         }
         if (null == myRulesMgr.Events)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): myRulesMgr.Events=null");
            CtorError = true;
            return;
         }
         gi.Events.Add(gi.EventActive);
      }
      private bool CreateEvents(IGameInstance gi)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEvents(): myRulesMgr=null");
            return false;
         }
         try
         {
            string filename = ConfigFileReader.theConfigDirectory + "Events.txt";
            ConfigFileReader cfr = new ConfigFileReader(filename);
            if (true == cfr.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateEvents(): cfr.CtorError=true");
               return false;
            }
            myRulesMgr.Events = cfr.Entries;
            if (0 == myRulesMgr.Events.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateEvents(): myRulesMgr.Events.Count=0");
               return false;
            }
            foreach (string key in myRulesMgr.Events.Keys) // For each event, create a dictionary entry. There can be no more than three die rolls per event
               gi.DieResults[key] = new int[3] { Utilities.NO_RESULT, Utilities.NO_RESULT, Utilities.NO_RESULT };
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEvents(): e=" + e.ToString());
            return false;
         }
         return true;
      }
      //--------------------------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myGameInstance=null");
            return;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myRulesMgr=null");
            return;
         }
         if (null == myScrollViewerTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView() myScrollViewerTextBlock=null");
            return;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView() myDieRoller=null");
            return;
         }
         gi.IsGridActive = true;
         switch (action)
         {
            case GameAction.UnitTestCommand:
            case GameAction.UnitTestNext:
               break;
            case GameAction.UpdateBattleBoard:
               break;
            case GameAction.UpdateGameOptions:
               break;
            case GameAction.UpdateUndo:
               myScrollViewerTextBlock.Cursor = System.Windows.Input.Cursors.Arrow;
               gi.IsGridActive = false;
               if (false == OpenEvent(gi, gi.EventActive))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
            case GameAction.UpdateNewGame:
            case GameAction.UpdateLoadingGame:
               myGameInstance = gi;
               myRulesMgr.GameInstance = gi;
               gi.IsGridActive = false;
               myScrollViewerTextBlock.Cursor = Cursors.Arrow;
               try // resync the gi.DieResults[] to initial conditions
               {
                  foreach (string key in myRulesMgr.Events.Keys)
                     gi.DieResults[key] = new int[3] { Utilities.NO_RESULT, Utilities.NO_RESULT, Utilities.NO_RESULT };
               }
               catch (Exception e)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateEvents(): e=" + e.ToString());
                  return;
               }
               if (false == OpenEvent(gi, gi.EventActive))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
            case GameAction.ShowAfterActionReportDialog:
               if (null == myAfterActionDialog)
               {
                  IAfterActionReport? aar = gi.Reports.GetLast();
                  if (null == aar)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView():  gi.Reports.GetLast()=null");
                     return;
                  }
                  AfterActionReportUserControl aarUserControl = new AfterActionReportUserControl(aar);
                  if (true == aarUserControl.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): AfterActionReportUserControl CtorError=true");
                     return;
                  }
                  myAfterActionDialog = new AfterActionDialog(aar, CloseAfterActionDialog);
                  myAfterActionDialog.Show();
               }
               break;
            case GameAction.ShowCombatCalendarDialog:
               if( false == myRulesMgr.ShowTable("Calendar"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): SmyRulesMgr.ShowTable(Calendar)=false");
                  return;
               }
               break;
            case GameAction.ShowReportErrorDialog:
               ShowReportErrorDialog dialogReportError = new ShowReportErrorDialog();
               dialogReportError.Show();
               break;
            case GameAction.ShowAboutDialog:
               ShowAboutDialog dialogAbout = new ShowAboutDialog();
               dialogAbout.Show();
               break;
            case GameAction.ShowRuleListingDialog:
               RuleListingDialog dialogRuleListing = new RuleListingDialog(myRulesMgr, false);
               if (true == dialogRuleListing.CtorError)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): RuleListingDialog CtorError=true");
                  return;
               }
               dialogRuleListing.Show();
               break;
            case GameAction.ShowEventListingDialog:
               RuleListingDialog dialogEventListing = new RuleListingDialog(myRulesMgr, true);
               if (true == dialogEventListing.CtorError)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): RuleListingDialog CtorError=true");
                  return;
               }
               dialogEventListing.Show();
               break;
            case GameAction.ShowTableListing:
               TableListingDialog dialogTableListing = new TableListingDialog(myRulesMgr);
               if (true == dialogTableListing.CtorError)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): TableListingDialog CtorError=true");
                  return;
               }
               dialogTableListing.Show();
               break;
            case GameAction.ShowMovementDiagramDialog:
               ShowMovementDiagramDialog dialogMovementDiagram = new ShowMovementDiagramDialog();
               dialogMovementDiagram.Show();
               break;
            case GameAction.SetupAssignCrewRating:
            case GameAction.MorningBriefingAssignCrewRating:
               EventViewerCrewSetup newCrewMgr = new EventViewerCrewSetup(myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == newCrewMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): newCrewMgr.CtorError=true");
               else if (false == newCrewMgr.AssignNewCrewRatings(ShowCrewRatingResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): AssignNewCrewRatings() returned false");
               break;
            case GameAction.MorningBriefingAmmoLoad:
            case GameAction.MovementAmmoLoad:
               EventViewerAmmoSetup newAmmoLoadMgr = new EventViewerAmmoSetup(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == newAmmoLoadMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): newAmmoLoadMgr.CtorError=true");
               else if (false == newAmmoLoadMgr.LoadAmmo(ShowAmmoLoadResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): LoadAmmo() returned false");
               break;
            case GameAction.MorningBriefingAmmoReadyRackLoad:
               break;
            case GameAction.BattleActivation:
               EventViewerBattleSetup battleSetupMgr = new EventViewerBattleSetup(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == battleSetupMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): battleSetupMgr.CtorError=true");
               else if (false == battleSetupMgr.SetupBattle(ShowBattleSetupResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): SetupBattle() returned false");
               break;
            case GameAction.BattleResolveAdvanceFire:
               EventViewerResolveAdvanceFire battleResolveAdvFire = new EventViewerResolveAdvanceFire(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == battleResolveAdvFire.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): battleResolveAdvFire.CtorError=true");
               else if (false == battleResolveAdvFire.ResolveAdvanceFire(ShowBattleSetupFireResults) )
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ResolveAdvanceFire() returned false");
               break;
            case GameAction.BattleResolveArtilleryFire:
               EventViewerResolveArtilleryFire battleResolveArtFire = new EventViewerResolveArtilleryFire(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == battleResolveArtFire.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): battleResolveAdvFire.CtorError=true");
               else if (false == battleResolveArtFire.ResolveArtilleryFire(ShowBattleSetupFireResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ResolveArtilleryFire() returned false");
               break;
            case GameAction.BattleResolveAirStrike:
               EventViewerResolveAirStrike battleResolveAirStrike = new EventViewerResolveAirStrike(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == battleResolveAirStrike.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): battleResolveAdvFire.CtorError=true");
               else if (false == battleResolveAirStrike.ResolveAirStrike(ShowBattleSetupFireResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ResolveAirStrike() returned false");
               break;
            case GameAction.BattleAmbush:
               EventViewerEnemyAction battleAmbush = new EventViewerEnemyAction(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == battleAmbush.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): battleAmbush.CtorError=true");
               else if (false == battleAmbush.PerformEnemyAction(ShowBattleAmbush))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ResolveAirStrike() returned false");
               break;
            case GameAction.EveningDebriefingStart:
               EventViewerRatingImprove crewRatingImprove = new EventViewerRatingImprove(myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == crewRatingImprove.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): crewRatingImprove.CtorError=true");
               else if (false == crewRatingImprove.ImproveRatings(ShowRatingImproveResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ImproveRatings() returned false");
               break;
            case GameAction.UpdateEventViewerDisplay:
               gi.IsGridActive = false;
               if (false == OpenEvent(gi, gi.EventDisplayed))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
            case GameAction.EndGameLost:
            case GameAction.EndGameWin:
            default:
               gi.IsGridActive = false;
               if (false == OpenEvent(gi, gi.EventActive))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
         }
         if( null != myAfterActionDialog )
            myAfterActionDialog.UpdateReport();
      }
      public bool OpenEvent(IGameInstance gi, string key)
      {
         if (null == myScrollViewerTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myScrollViewerTextBlock=null");
            return false;
         }
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myTextBlock=null");
            return false;
         }
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myRulesMgr=null");
            return false;
         }
         if (null == myRulesMgr.Events)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myRulesMgr.Events=null");
            return false;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myDieRoller=null");
            return false;
         }
         //------------------------------------
         try
         {
            foreach (Inline inline in myTextBlock.Inlines) // Clean up resources from old link before adding new one
            {
               if (inline is InlineUIContainer)
               {
                  InlineUIContainer ui = (InlineUIContainer)inline;
                  if (ui.Child is Button b)
                     b.Click -= Button_Click;
               }
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): for key=" + key + " e=" + e.ToString());
            return false;
         }
         //------------------------------------
         try
         {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myTextBlockDisplay' xml:space='preserve' Width='573' Background='#FFB9EA9E' FontFamily='Georgia' FontSize='18' TextWrapping='WrapWithOverflow' IsHyphenationEnabled='true' LineStackingStrategy='BlockLineHeight' Margin='0,0,0,0'>");
            sb.Append(myRulesMgr.Events[key]);
            sb.Append(@"</TextBlock>");
            StringReader sr = new StringReader(sb.ToString());
            XmlTextReader xr = new XmlTextReader(sr);
            myTextBlock = (TextBlock)XamlReader.Load(xr);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): for key=" + key + " e=" + e.ToString());
            return false;
         }
         //------------------------------------
         myScrollViewerTextBlock.Content = myTextBlock;
         myTextBlock.MouseDown += TextBlock_MouseDown;
         //--------------------------------------------------
         int dieNumIndex = 0;
         bool isModified = true;
         bool[] isDieShown = new bool[4] { true, false, false, false };
         int[]? eventDieRolls = null;
         try
         {
            eventDieRolls = gi.DieResults[key];
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): for key=" + key + " e=" + e.ToString());
            return false;
         }
         //------------------------------------
         while (dieNumIndex < 3 && true == isModified) // substitute die rolls that have occurred when multiple die rolls are in myTextBlock
         {
            int dieCount = 0;
            isModified = false;
            foreach (Inline inline in myTextBlock.Inlines)
            {
               if (inline is InlineUIContainer)
               {
                  InlineUIContainer ui = (InlineUIContainer)inline;
                  if (ui.Child is Button b)
                  {
                     if (false == SetButtonState(gi, key, b))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): SetButtonState() returned false");
                        return false;
                     }
                  }
                  else if (ui.Child is Image img)
                  {
                     string imageName = img.Name;
                     if (true == img.Name.Contains("Continue"))
                        imageName = "Continue";
                     else if( true == img.Name.Contains("DieRollWhite"))
                        imageName = "DieRollWhite";
                     else if (true == img.Name.Contains("DieRollBlue"))
                        imageName = "DieRollBlue";
                     else if (true == img.Name.Contains("DieRoll"))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): imageName=DieRoll for key=" + key);
                        return false;
                     }
                     string fullImagePath = MapImage.theImageDirectory + Utilities.RemoveSpaces(imageName) + ".gif";
                     System.Windows.Media.Imaging.BitmapImage bitImage = new BitmapImage();
                     bitImage.BeginInit();
                     bitImage.UriSource = new Uri(fullImagePath, UriKind.Absolute);
                     bitImage.EndInit();
                     img.Source = bitImage;
                     ImageBehavior.SetAnimatedSource(img, img.Source);
                     if ((true == img.Name.Contains("DieRollWhite")) || (true == img.Name.Contains("DieRollBlue")))
                     {
                        if (true == isDieShown[dieCount])
                        {
                           if (Utilities.NO_RESULT == eventDieRolls[dieNumIndex]) // if true, perform a one time insert b/c dieNumIndex increments by one
                           {

                           }
                           else
                           {
                              img.Visibility = Visibility.Hidden;
                              if (false == gi.IsMultipleSelectForDieResult)
                              {
                                 Run newInline = new Run(eventDieRolls[dieNumIndex].ToString());  // Insert the die roll number result
                                 myTextBlock.Inlines.InsertBefore(inline, newInline); // If modified, need to start again
                              }
                              else
                              {
                                 Button b1 = new Button() { Content = eventDieRolls[dieNumIndex].ToString(), FontFamily = myFontFam1, FontSize = 12, Height = 16, Width = 48 };
                                 myTextBlock.Inlines.InsertAfter(inline, new InlineUIContainer(b1));
                                 b1.Click += Button_Click;
                              }
                              isModified = true;
                              ++dieNumIndex;
                              isDieShown[dieCount] = false;
                              isDieShown[++dieCount] = true;
                              break;
                           }
                        }
                     }
                  }
               }
            }// end foreach
         } // end while
           //--------------------------------------------------
         myDieRoller.DieMutex.WaitOne();
         if( false == UpdateEventContent(gi, key))
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): UpdateEventContent() returned false");
            return false;
         }
         myDieRoller.DieMutex.ReleaseMutex();
         //--------------------------------------------------
         if (gi.EventDisplayed == gi.EventActive)
         {
            myScrollViewerTextBlock.Background = Utilities.theBrushScrollViewerActive;
            myTextBlock.Background = Utilities.theBrushScrollViewerActive;
         }
         else
         {
            myScrollViewerTextBlock.Background = Utilities.theBrushScrollViewerInActive;
            myTextBlock.Background = Utilities.theBrushScrollViewerInActive;
         }
            
         return true;
      }
      public bool ShowRule(string key)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRule(): myRulesMgr=null");
            return false;
         }
         if (false == myRulesMgr.ShowRule(key))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRule() key=" + key);
            return false;
         }
         return true;
      }
      public bool ShowTable(string key)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTable(): myRulesMgr=null");
            return false;
         }
         if (false == myRulesMgr.ShowTable(key))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTable() key=" + key);
            return false;
         }
         return true;
      }
      public bool ShowRegion(string key)
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): myCanvasMain=null");
            return false;
         }
         if (null == myTerritories)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): myTerritories=null");
            return false;
         }
         // Remove any existing UI elements from the Canvas
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Polygon)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myCanvasMain.Children.Remove(ui1);
         //--------------------------------
         ITerritory? t = myTerritories.Find(key);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): Unable to find name=" + key);
            return false;
         }
         //--------------------------------
         if (false == SetThumbnailState(myCanvasMain, t))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): SetThumbnailState returned false name=" + key);
            return false;
         }
         PointCollection points = new PointCollection();
         foreach (IMapPoint mp1 in t.Points)
            points.Add(new System.Windows.Point(mp1.X, mp1.Y));
         Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Tag = t.ToString() };
         myCanvasMain.Children.Add(aPolygon);
         return true;
      }
      //--------------------------------------------------------------------
      private bool UpdateEventContent(IGameInstance gi, string key)
      {
         if( null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): myTextBlock=null");
            return false;
         }
         Logger.Log(LogEnum.LE_VIEW_APPEND_EVENT, "UpdateEventContent(): k=" + key + " d0=" + gi.DieResults[key][0].ToString() + " d1=" + gi.DieResults[key][1].ToString() + " d2=" + gi.DieResults[key][2].ToString());
         IAfterActionReport? report = gi.Reports.GetLast();
         if( null == report )
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent():  gi.Reports.GetLast()");
            return false;
         }
         ICombatCalendarEntry? entry = TableMgr.theCombatCalendarEntries[gi.Day];
         if( null == entry )
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): entry=null for day=" + gi.Day);
            return false;
         }
         //--------------------------------------------------------------------------
         int firstDieResult = gi.DieResults[key][0];
         switch (key)
         {
            case "e006":
               ReplaceText("DATE", report.Day);
               switch(report.Resistance)
               {
                  case EnumResistance.Light:
                     ReplaceText("RESISTANCE", "Light");
                     break;
                  case EnumResistance.Medium:
                     ReplaceText("RESISTANCE", "Medium");
                     break;
                  case EnumResistance.Heavy:
                     ReplaceText("RESISTANCE", "Heavy");
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): reached default resistance=" + report.Resistance.ToString());
                     return false;
               }
               ReplaceText("PROBABILITY", report.Probability.ToString());
               if (Utilities.NO_RESULT == firstDieResult) 
               {
                  Image imgSun = new Image { Source = MapItem.theMapImages.GetBitmapImage("Morning"), Width = 300, Height = 150 };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                           "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgSun));
               }
               else if (report.Probability < firstDieResult) // skip today action
               {
                  Image imgSkip = new Image { Source = MapItem.theMapImages.GetBitmapImage("Sherman4"), Width = 300, Height = 190, Name = "GotoMorningBriefingEnd" }; 
                  myTextBlock.Inlines.Add(new Run("                               "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgSkip));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("No combat for today. Sleep good tonight. Click image to continue."));
               }
               else 
               {
                  Image imgBrief = new Image { Source = MapItem.theMapImages.GetBitmapImage("DailyDecision"), Width = 200, Height = 200, Name = "GotoMorningBriefing" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                  "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgBrief));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Possible contact. Click image to continue."));
               }
               break;
            case "e007":
               if ( Utilities.NO_RESULT < gi.DieResults[key][0] )
               {
                  Image? imgWeather = null;
                  switch (report.Weather)
                  {
                     case "Clear":
                        imgWeather = new Image { Source = MapItem.theMapImages.GetBitmapImage("WeatherClear"), Width = 150, Height = 150, Name = "WeatherRollEnd" };
                        break;
                     case "Overcast":
                        imgWeather = new Image { Source = MapItem.theMapImages.GetBitmapImage("WeatherOvercast"), Width = 400, Height = 266, Name = "WeatherRollEnd" };
                        break;
                     case "Fog":
                        imgWeather = new Image { Source = MapItem.theMapImages.GetBitmapImage("WeatherFog"), Width = 400, Height = 266, Name = "WeatherRollEnd" };
                        break;
                     case "Mud":
                        imgWeather = new Image { Source = MapItem.theMapImages.GetBitmapImage("WeatherMud"), Width = 400, Height = 266, Name = "WeatherRollEnd" };
                        break;
                     case "Mud/Overcast":
                        imgWeather = new Image { Source = MapItem.theMapImages.GetBitmapImage("WeatherOvercastMud"), Width = 400, Height = 266, Name = "WeatherRollEnd" };
                        break;
                     case "Snow":
                        imgWeather = new Image { Source = MapItem.theMapImages.GetBitmapImage("WeatherSnowFalling"), Width = 400, Height = 266, Name = "WeatherRollEnd" };
                        break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): reached default snow=" + report.Weather);
                        return false;
                  }
                  if( null == imgWeather)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): img=null for key=" + key);
                     return false;
                  }
                  myTextBlock.Inlines.Add(new Run("Weather calls for " + report.Weather + ":"));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  if( "Clear" == report.Weather )
                     myTextBlock.Inlines.Add(new Run("                                    "));
                  else
                     myTextBlock.Inlines.Add(new Run("                "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgWeather));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e008":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image? imgWeather = null;
                  switch (report.Weather)
                  {
                     case "Ground Snow":
                        imgWeather = new Image { Source = MapItem.theMapImages.GetBitmapImage("WeatherSnowGround"), Width = 400, Height = 266, Name = "SnowRollEnd" };
                        break;
                     case "Deep Snow":
                        imgWeather = new Image { Source = MapItem.theMapImages.GetBitmapImage("WeatherSnowDeep"), Width = 400, Height = 266, Name = "SnowRollEnd" };
                        break;
                     case "Falling Snow":
                        imgWeather = new Image { Source = MapItem.theMapImages.GetBitmapImage("WeatherSnowFalling"), Width = 400, Height = 266, Name = "SnowRollEnd" };
                        break;
                     case "Falling and Deep Snow":
                        imgWeather = new Image { Source = MapItem.theMapImages.GetBitmapImage("WeatherSnowFallingDeep"), Width = 400, Height = 266, Name = "SnowRollEnd" };
                        break;
                     case "Falling and Ground Snow":
                        imgWeather = new Image { Source = MapItem.theMapImages.GetBitmapImage("WeatherSnowFallingGround"), Width = 400, Height = 266, Name = "SnowRollEnd" };
                        break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): reached default snow=" + report.Weather);
                        return false;
                  }
                  if (null == imgWeather)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): img=null for key=" + key);
                     return false;
                  }
                  myTextBlock.Inlines.Add(new Run("Weather calls for " + report.Weather + ":"));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("               "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgWeather));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e009":
               TankCard card = new TankCard(report.TankCardNum);
               ReplaceText("AMMO_NORMAL_LOAD", card.myNumMainGunRound.ToString());
               if ("75" == card.myMainGun)
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("-- WP:") { FontWeight = FontWeights.Bold });
                  myTextBlock.Inlines.Add(new Run(" Unlimited "));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("-- HBCI:") { FontWeight = FontWeights.Bold });
                  myTextBlock.Inlines.Add(new Run(" 1D Roll "));
               }
               //-----------------------------------------------
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                     "));
               Image imgContinue = new Image { Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = 100, Height = 100, Name = "GotoMorningAmmoLimitsSetEnd" };
               myTextBlock.Inlines.Add(new InlineUIContainer(imgContinue));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e010":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  StringBuilder sbE010 = new StringBuilder("On ");
                  sbE010.Append(entry.Date);
                  sbE010.Append(": Sunset = ");
                  sbE010.Append(Utilities.GetTime(report.SunriseHour, report.SunriseMin));
                  sbE010.Append(" and Sunrise = ");
                  sbE010.Append(Utilities.GetTime(report.SunsetHour, report.SunsetMin));
                  myTextBlock.Inlines.Add(new Run(sbE010.ToString()));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  int heLost = firstDieResult * 2;
                  sbE010 = new StringBuilder(" HE Expended = " + heLost.ToString() + "   and   .30MG expended = " + firstDieResult.ToString());
                  myTextBlock.Inlines.Add(new Run(sbE010.ToString()));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                   "));
                  Image imgClock = new Image { Source = MapItem.theMapImages.GetBitmapImage("MilitaryWatch"), Width = 200, Height = 100, Name = "PreparationsDeployment" };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgClock));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e011":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  StringBuilder sbE011 = new StringBuilder();
                  sbE011.Append(" Is Hulled Down =  ");
                  sbE011.Append( gi.Sherman.IsHullDown.ToString() );
                  sbE011.Append("\n Is Moving  = ");
                  sbE011.Append(gi.Sherman.IsMoving.ToString());
                  sbE011.Append("\n Is Lead Tank  = ");
                  sbE011.Append(gi.IsLeadTank.ToString());
                  myTextBlock.Inlines.Add(new Run(sbE011.ToString()));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge011 = new Image { Name = "PreparationsDeploymentEnd" };
                  if (true == gi.Sherman.IsHullDown)
                  {
                     imge011.Source = MapItem.theMapImages.GetBitmapImage("c14HullDown");
                     imge011.Width = 300;
                     imge011.Height = 150;
                     myTextBlock.Inlines.Add(new Run("                            "));
                  }
                  else if( true == gi.Sherman.IsMoving )
                  {
                     imge011.Source = MapItem.theMapImages.GetBitmapImage("c13Moving");
                     imge011.Width = 100;
                     imge011.Height = 133;
                     myTextBlock.Inlines.Add(new Run("                                           "));
                  }
                  else
                  {
                     imge011.Source = MapItem.theMapImages.GetBitmapImage("Continue");
                     imge011.Width = 100;
                     imge011.Height = 100;
                     myTextBlock.Inlines.Add(new Run("                                           "));
                  }
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge011));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e015":
               IMapItem? loaderSpot = gi.BattleStacks.FindMapItem("LoaderSpot");
               if (null != loaderSpot)
               {
                  Image imge015 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c18LoaderSpot"), Width = 100, Height = 100, Name = "PreparationsCommanderSpot" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                           "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge015));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e016":
               IMapItem? cmdrSpot = gi.BattleStacks.FindMapItem("CommanderSpot");
               if (null != cmdrSpot)
               {
                  Image imge016 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c19CommanderSpot"), Width = 100, Height = 100, Name = "PreparationsFinal" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                           "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge016));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e018":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge018 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c33StartArea"), Width = 100, Height = 100, Name = "MovementExitAreaSet" };
                  myTextBlock.Inlines.Add(new Run("                                           "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge018));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e019":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge019 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c34ExitArea"), Width = 100, Height = 100, Name = "MovementEnemyStrengthChoice" };
                  myTextBlock.Inlines.Add(new Run("                                           "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge019));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e021":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  if( null == gi.EnemyStrengthCheckTerritory )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): gi.EnemyStrenthCheck=null");
                     return false;
                  }
                  Image imge021 = new Image { Width = 100, Height = 100, Name = "MovementChooseOption" };
                  IStack? stack = gi.MoveStacks.Find(gi.EnemyStrengthCheckTerritory);
                  if( null == stack )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): stack=null for e021");
                     return false;
                  }
                  if( 0 < stack.MapItems.Count )
                  {
                     IMapItem? mi = stack.MapItems[0];
                     if (null == mi)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): mi=null for e021");
                        return false;
                     }
                     if (1 == mi.Count)
                        imge021.Source = MapItem.theMapImages.GetBitmapImage("c36Light");
                     else if (2 == mi.Count)
                        imge021.Source = MapItem.theMapImages.GetBitmapImage("c37Medium");
                     else
                        imge021.Source = MapItem.theMapImages.GetBitmapImage("c38Heavy");
                  }
                  myTextBlock.Inlines.Add(new Run("                                           "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge021));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e022":
               if (true == gi.IsAirStrikePending)
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Air Strike is pending. Perform strength check or artillery support while waiting --or-- "));
                  Button b1 = new Button() { Content = "Cancel", FontFamily = myFontFam1, FontSize = 12 };
                  b1.Click += Button_Click;
                  myTextBlock.Inlines.Add(new InlineUIContainer(b1));
                  myTextBlock.Inlines.Add(new Run(" Air Strike if you want different choice. Time is not recovered."));
               }
               break;
            case "e024":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge024 = new Image { Source= MapItem.theMapImages.GetBitmapImage("c39ArtillerySupport"), Width = 100, Height = 100, Name = "MovementChooseOption" };
                  myTextBlock.Inlines.Add(new Run("                                           "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge024));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  if ( 7 < gi.DieResults[key][0] )
                     myTextBlock.Inlines.Add(new Run("No Artillery Support available now. Click image to continue."));
                  else
                     myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e026":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge026 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c40AirStrike"), Width = 100, Height = 100, Name = "MovementChooseOption" };
                  myTextBlock.Inlines.Add(new Run("                                           "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge026));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  if (4 < gi.DieResults[key][0])
                     myTextBlock.Inlines.Add(new Run("No Air Strike available now. Click image to continue."));
                  else
                     myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e027":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge027 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c29AmmoReload"), Width = 100, Height = 100, Name = "MovementChooseOption" };
                  myTextBlock.Inlines.Add(new Run("                                           "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge027));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  if (7 < gi.DieResults[key][0])
                     myTextBlock.Inlines.Add(new Run("No Resupply available now. Click image to continue."));
                  else
                     myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e030": // This event is only shown if battle check resulted in combat
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  int heRoundsUsed = (int)Math.Floor((double)gi.DieResults[key][0] / 2.0);
                  int mgRoundsUsed = gi.DieResults[key][0];
                  StringBuilder sb = new StringBuilder();
                  sb.Append("HE Rounds Used = ");
                  sb.Append(heRoundsUsed.ToString());
                  sb.Append("\nMG Boxes Used = ");
                  sb.Append(mgRoundsUsed.ToString());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run(sb.ToString()));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge030 = new Image { Width = 100, Height = 100, Name = "MovementAdvanceFire", Source = MapItem.theMapImages.GetBitmapImage("c44AdvanceFire") };
                  myTextBlock.Inlines.Add(new Run("                                          "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge030));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e031":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  if (null == gi.EnemyStrengthCheckTerritory)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): gi.EnemyStrenthCheck=null");
                     return false;
                  }
                  Image imge030 = new Image { Width = 100, Height = 100, Name= "MovementBattleCheck" };
                  if (EnumResistance.Light == gi.BattleResistance)
                     imge030.Source = MapItem.theMapImages.GetBitmapImage("c36Light");
                  else if (EnumResistance.Medium == gi.BattleResistance)
                     imge030.Source = MapItem.theMapImages.GetBitmapImage("c37Medium");
                  else if (EnumResistance.Heavy == gi.BattleResistance)
                     imge030.Source = MapItem.theMapImages.GetBitmapImage("c38Heavy");
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): gi.BattleResistance=" + gi.BattleResistance.ToString());
                     return false;
                  }
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                      "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge030));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e032": // This event is only shown if battle check resulted in combat
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge031 = new Image { Width = 150, Height = 150, Name = "BattleStart", Source = MapItem.theMapImages.GetBitmapImage("Combat") }; 
                  myTextBlock.Inlines.Add(new Run("Combat! Enter Battle Board."));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                     "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge031));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e033":
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               Image imge032 = new Image { Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("c28UsControl") };
               Button b2 = new Button() { FontFamily = myFontFam1, FontSize = 12 };
               if ( false == gi.IsDaylightLeft(report))
               {                 
                  imge032.Name = "Debrief";
                  b2.Content = "r4.9";
                  b2.Click += Button_Click;
                  myTextBlock.Inlines.Add(new Run("Since there is no daylight left, go to Evening Debriefing "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(b2));
                  myTextBlock.Inlines.Add(new Run("."));
               }
               else
               {
                  bool isExitArea;
                  if( false == gi.IsExitArea(out isExitArea))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): gi.IsExitArea() returned false");
                     return false;
                  }
                  if( true == isExitArea )
                  {
                     imge032.Name = "MovementStartAreaRestart";
                     b2.Content = "r4.51";
                     b2.Click += Button_Click;
                     myTextBlock.Inlines.Add(new Run("Since in exit area and daylight remains, determine a new start area per "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(b2));
                     myTextBlock.Inlines.Add(new Run("."));
                  }
                  else
                  {
                     imge032.Name = "MovementEnemyStrengthChoice";
                     b2.Content = "r4.53";
                     b2.Click += Button_Click;
                     myTextBlock.Inlines.Add(new Run("Since not in exit area and daylight remains, go to Enemy Strength Check "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(b2));
                     myTextBlock.Inlines.Add(new Run("."));
                  }
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                           "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imge032));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e035":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image? imge037 = null;
                  if (true == gi.IsAmbush)
                  {
                     myTextBlock.Inlines.Add(new Run("Ambush! Click image to continue."));
                     imge037 = new Image { Name = "Ambush", Width = 400, Height = 240, Source = MapItem.theMapImages.GetBitmapImage("Ambush") };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                "));
                  }
                  else
                  {
                     myTextBlock.Inlines.Add(new Run("No ambush. Click image to continue."));
                     imge037 = new Image { Name="Continue35", Width = 200, Height = 210, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                  "));
                  }
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge037));
               }
               else
               {
                  if ((true == report.Weather.Contains("Rain")) || (true == report.Weather.Contains("Fog")) || (true == report.Weather.Contains("Falling")))
                     myTextBlock.Inlines.Add(new Run("Subtracting one for Rain, Fog, or Falling Snow."));
               }
               break;
            default:
               break;
         }
         return true;
      }
      private void ReplaceText(string keyword, string newString)
      {
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReplaceText(): myTextBlock=null");
            return;
         }
         TextRange text = new TextRange(myTextBlock.ContentStart, myTextBlock.ContentEnd);
         TextPointer current = text.Start.GetInsertionPosition(LogicalDirection.Forward);
         while (current != null)
         {
            string textInRun = current.GetTextInRun(LogicalDirection.Forward);
            if (!string.IsNullOrWhiteSpace(textInRun))
            {
               int index = textInRun.IndexOf(keyword);
               if (index != -1)
               {
                  TextPointer selectionStart = current.GetPositionAtOffset(index, LogicalDirection.Forward);
                  TextPointer selectionEnd = selectionStart.GetPositionAtOffset(keyword.Length, LogicalDirection.Forward);
                  TextRange selection = new TextRange(selectionStart, selectionEnd);
                  selection.Text = newString;
               }
            }
            current = current.GetNextContextPosition(LogicalDirection.Forward);
         }
      }
      private bool SetThumbnailState(Canvas c, ITerritory t)
      {
         ScrollViewer scrollViewer = (ScrollViewer)c.Parent; // set thumbnails of scroll viewer to find the target hex
         if (null == scrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetThumbnailState(): scrollViewer=null");
            return false;
         }
         double percentHeight = (t.CenterPoint.Y / c.ActualHeight);
         double percentToScroll = 0.0;
         if (percentHeight < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentHeight)
            percentToScroll = 1.0;
         else
            percentToScroll = percentHeight / 0.5 - 0.5;
         double amountToScroll = percentToScroll * scrollViewer.ScrollableHeight;
         scrollViewer.ScrollToVerticalOffset(amountToScroll);
         //--------------------------------------------------------------------
         double percentWidth = (t.CenterPoint.X / c.ActualWidth);
         if (percentWidth < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentWidth)
            percentToScroll = 1.0;
         else
            percentToScroll = percentWidth / 0.5 - 0.5;
         amountToScroll = percentToScroll * scrollViewer.ScrollableWidth;
         scrollViewer.ScrollToHorizontalOffset(amountToScroll);
         return true;
      }
      private bool SetButtonState(IGameInstance gi, string key, Button b)
      {
         string content = (string)b.Content;
         if (null == content)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer.SetButtonState(): content=null for key=" + key);
            return false;
         }
         if ((key != gi.EventActive) && (false == content.StartsWith("e")))
         {
            b.IsEnabled = false;
            return true;
         }
         switch (key)
         {
            case "e022":
               if ("Strength Check" == content)
               {
                  bool isStrengthCheck = false;
                  if ((false == IsEnemyStrengthCheckNeeded(gi, out isStrengthCheck)))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EventViewer.SetButtonState(): IsEnemyStrengthCheckNeeded() returned false");
                     return false;
                  }
                  b.IsEnabled = isStrengthCheck;
               }
               else if ("Strike" == content)
               {
                  if ((false == SetButtonAirStrike(gi, b)))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EventViewer.SetButtonState(): SetButtonStateEnemyStrength() returned false");
                     return false;
                  }
               }
               break;
            default:
               break;
         }
         b.Click += Button_Click;
         return true;
      }
      private bool IsEnemyStrengthCheckNeeded(IGameInstance gi, out bool isCheckNeeded)
      {
         isCheckNeeded = false;
         ITerritory? enteredTerritory = gi.EnteredArea;
         if (null == enteredTerritory)
         {
            IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
            if (null == taskForce)
            {
               Logger.Log(LogEnum.LE_ERROR, "SetButtonStateEnemyStrength(): taskForce=null");
               return false;
            }
            enteredTerritory = taskForce.TerritoryCurrent;
         }
         //--------------------------------
         List<String> sTerritories = enteredTerritory.Adjacents;
         foreach (string s in sTerritories)  // Look at each adjacent territory
         {
            if (true == s.Contains("E")) // Ignore Entry or Exit Areas
               continue;
            Logger.Log(LogEnum.LE_SHOW_ENEMY_STRENGTH, "IsEnemyStrengthCheckNeeded(): Checking territory=" + enteredTerritory.Name + " adj=" + s);
            ITerritory? t = Territories.theTerritories.Find(s);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEnemyStrengthCheckNeeded(): t=null for s=" + s);
               return false;
            }
            Logger.Log(LogEnum.LE_SHOW_ENEMY_STRENGTH, "IsEnemyStrengthCheckNeeded(): Checking territory=" + s);
            IStack? stack = gi.MoveStacks.Find(t);
            if (null == stack)
            {
               Logger.Log(LogEnum.LE_SHOW_ENEMY_STRENGTH, "IsEnemyStrengthCheckNeeded(): no stack for=" + s + " in " + gi.MoveStacks.ToString());
               isCheckNeeded = true;
               return true;
            }
            else
            {
               bool isCounterInStack = false;
               foreach (IMapItem mi1 in stack.MapItems)
               {
                  if ((true == mi1.Name.Contains("Strength")) || (true == mi1.Name.Contains("UsControl")))
                  {
                     Logger.Log(LogEnum.LE_SHOW_ENEMY_STRENGTH, "SetButtonStateEnemyStrength(): Found mi=" + mi1.Name + " for=" + s + " in " + gi.MoveStacks.ToString());
                     isCounterInStack = true;
                     break;
                  }
               }
               if (false == isCounterInStack)
               {
                  Logger.Log(LogEnum.LE_SHOW_ENEMY_STRENGTH, "SetButtonStateEnemyStrength(): no counter for=" + s + " in " + gi.MoveStacks.ToString());
                  isCheckNeeded = true;
                  return true;
               }
            }
         }
         return true;
      }
      private bool SetButtonAirStrike(IGameInstance gi, Button b)
      {
         if( true == gi.IsAirStrikePending )
            b.IsEnabled = false;
         else
            b.IsEnabled = true;
         return true;
      }
      //--------------------------------------------------------------------
      public void CloseAfterActionDialog()
      {
         myAfterActionDialog = null;
      }
      public void ShowDieResult(int dieRoll)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResult(): myGameInstance=null");
            return;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResult(): myGameEngine=null");
            return;
         }
         myGameInstance.EventActive = myGameInstance.EventDisplayed; // As soon as you roll the die, the current event becomes the active event
         GameAction action = myGameInstance.DieRollAction;
         StringBuilder sb11 = new StringBuilder("      ######ShowDieResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(action.ToString());
         sb11.Append(" dr="); sb11.Append(dieRoll.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref action, dieRoll);
      }
      public bool ShowCrewRatingResults()
      {
         if( null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowCrewRatingResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowCrewRatingResults(): myGameEngine=null");
            return false;
         }
         GameAction outAction = GameAction.Error;
         if( GamePhase.GameSetup == myGameInstance.GamePhase ) 
            outAction = GameAction.SetupShowCombatCalendarCheck;
         else
            outAction = GameAction.MorningBriefingAssignCrewRatingEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowCrewRatingResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowAmmoLoadResults()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowAmmoLoadResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowAmmoLoadResults(): myGameEngine=null");
            return false;
         }
         GameAction outAction = GameAction.Error;
         if ( GamePhase.MorningBriefing == myGameInstance.GamePhase)
            outAction = GameAction.MorningBriefingTimeCheck;
         else
            outAction = GameAction.MovementChooseOption;
         StringBuilder sb11 = new StringBuilder("     ######ShowAmmoLoadResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowBattleSetupResults()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupResults(): myGameEngine=null");
            return false;
         }
         //--------------------------------------------------
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupFireResults(): lastReport=null");
            return false;
         }
         if (null == myGameInstance.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupFireResults(): myGameInstance.EnteredArea=null");
            return false;
         }
         IStack? stackEnteredArea = myGameInstance.MoveStacks.Find(myGameInstance.EnteredArea);
         if (null == stackEnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupFireResults(): stackEnteredArea=null");
            return false;
         }
         bool isAirStrike = false;
         bool isArtilleryStrike = false;
         foreach (IMapItem mi in stackEnteredArea.MapItems)
         {
            if (true == mi.Name.Contains("Air"))
               isAirStrike = true;
            if (true == mi.Name.Contains("Artillery"))
               isArtilleryStrike = true;
         }
         //--------------------------------------------------
         GameAction outAction = GameAction.BattleAmbushStart;
         int enemyCount = 0;
         bool isEnemyUnitAvailableForAirStrike = false;
         IMapItems removals = new MapItems();
         foreach (IStack stack in myGameInstance.BattleStacks)
         {
            Logger.Log(LogEnum.LE_VIEW_ADV_FIRE_RESOLVE, "ShowBattleSetupFireResults(): stack=" + stack.ToString());
            bool isEnemyUnitInTerritory = false;
            IMapItem? advanceFireMarker = null; ;
            foreach (IMapItem mi in stack.MapItems)
            {
               Logger.Log(LogEnum.LE_VIEW_ADV_FIRE_RESOLVE, "ShowBattleSetupFireResults(): mi=" + mi.Name);
               if (true == Utilities.IsEnemyUnit(mi))
               {
                  ++enemyCount;
                  Logger.Log(LogEnum.LE_VIEW_ADV_FIRE_RESOLVE, "ShowBattleSetupFireResults(): c=" + enemyCount);
                  if (((false == lastReport.Weather.Contains("Fog")) && (false == lastReport.Weather.Contains("Falling"))) || (("B6M" != stack.Territory.Name) && ("B6L" != stack.Territory.Name)))
                  {
                     isEnemyUnitAvailableForAirStrike = true;
                     isEnemyUnitInTerritory = true;
                     break;
                  }
               }
               if (true == mi.Name.Contains("AdvanceFire"))
                  advanceFireMarker = mi;
            }
            if (null == advanceFireMarker)
               continue;
            if (true == isEnemyUnitInTerritory)
               outAction = GameAction.BattleResolveAdvanceFire;
            else
               removals.Add(advanceFireMarker);
         }
         //--------------------------------------------------
         if (GameAction.BattleResolveAdvanceFire != outAction) 
         {
            if (true == isArtilleryStrike)
               outAction = GameAction.BattleResolveArtilleryFire;
            else if ((true == isAirStrike) && (true == isEnemyUnitAvailableForAirStrike))
               outAction = GameAction.BattleResolveAirStrike;
         }
         if (0 == enemyCount)
            outAction = GameAction.BattleEmpty;
         //--------------------------------------------------
         foreach (IMapItem mi in removals)
            myGameInstance.BattleStacks.Remove(mi);
         //--------------------------------------------------
         StringBuilder sb11 = new StringBuilder("     ######ShowBattleSetupResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowBattleSetupFireResults()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupFireResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupFireResults(): myGameEngine=null");
            return false;
         }
         //--------------------------------------------------
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupFireResults(): lastReport=null");
            return false;
         }
         if (null == myGameInstance.EnteredArea)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupFireResults(): myGameInstance.EnteredArea=null");
            return false;
         }
         IStack? stackEnteredArea = myGameInstance.MoveStacks.Find(myGameInstance.EnteredArea);
         if( null == stackEnteredArea )
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupFireResults(): stackEnteredArea=null");
            return false;
         }
         bool isAirStrike = false;
         bool isArtilleryStrike = false;
         foreach (IMapItem mi in stackEnteredArea.MapItems)
         {
            if (true == mi.Name.Contains("Air"))
               isAirStrike = true;
            if (true == mi.Name.Contains("Artillery"))
               isArtilleryStrike = true;
         }
         //--------------------------------------------------
         GameAction outAction = GameAction.BattleAmbushStart;
         int enemyCount = 0;
         bool isEnemyUnitAvailableForAirStrike = false;
         foreach (IStack stack in myGameInstance.BattleStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == Utilities.IsEnemyUnit(mi))
               {
                  ++enemyCount;
                  if ( ((false == lastReport.Weather.Contains("Fog")) && (false == lastReport.Weather.Contains("Falling"))) || (("B6M" != stack.Territory.Name) && ("B6L" != stack.Territory.Name)) )
                  {
                     isEnemyUnitAvailableForAirStrike = true;
                     break;
                  }
               }
            }
         }
         //--------------------------------------------------
         if (true == isArtilleryStrike) 
            outAction = GameAction.BattleResolveArtilleryFire;
         else if ((true == isAirStrike) && (true == isEnemyUnitAvailableForAirStrike) )
            outAction = GameAction.BattleResolveAirStrike;
         if ( 0 == enemyCount )
            outAction = GameAction.BattleEmpty;
         //--------------------------------------------------
         StringBuilder sb11 = new StringBuilder("     ######ShowBattleSetupFireResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowBattleAmbush()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupFireResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupFireResults(): myGameEngine=null");
            return false;
         }
         GameAction outAction = GameAction.Error;
         //--------------------------------------------------
         StringBuilder sb11 = new StringBuilder("     ######ShowBattleAmbush() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowRatingImproveResults()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRatingImproveResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRatingImproveResults(): myGameEngine=null");
            return false;
         }
         GameAction outAction = GameAction.Error;
         if (GamePhase.GameSetup == myGameInstance.GamePhase)
            outAction = GameAction.SetupShowCombatCalendarCheck;
         else
            outAction = GameAction.MorningBriefingAssignCrewRatingEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowRatingImproveResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      //--------------------------------------------------------------------
      private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): myGameEngine=null");
            return;
         }
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): myGameInstance=null");
            return;
         }
         if ( null == myCanvasMain )
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): myCanvasMain=null");
            return;
         }
         if (null == myDieRoller)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): myDieRoller=null");
            return;
         }
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): myTextBlock=null");
            return;
         }
         //------------------------------------------------------------------------
         GameAction action = GameAction.Error;
         if (myGameInstance.EventActive != myGameInstance.EventDisplayed) // if an image is clicked, only take action if on active screen
         {
            ReturnToActiveEventDialog dialog = new ReturnToActiveEventDialog(); // Get the name from user
            dialog.Topmost = true;
            if (true == dialog.ShowDialog())
            {
               GameAction actionGoto = GameAction.UpdateEventViewerActive;
               myGameEngine.PerformAction(ref myGameInstance, ref actionGoto);
            }
            return;
         }
         //------------------------------------------------------------------------
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myTextBlock, p);  // Get the Point where the hit test occurrs
         foreach (Inline item in myTextBlock.Inlines)
         {
            if (item is InlineUIContainer ui)
            {
               if (ui.Child is Image)
               {
                  Image img = (Image)ui.Child;
                  if (result.VisualHit == img)
                  {
                     RollEndCallback rollEndCallback = ShowDieResult;
                     switch (img.Name)
                     {
                        case "DieRollWhite":
                           myDieRoller.RollMovingDie(myCanvasMain, rollEndCallback);
                           img.Visibility = Visibility.Hidden;
                           return;
                        case "DieRollBlue":
                           myDieRoller.RollMovingDice(myCanvasMain, rollEndCallback);
                           img.Visibility = Visibility.Hidden;
                           return;
                        case "Continue001":
                           action = GameAction.SetupShowMovementBoard;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MapMovement":
                           action = GameAction.SetupShowBattleBoard;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MapBattle":
                           action = GameAction.SetupShowTankCard;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "m001M4":
                           action = GameAction.SetupShowAfterActionReport;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "Continue005":
                           action = GameAction.SetupAssignCrewRating;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "GotoMorningBriefing":
                           action = GameAction.MorningBriefingBegin;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "WeatherRollEnd":
                           action = GameAction.MorningBriefingWeatherRollEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "SnowRollEnd":
                           action = GameAction.MorningBriefingSnowRollEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "GotoMorningAmmoLimitsSetEnd":
                           action = GameAction.MorningBriefingAmmoLoad;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "GotoMorningBriefingEnd":
                           action = GameAction.MorningBriefingEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "PreparationsDeployment":
                           action = GameAction.PreparationsDeployment;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "PreparationsDeploymentEnd":
                           action = GameAction.PreparationsHatches;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "c15OpenHatch":
                           action = GameAction.PreparationsGunLoad;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "c17GunLoad":
                           action = GameAction.PreparationsTurret;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "c16Turret":
                           action = GameAction.PreparationsLoaderSpot;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "PreparationsCommanderSpot":
                           action = GameAction.PreparationsCommanderSpot;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "PreparationsFinal":
                           action = GameAction.PreparationsFinal;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "Continue017":
                           action = GameAction.MovementStartAreaSet;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MovementExitAreaSet":
                           action = GameAction.MovementExitAreaSet;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MovementEnemyStrengthChoice":
                           action = GameAction.MovementChooseOption;
                           bool isStrengthCheckNeeded = false;
                           if( false == IsEnemyStrengthCheckNeeded(myGameInstance, out isStrengthCheckNeeded))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): IsEnemyStrengthCheckNeeded() returned false");
                              return;
                           }
                           if( true == isStrengthCheckNeeded )
                              action = GameAction.MovementEnemyStrengthChoice;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MovementChooseOption":
                           action = GameAction.MovementChooseOption;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "c44AdvanceFire":
                           action = GameAction.MovementAdvanceFireAmmoUseCheck;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "c44AdvanceFireDeny":
                           action = GameAction.MovementAdvanceFireSkip;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MovementAdvanceFire":
                           action = GameAction.MovementAdvanceFire;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MovementBattleCheck":
                           action = GameAction.MovementBattleCheck;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MovementStartAreaRestart":
                           action = GameAction.MovementStartAreaRestart;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "BattleStart":
                           if( true == myGameInstance.IsAdvancingFireChosen )
                              action = GameAction.BattleStart;
                           else
                              action = GameAction.BattleActivation;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Ambush":
                           action = GameAction.BattleAmbush;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue35":
                           action = GameAction.BattleRoundSequenceStart;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Sherman4": // Battle Board is Empty
                           action = GameAction.BattleEmptyResolve;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Debrief":
                           action = GameAction.EveningDebriefingStart;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        default:
                           break;// do nothing
                     }
                  }
               }
            }
         }
         //---------------------------------------------
         // Click anywhere to continue
         switch (myGameInstance.EventActive)
         {

            default:
               break;
         }
      }
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): myGameInstance=null");
            return;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): myGameEngine=null");
            return;
         }
         GameAction action = GameAction.Error;
         Button b = (Button)sender;
         e.Handled = true;
         string key = (string)b.Content;
         if (true == key.StartsWith("r")) // rules based click
         {
            if (false == ShowRule(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowRule() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("e")) // event based click
         {
            myGameInstance.EventDisplayed = key;
            action = GameAction.UpdateEventViewerDisplay;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
         else
         {
            if (false == Button_ClickShowOther(key, b.Name, out action))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): CloseEvent() return false for key=" + key);
               return;
            }
         }
      }
      private bool Button_ClickShowOther(string content, string name, out GameAction action)
      {
         action = GameAction.Error;
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): myGameEngine=null");
            return false;
         }
         switch (content)
         {
            case "  -  ":
               action = GameAction.PreparationsTurretRotateLeft;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "  +  ":
               action = GameAction.PreparationsTurretRotateRight;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "AAR":
               if (null == myAfterActionDialog)
               {
                  IAfterActionReport? aar = myGameInstance.Reports.GetLast();
                  if (null == aar)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView():  gi.Reports.GetLast()=null");
                     return false;
                  }
                  AfterActionReportUserControl aarUserControl = new AfterActionReportUserControl(aar);
                  if (true == aarUserControl.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): AfterActionReportUserControl CtorError=true");
                     return false;
                  }
                  myAfterActionDialog = new AfterActionDialog(aar, CloseAfterActionDialog);
                  myAfterActionDialog.Show();
               }
               break;
            case "Begin Game":
               action = GameAction.SetupShowMapHistorical;
               //action = GameAction.TestingStartMorningBriefing; // <cgs> TEST
               //action = GameAction.TestingStartPreparations; // <cgs> TEST
               //action = GameAction.TestingStartMovement; // <cgs> TEST
               action = GameAction.TestingStartBattle; // <cgs> TEST
               action = GameAction.TestingStartAmbush; // <cgs> TEST
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Cancel":
               action = GameAction.MovementAirStrikeCancel;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Enter":
               action = GameAction.MovementEnterArea;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Read Rules":
               if (false == ShowRule("r1.1"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): ShowRule(r1.1) returned false");
                  return false;
               }
               break;
            case "Resupply":
               action = GameAction.MovementResupplyCheck;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Strength Check":
               action = GameAction.MovementEnemyStrengthChoice;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Strike":
               action = GameAction.MovementAirStrikeChoice;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Support":
               action = GameAction.MovementArtillerySupportChoice;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            default:
               if (false == ShowTable(content))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): ShowTable() returned false for key=" + content);
                  return false;
               }
               break;
         }
         return true;
      }
   }
}
