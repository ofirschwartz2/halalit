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

    #region Enemy destruction
    private void OnEnemyDeath(object initiator, DeathEventArguments arguments)
    {
        GameObject enemyToKill = ((MonoBehaviour)initiator).gameObject;

        InvokeItemDropEvent(enemyToKill);
        InvokeExplosionEvent(enemyToKill);
        Destroy(enemyToKill);
    }

    private void InvokeItemDropEvent(GameObject enemyToKill)
    {
        Vector2 dropForce = RandomGenerator.GetInsideUnitCircle() * enemyToKill.transform.localScale.x;
        DropEventArguments itemDropEventArguments = new(enemyToKill.GetComponent<EnemyDropper>().GetDropper(), enemyToKill.transform.position, dropForce);
        DropEvent.Invoke(EventName.NEW_ITEM_DROP, this, itemDropEventArguments);
    }

    private void InvokeExplosionEvent(GameObject enemyToKill)
    {
        EnemyExplosionEventArguments enemyExplosionEventArguments = new(enemyToKill.transform.position);
        EnemyExplosionEvent.Invoke(EventName.ENEMY_EXPLOSION, this, enemyExplosionEventArguments);
    }
    #endregion
}
