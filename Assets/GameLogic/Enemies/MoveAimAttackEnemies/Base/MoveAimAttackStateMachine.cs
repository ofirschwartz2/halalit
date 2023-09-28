using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class MoveAimAttackStateMachine : MonoBehaviour
{
    [SerializeField]
    private MoveAimAttackEnemyState _moveAimAttackEnemyState;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private MoveAimAttackMove _moveAimAttackMove;
    [SerializeField]
    private MoveAimAttackAim _moveAimAttackAim;
    [SerializeField]
    private MoveAimAttackAttack _moveAimAttackAttack;

    void Start()
    {
        _moveAimAttackMove.SetDirection();
        _moveAimAttackMove.SetMoving();
    }

    void FixedUpdate()
    {
        switch (_moveAimAttackEnemyState)
        {
            case MoveAimAttackEnemyState.MOVING:
                Moving();
                break;
            case MoveAimAttackEnemyState.AIMING:
                Aiming();
                break;
            case MoveAimAttackEnemyState.ATTACKING:
                Attacking();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Moving()
    {
        if (_moveAimAttackMove.DidMovingTimePass())
        {
            _moveAimAttackAim.SetAiming();
            _moveAimAttackEnemyState = MoveAimAttackEnemyState.AIMING;
        }
        else 
        {
            _moveAimAttackMove.MovingState();
        }
    }

    private void Aiming()
    {
        if (_moveAimAttackAim.DidAimingTimePass())
        {
            _moveAimAttackAttack.SetAttacking();
            _moveAimAttackEnemyState = MoveAimAttackEnemyState.ATTACKING;
        } 
        else
        {
            _moveAimAttackAim.AimingState(transform);
        }
    }

    private void Attacking() 
    {
        if (_moveAimAttackAttack.DidAttackingTimePass())
        {
            _moveAimAttackMove.SetMoving();
            _moveAimAttackEnemyState = MoveAimAttackEnemyState.MOVING;
        }
        else
        {
            _moveAimAttackAttack.AttackingState(transform);
        }
    }

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.DidHitEdge(other.gameObject.tag))
        {
            _moveAimAttackMove.SetNewDirection(other.gameObject.tag);
        }
    }
    #endregion
}