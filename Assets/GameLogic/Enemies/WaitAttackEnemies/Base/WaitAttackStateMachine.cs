using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class WaitAttackStateMachine : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private WaitAttackWait _waitAttackWait;
    [SerializeField]
    private WaitAttackAttack _waitAttackAttack;

    private WaitAttackEnemyState _followingEnemyState;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
        _waitAttackWait.SetWaiting();
        _followingEnemyState = WaitAttackEnemyState.WAITING;
    }

    void Update()
    {
        switch (_followingEnemyState)
        {
            case WaitAttackEnemyState.WAITING:
                Waiting();
                break;
            case WaitAttackEnemyState.ATTACKING:
                Attacking();
                break;
        }
    }

    private void Waiting()
    {
        if (!_waitAttackWait.ShouldStopWaiting())
        {
            _waitAttackWait.Wait();
        }
        else 
        {
            _waitAttackAttack.SetAttacking();
            _followingEnemyState = WaitAttackEnemyState.ATTACKING;
        }
    }

    private void Attacking()
    {
        if (!_waitAttackAttack.ShouldStopAttacking())
        {
            _waitAttackAttack.Attack();
        }
        else
        {
            _waitAttackWait.SetWaiting();
            _followingEnemyState = WaitAttackEnemyState.WAITING;
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        //TODO: refactor this. should this be in the EventHandler?
        if (EnemyUtils.ShouldKillMe(other))
        {
            Die();
        }
        else if (EnemyUtils.ShouldKnockEnemyBack(LayerMask.LayerToName(gameObject.layer), other))
        {
            EnemyUtils.KnockMeBack(_rigidBody, other);
        }
    }
    #endregion
}