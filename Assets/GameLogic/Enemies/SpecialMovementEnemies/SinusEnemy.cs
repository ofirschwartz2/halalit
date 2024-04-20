using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

public class SinusEnemy : SeedfulRandomGeneratorUser
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _sinAxisMovementAmplitude;
    [SerializeField]
    private float _changeSinForceInterval;
    [SerializeField]
    private float _otherAxisMovementAmplitude;
    [SerializeField]
    private float _otherAxisSpeedLimit;

    private Direction _sinDirection;
    private float _changeSinForceDirectionTime;
    private Vector2 _sinAxisDirection, _otherAxisDirection;
    private Dictionary<string, Direction> _newDirections = new()
    {
        { Tag.TOP_EDGE.GetDescription(), Direction.DOWN },
        { Tag.RIGHT_EDGE.GetDescription(), Direction.LEFT },
        { Tag.BOTTOM_EDGE.GetDescription(), Direction.UP },
        { Tag.LEFT_EDGE.GetDescription(), Direction.RIGHT },
    };

    void Start()
    {
        _sinDirection = _seedfulRandomGenerator.GetRandomDirection();
        SetSinDirections(_sinDirection);
        SetChangeSinForceDirectionTime(_changeSinForceInterval / 2);
    }

    void FixedUpdate()
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
        return Utils.IsUnderSpeedLimit(otherAxisVelocity, _otherAxisSpeedLimit);
    }

    private void SetChangeSinForceDirectionTime(float interval)
    {
        _changeSinForceDirectionTime = Time.time + interval;
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
        if (Utils.DidHitEdge(other.gameObject.tag))
        {
            ChangeSinDirection(_newDirections[other.gameObject.tag]);
        }
    }
    #endregion
}
