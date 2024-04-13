using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [SerializeField]
    private int _highScore;

    [SerializeField]
    private Text _scoreText;

    [SerializeField]
    private Text _highScoreText;

    private void Awake()
    {
        _highScore = 0;
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        HalalitDeathEvent.HalalitDeath += TrySetHighScore;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void TrySetHighScore(object initiator, HalalitDeathEventArguments arguments)
    {
        var deathScore = _scoreText.GetComponent<Score>().GetScore();
        if (deathScore > _highScore)
        {
            _highScore = deathScore;
        }
    }
}