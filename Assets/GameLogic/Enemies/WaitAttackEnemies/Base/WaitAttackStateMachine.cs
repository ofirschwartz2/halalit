using Assets.Enums;
using UnityEngine;

public class WaitAttackStateMachine : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private WaitAttackWait _waitAttackWait;
    [SerializeField]
    private WaitAttackAttack _waitAttackAttack;
    
    private WaitAttackEnemyState _followingEnemyState;

    void Start()
    {
        _waitAttackWait.SetWaiting();
        _followingEnemyState = WaitAttackEnemyState.WAITING;
    }

    void FixedUpdate()
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

        SpeedLimiter.LimitSpeed(_rigidBody);
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
}