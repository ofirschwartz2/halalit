using Assets.Enums;
using Assets.Utils;
using System;
using System.ComponentModel;
using UnityEngine;

public class GreekEnemy : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _changeGreekDirectionInterval;

    private GreekEnemyMovementState _movementStage;
    private Direction _greekDirection;
    private float _changeDirectionTime, _startMovementTime;
    private Vector2 _currentMovementDirection;
    private bool _waitForNextStage;

    void Start()
    {
        _waitForNextStage = false;
        _greekDirection = Utils.GetRandomDirection();
        _movementStage = GreekEnemyMovementState.ONE;
        _currentMovementDirection = GetStageDirectionVector(_movementStage, _greekDirection);
        SetChangeDirectionTime();
    }

    void FixedUpdate()
    {
        if (DidChangeDirectionTimePass())
        {
            ChangeGreekStage();
        }

        if (!_waitForNextStage) 
        {
            EnemyMovementUtils.MoveInStraightLine(
                _rigidBody,
                _currentMovementDirection,
                _movementAmplitude,
                _changeGreekDirectionInterval,
                _startMovementTime);
        }
    }

    private void ChangeGreekStage()
    {
        _movementStage = GetNextStage<GreekEnemyMovementState>();
        _startMovementTime = _changeDirectionTime;
        SetChangeDirectionTime();
        _currentMovementDirection = GetStageDirectionVector(_movementStage, _greekDirection);
        _waitForNextStage = false;
    }

    private void ChangeGreekDirection(Direction newDirection)
    {
        _greekDirection = newDirection;
        _movementStage = GreekEnemyMovementState.FOUR;
        _startMovementTime = _changeDirectionTime;
        _currentMovementDirection = GetStageDirectionVector(_movementStage, _greekDirection);
        _rigidBody.velocity = Vector2.zero;
        _waitForNextStage = true;
    }

    private Vector2 GetStageDirectionVector(GreekEnemyMovementState stage, Direction direction) 
    {
        return direction switch
        {
            Direction.UP => stage switch
            {
                GreekEnemyMovementState.ONE or GreekEnemyMovementState.THREE => Vector2.up,
                GreekEnemyMovementState.TWO => Vector2.right,
                GreekEnemyMovementState.FOUR => Vector2.left,
                _ => throw new Exception("GreekEnemyMovement Stage not supported"),
            },
            Direction.RIGHT => stage switch
            {
                GreekEnemyMovementState.ONE or GreekEnemyMovementState.THREE => Vector2.right,
                GreekEnemyMovementState.TWO => Vector2.down,
                GreekEnemyMovementState.FOUR => Vector2.up,
                _ => throw new Exception("GreekEnemyMovementStage not supported"),
            },
            Direction.LEFT => stage switch
            {
                GreekEnemyMovementState.ONE or GreekEnemyMovementState.THREE => Vector2.left,
                GreekEnemyMovementState.TWO => Vector2.up,
                GreekEnemyMovementState.FOUR => Vector2.down,
                _ => throw new Exception("GreekEnemyMovementStage not supported"),
            },
            Direction.DOWN => stage switch
            {
                GreekEnemyMovementState.ONE or GreekEnemyMovementState.THREE => Vector2.down,
                GreekEnemyMovementState.TWO => Vector2.left,
                GreekEnemyMovementState.FOUR => Vector2.right,
                _ => throw new Exception("GreekEnemyMovementStage not supported"),
            },
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    private bool DidChangeDirectionTimePass()
    {
        return Time.time > _changeDirectionTime;
    }

    private TGreekDirection GetNextStage<TGreekDirection>()
    {
        TGreekDirection[] enumValues = (TGreekDirection[])Enum.GetValues(typeof(TGreekDirection));
        int currentIndex = Array.IndexOf(enumValues, _movementStage);
        int nextIndex = (currentIndex + 1) % enumValues.Length;

        return enumValues[nextIndex];
    }

    private void SetChangeDirectionTime()
    {
        _changeDirectionTime += _changeGreekDirectionInterval;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.DidHitEdge(other.gameObject.tag))
        {
            switch (other.gameObject.tag)
            {
                case "TopEdge":
                    ChangeGreekDirection(Direction.DOWN);
                    break;
                case "RightEdge":
                    ChangeGreekDirection(Direction.LEFT);
                    break;
                case "BottomEdge":
                    ChangeGreekDirection(Direction.UP);
                    break;
                case "LeftEdge":
                    ChangeGreekDirection(Direction.RIGHT);
                    break;
            }
        }
    }
}
