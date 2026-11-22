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
        SeedfulRandomGenerator randomGenerator = enemyToKill.GetComponent<EnemySharedBehavior>()._seedfulRandomGenerator;

        TryInvokeValuableDropEvent(enemyToKill, randomGenerator);
        TryInvokeItemDropEvent(enemyToKill, randomGenerator);
        InvokeExplosionEvent(enemyToKill);
        Destroy(enemyToKill);
    }

    private void TryInvokeValuableDropEvent(GameObject enemyToKill, SeedfulRandomGenerator randomGenerator)
    {
        var potentialValuableDrops = enemyToKill.GetComponent<PotentialValuableDrops>().GetValuablesToChances();
        foreach (var potentialValuableDrop in potentialValuableDrops)
        {
            float randomSeededNumber = randomGenerator.RangeZeroToOne();
            if (randomSeededNumber <= potentialValuableDrop.Value)
            {
                Vector2 dropForce = randomGenerator.GetInsideUnitCircle() * enemyToKill.transform.localScale.x;
                ValuableDropEventArguments valuableDropEventArguments = new(enemyToKill.GetComponent<EnemyDropper>().GetDropper(), enemyToKill.transform.position, dropForce, potentialValuableDrop.Key);
                ValuableDropEvent.Invoke(EventName.NEW_VALUABLE_DROP, this, valuableDropEventArguments);
                return;
            }
        }
    }

    // TODO: refactor? - first check chances, then drop if needed
    private void TryInvokeItemDropEvent(GameObject enemyToKill, SeedfulRandomGenerator randomGenerator)
    {
        Vector2 dropForce = randomGenerator.GetInsideUnitCircle() * enemyToKill.transform.localScale.x;
        ItemDropEventArguments itemDropEventArguments = new(enemyToKill.GetComponent<EnemyDropper>().GetDropper(), enemyToKill.transform.position, dropForce);
        ItemDropEvent.Invoke(EventName.NEW_ITEM_DROP, this, itemDropEventArguments);
    }

    private void InvokeExplosionEvent(GameObject enemyToKill)
    {
        EnemyExplosionEventArguments enemyExplosionEventArguments = new(enemyToKill.transform.position);
        EnemyExplosionEvent.Invoke(EventName.ENEMY_EXPLOSION, this, enemyExplosionEventArguments);
    }
    #endregion
}