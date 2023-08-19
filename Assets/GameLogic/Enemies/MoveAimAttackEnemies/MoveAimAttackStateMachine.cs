using Assets.Animators;
using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class MoveAimAttackStateMachine : MonoBehaviour
{
    [SerializeField]
    private MoveAimAttackEnemyState _moveAimAttackEnemyState;

    [SerializeField]
    private MoveAimAttackMovement _moveAimAttackMovement;
    [SerializeField]
    private IMoveAimAttackAim _moveAimAttackAim;
    [SerializeField]
    private IMoveAimAttackAttack _moveAimAttackAttack;

    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _speedLimit;
    [SerializeField]
    private float _movingInterval;
    [SerializeField]
    private float _aimingInterval;
    [SerializeField] 
    private float _attackingInterval;
    [SerializeField]
    private float _shootingRange;
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    public GameObject ShotPrefab;
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
        if (_moveAimAttackMovement.DidMovingTimePass())
        {
            _moveAimAttackAim.SetAiming();
            _moveAimAttackEnemyState = MoveAimAttackEnemyState.AIMING;
        }
        else 
        {
            _moveAimAttackMovement.MovingState();
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
            _moveAimAttackAim.AimingState();
        }
    }
    private void Attacking() 
    {
        if (_moveAimAttackAttack.DidAttackingTimePass()) // ELSE IF?
        {
            _moveAimAttackMovement.SetMoving();
            _moveAimAttackEnemyState = MoveAimAttackEnemyState.MOVING;
        }
        else
        {
            _moveAimAttackAttack.AttackingState();
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