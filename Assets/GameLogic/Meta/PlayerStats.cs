using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Leaderboards;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.CloudSave.Models;
using Assets.Utils;


#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsEditMode")]
#endif

public static class PlayerStats
{

    public static bool _isNewHighScore, _isDailyRun;
    public static int? _dailySeed, _highScore, _dailyScore;
    public static string _dateInFormat, _dailyScoreKey, _highScoreKey;
    public static Dictionary<string, object> _playerHighScore, _playerDailyScore;
    public static DateTime _dailyDate;
    // Daily replay (#244): the daily's track as JSON, handed over by DailyTrackRecorder when the ship dies. Null after a normal run
    public static string _dailyTrack;

    // The online services, swapped for fakes by the tests (#175)
    private static Func<Task> _signIn;
    private static Func<Dictionary<string, object>, Task> _saveScores;
    private static Func<HashSet<string>, Task<Dictionary<string, int>>> _loadScores;
    private static Func<string, double, Task> _submitScore;
    private static Func<DateTime, string, Task> _saveDailyTrack;

    static PlayerStats()
    {
        UseRealServices();
    }

#region Init
    // The running load (#176). The menu and the leaderboard both load, so a second call joins it instead of signing in on top of it
    private static Task _load;

    // A finished load is never reused, so each screen gets fresh scores, and a failed sign-in is tried again on the next screen
    public static Task InitAllAsync()
    {
        if (_load == null || _load.IsCompleted)
        {
            _load = LoadAsync();
        }

        return _load;
    }

    private static async Task LoadAsync()
    {
        await _signIn();

        // The results card may still be saving the run (#249). This save joins its chain, so it never runs on top of it.
        // A failed save does not stop the reset below (#175). Otherwise the daily stays on, and the next PLAY becomes a daily
        try
        {
            await SaveRunScoresAsync();
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"PlayerStats: could not save the last run's scores. {exception.Message}");
        }

        SetDefaultPlayerStatsValues();

        SetDailySeeds();

        await SyncPlayerFromServerAsync();
    }

    internal static void SetDefaultPlayerStatsValues()
    {
        _dailyDate = DailySeed.GetToday();
        _dateInFormat = DailySeed.GetDateKey(_dailyDate);
        _isNewHighScore = false;
        _highScore = null;
        _isDailyRun = false;
        _dailyScore = null;
        _dailySeed = null;
        _dailyScoreKey = "DailyScore_" + _dateInFormat;
        _highScoreKey = "HighScore";
        _playerHighScore = new Dictionary<string, object>();
        _playerDailyScore = new Dictionary<string, object>();
        _dailyTrack = null;
    }
#endregion

#region Sync Player Info
    // A failure is not logged here (#176): the screen that awaits the load logs it once
    public static async Task SyncPlayerFromServerAsync()
    {
        var playerStats = await _loadScores(new HashSet<string>() { _highScoreKey, _dailyScoreKey });

        UpdateScoreFromPlayerStats(playerStats, _dailyScoreKey, ref _dailyScore);
        UpdateScoreFromPlayerStats(playerStats, _highScoreKey, ref _highScore);
    }

    private static void UpdateScoreFromPlayerStats(IDictionary<string, int> playerStats, string key, ref int? score)
    {
        if (playerStats.TryGetValue(key, out var savedScore))
        {
            score = savedScore;
        }
    }

    private static async Task<Dictionary<string, int>> LoadScoresFromCloudAsync(HashSet<string> keys)
    {
        var items = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
        var scores = new Dictionary<string, int>();

        foreach (var keyAndItem in items)
        {
            scores[keyAndItem.Key] = keyAndItem.Value.Value.GetAs<int>();
        }

        return scores;
    }
    
#endregion

#region Save Scores
    
    // The run's save, started by the results card when it opens (#249). A later save waits for it, so two saves never overlap
    private static Task _runScoresSave = Task.CompletedTask;

    internal static Task SaveRunScoresAsync()
    {
        _runScoresSave = SaveAfterAsync(_runScoresSave);
        return _runScoresSave;
    }

    private static async Task SaveAfterAsync(Task previousSave)
    {
        await WaitQuietlyAsync(previousSave);
        await TrySaveHighScoresAsync();
    }

    private static async Task WaitQuietlyAsync(Task save)
    {
        try
        {
            await save;
        }
        catch (Exception)
        {
            // The results card already logged this failure, and the flags still mark what is left to save
        }
    }

    // Each save is tried on its own (#175). A failed high score no longer skips the daily score, which left the daily on for the next run.
    // The first failure is passed on, so the results card still sees it. A second one is logged here
    internal static async Task TrySaveHighScoresAsync()
    {
        Exception firstFailure = null;

        try
        {
            await TrySaveNewHighScoreAsync();
        }
        catch (Exception exception)
        {
            firstFailure = exception;
        }

        try
        {
            await TrySaveNewDailyScoreAsync();
        }
        catch (Exception exception)
        {
            if (firstFailure == null)
            {
                firstFailure = exception;
            }
            else
            {
                Debug.LogWarning($"PlayerStats: could not save the daily score either. {exception.Message}");
            }
        }

        if (firstFailure != null)
        {
            ExceptionDispatchInfo.Capture(firstFailure).Throw();
        }
    }

    internal static async Task TrySaveNewDailyScoreAsync()
    {
        Debug.Log($"TrySaveNewDailyScoreAsync: _isDailyRun: {_isDailyRun}");

        if (_isDailyRun)
        {
            await SaveDailyScoreAsync();
            _isDailyRun = false;
        }
    }

    private static async Task SaveDailyScoreAsync()
    {
        _playerDailyScore = new Dictionary<string, object>();
        _playerDailyScore[_dailyScoreKey] = _dailyScore;

        await _saveScores(_playerDailyScore);

        if (DailyLeaderboard.ShouldSubmitDaily(_dailyScore, _dailyDate, DailySeed.GetToday()))
        {
            await TrySaveDailyTrackAsync();

            try
            {
                await _submitScore(Constants.DAILY_LEADERBOARD_ID, (double)_dailyScore);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in SaveDailyScoreAsync, daily leaderboard submit: {e.Message}");
            }
        }
    }

    // Daily replay (#244): saved before the score, so a friend who sees the score can also load the ship.
    // A failed track never blocks the score
    private static async Task TrySaveDailyTrackAsync()
    {
        if (_dailyTrack == null)
        {
            return;
        }

        try
        {
            await _saveDailyTrack(_dailyDate, _dailyTrack);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SaveDailyScoreAsync, daily track save: {e.Message}");
        }
    }

    internal static async Task TrySaveNewHighScoreAsync()
    {
        Debug.Log($"TrySaveNewHighScoreAsync: _isNewHighScore: {_isNewHighScore}");
        
        if (_isNewHighScore)
        {
            // A run that ends during the save can set a newer best (#249). Its flag then stays on, so the save chained after this one sends it
            int? highScoreBeingSaved = _highScore;
            await SavePlayerHighScoreAsync();

            if (_highScore == highScoreBeingSaved)
            {
                _isNewHighScore = false;
            }
        }
    }

    private static async Task SavePlayerHighScoreAsync()
    {
        // Copied before the first await, so Cloud Save and the leaderboard get the same best even when a run ends during the save (#249)
        int? highScore = _highScore;
        _playerHighScore = new Dictionary<string, object>();
        _playerHighScore[_highScoreKey] = highScore;

        await _saveScores(_playerHighScore);
        await _submitScore(Constants.HIGHSCORE_LEADERBOARD_ID, (double)highScore);
    }

#endregion

#region Daily

    public static void SetDailySeeds()
    {
        _dailySeed = DailySeed.GetSeed(_dailyDate);
    }

    public static void SetStartDaily()
    {
        _isDailyRun = true;
    }

    // PLAY always starts a normal run, even when a daily was left on (#175)
    public static void SetStartNormal()
    {
        _isDailyRun = false;
    }

#endregion

#region Services
    private static void UseRealServices()
    {
        _signIn = Authentication.Initialize;
        _saveScores = data => CloudSaveService.Instance.Data.Player.SaveAsync(data);
        _loadScores = LoadScoresFromCloudAsync;
        _submitScore = (leaderboardId, score) => LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
        _saveDailyTrack = DailyTrackStore.SaveAsync;
    }

#if UNITY_EDITOR
    internal static void SetServices(Func<Task> signIn, Func<Dictionary<string, object>, Task> saveScores, Func<HashSet<string>, Task<Dictionary<string, int>>> loadScores, Func<string, double, Task> submitScore, Func<DateTime, string, Task> saveDailyTrack)
    {
        _signIn = signIn;
        _saveScores = saveScores;
        _loadScores = loadScores;
        _submitScore = submitScore;
        _saveDailyTrack = saveDailyTrack;
    }

    internal static void ResetServices()
    {
        UseRealServices();
        _runScoresSave = Task.CompletedTask;
        _load = null;
    }

    // Read by the tests' guard (#307). Two delegates for the same static method compare equal
    internal static bool IsSignInReal()
    {
        return _signIn == (Func<Task>)Authentication.Initialize;
    }
#endif
#endregion

}