using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Leaderboards;


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
        var playerStats = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>() { _highScoreKey, _dailyScoreKey, _dailySeedKey });

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