using Assets.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyExplosionInitiator : MonoBehaviour
{
    [SerializeField]
    private GameObject _exemyExplosionPrefab;

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
        Instantiate(_exemyExplosionPrefab, arguments.Position, Quaternion.identity);
    }
    #endregion
}
