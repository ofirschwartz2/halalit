using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Text _highScoreText;
    [SerializeField]
    private Text _dailyScoreText;

    private Dictionary<string, object> _playerHighScore;
    private Dictionary<string, object> _playerDailyScore;
    private string _dailyScoreKey, _highScoreKey;

    private async void Start()
    {
        _dailyScoreKey = "DailyScore_" + DateTime.Now.ToString("dd-MM-yy");
        _highScoreKey = "HighScore";
        await Authentication.InitializationTask;

        await TryToUpdatePlayerStatsAsync();
        await GetPlayerStatsAsync();
        SetHighScoreText();
        SetDailyScoreText();

        PlayerStats._isDaily = false;
        PlayerStats._newHighScore = false;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneName.PLAYGROUND.GetDescription());
    }

    public void StartDaily()
    {
        PlayerStats._isDaily = true;
        PlayerStats._dailyDate = DateTime.Now;
        StartGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public async Task GetPlayerStatsAsync()
    {
        var playerStats = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>() { _highScoreKey, _dailyScoreKey });
        SetPlayerStat(playerStats);

        PlayerStats.SetPlayerStat(playerStats, _dailyScoreKey);
        if (playerStats.TryGetValue(_dailyScoreKey, out var DailyScoreObject))
        {
            PlayerStats._dailyScore = DailyScoreObject.Value.GetAs<int>();
        }

        if (playerStats.TryGetValue(_highScoreKey, out var HighScoreObject))
        {
            PlayerStats._highScore = HighScoreObject.Value.GetAs<int>();
        }
    }

    private void SetPlayerStat(Dictionary<string, Item> playerStats)
    {
        
    }

    public void SetHighScoreText()
    {
        if (PlayerStats._highScore != 0)
        {
            _highScoreText.text = "High Score: " + PlayerStats._highScore;
        }        
    }

    public void SetDailyScoreText()
    {
        if (PlayerStats._dailyScore != null)
        {
            _dailyScoreText.text = "Daily Score: " + PlayerStats._dailyScore;
        }
    }

    public async Task TryToUpdatePlayerStatsAsync()
    {
        await TrySetNewHighScoreAsync();
        await TrySetNewDailyScoreAsync();
    }

    private async Task TrySetNewDailyScoreAsync()
    {
        if (PlayerStats._isDaily)
        {
            _playerDailyScore = new Dictionary<string, object>();
            _playerDailyScore[_dailyScoreKey] = PlayerStats._dailyScore;

            await CloudSaveService.Instance.Data.Player.SaveAsync(_playerDailyScore);
            SetDailyScoreText();

            PlayerStats._isDaily = false;
        }
    }

    private async Task TrySetNewHighScoreAsync()
    {
        if (PlayerStats._newHighScore)
        {
            _playerHighScore = new Dictionary<string, object>();
            _playerHighScore["HighScore"] = PlayerStats._highScore;

            await CloudSaveService.Instance.Data.Player.SaveAsync(_playerHighScore);

            PlayerStats._newHighScore = false;
        }
    }
}