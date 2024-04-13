using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Text _highScoreText;

    private void Awake()
    {
        SetEventListeners();
        SetHighScore();
    }

    private void SetEventListeners()
    {
        HalalitDeathEvent.HalalitDeath += HandleDeath;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void HandleDeath(object initiator, HalalitDeathEventArguments arguments)
    {
        SetHighScore();
    }

    public void SetHighScore()
    {
        _highScoreText.text = "High Score: " + Score._highScore;
    }
}