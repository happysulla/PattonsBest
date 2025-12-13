
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using Point = System.Windows.Point;
using CheckBox = System.Windows.Controls.CheckBox;
using Windows.ApplicationModel.Background;

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
      public RuleDialogViewer? myRulesMgr = null;
      private AfterActionDialog? myAfterActionDialog = null;
      private ShowReportErrorDialog? myReportErrorDialog = null;
      private ShowAboutDialog? myDialogAbout = null;
      private RuleListingDialog? myDialogRuleListing = null;
      private RuleListingDialog? myDialogEventListing = null;
      private TableListingDialog? myDialogTableListing = null;
      private ShowMovementDiagramDialog? myDialogMovementDiagram = null;
      //--------------------------------------------------------------------
      private ScrollViewer? myScrollViewerTextBlock;
      private Canvas? myCanvasMain = null;
      private TextBlock? myTextBlock = null;
      private int myNumSmokeAttacksThisRound = 0;
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
         if (false == CreateEvents(gi)) // EventViewer()
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): Create_Events() returned false");
            CtorError = true;
            return;
         }
         if (null == myRulesMgr.Events)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer(): myRulesMgr.Events=null");
            CtorError = true;
            return;
         }
      }
      private bool CreateEvents(IGameInstance gi)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Events(): myRulesMgr=null");
            return false;
         }
         try
         {
            string filename = ConfigFileReader.theConfigDirectory + "Events.txt";
            ConfigFileReader cfr = new ConfigFileReader(filename);
            if (true == cfr.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Events(): cfr.CtorError=true");
               return false;
            }
            myRulesMgr.Events = cfr.Entries;
            if (0 == myRulesMgr.Events.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_Events(): myRulesMgr.Events.Count=0");
               return false;
            }
            foreach (string key in myRulesMgr.Events.Keys) // For each event, create a dictionary entry. There can be no more than three die rolls per event
               gi.DieResults[key] = new int[3] { Utilities.NO_RESULT, Utilities.NO_RESULT, Utilities.NO_RESULT };
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_Events(): e=" + e.ToString());
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
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myGameEngine=null");
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myDieRoller=null");
            return;
         }
         gi.IsGridActive = true;
         switch (action)
         {
            case GameAction.UnitTestCommand:
            case GameAction.UnitTestNext:
            case GameAction.UpdateBattleBoard:
            case GameAction.UpdateTankExplosion:
            case GameAction.UpdateTankBrewUp:
            case GameAction.ShowTankForcePath:
            case GameAction.ShowRoads:
               break;
            case GameAction.UpdateGameOptions:
               if (false == OpenEvent(gi, gi.EventActive))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
            case GameAction.MorningBriefingCalendarRoll:
            case GameAction.MorningBriefingDayOfRest:
            case GameAction.EventDebriefDecorationHeart:
            case GameAction.EventDebriefDecorationContinue:
            case GameAction.EventDebriefDecorationBronzeStar:
            case GameAction.EventDebriefDecorationSilverStar:
            case GameAction.EventDebriefDecorationCross:
            case GameAction.EventDebriefDecorationHonor:
               if ( null != myAfterActionDialog )
               {
                  myAfterActionDialog.Close();
                  AfterActionReportUserControl aarUserControl = new AfterActionReportUserControl(gi);
                  if (true == aarUserControl.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): AfterActionReportUserControl CtorError=true");
                     return;
                  }
                  myAfterActionDialog = new AfterActionDialog(gi, CloseAfterActionDialog);
                  myAfterActionDialog.Show();
               }
               gi.IsGridActive = false;
               if (false == OpenEvent(gi, gi.EventActive))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
            case GameAction.MorningBriefingReturningCrewman:
            case GameAction.UpdateAfterActionReport:
               if (null != myAfterActionDialog)
                  myAfterActionDialog.UpdateReport(gi);
               return;
            case GameAction.UpdateUndo:
               myScrollViewerTextBlock.Cursor = System.Windows.Input.Cursors.Arrow;
               gi.IsGridActive = false;
               if (false == OpenEvent(gi, gi.EventActive))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
            case GameAction.UpdateNewGame:
               myGameInstance = gi;
               myRulesMgr.GameInstance = gi;
               gi.IsGridActive = false;
               myScrollViewerTextBlock.Cursor = Cursors.Arrow;
               foreach (string key in myRulesMgr.Events.Keys) // For each event, create a dictionary entry. There can be no more than three die rolls per event
                  gi.DieResults[key] = new int[3] { Utilities.NO_RESULT, Utilities.NO_RESULT, Utilities.NO_RESULT };
               if (false == OpenEvent(gi, gi.EventActive))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
                  return;
               }
               break;
            case GameAction.UpdateLoadingGame:
               myGameInstance = gi;
               myRulesMgr.GameInstance = gi;
               gi.IsGridActive = false;
               myScrollViewerTextBlock.Cursor = Cursors.Arrow;
               if (false == OpenEvent(gi, gi.EventActive))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
                  return;
               }
               break;
            case GameAction.ShowAfterActionReportDialog:
               if (null == myAfterActionDialog)
               {
                  AfterActionReportUserControl aarUserControl = new AfterActionReportUserControl(gi);
                  if (true == aarUserControl.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): AfterActionReportUserControl CtorError=true");
                     return;
                  }
                  myAfterActionDialog = new AfterActionDialog(gi, CloseAfterActionDialog);
                  myAfterActionDialog.Show();
               }
               else
               {
                  myAfterActionDialog.WindowState = WindowState.Normal;
                  myAfterActionDialog.Activate();
               }
               break;
            case GameAction.ShowRuleListingDialog:
               if (null == myDialogRuleListing)
               {
                  myDialogRuleListing = new RuleListingDialog(myRulesMgr, false, CloseRuleListingDialog);
                  if (true == myDialogRuleListing.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): RuleListingDialog CtorError=true");
                     return;
                  }
                  myDialogRuleListing.Show();
               }
               else
               {
                  myDialogRuleListing.WindowState = WindowState.Normal;
                  myDialogRuleListing.Activate();
               }
               break;
            case GameAction.ShowEventListingDialog:
               if (null == myDialogEventListing)
               {
                  myDialogEventListing = new RuleListingDialog(myRulesMgr, false, CloseEventListingDialog);
                  if (true == myDialogEventListing.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myDialogEventListing CtorError=true");
                     return;
                  }
                  myDialogEventListing.Show();
               }
               else
               {
                  myDialogEventListing.WindowState = WindowState.Normal;
                  myDialogEventListing.Activate();
               }
               break;
            case GameAction.ShowTableListing:
               if (null == myDialogTableListing)
               {
                  myDialogTableListing = new TableListingDialog(myRulesMgr, CloseTableListingDialog);
                  if (true == myDialogTableListing.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myDialogTableListing CtorError=true");
                     return;
                  }
                  myDialogTableListing.Show();
               }
               else
               {
                  myDialogTableListing.WindowState = WindowState.Normal;
                  myDialogTableListing.Activate();
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
               if (null == myReportErrorDialog)
               {
                  myReportErrorDialog = new ShowReportErrorDialog(CloseReportErrorDialog);
                  myReportErrorDialog.Show();
               }
               else
               {
                  myReportErrorDialog.WindowState = WindowState.Normal;
                  myReportErrorDialog.Activate();
               }
               break;
            case GameAction.ShowAboutDialog:
               if (null == myDialogAbout)
               {
                  myDialogAbout = new ShowAboutDialog(CloseShowAboutDialog);
                  if (true == myDialogAbout.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): myDialogAbout CtorError=true");
                     return;
                  }
                  myDialogAbout.Show();
               }
               else
               {
                  myDialogAbout.WindowState = WindowState.Normal;
                  myDialogAbout.Activate();
               }
               break;
            case GameAction.ShowMovementDiagramDialog:
               if (null == myDialogMovementDiagram)
               {
                  myDialogMovementDiagram = new ShowMovementDiagramDialog(CloseMovementDialog);
                  myDialogMovementDiagram.Show();
               }
               else
               {
                  myDialogMovementDiagram.WindowState = WindowState.Normal;
                  myDialogMovementDiagram.Activate();
               }
               break;
            case GameAction.ShowGameFeatsDialog:
               ShowFeatDisplayDialog dialogShowFeats = new ShowFeatDisplayDialog(myRulesMgr);
               if (true == dialogShowFeats.CtorError)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): FeatDisplayDialog CtorError=true");
                  return;
               }
               if (true == dialogShowFeats.ShowDialog())
               {
               }
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
            case GameAction.MovementBattleActivation:
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
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): BattleResolveArtilleryFire.CtorError=true");
               else if (false == battleResolveArtFire.ResolveArtilleryFire(ShowBattleSetupFireResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ResolveArtilleryFire() returned false");
               break;
            case GameAction.BattleRoundSequenceFriendlyAction:
               EventViewerResolveArtilleryFire battleResolveFriendlyAction = new EventViewerResolveArtilleryFire(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == battleResolveFriendlyAction.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): battleResolveFriendlyAction.CtorError=true");
               else if (false == battleResolveFriendlyAction.ResolveArtilleryFire(ShowFriendlyActionResults))
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
               else if (false == battleAmbush.PerformEnemyAction(ShowAmbushResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): PerformEnemyAction() returned false");
               break;
            case GameAction.BattleRoundSequenceEnemyAction:
               EventViewerEnemyAction battleEnemyAction = new EventViewerEnemyAction(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == battleEnemyAction.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): battleEnemyAction.CtorError=true");
               else if (false == battleEnemyAction.PerformEnemyAction(ShowEnemyActionResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): PerformEnemyAction() returned false");
               break;
            case GameAction.BattleRoundSequenceSpotting:
               EventViewerSpottingMgr spottingMgr = new EventViewerSpottingMgr(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == spottingMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): spottingMgr.CtorError=true");
               else if (false == spottingMgr.PerformSpotting(ShowSpottingResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): PerformSpotting() returned false");
               break;
            case GameAction.BattleCollateralDamageCheck:
            case GameAction.BattleRoundSequenceCollateralDamageCheck:
            case GameAction.BattleRoundSequenceHarrassingFire:
               EventViewerTankCollateral collateralCheck = new EventViewerTankCollateral(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == collateralCheck.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): collateralCheck.CtorError=true");
               else if (false == collateralCheck.ResolveCollateralDamage(ShowCollateralDamageResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ResolveCollateralDamage() returned false");
               break;
            case GameAction.BattleRoundSequenceChangeFacing:
               EventViewerChangeFacing facingChangeMgr = new EventViewerChangeFacing(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == facingChangeMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): facingChangeMgr.CtorError=true");
               else if (false == facingChangeMgr.PerformFacingChange(ShowFacingChangeResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): PerformFacingChange() returned false");
               break;
            case GameAction.EveningDebriefingRatingImprovement:
            case GameAction.MorningBriefingTrainCrew:
               EventViewerRatingImprove crewRatingImprove = new EventViewerRatingImprove(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == crewRatingImprove.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): crewRatingImprove.CtorError=true");
               else if (false == crewRatingImprove.ImproveRatings(ShowRatingImproveResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ImproveRatings() returned false");
               break;
            case GameAction.BattleShermanKilled:
            case GameAction.BattleRoundSequenceShermanKilled:
               EventViewerTankDestroyed tankDestroyed = new EventViewerTankDestroyed(myGameEngine, myGameInstance, myCanvasMain, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == tankDestroyed.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): tankDestroyed.CtorError=true");
               else if (false == tankDestroyed.ResolveTankDestroyed(ShowTankDestroyedResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ResolveTankDestroyed() returned false");
               break;
            case GameAction.MorningBriefingBegin:
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
            myAfterActionDialog.UpdateReport(gi);
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
                  {
                     b.Click -= Button_Click;
                  }
                  if (ui.Child is CheckBox cb)
                  {
                     cb.Checked -= CheckBox_Checked;
                     cb.Unchecked -= CheckBox_Unchecked;
                  }
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
         catch (System.Windows.Markup.XamlParseException e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): for key=" + key + " e=" + e.ToString());
            return false;
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
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): gi.DieResults[key] exception for key=" + key + " e=" + e.ToString());
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
                  else if (ui.Child is CheckBox cb)
                  {
                     if (false == SetCheckboxState(gi, key, cb))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): SetCheckboxState() returned false");
                        return false;
                     }
                  }
                  else if (ui.Child is Image img)
                  {
                     string imageName = img.Name;
                     if (true == img.Name.Contains("Continue"))
                        imageName = "Continue";
                     else if (true == img.Name.Contains("Ambulance"))
                        imageName = "Ambulance";
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
         Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Name = t.Name };
         myCanvasMain.Children.Add(aPolygon);
         return true;
      }
      //--------------------------------------------------------------------
      private bool UpdateEventContent(IGameInstance gi, string key)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): myGameEngine=null");
            return false;
         }
         if ( null == myTextBlock)
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
            case "e005a":

               Button be005a1 = new Button() { Name= "ButtonDecreaseDate", Content = "   -   ", FontFamily = myFontFam1, FontSize = 12 };
               be005a1.Click += Button_Click;
               Button be005a2 = new Button() { Name = "ButtonIncreaseDate", Content = "   +   ", FontFamily = myFontFam1, FontSize = 12 };
               be005a2.Click += Button_Click;
               Button be005a3 = new Button() { Name = "ButtonDecreaseTankNum", Content = "   -   ", FontFamily = myFontFam1, FontSize = 12 };
               be005a3.Click += Button_Click;
               Button be005a4 = new Button() { Name = "ButtonIncreaseTankNum", Content = "   +   ", FontFamily = myFontFam1, FontSize = 12 };
               be005a4.Click += Button_Click;
               //------------------------------
               string date = TableMgr.GetMonth(myGameInstance.Day); // no new tank models allowed in Jul/Aug 1944
               if (("Jul" == date) || ("Aug" == date))
               {
                  be005a3.IsEnabled = false;
                  be005a4.IsEnabled = false;
               }
               Image imge005a = new Image { Source = MapItem.theMapImages.GetBitmapImage("Continue"), Width = 100, Height = 100, Name = "Continue005a" };
               myTextBlock.Inlines.Add(new Run(" Date: "));
               myTextBlock.Inlines.Add(be005a1);
               myTextBlock.Inlines.Add(new Run("  "));
               myTextBlock.Inlines.Add(be005a2);
               myTextBlock.Inlines.Add(new Run("    "));
               myTextBlock.Inlines.Add(new Run(TableMgr.GetDate(myGameInstance.Day)));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Tank: "));
               myTextBlock.Inlines.Add(be005a3);
               myTextBlock.Inlines.Add(new Run("  "));
               myTextBlock.Inlines.Add(be005a4);
               myTextBlock.Inlines.Add(new Run("    Num = "));
               myTextBlock.Inlines.Add(new Run(report.TankCardNum.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                            "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imge005a));
               break;
            case "e006":
               ReplaceText("DATE", report.Day);
               switch (report.Scenario)
               {
                  case EnumScenario.Advance:
                     ReplaceText("SCENARIO", "Advance");
                     break;
                  case EnumScenario.Battle:
                     ReplaceText("SCENARIO", "Battle");
                     break;
                  case EnumScenario.Counterattack:
                     ReplaceText("SCENARIO", "Counterattack");
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): reached default key=" + key + " for scenario=" + report.Scenario.ToString());
                     return false;
               }
               switch (report.Resistance)
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
                  Image imgSkip = new Image { Source = MapItem.theMapImages.GetBitmapImage("Sherman4"), Width = 300, Height = 190, Name = "GotoMorningBriefingDayOfRest" };
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
            case "e006a":
               Image imge006a = new Image { Name = "MorningBriefingTankReplaceChoice", Width = 50, Height = 50, Source = MapItem.theMapImages.GetBitmapImage("t01Deny") };
               myTextBlock.Inlines.Add(new InlineUIContainer(imge006a));
               myTextBlock.Inlines.Add("  Replace existing tank ");
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               if( false == gi.Sherman.IsKilled )
               {
                  Image imge006b = new Image { Name = "MorningBriefingTankKeepChoice", Width = 50, Height = 50, Source = MapItem.theMapImages.GetBitmapImage("t01") };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge006b));
                  myTextBlock.Inlines.Add("  Keep existing tank");
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click one of images to continue."));
               }
               else
               {
                  myTextBlock.Inlines.Add(new Run("Since your tank is destroyed, you must click replace tank image to continue."));
               }
               break;
            case "e007a":
               if ( 0 == gi.InjuredCrewMembers.Count )
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): gi.InjuredCrewMembers.Count=0 for key=" + key);
                  return false;
               }
               StringBuilder sbe007a = new StringBuilder();
               foreach(IMapItem mi in gi.InjuredCrewMembers)
               {
                  ICrewMember? cm = mi as ICrewMember;
                  if( null == cm )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): invalid mi=" + mi.Name + " for key=" + key);
                     return false;
                  }
                  if ((TableMgr.MIA == cm.WoundDaysUntilReturn) || (TableMgr.KIA == cm.WoundDaysUntilReturn))
                     continue;
                  sbe007a.Append(" -- ");
                  sbe007a.Append(cm.Rank);
                  sbe007a.Append(". ");
                  sbe007a.Append(cm.Name);
                  sbe007a.Append(" as ");
                  sbe007a.Append(cm.Role);
                  sbe007a.Append(" returns ");
                  if( cm.WoundDaysUntilReturn < 1 )
                  {
                     sbe007a.Append("now\n");
                  }
                  else
                  {
                     sbe007a.Append("in ");
                     sbe007a.Append(cm.WoundDaysUntilReturn);
                     sbe007a.Append(" days\n");
                  }
                }
               myTextBlock.Inlines.Add(new Run(sbe007a.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               Image imge007a = new Image { Source = MapItem.theMapImages.GetBitmapImage("Hospital"), Width = 300, Height = 200, Name = "HealCrewman" };
               myTextBlock.Inlines.Add(new Run("                          "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imge007a));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e007c":
               if (null == gi.ReturningCrewman)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): ReturningCrewman=null for key=" + key);
                  return false;
               }
               ICrewMember? existing = gi.GetCrewMemberByRole(gi.ReturningCrewman.Role);
               if (null == existing)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): existing=null for role=" + gi.ReturningCrewman.Role);
                  return false;
               }
               string imageName = MapImage.GetImageByRole(gi.ReturningCrewman.Role);
               Image imge106a = new Image { Name = "ExistingCrewman", Width = 50, Height = 50, Source = MapItem.theMapImages.GetBitmapImage(imageName) };
               myTextBlock.Inlines.Add(new InlineUIContainer(imge106a));
               myTextBlock.Inlines.Add("  Keep existing: ");
               myTextBlock.Inlines.Add(existing.Rank);
               myTextBlock.Inlines.Add(". ");
               myTextBlock.Inlines.Add(existing.Name);
               myTextBlock.Inlines.Add(" rating=");
               myTextBlock.Inlines.Add(existing.Rating.ToString());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               Image imge106b = new Image { Name = "ReturningCrewman", Width = 50, Height = 50, Source = MapItem.theMapImages.GetBitmapImage(imageName) };
               myTextBlock.Inlines.Add(new InlineUIContainer(imge106b));
               myTextBlock.Inlines.Add("  Keep returning: ");
               myTextBlock.Inlines.Add(gi.ReturningCrewman.Rank);
               myTextBlock.Inlines.Add(". ");
               myTextBlock.Inlines.Add(gi.ReturningCrewman.Name);
               myTextBlock.Inlines.Add(" rating=");
               myTextBlock.Inlines.Add(gi.ReturningCrewman.Rating.ToString());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click one of images to continue."));
               break;
            case "e007d":
               if( 0 < gi.DieResults[key][0])
               {
                  string imageTankName = "t";
                  if (report.TankCardNum < 10)
                     imageTankName += "0";
                  imageTankName += report.TankCardNum.ToString();
                  Image imgTank = new Image { Source = MapItem.theMapImages.GetBitmapImage(imageTankName), Width = 120, Height = 120, Name = "TankReplacement" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                          "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgTank));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e007e":
               if (0 < gi.DieResults[key][0])
               {
                  Image? imge007e = null;
                  if ((gi.DieResults[key][0] < 4) || ((11 < report.TankCardNum) && (gi.DieResults[key][0] < 6)))
                     imge007e = new Image { Source = MapItem.theMapImages.GetBitmapImage("c75Hvss"), Width = 100, Height = 100, Name = "MorningBriefingHvssSet" };
                  else
                     imge007e = new Image { Source = MapItem.theMapImages.GetBitmapImage("c75HvssDeny"), Width = 100, Height = 100, Name = "MorningBriefingHvssSet" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                             "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge007e));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e007f":
               string tankImageName = "t";
               if (9 < report.TankCardNum)
                  tankImageName += report.TankCardNum.ToString();
               else
                  tankImageName += ("0" + report.TankCardNum.ToString());
               Image imge007f = new Image { Width = 100, Height = 100, Name = "MorningBriefingTankReplacementEnd", Source = MapItem.theMapImages.GetBitmapImage(tankImageName) };          
               myTextBlock.Inlines.Add(new Run("                                               "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imge007f));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e008":
               if (false == UpdateEventContentWeather(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): UpdateEventContentWeather() returned false for key=" + key);
                  return false;
               }
               break;
            case "e008a":
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
                     case "Falling":
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
               else
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("-- HVAP:") { FontWeight = FontWeights.Bold });
                  myTextBlock.Inlines.Add(new Run(" 1D: (1->3)=1 (4->7)=2 (8->10)=3"));
               }
               //-----------------------------------------------
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                             "));
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
                  Image imgClock = new Image { Source = MapItem.theMapImages.GetBitmapImage("MilitaryWatch"), Width = 200, Height = 100, Name = "MorningBriefingDeployment" };
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
                  sbE011.Append(gi.Sherman.IsHullDown.ToString());
                  sbE011.Append("\n Is Moving  = ");
                  sbE011.Append(gi.Sherman.IsMoving.ToString());
                  sbE011.Append("\n Is Lead Tank  = ");
                  sbE011.Append(gi.IsLeadTank.ToString());
                  myTextBlock.Inlines.Add(new Run(sbE011.ToString()));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge011 = new Image { Name = "MorningBriefingDeploymentEnd" };
                  if (true == gi.Sherman.IsHullDown)
                  {
                     imge011.Source = MapItem.theMapImages.GetBitmapImage("c14HullDown");
                     imge011.Width = 300;
                     imge011.Height = 150;
                     myTextBlock.Inlines.Add(new Run("                            "));
                  }
                  else if (true == gi.Sherman.IsMoving)
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
            case "e011a":
               StringBuilder sbE011a = new StringBuilder();
               sbE011a.Append(" Is Hulled Down = True");
               sbE011a.Append("\n Is Moving  = False");
               sbE011a.Append("\n Is Lead Tank  = False");
               myTextBlock.Inlines.Add(new Run(sbE011a.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               Image imge011a = new Image { Name = "MorningBriefingDeploymentEnd" };
               imge011a.Source = MapItem.theMapImages.GetBitmapImage("c14HullDown");
               imge011a.Width = 300;
               imge011a.Height = 150;
               myTextBlock.Inlines.Add(new Run("                            "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imge011a));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e014":
               myTextBlock.Inlines.Add(new Run("                                               "));
               Image? imgee014 = null;
               double rotation = gi.Sherman.RotationHull + gi.Sherman.RotationTurret;
               if (359.0 < rotation)
                  rotation -= 360.0;
               switch (rotation)
               {
                  case 0.0:
                     imgee014 = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75") };
                     break;
                  case 60.0:
                     imgee014 = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75t060") };
                     break;
                  case 120.0:
                     imgee014 = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75t120") };
                     break;
                  case 180.0:
                     imgee014 = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75t180") };
                     break;
                  case 240.0:
                     imgee014 = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75t240") };
                     break;
                  case 300.0:
                     imgee014 = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75t300") };
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): reached default tr=" + gi.Sherman.RotationTurret.ToString());
                     return false;
               }
               myTextBlock.Inlines.Add(new InlineUIContainer(imgee014));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("When you are satisfied with the current orientation, click image between buttons to continue."));
               break;
            case "e015":
               IMapItem? loaderSpot = gi.BattleStacks.FindMapItem("LoaderSpot");
               if (null != loaderSpot)
               {
                  Image imge015 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c18LoaderSpot"), Width = 100, Height = 100, Name = "PreparationsCommanderSpot" };
                  myTextBlock.Inlines.Add(new Run("                                              "));
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
                  Image imge019 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c34ExitArea"), Width = 100, Height = 100};
                  if (EnumScenario.Counterattack == report.Scenario)
                     imge019.Name = "MovementEnemyCheckCounterattack"; // UpdateEventContent(): e019 
                  else
                     imge019.Name = "MovementEnemyStrengthChoice"; // UpdateEventContent(): e019 
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
                  if (null == gi.EnemyStrengthCheckTerritory)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): gi.EnemyStrenthCheck=null");
                     return false;
                  }
                  Image imge021 = new Image { Width = 100, Height = 100, Name = "MovementChooseOption" };
                  IStack? stack = gi.MoveStacks.Find(gi.EnemyStrengthCheckTerritory);
                  if (null == stack)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): stack=null for e021");
                     return false;
                  }
                  if (0 < stack.MapItems.Count)
                  {
                     if (EnumResistance.Light == gi.BattleResistance)
                        imge021.Source = MapItem.theMapImages.GetBitmapImage("c36Light");
                     else if (EnumResistance.Medium == gi.BattleResistance)
                        imge021.Source = MapItem.theMapImages.GetBitmapImage("c37Medium");
                     else if (EnumResistance.Heavy == gi.BattleResistance)
                        imge021.Source = MapItem.theMapImages.GetBitmapImage("c38Heavy");
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): reached default gi.Resistance=" + gi.BattleResistance.ToString());
                        return false;
                     }
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
                  Image imge024 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c39ArtillerySupport"), Width = 100, Height = 100, Name = "MovementChooseOption" };
                  myTextBlock.Inlines.Add(new Run("                                           "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge024));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  if (7 < gi.DieResults[key][0])
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
                  Image imge027 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c29AmmoReload"), Width = 100, Height = 100, Name = "Resupply" };
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
                  Image imge030 = new Image { Width = 100, Height = 100, Name = "MovementBattleCheck" };
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
            case "e032": // This event is only shown if battle check resulted in possible combat for the day
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
            case "e032a":
               IMapItem? taskForce = gi.MoveStacks.FindMapItem("TaskForce");
               if (null == taskForce)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ResolveBattle_CheckCounterattackRoll(): taskForce=null");
                  return false;
               }
               //-------------------------------------------------
               ReplaceText("TIME_OF_DAY", TableMgr.GetTime(report));
               ReplaceText("RESISTANCE_OF_DAY", report.Resistance.ToString());
               ReplaceText("AREA_TYPE", taskForce.TerritoryCurrent.Type);
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  int dieRoll = gi.DieResults[key][0];
                  if ("A" == taskForce.TerritoryCurrent.Type)
                     dieRoll += 1;
                  if ("C" == taskForce.TerritoryCurrent.Type)
                     dieRoll += 2;
                  //-------------------------------------------------
                  bool isCombat = false;
                  switch (report.Resistance)
                  {
                     case EnumResistance.Light:
                        if (7 < dieRoll) // battle
                           isCombat = true;
                        break;
                     case EnumResistance.Medium:
                        if (5 < dieRoll)  // battle
                           isCombat = true;
                        break;
                     case EnumResistance.Heavy:
                        if (3 < dieRoll)  // battle
                           isCombat = true;
                        break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "ResolveBattle_CheckCounterattackRoll(): reached default with resistance=" + report.Resistance.ToString());
                        return false;
                  }
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge032a = new Image { Width = 150, Height = 150 };
                  if ( true == isCombat )
                  {
                     imge032a.Name = "MovementBattleActivation";
                     myTextBlock.Inlines.Add(new Run("Combat! Enter Battle Board."));
                     imge032a.Source = MapItem.theMapImages.GetBitmapImage("Combat");
                  }
                  else
                  {
                     if (true == gi.IsDaylightLeft(report))
                     {
                        imge032a.Name = "Continue32a";
                        myTextBlock.Inlines.Add(new Run("No combat!"));
                        imge032a.Source = MapItem.theMapImages.GetBitmapImage("Continue");
                     }
                     else
                     {
                        imge032a.Name = "DebriefStart";
                        myTextBlock.Inlines.Add(new Run("Since there is no daylight left, go to Evening Debriefing "));
                        imge032a.Source = MapItem.theMapImages.GetBitmapImage("c28UsControl");
                     }
                  }
                  myTextBlock.Inlines.Add(new Run(" Click image to continue."));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                         "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge032a));
               }
               break;
            case "e033":
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               Image imge033 = new Image { Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("c28UsControl") };
               Button b2 = new Button() { FontFamily = myFontFam1, FontSize = 12 };
               if (false == gi.IsDaylightLeft(report))
               {
                  imge033.Name = "DebriefStart";
                  b2.Content = "r4.9";
                  b2.Click += Button_Click;
                  myTextBlock.Inlines.Add(new Run("Since there is no daylight left, go to Evening Debriefing "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(b2));
                  myTextBlock.Inlines.Add(new Run("."));
               }
               else
               {
                  bool isExitArea;
                  if (false == gi.IsTaskForceInExitArea(out isExitArea))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): myGameInstance.IsTaskForce_InExitArea() returned false");
                     return false;
                  }
                  if (true == isExitArea)  // This occurs when no fight happens in exit territory
                  {
                     imge033.Name = "MovementStartAreaRestart";
                     b2.Content = "r4.51";
                     b2.Click += Button_Click;
                     myTextBlock.Inlines.Add(new Run("Since in exit area and daylight remains, determine a new start area per "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(b2));
                     myTextBlock.Inlines.Add(new Run("."));
                  }
                  else
                  {
                     imge033.Name = "MovementEnemyStrengthChoice"; // UpdateEventContent(): e033 - No Combat
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
               myTextBlock.Inlines.Add(new InlineUIContainer(imge033));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e035":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image? imge035 = null;
                  if (BattlePhase.Ambush == gi.BattlePhase)
                  {
                     myTextBlock.Inlines.Add(new Run("Ambush! Click image to continue."));
                     imge035 = new Image { Name = "Ambush", Width = 400, Height = 240, Source = MapItem.theMapImages.GetBitmapImage("Ambush") };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                "));
                  }
                  else
                  {
                     myTextBlock.Inlines.Add(new Run("No ambush. Click image to continue."));
                     imge035 = new Image { Name = "Continue35", Width = 200, Height = 210, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                  "));
                  }
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge035));
               }
               else
               {
                  if ((true == report.Weather.Contains("Rain")) || (true == report.Weather.Contains("Fog")) || (true == report.Weather.Contains("Falling")))
                     myTextBlock.Inlines.Add(new Run("Subtracting one for Rain, Fog, or Falling Snow."));
               }
               break;
            case "e036a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgClock = new Image { Source = MapItem.theMapImages.GetBitmapImage("MilitaryWatch"), Width = 200, Height = 100, Name = "MovementCounterattackEllapsedTimeRoll" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                   "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgClock));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e038":
               bool isOrdersGiven = false;
               if (false == IsOrdersGiven(gi, out isOrdersGiven))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): IsOrdersGiven() returned false for key=" + key);
                  return false;
               }
               if (true == isOrdersGiven)
               {
                  Image imge038 = new Image { Name = "Continue38", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new Run("                                          "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge038));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e039a":
            case "e039b":
            case "e039c":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  string result = "=   " + TableMgr.GetRandomEvent(report.Scenario, gi.DieResults[key][0]);
                  myTextBlock.Inlines.Add(new Run(result));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge039 = new Image { Name = "Continue39", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new Run("                                          "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge039));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e042":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new Run("Num of Friendlies Knocked Out:  "));
                  if (gi.DieResults[key][0] < 7)
                     myTextBlock.Inlines.Add(new Run("1 Squad"));
                  else if (gi.DieResults[key][0] < 10)
                     myTextBlock.Inlines.Add(new Run("2 Squads"));
                  else
                     myTextBlock.Inlines.Add(new Run("3 Squads"));
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge042 = new Image { Name = "EnemyArtilleryEnd", Width = 200, Height = 115, Source = MapItem.theMapImages.GetBitmapImage("SquadKia") };
                  myTextBlock.Inlines.Add(new Run("                                  "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge042));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e043":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image? img043 = null;
                  if (gi.DieResults[key][0] < 2)
                  {
                     myTextBlock.Inlines.Add(new Run("One Friendly Tank Knocked Out."));
                     img043 = new Image { Name = "MineFieldAttackEnd", Width = 300, Height = 250, Source = MapItem.theMapImages.GetBitmapImage("ShermanKia") };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                  "));
                  }
                  else if (gi.DieResults[key][0] < 3)
                  {
                     myTextBlock.Inlines.Add(new Run("Your tank disabled."));
                     img043 = new Image { Name = "MineFieldAttackEnd", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("c106ThrownTrack") };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                            "));
                  }
                  else
                  {
                     myTextBlock.Inlines.Add(new Run("No effect."));
                     img043 = new Image { Name = "MineFieldAttackEnd", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                  "));
                  }
                  myTextBlock.Inlines.Add(new InlineUIContainer(img043));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e043b":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image? img043b = null;
                  if (9 == gi.DieResults[key][0])
                  {
                     img043b = new Image { Name = "MineFieldAttackDisableRollEnd", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("OBlood1") };
                     myTextBlock.Inlines.Add(new Run("Possibly Driver Wounds."));
                  }
                  else if (10 == gi.DieResults[key][0])
                  {
                     img043b = new Image { Name = "MineFieldAttackDisableRollEnd", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("OBlood1") };
                     myTextBlock.Inlines.Add(new Run("Possibly Assistant Driver Wounds."));
                  }
                  else
                  {
                     img043b = new Image { Name = "MineFieldAttackDisableRollEnd", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                     myTextBlock.Inlines.Add(new Run("No Effect."));
                  }
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                          "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(img043b));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e043c":
               ICrewMember? driver = gi.GetCrewMemberByRole("Driver");
               if (null == driver)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): driver=null for key=" + key);
                  return false;
               }
               int modifiere043c = TableMgr.GetWoundsModifier(gi, driver, false, false, false);
               if (TableMgr.FN_ERROR == modifiere043c)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): TableMgr.GetWoundsModifier() returned error for driver");
                  return false;
               }
               myTextBlock.Inlines.Add(new Run("Wounds Modifier: "));
               myTextBlock.Inlines.Add(new Run(modifiere043c.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  int combo = gi.DieResults[key][0] + modifiere043c;
                  driver.Zoom = 2.0;
                  string result = TableMgr.SetWounds(gi, driver, gi.DieResults[key][0], modifiere043c);
                  if ("ERROR" == result)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): driver GetWounds() returned error for key=" + key);
                     return false;
                  }
                  Button bDriver = new Button() { Name = "DriverWounded", FontFamily = myFontFam1, FontSize = 12, Height = driver.Zoom * Utilities.theMapItemSize, Width = driver.Zoom * Utilities.theMapItemSize };
                  bDriver.Click += Button_Click;
                  CrewMember.SetButtonContent(bDriver, driver, true, true);
                  myTextBlock.Inlines.Add(new Run("Roll + Modifier = "));
                  myTextBlock.Inlines.Add(new Run(combo.ToString()));
                  myTextBlock.Inlines.Add(new Run(" = "));
                  myTextBlock.Inlines.Add(new Run(result));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(bDriver));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
                  driver.Zoom = Utilities.ZOOM;
               }
               break;
            case "e043d":
               ICrewMember? assistant = gi.GetCrewMemberByRole("Assistant");
               if (null == assistant)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): assistant=null for key=" + key);
                  return false;
               }
               int modifiere043d = TableMgr.GetWoundsModifier(gi, assistant, false, false, false);
               if (TableMgr.FN_ERROR == modifiere043d)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): TableMgr.GetWoundsModifier() returned error for assistant");
                  return false;
               }
               myTextBlock.Inlines.Add(new Run("Wounds Modifier: "));
               myTextBlock.Inlines.Add(new Run(modifiere043d.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  int combo = gi.DieResults[key][0] + modifiere043d;
                  assistant.Zoom = 2.0;
                  int modifier = TableMgr.GetWoundsModifier(gi, assistant, false, false, false);
                  if (TableMgr.FN_ERROR == modifier)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): TableMgr.GetWoundsModifier() returned error for assistant");
                     return false;
                  }
                  string result = TableMgr.SetWounds(gi, assistant, gi.DieResults[key][0], modifier);
                  if ("ERROR" == result)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): driver GetWounds() returned error for key=" + key);
                     return false;
                  }
                  Button bAssistant = new Button() { Name = "AssistantWounded", FontFamily = myFontFam1, FontSize = 12, Height = assistant.Zoom * Utilities.theMapItemSize, Width = assistant.Zoom * Utilities.theMapItemSize };
                  bAssistant.Click += Button_Click;
                  CrewMember.SetButtonContent(bAssistant, assistant, true, true);
                  myTextBlock.Inlines.Add(new Run("Roll + Modifier = "));
                  myTextBlock.Inlines.Add(new Run(combo.ToString()));
                  myTextBlock.Inlines.Add(new Run(" = "));
                  myTextBlock.Inlines.Add(new Run(result));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(bAssistant));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
                  assistant.Zoom = Utilities.ZOOM;
               }
               break;
            case "e044":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  StringBuilder sb = new StringBuilder();
                  sb.Append("Panzerfaust Attack Sector is ");
                  char sector = '1';
                  switch (gi.DieResults[key][0])
                  {
                     case 1: sb.Append("1."); sector = '1'; break;
                     case 2: sb.Append("2."); sector = '2'; break;
                     case 3: sb.Append("3."); sector = '3'; break;
                     case 4: case 5: sb.Append("4-5."); sector = '4'; break;
                     case 6: case 7: case 8: sb.Append("6-8."); sector = '6'; break;
                     case 9: case 10: sb.Append("9-10."); sector = '9'; break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): reached default for gi.DieResults[key][0]=" + gi.DieResults[key][0].ToString());
                        return false;
                  }
                  string tName = "B" + sector + "M";
                  IStack? stack = gi.BattleStacks.Find(tName);
                  if (null != stack)
                  {
                     foreach (IMapItem mi in stack.MapItems)
                     {
                        if (true == mi.Name.Contains("UsControl"))
                        {
                           sb.Append(" Ignore attack since sector under US Control.");
                           break;
                        }
                     }
                  }
                  myTextBlock.Inlines.Add(new Run(sb.ToString()));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge044 = new Image { Name = "PanzerfaultSector", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("c107Panzerfaust") };
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge044));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e044a":
               if (null == gi.Panzerfaust)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): gi.Panzerfaust=null for key=" + key);
                  return false;
               }
               StringBuilder sb44a = new StringBuilder();
               if (91 < gi.Panzerfaust.myDay)
                  sb44a.Append(" -1 for Dec 1944 or later\n");
               if (true == gi.Panzerfaust.myIsShermanMoving)
                  sb44a.Append(" -1 for Sherman moving\n");
               if (true == gi.Panzerfaust.myIsLeadTank)
                  sb44a.Append(" -1 for Lead Tank\n");
               if (true == gi.Panzerfaust.myIsAdvancingFireZone)
                  sb44a.Append(" +3 for Advancing Fire Zone\n");
               if (('1' == gi.Panzerfaust.mySector) || ('2' == gi.Panzerfaust.mySector) || ('3' == gi.Panzerfaust.mySector))
                  sb44a.Append(" -1 for Attack in Sector 1, 2, or 3\n");
               if (0 == sb44a.Length)
                  sb44a.Append(" None\n");
               myTextBlock.Inlines.Add(new Run(sb44a.ToString()));
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge044a = new Image { Name = "PanzerfaultAttack", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("c107Panzerfaust") };
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge044a));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e044b":
               if (null == gi.Panzerfaust)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): gi.Panzerfaust=null for key=" + key);
                  return false;
               }
               StringBuilder sb44b = new StringBuilder();
               if (true == gi.Panzerfaust.myIsShermanMoving)
                  sb44b.Append(" +2 for Sherman moving\n");
               if (true == gi.Panzerfaust.myIsAdvancingFireZone)
                  sb44b.Append(" +3 for Advancing Fire Zone\n");
               myTextBlock.Inlines.Add(new Run(sb44b.ToString()));
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge044b = new Image { Name = "PanzerfaultToHit", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("c107Panzerfaust") };
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge044b));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e044c":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge044c = new Image { Name = "PanzerfaultToKill", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("c107Panzerfaust") };
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge044c));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e045":
               if( true == gi.IsHarrassingFireBonus )
                  myTextBlock.Inlines.Add(new Run("If wounds occurs, use -10 modifier since either no LW/MG at medium/close range --or-- all close regions with advancing fire or US controlled markers."));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               Image imge045 = new Image { Name = "CollateralDamage", Width = 325, Height = 200, Source = MapItem.theMapImages.GetBitmapImage("CollateralDamage") };
               myTextBlock.Inlines.Add(new Run("                        "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imge045));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e046":
               if (null != gi.FriendlyAdvance)
               {
                  Image imge046 = new Image { Name = "Continue046a", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("c28UsControl") };
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge046));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  myTextBlock.Inlines.Add(new Run("Click highlighted region to choose."));
               }
               break;
            case "e048":
               if (null != gi.EnemyAdvance)
               {
                  if (true == gi.IsEnemyAdvanceComplete)
                  {
                     Image imge046 = new Image { Name = "Continue047", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("c36Light") };
                     myTextBlock.Inlines.Add(new Run("                                            "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(imge046));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Click image to continue."));
                  }
               }
               break;
            case "e050":
               if ("None" != gi.GetAmmoReloadType())  // only show continue image after user selected a Ammo Reload 
               {
                  Image imge050 = new Image { Name = "Continue50", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new Run("                                          "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge050));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e050a":
               if ("None" != gi.GetGunLoadType())  // only show continue image after user selected a Ammo Reload 
               {
                  Image imge050 = new Image { Name = "Continue50a", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new Run("                                          "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge050));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e051":
               string modiferString = UpdateEventContentGetMovingModifier(gi);
               myTextBlock.Inlines.Add(new Run(modiferString));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Roll for Effect on Sherman: "));
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
                  bmi.EndInit();
                  Image imgDice = new Image { Name = "DieRollBlue", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(imgDice, bmi);
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
               }
               else
               {
                  int combo = gi.DieResults[key][0] + TableMgr.GetMovingModifier(gi);
                  myTextBlock.Inlines.Add(new Run(combo.ToString()));
                  myTextBlock.Inlines.Add(new Run("  " + gi.MovementEffectOnSherman));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll for Effect on Enemy: " + gi.MovementEffectOnEnemy));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("White Die Roll: " + DieRoller.WhiteDie.ToString()));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  Image imge51 = new Image { Name = "Continue51", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge51));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e051a":
               string modiferStringe051a = UpdateEventContentGetBoggedDownModifier(gi);
               myTextBlock.Inlines.Add(new Run(modiferStringe051a));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Roll for Result: "));
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
                  bmi.EndInit();
                  Image imgDice = new Image { Name = "DieRollBlue", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(imgDice, bmi);
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
               }
               else
               {
                  if( 100 == gi.DieResults[key][0] )
                  {
                     myTextBlock.Inlines.Add(new Run("= Assistance Needed"));
                  }
                  else
                  {
                     int modifier = TableMgr.GetBoggedDownModifier(gi);
                     int comboe51a = gi.DieResults[key][0] + modifier;
                     myTextBlock.Inlines.Add(new Run(comboe51a.ToString()));
                     if (comboe51a < 11)
                     {
                        myTextBlock.Inlines.Add(new Run("= Tank Free"));
                     }
                     else if (comboe51a < 81)
                     {
                        myTextBlock.Inlines.Add(new Run("= No Effect"));
                     }
                     else if (comboe51a < 91)
                     {
                        myTextBlock.Inlines.Add(new Run("= Tank Throws Track"));
                     }
                     else
                     {
                        myTextBlock.Inlines.Add(new Run("= Assistance Needed"));
                     }
                  }
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  Image imge51a = new Image { Name = "Continue51a", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge51a));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e052":
               myTextBlock.Inlines.Add(new Run("                                             "));
               Image? imge52 = null;
               switch (gi.Sherman.RotationHull)
               {
                  case 0.0:
                     imge52 = new Image { Name = "ShermanPivot", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("ShermanPivot") };
                     break;
                  case 60.0:
                     imge52 = new Image { Name = "ShermanPivot", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("ShermanPivoth060") };
                     break;
                  case 120.0:
                     imge52 = new Image { Name = "ShermanPivot", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("ShermanPivoth120") };
                     break;
                  case 180.0:
                     imge52 = new Image { Name = "ShermanPivot", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("ShermanPivoth180") };
                     break;
                  case 240.0:
                     imge52 = new Image { Name = "ShermanPivot", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("ShermanPivoth240") };
                     break;
                  case 300.0:
                     imge52 = new Image { Name = "ShermanPivot", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("ShermanPivoth300") };
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): reached default tr=" + gi.Sherman.RotationTurret.ToString());
                     return false;
               }
               myTextBlock.Inlines.Add(new InlineUIContainer(imge52));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("When you are satisfied with the current orientation, click image between buttons to continue."));
               break;
            case "e052a":
               myTextBlock.Inlines.Add(new Run("                                               "));
               Image? imge52a = null;
               double rotation52a = gi.Sherman.RotationHull + gi.Sherman.RotationTurret;
               if (359.0 < rotation52a)
                  rotation52a -= 360.0;
               switch (rotation52a)
               {
                  case 0.0:
                     imge52a = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75") };
                     break;
                  case 60.0:
                     imge52a = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75t060") };
                     break;
                  case 120.0:
                     imge52a = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75t120") };
                     break;
                  case 180.0:
                     imge52a = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75t180") };
                     break;
                  case 240.0:
                     imge52a = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75t240") };
                     break;
                  case 300.0:
                     imge52a = new Image { Name = "c16TurretSherman75", Width = 120, Height = 120, Source = MapItem.theMapImages.GetBitmapImage("c16TurretSherman75t300") };
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): reached default tr=" + gi.Sherman.RotationTurret.ToString());
                     return false;
               }
               myTextBlock.Inlines.Add(new InlineUIContainer(imge52a));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("When you are satisfied with the current orientation, click image between buttons to continue."));
               break;
            case "e053a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  if ((98 == gi.DieResults[key][0]) || (99 == gi.DieResults[key][0]) || (100 == gi.DieResults[key][0]))
                     myTextBlock.Inlines.Add("=  GUN MALFUNCTIONS!");
                  else
                     myTextBlock.Inlines.Add("=  No Effect");
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  Image imge053a = new Image { Name = "Continue53a", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge053a));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add("Click image to continue.");
               }
               break;
            case "e053b": 
               if( false == UpdateEventContentToGetToHit(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): UpdateEvent_ContentToGetToHit() returned false for key=" + key);
                  return false;
               }
               break;
            case "e053c":
               string modiferRateOfFire = UpdateEventContentRateOfFireModifier(gi);
               myTextBlock.Inlines.Add(new Run(modiferRateOfFire));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Choose either   "));
               Button be53c1 = new Button() { FontFamily = myFontFam1, FontSize = 12, Content="Fire" };
               be53c1.Click += Button_Click;
               myTextBlock.Inlines.Add(new InlineUIContainer(be53c1));
               myTextBlock.Inlines.Add(new Run("   or   "));
               Button be53c2 = new Button() { FontFamily = myFontFam1, FontSize = 12, Content = "Skip" };
               be53c2.Click += Button_Click;
               myTextBlock.Inlines.Add(new InlineUIContainer(be53c2));
               myTextBlock.Inlines.Add(new Run("   to continue."));
               break;
            case "e053d":
               if( false == UpdateEventContentToKillInfantry(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): UpdateEventContentToKillInfantry() returned false for key=" + key);
                  return false;
               }
               break;
            case "e053e":
               if( false == UpdateEventContentToKillVehicle(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): UpdateEventContentToKillVehicle() returned error for key=" + key);
                  return false;
               }
               break;
            case "e054": 
               if ( 0 < gi.Targets.Count )
                  myTextBlock.Inlines.Add(new Run("Select either a blue zone for area fire or a target enclosed by a red box. Only spotted units may be targeted."));
               foreach (IMapItem crewAction in gi.CrewActions)
               {
                  if (("Commander_MGFire" == crewAction.Name) && (false == gi.IsCommanderDirectingMgFire))
                  {
                     CheckBox cbe054 = new CheckBox() { FontSize = 12, IsEnabled = true, IsChecked = false, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center };
                     cbe054.Checked += CheckBoxCmdrFire_Checked;
                     cbe054.Unchecked += CheckBoxCmdrFire_Unchecked;
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak()); 
                     myTextBlock.Inlines.Add(new InlineUIContainer(cbe054));
                     myTextBlock.Inlines.Add(new Run(" Check if Commander directs fire - only allowed once per round"));
                  }
               }
               break;
            case "e054a":
               if (false == UpdateEventContentMgToKill(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): UpdateEventContentMgToKillVehicle() returned error for key=" + key);
                  return false;
               }
               break;
            case "e054b":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  if (gi.DieResults[key][0] < 31) // Assume that sub MG do not use ammo
                     myTextBlock.Inlines.Add("= Use One MG Ammo box.");
                  else if (97 < gi.DieResults[key][0])
                     myTextBlock.Inlines.Add("= MG malfunction!");
                  else 
                     myTextBlock.Inlines.Add("= No Effect.");
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  Image imge54b = new Image { Name = "Continue54b", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge54b));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e055":
               int replacementCount = 0;
               foreach (IMapItem crewAction in gi.CrewActions)
               {
                  if (true == crewAction.Name.Contains("RepairScope") )
                     replacementCount++;
               }
               if (0 == replacementCount)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): 0 = replacementCount=" + replacementCount.ToString() );
                  return false;
               }
               if (report.AmmoPeriscope < replacementCount)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): replacementCount=" + replacementCount.ToString() + " > periscopeCount=" + report.AmmoPeriscope.ToString());
                  return false;
               }
               ReplaceText("PERISCOPE_REPLACEMENT", replacementCount.ToString());
               ReplaceText("PERISCOPE_REPLACEMENT_TOTAL", report.AmmoPeriscope.ToString());
               break;
            case "e056":
               myTextBlock.Inlines.Add(new Run("Modifiers") { TextDecorations = TextDecorations.Underline });
               myTextBlock.Inlines.Add(new LineBreak());
               int modifier56;
               string modiferStringe56 = UpdateEventContentMainGunRepair(gi, out modifier56);
               if ("ERROR" == modiferStringe56)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): UpdateEventContentMainGunRepair() returned ERROR");
                  return false;
               }
               myTextBlock.Inlines.Add(new Run(modiferStringe56));
               myTextBlock.Inlines.Add(new Run("Die Roll: "));
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
                  bmi.EndInit();
                  Image imgDice = new Image { Name = "DieRollBlue", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(imgDice, bmi);
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
               }
               else
               {
                  int combo = gi.DieResults[key][0] + modifier56;
                  myTextBlock.Inlines.Add(new Run(gi.DieResults[key][0].ToString()));
                  myTextBlock.Inlines.Add(new Run( " - " + Math.Abs(modifier56).ToString()));
                  myTextBlock.Inlines.Add(new Run(" = " + combo.ToString()));
                  if (gi.DieResults[key][0] < 21) // Assume that sub MG do not use ammo
                     myTextBlock.Inlines.Add(new Run(" = Gun Repaired."));
                  else if ((90 < combo) || (97 < gi.DieResults[key][0]))
                     myTextBlock.Inlines.Add(new Run(" = GUN BROKEN!"));
                  else
                     myTextBlock.Inlines.Add(new Run(" = No Effect."));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  Image imge54b = new Image { Name = "Continue56", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge54b));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e056a":
               myTextBlock.Inlines.Add(new Run("Modifiers") { TextDecorations = TextDecorations.Underline });
               myTextBlock.Inlines.Add(new LineBreak());
               int modifier56a;
               string modiferStringe56a = UpdateEventContentAaMgRepair(gi, out modifier56a);
               if ("ERROR" == modiferStringe56a)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): UpdateEventContentAaMgRepair() returned ERROR");
                  return false;
               }
               myTextBlock.Inlines.Add(new Run(modiferStringe56a));
               myTextBlock.Inlines.Add(new Run("Die Roll: "));
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
                  bmi.EndInit();
                  Image imgDice = new Image { Name = "DieRollBlue", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(imgDice, bmi);
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
               }
               else
               {
                  int combo = gi.DieResults[key][0] + modifier56a;
                  myTextBlock.Inlines.Add(new Run(gi.DieResults[key][0].ToString()));
                  myTextBlock.Inlines.Add(new Run(" - " + Math.Abs(modifier56a).ToString()));
                  myTextBlock.Inlines.Add(new Run(" = " + combo.ToString()));
                  if (combo < 21) // Assume that sub MG do not use ammo
                     myTextBlock.Inlines.Add(new Run(" = Gun Repaired."));
                  else if ((90 < combo) || (97 < gi.DieResults[key][0]) )
                     myTextBlock.Inlines.Add(new Run(" = GUN BROKEN!"));
                  else
                     myTextBlock.Inlines.Add(new Run(" = No Effect."));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  Image imge54b = new Image { Name = "Continue56a", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge54b));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e056b":
               myTextBlock.Inlines.Add(new Run("Modifiers") { TextDecorations = TextDecorations.Underline });
               myTextBlock.Inlines.Add(new LineBreak());
               StringBuilder sbe056b = new StringBuilder();
               ICrewMember? assistante56b = gi.GetCrewMemberByRole("Assistant");
               if (null == assistante56b)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): assistant=null");
                  return false;
               }
               sbe056b.Append(" -");
               sbe056b.Append(assistante56b.Rating.ToString());
               sbe056b.Append(" for assistant rating\n\n");
               myTextBlock.Inlines.Add(new Run(sbe056b.ToString()));
               myTextBlock.Inlines.Add(new Run("Die Roll: "));
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
                  bmi.EndInit();
                  Image imgDice = new Image { Name = "DieRollBlue", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(imgDice, bmi);
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
               }
               else
               {
                  int combo = gi.DieResults[key][0] - assistante56b.Rating;
                  myTextBlock.Inlines.Add(new Run(gi.DieResults[key][0].ToString()));
                  myTextBlock.Inlines.Add(new Run(" - " + assistante56b.Rating.ToString()));
                  myTextBlock.Inlines.Add(new Run(" = " + combo.ToString()));
                  if (gi.DieResults[key][0] < 21) // Assume that sub MG do not use ammo
                     myTextBlock.Inlines.Add(" = Gun Repaired.");
                  else if ((90 < combo) || (97 < gi.DieResults[key][0]))
                     myTextBlock.Inlines.Add(" = GUN BROKEN!");
                  else
                     myTextBlock.Inlines.Add(" = No Effect.");
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  Image imge56b = new Image { Name = "Continue56b", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge56b));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e056c":
               myTextBlock.Inlines.Add(new Run("Modifiers") { TextDecorations = TextDecorations.Underline });
               myTextBlock.Inlines.Add(new LineBreak());
               StringBuilder sbe056c = new StringBuilder();
               ICrewMember? loader56c = gi.GetCrewMemberByRole("Loader");
               if (null == loader56c)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): assistant=null");
                  return false;
               }
               sbe056c.Append(" -");
               sbe056c.Append(loader56c.Rating.ToString());
               sbe056c.Append(" for loader rating\n\n");
               myTextBlock.Inlines.Add(new Run(sbe056c.ToString()));
               myTextBlock.Inlines.Add(new Run("Die Roll: "));
               if (Utilities.NO_RESULT == gi.DieResults[key][0])
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
                  bmi.EndInit();
                  Image imgDice = new Image { Name = "DieRollBlue", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(imgDice, bmi);
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
               }
               else
               {
                  int combo = gi.DieResults[key][0] - loader56c.Rating;
                  myTextBlock.Inlines.Add(new Run(gi.DieResults[key][0].ToString()));
                  myTextBlock.Inlines.Add(new Run(" - " + loader56c.Rating.ToString()));
                  myTextBlock.Inlines.Add(new Run(" = " + combo.ToString()));
                  if (gi.DieResults[key][0] < 21) // Assume that sub MG do not use ammo
                     myTextBlock.Inlines.Add(" = Gun Repaired.");
                  else if ((90 < combo) || (97 < gi.DieResults[key][0]))
                     myTextBlock.Inlines.Add(" = GUN BROKEN!");
                  else
                     myTextBlock.Inlines.Add(" = No Effect.");
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  Image imge56c = new Image { Name = "Continue56c", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge56c));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e101":
               if (false == UpdateEventContentVictoryPointTotal(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): UpdateEventContentVictoryPointTotal() returned error for key=" + key);
                  return false;
               }
               break;
            case "e102":
               if (false == UpdateEventContentPromotion(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): UpdateEventContentPromotion() returned error for key=" + key);
                  return false;
               }
               break;
            case "e103":
               if( false == UpdateEventContentDecoration(gi))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): UpdateEventContentDecoration() returned error for key=" + key);
                  return false;
               }
               break;
            case "e104":
               ReplaceText("NUMBER_PURPLE_HEARTS", gi.NumPurpleHeart.ToString());
               break;
            case "e501":
               StringBuilder sbEndWon = new StringBuilder();
               sbEndWon.Append("Game ends on ");
               sbEndWon.Append(TableMgr.GetDate(gi.Day));
               sbEndWon.Append(" due to '");
               sbEndWon.Append(gi.EndGameReason);
               sbEndWon.Append("'");
               myTextBlock.Inlines.Add(new Run(sbEndWon.ToString()));
               Image? imgEndGameWon = null;
               switch (Utilities.RandomGenerator.Next(10))
               {
                  case 0:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Win"), Width = 300, Height = 300 };
                     break;
                  case 1:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Muscle"), Width = 300, Height = 300 };
                     break;
                  case 2:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Muscle"), Width = 300, Height = 300 };
                     break;
                  case 3:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Star"), Width = 300, Height = 300 };
                     break;
                  case 4:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Star"), Width = 300, Height = 300 };
                     break;
                  default:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Win"), Width = 300, Height = 300 };
                     break;
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                  "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imgEndGameWon));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue show game statistics and feats."));
               break;
            case "e502":
               StringBuilder sbEndLost = new StringBuilder();
               sbEndLost.Append("Game ends on ");
               sbEndLost.Append(TableMgr.GetDate(gi.Day));
               sbEndLost.Append(" due to ");
               sbEndLost.Append(gi.EndGameReason);
               myTextBlock.Inlines.Add(new Run(sbEndLost.ToString()));
               Image? imgEndGameLost = null;
               switch (Utilities.RandomGenerator.Next(11))
               {
                  case 0:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Deny"), Width = 300, Height = 300 };
                     break;
                  case 1:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Idiot"), Width = 300, Height = 300 };
                     break;
                  case 2:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("OBlood1"), Width = 300, Height = 300 };
                     break;
                  case 3:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("FarmerDead"), Width = 300, Height = 300 };
                     break;
                  case 4:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Skulls"), Width = 300, Height = 300 };
                     break;
                  default:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Frown"), Width = 300, Height = 300 };
                     break;
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                  "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imgEndGameLost));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue show game statistics and feats."));
               break;
            default:
               break;
         }
         return true;
      }
      private bool UpdateEventContentWeather(IGameInstance gi)
      {
         //----------------------------------------
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentWeather(): myTextBlock=null");
            return false;
         }
         string key = gi.EventActive;
         //----------------------------------------
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentWeather():  gi.Reports.GetLast()");
            return false;
         }
         //----------------------------------------
         if (Utilities.NO_RESULT < gi.DieResults[key][0])
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
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentWeather(): reached default snow=" + report.Weather);
                  return false;
            }
            if (null == imgWeather)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentWeather(): img=null for key=" + key);
               return false;
            }
            myTextBlock.Inlines.Add(new Run("Weather calls for " + report.Weather + ":"));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            if ("Clear" == report.Weather)
               myTextBlock.Inlines.Add(new Run("                                        "));
            else
               myTextBlock.Inlines.Add(new Run("                "));
            myTextBlock.Inlines.Add(new InlineUIContainer(imgWeather));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("Click image to continue."));
         }
         return true;
      }
      private string UpdateEventContentGetMovingModifier(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetMovingModifier(): lastReport=null");
            return "ERROR";
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //-------------------------------------------------
         ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
         if (null == commander)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetMovingModifier(): commander=null");
            return "ERROR";
         }
         //-------------------------------------------------
         ICrewMember? driver = gi.GetCrewMemberByRole("Driver");
         if (null == driver)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetMovingModifier(): driver=null");
            return "ERROR";
         }
         //-------------------------------------------------
         StringBuilder sb51 = new StringBuilder();
         sb51.Append("-");
         sb51.Append(driver.Rating.ToString());
         sb51.Append(" for driver rating\n");
         //-------------------------------------------------
         bool isCommanderDirectingMovement = false;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if ("Commander_Move" == crewAction.Name)
               isCommanderDirectingMovement = true;
         }
         if( true == isCommanderDirectingMovement )
         {
            if (false == commander.IsButtonedUp)
            {
               sb51.Append("-");
               sb51.Append(commander.Rating.ToString());
               sb51.Append(" for cmdr rating directing move\n");
            }
            else if (true == card.myIsVisionCupola)
            {
               int rating = (int)Math.Floor(commander.Rating / 2.0);
               sb51.Append(rating.ToString());
               sb51.Append(" for cmdr rating directing move using cupola\n");
            }
         }
         if( null != gi.ShermanHvss )
            sb51.Append("-2 for HVSS suspension\n");
         if (true == driver.IsButtonedUp)
            sb51.Append("+5 Driver buttoned up\n");
         //-------------------------------------------------
         if (true == lastReport.Weather.Contains("Ground Snow"))
            sb51.Append("+3 for Ground Snow\n");
         else if (true == lastReport.Weather.Contains("Falling"))
            sb51.Append("+6 for Deep Snow\n");
         else if (true == lastReport.Weather.Contains("Mud"))
            sb51.Append("+9 for Mud\n");
         //-------------------------------------------------
         if (0 == sb51.Length)
            return "None";
         else
            return sb51.ToString();
      }
      private string UpdateEventContentGetBoggedDownModifier(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetMovingModifier(): lastReport=null");
            return "ERROR";
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //-------------------------------------------------
         ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
         if (null == commander)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetMovingModifier(): commander=null");
            return "ERROR";
         }
         //-------------------------------------------------
         ICrewMember? driver = gi.GetCrewMemberByRole("Driver");
         if (null == driver)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetMovingModifier(): driver=null");
            return "ERROR";
         }
         //-------------------------------------------------
         StringBuilder sb51 = new StringBuilder();
         sb51.Append("-");
         sb51.Append(driver.Rating.ToString());
         sb51.Append(" for driver rating\n");
         //-------------------------------------------------
         bool isCommanderDirectingMovement = false;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if ("Commander_Move" == crewAction.Name)
               isCommanderDirectingMovement = true;
         }
         if (true == isCommanderDirectingMovement)
         {
            if (false == commander.IsButtonedUp)
            {
               sb51.Append("-");
               sb51.Append(commander.Rating.ToString());
               sb51.Append(" for cmdr rating directing move\n");
            }
         }
         if (null != gi.ShermanHvss )
            sb51.Append("-5 for HVSS suspension\n");
         if (true == driver.IsButtonedUp)
            sb51.Append("+10 Driver buttoned up\n");
         //-------------------------------------------------
         if (0 == sb51.Length)
            return "None";
         else
            return sb51.ToString();
      }
      private bool UpdateEventContentToGetToHit(IGameInstance gi)
      {
         myNumSmokeAttacksThisRound = 0;
         if ( null == myTextBlock )
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEvent_ContentToGetToHit(): myTextBlock=null");
            return false;
         }
         string key = gi.EventActive;
         if (null == gi.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEvent_ContentToGetToHit(): gi.TargetMainGun=null for key=" + key);
            return false;
         }
         double size = gi.TargetMainGun.Zoom * Utilities.theMapItemSize;
         System.Windows.Controls.Button bEnemy = new Button { Width = size, Height = size, BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Colors.Transparent) };
         MapItem.SetButtonContent(bEnemy, gi.TargetMainGun);
         myTextBlock.Inlines.Add(new Run("                                                   "));
         myTextBlock.Inlines.Add(new InlineUIContainer(bEnemy));
         myTextBlock.Inlines.Add(new LineBreak());
         myTextBlock.Inlines.Add(new LineBreak());
         //----------------------------------------------
         string gunload = gi.GetGunLoadType();
         myTextBlock.Inlines.Add(new Run("       Choose either   "));
         Button be53b1 = new Button() { FontFamily = myFontFam1, FontSize = 12, Content = "Direct" };
         if (("Hbci" == gunload) || ("Wp" == gunload) || ("None" == gunload) || (false == String.IsNullOrEmpty(gi.ShermanTypeOfFire))) // cannot be direct fire
            be53b1.IsEnabled = false;
         be53b1.Click += Button_Click;
         myTextBlock.Inlines.Add(new InlineUIContainer(be53b1));
         myTextBlock.Inlines.Add(new Run("   or   "));
         Button be53b2 = new Button() { FontFamily = myFontFam1, FontSize = 12, Content = " Area " };
         if (("Wp" != gunload) && ("Hbci" != gunload))
         {
            if (("Ap" == gunload) || ("Hvap" == gunload) || ("None" == gunload) || (true == gi.TargetMainGun.IsVehicle) || (false == String.IsNullOrEmpty(gi.ShermanTypeOfFire))) // AP and HVAP cannot be area fire
               be53b2.IsEnabled = false;
         }
         be53b2.Click += Button_Click;
         myTextBlock.Inlines.Add(new InlineUIContainer(be53b2));
         //----------------------------------------------
         if (false == UpdateEventContentGetToHitModifierImmobilization(gi, gi.TargetMainGun))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEvent_ContentToGetToHit(): UpdateEventContentGetToHitModifierImmobilization() returned false");
            return false;
         }
         //----------------------------------------------
         if (false == String.IsNullOrEmpty(gi.ShermanTypeOfFire))  // EventViewer.UpdateEventContentToGetToHit()
         {
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("Modifiers") { TextDecorations = TextDecorations.Underline });
            myTextBlock.Inlines.Add(new LineBreak());
            string modiferMainGunFiring = UpdateEventContentGetToHitModifier(gi, gi.TargetMainGun);
            myTextBlock.Inlines.Add(new Run(modiferMainGunFiring));
             myTextBlock.Inlines.Add(new LineBreak());
            double toHitNum = TableMgr.GetShermanToHitBaseNumber(gi, gi.TargetMainGun);
            if (TableMgr.FN_ERROR == toHitNum)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEvent_ContentToGetToHit(): GetShermanToHitBaseNumber() returned error for key=" + key);
               return false;
            }
            double modifier = TableMgr.GetShermanToHitModifier(gi, gi.TargetMainGun);
            if (TableMgr.FN_ERROR == toHitNum)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEvent_ContentToGetToHit(): GetShermanToHitModifier() returned error for key=" + key);
               return false;
            }
            double combo = toHitNum - modifier;
            StringBuilder sb = new StringBuilder();
            sb.Append("To hit, roll ");
            sb.Append(toHitNum.ToString("F0"));
            if( modifier < 0 )
               sb.Append(" + ");
            else
               sb.Append(" - ");
            sb.Append(Math.Abs(modifier).ToString("F0"));
            sb.Append(" = ");
            sb.Append(combo.ToString("F0"));
            sb.Append(" or less: ");
            myTextBlock.Inlines.Add(new Run(sb.ToString()));
            if (Utilities.NO_RESULT == gi.DieResults[key][0])
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
               bmi.EndInit();
               Image imgDice = new Image { Name = "DieRollBlue", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(imgDice, bmi);
               myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
            }
            else
            {
               myTextBlock.Inlines.Add(new Run(gi.DieResults[key][0].ToString()));
               Image? imge53b = null;
               if ( (98 == gi.DieResults[key][0]) || (99 == gi.DieResults[key][0]) || (100 == gi.DieResults[key][0]) )
               {
                  myTextBlock.Inlines.Add("  =  GUN MALFUNCTIONS!");
                  imge53b = new Image { Name = "Continue53b", Width = 80, Height = 80, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
               }
               else if ((combo < gi.DieResults[key][0]) )
               {
                  myTextBlock.Inlines.Add("  =  MISS");
                  imge53b = new Image { Name = "Continue53b", Width = 80, Height = 80, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
               }
               else
               {
                  if( true == gi.IsShermanDeliberateImmobilization)
                  {
                     myTextBlock.Inlines.Add("  =  IMMOBILIZED");
                     imge53b = new Image { Name = "BattleRoundSequenceShermanHit", Width = 80, Height = 80, Source = MapItem.theMapImages.GetBitmapImage("c106ThrownTrack") };
                  }
                  else
                  {
                     myTextBlock.Inlines.Add("  =  HIT");
                     switch (gi.FiredAmmoType)
                     {
                        case "Ap":
                        case "Hvap":
                           imge53b = new Image { Name = "BattleRoundSequenceShermanHit", Width = 80, Height = 80, Source = MapItem.theMapImages.GetBitmapImage("c100ApHit") };
                           break;
                        case "He":
                           imge53b = new Image { Name = "BattleRoundSequenceShermanHit", Width = 80, Height = 80, Source = MapItem.theMapImages.GetBitmapImage("c101HeHit") };
                           break;
                        case "Wp":
                        case "Hbci":
                           imge53b = new Image { Name = "BattleRoundSequenceShermanHit", Width = 80, Height = 80, Source = MapItem.theMapImages.GetBitmapImage("c102SmokeHit") };
                           break;
                        default:
                           Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToGetToHit(): reached default gunLoad=" + gi.ShermanTypeOfFire + " for key=" + key);
                           return false;
                     }
                  }
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                                 "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imge53b));
            }
         }
         return true;
      }
      private string UpdateEventContentGetToHitModifier(IGameInstance gi, IMapItem enemyUnit)
      {
         //------------------------------------
         if (3 != enemyUnit.TerritoryCurrent.Name.Length)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): 3 != TerritoryCurrent.Name.Length=" + enemyUnit.TerritoryCurrent.Name);
            return "ERROR";
         }
         char range = enemyUnit.TerritoryCurrent.Name[2];
         //------------------------------------
         bool isCommanderDirectingFire = false;
         bool isShermanMoving = false;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if ("Commander_MainGunFire" == crewAction.Name)
               isCommanderDirectingFire = true;
            if ("Driver_Forward" == crewAction.Name)
               isShermanMoving = true;
            if ("Driver_ForwardToHullDown" == crewAction.Name)
               isShermanMoving = true;
            if ("Driver_Reverse" == crewAction.Name)
               isShermanMoving = true;
            if ("Driver_ReverseToHullDown" == crewAction.Name)
               isShermanMoving = true;
            if ("Driver_PivotTank" == crewAction.Name)
               isShermanMoving = true;
         }
         //------------------------------------
         ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
         if (null == commander)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): commander=null");
            return "ERROR";
         }
         ICrewMember? gunner = gi.GetCrewMemberByRole("Gunner");
         if (null == gunner)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): gunner=null");
            return "ERROR";
         }
         //------------------------------------
         StringBuilder sb51 = new StringBuilder();
         //------------------------------------
         int numShots = 0;
         if (true == enemyUnit.EnemyAcquiredShots.ContainsKey("Sherman")) // Fire_AndReloadGun() - Increase when firing at a target
            numShots = enemyUnit.EnemyAcquiredShots["Sherman"];
         if (0 == numShots)
         {
            Logger.Log(LogEnum.LE_SHOW_NUM_ENEMY_SHOTS, "UpdateEventContent_GetToHitModifier(): acq=" + numShots.ToString() + " isCommanderDirectingFire=" + isCommanderDirectingFire.ToString() + " commander.IsButtonedUp=" + commander.IsButtonedUp.ToString() + " for enemyUnit=" + enemyUnit.Name);
            if ( (false == isCommanderDirectingFire) || (true == commander.IsButtonedUp) )
               sb51.Append("+10 for first shot\n");
         }
         else if (1 == numShots)
         {
            Logger.Log(LogEnum.LE_SHOW_NUM_ENEMY_SHOTS, "UpdateEventContent_GetToHitModifier(): SHOW +1 acq=" + numShots.ToString() + " for enemyUnit=" + enemyUnit.Name);
            if ('C' == range)
               sb51.Append("-5 for 2nd shot at close range\n");
            else if ('M' == range)
               sb51.Append("-10 for 2nd shot at medium range\n");
            else if ('L' == range)
               sb51.Append("-15 for 2nd shot at long range\n");
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): reached default range=" + range);
               return "ERROR";
            }
         }
         else if (1 < numShots)
         {
            Logger.Log(LogEnum.LE_SHOW_NUM_ENEMY_SHOTS, "UpdateEventContent_GetToHitModifier(): SHOW +2 acq=" + numShots.ToString() + " for enemyUnit=" + enemyUnit.Name);
            if ('C' == range)
               sb51.Append("-10 for 3rd shot at close range\n");
            else if ('M' == range)
               sb51.Append("-20 for 3rd shot at medium range\n");
            else if ('L' == range)
               sb51.Append("-30 for 3rd shot at long range\n");
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): reached default range=" + range);
               return "ERROR";
            }
         }
         //------------------------------------
         if ((true == enemyUnit.IsVehicle) && (true == enemyUnit.IsMoving))
         {
            if ('C' == range)
               sb51.Append("+20 for moving target at close range\n");
            else if ('M' == range)
               sb51.Append("+25 for moving target at medium range\n");
            else if ('L' == range)
               sb51.Append("+25 for moving target at long range\n");
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): reached default range=" + range);
               return "ERROR";
            }
         }
         //------------------------------------
         if (true == isCommanderDirectingFire)
         {
            sb51.Append("-");
            sb51.Append(commander.Rating.ToString());
            sb51.Append(" for commander rating directing fire\n");
         }
         sb51.Append("-");
         sb51.Append(gunner.Rating.ToString());
         sb51.Append(" for gunner rating\n");
         //------------------------------------
         if(gi.Sherman.RotationTurret != gi.ShermanRotationTurretOld)
         {
            double t1 = 360 - gi.Sherman.RotationTurret;
            double t2 = gi.ShermanRotationTurretOld;
            double totalAngle = t1 + t2;
            if (360.0 < totalAngle)
               totalAngle = totalAngle - 360;
            if (180.0 < totalAngle)
               totalAngle = 360 - totalAngle;
            int numRotations = (int)(totalAngle / 60.0);
            int turretMod = (int)(10.0 * numRotations);
            sb51.Append("+");
            sb51.Append(turretMod.ToString());
            sb51.Append(" for turret rotations\n");
         }
         //------------------------------------
         if (true == enemyUnit.Name.Contains("Pak43"))
            sb51.Append("+10 for firing at 88LL AT Gun\n");
         else if ((true == enemyUnit.Name.Contains("Pak40")) || (true == enemyUnit.Name.Contains("Pak38")))
            sb51.Append("+20 for firing at 50L or 75L AT Gun\n");
         //==================================
         Logger.Log(LogEnum.LE_SHOW_TO_HIT_MODIFIER, "UpdateEventContent_GetToHitModifier(): ------------>>>>>>>>>>>gi.ShermanTypeOfFire=" + gi.ShermanTypeOfFire + " range=" + range.ToString());
         if ("Direct" == gi.ShermanTypeOfFire)   // EventViewer.UpdateEventContent_GetToHitModifier()
         {
            //----------------------------
            if (true == enemyUnit.IsVehicle)
            {
               //----------------------------
               string enemyUnitType = enemyUnit.GetEnemyUnit();
               if ("ERROR" == enemyUnitType)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): unknown enemyUnit=" + enemyUnit.Name);
                  return "ERROR";
               }
               switch (enemyUnitType)
               {
                  case "SPG":
                  case "STuGIIIg": // small size
                  case "JdgPzIV":
                  case "JdgPz38t":
                  case "SPW":
                     sb51.Append("+10 for small target\n"); break;
                  case "PzIV":  // average size
                  case "MARDERII":
                  case "MARDERIII":
                  case "PSW":  
                  case "TRUCK":
                     break;
                  case "TANK":
                  case "PzV":  // large size
                  case "PzVIe":
                     if ('C' == range)
                        sb51.Append("-5 for large target at close range\n");
                     else if ('M' == range)
                        sb51.Append("-10 for large target at medium range\n");
                     else if ('L' == range)
                        sb51.Append("-15 for large target at large range\n");

                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): reached default range=" + range);
                        return "ERROR";
                     }
                     break;
                  case "PzVIb": // very large size
                     if ('C' == range)
                        sb51.Append("-10 for very large target at close range\n");
                     else if ('M' == range)
                        sb51.Append("-20 for very large target at medium range\n");
                     else if ('L' == range)
                        sb51.Append("-30 for very large target at large range\n");
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): reached default range=" + range);
                        return "ERROR";
                     }
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetToHitModifier(): Reached Default for vehicle=enemyUnitType=" + enemyUnitType);
                     return "ERROR";
               }
               //----------------------------
               if (true == isShermanMoving)
                  sb51.Append("+25 for moving or pivoting Sherman\n");
               //----------------------------
               if (true == enemyUnit.IsWoods)
               {
                  if ('C' == range)
                     sb51.Append("+5 for target in woods at close range\n");
                  else if ('M' == range)
                     sb51.Append("+10 for target in woods at medium range\n");
                  else if ('L' == range)
                     sb51.Append("+15 for target in woods at large range\n");
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): reached default range=" + range);
                     return "ERROR";
                  }
               }
               if ((true == enemyUnit.IsBuilding) && (false == enemyUnit.IsVehicle))
               {
                  if ('C' == range)
                     sb51.Append("+10 for target in building at close range\n");
                  else if ('M' == range)
                     sb51.Append("+15 for target in building at medium range\n");
                  else if ('L' == range)
                     sb51.Append("+25 for target in building at large range\n");
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): reached default range=" + range);
                     return "ERROR";
                  }
               }
               if ((true == enemyUnit.IsFortification) && (false == enemyUnit.IsVehicle))
               {
                  if ('C' == range)
                     sb51.Append("+15 for target in fortification at close range\n");
                  else if ('M' == range)
                     sb51.Append("+25 for target in fortification at medium range\n");
                  else if ('L' == range)
                     sb51.Append("+35 for target in fortification at large range\n");
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): reached default range=" + range);
                     return "ERROR";
                  }
               }
               //----------------------------
               if (true == gi.IsShermanDeliberateImmobilization)
               {
                  if ('C' == range)
                     sb51.Append("+65 for deliberate immobilization\n");
                  else if ('M' == range)
                     sb51.Append("+55 for deliberate immobilization\n");
                  else if ('L' == range)
                     sb51.Append("+45 for deliberate immobilization\n");
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent_GetToHitModifier(): reached default range=" + range);
                     return "ERROR";
                  }
               }
            }
         }
         if (0 == sb51.Length)
            return "None";
         else
            return sb51.ToString();
      }
      private bool UpdateEventContentGetToHitModifierImmobilization(IGameInstance gi, IMapItem enemyUnit)
      {
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetToHitModifierImmobilization(): myTextBlock=null");
            return false;
         }
         if ( null == gi.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetToHitModifierImmobilization(): gi.TargetMainGun=null");
            return false;
         }
         if ((true == gi.TargetMainGun.IsVehicle) && ("Direct" ==  gi.ShermanTypeOfFire) && (false == gi.TargetMainGun.IsHullDown)) // EventViewer.UpdateEventContent_GetToHitModifierImmobilization()
         {
            if (3 != enemyUnit.TerritoryCurrent.Name.Length)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetToHitModifierImmobilization(): 3 != TerritoryCurrent.Name.Length=" + enemyUnit.TerritoryCurrent.Name);
               return false;
            }
            CheckBox cbe053 = new CheckBox() { IsEnabled=false, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            if ( Utilities.NO_RESULT == gi.DieResults["e053b"][0])
            {
               cbe053.IsEnabled = true;
               if (true == gi.IsShermanDeliberateImmobilization)
               {
                  cbe053.IsChecked = true;
                  cbe053.Unchecked += CheckBoxImmobilization_Unchecked;
               }
               else
               {
                  cbe053.IsChecked = false;
                  cbe053.Checked += CheckBoxImmobilization_Checked;
               }
            }
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new InlineUIContainer(cbe053));
            myTextBlock.Inlines.Add(new Run(" Check if trying to immobilize"));
         }
         //------------------------------------
         return true;
      }
      private string UpdateEventContentRateOfFireModifier(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetMovingModifier(): lastReport=null");
            return "ERROR";
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //-------------------------------------------------
         ICrewMember? gunner = gi.GetCrewMemberByRole("Gunner");
         if (null == gunner)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanRateOfFireModifier(): gunner=null");
            return "ERROR";
         }
         //-------------------------------------------------
         ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
         if (null == loader)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetShermanRateOfFireModifier(): loader=null");
            return "ERROR";
         }
         //-------------------------------------------------
         int rateOfFireModifier = 0;
         StringBuilder sb51 = new StringBuilder();
         sb51.Append("Rate of fire is achieved when the to hit die roll (");
         sb51.Append(gi.DieResults["e053c"][0]);
         sb51.Append(") is less or equal to  rate of fire base number (");
         if( "75" == card.myMainGun )
            sb51.Append("30) plus these modifiers: \n\n");
         else
            sb51.Append("20) plus these modifiers: \n\n");
         sb51.Append("+");
         sb51.Append(gunner.Rating.ToString());
         sb51.Append(" for gunner rating\n");
         sb51.Append("+");
         sb51.Append(loader.Rating.ToString());
         sb51.Append(" for loader rating\n");
         //-------------------------------------------------
         if (true == gi.IsReadyRackReload())
         {
            rateOfFireModifier -= 10;
            sb51.Append("+10 for pulling from ready rack\n");
         }
         else
         {
            bool isAssistantPassesAmmo = false;
            foreach (IMapItem crewAction in gi.CrewActions)
            {
               if ("Assistant_PassAmmo" == crewAction.Name)
                  isAssistantPassesAmmo = true;
            }
            if (true == isAssistantPassesAmmo)
            {
               ICrewMember? assistant = gi.GetCrewMemberByRole("Assistant");
               if (null == assistant)
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetShermanRateOfFireModifier(): assistant=null");
                  return "ERROR";
               }
               sb51.Append("+");
               sb51.Append(assistant.Rating.ToString());
               sb51.Append(" for assistant rating\n");
            }
         }
         if (0 == sb51.Length)
            return "None";
         else
            return sb51.ToString();
      }
      private bool UpdateEventContentToKillInfantry(IGameInstance gi)
      {
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillInfantry(): myTextBlock=null");
            return false;
         }
         string key = gi.EventActive;
         //--------------------------------------------------------------------------
         string modiferToKillInfantry = UpdateEventContentToKillInfantryModifier(gi);
         bool isImageShown = false;
         if (null == gi.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillInfantry(): gi.TargetMainGun=null for key=" + key);
            return false;
         }
         if (0 == gi.ShermanHits.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillInfantry(): gi.Sherman_Hits.Count=0 for key=" + key);
            return false;
         }
         ShermanAttack hit = gi.ShermanHits[0];
         myTextBlock.Inlines.Add(new Run(modiferToKillInfantry));
         myTextBlock.Inlines.Add(new LineBreak());
         myTextBlock.Inlines.Add(new LineBreak());
         int toKillNum = TableMgr.GetShermanToKillInfantryBaseNumber(gi, gi.TargetMainGun, hit);
         if (TableMgr.FN_ERROR == toKillNum)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillInfantry(): GetShermanToKillInfantryBaseNumber() returned error for key=" + key);
            return false;
         }
         int modifier = TableMgr.GetShermanToKillInfantryModifier(gi, gi.TargetMainGun, hit);
         if (TableMgr.FN_ERROR == modifier)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillInfantry(): GetShermanToKillInfantryModifier() returned error for key=" + key);
            return false;
         }
         if (TableMgr.KIA == modifier)
         {
            myTextBlock.Inlines.Add(new Run("Target is automatically killed!"));
            isImageShown = true;
         }
         else if (TableMgr.NO_CHANCE == modifier)
         {
            myTextBlock.Inlines.Add(new Run("No chance to kill target with " + hit.myAmmoType.ToUpper() + " ammo!"));
            isImageShown = true;
         }
         else
         {
            int combo = toKillNum - modifier;
            StringBuilder sb = new StringBuilder();
            sb.Append("To kill, roll ");
            sb.Append(toKillNum.ToString());
            if (modifier < 0)
               sb.Append(" + ");
            else
               sb.Append(" - ");
            sb.Append(Math.Abs(modifier).ToString("F0"));
            sb.Append(" = ");
            sb.Append(combo.ToString());
            sb.Append(" or less: ");
            myTextBlock.Inlines.Add(new Run(sb.ToString()));
            if (Utilities.NO_RESULT == gi.DieResults[key][0])
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
               bmi.EndInit();
               Image imgDice = new Image { Name = "DieRollBlue", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(imgDice, bmi);
               myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
            }
            else
            {
               myTextBlock.Inlines.Add(new Run(gi.DieResults[key][0].ToString()));
               if (combo < gi.DieResults[key][0])
                  myTextBlock.Inlines.Add("  =  NO EFFECT");
               else
                  myTextBlock.Inlines.Add("  =  KILL");
               isImageShown = true;
            }
         }
         if (true == isImageShown)
         {
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("                                            "));
            Image imge53d = new Image { Name = "Continue53d", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
            myTextBlock.Inlines.Add(new InlineUIContainer(imge53d));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("Click image to continue."));
         }
         return true;
      }
      private string UpdateEventContentToKillInfantryModifier(IGameInstance gi)
      {
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentGetMovingModifier(): lastReport=null");
            return "ERROR";
         }
         //-------------------------------------------------
         if (null == gi.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillInfantryModifier(): gi.TargetMainGun=null");
            return "ERROR";
         }
         //-------------------------------------------------
         if (0 == gi.ShermanHits.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillInfantryModifier(): gi.Sherman_Hits.Count=0");
            return "ERROR";
         }
         ShermanAttack hit = gi.ShermanHits[0];
         //------------------------------------------------- 
         StringBuilder sb51 = new StringBuilder();
         if (("Direct" != hit.myAttackType) || (true == hit.myIsCriticalHit))
         {
            if (true == gi.TargetMainGun.IsBuilding)
            {
               if (false == hit.myIsCriticalHit)
                  sb51.Append("+15 Target in Building\n");
               else
                  sb51.Append("-15 Target in Building\n");
            }
            if (true == gi.TargetMainGun.IsWoods)
            {
               if (false == hit.myIsCriticalHit)
                  sb51.Append("+10 Target in Woods\n");
               else
                  sb51.Append("-10 Target in Woods\n");
            }
            if (true == gi.TargetMainGun.IsFortification)
            {
               if (false == hit.myIsCriticalHit)
                  sb51.Append("+20 Target in Fortification\n");
               else
                  sb51.Append("-20 Target in Fortification\n");
            }
         }
         //------------------------------------
         if ((true == gi.TargetMainGun.Name.Contains("ATG")) || (true == gi.TargetMainGun.Name.Contains("Pak43")) || (true == gi.TargetMainGun.Name.Contains("Pak40")) || (true == gi.TargetMainGun.Name.Contains("Pak38")))
         {
            sb51.Append("+15 for ATG Target\n");
         }
         //------------------------------------
         if (true == gi.TargetMainGun.IsMovingInOpen)
            sb51.Append("-10 Target Moving in Open\n");
         //------------------------------------
         if (true == lastReport.Weather.Contains("Deep Snow") || true == lastReport.Weather.Contains("Mud"))         
            sb51.Append("+5 Mud or Deep Snow Weather\n");
         if (0 == sb51.Length)
            return "None";
         else
            return sb51.ToString();
      }
      private bool UpdateEventContentToKillVehicle(IGameInstance gi)
      {
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicle(): myTextBlock=null");
            return false;
         }
         string key = gi.EventActive;
         //------------------------------------
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicle(): myGameInstance=null for key=" + key);
            return false;
         }
         //------------------------------------
         if (null == gi.TargetMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicle(): gi.TargetMainGun=null for key=" + key);
            return false;
         }
         //------------------------------------
         if (0 == gi.ShermanHits.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicle(): gi.Sherman_Hits.Count=0 for key=" + key);
            return false;
         }
         //------------------------------------
         ShermanAttack hit = gi.ShermanHits[0];
         if ("Wp" == hit.myAmmoType) 
         {
            myNumSmokeAttacksThisRound++;
            StringBuilder sb1 = new StringBuilder();
            sb1.Append("The hit with the WP round already caused one Smoke Marker to appear in the target&apos; zone.\n\n");
            sb1.Append("Smoke Attack #");
            sb1.Append(myNumSmokeAttacksThisRound.ToString());
            sb1.Append(" out of ");
            sb1.Append(myGameInstance.NumSmokeAttacksThisRound.ToString());
            myTextBlock.Inlines.Add(new Run(sb1.ToString()));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("The hit with the WP round already caused one Smoke Marker to appear in the target&apos; zone."));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("                                            "));
            Image imge53e = new Image { Name = "Continue53f", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
            myTextBlock.Inlines.Add(new InlineUIContainer(imge53e));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("Click image to continue."));
         }
         else if ("Hbci" == hit.myAmmoType)
         {
            myNumSmokeAttacksThisRound++;
            StringBuilder sb1 = new StringBuilder();
            sb1.Append("The hit with the HBCI round already caused two Smoke Marker to appear in the target&apos; zone.\n\n");
            sb1.Append("Smoke Attack #");
            sb1.Append(myNumSmokeAttacksThisRound.ToString());
            sb1.Append(" out of ");
            sb1.Append(myGameInstance.NumSmokeAttacksThisRound.ToString());
            myTextBlock.Inlines.Add(new Run(sb1.ToString()));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("                                            "));
            Image imge53e = new Image { Name = "Continue53f", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
            myTextBlock.Inlines.Add(new InlineUIContainer(imge53e));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("Click image to continue."));
         }
         else if (true == hit.myIsImmobilization)
         {
            myTextBlock.Inlines.Add(new Run("Ammo hit the tracks resulting in immobilization."));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("                                            "));
            Image imge53e = new Image { Name = "ThrownTrack", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("c106ThrownTrack") };
            myTextBlock.Inlines.Add(new InlineUIContainer(imge53e));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("Click image to continue."));
         }
         else
         {
            IAfterActionReport? report = gi.Reports.GetLast();
            if (null == report)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicle():  gi.Reports.GetLast()");
               return false;
            }
            TankCard tankcard = new TankCard(report.TankCardNum);
            //------------------------------------
            Button be53ce1 = new Button() { FontFamily = myFontFam1, FontSize = 12 };
            be53ce1.Click += Button_Click;
            myTextBlock.Inlines.Add(new Run("For each hit scored against a target, consult the "));
            if ("75" == tankcard.myMainGun) // This screen only applies to HE and AP hits against enemy vehicles   
            {
               if ("He" == hit.myAmmoType)
                  be53ce1.Content = "HE To Kill (75)";
               else if ("Ap" == hit.myAmmoType)
                  be53ce1.Content = "AP To Kill (75)";
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicle():  unknown ammotype=" + hit.myAmmoType + " guntype=" + tankcard.myMainGun);
                  return false;
               }
            }
            else if ("76L" == tankcard.myMainGun)
            {
               if ("He" == hit.myAmmoType)
                  be53ce1.Content = "HE To Kill (76)";
               else if (("Ap" == hit.myAmmoType) || ("Hvap" == hit.myAmmoType))
                  be53ce1.Content = "AP To Kill (76L)";
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicle():  unknown ammotype=" + hit.myAmmoType + " guntype=" + tankcard.myMainGun);
                  return false;
               }
            }
            else if ("76LL" == tankcard.myMainGun)
            {
               if ("He" == hit.myAmmoType)
                  be53ce1.Content = "HE To Kill (76)";
               else if (("Ap" == hit.myAmmoType) || ("Hvap" == hit.myAmmoType))
                  be53ce1.Content = "AP To Kill (76LL)";
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicle():  unknown ammotype=" + hit.myAmmoType + " guntype=" + tankcard.myMainGun);
                  return false;
               }
            }
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicle():  unknown guntype=" + tankcard.myMainGun);
               return false;
            }
            myTextBlock.Inlines.Add(new InlineUIContainer(be53ce1));
            be53ce1.Click += Button_Click;
            myTextBlock.Inlines.Add(new Run(" Table to determine if the target is knocked out (KO'ed)."));
            //------------------------------------
            if (true == hit.myIsCriticalHit)
            {
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run(" CRITICAL HIT!! "));
            }
            //------------------------------------
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("First, roll hit location: "));
            if (Utilities.NO_RESULT == gi.DieResults[key][0])
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollWhite.gif", UriKind.Absolute);
               bmi.EndInit();
               Image imgDice = new Image { Name = "DieRollWhite", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(imgDice, bmi);
               myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
            }
            else
            {
               myTextBlock.Inlines.Add(new Run(gi.DieResults[key][0].ToString()));
               myTextBlock.Inlines.Add(new Run("  =  "));
               myTextBlock.Inlines.Add(new Run(hit.myHitLocation));
               myTextBlock.Inlines.Add(new LineBreak());
               //------------------------------------
               if (true == hit.myHitLocation.Contains("MISS"))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  Image imge53e = new Image { Name = "Miss", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge53e));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else if (true == hit.myHitLocation.Contains("Thrown Track"))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  Image imge53e = new Image { Name = "ThrownTrack", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("c106ThrownTrack") };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge53e));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else if ((true == hit.myHitLocation.Contains("Hull")) || (true == hit.myHitLocation.Contains("Turret")))
               {
                  int toKillNum = UpdateEventContentToKillVehicleGetToKillNum(gi, tankcard, hit);
                  if (TableMgr.FN_ERROR == toKillNum)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicle(): UpdateEventContentToKillVehicleCheckGetToKillNum() returned false");
                     return false;
                  }
                  bool isShowImage = false;
                  if (TableMgr.KIA == toKillNum)
                  {
                     isShowImage = true;
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Target is automatically killed!"));
                  }
                  else if (TableMgr.NO_CHANCE == toKillNum)
                  {
                     isShowImage = true;
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("No chance in killing the target!"));
                  }
                  else
                  {
                     StringBuilder sb = new StringBuilder();
                     sb.Append("To kill, roll ");
                     sb.Append(toKillNum.ToString("F0"));
                     sb.Append(" or less: ");
                     myTextBlock.Inlines.Add(new Run(sb.ToString()));
                     if (Utilities.NO_RESULT == gi.DieResults[key][1])
                     {
                        BitmapImage bmi = new BitmapImage();
                        bmi.BeginInit();
                        bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
                        bmi.EndInit();
                        Image imgDice = new Image { Name = "DieRollBlue", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                        ImageBehavior.SetAnimatedSource(imgDice, bmi);
                        myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
                     }
                     else
                     {
                        isShowImage = true;
                        myTextBlock.Inlines.Add(new Run(gi.DieResults[key][1].ToString()));
                        if (toKillNum < gi.DieResults[key][1])
                           myTextBlock.Inlines.Add("  =  NO EFFECT");
                        else
                           myTextBlock.Inlines.Add("  =  KILL");
                     }
                  }
                  if (true == isShowImage)
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                            "));
                     Image imge53e = new Image { Name = "Continue53e", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
                     myTextBlock.Inlines.Add(new InlineUIContainer(imge53e));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Click image to continue."));
                  }
               }
            }
         }
         return true;
      }
      private int UpdateEventContentToKillVehicleGetToKillNum(IGameInstance gi,TankCard tankcard, ShermanAttack hit)
      {
         if( null == gi.TargetMainGun )
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicleCheckIfNoChance(): gi.TargetMainGun=null");
            return TableMgr.FN_ERROR;
         }
         //------------------------------------
         int toKillNum = 0;
         if ("75" == tankcard.myMainGun)
         {
            if ("He" == hit.myAmmoType)
               toKillNum = TableMgr.GetShermanToKill75HeVehicleBaseNumber(gi, gi.TargetMainGun, hit);
            else if ("Ap" == hit.myAmmoType)
               toKillNum = TableMgr.GetShermanToKill75ApVehicleBaseNumber(gi, gi.TargetMainGun, hit);
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicleCheckIfNoChance():  unsupported ammo type=" + hit.myAmmoType);
               return TableMgr.FN_ERROR;
            }
         }
         else if ("76L" == tankcard.myMainGun)
         {
            if ("He" == hit.myAmmoType)
               toKillNum = TableMgr.GetShermanToKill76HeVehicleBaseNumber(gi, gi.TargetMainGun, gi.ShermanHits[0]);
            else if ("Ap" == hit.myAmmoType)
               toKillNum = TableMgr.GetShermanToKill76ApVehicleBaseNumber(gi, gi.TargetMainGun, gi.ShermanHits[0]);
            else if ("Hvap" == hit.myAmmoType)
               toKillNum = TableMgr.GetShermanToKill76HvapVehicleBaseNumber(gi, gi.TargetMainGun, gi.ShermanHits[0]);
            else
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicleCheckIfNoChance():  unsupported ammo type=" + hit.myAmmoType);
               return TableMgr.FN_ERROR;
            }
         }
         else if ("76LL" == tankcard.myMainGun)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicleCheckIfNoChance():  unsupported gun type=" + tankcard.myMainGun);
            return TableMgr.FN_ERROR;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicleCheckIfNoChance():  unknown gun type=" + tankcard.myMainGun);
            return TableMgr.FN_ERROR;
         }
         if (TableMgr.FN_ERROR == toKillNum)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentToKillVehicleCheckIfNoChance(): GetShermanToHitBaseNumber() returned error");
            return TableMgr.FN_ERROR;
         }
         return toKillNum;
      }
      private bool UpdateEventContentMgToKill(IGameInstance gi)
      {
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKill(): myTextBlock=null");
            return false;
         }
         string key = gi.EventActive;
         //------------------------------------
         if (null == gi.TargetMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKill(): gi.TargetMg=null for key=" + key);
            return false;
         }
         //------------------------------------
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKill():  gi.Reports.GetLast()");
            return false;
         }
         //------------------------------------
         string mgType = "None";
         if (true == gi.IsShermanFiringAaMg)
            mgType = "Aa";
         else if (true == gi.IsShermanFiringBowMg)
            mgType = "Bow";
         else if (true == gi.IsShermanFiringCoaxialMg)
            mgType = "Coaxial";
         else if (true == gi.IsShermanFiringSubMg)
            mgType = "Sub";
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKill(): unknown MG firing");
            return false;
         }
         //------------------------------------
         myTextBlock.Inlines.Add(new LineBreak());
         myTextBlock.Inlines.Add(new Run("Modifiers") { TextDecorations = TextDecorations.Underline });
         myTextBlock.Inlines.Add(new LineBreak());
         string modiferMgFiring = UpdateEventContentMgToKillModifier(gi, mgType);
         if( "ERROR" == modiferMgFiring )
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKill():  UpdateEventContentMgToKillModifier() returned false");
            return false;
         }
         myTextBlock.Inlines.Add(new Run(modiferMgFiring));
         myTextBlock.Inlines.Add(new LineBreak());
         //------------------------------------
         int toKillNum = TableMgr.GetShermanMgToKillNumber(gi, gi.TargetMg, mgType);
         if (TableMgr.FN_ERROR == toKillNum)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKill(): Get_ShermanMgToKillNumber() returned error for key=" + key);
            return false;
         }
         else if (TableMgr.NO_CHANCE == toKillNum)
         {
            myTextBlock.Inlines.Add(new Run("No chance to kill. "));
            myTextBlock.Inlines.Add(new Run("Click image to continue."));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("                                            "));
            Image imge53e = new Image { Name = "Continue54a", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
            myTextBlock.Inlines.Add(new InlineUIContainer(imge53e));
            return true;
         }
         //------------------------------------
         int modifier = TableMgr.GetShermanMgToKillModifier(gi, gi.TargetMg, mgType);
         if (TableMgr.FN_ERROR == modifier)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKill(): GetShermanMgToKillModifier() returned error for key=" + key);
            return false;
         }
         int comboMg = toKillNum - modifier;
         StringBuilder sb = new StringBuilder();
         sb.Append("To kill, roll ");
         sb.Append(toKillNum.ToString("F0"));
         if (modifier < 0)
            sb.Append(" + ");
         else
            sb.Append(" - ");
         sb.Append(Math.Abs(modifier).ToString());
         sb.Append(" = ");
         sb.Append(comboMg.ToString());
         sb.Append(" or less: ");
         myTextBlock.Inlines.Add(new Run(sb.ToString()));
         if (Utilities.NO_RESULT == gi.DieResults[key][0])
         {
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
            bmi.EndInit();
            Image imgDice = new Image { Name = "DieRollBlue", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
            ImageBehavior.SetAnimatedSource(imgDice, bmi);
            myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
         }
         else
         {
            myTextBlock.Inlines.Add(new Run(gi.DieResults[key][0].ToString()));
            if(gi.DieResults[key][0] < 4)
               myTextBlock.Inlines.Add(new Run("  =  Automatic Kill!"));
            else if (97 < gi.DieResults[key][0])
               myTextBlock.Inlines.Add(new Run("  =  Gun Malfunctions!"));
            else if (comboMg < gi.DieResults[key][0])
               myTextBlock.Inlines.Add(new Run("  =  NO EFFECT"));
            else
               myTextBlock.Inlines.Add(new Run("  =  KILL"));
            StringBuilder sbe53e = new StringBuilder();
            if ((1 == DieRoller.BlueDie) || (2 == DieRoller.BlueDie) || (3 == DieRoller.BlueDie) || (97 < gi.DieResults[key][0]))
               sbe53e.Append("\n\n");
            if (( 1 == DieRoller.BlueDie) || (2 == DieRoller.BlueDie) || (3 == DieRoller.BlueDie) )
               sbe53e.Append("One MG Ammo Expended! ");
            if( 0 < sbe53e.Length)
               myTextBlock.Inlines.Add(new Run(sbe53e.ToString()));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("                                            "));
            Image imge53e = new Image { Name = "Continue54a", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
            myTextBlock.Inlines.Add(new InlineUIContainer(imge53e));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("Click image to continue."));
         }
         return true;
      }
      private string UpdateEventContentMgToKillModifier(IGameInstance gi, string mgType)
      {
         //------------------------------------
         if (null == gi.TargetMg)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKillModifier(): gi.TargetMg=null");
            return "ERROR";
         }
         //------------------------------------
         if (3 != gi.TargetMg.TerritoryCurrent.Name.Length)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKillModifier(): 3 != TerritoryCurrent.Name.Length=" + gi.TargetMg.TerritoryCurrent.Name);
            return "ERROR";
         }
         char range = gi.TargetMg.TerritoryCurrent.Name[2];
         if (('C' != range) && ('M' != range) && ('L' != range))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKillModifier(): unknown range=" + range.ToString() + " t=" + gi.TargetMg.TerritoryCurrent.Name);
            return "ERROR";
         }
         //------------------------------------
         string enemyUnitType = gi.TargetMg.GetEnemyUnit();
         if ("ERROR" == enemyUnitType)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKillModifier(): unknown gi.TargetMg=" + gi.TargetMg.Name);
            return "ERROR";
         }
         if (("LW" != enemyUnitType) && ("MG" != enemyUnitType) && ("ATG" != enemyUnitType) && ("Pak38" != enemyUnitType) && ("Pak40" != enemyUnitType) && ("Pak43" != enemyUnitType) && ("TRUCK" != enemyUnitType))
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKillModifier(): MG fire not appropriate for enemyType=" + enemyUnitType);
            return "ERROR";
         }
         //------------------------------------
         bool isCommanderFiring = false;
         bool isLoaderFiring = false;
         bool isGunnerFiring = false;
         bool isAssistantFiring = false;
         bool isMovingOrPivoting = false;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if (("Commander_FireAaMg" == crewAction.Name) && ("Aa" == mgType) )
               isCommanderFiring = true;
            if (("Commander_FireSubMg" == crewAction.Name) && ("Sub" == mgType))
               isCommanderFiring = true;
            if (("Loader_FireAaMg" == crewAction.Name) && ("Aa" == mgType))
               isLoaderFiring = true;
            if (("Loader_FireSubMg" == crewAction.Name) && ("Sub" == mgType)) 
               isLoaderFiring = true;
            if (("Gunner_FireCoaxialMg" == crewAction.Name) && ("Coaxial" == mgType))
               isGunnerFiring = true;
            if (("Assistant_FireBowMg" == crewAction.Name) && ("Bow" == mgType))
               isAssistantFiring = true;
            if ("Driver_Forward" == crewAction.Name)
               isMovingOrPivoting = true;
            if ("Driver_ForwardToHullDown" == crewAction.Name)
               isMovingOrPivoting = true;
            if ("Driver_Reverse" == crewAction.Name)
               isMovingOrPivoting = true;
            if ("Driver_ReverseToHullDown" == crewAction.Name)
               isMovingOrPivoting = true;
            if ("Driver_PivotTank" == crewAction.Name)
               isMovingOrPivoting = true;
         }
         //------------------------------------
         StringBuilder sb = new StringBuilder();
         //------------------------------------
         if (true == gi.IsCommanderDirectingMgFire) // Commander directs MG fire for all Advancing Fire in Battle Round - maybe not same as rules, but not a major game changer
         {
            ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
            if (null == commander)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKillModifier(): commander=null");
               return "ERROR";
            }
            sb.Append("-");
            sb.Append(commander.Rating.ToString());
            sb.Append(" for commander directing fire\n");
         }
         //------------------------------------
         if (true == isCommanderFiring)
         {
            ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
            if (null == commander)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKillModifier(): commander=null");
               return "ERROR";
            }
            sb.Append("-");
            sb.Append(commander.Rating.ToString());
            sb.Append(" for commander rating\n");
         }
         //------------------------------------
         if (true == isLoaderFiring)
         {
            ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
            if (null == loader)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKillModifier(): loader=null");
               return "ERROR";
            }
            sb.Append("-");
            sb.Append(loader.Rating.ToString());
            sb.Append(" for loader rating\n");
         }
         //------------------------------------
         if (true == isGunnerFiring)
         {
            ICrewMember? gunner = gi.GetCrewMemberByRole("Gunner");
            if (null == gunner)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKillModifier(): gunner=null");
               return "ERROR";
            }
            sb.Append("-");
            sb.Append(gunner.Rating.ToString());
            sb.Append(" for gunner rating\n");
         }
         //------------------------------------
         if (true == isAssistantFiring)
         {
            ICrewMember? assistant = gi.GetCrewMemberByRole("Assistant");
            if (null == assistant)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentMgToKillModifier(): assistant=null");
               return "ERROR";
            }
            sb.Append("-");
            sb.Append(assistant.Rating.ToString());
            sb.Append(" for assistant rating\n");
         }
         //------------------------------------
         if ( (true == gi.TargetMg.IsMoving) && ("LW" == enemyUnitType) )
         {
            if ("Bow" == mgType)
               sb.Append("-10 if LW moving with bow MG\n");
            else if ("Coaxial" == mgType)
               sb.Append("-15 if LW moving with co-axial MG\n");
            else if ("Aa" == mgType)
               sb.Append("-15 if LW moving with AA MG\n");
            else
               sb.Append("-5 if LW moving with Sub MG\n");
         }
         //------------------------------------
         if (true == isMovingOrPivoting)
         {
            sb.Append("+10 for Sherman moving or pivoting\n");
         }
         //------------------------------------
         if (true == gi.TargetMg.IsWoods)
         {
            sb.Append("+10 if target in woods\n");
         }
         //------------------------------------
         if (true == gi.TargetMg.IsBuilding)
         {
            sb.Append("+15 if target in building \n");
         }
         //------------------------------------
         if (("ATG" == enemyUnitType) || ("Pak38" == enemyUnitType) || ("Pak40" == enemyUnitType) || ("Pak43" == enemyUnitType))
         {
            sb.Append("+15 if target is ");
            sb.Append(enemyUnitType);
            sb.Append("\n");
         }
         //------------------------------------
         if (true == gi.TargetMg.IsFortification)
         {
            sb.Append("+20 if target in fortification\n");
         }
         return sb.ToString() ;
      }
      private bool UpdateEventContentVictoryPointTotal(IGameInstance gi)
      {
         //----------------------------------------
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentVictoryPointTotal(): myTextBlock=null");
            return false;
         }
         string key = gi.EventActive;
         //----------------------------------------
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentVictoryPointTotal():  gi.Reports.GetLast()");
            return false;
         }
         //----------------------------------------
         StringBuilder sbe101 = new StringBuilder();
         sbe101.Append("Engagement Victory Points: ");
         sbe101.Append(report.VictoryPtsTotalEngagement.ToString());
         sbe101.Append("\nCampaign Victory Points: ");
         sbe101.Append(gi.VictoryPtsTotalCampaign.ToString());
         //----------------------------------------
         Image? imge101 = null;
         if (true == gi.IsCommanderKilled)
         {
            sbe101.Append("\n\nYou as the commander are killed. Engagement Lost!");
            imge101 = new Image { Name = "CampaignOver", Width = 200, Height = 200, Source = MapItem.theMapImages.GetBitmapImage("CommanderDead") };
            myTextBlock.Inlines.Add(new Run(sbe101.ToString()));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("                                    "));
         }
         else if ( 0 >= report.VictoryPtsTotalEngagement )
         {
            sbe101.Append("\n\nTotal Victory Points is not positive. Engagement Lost!");
            imge101 = new Image { Name = "EventDebriefVictoryPts", Width = 200, Height = 200, Source = MapItem.theMapImages.GetBitmapImage("Deny") };
            myTextBlock.Inlines.Add(new Run(sbe101.ToString()));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("                                    "));
         }
         else
         {
            sbe101.Append("\n\nTotal Victory Points is greater than zero. Engagement Won!");
            imge101 = new Image { Name = "EventDebriefVictoryPts", Width = 100, Height = 200, Source = MapItem.theMapImages.GetBitmapImage("Victory") };
            myTextBlock.Inlines.Add(new Run(sbe101.ToString()));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("                                                 "));
         }
         myTextBlock.Inlines.Add(new InlineUIContainer(imge101));
         myTextBlock.Inlines.Add(new LineBreak());
         myTextBlock.Inlines.Add(new LineBreak());
         myTextBlock.Inlines.Add(new Run("Click image to continue."));
         return true;
      }
      private bool UpdateEventContentPromotion(IGameInstance gi)
      {
         //----------------------------------------
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentPromotion(): myTextBlock=null");
            return false;
         }
         string key = gi.EventActive;
         //----------------------------------------
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentPromotion():  gi.Reports.GetLast()");
            return false;
         }
         //----------------------------------------
         string promoDate = TableMgr.GetDate(gi.PromotionDay);
         StringBuilder sbPromo = new StringBuilder();
         sbPromo.Append("Promotion Points: ");
         sbPromo.Append(gi.PromotionPointNum.ToString());
         sbPromo.Append("\nLast Promotion Date: ");
         sbPromo.Append(promoDate);
         myTextBlock.Inlines.Add(sbPromo.ToString());
         myTextBlock.Inlines.Add(new LineBreak());
         myTextBlock.Inlines.Add(new LineBreak());
         //------------------------------------------
         Image? imge102 = null;
         switch (report.Commander.Rank)
         {
            case "Sgt":
               imge102 = new Image { Name = "EventDebriefPromotion", Width = 100, Height = 132, Source = MapItem.theMapImages.GetBitmapImage("RankSergeant") };
               break;
            case "Ssg":
               imge102 = new Image { Name = "EventDebriefPromotion", Width = 100, Height = 157, Source = MapItem.theMapImages.GetBitmapImage("RankStaffSergeant") };
               break;
            case "2Lt":
               imge102 = new Image { Name = "EventDebriefPromotion", Width = 100, Height = 250, Source = MapItem.theMapImages.GetBitmapImage("RankFirstLieutenant") };
               break;
            case "1Lt":
               imge102 = new Image { Name = "EventDebriefPromotion", Width = 100, Height = 250, Source = MapItem.theMapImages.GetBitmapImage("RankSecondLieutenant") };
               break;
            case "Cpt":
               imge102 = new Image { Name = "EventDebriefPromotion", Width = 100, Height = 92, Source = MapItem.theMapImages.GetBitmapImage("RankCaptian") };
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): reached default newRank=" + report.Commander.Rank);
               return false;
         }
         myTextBlock.Inlines.Add(new Run("                                              "));
         myTextBlock.Inlines.Add(new InlineUIContainer(imge102));
         myTextBlock.Inlines.Add(new LineBreak());
         myTextBlock.Inlines.Add(new LineBreak());
         if (true == gi.IsPromoted)
            myTextBlock.Inlines.Add(new Run("Congratulations on promotion! "));
         myTextBlock.Inlines.Add(new Run("Click image to continue."));
         return true;
      }
      private bool UpdateEventContentDecoration(IGameInstance gi)
      {
         //----------------------------------------
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentDecoration(): myTextBlock=null");
            return false;
         }
         string key = gi.EventActive;
         //----------------------------------------
         IAfterActionReport? report = gi.Reports.GetLast();
         if (null == report)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentDecoration():  gi.Reports.GetLast()");
            return false;
         }
         //----------------------------------------
         ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
         if (null == commander)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContentDecoration(): commander=null for key=" + key);
            return false;
         }
         int modifierDecoration = 0;
         StringBuilder sbe103 = new StringBuilder();
         //----------------------------------------
         int victoryPointsYourTank = report.VictoryPtsTotalYourTank * 2;
         if (-1 < victoryPointsYourTank)
            sbe103.Append("+");
         sbe103.Append(victoryPointsYourTank.ToString());
         sbe103.Append(" for VPs from your tank\n");
         modifierDecoration += victoryPointsYourTank;
         //----------------------------------------
         int victoryPointsOther = report.VictoryPtsTotalFriendlyForces + report.VictoryPtsTotalTerritory;
         if (-1 < victoryPointsOther)
            sbe103.Append("+");
         sbe103.Append(victoryPointsOther.ToString());
         sbe103.Append(" for VPs from friendly forces and captured territory\n");
         modifierDecoration += victoryPointsOther;
         //----------------------------------------
         if (true == gi.IsCommanderRescuePerformed)
         {
            sbe103.Append("+25 for rescuing crewman\n");
            modifierDecoration += 25;
         }
         //----------------------------------------
         if (("2Lt" == commander.Rank) || ("1Lt" == commander.Rank) || ("Cpt" == commander.Rank))
         {
            sbe103.Append("+5 since you are officer\n");
            modifierDecoration += 5;
         }
         //----------------------------------------
         if ("None" != commander.Wound)
         {
            sbe103.Append("+10 since you were wounded\n");
            modifierDecoration += 10;
         }
         //----------------------------------------
         string month = TableMgr.GetMonth(gi.Day);
         if (("Nov" == month) || ("Dec" == month))
         {
            sbe103.Append("+5 since month is ");
            sbe103.Append(month);
            sbe103.Append("\n");
            modifierDecoration += 5;
         }
         myTextBlock.Inlines.Add(new Run(sbe103.ToString()));
         //----------------------------------------
         if (modifierDecoration < 100)
         {
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add("Do not qualify for decoration. Click image to continue.");
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new Run("                                            "));
            Image imge103 = new Image { Name = "Continue103", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
            myTextBlock.Inlines.Add(new InlineUIContainer(imge103));
         }
         else if (Utilities.NO_RESULT == gi.DieResults[key][0])
         {
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add("Roll for Decoration: ");
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.UriSource = new Uri(MapImage.theImageDirectory + "DieRollBlue.gif", UriKind.Absolute);
            bmi.EndInit();
            Image imgDice = new Image { Name = "DieRollBlue", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
            ImageBehavior.SetAnimatedSource(imgDice, bmi);
            myTextBlock.Inlines.Add(new InlineUIContainer(imgDice));
         }
         else if (Utilities.NO_RESULT < gi.DieResults[key][0])
         {
            int combo = gi.DieResults[key][0] + modifierDecoration;
            if (199 < combo)
            {
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               Image imgBronze = new Image { Name = "DecorationBronzeStar", Width = 100, Height = 180, Source = MapItem.theMapImages.GetBitmapImage("DecorationBronzeStar") };
               Image imgSilver = new Image { Name = "DecorationSilverStar", Width = 100, Height = 180, Source = MapItem.theMapImages.GetBitmapImage("DecorationSilverStar") };
               Image imgCross = new Image { Name = "DecorationDistinguishedCross", Width = 100, Height = 180, Source = MapItem.theMapImages.GetBitmapImage("DecorationDistinguishedCross") };
               Image imgHonor = new Image { Name = "DecorationMedalOfHonor", Width = 100, Height = 180, Source = MapItem.theMapImages.GetBitmapImage("DecorationMedalOfHonor") };
               if (299 < combo)
               {
                  myTextBlock.Inlines.Add("Qualify for Decoration. Click one of images to continue:");
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("         "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgBronze));
                  myTextBlock.Inlines.Add(new Run("     "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgSilver));
                  myTextBlock.Inlines.Add(new Run("     "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgCross));
                  myTextBlock.Inlines.Add(new Run("     "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgHonor));
               }
               else if (249 < combo)
               {
                  myTextBlock.Inlines.Add("Qualify for Decoration. Click one of images to continue:");
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                   "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgBronze));
                  myTextBlock.Inlines.Add(new Run("     "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgSilver));
                  myTextBlock.Inlines.Add(new Run("     "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgCross));
               }
               else if (224 < combo)
               {
                  myTextBlock.Inlines.Add("Qualify for Decoration. Click one of images to continue:");
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                             "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgBronze));
                  myTextBlock.Inlines.Add(new Run("     "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgSilver));
               }
               else
               {
                  myTextBlock.Inlines.Add("Qualify for Decoration. Click image to continue:");
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgBronze));
               }
            }
            else
            {
               myTextBlock.Inlines.Add("Do not qualify for decoration. Click image to continue.");
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                            "));
               Image imge103 = new Image { Name = "Continue103", Width = 100, Height = 100, Source = MapItem.theMapImages.GetBitmapImage("Continue") };
               myTextBlock.Inlines.Add(new InlineUIContainer(imge103));
            }
         }
         return true;
      }
      private string UpdateEventContentMainGunRepair(IGameInstance gi, out int modifier)
      {
         modifier = 0;
         bool isGunnerHelpingRepair = false;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if ("Gunner_RepairMainGun" == crewAction.Name)
               isGunnerHelpingRepair = true;
         }
         string key = gi.EventActive;
         StringBuilder sbe056 = new StringBuilder();
         ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
         if (null == loader)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): loader=null");
            return "ERROR";
         }
         modifier -= loader.Rating;
         sbe056.Append(" -");
         sbe056.Append(loader.Rating.ToString());
         sbe056.Append(" for loader rating\n");
         if (true == isGunnerHelpingRepair)
         {
            ICrewMember? gunner = gi.GetCrewMemberByRole("Gunner");
            if (null == gunner)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): gunner=null");
               return "ERROR";
            }
            modifier -= gunner.Rating;
            sbe056.Append(" -");
            sbe056.Append(gunner.Rating.ToString());
            sbe056.Append(" for gunner rating\n");
         }
         sbe056.Append("\n");
         return sbe056.ToString();
      }
      private string UpdateEventContentAaMgRepair(IGameInstance gi, out int modifier)
      {
         modifier = 0;
         bool isCommanderRepairing = false;
         bool isLoaderRepairing = false;
         foreach (IMapItem crewAction in gi.CrewActions)
         {
            if ("Commander_RepairAaMg" == crewAction.Name)
               isCommanderRepairing = true;
            if ("Loader_RepairAaMg" == crewAction.Name)
               isLoaderRepairing = true;
         }
         string key = gi.EventActive;
         StringBuilder sbe056 = new StringBuilder();
         if (true == isLoaderRepairing)
         {
            ICrewMember? loader = gi.GetCrewMemberByRole("Loader");
            if (null == loader)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): loader=null");
               return "ERROR";
            }
            modifier -= loader.Rating;
            sbe056.Append(" -");
            sbe056.Append(loader.Rating.ToString());
            sbe056.Append(" for loader rating\n");
         }
         if (true == isCommanderRepairing)
         {
            ICrewMember? commander = gi.GetCrewMemberByRole("Commander");
            if (null == commander)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEventContent(): commander=null");
               return "ERROR";
            }
            modifier -= commander.Rating;
            sbe056.Append(" -");
            sbe056.Append(commander.Rating.ToString());
            sbe056.Append(" for commander rating\n");
         }
         sbe056.Append("\n");
         return sbe056.ToString();
      }
      //--------------------------------------------------------------------
      private bool IsOrdersGiven(IGameInstance gi, out bool isOrdersGiven)
      {
         bool isAssistantOrderGiven = false;
         bool isGunnerOrderGiven = false;
         bool isCommanderOrderGiven = false;
         foreach (IMapItem mi in gi.CrewActions) // Loader and Driver have default actions
         {
            if (true == mi.Name.Contains("Assistant")) // assistant can always pass ammo
               isAssistantOrderGiven = true;
            if ((true == mi.Name.Contains("Gunner")) || (("Gunner" == gi.SwitchedCrewMemberRole) && (true == mi.Name.Contains("Switch"))) )
               isGunnerOrderGiven = true;
            if ( (true == mi.Name.Contains("Commander")) || (("Commander" == gi.SwitchedCrewMemberRole) && (true == mi.Name.Contains("Switch")) ) )
               isCommanderOrderGiven = true;
         }
         //-----------------------------------------------
         if (false == isAssistantOrderGiven)
         {
            if (false == gi.IsCrewActionSelectable("Assistant", out isAssistantOrderGiven))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsOrdersGiven(): IsCrewActionSelectable(Assistant) returned false");
               isOrdersGiven = false;
               return false;
            }
         }
         //-----------------------------------------------
         if ( false == isGunnerOrderGiven)
         {
            if (false == gi.IsCrewActionSelectable("Gunner", out isGunnerOrderGiven))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsOrdersGiven(): IsCrewActionSelectable(Gunner) returned false");
               isOrdersGiven = false;
               return false;
            }
         }
         //-----------------------------------------------
         if (false == isCommanderOrderGiven)
         {
            if (false == gi.IsCrewActionSelectable("Commander", out isCommanderOrderGiven))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsOrdersGiven():  IsCrewActionSelectable(Commander) returned false");
               isOrdersGiven = false;
               return false;
            }
         }
         //-----------------------------------------------
         if ((true == isAssistantOrderGiven) && (true == isGunnerOrderGiven) && (true == isCommanderOrderGiven))
            isOrdersGiven = true;
         else
            isOrdersGiven = false;
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
         //---------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer.SetButtonState(): lastReport=null");
            return false;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //---------------------------------------------------
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
                  if ((false == SetButtonAirStrike(gi, lastReport, b)))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EventViewer.SetButtonState(): SetButtonStateEnemyStrength() returned false");
                     return false;
                  }
               }
               break;
            case "e032a":
               if ("Resupply" == content)
               {
                  if( Utilities.NO_RESULT == gi.DieResults[key][0])
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               break;
            case "e054":
               bool isSubMgFire = false;
               bool isAAFire = false;
               bool isCoaxialMgFire = false;
               bool isBowMgFire = false;
               foreach (IMapItem crewAction in gi.CrewActions)
               {
                  if ( ("Loader_FireSubMg" == crewAction.Name) && (false == gi.IsShermanFiredSubMg) )  // can only fire MG once per round
                     isSubMgFire = true;
                  if ( ("Loader_FireAaMg" == crewAction.Name)  && (false == gi.IsShermanFiredAaMg) )
                     isAAFire = true;
                  if ( ("Gunner_FireCoaxialMg" == crewAction.Name) && (false == gi.IsShermanFiredCoaxialMg) )
                     isCoaxialMgFire = true;
                  if ( ("Assistant_FireBowMg" == crewAction.Name) && (false == gi.IsShermanFiredBowMg) && (false == gi.Sherman.IsHullDown) )
                     isBowMgFire = true;
                  if ( ("Commander_FireSubMg" == crewAction.Name) && (false == gi.IsShermanFiredSubMg) )
                     isSubMgFire = true;
                  if ( ("Commander_FireAaMg" == crewAction.Name) && (false == gi.IsShermanFiredAaMg) )
                     isAAFire = true;
               }
               if ("  AA MG   " == content )
                   b.IsEnabled = isAAFire;
               else if ("  Bow MG  " == content)
                  b.IsEnabled = isBowMgFire;
               else if ("Coaxial MG" == content)
                  b.IsEnabled = isCoaxialMgFire;
               else if ("  Sub MG  " == content)
                  b.IsEnabled = isSubMgFire;
               break;
            case "e059":
               string name = b.Name;
               if( true == String.IsNullOrEmpty(name))
               {
                  b.IsEnabled = true;
                  return true;
               }
               int readyRackLoadCount = 0;
               int ammoAvailable = 0;
               string gunLoadType = gi.GetGunLoadType();
               int totalReloadLoad = gi.GetReadyRackTotalLoad();
               switch (name)
               {
                  case "HeMinus":
                     readyRackLoadCount = gi.GetReadyRackReload("He");
                     if (0 < readyRackLoadCount)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                     break;
                  case "ApMinus":
                     readyRackLoadCount = gi.GetReadyRackReload("Ap");
                     if (0 < readyRackLoadCount)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                     break;
                  case "WpMinus":
                     readyRackLoadCount = gi.GetReadyRackReload("Wp");
                     if (0 < readyRackLoadCount)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                     break;
                  case "HbciMinus":
                     readyRackLoadCount = gi.GetReadyRackReload("Hbci");
                     if (0 < readyRackLoadCount)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                     break;
                  case "HvapMinus":
                     readyRackLoadCount = gi.GetReadyRackReload("Hvap");
                     if (0 < readyRackLoadCount)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                     break;
                  case "HePlus":
                     readyRackLoadCount = gi.GetReadyRackReload("He");
                     ammoAvailable = lastReport.MainGunHE - readyRackLoadCount;
                     if ("He" == gunLoadType) // subtract one if a round is int the gun tube
                        ammoAvailable--;
                     if( (0 < ammoAvailable) && (totalReloadLoad < card.myMaxReadyRackCount) )
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                     break;
                  case "ApPlus":
                     readyRackLoadCount = gi.GetReadyRackReload("Ap");
                     ammoAvailable = lastReport.MainGunAP - readyRackLoadCount;
                     if ("Ap" == gunLoadType) // subtract one if a round is int the gun tube
                        ammoAvailable--;
                     if ((0 < ammoAvailable) && (totalReloadLoad < card.myMaxReadyRackCount))
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                     break;
                  case "WpPlus":
                     readyRackLoadCount = gi.GetReadyRackReload("Wp");
                     ammoAvailable = lastReport.MainGunWP - readyRackLoadCount;
                     if ("Wp" == gunLoadType) // subtract one if a round is int the gun tube
                        ammoAvailable--;
                     if ((0 < ammoAvailable) && (totalReloadLoad < card.myMaxReadyRackCount))
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                     break;
                  case "HbciPlus":
                     readyRackLoadCount = gi.GetReadyRackReload("Hbci");
                     ammoAvailable = lastReport.MainGunHBCI - readyRackLoadCount;
                     if ("Hbci" == gunLoadType) // subtract one if a round is int the gun tube
                        ammoAvailable--;
                     if ((0 < ammoAvailable) && (totalReloadLoad < card.myMaxReadyRackCount))
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                     break;
                  case "HvapPlus":
                     readyRackLoadCount = gi.GetReadyRackReload("Hvap");
                     ammoAvailable = lastReport.MainGunHVAP - readyRackLoadCount;
                     if ("Hvap" == gunLoadType) // subtract one if a round is int the gun tube
                        ammoAvailable--;
                     if ((0 < ammoAvailable) && (totalReloadLoad < card.myMaxReadyRackCount))
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                     break;
                  default:
                     break;
               }
               break;
            default:
               break;
         }
         b.Click += Button_Click;
         return true;
      }
      private bool SetButtonAirStrike(IGameInstance gi, IAfterActionReport lastReport, Button b)
      {
         if( (true == lastReport.Weather.Contains("Overcast")) || (true == lastReport.Weather.Contains("Fog")) || (true == lastReport.Weather.Contains("Falling")) || (true == gi.IsAirStrikePending))
            b.IsEnabled = false;
         else
            b.IsEnabled = true;
         return true;
      }
      private bool SetCheckboxState(IGameInstance gi, string key, CheckBox cb)
      {
         Logger.Log(LogEnum.LE_VIEW_CONTROL_NAME, "Set_CheckboxState(): cb.Name=" + cb.Name + " for ae=" + gi.EventActive);
         //------------------------------------
         Option option = gi.Options.Find(cb.Name);
         if (true == option.IsEnabled)
            cb.IsChecked = true;
         //------------------------------------
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Set_CheckboxState(): myGameInstance=null");
            return false;
         }
         Logger.Log(LogEnum.LE_SHOW_AUTOSETUP_BATTLEPREP, "SetCheckboxState(): isSetupPerformed=" + gi.BattlePrep.myIsSetupPerformed.ToString() + " for key=" + key);
         if (("e011" == key) && (false == myGameInstance.BattlePrep.myIsSetupPerformed))
         {
            cb.Visibility = Visibility.Hidden;
            option.IsEnabled = false;
            cb.IsChecked = false;
         }
         else
         {
            cb.Visibility = Visibility.Visible;
         }
         //------------------------------------
         cb.Checked += CheckBox_Checked;
         cb.Unchecked += CheckBox_Unchecked;
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
      private bool ContinueAfterBattlePrep(IGameInstance gi, ref GameAction outAction)
      {
         //---------------------------------------------------
         IAfterActionReport? lastReport = gi.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "Continue_AfterBattlePrep(): lastReport=null");
            return false;
         }
         TankCard card = new TankCard(lastReport.TankCardNum);
         //-----------------------------------------------------
         bool isMapStartAreaExist = false;
         foreach (IStack stack in gi.MoveStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.Name.Contains("StartArea"))
               {
                  isMapStartAreaExist = true;
                  break;
               }
            }
         }
         //-----------------------------------------------------
         bool isMapExitAreaExist = false;
         foreach (IStack stack in gi.MoveStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.Name.Contains("ExitArea"))
               {
                  isMapExitAreaExist = true;
                  break;
               }
            }
         }
         //-----------------------------------------------------
         if (EnumScenario.Counterattack == lastReport.Scenario)
         {
            bool isStartAreaReached = false;
            if (true == isMapStartAreaExist)
            {
               if (false == gi.IsTaskForceInStartArea(out isStartAreaReached))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Continue_AfterBattlePrep(): IsTaskForceInStartArea() returned false");
                  return false;
               }
               if (true == isStartAreaReached)
                  outAction = GameAction.MovementStartAreaRestartAfterBattle;
               else
                  outAction = GameAction.MovementEnemyCheckCounterattack;
            }
            else
            {
               outAction = GameAction.MovementStartAreaSet; // Setting the first start area
            }
         }
         else
         {
            bool isExitAreaReached = false;
            if (true == isMapExitAreaExist)
            {
               if (false == gi.IsTaskForceInExitArea(out isExitAreaReached))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Continue_AfterBattlePrep(): IsTaskForceInExitArea() returned false");
                  return false;
               }
            }
            if (true == isExitAreaReached)
               outAction = GameAction.MovementStartAreaRestartAfterBattle;
            else if (false == isMapStartAreaExist)
               outAction = GameAction.MovementStartAreaSet; // Setting the first start area
            else
            {
               outAction = GameAction.MovementEnemyStrengthChoice;  // Continue_AfterBattlePrep(): "Continue 017" - Preparations Final
               if (false == gi.IsShermanAdvancingOnMoveBoard) // If sherman advancing on BattleBoard, there will be enemy units in same area of move board until Sherman moves out
               {
                  bool isEnemyInMoveArea;
                  if (false == gi.IsTaskForceInEnemyArea(out isEnemyInMoveArea))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "Continue_AfterBattlePrep(): IsTaskForceInEnemyArea() returned false");
                     return false;
                  }
                  if (true == isEnemyInMoveArea)
                     outAction = GameAction.BattleActivation;  // Continue_AfterBattlePrep() 
               }
            }
         }
         return true;
      }
      //--------------------------------------------------------------------
      public void CloseAfterActionDialog()
      {
         myAfterActionDialog = null;
      }
      public void CloseMovementDialog()
      {
         myDialogMovementDiagram = null;
      }
      public void CloseEventListingDialog()
      {
         myDialogEventListing = null;
      }
      public void CloseTableListingDialog()
      {
         myDialogTableListing = null;
      }
      public void CloseRuleListingDialog()
      {
         myDialogRuleListing = null;
      }
      public void CloseReportErrorDialog()
      {
         myReportErrorDialog = null;
      }
      public void CloseShowAboutDialog()
      {
         myDialogAbout = null;
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
            Logger.Log(LogEnum.LE_ERROR, "Show_CrewRatingResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Show_CrewRatingResults(): myGameEngine=null");
            return false;
         }
         GameAction outAction = GameAction.Error;
         if( GamePhase.GameSetup == myGameInstance.GamePhase )
         {
            Option option = myGameInstance.Options.Find("SingleDayScenario");
            if( true == option.IsEnabled )
               outAction = GameAction.SetupShowSingleDayBattleStart;
            else
               outAction = GameAction.SetupShowCombatCalendarCheck;
         }
         else if( GamePhase.MorningBriefing == myGameInstance.GamePhase )
         {
            outAction = GameAction.MorningBriefingDayOfRest;
         }
         else if( 0 < myGameInstance.ShermanAdvanceOrRetreatEnemies.Count)
         {
            outAction = GameAction.BattleRoundSequenceCrewReplaced; // enemies transfer to Move board due to advancing or retreating Sherman
         }
         else
         {
            outAction = GameAction.BattleCrewReplaced;
         }
         StringBuilder sb11 = new StringBuilder("     ######Show_CrewRatingResults() :");
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
         {
            outAction = GameAction.MorningBriefingTimeCheck;
         }
         else
         {
            IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
            if (null == lastReport)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetupFireResults(): lastReport=null");
               return false;
            }
            if( EnumScenario.Counterattack == lastReport.Scenario )
               outAction = GameAction.MovementEnemyCheckCounterattack;
            else
               outAction = GameAction.MovementChooseOption;
         }
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
            Logger.Log(LogEnum.LE_ERROR, "Show_BattleSetupResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Show_BattleSetupResults(): myGameEngine=null");
            return false;
         }
         //--------------------------------------------------
         GameAction outAction = GameAction.Error;
         if ( BattlePhase.AmbushRandomEvent == myGameInstance.BattlePhase ) // Friendly Action Result during Ambush phase
         {
            Logger.Log(LogEnum.LE_SHOW_BATTLE_ROUND_START, "Show_BattleSetupResults(): AmbushRandomEvent e=" + myGameInstance.EventActive);
            outAction = GameAction.BattleRoundSequenceStart; // Show_BattleSetupResults() - AmbushRandomEvent
         }
         else if (BattlePhase.RandomEvent == myGameInstance.BattlePhase) // Friendly Action Result in BattleRoundSequence phase
         {
            outAction = GameAction.BattleRoundSequenceBackToSpotting; // Show_BattleSetupResults - FriendlyAction
         }
         else                                                               // Battle Setup phase
         {
            IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
            if (null == lastReport)
            {
               Logger.Log(LogEnum.LE_ERROR, "Show_BattleSetupResults(): lastReport=null");
               return false;
            }
            if (null == myGameInstance.EnteredArea)
            {
               Logger.Log(LogEnum.LE_ERROR, "Show_BattleSetupResults(): myGameInstance.EnteredArea=null");
               return false;
            }
            IStack? stackEnteredArea = myGameInstance.MoveStacks.Find(myGameInstance.EnteredArea);
            if (null == stackEnteredArea)
            {
               Logger.Log(LogEnum.LE_ERROR, "Show_BattleSetupResults(): stackEnteredArea=null");
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
            outAction = GameAction.BattleAmbushStart;
            int enemyCount = 0;
            IMapItems removals = new MapItems();
            foreach (IStack stack in myGameInstance.BattleStacks)
            {
               Logger.Log(LogEnum.LE_VIEW_ADV_FIRE_RESOLVE, "Show_BattleSetupResults(): stack=" + stack.ToString());
               bool isEnemyUnitInTerritory = false;
               IMapItem? advanceFireMarker = null;
               foreach (IMapItem mi in stack.MapItems)
               {
                  Logger.Log(LogEnum.LE_VIEW_ADV_FIRE_RESOLVE, "Show_BattleSetupResults(): mi=" + mi.Name);
                  if (true == mi.IsEnemyUnit())
                  {
                     ++enemyCount;
                     Logger.Log(LogEnum.LE_VIEW_ADV_FIRE_RESOLVE, "Show_BattleSetupResults(): c=" + enemyCount);
                     isEnemyUnitInTerritory = true;
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
               else if ( (true == isAirStrike) && (false == lastReport.Weather.Contains("Fog")) && (false == lastReport.Weather.Contains("Falling")) )
                  outAction = GameAction.BattleResolveAirStrike;
            }
            //--------------------------------------------------
            if (0 == enemyCount)
               outAction = GameAction.BattleEmpty;   // Show_BattleSetupResults()
            foreach (IMapItem mi in removals)
               myGameInstance.BattleStacks.Remove(mi);
         }
         //--------------------------------------------------
         StringBuilder sb11 = new StringBuilder("     ######Show_BattleSetupResults() :");
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
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetup_FireResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetup_FireResults(): myGameEngine=null");
            return false;
         }
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetup_FireResults(): lastReport=null");
            return false;
         }
         //--------------------------------------------------
         bool isAirStrike = false;
         bool isArtilleryStrike = false;
         GameAction outAction = GameAction.Error;
         if( BattlePhase.RandomEvent == myGameInstance.BattlePhase )
         {
            outAction = GameAction.BattleRoundSequenceBackToSpotting; // ShowBattleSetup_FireResults() - AmbushRandomEvent 
         }
         else if (BattlePhase.FriendlyAction == myGameInstance.BattlePhase)  // Friendly action during counterattack ambush 
         {
            myGameInstance.GamePhase = GamePhase.Battle;
            outAction = GameAction.BattleRandomEvent; // ShowBattleSetup_FireResults() - AmbushRandomEvent 
         }
         else if (BattlePhase.AmbushRandomEvent == myGameInstance.BattlePhase)  // Friendly action during ambush as part of Random Events
         {
            Logger.Log(LogEnum.LE_SHOW_BATTLE_ROUND_START, "ShowBattleSetup_FireResults(): AmbushRandomEvent e=" + myGameInstance.EventActive);
            outAction = GameAction.BattleRoundSequenceStart; // ShowBattleSetup_FireResults() - AmbushRandomEvent 
         }
         else if (GamePhase.Battle == myGameInstance.GamePhase)
         {
            if (null == myGameInstance.EnteredArea)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetup_FireResults(): myGameInstance.EnteredArea=null");
               return false;
            }
            IStack? stackEnteredArea = myGameInstance.MoveStacks.Find(myGameInstance.EnteredArea);
            if (null == stackEnteredArea)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetup_FireResults(): stackEnteredArea=null");
               return false;
            }
            foreach (IMapItem mi in stackEnteredArea.MapItems)
            {
               if (true == mi.Name.Contains("Air"))
                  isAirStrike = true;
               if (true == mi.Name.Contains("Artillery"))
                  isArtilleryStrike = true;
            }
            //--------------------------------------------------
            bool isEnemyUnitAvailableForAirStrike = false;
            foreach (IStack stack in myGameInstance.BattleStacks)
            {
               foreach (IMapItem mi in stack.MapItems)
               {
                  if (true == mi.IsEnemyUnit())
                  {
                     if (((false == lastReport.Weather.Contains("Fog")) && (false == lastReport.Weather.Contains("Falling"))) || (("B6M" != stack.Territory.Name) && ("B6L" != stack.Territory.Name)))
                     {
                        isEnemyUnitAvailableForAirStrike = true;
                        break;
                     }
                  }
               }
            }
            if (true == isArtilleryStrike)
               outAction = GameAction.BattleResolveArtilleryFire;
            else if ((true == isAirStrike) && (true == isEnemyUnitAvailableForAirStrike))
               outAction = GameAction.BattleResolveAirStrike;
            else
               outAction = GameAction.BattleAmbushStart;
         }
         else if (GamePhase.Battle == myGameInstance.GamePhase) // Advancing Fire Finished
         {
            outAction = GameAction.BattleAmbushStart;
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowBattleSetup_FireResults(): Reached default GP=" + myGameInstance.GamePhase.ToString() + " BP=" + myGameInstance.BattlePhase.ToString());
            return false;
         }
         //--------------------------------------------------
         bool isBattleBoardEmpty = true;
         foreach (IStack stack in myGameInstance.BattleStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.IsEnemyUnit())
               {
                  isBattleBoardEmpty = false;
                  break;
               }
            }
         }
         if ( true == isBattleBoardEmpty)
            outAction = GameAction.BattleEmpty;  // ShowBattleSetup_FireResults()
         //--------------------------------------------------
         StringBuilder sb11 = new StringBuilder("     ######ShowBattleSetup_FireResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         sb11.Append(" bp="); sb11.Append(myGameInstance.BattlePhase.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowAmbushResults()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowAmbushResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowAmbushResults(): myGameEngine=null");
            return false;
         }
         GameAction outAction = GameAction.Error;
         //------------------------------------------
         if ( 0 < myGameInstance.NumCollateralDamage )
         {
            outAction = GameAction.BattleCollateralDamageCheck;
         }
         else if (null != myGameInstance.Death)
         {
            outAction = GameAction.BattleShermanKilled;
         }
         else
         {
            outAction = GameAction.BattleEmpty;  // Show_AmbushResults()
            foreach (IStack stack in myGameInstance.BattleStacks)
            {
               foreach (IMapItem mi in stack.MapItems)
               {
                  if (true == mi.IsEnemyUnit())
                  {
                     outAction = GameAction.BattleRandomEvent;
                     break;
                  }
               }
            }
         }
         //--------------------------------------------------
         StringBuilder sb11 = new StringBuilder("     ######ShowAmbushResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowCollateralDamageResults()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "Show_CollateralDamageResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "Show_CollateralDamageResults(): myGameEngine=null");
            return false;
         }
         //-----------------------------------------
         GameAction outAction = GameAction.Error;
         if (BattlePhase.Ambush == myGameInstance.BattlePhase)
         {
            outAction = GameAction.BattleRandomEvent;
         }
         else if (BattlePhase.AmbushRandomEvent == myGameInstance.BattlePhase)
         {
            Logger.Log(LogEnum.LE_SHOW_BATTLE_ROUND_START, "Show_CollateralDamageResults(): AmbushRandomEvent e=" + myGameInstance.EventActive);
            outAction = GameAction.BattleRoundSequenceStart; // Show_CollateralDamageResults() - BattlePhase.AmbushRandomEvent
         }
         else if (BattlePhase.EnemyAction == myGameInstance.BattlePhase)
         {
            outAction = GameAction.BattleRoundSequenceFriendlyAction; // Show_CollateralDamageResults()- BattlePhase.BattleRoundSequenceFriendlyAction
         }
         else if (BattlePhase.RandomEvent == myGameInstance.BattlePhase)
         {
            outAction = GameAction.BattleRoundSequenceBackToSpotting; // Show_CollateralDamageResults() - BattlePhase.AmbushRandomEvent
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowCollateralDamageResults(): reached default BattlePhase=" + myGameInstance.BattlePhase.ToString());
            return false;
         }
         //-----------------------------------------
         bool isBattleBoardEmpty = true;
         foreach (IStack stack in myGameInstance.BattleStacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (true == mi.IsEnemyUnit())
               {
                  isBattleBoardEmpty = false;
                  break;
               }
            }
         }
         if (true == isBattleBoardEmpty)
            outAction = GameAction.BattleEmpty;  // Show_CollateralDamageResults()
         //--------------------------------------------------
         StringBuilder sb11 = new StringBuilder("     ######Show_CollateralDamageResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowSpottingResults()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSpottingResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSpottingResults(): myGameEngine=null");
            return false;
         }
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowSpottingResults():  myGameInstance.Reports.GetLast()");
            return false;
         }
         //------------------------------------------------------------------------
         GameAction outAction = GameAction.Error;
         outAction = GameAction.BattleRoundSequenceSpottingEnd;
         //--------------------------------------------------
         StringBuilder sb11 = new StringBuilder("     ######ShowSpottingResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }     
      public bool ShowTankDestroyedResults()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTankDestroyedResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTankDestroyedResults(): myGameEngine=null");
            return false;
         }
         GameAction outAction = GameAction.Error;
         outAction = GameAction.EveningDebriefingStart;
         StringBuilder sb11 = new StringBuilder("     ######ShowTankDestroyedResults() :");
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
         GameAction outAction = GameAction.EveningDebriefingRatingImprovementEnd;
         if( GamePhase.MorningBriefing == myGameInstance.GamePhase )
            outAction = GameAction.EveningDebriefingRatingTrainingEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowRatingImproveResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowFacingChangeResults()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowFacingChangeResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowFacingChangeResults(): myGameEngine=null");
            return false;
         }
         GameAction outAction = GameAction.BattleRoundSequenceChangeFacingEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowFacingChangeResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowEnemyActionResults()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowEnemyActionResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowEnemyActionResults(): myGameEngine=null");
            return false;
         }
         GameAction outAction = GameAction.Error;
         //------------------------------------------
         if (0 < myGameInstance.NumCollateralDamage)
         {
            outAction = GameAction.BattleCollateralDamageCheck;
         }
         else if (null != myGameInstance.Death)
         {
            outAction = GameAction.BattleShermanKilled;
         }
         else
         {
            if( BattlePhase.EnemyAction == myGameInstance.BattlePhase )
            {
               outAction = GameAction.BattleRoundSequenceRandomEvent; // random event if no more enemy units
               foreach (IStack stack in myGameInstance.BattleStacks)
               {
                  foreach (IMapItem mi in stack.MapItems)
                  {
                     if (true == mi.IsEnemyUnit())
                     {
                        outAction = GameAction.BattleRoundSequenceFriendlyAction;
                        break;
                     }
                  }
               }
            }
            else
            {
               outAction = GameAction.BattleRandomEvent; // coming out of ambush is random event
            }
         }
         //--------------------------------------------------
         StringBuilder sb11 = new StringBuilder("     ######ShowEnemyActionResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowFriendlyActionResults()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowFriendlyActionResults(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowFriendlyActionResults(): myGameEngine=null");
            return false;
         }
         GameAction outAction = GameAction.Error;
         //------------------------------------------
         if (0 < myGameInstance.NumCollateralDamage)
         {
            outAction = GameAction.BattleCollateralDamageCheck;
         }
         else if (null != myGameInstance.Death)
         {
            outAction = GameAction.BattleShermanKilled;
         }
         else
         {
            outAction = GameAction.BattleRoundSequenceRandomEvent;
         }
         //--------------------------------------------------
         StringBuilder sb11 = new StringBuilder("     ######ShowEnemyActionResults() :");
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
         IAfterActionReport? lastReport = myGameInstance.Reports.GetLast();
         if (null == lastReport)
         {
            Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown():  myGameInstance.Reports.GetLast()");
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
                        case "DoorClosing":
                           action = GameAction.EndGameExit;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
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
                        case "m01":
                           action = GameAction.SetupShowAfterActionReport;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "Continue005":
                           Option optionSingleDayScenario = myGameInstance.Options.Find("SingleDayScenario");
                           if( true == optionSingleDayScenario.IsEnabled )
                              action = GameAction.SetupSingleGameDay;
                           else
                              action = GameAction.SetupAssignCrewRating;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "Continue005a":
                           action = GameAction.SetupAssignCrewRating;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "GotoMorningBriefing":
                           action = GameAction.MorningBriefingBegin;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "Ambulance3": // Morning Briefing
                           if (EnumScenario.Retrofit == lastReport.Scenario)
                           {
                              myGameInstance.EventDisplayed = myGameInstance.EventActive = "e006a";       
                              myGameInstance.DieRollAction = GameAction.DieRollActionNone;   
                           }
                           else
                           {
                              myGameInstance.EventDisplayed = myGameInstance.EventActive = "e006";                  
                              myGameInstance.DieRollAction = GameAction.MorningBriefingCalendarRoll;
                           }
                           action = GameAction.UpdateEventViewerActive;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "HealCrewman":
                           action = GameAction.MorningBriefingCrewmanHealing;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "ExistingCrewman":
                           action = GameAction.MorningBriefingExistingCrewman;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "ReturningCrewman":
                           action = GameAction.MorningBriefingReturningCrewman;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "MorningBriefingTankReplaceChoice":
                           action = GameAction.MorningBriefingTankReplaceChoice;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "MorningBriefingTankKeepChoice":
                           action = GameAction.MorningBriefingTankKeepChoice;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "c75Hvss": // trained on HVSS
                           action = GameAction.MorningBriefingTrainCrewHvssEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "TankReplacement":
                           action = GameAction.MorningBriefingTankReplacementRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "MorningBriefingTankReplacementEnd":
                           action = GameAction.MorningBriefingTankReplacementEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue06b":
                           action = GameAction.MorningBriefingTrainCrew;
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
                           Option optionAutoRollAmmoLoad = myGameInstance.Options.Find("AutoRollAmmoLoad");
                           if(true == optionAutoRollAmmoLoad.IsEnabled)
                              action = GameAction.MorningBriefingAmmoLoadSkip;
                           else
                              action = GameAction.MorningBriefingAmmoLoad;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "GotoMorningBriefingDayOfRest":
                           action = GameAction.MorningBriefingDayOfRest;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MorningBriefingDeployment":
                           action = GameAction.MorningBriefingDeployment;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MorningBriefingHvssSet":
                           action = GameAction.MorningBriefingTankReplacementHvssRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MorningBriefingDeploymentEnd":
                           Option optionAutoPreparation = myGameInstance.Options.Find("AutoPreparation");
                           if( true == optionAutoPreparation.IsEnabled)
                           {
                              myGameInstance.GamePhase = GamePhase.Movement;
                              action = GameAction.PreparationsFinalSkip; // user skips battle prep steps and uses previous setup
                           }
                           else
                           {
                              action = GameAction.PreparationsHatches;
                           }
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "c15OpenHatch":
                           action = GameAction.PreparationsGunLoad;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "c17GunLoad":
                        case "Continue13a":
                           action = GameAction.PreparationsTurret;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "c16TurretSherman75":
                           if( BattlePhase.ConductCrewAction == myGameInstance.BattlePhase)
                              action = GameAction.BattleRoundSequenceTurretEnd;
                           else
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
                        case "Continue017": // Preparations Final
                           if(false == ContinueAfterBattlePrep(myGameInstance, ref action))
                           {
                              Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): ContinueAfterBattlePrep() returned false");
                              return;
                           }
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "Sherman1":
                           if( "e099" == myGameInstance.EventDisplayed)
                           {
                              action = GameAction.BattleRoundSequenceShermanAdvanceOrRetreatEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           }
                           if ("e099a" == myGameInstance.EventDisplayed)
                           {
                              action = GameAction.BattleActivation;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           }
                           break;
                        case "MovementExitAreaSet":
                           action = GameAction.MovementExitAreaSet;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MovementEnemyCheckCounterattack":
                           action = GameAction.MovementEnemyCheckCounterattack;
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
                              action = GameAction.MovementEnemyStrengthChoice;   // TextBlock_MouseDown(): "MovementEnemyStrengthChoice"
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "MovementChooseOption":
                           action = GameAction.MovementChooseOption;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "Resupply":
                           action = GameAction.MovementResupplyCheckRoll;
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
                           if( true == myGameInstance.IsAdvancingFireChosen) 
                              action = GameAction.BattleStart;
                           else
                              action = GameAction.BattleActivation;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "MovementBattleActivation":
                           action = GameAction.MovementBattleActivation;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue32a":  // Counterattack Check
                           action = GameAction.MovementBattleCheckCounterattackRoll;  
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Ambush":
                           if( EnumScenario.Counterattack == lastReport.Scenario )
                           {
                              myGameInstance.GamePhase = GamePhase.BattleRoundSequence; // perform one round of attack before enemy does anything
                              action = GameAction.BattleRoundSequenceAmbushCounterattack;
                           }
                           else
                           {
                              action = GameAction.BattleAmbush; // results in Enemy Activation
                           }
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue35":  // Ambush Check
                           Logger.Log(LogEnum.LE_SHOW_BATTLE_ROUND_START, "TextBlock_MouseDown(): Ambush Check e=" + myGameInstance.EventActive);
                           action = GameAction.BattleRoundSequenceStart;  // Ambush Check 
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "MovementCounterattackEllapsedTimeRoll":
                           action = GameAction.MovementCounterattackEllapsedTimeRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue43":  // minefield attack
                           if( BattlePhase.AmbushRandomEvent == myGameInstance.BattlePhase)
                           {
                              Logger.Log(LogEnum.LE_SHOW_BATTLE_ROUND_START, "TextBlock_MouseDown(): Minefield Attack e=" + myGameInstance.EventActive);
                              action = GameAction.BattleRoundSequenceStart;           // Minefield Attack
                           }
                           else
                           {
                              action = GameAction.BattleRoundSequenceBackToSpotting;  // Minefield Attack
                           }
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "DebriefStart":
                           action = GameAction.EveningDebriefingStart;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "Debrief":
                           action = GameAction.EveningDebriefingRatingImprovement;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           return;
                        case "c108Smoke1": // smoke depletion
                           action = GameAction.BattleRoundSequenceSmokeDepletionEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue36":
                           action = GameAction.BattleEmptyResolve;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue38":
                           bool isLoaderLoading = true;
                           foreach (IMapItem crewAction in myGameInstance.CrewActions)
                           {
                              if (true == crewAction.Name.Contains("Loader_Load"))
                              {
                                 isLoaderLoading = true;
                                 break;
                              }
                              if (true == crewAction.Name.Contains("Loader")) // if loader is doing anything other than loading, do not load gun
                              {
                                 isLoaderLoading = false;
                                 break;
                              }
                           }
                           bool isGunnerFiring = false;
                           foreach (IMapItem crewAction in myGameInstance.CrewActions)
                           {
                              if (true == crewAction.Name.Contains("Gunner_FireMainGun"))
                                 isGunnerFiring = true;
                              if (true == crewAction.Name.Contains("Gunner_RotateFireMainGun"))
                                 isGunnerFiring = true;
                           }
                           if ( (true == isLoaderLoading) && (true == isGunnerFiring) ) // do not show ammo orders screen if gunneris not firing or loader is not loading
                              action = GameAction.BattleRoundSequenceAmmoOrders;
                           else
                              action = GameAction.BattleRoundSequenceConductCrewAction; // skip ammo orders
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue39":
                           action = GameAction.BattleRandomEventRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "MilitaryWatch":
                           if (BattlePhase.AmbushRandomEvent == myGameInstance.BattlePhase)
                           {
                              Logger.Log(LogEnum.LE_SHOW_BATTLE_ROUND_START, "TextBlock_MouseDown(): Military Watch e=" + myGameInstance.EventActive);
                              action = GameAction.BattleRoundSequenceStart;          // Military Watch
                           }
                           else
                           {
                              action = GameAction.BattleRoundSequenceNextActionAfterRandomEvent; // Military Watch
                           }
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "EnemyArtilleryEnd":
                           action = GameAction.BattleRoundSequenceEnemyArtilleryRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "MineFieldAttackEnd":
                           action = GameAction.BattleRoundSequenceMinefieldRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "MineFieldAttackDisableRollEnd":
                           action = GameAction.BattleRoundSequenceMinefieldDisableRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "PanzerfaultSector":
                           action = GameAction.BattleRoundSequencePanzerfaustSectorRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "PanzerfaultAttack":
                           action = GameAction.BattleRoundSequencePanzerfaustAttackRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "PanzerfaultToHit":
                           action = GameAction.BattleRoundSequencePanzerfaustToHitRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "PanzerfaultToKill":
                           action = GameAction.BattleRoundSequencePanzerfaustToKillRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "CollateralDamage":
                           action = GameAction.BattleRoundSequenceHarrassingFire;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue046a": // Friendly Advance Ignored
                        case "Continue047":  // Enemy Reinforcements
                           if( BattlePhase.AmbushRandomEvent == myGameInstance.BattlePhase)
                           {
                              Logger.Log(LogEnum.LE_SHOW_BATTLE_ROUND_START, "TextBlock_MouseDown(): Frendly Advance Ignored | Enemy Reinforcements e=" + myGameInstance.EventActive);
                              action = GameAction.BattleRoundSequenceStart;           // Friendly Advance Ignored | Enemy Reinforcements 
                           }
                           else
                           {
                              action = GameAction.BattleRoundSequenceBackToSpotting; // Friendly Advance Ignored | Enemy Reinforcements 
                           }
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue50":
                        case "Continue50c":
                        case "Continue50d":
                           action = GameAction.BattleRoundSequenceConductCrewAction;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue50a":
                           action = GameAction.BattleRoundSequenceLoadMainGunEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue50b":
                           action = GameAction.BattleRoundSequenceStart;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue51":
                           action = GameAction.BattleRoundSequenceMovementRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue51a":
                           action = GameAction.BattleRoundSequenceBoggedDownRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "ShermanPivot":
                           action = GameAction.BattleRoundSequenceMovementPivotEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue53a":
                           action = GameAction.BattleRoundSequenceShermanToHitRollNothing;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue53b":
                           if( 0 < myGameInstance.ShermanHits.Count )
                              action = GameAction.BattleRoundSequenceShermanSkipRateOfFire; // If sherman misses, do same thing as skip ROF if there are previous hits
                           else
                              action = GameAction.BattleRoundSequenceShermanFiringMainGunEnd; // Miss target after hitting
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "BattleRoundSequenceShermanHit":
                           action = GameAction.BattleRoundSequenceShermanToHitRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Miss":
                           if (0 < myGameInstance.ShermanHits.Count)
                              action = GameAction.BattleRoundSequenceShermanToKillRollMiss; // If sherman misses, still need to clean up from firing main gun
                           else
                              action = GameAction.BattleRoundSequenceShermanFiringMainGunEnd; // Miss target
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "ThrownTrack":
                           action = GameAction.BattleRoundSequenceShermanToKillRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue53d":
                           action = GameAction.BattleRoundSequenceShermanToKillRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue53e":
                           action = GameAction.BattleRoundSequenceShermanToKillRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue53f":
                           action = GameAction.BattleRoundSequenceShermanToKillRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue54a":
                           action = GameAction.BattleRoundSequenceFireMachineGunRollEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue54b":
                           action = GameAction.BattleRoundSequencePlaceAdvanceFireRollEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "BrokenPeriscope":
                           action = GameAction.BattleRoundSequenceReplacePeriscopes;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue56":
                           action = GameAction.BattleRoundSequenceRepairMainGunRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue56a":
                           action = GameAction.BattleRoundSequenceRepairAaMgRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue56b":
                           action = GameAction.BattleRoundSequenceRepairBowMgRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue56c":
                           action = GameAction.BattleRoundSequenceRepairCoaxialMgRoll;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "c58LFireMortar":
                           action = GameAction.BattleRoundSequenceShermanFiringMortar;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "c70ThrowSmokeGrenade":
                           action = GameAction.BattleRoundSequenceShermanThrowGrenade;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "c60LRestockReadyRack":
                           action = GameAction.BattleRoundSequenceReadyRackEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue60":
                           action = GameAction.BattleRoundSequenceStart;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "CarryingMan":
                           action = GameAction.BattleRoundSequenceCrewSwitchEnd;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Ambulance1":  // Resolve_EmptyBattleBoard() - Battle Ends
                           action = GameAction.MorningBriefingAssignCrewRating;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Ambulance2":  // MoveShermanAdvanceOrRetreat() - Advance or Retreat
                           action = GameAction.MorningBriefingAssignCrewRating;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "CampaignOver":
                           action = GameAction.EveningDebriefingVictoryPointsCalculated;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "EventDebriefVictoryPts":
                           action = GameAction.EveningDebriefingVictoryPointsCalculated;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "EventDebriefPromotion":
                           action = GameAction.EventDebriefPromotion;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "Continue103":
                           action = GameAction.EventDebriefDecorationContinue;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "DecorationBronzeStar":
                           action = GameAction.EventDebriefDecorationBronzeStar;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "DecorationSilverStar":
                           action = GameAction.EventDebriefDecorationSilverStar;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "DecorationDistinguishedCross":
                           action = GameAction.EventDebriefDecorationCross;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "DecorationMedalOfHonor":
                           action = GameAction.EventDebriefDecorationHonor;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "DecorationPurpleHeart":
                           action = GameAction.EventDebriefDecorationHeart;
                           myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                           break;
                        case "EndGameShowStats":
                           GameFeat changedFeat;
                           Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Reset_RoundSetState(): \n  Feats=" + GameEngine.theInGameFeats.ToString() + " \n SFeats=" + GameEngine.theStartingFeats.ToString());
                           if (false == GameEngine.theInGameFeats.GetFeatChange(GameEngine.theStartingFeats, out changedFeat)) // End Game Feats and Stats
                           {
                              Logger.Log(LogEnum.LE_SHOW_BATTLE_ROUND_START, "TextBlock_MouseDown(): Get_FeatChange() returned false");
                              return;
                           }
                           if( true == String.IsNullOrEmpty(changedFeat.Key))
                           {
                              action = GameAction.EndGameShowStats;
                           }
                           else
                           {
                              Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "TextBlock_MouseDown(): Change=" + changedFeat.ToString());
                              action = GameAction.EndGameShowFeats;
                           }
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
         //----------------------------------------------------
         if ("DriverWounded" == b.Name)
         {
            action = GameAction.BattleRoundSequenceMinefieldDriverWoundRoll;
            myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
            return;
         }
         if ("AssistantWounded" == b.Name)
         {
            action = GameAction.BattleRoundSequenceMinefieldAssistantWoundRoll;
            myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
            return;
         }
         //----------------------------------------------------
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
            case "   -   ":
               switch (name)
               {
                  case "ButtonDecreaseDate":
                     action = GameAction.SetupDecreaseDate;
                     break;
                  case "ButtonDecreaseTankNum":
                     action = GameAction.MorningBriefingDecreaseTankNum;
                     break;
                  case "ButtonPivotHullLeft":
                     action = GameAction.BattleRoundSequencePivotLeft;
                     break;
                  case "ButtonPivotTurretLeft":
                     if (BattlePhase.ConductCrewAction == myGameInstance.BattlePhase)
                        action = GameAction.BattleRoundSequenceTurretEndRotateLeft;
                     else
                        action = GameAction.PreparationsTurretRotateLeft;
                     break;
                  case "HeMinus":
                     action = GameAction.BattleRoundSequenceReadyRackHeMinus;
                     break;
                  case "ApMinus":
                     action = GameAction.BattleRoundSequenceReadyRackApMinus;
                     break;
                  case "WpMinus":
                     action = GameAction.BattleRoundSequenceReadyRackWpMinus;
                     break;
                  case "HbciMinus":
                     action = GameAction.BattleRoundSequenceReadyRackHbciMinus;
                     break;
                  case "HvapMinus":
                     action = GameAction.BattleRoundSequenceReadyRackHvapMinus;
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): reached default Unknown name=" + name);
                     return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "   +   ":
               switch (name)
               {
                  case "ButtonIncreaseDate":
                     action = GameAction.SetupIncreaseDate;
                     break;
                  case "ButtonIncreaseTankNum":
                     action = GameAction.MorningBriefingIncreaseTankNum;
                     break;
                  case "ButtonPivotHullRight":
                     action = GameAction.BattleRoundSequencePivotRight;
                     break;
                  case "ButtonPivotTurretRight":
                     if (BattlePhase.ConductCrewAction == myGameInstance.BattlePhase)
                        action = GameAction.BattleRoundSequenceTurretEndRotateRight;
                     else
                        action = GameAction.PreparationsTurretRotateRight;
                     break;
                  case "HePlus":
                     action = GameAction.BattleRoundSequenceReadyRackHePlus;
                     break;
                  case "ApPlus":
                     action = GameAction.BattleRoundSequenceReadyRackApPlus;
                     break;
                  case "WpPlus":
                     action = GameAction.BattleRoundSequenceReadyRackWpPlus;
                     break;
                  case "HbciPlus":
                     action = GameAction.BattleRoundSequenceReadyRackHbciPlus;
                     break;
                  case "HvapPlus":
                     action = GameAction.BattleRoundSequenceReadyRackHvapPlus;
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): reached default Unknown name=" + name);
                     return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case " Area ":
               myGameInstance.ShermanTypeOfFire = "Area";
               action = GameAction.BattleRoundSequenceShermanFiringMainGun; // Area Button
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "  AA MG   ":
               action = GameAction.BattleRoundSequenceFireAaMg;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "  Bow MG  ":
               action = GameAction.BattleRoundSequenceFireBowMg;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Coaxial MG":
               action = GameAction.BattleRoundSequenceFireCoaxialMg;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "  Sub MG  ":
               action = GameAction.BattleRoundSequenceFireSubMg;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "   Skip   ":
               action = GameAction.BattleRoundSequenceFireMgSkip;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "AAR":
               if (null == myAfterActionDialog)
               {
                  AfterActionReportUserControl aarUserControl = new AfterActionReportUserControl(myGameInstance);
                  if (true == aarUserControl.CtorError)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): AfterActionReportUserControl CtorError=true");
                     return false;
                  }
                  myAfterActionDialog = new AfterActionDialog(myGameInstance, CloseAfterActionDialog);
                  myAfterActionDialog.Show();
               }
               break;
            case " Begin Campaign":
               myGameInstance.Options.SetValue("SingleDayScenario", false);
               if (false == ShowTutorialScreens())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ShowTutorialScreens() returned false");
                  return false;
               }
               break;
            case " Begin One Day ":
               myGameInstance.Options.SetValue("SingleDayScenario", true);
               if (false == ShowTutorialScreens())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ShowTutorialScreens() returned false");
                  return false;
               }
               break;
            case "Cancel":
               action = GameAction.MovementAirStrikeCancel;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Direct":
               myGameInstance.ShermanTypeOfFire = "Direct";
               action = GameAction.BattleRoundSequenceShermanFiringMainGun;  // Direct Buttn
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Enter":
               action = GameAction.MovementEnterArea;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Fire":
               action = GameAction.BattleRoundSequenceShermanFiringMainGun; // Fire Button
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "   Read Rules  ":
               if (false == ShowRule("r1.0"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): ShowRule(r1.1) returned false");
                  return false;
               }
               break;
            case "Resupply":
               action = GameAction.MovementResupplyCheck;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Skip":
               action = GameAction.BattleRoundSequenceShermanSkipRateOfFire; // Skip the Rate of Fire
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case " Skip ":
               myGameInstance.ShermanTypeOfFire = "Skip";
               action = GameAction.BattleRoundSequenceShermanFiringMainGunNot;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Strength Check":
               action = GameAction.MovementEnemyStrengthChoice;   // Button_ClickShowOther(): "Strength Check"
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
      private void CheckBox_Checked(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Checked(): myGameInstance=null");
            return;
         }
         CheckBox cb = (CheckBox)sender;
         e.Handled = true;
         Option option = myGameInstance.Options.Find(cb.Name);
         option.IsEnabled = true;
      }
      private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Unchecked(): myGameInstance=null");
            return;
         }
         CheckBox cb = (CheckBox)sender;
         e.Handled = true;
         Option option = myGameInstance.Options.Find(cb.Name);
         option.IsEnabled = false;
      }
      private void CheckBoxCmdrFire_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         if(null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxCmdFire_Checked(): myGameInstance=null");
            return;
         }
         myGameInstance.IsCommanderDirectingMgFire = true;
      }
      private void CheckBoxCmdrFire_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = false;
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxCmdFire_Unchecked(): myGameInstance=null");
            return;
         }
         myGameInstance.IsCommanderDirectingMgFire = false; //  EventViewer.CheckBoxCmdrFire_Unchecked()
      }
      private void CheckBoxImmobilization_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Immobilization_Checked(): myGameInstance=null");
            return;
         }
         myGameInstance.IsShermanDeliberateImmobilization = true;
         Logger.Log(LogEnum.LE_SHOW_IMMOBILIZATION, "CheckBoxImmobilization_Checked(): gi.IsShermanDeliberateImmobilization=" + myGameInstance.IsShermanDeliberateImmobilization.ToString());
         if (false == OpenEvent(myGameInstance, myGameInstance.EventActive))
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Immobilization_Checked(): OpenEvent() returned false ae=" + myGameInstance.EventActive );
      }
      private void CheckBoxImmobilization_Unchecked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = false;
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxCmdFire_Unchecked(): myGameInstance=null");
            return;
         }
         myGameInstance.IsShermanDeliberateImmobilization = false;
         Logger.Log(LogEnum.LE_SHOW_IMMOBILIZATION, "CheckBoxCmdFire_Unchecked(): gi.IsShermanDeliberateImmobilization=" + myGameInstance.IsShermanDeliberateImmobilization.ToString());
         if (false == OpenEvent(myGameInstance, myGameInstance.EventActive))
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxImmobilization_Checked(): OpenEvent() returned false ae=" + myGameInstance.EventActive );
      }
      private bool ShowTutorialScreens()
      {
         GameAction action = GameAction.SetupShowMapHistorical;
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTutorialScreens(): myGameInstance=null");
            return false;
         }
         if (null == myGameEngine)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTutorialScreens(): myGameEngine=null");
            return false;
         }
         Option option;
         //--------------------------
         {
            option = myGameInstance.Options.Find("SkipTutorial1");
            if (true == option.IsEnabled)
               action = GameAction.SetupShowMovementBoard;
         }
         if(GameAction.SetupShowMovementBoard == action)
         {
            option = myGameInstance.Options.Find("SkipTutorial2");
            if (true == option.IsEnabled)
               action = GameAction.SetupShowBattleBoard;
         }
         if (GameAction.SetupShowBattleBoard == action)
         {
            option = myGameInstance.Options.Find("SkipTutorial3");
            if (true == option.IsEnabled)
               action = GameAction.SetupShowTankCard;
         }
         if (GameAction.SetupShowTankCard == action)
         {
            option = myGameInstance.Options.Find("SkipTutorial4");
            if (true == option.IsEnabled)
               action = GameAction.SetupShowAfterActionReport;
         }
         if (GameAction.SetupShowAfterActionReport == action)
         {
            option = myGameInstance.Options.Find("SkipTutorial5");
            if (true == option.IsEnabled)
            {
               option = myGameInstance.Options.Find("SingleDayScenario");
               if (true == option.IsEnabled)
                  action = GameAction.SetupSingleGameDay;
               else
                  action = GameAction.SetupAssignCrewRating;
            }
         }
         //action = GameAction.TestingStartMorningBriefing;  // <cgs> TEST - skip the ammo setup
         //action = GameAction.TestingStartPreparations;     // <cgs> TEST - skip morning briefing and crew/ammo setup
         //action = GameAction.TestingStartMovement;         // <cgs> TEST - start with movement - skip battle prep phase
         //action = GameAction.TestingStartBattle;           // <cgs> TEST - skip the movement portion - begin with battle setup
         //action = GameAction.TestingStartAmbush;           // <cgs> TEST - skip battle setup
         myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
         return true;
      }
   }
}
