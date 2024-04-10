using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [SerializeField]
    private int _highScore;

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void TrySetHighScore(int score)
    {
        if (score > _highScore)
        {
            _highScore = score;
        }
    }
}