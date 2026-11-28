using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerStatsTests
{
    private const string HIGH_SCORE_SAVE_FAILURE = "Fake high score save failure";
    private const string DAILY_SCORE_SAVE_FAILURE = "Fake daily score save failure";
    private const string LOAD_SAVE_WARNING = "PlayerStats: could not save the last run's scores";
    private const string SECOND_SAVE_FAILURE_WARNING = "PlayerStats: could not save the daily score either";
    private const string PLAYER_STATS_LOG_PREFIX = "PlayerStats:";
    private const string SCORE_LOAD_FAILURE = "Fake score load failure";
    private const int RUN_SCORE = 40;
    private const int SAVED_HIGH_SCORE = 70;

    private List<string> _savedKeys;
    private List<string> _submittedLeaderboards;
    private int _saveCalls;
    private int _signInCalls;
    private int _playerStatsWarnings;
    private bool _shouldHighScoreSaveFail, _shouldDailyScoreSaveFail, _shouldScoreLoadFail;
    private TaskCompletionSource<bool> _pendingSave;
    private TaskCompletionSource<bool> _pendingSignIn;
    private Dictionary<string, int> _scoresOnServer;

    [SetUp]
    public void SetUp()
    {
        PlayerStats.SetDefaultPlayerStatsValues();

        _savedKeys = new List<string>();
        _submittedLeaderboards = new List<string>();
        _saveCalls = 0;
        _signInCalls = 0;
        _shouldHighScoreSaveFail = false;
        _shouldDailyScoreSaveFail = false;
        _shouldScoreLoadFail = false;
        _pendingSave = null;
        _pendingSignIn = null;
        _scoresOnServer = new Dictionary<string, int>();
        _playerStatsWarnings = 0;

        Application.logMessageReceived += CountPlayerStatsWarning;
        PlayerStats.SetServices(FakeSignIn, FakeSaveScores, FakeLoadScores, FakeSubmitScore, FakeSaveDailyTrack);
    }

    [TearDown]
    public void TearDown()
    {
        Application.logMessageReceived -= CountPlayerStatsWarning;
        TestServices.UseFakes();
        PlayerStats.SetDefaultPlayerStatsValues();
    }

    [Test]
    public void FailedHighScoreSaveStillSavesTheDailyScore()
    {
        // GIVEN
        // A daily that is also a new best, and the high score save fails (for example, leaderboard "1" is missing)
        SetDailyThatIsANewBest();
        _shouldHighScoreSaveFail = true;

        // WHEN
        var save = PlayerStats.TrySaveHighScoresAsync();

        // THEN
        AssertWrapper.IsTrue(save.IsCompleted, "The save did not finish, though every fake service had finished");
        AssertWrapper.IsTrue(save.IsFaulted, "The save hid the high score failure, so the results card cannot show it");
        AssertWrapper.AreEqual(HIGH_SCORE_SAVE_FAILURE, save.Exception.InnerException.Message, "The save did not pass on the first failure");
        AssertWrapper.IsTrue(_savedKeys.Contains(PlayerStats._dailyScoreKey), "A failed high score save skipped the daily score save");
        AssertWrapper.IsTrue(_submittedLeaderboards.Contains(Constants.DAILY_LEADERBOARD_ID), "A failed high score save skipped the daily leaderboard");
        AssertWrapper.IsFalse(PlayerStats._isDailyRun, "The daily was saved, but the daily stayed on for the next run");
        AssertWrapper.IsTrue(PlayerStats._isNewHighScore, "The high score was not saved, but its flag was cleared, so no later save sends it");
    }

    [Test]
    public void FailedDailyScoreSaveStillSavesTheHighScore()
    {
        // GIVEN
        SetDailyThatIsANewBest();
        _shouldDailyScoreSaveFail = true;

        // WHEN
        var save = PlayerStats.TrySaveHighScoresAsync();

        // THEN
        AssertWrapper.IsTrue(save.IsCompleted, "The save did not finish, though every fake service had finished");
        AssertWrapper.IsTrue(save.IsFaulted, "The save hid the daily score failure, so the results card cannot show it");
        AssertWrapper.AreEqual(DAILY_SCORE_SAVE_FAILURE, save.Exception.InnerException.Message, "The save did not pass on the daily score failure");
        AssertWrapper.IsTrue(_savedKeys.Contains(PlayerStats._highScoreKey), "The high score was not saved");
        AssertWrapper.IsTrue(_submittedLeaderboards.Contains(Constants.HIGHSCORE_LEADERBOARD_ID), "The high score was not sent to its leaderboard");
        AssertWrapper.IsFalse(PlayerStats._isNewHighScore, "The high score was saved, but its flag stayed on");
        AssertWrapper.IsFalse(_submittedLeaderboards.Contains(Constants.DAILY_LEADERBOARD_ID), "The daily score was sent to the daily leaderboard, though its save failed");
        AssertWrapper.IsTrue(PlayerStats._isDailyRun, "The daily score was not saved, but its flag was cleared, so the menu's save does not retry it");
    }

    [Test]
    public void SuccessfulSavesSendBothScoresAndEndTheDaily()
    {
        // GIVEN
        SetDailyThatIsANewBest();

        // WHEN
        var save = PlayerStats.TrySaveHighScoresAsync();

        // THEN
        AssertWrapper.IsTrue(save.IsCompleted, "The save did not finish, though every fake service had finished");
        AssertWrapper.IsFalse(save.IsFaulted, "The save failed, though every fake service worked");
        AssertWrapper.IsTrue(_savedKeys.Contains(PlayerStats._highScoreKey), "The high score was not saved");
        AssertWrapper.IsTrue(_savedKeys.Contains(PlayerStats._dailyScoreKey), "The daily score was not saved");
        AssertWrapper.IsTrue(_submittedLeaderboards.Contains(Constants.HIGHSCORE_LEADERBOARD_ID), "The high score was not sent to its leaderboard");
        AssertWrapper.IsTrue(_submittedLeaderboards.Contains(Constants.DAILY_LEADERBOARD_ID), "The daily score was not sent to the daily leaderboard");
        AssertWrapper.IsFalse(PlayerStats._isNewHighScore, "The high score was saved, but its flag stayed on");
        AssertWrapper.IsFalse(PlayerStats._isDailyRun, "The daily was saved, but the daily stayed on for the next run");
        AssertWrapper.AreEqual(0, _playerStatsWarnings, "A save that worked logged a warning");
    }

    [Test]
    public void BothFailedSavesPassOnTheFirstAndLogTheSecondOnce()
    {
        // GIVEN
        // Offline: both saves fail
        SetDailyThatIsANewBest();
        _shouldHighScoreSaveFail = true;
        _shouldDailyScoreSaveFail = true;
        LogAssert.Expect(LogType.Warning, new Regex(SECOND_SAVE_FAILURE_WARNING));

        // WHEN
        var save = PlayerStats.TrySaveHighScoresAsync();

        // THEN
        AssertWrapper.IsTrue(save.IsCompleted, "The save did not finish, though every fake service had finished");
        AssertWrapper.IsTrue(save.IsFaulted, "The save hid both failures, so the results card cannot show them");
        AssertWrapper.AreEqual(HIGH_SCORE_SAVE_FAILURE, save.Exception.InnerException.Message, "The save did not pass on the first failure, the high score's");
        AssertWrapper.AreEqual(1, _playerStatsWarnings, "The second failure was not logged as exactly one warning");
        AssertWrapper.AreEqual(0, _submittedLeaderboards.Count, "A score was sent to a leaderboard, though its save failed");
        AssertWrapper.IsTrue(PlayerStats._isNewHighScore, "The high score was not saved, but its flag was cleared");
        AssertWrapper.IsTrue(PlayerStats._isDailyRun, "The daily score was not saved, but its flag was cleared");
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public void OneFailedSaveLogsNoWarning(bool shouldHighScoreSaveFail, bool shouldDailyScoreSaveFail)
    {
        // GIVEN
        // The failure is passed on to the results card, which logs it. Logging it here as well would log it twice
        SetDailyThatIsANewBest();
        _shouldHighScoreSaveFail = shouldHighScoreSaveFail;
        _shouldDailyScoreSaveFail = shouldDailyScoreSaveFail;

        // WHEN
        var save = PlayerStats.TrySaveHighScoresAsync();

        // THEN
        AssertWrapper.IsTrue(save.IsCompleted, "The save did not finish, though every fake service had finished");
        AssertWrapper.IsTrue(save.IsFaulted, "The save hid its failure, so the warning count below says nothing");
        AssertWrapper.AreEqual(0, _playerStatsWarnings, "One failed save was logged by PlayerStats, so the results card's warning is doubled");
    }

    [Test]
    public void LoadAfterAFailedDailySaveEndsTheDaily()
    {
        // GIVEN
        // The daily's save failed on the results card and fails again in the menu's load
        PlayerStats.SetStartDaily();
        PlayerStats._dailyScore = RUN_SCORE;
        _shouldDailyScoreSaveFail = true;
        _scoresOnServer[PlayerStats._highScoreKey] = SAVED_HIGH_SCORE;
        LogAssert.Expect(LogType.Warning, new Regex(LOAD_SAVE_WARNING));

        // WHEN
        var load = PlayerStats.InitAllAsync();

        // THEN
        AssertWrapper.IsTrue(load.IsCompleted, "The load did not finish, though every fake service had finished");
        AssertWrapper.IsFalse(load.IsFaulted, "A failed save broke the menu's load");
        AssertWrapper.IsFalse(PlayerStats._isDailyRun, "The daily stayed on after the load, so the next PLAY would be a daily");
        AssertWrapper.IsTrue(PlayerStats._highScore == SAVED_HIGH_SCORE, "The load did not read the saved high score after a failed save");
        AssertWrapper.IsTrue(PlayerStats._dailySeed != null, "The load did not set today's daily seed after a failed save");
    }

    [TestCase(true, false)]
    [TestCase(true, true)]
    public void LoadAfterAnyOtherFailedSaveEndsTheDaily(bool shouldHighScoreSaveFail, bool shouldDailyScoreSaveFail)
    {
        // GIVEN
        // The daily-only failure is LoadAfterAFailedDailySaveEndsTheDaily. Here the high score save fails, alone or with the daily's
        SetDailyThatIsANewBest();
        _shouldHighScoreSaveFail = shouldHighScoreSaveFail;
        _shouldDailyScoreSaveFail = shouldDailyScoreSaveFail;
        _scoresOnServer[PlayerStats._highScoreKey] = SAVED_HIGH_SCORE;

        if (shouldHighScoreSaveFail && shouldDailyScoreSaveFail)
        {
            LogAssert.Expect(LogType.Warning, new Regex(SECOND_SAVE_FAILURE_WARNING));
        }

        LogAssert.Expect(LogType.Warning, new Regex(LOAD_SAVE_WARNING));

        // WHEN
        var load = PlayerStats.InitAllAsync();

        // THEN
        AssertWrapper.IsTrue(load.IsCompleted, "The load did not finish, though every fake service had finished");
        AssertWrapper.IsFalse(load.IsFaulted, "A failed save broke the menu's load");
        AssertWrapper.IsFalse(PlayerStats._isDailyRun, "The daily stayed on after the load, so the next PLAY would be a daily");
        AssertWrapper.IsTrue(PlayerStats._highScore == SAVED_HIGH_SCORE, "The load did not read the saved high score after a failed save");
        AssertWrapper.IsTrue(PlayerStats._dailySeed != null, "The load did not set today's daily seed after a failed save");
    }

    [Test]
    public void FailedScoreLoadStillEndsTheDaily()
    {
        // GIVEN
        // Offline after a daily: the daily's save fails, then the score load fails.
        // The load logs no error of its own (#176): the screen that awaits it logs the failure once
        PlayerStats.SetStartDaily();
        PlayerStats._dailyScore = RUN_SCORE;
        _shouldDailyScoreSaveFail = true;
        _shouldScoreLoadFail = true;
        LogAssert.Expect(LogType.Warning, new Regex(LOAD_SAVE_WARNING));

        // WHEN
        var load = PlayerStats.InitAllAsync();

        // THEN
        AssertWrapper.IsTrue(load.IsCompleted, "The load did not finish, though every fake service had finished");
        AssertWrapper.IsTrue(load.IsFaulted, "The load hid the failed score load, so the menu cannot know its scores are missing");
        AssertWrapper.AreEqual(SCORE_LOAD_FAILURE, load.Exception.InnerException.Message, "The load did not pass on the score load's failure");
        AssertWrapper.AreEqual(1, _playerStatsWarnings, "PlayerStats logged the failure more than once, though only the screen that awaits the load should log the score load's failure");
        AssertWrapper.IsFalse(PlayerStats._isDailyRun, "The daily stayed on after a failed score load, so the next PLAY would be a daily");
        AssertWrapper.IsTrue(PlayerStats._dailySeed != null, "The load did not set today's daily seed before the score load");
    }

    [Test]
    public void PlayAfterADailyIsANormalRun()
    {
        // GIVEN
        PlayerStats.SetStartDaily();

        // WHEN
        PlayerStats.SetStartNormal();

        // THEN
        AssertWrapper.IsFalse(PlayerStats._isDailyRun, "PLAY after a daily started another daily");
    }

    [Test]
    public void LoadWaitsForTheCardsSave()
    {
        // GIVEN
        // The results card started the run's save, and it is still running
        PlayerStats._highScore = RUN_SCORE;
        PlayerStats._isNewHighScore = true;
        _pendingSave = new TaskCompletionSource<bool>();
        PlayerStats.SaveRunScoresAsync();

        // WHEN
        var load = PlayerStats.InitAllAsync();

        // THEN
        AssertWrapper.AreEqual(1, _saveCalls, "The load saved on top of the card's save");
        AssertWrapper.IsFalse(load.IsCompleted, "The load finished before the card's save");
        AssertWrapper.IsTrue(PlayerStats._dailySeed == null, "The load reset the scores before the card's save finished");
    }

    [Test]
    public void SecondLoadJoinsTheRunningLoad()
    {
        // GIVEN
        // The menu's load is still signing in when the leaderboard asks for its own (#176)
        _pendingSignIn = new TaskCompletionSource<bool>();
        var firstLoad = PlayerStats.InitAllAsync();

        // WHEN
        var secondLoad = PlayerStats.InitAllAsync();

        // THEN
        AssertWrapper.IsFalse(firstLoad.IsCompleted, "The first load finished, though its sign-in is still running");
        AssertWrapper.AreSame(firstLoad, secondLoad, "The second load started its own load instead of joining the running one");
        AssertWrapper.AreEqual(1, _signInCalls, "Two loads signed in on top of each other");
    }

    [Test]
    public void LoadAfterAFinishedLoadSignsInAgain()
    {
        // GIVEN
        // A finished load is never reused, so the next screen gets fresh scores and a failed sign-in is tried again
        var firstLoad = PlayerStats.InitAllAsync();
        AssertWrapper.IsTrue(firstLoad.IsCompleted, "The first load did not finish, though every fake service had finished");

        // WHEN
        var secondLoad = PlayerStats.InitAllAsync();

        // THEN
        AssertWrapper.IsTrue(secondLoad.IsCompleted, "The second load did not finish, though every fake service had finished");
        AssertWrapper.IsFalse(secondLoad.IsFaulted, "The second load failed, though every fake service worked");
        AssertWrapper.AreEqual(2, _signInCalls, "The second load reused the first, finished load instead of signing in again");
    }

    [Test]
    public void LoadAfterAFailedLoadSignsInAgain()
    {
        // GIVEN
        // Offline on the menu: its load fails. A failed load is never reused, so the next screen tries again (#176)
        _shouldScoreLoadFail = true;
        var failedLoad = PlayerStats.InitAllAsync();
        AssertWrapper.IsTrue(failedLoad.IsCompleted, "The first load did not finish, though every fake service had finished");
        AssertWrapper.IsTrue(failedLoad.IsFaulted, "The first load hid the failed score load");
        _shouldScoreLoadFail = false;
        _scoresOnServer[PlayerStats._highScoreKey] = SAVED_HIGH_SCORE;

        // WHEN
        var secondLoad = PlayerStats.InitAllAsync();

        // THEN
        AssertWrapper.AreEqual(2, _signInCalls, "The second load reused the failed load instead of signing in again");
        AssertWrapper.IsTrue(secondLoad.IsCompleted, "The second load did not finish, though every fake service had finished");
        AssertWrapper.IsFalse(secondLoad.IsFaulted, "The second load failed, though every fake service worked this time");
        AssertWrapper.IsTrue(PlayerStats._highScore == SAVED_HIGH_SCORE, "The second load did not read the saved high score");
    }

    #region Fakes
    private void SetDailyThatIsANewBest()
    {
        PlayerStats.SetStartDaily();
        PlayerStats._dailyScore = RUN_SCORE;
        PlayerStats._highScore = RUN_SCORE;
        PlayerStats._isNewHighScore = true;
    }

    private Task FakeSignIn()
    {
        _signInCalls++;

        if (_pendingSignIn != null)
        {
            return _pendingSignIn.Task;
        }

        return Task.CompletedTask;
    }

    private Task FakeSaveScores(Dictionary<string, object> scores)
    {
        _saveCalls++;

        if (_pendingSave != null)
        {
            return _pendingSave.Task;
        }

        if (_shouldHighScoreSaveFail && scores.ContainsKey(PlayerStats._highScoreKey))
        {
            return Task.FromException(new Exception(HIGH_SCORE_SAVE_FAILURE));
        }

        if (_shouldDailyScoreSaveFail && scores.ContainsKey(PlayerStats._dailyScoreKey))
        {
            return Task.FromException(new Exception(DAILY_SCORE_SAVE_FAILURE));
        }

        _savedKeys.AddRange(scores.Keys);
        return Task.CompletedTask;
    }

    private Task<Dictionary<string, int>> FakeLoadScores(HashSet<string> keys)
    {
        if (_shouldScoreLoadFail)
        {
            return Task.FromException<Dictionary<string, int>>(new Exception(SCORE_LOAD_FAILURE));
        }

        return Task.FromResult(_scoresOnServer);
    }

    private Task FakeSubmitScore(string leaderboardId, double score)
    {
        _submittedLeaderboards.Add(leaderboardId);
        return Task.CompletedTask;
    }

    private Task FakeSaveDailyTrack(DateTime israelDay, string trackJson)
    {
        return Task.CompletedTask;
    }

    private void CountPlayerStatsWarning(string message, string stackTrace, LogType type)
    {
        if (type == LogType.Warning && message.StartsWith(PLAYER_STATS_LOG_PREFIX))
        {
            _playerStatsWarnings++;
        }
    }
    #endregion
}
