using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class SinusEnemyMovement : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _sinAxisMovementAmplitude;
    [SerializeField]
    private float _changeSinForceInterval;
    [SerializeField]
    private float _otherAxisMovementAmplitude;
    [SerializeField]
    private float _otherAxisSpeedLimit;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private Direction _sinDirection;
    private float _changeSinForceDirectionTime;
    private Vector2 _sinAxisDirection, _otherAxisDirection;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
        _sinDirection = Utils.GetRandomDirection();
        SetSinDirections(_sinDirection);
        SetChangeSinForceDirectionTime(_changeSinForceInterval / 2);
    }

    void Update()
    {
        otherAxisMovement(_otherAxisDirection);
        SinAxisMovement();
    }

    private void SetSinDirections(Direction sinDirection) 
    {
        switch (sinDirection)
        {
            case Direction.UP:
                _otherAxisDirection = Vector2.up;
                _sinAxisDirection = Vector2.right;
                break;
            case Direction.RIGHT:
                _otherAxisDirection = Vector2.right;
                _sinAxisDirection = Vector2.up;
                break;
            case Direction.LEFT:
                _otherAxisDirection = Vector2.left;
                _sinAxisDirection = Vector2.up;
                break;
            case Direction.DOWN:
                _otherAxisDirection = Vector2.down;
                _sinAxisDirection = Vector2.right;
                break;
        }
    }
    private void SinAxisMovement()
    {
        if (DidSinForceDirectionTimePass())
        {
            SetChangeSinForceDirectionTime(_changeSinForceInterval);
            SinForceChangeDirection();
        }
        _rigidBody.AddForce(_sinAxisDirection * _sinAxisMovementAmplitude);
    }

    private void SinForceChangeDirection()
    {
        _sinAxisDirection = _sinAxisDirection * (-1);
    }

    private bool DidSinForceDirectionTimePass()
    {
        return Time.time > _changeSinForceDirectionTime;
    }

    private void otherAxisMovement(Vector2 otherAxisDirection)
    {
        if (otherAxisIsUnderSpeedLimit(otherAxisDirection))
        {
            _rigidBody.AddForce(otherAxisDirection * _otherAxisMovementAmplitude);
        }
    }

    private bool otherAxisIsUnderSpeedLimit(Vector2 otherAxisDirection)
    {
        var otherAxisVelocity = Utils.GetVelocityInDirection(_rigidBody.velocity, otherAxisDirection);
        return Math.Abs(otherAxisVelocity) < _otherAxisSpeedLimit;
    }

    private void SetChangeSinForceDirectionTime(float interval)
    {
        _changeSinForceDirectionTime = Time.time + interval;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void ChangeSinDirection(Direction newDirection) 
    {
        _sinDirection = newDirection;
        SetSinDirections(_sinDirection);
        _rigidBody.velocity = Vector2.zero;
        SetChangeSinForceDirectionTime(_changeSinForceInterval / 2);
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
        else if (other.gameObject.CompareTag("TopEdge") || other.gameObject.CompareTag("RightEdge") || other.gameObject.CompareTag("BottomEdge") || other.gameObject.CompareTag("LeftEdge"))
        {
            switch (other.gameObject.tag)
            {
                case "TopEdge":
                    ChangeSinDirection(Direction.DOWN);
                    break;
                case "RightEdge":
                    ChangeSinDirection(Direction.LEFT);
                    break;
                case "BottomEdge":
                    ChangeSinDirection(Direction.UP);
                    break;
                case "LeftEdge":
                    ChangeSinDirection(Direction.RIGHT);
                    break;
            }
        }
    }
    #endregion
}