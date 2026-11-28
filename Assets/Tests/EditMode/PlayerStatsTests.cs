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
    private const int RUN_SCORE = 40;
    private const int SAVED_HIGH_SCORE = 70;

    private List<string> _savedKeys;
    private List<string> _submittedLeaderboards;
    private int _saveCalls;
    private bool _shouldHighScoreSaveFail, _shouldDailyScoreSaveFail;
    private TaskCompletionSource<bool> _pendingSave;
    private Dictionary<string, int> _scoresOnServer;

    [SetUp]
    public void SetUp()
    {
        PlayerStats.SetDefaultPlayerStatsValues();

        _savedKeys = new List<string>();
        _submittedLeaderboards = new List<string>();
        _saveCalls = 0;
        _shouldHighScoreSaveFail = false;
        _shouldDailyScoreSaveFail = false;
        _pendingSave = null;
        _scoresOnServer = new Dictionary<string, int>();

        PlayerStats.SetServices(FakeSignIn, FakeSaveScores, FakeLoadScores, FakeSubmitScore, FakeSaveDailyTrack);
    }

    [TearDown]
    public void TearDown()
    {
        PlayerStats.ResetServices();
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
        AssertWrapper.IsTrue(_savedKeys.Contains(PlayerStats._highScoreKey), "The high score was not saved");
        AssertWrapper.IsTrue(_submittedLeaderboards.Contains(Constants.HIGHSCORE_LEADERBOARD_ID), "The high score was not sent to its leaderboard");
        AssertWrapper.IsFalse(PlayerStats._isNewHighScore, "The high score was saved, but its flag stayed on");
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
    #endregion
}
