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

        TryInvokeValuableDropEvent(enemyToKill);
        TryInvokeItemDropEvent(enemyToKill);
        InvokeExplosionEvent(enemyToKill);
        Destroy(enemyToKill);
    }

    private void TryInvokeValuableDropEvent(GameObject enemyToKill)
    {
        var potentialValuableDrops = enemyToKill.GetComponent<PotentialValuableDrops>().GetValuablesToChances();
        var randomSeededNumbers = enemyToKill.GetComponent<RandomSeededNumbers>();
        float randomSeededNumber;
        foreach (var potentialValuableDrop in potentialValuableDrops)
        {
            randomSeededNumber = randomSeededNumbers.PopRandomSeededNumber();
            if (randomSeededNumber <= potentialValuableDrop.Value)
            {
                Vector2 dropForce = RandomGenerator.GetInsideUnitCircle() * enemyToKill.transform.localScale.x;
                ValuableDropEventArguments valuableDropEventArguments = new(enemyToKill.GetComponent<EnemyDropper>().GetDropper(), enemyToKill.transform.position, dropForce, potentialValuableDrop.Key);
                ValuableDropEvent.Invoke(EventName.NEW_VALUABLE_DROP, this, valuableDropEventArguments);
                return;
            }
        }
    }

    // TODO: refactor? - first check chances, then drop if needed
    private void TryInvokeItemDropEvent(GameObject enemyToKill)
    {
        Vector2 dropForce = RandomGenerator.GetInsideUnitCircle() * enemyToKill.transform.localScale.x;
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