using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class EnemiesDestructor : MonoBehaviour
{

    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        DeathEvent.EnemyDeath += OnEnemyDeath;
    }
    #endregion

    #region Destroy

    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    public void DestroyEventListeners()
    {
        DeathEvent.EnemyDeath -= OnEnemyDeath;
    }
    #endregion

    #region Enemy destruction
    private void OnEnemyDeath(object initiator, TargetDeathEventArguments arguments)
    {
        GameObject enemyToKill = ((MonoBehaviour)initiator).gameObject;

        Destroy(enemyToKill);
    }

    #endregion
}