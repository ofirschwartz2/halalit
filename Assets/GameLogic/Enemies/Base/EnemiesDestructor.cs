using Assets.Enums;
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

    #region Asteroid destruction
    private void OnEnemyDeath(object initiator, DeathEventArguments arguments)
    {
        GameObject enemyToKill = ((MonoBehaviour)initiator).gameObject;

        InvokeItemDropEvent(enemyToKill);
        Destroy(enemyToKill);
    }

    private void InvokeItemDropEvent(GameObject enemyToKill)
    {
        Vector2 dropForce = Random.onUnitSphere * enemyToKill.transform.localScale.x;
        DropEventArguments itemDropEventArguments = new(enemyToKill.GetComponent<EnemyDropper>().GetDropper(), enemyToKill.transform.position, dropForce);
        DropEvent.Invoke(EventName.NEW_ITEM_DROP, this, itemDropEventArguments);
    }
    #endregion
}
