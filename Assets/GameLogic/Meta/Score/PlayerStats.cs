using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Services.CloudSave.Models;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

public class PlayerStats : MonoBehaviour
{
    public static bool _newHighScore;
    public static int _highScore;
    public static string _playerId;

    public static bool _isDaily;
    public static DateTime _dailyDate;
    public static int? _dailyScore;
    //private string _dailyScoreKey, _highScoreKey;

    private void Start()
    {
        _newHighScore = false;
        _highScore = 0;
        _playerId = "";
        _isDaily = false;
        _dailyDate = DateTime.Now;
        _dailyScore = null;
        _dailyScoreKey = "DailyScore_" + DateTime.Now.ToString("dd-MM-yy");
        _highScoreKey = "HighScore";
    }

    private void SetPlayerStat(Dictionary<string, Item> playerStats, string _highScoreKey)
    {
        if (playerStats.TryGetValue(_highScoreKey, out var HighScoreObject))
        {
            PlayerStats._highScore = HighScoreObject.Value.GetAs<int>();
        }
    }
}