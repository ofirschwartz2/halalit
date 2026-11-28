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
// Every test fakes PlayerStats' services before the menu loads, so no test signs in or loads for real.
// The fakes read this class's fields when the menu calls them, in its Awake on the next frame, so a test sets them before its first yield
public class MainMenuTests
{
    private const int MAX_FRAMES_TO_LOAD = 10;
    private const int FRAMES_TO_IGNORE_A_TAP = 3;
    private const string SERVICES_START_FAILURE = "Fake Unity Services start failure";
    private const string SCORE_LOAD_FAILURE = "Fake score load failure";
    private const string MENU_LOAD_WARNING = "MainMenu: could not load your scores";

    private TaskCompletionSource<bool> _pendingSignIn;
    private bool _shouldSignInFail, _shouldScoreLoadFail;

    [SetUp]
    public void SetUp()
    {
        PlayerStats.SetDefaultPlayerStatsValues();
        _pendingSignIn = null;
        _shouldSignInFail = false;
        _shouldScoreLoadFail = false;
        PlayerStats.SetServices(FakeSignIn, FakeSaveScores, FakeLoadScores, FakeSubmitScore, FakeSaveDailyTrack);
        SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
    }

    [UnityTest]
    public IEnumerator PlayIsIgnoredWhileScoresLoad()
    {
        // GIVEN
        // The sign-in is still running
        _pendingSignIn = new TaskCompletionSource<bool>();
        yield return null;
        var mainMenu = GetMainMenu();
        AssertWrapper.IsTrue(mainMenu != null, "The MainMenu scene has no MainMenu");
        AssertWrapper.IsTrue(mainMenu.IsLoading(), "The menu is not loading, though its sign-in is still running");

        // WHEN
        mainMenu.StartGame();
        mainMenu.StartDaily();

        for (int i = 0; i < FRAMES_TO_IGNORE_A_TAP; i++)
        {
            yield return null;
        }

        // THEN
        AssertWrapper.AreEqual(SceneName.MAIN_MENU.GetDescription(), SceneManager.GetActiveScene().name, "A tap during the load left the menu, so the load's reset could land in the middle of the run");
        AssertWrapper.IsTrue(mainMenu.IsLoading(), "The menu stopped loading, though its sign-in is still running");
        AssertWrapper.IsFalse(mainMenu.IsDailyButtonOn(), "DAILY is on before the scores loaded");
        AssertWrapper.IsFalse(PlayerStats._isDailyRun, "DAILY during the load started a daily, though today's seed is not set yet");
    }

    [UnityTest]
    public IEnumerator FailedLoadKeepsDailyOffAndPlayOn()
    {
        // GIVEN
        // Offline: the score load fails
        _shouldScoreLoadFail = true;
        LogAssert.Expect(LogType.Warning, new Regex(MENU_LOAD_WARNING));
        yield return null;
        var mainMenu = GetMainMenu();
        AssertWrapper.IsTrue(mainMenu != null, "The MainMenu scene has no MainMenu");
        yield return WaitForTheLoadToEnd(mainMenu);
        AssertWrapper.IsFalse(mainMenu.IsLoading(), "The menu is still loading after a failed load, so PLAY stays dead");
        AssertWrapper.IsFalse(mainMenu.IsDailyButtonOn(), "DAILY is on after a failed load, though the game cannot know whether you already played today");
        var currentSeed = TestUtils.SetRandomSeed();

        // WHEN
        mainMenu.StartGame();
        yield return WaitForScene(TestUtils.PLAYGROUND_SCENE_NAME);

        // THEN
        AssertWrapper.AreEqual(TestUtils.PLAYGROUND_SCENE_NAME, SceneManager.GetActiveScene().name, "PLAY did not start a run after a failed load", currentSeed);
    }

    // Authentication.Initialize logs a failed start of Unity Services and goes on, and then SetupEvents throws.
    // The fake sign-in stands in for that: it fails as Initialize would
    [UnityTest]
    public IEnumerator ServicesThatFailToStartKeepDailyOff()
    {
        // GIVEN
        _shouldSignInFail = true;
        LogAssert.Expect(LogType.Warning, new Regex(MENU_LOAD_WARNING));

        // WHEN
        yield return null;
        var mainMenu = GetMainMenu();
        AssertWrapper.IsTrue(mainMenu != null, "The MainMenu scene has no MainMenu");
        yield return WaitForTheLoadToEnd(mainMenu);

        // THEN
        AssertWrapper.IsFalse(mainMenu.IsLoading(), "The menu is still loading after the sign-in threw, so PLAY stays dead");
        AssertWrapper.IsFalse(mainMenu.IsDailyButtonOn(), "DAILY is on after the sign-in threw, though the game cannot know whether you already played today");
    }

    // The test runner fails a test on any logged error, so a menu that wrote to its destroyed texts and button would fail here
    [UnityTest]
    public IEnumerator LeavingTheMenuDuringTheLoadLogsNoError()
    {
        // GIVEN
        _pendingSignIn = new TaskCompletionSource<bool>();
        yield return null;
        var mainMenu = GetMainMenu();
        AssertWrapper.IsTrue(mainMenu != null, "The MainMenu scene has no MainMenu");
        AssertWrapper.IsTrue(mainMenu.IsLoading(), "The menu is not loading, though its sign-in is still running");
        var currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        yield return WaitForTheMenuToGo(mainMenu);
        AssertWrapper.IsTrue(mainMenu == null, "The menu was not destroyed when the Playground loaded", currentSeed);

        // WHEN
        _pendingSignIn.SetResult(true);
        yield return WaitForTheDailySeed();

        // THEN
        AssertWrapper.IsTrue(PlayerStats._dailySeed != null, "The load did not finish after the menu was gone", currentSeed);
    }

    [UnityTest]
    public IEnumerator PlayAfterADailyStartsANormalRun()
    {
        // GIVEN
        yield return null;
        var mainMenu = GetMainMenu();
        AssertWrapper.IsTrue(mainMenu != null, "The MainMenu scene has no MainMenu");
        yield return WaitForTheLoadToEnd(mainMenu);
        AssertWrapper.IsFalse(mainMenu.IsLoading(), "The menu did not finish loading, though every fake service had finished");
        // A daily left on, as by a daily whose save failed
        PlayerStats.SetStartDaily();
        var currentSeed = TestUtils.SetRandomSeed();

        // WHEN
        mainMenu.StartGame();

        // THEN
        AssertWrapper.IsFalse(PlayerStats._isDailyRun, "PLAY after a daily started another daily", currentSeed);
        yield return WaitForScene(TestUtils.PLAYGROUND_SCENE_NAME);
        AssertWrapper.AreEqual(TestUtils.PLAYGROUND_SCENE_NAME, SceneManager.GetActiveScene().name, "PLAY did not load the Playground", currentSeed);
    }

    #region Waits
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

    private IEnumerator WaitForTheDailySeed()
    {
        for (int i = 0; i < MAX_FRAMES_TO_LOAD && PlayerStats._dailySeed == null; i++)
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
    private MainMenu GetMainMenu()
    {
        return UnityEngine.Object.FindObjectOfType<MainMenu>();
    }

    private Task FakeSignIn()
    {
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

    private Task<Dictionary<string, int>> FakeLoadScores(HashSet<string> keys)
    {
        if (_shouldScoreLoadFail)
        {
            return Task.FromException<Dictionary<string, int>>(new Exception(SCORE_LOAD_FAILURE));
        }

        return Task.FromResult(new Dictionary<string, int>());
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

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        PlayerStats.ResetServices();
        PlayerStats.SetDefaultPlayerStatsValues();
    }
}
