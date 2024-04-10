using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class HalalitDestructor : MonoBehaviour
{

    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        DeathEvent.HalalitDeath += OnHalalitDeath;
    }
    #endregion

    #region Destroy

    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    public void DestroyEventListeners()
    {
        DeathEvent.HalalitDeath -= OnHalalitDeath;
    }
    #endregion

    #region Halalit destruction
    private void OnHalalitDeath(object initiator, TargetDeathEventArguments arguments)
    {
        GameObject enemyToKill = ((MonoBehaviour)initiator).gameObject;

        Destroy(enemyToKill);
    }

}
