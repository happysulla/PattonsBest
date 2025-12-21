using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace Pattons_Best
{
   public class ShermanDeath
   {
      public bool myCtorError = false;
      public IMapItem myEnemyUnit;
      public string myHitLocation = "";
      public string myEnemyFireDirection = "";
      public int myDay = 0;
      public string myCause = "";
      public bool myIsAmbush = false;
      public bool myIsExplosion = false;
      public bool myIsBailout = false;
      public bool myIsBrewUp = false;
      public ShermanDeath(IMapItem eu)
      {
         myEnemyUnit = eu;
      }
      public ShermanDeath(IGameInstance gi, IMapItem eu, string loc, string cause)
      {
         myEnemyUnit = eu;
         myHitLocation = loc;
         if ("ERROR" == loc)
         {
            myCtorError = true;
            Logger.Log(LogEnum.LE_ERROR, "ShermanDeath(): loc=ERROR");
         }
         myCause = cause;
         myDay = gi.Day;
         myIsAmbush = ((BattlePhase.Ambush == gi.BattlePhase) || (BattlePhase.AmbushRandomEvent == gi.BattlePhase));
         myEnemyFireDirection = TableMgr.GetEnemyFireDirection(gi, eu, myHitLocation);
         if ("ERROR" == myEnemyFireDirection)
         {
            myCtorError = true;
            Logger.Log(LogEnum.LE_ERROR, "ShermanDeath(): GetEnemyFireDirection() returned ERROR");
         }
      }
   }
   public class ShermanSetup
   {
      public bool myIsSetupPerformed = false; // set to true after 1st setup complete
      public IMapItems myHatches = new MapItems();
      public string myAmmoType = "";
      public double myTurretRotation = 0.0;
      public string myLoaderSpotTerritory = "";
      public string myCommanderSpotTerritory = "";
      public ShermanSetup(){}
      public void Clear()
      {
         myIsSetupPerformed = false;
         myHatches.Clear();
         myAmmoType = "";
         myTurretRotation = 0.0;
         myLoaderSpotTerritory = "";
         myCommanderSpotTerritory = "";
      }
   }
   public class PanzerfaustAttack
   {
      public IMapItem myEnemyUnit;
      public int myDay = 0;
      public bool myIsShermanMoving = false;
      public bool myIsLeadTank = false;
      public bool myIsAdvancingFireZone = false;
      public char mySector = 'E';
      public PanzerfaustAttack(IMapItem eu)
      {
         myEnemyUnit = eu;
      }
      public PanzerfaustAttack(IGameInstance gi, IMapItem enemyUnit, bool isAdvanceFire, char sector)
      {
         myEnemyUnit = enemyUnit;
         myDay = gi.Day;
         myIsShermanMoving = gi.Sherman.IsMoving;
         myIsLeadTank = gi.IsLeadTank;
         myIsAdvancingFireZone = isAdvanceFire;
         mySector = sector;
      }
   }
   public class ShermanAttack
   {
      public string myAttackType; // area or direct fire
      public string myAmmoType;   // He, Ap, Hbci, Wp,
      public bool myIsCriticalHit;
      public string myHitLocation = ""; // Turret, Hull, Thrown Track
      public bool myIsNoChance = false;
      public bool myIsImmobilization = false;
      public ShermanAttack(string attack, string ammo, bool critical, bool immobilization)
      {
         myAttackType = attack;
         myAmmoType = ammo;
         myIsCriticalHit = critical;
         myIsImmobilization = immobilization;
      }
   }
   public enum EnumScenario
   {
      Advance,
      Battle,
      Counterattack,
      Retrofit,
      None
   };
   public enum EnumResistance
   {
      Light,
      Medium,
      Heavy,
      None
   };
   public enum EnumSpottingResult
   {
      HIDDEN,
      UNSPOTTED,
      SPOTTED,
      IDENTIFIED
   };
   public enum EnumDecoration
   {
      ED_BronzeStar,
      ED_SilverStar,
      ED_DistinguisedServiceCross,
      ED_MedalOfHonor,
      ED_PurpleHeart,
      ED_EuropeanCampain,
      ED_WW2Victory
   };
   public enum GamePhase
   {
      UnitTest,
      GameSetup,
      MorningBriefing,
      Preparations,
      Movement,
      Battle,
      BattleRoundSequence,
      EveningDebriefing,
      EndGame,
      Error
   };
   public enum BattlePhase
   {
      None,
      Ambush,
      AmbushRandomEvent,
      Spotting,
      MarkCrewAction,
      MarkAmmoReload,
      ConductCrewAction,
      EnemyAction,
      FriendlyAction,
      RandomEvent,
      BackToSpotting,
      Error
   };
   public enum CrewActionPhase
   {
      None,
      Movement,
      TankMainGunFire,
      TankMgFire,
      ReplacePeriscope,
      RepairGun,
      FireMortar,
      ThrowGrenades,
      RestockReadyRack,
      CrewSwitch,
      Error
   };
   public enum GameAction
   {
      RemoveSplashScreen,
      UpdateStatusBar,
      UpdateTankCard,
      UpdateAfterActionReport,
      UpdateBattleBoard,
      UpdateTankExplosion,
      UpdateTankBrewUp,
      UpdateShowRegion,
      UpdateEventViewerDisplay,
      UpdateEventViewerActive,
      DieRollActionNone,          // The field in IGameInstance indicates what the roll apply. If none expected, it is set to this value.

      UpdateNewGame,              // Menu Options
      UpdateNewGameEnd,           // finish setting up for new game
      UpdateGameOptions, 
      UpdateLoadingGame,
      UpdateUndo,

      TestingStartMorningBriefing,
      TestingStartPreparations,
      TestingStartMovement,
      TestingStartBattle,
      TestingStartAmbush,

      ShowCombatCalendarDialog,
      ShowAfterActionReportDialog,
      ShowTankForcePath,
      ShowMovementDiagramDialog,
      ShowRoads,
      ShowRuleListingDialog,
      ShowEventListingDialog,
      ShowTableListing,
      ShowGameFeatsDialog,
      ShowReportErrorDialog,
      ShowAboutDialog,

      UnitTestStart,
      UnitTestCommand,
      UnitTestNext,
      UnitTestTest,
      UnitTestCleanup,

      EndGameWin,
      EndGameLost,
      EndGameShowFeats,
      EndGameShowStats,
      EndGameClose,
      EndGameExit,
      ExitGame,

      SetupShowMapHistorical,
      SetupShowMovementBoard,
      SetupShowBattleBoard,
      SetupShowTankCard,
      SetupShowAfterActionReport,
      SetupAssignCrewRating,
      SetupShowCombatCalendarCheck,
      SetupChooseFunOptions,
      SetupCombatCalendarRoll,
      SetupFinalize,
      SetupSingleGameDay,
      SetupDecreaseDate,
      SetupIncreaseDate,
      SetupShowSingleDayBattleStart,

      MorningBriefingBegin,
      MorningBriefingCrewmanHealing,
      MorningBriefingAssignCrewRating,
      MorningBriefingExistingCrewman,
      MorningBriefingReturningCrewman,
      MorningBriefingAssignCrewRatingEnd,
      MorningBriefingTankReplaceChoice,
      MorningBriefingTankKeepChoice,
      MorningBriefingTrainCrew,
      EveningDebriefingRatingTrainingEnd,
      MorningBriefingTrainCrewHvssEnd,
      MorningBriefingTankReplacementRoll,
      MorningBriefingTankReplacementHvssRoll,
      MorningBriefingDecreaseTankNum,
      MorningBriefingIncreaseTankNum,
      MorningBriefingTankReplacementEnd,
      MorningBriefingCalendarRoll,
      MorningBriefingWeatherRoll,
      MorningBriefingWeatherRollEnd,
      MorningBriefingSnowRoll,
      MorningBriefingSnowRollEnd,
      MorningBriefingAmmoLoad,
      MorningBriefingAmmoLoadSkip,
      MorningBriefingAmmoReadyRackLoad,
      MorningBriefingTimeCheck,
      MorningBriefingTimeCheckRoll,
      MorningBriefingDayOfRest,
      MorningBriefingDeployment,

      PreparationsDeploymentRoll,
      PreparationsHatches,
      PreparationsShowHatchAction,
      PreparationsGunLoad,
      PreparationsLoader,
      PreparationsGunLoadSelect,
      PreparationsTurret,
      PreparationsTurretRotateLeft,
      PreparationsTurretRotateRight,
      PreparationsLoaderSpot,
      PreparationsLoaderSpotSet,
      PreparationsCommanderSpot,
      PreparationsCommanderSpotSet,
      PreparationsFinal,
      PreparationsFinalSkip,
      PreparationsShowFeat,
      PreparationsShowFeatEnd,
      PreparationsRepairMainGunRoll,
      PreparationsRepairMainGunRollEnd,
      PreparationsRepairAaMgRoll,
      PreparationsRepairAaMgRollEnd,
      PreparationsRepairCoaxialMgRoll,
      PreparationsRepairCoaxialMgRollEnd,
      PreparationsRepairBowMgRoll,
      PreparationsRepairBowMgRollEnd,
      PreparationsReplaceCrewEnd,

      MovementStartAreaSet,
      MovementStartAreaSetRoll,
      MovementExitAreaSet,
      MovementExitAreaSetRoll,
      MovementEnemyStrengthChoice,
      MovementEnemyStrengthCheckTerritory,
      MovementEnemyStrengthCheckTerritoryRoll,
      MovementEnemyCheckCounterattack,
      MovementBattleCheckCounterattackRoll,
      MovementCounterattackEllapsedTimeRoll,
      MovementBattleActivation,
      MovementChooseOption,
      MovementArtillerySupportChoice,
      MovementArtillerySupportCheck,
      MovementArtillerySupportCheckRoll,
      MovementAirStrikeChoice,
      MovementAirStrikeCheckTerritory,
      MovementAirStrikeCheckTerritoryRoll,
      MovementAirStrikeCancel,
      MovementResupplyCheck,
      MovementResupplyCheckRoll,
      MovementAmmoLoad,
      MovementEnterArea,
      MovementAdvanceFireChoice,
      MovementAdvanceFireAmmoUseCheck,
      MovementAdvanceFireAmmoUseRoll,
      MovementAdvanceFire,
      MovementAdvanceFireSkip,
      MovementEnterAreaUsControl,
      MovementStrengthBattleBoardRoll,
      MovementBattleCheck,
      MovementBattleCheckRoll,
      MovementStartAreaRestart,
      MovementStartAreaRestartAfterBattle,
      MovementExit,
      MovementRetreatStartBattle,
      MovementRainRoll,
      MovementRainRollEnd,
      MovementSnowRoll,
      MovementSnowRollEnd,

      BattleStart,
      BattleActivation,
      BattlePlaceAdvanceFire,
      BattleResolveAdvanceFire,
      BattleResolveArtilleryFire,
      BattleResolveAirStrike,
      BattleAmbushStart,
      BattleAmbushRoll,
      BattleSetupEnd,
      BattleAmbush,
      BattleRandomEvent,
      BattleRandomEventRoll,
      BattleCollateralDamageCheck,
      BattleCrewReplaced,
      BattleEmpty,
      BattleEmptyResolve,
      BattleShermanKilled,

      BattleRoundSequenceStart,
      BattleRoundSequenceAmbushCounterattack,
      BattleRoundSequenceSmokeDepletionEnd,
      BattleRoundSequenceSpotting,
      BattleRoundSequenceSpottingEnd,
      BattleRoundSequenceCrewOrders,
      BattleRoundSequenceAmmoOrders,
      BattleRoundSequenceConductCrewAction,

      BattleRoundSequenceMovementRoll,
      BattleRoundSequenceBoggedDownRoll,
      BattleRoundSequencePivot,
      BattleRoundSequencePivotLeft,
      BattleRoundSequencePivotRight,
      BattleRoundSequenceMovementPivotEnd,
      BattleRoundSequenceChangeFacing,
      BattleRoundSequenceChangeFacingEnd,
      BattleRoundSequenceTurretEnd,
      BattleRoundSequenceTurretEndRotateLeft,
      BattleRoundSequenceTurretEndRotateRight,

      BattleRoundSequenceShermanFiringSelectTarget,
      BattleRoundSequenceShermanFiringMainGun,
      BattleRoundSequenceShermanFiringMainGunEnd,
      BattleRoundSequenceShermanFiringMainGunNot,
      BattleRoundSequenceShermanToHitRoll,
      BattleRoundSequenceShermanSkipRateOfFire,
      BattleRoundSequenceShermanToKillRoll,
      BattleRoundSequenceShermanToHitRollNothing,
      BattleRoundSequenceShermanToKillRollMiss,

      BattleRoundSequenceShermanFiringSelectTargetMg,
      BattleRoundSequenceFireAaMg,
      BattleRoundSequenceFireBowMg,
      BattleRoundSequenceFireCoaxialMg,
      BattleRoundSequenceFireSubMg,
      BattleRoundSequenceFireMgSkip,
      BattleRoundSequenceShermanFiringMachineGun,
      BattleRoundSequenceFireMachineGunRoll,
      BattleRoundSequenceFireMachineGunRollEnd,
      BattleRoundSequencePlaceAdvanceFire,
      BattleRoundSequencePlaceAdvanceFireRoll,
      BattleRoundSequencePlaceAdvanceFireRollEnd,
      BattleRoundSequenceReplacePeriscopes,
      BattleRoundSequenceRepairMainGunRoll,
      BattleRoundSequenceRepairAaMgRoll,
      BattleRoundSequenceRepairCoaxialMgRoll ,
      BattleRoundSequenceRepairBowMgRoll,
      BattleRoundSequenceShermanFiringMortar,
      BattleRoundSequenceShermanThrowGrenade,
      BattleRoundSequenceReadyRackHeMinus,
      BattleRoundSequenceReadyRackApMinus,
      BattleRoundSequenceReadyRackWpMinus,
      BattleRoundSequenceReadyRackHbciMinus,
      BattleRoundSequenceReadyRackHvapMinus,
      BattleRoundSequenceReadyRackHePlus,
      BattleRoundSequenceReadyRackApPlus,
      BattleRoundSequenceReadyRackWpPlus,
      BattleRoundSequenceReadyRackHbciPlus,
      BattleRoundSequenceReadyRackHvapPlus,
      BattleRoundSequenceReadyRackEnd,
      BattleRoundSequenceCrewSwitchEnd,
      BattleRoundSequenceCrewReplaced,

      BattleRoundSequenceEnemyAction,
      BattleRoundSequenceCollateralDamageCheck,
      BattleRoundSequenceFriendlyAction,
      BattleRoundSequenceRandomEvent,
      BattleRoundSequenceBackToSpotting,
      BattleRoundSequenceNextActionAfterRandomEvent,
      BattleRoundSequenceLoadMainGun,
      BattleRoundSequenceLoadMainGunEnd,

      BattleRoundSequenceShermanKilled,
      BattleRoundSequenceEnemyArtilleryRoll,
      BattleRoundSequenceMinefieldRoll,
      BattleRoundSequenceMinefieldDisableRoll,
      BattleRoundSequenceMinefieldDriverWoundRoll,
      BattleRoundSequenceMinefieldAssistantWoundRoll,
      BattleRoundSequencePanzerfaustSectorRoll,
      BattleRoundSequencePanzerfaustAttackRoll,
      BattleRoundSequencePanzerfaustToHitRoll,
      BattleRoundSequencePanzerfaustToKillRoll,
      BattleRoundSequenceHarrassingFire,
      BattleRoundSequenceFriendlyAdvance,
      BattleRoundSequenceFriendlyAdvanceSelected,
      BattleRoundSequenceEnemyAdvance,
      BattleRoundSequenceEnemyAdvanceEnd,
      BattleRoundSequenceShermanAdvanceOrRetreat,
      BattleRoundSequenceShermanAdvanceOrRetreatEnd,
      BattleRoundSequenceShermanRetreatChoice,
      BattleRoundSequenceShermanRetreatChoiceEnd,
      BattleRoundSequenceShowFeat,
      BattleRoundSequenceShowFeatEnd,

      EveningDebriefingStart,
      EveningDebriefingRatingImprovement,
      EveningDebriefingRatingImprovementEnd,
      EveningDebriefingCrewReplacedEnd,
      EveningDebriefingVictoryPointsCalculated,
      EventDebriefPromotion,
      EventDebriefDecorationStart,
      EventDebriefDecorationContinue,
      EventDebriefDecorationBronzeStar,
      EventDebriefDecorationSilverStar,
      EventDebriefDecorationCross,
      EventDebriefDecorationHonor,
      EventDebriefDecorationHeart,
      EveningDebriefingResetDay,
      EveningDebriefingShowFeat,
      EveningDebriefingShowFeatEnd,
      Error
   };
   //================================================================================================
   // GameState is a subclass representing the state pattern. For each game state, there can be different
   // game phases and game actions. The GameEngine makes a call that each class can act on..
   // GameEngine.PerformAction() ==> GameState.PerformAction()
   // GameState.PerformAction() ==> GameState.PerformAction()
   public interface IGameEngine
   {
      List<IView> Views { get; }
      void RegisterForUpdates(IView view);
      void PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll = 0);
      bool CreateUnitTests(IGameInstance gi, DockPanel dp, EventViewer ev, IDieRoller dr, CanvasImageViewer civ);
   }
}
