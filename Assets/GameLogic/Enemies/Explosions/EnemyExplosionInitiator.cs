using UnityEngine;

public class EnemyExplosionInitiator : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyExplosionPrefab;

    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        EnemyExplosionEvent.NewEnemyExplosion += Explode;
    }
    #endregion

    #region Events actions
    private void Explode(object initiator, EnemyExplosionEventArguments arguments)
    {
        Instantiate(_enemyExplosionPrefab, arguments.Position, Quaternion.identity);
    }
    #endregion
}
