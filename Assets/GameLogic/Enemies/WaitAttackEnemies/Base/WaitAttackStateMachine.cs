using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using System;
using UnityEngine;

public class WaitAttackStateMachine : MonoBehaviour, IPausableEnemyBehaviour, IInterruptibleEnemyBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private WaitAttackWait _waitAttackWait;
    [SerializeField]
    private WaitAttackAttack _waitAttackAttack;
    
    private WaitAttackEnemyState _followingEnemyState;
    private float _stunEnd;

    public event Action AttackFinished;

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
            case WaitAttackEnemyState.STUNNED:
                Stunned();
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
            AttackFinished?.Invoke();
        }
    }

    // Stands still, with no force and no spin, until the stun ends. Then a fresh wait starts.
    private void Stunned()
    {
        if (Utils.ShouldDie(_stunEnd))
        {
            _waitAttackWait.SetWaiting();
            _followingEnemyState = WaitAttackEnemyState.WAITING;
        }
    }

    #region Interrupt
    // Interrupt (#238): open while the wait winds up, in its last windupTime seconds. The enabled check matters: this machine
    // has no OnDisable, so its state stays WAITING while Wounded Retreat has it paused.
    public bool IsInInterruptWindow(float windupTime)
    {
        return enabled && _followingEnemyState == WaitAttackEnemyState.WAITING && _waitAttackWait.IsWindingUp(windupTime);
    }

    // Interrupt (#238): cancels the coming charge and stands still for stunDuration.
    public void InterruptAttack(float stunDuration)
    {
        _rigidBody.velocity = Vector2.zero;
        _stunEnd = Utils.GetEndOfLifeTime(stunDuration);
        _followingEnemyState = WaitAttackEnemyState.STUNNED;
    }
    #endregion

#if UNITY_EDITOR
    internal WaitAttackEnemyState GetState()
    {
        return _followingEnemyState;
    }
#endif
}