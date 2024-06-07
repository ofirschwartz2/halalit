using Assets.Enums;
using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsPlayMode")]
#endif

public class HalalitDeath : MonoBehaviour
{
    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        HalalitDeathEvent.HalalitDeath += OnHalalitDeath;
    }
    #endregion

    #region Destroy
    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    public void DestroyEventListeners()
    {
        HalalitDeathEvent.HalalitDeath -= OnHalalitDeath;
    }
    #endregion

    #region Instadeath
    private void Update() 
    {
        if (!Utils.IsCenterInsideTheWorld(gameObject) && !Utils.IsCenterInExternalSafeIsland(transform.position)) 
        {
            HalalitDeathEvent.InvokeHalalitDeath(this, new());
        }
    }
    #endregion

    #region Enemy destruction
    private void OnHalalitDeath(object initiator, HalalitDeathEventArguments arguments)
    {
        SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
    }
    #endregion
}