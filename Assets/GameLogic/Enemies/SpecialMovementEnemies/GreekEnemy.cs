using Assets.Enums;
using Assets.Utils;
using System;
using System.ComponentModel;
using UnityEngine;

public class GreekEnemy : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _changeGreekDirectionInterval;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private GreekEnemyMovementState _movementStage;
    private Direction _greekDirection;
    private float _changeDirectionTime, _startMovementTime;
    private Vector2 _currentMovementDirection;
    private bool _waitForNextStage;

    void Start()
    {
        if (_useConfigFile) 
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _waitForNextStage = false;
        _greekDirection = Utils.GetRandomDirection();
        _movementStage = GreekEnemyMovementState.ONE;
        _currentMovementDirection = GetStageDirectionVector(_movementStage, _greekDirection);
        SetChangeDirectionTime();
    }

    void Update()
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
        switch (direction)
        {
            case Direction.UP:
                switch (stage)
                {
                    case GreekEnemyMovementState.ONE:
                    case GreekEnemyMovementState.THREE:
                        return Vector2.up;
                    case GreekEnemyMovementState.TWO:
                        return Vector2.right;
                    case GreekEnemyMovementState.FOUR:
                        return Vector2.left;
                    default:
                        throw new Exception("GreekEnemyMovement Stage not supported");
                }
            case Direction.RIGHT:
                switch (stage)
                {
                    case GreekEnemyMovementState.ONE:
                    case GreekEnemyMovementState.THREE:
                        return Vector2.right;
                    case GreekEnemyMovementState.TWO:
                        return Vector2.down;
                    case GreekEnemyMovementState.FOUR:
                        return Vector2.up;
                    default:
                        throw new Exception("GreekEnemyMovementStage not supported");
                }
            case Direction.LEFT:
                switch (stage)
                {
                    case GreekEnemyMovementState.ONE:
                    case GreekEnemyMovementState.THREE:
                        return Vector2.left;
                    case GreekEnemyMovementState.TWO:
                        return Vector2.up;
                    case GreekEnemyMovementState.FOUR:
                        return Vector2.down;
                    default:
                        throw new Exception("GreekEnemyMovementStage not supported");
                }
            case Direction.DOWN:
                switch (stage)
                {
                    case GreekEnemyMovementState.ONE:
                    case GreekEnemyMovementState.THREE:
                        return Vector2.down;
                    case GreekEnemyMovementState.TWO:
                        return Vector2.left;
                    case GreekEnemyMovementState.FOUR:
                        return Vector2.right;
                    default:
                        throw new Exception("GreekEnemyMovementStage not supported");
                }
            default:
                throw new InvalidEnumArgumentException();
        }
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
        if (EnemyUtils.ShouldKnockEnemyBack(LayerMask.LayerToName(gameObject.layer), other))
        {
            EnemyUtils.KnockMeBack(_rigidBody, other);
        }
        else if (Utils.DidHitEdge(other.gameObject.tag))
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
