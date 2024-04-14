using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
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

    #region Enemy destruction
    private void OnHalalitDeath(object initiator, HalalitDeathEventArguments arguments)
    {
        SceneManager.LoadScene(0);
    }

    #endregion
}