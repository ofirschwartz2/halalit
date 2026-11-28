using Assets.Enums;
using Assets.Utils;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Text _highScoreText;
    [SerializeField]
    private Text _dailyScoreText;
    [SerializeField]
    private Text _dailyTwistText;

    [SerializeField]
    private Button _dailyButton;

    // PLAY and DAILY wait for the scores (#176). A tap before the load ends is ignored
    private bool _isLoading;

    private async void Awake()
    {
        _isLoading = true;
        _dailyButton.interactable = false;
        bool didLoad = false;

        try
        {
            await PlayerStats.InitAllAsync();
            didLoad = true;
        }
        catch (Exception exception)
        {
            // A warning, not an error: being offline is expected
            Debug.LogWarning($"MainMenu: could not load your scores. {exception.Message}");
        }

        // The load can outlive the menu, for example after LDRBRD
        if (this == null)
        {
            return;
        }

        _isLoading = false;
        TrySetScoreTexts();
        TrySetDailyTwistText();

        // Without your scores the game cannot know whether you already played today, so DAILY stays off after a failed load
        _dailyButton.interactable = didLoad && PlayerStats._dailyScore == null;
    }

    #region ButtonMethods
    public void StartGame()
    {
        if (_isLoading) return;

        PlayerStats.SetStartNormal();
        LoadPlayground();
    }

    public void StartDaily()
    {
        if (_isLoading || !_dailyButton.interactable) return;

        PlayerStats.SetStartDaily();
        LoadPlayground();
    }

    public void LeaderBoard()
    {
        SceneManager.LoadScene(SceneName.LEADERBOARD.GetDescription());
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    #endregion

    private void LoadPlayground()
    {
        SceneManager.LoadScene(SceneName.PLAYGROUND.GetDescription());
    }

    #region Setters
    private void TrySetScoreTexts()
    {
        TrySetHighScoreText();
        TrySetDailyScoreText();
    }

    public void TrySetHighScoreText()
    {
        if (PlayerStats._highScore != null)
        {
            _highScoreText.text = "High Score: " + PlayerStats._highScore;
        }        
    }

    public void TrySetDailyScoreText()
    {
        if (PlayerStats._dailyScore != null)
        {
            _dailyScoreText.text = "Daily Score: " + PlayerStats._dailyScore;
        }
    }

    // Shown even when today's daily is already played. An unwired label is skipped, so the menu still works.
    // PlayerStats.InitAllAsync sets the daily's date before its seed, so a set seed means the date is set too
    private void TrySetDailyTwistText()
    {
        if (_dailyTwistText != null && PlayerStats._dailySeed != null)
        {
            _dailyTwistText.text = DailyTwists.PickForDate(PlayerStats._dailyDate).GetDisplayLine();
        }
    }
    #endregion

#if UNITY_EDITOR
    internal bool IsLoading()
    {
        return _isLoading;
    }

    internal bool IsDailyButtonOn()
    {
        return _dailyButton.interactable;
    }
#endif
}