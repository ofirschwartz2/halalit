using Assets.Enums;
using Assets.Utils;
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

    private async void Awake()
    {
        await PlayerInitialization();
        TrySetScoreTexts();
        TrySetDailyTwistText();

        _dailyButton.interactable = PlayerStats._dailyScore == null;

    }

    private static async Task PlayerInitialization() // TODO: FIX
    {
        if (Authentication.InitializationTask == null)
        {
            await Authentication.Initialize();
        }
        else
        {
            await Authentication.InitializationTask;
        }

        await PlayerStats.InitAllAsync();
    }

    #region ButtonMethods
    public void StartGame()
    {
        PlayerStats.SetStartNormal();
        LoadPlayground();
    }

    public void StartDaily()
    {
        if (!_dailyButton.interactable) return;

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
}