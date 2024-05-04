using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Leaderboards;
using UnityEngine.SocialPlatforms.Impl;


#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

public static class PlayerStats
{
    public static bool _isNewHighScore;
    public static string _playerId;

    public static bool _isDailyRun;
    public static DateTime _dailyDate;
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
        _dailySeedKey = "DailySeed_" + DateTime.Now.ToString("dd-MM-yy");
        _highScoreKey = "HighScore";
        _playerHighScore = new Dictionary<string, object>();
        _playerDailyScore = new Dictionary<string, object>();
        //await SetAllDailySeeds();

        await SyncPlayerFromServerAsync();

    }

    /*
    private static async Task SetAllDailySeeds()
    {
        var allDailySeeds = new Dictionary<string, object>();
        allDailySeeds["DailySeed_03-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_04-05-24"] = SeedlessRandomGenerator.GetNumber();
        
        allDailySeeds["DailySeed_05-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_06-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_07-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_08-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_09-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_10-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_11-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_12-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_13-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_14-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_15-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_16-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_17-05-24"] = SeedlessRandomGenerator.GetNumber();
        allDailySeeds["DailySeed_18-05-24"] = SeedlessRandomGenerator.GetNumber();
        
        await CloudSaveService.Instance.Data.ForceSaveAsync(allDailySeeds);
        // not doing global, but JUST FOR THE PLAYER...
    }
    */

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