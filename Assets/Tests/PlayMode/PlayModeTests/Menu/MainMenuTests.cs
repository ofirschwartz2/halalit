using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Main menu (#176): the scores load once, PLAY and DAILY wait for them, and the menu survives a failed load or being left during it.
// Every test fakes PlayerStats' services in [SetUp], sets how the fakes behave, and only then loads the menu with LoadMenu.
// So the menu's Awake can never start its load before the fakes are ready, whatever the test runner's frame timing.
// Each test also checks that the fake sign-in ran: a load that reached the real services would leave it at 0 and fail loudly
public class MainMenuTests
{
    private const int MAX_FRAMES_TO_LOAD = 10;
    private const int FRAMES_TO_IGNORE_A_TAP = 3;
    private const int ALREADY_PLAYED_DAILY_SCORE = 30;
    private const string SERVICES_START_FAILURE = "Fake Unity Services start failure";
    private const string SCORE_LOAD_FAILURE = "Fake score load failure";
    private const string MENU_LOAD_WARNING = "MainMenu: could not load your scores";

    private int _currentSeed;
    private int _signInCalls, _scoreLoadCalls;
    private TaskCompletionSource<bool> _pendingSignIn;
    private bool _shouldSignInFail, _shouldScoreLoadFail, _hasPlayedTodaysDaily;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        PlayerStats.SetDefaultPlayerStatsValues();
        _signInCalls = 0;
        _scoreLoadCalls = 0;
        _pendingSignIn = null;
        _shouldSignInFail = false;
        _shouldScoreLoadFail = false;
        _hasPlayedTodaysDaily = false;
        PlayerStats.SetServices(FakeSignIn, FakeSaveScores, FakeLoadScores, FakeSubmitScore, FakeSaveDailyTrack);
    }

    [UnityTest]
    public IEnumerator PlayIsIgnoredWhileScoresLoad()
    {
        // GIVEN
        // The sign-in is still running
        _pendingSignIn = new TaskCompletionSource<bool>();
        MainMenu mainMenu = null;
        yield return LoadMenu(menu => mainMenu = menu);
        AssertWrapper.IsTrue(mainMenu.IsLoading(), "The menu is not loading, though its sign-in is still running", _currentSeed);

        // WHEN
        mainMenu.StartGame();
        mainMenu.StartDaily();

        for (int i = 0; i < FRAMES_TO_IGNORE_A_TAP; i++)
        {
            yield return null;
        }

        // THEN
        AssertWrapper.AreEqual(SceneName.MAIN_MENU.GetDescription(), SceneManager.GetActiveScene().name, "A tap during the load left the menu, so the load's reset could land in the middle of the run", _currentSeed);
        AssertWrapper.IsTrue(mainMenu.IsLoading(), "The menu stopped loading, though its sign-in is still running", _currentSeed);
        AssertWrapper.IsFalse(mainMenu.IsDailyButtonOn(), "DAILY is on before the scores loaded", _currentSeed);
        AssertWrapper.IsFalse(PlayerStats._isDailyRun, "DAILY during the load started a daily, though today's seed is not set yet", _currentSeed);
    }

    // The other half of the DAILY rule: FailedLoadKeepsDailyOffAndPlayOn and ServicesThatFailToStartKeepDailyOff only mean something if DAILY can come on
    [UnityTest]
    public IEnumerator SuccessfulLoadTurnsDailyOn()
    {
        // GIVEN
        // Every fake service works, and today's daily is not played yet
        MainMenu mainMenu = null;

        // WHEN
        yield return LoadMenu(menu => mainMenu = menu);
        yield return WaitForTheLoadToEnd(mainMenu);

        // THEN
        AssertWrapper.IsFalse(mainMenu.IsLoading(), "The menu did not finish loading, though every fake service had finished", _currentSeed);
        AssertWrapper.AreEqual(1, _scoreLoadCalls, "The menu's load did not load the scores", _currentSeed);
        AssertWrapper.IsTrue(mainMenu.IsDailyButtonOn(), "DAILY stayed off after a load that worked, with today's daily not played", _currentSeed);
    }

    [UnityTest]
    public IEnumerator PlayedDailyKeepsDailyOff()
    {
        // GIVEN
        // The server already has today's daily score
        _hasPlayedTodaysDaily = true;
        MainMenu mainMenu = null;

        // WHEN
        yield return LoadMenu(menu => mainMenu = menu);
        yield return WaitForTheLoadToEnd(mainMenu);

        // THEN
        AssertWrapper.IsFalse(mainMenu.IsLoading(), "The menu did not finish loading, though every fake service had finished", _currentSeed);
        AssertWrapper.IsTrue(PlayerStats._dailyScore == ALREADY_PLAYED_DAILY_SCORE, "The menu's load did not read today's daily score", _currentSeed);
        AssertWrapper.IsFalse(mainMenu.IsDailyButtonOn(), "DAILY is on, though today's daily is already played", _currentSeed);
    }

    [UnityTest]
    public IEnumerator FailedLoadKeepsDailyOffAndPlayOn()
    {
        // GIVEN
        // Offline: the score load fails
        _shouldScoreLoadFail = true;
        LogAssert.Expect(LogType.Warning, new Regex(MENU_LOAD_WARNING));
        MainMenu mainMenu = null;
        yield return LoadMenu(menu => mainMenu = menu);
        yield return WaitForTheLoadToEnd(mainMenu);
        AssertWrapper.IsFalse(mainMenu.IsLoading(), "The menu is still loading after a failed load, so PLAY stays dead", _currentSeed);
        AssertWrapper.AreEqual(1, _scoreLoadCalls, "The menu's load did not reach the score load, so this is not the failure under test", _currentSeed);
        AssertWrapper.IsFalse(mainMenu.IsDailyButtonOn(), "DAILY is on after a failed load, though the game cannot know whether you already played today", _currentSeed);

        // WHEN
        mainMenu.StartGame();
        yield return WaitForScene(TestUtils.PLAYGROUND_SCENE_NAME);

        // THEN
        AssertWrapper.AreEqual(TestUtils.PLAYGROUND_SCENE_NAME, SceneManager.GetActiveScene().name, "PLAY did not start a run after a failed load", _currentSeed);
    }

    // Authentication.Initialize logs a failed start of Unity Services and goes on, and then SetupEvents throws.
    // The fake sign-in stands in for that: it fails as Initialize would
    [UnityTest]
    public IEnumerator ServicesThatFailToStartKeepDailyOff()
    {
        // GIVEN
        _shouldSignInFail = true;
        LogAssert.Expect(LogType.Warning, new Regex(MENU_LOAD_WARNING));
        MainMenu mainMenu = null;

        // WHEN
        yield return LoadMenu(menu => mainMenu = menu);
        yield return WaitForTheLoadToEnd(mainMenu);

        // THEN
        AssertWrapper.IsFalse(mainMenu.IsLoading(), "The menu is still loading after the sign-in threw, so PLAY stays dead", _currentSeed);
        AssertWrapper.IsFalse(mainMenu.IsDailyButtonOn(), "DAILY is on after the sign-in threw, though the game cannot know whether you already played today", _currentSeed);
    }

    // The test runner fails a test on any logged error, so a menu that wrote to its destroyed texts and button would fail here
    [UnityTest]
    public IEnumerator LeavingTheMenuDuringTheLoadLogsNoError()
    {
        // GIVEN
        _pendingSignIn = new TaskCompletionSource<bool>();
        MainMenu mainMenu = null;
        yield return LoadMenu(menu => mainMenu = menu);
        AssertWrapper.IsTrue(mainMenu.IsLoading(), "The menu is not loading, though its sign-in is still running", _currentSeed);
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        yield return WaitForTheMenuToGo(mainMenu);
        AssertWrapper.IsNull(mainMenu, _currentSeed, "The menu was not destroyed when the Playground loaded");

        // WHEN
        _pendingSignIn.SetResult(true);
        yield return WaitForTheScoreLoad();

        // THEN
        AssertWrapper.IsTrue(PlayerStats._dailySeed != null, "The load did not set today's seed after the menu was gone", _currentSeed);
        AssertWrapper.AreEqual(1, _scoreLoadCalls, "The load did not finish after the menu was gone", _currentSeed);
    }

    [UnityTest]
    public IEnumerator PlayAfterADailyStartsANormalRun()
    {
        // GIVEN
        MainMenu mainMenu = null;
        yield return LoadMenu(menu => mainMenu = menu);
        yield return WaitForTheLoadToEnd(mainMenu);
        AssertWrapper.IsFalse(mainMenu.IsLoading(), "The menu did not finish loading, though every fake service had finished", _currentSeed);
        // A daily left on, as by a daily whose save failed
        PlayerStats.SetStartDaily();

        // WHEN
        mainMenu.StartGame();

        // THEN
        AssertWrapper.IsFalse(PlayerStats._isDailyRun, "PLAY after a daily started another daily", _currentSeed);
        yield return WaitForScene(TestUtils.PLAYGROUND_SCENE_NAME);
        AssertWrapper.AreEqual(TestUtils.PLAYGROUND_SCENE_NAME, SceneManager.GetActiveScene().name, "PLAY did not load the Playground", _currentSeed);
    }

    // LDRBRD stays live during the load. The leaderboard's own load must join the menu's, not sign in on top of it ("already signing in").
    // The sign-in is never finished here: a finished load would let the leaderboard ask Unity's real Leaderboards service for its list
    [UnityTest]
    public IEnumerator LeaderboardOpenedDuringTheMenuLoadJoinsIt()
    {
        // GIVEN
        _pendingSignIn = new TaskCompletionSource<bool>();
        MainMenu mainMenu = null;
        yield return LoadMenu(menu => mainMenu = menu);
        AssertWrapper.IsTrue(mainMenu.IsLoading(), "The menu is not loading, though its sign-in is still running", _currentSeed);

        // WHEN
        mainMenu.LeaderBoard();
        yield return WaitForTheLeaderboard();

        // THEN
        AssertWrapper.IsNotNull(UnityEngine.Object.FindObjectOfType<LeaderBoard>(), _currentSeed, "LDRBRD during the menu's load did not open the leaderboard");
        AssertWrapper.AreEqual(1, _signInCalls, "The leaderboard signed in on top of the menu's running sign-in", _currentSeed);
    }

    // A finished load is not reused, so coming back to the menu loads fresh scores, and a failed sign-in is tried again
    [UnityTest]
    public IEnumerator ReopenedMenuSignsInAgain()
    {
        // GIVEN
        MainMenu firstMenu = null;
        yield return LoadMenu(menu => firstMenu = menu);
        yield return WaitForTheLoadToEnd(firstMenu);
        AssertWrapper.IsFalse(firstMenu.IsLoading(), "The first menu did not finish loading, though every fake service had finished", _currentSeed);

        // WHEN
        SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
        yield return WaitForTheMenuToGo(firstMenu);
        AssertWrapper.IsNull(firstMenu, _currentSeed, "The first menu was not destroyed when the menu loaded again");
        var secondMenu = UnityEngine.Object.FindObjectOfType<MainMenu>();
        AssertWrapper.IsNotNull(secondMenu, _currentSeed, "The MainMenu scene has no MainMenu");
        yield return WaitForTheLoadToEnd(secondMenu);

        // THEN
        AssertWrapper.IsFalse(secondMenu.IsLoading(), "The second menu did not finish loading, though every fake service had finished", _currentSeed);
        AssertWrapper.AreEqual(2, _signInCalls, "The second menu reused the first, finished load instead of signing in again", _currentSeed);
        AssertWrapper.AreEqual(2, _scoreLoadCalls, "The second menu did not load fresh scores", _currentSeed);
    }

    #region Load and waits
    // Loads the menu after the test has set its fakes, waits for the menu's Awake, and checks that the load used the fake sign-in
    private IEnumerator LoadMenu(Action<MainMenu> onLoaded)
    {
        SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
        MainMenu mainMenu = null;

        for (int i = 0; i < MAX_FRAMES_TO_LOAD && mainMenu == null; i++)
        {
            yield return null;
            mainMenu = UnityEngine.Object.FindObjectOfType<MainMenu>();
        }

        AssertWrapper.IsNotNull(mainMenu, _currentSeed, "The MainMenu scene has no MainMenu");
        AssertWrapper.AreEqual(1, _signInCalls, "The menu's load did not use the fake sign-in, so it may have signed in for real", _currentSeed);
        onLoaded(mainMenu);
    }

    // A finished task's continuation runs on Unity's synchronization context, a frame or two later
    private IEnumerator WaitForTheLoadToEnd(MainMenu mainMenu)
    {
        for (int i = 0; i < MAX_FRAMES_TO_LOAD && mainMenu != null && mainMenu.IsLoading(); i++)
        {
            yield return null;
        }
    }

    private IEnumerator WaitForTheMenuToGo(MainMenu mainMenu)
    {
        for (int i = 0; i < MAX_FRAMES_TO_LOAD && mainMenu != null; i++)
        {
            yield return null;
        }
    }

    private IEnumerator WaitForTheScoreLoad()
    {
        for (int i = 0; i < MAX_FRAMES_TO_LOAD && _scoreLoadCalls == 0; i++)
        {
            yield return null;
        }
    }

    private IEnumerator WaitForTheLeaderboard()
    {
        for (int i = 0; i < MAX_FRAMES_TO_LOAD && UnityEngine.Object.FindObjectOfType<LeaderBoard>() == null; i++)
        {
            yield return null;
        }
    }

    private IEnumerator WaitForScene(string sceneName)
    {
        for (int i = 0; i < MAX_FRAMES_TO_LOAD && SceneManager.GetActiveScene().name != sceneName; i++)
        {
            yield return null;
        }
    }
    #endregion

    #region Fakes
    private Task FakeSignIn()
    {
        _signInCalls++;

        if (_shouldSignInFail)
        {
            return Task.FromException(new Exception(SERVICES_START_FAILURE));
        }

        if (_pendingSignIn != null)
        {
            return _pendingSignIn.Task;
        }

        return Task.CompletedTask;
    }

    private Task FakeSaveScores(Dictionary<string, object> scores)
    {
        return Task.CompletedTask;
    }

    // Counted when it returns, so a test that waits for it knows the whole load ran on the fakes
    private Task<Dictionary<string, int>> FakeLoadScores(HashSet<string> keys)
    {
        _scoreLoadCalls++;

        if (_shouldScoreLoadFail)
        {
            return Task.FromException<Dictionary<string, int>>(new Exception(SCORE_LOAD_FAILURE));
        }

        var scores = new Dictionary<string, int>();

        if (_hasPlayedTodaysDaily)
        {
            scores[PlayerStats._dailyScoreKey] = ALREADY_PLAYED_DAILY_SCORE;
        }

        return Task.FromResult(scores);
    }

    private Task FakeSubmitScore(string leaderboardId, double score)
    {
        return Task.CompletedTask;
    }

    private Task FakeSaveDailyTrack(DateTime israelDay, string trackJson)
    {
        return Task.CompletedTask;
    }
    #endregion

    // A sign-in left pending is never finished here: finishing it after ResetServices would run the rest of the load on the real services
    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        PlayerStats.ResetServices();
        PlayerStats.SetDefaultPlayerStatsValues();
    }
}
