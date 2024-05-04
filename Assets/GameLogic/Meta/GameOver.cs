using Assets.Enums;
using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

public class GameOver : MonoBehaviour
{
    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        GameOverEvent.GameOver += OnGameOver;
    }
    #endregion

    #region Destroy

    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    public void DestroyEventListeners()
    {
        GameOverEvent.GameOver -= OnGameOver;
    }
    #endregion

    #region GameOver
    private void OnGameOver(object initiator, GameOverEventArguments arguments)
    {
        GameObject.FindGameObjectWithTag(Tag.SCORE.GetDescription()).GetComponent<Score>().SetGameStats();
        
        SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
    }

    #endregion
}