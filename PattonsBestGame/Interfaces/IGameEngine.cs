using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Pattons_Best
{
   [Serializable]
   public enum GamePhase
   {
      UnitTest,
      GameSetup,
      StartGame,
      EndGame,
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
      UpdateShowRegion,
      UpdateEventViewerDisplay,
      UpdateEventViewerActive,
      DieRollActionNone, // The field in IGameInstance indicates what the roll apply. If none expected, it is set to this value.

      UpdateNewGame,  // Menu Options
      UpdateGameOptions, 
      UpdateLoadingGame,
      UpdateLoadingGameReturnToJail,
      UpdateUndo,

      ShowInventory,
      ShowRuleListing,
      ShowEventListing,
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
      EndGameFinal,      // Show the final location on map with final path
      EndGameExit,
      ExitGame,

      SetupGameOptionChoice,
      SetupChooseFunOptions,
      SetupFinalize,
      Error
   };
   public interface IGameEngine
   {
      List<IView> Views { get; }
      void RegisterForUpdates(IView view);
      void PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll = 0);
      bool CreateUnitTests(IGameInstance gi, DockPanel dp, EventViewer ev, IDieRoller dr);
   }
}
