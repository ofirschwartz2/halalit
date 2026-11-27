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
    private ResultsCard _resultsCard;

    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        GameOverEvent.GameOver += OnGameOver;
    }

    // The card is found, not wired, so the Halalit needs no new field (#249)
    private void Start()
    {
        _resultsCard = FindObjectOfType<ResultsCard>();
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
        // Until the Playground has a wired results card, a death loads the menu as before (#249). An unwired card switches itself off
        if (_resultsCard == null || !_resultsCard.isActiveAndEnabled)
        {
            Debug.LogWarning("GameOver: the Playground has no wired ResultsCard, so the menu loads.");
            SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
            return;
        }

        _resultsCard.Open();
    }
    #endregion
}