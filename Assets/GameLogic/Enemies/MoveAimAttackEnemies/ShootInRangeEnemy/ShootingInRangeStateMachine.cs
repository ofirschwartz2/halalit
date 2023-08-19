using Assets.Animators;
using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class ShootingInRangeStateMachine : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private MoveAimAttackEnemyState _moveAimAttackEnemyState;
    [SerializeField]
    private MoveAimAttackMovement _moveAimAttackMovement;
    [SerializeField]
    private ShootingInRangeAim _shootingInRangeAim;
    [SerializeField]
    private ShootingInRangeAttack _shootingInRangeAttack;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _moveAimAttackMovement.SetDirection();
        _moveAimAttackMovement.SetMoving();
    }

    void Update()
    {
        switch (_moveAimAttackEnemyState)
        {
            case MoveAimAttackEnemyState.MOVING:
                if (_moveAimAttackMovement.DidMovingTimePass())
                {
                    _shootingInRangeAim.SetAiming();
                    _moveAimAttackEnemyState = MoveAimAttackEnemyState.AIMING;
                }
                _moveAimAttackMovement.MovingState();
                break;
            case MoveAimAttackEnemyState.AIMING:
                if (_shootingInRangeAim.DidAimingTimePass())
                {
                    _shootingInRangeAttack.SetAttacking();
                    _moveAimAttackEnemyState = MoveAimAttackEnemyState.ATTACKING;
                }
                _shootingInRangeAim.AimingState();
                break;
            case MoveAimAttackEnemyState.ATTACKING:
                if (_shootingInRangeAttack.DidAttackingTimePass()) // ELSE IF?
                {
                    _moveAimAttackMovement.SetMoving();
                    _moveAimAttackEnemyState = MoveAimAttackEnemyState.MOVING;
                }
                _shootingInRangeAttack.AttackingState();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Moving()
    {
        if (_moveAimAttackMovement.DidMovingTimePass())
        {
            _shootingInRangeAim.SetAiming();
            _moveAimAttackEnemyState = MoveAimAttackEnemyState.AIMING;
        }
        else
        {
            _moveAimAttackMovement.MovingState();
        }
    }
    private void Aiming()
    {
        if (_shootingInRangeAim.DidAimingTimePass())
        {
            _shootingInRangeAttack.SetAttacking();
            _moveAimAttackEnemyState = MoveAimAttackEnemyState.ATTACKING;
        }
        else
        {
            _shootingInRangeAim.AimingState();
        }
    }
    private void Attacking()
    {
        if (_shootingInRangeAttack.DidAttackingTimePass()) // ELSE IF?
        {
            _moveAimAttackMovement.SetMoving();
            _moveAimAttackEnemyState = MoveAimAttackEnemyState.MOVING;
        }
        else
        {
            _shootingInRangeAttack.AttackingState();
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
        else if (EnemyUtils.ShouldKnockMeBack(other))
        {
            EnemyUtils.KnockMeBack(_rigidBody, other);
        }
        else if (Utils.DidHitEdge(other.gameObject.tag))
        {
            _moveAimAttackMovement.SetNewDirection(other.gameObject.tag);
        }
    }
    #endregion
}