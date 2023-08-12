using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class FollowingEnemy : MonoBehaviour
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
    private FollowingEnemyState _followingEnemyState;
    private Vector2 _halalitDirection;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
        _followingEnemyState = FollowingEnemyState.SET_WAITING;
        SetWaitingTime();
    }

    void Update()
    {
        switch (_followingEnemyState)
        {
            case FollowingEnemyState.SET_WAITING:
                SetWaiting();
                break;
            case FollowingEnemyState.WAITING:
                Debug.Log("WAITING");
                Waiting();
                break;
            case FollowingEnemyState.SET_ATTACKING:
                Debug.Log("SET_ATTACKING");
                SetAttacking();
                break;
            case FollowingEnemyState.ATTACKING:
                Debug.Log("ATTACKING");
                Attacking();
                break;
        }
    }

    private void Waiting()
    {
        if (DidWaitingTimePass())
        {
            _followingEnemyState = FollowingEnemyState.SET_ATTACKING;
        }
    }

    private void Attacking()
    {
        if (DidAttackTimePass())
        {
            _followingEnemyState = FollowingEnemyState.SET_WAITING;
        }
        else
        {
            EnemyUtils.MoveInStraightLine(_rigidBody, _halalitDirection, _movementAmplitude, _oneAttackInterval, _stratAttackingTime);
        }
    }
    private void Die()
    {
        Destroy(gameObject);
    }

    #region Setters
    private void SetWaiting()
    {
        SetWaitingTime();
        _followingEnemyState = FollowingEnemyState.WAITING;
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
        _followingEnemyState = FollowingEnemyState.ATTACKING;
    }

    private void SetHalalitDirection()
    {
        var halalit = GameObject.FindGameObjectWithTag("Halalit");
        var halalitPosition = halalit.transform.position;
        _halalitDirection = Utils.NormalizeVector2(halalitPosition - transform.position);
    }
    #endregion

    #region Predicates
    private bool DidAttackTimePass()
    {
        return Time.time > _finishAttackingTime;
    }

    private bool DidWaitingTimePass()
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