using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Pattons_Best
{
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
   public enum EnumModel
   {
      M4_A,
      M4_B,
      M4_C,
      M4A1_A,
      M4A1_B,
      M4A1_C,
      M4A3_A,
      M4A3_B,
      M4A3_C,
      M4A3_75W_D,
      M4A3_75W_E,
      M4A3E2_75W_F,
      M4A3E2_76W_F,
      M4A1_76W_G,
      M4A1_76W_H,
      M4A3_76W_H,
      M4A3_76W_G
   };
   [Serializable]
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
   [Serializable]
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
   [Serializable]
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
      Error
   };
   // GameState is a subclass representing the state pattern. For each game state, there can be different
   // game phases and game actions. The GameEngine makes a call that each class can act on..
   // GameEngine.PerformAction() ==> GameState.PerformAction()
   // GameState.PerformAction() ==> GameState.PerformAction()
   [Serializable]
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
      ShowMovementDiagramDialog,
      ShowInventoryDialog,
      ShowRuleListingDialog,
      ShowEventListingDialog,
      ShowTableListing,
      ShowGameFeats,
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
      EndGameFinal,  
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

      MorningBriefingBegin,
      MorningBriefingAssignCrewRating,
      MorningBriefingAssignCrewRatingEnd,
      MorningBriefingCalendarRoll,
      MorningBriefingWeatherRoll,
      MorningBriefingWeatherRollEnd,
      MorningBriefingSnowRoll,
      MorningBriefingSnowRollEnd,
      MorningBriefingAmmoLoad,
      MorningBriefingAmmoReadyRackLoad,
      MorningBriefingTimeCheck,
      MorningBriefingTimeCheckRoll,
      MorningBriefingEnd,

      PreparationsDeployment,
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

      MovementStartAreaSet,
      MovementStartAreaSetRoll,
      MovementExitAreaSet,
      MovementExitAreaSetRoll,
      MovementEnemyStrengthChoice,
      MovementEnemyStrengthCheckTerritory,
      MovementEnemyStrengthCheckTerritoryRoll,
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
      MovementStrengthRollBattleBoard,
      MovementBattleCheck,
      MovementBattleCheckRoll,
      MovementStartAreaRestart,
      MovementExit,

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
      
      BattleEmpty,
      BattleEmptyResolve,
      BattleShermanKilled,

      BattleRoundSequenceStart,
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
      BattleRoundSequenceShermanToHitRoll,
      BattleRoundSequenceShermanSkipRateOfFire,
      BattleRoundSequenceShermanToKillRoll,
      BattleRoundSequenceShermanToHitRollNothing,

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

      BattleRoundSequenceEnemyAction,
      BattleRoundSequenceCollateralDamageCheck,
      BattleRoundSequenceFriendlyAction,
      BattleRoundSequenceRandomEvent,
      BattleRoundSequenceBackToSpotting,
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

      EveningDebriefingStart,
      EveningDebriefingRatingImprovement,
      EveningDebriefingRatingImprovementEnd,
      EveningDebriefingVictoryPointsCalculated,
      EventDebriefPromotion,
      EventDebriefDecorationStart,
      EventDebriefDecorationContinue,
      EventDebriefDecorationBronzeStar,
      EventDebriefDecorationSilverStar,
      EventDebriefDecorationCross,
      EventDebriefDecorationHonor,
      EventDebriefDecorationHeart,
      Error
   };
   public interface IGameEngine
   {
      List<IView> Views { get; }
      void RegisterForUpdates(IView view);
      void PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll = 0);
      bool CreateUnitTests(IGameInstance gi, DockPanel dp, EventViewer ev, IDieRoller dr, CanvasImageViewer civ);
   }
}
