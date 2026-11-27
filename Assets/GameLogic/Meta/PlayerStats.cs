using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Leaderboards;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.CloudSave.Models;
using Assets.Utils;


#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

public static class PlayerStats
{

    public static bool _isNewHighScore, _isDailyRun;
    public static int? _dailySeed, _highScore, _dailyScore;
    public static string _playerId, _dateInFormat, _dailyScoreKey, _highScoreKey;
    public static Dictionary<string, object> _playerHighScore, _playerDailyScore;
    public static DateTime _dailyDate;

#region Init
    public async static Task InitAllAsync()
    {
        await InitAuthenticationAsync();

        // The results card may still be saving the run (#249). This save and the reset wait for it, so they never run on top of it
        await WaitQuietlyAsync(_runScoresSave);
        await TrySaveHighScoresAsync();

        SetDefaultPlayerStatsValues();

        SetDailySeeds();

        await SyncPlayerFromServerAsync();
    }

    private static async Task InitAuthenticationAsync()
    {
        if (Authentication.InitializationTask == null)
        {
            await Authentication.Initialize();
        }
        else
        {
            await Authentication.InitializationTask;
        }
    }

    internal static void SetDefaultPlayerStatsValues()
    {
        _playerId = "";
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
    }
#endregion

#region Sync Player Info
    public static async Task SyncPlayerFromServerAsync()
    {
        try
        {
            var playerStats = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>() { _highScoreKey, _dailyScoreKey });

            UpdateScoreFromPlayerStats(playerStats, _dailyScoreKey, ref _dailyScore);
            UpdateScoreFromPlayerStats(playerStats, _highScoreKey, ref _highScore);

        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SyncPlayerFromServerAsync: {e.Message}. _dailyScoreKey: {_dailyScoreKey}, _highScoreKey: {_highScoreKey}");
            throw;
        }
    }

    private static void UpdateScoreFromPlayerStats(IDictionary<string, Item> playerStats, string key, ref int? score)
    {
        if (playerStats.TryGetValue(key, out var scoreObject))
        {
            score = scoreObject.Value.GetAs<int>();
        }
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

    internal static async Task TrySaveHighScoresAsync()
    {
        await TrySaveNewHighScoreAsync();
        await TrySaveNewDailyScoreAsync();
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

    public static async Task SaveDailyScoreAsync()
    {
        _playerDailyScore = new Dictionary<string, object>();
        _playerDailyScore[_dailyScoreKey] = _dailyScore;

        await CloudSaveService.Instance.Data.Player.SaveAsync(_playerDailyScore);

        if (DailyLeaderboard.ShouldSubmitDaily(_dailyScore, _dailyDate, DailySeed.GetToday()))
        {
            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(Constants.DAILY_LEADERBOARD_ID, (double)_dailyScore);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in SaveDailyScoreAsync, daily leaderboard submit: {e.Message}");
            }
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

    public static async Task SavePlayerHighScoreAsync()
    {
        // Copied before the first await, so Cloud Save and the leaderboard get the same best even when a run ends during the save (#249)
        int? highScore = _highScore;
        _playerHighScore = new Dictionary<string, object>();
        _playerHighScore[_highScoreKey] = highScore;

        await CloudSaveService.Instance.Data.Player.SaveAsync(_playerHighScore);
        await LeaderboardsService.Instance.AddPlayerScoreAsync(Constants.HIGHSCORE_LEADERBOARD_ID, (double)highScore);
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

#endregion

}