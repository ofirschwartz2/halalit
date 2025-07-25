using Assets.Enums;
using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsPlayMode")]
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

/* There is no case when Halalit is out of the screen, causes bug in the tests
    #region Instadeath
    private void Update() 
    {
        if (!Utils.IsCenterInsideTheWorld(gameObject) && !Utils.IsCenterInExternalSafeIsland(transform.position)) 
        {
            GameOverEvent.InvokeGameOver(this, new());
        }
    }
    #endregion
*/

    #region Enemy destruction
    private void OnGameOver(object initiator, GameOverEventArguments arguments)
    {
        SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
    }
    #endregion
}