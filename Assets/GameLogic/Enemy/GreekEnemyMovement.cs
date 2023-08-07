using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GreekEnemyMovement : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _changeGreekDirectionInterval;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private VerticalGreekEnemyDirection _direction;
    private float _changeDirectionTime, _startMovementTime;

    void Start()
    {
        if (_useConfigFile) 
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _direction = VerticalGreekEnemyDirection.UP1;
        SetChangeDirectionTime();
    }

    void Update()
    {
        if (DidChangeDirectionTimePass())
        {
            ChangeDirection();
        }

        Move();
    }

    private void ChangeDirection()
    {
        _direction = GetNextDirection<VerticalGreekEnemyDirection>();
        _startMovementTime = _changeDirectionTime;
        SetChangeDirectionTime();
    }

    private void Move()
    {
        GreekMove(_direction);
    }

    private void GreekMove(VerticalGreekEnemyDirection direction)
    {
        Vector2 directionVector;

        switch (direction)
        {
            case VerticalGreekEnemyDirection.UP1:
            case VerticalGreekEnemyDirection.UP2:
                directionVector = Vector2.up;
                break;
            case VerticalGreekEnemyDirection.RIGHT:
                directionVector = Vector2.right;
                break;
            case VerticalGreekEnemyDirection.LEFT:
                directionVector = Vector2.left;
                break;
            default:
                throw new ArgumentException("Invalid direction");
        }
        Debug.Log(Time.deltaTime);
        EnemyUtils.MoveInStraightLine(
            _rigidBody,
            directionVector,
            _movementAmplitude,
            _changeGreekDirectionInterval,
            _startMovementTime);
    }

    private void GreekMove(HorizontalGreekEnemyDirection direction, bool isSpeedingUp)
    {
        var speedUpDlowDownNumber = isSpeedingUp ? 1 : -1;

        switch (direction)
        {
            case HorizontalGreekEnemyDirection.RIGHT1:
            case HorizontalGreekEnemyDirection.RIGHT2:
                _rigidBody.AddForce(Vector2.right * _movementAmplitude * speedUpDlowDownNumber);
                break;
            case HorizontalGreekEnemyDirection.UP:
                _rigidBody.AddForce(Vector2.up * _movementAmplitude * speedUpDlowDownNumber);
                break;
            case HorizontalGreekEnemyDirection.DOWN:
                _rigidBody.AddForce(Vector2.down * _movementAmplitude * speedUpDlowDownNumber);
                break;
        }
    }

    private bool DidChangeDirectionTimePass()
    {
        return Time.time > _changeDirectionTime;
    }

    private TGreekDirection GetNextDirection<TGreekDirection>()
    {
        TGreekDirection[] enumValues = (TGreekDirection[])Enum.GetValues(typeof(TGreekDirection));
        int currentIndex = Array.IndexOf(enumValues, _direction);
        int nextIndex = (currentIndex + 1) % enumValues.Length;

        return enumValues[nextIndex];
    }

    private void SetChangeDirectionTime()
    {
        _changeDirectionTime = _changeDirectionTime + _changeGreekDirectionInterval;
        Debug.Log(_changeDirectionTime);
        Debug.Log(_changeGreekDirectionInterval);
    }

}