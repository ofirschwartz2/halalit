using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Leaderboards;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.CloudSave.Models;


#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

public static class PlayerStats
{
    public static bool _isNewHighScore;
    public static string _playerId;

    public static bool _isDailyRun;
    public static DateTime _dailyDate;
    public static Dictionary<string, int> _dailySeeds;
    public static int? _dailySeed;
    public static int? _highScore, _dailyScore;

    public static string _dailyScoreKey, _highScoreKey;
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
        _highScoreKey = "HighScore";
        _dailySeeds = new Dictionary<string, int>()
        {
            { "DailySeed_13-12-24", 1823848950 },
            { "DailySeed_14-12-24", -542789631 },
            { "DailySeed_15-12-24", 967123405 },
            { "DailySeed_16-12-24", -234567890 },
            { "DailySeed_17-12-24", 1357924680 },
            { "DailySeed_18-12-24", -891234567 },
            { "DailySeed_19-12-24", 456789123 },
            { "DailySeed_20-12-24", -123456789 },
            { "DailySeed_21-12-24", 2047483647 },
            { "DailySeed_22-12-24", -987654321 },
            { "DailySeed_23-12-24", 741852963 },
            { "DailySeed_24-12-24", -369258147 },
            { "DailySeed_25-12-24", 159753468 },
            { "DailySeed_26-12-24", -852963741 },
            { "DailySeed_27-12-24", 246813579 },
            { "DailySeed_28-12-24", -147258369 },
            { "DailySeed_29-12-24", 963852741 },
            { "DailySeed_30-12-24", -741852963 },
            { "DailySeed_31-12-24", 357159852 },
            { "DailySeed_01-01-25", -258369147 }
        };
        _playerHighScore = new Dictionary<string, object>();
        _playerDailyScore = new Dictionary<string, object>();

        await SyncPlayerFromServerAsync();

        Debug.Log("Finished InitAllAsync");
    }


   
    private static int GenerateRandomSeed()
    {
        System.Random random = new System.Random();
        return random.Next(Int32.MinValue, Int32.MaxValue);
    }

    public static async Task SyncPlayerFromServerAsync()
    {
        try
        {

            Debug.Log("Starting SyncPlayerFromServerAsync");

            Debug.Log("Starting SyncPlayerFromServerAsync");
            Debug.Log($"Current Player ID: {Unity.Services.Authentication.AuthenticationService.Instance.PlayerId}");

            Debug.Log($"Is Unity Services Initialized: {UnityServices.State}");
            Debug.Log($"Attempting to load keys: {_highScoreKey}, {_dailyScoreKey}");

            var playerStats = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string>() { _highScoreKey, _dailyScoreKey });

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


            _dailySeed = GetDailySeed(DateTime.Now);
            Debug.Log("Finished SyncPlayerFromServerAsync");

        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SyncPlayerFromServerAsync: {e.Message}");
            throw;
        }
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

    public static int GetDailySeed(DateTime date)
    {
        string key = $"DailySeed_{date.ToString("dd-MM-yy")}";
        if (_dailySeeds.TryGetValue(key, out int value) && value is int intValue)
        {
            return intValue;
        }
        return 0;
    }

}