using Assets.Common;
using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FollowingEnemyMovementController : MonoBehaviour
{
    public float Force;
    public bool UseConfigFile;
              
    private Rigidbody2D _rigidBody;          // Reference to the Rigidbody2D component
    private float _attackLoadingInterval, _speedUpSlowDownInterval, _oneAttackInterval, _waitingTime, _finishAttackingTime;
    private bool _isSpeedingUp;
    private FollowingEnemyState _followingEnemyState;
    private Vector2 _halalitDirection;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();  // Get the reference to the Rigidbody2D component
        _attackLoadingInterval = 4;
        _oneAttackInterval = 2;
        _speedUpSlowDownInterval = (8/10) * _oneAttackInterval;
        _followingEnemyState = FollowingEnemyState.SET_WAITING;
        SetWaitingTime();
        if (UseConfigFile)
            ConfigureFromFile();
        tag = "FollowingEnemy"; // TODO: should this always be "Enemy" on the Enemy level?
    }

    void Update()
    {
        switch(_followingEnemyState)
        {
            case FollowingEnemyState.SET_WAITING:
                SetWaiting();
                break;
            case FollowingEnemyState.WAITING:
                Waiting();
                break;
            case FollowingEnemyState.SET_ATTACKING:
                SetAttacking();
                break;
            case FollowingEnemyState.ATTACKING:
                Attacking();
                break;
        }
    }

    private void SetWaiting()
    {
        SetWaitingTime();
        _followingEnemyState = FollowingEnemyState.WAITING;
    }

    private void Waiting()
    {
        if (DidWaitingTimePass())
        {
            _followingEnemyState = FollowingEnemyState.SET_ATTACKING;
        }
    }

    private void SetAttacking()
    {
        SetHalalitDirection();
        SetFinishAttackingTime();
        _followingEnemyState = FollowingEnemyState.ATTACKING;
    }

    private void SetHalalitDirection()
    {
        var halalit = GameObject.FindGameObjectWithTag("Halalit");
        var halalitPosition = halalit.transform.position;
        _halalitDirection = NormalizeVector2(halalitPosition - transform.position);
    }

    private Vector2 NormalizeVector2(Vector2 vector) // TODO: MOVE TO UTILS
    {
        var magnitude = (float)Math.Sqrt(Math.Pow(vector.x, 2) + Math.Pow(vector.y, 2));
        return new Vector2(vector.x / magnitude, vector.y / magnitude);
    }

    private void Attacking()
    {

        if (DidAttackTimePass())
        {
            _followingEnemyState = FollowingEnemyState.SET_WAITING;
        }
        else
        {
            TryChangeSpeedUpSlowDown();
            Move();
        }
    }

    private void Move()
    {
        var speedUpDlowDownNumber = _isSpeedingUp ? 1 : -1;
        _rigidBody.AddForce(_halalitDirection * Force * speedUpDlowDownNumber);
    }

    private void SetFinishAttackingTime()
    {
        _finishAttackingTime = Time.time + _oneAttackInterval;
    }

    private void SetWaitingTime()
    {
        _waitingTime = Time.time + _attackLoadingInterval;
    }

    private bool DidAttackTimePass()
    {
        return Time.time > _finishAttackingTime;
    }

    private void TryChangeSpeedUpSlowDown()
    {
        if (ShouldSlowDown())
        {
            _isSpeedingUp = false;
        }
        else
        {
            _isSpeedingUp = true;
        }
    }

    private bool ShouldSlowDown()
    {
        return _finishAttackingTime - Time.time  < _speedUpSlowDownInterval;
    }

    private bool DidWaitingTimePass()
    {
        return Time.time > _waitingTime;
    }

    private void ConfigureFromFile()
    {
        string[] props = { "Force" };
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
        Force = propsFromConfig["Force"];
    }
}
