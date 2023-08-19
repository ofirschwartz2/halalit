using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class ZigZagEnemy : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _changeZigZagDirectionInterval;
    [SerializeField]
    private float _changeFromDirectionAngle;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private Vector2 _direction;
    private ZigZagDirection _zigZagDirectionFlag;
    private float _changeZigZagDirectionTime, _startMovementTime;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _zigZagDirectionFlag = ZigZagDirection.ZAG;
        _direction = Utils.GetRandomVector2OnCircle();
        SetChangeZigZagDirectionTime();

    }

    void Update()
    {
        if (DidZigZagTimePass())
        {
            ChangeZigZagDirection();
        }

        EnemyUtils.MoveInStraightLine(_rigidBody, _direction, _movementAmplitude, _changeZigZagDirectionInterval, _startMovementTime);
    }

    private void ChangeZigZagDirection()
    {
        if (_zigZagDirectionFlag == ZigZagDirection.ZIG)
        {
            _direction = Utils.AddAngleToVector(_direction, _changeFromDirectionAngle);
            _zigZagDirectionFlag = ZigZagDirection.ZAG;
        }
        else
        {
            _direction = Utils.AddAngleToVector(_direction, _changeFromDirectionAngle * (-1));
            _zigZagDirectionFlag = ZigZagDirection.ZIG;
        }
        SetChangeZigZagDirectionTime();
    }

    private void SetChangeZigZagDirectionTime()
    {
        _changeZigZagDirectionTime = Time.time + _changeZigZagDirectionInterval;
        _startMovementTime = Time.time;
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
            SetChangeZigZagDirectionTime();
            _direction = EnemyUtils.GetAnotherDirectionFromEdge(_rigidBody, other.gameObject.tag);
        }
    }
    #endregion

    #region Predicates
    private bool DidZigZagTimePass()
    {
        return Time.time > _changeZigZagDirectionTime;
    }
    #endregion
}