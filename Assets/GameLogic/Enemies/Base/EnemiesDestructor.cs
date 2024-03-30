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

        TryDropValuable(enemyToKill);
        TryInvokeItemDropEvent(enemyToKill);
        InvokeExplosionEvent(enemyToKill);
        Destroy(enemyToKill);
    }

    private void TryDropValuable(GameObject enemyToKill)
    {
        var potentialValuableDrops = enemyToKill.GetComponent<PotentialValuableDrops>().GetValuablesWithChances();
        var randomSeededNumbers = enemyToKill.GetComponent<RandomSeededNumbers>();
        float randomSeededNumber;
        foreach (var potentialValuableDrop in potentialValuableDrops)
        {
            randomSeededNumber = randomSeededNumbers.PopRandomSeededNumber();
            if (randomSeededNumber <= potentialValuableDrop.Value)
            {
                var valuable = Instantiate(potentialValuableDrop.Key, enemyToKill.transform.position, Quaternion.identity);
                Vector2 dropForce = RandomGenerator.GetInsideUnitCircle() * enemyToKill.transform.localScale.x;
                valuable.GetComponent<Rigidbody2D>().AddForce(dropForce, ForceMode2D.Impulse);
                return;
            }
        }
    }

    // TODO: refactor? - first check chances, then drop if needed
    private void TryInvokeItemDropEvent(GameObject enemyToKill)
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
