using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Leaderboards;
using UnityEngine;
using Unity.Services.Core;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

public static class PlayerStats
{
    public static bool _isNewHighScore;
    public static string _playerId;

    public static bool _isDailyRun;
    public static DateTime _dailyDate;
    public static Dictionary<string, object> _dailySeeds;
    public static int? _dailySeed;
    public static int? _highScore, _dailyScore;

    public static string _dailyScoreKey, _highScoreKey, _dailySeedKey;
    public static Dictionary<string, object> _playerHighScore, _playerDailyScore;
    const string HIGHSCORE_LEADERBOARD_ID = "1";

    public async static Task InitAllAsync()
    {
        Debug.Log("Starting InitAllAsync");

        await TrySaveNewHighScoreAsync();
        await TrySaveNewDailyScoreAsync();

        _isNewHighScore = false;
        _highScore = null;
        _playerId = "";
        _isDailyRun = false;
        _dailyDate = DateTime.Now;
        _dailyScore = null;
        _dailyScoreKey = "DailyScore_" + DateTime.Now.ToString("dd-MM-yy");
        // TODO: fix - move from Player to Game
        _dailySeedKey = "DailySeed_04-05-24"; // "DailySeed_" + DateTime.Now.ToString("dd-MM-yy");
        _highScoreKey = "HighScore";
        _playerHighScore = new Dictionary<string, object>();
        _playerDailyScore = new Dictionary<string, object>();
        SetAllDailySeeds();

        await SyncPlayerFromServerAsync();

        Debug.Log("Finished InitAllAsync");
    }


    private static void SetAllDailySeeds()
    {
        // TODO: Fix
        _dailySeeds = new Dictionary<string, object>();
        _dailySeeds["12-06-24"] = 1;
        _dailySeeds["13-06-24"] = 2;
        _dailySeeds["14-06-24"] = 3;
        _dailySeeds["15-06-24"] = 4;
        _dailySeeds["16-06-24"] = 5;
        _dailySeeds["17-06-24"] = 6;
        _dailySeeds["18-06-24"] = 7;
        _dailySeeds["19-06-24"] = 8;
        _dailySeeds["20-06-24"] = 9;
        _dailySeeds["21-06-24"] = 10;
        // TODO: Fix
    }


    public static async Task SyncPlayerFromServerAsync()
    {
        try
        {

            Debug.Log("Starting SyncPlayerFromServerAsync");

            Debug.Log("Starting SyncPlayerFromServerAsync");
            Debug.Log($"Current Player ID: {Unity.Services.Authentication.AuthenticationService.Instance.PlayerId}");

            Debug.Log($"Is Unity Services Initialized: {UnityServices.State}");
            Debug.Log($"Attempting to load keys: {_highScoreKey}, {_dailyScoreKey}, {_dailySeedKey}");

            var playerStats = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string>() { _highScoreKey, _dailyScoreKey, _dailySeedKey });

            Debug.Log($"Received playerStats with {playerStats.Count} entries");
            foreach (var playerStat in playerStats)
            {
                Debug.Log($"Key: {playerStat.Key}, Value: {playerStat.Value?.Value}");
            }

            if (playerStats.TryGetValue(_dailyScoreKey, out var DailyScoreObject))
            {
                _dailyScore = DailyScoreObject.Value.GetAs<int>();
            }

            if (playerStats.TryGetValue(_highScoreKey, out var HighScoreObject))
            {
                _highScore = HighScoreObject.Value.GetAs<int>();
            }

            if (playerStats.TryGetValue(_dailySeedKey, out var DailySeedObject))
            {
                _dailySeed = DailySeedObject.Value.GetAs<int>();
            }
            Debug.Log("Finished SyncPlayerFromServerAsync");

        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SyncPlayerFromServerAsync: {e.Message}");
            throw;
        }
    }

    private static async Task testSaveAndLoad()
    {
        // First try to save some test data
        var testData = new Dictionary<string, object>
            {
                { "DailyScore_02-05-24", 60 },
                { "DailyScore_04-05-24", 70 },
                { "DailyScore_24-10-24", 40 },
                { "DailySeed_03-05-24", 1539402699 },
                { "DailySeed_04-05-24", 1823848950 },
                { "HighScore", 40 }
            };
        await CloudSaveService.Instance.Data.Player.SaveAsync(testData);
        Debug.Log("Test data saved");

        // Then try to load it back
        var testLoad = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>() { "test_key" });
        Debug.Log($"Test load result count: {testLoad.Count}");
    }

    public static void SetStartDaily()
    {
        _isDailyRun = true;
        _dailyDate = DateTime.Now;
    }

    public static async Task SaveDailyScoreAsync()
    {
        _playerDailyScore = new Dictionary<string, object>();
        _playerDailyScore[_dailyScoreKey] = _dailyScore;

        await CloudSaveService.Instance.Data.Player.SaveAsync(_playerDailyScore);
    }

    public static async Task SavePlayerHighScoreAsync()
    {
        _playerHighScore = new Dictionary<string, object>();
        _playerHighScore[_highScoreKey] = _highScore;

        await CloudSaveService.Instance.Data.Player.SaveAsync(_playerHighScore);
        await LeaderboardsService.Instance.AddPlayerScoreAsync(HIGHSCORE_LEADERBOARD_ID, (double)_highScore);
    }

    internal static async Task TrySaveNewDailyScoreAsync()
    {
        if (_isDailyRun)
        {
            await SaveDailyScoreAsync();
            _isDailyRun = false;
        }
    }

    internal static async Task TrySaveNewHighScoreAsync()
    {
        if (_isNewHighScore)
        {
            await SavePlayerHighScoreAsync();
            _isNewHighScore = false;
        }
    }

}