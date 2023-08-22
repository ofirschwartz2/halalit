using Assets.Enums;
using Assets.Utils;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WaitAttackStateMachine : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _attackLoadingInterval;
    [SerializeField]
    private float _oneAttackInterval;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private float _waitingTime, _stratAttackingTime, _finishAttackingTime;
    private WaitAttackEnemyState _followingEnemyState;
    private Vector2 _halalitDirection;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
        SetWaiting();
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
        if (!ShouldStopWaiting())
        {
            Wait();
        }
        else 
        {
            SetAttacking();
            _followingEnemyState = WaitAttackEnemyState.ATTACKING;
        }
    }

    private void Attacking()
    {
        if (!ShouldStopAttacking())
        {
            Attack();
        }
        else
        {
            SetWaiting();
            _followingEnemyState = WaitAttackEnemyState.WAITING;
        }
    }

    private void Wait() 
    {
    }

    private void Attack() 
    {
        EnemyUtils.MoveInStraightLine(_rigidBody, _halalitDirection, _movementAmplitude, _oneAttackInterval, _stratAttackingTime);
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    #region Setters
    private void SetWaiting()
    {
        SetWaitingTime();
    }

    private void SetAttackingTimes()
    {
        _finishAttackingTime = Time.time + _oneAttackInterval;
        _stratAttackingTime = Time.time;
    }

    private void SetWaitingTime()
    {
        _waitingTime = Time.time + _attackLoadingInterval;
    }

    private void SetAttacking()
    {
        SetHalalitDirection();
        SetAttackingTimes();
        _followingEnemyState = WaitAttackEnemyState.ATTACKING;
    }

    private void SetHalalitDirection()
    {
        var halalit = GameObject.FindGameObjectWithTag("Halalit");
        var halalitPosition = halalit.transform.position;
        _halalitDirection = Utils.NormalizeVector2(halalitPosition - transform.position);
    }
    #endregion

    #region Predicates
    private bool ShouldStopAttacking()
    {
        return Time.time > _finishAttackingTime;
    }

    private bool ShouldStopWaiting()
    {
        return Time.time > _waitingTime;
    }
    #endregion

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
    }
    #endregion
}