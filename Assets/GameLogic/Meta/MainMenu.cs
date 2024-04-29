using Assets.Enums;
using Assets.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Text _highScoreText;

    private void Awake()
    {
        SetHighScore();
    }

    /*
    private async void Start()
    {
        //await UnityServices.InitializeAsync();
    }
    */

    public void StartGame()
    {
        SceneManager.LoadScene(SceneName.PLAYGROUND.GetDescription());
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetHighScore()
    {
        _highScoreText.text = "High Score: " + HighScore._highScore;
    }
}