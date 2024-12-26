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
    private Button _dailyButton;

    private async void Awake()
    {
        await PlayerInitialization();
        await TrySetNewDailyScoreAsync();
        await TrySetNewHighScoreAsync();
        TrySetScoreTexts();

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
        SceneManager.LoadScene(SceneName.PLAYGROUND.GetDescription());
    }

    public void StartDaily()
    {
        if (!_dailyButton.interactable) return;

        PlayerStats.SetStartDaily();
        StartGame();
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
    #endregion

    private async Task TrySetNewDailyScoreAsync()
    {
        if (PlayerStats._isDailyRun)
        {
            await PlayerStats.SaveDailyScoreAsync();

            PlayerStats._isDailyRun = false;
        }
    }

    private async Task TrySetNewHighScoreAsync()
    {
        if (PlayerStats._isNewHighScore)
        {
            await PlayerStats.SavePlayerHighScoreAsync();

            PlayerStats._isNewHighScore = false;
        }
    }
}