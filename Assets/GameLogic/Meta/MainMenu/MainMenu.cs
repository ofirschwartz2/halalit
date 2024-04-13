using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [SerializeField]
    private int _highScore;

    [SerializeField]
    private Text highScoreText;

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
        var deathScore = Score.GetScore();
        if (deathScore > _highScore)
        {
            _highScore = deathScore;
        }
    }
}