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

    const string HIGHSCORE_LEADERBOARD_ID = "1"; // TODO: move 

    public static bool _isNewHighScore, _isDailyRun;
    public static int? _dailySeed, _highScore, _dailyScore;
    public static string _playerId, _dateInFormat, _dailyScoreKey, _highScoreKey;
    public static Dictionary<string, int> _dailySeeds;
    public static Dictionary<string, object> _playerHighScore, _playerDailyScore;

#region Init
    public async static Task InitAllAsync()
    {
        Debug.Log("Starting InitAllAsync");

        await TrySaveHighScoresAsync();

        SetDefaultValues();

        SetDailySeeds();

        await SyncPlayerFromServerAsync();

        Debug.Log("Finished InitAllAsync");
    }

    internal static void SetDefaultValues()
    {
        _playerId = "";
        _dateInFormat = DateTime.Now.ToString("dd-MM-yy");
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
            Debug.LogError($"Error in SyncPlayerFromServerAsync: {e.Message}");
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
    
    internal static async Task TrySaveHighScoresAsync()
    {
        await TrySaveNewHighScoreAsync();
        await TrySaveNewDailyScoreAsync();
    }

    internal static async Task TrySaveNewDailyScoreAsync()
    {
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
    }

    internal static async Task TrySaveNewHighScoreAsync()
    {
        if (_isNewHighScore)
        {
            await SavePlayerHighScoreAsync();
            _isNewHighScore = false;
        }
    }

    public static async Task SavePlayerHighScoreAsync()
    {
        _playerHighScore = new Dictionary<string, object>();
        _playerHighScore[_highScoreKey] = _highScore;

        await CloudSaveService.Instance.Data.Player.SaveAsync(_playerHighScore);
        await LeaderboardsService.Instance.AddPlayerScoreAsync(HIGHSCORE_LEADERBOARD_ID, (double)_highScore);
    }

#endregion

#region Daily

    public static void SetDailySeeds()
    {
        _dailySeeds = GetDailySeeds();
        _dailySeed = GetDailySeed(_dateInFormat);
    }

    private static Dictionary<string, int> GetDailySeeds()
    {
        var dailySeedKeyPrefix = "DailySeed_";

    return new Dictionary<string, int>()
    {
        { dailySeedKeyPrefix + "13-12-24", 1823848950 },
        { dailySeedKeyPrefix + "14-12-24", -542789631 },
        { dailySeedKeyPrefix + "15-12-24", 967123405 },
        { dailySeedKeyPrefix + "16-12-24", -234567890 },
        { dailySeedKeyPrefix + "17-12-24", 1357924680 },
        { dailySeedKeyPrefix + "18-12-24", -891234567 },
        { dailySeedKeyPrefix + "19-12-24", 456789123 },
        { dailySeedKeyPrefix + "20-12-24", -123456789 },
        { dailySeedKeyPrefix + "21-12-24", 2047483647 },
        { dailySeedKeyPrefix + "22-12-24", -987654321 },
        { dailySeedKeyPrefix + "23-12-24", 741852963 },
        { dailySeedKeyPrefix + "24-12-24", -369258147 },
        { dailySeedKeyPrefix + "25-12-24", 159753468 },
        { dailySeedKeyPrefix + "26-12-24", -852963741 },
        { dailySeedKeyPrefix + "27-12-24", 246813579 },
        { dailySeedKeyPrefix + "28-12-24", -147258369 },
        { dailySeedKeyPrefix + "29-12-24", 963852741 },
        { dailySeedKeyPrefix + "30-12-24", -741852963 },
        { dailySeedKeyPrefix + "31-12-24", 357159852 },
        { dailySeedKeyPrefix + "01-01-25", -258369147 }
        };
    }

    public static int GetDailySeed(string _dateInFormat)
    {
        string key = $"DailySeed_{_dateInFormat}";
        if (_dailySeeds.TryGetValue(key, out int value) && value is int dailySeed)
        {
            return dailySeed;
        }
        return 0;
    }

    public static void SetStartDaily()
    {
        _isDailyRun = true;
    }

#endregion

}